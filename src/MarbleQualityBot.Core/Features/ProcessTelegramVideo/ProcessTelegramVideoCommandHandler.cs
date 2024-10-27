using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MarbleQualityBot.Core.Config;
using MarbleQualityBot.Core.Domain.Entities;
using MarbleQualityBot.Core.Features.ProcessTelegramMessage;
using Telegram.Bot;
using Telegram.Bot.Exceptions;

namespace MarbleQualityBot.Core.Features.ProcessTelegramImage;

public record ProcessTelegramVideoCommand(VideoMessage Message) : MediatR.IRequest;

public class ProcessTelegramVideoCommandHandler : IRequestHandler<ProcessTelegramVideoCommand>
{
    private readonly ITelegramBotClient _botClient;
    private readonly ILogger<ProcessTelegramTextHandler> _logger;
    private readonly TelegramBotSettings _botSettings;
    private readonly int _maxFileSizeInBytes;
    public ProcessTelegramVideoCommandHandler(
        ITelegramBotClient botClient,
        IOptions<TelegramBotSettings> botSettings,
        ILogger<ProcessTelegramTextHandler> logger)
    {
        _botClient = botClient;
        _botSettings = botSettings.Value;
        _maxFileSizeInBytes = _botSettings.MaxFileSizeMb * 1024 * 1024;
        _logger = logger;

        if (!Directory.Exists(_botSettings.LocalStoragePath))
        {
            Directory.CreateDirectory(_botSettings.LocalStoragePath);
        }
    }

    public async Task Handle(ProcessTelegramVideoCommand request, CancellationToken ct)
    {
        try
        {
            var file = await _botClient.GetFileAsync(request.Message.FileId, ct);

            var fileSize = file.FileSize ?? 0L;
            var filePath = file.FilePath;

            if (file != null && filePath != null)
            {
                if (file.FileSize > _maxFileSizeInBytes)
                {
                    await _botClient.SendTextMessageAsync(request.Message.ChatId,
                        $"File size exceeds the {_botSettings.MaxFileSizeMb} MB limit and has been deleted. Please send a smaller file.");

                    return;
                }

                var localFilePath = Path.Combine(_botSettings.LocalStoragePath, file.FileUniqueId + Path.GetExtension(filePath));

                using (var fileStream = new FileStream(localFilePath, FileMode.Create))
                {
                    await _botClient.DownloadFileAsync(filePath, fileStream);
                }

                var fileUrl = $"{_botSettings.FileUrl}/{filePath}";

                _logger.LogInformation($"Received File from chatId: {request.Message.ChatId}, saved locally: {localFilePath}");

                File.Delete(localFilePath);

                await _botClient.SendTextMessageAsync(request.Message.ChatId, $"Got a video");
            }
        }
        catch (ApiRequestException ex)
        {
            if (ex.Message.Contains("Bad Request: file is too big"))
            {
                await _botClient.SendTextMessageAsync(request.Message.ChatId, $"The file you are trying to send is too large. Please ensure it is under {_botSettings.MaxFileSizeMb} MB.");
            }
            else
            {
                throw;
            }
        }
    }
}