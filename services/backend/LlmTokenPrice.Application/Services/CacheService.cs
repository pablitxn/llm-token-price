using LlmTokenPrice.Application.Interfaces;
using LlmTokenPrice.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace LlmTokenPrice.Application.Services;

/// <summary>
/// Application-layer cache service implementing ICacheService.
/// Delegates to ICacheRepository (Domain port) while providing Application-specific interface.
/// </summary>
/// <remarks>
/// This service provides a clean Application-layer abstraction over the Domain's ICacheRepository,
/// maintaining proper layer boundaries in hexagonal architecture.
/// Used by controllers and application services for caching operations.
/// </remarks>
public class CacheService : ICacheService
{
    private readonly ICacheRepository _cacheRepository;
    private readonly ILogger<CacheService> _logger;

    public CacheService(
        ICacheRepository cacheRepository,
        ILogger<CacheService> logger)
    {
        _cacheRepository = cacheRepository ?? throw new ArgumentNullException(nameof(cacheRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<T?> GetAsync<T>(string key) where T : class
    {
        _logger.LogDebug("CacheService: Getting key {Key}", key);
        return await _cacheRepository.GetAsync<T>(key);
    }

    /// <inheritdoc />
    public async Task SetAsync<T>(string key, T value, TimeSpan ttl) where T : class
    {
        _logger.LogDebug("CacheService: Setting key {Key} with TTL {TTL}", key, ttl);
        await _cacheRepository.SetAsync(key, value, ttl);
    }

    /// <inheritdoc />
    public async Task RemoveAsync(string key)
    {
        _logger.LogDebug("CacheService: Removing key {Key}", key);
        await _cacheRepository.DeleteAsync(key);
    }

    /// <inheritdoc />
    public async Task RemovePatternAsync(string pattern)
    {
        _logger.LogDebug("CacheService: Removing pattern {Pattern}", pattern);
        var deletedCount = await _cacheRepository.RemoveByPatternAsync(pattern);
        _logger.LogInformation("CacheService: Removed {Count} keys matching pattern {Pattern}", deletedCount, pattern);
    }
}
