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
}
