using FluentAssertions;
using LlmTokenPrice.Application.Interfaces;
using LlmTokenPrice.Application.Services;
using LlmTokenPrice.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Moq;

namespace LlmTokenPrice.Application.Tests.Services;

/// <summary>
/// Unit tests for CacheService
/// Tests cache operations: Get, Set, Remove, RemovePattern
/// Story 2.13 Task 4.6: Verify cache service abstraction layer
/// </summary>
public class CacheServiceTests
{
    private readonly Mock<ICacheRepository> _mockCacheRepository;
    private readonly Mock<ILogger<CacheService>> _mockLogger;
    private readonly ICacheService _cacheService;

    public CacheServiceTests()
    {
        _mockCacheRepository = new Mock<ICacheRepository>();
        _mockLogger = new Mock<ILogger<CacheService>>();
        _cacheService = new CacheService(_mockCacheRepository.Object, _mockLogger.Object);
    }

    #region GetAsync Tests

    [Fact]
    public async Task GetAsync_ShouldReturnValue_WhenKeyExists()
    {
        // Arrange
        const string cacheKey = "test:key";
        var expectedValue = new TestDto { Id = 1, Name = "Test" };

        _mockCacheRepository
            .Setup(x => x.GetAsync<TestDto>(cacheKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedValue);

        // Act
        var result = await _cacheService.GetAsync<TestDto>(cacheKey);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedValue);

        _mockCacheRepository.Verify(
            x => x.GetAsync<TestDto>(cacheKey, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetAsync_ShouldReturnNull_WhenKeyDoesNotExist()
    {
        // Arrange
        const string cacheKey = "test:missing";

        _mockCacheRepository
            .Setup(x => x.GetAsync<TestDto>(cacheKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TestDto?)null);

        // Act
        var result = await _cacheService.GetAsync<TestDto>(cacheKey);

        // Assert
        result.Should().BeNull();

        _mockCacheRepository.Verify(
            x => x.GetAsync<TestDto>(cacheKey, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region SetAsync Tests

    [Fact]
    public async Task SetAsync_ShouldCallRepository_WithCorrectParameters()
    {
        // Arrange
        const string cacheKey = "test:key";
        var value = new TestDto { Id = 1, Name = "Test" };
        var ttl = TimeSpan.FromHours(1);

        _mockCacheRepository
            .Setup(x => x.SetAsync(cacheKey, value, ttl, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _cacheService.SetAsync(cacheKey, value, ttl);

        // Assert
        _mockCacheRepository.Verify(
            x => x.SetAsync(cacheKey, value, ttl, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SetAsync_ShouldHandleLongTtl()
    {
        // Arrange
        const string cacheKey = "test:key";
        var value = new TestDto { Id = 1, Name = "Test" };
        var ttl = TimeSpan.FromDays(7);

        _mockCacheRepository
            .Setup(x => x.SetAsync(cacheKey, value, ttl, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _cacheService.SetAsync(cacheKey, value, ttl);

        // Assert
        _mockCacheRepository.Verify(
            x => x.SetAsync(cacheKey, value, ttl, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region RemoveAsync Tests

    [Fact]
    public async Task RemoveAsync_ShouldCallRepository_WithCorrectKey()
    {
        // Arrange
        const string cacheKey = "test:key";

        _mockCacheRepository
            .Setup(x => x.DeleteAsync(cacheKey, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _cacheService.RemoveAsync(cacheKey);

        // Assert
        _mockCacheRepository.Verify(
            x => x.DeleteAsync(cacheKey, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region RemovePatternAsync Tests

    [Fact]
    public async Task RemovePatternAsync_ShouldCallRepository_WithCorrectPattern()
    {
        // Arrange
        const string pattern = "test:*";
        const int deletedCount = 5;

        _mockCacheRepository
            .Setup(x => x.RemoveByPatternAsync(pattern, It.IsAny<CancellationToken>()))
            .ReturnsAsync(deletedCount);

        // Act
        await _cacheService.RemovePatternAsync(pattern);

        // Assert
        _mockCacheRepository.Verify(
            x => x.RemoveByPatternAsync(pattern, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RemovePatternAsync_ShouldHandleWildcardPatterns()
    {
        // Arrange
        const string pattern = "cache:models:*";
        const int deletedCount = 10;

        _mockCacheRepository
            .Setup(x => x.RemoveByPatternAsync(pattern, It.IsAny<CancellationToken>()))
            .ReturnsAsync(deletedCount);

        // Act
        await _cacheService.RemovePatternAsync(pattern);

        // Assert
        _mockCacheRepository.Verify(
            x => x.RemoveByPatternAsync(pattern, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RemovePatternAsync_ShouldHandleZeroMatches()
    {
        // Arrange
        const string pattern = "nonexistent:*";
        const int deletedCount = 0;

        _mockCacheRepository
            .Setup(x => x.RemoveByPatternAsync(pattern, It.IsAny<CancellationToken>()))
            .ReturnsAsync(deletedCount);

        // Act
        await _cacheService.RemovePatternAsync(pattern);

        // Assert
        _mockCacheRepository.Verify(
            x => x.RemoveByPatternAsync(pattern, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region Constructor Tests

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenCacheRepositoryIsNull()
    {
        // Act
        var act = () => new CacheService(null!, _mockLogger.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("cacheRepository");
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenLoggerIsNull()
    {
        // Act
        var act = () => new CacheService(_mockCacheRepository.Object, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    #endregion

    /// <summary>
    /// Test DTO for cache service tests
    /// </summary>
    private class TestDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
