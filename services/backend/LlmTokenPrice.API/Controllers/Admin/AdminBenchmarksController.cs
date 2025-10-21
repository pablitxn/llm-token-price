using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LlmTokenPrice.Application.DTOs;
using LlmTokenPrice.Application.Services;
using FluentValidation;

namespace LlmTokenPrice.API.Controllers.Admin;

/// <summary>
/// API controller for admin benchmark management operations.
/// Provides endpoints for viewing, creating, updating, and deleting benchmark definitions.
/// </summary>
/// <remarks>
/// All endpoints require JWT authentication via [Authorize] attribute.
/// Admin endpoints return ALL benchmarks (including inactive) and are NOT cached.
/// Benchmark mutations invalidate cache patterns: cache:benchmarks:*, cache:qaps:*, cache:bestvalue:*
/// </remarks>
[ApiController]
[Route("api/admin/benchmarks")]
[Authorize] // All endpoints require JWT authentication
[Produces("application/json")]
public class AdminBenchmarksController : ControllerBase
{
    private readonly IAdminBenchmarkService _benchmarkService;
    private readonly CSVImportService _csvImportService;
    private readonly IValidator<CreateBenchmarkRequest> _createValidator;
    private readonly IValidator<UpdateBenchmarkRequest> _updateValidator;
    private readonly ILogger<AdminBenchmarksController> _logger;

    /// <summary>
    /// Initializes a new instance of the AdminBenchmarksController.
    /// </summary>
    /// <param name="benchmarkService">The admin benchmark service for data operations.</param>
    /// <param name="csvImportService">The CSV import service for bulk score import (Story 2.11).</param>
    /// <param name="createValidator">FluentValidation validator for create requests.</param>
    /// <param name="updateValidator">FluentValidation validator for update requests.</param>
    /// <param name="logger">Logger for request tracking and diagnostics.</param>
    public AdminBenchmarksController(
        IAdminBenchmarkService benchmarkService,
        CSVImportService csvImportService,
        IValidator<CreateBenchmarkRequest> createValidator,
        IValidator<UpdateBenchmarkRequest> updateValidator,
        ILogger<AdminBenchmarksController> logger)
    {
        _benchmarkService = benchmarkService ?? throw new ArgumentNullException(nameof(benchmarkService));
        _csvImportService = csvImportService ?? throw new ArgumentNullException(nameof(csvImportService));
        _createValidator = createValidator ?? throw new ArgumentNullException(nameof(createValidator));
        _updateValidator = updateValidator ?? throw new ArgumentNullException(nameof(updateValidator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Retrieves all benchmark definitions for admin panel (including inactive).
    /// Supports filtering by category.
    /// </summary>
    /// <param name="includeInactive">If true, includes inactive benchmarks. Default: true.</param>
    /// <param name="category">Optional category filter (Reasoning, Code, Math, Language, Multimodal).</param>
    /// <param name="cancellationToken">Cancellation token for async operation.</param>
    /// <returns>List of all benchmarks ordered alphabetically by name.</returns>
    /// <response code="200">Returns the list of benchmarks (including inactive by default).</response>
    /// <response code="401">If JWT token is missing, invalid, or expired.</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<BenchmarkResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<BenchmarkResponseDto>>> GetAllBenchmarks(
        [FromQuery] bool includeInactive = true,
        [FromQuery] string? category = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Admin: Fetching all benchmarks (includeInactive: {IncludeInactive}, category: {Category})",
            includeInactive, category ?? "all");

        var benchmarks = await _benchmarkService.GetAllBenchmarksAsync(
            includeInactive,
            category,
            cancellationToken);

        return Ok(benchmarks);
    }

    /// <summary>
    /// Retrieves a single benchmark by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier (GUID) of the benchmark.</param>
    /// <param name="cancellationToken">Cancellation token for async operation.</param>
    /// <returns>The benchmark details if found.</returns>
    /// <response code="200">Returns the benchmark details.</response>
    /// <response code="404">If benchmark with specified ID is not found.</response>
    /// <response code="401">If JWT token is missing, invalid, or expired.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(BenchmarkResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<BenchmarkResponseDto>> GetBenchmarkById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Admin: Fetching benchmark {BenchmarkId}", id);

        var benchmark = await _benchmarkService.GetBenchmarkByIdAsync(id, cancellationToken);

        if (benchmark == null)
        {
            _logger.LogWarning("Admin: Benchmark {BenchmarkId} not found", id);
            return NotFound(new { error = "Benchmark not found" });
        }

        return Ok(benchmark);
    }

