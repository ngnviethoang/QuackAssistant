using System.Reflection;
using System.Text;
using QuackAssistant.Shared;
using QuackAssistant.Shared.Attributes;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace QuackAssistant.CommandHandlers;

[Command(TeleCommands.Help, Description = "hiển thị menu hướng dẫn sử dụng", Example = "/help")]
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
        sb.AppendLine("<b><u>📘 SỔ THU CHI – MENU</u></b>\n");

        var commands = Assembly.GetExecutingAssembly().GetTypes()
            .Where(type => !type.IsAbstract && typeof(ICommandHandler).IsAssignableFrom(type))
            .Select(type => type.GetCustomAttribute<CommandAttribute>())
            .Where(commandAttribute => commandAttribute != null)
            .OrderBy(commandAttribute => commandAttribute!.Name);

        foreach (var cmd in commands!)
        {
            sb.AppendLine($"<b>{cmd!.Name}</b> – {cmd.Description}");

            if (!string.IsNullOrWhiteSpace(cmd.Example)) sb.AppendLine($"<i>Ví dụ:</i> <code>{cmd.Example}</code>");

            sb.AppendLine();
        }

        var menu = sb.ToString();

        await _telegramBotClient.SendMessage(message.Chat, menu, ParseMode.Html, replyMarkup: new ReplyKeyboardRemove());
    }
}