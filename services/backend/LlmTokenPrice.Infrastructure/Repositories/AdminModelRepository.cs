using LlmTokenPrice.Domain.Entities;
using LlmTokenPrice.Domain.Repositories;
using LlmTokenPrice.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LlmTokenPrice.Infrastructure.Repositories;

/// <summary>
/// Admin repository implementation for Model entity using Entity Framework Core.
/// Provides admin-specific data access operations including inactive models and search.
/// </summary>
/// <remarks>
/// This is an "adapter" in Hexagonal Architecture - implements the IAdminModelRepository port.
/// Key differences from public ModelRepository:
/// - Returns ALL models (removes IsActive filter)
/// - Orders by UpdatedAt DESC (most recent first)
/// - Supports search/filter parameters
/// - Uses Include() for eager loading to avoid N+1 query problems.
/// </remarks>
public class AdminModelRepository : IAdminModelRepository
{
    private readonly AppDbContext _context;

    /// <summary>
    /// Initializes a new instance of the AdminModelRepository.
    /// </summary>
    /// <param name="context">The database context for data access.</param>
    public AdminModelRepository(AppDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc />
    public async Task<List<Model>> GetAllModelsAsync(
        string? searchTerm = null,
        string? provider = null,
        string? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Models
            .Include(m => m.Capability)
            .Include(m => m.BenchmarkScores)
                .ThenInclude(bs => bs.Benchmark)
            .AsQueryable();

        // Apply search filter (case-insensitive search by name or provider)
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var lowerSearchTerm = searchTerm.ToLower();
            query = query.Where(m =>
                m.Name.ToLower().Contains(lowerSearchTerm) ||
                m.Provider.ToLower().Contains(lowerSearchTerm));
        }

        // Apply provider filter (exact match, case-insensitive)
        if (!string.IsNullOrWhiteSpace(provider))
        {
            query = query.Where(m => m.Provider.ToLower() == provider.ToLower());
        }

        // Apply status filter (exact match, case-insensitive)
        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(m => m.Status.ToLower() == status.ToLower());
        }

        // Order by UpdatedAt DESC (most recently updated first)
        // Unlike public repository, we do NOT filter by IsActive
        return await query
            .OrderByDescending(m => m.UpdatedAt)
            .AsNoTracking() // Read-only query optimization
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Model?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // Unlike public repository, we do NOT filter by IsActive
        // NOTE: Tracking enabled for update scenarios (Story 2.7)
        return await _context.Models
            .Include(m => m.Capability)
            .Include(m => m.BenchmarkScores)
                .ThenInclude(bs => bs.Benchmark)
            .Where(m => m.Id == id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> DeleteModelAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // Find the model (tracking enabled for updates)
        var model = await _context.Models
            .Where(m => m.Id == id)
            .FirstOrDefaultAsync(cancellationToken);

        if (model == null || !model.IsActive)
        {
            return false; // Model not found or already inactive
        }

        // Perform soft delete
        model.IsActive = false;
        model.UpdatedAt = DateTime.UtcNow;

        // Save changes to database
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }

    /// <inheritdoc />
    public async Task<Guid> CreateModelAsync(Model model, CancellationToken cancellationToken = default)
    {
        // Ensure timestamps are set
        model.CreatedAt = DateTime.UtcNow;
        model.UpdatedAt = DateTime.UtcNow;
        model.IsActive = true;

        // Add model to context
        await _context.Models.AddAsync(model, cancellationToken);

        // Save changes to database
        await _context.SaveChangesAsync(cancellationToken);

        return model.Id;
    }

    /// <inheritdoc />
    public async Task CreateCapabilityAsync(Capability capability, CancellationToken cancellationToken = default)
    {
        // Add capability to context
        await _context.Capabilities.AddAsync(capability, cancellationToken);

        // Save changes to database
        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Model?> GetByNameAndProviderAsync(string name, string provider, CancellationToken cancellationToken = default)
    {
        // Case-insensitive comparison for duplicate detection
        // Returns model even if IsActive = false
        // Does not include related entities for performance
        return await _context.Models
            .Where(m => m.Name.ToLower() == name.ToLower() &&
                       m.Provider.ToLower() == provider.ToLower())
            .AsNoTracking() // Read-only query optimization
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
