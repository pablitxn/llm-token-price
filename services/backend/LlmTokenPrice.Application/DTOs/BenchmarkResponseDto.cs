namespace LlmTokenPrice.Application.DTOs;

/// <summary>
/// Response DTO for benchmark definition.
/// Used by GET /api/admin/benchmarks and POST/PUT responses.
/// </summary>
/// <remarks>
/// Includes all benchmark fields plus metadata (Id, CreatedAt, IsActive).
/// Excludes navigation properties (Scores) for performance.
/// Category and Interpretation returned as enum string values.
/// </remarks>
public record BenchmarkResponseDto
{
    /// <summary>
    /// Unique identifier for the benchmark (UUID/GUID).
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Short benchmark identifier (e.g., "MMLU").
    /// </summary>
    public required string BenchmarkName { get; init; }

    /// <summary>
    /// Full descriptive name.
    /// </summary>
    public required string FullName { get; init; }

    /// <summary>
    /// Detailed description of the benchmark.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Category classification ("Reasoning", "Code", "Math", "Language", "Multimodal").
    /// </summary>
    public required string Category { get; init; }

    /// <summary>
    /// Interpretation rule ("HigherBetter" or "LowerBetter").
    /// </summary>
    public required string Interpretation { get; init; }

    /// <summary>
    /// Minimum value in typical score range.
    /// </summary>
    public required decimal TypicalRangeMin { get; init; }

    /// <summary>
    /// Maximum value in typical score range.
    /// </summary>
    public required decimal TypicalRangeMax { get; init; }

    /// <summary>
    /// Weight in QAPS calculation (0.00 to 1.00).
    /// </summary>
    public required decimal WeightInQaps { get; init; }

    /// <summary>
    /// Timestamp when benchmark was created.
    /// </summary>
    public required DateTime CreatedAt { get; init; }

    /// <summary>
    /// Soft-delete flag. False if benchmark is inactive/deleted.
    /// </summary>
    public required bool IsActive { get; init; }
}
