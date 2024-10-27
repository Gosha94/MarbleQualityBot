namespace MarbleQualityBot.Core.Domain.Entities;

public class Inference
{
    public Guid InferenceId { get; set; }
    public double Time { get; set; }
    public ImageData Image { get; set; }
    public List<Prediction> Predictions { get; set; } = new List<Prediction>();
}

public class ImageData
{
    public int Width { get; set; }
    public int Height { get; set; }
}

public class Prediction
{
    public double X { get; set; }
    public double Y { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }
    public double Confidence { get; set; }
    public string Class { get; set; }
    public int ClassId { get; set; }
    public Guid DetectionId { get; set; }
}