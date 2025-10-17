namespace LlmTokenPrice.Application.DTOs;

/// <summary>
/// Data Transfer Object representing model capabilities for API responses.
/// Contains technical specifications and feature flags without internal entity details.
/// </summary>
/// <remarks>
/// Nested within ModelDto to provide capability information.
/// Mapped from Capability entity in Application layer services.
/// </remarks>
public record CapabilityDto
{
    /// <summary>
    /// Maximum context window size in tokens (e.g., 4096, 8192, 128000).
    /// </summary>
    public required int ContextWindow { get; init; }

    /// <summary>
    /// Maximum number of tokens the model can generate in a single response.
    /// Null if provider doesn't specify separate output limits.
    /// </summary>
    public int? MaxOutputTokens { get; init; }

    /// <summary>
    /// Indicates whether model supports OpenAI-style function calling / tool use.
    /// </summary>
    public required bool SupportsFunctionCalling { get; init; }

    /// <summary>
    /// Indicates whether model supports vision/image inputs (multimodal capability).
    /// </summary>
    public required bool SupportsVision { get; init; }

    /// <summary>
    /// Indicates whether model supports audio input processing.
    /// </summary>
    public required bool SupportsAudioInput { get; init; }

    /// <summary>
    /// Indicates whether model supports audio output generation.
    /// </summary>
    public required bool SupportsAudioOutput { get; init; }

    /// <summary>
    /// Indicates whether model supports streaming responses (token-by-token delivery).
    /// </summary>
    public required bool SupportsStreaming { get; init; }

    /// <summary>
    /// Indicates whether model supports JSON mode for structured output.
    /// </summary>
    public required bool SupportsJsonMode { get; init; }
}
