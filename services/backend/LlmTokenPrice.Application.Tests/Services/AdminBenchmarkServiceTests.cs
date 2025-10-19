using FluentAssertions;
using LlmTokenPrice.Application.DTOs;
using LlmTokenPrice.Application.Services;
using LlmTokenPrice.Domain.Entities;
using LlmTokenPrice.Domain.Enums;
using LlmTokenPrice.Domain.Repositories;
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
    private readonly AdminBenchmarkService _service;

    public AdminBenchmarkServiceTests()
    {
        _benchmarkRepositoryMock = new Mock<IBenchmarkRepository>();
        _service = new AdminBenchmarkService(_benchmarkRepositoryMock.Object);
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
}
