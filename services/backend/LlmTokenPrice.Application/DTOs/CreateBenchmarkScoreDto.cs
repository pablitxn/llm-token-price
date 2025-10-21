namespace LlmTokenPrice.Application.DTOs;

/// <summary>
/// Request payload for creating a new benchmark score for a model.
/// Used by POST /api/admin/models/{modelId}/benchmarks endpoint.
/// Validated using FluentValidation (CreateBenchmarkScoreValidator).
/// </summary>
/// <remarks>
/// All fields are validated server-side:
/// - BenchmarkId: Required GUID
/// - Score: Required decimal value
/// - MaxScore: Optional, must be >= Score if provided
/// - SourceUrl: Optional, must be valid URL format
/// - Notes: Optional, max 500 characters
/// </remarks>
public record CreateBenchmarkScoreDto
{
    /// <summary>
    /// The benchmark ID to add score for.
    /// Required.
    /// </summary>
    public required Guid BenchmarkId { get; init; }

    /// <summary>
    /// The score value achieved by the model.
    /// Required. Can be outside typical range (admin override allowed).
    /// </summary>
    public required decimal Score { get; init; }

    /// <summary>
    /// Optional maximum possible score (for percentage-based benchmarks).
    /// If provided, must be >= Score.
    /// </summary>
    public decimal? MaxScore { get; init; }

    /// <summary>
    /// Date when the test was performed.
    /// Optional. Defaults to current UTC date if not provided.
    /// </summary>
    public DateTime? TestDate { get; init; }

    /// <summary>
    /// Optional URL to the source of the benchmark result.
    /// Must be valid URL format if provided.
    /// </summary>
    public string? SourceUrl { get; init; }

    /// <summary>
    /// Whether this score has been verified by administrators.
    /// Default: false.
    /// </summary>
    public bool Verified { get; init; } = false;

    /// <summary>
    /// Optional notes about the benchmark score.
    /// Max 500 characters.
    /// </summary>
    public string? Notes { get; init; }
}
