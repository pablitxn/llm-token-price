using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LlmTokenPrice.Application.DTOs;
using LlmTokenPrice.Application.Interfaces;

namespace LlmTokenPrice.API.Controllers.Admin;

/// <summary>
/// API controller for audit log operations.
/// Provides endpoints for viewing and exporting audit trail records of admin CRUD operations.
/// </summary>
/// <remarks>
/// All endpoints require JWT authentication via [Authorize] attribute.
/// Audit logs are immutable (append-only) and provide complete traceability for compliance.
/// Story 2.13 Task 14: Implement comprehensive audit logging.
/// </remarks>
[ApiController]
[Route("api/admin/audit-log")]
[Authorize] // All endpoints require JWT authentication
[Produces("application/json")]
public class AuditLogController : ControllerBase
{
    private readonly IAuditLogService _auditLogService;
    private readonly ILogger<AuditLogController> _logger;

    /// <summary>
    /// Initializes a new instance of the AuditLogController.
    /// </summary>
    /// <param name="auditLogService">The audit log service for data operations.</param>
    /// <param name="logger">Logger for request tracking and diagnostics.</param>
    public AuditLogController(
        IAuditLogService auditLogService,
        ILogger<AuditLogController> logger)
    {
        _auditLogService = auditLogService ?? throw new ArgumentNullException(nameof(auditLogService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Retrieves paginated audit logs with optional filtering.
    /// Supports filtering by user, entity type, action, and date range.
    /// </summary>
    /// <param name="page">Page number (1-indexed, default: 1).</param>
    /// <param name="pageSize">Page size (default: 20, max: 100).</param>
    /// <param name="userId">Optional filter by user identifier (email or username).</param>
    /// <param name="entityType">Optional filter by entity type (Model, Benchmark, BenchmarkScore).</param>
    /// <param name="action">Optional filter by action (Create, Update, Delete, Import).</param>
    /// <param name="startDate">Optional filter for audit logs after this date (inclusive, ISO 8601 format).</param>
    /// <param name="endDate">Optional filter for audit logs before this date (inclusive, ISO 8601 format).</param>
    /// <param name="cancellationToken">Cancellation token for async operation.</param>
    /// <returns>Paginated list of audit logs ordered by timestamp descending (most recent first).</returns>
    /// <response code="200">Returns the paginated audit logs.</response>
    /// <response code="400">If pagination parameters are invalid.</response>
    /// <response code="401">If JWT token is missing, invalid, or expired.</response>
    /// <example>
    /// GET /api/admin/audit-log?page=1&amp;pageSize=20
    /// GET /api/admin/audit-log?userId=admin@example.com
    /// GET /api/admin/audit-log?entityType=Model&amp;action=Update
    /// GET /api/admin/audit-log?startDate=2024-01-01&amp;endDate=2024-01-31
    ///
    /// Response:
    /// {
    ///   "data": {
    ///     "items": [
    ///       {
    ///         "id": "...",
    ///         "timestamp": "2024-01-15T10:30:00Z",
    ///         "userId": "admin@example.com",
    ///         "action": "Update",
    ///         "entityType": "Model",
    ///         "entityId": "...",
    ///         "oldValues": "{...}",
    ///         "newValues": "{...}"
    ///       }
    ///     ],
    ///     "pagination": {
    ///       "currentPage": 1,
    ///       "pageSize": 20,
    ///       "totalItems": 150,
    ///       "totalPages": 8,
    ///       "hasNextPage": true,
    ///       "hasPreviousPage": false
    ///     }
    ///   },
    ///   "meta": {
    ///     "timestamp": "2024-01-15T10:30:00Z"
    ///   }
    /// }
    /// </example>
    [HttpGet]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAuditLogs(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? userId = null,
        [FromQuery] string? entityType = null,
        [FromQuery] string? action = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Admin: Fetching audit logs (page: {Page}, pageSize: {PageSize}, userId: {UserId}, entityType: {EntityType}, action: {Action})",
            page,
            pageSize,
            userId ?? "all",
            entityType ?? "all",
            action ?? "all");

        try
        {
            // Create pagination parameters
            var pagination = new PaginationParams
            {
                Page = page,
                PageSize = pageSize
            };

            // Validate pagination parameters
            if (!pagination.IsValid())
            {
                return BadRequest(new
                {
                    error = "Invalid pagination parameters",
                    details = "Page must be >= 1 and pageSize must be between 1 and 100"
                });
            }

            // Fetch paginated audit logs
            var result = await _auditLogService.GetAuditLogsAsync(
                pagination,
                userId,
                entityType,
                action,
                startDate,
                endDate,
                cancellationToken);

            return Ok(new
            {
                data = result,
                meta = new
                {
                    timestamp = DateTime.UtcNow
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Admin: Error fetching audit logs");
            return StatusCode(500, new
            {
                error = "An error occurred while fetching audit logs",
                message = ex.Message
            });
        }
    }

    /// <summary>
    /// Exports audit logs to CSV format with optional filtering.
    /// Returns a downloadable CSV file containing all audit logs matching the filters.
    /// </summary>
    /// <param name="userId">Optional filter by user identifier (email or username).</param>
    /// <param name="entityType">Optional filter by entity type (Model, Benchmark, BenchmarkScore).</param>
    /// <param name="action">Optional filter by action (Create, Update, Delete, Import).</param>
    /// <param name="startDate">Optional filter for audit logs after this date (inclusive, ISO 8601 format).</param>
    /// <param name="endDate">Optional filter for audit logs before this date (inclusive, ISO 8601 format).</param>
    /// <param name="cancellationToken">Cancellation token for async operation.</param>
    /// <returns>CSV file with audit log data.</returns>
    /// <response code="200">Returns the CSV file (Content-Type: text/csv).</response>
    /// <response code="401">If JWT token is missing, invalid, or expired.</response>
    /// <example>
    /// GET /api/admin/audit-log/export
    /// GET /api/admin/audit-log/export?userId=admin@example.com
    /// GET /api/admin/audit-log/export?entityType=Model&amp;action=Delete
    /// GET /api/admin/audit-log/export?startDate=2024-01-01&amp;endDate=2024-01-31
    ///
    /// Response: CSV file download
    /// Filename: audit-log-2024-01-15.csv
    /// </example>
    [HttpGet("export")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ExportAuditLogs(
        [FromQuery] string? userId = null,
        [FromQuery] string? entityType = null,
        [FromQuery] string? action = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Admin: Exporting audit logs to CSV (userId: {UserId}, entityType: {EntityType}, action: {Action})",
            userId ?? "all",
            entityType ?? "all",
            action ?? "all");

        try
        {
            // Generate CSV export
            var csvBytes = await _auditLogService.ExportAuditLogsToCsvAsync(
                userId,
                entityType,
                action,
                startDate,
                endDate,
                cancellationToken);

            // Generate filename with current date
            var filename = $"audit-log-{DateTime.UtcNow:yyyy-MM-dd}.csv";

            _logger.LogInformation(
                "Admin: CSV export generated - {ByteCount} bytes, filename: {Filename}",
                csvBytes.Length,
                filename);

            // Return CSV file
            return File(csvBytes, "text/csv", filename);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Admin: Error exporting audit logs to CSV");
            return StatusCode(500, new
            {
                error = "An error occurred while exporting audit logs",
                message = ex.Message
            });
        }
    }
}
