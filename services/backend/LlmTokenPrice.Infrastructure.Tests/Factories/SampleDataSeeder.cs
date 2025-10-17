using Bogus;
using LlmTokenPrice.Domain.Entities;

namespace LlmTokenPrice.Infrastructure.Tests.Factories;

/// <summary>
/// AC#8: Factory class that creates valid test entities using Bogus faker library.
/// Provides realistic sample data for Models, Capabilities, Benchmarks, and BenchmarkScores.
/// </summary>
public static class SampleDataSeeder
{
    private static readonly Faker Faker = new();

    /// <summary>
    /// AC#8: Creates a valid Model entity with realistic data.
    /// </summary>
    /// <param name="modelName">Optional model name override</param>
    /// <param name="providerName">Optional provider name override</param>
    /// <returns>A Model entity with valid timestamps and relationships</returns>
    public static Model CreateModel(string? modelName = null, string? providerName = null)
    {
        var providers = new[] { "OpenAI", "Anthropic", "Google", "Meta", "Mistral", "Cohere", "AI21" };
        var contextSizes = new[] { 4096, 8192, 16384, 32768, 128000, 200000 };

        return new Model
        {
            Id = Guid.NewGuid(),
            Name = modelName ?? Faker.Random.Words(2) + "-" + Faker.Random.AlphaNumeric(4),
            Provider = providerName ?? Faker.PickRandom(providers),
            InputPricePer1M = decimal.Round(Faker.Random.Decimal(0.01m, 50m), 2),
            OutputPricePer1M = decimal.Round(Faker.Random.Decimal(0.01m, 100m), 2),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true
        };
    }

    /// <summary>
    /// AC#8: Creates a valid Capability entity linked to a model.
    /// </summary>
    /// <param name="modelId">The model ID this capability belongs to</param>
    /// <returns>A Capability entity with realistic feature flags</returns>
    public static Capability CreateCapability(Guid modelId)
    {
        return new Capability
        {
            Id = Guid.NewGuid(),
            ModelId = modelId,
            ContextWindow = Faker.PickRandom(new[] { 4096, 8192, 16384, 32768, 128000, 200000 }),
            SupportsVision = Faker.Random.Bool(0.3f),       // 30% chance
            SupportsAudioInput = Faker.Random.Bool(0.2f),   // 20% chance
            SupportsAudioOutput = Faker.Random.Bool(0.2f),  // 20% chance
            SupportsFunctionCalling = Faker.Random.Bool(0.7f), // 70% chance
            SupportsJsonMode = Faker.Random.Bool(0.8f),     // 80% chance
            SupportsStreaming = Faker.Random.Bool(0.9f)     // 90% chance
        };
    }

