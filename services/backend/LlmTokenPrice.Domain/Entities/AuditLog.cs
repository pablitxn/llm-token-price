namespace LlmTokenPrice.Domain.Entities;

/// <summary>
/// Represents an audit log entry capturing all admin CRUD operations for compliance and traceability.
/// This is a pure domain entity with no infrastructure dependencies (EF attributes, data annotations, etc.).
/// </summary>
/// <remarks>
/// Entity configuration via Fluent API in Infrastructure layer (AuditLogConfiguration.cs).
/// Table name: audit_logs (snake_case per PostgreSQL conventions).
/// Supports immutable append-only logging pattern - records are NEVER updated or deleted.
/// </remarks>
public class AuditLog
{
    /// <summary>
    /// Unique identifier for the audit log entry (UUID/GUID for distributed system compatibility).
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Timestamp when the action occurred (UTC).
    /// Indexed for date range filtering and chronological queries.
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// User identifier who performed the action (email or username from JWT claims).
    /// Indexed for filtering by user.
    /// </summary>
    public string UserId { get; set; } = null!;

    /// <summary>
    /// Action performed: "Create", "Update", "Delete", "Import".
    /// Constrained to enum values in infrastructure layer.
    /// Indexed for action type filtering.
    /// </summary>
    public string Action { get; set; } = null!;

    /// <summary>
    /// Entity type affected: "Model", "Benchmark", "BenchmarkScore".
    /// Indexed for entity type filtering.
    /// </summary>
    public string EntityType { get; set; } = null!;

    /// <summary>
    /// Unique identifier of the affected entity (e.g., Model.Id, Benchmark.Id).
    /// Indexed for entity-level audit trail queries.
    /// </summary>
    public Guid EntityId { get; set; }

    /// <summary>
    /// JSON-serialized representation of the entity BEFORE the operation.
    /// NULL for Create actions (no previous state).
    /// Stored as JSONB in PostgreSQL for queryability and compression.
    /// </summary>
    public string? OldValues { get; set; }

    /// <summary>
    /// JSON-serialized representation of the entity AFTER the operation.
    /// NULL for Delete actions (entity no longer exists).
    /// Stored as JSONB in PostgreSQL for queryability and compression.
    /// </summary>
    public string? NewValues { get; set; }
}
