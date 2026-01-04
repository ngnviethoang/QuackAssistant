using Telegram.Bot.Types;

namespace QuackAssistant.Shared.Dispatchers;

public interface IDispatcher
{
    Task DispatchAsync(Message message);
}