    /// <summary>
    /// Creates a new benchmark definition.
    /// Validates unique name constraint (case-insensitive).
    /// </summary>
    /// <param name="request">The benchmark creation request with all required fields.</param>
    /// <param name="cancellationToken">Cancellation token for async operation.</param>
    /// <returns>Created benchmark with Location header pointing to GET endpoint.</returns>
    /// <response code="201">Benchmark created successfully. Returns benchmark details with Location header.</response>
    /// <response code="400">If validation fails (invalid fields, range validation, etc.).</response>
    /// <response code="409">If benchmark with same name already exists (case-insensitive).</response>
    /// <response code="401">If JWT token is missing, invalid, or expired.</response>
    [HttpPost]
    [ProducesResponseType(typeof(BenchmarkResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<BenchmarkResponseDto>> CreateBenchmark(
        [FromBody] CreateBenchmarkRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Admin: Creating benchmark {BenchmarkName}", request.BenchmarkName);

        // Validate request using FluentValidation
        var validationResult = await _createValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            _logger.LogWarning("Admin: Validation failed for benchmark creation: {Errors}",
                string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));
            return BadRequest(new { errors = validationResult.Errors.Select(e => new { field = e.PropertyName, message = e.ErrorMessage }) });
        }

        try
        {
            // Create benchmark
            var benchmarkId = await _benchmarkService.CreateBenchmarkAsync(request, cancellationToken);

            // Fetch created benchmark to return in response
            var createdBenchmark = await _benchmarkService.GetBenchmarkByIdAsync(benchmarkId, cancellationToken);

            _logger.LogInformation("Admin: Benchmark {BenchmarkId} created successfully", benchmarkId);

            // Return 201 Created with Location header
            return CreatedAtAction(
                nameof(GetBenchmarkById),
                new { id = benchmarkId },
                createdBenchmark);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already exists"))
        {
            _logger.LogWarning("Admin: Duplicate benchmark name: {BenchmarkName}", request.BenchmarkName);
            return Conflict(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Updates an existing benchmark definition.
    /// BenchmarkName is immutable and cannot be changed.
    /// </summary>
    /// <param name="id">The unique identifier (GUID) of the benchmark to update.</param>
    /// <param name="request">The benchmark update request.</param>
    /// <param name="cancellationToken">Cancellation token for async operation.</param>
    /// <returns>Updated benchmark details.</returns>
    /// <response code="200">Benchmark updated successfully. Returns updated benchmark details.</response>
    /// <response code="400">If validation fails.</response>
    /// <response code="404">If benchmark with specified ID is not found.</response>
    /// <response code="401">If JWT token is missing, invalid, or expired.</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(BenchmarkResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<BenchmarkResponseDto>> UpdateBenchmark(
        Guid id,
        [FromBody] UpdateBenchmarkRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Admin: Updating benchmark {BenchmarkId}", id);

        // Validate request using FluentValidation
        var validationResult = await _updateValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            _logger.LogWarning("Admin: Validation failed for benchmark update: {Errors}",
                string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));
            return BadRequest(new { errors = validationResult.Errors.Select(e => new { field = e.PropertyName, message = e.ErrorMessage }) });
        }

        // Update benchmark
        var updatedBenchmark = await _benchmarkService.UpdateBenchmarkAsync(id, request, cancellationToken);

        if (updatedBenchmark == null)
        {
            _logger.LogWarning("Admin: Benchmark {BenchmarkId} not found for update", id);
            return NotFound(new { error = "Benchmark not found" });
        }

        _logger.LogInformation("Admin: Benchmark {BenchmarkId} updated successfully", id);
        return Ok(updatedBenchmark);
    }

    /// <summary>
    /// Deletes a benchmark (soft delete - sets IsActive = false).
    /// Checks for dependent BenchmarkScores before deletion.
    /// </summary>
    /// <param name="id">The unique identifier (GUID) of the benchmark to delete.</param>
    /// <param name="cancellationToken">Cancellation token for async operation.</param>
    /// <returns>No content on successful deletion.</returns>
    /// <response code="204">Benchmark deleted successfully (soft delete).</response>
    /// <response code="400">If benchmark has dependent scores (cannot be deleted).</response>
    /// <response code="404">If benchmark with specified ID is not found.</response>
    /// <response code="401">If JWT token is missing, invalid, or expired.</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteBenchmark(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Admin: Deleting benchmark {BenchmarkId}", id);

        try
        {
            var deleted = await _benchmarkService.DeleteBenchmarkAsync(id, cancellationToken);

            if (!deleted)
            {
                _logger.LogWarning("Admin: Benchmark {BenchmarkId} not found for deletion", id);
                return NotFound(new { error = "Benchmark not found or already inactive" });
            }

            _logger.LogInformation("Admin: Benchmark {BenchmarkId} deleted successfully", id);
            return NoContent();
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("associated scores"))
        {
            _logger.LogWarning("Admin: Cannot delete benchmark {BenchmarkId}: {Reason}", id, ex.Message);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Imports multiple benchmark scores via CSV file upload (Story 2.11 AC#3).
    /// Validates each row, imports valid scores in batch, returns detailed results.
    /// </summary>
    /// <param name="file">CSV file with columns: model_id, benchmark_name, score, max_score, test_date, source_url, verified, notes</param>
    /// <param name="cancellationToken">Cancellation token for async operation</param>
    /// <returns>
    /// 200 OK: Import result with success/failure counts and error details (partial success allowed)
    /// 400 Bad Request: No file uploaded, invalid file format, or parsing completely failed
    /// 401 Unauthorized: Missing or invalid JWT token
    /// 413 Payload Too Large: File exceeds 10MB limit (handled by [RequestSizeLimit] attribute)
    /// </returns>
    /// <remarks>
    /// Partial success pattern: Valid rows are imported even if some rows fail validation.
    /// Returns 200 OK with detailed error list for failed rows.
    /// Returns 400 only if the file itself is invalid or cannot be parsed at all.
    /// </remarks>
    [HttpPost("import-csv")]
    [RequestSizeLimit(10_485_760)] // 10MB file size limit
    [ProducesResponseType(typeof(CSVImportResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ImportBenchmarkScoresCSV(
        IFormFile file,
        CancellationToken cancellationToken = default)
    {
        // Validate file presence
        if (file == null || file.Length == 0)
        {
            _logger.LogWarning("Admin: CSV import attempted with no file");
            return BadRequest(new { error = "No file uploaded" });
        }

        // Validate file extension
        if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Admin: CSV import attempted with invalid file format: {FileName}", file.FileName);
            return BadRequest(new { error = "File must be CSV format" });
        }

        _logger.LogInformation(
            "Admin: Starting CSV import - File: {FileName}, Size: {FileSize} bytes",
            file.FileName,
            file.Length);

        try
        {
            // Process CSV file
            using var stream = file.OpenReadStream();
            var result = await _csvImportService.ImportBenchmarkScoresAsync(
                stream,
                skipDuplicates: true, // Task 9: Default to skip duplicates
                cancellationToken);

            _logger.LogInformation(
                "Admin: CSV import completed - Total: {Total}, Successful: {Success}, Failed: {Failed}, Skipped: {Skipped}",
                result.TotalRows,
                result.SuccessfulImports,
                result.FailedImports,
                result.SkippedDuplicates);

            // Return 200 OK even with partial failures (Story 2.11 AC#6)
            return Ok(new
            {
                data = result,
                meta = new
                {
                    message = $"Import completed: {result.SuccessfulImports} successful, {result.FailedImports} failed, {result.SkippedDuplicates} skipped",
                    timestamp = DateTime.UtcNow,
                    cached = false
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Admin: Fatal error during CSV import - File: {FileName}", file.FileName);
            return BadRequest(new { error = "CSV file could not be processed. Please check file format and try again." });
        }
    }
}
