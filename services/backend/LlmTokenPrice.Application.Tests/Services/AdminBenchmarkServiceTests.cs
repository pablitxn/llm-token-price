using FluentAssertions;
using LlmTokenPrice.Application.DTOs;
using LlmTokenPrice.Application.Services;
using LlmTokenPrice.Domain.Caching;
using LlmTokenPrice.Domain.Entities;
using LlmTokenPrice.Domain.Enums;
using LlmTokenPrice.Domain.Repositories;
using LlmTokenPrice.Domain.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace LlmTokenPrice.Application.Tests.Services;

/// <summary>
/// Unit tests for AdminBenchmarkService
/// Story 2.9 - Task 10.4: Test service layer business logic
/// Priority: P2 (Medium - service testing)
/// </summary>
public class AdminBenchmarkServiceTests
{
    private readonly Mock<IBenchmarkRepository> _benchmarkRepositoryMock;
    private readonly Mock<IAdminModelRepository> _adminModelRepositoryMock;
    private readonly BenchmarkNormalizer _benchmarkNormalizer;
    private readonly Mock<ICacheRepository> _cacheRepositoryMock;
    private readonly Mock<ILogger<AdminBenchmarkService>> _loggerMock;
    private readonly AdminBenchmarkService _service;

    public AdminBenchmarkServiceTests()
    {
        _benchmarkRepositoryMock = new Mock<IBenchmarkRepository>();
        _adminModelRepositoryMock = new Mock<IAdminModelRepository>();
        _benchmarkNormalizer = new BenchmarkNormalizer();
        _cacheRepositoryMock = new Mock<ICacheRepository>();
        _loggerMock = new Mock<ILogger<AdminBenchmarkService>>();

        // Setup cache mock to return 0 invalidated keys by default
        _cacheRepositoryMock.Setup(x => x.RemoveByPatternAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        _service = new AdminBenchmarkService(
            _benchmarkRepositoryMock.Object,
            _adminModelRepositoryMock.Object,
            _benchmarkNormalizer,
            _cacheRepositoryMock.Object,
            _loggerMock.Object);
    }

    #region CreateBenchmarkAsync Tests

    /// <summary>
    /// [P2] Should create benchmark successfully when name is unique
    /// Story 2.9 AC#4: POST /api/admin/benchmarks endpoint creates benchmark definition
    /// </summary>
    [Fact]
    public async Task CreateBenchmarkAsync_WithUniqueNam_ShouldCreateBenchmark()
    {
        // GIVEN: Valid create request with unique benchmark name
        var request = new CreateBenchmarkRequest
        {
            BenchmarkName = "MMLU",
            FullName = "Massive Multitask Language Understanding",
            Description = "Tests reasoning ability",
            Category = "Reasoning",
            Interpretation = "HigherBetter",
            TypicalRangeMin = 0,
            TypicalRangeMax = 100,
            WeightInQaps = 0.30m
        };

        _benchmarkRepositoryMock
            .Setup(r => r.GetByNameAsync("MMLU", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Benchmark?)null); // No duplicate

        _benchmarkRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Benchmark>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _benchmarkRepositoryMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // WHEN: Creating benchmark
        var benchmarkId = await _service.CreateBenchmarkAsync(request);

        // THEN: Benchmark is created successfully
        benchmarkId.Should().NotBeEmpty();
        _benchmarkRepositoryMock.Verify(r => r.AddAsync(It.Is<Benchmark>(b =>
            b.BenchmarkName == "MMLU" &&
            b.FullName == "Massive Multitask Language Understanding" &&
            b.Category == BenchmarkCategory.Reasoning &&
            b.Interpretation == BenchmarkInterpretation.HigherBetter &&
            b.WeightInQaps == 0.30m
        ), It.IsAny<CancellationToken>()), Times.Once);
        _benchmarkRepositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// [P2] Should throw InvalidOperationException when benchmark name already exists
    /// Story 2.9 AC#6: Validation ensures benchmark names are unique
    /// </summary>
    [Fact]
    public async Task CreateBenchmarkAsync_WithDuplicateName_ShouldThrowException()
    {
        // GIVEN: Create request with duplicate benchmark name
        var request = new CreateBenchmarkRequest
        {
            BenchmarkName = "MMLU",
            FullName = "New MMLU",
            Category = "Reasoning",
            Interpretation = "HigherBetter",
            TypicalRangeMin = 0,
            TypicalRangeMax = 100,
            WeightInQaps = 0.30m
        };

        var existingBenchmark = new Benchmark
        {
            Id = Guid.NewGuid(),
            BenchmarkName = "MMLU",
            FullName = "Existing MMLU",
            Category = BenchmarkCategory.Reasoning,
            Interpretation = BenchmarkInterpretation.HigherBetter,
            WeightInQaps = 0.30m,
            IsActive = true
        };

        _benchmarkRepositoryMock
            .Setup(r => r.GetByNameAsync("MMLU", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingBenchmark);

        // WHEN: Attempting to create duplicate benchmark
        // THEN: Should throw InvalidOperationException
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _service.CreateBenchmarkAsync(request)
        );

        exception.Message.Should().Contain("already exists");
        _benchmarkRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Benchmark>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// [P2] Should throw InvalidOperationException for invalid category enum
    /// </summary>
    [Fact]
    public async Task CreateBenchmarkAsync_WithInvalidCategory_ShouldThrowException()
    {
        // GIVEN: Create request with invalid category
        var request = new CreateBenchmarkRequest
        {
            BenchmarkName = "TestBenchmark",
            FullName = "Test",
            Category = "InvalidCategory",
            Interpretation = "HigherBetter",
            TypicalRangeMin = 0,
            TypicalRangeMax = 100,
            WeightInQaps = 0.30m
        };

        _benchmarkRepositoryMock
            .Setup(r => r.GetByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Benchmark?)null);

        // WHEN: Attempting to create benchmark with invalid category
        // THEN: Should throw InvalidOperationException
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _service.CreateBenchmarkAsync(request)
        );

        exception.Message.Should().Contain("Invalid category");
    }

    #endregion

    #region UpdateBenchmarkAsync Tests

    /// <summary>
    /// [P2] Should update benchmark successfully when benchmark exists
    /// Story 2.9 AC#5: Edit functionality for benchmarks
    /// </summary>
    [Fact]
    public async Task UpdateBenchmarkAsync_WithValidId_ShouldUpdateBenchmark()
    {
        // GIVEN: Existing benchmark and valid update request
        var benchmarkId = Guid.NewGuid();
        var existingBenchmark = new Benchmark
        {
            Id = benchmarkId,
            BenchmarkName = "MMLU",
            FullName = "Old Full Name",
            Category = BenchmarkCategory.Reasoning,
            Interpretation = BenchmarkInterpretation.HigherBetter,
            TypicalRangeMin = 0,
            TypicalRangeMax = 100,
            WeightInQaps = 0.20m,
            IsActive = true
        };

        var updateRequest = new UpdateBenchmarkRequest
        {
            FullName = "Updated Full Name",
            Description = "Updated description",
            Category = "Code",
            Interpretation = "LowerBetter",
            TypicalRangeMin = 10,
            TypicalRangeMax = 90,
            WeightInQaps = 0.35m
        };

        _benchmarkRepositoryMock
            .Setup(r => r.GetByIdAsync(benchmarkId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingBenchmark);

        _benchmarkRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Benchmark>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _benchmarkRepositoryMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // WHEN: Updating benchmark
        var result = await _service.UpdateBenchmarkAsync(benchmarkId, updateRequest);

        // THEN: Benchmark is updated successfully
        result.Should().NotBeNull();
        result!.FullName.Should().Be("Updated Full Name");
        result.Category.Should().Be("Code");
        result.Interpretation.Should().Be("LowerBetter");
        result.WeightInQaps.Should().Be(0.35m);

        _benchmarkRepositoryMock.Verify(r => r.UpdateAsync(It.Is<Benchmark>(b =>
            b.Id == benchmarkId &&
            b.BenchmarkName == "MMLU" && // Name should remain unchanged
            b.FullName == "Updated Full Name" &&
            b.Category == BenchmarkCategory.Code &&
            b.WeightInQaps == 0.35m
        ), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// [P2] Should return null when benchmark does not exist
    /// </summary>
    [Fact]
    public async Task UpdateBenchmarkAsync_WithNonExistentId_ShouldReturnNull()
    {
        // GIVEN: Non-existent benchmark ID
        var benchmarkId = Guid.NewGuid();
        var updateRequest = new UpdateBenchmarkRequest
        {
            FullName = "Updated",
            Category = "Reasoning",
            Interpretation = "HigherBetter",
            TypicalRangeMin = 0,
            TypicalRangeMax = 100,
            WeightInQaps = 0.30m
        };

        _benchmarkRepositoryMock
            .Setup(r => r.GetByIdAsync(benchmarkId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Benchmark?)null);

        // WHEN: Attempting to update non-existent benchmark
        var result = await _service.UpdateBenchmarkAsync(benchmarkId, updateRequest);

        // THEN: Should return null
        result.Should().BeNull();
        _benchmarkRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Benchmark>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region DeleteBenchmarkAsync Tests

    /// <summary>
    /// [P2] Should soft-delete benchmark when no dependent scores exist
    /// Story 2.9 AC#5: Delete functionality for benchmarks
    /// </summary>
    [Fact]
    public async Task DeleteBenchmarkAsync_WithNoDependentScores_ShouldDeleteBenchmark()
    {
        // GIVEN: Benchmark with no dependent scores
        var benchmarkId = Guid.NewGuid();

        _benchmarkRepositoryMock
            .Setup(r => r.HasDependentScoresAsync(benchmarkId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _benchmarkRepositoryMock
            .Setup(r => r.DeleteAsync(benchmarkId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // WHEN: Deleting benchmark
        var result = await _service.DeleteBenchmarkAsync(benchmarkId);

        // THEN: Benchmark is soft-deleted successfully
        result.Should().BeTrue();
        _benchmarkRepositoryMock.Verify(r => r.DeleteAsync(benchmarkId, It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// [P2] Should throw InvalidOperationException when benchmark has dependent scores
    /// Story 2.9 - Task 7.4/7.5: Check dependencies before deletion
    /// </summary>
    [Fact]
    public async Task DeleteBenchmarkAsync_WithDependentScores_ShouldThrowException()
    {
        // GIVEN: Benchmark with dependent benchmark scores
        var benchmarkId = Guid.NewGuid();

        _benchmarkRepositoryMock
            .Setup(r => r.HasDependentScoresAsync(benchmarkId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true); // Has dependent scores

        // WHEN: Attempting to delete benchmark with dependencies
        // THEN: Should throw InvalidOperationException
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _service.DeleteBenchmarkAsync(benchmarkId)
        );

        exception.Message.Should().Contain("associated scores");
        _benchmarkRepositoryMock.Verify(r => r.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region GetAllBenchmarksAsync Tests

    /// <summary>
    /// [P2] Should return all benchmarks including inactive when includeInactive is true
    /// </summary>
    [Fact]
    public async Task GetAllBenchmarksAsync_WithIncludeInactive_ShouldReturnAllBenchmarks()
    {
        // GIVEN: Mix of active and inactive benchmarks
        var benchmarks = new List<Benchmark>
        {
            new()
            {
                Id = Guid.NewGuid(),
                BenchmarkName = "MMLU",
                FullName = "MMLU Full",
                Category = BenchmarkCategory.Reasoning,
                Interpretation = BenchmarkInterpretation.HigherBetter,
                WeightInQaps = 0.30m,
                IsActive = true
            },
            new()
            {
                Id = Guid.NewGuid(),
                BenchmarkName = "OldBenchmark",
                FullName = "Deprecated Benchmark",
                Category = BenchmarkCategory.Code,
                Interpretation = BenchmarkInterpretation.HigherBetter,
                WeightInQaps = 0.20m,
                IsActive = false
            }
        };

        _benchmarkRepositoryMock
            .Setup(r => r.GetAllAsync(true, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(benchmarks);

        // WHEN: Getting all benchmarks with includeInactive = true
        var result = await _service.GetAllBenchmarksAsync(includeInactive: true);

        // THEN: Should return both active and inactive benchmarks
        result.Should().HaveCount(2);
        result.Should().Contain(b => b.BenchmarkName == "MMLU" && b.IsActive);
        result.Should().Contain(b => b.BenchmarkName == "OldBenchmark" && !b.IsActive);
    }

    /// <summary>
    /// [P2] Should filter benchmarks by category
    /// </summary>
    [Fact]
    public async Task GetAllBenchmarksAsync_WithCategoryFilter_ShouldReturnFilteredBenchmarks()
    {
        // GIVEN: Benchmarks with different categories
        var benchmarks = new List<Benchmark>
        {
            new()
            {
                Id = Guid.NewGuid(),
                BenchmarkName = "MMLU",
                FullName = "MMLU",
                Category = BenchmarkCategory.Reasoning,
                Interpretation = BenchmarkInterpretation.HigherBetter,
                WeightInQaps = 0.30m,
                IsActive = true
            }
        };

        _benchmarkRepositoryMock
            .Setup(r => r.GetAllAsync(true, "Reasoning", It.IsAny<CancellationToken>()))
            .ReturnsAsync(benchmarks);

        // WHEN: Getting benchmarks filtered by Reasoning category
        var result = await _service.GetAllBenchmarksAsync(includeInactive: true, categoryFilter: "Reasoning");

        // THEN: Should return only Reasoning benchmarks
        result.Should().HaveCount(1);
        result.First().Category.Should().Be("Reasoning");
    }

    #endregion

    #region GetBenchmarkByIdAsync Tests

    /// <summary>
    /// [P2] Should return benchmark DTO when benchmark exists
    /// </summary>
    [Fact]
    public async Task GetBenchmarkByIdAsync_WithValidId_ShouldReturnBenchmark()
    {
        // GIVEN: Existing benchmark
        var benchmarkId = Guid.NewGuid();
        var benchmark = new Benchmark
        {
            Id = benchmarkId,
            BenchmarkName = "MMLU",
            FullName = "Massive Multitask Language Understanding",
            Category = BenchmarkCategory.Reasoning,
            Interpretation = BenchmarkInterpretation.HigherBetter,
            TypicalRangeMin = 0,
            TypicalRangeMax = 100,
            WeightInQaps = 0.30m,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _benchmarkRepositoryMock
            .Setup(r => r.GetByIdAsync(benchmarkId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(benchmark);

        // WHEN: Getting benchmark by ID
        var result = await _service.GetBenchmarkByIdAsync(benchmarkId);

        // THEN: Should return mapped DTO
        result.Should().NotBeNull();
        result!.Id.Should().Be(benchmarkId);
        result.BenchmarkName.Should().Be("MMLU");
        result.FullName.Should().Be("Massive Multitask Language Understanding");
        result.Category.Should().Be("Reasoning");
        result.WeightInQaps.Should().Be(0.30m);
    }

    /// <summary>
    /// [P2] Should return null when benchmark does not exist
    /// </summary>
    [Fact]
    public async Task GetBenchmarkByIdAsync_WithNonExistentId_ShouldReturnNull()
    {
        // GIVEN: Non-existent benchmark ID
        var benchmarkId = Guid.NewGuid();

        _benchmarkRepositoryMock
            .Setup(r => r.GetByIdAsync(benchmarkId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Benchmark?)null);

        // WHEN: Getting non-existent benchmark
        var result = await _service.GetBenchmarkByIdAsync(benchmarkId);

        // THEN: Should return null
        result.Should().BeNull();
    }

    #endregion

    #region AddScoreAsync Tests (Story 2.10)

    /// <summary>
    /// [P0] Should add benchmark score successfully when score is unique
    /// Story 2.10 AC#1: Admins can add benchmark scores to models
    /// </summary>
    [Fact]
    public async Task AddScoreAsync_WithUniqueScore_ShouldAddScoreWithNormalization()
    {
        // GIVEN: Valid score request with no duplicate
        var modelId = Guid.NewGuid();
        var benchmarkId = Guid.NewGuid();
        var model = new Model
        {
            Id = modelId,
            Name = "GPT-4",
            Provider = "OpenAI",
            InputPricePer1M = 30m,
            OutputPricePer1M = 60m,
            IsActive = true
        };
        var benchmark = new Benchmark
        {
            Id = benchmarkId,
            BenchmarkName = "MMLU",
            FullName = "Massive Multitask Language Understanding",
            Category = BenchmarkCategory.Reasoning,
            TypicalRangeMin = 0,
            TypicalRangeMax = 100,
            WeightInQaps = 0.30m,
            IsActive = true
        };

        var request = new CreateBenchmarkScoreDto
        {
            BenchmarkId = benchmarkId,
            Score = 87.5m,
            MaxScore = 100m,
            TestDate = DateTime.UtcNow,
            SourceUrl = "https://example.com/results",
            Verified = true,
            Notes = "Official test results"
        };

        _adminModelRepositoryMock
            .Setup(r => r.GetByIdAsync(modelId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(model);

        _benchmarkRepositoryMock
            .Setup(r => r.GetScoreAsync(modelId, benchmarkId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((BenchmarkScore?)null); // No duplicate

        _benchmarkRepositoryMock
            .Setup(r => r.GetByIdAsync(benchmarkId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(benchmark);

        _benchmarkRepositoryMock
            .Setup(r => r.AddScoreAsync(It.IsAny<BenchmarkScore>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _benchmarkRepositoryMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // WHEN: Adding score
        var scoreId = await _service.AddScoreAsync(modelId, request);

        // THEN: Score is created with correct normalization
        scoreId.Should().NotBe(Guid.Empty);
        _benchmarkRepositoryMock.Verify(r => r.AddScoreAsync(It.Is<BenchmarkScore>(s =>
            s.ModelId == modelId &&
            s.BenchmarkId == benchmarkId &&
            s.Score == 87.5m &&
            s.MaxScore == 100m &&
            s.NormalizedScore == 0.875m && // (87.5 - 0) / (100 - 0) = 0.875
            s.Verified == true &&
            s.Notes == "Official test results"
        ), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// [P0] Should throw InvalidOperationException when duplicate score exists
    /// Story 2.10 AC#3: Validation prevents duplicate scores (one per model-benchmark pair)
    /// </summary>
    [Fact]
    public async Task AddScoreAsync_WithDuplicateScore_ShouldThrowException()
    {
        // GIVEN: Score already exists for this model-benchmark pair
        var modelId = Guid.NewGuid();
        var benchmarkId = Guid.NewGuid();

        var model = new Model
        {
            Id = modelId,
            Name = "TestModel",
            Provider = "TestProvider",
            InputPricePer1M = 10m,
            OutputPricePer1M = 20m,
            IsActive = true
        };

        var benchmark = new Benchmark
        {
            Id = benchmarkId,
            BenchmarkName = "MMLU",
            Category = BenchmarkCategory.Reasoning,
            TypicalRangeMin = 0,
            TypicalRangeMax = 100,
            WeightInQaps = 0.30m,
            IsActive = true
        };

        var existingScore = new BenchmarkScore
        {
            Id = Guid.NewGuid(),
            ModelId = modelId,
            BenchmarkId = benchmarkId,
            Score = 85.0m,
            NormalizedScore = 0.85m
        };

        var request = new CreateBenchmarkScoreDto
        {
            BenchmarkId = benchmarkId,
            Score = 90.0m
        };

        _adminModelRepositoryMock
            .Setup(r => r.GetByIdAsync(modelId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(model);

        _benchmarkRepositoryMock
            .Setup(r => r.GetByIdAsync(benchmarkId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(benchmark);

        _benchmarkRepositoryMock
            .Setup(r => r.GetScoreAsync(modelId, benchmarkId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingScore); // Duplicate exists

        // WHEN: Attempting to add duplicate score
        // THEN: Should throw InvalidOperationException
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _service.AddScoreAsync(modelId, request)
        );

        exception.Message.Should().Contain("already exists");
        _benchmarkRepositoryMock.Verify(r => r.AddScoreAsync(It.IsAny<BenchmarkScore>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// [P1] Should calculate normalized score correctly for different ranges
    /// Story 2.10 AC#2: Server-side normalization using benchmark's typical range
    /// </summary>
    [Theory]
    [InlineData(87.5, 0, 100, 0.875)]  // Typical 0-100 range
    [InlineData(0.85, 0, 1, 0.85)]     // Already normalized (0-1)
    [InlineData(650, 200, 800, 0.75)]  // SAT-style range
    [InlineData(50, 0, 100, 0.5)]      // Mid-range
    public async Task AddScoreAsync_ShouldNormalizeScoreCorrectly(
        decimal score,
        decimal rangeMin,
        decimal rangeMax,
        decimal expectedNormalized)
    {
        // GIVEN: Benchmark with specific range and score to normalize
        var modelId = Guid.NewGuid();
        var benchmarkId = Guid.NewGuid();
        var model = new Model
        {
            Id = modelId,
            Name = "TestModel",
            Provider = "TestProvider",
            InputPricePer1M = 10m,
            OutputPricePer1M = 20m,
            IsActive = true
        };
        var benchmark = new Benchmark
        {
            Id = benchmarkId,
            BenchmarkName = "TestBenchmark",
            Category = BenchmarkCategory.Reasoning,
            TypicalRangeMin = rangeMin,
            TypicalRangeMax = rangeMax,
            WeightInQaps = 0.30m,
            IsActive = true
        };

        var request = new CreateBenchmarkScoreDto
        {
            BenchmarkId = benchmarkId,
            Score = score
        };

        _adminModelRepositoryMock
            .Setup(r => r.GetByIdAsync(modelId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(model);

        _benchmarkRepositoryMock
            .Setup(r => r.GetScoreAsync(modelId, benchmarkId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((BenchmarkScore?)null);

        _benchmarkRepositoryMock
            .Setup(r => r.GetByIdAsync(benchmarkId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(benchmark);

        _benchmarkRepositoryMock
            .Setup(r => r.AddScoreAsync(It.IsAny<BenchmarkScore>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _benchmarkRepositoryMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // WHEN: Adding score
        await _service.AddScoreAsync(modelId, request);

        // THEN: Normalized score is calculated correctly
        _benchmarkRepositoryMock.Verify(r => r.AddScoreAsync(It.Is<BenchmarkScore>(s =>
            s.NormalizedScore == expectedNormalized
        ), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// [P1] Should throw KeyNotFoundException when benchmark does not exist
    /// </summary>
    [Fact]
    public async Task AddScoreAsync_WithNonExistentBenchmark_ShouldThrowException()
    {
        // GIVEN: Request referencing non-existent benchmark
        var modelId = Guid.NewGuid();
        var benchmarkId = Guid.NewGuid();

        var model = new Model
        {
            Id = modelId,
            Name = "TestModel",
            Provider = "TestProvider",
            InputPricePer1M = 10m,
            OutputPricePer1M = 20m,
            IsActive = true
        };

        var request = new CreateBenchmarkScoreDto
        {
            BenchmarkId = benchmarkId,
            Score = 87.5m
        };

        _adminModelRepositoryMock
            .Setup(r => r.GetByIdAsync(modelId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(model);

        _benchmarkRepositoryMock
            .Setup(r => r.GetScoreAsync(modelId, benchmarkId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((BenchmarkScore?)null);

        _benchmarkRepositoryMock
            .Setup(r => r.GetByIdAsync(benchmarkId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Benchmark?)null); // Benchmark doesn't exist

        // WHEN: Attempting to add score with non-existent benchmark
        // THEN: Should throw InvalidOperationException
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _service.AddScoreAsync(modelId, request)
        );

        exception.Message.Should().Contain("Benchmark");
    }

    #endregion

    #region GetScoresByModelIdAsync Tests (Story 2.10)

    /// <summary>
    /// [P1] Should return all scores for a model with denormalized benchmark data
    /// Story 2.10 AC#4: Display list of benchmark scores in model edit page
    /// </summary>
    [Fact]
    public async Task GetScoresByModelIdAsync_ShouldReturnScoresWithBenchmarkData()
    {
        // GIVEN: Model with multiple benchmark scores
        var modelId = Guid.NewGuid();
        var benchmark1 = new Benchmark
        {
            Id = Guid.NewGuid(),
            BenchmarkName = "MMLU",
            FullName = "Massive Multitask Language Understanding",
            Category = BenchmarkCategory.Reasoning,
            TypicalRangeMin = 0,
            TypicalRangeMax = 100,
            IsActive = true
        };

        var benchmark2 = new Benchmark
        {
            Id = Guid.NewGuid(),
            BenchmarkName = "HumanEval",
            FullName = "Human Eval Code Benchmark",
            Category = BenchmarkCategory.Code,
            TypicalRangeMin = 0,
            TypicalRangeMax = 100,
            IsActive = true
        };

        var scores = new List<BenchmarkScore>
        {
            new()
            {
                Id = Guid.NewGuid(),
                ModelId = modelId,
                BenchmarkId = benchmark1.Id,
                Benchmark = benchmark1,
                Score = 87.5m,
                MaxScore = 100m,
                NormalizedScore = 0.875m,
                Verified = true,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                ModelId = modelId,
                BenchmarkId = benchmark2.Id,
                Benchmark = benchmark2,
                Score = 75.0m,
                NormalizedScore = 0.75m,
                Verified = false,
                CreatedAt = DateTime.UtcNow
            }
        };

        _benchmarkRepositoryMock
            .Setup(r => r.GetScoresByModelIdAsync(modelId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(scores);

        // WHEN: Getting scores for model
        var result = await _service.GetScoresByModelIdAsync(modelId);

        // THEN: Should return DTOs with denormalized data
        result.Should().HaveCount(2);
        result.First().BenchmarkName.Should().Be("MMLU");
        result.First().Category.Should().Be("Reasoning");
        result.First().Score.Should().Be(87.5m);
        result.First().NormalizedScore.Should().Be(0.875m);
        result.First().Verified.Should().BeTrue();
    }

    /// <summary>
    /// [P1] Should mark scores as out-of-range when they exceed typical bounds
    /// Story 2.10 AC#5: Display warning when score is outside typical range
    /// </summary>
    [Fact]
    public async Task GetScoresByModelIdAsync_ShouldFlagOutOfRangeScores()
    {
        // GIVEN: Scores both within and outside typical range
        var modelId = Guid.NewGuid();
        var benchmark = new Benchmark
        {
            Id = Guid.NewGuid(),
            BenchmarkName = "MMLU",
            Category = BenchmarkCategory.Reasoning,
            TypicalRangeMin = 60m,
            TypicalRangeMax = 95m,
            IsActive = true
        };

        var scores = new List<BenchmarkScore>
        {
            new()
            {
                Id = Guid.NewGuid(),
                ModelId = modelId,
                BenchmarkId = benchmark.Id,
                Benchmark = benchmark,
                Score = 87.5m, // Within range
                NormalizedScore = 0.875m,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                ModelId = modelId,
                BenchmarkId = benchmark.Id,
                Benchmark = benchmark,
                Score = 45.2m, // Below typical min
                NormalizedScore = 0.452m,
                CreatedAt = DateTime.UtcNow
            }
        };

        _benchmarkRepositoryMock
            .Setup(r => r.GetScoresByModelIdAsync(modelId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(scores);

        // WHEN: Getting scores
        var result = await _service.GetScoresByModelIdAsync(modelId);

        // THEN: Out-of-range scores should be flagged
        result.Should().HaveCount(2);
        result.First().IsOutOfRange.Should().BeFalse(); // 87.5 is within [60, 95]
        result.Last().IsOutOfRange.Should().BeTrue();   // 45.2 is below 60
    }

    #endregion

    #region UpdateScoreAsync Tests (Story 2.10)

    /// <summary>
    /// [P1] Should update score and recalculate normalized value
    /// Story 2.10 AC#6: Admins can edit existing scores
    /// </summary>
    [Fact]
    public async Task UpdateScoreAsync_ShouldUpdateScoreWithRecalculation()
    {
        // GIVEN: Existing score and update request
        var modelId = Guid.NewGuid();
        var scoreId = Guid.NewGuid();
        var benchmarkId = Guid.NewGuid();

        var benchmark = new Benchmark
        {
            Id = benchmarkId,
            BenchmarkName = "MMLU",
            Category = BenchmarkCategory.Reasoning,
            TypicalRangeMin = 0,
            TypicalRangeMax = 100,
            IsActive = true
        };

        var existingScore = new BenchmarkScore
        {
            Id = scoreId,
            ModelId = modelId,
            BenchmarkId = benchmarkId,
            Benchmark = benchmark,
            Score = 85.0m,
            NormalizedScore = 0.85m,
            Verified = false
        };

        var updateRequest = new CreateBenchmarkScoreDto
        {
            BenchmarkId = benchmarkId,
            Score = 92.5m,
            MaxScore = 100m,
            Verified = true,
            Notes = "Updated results"
        };

        _benchmarkRepositoryMock
            .Setup(r => r.GetScoreByIdAsync(scoreId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingScore);

        _benchmarkRepositoryMock
            .Setup(r => r.GetByIdAsync(benchmarkId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(benchmark);

        _benchmarkRepositoryMock
            .Setup(r => r.UpdateScoreAsync(It.IsAny<BenchmarkScore>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _benchmarkRepositoryMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // WHEN: Updating score
        var result = await _service.UpdateScoreAsync(modelId, scoreId, updateRequest);

        // THEN: Score is updated with recalculated normalization
        result.Should().NotBeNull();
        _benchmarkRepositoryMock.Verify(r => r.UpdateScoreAsync(It.Is<BenchmarkScore>(s =>
            s.Id == scoreId &&
            s.Score == 92.5m &&
            s.MaxScore == 100m &&
            s.NormalizedScore == 0.925m && // Recalculated
            s.Verified == true &&
            s.Notes == "Updated results"
        ), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// [P2] Should return null when score does not exist
    /// </summary>
    [Fact]
    public async Task UpdateScoreAsync_WithNonExistentScore_ShouldReturnNull()
    {
        // GIVEN: Non-existent score ID
        var modelId = Guid.NewGuid();
        var scoreId = Guid.NewGuid();
        var updateRequest = new CreateBenchmarkScoreDto
        {
            BenchmarkId = Guid.NewGuid(),
            Score = 90.0m
        };

        _benchmarkRepositoryMock
            .Setup(r => r.GetScoreByIdAsync(scoreId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((BenchmarkScore?)null);

        // WHEN: Attempting to update non-existent score
        var result = await _service.UpdateScoreAsync(modelId, scoreId, updateRequest);

        // THEN: Should return null
        result.Should().BeNull();
        _benchmarkRepositoryMock.Verify(r => r.UpdateScoreAsync(It.IsAny<BenchmarkScore>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region DeleteScoreAsync Tests (Story 2.10)

    /// <summary>
    /// [P1] Should delete score successfully when it exists
    /// Story 2.10 AC#7: Admins can delete benchmark scores
    /// </summary>
    [Fact]
    public async Task DeleteScoreAsync_WithValidId_ShouldDeleteScore()
    {
        // GIVEN: Existing score
        var modelId = Guid.NewGuid();
        var scoreId = Guid.NewGuid();

        var existingScore = new BenchmarkScore
        {
            Id = scoreId,
            ModelId = modelId,
            BenchmarkId = Guid.NewGuid(),
            Score = 85.0m,
            NormalizedScore = 0.85m
        };

        _benchmarkRepositoryMock
            .Setup(r => r.GetScoreByIdAsync(scoreId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingScore);

        _benchmarkRepositoryMock
            .Setup(r => r.DeleteScoreAsync(scoreId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // WHEN: Deleting score
        var result = await _service.DeleteScoreAsync(modelId, scoreId);

        // THEN: Score is deleted successfully
        result.Should().BeTrue();
        _benchmarkRepositoryMock.Verify(r => r.DeleteScoreAsync(scoreId, It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// [P2] Should return false when score does not exist
    /// </summary>
    [Fact]
    public async Task DeleteScoreAsync_WithNonExistentId_ShouldReturnFalse()
    {
        // GIVEN: Non-existent score ID
        var modelId = Guid.NewGuid();
        var scoreId = Guid.NewGuid();

        _benchmarkRepositoryMock
            .Setup(r => r.DeleteScoreAsync(scoreId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // WHEN: Attempting to delete non-existent score
        var result = await _service.DeleteScoreAsync(modelId, scoreId);

        // THEN: Should return false
        result.Should().BeFalse();
    }

    #endregion
}