    /// <summary>
    /// AC#8: Creates a valid Benchmark entity with standard definitions.
    /// </summary>
    /// <param name="benchmarkName">Optional benchmark name (e.g., "MMLU", "HumanEval")</param>
    /// <param name="category">Optional category (e.g., "Reasoning", "Code", "Math")</param>
    /// <returns>A Benchmark entity with description and category</returns>
    public static Benchmark CreateBenchmark(string? benchmarkName = null, string? category = null)
    {
        var standardBenchmarks = new Dictionary<string, string>
        {
            { "MMLU", "Reasoning" },
            { "HumanEval", "Code" },
            { "GSM8K", "Math" },
            { "HELM", "Language" },
            { "MT-Bench", "Language" },
            { "Big-Bench Hard", "Reasoning" },
            { "MBPP", "Code" },
            { "TruthfulQA", "Language" },
            { "HellaSwag", "Language" },
            { "MATH", "Math" }
        };

        string name;
        string benchmarkCategory;

        if (benchmarkName != null)
        {
            name = benchmarkName;
            benchmarkCategory = category ?? standardBenchmarks.GetValueOrDefault(benchmarkName, "General");
        }
        else
        {
            var entry = Faker.PickRandom(standardBenchmarks.ToList());
            name = entry.Key;
            benchmarkCategory = entry.Value;
        }

        return new Benchmark
        {
            Id = Guid.NewGuid(),
            BenchmarkName = name,
            Description = $"Measures {benchmarkCategory.ToLower()} capabilities using {name} dataset",
            Category = benchmarkCategory,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// AC#8: Creates a valid BenchmarkScore entity with realistic score ranges.
    /// Validates score is between 0.0 and 100.0 (or benchmark-specific max).
    /// </summary>
    /// <param name="modelId">The model ID this score belongs to</param>
    /// <param name="benchmarkId">The benchmark ID being scored</param>
    /// <param name="score">Optional score override (will be validated)</param>
    /// <returns>A BenchmarkScore entity with valid score and timestamps</returns>
    public static BenchmarkScore CreateBenchmarkScore(Guid modelId, Guid benchmarkId, decimal? score = null)
    {
        decimal scoreValue;

        if (score.HasValue)
        {
            // Validate provided score is in acceptable range
            if (score.Value < 0 || score.Value > 100)
            {
                throw new ArgumentOutOfRangeException(nameof(score),
                    $"Score must be between 0.0 and 100.0. Provided: {score.Value}");
            }

            scoreValue = score.Value;
        }
        else
        {
            // Generate realistic score based on typical LLM performance distributions
            // Most models score between 60-95 on modern benchmarks
            scoreValue = decimal.Round(Faker.Random.Decimal(60m, 95m), 2);
        }

        return new BenchmarkScore
        {
            Id = Guid.NewGuid(),
            ModelId = modelId,
            BenchmarkId = benchmarkId,
            Score = scoreValue,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// AC#8: Creates a complete Model with Capability and N BenchmarkScores.
    /// Useful for tests that need fully populated entities with relationships.
    /// </summary>
    /// <param name="benchmarks">List of benchmarks to create scores for</param>
    /// <param name="scoreCount">Number of benchmark scores to create (defaults to all benchmarks)</param>
    /// <returns>A Model with Capability and BenchmarkScores</returns>
    public static Model CreateModelWithRelationships(List<Benchmark> benchmarks, int? scoreCount = null)
    {
        var model = CreateModel();
        model.Capability = CreateCapability(model.Id);

        var scoresToCreate = scoreCount ?? benchmarks.Count;
        var selectedBenchmarks = Faker.PickRandom(benchmarks, Math.Min(scoresToCreate, benchmarks.Count));

        model.BenchmarkScores = selectedBenchmarks
            .Select(b => CreateBenchmarkScore(model.Id, b.Id))
            .ToList();

        return model;
    }

    /// <summary>
    /// AC#8: Creates a set of standard benchmarks used across tests.
    /// Includes the 5 most common benchmarks: MMLU, HumanEval, GSM8K, HELM, MT-Bench.
    /// </summary>
    /// <returns>List of 5 standard Benchmark entities</returns>
    public static List<Benchmark> CreateStandardBenchmarks()
    {
        return new List<Benchmark>
        {
            CreateBenchmark("MMLU", "Reasoning"),
            CreateBenchmark("HumanEval", "Code"),
            CreateBenchmark("GSM8K", "Math"),
            CreateBenchmark("HELM", "Language"),
            CreateBenchmark("MT-Bench", "Language")
        };
    }

    /// <summary>
    /// AC#8: Creates a set of N models with full relationships (Capability + 3 random BenchmarkScores each).
    /// Useful for populating test database with realistic datasets.
    /// </summary>
    /// <param name="count">Number of models to create</param>
    /// <param name="benchmarks">Benchmarks to assign scores for</param>
    /// <returns>List of complete Model entities</returns>
    public static List<Model> CreateModels(int count, List<Benchmark> benchmarks)
    {
        return Enumerable.Range(0, count)
            .Select(_ => CreateModelWithRelationships(benchmarks, scoreCount: 3))
            .ToList();
    }
}
