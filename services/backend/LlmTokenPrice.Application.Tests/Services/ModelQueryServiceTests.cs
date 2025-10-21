using FluentAssertions;
using LlmTokenPrice.Application.DTOs;
using LlmTokenPrice.Application.Services;
using LlmTokenPrice.Domain.Entities;
using LlmTokenPrice.Domain.Enums;
using LlmTokenPrice.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Moq;

namespace LlmTokenPrice.Application.Tests.Services;

/// <summary>
/// Unit tests for ModelQueryService to verify entity-to-DTO mapping logic.
/// Tests edge cases: null capabilities, empty benchmarks, top 3 ordering.
/// </summary>
public class ModelQueryServiceTests
{
    private readonly Mock<IModelRepository> _mockRepository;
    private readonly Mock<ICacheRepository> _mockCacheRepository;
    private readonly Mock<ILogger<ModelQueryService>> _mockLogger;
    private readonly ModelQueryService _service;

    public ModelQueryServiceTests()
    {
        _mockRepository = new Mock<IModelRepository>();
        _mockCacheRepository = new Mock<ICacheRepository>();
        _mockLogger = new Mock<ILogger<ModelQueryService>>();

        // Setup cache to always return null (cache miss) for unit tests
        _mockCacheRepository
            .Setup(c => c.GetAsync<List<ModelDto>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((List<ModelDto>?)null);

        _mockCacheRepository
            .Setup(c => c.GetAsync<ModelDto>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ModelDto?)null);

        _service = new ModelQueryService(_mockRepository.Object, _mockCacheRepository.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetAllModelsAsync_WithNullCapability_ReturnsNullCapabilityDto()
    {
        // Arrange
        var model = CreateTestModel(id: Guid.NewGuid(), name: "Test Model", provider: "TestProvider");
        model.Capability = null; // Edge case: model without capabilities

        _mockRepository
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Model> { model });

        // Act
        var result = await _service.GetAllModelsAsync();

        // Assert
        result.Should().HaveCount(1);
        result[0].Capabilities.Should().BeNull("model has no capability data");
    }

    [Fact]
    public async Task GetAllModelsAsync_WithZeroBenchmarks_ReturnsEmptyTopBenchmarks()
    {
        // Arrange
        var model = CreateTestModel(id: Guid.NewGuid(), name: "Test Model", provider: "TestProvider");
        model.BenchmarkScores = new List<BenchmarkScore>(); // Edge case: no benchmarks

        _mockRepository
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Model> { model });

        // Act
        var result = await _service.GetAllModelsAsync();

        // Assert
        result.Should().HaveCount(1);
        result[0].TopBenchmarks.Should().BeEmpty("model has no benchmark scores");
    }

    [Fact]
    public async Task GetAllModelsAsync_WithFiveBenchmarks_ReturnsOnlyTop3OrderedByScoreDesc()
    {
        // Arrange
        var model = CreateTestModel(id: Guid.NewGuid(), name: "Test Model", provider: "TestProvider");

        // Create 5 benchmark scores with different values
        var benchmark1 = CreateBenchmark("MMLU");
        var benchmark2 = CreateBenchmark("HumanEval");
        var benchmark3 = CreateBenchmark("GSM8K");
        var benchmark4 = CreateBenchmark("HellaSwag");
        var benchmark5 = CreateBenchmark("TruthfulQA");

        model.BenchmarkScores = new List<BenchmarkScore>
        {
            CreateBenchmarkScore(model, benchmark1, score: 75.0m), // 4th highest
            CreateBenchmarkScore(model, benchmark2, score: 90.5m), // 1st highest
            CreateBenchmarkScore(model, benchmark3, score: 85.3m), // 2nd highest
            CreateBenchmarkScore(model, benchmark4, score: 70.2m), // 5th highest
            CreateBenchmarkScore(model, benchmark5, score: 80.1m)  // 3rd highest
        };

        _mockRepository
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Model> { model });

        // Act
        var result = await _service.GetAllModelsAsync();

        // Assert
        result.Should().HaveCount(1);
        result[0].TopBenchmarks.Should().HaveCount(3, "only top 3 benchmarks should be returned");

        // Verify ordering: highest score first
        result[0].TopBenchmarks[0].BenchmarkName.Should().Be("HumanEval", "90.5 is the highest score");
        result[0].TopBenchmarks[0].Score.Should().Be(90.5m);

        result[0].TopBenchmarks[1].BenchmarkName.Should().Be("GSM8K", "85.3 is the second highest");
        result[0].TopBenchmarks[1].Score.Should().Be(85.3m);

