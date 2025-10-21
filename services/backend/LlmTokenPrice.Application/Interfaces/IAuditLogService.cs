using LlmTokenPrice.Application.DTOs;

namespace LlmTokenPrice.Application.Interfaces;

/// <summary>
/// Service interface for audit logging operations.
/// Provides methods to log admin CRUD operations for compliance and traceability.
/// </summary>
/// <remarks>
/// This service follows the append-only pattern: audit logs are immutable and never updated or deleted.
/// All operations are asynchronous to avoid blocking admin operations.
/// </remarks>
public interface IAuditLogService
{
    /// <summary>
    /// Logs a Create operation for an entity.
    /// </summary>
    /// <param name="userId">The user identifier who performed the action (from JWT claims).</param>
    /// <param name="entityType">The entity type (e.g., "Model", "Benchmark", "BenchmarkScore").</param>
    /// <param name="entityId">The unique identifier of the created entity.</param>
    /// <param name="newValues">The entity after creation (serialized as JSON).</param>
    /// <param name="cancellationToken">Cancellation token for async operation.</param>
    /// <returns>Task representing the async operation.</returns>
    Task LogCreateAsync(
        string userId,
        string entityType,
        Guid entityId,
        object newValues,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Logs an Update operation for an entity.
    /// </summary>
    /// <param name="userId">The user identifier who performed the action (from JWT claims).</param>
    /// <param name="entityType">The entity type (e.g., "Model", "Benchmark", "BenchmarkScore").</param>
    /// <param name="entityId">The unique identifier of the updated entity.</param>
    /// <param name="oldValues">The entity before the update (serialized as JSON).</param>
    /// <param name="newValues">The entity after the update (serialized as JSON).</param>
    /// <param name="cancellationToken">Cancellation token for async operation.</param>
    /// <returns>Task representing the async operation.</returns>
    Task LogUpdateAsync(
        string userId,
        string entityType,
        Guid entityId,
        object oldValues,
        object newValues,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Logs a Delete operation for an entity.
    /// </summary>
    /// <param name="userId">The user identifier who performed the action (from JWT claims).</param>
    /// <param name="entityType">The entity type (e.g., "Model", "Benchmark", "BenchmarkScore").</param>
    /// <param name="entityId">The unique identifier of the deleted entity.</param>
    /// <param name="oldValues">The entity before deletion (serialized as JSON).</param>
    /// <param name="cancellationToken">Cancellation token for async operation.</param>
    /// <returns>Task representing the async operation.</returns>
    Task LogDeleteAsync(
        string userId,
        string entityType,
        Guid entityId,
        object oldValues,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Logs a bulk Import operation (e.g., CSV import).
    /// </summary>
    /// <param name="userId">The user identifier who performed the action (from JWT claims).</param>
    /// <param name="entityType">The entity type being imported (e.g., "BenchmarkScore").</param>
    /// <param name="entityId">A representative identifier (e.g., first imported entity ID, or zero GUID for summary).</param>
    /// <param name="newValues">Summary of the import operation (e.g., row counts, file name).</param>
    /// <param name="cancellationToken">Cancellation token for async operation.</param>
    /// <returns>Task representing the async operation.</returns>
    Task LogImportAsync(
        string userId,
        string entityType,
        Guid entityId,
        object newValues,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves paginated audit logs with optional filtering.
    /// </summary>
    /// <param name="pagination">Pagination parameters (page, pageSize).</param>
    /// <param name="userId">Optional filter by user identifier.</param>
    /// <param name="entityType">Optional filter by entity type.</param>
    /// <param name="action">Optional filter by action (Create, Update, Delete, Import).</param>
    /// <param name="startDate">Optional filter for audit logs after this date (inclusive).</param>
    /// <param name="endDate">Optional filter for audit logs before this date (inclusive).</param>
    /// <param name="cancellationToken">Cancellation token for async operation.</param>
    /// <returns>Paginated result containing audit log entries.</returns>
    Task<PagedResult<AuditLogDto>> GetAuditLogsAsync(
        PaginationParams pagination,
        string? userId = null,
        string? entityType = null,
        string? action = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Exports audit logs to CSV format with optional filtering.
    /// </summary>
    /// <param name="userId">Optional filter by user identifier.</param>
    /// <param name="entityType">Optional filter by entity type.</param>
    /// <param name="action">Optional filter by action (Create, Update, Delete, Import).</param>
    /// <param name="startDate">Optional filter for audit logs after this date (inclusive).</param>
    /// <param name="endDate">Optional filter for audit logs before this date (inclusive).</param>
    /// <param name="cancellationToken">Cancellation token for async operation.</param>
    /// <returns>CSV file as byte array.</returns>
    Task<byte[]> ExportAuditLogsToCsvAsync(
        string? userId = null,
        string? entityType = null,
        string? action = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default);
}
