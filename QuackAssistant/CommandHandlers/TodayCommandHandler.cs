using Microsoft.EntityFrameworkCore;
using QuackAssistant.Data;
using QuackAssistant.Shared;
using QuackAssistant.Shared.Attributes;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace QuackAssistant.CommandHandlers;

[Command(TeleCommands.Today, Description = "xem thu chi hôm nay", Example = "/today")]
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
        var vnTimeZone = Helper.GetVietnamTimeZone();

        var vnNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vnTimeZone);

        var vnStart = vnNow.Date;
        var vnEnd = vnStart.AddDays(1);

        var utcStart = TimeZoneInfo.ConvertTimeToUtc(vnStart, vnTimeZone);
        var utcEnd = TimeZoneInfo.ConvertTimeToUtc(vnEnd, vnTimeZone);

        var transactions = await _dbContext.Transactions
            .Where(t => t.TransactionTime >= utcStart && t.TransactionTime < utcEnd)
            .ToListAsync();

        if (!transactions.Any())
        {
            await _telegramBotClient.SendMessage(message.Chat.Id, $"📅 {vnStart:dd-MM-yyyy}\n❌ No transactions found");
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
            $"📅 {vnStart:dd-MM-yyyy}\n" +
            $"💰 Income: {totalIncome:N0}\n" +
            $"💸 Expense: {totalExpense:N0}\n" +
            $"📊 Transactions: {transactions.Count}");
    }
}