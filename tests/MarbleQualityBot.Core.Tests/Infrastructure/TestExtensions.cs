using Bogus;
using MarbleQualityBot.Core.Domain.Entities;

namespace MarbleQualityBot.Core.Tests.Infrastructure;

public static class TestExtensions
{
    public static Inference GenerateRandomInference(int predictionCount = 5)
    {
        var faker = new Faker<Prediction>()
            .RuleFor(p => p.X, f => f.Random.Double(0, 100))
            .RuleFor(p => p.Y, f => f.Random.Double(0, 100))
            .RuleFor(p => p.Width, f => f.Random.Double(5, 50))
            .RuleFor(p => p.Height, f => f.Random.Double(5, 50))
            .RuleFor(p => p.Confidence, f => f.Random.Double(0, 1))
            .RuleFor(p => p.Class, f => f.PickRandom(new[] { "0", "1" }))
            .RuleFor(p => p.ClassId, f => f.Random.Int(0, 3))
            .RuleFor(p => p.DetectionId, f => f.Random.Guid().ToString());

        return new Inference
        {
            InferenceId = Guid.NewGuid().ToString(),
            Time = new Faker().Random.Double(0, 10),
            Image = new ImageData { Width = 1920, Height = 1080 },
            Predictions = faker.Generate(predictionCount)
        };
    }

    public static Inference GenerateCertainInference(int totalPredictions, int badPredictions)
    {
        if (badPredictions > totalPredictions)
            throw new ArgumentException("Bad predictions cannot exceed total predictions.");

        var faker = new Faker<Prediction>()
            .RuleFor(p => p.X, f => f.Random.Double(0, 500))
            .RuleFor(p => p.Y, f => f.Random.Double(0, 500))
            .RuleFor(p => p.Width, f => f.Random.Double(20, 100))
            .RuleFor(p => p.Height, f => f.Random.Double(20, 100))
            .RuleFor(p => p.Confidence, f => f.Random.Double(0.4, 1.0))
            .RuleFor(p => p.DetectionId, f => Guid.NewGuid().ToString())
            .RuleFor(p => p.ClassId, f => f.Random.Int(0, 3))
            .RuleFor(p => p.Class, (f, p) => p.ClassId.ToString());

        var badPredictionsList = faker.Clone()
            .RuleFor(p => p.Class, _ => "1") // Class 1: bad material
            .RuleFor(p => p.ClassId, _ => 1)
            .RuleFor(p => p.Confidence, f => f.Random.Double(0.5, 1.0))
            .Generate(badPredictions);

        var goodPredictionsList = faker.Clone()
            .RuleFor(p => p.Class, f => f.Random.Int(0, 2).ToString())
            .RuleFor(p => p.ClassId, f => f.Random.Int(0, 2))
            .Generate(totalPredictions - badPredictions);

        var allPredictions = badPredictionsList.Concat(goodPredictionsList).ToList();

        return new Inference
        {
            InferenceId = Guid.NewGuid().ToString(),
            Time = new Random().NextDouble() * 10,
            Image = new ImageData
            {
                Width = 1920,
                Height = 1080
            },
            Predictions = allPredictions
        };
    }
}