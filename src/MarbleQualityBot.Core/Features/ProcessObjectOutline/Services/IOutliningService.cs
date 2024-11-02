using MarbleQualityBot.Core.Domain.Entities;

namespace MarbleQualityBot.Core.Features.ProcessObjectOutline.Services;

public interface IOutliningService
{
    Task DrawPredictionsOnImage(string imagePath, Inference model);
}