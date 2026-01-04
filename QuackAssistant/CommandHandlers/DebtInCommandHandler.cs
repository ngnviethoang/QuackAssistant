using System.Text.RegularExpressions;
using QuackAssistant.Data;
using QuackAssistant.Data.Entities;
using QuackAssistant.Shared;
using QuackAssistant.Shared.Attributes;
using QuackAssistant.Shared.Enumerations;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace QuackAssistant.CommandHandlers;

// [Command(TeleCommands.DebtIn, Description = "ghi nợ phải thu", Example = "/debtin a Sơn, tiền cơm, 35k")]
public class DebtInCommandHandler : ICommandHandler
{
    private readonly QuackAssistantDbContext _dbContext;
    private readonly ITelegramBotClient _telegramBotClient;

    public DebtInCommandHandler(
        ITelegramBotClient telegramBotClient,
        QuackAssistantDbContext dbContext)
    {
        _telegramBotClient = telegramBotClient;
        _dbContext = dbContext;
    }

    public async Task HandleAsync(Message message)
    {
        if (string.IsNullOrWhiteSpace(message.Text))
        {
            await _telegramBotClient.SendMessage(message.Chat.Id, "❌ Invalid format");
            return;
        }

        var text = message.Text.Replace("/debtin", "", StringComparison.OrdinalIgnoreCase).Trim();
        if (!TryParseDebtInput(text, out var personName, out var description, out var amount))
        {
            await _telegramBotClient.SendMessage(
                message.Chat.Id,
                "❌ Format: /debtin a Sơn, tiền cơm, 35k");
            return;
        }

        var debtRecord = new Debt(personName: personName, description: description, amount: (int)amount, direction: DebtDirectionType.Receivable, creationTime: DateTimeOffset.UtcNow);

        _dbContext.Debts.Add(debtRecord);
        await _dbContext.SaveChangesAsync();

        await _telegramBotClient.SendMessage(
            message.Chat.Id,
            $"📒 <b>Ghi nợ phải thu</b>\n" +
            $"👤 {personName}\n" +
            $"📝 {description}\n" +
            $"💰 {amount:N0}",
            ParseMode.Html);
    }

    private static bool TryParseDebtInput(
        string input,
        out string personName,
        out string description,
        out decimal amount)
    {
        personName = description = string.Empty;
        amount = 0;

        var parts = input.Split(',', 3, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 3)
            return false;

        personName = parts[0].Trim();
        description = parts[1].Trim();

        var amountText = Regex.Replace(parts[2], @"[^\d]", "");
        return decimal.TryParse(amountText, out amount);
    }
}