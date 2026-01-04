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
        var allTypes = Assembly.GetExecutingAssembly().GetTypes();

        var commandTypes = allTypes
            .Where(t => !t.IsAbstract && typeof(ICommandHandler).IsAssignableFrom(t))
            .Where(t => t.GetCustomAttribute<CommandAttribute>() != null)
            .ToList();

        _commandHandlers = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);

        foreach (var type in commandTypes)
        {
            var attr = type.GetCustomAttribute<CommandAttribute>()!;

            _commandHandlers.TryAdd(attr.Name, type);

            if (!string.IsNullOrEmpty(attr.Alias))
            {
                _commandHandlers.TryAdd(attr.Alias, type);
            }

            services.AddTransient(type);
        }
    }
}