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

[Command(TeleCommands.Expense, Description = "ghi khoản chi", Example = "/expense cafe 35k")]
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
        if (string.IsNullOrEmpty(message.Text))
        {
            await _telegramBotClient.SendMessage(message.Chat.Id, "❌ Invalid format");
            return;
        }

        var parts = message.Text.Split(' ', 3, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 3)
        {
            await _telegramBotClient.SendMessage(message.Chat.Id, "❌ Invalid format");
            return;
        }

        var note = parts.ElementAt(1);
        var amountDigitsOnly = parts.ElementAt(2).Where(char.IsDigit).ToArray();
        var canParseAmount = int.TryParse(amountDigitsOnly, out var amount);
        if (string.IsNullOrEmpty(note) || !canParseAmount)
        {
            await _telegramBotClient.SendMessage(message.Chat.Id, "❌ Invalid format");
            return;
        }

        var categories = await _dbContext.Categories
            .Select(c => c.Name)
            .ToListAsync();

        var prompt = $"""
                      You are a finance assistant.
                      Choose the best category for the following expense.

                      Existing categories:
                      {string.Join(", ", categories)}

                      expense description:
                      "{note}"

                      Return ONLY the category name.
                      """;

        var model = _googleAi.CreateGenerativeModel("gemini-2.5-flash");
        var response = await model.GenerateContentAsync(prompt);

        var categoryName = response.Text.Trim();
        var category = await _dbContext.Categories.FirstOrDefaultAsync(c => c.Name == categoryName);
        if (category == null)
        {
            category = new Category
            {
                Id = Guid.NewGuid(),
                Name = categoryName
            };

            _dbContext.Categories.Add(category);
        }

        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            Amount = amount,
            Type = "EXPENSE",
            CategoryId = category.Id,
            Note = note,
            TransactionTime = DateTime.UtcNow
        };

        _dbContext.Transactions.Add(transaction);
        await _dbContext.SaveChangesAsync();

        await _telegramBotClient.SendMessage(message.Chat.Id, $"✅ Income added\n💰 {amount:N0}\n📂 {category.Name}");
    }
}