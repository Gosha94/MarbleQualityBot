using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;
using MarbleQualityBot.Core.Domain.Entities;
using SixLabors.Fonts;

namespace MarbleQualityBot.Core.Features.ProcessObjectOutline.Services;

public class ExpertService : IExpertService
{
    private const int _PADDING_PX = 5;
    private const double _LOWER_THRESHOLD = 0.5;
    private const string _REJECTED_CLASS = "1";

    public Task HighlightPredictionsOnImage(string imagePath, Inference model)
    {
        var classColors = new Dictionary<int, Color>
            {
                { 0, Color.Red },
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

                var x = prediction.X - (prediction.Width / 2);
                var y = prediction.Y - (prediction.Height / 2);
                var width = prediction.Width;
                var height = prediction.Height;

                var rectangle = new RectangleF((float)x, (float)y, (float)width, (float)height);

                image.Mutate(ctx => ctx.Draw(rectangleColor, 2, rectangle));

                // Shows as 'class 0 (53.5%)'
                var label = $"class {prediction.Class} ({prediction.Confidence:P1})";
                var textGraphicsOptions = new RichTextOptions(font) { HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Top };

                var textSize = TextMeasurer.MeasureSize(label, textGraphicsOptions);

                var labelX = x + (width / 2) - (textSize.Width / 2);
                var labelY = y + (height / 2) - (textSize.Height / 2);

                var labelPosition = new PointF((float)labelX, (float)labelY);

                image.Mutate(ctx => ctx.DrawText(label, font, rectangleColor, labelPosition));
            }

            image.Save(imagePath);
        }

        return Task.CompletedTask;
    }

    public Task<List<RejectedMaterialCoordinate>> TryCollectRejectedMaterialsCoordinates(Inference model)
        => Task.FromResult(
                model.Predictions
                    .Where(p => p.Class == _REJECTED_CLASS)
                    .Select(p => new RejectedMaterialCoordinate
                    {
                        CenterX = p.Width != 0 ? Math.Round(p.X + p.Width / 2) : Math.Round(p.X),
                        CenterY = p.Height != 0 ? Math.Round(p.Y + p.Height / 2) : Math.Round(p.Y)
                    })
                    .ToList());

    public Task<Inference> FilterInferenceByThreshold(Inference inference)
    {
        inference.Predictions = inference.Predictions
            .Where(p => p.Confidence > _LOWER_THRESHOLD)
            .ToList();

        return Task.FromResult(inference);
    }
}