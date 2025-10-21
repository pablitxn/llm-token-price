using LlmTokenPrice.Domain.Entities;

namespace LlmTokenPrice.Domain.Repositories;

/// <summary>
/// Repository interface (port) for AuditLog entity data access.
/// Defines contract for audit log operations without infrastructure dependencies.
/// </summary>
/// <remarks>
/// This is a "port" in Hexagonal Architecture - the Domain layer defines what it needs,
/// and the Infrastructure layer provides concrete implementations ("adapters").
/// Audit logs follow an append-only pattern: records are never updated or deleted.
/// </remarks>
public interface IAuditLogRepository
{
    /// <summary>
    /// Creates a new audit log entry in the database.
    /// </summary>
    /// <param name="auditLog">The audit log entity to persist.</param>
    /// <param name="cancellationToken">Cancellation token for async operation.</param>
    /// <returns>The created audit log entity with generated Id.</returns>
    Task<AuditLog> CreateAsync(AuditLog auditLog, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a paginated list of audit logs with optional filtering.
    /// Results are ordered by Timestamp descending (most recent first).
    /// </summary>
    /// <param name="page">Page number (1-indexed).</param>
    /// <param name="pageSize">Number of items per page.</param>
    /// <param name="userId">Optional filter by user identifier.</param>
    /// <param name="entityType">Optional filter by entity type.</param>
    /// <param name="action">Optional filter by action.</param>
    /// <param name="startDate">Optional filter for audit logs after this date (inclusive).</param>
    /// <param name="endDate">Optional filter for audit logs before this date (inclusive).</param>
    /// <param name="cancellationToken">Cancellation token for async operation.</param>
    /// <returns>Tuple containing the list of audit logs for the requested page and the total count matching the filters.</returns>
    Task<(List<AuditLog> AuditLogs, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        string? userId = null,
        string? entityType = null,
        string? action = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all audit logs matching the specified filters.
    /// Used for CSV export operations.
    /// Results are ordered by Timestamp descending (most recent first).
    /// </summary>
    /// <param name="userId">Optional filter by user identifier.</param>
    /// <param name="entityType">Optional filter by entity type.</param>
    /// <param name="action">Optional filter by action.</param>
    /// <param name="startDate">Optional filter for audit logs after this date (inclusive).</param>
    /// <param name="endDate">Optional filter for audit logs before this date (inclusive).</param>
    /// <param name="cancellationToken">Cancellation token for async operation.</param>
    /// <returns>List of audit logs matching the filters.</returns>
    Task<List<AuditLog>> GetAllAsync(
        string? userId = null,
        string? entityType = null,
        string? action = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all audit log entries for a specific entity.
    /// Used for entity-level audit trail queries.
    /// Results are ordered by Timestamp descending (most recent first).
    /// </summary>
    /// <param name="entityType">The entity type (e.g., "Model", "Benchmark").</param>
    /// <param name="entityId">The unique identifier of the entity.</param>
    /// <param name="cancellationToken">Cancellation token for async operation.</param>
    /// <returns>List of audit logs for the specified entity.</returns>
    Task<List<AuditLog>> GetByEntityAsync(
        string entityType,
        Guid entityId,
        CancellationToken cancellationToken = default);
}
