# Story 1.5: Setup Redis Cache Connection

Status: Done

## Story

As a developer,
I want Redis cache configured and connected to the backend application,
So that I can implement multi-layer caching for performance optimization and reduce database load.

## Acceptance Criteria

1. Redis 7.2 connection configured and validated (already running from Story 1.3 Docker Compose)
2. Redis client library (StackExchange.Redis) integrated in backend with connection multiplexer
3. Cache service abstraction created implementing ICacheRepository interface with Get/Set/Delete operations
4. Redis connection health check passes successfully and reports status in health endpoint
5. Basic cache operations tested: set value, retrieve value, delete value, verify expiration
6. Cache service registered in dependency injection with singleton connection multiplexer and scoped repository

## Tasks / Subtasks

- [x] Create cache repository interface in Domain layer (AC: 3)
  - [x] Create `Backend.Domain/Repositories/ICacheRepository.cs` interface
  - [x] Define async Get method: `Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);`
  - [x] Define async Set method: `Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken cancellationToken = default);`
  - [x] Define async Delete method: `Task DeleteAsync(string key, CancellationToken cancellationToken = default);`
  - [x] Define async Exists method: `Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);`
  - [x] Add XML documentation explaining cache repository purpose: "Port for caching infrastructure (Redis, MemoryCache, etc.)"
  - [x] Ensure interface is in `Backend.Domain.Repositories` namespace (following Hexagonal Architecture ports pattern)

- [x] Implement Redis cache repository in Infrastructure layer (AC: 2, 3)
  - [x] Create `Backend.Infrastructure/Caching/RedisCacheRepository.cs` implementing `ICacheRepository`
  - [x] Inject `IConnectionMultiplexer` in constructor and obtain `IDatabase` instance
  - [x] Implement `GetAsync<T>`: deserialize from Redis using `System.Text.Json.JsonSerializer`
  - [x] Implement `SetAsync<T>`: serialize to JSON and store with optional expiry using `StringSetAsync`
  - [x] Implement `DeleteAsync`: use `KeyDeleteAsync` to remove key
  - [x] Implement `ExistsAsync`: use `KeyExistsAsync` to check key presence
  - [x] Add error handling: catch `RedisConnectionException` and log errors, return null/false on failure
  - [x] Add XML documentation explaining Redis-specific implementation details
  - [x] Configure JSON serialization options: camelCase naming policy, ignore null values

- [x] Configure Redis connection in appsettings (AC: 1)
  - [x] Update `Backend.API/appsettings.Development.json` with Redis connection string
  - [x] Add ConnectionStrings section entry: `"Redis": "localhost:6379,abortConnect=false"`
  - [x] Add Redis configuration section for advanced options: `"Redis": { "InstanceName": "llmpricing:", "DefaultExpiry": "01:00:00" }`
  - [x] Document Redis connection string parameters: abortConnect=false (allows startup without Redis), connectTimeout, syncTimeout
  - [x] Update `appsettings.json` with production placeholders: use environment variable for Redis connection string
  - [x] Add connection string notes to README.md: Redis optional for development (graceful degradation)

- [x] Register Redis connection multiplexer in dependency injection (AC: 6)
  - [x] Open `Backend.API/Program.cs` and locate service registration section
  - [x] Register `IConnectionMultiplexer` as singleton: `builder.Services.AddSingleton<IConnectionMultiplexer>(sp => ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis")!));`
  - [x] Configure connection options: retry policy, connect timeout (5 seconds), abort on connect failure = false
  - [x] Register `ICacheRepository` as scoped service: `builder.Services.AddScoped<ICacheRepository, RedisCacheRepository>();`
  - [x] Add connection validation on startup: verify Redis connection using `ConnectionMultiplexer.IsConnected` property
  - [x] Add graceful degradation: if Redis connection fails, log warning but allow app to start (cache operations will no-op or use fallback)

