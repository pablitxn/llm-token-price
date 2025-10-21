using LlmTokenPrice.Domain.Entities;
using LlmTokenPrice.Domain.Repositories;
using LlmTokenPrice.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LlmTokenPrice.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of IAuditLogRepository.
/// Provides data access for audit log operations using PostgreSQL.
/// </summary>
/// <remarks>
/// Infrastructure adapter that implements the Domain port (IAuditLogRepository).
/// Follows append-only pattern: audit logs are immutable and never updated or deleted.
/// All queries are ordered by Timestamp DESC (most recent first).
/// </remarks>
public class AuditLogRepository : IAuditLogRepository
{
    private readonly AppDbContext _context;
    private readonly ILogger<AuditLogRepository> _logger;

    /// <summary>
    /// Initializes a new instance of the AuditLogRepository.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="logger">The logger for tracking operations.</param>
    public AuditLogRepository(
        AppDbContext context,
        ILogger<AuditLogRepository> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<AuditLog> CreateAsync(AuditLog auditLog, CancellationToken cancellationToken = default)
    {
        _context.AuditLogs.Add(auditLog);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogDebug(
            "Audit log created: {AuditLogId} - Action: {Action}, EntityType: {EntityType}, EntityId: {EntityId}, UserId: {UserId}",
            auditLog.Id,
            auditLog.Action,
            auditLog.EntityType,
            auditLog.EntityId,
            auditLog.UserId);

        return auditLog;
    }

    /// <inheritdoc />
    public async Task<(List<AuditLog> AuditLogs, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        string? userId = null,
        string? entityType = null,
        string? action = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        // Build query with filters
        var query = BuildQuery(userId, entityType, action, startDate, endDate);

        // Get total count (before pagination)
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination and execute query
        var auditLogs = await query
            .OrderByDescending(a => a.Timestamp) // Most recent first
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        _logger.LogDebug(
            "Retrieved {Count} audit logs (page {Page}, pageSize {PageSize}) from total {TotalCount}",
            auditLogs.Count,
            page,
            pageSize,
            totalCount);

        return (auditLogs, totalCount);
    }

    /// <inheritdoc />
    public async Task<List<AuditLog>> GetAllAsync(
        string? userId = null,
        string? entityType = null,
        string? action = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        // Build query with filters
        var query = BuildQuery(userId, entityType, action, startDate, endDate);

        // Execute query with ordering (most recent first)
        var auditLogs = await query
            .OrderByDescending(a => a.Timestamp)
            .ToListAsync(cancellationToken);

        _logger.LogDebug(
            "Retrieved {Count} audit logs for export (Filters: UserId={UserId}, EntityType={EntityType}, Action={Action})",
            auditLogs.Count,
            userId ?? "all",
            entityType ?? "all",
            action ?? "all");

        return auditLogs;
    }

    /// <inheritdoc />
    public async Task<List<AuditLog>> GetByEntityAsync(
        string entityType,
        Guid entityId,
        CancellationToken cancellationToken = default)
    {
        var auditLogs = await _context.AuditLogs
            .Where(a => a.EntityType == entityType && a.EntityId == entityId)
            .OrderByDescending(a => a.Timestamp)
            .ToListAsync(cancellationToken);

        _logger.LogDebug(
            "Retrieved {Count} audit logs for entity: {EntityType}/{EntityId}",
            auditLogs.Count,
            entityType,
            entityId);

        return auditLogs;
    }

    /// <summary>
    /// Builds a filtered query based on optional parameters.
    /// </summary>
    private IQueryable<AuditLog> BuildQuery(
        string? userId,
        string? entityType,
        string? action,
        DateTime? startDate,
        DateTime? endDate)
    {
        var query = _context.AuditLogs.AsQueryable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(userId))
        {
            query = query.Where(a => a.UserId == userId);
        }

        if (!string.IsNullOrWhiteSpace(entityType))
        {
            query = query.Where(a => a.EntityType == entityType);
        }

        if (!string.IsNullOrWhiteSpace(action))
        {
            query = query.Where(a => a.Action == action);
        }

        if (startDate.HasValue)
        {
            query = query.Where(a => a.Timestamp >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            // Include the entire end date (23:59:59.999)
            var endOfDay = endDate.Value.Date.AddDays(1).AddTicks(-1);
            query = query.Where(a => a.Timestamp <= endOfDay);
        }

        return query;
    }
}
