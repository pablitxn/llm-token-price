namespace LlmTokenPrice.Application.DTOs;

/// <summary>
/// Request payload for creating a new LLM model.
/// Used by POST /api/admin/models endpoint.
/// Full validation will be implemented in Story 2.5 using FluentValidation.
/// </summary>
public record CreateModelRequest
{
    /// <summary>
    /// Model name (e.g., "GPT-4 Turbo").
    /// Required, max 255 characters.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Model provider (e.g., "OpenAI", "Anthropic").
    /// Required, max 100 characters.
    /// </summary>
    public required string Provider { get; init; }

    /// <summary>
    /// Model version (e.g., "1.0", "0613").
    /// Optional, max 50 characters.
    /// </summary>
    public string? Version { get; init; }

    /// <summary>
    /// Model release date.
    /// Optional, ISO 8601 date string.
    /// </summary>
    public string? ReleaseDate { get; init; }

    /// <summary>
    /// Model status.
    /// Required, must be one of: "active", "deprecated", "beta".
    /// Default: "active".
    /// </summary>
    public required string Status { get; init; }

    /// <summary>
    /// Input token price per 1 million tokens.
    /// Required, must be positive, max 6 decimal places.
    /// </summary>
    public required decimal InputPricePer1M { get; init; }

    /// <summary>
    /// Output token price per 1 million tokens.
    /// Required, must be positive, max 6 decimal places.
    /// </summary>
    public required decimal OutputPricePer1M { get; init; }

    /// <summary>
    /// Currency code.
    /// Required, must be one of: "USD", "EUR", "GBP".
    /// Default: "USD".
    /// </summary>
    public required string Currency { get; init; }

    /// <summary>
    /// Pricing validity start date.
    /// Optional, ISO 8601 date string.
    /// Must be before PricingValidTo if both are provided.
    /// </summary>
    public string? PricingValidFrom { get; init; }

    /// <summary>
    /// Pricing validity end date.
    /// Optional, ISO 8601 date string.
    /// Must be after PricingValidFrom if both are provided.
    /// </summary>
    public string? PricingValidTo { get; init; }

    /// <summary>
    /// Model capabilities specification.
    /// Required, includes context window, max output tokens, and feature flags.
    /// Story 2.6: Capabilities captured during creation instead of using defaults.
    /// </summary>
    public required CreateCapabilityRequest Capabilities { get; init; }
}
