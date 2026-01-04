using System.Globalization;
using Microsoft.EntityFrameworkCore;
using QuackAssistant.Data;
using QuackAssistant.Shared;
using QuackAssistant.Shared.Attributes;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace QuackAssistant.CommandHandlers;

[Command(TeleCommands.Date, Description = "xem thu chi theo ngày", Example = "/date 03-01-2026")]
public class DateCommandHandler : ICommandHandler
{
    private readonly QuackAssistantDbContext _dbContext;
    private readonly ITelegramBotClient _telegramBotClient;

    public DateCommandHandler(
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

        var parts = message.Text.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2)
        {
            await _telegramBotClient.SendMessage(message.Chat.Id, "❌ Invalid format");
            return;
        }

        if (!DateTime.TryParseExact(parts[1], "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var inputDate))
        {
            await _telegramBotClient.SendMessage(message.Chat.Id, "❌ Invalid date format. Use dd-MM-yyyy (e.g. 31-01-2025)");
            return;
        }

        var vnTimeZone = Helper.GetVietnamTimeZone();
        var vnStart = inputDate.Date;
        var vnEnd = vnStart.AddDays(1);

        // Convert sang UTC để query DB
        var utcStart = TimeZoneInfo.ConvertTimeToUtc(vnStart, vnTimeZone);
        var utcEnd = TimeZoneInfo.ConvertTimeToUtc(vnEnd, vnTimeZone);

        var transactions = await _dbContext.Transactions
            .Where(t => t.TransactionTime >= utcStart && t.TransactionTime < utcEnd)
            .ToListAsync();

        if (!transactions.Any())
        {
            await _telegramBotClient.SendMessage(message.Chat.Id, $"📅 {inputDate:dd-MM-yyyy}\n❌ No transactions found");
            return;
        }

        var totalIncome = transactions
            .Where(t => t.Amount > 0)
            .Sum(t => t.Amount);

        var totalExpense = transactions
            .Where(t => t.Amount < 0)
            .Sum(t => Math.Abs(t.Amount));

        await _telegramBotClient.SendMessage(
            message.Chat.Id,
            $"📅 {inputDate:dd-MM-yyyy}\n" +
            $"💰 Income: {totalIncome:N0}\n" +
            $"💸 Expense: {totalExpense:N0}\n" +
            $"📊 Transactions: {transactions.Count}");
    }
}