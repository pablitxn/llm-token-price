using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LlmTokenPrice.Application.DTOs;
using LlmTokenPrice.Application.Interfaces;
using LlmTokenPrice.Application.Services;
using LlmTokenPrice.Infrastructure.Security;

namespace LlmTokenPrice.API.Controllers.Admin;

/// <summary>
/// API controller for admin model management operations.
/// Provides endpoints for viewing, creating, updating, and deleting models in the admin panel.
/// </summary>
/// <remarks>
/// All endpoints require JWT authentication via [Authorize] attribute.
/// Admin endpoints return ALL models (including inactive) and are NOT cached.
/// Story 2.13 Task 17: All text inputs are sanitized to prevent XSS attacks.
/// Story 2.13 Task 14: All CRUD operations logged to audit trail for compliance.
/// </remarks>
[ApiController]
[Route("api/admin/models")]
[Authorize] // All endpoints require JWT authentication
[Produces("application/json")]
public class AdminModelsController : ControllerBase
{
    private readonly IAdminModelService _adminModelService;
    private readonly IAdminBenchmarkService _adminBenchmarkService;
    private readonly IAuditLogService _auditLogService;
    private readonly InputSanitizationService _sanitizer;
    private readonly ILogger<AdminModelsController> _logger;

    /// <summary>
    /// Initializes a new instance of the AdminModelsController.
    /// </summary>
    /// <param name="adminModelService">The admin model service for data operations.</param>
    /// <param name="adminBenchmarkService">The admin benchmark service for score operations.</param>
    /// <param name="auditLogService">The audit log service for compliance tracking.</param>
    /// <param name="sanitizer">Input sanitization service for XSS protection.</param>
    /// <param name="logger">Logger for request tracking and diagnostics.</param>
    public AdminModelsController(
        IAdminModelService adminModelService,
        IAdminBenchmarkService adminBenchmarkService,
        IAuditLogService auditLogService,
        InputSanitizationService sanitizer,
        ILogger<AdminModelsController> logger)
    {
        _adminModelService = adminModelService ?? throw new ArgumentNullException(nameof(adminModelService));
        _adminBenchmarkService = adminBenchmarkService ?? throw new ArgumentNullException(nameof(adminBenchmarkService));
        _auditLogService = auditLogService ?? throw new ArgumentNullException(nameof(auditLogService));
        _sanitizer = sanitizer ?? throw new ArgumentNullException(nameof(sanitizer));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Retrieves all LLM models for admin panel (including inactive).
    /// Supports search by name/provider, filtering by status, and optional pagination.
    /// </summary>
    /// <param name="page">Optional page number (1-indexed, default: returns all).</param>
    /// <param name="pageSize">Optional page size (default: 20, max: 100).</param>
    /// <param name="searchTerm">Optional search term to filter by model name or provider (case-insensitive).</param>
    /// <param name="provider">Optional provider filter (exact match, case-insensitive).</param>
    /// <param name="status">Optional status filter (exact match, case-insensitive).</param>
    /// <param name="cancellationToken">Cancellation token for async operation.</param>
    /// <returns>List of all models (including inactive) with full audit data, optionally paginated.</returns>
    /// <response code="200">Returns the list of models (including inactive), paginated or full list.</response>
    /// <response code="400">If pagination parameters are invalid.</response>
    /// <response code="401">If JWT token is missing, invalid, or expired.</response>
    /// <response code="500">If an unexpected error occurs during processing.</response>
    /// <example>
    /// GET /api/admin/models
    /// GET /api/admin/models?searchTerm=gpt
    /// GET /api/admin/models?provider=OpenAI&amp;status=active
    /// GET /api/admin/models?page=1&amp;pageSize=20&amp;searchTerm=gpt
    ///
    /// Non-paginated response:
    /// {
    ///   "data": [ ... ],
    ///   "meta": {
    ///     "totalCount": 25,
    ///     "cached": false,
    ///     "timestamp": "2024-01-15T10:30:00Z"
    ///   }
    /// }
    ///
    /// Paginated response:
    /// {
    ///   "data": {
    ///     "items": [ ... ],
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
    ///     "cached": false,
    ///     "timestamp": "2024-01-15T10:30:00Z"
    ///   }
    /// }
    /// </example>
    [HttpGet]
    [ProducesResponseType(typeof(AdminApiResponse<List<AdminModelDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AdminApiResponse<PagedResult<AdminModelDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? provider = null,
        [FromQuery] string? status = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // If pagination parameters provided, use paginated endpoint
            if (page.HasValue || pageSize.HasValue)
            {
                _logger.LogInformation(
                    "Admin fetching paginated models (page: {Page}, pageSize: {PageSize}, search: {SearchTerm}, provider: {Provider}, status: {Status})",
                    page ?? 1, pageSize ?? 20,
                    searchTerm ?? "none",
                    provider ?? "none",
                    status ?? "none");

                var pagination = new PaginationParams
                {
                    Page = page ?? 1,
                    PageSize = pageSize ?? 20
                };

                if (!pagination.IsValid())
                {
                    _logger.LogWarning("Invalid pagination parameters: page={Page}, pageSize={PageSize}",
                        pagination.Page, pagination.PageSize);

                    return BadRequest(new
                    {
                        error = new
                        {
                            code = "VALIDATION_ERROR",
                            message = "Invalid pagination parameters",
                            details = new
                            {
                                page = pagination.Page,
                                pageSize = pagination.PageSize,
                                maxPageSize = 100,
                                validation = "Page must be >= 1, PageSize must be between 1 and 100"
                            }
                        }
                    });
                }

                var pagedResult = await _adminModelService.GetAllModelsPagedAsync(
                    pagination,
                    searchTerm,
                    provider,
                    status,
                    cancellationToken);

                _logger.LogInformation("Successfully retrieved page {Page} ({Count}/{Total} models) for admin",
                    pagination.Page, pagedResult.Items.Count, pagedResult.Pagination.TotalItems);

                var response = new AdminApiResponse<PagedResult<AdminModelDto>>
                {
                    Data = pagedResult,
                    Meta = new AdminApiResponseMeta
                    {
                        Cached = false, // Admin endpoints are NEVER cached
                        Timestamp = DateTime.UtcNow
                    }
                };

                return Ok(response);
            }
            else
            {
                // Original behavior - return all models
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
        try
        {
            // Story 2.13 Task 17: Sanitize all text inputs to prevent XSS attacks
            // Log security event if malicious content was detected BEFORE sanitization
            if (_sanitizer.ContainsPotentiallyMaliciousContent(request.Name) ||
                _sanitizer.ContainsPotentiallyMaliciousContent(request.Provider))
            {
                _logger.LogWarning(
                    "Potentially malicious content detected and sanitized in model creation request from {User}",
                    User.Identity?.Name ?? "Unknown");
            }

            // Create sanitized request (DTOs are immutable with init-only properties)
            var sanitizedRequest = new CreateModelRequest
            {
                Name = _sanitizer.Sanitize(request.Name) ?? string.Empty,
                Provider = _sanitizer.Sanitize(request.Provider) ?? string.Empty,
                Version = _sanitizer.Sanitize(request.Version) ?? string.Empty,
                Status = _sanitizer.Sanitize(request.Status) ?? string.Empty,
                Currency = _sanitizer.Sanitize(request.Currency) ?? string.Empty,
                InputPricePer1M = request.InputPricePer1M,
                OutputPricePer1M = request.OutputPricePer1M,
                ReleaseDate = request.ReleaseDate,
                Capabilities = request.Capabilities // Capabilities don't need text sanitization (boolean/numeric)
            };

            _logger.LogInformation(
                "Creating new model: {ModelName} by {Provider}",
                sanitizedRequest.Name,
                sanitizedRequest.Provider);

            // Create model (FluentValidation runs automatically via middleware)
            var modelId = await _adminModelService.CreateModelAsync(sanitizedRequest, cancellationToken);

            // Fetch created model for response
            var createdModel = await _adminModelService.GetModelByIdAsync(modelId, cancellationToken);

            if (createdModel == null)
            {
                _logger.LogError("Model was created but could not be retrieved: {ModelId}", modelId);
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    error = new
                    {
                        code = "INTERNAL_ERROR",
                        message = "Model was created but could not be retrieved",
                        details = "Please contact administrator"
                    }
                });
            }

            _logger.LogInformation(
                "Model created successfully: {ModelId} - {ModelName}",
                modelId,
                createdModel.Name);

            // Story 2.13 Task 14: Log CREATE operation to audit trail
            var userId = User.Identity?.Name ?? "Unknown";
            await _auditLogService.LogCreateAsync(
                userId,
                "Model",
                modelId,
                createdModel, // Serialize created model as JSON
                cancellationToken);

            // Return 201 Created with Location header
            return CreatedAtAction(
                nameof(GetById),
                new { id = modelId },
                new AdminApiResponse<AdminModelDto>
                {
                    Data = createdModel,
                    Meta = new AdminApiResponseMeta
                    {
                        TotalCount = null,
                        Cached = false,
                        Timestamp = DateTime.UtcNow
                    }
                });
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already exists"))
        {
            _logger.LogWarning(
                "Duplicate model creation attempted: {ModelName} by {Provider}",
                request.Name,
                request.Provider);

            return BadRequest(new
            {
                error = new
                {
                    code = "DUPLICATE_MODEL",
                    message = ex.Message,
                    details = new[]
                    {
                        new { field = "name", message = "Model with this name and provider already exists" },
                        new { field = "provider", message = "Model with this name and provider already exists" }
                    }
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error creating model: {ModelName} by {Provider}",
                request.Name,
                request.Provider);

            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                error = new
                {
                    code = "INTERNAL_ERROR",
                    message = "An unexpected error occurred while creating the model",
                    details = ex.Message
                }
            });
        }
    }

    /// <summary>
    /// Updates an existing LLM model in the system.
    /// Updates model properties, capabilities, and refreshes UpdatedAt timestamp.
    /// </summary>
    /// <param name="id">The unique identifier (GUID) of the model to update.</param>
    /// <param name="request">The update model request with fields to modify.</param>
    /// <param name="cancellationToken">Cancellation token for async operation.</param>
    /// <returns>Updated model with refreshed audit timestamps.</returns>
    /// <response code="200">Model successfully updated.</response>
    /// <response code="400">Validation failed (invalid data).</response>
    /// <response code="404">Model not found.</response>
    /// <response code="401">If JWT token is missing, invalid, or expired.</response>
    /// <response code="500">If an unexpected error occurs during processing.</response>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(AdminApiResponse<AdminModelDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] CreateModelRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "Updating model: {ModelId} - {ModelName} by {Provider}",
                id,
                request.Name,
                request.Provider);

            // Story 2.13 Task 14: Fetch old values BEFORE update for audit trail
            var oldModel = await _adminModelService.GetModelByIdAsync(id, cancellationToken);

            // Update model (FluentValidation runs automatically via middleware)
            var updatedModel = await _adminModelService.UpdateModelAsync(id, request, cancellationToken);

            if (updatedModel == null)
            {
                _logger.LogWarning("Model not found for update: {ModelId}", id);
                return NotFound(new
                {
                    error = new
                    {
                        code = "NOT_FOUND",
                        message = $"Model with ID {id} not found"
                    }
                });
            }

            _logger.LogInformation(
                "Model updated successfully: {ModelId} - {ModelName}",
                id,
                updatedModel.Name);

            // Story 2.13 Task 14: Log UPDATE operation to audit trail
            var userId = User.Identity?.Name ?? "Unknown";
            await _auditLogService.LogUpdateAsync(
                userId,
                "Model",
                id,
                oldModel!, // Old values captured before update
                updatedModel, // New values after update
                cancellationToken);

            // Return 200 OK with updated model
            return Ok(new AdminApiResponse<AdminModelDto>
            {
                Data = updatedModel,
                Meta = new AdminApiResponseMeta
                {
                    Cached = false,
                    Timestamp = DateTime.UtcNow
                }
            });
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already exists"))
        {
            _logger.LogWarning(
                "Duplicate model update attempted: {ModelId} - {ModelName} by {Provider}",
                id,
                request.Name,
                request.Provider);

            return Conflict(new
            {
                error = new
                {
                    code = "DUPLICATE_MODEL",
                    message = ex.Message,
                    details = new[]
                    {
                        new { field = "name", message = "Model with this name and provider already exists" },
                        new { field = "provider", message = "Model with this name and provider already exists" }
                    }
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error updating model: {ModelId} - {ModelName} by {Provider}",
                id,
                request.Name,
                request.Provider);

            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                error = new
                {
                    code = "INTERNAL_ERROR",
                    message = "An unexpected error occurred while updating the model",
                    details = ex.Message
                }
            });
        }
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

            // Story 2.13 Task 14: Fetch old values BEFORE deletion for audit trail
            var oldModel = await _adminModelService.GetModelByIdAsync(id, cancellationToken);

            if (oldModel == null)
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

            var success = await _adminModelService.DeleteModelAsync(id, cancellationToken);

            if (!success)
            {
                // This should never happen since we just confirmed model exists above
                _logger.LogError("Model existed but deletion failed unexpectedly: {ModelId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    error = new
                    {
                        code = "INTERNAL_ERROR",
                        message = "Model deletion failed unexpectedly"
                    }
                });
            }

            _logger.LogInformation("Successfully deleted model: {ModelId}", id);

            // Story 2.13 Task 14: Log DELETE operation to audit trail
            var userId = User.Identity?.Name ?? "Unknown";
            await _auditLogService.LogDeleteAsync(
                userId,
                "Model",
                id,
                oldModel, // Old values captured before deletion
                cancellationToken);

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

    // ========== Benchmark Score Management Endpoints ==========

    /// <summary>
    /// Adds a benchmark score to a model.
    /// Validates model and benchmark existence, prevents duplicates, calculates normalized score.
    /// </summary>
    /// <param name="modelId">The unique identifier (GUID) of the model.</param>
    /// <param name="dto">The benchmark score creation request with score data.</param>
    /// <param name="cancellationToken">Cancellation token for async operation.</param>
    /// <returns>The created benchmark score with normalized score and out-of-range flag.</returns>
    /// <response code="201">Returns the newly created benchmark score.</response>
    /// <response code="400">If model/benchmark not found or duplicate score exists.</response>
    /// <response code="401">If JWT token is missing, invalid, or expired.</response>
    /// <response code="500">If an unexpected error occurs during processing.</response>
    [HttpPost("{modelId}/benchmarks")]
    [ProducesResponseType(typeof(AdminApiResponse<BenchmarkScoreResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AddBenchmarkScore(
        Guid modelId,
        [FromBody] CreateBenchmarkScoreDto dto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation(
                "Adding benchmark score for model {ModelId}, benchmark {BenchmarkId}",
                modelId,
                dto.BenchmarkId);

            var score = await _adminBenchmarkService.AddScoreAsync(modelId, dto, cancellationToken);

            _logger.LogInformation(
                "Successfully added benchmark score {ScoreId} for model {ModelId}",
                score.Id,
                modelId);

            return CreatedAtAction(
                nameof(GetModelBenchmarkScores),
                new { modelId },
                new AdminApiResponse<BenchmarkScoreResponseDto>
                {
                    Data = score,
                    Meta = new AdminApiResponseMeta
                    {
                        TotalCount = null,
                        Cached = false,
                        Timestamp = DateTime.UtcNow
                    }
                });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation while adding benchmark score for model {ModelId}", modelId);
            return BadRequest(new
            {
                error = new
                {
                    code = ex.Message.Contains("not found") ? "NOT_FOUND" : "DUPLICATE_SCORE",
                    message = ex.Message
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while adding benchmark score for model {ModelId}", modelId);
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                error = new
                {
                    code = "INTERNAL_ERROR",
                    message = "An error occurred while adding the benchmark score",
                    details = ex.Message
                }
            });
        }
    }

    /// <summary>
    /// Retrieves all benchmark scores for a specific model.
    /// </summary>
    /// <param name="modelId">The unique identifier (GUID) of the model.</param>
    /// <param name="cancellationToken">Cancellation token for async operation.</param>
    /// <returns>List of benchmark scores for the model.</returns>
    /// <response code="200">Returns the list of benchmark scores.</response>
    /// <response code="401">If JWT token is missing, invalid, or expired.</response>
    /// <response code="500">If an unexpected error occurs during processing.</response>
    [HttpGet("{modelId}/benchmarks")]
    [ProducesResponseType(typeof(AdminApiResponse<List<BenchmarkScoreResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetModelBenchmarkScores(
        Guid modelId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Retrieving benchmark scores for model {ModelId}", modelId);

            var scores = await _adminBenchmarkService.GetScoresByModelIdAsync(modelId, cancellationToken);

            return Ok(new AdminApiResponse<List<BenchmarkScoreResponseDto>>
            {
                Data = scores,
                Meta = new AdminApiResponseMeta
                {
                    TotalCount = scores.Count,
                    Cached = false,
                    Timestamp = DateTime.UtcNow
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving benchmark scores for model {ModelId}", modelId);
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                error = new
                {
                    code = "INTERNAL_ERROR",
                    message = "An error occurred while retrieving benchmark scores",
                    details = ex.Message
                }
            });
        }
    }

    /// <summary>
    /// Updates an existing benchmark score for a model.
    /// Recalculates normalized score and invalidates cache.
    /// </summary>
    /// <param name="modelId">The unique identifier (GUID) of the model.</param>
    /// <param name="scoreId">The unique identifier (GUID) of the score to update.</param>
    /// <param name="dto">The benchmark score update request with new score data.</param>
    /// <param name="cancellationToken">Cancellation token for async operation.</param>
    /// <returns>The updated benchmark score.</returns>
    /// <response code="200">Returns the updated benchmark score.</response>
    /// <response code="400">If benchmark not found or duplicate exists.</response>
    /// <response code="404">If score not found or doesn't belong to this model.</response>
    /// <response code="401">If JWT token is missing, invalid, or expired.</response>
    /// <response code="500">If an unexpected error occurs during processing.</response>
    [HttpPut("{modelId}/benchmarks/{scoreId}")]
    [ProducesResponseType(typeof(AdminApiResponse<BenchmarkScoreResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateBenchmarkScore(
        Guid modelId,
        Guid scoreId,
        [FromBody] CreateBenchmarkScoreDto dto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation(
                "Updating benchmark score {ScoreId} for model {ModelId}",
                scoreId,
                modelId);

            var score = await _adminBenchmarkService.UpdateScoreAsync(modelId, scoreId, dto, cancellationToken);

            if (score == null)
            {
                _logger.LogWarning("Benchmark score {ScoreId} not found for model {ModelId}", scoreId, modelId);
                return NotFound(new
                {
                    error = new
                    {
                        code = "NOT_FOUND",
                        message = $"Benchmark score {scoreId} not found for model {modelId}"
                    }
                });
            }

            _logger.LogInformation(
                "Successfully updated benchmark score {ScoreId} for model {ModelId}",
                scoreId,
                modelId);

            return Ok(new AdminApiResponse<BenchmarkScoreResponseDto>
            {
                Data = score,
                Meta = new AdminApiResponseMeta
                {
                    TotalCount = null,
                    Cached = false,
                    Timestamp = DateTime.UtcNow
                }
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation while updating benchmark score {ScoreId}", scoreId);
            return BadRequest(new
            {
                error = new
                {
                    code = ex.Message.Contains("not found") ? "NOT_FOUND" : "DUPLICATE_SCORE",
                    message = ex.Message
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating benchmark score {ScoreId}", scoreId);
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                error = new
                {
                    code = "INTERNAL_ERROR",
                    message = "An error occurred while updating the benchmark score",
                    details = ex.Message
                }
            });
        }
    }

    /// <summary>
    /// Deletes a benchmark score from a model (hard delete).
    /// Invalidates cache after deletion.
    /// </summary>
    /// <param name="modelId">The unique identifier (GUID) of the model.</param>
    /// <param name="scoreId">The unique identifier (GUID) of the score to delete.</param>
    /// <param name="cancellationToken">Cancellation token for async operation.</param>
    /// <returns>No content on successful deletion.</returns>
    /// <response code="204">Score successfully deleted.</response>
    /// <response code="404">If score not found or doesn't belong to this model.</response>
    /// <response code="401">If JWT token is missing, invalid, or expired.</response>
    /// <response code="500">If an unexpected error occurs during processing.</response>
    [HttpDelete("{modelId}/benchmarks/{scoreId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteBenchmarkScore(
        Guid modelId,
        Guid scoreId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation(
                "Deleting benchmark score {ScoreId} for model {ModelId}",
                scoreId,
                modelId);

            var success = await _adminBenchmarkService.DeleteScoreAsync(modelId, scoreId, cancellationToken);

            if (!success)
            {
                _logger.LogWarning("Benchmark score {ScoreId} not found for model {ModelId}", scoreId, modelId);
                return NotFound(new
                {
                    error = new
                    {
                        code = "NOT_FOUND",
                        message = $"Benchmark score {scoreId} not found for model {modelId}"
                    }
                });
            }

            _logger.LogInformation(
                "Successfully deleted benchmark score {ScoreId} for model {ModelId}",
                scoreId,
                modelId);

            return NoContent(); // 204 No Content
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting benchmark score {ScoreId}", scoreId);
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                error = new
                {
                    code = "INTERNAL_ERROR",
                    message = "An error occurred while deleting the benchmark score",
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
