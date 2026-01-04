using Microsoft.EntityFrameworkCore;
using QuackAssistant.Data;
using QuackAssistant.Shared;
using QuackAssistant.Shared.Attributes;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace QuackAssistant.CommandHandlers;

[Command(TeleCommands.DeleteTransaction, Alias = "/d+", Description = "delete a transaction by id", Example = "/d+ [Guid]")]
public class DeleteTransactionCommandHandler : ICommandHandler
{
    private readonly QuackAssistantDbContext _dbContext;
    private readonly ITelegramBotClient _telegramBotClient;

    public DeleteTransactionCommandHandler(
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
            await _telegramBotClient.SendMessage(message.Chat.Id, "❌ Invalid format. Use /d+ [Code]");
            return;
        }

        var parts = message.Text.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 2)
        {
            await _telegramBotClient.SendMessage(message.Chat.Id, "❌ Invalid format. Use /d+ [Code]");
            return;
        }

        var uniqueCode = string.Join(' ', parts.Skip(1).Take(parts.Length - 1));

        var transaction = await _dbContext.Transactions
            .Include(t => t.Category)
            .FirstOrDefaultAsync(t => string.Equals(t.Code, uniqueCode));

        if (transaction == null)
        {
            await _telegramBotClient.SendMessage(message.Chat.Id, $"❌ No transaction found with Code: {uniqueCode}.");
            return;
        }

        _dbContext.Transactions.Remove(transaction);
        await _dbContext.SaveChangesAsync();

        await _telegramBotClient.SendMessage(
            message.Chat.Id,
            $"✅ Transaction deleted\n💰 {transaction.Amount:N0}\n📂 {transaction.Category.Name}\n📝 {transaction.Note}"
        );
    }
}