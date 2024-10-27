using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MarbleQualityBot.Core.Config;
using MarbleQualityBot.Core.Domain.Entities;
using MarbleQualityBot.Core.Features.ProcessTelegramMessage;
using MarbleQualityBot.Core.Integrations.Clients;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace MarbleQualityBot.Core.Features.ProcessTelegramImage;

public record ProcessTelegramImageCommand(ImageMessage Message) : IRequest;

public class ProcessTelegramImageCommandHandler : IRequestHandler<ProcessTelegramImageCommand>
{
    private readonly ITelegramBotClient _botClient;
    private readonly IDetectionApi _detectionApi;
    private readonly TelegramBotSettings _botSettings;
    private readonly int _maxFileSizeInBytes;
    private readonly ILogger<ProcessTelegramTextHandler> _logger;

    public ProcessTelegramImageCommandHandler(
        ITelegramBotClient botClient,
        IDetectionApi detectionApi,
        IOptions<TelegramBotSettings> botSettings,
        ILogger<ProcessTelegramTextHandler> logger)
    {
        _botClient = botClient;
        _detectionApi = detectionApi;
        _botSettings = botSettings.Value;
        _maxFileSizeInBytes = _botSettings.MaxFileSizeMb * 1024 * 1024;
        _logger = logger;

        if (!Directory.Exists(_botSettings.LocalStoragePath))
        {
            Directory.CreateDirectory(_botSettings.LocalStoragePath);
        }
    }

    public async Task Handle(ProcessTelegramImageCommand request, CancellationToken ct)
    {
        var file = await _botClient.GetFileAsync(request.Message.FileId, ct);
        var fileSize = file.FileSize ?? 0L;

        if (file != null && file.FilePath != null)
        {
            if (file.FileSize > _maxFileSizeInBytes)
            {
                await _botClient.SendTextMessageAsync(request.Message.ChatId,
                    $"File size exceeds the {_botSettings.MaxFileSizeMb} MB limit and has been deleted. Please send a smaller file.");

                return;
            }

            await _botClient.SendTextMessageAsync(request.Message.ChatId, $"Processing your picture...");

            var fullFilePath = $"https://api.telegram.org/file/bot{_botSettings.BotToken}/{file.FilePath}";

            var response = await _detectionApi.DetectFromUrl(fullFilePath);
            await Task.Delay(5000);

            var jsonFileName = $"inference_result_{Guid.NewGuid()}.json";
            await System.IO.File.WriteAllTextAsync(jsonFileName, response);

            using var fileStream = new FileStream(jsonFileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            var inputOnlineFile = InputFile.FromStream(fileStream, jsonFileName);
            await _botClient.SendDocumentAsync(request.Message.ChatId, inputOnlineFile, caption: "Inference Results");

            fileStream.Close();
            System.IO.File.Delete(jsonFileName);
        }
    }
}