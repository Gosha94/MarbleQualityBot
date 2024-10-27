using MediatR;
using Microsoft.Extensions.Logging;
using MarbleQualityBot.Core.Domain.Entities;
using Telegram.Bot;

namespace MarbleQualityBot.Core.Features.ProcessTelegramMessage;

public record ProcessTelegramTextCommand(TextMessage Message) : IRequest;

public class ProcessTelegramTextHandler : IRequestHandler<ProcessTelegramTextCommand>
{
    private readonly ITelegramBotClient _botClient;
    private readonly ILogger<ProcessTelegramTextHandler> _logger;

    public ProcessTelegramTextHandler(
        ITelegramBotClient botClient,
        ILogger<ProcessTelegramTextHandler> logger)
    {
        _botClient = botClient;
        _logger = logger;
    }

    public async Task Handle(ProcessTelegramTextCommand request, CancellationToken ct)
    {
        var message = request.Message;
        string response = $"You said: {message.MessageText}";

        await _botClient.SendTextMessageAsync(message.ChatId, response, cancellationToken: ct);
        _logger.LogInformation($"{nameof(ProcessTelegramTextHandler)}: Request was processed, response: '{response}' was sent to ChatId: {message.ChatId}");
    }
}