using LlmTokenPrice.Application.DTOs;
using LlmTokenPrice.Domain.Entities;
using LlmTokenPrice.Domain.Repositories;

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
/// </remarks>
public class AdminModelService : IAdminModelService
{
    private readonly IAdminModelRepository _adminRepository;

    /// <summary>
    /// Initializes a new instance of the AdminModelService.
    /// </summary>
    /// <param name="adminRepository">The admin model repository for data access.</param>
    public AdminModelService(IAdminModelRepository adminRepository)
    {
        _adminRepository = adminRepository ?? throw new ArgumentNullException(nameof(adminRepository));
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
    public async Task<AdminModelDto?> GetModelByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var model = await _adminRepository.GetByIdAsync(id, cancellationToken);
        return model == null ? null : MapToAdminDto(model);
    }

    /// <inheritdoc />
    public async Task<bool> DeleteModelAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _adminRepository.DeleteModelAsync(id, cancellationToken);
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
            BenchmarkName = benchmarkScore.Benchmark.BenchmarkName,
            Score = benchmarkScore.Score,
            MaxScore = benchmarkScore.MaxScore,
            NormalizedScore = benchmarkScore.NormalizedScore
        };
    }
}
