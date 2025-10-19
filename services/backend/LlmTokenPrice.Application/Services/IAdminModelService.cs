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
}
