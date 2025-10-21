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

    // ========== Benchmark Score Management Methods ==========

    /// <summary>
    /// Retrieves a benchmark score for a specific model and benchmark combination.
    /// Used for duplicate detection before adding a new score.
    /// </summary>
    /// <param name="modelId">The unique identifier (GUID) of the model.</param>
    /// <param name="benchmarkId">The unique identifier (GUID) of the benchmark.</param>
    /// <param name="cancellationToken">Cancellation token for async operation.</param>
    /// <returns>BenchmarkScore if found, null otherwise.</returns>
    /// <remarks>
    /// Enforces unique constraint: one score per (ModelId, BenchmarkId) combination.
    /// Returns null if no score exists (safe to create new).
    /// Does not eagerly load navigation properties (Model, Benchmark) for performance.
    /// </remarks>
    Task<BenchmarkScore?> GetScoreAsync(Guid modelId, Guid benchmarkId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new benchmark score to the database.
    /// Sets CreatedAt to DateTime.UtcNow.
    /// </summary>
    /// <param name="score">The benchmark score entity to create.</param>
    /// <param name="cancellationToken">Cancellation token for async operation.</param>
    /// <remarks>
    /// This method:
    /// 1. Sets CreatedAt = DateTime.UtcNow
    /// 2. Adds score to DbContext
    /// 3. Does NOT call SaveChangesAsync (caller responsible for transaction management)
    /// Must call SaveChangesAsync separately to persist.
    /// Unique constraint (ModelId, BenchmarkId) enforced at database level.
    /// </remarks>
    Task AddScoreAsync(BenchmarkScore score, CancellationToken cancellationToken = default);

    /// <summary>
    /// Bulk adds multiple benchmark scores to the database (Story 2.11).
    /// Optimized for CSV import to avoid N individual insert operations.
    /// Sets CreatedAt to DateTime.UtcNow for all scores.
    /// </summary>
    /// <param name="scores">Collection of benchmark score entities to create.</param>
    /// <param name="cancellationToken">Cancellation token for async operation.</param>
    /// <remarks>
    /// This method:
    /// 1. Sets CreatedAt = DateTime.UtcNow for all scores
    /// 2. Adds scores to DbContext in batch using AddRangeAsync
    /// 3. Does NOT call SaveChangesAsync (caller responsible for transaction management)
    /// Must call SaveChangesAsync separately to persist all scores in single transaction.
    /// Unique constraint (ModelId, BenchmarkId) enforced at database level (violators will cause SaveChangesAsync to fail).
    /// Performance: Use this for importing multiple scores to reduce database round-trips.
    /// </remarks>
    Task BulkAddScoresAsync(IEnumerable<BenchmarkScore> scores, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all benchmark scores for a specific model.
    /// Eagerly loads related Benchmark entity for display purposes.
    /// </summary>
    /// <param name="modelId">The unique identifier (GUID) of the model.</param>
    /// <param name="cancellationToken">Cancellation token for async operation.</param>
    /// <returns>List of benchmark scores ordered by Benchmark.BenchmarkName ascending.</returns>
    /// <remarks>
    /// Eagerly loads Benchmark navigation property using Include().
    /// Returns empty list if model has no scores.
    /// Used for displaying scores list in model edit page.
    /// Ordered alphabetically by BenchmarkName for consistent UI display.
    /// </remarks>
    Task<List<BenchmarkScore>> GetScoresByModelIdAsync(Guid modelId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a single benchmark score by its unique identifier.
    /// </summary>
    /// <param name="scoreId">The unique identifier (GUID) of the benchmark score.</param>
    /// <param name="cancellationToken">Cancellation token for async operation.</param>
    /// <returns>BenchmarkScore if found, null otherwise.</returns>
    /// <remarks>
    /// Used for edit/delete operations on individual scores.
    /// Eagerly loads Benchmark navigation property for denormalized display data.
    /// </remarks>
    Task<BenchmarkScore?> GetScoreByIdAsync(Guid scoreId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing benchmark score.
    /// EF Core change tracking handles UPDATE on SaveChangesAsync call.
    /// </summary>
    /// <param name="score">The benchmark score entity to update (must be tracked by DbContext).</param>
    /// <param name="cancellationToken">Cancellation token for async operation.</param>
    /// <remarks>
    /// This method updates the score entity in the DbContext.
    /// Does NOT call SaveChangesAsync (caller responsible for transaction management).
    /// Must call SaveChangesAsync separately to persist.
    /// </remarks>
    Task UpdateScoreAsync(BenchmarkScore score, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a benchmark score from the database (hard delete).
    /// </summary>
    /// <param name="scoreId">The unique identifier (GUID) of the score to delete.</param>
    /// <param name="cancellationToken">Cancellation token for async operation.</param>
    /// <returns>True if score was found and deleted, false if not found.</returns>
    /// <remarks>
    /// Performs HARD delete (physical removal from database).
    /// Unlike models/benchmarks, scores don't use soft delete.
    /// Cascade delete: deleting Model or Benchmark automatically deletes scores.
    /// Calls SaveChangesAsync to persist change.
    /// </remarks>
    Task<bool> DeleteScoreAsync(Guid scoreId, CancellationToken cancellationToken = default);

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

    /// <summary>
    /// Begins a new database transaction scope for all-or-nothing operations.
    /// Story 2.13 Task 6: Enables transactional CSV import (rollback on any error).
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for async operation.</param>
    /// <returns>Transaction scope that must be committed or will auto-rollback on disposal.</returns>
    /// <remarks>
    /// Use this for operations that must be atomic (all succeed or all fail).
    /// Example: CSV import where all rows must be valid before importing any.
    /// IMPORTANT: Caller must await CommitAsync() to persist changes, otherwise transaction rolls back.
    /// Usage pattern:
    /// <code>
    /// await using var transaction = await repository.BeginTransactionAsync();
    /// // ... perform operations ...
    /// await transaction.CommitAsync(); // Commit if all succeeded
    /// </code>
    /// </remarks>
    Task<ITransactionScope> BeginTransactionAsync(CancellationToken cancellationToken = default);
}
