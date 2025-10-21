using LlmTokenPrice.Application.DTOs;

namespace LlmTokenPrice.Application.Services;

/// <summary>
/// Service interface for querying model data.
/// Defines use cases for retrieving and presenting model information to API clients.
/// </summary>
/// <remarks>
/// Application layer service that orchestrates domain logic and data access.
/// Implements use cases by coordinating repositories and mapping entities to DTOs.
/// </remarks>
public interface IModelQueryService
{
    /// <summary>
    /// Retrieves all active models with their capabilities and top benchmark scores.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for async operation.</param>
    /// <returns>List of ModelDto objects ready for API response.</returns>
    /// <remarks>
    /// Returns only active models (IsActive = true).
    /// Includes nested CapabilityDto and top 3 BenchmarkScoreDto entries per model.
    /// </remarks>
    Task<List<ModelDto>> GetAllModelsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a single model by its unique identifier with capabilities and benchmark scores.
    /// </summary>
    /// <param name="id">The unique identifier (GUID) of the model.</param>
    /// <param name="cancellationToken">Cancellation token for async operation.</param>
    /// <returns>ModelDto if found, null otherwise.</returns>
    /// <remarks>
    /// Returns null if model doesn't exist or is not active.
    /// Includes nested CapabilityDto and top 3 BenchmarkScoreDto entries.
    /// </remarks>
    Task<ModelDto?> GetModelByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a paginated list of active models with their capabilities and top benchmark scores.
    /// </summary>
    /// <param name="pagination">Pagination parameters (page number and page size).</param>
    /// <param name="cancellationToken">Cancellation token for async operation.</param>
    /// <returns>PagedResult containing ModelDto objects and pagination metadata.</returns>
    /// <remarks>
    /// Story 2.13 Task 5: Pagination implementation.
    ///
    /// Returns only active models (IsActive = true).
    /// Includes nested CapabilityDto and top 3 BenchmarkScoreDto entries per model.
    /// Uses cache-aside pattern with pagination-aware cache keys.
    ///
    /// Response includes:
    /// - items: List of ModelDto for the requested page
    /// - pagination: Metadata (currentPage, totalPages, hasNextPage, etc.)
    /// </remarks>
    Task<PagedResult<ModelDto>> GetAllModelsPagedAsync(
        PaginationParams pagination,
        CancellationToken cancellationToken = default);
}
