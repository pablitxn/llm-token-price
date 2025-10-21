using LlmTokenPrice.Application.DTOs;
using LlmTokenPrice.Domain.Caching;
using LlmTokenPrice.Domain.Entities;
using LlmTokenPrice.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace LlmTokenPrice.Application.Services;

/// <summary>
/// Service implementation for admin-specific model data operations.
/// Orchestrates data access and entity-to-AdminModelDto mapping for admin API responses.
/// </summary>
/// <remarks>
/// Application layer service that implements admin use cases by:
/// 1. Calling admin repository to fetch entities (including inactive)
/// 2. Mapping entities to AdminModelDto for admin API consumption
/// 3. Applying business rules (e.g., top 3 benchmarks, sort by updated_at DESC)
/// 4. Invalidating caches on CRUD operations (model data + QAPS scores)
/// </remarks>
public class AdminModelService : IAdminModelService
{
    private readonly IAdminModelRepository _adminRepository;
    private readonly ICacheRepository _cacheRepository;
    private readonly ILogger<AdminModelService> _logger;

    /// <summary>
    /// Initializes a new instance of the AdminModelService.
    /// </summary>
    /// <param name="adminRepository">The admin model repository for data access.</param>
    /// <param name="cacheRepository">The cache repository for invalidation.</param>
    /// <param name="logger">The logger for tracking cache operations.</param>
    public AdminModelService(
        IAdminModelRepository adminRepository,
        ICacheRepository cacheRepository,
        ILogger<AdminModelService> logger)
    {
        _adminRepository = adminRepository ?? throw new ArgumentNullException(nameof(adminRepository));
        _cacheRepository = cacheRepository ?? throw new ArgumentNullException(nameof(cacheRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<List<AdminModelDto>> GetAllModelsAsync(
        string? searchTerm = null,
        string? provider = null,
        string? status = null,
        CancellationToken cancellationToken = default)
    {
        var models = await _adminRepository.GetAllModelsAsync(
            searchTerm,
            provider,
            status,
            cancellationToken);

        return models.Select(MapToAdminDto).ToList();
    }

    /// <inheritdoc />
    public async Task<PagedResult<AdminModelDto>> GetAllModelsPagedAsync(
        PaginationParams pagination,
        string? searchTerm = null,
        string? provider = null,
        string? status = null,
        CancellationToken cancellationToken = default)
    {
        // Validate pagination parameters
        if (!pagination.IsValid())
        {
            throw new ArgumentException("Invalid pagination parameters", nameof(pagination));
        }

        // Fetch paginated models from repository
        var (models, totalCount) = await _adminRepository.GetAllModelsPagedAsync(
            pagination.Page,
            pagination.PageSize,
            searchTerm,
            provider,
            status,
            cancellationToken);

        // Map entities to DTOs
        var dtos = models.Select(MapToAdminDto).ToList();

        // Create paged result
        var pagedResult = PagedResult<AdminModelDto>.Create(
            dtos,
            pagination.Page,
            pagination.PageSize,
            totalCount);

        _logger.LogInformation(
            "Retrieved page {Page} size {PageSize} ({Count}/{Total} models) for admin (search: {SearchTerm}, provider: {Provider}, status: {Status})",
            pagination.Page, pagination.PageSize, dtos.Count, totalCount,
            searchTerm ?? "none", provider ?? "none", status ?? "none");

        return pagedResult;
    }

    /// <inheritdoc />
    public async Task<AdminModelDto?> GetModelByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var model = await _adminRepository.GetByIdAsync(id, cancellationToken);
        return model == null ? null : MapToAdminDto(model);
    }

    /// <inheritdoc />
    public async Task<bool> DeleteModelAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var deleted = await _adminRepository.DeleteModelAsync(id, cancellationToken);

        if (deleted)
        {
            // Invalidate model caches and QAPS calculations
            await InvalidateModelCachesAsync(cancellationToken);
            _logger.LogInformation("Deleted model {ModelId} and invalidated caches", id);
        }

        return deleted;
    }

    /// <inheritdoc />
    public async Task<Guid> CreateModelAsync(CreateModelRequest request, CancellationToken cancellationToken = default)
    {
        // 1. Check for duplicate model (case-insensitive name + provider)
        var existingModel = await _adminRepository.GetByNameAndProviderAsync(
            request.Name,
            request.Provider,
            cancellationToken);

        if (existingModel != null)
        {
            throw new InvalidOperationException(
                $"A model with name '{request.Name}' and provider '{request.Provider}' already exists.");
        }

        // 2. Create Model entity from request
        var now = DateTime.UtcNow;
        var model = new Model
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Provider = request.Provider,
            Version = request.Version,
            ReleaseDate = !string.IsNullOrEmpty(request.ReleaseDate)
                ? DateTime.SpecifyKind(DateTime.Parse(request.ReleaseDate), DateTimeKind.Utc)
                : null,
            Status = request.Status,
            InputPricePer1M = request.InputPricePer1M,
            OutputPricePer1M = request.OutputPricePer1M,
            Currency = request.Currency,
            PricingValidFrom = !string.IsNullOrEmpty(request.PricingValidFrom)
                ? DateTime.SpecifyKind(DateTime.Parse(request.PricingValidFrom), DateTimeKind.Utc)
                : null,
            PricingValidTo = !string.IsNullOrEmpty(request.PricingValidTo)
                ? DateTime.SpecifyKind(DateTime.Parse(request.PricingValidTo), DateTimeKind.Utc)
                : null,
            // Story 2.12: Set pricing timestamp on creation
            PricingUpdatedAt = now
            // Other timestamps (CreatedAt, UpdatedAt) and IsActive set by repository
        };

        // 3. Create Capability entity from request (Story 2.6: using DTO values instead of defaults)
        var capability = new Capability
        {
            Id = Guid.NewGuid(),
            ModelId = model.Id,
            ContextWindow = request.Capabilities.ContextWindow,
            MaxOutputTokens = request.Capabilities.MaxOutputTokens,
            SupportsFunctionCalling = request.Capabilities.SupportsFunctionCalling,
            SupportsVision = request.Capabilities.SupportsVision,
            SupportsAudioInput = request.Capabilities.SupportsAudioInput,
            SupportsAudioOutput = request.Capabilities.SupportsAudioOutput,
            SupportsStreaming = request.Capabilities.SupportsStreaming,
            SupportsJsonMode = request.Capabilities.SupportsJsonMode
        };

        // 4. Persist model and capability in single transaction
        var modelId = await _adminRepository.CreateModelAsync(model, cancellationToken);
        await _adminRepository.CreateCapabilityAsync(capability, cancellationToken);

        // 5. Invalidate model caches and QAPS calculations
        await InvalidateModelCachesAsync(cancellationToken);
        _logger.LogInformation("Created model {ModelId} ({Name}) and invalidated caches", modelId, model.Name);

        return modelId;
    }

