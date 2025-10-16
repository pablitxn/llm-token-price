namespace LlmTokenPrice.Domain.Entities;

/// <summary>
/// Represents an LLM model with pricing and metadata.
/// This is a pure domain entity with no infrastructure dependencies (EF attributes, data annotations, etc.).
/// </summary>
/// <remarks>
/// Entity configuration via Fluent API in Infrastructure layer (ModelConfiguration.cs).
/// Table name: models (snake_case per PostgreSQL conventions).
/// </remarks>
public class Model
{
    /// <summary>
    /// Unique identifier for the model (UUID/GUID for distributed system compatibility).
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Model name (e.g., "GPT-4", "Claude 3 Opus", "Gemini Pro").
    /// Required field, combined with Provider forms unique constraint.
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Provider/vendor name (e.g., "OpenAI", "Anthropic", "Google").
    /// Required field, indexed for filtering queries.
    /// </summary>
    public string Provider { get; set; } = null!;

    /// <summary>
    /// Model version identifier (e.g., "0613", "20240229"). Optional field.
    /// Allows multiple NULLs in unique constraint (Name, Provider).
    /// </summary>
    public string? Version { get; set; }

    /// <summary>
    /// Model release date. Optional field for historical tracking.
    /// </summary>
    public DateTime? ReleaseDate { get; set; }

    /// <summary>
    /// Model status (e.g., "active", "deprecated", "preview").
    /// Defaults to "active". Indexed for filtering active models.
    /// </summary>
    public string Status { get; set; } = "active";

    /// <summary>
    /// Input token pricing per 1 million tokens.
    /// Stored as decimal(10,6) for sub-cent accuracy.
    /// </summary>
    public decimal InputPricePer1M { get; set; }

    /// <summary>
    /// Output token pricing per 1 million tokens.
    /// Stored as decimal(10,6) for sub-cent accuracy.
    /// </summary>
    public decimal OutputPricePer1M { get; set; }

    /// <summary>
    /// Currency code for pricing (e.g., "USD", "EUR").
    /// Defaults to "USD". Supports future multi-currency pricing.
    /// </summary>
    public string Currency { get; set; } = "USD";

    /// <summary>
    /// Start date for pricing validity period. Optional for time-bounded pricing.
    /// </summary>
    public DateTime? PricingValidFrom { get; set; }

    /// <summary>
    /// End date for pricing validity period. Optional for promotional pricing.
    /// </summary>
    public DateTime? PricingValidTo { get; set; }

    /// <summary>
    /// Timestamp of last automated price scraping. Optional field for Phase 2 scraping.
    /// </summary>
    public DateTime? LastScrapedAt { get; set; }

    /// <summary>
    /// Soft delete flag. False indicates model is deleted but preserved for audit trail.
    /// Defaults to true.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Timestamp when model record was created. Required audit field.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Timestamp when model record was last updated. Required audit field.
    /// Indexed for freshness queries (e.g., "models updated in last 7 days").
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    // Navigation Properties (one-way relationships from Model)

    /// <summary>
    /// One-to-one relationship to model capabilities (context window, feature flags).
    /// Nullable because capability data may not be available for all models.
    /// Cascade delete: deleting Model deletes associated Capability.
    /// </summary>
    public Capability? Capability { get; set; }

    /// <summary>
    /// One-to-many relationship to benchmark scores.
    /// Collection of performance scores across different benchmarks (MMLU, HumanEval, etc.).
    /// Cascade delete: deleting Model deletes all associated BenchmarkScores.
    /// </summary>
    public ICollection<BenchmarkScore> BenchmarkScores { get; set; } = new List<BenchmarkScore>();
}