        result[0].TopBenchmarks[2].BenchmarkName.Should().Be("TruthfulQA", "80.1 is the third highest");
        result[0].TopBenchmarks[2].Score.Should().Be(80.1m);
    }

    [Fact]
    public async Task GetAllModelsAsync_WithValidModel_MapsAllFieldsCorrectly()
    {
        // Arrange
        var modelId = Guid.NewGuid();
        var updatedAt = DateTime.UtcNow;
        var model = CreateTestModel(
            id: modelId,
            name: "GPT-4",
            provider: "OpenAI",
            version: "0613",
            status: "active",
            inputPrice: 30.0m,
            outputPrice: 60.0m,
            currency: "USD",
            updatedAt: updatedAt
        );

        _mockRepository
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Model> { model });

        // Act
        var result = await _service.GetAllModelsAsync();

        // Assert
        result.Should().HaveCount(1);
        var dto = result[0];

        dto.Id.Should().Be(modelId);
        dto.Name.Should().Be("GPT-4");
        dto.Provider.Should().Be("OpenAI");
        dto.Version.Should().Be("0613");
        dto.Status.Should().Be("active");
        dto.InputPricePer1M.Should().Be(30.0m);
        dto.OutputPricePer1M.Should().Be(60.0m);
        dto.Currency.Should().Be("USD");
        dto.UpdatedAt.Should().Be(updatedAt);
        dto.Capabilities.Should().NotBeNull();
        dto.TopBenchmarks.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetModelByIdAsync_WhenModelNotFound_ReturnsNull()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        _mockRepository
            .Setup(r => r.GetByIdAsync(nonExistentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Model?)null);

        // Act
        var result = await _service.GetModelByIdAsync(nonExistentId);

        // Assert
        result.Should().BeNull("repository returned null for non-existent model");
    }

    [Fact]
    public async Task GetModelByIdAsync_WhenModelExists_ReturnsMappedDto()
    {
        // Arrange
        var modelId = Guid.NewGuid();
        var model = CreateTestModel(id: modelId, name: "Claude 3 Opus", provider: "Anthropic");

        _mockRepository
            .Setup(r => r.GetByIdAsync(modelId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(model);

        // Act
        var result = await _service.GetModelByIdAsync(modelId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(modelId);
        result.Name.Should().Be("Claude 3 Opus");
        result.Provider.Should().Be("Anthropic");
    }

    // Helper methods for creating test entities

    private static Model CreateTestModel(
        Guid id,
        string name,
        string provider,
        string? version = null,
        string status = "active",
        decimal inputPrice = 10.0m,
        decimal outputPrice = 20.0m,
        string currency = "USD",
        DateTime? updatedAt = null)
    {
        return new Model
        {
            Id = id,
            Name = name,
            Provider = provider,
            Version = version,
            Status = status,
            InputPricePer1M = inputPrice,
            OutputPricePer1M = outputPrice,
            Currency = currency,
            UpdatedAt = updatedAt ?? DateTime.UtcNow,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            Capability = new Capability
            {
                Id = Guid.NewGuid(),
                ModelId = id,
                ContextWindow = 8192,
                MaxOutputTokens = 4096,
                SupportsFunctionCalling = true,
                SupportsVision = false,
                SupportsAudioInput = false,
                SupportsAudioOutput = false,
                SupportsStreaming = true,
                SupportsJsonMode = true
            },
            BenchmarkScores = new List<BenchmarkScore>
            {
                CreateBenchmarkScore(
                    new Model { Id = id },
                    CreateBenchmark("MMLU"),
                    score: 86.4m
                )
            }
        };
    }

    private static Benchmark CreateBenchmark(string name)
    {
        return new Benchmark
        {
            Id = Guid.NewGuid(),
            BenchmarkName = name,
            FullName = $"{name} Full Name",
            Category = BenchmarkCategory.Reasoning,
            Description = $"{name} benchmark description",
            TypicalRangeMin = 0.0m,
            TypicalRangeMax = 100.0m,
            Interpretation = BenchmarkInterpretation.HigherBetter,
            WeightInQaps = 0.30m,
            CreatedAt = DateTime.UtcNow
        };
    }

    private static BenchmarkScore CreateBenchmarkScore(Model model, Benchmark benchmark, decimal score)
    {
        return new BenchmarkScore
        {
            Id = Guid.NewGuid(),
            ModelId = model.Id,
            BenchmarkId = benchmark.Id,
            Score = score,
            MaxScore = 100.0m,
            NormalizedScore = score / 100.0m,
            Verified = true,
            TestDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            Benchmark = benchmark
        };
    }
}
