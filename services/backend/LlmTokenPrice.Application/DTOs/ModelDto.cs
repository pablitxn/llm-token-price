namespace LlmTokenPrice.Application.DTOs;

/// <summary>
/// Data Transfer Object representing an LLM model for API responses.
/// Contains model pricing, metadata, capabilities, and benchmark scores.
/// </summary>
/// <remarks>
/// Used by API controllers to return model data to clients.
/// Mapped from Model entity in Application layer services.
/// JSON property names are camelCase (configured in Program.cs).
/// </remarks>
public record ModelDto
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
    /// Model status (e.g., "active", "deprecated", "preview").
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
    /// Timestamp when model record was last updated.
    /// Used to display data freshness to users.
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
