using Microsoft.Extensions.Options;
using MarbleQualityBot.Core.Config;
using MarbleQualityBot.Core.Features.ProcessTelegramMessage;
using Telegram.Bot;
using MarbleQualityBot.Core.Integrations.Clients;
using MarbleQualityBot.Core.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

builder.Configuration.AddEnvironmentVariables("BOT_TOKEN");
builder.Configuration.AddEnvironmentVariables("API_KEY");

if (!builder.Environment.IsProduction())
{
    builder.Configuration.AddUserSecrets<Program>();
}

builder.Services.Configure<TelegramBotSettings>(builder.Configuration.GetSection("TelegramBotSettings"));

builder.Services.PostConfigure<TelegramBotSettings>(options =>
{
    var secretValue = Environment.GetEnvironmentVariable("BOT_TOKEN");

    if (string.IsNullOrEmpty(secretValue))
    {
        secretValue = builder.Configuration["TelegramBotSettings:Token"];
    }

    options.BotToken = secretValue ?? string.Empty;
    options.FileUrl = options.FileUrl + secretValue;
});

builder.Services.Configure<DetectionApiSettings>(builder.Configuration.GetSection("DetectionApiSettings"));

builder.Services.PostConfigure<DetectionApiSettings>(options =>
{

    var secretValue = Environment.GetEnvironmentVariable("API_KEY");

    if (string.IsNullOrEmpty(secretValue))
    {
        secretValue = builder.Configuration["DetectionApiSettings:ApiKey"];
    }

    options.ApiKey = secretValue ?? string.Empty;
});

builder.Services.AddTransient<IKnowledgeBaseService, KnowledgeBaseService>();
builder.Services.AddTransient<IExpertService, ExpertService>();
builder.Services.AddTransient<IDetectionApi, DetectionApi>();

builder.Services.AddTransient<ITelegramBotClient>(sp =>
{
    var telegramSettings = sp.GetRequiredService<IOptions<TelegramBotSettings>>().Value;
    return new TelegramBotClient(telegramSettings.BotToken);
});

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(ProcessTelegramTextCommand).Assembly));

builder.Services
    .AddControllers()
    .AddNewtonsoftJson();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();