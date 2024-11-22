using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MarbleQualityBot.Core.Config;
using MarbleQualityBot.Core.Domain.Entities;
using MarbleQualityBot.Core.Features.ProcessTelegramMessage;
using MarbleQualityBot.Core.Integrations.Clients;
using Telegram.Bot;
using System.Text.Json;
using MarbleQualityBot.Core.Features.ProcessObjectOutline.Services;
using Telegram.Bot.Types;
using Microsoft.VisualBasic;
using System.Text;

namespace MarbleQualityBot.Core.Features.ProcessTelegramImage;

public record ProcessTelegramImageCommand(ImageMessage Message) : IRequest;

public class ProcessTelegramImageCommandHandler : IRequestHandler<ProcessTelegramImageCommand>
{
    private readonly ITelegramBotClient _botClient;
    private readonly IDetectionApi _detectionApi;
    private readonly IExpertService _expertSystem;
    private readonly TelegramBotSettings _botSettings;
    private readonly int _maxFileSizeInBytes;
    private readonly ILogger<ProcessTelegramTextHandler> _logger;
    
    public ProcessTelegramImageCommandHandler(
        ITelegramBotClient botClient,
        IDetectionApi detectionApi,
        IExpertService outliningService,
        IOptions<TelegramBotSettings> botSettings,
        ILogger<ProcessTelegramTextHandler> logger)
    {
        _botClient = botClient;
        _detectionApi = detectionApi;
        _expertSystem = outliningService;
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
                    $"File size exceeds the {_botSettings.MaxFileSizeMb} MB limit and has been deleted. Please send a smaller file.", cancellationToken: ct);

                return;
            }

            await _botClient.SendTextMessageAsync(request.Message.ChatId, $"Processing your picture...", cancellationToken: ct);

            var fullFilePath = $"https://api.telegram.org/file/bot{_botSettings.BotToken}/{file.FilePath}";

            // Just request throttling
            await Task.Delay(2000);

            var response = await _detectionApi.DetectFromUrl(fullFilePath);
            var inferenceModel = JsonSerializer.Deserialize<Inference>(response) ?? new Inference();

            var filteredInference = await _expertSystem.FilterInferenceByThreshold(inferenceModel);

            var currentProcessTraceGuid = Guid.NewGuid();

            var localImagePath = Path.Combine(Directory.GetCurrentDirectory(), _botSettings.LocalStoragePath, currentProcessTraceGuid + Path.GetExtension(file.FilePath));

            using (var httpClient = new HttpClient())
            {
                var imageBytes = await httpClient.GetByteArrayAsync(fullFilePath);

                await System.IO.File.WriteAllBytesAsync(localImagePath, imageBytes);
            }

            await _expertSystem.HighlightPredictionsOnImage(localImagePath, filteredInference);

            using (var imageFileStream = new FileStream(localImagePath, FileMode.Open, FileAccess.Read))
            {
                var inputFileStream = InputFile.FromStream(imageFileStream);

                var message = await _botClient.SendPhotoAsync(
                    chatId: request.Message.ChatId,
                    photo: inputFileStream,
                    caption: "Here is your outlined image!",
                    cancellationToken: ct
                );

                _logger.LogInformation($"Outlined image sent successfully! Message ID: {message.MessageId}");
            }

            var rejectedRockCoordinates = await _expertSystem.TryCollectRejectedMaterialsCoordinates(filteredInference);

            if (rejectedRockCoordinates.Any())
            {
                var sb = new StringBuilder();
                sb.AppendLine("Please, use these coordinates below to remove rejected rocks:");

                rejectedRockCoordinates
                    .ForEach(rc =>
                        {
                            sb.AppendLine($"X={rc.CenterX}, Y={rc.CenterY}");
                        });

                await _botClient.SendTextMessageAsync(request.Message.ChatId, sb.ToString(), cancellationToken: ct);
            }

            var jsonFileName = $"detection_result_{currentProcessTraceGuid}.json";
            var localJsonFilePath = Path.Combine(Directory.GetCurrentDirectory(), _botSettings.LocalStoragePath, jsonFileName);
            await System.IO.File.WriteAllTextAsync(localJsonFilePath, response);

            // Just delay before deleting image
            await Task.Delay(1000);

            System.IO.File.Delete(localJsonFilePath);
            System.IO.File.Delete(localImagePath);
        }
    }
}