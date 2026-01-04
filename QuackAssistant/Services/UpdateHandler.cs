using GenerativeAI;
using Microsoft.Extensions.Options;
using QuackAssistant.ConfigurationOptions;
using QuackAssistant.Data;
using QuackAssistant.Shared.Dispatchers;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace QuackAssistant.Services;

public class UpdateHandler : IUpdateHandler
{
    private readonly QuackAssistantDbContext _dbContext;
    private readonly IDispatcher _dispatcher;
    private readonly GoogleAi _googleAi;
    private readonly ILogger<UpdateHandler> _logger;
    private readonly ITelegramBotClient _telegramBotClient;

    public UpdateHandler(
        ITelegramBotClient telegramBotClient,
        ILogger<UpdateHandler> logger,
        QuackAssistantDbContext dbContext,
        IOptions<AppSettings> appSettings,
        IDispatcher dispatcher)
    {
        _telegramBotClient = telegramBotClient;
        _logger = logger;
        _dbContext = dbContext;
        _dispatcher = dispatcher;
        _googleAi = new GoogleAi(appSettings.Value.GeminiAi.ApiKey);
    }

    public async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource source,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("HandleError: {Exception}", exception);
        // Cooldown in case of network connection error
        if (exception is RequestException) await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
    }

    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (update.Message != null) await _dispatcher.DispatchAsync(update.Message);
    }
}