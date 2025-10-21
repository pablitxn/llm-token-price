using LlmTokenPrice.Application.DTOs;
using LlmTokenPrice.Domain.Entities;
using LlmTokenPrice.Domain.Enums;
using LlmTokenPrice.Domain.Repositories;
using LlmTokenPrice.Domain.Services;

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
/// 5. Managing benchmark scores with normalization and duplicate prevention
/// </remarks>
public class AdminBenchmarkService : IAdminBenchmarkService
{
    private readonly IBenchmarkRepository _benchmarkRepository;
    private readonly IAdminModelRepository _adminModelRepository;
    private readonly BenchmarkNormalizer _benchmarkNormalizer;

    /// <summary>
    /// Initializes a new instance of the AdminBenchmarkService.
    /// </summary>
    /// <param name="benchmarkRepository">The benchmark repository for data access.</param>
    /// <param name="adminModelRepository">The admin model repository for model validation.</param>
    /// <param name="benchmarkNormalizer">The domain service for score normalization.</param>
    public AdminBenchmarkService(
        IBenchmarkRepository benchmarkRepository,
        IAdminModelRepository adminModelRepository,
        BenchmarkNormalizer benchmarkNormalizer)
    {
        _benchmarkRepository = benchmarkRepository ?? throw new ArgumentNullException(nameof(benchmarkRepository));
        _adminModelRepository = adminModelRepository ?? throw new ArgumentNullException(nameof(adminModelRepository));
        _benchmarkNormalizer = benchmarkNormalizer ?? throw new ArgumentNullException(nameof(benchmarkNormalizer));
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

    // ========== Benchmark Score Management Methods ==========

    /// <inheritdoc />
    public async Task<BenchmarkScoreResponseDto> AddScoreAsync(
        Guid modelId,
        CreateBenchmarkScoreDto dto,
        CancellationToken cancellationToken = default)
    {
        // 1. Validate model exists
        var model = await _adminModelRepository.GetByIdAsync(modelId, cancellationToken);
        if (model == null)
        {
            throw new InvalidOperationException($"Model with ID '{modelId}' not found.");
        }

        // 2. Validate benchmark exists
        var benchmark = await _benchmarkRepository.GetByIdAsync(dto.BenchmarkId, cancellationToken);
        if (benchmark == null)
        {
            throw new InvalidOperationException($"Benchmark with ID '{dto.BenchmarkId}' not found.");
        }

        // 3. Check for duplicate score (ModelId + BenchmarkId must be unique)
        var existingScore = await _benchmarkRepository.GetScoreAsync(modelId, dto.BenchmarkId, cancellationToken);
        if (existingScore != null)
        {
            throw new InvalidOperationException(
                $"A score for benchmark '{benchmark.BenchmarkName}' already exists for this model. Use update to modify existing scores.");
        }

        // 4. Calculate normalized score using domain service
        var normalizedScore = _benchmarkNormalizer.Normalize(
            dto.Score,
            benchmark.TypicalRangeMin ?? 0,
            benchmark.TypicalRangeMax ?? 100);

        // 5. Check if score is outside typical range (for warning flag, not blocking)
        var isOutOfRange = !_benchmarkNormalizer.IsWithinTypicalRange(
            dto.Score,
            benchmark.TypicalRangeMin ?? 0,
            benchmark.TypicalRangeMax ?? 100);

        // 6. Create BenchmarkScore entity
        var score = new BenchmarkScore
        {
            Id = Guid.NewGuid(),
            ModelId = modelId,
            BenchmarkId = dto.BenchmarkId,
            Score = dto.Score,
            MaxScore = dto.MaxScore,
            NormalizedScore = normalizedScore,
            TestDate = dto.TestDate ?? DateTime.UtcNow,
            SourceUrl = string.IsNullOrWhiteSpace(dto.SourceUrl) ? null : dto.SourceUrl,
            Verified = dto.Verified,
            Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes
            // CreatedAt set by repository
        };

        // 7. Persist score
        await _benchmarkRepository.AddScoreAsync(score, cancellationToken);
        await _benchmarkRepository.SaveChangesAsync(cancellationToken);

        // 8. TODO: Invalidate cache patterns
        // - cache:model:{modelId}:*
        // - cache:bestvalue:*
        // - cache:qaps:*

        // 9. Return response DTO with denormalized data
        return new BenchmarkScoreResponseDto
        {
            Id = score.Id,
            ModelId = score.ModelId,
            BenchmarkId = score.BenchmarkId,
            BenchmarkName = benchmark.BenchmarkName,
            Category = benchmark.Category.ToString(),
            Score = score.Score,
            MaxScore = score.MaxScore,
            NormalizedScore = score.NormalizedScore!.Value,
            TestDate = score.TestDate,
            SourceUrl = score.SourceUrl,
            Verified = score.Verified,
            Notes = score.Notes,
            CreatedAt = score.CreatedAt,
            IsOutOfRange = isOutOfRange
        };
    }

    /// <inheritdoc />
    public async Task<List<BenchmarkScoreResponseDto>> GetScoresByModelIdAsync(
        Guid modelId,
        CancellationToken cancellationToken = default)
    {
        var scores = await _benchmarkRepository.GetScoresByModelIdAsync(modelId, cancellationToken);

        return scores.Select(score => new BenchmarkScoreResponseDto
        {
            Id = score.Id,
            ModelId = score.ModelId,
            BenchmarkId = score.BenchmarkId,
            BenchmarkName = score.Benchmark!.BenchmarkName,
            Category = score.Benchmark.Category.ToString(),
            Score = score.Score,
            MaxScore = score.MaxScore,
            NormalizedScore = score.NormalizedScore ?? 0,
            TestDate = score.TestDate,
            SourceUrl = score.SourceUrl,
            Verified = score.Verified,
            Notes = score.Notes,
            CreatedAt = score.CreatedAt,
            IsOutOfRange = !_benchmarkNormalizer.IsWithinTypicalRange(
                score.Score,
                score.Benchmark.TypicalRangeMin ?? 0,
                score.Benchmark.TypicalRangeMax ?? 100)
        }).ToList();
    }

    /// <inheritdoc />
    public async Task<BenchmarkScoreResponseDto?> UpdateScoreAsync(
        Guid modelId,
        Guid scoreId,
        CreateBenchmarkScoreDto dto,
        CancellationToken cancellationToken = default)
    {
        // 1. Fetch existing score
        var score = await _benchmarkRepository.GetScoreByIdAsync(scoreId, cancellationToken);
        if (score == null || score.ModelId != modelId)
        {
            return null; // Score not found or doesn't belong to this model
        }

        // 2. Validate benchmark exists (if changed)
        var benchmark = await _benchmarkRepository.GetByIdAsync(dto.BenchmarkId, cancellationToken);
        if (benchmark == null)
        {
            throw new InvalidOperationException($"Benchmark with ID '{dto.BenchmarkId}' not found.");
        }

        // 3. If BenchmarkId changed, check for duplicate
        if (score.BenchmarkId != dto.BenchmarkId)
        {
            var existingScore = await _benchmarkRepository.GetScoreAsync(modelId, dto.BenchmarkId, cancellationToken);
            if (existingScore != null)
            {
                throw new InvalidOperationException(
                    $"A score for benchmark '{benchmark.BenchmarkName}' already exists for this model.");
            }
        }

        // 4. Recalculate normalized score
        var normalizedScore = _benchmarkNormalizer.Normalize(
            dto.Score,
            benchmark.TypicalRangeMin ?? 0,
            benchmark.TypicalRangeMax ?? 100);

        var isOutOfRange = !_benchmarkNormalizer.IsWithinTypicalRange(
            dto.Score,
            benchmark.TypicalRangeMin ?? 0,
            benchmark.TypicalRangeMax ?? 100);

        // 5. Update score fields
        score.BenchmarkId = dto.BenchmarkId;
        score.Score = dto.Score;
        score.MaxScore = dto.MaxScore;
        score.NormalizedScore = normalizedScore;
        score.TestDate = dto.TestDate ?? score.TestDate;
        score.SourceUrl = string.IsNullOrWhiteSpace(dto.SourceUrl) ? null : dto.SourceUrl;
        score.Verified = dto.Verified;
        score.Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes;

        // 6. Save changes
        await _benchmarkRepository.UpdateScoreAsync(score, cancellationToken);
        await _benchmarkRepository.SaveChangesAsync(cancellationToken);

        // 7. TODO: Invalidate cache patterns

        // 8. Return updated score
        return new BenchmarkScoreResponseDto
        {
            Id = score.Id,
            ModelId = score.ModelId,
            BenchmarkId = score.BenchmarkId,
            BenchmarkName = benchmark.BenchmarkName,
            Category = benchmark.Category.ToString(),
            Score = score.Score,
            MaxScore = score.MaxScore,
            NormalizedScore = score.NormalizedScore!.Value,
            TestDate = score.TestDate,
            SourceUrl = score.SourceUrl,
            Verified = score.Verified,
            Notes = score.Notes,
            CreatedAt = score.CreatedAt,
            IsOutOfRange = isOutOfRange
        };
    }

    /// <inheritdoc />
    public async Task<bool> DeleteScoreAsync(
        Guid modelId,
        Guid scoreId,
        CancellationToken cancellationToken = default)
    {
        // 1. Verify score exists and belongs to the model
        var score = await _benchmarkRepository.GetScoreByIdAsync(scoreId, cancellationToken);
        if (score == null || score.ModelId != modelId)
        {
            return false; // Score not found or doesn't belong to this model
        }

        // 2. Delete score (hard delete)
        var deleted = await _benchmarkRepository.DeleteScoreAsync(scoreId, cancellationToken);

        // 3. TODO: Invalidate cache patterns
        // - cache:model:{modelId}:*
        // - cache:bestvalue:*
        // - cache:qaps:*

        return deleted;
    }
}
