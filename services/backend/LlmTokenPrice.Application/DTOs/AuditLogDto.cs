namespace LlmTokenPrice.Application.DTOs;

/// <summary>
/// Data Transfer Object for audit log entries.
/// Represents a single audit trail record for admin CRUD operations.
/// </summary>
public record AuditLogDto
{
    /// <summary>
    /// Unique identifier for the audit log entry.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Timestamp when the action occurred (UTC).
    /// </summary>
    public required DateTime Timestamp { get; init; }

    /// <summary>
    /// User identifier who performed the action (email or username).
    /// </summary>
    public required string UserId { get; init; }

    /// <summary>
    /// Action performed: "Create", "Update", "Delete", "Import".
    /// </summary>
    public required string Action { get; init; }

    /// <summary>
    /// Entity type affected: "Model", "Benchmark", "BenchmarkScore".
    /// </summary>
    public required string EntityType { get; init; }

    /// <summary>
    /// Unique identifier of the affected entity.
    /// </summary>
    public required Guid EntityId { get; init; }

    /// <summary>
    /// JSON string representing the entity BEFORE the operation.
    /// NULL for Create actions (no previous state).
    /// </summary>
    public string? OldValues { get; init; }

    /// <summary>
    /// JSON string representing the entity AFTER the operation.
    /// NULL for Delete actions (entity no longer exists).
    /// </summary>
    public string? NewValues { get; init; }
}
