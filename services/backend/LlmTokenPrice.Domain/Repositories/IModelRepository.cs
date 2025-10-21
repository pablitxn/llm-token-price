using LlmTokenPrice.Domain.Entities;

namespace LlmTokenPrice.Domain.Repositories;

/// <summary>
/// Repository interface (port) for Model entity data access.
/// Defines contract for model data operations without infrastructure dependencies.
/// </summary>
/// <remarks>
/// This is a "port" in Hexagonal Architecture - the Domain layer defines what it needs,
/// and the Infrastructure layer provides concrete implementations ("adapters").
/// Implementations should use EF Core with eager loading (Include) to avoid N+1 queries.
/// </remarks>
public interface IModelRepository
{
    /// <summary>
    /// Retrieves all active models from the database.
    /// Includes related Capability and BenchmarkScores with Benchmark data.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for async operation.</param>
    /// <returns>List of active models with all related data loaded.</returns>
    /// <remarks>
    /// Only returns models where IsActive = true (excludes soft-deleted models).
    /// Eagerly loads Capability and BenchmarkScores.ThenInclude(Benchmark) to avoid N+1 queries.
    /// </remarks>
    Task<List<Model>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a single model by its unique identifier.
    /// Includes related Capability and BenchmarkScores with Benchmark data.
    /// </summary>
    /// <param name="id">The unique identifier (GUID) of the model.</param>
    /// <param name="cancellationToken">Cancellation token for async operation.</param>
    /// <returns>Model if found, null otherwise.</returns>
    /// <remarks>
    /// Returns null if model doesn't exist or IsActive = false.
    /// Eagerly loads Capability and BenchmarkScores.ThenInclude(Benchmark) to avoid N+1 queries.
    /// </remarks>
    Task<Model?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a paginated list of active models from the database.
    /// Includes related Capability and BenchmarkScores with Benchmark data.
    /// </summary>
    /// <param name="page">Page number (1-indexed).</param>
    /// <param name="pageSize">Number of items per page.</param>
    /// <param name="cancellationToken">Cancellation token for async operation.</param>
    /// <returns>Tuple containing the list of models for the requested page and the total count of active models.</returns>
    /// <remarks>
    /// Story 2.13 Task 5: Pagination implementation.
    ///
    /// Only returns models where IsActive = true (excludes soft-deleted models).
    /// Eagerly loads Capability and BenchmarkScores.ThenInclude(Benchmark) to avoid N+1 queries.
    ///
    /// Implementation uses efficient database-level pagination:
    /// - Skip((page - 1) * pageSize)
    /// - Take(pageSize)
    /// - Separate Count() query for total (can be cached)
    ///
    /// Page numbering: 1-indexed (page=1 is first page)
    /// </remarks>
    Task<(List<Model> Items, int TotalCount)> GetAllPagedAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}
