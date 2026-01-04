using QuackAssistant.Data;
using QuackAssistant.Shared;
using QuackAssistant.Shared.Attributes;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace QuackAssistant.CommandHandlers;

[Command(TeleCommands.DebtOut, Description = "ghi nợ phải trả", Example = "/debtout a Mai, tiền điện, 500k")]
public class DebtOutCommandHandler : ICommandHandler
{
    private readonly QuackAssistantDbContext _dbContext;
    private readonly ITelegramBotClient _telegramBotClient;

    public DebtOutCommandHandler(
        ITelegramBotClient telegramBotClient,
        QuackAssistantDbContext dbContext)
    {
        _telegramBotClient = telegramBotClient;
        _dbContext = dbContext;
    }

    public Task HandleAsync(Message message)
    {
        throw new NotImplementedException();
    }
}