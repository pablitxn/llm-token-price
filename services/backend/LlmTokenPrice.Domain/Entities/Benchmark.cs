using LlmTokenPrice.Domain.Enums;

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
    /// Valid values: Reasoning, Code, Math, Language, Multimodal.
    /// Required field for proper QAPS calculation and benchmark organization.
    /// </summary>
    public BenchmarkCategory Category { get; set; }

    /// <summary>
    /// Interpretation rule for score comparison (HigherBetter or LowerBetter).
    /// Used to normalize scores for QAPS calculation and ranking.
    /// Required field (defaults to HigherBetter if not specified).
    /// </summary>
    public BenchmarkInterpretation Interpretation { get; set; }

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
    /// Weight assigned to this benchmark in QAPS calculation (0.00 to 1.00).
    /// Determines the contribution of this benchmark to the composite quality score.
    /// Stored as decimal(3,2) for precision. Default: 0.00
    /// </summary>
    /// <remarks>
    /// Typical weights by category:
    /// - Reasoning: 0.30, Code: 0.25, Math: 0.20, Language: 0.15, Multimodal: 0.10
    /// Admin can customize per benchmark for fine-tuned quality calculations.
    /// </remarks>
    public decimal WeightInQaps { get; set; }

    /// <summary>
    /// Timestamp when benchmark definition was created. Required audit field.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Soft-delete flag. When false, benchmark is hidden from public queries but preserved for audit trail.
    /// Default: true (active). Set to false instead of hard-deleting for data integrity.
    /// </summary>
    public bool IsActive { get; set; } = true;

    // Navigation Properties

    /// <summary>
    /// One-to-many relationship to benchmark scores across different models.
    /// Collection of all models' performance on this benchmark.
    /// Cascade delete: deleting Benchmark deletes all associated BenchmarkScores.
    /// </summary>
    public ICollection<BenchmarkScore> Scores { get; set; } = new List<BenchmarkScore>();
}
