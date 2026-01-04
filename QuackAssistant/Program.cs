using Microsoft.EntityFrameworkCore;
using QuackAssistant.ConfigurationOptions;
using QuackAssistant.Data;
using QuackAssistant.Services;
using QuackAssistant.Shared.Dispatchers;
using Telegram.Bot;
using Telegram.Bot.Polling;

var builder = WebApplication.CreateBuilder(args);

var appSettings = new AppSettings();
builder.Configuration.Bind(appSettings);
builder.Services.Configure<AppSettings>(builder.Configuration);

builder.Services.AddDbContextPool<QuackAssistantDbContext>(options =>
{
    options.UseNpgsql(appSettings.ConnectionStrings.QuackAssistant);
    options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
});

builder.Services
    .AddHttpClient("tgwebhook")
    .RemoveAllLoggers()
    .AddTypedClient<ITelegramBotClient>(httpClient => new TelegramBotClient(appSettings.TelegramBot.BotToken, httpClient));

builder.Services.AddControllers();
builder.Services.AddDispatcher();
builder.Services.AddTransient<IUpdateHandler, UpdateHandler>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.Configure<RouteOptions>(options => options.LowercaseUrls = true);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<QuackAssistantDbContext>();
    await dbContext.Database.MigrateAsync();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();
app.Run();