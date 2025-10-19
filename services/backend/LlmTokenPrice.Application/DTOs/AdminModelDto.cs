namespace LlmTokenPrice.Application.DTOs;

/// <summary>
/// Data Transfer Object representing an LLM model for admin API responses.
/// Contains all model data including inactive models and full audit timestamps.
/// </summary>
/// <remarks>
/// Admin version extends public ModelDto with additional admin-specific fields:
/// - Includes inactive/deprecated/beta models (unlike public API which filters to active only)
/// - Includes CreatedAt timestamp for full audit trail
/// - Used by admin controllers to return model data to admin panel.
/// JSON property names are camelCase (configured in Program.cs).
/// </remarks>
public record AdminModelDto
{
    /// <summary>
    /// Unique identifier for the model (UUID/GUID).
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Model name (e.g., "GPT-4", "Claude 3 Opus", "Gemini Pro").
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Provider/vendor name (e.g., "OpenAI", "Anthropic", "Google").
    /// </summary>
    public required string Provider { get; init; }

    /// <summary>
    /// Model version identifier (e.g., "0613", "20240229").
    /// Null if version information is not available.
    /// </summary>
    public string? Version { get; init; }

    /// <summary>
    /// Model status (e.g., "active", "deprecated", "beta").
    /// Admin API returns ALL statuses (unlike public API which filters to active only).
    /// </summary>
    public required string Status { get; init; }

    /// <summary>
    /// Input token pricing per 1 million tokens.
    /// </summary>
    public required decimal InputPricePer1M { get; init; }

    /// <summary>
    /// Output token pricing per 1 million tokens.
    /// </summary>
    public required decimal OutputPricePer1M { get; init; }

    /// <summary>
    /// Currency code for pricing (e.g., "USD", "EUR").
    /// </summary>
    public required string Currency { get; init; }

    /// <summary>
    /// Soft delete flag. False indicates model is deleted but preserved for audit trail.
    /// Admin API returns all models including inactive (IsActive = false).
    /// </summary>
    public required bool IsActive { get; init; }

    /// <summary>
    /// Timestamp when model record was created. Required for admin audit trail.
    /// </summary>
    public required DateTime CreatedAt { get; init; }

    /// <summary>
    /// Timestamp when model record was last updated. Required audit field.
    /// Used for displaying data freshness and sorting in admin panel (most recent first).
    /// </summary>
    public required DateTime UpdatedAt { get; init; }

    /// <summary>
    /// Model capabilities (context window, feature flags).
    /// Null if capability data is not available for this model.
    /// </summary>
    public CapabilityDto? Capabilities { get; init; }

    /// <summary>
    /// Top benchmark scores for the model (typically top 3, ordered by score DESC).
    /// Empty list if no benchmark data is available.
    /// </summary>
    public required List<BenchmarkScoreDto> TopBenchmarks { get; init; }
}
