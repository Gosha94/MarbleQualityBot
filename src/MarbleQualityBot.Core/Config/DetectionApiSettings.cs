namespace MarbleQualityBot.Core.Config;

public class DetectionApiSettings
{
    public string Url { get; set; }

    public string DatasetId { get; set; }

    public string VersionId { get; set; }

    public string ApiKey { get; set; }

    public double Confidence { get; set; }
}