using System.Text.Json.Serialization;

namespace MarbleQualityBot.Core.Domain.Entities;

public class Inference
{
    [JsonPropertyName("inference_id")]
    public string InferenceId { get; set; }

    [JsonPropertyName("time")]
    public double Time { get; set; }

    [JsonPropertyName("image")]
    public ImageData Image { get; set; }

    [JsonPropertyName("predictions")]
    public List<Prediction> Predictions { get; set; } = new List<Prediction>();
}

public class ImageData
{
    [JsonPropertyName("width")]
    public int Width { get; set; }
    
    [JsonPropertyName("height")]
    public int Height { get; set; }
}

public class Prediction
{
    [JsonPropertyName("x")]
    public double X { get; set; }
    
    [JsonPropertyName("y")]
    public double Y { get; set; }

    [JsonPropertyName("width")]
    public double Width { get; set; }

    [JsonPropertyName("height")]
    public double Height { get; set; }

    [JsonPropertyName("confidence")]
    public double Confidence { get; set; }

    [JsonPropertyName("class")]
    public string Class { get; set; }
    
    [JsonPropertyName("class_id")]
    public int ClassId { get; set; }
    
    [JsonPropertyName("detection_id")]
    public string DetectionId { get; set; }
}