using MarbleQualityBot.Core.Domain.Entities;

namespace MarbleQualityBot.Core.Services;

public interface IExpertService
{
    Task HighlightPredictionsOnImage(string imagePath, Inference model, CancellationToken ct);

    Task<Inference> FilterInferenceByThreshold(Inference inference, CancellationToken ct);

    Task<List<ExpertSuggestion>> TryCollectExpertSuggestions(Inference model, CancellationToken ct);
}