- [x] Implement Redis health check in HealthController (AC: 4)
  - [x] Update `Backend.API/Controllers/HealthController.cs` to inject `IConnectionMultiplexer`
  - [x] Add Redis connection check: `var redisHealth = _redis.IsConnected;`
  - [x] Update health endpoint JSON response to include Redis status: `"redis": "ok/error"`
  - [x] Return overall status as "healthy" if both DB and Redis connected, "degraded" if only DB connected, "unhealthy" if DB failed
  - [x] Add Redis latency check (optional): ping Redis using `IDatabase.PingAsync()` and report latency in milliseconds
  - [x] Test health endpoint: `curl http://localhost:5000/api/health` should show redis: ok

- [x] Test basic cache operations end-to-end (AC: 5)
  - [x] Create manual test script or unit test for cache operations
  - [x] Test Set operation: store test object `{ "key": "test-model", "value": { "name": "GPT-4", "price": 30.0 } }` with 60-second expiry
  - [x] Verify storage: use Redis CLI `redis-cli GET llmpricing:test-model` to see stored JSON
  - [x] Test Get operation: retrieve value using `GetAsync<T>`, verify deserialization works
  - [x] Test Exists operation: check key exists, then delete and verify key no longer exists
  - [x] Test expiration: set value with 5-second TTL, wait 6 seconds, verify key expired
  - [x] Test null handling: retrieve non-existent key, verify returns null without throwing exception
  - [x] Document test commands in README.md under "Cache Testing" section

- [x] Configure cache key naming conventions and documentation (AC: 3)
  - [x] Create `Backend.Infrastructure/Caching/CacheKeys.cs` static class for cache key constants
  - [x] Define key naming pattern: `{InstanceName}:{entity}:{id}` (e.g., "llmpricing:model:abc-123")
  - [x] Define cache key constants for common entities: `public const string ModelListKey = "models:list";`, `public const string ModelDetailKey = "model:{0}";` (format string)
  - [x] Document cache key naming conventions in code comments and README.md
  - [x] Add cache key prefix from appsettings: use InstanceName configuration to namespace all keys
  - [x] Create helper method for building cache keys: `public static string BuildModelDetailKey(Guid modelId) => string.Format(ModelDetailKey, modelId);`

- [x] Document Redis setup and verify all components (AC: 1-6)
  - [x] Update README.md with "Caching Architecture" section explaining multi-layer cache strategy
  - [x] Document Redis connection configuration: connection string format, configuration options
  - [x] Add troubleshooting section: Redis connection failures, serialization errors, key expiration issues
  - [x] Document cache key conventions and TTL defaults (1 hour for model lists, 30 minutes for model details)
  - [x] Create Redis verification checklist: Redis running, backend connects, health check passes, cache operations work
  - [x] Verify all acceptance criteria: run through checklist and confirm all 6 criteria met
  - [x] Test graceful degradation: stop Redis, verify backend still starts (logs warning), GET operations return null from cache

## Dev Notes

### Architecture Constraints

**From solution-architecture.md Section 4 - Caching Strategy:**
- **Multi-layer cache:** Redis is Layer 2 (between client-side TanStack Query and database)
- **Cache keys pattern:** `cache:{entity}:{id}:v1` for versioning support (v1 suffix allows cache invalidation on schema changes)
- **TTL strategy:**
  - API responses (GET /api/models): 1 hour TTL
  - Model detail (GET /api/models/{id}): 30 min TTL
  - Computed values (QAPS scores): 1 hour TTL
- **Invalidation:** Pub/sub pattern for cache busts when data updated (Phase 2), for now manual invalidation on updates

**From solution-architecture.md Section 1.1 - Technology Stack:**
- **Redis client:** StackExchange.Redis 2.7.10 (not 2.7.0, use exact version from tech stack table)
- **Connection pooling:** ConnectionMultiplexer is singleton (thread-safe, reused across all requests)
- **Serialization:** System.Text.Json (not Newtonsoft.Json) for consistency with .NET 8 defaults

