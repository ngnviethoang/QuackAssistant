using System.Reflection;
using QuackAssistant.CommandHandlers;
using QuackAssistant.Shared.Attributes;
using Telegram.Bot.Types;

namespace QuackAssistant.Shared.Dispatchers;

public sealed class Dispatcher : IDispatcher
{
    private static Dictionary<string, Type> _commandHandlers;
    private readonly IServiceProvider _serviceProvider;

    public Dispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task DispatchAsync(Message message)
    {
        if (string.IsNullOrWhiteSpace(message.Text)) return;

        var command = GetCommand(message.Text);
        var handler = _commandHandlers.TryGetValue(command, out var handlerType)
            ? (ICommandHandler)_serviceProvider.GetRequiredService(handlerType)
            : (ICommandHandler)_serviceProvider.GetRequiredService(typeof(HelpCommandHandler));
        await handler.HandleAsync(message);
    }

    private static string GetCommand(string text)
    {
        return text.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries)[0];
    }

    internal static void RegisterHandlers(IServiceCollection services)
    {
        _commandHandlers = Assembly.GetExecutingAssembly().GetTypes()
            .Where(type => !type.IsAbstract && typeof(ICommandHandler).IsAssignableFrom(type))
            .Where(type => type.GetCustomAttribute<CommandAttribute>() != null)
            .ToDictionary(type => type.GetCustomAttribute<CommandAttribute>()!.Name, i => i, StringComparer.OrdinalIgnoreCase);

        foreach (var type in _commandHandlers.Values) services.AddTransient(type);
    }
}