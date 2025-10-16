namespace LlmTokenPrice.Domain.Entities;

/// <summary>
/// Represents a model's performance score on a specific benchmark.
/// This is a pure domain entity with no infrastructure dependencies.
/// </summary>
/// <remarks>
/// Join entity between Model and Benchmark with additional score metadata.
/// Many-to-one relationship with Model (one model has many benchmark scores).
/// Many-to-one relationship with Benchmark (one benchmark has scores from many models).
/// Entity configuration via Fluent API in Infrastructure layer (BenchmarkScoreConfiguration.cs).
/// Table name: model_benchmark_scores (snake_case per PostgreSQL conventions).
/// </remarks>
public class BenchmarkScore
{
    /// <summary>
    /// Unique identifier for the benchmark score record (UUID/GUID).
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Foreign key to the associated Model.
    /// Part of composite unique constraint (ModelId, BenchmarkId).
    /// </summary>
    public Guid ModelId { get; set; }

    /// <summary>
    /// Foreign key to the associated Benchmark.
    /// Part of composite unique constraint (ModelId, BenchmarkId).
    /// </summary>
    public Guid BenchmarkId { get; set; }

    /// <summary>
    /// Raw score value from the benchmark evaluation.
    /// Stored as decimal(6,2) for precision (e.g., 87.5, 0.95, 100.00).
    /// Required field representing actual test performance.
    /// </summary>
    public decimal Score { get; set; }

    /// <summary>
    /// Maximum possible score for the benchmark (e.g., 100 for percentages).
    /// Optional field used for score context and normalization.
    /// Stored as decimal(6,2) matching Score precision.
    /// </summary>
    public decimal? MaxScore { get; set; }

    /// <summary>
    /// Normalized score in range [0, 1] for QAPS calculation.
    /// Calculated using: (Score - Min) / (Max - Min) per benchmark's typical range.
    /// Stored as decimal(5,4) for high precision (e.g., 0.8750).
    /// Optional field (computed on-demand or cached for performance).
    /// </summary>
    public decimal? NormalizedScore { get; set; }

    /// <summary>
    /// Date when the benchmark test was conducted.
    /// Optional field for tracking score freshness and historical performance.
    /// </summary>
    public DateTime? TestDate { get; set; }

    /// <summary>
    /// URL to the source of the benchmark score (paper, leaderboard, provider docs).
    /// Optional field for verification and citation.
    /// </summary>
    public string? SourceUrl { get; set; }

    /// <summary>
    /// Indicates whether score has been manually verified for accuracy.
    /// Defaults to false. Used to flag community-submitted vs. verified scores.
    /// </summary>
    public bool Verified { get; set; } = false;

    /// <summary>
    /// Additional notes about the score (test conditions, caveats, methodology).
    /// Optional field for context and disclaimers.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Timestamp when score record was created. Required audit field.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    // Navigation Properties

    /// <summary>
    /// Many-to-one navigation property to the parent Model.
    /// Required navigation (null-forgiving operator) because score cannot exist without Model.
    /// </summary>
    public Model Model { get; set; } = null!;

    /// <summary>
    /// Many-to-one navigation property to the associated Benchmark.
    /// Required navigation (null-forgiving operator) because score cannot exist without Benchmark.
    /// </summary>
    public Benchmark Benchmark { get; set; } = null!;
}
