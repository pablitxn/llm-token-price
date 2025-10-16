namespace LlmTokenPrice.Infrastructure.Caching;

/// <summary>
/// Centralized cache key constants and builders following the naming pattern:
/// {InstanceName}:{entity}:{id}:v1
/// </summary>
/// <remarks>
/// Cache key naming conventions:
/// - Use instance name prefix (e.g., "llmpricing:") to namespace keys (supports multi-tenancy)
/// - Entity name in plural (e.g., "models", "benchmarks")
/// - Include version suffix ":v1" for schema versioning (allows cache invalidation on breaking changes)
/// - Use format strings for parameterized keys (e.g., "model:{0}")
///
/// TTL Strategy (from solution-architecture.md):
/// - Model list (GET /api/models): 1 hour
/// - Model detail (GET /api/models/{id}): 30 minutes
/// - QAPS scores (computed values): 1 hour
/// </remarks>
public static class CacheKeys
{
    /// <summary>
    /// Cache instance name prefix. Configured via appsettings Redis:InstanceName.
    /// Default: "llmpricing:"
    /// </summary>
    public const string InstancePrefix = "llmpricing:";

    /// <summary>
    /// Cache key for the full models list.
    /// Example: "llmpricing:models:list:v1"
    /// TTL: 1 hour
    /// </summary>
    public const string ModelListKey = InstancePrefix + "models:list:v1";

    /// <summary>
    /// Cache key pattern for individual model details.
    /// Use BuildModelDetailKey(modelId) to construct the full key.
    /// Example: "llmpricing:model:abc-123:v1"
    /// TTL: 30 minutes
    /// </summary>
    public const string ModelDetailKeyPattern = InstancePrefix + "model:{0}:v1";

    /// <summary>
    /// Cache key pattern for QAPS (Quality-Adjusted Price per Score) values.
    /// Example: "llmpricing:qaps:bestvalue:v1"
    /// TTL: 1 hour
    /// </summary>
    public const string QapsKeyPattern = InstancePrefix + "qaps:{0}:v1";

    /// <summary>
    /// Cache key pattern for benchmark lists.
    /// Example: "llmpricing:benchmarks:list:v1"
    /// TTL: 1 hour
    /// </summary>
    public const string BenchmarkListKey = InstancePrefix + "benchmarks:list:v1";

    /// <summary>
    /// Builds a cache key for a specific model by ID.
    /// </summary>
    /// <param name="modelId">The model GUID</param>
    /// <returns>Formatted cache key (e.g., "llmpricing:model:abc-123:v1")</returns>
    public static string BuildModelDetailKey(Guid modelId)
    {
        return string.Format(ModelDetailKeyPattern, modelId);
    }

    /// <summary>
    /// Builds a cache key for QAPS calculation results.
    /// </summary>
    /// <param name="filterName">The filter/query name (e.g., "bestvalue", "cheapest")</param>
    /// <returns>Formatted cache key (e.g., "llmpricing:qaps:bestvalue:v1")</returns>
    public static string BuildQapsKey(string filterName)
    {
        return string.Format(QapsKeyPattern, filterName.ToLowerInvariant());
    }

    /// <summary>
    /// Default TTL values for different cache keys (from solution-architecture.md).
    /// </summary>
    public static class DefaultTtl
    {
        /// <summary>1 hour TTL for API responses (model lists, benchmark lists)</summary>
        public static readonly TimeSpan ApiResponses = TimeSpan.FromHours(1);

        /// <summary>30 minutes TTL for model detail pages</summary>
        public static readonly TimeSpan ModelDetail = TimeSpan.FromMinutes(30);

        /// <summary>1 hour TTL for computed values (QAPS scores)</summary>
        public static readonly TimeSpan ComputedValues = TimeSpan.FromHours(1);
    }
}