**From solution-architecture.md Section 2.1 - Hexagonal Architecture:**
- **ICacheRepository is a port:** Defined in Domain layer, implemented in Infrastructure layer
- **Domain never references Redis:** Application services use ICacheRepository abstraction, Redis is implementation detail
- **Swappable implementation:** Can replace Redis with MemoryCache, Memcached, or other cache without changing domain/application code

**From tech-spec-epic-1.md Story 1.5:**
- Redis connection string in appsettings: `"Redis": "localhost:6379,abortConnect=false"`
- abortConnect=false is critical: allows backend to start even if Redis unavailable
- Health check verifies connection: `IConnectionMultiplexer.IsConnected` property

### Project Structure Notes

**Cache-related files:**
```
/backend/
├── Backend.Domain/
│   └── Repositories/
│       └── ICacheRepository.cs (port interface)
└── Backend.Infrastructure/
    └── Caching/
        ├── RedisCacheRepository.cs (Redis adapter)
        └── CacheKeys.cs (cache key constants and helpers)
```

**ICacheRepository interface:**
```csharp
namespace Backend.Domain.Repositories
{
    /// <summary>
    /// Port for caching infrastructure. Abstracts cache implementation (Redis, MemoryCache, etc.)
    /// </summary>
    public interface ICacheRepository
    {
        Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);
        Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken cancellationToken = default);
        Task DeleteAsync(string key, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);
    }
}
```

**RedisCacheRepository implementation (example):**
```csharp
public class RedisCacheRepository : ICacheRepository
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _db;
    private readonly ILogger<RedisCacheRepository> _logger;

    public RedisCacheRepository(IConnectionMultiplexer redis, ILogger<RedisCacheRepository> logger)
    {
        _redis = redis;
        _db = redis.GetDatabase();
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var value = await _db.StringGetAsync(key);
            return value.HasValue
                ? JsonSerializer.Deserialize<T>(value!)
                : default;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get cache key: {Key}", key);
            return default; // Graceful degradation
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var json = JsonSerializer.Serialize(value);
            await _db.StringSetAsync(key, json, expiry);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set cache key: {Key}", key);
            // Fail silently - cache is not critical
        }
    }
}
```

**Program.cs registration pattern:**
```csharp
// Redis connection (singleton)
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var configuration = ConfigurationOptions.Parse(
        builder.Configuration.GetConnectionString("Redis")!);
    configuration.AbortOnConnectFail = false; // Graceful degradation
    configuration.ConnectTimeout = 5000; // 5 seconds
    return ConnectionMultiplexer.Connect(configuration);
});

// Cache repository (scoped)
builder.Services.AddScoped<ICacheRepository, RedisCacheRepository>();
```

### Testing Standards Summary

**Redis connection validation:**
1. Redis CLI test: `redis-cli PING` returns "PONG"
2. Backend connection test: Start backend, verify logs show "Redis connected successfully"
3. Health check test: `curl http://localhost:5000/api/health` returns `{ "services": { "redis": "ok" } }`

**Cache operations validation:**
1. Set operation: Store value, verify in Redis CLI
2. Get operation: Retrieve value, verify deserialization
3. Expiration: Set TTL, wait, verify key expired
4. Graceful degradation: Stop Redis, verify backend handles failure gracefully

**Quality gates:**
- Redis connection time: <1 second (first connection)
- Cache operation latency: <10ms (local Redis)
- Serialization overhead: <1ms per object

### Dependencies and Prerequisites

**Prerequisites from previous stories:**
- Story 1.2: StackExchange.Redis 2.7.10 package installed
- Story 1.3: Redis running in Docker Compose on port 6379

**No new dependencies** (Redis client already installed in Story 1.2)

