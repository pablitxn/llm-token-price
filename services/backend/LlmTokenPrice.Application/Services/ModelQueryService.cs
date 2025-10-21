using LlmTokenPrice.Application.DTOs;
using LlmTokenPrice.Domain.Caching;
using LlmTokenPrice.Domain.Entities;
using LlmTokenPrice.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace LlmTokenPrice.Application.Services;

/// <summary>
/// Service implementation for querying model data.
/// Orchestrates data access and entity-to-DTO mapping for API responses.
/// </summary>
/// <remarks>
/// Application layer service that implements use cases by:
/// 1. Checking cache for existing data (Redis layer)
/// 2. On cache miss: Calling domain repositories to fetch entities
/// 3. Caching result with appropriate TTL
/// 4. Mapping entities to DTOs for API consumption
/// 5. Applying business rules (e.g., top 3 benchmarks only)
///
/// Cache Strategy:
/// - Model list: 1 hour TTL (frequently accessed, changes infrequently)
/// - Model detail: 30 minutes TTL (user-specific, may change more often)
/// - Cache invalidation: Handled by AdminModelService on CRUD operations
/// </remarks>
public class ModelQueryService : IModelQueryService
{
    private readonly IModelRepository _modelRepository;
    private readonly ICacheRepository _cacheRepository;
    private readonly ILogger<ModelQueryService> _logger;

    /// <summary>
    /// Initializes a new instance of the ModelQueryService.
    /// </summary>
    /// <param name="modelRepository">The model repository for data access.</param>
    /// <param name="cacheRepository">The cache repository for Redis operations.</param>
    /// <param name="logger">The logger for tracking cache hits/misses.</param>
    public ModelQueryService(
        IModelRepository modelRepository,
        ICacheRepository cacheRepository,
        ILogger<ModelQueryService> logger)
    {
        _modelRepository = modelRepository ?? throw new ArgumentNullException(nameof(modelRepository));
        _cacheRepository = cacheRepository ?? throw new ArgumentNullException(nameof(cacheRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<List<ModelDto>> GetAllModelsAsync(CancellationToken cancellationToken = default)
    {
        // 1. Try to get from cache
        var cached = await _cacheRepository.GetAsync<List<ModelDto>>(
            CacheConfiguration.ModelListKey,
            cancellationToken);

        if (cached != null)
        {
            _logger.LogDebug("Cache hit for model list ({Count} models)", cached.Count);
            return cached;
        }

        // 2. Cache miss - fetch from database
        _logger.LogDebug("Cache miss for model list - fetching from database");
        var models = await _modelRepository.GetAllAsync(cancellationToken);
        var dtos = models.Select(MapToDto).ToList();

        // 3. Cache result with 1 hour TTL
        await _cacheRepository.SetAsync(
            CacheConfiguration.ModelListKey,
            dtos,
            CacheConfiguration.DefaultTtl.ApiResponses,
            cancellationToken);

        _logger.LogInformation("Cached model list ({Count} models) with TTL {TTL}",
            dtos.Count, CacheConfiguration.DefaultTtl.ApiResponses);

        return dtos;
    }

    /// <inheritdoc />
    public async Task<ModelDto?> GetModelByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // 1. Try to get from cache
        var cacheKey = CacheConfiguration.BuildModelDetailKey(id);
        var cached = await _cacheRepository.GetAsync<ModelDto>(cacheKey, cancellationToken);

        if (cached != null)
        {
            _logger.LogDebug("Cache hit for model detail: {ModelId}", id);
            return cached;
        }

        // 2. Cache miss - fetch from database
        _logger.LogDebug("Cache miss for model detail: {ModelId} - fetching from database", id);
        var model = await _modelRepository.GetByIdAsync(id, cancellationToken);

        if (model == null)
        {
            _logger.LogDebug("Model not found: {ModelId}", id);
            return null;
        }

        var dto = MapToDto(model);

        // 3. Cache result with 30 minutes TTL
        await _cacheRepository.SetAsync(
            cacheKey,
            dto,
            CacheConfiguration.DefaultTtl.ModelDetail,
            cancellationToken);

        _logger.LogInformation("Cached model detail: {ModelId} with TTL {TTL}",
            id, CacheConfiguration.DefaultTtl.ModelDetail);

        return dto;
    }

    /// <summary>
    /// Maps a Model entity to a ModelDto for API response.
    /// Includes nested capability and top 3 benchmark scores.
    /// </summary>
    /// <param name="model">The model entity from database.</param>
    /// <returns>ModelDto ready for JSON serialization.</returns>
    private static ModelDto MapToDto(Model model)
    {
        return new ModelDto
        {
            Id = model.Id,
            Name = model.Name,
            Provider = model.Provider,
            Version = model.Version,
            Status = model.Status,
            InputPricePer1M = model.InputPricePer1M,
            OutputPricePer1M = model.OutputPricePer1M,
            Currency = model.Currency,
            UpdatedAt = model.UpdatedAt,
            PricingUpdatedAt = model.PricingUpdatedAt, // Story 2.12
            Capabilities = model.Capability == null ? null : MapCapabilityToDto(model.Capability),
            TopBenchmarks = model.BenchmarkScores
                .OrderByDescending(bs => bs.Score)
                .Take(3)
                .Select(MapBenchmarkScoreToDto)
                .ToList()
        };
    }

    /// <summary>
    /// Maps a Capability entity to a CapabilityDto.
    /// </summary>
    /// <param name="capability">The capability entity from database.</param>
    /// <returns>CapabilityDto ready for nested inclusion in ModelDto.</returns>
    private static CapabilityDto MapCapabilityToDto(Capability capability)
    {
        return new CapabilityDto
        {
            ContextWindow = capability.ContextWindow,
            MaxOutputTokens = capability.MaxOutputTokens,
            SupportsFunctionCalling = capability.SupportsFunctionCalling,
            SupportsVision = capability.SupportsVision,
            SupportsAudioInput = capability.SupportsAudioInput,
            SupportsAudioOutput = capability.SupportsAudioOutput,
            SupportsStreaming = capability.SupportsStreaming,
            SupportsJsonMode = capability.SupportsJsonMode
        };
    }

    /// <summary>
    /// Maps a BenchmarkScore entity to a BenchmarkScoreDto.
    /// Includes benchmark name from the related Benchmark entity.
    /// </summary>
    /// <param name="benchmarkScore">The benchmark score entity from database.</param>
    /// <returns>BenchmarkScoreDto ready for nested inclusion in ModelDto.</returns>
    private static BenchmarkScoreDto MapBenchmarkScoreToDto(BenchmarkScore benchmarkScore)
    {
        return new BenchmarkScoreDto
        {
            BenchmarkName = benchmarkScore.Benchmark.BenchmarkName,
            Score = benchmarkScore.Score,
            MaxScore = benchmarkScore.MaxScore,
            NormalizedScore = benchmarkScore.NormalizedScore
        };
    }
}
