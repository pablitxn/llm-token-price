namespace LlmTokenPrice.Domain.Entities;

/// <summary>
/// Represents a benchmark/evaluation metric for LLM performance measurement.
/// This is a pure domain entity with no infrastructure dependencies.
/// </summary>
/// <remarks>
/// Benchmarks are reference datasets used to evaluate model capabilities (MMLU, HumanEval, GSM8K, etc.).
/// One-to-many relationship with BenchmarkScore (one benchmark has many scores from different models).
/// Entity configuration via Fluent API in Infrastructure layer (BenchmarkConfiguration.cs).
/// Table name: benchmarks (snake_case per PostgreSQL conventions).
/// </remarks>
public class Benchmark
{
    /// <summary>
    /// Unique identifier for the benchmark (UUID/GUID).
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Short benchmark identifier (e.g., "MMLU", "HumanEval", "GSM8K").
    /// Required field with unique constraint. Used as primary display name.
    /// </summary>
    public string BenchmarkName { get; set; } = null!;

    /// <summary>
    /// Full descriptive name of the benchmark (e.g., "Massive Multitask Language Understanding").
    /// Optional field for detailed display and documentation.
    /// </summary>
    public string? FullName { get; set; }

    /// <summary>
    /// Detailed description of what the benchmark measures and methodology.
    /// Optional field for user education and context.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Category classification for benchmark grouping and QAPS weighting.
    /// Valid values: "reasoning", "code", "math", "language", "multimodal".
    /// Optional field allowing null for uncategorized benchmarks.
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// Interpretation rule for score comparison ("higher_better" or "lower_better").
    /// Used to normalize scores for QAPS calculation and ranking.
    /// Optional field (defaults to "higher_better" if not specified).
    /// </summary>
    public string? Interpretation { get; set; }

    /// <summary>
    /// Minimum value in typical score range (e.g., 0 for percentages, 1 for rankings).
    /// Stored as decimal(5,2) for score normalization in QAPS algorithm.
    /// Optional field (some benchmarks have unbounded ranges).
    /// </summary>
    public decimal? TypicalRangeMin { get; set; }

    /// <summary>
    /// Maximum value in typical score range (e.g., 100 for percentages).
    /// Stored as decimal(5,2) for score normalization in QAPS algorithm.
    /// Optional field (some benchmarks have unbounded ranges).
    /// </summary>
    public decimal? TypicalRangeMax { get; set; }

    /// <summary>
    /// Timestamp when benchmark definition was created. Required audit field.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    // Navigation Properties

    /// <summary>
    /// One-to-many relationship to benchmark scores across different models.
    /// Collection of all models' performance on this benchmark.
    /// Cascade delete: deleting Benchmark deletes all associated BenchmarkScores.
    /// </summary>
    public ICollection<BenchmarkScore> Scores { get; set; } = new List<BenchmarkScore>();
}
