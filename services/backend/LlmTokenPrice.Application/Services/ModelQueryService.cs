using LlmTokenPrice.Application.DTOs;
using LlmTokenPrice.Domain.Entities;
using LlmTokenPrice.Domain.Repositories;

namespace LlmTokenPrice.Application.Services;

/// <summary>
/// Service implementation for querying model data.
/// Orchestrates data access and entity-to-DTO mapping for API responses.
/// </summary>
/// <remarks>
/// Application layer service that implements use cases by:
/// 1. Calling domain repositories to fetch entities
/// 2. Mapping entities to DTOs for API consumption
/// 3. Applying business rules (e.g., top 3 benchmarks only)
/// </remarks>
public class ModelQueryService : IModelQueryService
{
    private readonly IModelRepository _modelRepository;

    /// <summary>
    /// Initializes a new instance of the ModelQueryService.
    /// </summary>
    /// <param name="modelRepository">The model repository for data access.</param>
    public ModelQueryService(IModelRepository modelRepository)
    {
        _modelRepository = modelRepository ?? throw new ArgumentNullException(nameof(modelRepository));
    }

    /// <inheritdoc />
    public async Task<List<ModelDto>> GetAllModelsAsync(CancellationToken cancellationToken = default)
    {
        var models = await _modelRepository.GetAllAsync(cancellationToken);
        return models.Select(MapToDto).ToList();
    }

    /// <inheritdoc />
    public async Task<ModelDto?> GetModelByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var model = await _modelRepository.GetByIdAsync(id, cancellationToken);
        return model == null ? null : MapToDto(model);
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
