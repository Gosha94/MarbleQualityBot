namespace MarbleQualityBot.Core.Domain.Entities;

public class ExpertSuggestion
{
    public string Suggestion {  get; set; } = string.Empty;

    public double CenterX { get; set; }

    public double CenterY { get; set; }
}