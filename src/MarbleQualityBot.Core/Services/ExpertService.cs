using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;
using MarbleQualityBot.Core.Domain.Entities;
using SixLabors.Fonts;
using MarbleQualityBot.Core.Domain.Enums;
using AngleSharp.Text;

namespace MarbleQualityBot.Core.Services;

public class ExpertService : IExpertService
{
    private const int _PADDING_PX = 5;
    private const double _LOWER_THRESHOLD = 0.35;
    private const string _REJECTED_CLASS = "1";

    private readonly IKnowledgeBaseService _knowledgeBaseService;

    public ExpertService(IKnowledgeBaseService knowledgeBaseService)
    {
        _knowledgeBaseService = knowledgeBaseService;
    }

    public Task HighlightPredictionsOnImage(string imagePath, Inference model, CancellationToken ct)
    {
        var classColors = new Dictionary<int, Color>
            {
                // Good stones
                { 0, Color.Red },
                // Bad ones
                { 1, Color.Blue },
                { 2, Color.Green },
                { 3, Color.Yellow }
            };

        using (Image image = Image.Load(imagePath))
        {
            var fontCollection = new FontCollection();
            var fontPath = Path.Combine(Directory.GetCurrentDirectory(), "Fonts", "Amatic-Bold.ttf");
            var font = fontCollection.Add(fontPath).CreateFont(36);

            foreach (var prediction in model.Predictions)
            {
                Color rectangleColor = classColors.ContainsKey(prediction.ClassId) ? classColors[prediction.ClassId] : Color.LightPink;

                var x = prediction.X - prediction.Width / 2;
                var y = prediction.Y - prediction.Height / 2;
                var width = prediction.Width;
                var height = prediction.Height;

                var rectangle = new RectangleF((float)x, (float)y, (float)width, (float)height);

                image.Mutate(ctx => ctx.Draw(rectangleColor, 2, rectangle));

                // Shows as 'class 0 (53.5%)'
                var label = $"class {prediction.Class} ({prediction.Confidence:P1})";
                var textGraphicsOptions = new RichTextOptions(font) { HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Top };

                var textSize = TextMeasurer.MeasureSize(label, textGraphicsOptions);

                var labelX = x + width / 2 - textSize.Width / 2;
                var labelY = y + height / 2 - textSize.Height / 2;

                var labelPosition = new PointF((float)labelX, (float)labelY);

                image.Mutate(ctx => ctx.DrawText(label, font, rectangleColor, labelPosition));
            }

            image.Save(imagePath);
        }

        return Task.CompletedTask;
    }

    public async Task<List<ExpertSuggestion>> TryCollectExpertSuggestions(Inference model, CancellationToken ct)
    {
        var tasks = model.Predictions
            .Where(p => int.TryParse(p.Class, out _))
            .Select(async prediction =>
            {
                var classId = int.Parse(prediction.Class);
                return new ExpertSuggestion
                {
                    Suggestion = await GetPredictionSuggestion(classId, prediction.Confidence, ct),
                    CenterX = prediction.Width != 0 ? Math.Round(prediction.X + prediction.Width / 2) : Math.Round(prediction.X),
                    CenterY = prediction.Height != 0 ? Math.Round(prediction.Y + prediction.Height / 2) : Math.Round(prediction.Y)
                };
            });

        var suggestions = await Task.WhenAll(tasks);

        return suggestions
            .OrderBy(s => s.Suggestion)
            .ToList();
    }

    public Task<Inference> FilterInferenceByThreshold(Inference inference, CancellationToken ct)
    {
        inference.Predictions = inference.Predictions
            .Where(p => p.Confidence > _LOWER_THRESHOLD)
            .ToList();

        return Task.FromResult(inference);
    }

    private Task<string> GetPredictionSuggestion(int classId, double confidence, CancellationToken ct)
    {
        ConfidenceLevel confidenceLevel = confidence switch
        {
            >= 0.8 => ConfidenceLevel.HIGH,
            >= 0.5 => ConfidenceLevel.MEDIUM,
            _ => ConfidenceLevel.LOW,
        };

        return _knowledgeBaseService.GetSuggestion(classId, confidenceLevel, ct);
    }
}