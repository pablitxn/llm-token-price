using LlmTokenPrice.Application.DTOs;

namespace LlmTokenPrice.Application.Services;

/// <summary>
/// Service interface for admin-specific benchmark management operations.
/// Orchestrates benchmark CRUD with validation, duplicate detection, and cache invalidation.
/// </summary>
/// <remarks>
/// Application layer service that implements admin use cases by:
/// 1. Calling repository to persist/fetch benchmark entities
/// 2. Mapping entities to BenchmarkResponseDto for API consumption
/// 3. Enforcing business rules (unique names, dependency checks before deletion)
/// 4. Invalidating cache patterns after mutations
/// </remarks>
public interface IAdminBenchmarkService
{
    /// <summary>
    /// Retrieves all benchmark definitions from the database.
    /// </summary>
    /// <param name="includeInactive">If true, includes inactive benchmarks. Default: true for admin.</param>
    /// <param name="categoryFilter">Optional category filter.</param>
    /// <param name="cancellationToken">Cancellation token for async operation.</param>
    /// <returns>List of BenchmarkResponseDto ordered alphabetically by name.</returns>
    Task<List<BenchmarkResponseDto>> GetAllBenchmarksAsync(
        bool includeInactive = true,
        string? categoryFilter = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a single benchmark by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier (GUID) of the benchmark.</param>
    /// <param name="cancellationToken">Cancellation token for async operation.</param>
    /// <returns>BenchmarkResponseDto if found, null otherwise.</returns>
    Task<BenchmarkResponseDto?> GetBenchmarkByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new benchmark definition.
    /// Validates unique name constraint and invalidates cache after creation.
    /// </summary>
    /// <param name="request">The benchmark creation request with all required fields.</param>
    /// <param name="cancellationToken">Cancellation token for async operation.</param>
    /// <returns>The unique identifier (GUID) of the created benchmark.</returns>
    /// <exception cref="InvalidOperationException">Thrown if benchmark name already exists.</exception>
    Task<Guid> CreateBenchmarkAsync(CreateBenchmarkRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing benchmark definition.
    /// BenchmarkName is immutable (cannot be changed). Invalidates cache after update.
    /// </summary>
    /// <param name="id">The unique identifier (GUID) of the benchmark to update.</param>
    /// <param name="request">The benchmark update request.</param>
    /// <param name="cancellationToken">Cancellation token for async operation.</param>
    /// <returns>Updated BenchmarkResponseDto if found, null if benchmark doesn't exist.</returns>
    Task<BenchmarkResponseDto?> UpdateBenchmarkAsync(Guid id, UpdateBenchmarkRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a benchmark (soft delete - sets IsActive = false).
    /// Checks for dependent BenchmarkScores before deletion.
    /// </summary>
    /// <param name="id">The unique identifier (GUID) of the benchmark to delete.</param>
    /// <param name="cancellationToken">Cancellation token for async operation.</param>
    /// <returns>True if benchmark was found and deleted, false if not found.</returns>
    /// <exception cref="InvalidOperationException">Thrown if benchmark has dependent scores.</exception>
    Task<bool> DeleteBenchmarkAsync(Guid id, CancellationToken cancellationToken = default);

    // ========== Benchmark Score Management Methods ==========

    /// <summary>
    /// Adds a benchmark score for a model.
    /// Validates model and benchmark existence, checks for duplicates, calculates normalized score.
    /// </summary>
    /// <param name="modelId">The unique identifier (GUID) of the model.</param>
    /// <param name="dto">The benchmark score creation request.</param>
    /// <param name="cancellationToken">Cancellation token for async operation.</param>
    /// <returns>BenchmarkScoreResponseDto with normalized score and out-of-range flag.</returns>
    /// <exception cref="InvalidOperationException">Thrown if model/benchmark not found or duplicate score exists.</exception>
    Task<BenchmarkScoreResponseDto> AddScoreAsync(
        Guid modelId,
        CreateBenchmarkScoreDto dto,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all benchmark scores for a specific model.
    /// </summary>
    /// <param name="modelId">The unique identifier (GUID) of the model.</param>
    /// <param name="cancellationToken">Cancellation token for async operation.</param>
    /// <returns>List of BenchmarkScoreResponseDto ordered by benchmark name.</returns>
    Task<List<BenchmarkScoreResponseDto>> GetScoresByModelIdAsync(Guid modelId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing benchmark score.
    /// Recalculates normalized score and invalidates cache.
    /// </summary>
    /// <param name="modelId">The unique identifier (GUID) of the model.</param>
    /// <param name="scoreId">The unique identifier (GUID) of the score to update.</param>
    /// <param name="dto">The benchmark score update request.</param>
    /// <param name="cancellationToken">Cancellation token for async operation.</param>
    /// <returns>Updated BenchmarkScoreResponseDto if found, null if score doesn't exist.</returns>
    Task<BenchmarkScoreResponseDto?> UpdateScoreAsync(
        Guid modelId,
        Guid scoreId,
        CreateBenchmarkScoreDto dto,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a benchmark score (hard delete - physical removal).
    /// Invalidates cache after deletion.
    /// </summary>
    /// <param name="modelId">The unique identifier (GUID) of the model.</param>
    /// <param name="scoreId">The unique identifier (GUID) of the score to delete.</param>
    /// <param name="cancellationToken">Cancellation token for async operation.</param>
    /// <returns>True if score was found and deleted, false if not found.</returns>
    Task<bool> DeleteScoreAsync(Guid modelId, Guid scoreId, CancellationToken cancellationToken = default);
}
