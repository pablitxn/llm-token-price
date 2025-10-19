namespace LlmTokenPrice.Application.DTOs;

/// <summary>
/// Request payload for creating a new benchmark definition.
/// Used by POST /api/admin/benchmarks endpoint.
/// Validated using FluentValidation (CreateBenchmarkValidator).
/// </summary>
/// <remarks>
/// All fields are validated server-side:
/// - BenchmarkName: Required, max 50 chars, alphanumeric + underscore, UNIQUE (case-insensitive)
/// - Category: Required enum (Reasoning, Code, Math, Language, Multimodal)
/// - Interpretation: Required enum (HigherBetter, LowerBetter)
/// - TypicalRangeMin must be less than TypicalRangeMax
/// - WeightInQaps: 0.00 to 1.00, max 2 decimal places
/// </remarks>
public record CreateBenchmarkRequest
{
    /// <summary>
    /// Short benchmark identifier (e.g., "MMLU", "HumanEval", "GSM8K").
    /// Required, max 50 characters, alphanumeric + underscore only.
    /// Must be unique (case-insensitive). Primary display name.
    /// </summary>
    public required string BenchmarkName { get; init; }

    /// <summary>
    /// Full descriptive name of the benchmark (e.g., "Massive Multitask Language Understanding").
    /// Required, max 255 characters.
    /// </summary>
    public required string FullName { get; init; }

    /// <summary>
    /// Detailed description of what the benchmark measures and methodology.
    /// Optional, max 1000 characters.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Category classification for benchmark grouping and QAPS weighting.
    /// Required, must be one of: "Reasoning", "Code", "Math", "Language", "Multimodal".
    /// Determines default weight in QAPS calculation.
    /// </summary>
    public required string Category { get; init; }

    /// <summary>
    /// Interpretation rule for score comparison.
    /// Required, must be one of: "HigherBetter", "LowerBetter".
    /// Used for normalization in QAPS algorithm. Default: "HigherBetter".
    /// </summary>
    public required string Interpretation { get; init; }

    /// <summary>
    /// Minimum value in typical score range (e.g., 0 for percentages).
    /// Required for QAPS normalization. Must be less than TypicalRangeMax.
    /// </summary>
    public required decimal TypicalRangeMin { get; init; }

    /// <summary>
    /// Maximum value in typical score range (e.g., 100 for percentages).
    /// Required for QAPS normalization. Must be greater than TypicalRangeMin.
    /// </summary>
    public required decimal TypicalRangeMax { get; init; }

    /// <summary>
    /// Weight assigned to this benchmark in QAPS calculation (0.00 to 1.00).
    /// Required, range 0-1, max 2 decimal places. Default: 0.00.
    /// Determines contribution to composite quality score.
    /// </summary>
    /// <remarks>
    /// Typical weights by category:
    /// - Reasoning: 0.30, Code: 0.25, Math: 0.20, Language: 0.15, Multimodal: 0.10
    /// Admin can customize per benchmark.
    /// </remarks>
    public required decimal WeightInQaps { get; init; }
}
