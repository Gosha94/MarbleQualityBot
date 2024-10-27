namespace MarbleQualityBot.Core.Integrations.Clients;

public interface IDetectionApi
{
    Task<string> DetectFromUrl(string imageUrl);

    Task<string> DetectFromPath(string filePath);
}