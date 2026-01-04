using System.Text;
using Microsoft.EntityFrameworkCore;
using QuackAssistant.Data;
using QuackAssistant.Shared;
using QuackAssistant.Shared.Attributes;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace QuackAssistant.CommandHandlers;

[Command(TeleCommands.Today, Alias = "/td", Description = "xem thu chi hôm nay", Example = "/today")]
public class TodayCommandHandler : ICommandHandler
{
    private readonly QuackAssistantDbContext _dbContext;
    private readonly ITelegramBotClient _telegramBotClient;

    public TodayCommandHandler(
        ITelegramBotClient telegramBotClient,
        QuackAssistantDbContext dbContext)
    {
        _telegramBotClient = telegramBotClient;
        _dbContext = dbContext;
    }

    public async Task HandleAsync(Message message)
    {
        var vnStart = DateTime.UtcNow.AddHours(7).Date;
        var vnEnd = vnStart.AddDays(1).Date;

        var transactions = await _dbContext.Transactions
            .Where(t => t.TransactionTime.Date >= vnStart.Date && t.TransactionTime.Date < vnEnd.Date)
            .OrderBy(t => t.TransactionTime)
            .ToListAsync();

        if (!transactions.Any())
        {
            await _telegramBotClient.SendMessage(message.Chat.Id, $"📅 {vnStart:dd-MM-yyyy}\n❌ No transactions found");
            return;
        }

        var sb = new StringBuilder();
        sb.AppendLine($"📅 {vnStart:dd-MM-yyyy}");
        sb.AppendLine();

        var totalIncome = 0;
        var totalExpense = 0;

        foreach (var t in transactions)
        {
            var sign = t.Amount > 0 ? "💰" : "💸";
            var amount = Math.Abs(t.Amount);

            sb.AppendLine($"{sign} {amount:N0} – {t.Note} ({t.TransactionTime.DateTime:HH:mm}) [Code: {t.Code}]");

            if (t.Amount > 0)
            {
                totalIncome += t.Amount;
            }
            else
            {
                totalExpense += amount;
            }
        }

        sb.AppendLine();
        sb.AppendLine($"📊 Total Transactions: {transactions.Count}");
        sb.AppendLine($"💰 Total Income: {totalIncome:N0}");
        sb.AppendLine($"💸 Total Expense: {totalExpense:N0}");

        await _telegramBotClient.SendMessage(message.Chat.Id, sb.ToString(), ParseMode.Html);
    }
}