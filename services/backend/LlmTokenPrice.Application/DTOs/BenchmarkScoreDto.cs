namespace LlmTokenPrice.Application.DTOs;

/// <summary>
/// Data Transfer Object representing a benchmark score for API responses.
/// Contains benchmark name and score information without internal entity details.
/// </summary>
/// <remarks>
/// Used in ModelDto to display top benchmark scores.
/// Mapped from BenchmarkScore entity in Application layer services.
/// </remarks>
public record BenchmarkScoreDto
{
    /// <summary>
    /// Name of the benchmark (e.g., "MMLU", "HumanEval", "GSM8K").
    /// </summary>
    public required string BenchmarkName { get; init; }

    /// <summary>
    /// Raw score value from the benchmark evaluation.
    /// </summary>
    public required decimal Score { get; init; }

    /// <summary>
    /// Maximum possible score for the benchmark (e.g., 100 for percentages).
    /// Null if benchmark doesn't have a fixed maximum.
    /// </summary>
    public decimal? MaxScore { get; init; }

    /// <summary>
    /// Normalized score in range [0, 1] for QAPS calculation.
    /// Null if normalization hasn't been computed yet.
    /// </summary>
    public decimal? NormalizedScore { get; init; }
}
