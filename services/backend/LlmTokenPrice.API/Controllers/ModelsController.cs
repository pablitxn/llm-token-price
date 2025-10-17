using LlmTokenPrice.Application.DTOs;
using LlmTokenPrice.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace LlmTokenPrice.API.Controllers;

/// <summary>
/// API controller for LLM model data operations.
/// Provides endpoints for querying model pricing, capabilities, and benchmark scores.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ModelsController : ControllerBase
{
    private readonly IModelQueryService _queryService;
    private readonly ILogger<ModelsController> _logger;

    /// <summary>
    /// Initializes a new instance of the ModelsController.
    /// </summary>
    /// <param name="queryService">The model query service for data operations.</param>
    /// <param name="logger">Logger for request tracking and diagnostics.</param>
    public ModelsController(IModelQueryService queryService, ILogger<ModelsController> logger)
    {
        _queryService = queryService ?? throw new ArgumentNullException(nameof(queryService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Retrieves all active LLM models with pricing, capabilities, and top benchmark scores.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for async operation.</param>
    /// <returns>List of models with nested capability and benchmark data.</returns>
    /// <response code="200">Returns the list of active models.</response>
    /// <response code="500">If an unexpected error occurs during processing.</response>
    /// <example>
    /// GET /api/models
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
    ///       "updatedAt": "2024-01-15T10:30:00Z",
    ///       "capabilities": {
    ///         "contextWindow": 8192,
    ///         "maxOutputTokens": 4096,
    ///         "supportsFunctionCalling": true,
    ///         "supportsVision": false,
    ///         "supportsAudioInput": false,
    ///         "supportsAudioOutput": false,
    ///         "supportsStreaming": true,
    ///         "supportsJsonMode": true
    ///       },
    ///       "topBenchmarks": [
    ///         {
    ///           "benchmarkName": "MMLU",
    ///           "score": 86.4,
    ///           "maxScore": 100.0,
    ///           "normalizedScore": 0.864
    ///         }
    ///       ]
    ///     }
    ///   ],
    ///   "meta": {
    ///     "count": 10,
    ///     "cached": false,
    ///     "timestamp": "2024-01-15T10:30:00Z"
    ///   }
    /// }
    /// </example>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<ModelDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Fetching all active models");

            var models = await _queryService.GetAllModelsAsync(cancellationToken);

            _logger.LogInformation("Successfully retrieved {Count} models", models.Count);

            var response = new ApiResponse<List<ModelDto>>
            {
                Data = models,
                Meta = new ApiResponseMeta
                {
                    Count = models.Count,
                    Cached = false,
                    Timestamp = DateTime.UtcNow
                }
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching models");
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
    /// Retrieves a single LLM model by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier (GUID) of the model.</param>
    /// <param name="cancellationToken">Cancellation token for async operation.</param>
    /// <returns>Model with nested capability and benchmark data.</returns>
    /// <response code="200">Returns the requested model.</response>
    /// <response code="404">If the model is not found or is inactive.</response>
    /// <response code="500">If an unexpected error occurs during processing.</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<ModelDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Fetching model with ID: {ModelId}", id);

            var model = await _queryService.GetModelByIdAsync(id, cancellationToken);

            if (model == null)
            {
                _logger.LogWarning("Model not found: {ModelId}", id);
                return NotFound(new
                {
                    error = new
                    {
                        code = "NOT_FOUND",
                        message = $"Model with ID {id} not found or is inactive"
                    }
                });
            }

            _logger.LogInformation("Successfully retrieved model: {ModelName}", model.Name);

            var response = new ApiResponse<ModelDto>
            {
                Data = model,
                Meta = new ApiResponseMeta
                {
                    Cached = false,
                    Timestamp = DateTime.UtcNow
                }
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching model {ModelId}", id);
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
}

/// <summary>
/// Standard API response wrapper for successful operations.
/// </summary>
/// <typeparam name="T">The type of data being returned.</typeparam>
public class ApiResponse<T>
{
    /// <summary>
    /// The actual data payload (model, list of models, etc.).
    /// </summary>
    public required T Data { get; init; }

    /// <summary>
    /// Metadata about the response (count, cache status, timestamp).
    /// </summary>
    public required ApiResponseMeta Meta { get; init; }
}

/// <summary>
/// Metadata included in all API responses.
/// </summary>
public class ApiResponseMeta
{
    /// <summary>
    /// Number of items in the response (for collection responses).
    /// Null for single-item responses.
    /// </summary>
    public int? Count { get; init; }

    /// <summary>
    /// Indicates whether the response was served from cache.
    /// Always false in Epic 1 (caching added in Epic 2).
    /// </summary>
    public required bool Cached { get; init; }

    /// <summary>
    /// UTC timestamp when the response was generated.
    /// </summary>
    public required DateTime Timestamp { get; init; }
}
