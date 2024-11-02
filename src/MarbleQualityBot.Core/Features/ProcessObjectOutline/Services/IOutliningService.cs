using MarbleQualityBot.Core.Domain.Entities;

namespace MarbleQualityBot.Core.Features.ProcessObjectOutline.Services;

public interface IOutliningService
{
    void DrawPredictionsOnImage(string imagePath, Inference model);
}