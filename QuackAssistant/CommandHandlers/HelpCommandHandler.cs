using System.Reflection;
using System.Text;
using QuackAssistant.Shared;
using QuackAssistant.Shared.Attributes;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace QuackAssistant.CommandHandlers;

[Command(TeleCommands.Help, Alias = "/h", Description = "display the help menu", Example = "/help")]
public class HelpCommandHandler : ICommandHandler
{
    private readonly ITelegramBotClient _telegramBotClient;

    public HelpCommandHandler(ITelegramBotClient telegramBotClient)
    {
        _telegramBotClient = telegramBotClient;
    }

    public async Task HandleAsync(Message message)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<b><u>📘 EXPENSE & INCOME – MENU</u></b>\n");

        var commands = Assembly.GetExecutingAssembly().GetTypes()
            .Where(type => !type.IsAbstract && typeof(ICommandHandler).IsAssignableFrom(type))
            .Select(type => type.GetCustomAttribute<CommandAttribute>())
            .Where(attr => attr != null)
            .OrderBy(attr => attr!.Name);

        foreach (var cmd in commands!)
        {
            var nameDisplay = cmd!.Alias != null ? $"{cmd.Name} ({cmd.Alias})" : cmd.Name;
            sb.AppendLine($"<b>{nameDisplay}</b> – {cmd.Description}");

            if (!string.IsNullOrWhiteSpace(cmd.Example))
            {
                sb.AppendLine($"<i>Example:</i> <code>{cmd.Example}</code>");
            }

            sb.AppendLine();
        }

        var menu = sb.ToString();

        await _telegramBotClient.SendMessage(message.Chat.Id, menu, ParseMode.Html, replyMarkup: new ReplyKeyboardRemove());
    }
}