    /// <inheritdoc />
    public async Task<AdminModelDto?> UpdateModelAsync(Guid id, CreateModelRequest request, CancellationToken cancellationToken = default)
    {
        // 1. Fetch existing model with capabilities
        var model = await _adminRepository.GetByIdAsync(id, cancellationToken);

        if (model == null)
        {
            return null; // Model not found - controller will return 404
        }

        // 2. Check for duplicate name+provider on DIFFERENT models (exclude current model)
        var existingModel = await _adminRepository.GetByNameAndProviderAsync(
            request.Name,
            request.Provider,
            cancellationToken);

        if (existingModel != null && existingModel.Id != id)
        {
            throw new InvalidOperationException(
                $"A model with name '{request.Name}' and provider '{request.Provider}' already exists.");
        }

        // 3. Detect pricing changes for PricingUpdatedAt timestamp (Story 2.12)
        var oldInputPrice = model.InputPricePer1M;
        var oldOutputPrice = model.OutputPricePer1M;
        var pricingChanged = oldInputPrice != request.InputPricePer1M ||
                             oldOutputPrice != request.OutputPricePer1M;

        // 4. Update Model entity fields
        model.Name = request.Name;
        model.Provider = request.Provider;
        model.Version = request.Version;
        model.ReleaseDate = !string.IsNullOrEmpty(request.ReleaseDate)
            ? DateTime.SpecifyKind(DateTime.Parse(request.ReleaseDate), DateTimeKind.Utc)
            : null;
        model.Status = request.Status;
        model.InputPricePer1M = request.InputPricePer1M;
        model.OutputPricePer1M = request.OutputPricePer1M;
        model.Currency = request.Currency;
        model.PricingValidFrom = !string.IsNullOrEmpty(request.PricingValidFrom)
            ? DateTime.SpecifyKind(DateTime.Parse(request.PricingValidFrom), DateTimeKind.Utc)
            : null;
        model.PricingValidTo = !string.IsNullOrEmpty(request.PricingValidTo)
            ? DateTime.SpecifyKind(DateTime.Parse(request.PricingValidTo), DateTimeKind.Utc)
            : null;
        model.UpdatedAt = DateTime.UtcNow;

        // 5. Update PricingUpdatedAt only when pricing fields actually change (Story 2.12)
        if (pricingChanged)
        {
            model.PricingUpdatedAt = DateTime.UtcNow;
        }

        // 6. Update Capability entity fields (if exists)
        if (model.Capability != null)
        {
            model.Capability.ContextWindow = request.Capabilities.ContextWindow;
            model.Capability.MaxOutputTokens = request.Capabilities.MaxOutputTokens;
            model.Capability.SupportsFunctionCalling = request.Capabilities.SupportsFunctionCalling;
            model.Capability.SupportsVision = request.Capabilities.SupportsVision;
            model.Capability.SupportsAudioInput = request.Capabilities.SupportsAudioInput;
            model.Capability.SupportsAudioOutput = request.Capabilities.SupportsAudioOutput;
            model.Capability.SupportsStreaming = request.Capabilities.SupportsStreaming;
            model.Capability.SupportsJsonMode = request.Capabilities.SupportsJsonMode;
        }

        // 7. Save changes (EF Core change tracking handles UPDATE)
        await _adminRepository.SaveChangesAsync(cancellationToken);

        // 8. Invalidate model caches and QAPS calculations
        await InvalidateModelCachesAsync(cancellationToken);
        _logger.LogInformation("Updated model {ModelId} ({Name}) and invalidated caches", id, model.Name);

        // 9. Return updated model as DTO
        return MapToAdminDto(model);
    }

