namespace LlmTokenPrice.Application.DTOs;

/// <summary>
/// Request payload for adding a benchmark score to a model.
/// Used by POST /api/admin/models/{modelId}/benchmarks endpoint.
/// Validated using FluentValidation (CreateBenchmarkScoreValidator).
/// </summary>
/// <remarks>
/// Server-side validation enforces:
/// - BenchmarkId: Required (must reference existing benchmark)
/// - Score: Required (finite number)
/// - MaxScore: Optional, must be >= Score if provided
/// - SourceUrl: Optional, must be valid URL format if provided
/// - Notes: Optional, max 500 characters
///
/// Business Rules:
/// - Unique constraint: One score per (ModelId, BenchmarkId) combination
/// - Out-of-range warning: Score outside TypicalRange shows warning but allows submission
/// - NormalizedScore: Calculated server-side using BenchmarkNormalizer, not user input
/// </remarks>
public record CreateBenchmarkScoreDto
{
    /// <summary>
    /// ID of the benchmark being scored (e.g., MMLU, HumanEval).
    /// Required. Must reference an existing active benchmark.
    /// </summary>
    public required Guid BenchmarkId { get; init; }

    /// <summary>
    /// Raw benchmark score from evaluation (e.g., 87.5 for 87.5% on MMLU).
    /// Required. Validated to be a finite number.
    /// </summary>
    /// <remarks>
    /// If score falls outside the benchmark's TypicalRange, a warning is shown in the UI,
    /// but submission is allowed (admin can override for legitimate outlier scores).
    /// </remarks>
    public required decimal Score { get; init; }

    /// <summary>
    /// Maximum possible score for percentage-based benchmarks (e.g., 100 for percentages).
    /// Optional. If provided, must be >= Score.
    /// </summary>
    /// <example>
    /// For "75 out of 100 questions correct", Score=75, MaxScore=100.
    /// </example>
    public decimal? MaxScore { get; init; }

    /// <summary>
    /// Date when the benchmark test was conducted.
    /// Optional. Defaults to current date if not provided.
    /// </summary>
    /// <remarks>
    /// Useful for tracking score changes over time as models are updated.
    /// Stored in UTC in the database.
    /// </remarks>
    public DateTime? TestDate { get; init; }

    /// <summary>
    /// URL to the source of the benchmark result (e.g., paper, leaderboard, blog post).
    /// Optional. Validated to be a valid URL format if provided.
    /// </summary>
    /// <example>
    /// "https://huggingface.co/spaces/HuggingFaceH4/open_llm_leaderboard"
    /// </example>
    public string? SourceUrl { get; init; }

    /// <summary>
    /// Indicates if the score is verified/official (true) or community-submitted (false).
    /// Optional. Defaults to false.
    /// </summary>
    /// <remarks>
    /// Verified scores are displayed with a badge in the UI.
    /// Admins should verify official scores from model creators/papers.
    /// </remarks>
    public bool Verified { get; init; } = false;

    /// <summary>
    /// Additional context or notes about the benchmark score.
    /// Optional. Max 500 characters.
    /// </summary>
    /// <example>
    /// "5-shot evaluation using official eval harness"
    /// </example>
    public string? Notes { get; init; }
}
