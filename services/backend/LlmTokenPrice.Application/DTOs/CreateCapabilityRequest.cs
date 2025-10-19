namespace LlmTokenPrice.Application.DTOs;

/// <summary>
/// Request payload for model capabilities specification.
/// Used as nested object in CreateModelRequest.
/// Story 2.6: Capabilities are now captured during model creation instead of using defaults.
/// </summary>
public record CreateCapabilityRequest
{
    /// <summary>
    /// Maximum number of tokens the model can process in a single request.
    /// Required, must be between 1,000 and 2,000,000 tokens.
    /// </summary>
    public required int ContextWindow { get; init; }

    /// <summary>
    /// Maximum tokens in model's response.
    /// Optional, must be positive and <= ContextWindow if specified.
    /// </summary>
    public int? MaxOutputTokens { get; init; }

    /// <summary>
    /// Whether the model supports function calling (tool use).
    /// Default: false.
    /// </summary>
    public bool SupportsFunctionCalling { get; init; } = false;

    /// <summary>
    /// Whether the model supports vision capabilities (image understanding).
    /// Default: false.
    /// </summary>
    public bool SupportsVision { get; init; } = false;

    /// <summary>
    /// Whether the model supports audio input (speech-to-text).
    /// Default: false.
    /// </summary>
    public bool SupportsAudioInput { get; init; } = false;

    /// <summary>
    /// Whether the model supports audio output (text-to-speech).
    /// Default: false.
    /// </summary>
    public bool SupportsAudioOutput { get; init; } = false;

    /// <summary>
    /// Whether the model supports streaming responses.
    /// Default: true (most modern LLMs support streaming).
    /// </summary>
    public bool SupportsStreaming { get; init; } = true;

    /// <summary>
    /// Whether the model supports JSON mode (structured output).
    /// Default: false.
    /// </summary>
    public bool SupportsJsonMode { get; init; } = false;
}
