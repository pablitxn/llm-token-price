using System.Text.Json;
using LlmTokenPrice.Domain.Repositories;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace LlmTokenPrice.Infrastructure.Caching;

/// <summary>
/// Redis implementation of ICacheRepository using StackExchange.Redis.
/// </summary>
/// <remarks>
/// Implements the cache port defined in Domain layer with Redis-specific logic.
/// Uses System.Text.Json for serialization (consistent with .NET 9 defaults).
/// Handles connection failures gracefully to support optional caching (graceful degradation).
/// </remarks>
public class RedisCacheRepository : ICacheRepository
{
    private readonly IConnectionMultiplexer? _redis;
    private readonly IDatabase? _db;
    private readonly ILogger<RedisCacheRepository> _logger;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly bool _isAvailable;

    public RedisCacheRepository(
        IConnectionMultiplexer? redis,
        ILogger<RedisCacheRepository> logger)
    {
        _redis = redis;
        _db = redis?.GetDatabase();
        _logger = logger;
        _isAvailable = redis != null && redis.IsConnected;

        // Configure JSON serialization: camelCase naming, ignore null values
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = false // Compact JSON for cache efficiency
        };
    }

    /// <inheritdoc />
    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        if (!_isAvailable || _db == null)
        {
            _logger.LogDebug("Cache unavailable, returning null for key: {Key}", key);
            return default;
        }

        try
        {
            var value = await _db.StringGetAsync(key);

            if (!value.HasValue)
            {
                _logger.LogDebug("Cache miss for key: {Key}", key);
                return default;
            }

            _logger.LogDebug("Cache hit for key: {Key}", key);
            return JsonSerializer.Deserialize<T>(value!, _jsonOptions);
        }
        catch (RedisConnectionException ex)
        {
            _logger.LogError(ex, "Redis connection failed while getting key: {Key}", key);
            return default; // Graceful degradation: return null on connection failure
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize cached value for key: {Key}. Deleting corrupted entry.", key);
            // Self-healing: Remove corrupted cache entry to prevent repeated failures
            await DeleteAsync(key, cancellationToken);
            return default; // Return null on deserialization error
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error getting cache key: {Key}", key);
            return default;
        }
    }

    /// <inheritdoc />
    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken cancellationToken = default)
    {
        if (!_isAvailable || _db == null)
        {
            _logger.LogDebug("Cache unavailable, skipping set for key: {Key}", key);
            return;
        }

        try
        {
            var json = JsonSerializer.Serialize(value, _jsonOptions);
            await _db.StringSetAsync(key, json, expiry);

            _logger.LogDebug("Cache set for key: {Key} with expiry: {Expiry}", key, expiry?.ToString() ?? "none");
        }
        catch (RedisConnectionException ex)
        {
            _logger.LogError(ex, "Redis connection failed while setting key: {Key}", key);
            // Fail silently - cache is not critical for application functionality
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to serialize value for key: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error setting cache key: {Key}", key);
        }
    }

    /// <inheritdoc />
    public async Task DeleteAsync(string key, CancellationToken cancellationToken = default)
    {
        if (!_isAvailable || _db == null)
        {
            _logger.LogDebug("Cache unavailable, skipping delete for key: {Key}", key);
            return;
        }

        try
        {
            var deleted = await _db.KeyDeleteAsync(key);

            if (deleted)
            {
                _logger.LogDebug("Cache key deleted: {Key}", key);
            }
            else
            {
                _logger.LogDebug("Cache key not found for deletion: {Key}", key);
            }
        }
        catch (RedisConnectionException ex)
        {
            _logger.LogError(ex, "Redis connection failed while deleting key: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error deleting cache key: {Key}", key);
        }
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        if (!_isAvailable || _db == null)
        {
            _logger.LogDebug("Cache unavailable, returning false for exists check: {Key}", key);
            return false;
        }

        try
        {
            var exists = await _db.KeyExistsAsync(key);
            _logger.LogDebug("Cache key exists check for {Key}: {Exists}", key, exists);
            return exists;
        }
        catch (RedisConnectionException ex)
        {
            _logger.LogError(ex, "Redis connection failed while checking key existence: {Key}", key);
            return false; // Return false on connection failure
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error checking cache key existence: {Key}", key);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<int> RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default)
    {
        if (!_isAvailable || _db == null || _redis == null)
        {
            _logger.LogDebug("Cache unavailable, skipping pattern delete for pattern: {Pattern}", pattern);
            return 0;
        }

        try
        {
            var deletedCount = 0;

            // Get server endpoint for SCAN command
            // Note: This assumes single-server Redis. For cluster, iterate all endpoints.
            var endpoints = _redis.GetEndPoints();
            if (endpoints.Length == 0)
            {
                _logger.LogWarning("No Redis endpoints available for pattern delete: {Pattern}", pattern);
                return 0;
            }

            var server = _redis.GetServer(endpoints[0]);

            // Use SCAN to iterate keys matching pattern (safe for large key sets)
            // Note: SCAN doesn't block Redis and is production-safe
            await foreach (var key in server.KeysAsync(pattern: pattern))
            {
                var deleted = await _db.KeyDeleteAsync(key);
                if (deleted)
                {
                    deletedCount++;
                }
            }

            _logger.LogInformation("Deleted {Count} cache keys matching pattern: {Pattern}", deletedCount, pattern);
            return deletedCount;
        }
        catch (RedisConnectionException ex)
        {
            _logger.LogError(ex, "Redis connection failed while deleting pattern: {Pattern}", pattern);
            return 0; // Graceful degradation: return 0 on connection failure
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error deleting cache keys by pattern: {Pattern}", pattern);
            return 0;
        }
    }
}
