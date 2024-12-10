using MarbleQualityBot.Core.Domain.Entities;
using MarbleQualityBot.Core.Domain.Enums;
using MarbleQualityBot.Core.Services;

namespace MarbleQualityBot.Core.Tests.Services;

public class KnowledgeBaseServiceTests
{
    private readonly KnowledgeBaseService _knowledgeBaseService;

    public KnowledgeBaseServiceTests()
    {
        _knowledgeBaseService = new KnowledgeBaseService();
    }

    [Theory]
    [InlineData(0, ConfidenceLevel.HIGH, "Stone should be crushed for premium countertops, flooring, and decorative sculptures.")]
    [InlineData(0, ConfidenceLevel.MEDIUM, "Stone can be used for interior wall cladding or medium-quality tiles.")]
    [InlineData(0, ConfidenceLevel.LOW, "Stone quality is uncertain; consider using it for prototypes or rough structures.")]
    [InlineData(1, ConfidenceLevel.HIGH, "Stone is suitable for paving pathways or constructing garden borders.")]
    [InlineData(1, ConfidenceLevel.MEDIUM, "Stone may be used for temporary structures or low-traffic areas like patios.")]
    [InlineData(1, ConfidenceLevel.LOW, "Stone may be ground into aggregate for basic concrete or filler material.")]
    public async Task GetSuggestion_ShouldReturnExpectedSuggestion(int classId, ConfidenceLevel confidenceLevel, string expectedSuggestion)
    {
        // Arrange
        var cancellationToken = CancellationToken.None;

        // Act
        var suggestion = await _knowledgeBaseService.GetSuggestion(classId, confidenceLevel, cancellationToken);

        // Assert
        Assert.Equal(expectedSuggestion, suggestion);
    }

    [Theory]
    [InlineData(2, ConfidenceLevel.HIGH)]
    [InlineData(0, (ConfidenceLevel)999)]
    [InlineData(-1, ConfidenceLevel.LOW)]
    public async Task GetSuggestion_ShouldReturnDefaultForUnknownClassOrConfidence(int classId, ConfidenceLevel confidenceLevel)
    {
        // Arrange
        var cancellationToken = CancellationToken.None;

        // Act
        var suggestion = await _knowledgeBaseService.GetSuggestion(classId, confidenceLevel, cancellationToken);

        // Assert
        Assert.Equal("No suggestion available.", suggestion);
    }
}