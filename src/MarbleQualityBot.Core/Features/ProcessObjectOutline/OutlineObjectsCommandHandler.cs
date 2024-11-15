using MediatR;
using Microsoft.Extensions.Logging;
using MarbleQualityBot.Core.Domain.Entities;
using MarbleQualityBot.Core.Features.ProcessObjectOutline.Services;

namespace MarbleQualityBot.Core.Features.ProcessObjectOutline;

public record OutlineObjectsCommand(string ImagePath, Inference PredictionModel) : IRequest<bool>;

public class OutlineObjectsCommandHandler : IRequestHandler<OutlineObjectsCommand, bool>
{
    private readonly IExpertService _outliningService;
    private readonly ILogger<OutlineObjectsCommandHandler> _logger;

    public OutlineObjectsCommandHandler(
        IExpertService outliningService,
        ILogger<OutlineObjectsCommandHandler> logger)
    {
        _outliningService = outliningService;
        _logger = logger;
    }

    public async Task<bool> Handle(OutlineObjectsCommand request, CancellationToken ct)
    {
        _outliningService.HighlightPredictionsOnImage(request.ImagePath, request.PredictionModel);

        _logger.LogInformation($"Image was outlined, saved instead an original one here: {request.ImagePath}");
        return true;
    }
}