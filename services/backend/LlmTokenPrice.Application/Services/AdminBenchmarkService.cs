using LlmTokenPrice.Application.DTOs;
using LlmTokenPrice.Domain.Entities;
using LlmTokenPrice.Domain.Enums;
using LlmTokenPrice.Domain.Repositories;

namespace LlmTokenPrice.Application.Services;

/// <summary>
/// Service implementation for admin-specific benchmark management operations.
/// Orchestrates data access, validation, and cache invalidation for benchmark CRUD.
/// </summary>
/// <remarks>
/// Application layer service that implements admin use cases by:
/// 1. Calling BenchmarkRepository to persist/fetch entities
/// 2. Mapping entities to BenchmarkResponseDto for admin API responses
/// 3. Enforcing business rules (unique names, dependency checks)
/// 4. Invalidating Redis cache patterns after mutations
/// </remarks>
public class AdminBenchmarkService : IAdminBenchmarkService
{
    private readonly IBenchmarkRepository _benchmarkRepository;

    /// <summary>
    /// Initializes a new instance of the AdminBenchmarkService.
    /// </summary>
    /// <param name="benchmarkRepository">The benchmark repository for data access.</param>
    public AdminBenchmarkService(IBenchmarkRepository benchmarkRepository)
    {
        _benchmarkRepository = benchmarkRepository ?? throw new ArgumentNullException(nameof(benchmarkRepository));
    }

    /// <inheritdoc />
    public async Task<List<BenchmarkResponseDto>> GetAllBenchmarksAsync(
        bool includeInactive = true,
        string? categoryFilter = null,
        CancellationToken cancellationToken = default)
    {
        var benchmarks = await _benchmarkRepository.GetAllAsync(
            includeInactive,
            categoryFilter,
            cancellationToken);

        return benchmarks.Select(MapToDto).ToList();
    }

    /// <inheritdoc />
    public async Task<BenchmarkResponseDto?> GetBenchmarkByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var benchmark = await _benchmarkRepository.GetByIdAsync(id, cancellationToken);
        return benchmark == null ? null : MapToDto(benchmark);
    }

    /// <inheritdoc />
    public async Task<Guid> CreateBenchmarkAsync(CreateBenchmarkRequest request, CancellationToken cancellationToken = default)
    {
        // 1. Check for duplicate benchmark name (case-insensitive)
        var existingBenchmark = await _benchmarkRepository.GetByNameAsync(
            request.BenchmarkName,
            cancellationToken);

        if (existingBenchmark != null)
        {
            throw new InvalidOperationException(
                $"A benchmark with name '{request.BenchmarkName}' already exists.");
        }

        // 2. Parse and validate enum values
        if (!Enum.TryParse<BenchmarkCategory>(request.Category, out var category))
        {
            throw new InvalidOperationException($"Invalid category: {request.Category}");
        }

        if (!Enum.TryParse<BenchmarkInterpretation>(request.Interpretation, out var interpretation))
        {
            throw new InvalidOperationException($"Invalid interpretation: {request.Interpretation}");
        }

        // 3. Create Benchmark entity from request
        var benchmark = new Benchmark
        {
            Id = Guid.NewGuid(),
            BenchmarkName = request.BenchmarkName,
            FullName = request.FullName,
            Description = request.Description,
            Category = category,
            Interpretation = interpretation,
            TypicalRangeMin = request.TypicalRangeMin,
            TypicalRangeMax = request.TypicalRangeMax,
            WeightInQaps = request.WeightInQaps
            // CreatedAt and IsActive set by repository
        };

        // 4. Persist benchmark
        await _benchmarkRepository.AddAsync(benchmark, cancellationToken);
        await _benchmarkRepository.SaveChangesAsync(cancellationToken);

        // 5. TODO: Invalidate cache patterns (cache:benchmarks:*, cache:qaps:*, cache:bestvalue:*)
        // Requires ICacheRepository.RemoveByPatternAsync() to be implemented

        return benchmark.Id;
    }

    /// <inheritdoc />
    public async Task<BenchmarkResponseDto?> UpdateBenchmarkAsync(
        Guid id,
        UpdateBenchmarkRequest request,
        CancellationToken cancellationToken = default)
    {
        // 1. Fetch existing benchmark
        var benchmark = await _benchmarkRepository.GetByIdAsync(id, cancellationToken);

        if (benchmark == null)
        {
            return null; // Benchmark not found - controller will return 404
        }

        // 2. Parse and validate enum values
        if (!Enum.TryParse<BenchmarkCategory>(request.Category, out var category))
        {
            throw new InvalidOperationException($"Invalid category: {request.Category}");
        }

        if (!Enum.TryParse<BenchmarkInterpretation>(request.Interpretation, out var interpretation))
        {
            throw new InvalidOperationException($"Invalid interpretation: {request.Interpretation}");
        }

        // 3. Update benchmark fields (BenchmarkName is immutable)
        benchmark.FullName = request.FullName;
        benchmark.Description = request.Description;
        benchmark.Category = category;
        benchmark.Interpretation = interpretation;
        benchmark.TypicalRangeMin = request.TypicalRangeMin;
        benchmark.TypicalRangeMax = request.TypicalRangeMax;
        benchmark.WeightInQaps = request.WeightInQaps;

        // 4. Save changes (EF Core change tracking handles UPDATE)
        await _benchmarkRepository.UpdateAsync(benchmark, cancellationToken);
        await _benchmarkRepository.SaveChangesAsync(cancellationToken);

        // 5. TODO: Invalidate cache patterns (especially if WeightInQaps changed)
        // Requires ICacheRepository.RemoveByPatternAsync() to be implemented

        // 6. Return updated benchmark as DTO
        return MapToDto(benchmark);
    }

    /// <inheritdoc />
    public async Task<bool> DeleteBenchmarkAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // 1. Check for dependent BenchmarkScores
        var hasDependentScores = await _benchmarkRepository.HasDependentScoresAsync(id, cancellationToken);

        if (hasDependentScores)
        {
            throw new InvalidOperationException(
                "Cannot delete benchmark with associated scores. Please remove all benchmark scores first.");
        }

        // 2. Perform soft delete (sets IsActive = false)
        var deleted = await _benchmarkRepository.DeleteAsync(id, cancellationToken);

        // 3. TODO: Invalidate cache patterns
        // Requires ICacheRepository.RemoveByPatternAsync() to be implemented

        return deleted;
    }

    /// <summary>
    /// Maps a Benchmark entity to a BenchmarkResponseDto for API response.
    /// </summary>
    /// <param name="benchmark">The benchmark entity from database.</param>
    /// <returns>BenchmarkResponseDto ready for JSON serialization.</returns>
    private static BenchmarkResponseDto MapToDto(Benchmark benchmark)
    {
        return new BenchmarkResponseDto
        {
            Id = benchmark.Id,
            BenchmarkName = benchmark.BenchmarkName,
            FullName = benchmark.FullName ?? string.Empty,
            Description = benchmark.Description,
            Category = benchmark.Category.ToString(),
            Interpretation = benchmark.Interpretation.ToString(),
            TypicalRangeMin = benchmark.TypicalRangeMin ?? 0,
            TypicalRangeMax = benchmark.TypicalRangeMax ?? 100,
            WeightInQaps = benchmark.WeightInQaps,
            CreatedAt = benchmark.CreatedAt,
            IsActive = benchmark.IsActive
        };
    }
}
