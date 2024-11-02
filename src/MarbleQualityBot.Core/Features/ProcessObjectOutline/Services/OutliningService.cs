using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;
using MarbleQualityBot.Core.Domain.Entities;
using SixLabors.Fonts;
using System.Reflection.Emit;

namespace MarbleQualityBot.Core.Features.ProcessObjectOutline.Services;

public class OutliningService : IOutliningService
{
    public const int _paddingPx = 5;

    public void DrawPredictionsOnImage(string imagePath, Inference model)
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
    }
}
