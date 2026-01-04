using Telegram.Bot.Types;

namespace QuackAssistant.Shared;

public interface ICommandHandler
{
    Task HandleAsync(Message message);
}