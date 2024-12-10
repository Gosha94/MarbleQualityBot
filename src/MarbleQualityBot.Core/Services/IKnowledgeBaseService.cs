using MarbleQualityBot.Core.Domain.Enums;

namespace MarbleQualityBot.Core.Services;

public interface IKnowledgeBaseService
{
    Task<string> GetSuggestion(int classId, ConfidenceLevel confidenceLevel, CancellationToken ct);
}