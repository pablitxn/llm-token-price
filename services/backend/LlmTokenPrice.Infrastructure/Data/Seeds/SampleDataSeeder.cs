using LlmTokenPrice.Domain.Entities;
using LlmTokenPrice.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LlmTokenPrice.Infrastructure.Data.Seeds;

/// <summary>
/// Seeds the database with sample LLM models, benchmarks, and benchmark scores for development and testing.
/// </summary>
/// <remarks>
/// This seeder implements idempotency checks to prevent duplicate data on multiple runs.
/// It creates:
/// - 5 benchmark definitions (MMLU, HumanEval, GSM8K, HELM, MT-Bench)
/// - 10 sample LLM models from 5 providers (OpenAI, Anthropic, Google, Meta, Mistral)
/// - Model capabilities for each model
/// - Benchmark scores (minimum 3 scores per model)
///
/// Data is based on realistic pricing and publicly available benchmark results as of January 2025.
/// </remarks>
public static class SampleDataSeeder
{
    /// <summary>
    /// Seeds the database with sample data if it's empty.
    /// </summary>
    /// <param name="context">The application database context.</param>
    /// <param name="logger">Optional logger for diagnostic messages.</param>
    public static async Task SeedAsync(AppDbContext context, ILogger? logger = null)
    {
        // Idempotency check: If data already exists, skip seeding
        if (await context.Models.AnyAsync())
        {
            logger?.LogInformation("Database already contains model data. Skipping seed.");
            return;
        }

        logger?.LogInformation("Seeding database with sample data...");

        try
        {
            // Create benchmarks first (referenced by benchmark scores)
            var benchmarks = CreateBenchmarks();
            await context.Benchmarks.AddRangeAsync(benchmarks);
            await context.SaveChangesAsync();
            logger?.LogInformation("Seeded {Count} benchmarks", benchmarks.Count);

            // Create models with capabilities and benchmark scores
            var models = CreateModelsWithCapabilitiesAndScores(benchmarks);
            await context.Models.AddRangeAsync(models);
            await context.SaveChangesAsync();
            logger?.LogInformation("Seeded {Count} models with capabilities and benchmark scores", models.Count);

            logger?.LogInformation("Database seeded successfully");
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "An error occurred while seeding the database");
            throw;
        }
    }

    /// <summary>
    /// Creates 5 benchmark definitions covering different capability categories.
    /// </summary>
    private static List<Benchmark> CreateBenchmarks()
    {
        // Ensure DateTime is marked as UTC to avoid PostgreSQL timezone issues
        var now = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);

        return new List<Benchmark>
        {
            new Benchmark
            {
                Id = Guid.NewGuid(),
                BenchmarkName = "MMLU",
                FullName = "Massive Multitask Language Understanding",
                Description = "Measures general knowledge and reasoning across 57 academic subjects including STEM, humanities, and social sciences.",
                Category = BenchmarkCategory.Reasoning,
                Interpretation = BenchmarkInterpretation.HigherBetter,
                WeightInQaps = 0.30m,
                TypicalRangeMin = 0,
                TypicalRangeMax = 100,
                CreatedAt = now
            },
            new Benchmark
            {
                Id = Guid.NewGuid(),
                BenchmarkName = "HumanEval",
                FullName = "HumanEval Code Generation",
                Description = "Evaluates code generation capability using 164 handwritten programming problems with unit tests.",
                Category = BenchmarkCategory.Code,
                Interpretation = BenchmarkInterpretation.HigherBetter,
                WeightInQaps = 0.25m,
                TypicalRangeMin = 0,
                TypicalRangeMax = 100,
                CreatedAt = now
            },
            new Benchmark
            {
                Id = Guid.NewGuid(),
                BenchmarkName = "GSM8K",
                FullName = "Grade School Math 8K",
                Description = "Tests mathematical reasoning with 8,500 grade school math word problems requiring multi-step solutions.",
                Category = BenchmarkCategory.Math,
                Interpretation = BenchmarkInterpretation.HigherBetter,
                WeightInQaps = 0.20m,
                TypicalRangeMin = 0,
                TypicalRangeMax = 100,
                CreatedAt = now
            },
            new Benchmark
            {
                Id = Guid.NewGuid(),
                BenchmarkName = "HELM",
                FullName = "Holistic Evaluation of Language Models",
                Description = "Comprehensive evaluation framework measuring accuracy, calibration, robustness, fairness, and efficiency.",
                Category = BenchmarkCategory.Language,
                Interpretation = BenchmarkInterpretation.HigherBetter,
                WeightInQaps = 0.15m,
                TypicalRangeMin = 0,
                TypicalRangeMax = 100,
                CreatedAt = now
            },
            new Benchmark
            {
                Id = Guid.NewGuid(),
                BenchmarkName = "MT-Bench",
                FullName = "Multi-Turn Benchmark",
                Description = "Evaluates conversational and instruction-following capabilities through multi-turn dialogues.",
                Category = BenchmarkCategory.Reasoning,
                Interpretation = BenchmarkInterpretation.HigherBetter,
                WeightInQaps = 0.30m,
                TypicalRangeMin = 0,
                TypicalRangeMax = 10,
                CreatedAt = now
            }
        };
    }

    /// <summary>
    /// Creates a benchmark score with validation against the benchmark's typical range.
    /// </summary>
    /// <param name="benchmark">The benchmark definition.</param>
    /// <param name="score">The score value to validate.</param>
    /// <param name="maxScore">The maximum possible score for this benchmark.</param>
    /// <param name="verified">Whether the score has been verified.</param>
    /// <param name="createdAt">The creation timestamp.</param>
    /// <returns>A validated BenchmarkScore entity.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when score is outside the benchmark's typical range.</exception>
    private static BenchmarkScore CreateScore(Benchmark benchmark, decimal score, decimal maxScore, bool verified, DateTime createdAt)
    {
        if (score < benchmark.TypicalRangeMin || score > benchmark.TypicalRangeMax)
        {
            throw new ArgumentOutOfRangeException(
                nameof(score),
                score,
                $"Score {score} is outside the typical range {benchmark.TypicalRangeMin}-{benchmark.TypicalRangeMax} for benchmark '{benchmark.BenchmarkName}'");
        }

        return new BenchmarkScore
        {
            Id = Guid.NewGuid(),
            BenchmarkId = benchmark.Id,
            Score = score,
            MaxScore = maxScore,
            CreatedAt = createdAt,
            Verified = verified
        };
    }

    /// <summary>
    /// Creates 10 sample LLM models with complete data (pricing, capabilities, benchmark scores).
    /// </summary>
    private static List<Model> CreateModelsWithCapabilitiesAndScores(List<Benchmark> benchmarks)
    {
        // Ensure DateTime is marked as UTC to avoid PostgreSQL timezone issues
        var now = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
        var models = new List<Model>();

        // Retrieve benchmarks by name for score assignment
        var mmlu = benchmarks.First(b => b.BenchmarkName == "MMLU");
        var humanEval = benchmarks.First(b => b.BenchmarkName == "HumanEval");
        var gsm8k = benchmarks.First(b => b.BenchmarkName == "GSM8K");
        var helm = benchmarks.First(b => b.BenchmarkName == "HELM");
        var mtBench = benchmarks.First(b => b.BenchmarkName == "MT-Bench");

        // 1. GPT-4 (OpenAI flagship model)
        models.Add(new Model
        {
            Id = Guid.NewGuid(),
            Name = "GPT-4",
            Provider = "OpenAI",
            Version = "gpt-4-0125-preview",
            ReleaseDate = DateTime.SpecifyKind(new DateTime(2023, 3, 14), DateTimeKind.Utc),
            Status = "active",
            InputPricePer1M = 30.00M,
            OutputPricePer1M = 60.00M,
            Currency = "USD",
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now,
            Capability = new Capability
            {
                Id = Guid.NewGuid(),
                ContextWindow = 128000,
                MaxOutputTokens = 4096,
                SupportsFunctionCalling = true,
                SupportsVision = true,
                SupportsAudioInput = false,
                SupportsAudioOutput = false,
                SupportsStreaming = true,
                SupportsJsonMode = true
            },
            BenchmarkScores = new List<BenchmarkScore>
            {
                CreateScore(mmlu, 86.4M, 100, true, now),
                CreateScore(humanEval, 67.0M, 100, true, now),
                CreateScore(gsm8k, 92.0M, 100, true, now),
                CreateScore(mtBench, 9.0M, 10, true, now)
            }
        });

        // 2. GPT-3.5 Turbo (OpenAI cost-effective model)
        models.Add(new Model
        {
            Id = Guid.NewGuid(),
            Name = "GPT-3.5 Turbo",
            Provider = "OpenAI",
            Version = "gpt-3.5-turbo-0125",
            ReleaseDate = DateTime.SpecifyKind(new DateTime(2022, 11, 30), DateTimeKind.Utc),
            Status = "active",
            InputPricePer1M = 0.50M,
            OutputPricePer1M = 1.50M,
            Currency = "USD",
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now,
            Capability = new Capability
            {
                Id = Guid.NewGuid(),
                ContextWindow = 16385,
                MaxOutputTokens = 4096,
                SupportsFunctionCalling = true,
                SupportsVision = false,
                SupportsAudioInput = false,
                SupportsAudioOutput = false,
                SupportsStreaming = true,
                SupportsJsonMode = true
            },
            BenchmarkScores = new List<BenchmarkScore>
            {
                new BenchmarkScore { Id = Guid.NewGuid(), BenchmarkId = mmlu.Id, Score = 70.0M, MaxScore = 100, CreatedAt = now, Verified = true },
                new BenchmarkScore { Id = Guid.NewGuid(), BenchmarkId = humanEval.Id, Score = 48.1M, MaxScore = 100, CreatedAt = now, Verified = true },
                new BenchmarkScore { Id = Guid.NewGuid(), BenchmarkId = gsm8k.Id, Score = 57.1M, MaxScore = 100, CreatedAt = now, Verified = true }
            }
        });

        // 3. Claude 3 Opus (Anthropic flagship model)
        models.Add(new Model
        {
            Id = Guid.NewGuid(),
            Name = "Claude 3 Opus",
            Provider = "Anthropic",
            Version = "claude-3-opus-20240229",
            ReleaseDate = DateTime.SpecifyKind(new DateTime(2024, 2, 29), DateTimeKind.Utc),
            Status = "active",
            InputPricePer1M = 15.00M,
            OutputPricePer1M = 75.00M,
            Currency = "USD",
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now,
            Capability = new Capability
            {
                Id = Guid.NewGuid(),
                ContextWindow = 200000,
                MaxOutputTokens = 4096,
                SupportsFunctionCalling = true,
                SupportsVision = true,
                SupportsAudioInput = false,
                SupportsAudioOutput = false,
                SupportsStreaming = true,
                SupportsJsonMode = false
            },
            BenchmarkScores = new List<BenchmarkScore>
            {
                new BenchmarkScore { Id = Guid.NewGuid(), BenchmarkId = mmlu.Id, Score = 86.8M, MaxScore = 100, CreatedAt = now, Verified = true },
                new BenchmarkScore { Id = Guid.NewGuid(), BenchmarkId = humanEval.Id, Score = 84.9M, MaxScore = 100, CreatedAt = now, Verified = true },
                new BenchmarkScore { Id = Guid.NewGuid(), BenchmarkId = gsm8k.Id, Score = 95.0M, MaxScore = 100, CreatedAt = now, Verified = true },
                new BenchmarkScore { Id = Guid.NewGuid(), BenchmarkId = mtBench.Id, Score = 8.8M, MaxScore = 10, CreatedAt = now, Verified = true }
            }
        });

        // 4. Claude 3 Sonnet (Anthropic balanced model)
        models.Add(new Model
        {
            Id = Guid.NewGuid(),
            Name = "Claude 3 Sonnet",
            Provider = "Anthropic",
            Version = "claude-3-sonnet-20240229",
            ReleaseDate = DateTime.SpecifyKind(new DateTime(2024, 2, 29), DateTimeKind.Utc),
            Status = "active",
            InputPricePer1M = 3.00M,
            OutputPricePer1M = 15.00M,
            Currency = "USD",
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now,
            Capability = new Capability
            {
                Id = Guid.NewGuid(),
                ContextWindow = 200000,
                MaxOutputTokens = 4096,
                SupportsFunctionCalling = true,
                SupportsVision = true,
                SupportsAudioInput = false,
                SupportsAudioOutput = false,
                SupportsStreaming = true,
                SupportsJsonMode = false
            },
            BenchmarkScores = new List<BenchmarkScore>
            {
                new BenchmarkScore { Id = Guid.NewGuid(), BenchmarkId = mmlu.Id, Score = 79.0M, MaxScore = 100, CreatedAt = now, Verified = true },
                new BenchmarkScore { Id = Guid.NewGuid(), BenchmarkId = humanEval.Id, Score = 73.0M, MaxScore = 100, CreatedAt = now, Verified = true },
                new BenchmarkScore { Id = Guid.NewGuid(), BenchmarkId = gsm8k.Id, Score = 92.3M, MaxScore = 100, CreatedAt = now, Verified = true }
            }
        });

        // 5. Claude 3 Haiku (Anthropic fast model)
        models.Add(new Model
        {
            Id = Guid.NewGuid(),
            Name = "Claude 3 Haiku",
            Provider = "Anthropic",
            Version = "claude-3-haiku-20240307",
            ReleaseDate = DateTime.SpecifyKind(new DateTime(2024, 3, 7), DateTimeKind.Utc),
            Status = "active",
            InputPricePer1M = 0.25M,
            OutputPricePer1M = 1.25M,
            Currency = "USD",
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now,
            Capability = new Capability
            {
                Id = Guid.NewGuid(),
                ContextWindow = 200000,
                MaxOutputTokens = 4096,
                SupportsFunctionCalling = true,
                SupportsVision = true,
                SupportsAudioInput = false,
                SupportsAudioOutput = false,
                SupportsStreaming = true,
                SupportsJsonMode = false
            },
            BenchmarkScores = new List<BenchmarkScore>
            {
                new BenchmarkScore { Id = Guid.NewGuid(), BenchmarkId = mmlu.Id, Score = 75.2M, MaxScore = 100, CreatedAt = now, Verified = true },
                new BenchmarkScore { Id = Guid.NewGuid(), BenchmarkId = humanEval.Id, Score = 75.9M, MaxScore = 100, CreatedAt = now, Verified = true },
                new BenchmarkScore { Id = Guid.NewGuid(), BenchmarkId = gsm8k.Id, Score = 88.9M, MaxScore = 100, CreatedAt = now, Verified = true }
            }
        });

        // 6. Gemini 1.5 Pro (Google flagship model)
        models.Add(new Model
        {
            Id = Guid.NewGuid(),
            Name = "Gemini 1.5 Pro",
            Provider = "Google",
            Version = "gemini-1.5-pro-001",
            ReleaseDate = DateTime.SpecifyKind(new DateTime(2024, 2, 15), DateTimeKind.Utc),
            Status = "active",
            InputPricePer1M = 3.50M,
            OutputPricePer1M = 10.50M,
            Currency = "USD",
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now,
            Capability = new Capability
            {
                Id = Guid.NewGuid(),
                ContextWindow = 1000000,
                MaxOutputTokens = 8192,
                SupportsFunctionCalling = true,
                SupportsVision = true,
                SupportsAudioInput = true,
                SupportsAudioOutput = false,
                SupportsStreaming = true,
                SupportsJsonMode = true
            },
            BenchmarkScores = new List<BenchmarkScore>
            {
                new BenchmarkScore { Id = Guid.NewGuid(), BenchmarkId = mmlu.Id, Score = 83.7M, MaxScore = 100, CreatedAt = now, Verified = true },
                new BenchmarkScore { Id = Guid.NewGuid(), BenchmarkId = humanEval.Id, Score = 71.9M, MaxScore = 100, CreatedAt = now, Verified = true },
                new BenchmarkScore { Id = Guid.NewGuid(), BenchmarkId = gsm8k.Id, Score = 91.7M, MaxScore = 100, CreatedAt = now, Verified = true },
                new BenchmarkScore { Id = Guid.NewGuid(), BenchmarkId = helm.Id, Score = 78.5M, MaxScore = 100, CreatedAt = now, Verified = true }
            }
        });

        // 7. Gemini 1.5 Flash (Google fast model)
        models.Add(new Model
        {
            Id = Guid.NewGuid(),
            Name = "Gemini 1.5 Flash",
            Provider = "Google",
            Version = "gemini-1.5-flash-001",
            ReleaseDate = DateTime.SpecifyKind(new DateTime(2024, 5, 14), DateTimeKind.Utc),
            Status = "active",
            InputPricePer1M = 0.35M,
            OutputPricePer1M = 1.05M,
            Currency = "USD",
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now,
            Capability = new Capability
            {
                Id = Guid.NewGuid(),
                ContextWindow = 1000000,
                MaxOutputTokens = 8192,
                SupportsFunctionCalling = true,
                SupportsVision = true,
                SupportsAudioInput = true,
                SupportsAudioOutput = false,
                SupportsStreaming = true,
                SupportsJsonMode = true
            },
            BenchmarkScores = new List<BenchmarkScore>
            {
                new BenchmarkScore { Id = Guid.NewGuid(), BenchmarkId = mmlu.Id, Score = 78.9M, MaxScore = 100, CreatedAt = now, Verified = true },
                new BenchmarkScore { Id = Guid.NewGuid(), BenchmarkId = humanEval.Id, Score = 74.3M, MaxScore = 100, CreatedAt = now, Verified = true },
                new BenchmarkScore { Id = Guid.NewGuid(), BenchmarkId = gsm8k.Id, Score = 86.5M, MaxScore = 100, CreatedAt = now, Verified = true }
            }
        });

        // 8. Llama 3 70B (Meta open-source flagship)
        models.Add(new Model
        {
            Id = Guid.NewGuid(),
            Name = "Llama 3 70B",
            Provider = "Meta",
            Version = "llama-3-70b-instruct",
            ReleaseDate = DateTime.SpecifyKind(new DateTime(2024, 4, 18), DateTimeKind.Utc),
            Status = "active",
            InputPricePer1M = 0.00M,  // Open source, free to self-host
            OutputPricePer1M = 0.00M,
            Currency = "USD",
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now,
            Capability = new Capability
            {
                Id = Guid.NewGuid(),
                ContextWindow = 8192,
                MaxOutputTokens = 2048,
                SupportsFunctionCalling = false,
                SupportsVision = false,
                SupportsAudioInput = false,
                SupportsAudioOutput = false,
                SupportsStreaming = true,
                SupportsJsonMode = false
            },
            BenchmarkScores = new List<BenchmarkScore>
            {
                new BenchmarkScore { Id = Guid.NewGuid(), BenchmarkId = mmlu.Id, Score = 82.0M, MaxScore = 100, CreatedAt = now, Verified = true },
                new BenchmarkScore { Id = Guid.NewGuid(), BenchmarkId = humanEval.Id, Score = 81.7M, MaxScore = 100, CreatedAt = now, Verified = true },
                new BenchmarkScore { Id = Guid.NewGuid(), BenchmarkId = gsm8k.Id, Score = 93.0M, MaxScore = 100, CreatedAt = now, Verified = true }
            }
        });

        // 9. Llama 3 8B (Meta open-source small model)
        models.Add(new Model
        {
            Id = Guid.NewGuid(),
            Name = "Llama 3 8B",
            Provider = "Meta",
            Version = "llama-3-8b-instruct",
            ReleaseDate = DateTime.SpecifyKind(new DateTime(2024, 4, 18), DateTimeKind.Utc),
            Status = "active",
            InputPricePer1M = 0.00M,  // Open source, free to self-host
            OutputPricePer1M = 0.00M,
            Currency = "USD",
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now,
            Capability = new Capability
            {
                Id = Guid.NewGuid(),
                ContextWindow = 8192,
                MaxOutputTokens = 2048,
                SupportsFunctionCalling = false,
                SupportsVision = false,
                SupportsAudioInput = false,
                SupportsAudioOutput = false,
                SupportsStreaming = true,
                SupportsJsonMode = false
            },
            BenchmarkScores = new List<BenchmarkScore>
            {
                new BenchmarkScore { Id = Guid.NewGuid(), BenchmarkId = mmlu.Id, Score = 68.4M, MaxScore = 100, CreatedAt = now, Verified = true },
                new BenchmarkScore { Id = Guid.NewGuid(), BenchmarkId = humanEval.Id, Score = 62.2M, MaxScore = 100, CreatedAt = now, Verified = true },
                new BenchmarkScore { Id = Guid.NewGuid(), BenchmarkId = gsm8k.Id, Score = 79.6M, MaxScore = 100, CreatedAt = now, Verified = true }
            }
        });

        // 10. Mistral Large (Mistral flagship model)
        models.Add(new Model
        {
            Id = Guid.NewGuid(),
            Name = "Mistral Large",
            Provider = "Mistral",
            Version = "mistral-large-latest",
            ReleaseDate = DateTime.SpecifyKind(new DateTime(2024, 2, 26), DateTimeKind.Utc),
            Status = "active",
            InputPricePer1M = 4.00M,
            OutputPricePer1M = 12.00M,
            Currency = "USD",
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now,
            Capability = new Capability
            {
                Id = Guid.NewGuid(),
                ContextWindow = 32000,
                MaxOutputTokens = 8192,
                SupportsFunctionCalling = true,
                SupportsVision = false,
                SupportsAudioInput = false,
                SupportsAudioOutput = false,
                SupportsStreaming = true,
                SupportsJsonMode = true
            },
            BenchmarkScores = new List<BenchmarkScore>
            {
                new BenchmarkScore { Id = Guid.NewGuid(), BenchmarkId = mmlu.Id, Score = 81.2M, MaxScore = 100, CreatedAt = now, Verified = true },
                new BenchmarkScore { Id = Guid.NewGuid(), BenchmarkId = humanEval.Id, Score = 45.1M, MaxScore = 100, CreatedAt = now, Verified = true },
                new BenchmarkScore { Id = Guid.NewGuid(), BenchmarkId = gsm8k.Id, Score = 81.2M, MaxScore = 100, CreatedAt = now, Verified = true },
                new BenchmarkScore { Id = Guid.NewGuid(), BenchmarkId = mtBench.Id, Score = 8.6M, MaxScore = 10, CreatedAt = now, Verified = true }
            }
        });

        return models;
    }
}
