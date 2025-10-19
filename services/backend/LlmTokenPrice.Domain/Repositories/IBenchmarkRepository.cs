using LlmTokenPrice.Domain.Entities;

namespace LlmTokenPrice.Domain.Repositories;

/// <summary>
/// Repository interface (port) for Benchmark entity data access (admin operations).
/// Provides full CRUD operations for benchmark definition management.
/// </summary>
/// <remarks>
/// This is a "port" in Hexagonal Architecture for benchmark admin use cases.
/// Key operations:
/// - GetAllAsync: Returns ALL benchmarks (active and inactive) for admin panel
/// - GetByNameAsync: Case-insensitive lookup for duplicate detection
/// - AddAsync/UpdateAsync/DeleteAsync: Full CRUD for benchmark lifecycle
/// No caching at repository level (caching handled by Application/API layers).
/// </remarks>
public interface IBenchmarkRepository
{
    /// <summary>
    /// Retrieves all benchmark definitions from the database (including inactive).
    /// </summary>
    /// <param name="includeInactive">If true, includes benchmarks with IsActive = false. Default: true for admin.</param>
    /// <param name="categoryFilter">Optional category filter (exact match).</param>
    /// <param name="cancellationToken">Cancellation token for async operation.</param>
    /// <returns>List of all benchmarks ordered by BenchmarkName ascending.</returns>
    /// <remarks>
    /// Unlike public repository, this returns ALL benchmarks regardless of IsActive status by default.
    /// Results ordered alphabetically by BenchmarkName for admin list view.
    /// </remarks>
    Task<List<Benchmark>> GetAllAsync(
        bool includeInactive = true,
        string? categoryFilter = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a single benchmark by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier (GUID) of the benchmark.</param>
    /// <param name="cancellationToken">Cancellation token for async operation.</param>
    /// <returns>Benchmark if found, null otherwise.</returns>
    /// <remarks>
    /// Returns benchmark even if IsActive = false (admin needs to view inactive benchmarks).
    /// Does not eagerly load Scores navigation property for performance.
    /// </remarks>
    Task<Benchmark?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a benchmark by its unique name (case-insensitive).
    /// Used for duplicate detection before creating a new benchmark.
    /// </summary>
    /// <param name="benchmarkName">The benchmark name to search for (case-insensitive).</param>
    /// <param name="cancellationToken">Cancellation token for async operation.</param>
    /// <returns>Benchmark if found with matching name, null otherwise.</returns>
    /// <remarks>
    /// Case-insensitive comparison using EF.Functions.ILike for PostgreSQL (ILIKE operator).
    /// Returns benchmark even if IsActive = false (duplicates not allowed regardless of status).
    /// Does not include related entities (Scores) for performance.
    /// </remarks>
    Task<Benchmark?> GetByNameAsync(string benchmarkName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new benchmark to the database.
    /// Sets CreatedAt to DateTime.UtcNow and IsActive to true.
    /// </summary>
    /// <param name="benchmark">The benchmark entity to create.</param>
    /// <param name="cancellationToken">Cancellation token for async operation.</param>
    /// <remarks>
    /// This method:
    /// 1. Sets CreatedAt = DateTime.UtcNow
    /// 2. Sets IsActive = true (default)
    /// 3. Adds benchmark to DbContext
    /// 4. Does NOT call SaveChangesAsync (caller responsible for transaction management)
    /// Must call SaveChangesAsync separately to persist.
    /// </remarks>
    Task AddAsync(Benchmark benchmark, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing benchmark.
    /// EF Core change tracking handles UPDATE on SaveChangesAsync call.
    /// </summary>
    /// <param name="benchmark">The benchmark entity to update (must be tracked by DbContext).</param>
    /// <param name="cancellationToken">Cancellation token for async operation.</param>
    /// <remarks>
    /// This method updates the benchmark entity in the DbContext.
    /// Does NOT call SaveChangesAsync (caller responsible for transaction management).
    /// Must call SaveChangesAsync separately to persist.
    /// </remarks>
    Task UpdateAsync(Benchmark benchmark, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a benchmark (soft delete - sets IsActive = false).
    /// Benchmark data is preserved in database for audit trail.
    /// </summary>
    /// <param name="id">The unique identifier (GUID) of the benchmark to delete.</param>
    /// <param name="cancellationToken">Cancellation token for async operation.</param>
    /// <returns>True if benchmark was found and deleted, false if not found.</returns>
    /// <remarks>
    /// Performs soft delete by:
    /// 1. Setting IsActive = false (benchmark won't appear in public API)
    /// 2. Calling SaveChangesAsync to persist change
    /// Benchmark is NOT physically removed from database, preserving audit trail.
    /// IMPORTANT: Caller must check for dependent BenchmarkScores before deletion.
    /// </remarks>
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a benchmark has any associated benchmark scores (dependency check).
    /// Used before deletion to prevent orphaning scores.
    /// </summary>
    /// <param name="benchmarkId">The unique identifier (GUID) of the benchmark.</param>
    /// <param name="cancellationToken">Cancellation token for async operation.</param>
    /// <returns>True if benchmark has associated scores, false otherwise.</returns>
    /// <remarks>
    /// Queries BenchmarkScores table to check for rows with matching BenchmarkId.
    /// Returns true if ANY scores exist (prevents deletion).
    /// Use before DeleteAsync to enforce referential integrity at application level.
    /// </remarks>
    Task<bool> HasDependentScoresAsync(Guid benchmarkId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves all pending changes to the database.
    /// Used for persisting entities added via AddAsync or updated via UpdateAsync.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for async operation.</param>
    /// <remarks>
    /// This method commits all tracked changes from the DbContext to the database.
    /// Typically used after AddAsync/UpdateAsync operations to commit transaction.
    /// </remarks>
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
