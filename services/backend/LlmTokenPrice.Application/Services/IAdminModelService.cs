using LlmTokenPrice.Application.DTOs;

namespace LlmTokenPrice.Application.Services;

/// <summary>
/// Service interface for admin-specific model data operations.
/// Defines contract for admin panel use cases (all models, search, management).
/// </summary>
/// <remarks>
/// Application layer service interface for admin use cases.
/// Unlike public IModelQueryService:
/// - Returns ALL models (including inactive, deprecated, beta)
/// - Supports search and filter parameters
/// - Returns AdminModelDto with full audit timestamps
/// - No caching (admin needs real-time data)
/// </remarks>
public interface IAdminModelService
{
    /// <summary>
    /// Retrieves all models for admin panel (including inactive).
    /// Supports search by name/provider and filtering by status.
    /// </summary>
    /// <param name="searchTerm">Optional search term to filter by model name or provider (case-insensitive).</param>
    /// <param name="provider">Optional provider filter (exact match, case-insensitive).</param>
    /// <param name="status">Optional status filter (exact match, case-insensitive).</param>
    /// <param name="cancellationToken">Cancellation token for async operation.</param>
    /// <returns>List of AdminModelDto with all fields (including CreatedAt, IsActive).</returns>
    /// <remarks>
    /// Returns models ordered by UpdatedAt DESC (most recently updated first).
    /// Includes all statuses: active, deprecated, beta, and IsActive=false models.
    /// </remarks>
    Task<List<AdminModelDto>> GetAllModelsAsync(
        string? searchTerm = null,
        string? provider = null,
        string? status = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a paginated list of models for admin panel (including inactive).
    /// Supports search by name/provider and filtering by status.
    /// </summary>
    /// <param name="pagination">Pagination parameters (page number and page size).</param>
    /// <param name="searchTerm">Optional search term to filter by model name or provider (case-insensitive).</param>
    /// <param name="provider">Optional provider filter (exact match, case-insensitive).</param>
    /// <param name="status">Optional status filter (exact match, case-insensitive).</param>
    /// <param name="cancellationToken">Cancellation token for async operation.</param>
    /// <returns>PagedResult containing AdminModelDto objects and pagination metadata.</returns>
    /// <remarks>
    /// Story 2.13 Task 5: Pagination implementation for admin endpoints.
    ///
    /// Returns models ordered by UpdatedAt DESC (most recently updated first).
    /// Includes all statuses: active, deprecated, beta, and IsActive=false models.
    /// Applies filters FIRST, then pagination on the filtered result set.
    ///
    /// Response includes:
    /// - items: List of AdminModelDto for the requested page
    /// - pagination: Metadata (currentPage, totalPages, hasNextPage, etc.)
    ///
    /// Admin endpoints do NOT use caching (always fresh data from database).
    /// </remarks>
    Task<PagedResult<AdminModelDto>> GetAllModelsPagedAsync(
        PaginationParams pagination,
        string? searchTerm = null,
        string? provider = null,
        string? status = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a single model by ID for admin editing (including inactive).
    /// </summary>
    /// <param name="id">The unique identifier (GUID) of the model.</param>
    /// <param name="cancellationToken">Cancellation token for async operation.</param>
    /// <returns>AdminModelDto if found, null otherwise.</returns>
    /// <remarks>
    /// Unlike public GetModelByIdAsync, returns model even if IsActive = false.
    /// </remarks>
    Task<AdminModelDto?> GetModelByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a model (soft delete - sets IsActive = false).
    /// Model data is preserved for audit purposes.
    /// </summary>
    /// <param name="id">The unique identifier (GUID) of the model to delete.</param>
    /// <param name="cancellationToken">Cancellation token for async operation.</param>
    /// <returns>True if model was found and deleted, false if not found.</returns>
    /// <remarks>
    /// Performs soft delete by setting IsActive = false and updating UpdatedAt timestamp.
    /// Model will no longer appear in public API but remains in database for audit trail.
    /// </remarks>
    Task<bool> DeleteModelAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new model with default capabilities in the database.
    /// Validates request, checks for duplicates, and persists model + capabilities in single transaction.
    /// </summary>
    /// <param name="request">The model creation request with all required fields.</param>
    /// <param name="cancellationToken">Cancellation token for async operation.</param>
    /// <returns>The unique identifier (GUID) of the created model.</returns>
    /// <exception cref="InvalidOperationException">Thrown when duplicate model (same name + provider) exists.</exception>
    /// <remarks>
    /// This method:
    /// 1. Validates request using FluentValidation (automatically via middleware)
    /// 2. Checks for duplicate model (case-insensitive name + provider)
    /// 3. Creates Model entity with timestamps (CreatedAt, UpdatedAt = DateTime.UtcNow, IsActive = true)
    /// 4. Creates Capability entity with default values (ContextWindow=0, all flags=false except SupportsStreaming=true)
    /// 5. Persists both entities in single EF Core transaction
    /// 6. Returns new model GUID for Location header in controller
    /// </remarks>
    Task<Guid> CreateModelAsync(CreateModelRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing model and its capabilities in the database.
    /// Fetches model, validates for duplicates (excluding self), updates properties, and refreshes UpdatedAt timestamp.
    /// </summary>
    /// <param name="id">The unique identifier (GUID) of the model to update.</param>
    /// <param name="request">The model update request with fields to modify.</param>
    /// <param name="cancellationToken">Cancellation token for async operation.</param>
    /// <returns>The updated AdminModelDto if model was found and updated, null if not found.</returns>
    /// <exception cref="InvalidOperationException">Thrown when duplicate model (same name + provider) exists on a DIFFERENT model.</exception>
    /// <remarks>
    /// This method:
    /// 1. Fetches existing model by ID (returns null if not found)
    /// 2. Checks for duplicate name+provider on DIFFERENT models (allows same model to keep its values)
    /// 3. Updates Model entity fields from request
    /// 4. Updates Capability entity fields from nested request
    /// 5. Sets UpdatedAt = DateTime.UtcNow
    /// 6. Persists changes in single EF Core transaction
    /// 7. Returns updated AdminModelDto for response
    /// </remarks>
    Task<AdminModelDto?> UpdateModelAsync(Guid id, CreateModelRequest request, CancellationToken cancellationToken = default);
}
