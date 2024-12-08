﻿using MarbleQualityBot.Core.Domain.Entities;

namespace MarbleQualityBot.Core.Features.ProcessObjectOutline.Services;

public interface IExpertService
{
    Task HighlightPredictionsOnImage(string imagePath, Inference model);

    Task<Inference> FilterInferenceByThreshold(Inference inference);

    Task<List<RejectedMaterialCoordinate>> TryCollectRejectedMaterialsCoordinates(Inference model);
}