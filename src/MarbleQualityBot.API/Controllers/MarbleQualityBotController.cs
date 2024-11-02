using MediatR;
using Microsoft.AspNetCore.Mvc;
using MarbleQualityBot.Core.Domain.Entities;
using MarbleQualityBot.Core.Features.ProcessTelegramImage;
using MarbleQualityBot.Core.Features.ProcessTelegramMessage;
using Telegram.Bot.Types;
using System.Text.Json;
using MarbleQualityBot.DTO.Requests;
using MarbleQualityBot.Core.Features.ProcessObjectOutline;
using MarbleQualityBot.Core.Config;
using Microsoft.Extensions.Options;

namespace MarbleQualityBot.API.Controllers;

[ApiController]
[Route("api/safe-head-backend")]
public class MarbleQualityBotController : ControllerBase
{
    [HttpPost("webhook")]
    public async Task<IActionResult> Post(
        [FromBody] Update update,
        [FromServices] IMediator _mediator)
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

    [HttpPost("outline-objects")]
    public async Task<IActionResult> OutlineObjects(
        [FromForm] OutlineObjectsRequest request,
        [FromServices] IOptions<TelegramBotSettings> _botSettings,
        [FromServices] IMediator _mediator)
    {
        if (request.ImageFile == null || request.ImageFile.Length == 0)
        {
            return BadRequest("Invalid image or predictions data.");
        }

        if (request.PredictionsJsonFile == null || request.PredictionsJsonFile.Length == 0)
        {
            return BadRequest("No predictions JSON file provided.");
        }

        var predictionsExtension = Path.GetExtension(request.PredictionsJsonFile.FileName).ToLowerInvariant();
        if (predictionsExtension != ".json")
        {
            return BadRequest("Only JSON files are allowed for predictions.");
        }

        var allowedExtensions = new[] { ".png", ".jpg", ".jpeg" };

        var fileExtension = Path.GetExtension(request.ImageFile.FileName).ToLower();

        var allowedMimeTypes = new[] { "image/png", "image/jpeg" };

        if (!allowedExtensions.Contains(fileExtension) || !allowedMimeTypes.Contains(request.ImageFile.ContentType))
        {
            return BadRequest("Invalid file type. Only PNG, JPG, and JPEG files are allowed.");
        }
        
        Inference inputModel;
        using (var memoryStream = new MemoryStream())
        {
            await request.PredictionsJsonFile.CopyToAsync(memoryStream);
            memoryStream.Position = 0;
            inputModel = await JsonSerializer.DeserializeAsync<Inference>(memoryStream) ?? new Inference();
        }

        var saveDirectory = Path.Combine(Directory.GetCurrentDirectory(), _botSettings.Value.LocalStoragePath);
        Directory.CreateDirectory(saveDirectory);

        var imagePath = Path.Combine(saveDirectory, request.ImageFile.FileName + '_' + Guid.NewGuid());

        using (var fileStream = new FileStream(imagePath, FileMode.Create))
        {
            await request.ImageFile.CopyToAsync(fileStream);
        }

        var isResultSuccess = await _mediator.Send(new OutlineObjectsCommand(imagePath, inputModel));

        if (!isResultSuccess)
        {
            return BadRequest($"Something went wrong with your request, please check image type and inference json");
        }

        using (var fileStream = new FileStream(imagePath, FileMode.Open))
        using (var outputStream = new MemoryStream())
        {
            fileStream.CopyTo(outputStream);
            return File(outputStream.ToArray(), request.ImageFile.ContentType, request.ImageFile.Name);
        }
    }
}