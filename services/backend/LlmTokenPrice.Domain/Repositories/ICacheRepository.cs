namespace LlmTokenPrice.Domain.Repositories;

/// <summary>
/// Port for caching infrastructure. Abstracts cache implementation (Redis, MemoryCache, etc.)
/// following Hexagonal Architecture pattern.
/// </summary>
/// <remarks>
/// This interface defines the contract for caching operations without coupling the domain
/// to any specific cache technology. Infrastructure layer provides concrete implementations.
/// Supports multi-layer caching strategy: Client (TanStack Query) → Redis → PostgreSQL.
/// </remarks>
public interface ICacheRepository
{
    /// <summary>
    /// Retrieves a cached value by key and deserializes it to the specified type.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the cached value to</typeparam>
    /// <param name="key">The cache key</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The cached value if found, otherwise null/default</returns>
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Stores a value in the cache with an optional expiration time.
    /// </summary>
    /// <typeparam name="T">The type of value to cache</typeparam>
    /// <param name="key">The cache key</param>
    /// <param name="value">The value to cache</param>
    /// <param name="expiry">Optional expiration time. If null, uses cache default (typically 1 hour)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a cached value by key.
    /// </summary>
    /// <param name="key">The cache key to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task DeleteAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a key exists in the cache.
    /// </summary>
    /// <param name="key">The cache key to check</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if key exists, false otherwise</returns>
    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);
}
