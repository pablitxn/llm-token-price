namespace LlmTokenPrice.Application.DTOs;

/// <summary>
/// Response DTO for benchmark score operations (GET, POST, PUT).
/// Returns detailed information about a model's benchmark score including metadata.
/// </summary>
/// <remarks>
/// Includes both persisted database fields and computed fields for UI convenience:
/// - NormalizedScore: Calculated using BenchmarkNormalizer for QAPS algorithm
/// - IsOutOfRange: Computed flag to trigger UI warnings for unusual scores
/// - BenchmarkName: Denormalized from Benchmark entity for display efficiency
///
/// Used in:
/// - POST /api/admin/models/{id}/benchmarks response (201 Created)
/// - GET /api/admin/models/{id}/benchmarks response (200 OK)
/// - PUT /api/admin/models/{modelId}/benchmarks/{scoreId} response (200 OK)
/// </remarks>
public record BenchmarkScoreResponseDto
{
    /// <summary>
    /// Unique identifier for this benchmark score record.
    /// Generated server-side on creation.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// ID of the model this score belongs to.
    /// Foreign key to models table.
    /// </summary>
    public required Guid ModelId { get; init; }

    /// <summary>
    /// ID of the benchmark being scored.
    /// Foreign key to benchmarks table.
    /// </summary>
    public required Guid BenchmarkId { get; init; }

    /// <summary>
    /// Display name of the benchmark (denormalized for efficiency).
    /// Example: "MMLU", "HumanEval", "GSM8K".
    /// </summary>
    /// <remarks>
    /// Denormalized from Benchmark.BenchmarkName to avoid JOIN in common queries.
    /// Reduces API roundtrips for UI rendering.
    /// </remarks>
    public required string BenchmarkName { get; init; }

    /// <summary>
    /// Category of the benchmark (denormalized for display).
    /// Example: "Reasoning", "Code", "Math", "Language", "Multimodal".
    /// </summary>
    public string? Category { get; init; }

    /// <summary>
    /// Raw benchmark score from evaluation.
    /// Example: 87.5 for 87.5% accuracy on MMLU.
    /// </summary>
    public required decimal Score { get; init; }

    /// <summary>
    /// Maximum possible score if applicable (e.g., 100 for percentages).
    /// Null if benchmark doesn't have a fixed maximum.
    /// </summary>
    public decimal? MaxScore { get; init; }

    /// <summary>
    /// Normalized score in [0, 1] range for QAPS calculation.
    /// Calculated using: (Score - TypicalRangeMin) / (TypicalRangeMax - TypicalRangeMin).
    /// Clamped to [0, 1] to handle outliers.
    /// </summary>
    /// <remarks>
    /// This is the value used in QAPS weighted averaging.
    /// Pre-computed and stored for performance (avoid recalculation on every QAPS query).
    /// </remarks>
    public required decimal NormalizedScore { get; init; }

    /// <summary>
    /// Date when the benchmark test was conducted.
    /// Null if date not recorded. Stored in UTC.
    /// </summary>
    public DateTime? TestDate { get; init; }

    /// <summary>
    /// URL to the source of the benchmark result (paper, leaderboard, etc.).
    /// Null if not provided.
    /// </summary>
    public string? SourceUrl { get; init; }

    /// <summary>
    /// Indicates if the score is verified/official (true) or community-submitted (false).
    /// Default: false.
    /// </summary>
    /// <remarks>
    /// Verified scores are displayed with a badge in the UI to indicate reliability.
    /// </remarks>
    public required bool Verified { get; init; }

    /// <summary>
    /// Additional context or notes about the score.
    /// Example: "5-shot evaluation using official eval harness".
    /// </summary>
    public string? Notes { get; init; }

    /// <summary>
    /// Timestamp when this score was created (UTC).
    /// Set automatically on insert, never updated.
    /// </summary>
    public required DateTime CreatedAt { get; init; }

    /// <summary>
    /// Computed flag indicating if Score falls outside the benchmark's TypicalRange.
    /// Used to trigger warnings in the admin UI.
    /// </summary>
    /// <remarks>
    /// True if Score &lt; TypicalRangeMin OR Score &gt; TypicalRangeMax.
    /// Does NOT block submission - admin can override for legitimate outliers.
    /// Calculated server-side using BenchmarkNormalizer.IsWithinTypicalRange().
    /// </remarks>
    public required bool IsOutOfRange { get; init; }
}
