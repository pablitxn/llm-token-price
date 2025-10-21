using LlmTokenPrice.Domain.Entities;
using LlmTokenPrice.Domain.Repositories;
using LlmTokenPrice.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LlmTokenPrice.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for Model entity using Entity Framework Core.
/// Provides data access operations for models with eager loading of related data.
/// </summary>
/// <remarks>
/// This is an "adapter" in Hexagonal Architecture - implements the IModelRepository port
/// defined in the Domain layer using infrastructure (EF Core).
/// Uses Include() for eager loading to avoid N+1 query problems.
/// </remarks>
public class ModelRepository : IModelRepository
{
    private readonly AppDbContext _context;

    /// <summary>
    /// Initializes a new instance of the ModelRepository.
    /// </summary>
    /// <param name="context">The database context for data access.</param>
    public ModelRepository(AppDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc />
    public async Task<List<Model>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Models
            .Include(m => m.Capability)
            .Include(m => m.BenchmarkScores)
                .ThenInclude(bs => bs.Benchmark)
            .Where(m => m.IsActive)
            .OrderBy(m => m.Provider)
                .ThenBy(m => m.Name)
            .AsNoTracking() // Read-only query optimization
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Model?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Models
            .Include(m => m.Capability)
            .Include(m => m.BenchmarkScores)
                .ThenInclude(bs => bs.Benchmark)
            .Where(m => m.Id == id && m.IsActive)
            .AsNoTracking() // Read-only query optimization
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<(List<Model> Items, int TotalCount)> GetAllPagedAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        // Base query for active models
        var query = _context.Models
            .Where(m => m.IsActive);

        // Get total count (separate query, can be optimized/cached)
        var totalCount = await query.CountAsync(cancellationToken);

        // Get paginated items with eager loading
        var items = await query
            .Include(m => m.Capability)
            .Include(m => m.BenchmarkScores)
                .ThenInclude(bs => bs.Benchmark)
            .OrderBy(m => m.Provider)
                .ThenBy(m => m.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking() // Read-only query optimization
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
}
