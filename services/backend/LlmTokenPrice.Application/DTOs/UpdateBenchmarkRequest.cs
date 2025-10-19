namespace LlmTokenPrice.Application.DTOs;

/// <summary>
/// Request payload for updating an existing benchmark definition.
/// Used by PUT /api/admin/benchmarks/{id} endpoint.
/// Validated using FluentValidation (UpdateBenchmarkValidator).
/// </summary>
/// <remarks>
/// BenchmarkName is immutable (cannot be changed via UPDATE) to preserve referential integrity.
/// All other fields can be updated.
/// Validation rules same as CreateBenchmarkRequest.
/// </remarks>
public record UpdateBenchmarkRequest
{
    /// <summary>
    /// Full descriptive name of the benchmark.
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
    /// </summary>
    public required string Category { get; init; }

    /// <summary>
    /// Interpretation rule for score comparison.
    /// Required, must be one of: "HigherBetter", "LowerBetter".
    /// </summary>
    public required string Interpretation { get; init; }

    /// <summary>
    /// Minimum value in typical score range.
    /// Required, must be less than TypicalRangeMax.
    /// </summary>
    public required decimal TypicalRangeMin { get; init; }

    /// <summary>
    /// Maximum value in typical score range.
    /// Required, must be greater than TypicalRangeMin.
    /// </summary>
    public required decimal TypicalRangeMax { get; init; }

    /// <summary>
    /// Weight assigned to this benchmark in QAPS calculation (0.00 to 1.00).
    /// Required, range 0-1, max 2 decimal places.
    /// </summary>
    /// <remarks>
    /// Changing weight invalidates QAPS cache (cache:qaps:*, cache:bestvalue:*).
    /// </remarks>
    public required decimal WeightInQaps { get; init; }
}
