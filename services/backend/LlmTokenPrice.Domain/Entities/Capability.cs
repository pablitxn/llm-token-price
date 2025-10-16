namespace LlmTokenPrice.Domain.Entities;

/// <summary>
/// Represents technical capabilities and limits of an LLM model.
/// This is a pure domain entity with no infrastructure dependencies.
/// </summary>
/// <remarks>
/// One-to-one relationship with Model (each model has exactly one Capability record).
/// Entity configuration via Fluent API in Infrastructure layer (CapabilityConfiguration.cs).
/// Table name: model_capabilities (snake_case per PostgreSQL conventions).
/// </remarks>
public class Capability
{
    /// <summary>
    /// Unique identifier for the capability record (UUID/GUID).
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Foreign key to the associated Model.
    /// Unique constraint enforces one-to-one relationship.
    /// </summary>
    public Guid ModelId { get; set; }

    /// <summary>
    /// Maximum context window size in tokens (e.g., 4096, 8192, 128000).
    /// Required field indicating how much text the model can process at once.
    /// </summary>
    public int ContextWindow { get; set; }

    /// <summary>
    /// Maximum number of tokens the model can generate in a single response.
    /// Optional field (some providers don't specify separate output limits).
    /// </summary>
    public int? MaxOutputTokens { get; set; }

    /// <summary>
    /// Indicates whether model supports OpenAI-style function calling / tool use.
    /// Used for filtering models capable of structured output and API integrations.
    /// </summary>
    public bool SupportsFunctionCalling { get; set; }

    /// <summary>
    /// Indicates whether model supports vision/image inputs (multimodal capability).
    /// Used for filtering models that can process images alongside text.
    /// </summary>
    public bool SupportsVision { get; set; }

    /// <summary>
    /// Indicates whether model supports audio input processing.
    /// Used for filtering models with speech-to-text or audio analysis capabilities.
    /// </summary>
    public bool SupportsAudioInput { get; set; }

    /// <summary>
    /// Indicates whether model supports audio output generation.
    /// Used for filtering models with text-to-speech capabilities.
    /// </summary>
    public bool SupportsAudioOutput { get; set; }

    /// <summary>
    /// Indicates whether model supports streaming responses (token-by-token delivery).
    /// Defaults to true as most modern LLMs support streaming.
    /// </summary>
    public bool SupportsStreaming { get; set; } = true;

    /// <summary>
    /// Indicates whether model supports JSON mode for structured output.
    /// Used for filtering models that guarantee valid JSON responses.
    /// </summary>
    public bool SupportsJsonMode { get; set; }

    // Navigation Properties

    /// <summary>
    /// One-to-one navigation property back to the parent Model.
    /// Required navigation (null-forgiving operator) because Capability cannot exist without Model.
    /// </summary>
    public Model Model { get; set; } = null!;
}
