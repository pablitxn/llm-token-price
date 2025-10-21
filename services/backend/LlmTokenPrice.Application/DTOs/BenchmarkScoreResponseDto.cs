namespace LlmTokenPrice.Application.DTOs;

/// <summary>
/// Response DTO for benchmark score including computed fields.
/// Returned by POST/PUT/GET endpoints for benchmark scores.
/// </summary>
/// <remarks>
/// Includes:
/// - All persisted fields from BenchmarkScore entity
/// - Denormalized BenchmarkName and Category for display convenience
/// - Computed IsOutOfRange flag for UI warnings
/// </remarks>
public record BenchmarkScoreResponseDto
{
    /// <summary>
    /// Unique identifier for the benchmark score.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Model ID this score belongs to.
    /// </summary>
    public required Guid ModelId { get; init; }

    /// <summary>
    /// Benchmark ID this score is for.
    /// </summary>
    public required Guid BenchmarkId { get; init; }

    /// <summary>
    /// Benchmark name (denormalized from Benchmark entity for display convenience).
    /// </summary>
    public required string BenchmarkName { get; init; }

    /// <summary>
    /// Benchmark category (denormalized for grouping in UI).
    /// </summary>
    public required string Category { get; init; }

    /// <summary>
    /// The score value achieved by the model.
    /// </summary>
    public required decimal Score { get; init; }

    /// <summary>
    /// Optional maximum possible score.
    /// </summary>
    public decimal? MaxScore { get; init; }

    /// <summary>
    /// Normalized score (0.0 - 1.0) used for QAPS calculation.
    /// Calculated using: (score - typicalMin) / (typicalMax - typicalMin)
    /// </summary>
    public required decimal NormalizedScore { get; init; }

    /// <summary>
    /// Date when the test was performed.
    /// </summary>
    public DateTime? TestDate { get; init; }

    /// <summary>
    /// URL to the source of the benchmark result.
    /// </summary>
    public string? SourceUrl { get; init; }

    /// <summary>
    /// Whether this score has been verified by administrators.
    /// </summary>
    public required bool Verified { get; init; }

    /// <summary>
    /// Additional notes about the score.
    /// </summary>
    public string? Notes { get; init; }

    /// <summary>
    /// When the score was created (UTC).
    /// </summary>
    public required DateTime CreatedAt { get; init; }

    /// <summary>
    /// Indicates if the score is outside the benchmark's typical range.
    /// Used to display warnings in the UI. Does not prevent submission.
    /// </summary>
    public required bool IsOutOfRange { get; init; }
}
