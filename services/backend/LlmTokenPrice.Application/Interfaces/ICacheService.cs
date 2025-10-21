namespace LlmTokenPrice.Application.Interfaces;

/// <summary>
/// Cache abstraction for Redis integration.
/// Supports get/set/remove operations with TTL and pattern-based invalidation.
/// Part of Hexagonal Architecture - defines port for caching adapter.
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Retrieves a cached value by key.
    /// </summary>
    /// <typeparam name="T">Type of cached object</typeparam>
    /// <param name="key">Cache key</param>
    /// <returns>Cached value if found, null otherwise</returns>
    Task<T?> GetAsync<T>(string key) where T : class;

    /// <summary>
    /// Stores a value in cache with specified TTL.
    /// </summary>
    /// <typeparam name="T">Type of object to cache</typeparam>
    /// <param name="key">Cache key</param>
    /// <param name="value">Value to cache</param>
    /// <param name="ttl">Time-to-live duration</param>
    Task SetAsync<T>(string key, T value, TimeSpan ttl) where T : class;

    /// <summary>
    /// Removes a single cache entry by key.
    /// </summary>
    /// <param name="key">Cache key to remove</param>
    Task RemoveAsync(string key);

    /// <summary>
    /// Removes all cache entries matching a pattern (e.g., "cache:models:*").
    /// Used for cache invalidation on admin CRUD operations.
    /// </summary>
    /// <param name="pattern">Pattern to match keys (supports wildcards)</param>
    Task RemovePatternAsync(string pattern);
}
