using LlmTokenPrice.Domain.Entities;
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
            query = query.Where(b => b.Category.ToString() == categoryFilter);
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

    /// <inheritdoc />
    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
