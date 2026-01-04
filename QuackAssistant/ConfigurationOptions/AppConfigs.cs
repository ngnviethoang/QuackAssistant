namespace QuackAssistant.ConfigurationOptions;

public class AppSettings
{
    public TelegramBotOptions TelegramBot { get; set; }

    public ConnectionStrings ConnectionStrings { get; set; }

    public GeminiAiOptions GeminiAi { get; set; }
}