using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using LlmTokenPrice.Application.DTOs;
using LlmTokenPrice.Application.Interfaces;
using LlmTokenPrice.Domain.Entities;
using LlmTokenPrice.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace LlmTokenPrice.Application.Services;

/// <summary>
/// Service implementation for audit logging operations.
/// Orchestrates audit log creation and retrieval with JSON serialization of entity values.
/// </summary>
/// <remarks>
/// Application layer service that implements audit logging use cases by:
/// 1. Serializing entity values to JSON (old/new states)
/// 2. Creating immutable audit log records via repository
/// 3. Providing paginated queries with filtering
/// 4. Exporting audit logs to CSV format
/// Follows append-only pattern: audit logs are never updated or deleted.
/// </remarks>
public class AuditLogService : IAuditLogService
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ILogger<AuditLogService> _logger;

    // JSON serializer options for consistent formatting
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = false, // Compact JSON for database storage
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        ReferenceHandler = ReferenceHandler.IgnoreCycles // Prevent circular reference issues
    };

    /// <summary>
    /// Initializes a new instance of the AuditLogService.
    /// </summary>
    /// <param name="auditLogRepository">The audit log repository for data access.</param>
    /// <param name="logger">The logger for tracking audit operations.</param>
    public AuditLogService(
        IAuditLogRepository auditLogRepository,
        ILogger<AuditLogService> logger)
    {
        _auditLogRepository = auditLogRepository ?? throw new ArgumentNullException(nameof(auditLogRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task LogCreateAsync(
        string userId,
        string entityType,
        Guid entityId,
        object newValues,
        CancellationToken cancellationToken = default)
    {
        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid(),
            Timestamp = DateTime.UtcNow,
            UserId = userId,
            Action = "Create",
            EntityType = entityType,
            EntityId = entityId,
            OldValues = null, // No previous state for Create
            NewValues = SerializeToJson(newValues)
        };

        await _auditLogRepository.CreateAsync(auditLog, cancellationToken);

        _logger.LogInformation(
            "Audit log created: User {UserId} created {EntityType} {EntityId}",
            userId,
            entityType,
            entityId);
    }

    /// <inheritdoc />
    public async Task LogUpdateAsync(
        string userId,
        string entityType,
        Guid entityId,
        object oldValues,
        object newValues,
        CancellationToken cancellationToken = default)
    {
        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid(),
            Timestamp = DateTime.UtcNow,
            UserId = userId,
            Action = "Update",
            EntityType = entityType,
            EntityId = entityId,
            OldValues = SerializeToJson(oldValues),
            NewValues = SerializeToJson(newValues)
        };

        await _auditLogRepository.CreateAsync(auditLog, cancellationToken);

        _logger.LogInformation(
            "Audit log created: User {UserId} updated {EntityType} {EntityId}",
            userId,
            entityType,
            entityId);
    }

    /// <inheritdoc />
    public async Task LogDeleteAsync(
        string userId,
        string entityType,
        Guid entityId,
        object oldValues,
        CancellationToken cancellationToken = default)
    {
        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid(),
            Timestamp = DateTime.UtcNow,
            UserId = userId,
            Action = "Delete",
            EntityType = entityType,
            EntityId = entityId,
            OldValues = SerializeToJson(oldValues),
            NewValues = null // No new state after Delete
        };

        await _auditLogRepository.CreateAsync(auditLog, cancellationToken);

        _logger.LogInformation(
            "Audit log created: User {UserId} deleted {EntityType} {EntityId}",
            userId,
            entityType,
            entityId);
    }

    /// <inheritdoc />
    public async Task LogImportAsync(
        string userId,
        string entityType,
        Guid entityId,
        object newValues,
        CancellationToken cancellationToken = default)
    {
        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid(),
            Timestamp = DateTime.UtcNow,
            UserId = userId,
            Action = "Import",
            EntityType = entityType,
            EntityId = entityId,
            OldValues = null, // No previous state for Import
            NewValues = SerializeToJson(newValues)
        };

        await _auditLogRepository.CreateAsync(auditLog, cancellationToken);

        _logger.LogInformation(
            "Audit log created: User {UserId} imported {EntityType} (summary: {EntityId})",
            userId,
            entityType,
            entityId);
    }

    /// <inheritdoc />
    public async Task<PagedResult<AuditLogDto>> GetAuditLogsAsync(
        PaginationParams pagination,
        string? userId = null,
        string? entityType = null,
        string? action = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        // Validate pagination parameters
        if (!pagination.IsValid())
        {
            throw new ArgumentException("Invalid pagination parameters", nameof(pagination));
        }

        // Fetch paginated audit logs from repository
        var (auditLogs, totalCount) = await _auditLogRepository.GetPagedAsync(
            pagination.Page,
            pagination.PageSize,
            userId,
            entityType,
            action,
            startDate,
            endDate,
            cancellationToken);

        // Map entities to DTOs
        var auditLogDtos = auditLogs.Select(MapToDto).ToList();

        // Return paginated result using factory method
        return PagedResult<AuditLogDto>.Create(
            auditLogDtos,
            pagination.Page,
            pagination.PageSize,
            totalCount);
    }

    /// <inheritdoc />
    public async Task<byte[]> ExportAuditLogsToCsvAsync(
        string? userId = null,
        string? entityType = null,
        string? action = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        // Fetch all audit logs matching filters
        var auditLogs = await _auditLogRepository.GetAllAsync(
            userId,
            entityType,
            action,
            startDate,
            endDate,
            cancellationToken);

        _logger.LogInformation(
            "Exporting {Count} audit logs to CSV (Filters: UserId={UserId}, EntityType={EntityType}, Action={Action})",
            auditLogs.Count,
            userId ?? "all",
            entityType ?? "all",
            action ?? "all");

        // Build CSV content
        var csv = new StringBuilder();

        // CSV header
        csv.AppendLine("Id,Timestamp,UserId,Action,EntityType,EntityId,OldValues,NewValues");

        // CSV rows
        foreach (var auditLog in auditLogs)
        {
            csv.AppendLine(FormatCsvRow(
                auditLog.Id.ToString(),
                auditLog.Timestamp.ToString("O"), // ISO 8601 format
                EscapeCsvField(auditLog.UserId),
                EscapeCsvField(auditLog.Action),
                EscapeCsvField(auditLog.EntityType),
                auditLog.EntityId.ToString(),
                EscapeCsvField(auditLog.OldValues ?? ""),
                EscapeCsvField(auditLog.NewValues ?? "")
            ));
        }

        return Encoding.UTF8.GetBytes(csv.ToString());
    }

    /// <summary>
    /// Serializes an object to compact JSON string.
    /// </summary>
    private static string SerializeToJson(object obj)
    {
        return JsonSerializer.Serialize(obj, JsonOptions);
    }

    /// <summary>
    /// Maps an AuditLog entity to AuditLogDto.
    /// </summary>
    private static AuditLogDto MapToDto(AuditLog auditLog)
    {
        return new AuditLogDto
        {
            Id = auditLog.Id,
            Timestamp = auditLog.Timestamp,
            UserId = auditLog.UserId,
            Action = auditLog.Action,
            EntityType = auditLog.EntityType,
            EntityId = auditLog.EntityId,
            OldValues = auditLog.OldValues,
            NewValues = auditLog.NewValues
        };
    }

    /// <summary>
    /// Escapes a CSV field (handles quotes, commas, newlines).
    /// </summary>
    private static string EscapeCsvField(string field)
    {
        if (string.IsNullOrEmpty(field))
        {
            return "";
        }

        // If field contains comma, quote, or newline, wrap in quotes and escape internal quotes
        if (field.Contains(',') || field.Contains('"') || field.Contains('\n') || field.Contains('\r'))
        {
            return $"\"{field.Replace("\"", "\"\"")}\"";
        }

        return field;
    }

    /// <summary>
    /// Formats a CSV row from fields.
    /// </summary>
    private static string FormatCsvRow(params string[] fields)
    {
        return string.Join(",", fields);
    }
}