    /// <summary>
    /// Maps a Model entity to an AdminModelDto for admin API response.
    /// Includes all fields including CreatedAt, IsActive, and nested data.
    /// </summary>
    /// <param name="model">The model entity from database.</param>
    /// <returns>AdminModelDto ready for JSON serialization.</returns>
    private static AdminModelDto MapToAdminDto(Model model)
    {
        return new AdminModelDto
        {
            Id = model.Id,
            Name = model.Name,
            Provider = model.Provider,
            Version = model.Version,
            Status = model.Status,
            InputPricePer1M = model.InputPricePer1M,
            OutputPricePer1M = model.OutputPricePer1M,
            Currency = model.Currency,
            IsActive = model.IsActive,
            CreatedAt = model.CreatedAt,
            UpdatedAt = model.UpdatedAt,
            PricingUpdatedAt = model.PricingUpdatedAt, // Story 2.12
            Capabilities = model.Capability == null ? null : MapCapabilityToDto(model.Capability),
            TopBenchmarks = model.BenchmarkScores?
                .OrderByDescending(bs => bs.Score)
                .Take(3)
                .Select(MapBenchmarkScoreToDto)
                .ToList() ?? new List<BenchmarkScoreDto>()
        };
    }

    /// <summary>
    /// Maps a Capability entity to a CapabilityDto.
    /// </summary>
    /// <param name="capability">The capability entity from database.</param>
    /// <returns>CapabilityDto ready for nested inclusion in AdminModelDto.</returns>
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
    /// <returns>BenchmarkScoreDto ready for nested inclusion in AdminModelDto.</returns>
    private static BenchmarkScoreDto MapBenchmarkScoreToDto(BenchmarkScore benchmarkScore)
    {
        return new BenchmarkScoreDto
        {
            BenchmarkName = benchmarkScore.Benchmark?.BenchmarkName ?? "Unknown",
            Score = benchmarkScore.Score,
            MaxScore = benchmarkScore.MaxScore,
            NormalizedScore = benchmarkScore.NormalizedScore
        };
    }

    /// <summary>
    /// Invalidates all model-related caches and QAPS calculations.
    /// Called after CREATE, UPDATE, or DELETE operations on models.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <remarks>
    /// Invalidation strategy:
    /// 1. Remove ALL model caches (list + details) using pattern "llmpricing:model*"
    /// 2. Remove ALL QAPS calculation caches using pattern "llmpricing:qaps:*"
    ///
    /// This is a conservative approach that ensures cache consistency at the cost of cache misses.
    /// Future optimization: Track which specific models changed and invalidate selectively.
    /// </remarks>
    private async Task InvalidateModelCachesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Invalidate all model-related caches (list + details)
            var modelsInvalidated = await _cacheRepository.RemoveByPatternAsync(
                CacheConfiguration.InvalidationPatterns.AllModels,
                cancellationToken);

            // Invalidate all QAPS calculation caches (model changes affect QAPS scores)
            var qapsInvalidated = await _cacheRepository.RemoveByPatternAsync(
                CacheConfiguration.InvalidationPatterns.AllQaps,
                cancellationToken);

            _logger.LogInformation(
                "Cache invalidation complete: {ModelsInvalidated} model keys, {QapsInvalidated} QAPS keys",
                modelsInvalidated, qapsInvalidated);
        }
        catch (Exception ex)
        {
            // Log error but don't throw - cache invalidation failure should not break CRUD operations
            _logger.LogError(ex, "Failed to invalidate model caches - some stale data may persist");
        }
    }
}
