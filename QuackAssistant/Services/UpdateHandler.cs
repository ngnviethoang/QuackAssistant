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

    private async Task<Message> Usage(Message msg)
    {
        const string usage = """
                             <b><u>📘 SỔ THU CHI – MENU</u></b>

                             <b>/income</b>   – ghi khoản thu (tiền vào)
                             <i>Ví dụ:</i> <code>/income lương 15tr</code>

                             <b>/expense</b>  – ghi khoản chi (tiền ra)
                             <i>Ví dụ:</i> <code>/expense cafe 35k</code>

                             <b>/today</b>    – xem thu chi hôm nay
                             <i>Ví dụ:</i> <code>/today</code>

                             <b>/date</b>     – xem theo ngày
                             <i>Ví dụ:</i> <code>/date 03-01-2026</code>

                             <b>/range</b>    – xem theo khoảng ngày
                             <i>Ví dụ:</i> <code>/range 01-01-2026 31-01-2026</code>

                             <b>/summary</b>  – tổng kết theo ngày / tháng
                             <i>Ví dụ:</i> <code>/summary month</code>

                             <b>/balance</b>  – xem số dư hiện tại
                             <i>Ví dụ:</i> <code>/balance</code>

                             <b>/edit</b>     – sửa giao dịch
                             <i>Ví dụ:</i> <code>/edit 12 cafe 40k</code>

                             <b>/delete</b>   – xoá giao dịch
                             <i>Ví dụ:</i> <code>/delete 12</code>

                             <b>/export</b>   – xuất dữ liệu (Excel / CSV)
                             <i>Ví dụ:</i> <code>/export month</code>

                             <b>/help</b>     – hướng dẫn sử dụng
                             <i>Ví dụ:</i> <code>/help</code>
                             """;

        return await _telegramBotClient.SendMessage(msg.Chat, usage, ParseMode.Html, replyMarkup: new ReplyKeyboardRemove());
    }
}