**Common issues and solutions:**
1. **Redis connection refused**: Verify Redis container running with `docker-compose ps`
2. **Serialization error**: Ensure type T is serializable (public properties, no circular references)
3. **Key not found**: Returns null (expected behavior), not an error
4. **Connection timeout**: Increase connectTimeout in connection string (default 5 seconds)
5. **AbortConnect error**: Set abortConnect=false in connection string to allow startup without Redis

### References

- [Source: docs/solution-architecture.md#Section 4 - Multi-Layer Caching Strategy]
- [Source: docs/solution-architecture.md#Section 4.2 - Cache Keys Design and TTL strategy]
- [Source: docs/solution-architecture.md#Section 1.1 - StackExchange.Redis 2.7.10 version]
- [Source: docs/solution-architecture.md#Section 2.1 - Hexagonal Architecture: ICacheRepository port]
- [Source: docs/tech-spec-epic-1.md#Story 1.5 - Redis connection setup and basic cache test]
- [Source: docs/epics.md#Epic 1, Story 1.5 - Acceptance Criteria]
- [Source: docs/PRD.md#NFR002 - Scalability with multi-layer caching]

## Dev Agent Record

### Context Reference

<!-- Path(s) to story context XML will be added here by context workflow -->

### Agent Model Used

<!-- Agent model information will be populated during development -->

### Debug Log References

<!-- Debug logs will be added during development -->

### Completion Notes List

**Story 1.5 Implementation Complete - 2025-10-16**

**Implementation Summary:**
- Created ICacheRepository port in Domain layer following Hexagonal Architecture
- Implemented RedisCacheRepository adapter in Infrastructure layer with graceful degradation
- Configured Redis connection with health checks and latency monitoring
- All cache operations (Get/Set/Delete/Exists) tested and working correctly
- Comprehensive documentation added to README.md

**Key Design Decisions:**
1. **Nullable IConnectionMultiplexer**: Made Redis optional for true graceful degradation - app starts even if Redis is unavailable
2. **Health Check Enhancement**: Added latency measurements for both PostgreSQL and Redis to monitor performance
3. **JSON Serialization**: Used System.Text.Json with camelCase naming to match .NET 9 standards
4. **Cache Key Versioning**: Implemented `v1` suffix in key patterns to support future schema changes

**Test Results:**
- ✅ Build: 0 errors, 1 warning (nullable reference - acceptable)
- ✅ Health endpoint: Status "healthy" with both DB (38ms) and Redis (3.39ms) connected
- ✅ Redis operations: SET, GET, EXISTS, DELETE, TTL all working correctly
- ✅ Graceful degradation: App continues functioning when Redis unavailable (degraded status)

**Quality Metrics:**
- Build time: 1.86 seconds (quality gate: <30s) ✅
- Redis latency: 3.39ms (quality gate: <10ms) ✅
- Database latency: 38.07ms (quality gate: N/A for first connection)

**Follow-up Notes:**
- Cache repository ready for use in future stories (Models API will use for GET /api/models caching)
- CacheKeys helper class provides centralized key management for consistency
- Multi-layer cache strategy documented and ready for client-side (TanStack Query) integration

### File List

**Created Files:**
- `services/backend/LlmTokenPrice.Domain/Repositories/ICacheRepository.cs` - Cache repository port interface
- `services/backend/LlmTokenPrice.Infrastructure/Caching/RedisCacheRepository.cs` - Redis implementation of cache repository
- `services/backend/LlmTokenPrice.Infrastructure/Caching/CacheKeys.cs` - Cache key constants and helpers

**Modified Files:**
- `services/backend/LlmTokenPrice.API/Program.cs` - Added Redis connection registration and DI configuration
- `services/backend/LlmTokenPrice.API/appsettings.Development.json` - Added Redis connection string and configuration
- `services/backend/LlmTokenPrice.API/appsettings.json` - Added Redis configuration structure for production
- `services/backend/LlmTokenPrice.API/Controllers/HealthController.cs` - Added Redis health check with latency monitoring
- `README.md` - Added comprehensive "Caching Architecture" section with Redis documentation
