using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using QuackAssistant.ConfigurationOptions;
using Swashbuckle.AspNetCore.Annotations;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;

namespace QuackAssistant.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TelegramBotController : ControllerBase
{
    private readonly ILogger<TelegramBotController> _logger;
    private readonly ITelegramBotClient _telegramBotClient;
    private readonly TelegramBotOptions _telegramBotOptions;
    private readonly IUpdateHandler _updateHandler;

    public TelegramBotController(
        IOptions<AppSettings> appSettings,
        ITelegramBotClient telegramBotClient,
        IUpdateHandler updateHandler,
        ILogger<TelegramBotController> logger)
    {
        _telegramBotClient = telegramBotClient;
        _updateHandler = updateHandler;
        _logger = logger;
        _telegramBotOptions = appSettings.Value.TelegramBot;
    }

    [HttpPost("webhook")]
    public async Task<IActionResult> SetWebHookAsync()
    {
        var webhookUrl = _telegramBotOptions.WebhookUrl;
        try
        {
            await _telegramBotClient.SetWebhook(
                webhookUrl,
                allowedUpdates: [],
                secretToken: _telegramBotOptions.SecretToken);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, exception.Message);
            return BadRequest(exception.Message);
        }

        return Ok($"Webhook set to {webhookUrl}");
    }

    [HttpGet("webhook")]
    public async Task<IActionResult> GetWebHookAsync()
    {
        var result = await _telegramBotClient.GetWebhookInfo();
        return Ok(result);
    }

    [HttpDelete("webhook")]
    public async Task<IActionResult> DeleteWebHookAsync()
    {
        await _telegramBotClient.DeleteWebhook();
        return Ok();
    }

    [HttpPost("update")]
    [SwaggerIgnore]
    public async Task<IActionResult> UpdateAsync([FromBody] Update update)
    {
        var cancellationToken = Request.HttpContext.RequestAborted;
        if (Request.Headers["X-Telegram-Bot-Api-Secret-Token"] != _telegramBotOptions.SecretToken) return Forbid();

        try
        {
            await _updateHandler.HandleUpdateAsync(_telegramBotClient, update, cancellationToken);
        }
        catch (Exception exception)
        {
            await _updateHandler.HandleErrorAsync(_telegramBotClient, exception, HandleErrorSource.HandleUpdateError, cancellationToken);
        }

        return Ok();
    }
}