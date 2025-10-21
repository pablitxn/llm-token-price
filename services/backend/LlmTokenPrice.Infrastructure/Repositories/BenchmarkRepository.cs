using LlmTokenPrice.Domain.Entities;
using LlmTokenPrice.Domain.Enums;
using LlmTokenPrice.Domain.Repositories;
using LlmTokenPrice.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LlmTokenPrice.Infrastructure.Repositories;

/// <summary>
/// Benchmark repository implementation using Entity Framework Core.
/// Provides full CRUD operations for benchmark definition management.
/// </summary>
/// <remarks>
/// This is an "adapter" in Hexagonal Architecture - implements the IBenchmarkRepository port.
/// Key operations:
/// - GetAllAsync: Returns all benchmarks (admin use, includes inactive by default)
/// - GetByNameAsync: Case-insensitive lookup using ILIKE for PostgreSQL
/// - AddAsync/UpdateAsync/DeleteAsync: Full CRUD with soft-delete support
/// - HasDependentScoresAsync: Dependency check before deletion
/// </remarks>
public class BenchmarkRepository : IBenchmarkRepository
{
    private readonly AppDbContext _context;

    /// <summary>
    /// Initializes a new instance of the BenchmarkRepository.
    /// </summary>
    /// <param name="context">The database context for data access.</param>
    public BenchmarkRepository(AppDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc />
    public async Task<List<Benchmark>> GetAllAsync(
        bool includeInactive = true,
        string? categoryFilter = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Benchmarks.AsQueryable();

        // Filter by IsActive if requested
        if (!includeInactive)
        {
            query = query.Where(b => b.IsActive);
        }

        // Apply category filter if provided
        if (!string.IsNullOrWhiteSpace(categoryFilter))
        {
            // Parse string to enum BEFORE query (EF Core can't translate .ToString())
            if (Enum.TryParse<BenchmarkCategory>(categoryFilter, ignoreCase: true, out var categoryEnum))
            {
                query = query.Where(b => b.Category == categoryEnum);
            }
        }

        // Order alphabetically by BenchmarkName
        return await query
            .OrderBy(b => b.BenchmarkName)
            .AsNoTracking() // Read-only query optimization
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Benchmark?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // Tracking enabled for update scenarios
        // Returns benchmark even if IsActive = false
        return await _context.Benchmarks
            .Where(b => b.Id == id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Benchmark?> GetByNameAsync(string benchmarkName, CancellationToken cancellationToken = default)
    {
        // Case-insensitive comparison using EF.Functions.ILike (PostgreSQL ILIKE operator)
        // Returns benchmark even if IsActive = false (duplicates not allowed regardless of status)
        return await _context.Benchmarks
            .Where(b => EF.Functions.ILike(b.BenchmarkName, benchmarkName))
            .AsNoTracking() // Read-only query optimization
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddAsync(Benchmark benchmark, CancellationToken cancellationToken = default)
    {
        // Ensure timestamps and defaults are set
        benchmark.CreatedAt = DateTime.UtcNow;
        benchmark.IsActive = true;

        // Add benchmark to context
        await _context.Benchmarks.AddAsync(benchmark, cancellationToken);

        // Note: Caller responsible for calling SaveChangesAsync
    }

    /// <inheritdoc />
    public async Task UpdateAsync(Benchmark benchmark, CancellationToken cancellationToken = default)
    {
        // EF Core change tracking handles UPDATE on SaveChangesAsync call
        // Benchmark entity must be tracked by DbContext
        _context.Benchmarks.Update(benchmark);

        // Note: Caller responsible for calling SaveChangesAsync
        await Task.CompletedTask; // Satisfy async signature
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // Find the benchmark (tracking enabled for updates)
        var benchmark = await _context.Benchmarks
            .Where(b => b.Id == id)
            .FirstOrDefaultAsync(cancellationToken);

        if (benchmark == null || !benchmark.IsActive)
        {
            return false; // Benchmark not found or already inactive
        }

        // Perform soft delete
        benchmark.IsActive = false;

        // Save changes to database
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }

    /// <inheritdoc />
    public async Task<bool> HasDependentScoresAsync(Guid benchmarkId, CancellationToken cancellationToken = default)
    {
        // Check if any BenchmarkScores reference this benchmark
        return await _context.BenchmarkScores
            .AnyAsync(bs => bs.BenchmarkId == benchmarkId, cancellationToken);
    }

    // ========== Benchmark Score Management Methods ==========

    /// <inheritdoc />
    public async Task<BenchmarkScore?> GetScoreAsync(Guid modelId, Guid benchmarkId, CancellationToken cancellationToken = default)
    {
        // Find existing score for this model+benchmark combination
        // Used for duplicate detection before adding new score
        return await _context.BenchmarkScores
            .Where(bs => bs.ModelId == modelId && bs.BenchmarkId == benchmarkId)
            .AsNoTracking() // Read-only query optimization
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddScoreAsync(BenchmarkScore score, CancellationToken cancellationToken = default)
    {
        // Ensure CreatedAt timestamp is set
        score.CreatedAt = DateTime.UtcNow;

        // Add score to context
        await _context.BenchmarkScores.AddAsync(score, cancellationToken);

        // Note: Caller responsible for calling SaveChangesAsync
    }

    /// <inheritdoc />
    public async Task<List<BenchmarkScore>> GetScoresByModelIdAsync(Guid modelId, CancellationToken cancellationToken = default)
    {
        // Retrieve all scores for a specific model
        // Eagerly load Benchmark for denormalized display data
        return await _context.BenchmarkScores
            .Where(bs => bs.ModelId == modelId)
            .Include(bs => bs.Benchmark) // Eager load for display
            .OrderBy(bs => bs.Benchmark!.BenchmarkName) // Order alphabetically by benchmark name
            .AsNoTracking() // Read-only query optimization
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<BenchmarkScore?> GetScoreByIdAsync(Guid scoreId, CancellationToken cancellationToken = default)
    {
        // Retrieve a single score by ID for edit/delete operations
        // Tracking enabled for update scenarios
        return await _context.BenchmarkScores
            .Include(bs => bs.Benchmark) // Eager load for denormalized display
            .Where(bs => bs.Id == scoreId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task UpdateScoreAsync(BenchmarkScore score, CancellationToken cancellationToken = default)
    {
        // EF Core change tracking handles UPDATE on SaveChangesAsync call
        // Score entity must be tracked by DbContext
        _context.BenchmarkScores.Update(score);

        // Note: Caller responsible for calling SaveChangesAsync
        await Task.CompletedTask; // Satisfy async signature
    }

    /// <inheritdoc />
    public async Task<bool> DeleteScoreAsync(Guid scoreId, CancellationToken cancellationToken = default)
    {
        // Find the score (tracking enabled for delete)
        var score = await _context.BenchmarkScores
            .Where(bs => bs.Id == scoreId)
            .FirstOrDefaultAsync(cancellationToken);

        if (score == null)
        {
            return false; // Score not found
        }

        // Perform HARD delete (physical removal)
        // Unlike models/benchmarks, scores don't use soft delete
        _context.BenchmarkScores.Remove(score);

        // Save changes to database
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }

    /// <inheritdoc />
    public async Task BulkAddScoresAsync(IEnumerable<BenchmarkScore> scores, CancellationToken cancellationToken = default)
    {
        // Set CreatedAt for all scores
        var now = DateTime.UtcNow;
        foreach (var score in scores)
        {
            score.CreatedAt = now;
        }

        // Bulk add using AddRangeAsync for performance (single database round-trip)
        await _context.BenchmarkScores.AddRangeAsync(scores, cancellationToken);

        // Note: Caller responsible for calling SaveChangesAsync to persist all scores in single transaction
        // If SaveChangesAsync fails (e.g., unique constraint violation), entire batch is rolled back
    }

    /// <inheritdoc />
    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ITransactionScope> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        // Begin EF Core database transaction
        var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        // Wrap in ITransactionScope adapter (Hexagonal Architecture pattern)
        return new TransactionScope(transaction);
    }
}
