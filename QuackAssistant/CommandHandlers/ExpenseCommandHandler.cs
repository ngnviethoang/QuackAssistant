using GenerativeAI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using QuackAssistant.ConfigurationOptions;
using QuackAssistant.Data;
using QuackAssistant.Data.Entities;
using QuackAssistant.Shared;
using QuackAssistant.Shared.Attributes;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace QuackAssistant.CommandHandlers;

[Command(TeleCommands.Expense, Alias = "/+", Description = "record an expense", Example = "/expense cafe 35k")]
public class ExpenseCommandHandler : ICommandHandler
{
    private readonly QuackAssistantDbContext _dbContext;
    private readonly GoogleAi _googleAi;
    private readonly ITelegramBotClient _telegramBotClient;

    public ExpenseCommandHandler(
        ITelegramBotClient telegramBotClient,
        QuackAssistantDbContext dbContext,
        IOptions<AppSettings> appSettings)
    {
        _telegramBotClient = telegramBotClient;
        _dbContext = dbContext;
        _googleAi = new GoogleAi(appSettings.Value.GeminiAi.ApiKey);
    }

    public async Task HandleAsync(Message message)
    {
        if (string.IsNullOrWhiteSpace(message.Text))
        {
            await _telegramBotClient.SendMessage(message.Chat.Id, "❌ Invalid format");
            return;
        }

        var parts = message.Text.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 3)
        {
            await _telegramBotClient.SendMessage(message.Chat.Id, "❌ Invalid format");
            return;
        }

        var amountText = parts[^1];
        var amountDigitsOnly = amountText.Where(char.IsDigit).ToArray();
        if (!int.TryParse(amountDigitsOnly, out var amount))
        {
            await _telegramBotClient.SendMessage(message.Chat.Id, "❌ Invalid format");
            return;
        }

        var note = string.Join(' ', parts.Skip(1).Take(parts.Length - 2));
        if (string.IsNullOrWhiteSpace(note))
        {
            await _telegramBotClient.SendMessage(message.Chat.Id, "❌ Invalid format");
            return;
        }

        var categories = await _dbContext.Categories
            .Select(c => c.Name)
            .ToListAsync();

        var prompt = $"""
                      You are a finance assistant. Choose the best category for the following expense. Existing categories:
                      {string.Join(", ", categories)} expense description: "{note}" Return ONLY the category name.
                      """;

        var model = _googleAi.CreateGenerativeModel("gemini-2.5-flash");
        var response = await model.GenerateContentAsync(prompt);

        var categoryName = response.Text.Trim();
        var category = await _dbContext.Categories.FirstOrDefaultAsync(c => string.Equals(c.Name, categoryName));
        if (category == null)
        {
            await _telegramBotClient.SendMessage(message.Chat.Id, "⚠️ No valid category found for this expense. The operation has been canceled.");
            return;
        }

        var vnDateTime = DateTime.UtcNow.AddHours(7);
        var uniqueCode = vnDateTime.ToString("HH:mm dd/MM/yyyy");
        var transaction = new Transaction(Guid.NewGuid(), amount, category.Id, note, vnDateTime, uniqueCode);

        _dbContext.Transactions.Add(transaction);
        await _dbContext.SaveChangesAsync();

        await _telegramBotClient.SendMessage(message.Chat.Id, $"✅ Income added\n💰 {amount:N0}\n📂 {category.Name}\n🆔 Code: {uniqueCode}");
    }
}