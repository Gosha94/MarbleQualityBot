using MediatR;
using Microsoft.AspNetCore.Mvc;
using MarbleQualityBot.Core.Domain.Entities;
using MarbleQualityBot.Core.Features.ProcessTelegramImage;
using MarbleQualityBot.Core.Features.ProcessTelegramMessage;
using Telegram.Bot.Types;

namespace MarbleQualityBot.API.Controllers;

[ApiController]
[Route("api/safe-head-backend")]
public class MarbleQualityBotController : ControllerBase
{
    [HttpPost("webhook")]
    public async Task<IActionResult> Post([FromBody] Update update, [FromServices] IMediator _mediator)
    {
        if (update == null)
        {
            return BadRequest("The update field is required.");
        }

        if (update?.Message != null)
        {
            var chatId = update.Message?.Chat.Id;
            var msgId = update.Message?.MessageId;

            if (!string.IsNullOrEmpty(update.Message.Text) && chatId.HasValue && msgId.HasValue)
            {
                var botMessage = new TextMessage(chatId.Value, msgId.Value, update.Message.Text ?? string.Empty);
                await _mediator.Send(new ProcessTelegramTextCommand(botMessage));
            }

            if (update.Message.Photo != null)
            {
                var photo = update.Message?.Photo?.Last();
                var botMessage = new ImageMessage(chatId.Value, photo.FileId, msgId.Value);
                await _mediator.Send(new ProcessTelegramImageCommand(botMessage));
            }

            if (update.Message.Video != null)
            {
                var video = update.Message?.Video;
                var botMessage = new VideoMessage(chatId.Value, video.FileId, msgId.Value);
                await _mediator.Send(new ProcessTelegramVideoCommand(botMessage));
            }
        }

        return Ok();
    }
}