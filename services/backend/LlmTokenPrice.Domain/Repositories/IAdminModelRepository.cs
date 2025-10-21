using LlmTokenPrice.Domain.Entities;

namespace LlmTokenPrice.Domain.Repositories;

/// <summary>
/// Repository interface (port) for admin-specific Model entity data access.
/// Extends base IModelRepository with admin-specific queries (search, all statuses).
/// </summary>
/// <remarks>
/// This is a "port" in Hexagonal Architecture for admin use cases.
/// Key differences from public IModelRepository:
/// - Returns ALL models (active, inactive, deprecated, beta)
/// - Supports search/filter parameters
/// - Orders by UpdatedAt DESC (most recent first) for admin panel
/// - No caching (admin needs real-time data for management tasks)
/// </remarks>
public interface IAdminModelRepository
{
    /// <summary>
    /// Retrieves all models from the database (including inactive).
    /// Includes related Capability and BenchmarkScores with Benchmark data.
    /// </summary>
    /// <param name="searchTerm">Optional search term to filter by model name or provider (case-insensitive).</param>
    /// <param name="provider">Optional provider filter (exact match, case-insensitive).</param>
    /// <param name="status">Optional status filter (exact match, case-insensitive).</param>
    /// <param name="cancellationToken">Cancellation token for async operation.</param>
    /// <returns>List of models (including inactive) with all related data loaded.</returns>
    /// <remarks>
    /// Unlike public repository, this returns ALL models regardless of IsActive status.
    /// Results ordered by UpdatedAt DESC (most recently updated first).
    /// Eagerly loads Capability and BenchmarkScores.ThenInclude(Benchmark) to avoid N+1 queries.
    /// </remarks>
    Task<List<Model>> GetAllModelsAsync(
        string? searchTerm = null,
        string? provider = null,
        string? status = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a paginated list of models from the database (including inactive).
    /// Includes related Capability and BenchmarkScores with Benchmark data.
    /// </summary>
    /// <param name="page">Page number (1-indexed).</param>
    /// <param name="pageSize">Number of items per page.</param>
    /// <param name="searchTerm">Optional search term to filter by model name or provider (case-insensitive).</param>
    /// <param name="provider">Optional provider filter (exact match, case-insensitive).</param>
    /// <param name="status">Optional status filter (exact match, case-insensitive).</param>
    /// <param name="cancellationToken">Cancellation token for async operation.</param>
    /// <returns>Tuple containing the list of models for the requested page and the total count of filtered models.</returns>
    /// <remarks>
    /// Story 2.13 Task 5: Pagination implementation for admin endpoints.
    ///
    /// Unlike public repository, this returns ALL models regardless of IsActive status.
    /// Applies filters FIRST, then pagination on the filtered result set.
    /// Results ordered by UpdatedAt DESC (most recently updated first).
    /// Eagerly loads Capability and BenchmarkScores.ThenInclude(Benchmark) to avoid N+1 queries.
    ///
    /// Implementation uses efficient database-level pagination:
    /// - Filter by searchTerm/provider/status (if provided)
    /// - Count filtered results for total
    /// - Skip((page - 1) * pageSize)
    /// - Take(pageSize)
    ///
    /// Page numbering: 1-indexed (page=1 is first page)
    /// </remarks>
    Task<(List<Model> Items, int TotalCount)> GetAllModelsPagedAsync(
        int page,
        int pageSize,
        string? searchTerm = null,
        string? provider = null,
        string? status = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a single model by its unique identifier (including inactive).
    /// Includes related Capability and BenchmarkScores with Benchmark data.
    /// </summary>
    /// <param name="id">The unique identifier (GUID) of the model.</param>
    /// <param name="cancellationToken">Cancellation token for async operation.</param>
    /// <returns>Model if found, null otherwise.</returns>
    /// <remarks>
    /// Unlike public repository, this returns model even if IsActive = false.
    /// Eagerly loads Capability and BenchmarkScores.ThenInclude(Benchmark) to avoid N+1 queries.
    /// </remarks>
    Task<Model?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a model (soft delete - sets IsActive = false and updates UpdatedAt).
    /// Model data is preserved in database for audit trail.
    /// </summary>
    /// <param name="id">The unique identifier (GUID) of the model to delete.</param>
    /// <param name="cancellationToken">Cancellation token for async operation.</param>
    /// <returns>True if model was found and deleted, false if not found.</returns>
    /// <remarks>
    /// Performs soft delete by:
    /// 1. Setting IsActive = false (model won't appear in public API)
    /// 2. Setting UpdatedAt = DateTime.UtcNow (audit trail)
    /// 3. Saving changes to database
    /// Model is NOT physically removed from database, preserving audit trail.
    /// </remarks>
    Task<bool> DeleteModelAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new model in the database.
    /// Sets CreatedAt and UpdatedAt to DateTime.UtcNow, IsActive to true.
    /// </summary>
    /// <param name="model">The model entity to create.</param>
    /// <param name="cancellationToken">Cancellation token for async operation.</param>
    /// <returns>The unique identifier (GUID) of the created model.</returns>
    /// <remarks>
    /// This method:
    /// 1. Adds the model to the DbContext
    /// 2. Calls SaveChangesAsync to persist to database
    /// 3. Returns the generated GUID
    /// Must be called within same transaction as CreateCapabilityAsync.
    /// </remarks>
    Task<Guid> CreateModelAsync(Model model, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new capability record linked to a model.
    /// Must be called after CreateModelAsync in the same transaction.
    /// </summary>
    /// <param name="capability">The capability entity to create with ModelId set.</param>
    /// <param name="cancellationToken">Cancellation token for async operation.</param>
    /// <remarks>
    /// This method:
    /// 1. Adds the capability to the DbContext
    /// 2. Calls SaveChangesAsync to persist to database
    /// Capability.ModelId must reference a valid model created in same transaction.
    /// One-to-one relationship enforced via unique constraint on ModelId.
    /// </remarks>
    Task CreateCapabilityAsync(Capability capability, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a model by its name and provider combination (case-insensitive).
    /// Used for duplicate detection before creating a new model.
    /// </summary>
    /// <param name="name">The model name to search for (case-insensitive).</param>
    /// <param name="provider">The provider name to search for (case-insensitive).</param>
    /// <param name="cancellationToken">Cancellation token for async operation.</param>
    /// <returns>Model if found with matching name and provider, null otherwise.</returns>
    /// <remarks>
    /// Case-insensitive comparison for duplicate detection.
    /// Returns model even if IsActive = false (duplicates not allowed regardless of status).
    /// Does not include related entities (Capability, BenchmarkScores) for performance.
    /// </remarks>
    Task<Model?> GetByNameAndProviderAsync(string name, string provider, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves all pending changes to the database.
    /// Used for updating entities tracked by EF Core change tracking.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for async operation.</param>
    /// <remarks>
    /// This method commits all tracked changes from the DbContext to the database.
    /// Typically used after updating entities fetched from GetByIdAsync.
    /// </remarks>
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
