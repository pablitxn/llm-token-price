using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LlmTokenPrice.Application.DTOs;
using LlmTokenPrice.Application.Services;

namespace LlmTokenPrice.API.Controllers.Admin;

/// <summary>
/// API controller for admin model management operations.
/// Provides endpoints for viewing, creating, updating, and deleting models in the admin panel.
/// </summary>
/// <remarks>
/// All endpoints require JWT authentication via [Authorize] attribute.
/// Admin endpoints return ALL models (including inactive) and are NOT cached.
/// </remarks>
[ApiController]
[Route("api/admin/models")]
[Authorize] // All endpoints require JWT authentication
[Produces("application/json")]
public class AdminModelsController : ControllerBase
{
    private readonly IAdminModelService _adminModelService;
    private readonly ILogger<AdminModelsController> _logger;

    /// <summary>
    /// Initializes a new instance of the AdminModelsController.
    /// </summary>
    /// <param name="adminModelService">The admin model service for data operations.</param>
    /// <param name="logger">Logger for request tracking and diagnostics.</param>
    public AdminModelsController(
        IAdminModelService adminModelService,
        ILogger<AdminModelsController> logger)
    {
        _adminModelService = adminModelService ?? throw new ArgumentNullException(nameof(adminModelService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Retrieves all LLM models for admin panel (including inactive).
    /// Supports search by name/provider and filtering by status.
    /// </summary>
    /// <param name="searchTerm">Optional search term to filter by model name or provider (case-insensitive).</param>
    /// <param name="provider">Optional provider filter (exact match, case-insensitive).</param>
    /// <param name="status">Optional status filter (exact match, case-insensitive).</param>
    /// <param name="cancellationToken">Cancellation token for async operation.</param>
    /// <returns>List of all models (including inactive) with full audit data.</returns>
    /// <response code="200">Returns the list of models (including inactive).</response>
    /// <response code="401">If JWT token is missing, invalid, or expired.</response>
    /// <response code="500">If an unexpected error occurs during processing.</response>
    /// <example>
    /// GET /api/admin/models
    /// GET /api/admin/models?searchTerm=gpt
    /// GET /api/admin/models?provider=OpenAI&status=active
    ///
    /// Response:
    /// {
    ///   "data": [
    ///     {
    ///       "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///       "name": "GPT-4",
    ///       "provider": "OpenAI",
    ///       "version": "0613",
    ///       "status": "active",
    ///       "inputPricePer1M": 30.00,
    ///       "outputPricePer1M": 60.00,
    ///       "currency": "USD",
    ///       "isActive": true,
    ///       "createdAt": "2024-01-10T08:00:00Z",
    ///       "updatedAt": "2024-01-15T10:30:00Z",
    ///       "capabilities": { ... },
    ///       "topBenchmarks": [ ... ]
    ///     }
    ///   ],
    ///   "meta": {
    ///     "totalCount": 25,
    ///     "cached": false,
    ///     "timestamp": "2024-01-15T10:30:00Z"
    ///   }
    /// }
    /// </example>
    [HttpGet]
    [ProducesResponseType(typeof(AdminApiResponse<List<AdminModelDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? provider = null,
        [FromQuery] string? status = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation(
                "Admin fetching all models (search: {SearchTerm}, provider: {Provider}, status: {Status})",
                searchTerm ?? "none",
                provider ?? "none",
                status ?? "none");

            var models = await _adminModelService.GetAllModelsAsync(
                searchTerm,
                provider,
                status,
                cancellationToken);

            _logger.LogInformation("Successfully retrieved {Count} models for admin", models.Count);

            var response = new AdminApiResponse<List<AdminModelDto>>
            {
                Data = models,
                Meta = new AdminApiResponseMeta
                {
                    TotalCount = models.Count,
                    Cached = false, // Admin endpoints are NEVER cached
                    Timestamp = DateTime.UtcNow
                }
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching models for admin");
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                error = new
                {
                    code = "INTERNAL_ERROR",
                    message = "An error occurred while retrieving models",
                    details = ex.Message
                }
            });
        }
    }

    /// <summary>
    /// Retrieves a single LLM model by its unique identifier (including inactive).
    /// </summary>
    /// <param name="id">The unique identifier (GUID) of the model.</param>
    /// <param name="cancellationToken">Cancellation token for async operation.</param>
    /// <returns>Model with full audit data and nested capability/benchmark data.</returns>
    /// <response code="200">Returns the requested model.</response>
    /// <response code="404">If the model is not found.</response>
    /// <response code="401">If JWT token is missing, invalid, or expired.</response>
    /// <response code="500">If an unexpected error occurs during processing.</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(AdminApiResponse<AdminModelDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Admin fetching model with ID: {ModelId}", id);

            var model = await _adminModelService.GetModelByIdAsync(id, cancellationToken);

            if (model == null)
            {
                _logger.LogWarning("Model not found for admin: {ModelId}", id);
                return NotFound(new
                {
                    error = new
                    {
                        code = "NOT_FOUND",
                        message = $"Model with ID {id} not found"
                    }
                });
            }

            _logger.LogInformation("Successfully retrieved model for admin: {ModelName}", model.Name);

            var response = new AdminApiResponse<AdminModelDto>
            {
                Data = model,
                Meta = new AdminApiResponseMeta
                {
                    Cached = false, // Admin endpoints are NEVER cached
                    Timestamp = DateTime.UtcNow
                }
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching model {ModelId} for admin", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                error = new
                {
                    code = "INTERNAL_ERROR",
                    message = "An error occurred while retrieving the model",
                    details = ex.Message
                }
            });
        }
    }

