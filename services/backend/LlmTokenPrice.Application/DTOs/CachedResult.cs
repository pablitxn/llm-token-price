namespace LlmTokenPrice.Application.DTOs;

/// <summary>
/// Wrapper for service responses that tracks whether data came from cache.
/// Used to populate ApiResponseMeta.Cached field accurately (Story 2.13, Task 4).
/// </summary>
/// <typeparam name="T">Type of wrapped data.</typeparam>
public class CachedResult<T>
{
    /// <summary>
    /// The actual data returned from the service.
    /// </summary>
    public required T Data { get; init; }

    /// <summary>
    /// Indicates whether the data was retrieved from cache (true) or database (false).
    /// </summary>
    public required bool FromCache { get; init; }

    /// <summary>
    /// Creates a cache hit result.
    /// </summary>
    public static CachedResult<T> FromCacheHit(T data) => new() { Data = data, FromCache = true };

    /// <summary>
    /// Creates a cache miss result (data from database).
    /// </summary>
    public static CachedResult<T> FromCacheMiss(T data) => new() { Data = data, FromCache = false };
}