    /// <summary>
    /// Creates a new LLM model in the system.
    /// PLACEHOLDER: Full implementation will be completed in Story 2.5.
    /// Returns 501 Not Implemented until backend validation and service layer are ready.
    /// </summary>
    /// <param name="request">The create model request with all required fields.</param>
    /// <param name="cancellationToken">Cancellation token for async operation.</param>
    /// <returns>Created model with ID and audit timestamps.</returns>
    /// <response code="201">Model successfully created.</response>
    /// <response code="400">Validation failed (invalid data).</response>
    /// <response code="401">If JWT token is missing, invalid, or expired.</response>
    /// <response code="501">Not implemented yet (Story 2.5 will implement this).</response>
    [HttpPost]
    [ProducesResponseType(typeof(AdminApiResponse<AdminModelDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status501NotImplemented)]
    public async Task<IActionResult> Create(
        [FromBody] CreateModelRequest request,
        CancellationToken cancellationToken)
    {
        // TODO: Story 2.5 - Implement full model creation with FluentValidation
        // - Validate request with FluentValidation
        // - Call IAdminModelService.CreateModelAsync
        // - Invalidate cache:models:* patterns
        // - Return 201 Created with AdminModelDto response

        _logger.LogWarning("POST /api/admin/models called but not yet implemented (Story 2.5)");

        return StatusCode(StatusCodes.Status501NotImplemented, new
        {
            error = new
            {
                code = "NOT_IMPLEMENTED",
                message = "Model creation endpoint will be implemented in Story 2.5",
                details = "Frontend form is ready, but backend validation and service layer are pending"
            }
        });
    }

    /// <summary>
    /// Deletes (soft delete) a model by setting isActive = false.
    /// Model data is preserved for audit purposes but will no longer appear in public API.
    /// </summary>
    /// <param name="id">The unique identifier (GUID) of the model to delete.</param>
    /// <param name="cancellationToken">Cancellation token for async operation.</param>
    /// <returns>NoContent (204) on successful deletion.</returns>
    /// <response code="204">Model successfully deleted (soft delete).</response>
    /// <response code="404">Model not found.</response>
    /// <response code="401">If JWT token is missing, invalid, or expired.</response>
    /// <response code="500">If an unexpected error occurs during processing.</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Admin deleting model with ID: {ModelId}", id);

            var success = await _adminModelService.DeleteModelAsync(id, cancellationToken);

            if (!success)
            {
                _logger.LogWarning("Model not found for deletion: {ModelId}", id);
                return NotFound(new
                {
                    error = new
                    {
                        code = "NOT_FOUND",
                        message = $"Model with ID {id} not found"
                    }
                });
            }

            _logger.LogInformation("Successfully deleted model: {ModelId}", id);

            return NoContent(); // 204 No Content
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting model {ModelId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                error = new
                {
                    code = "INTERNAL_ERROR",
                    message = "An error occurred while deleting the model",
                    details = ex.Message
                }
            });
        }
    }
}

/// <summary>
/// Standard API response wrapper for admin operations.
/// </summary>
/// <typeparam name="T">The type of data being returned.</typeparam>
public class AdminApiResponse<T>
{
    /// <summary>
    /// The actual data payload (model, list of models, etc.).
    /// </summary>
    public required T Data { get; init; }

    /// <summary>
    /// Metadata about the response (count, cache status, timestamp).
    /// </summary>
    public required AdminApiResponseMeta Meta { get; init; }
}

/// <summary>
/// Metadata included in all admin API responses.
/// </summary>
public class AdminApiResponseMeta
{
    /// <summary>
    /// Total number of items in the response (for collection responses).
    /// Null for single-item responses.
    /// </summary>
    public int? TotalCount { get; init; }

    /// <summary>
    /// Indicates whether the response was served from cache.
    /// Always false for admin endpoints (no caching for real-time data).
    /// </summary>
    public required bool Cached { get; init; }

    /// <summary>
    /// UTC timestamp when the response was generated.
    /// </summary>
    public required DateTime Timestamp { get; init; }
}
