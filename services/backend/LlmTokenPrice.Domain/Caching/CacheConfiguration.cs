namespace LlmTokenPrice.Domain.Caching;

/// <summary>
/// Domain-level cache configuration defining cache keys, TTLs, and invalidation patterns.
/// Located in Domain layer as these are business concerns (WHAT to cache), not technical implementation (HOW to cache).
/// </summary>
/// <remarks>
/// Hexagonal Architecture principle: Cache keys represent business concepts (models, benchmarks),
/// so they belong in the domain layer. Infrastructure layer handles Redis-specific implementation.
/// </remarks>
public static class CacheConfiguration
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

    /// <summary>
    /// Cache invalidation patterns for bulk cache clearing.
    /// Used with RemoveByPatternAsync for efficient cache invalidation.
    /// </summary>
    public static class InvalidationPatterns
    {
        /// <summary>
        /// Pattern to invalidate ALL model-related caches (list + details + paginated lists).
        /// Example: "llmpricing:model*"
        /// Use when: Model created, updated, or deleted
        ///
        /// This pattern matches:
        /// - Individual model details: "llmpricing:model:{guid}:v1"
        /// - Model lists: "llmpricing:models:list:v1"
        /// - Paginated lists: "llmpricing:models:list:v1:p{page}:s{size}"
        /// </summary>
        public const string AllModels = InstancePrefix + "model*";

        /// <summary>
        /// Pattern to invalidate ONLY model list caches (including paginated variants).
        /// Example: "llmpricing:models:*"
        /// Use when: Need to refresh list but keep individual model caches
        ///
        /// This pattern matches:
        /// - Non-paginated: "llmpricing:models:list:v1"
        /// - Paginated: "llmpricing:models:list:v1:p{page}:s{size}"
        /// </summary>
        public const string ModelLists = InstancePrefix + "models:*";

        /// <summary>
        /// Pattern to invalidate ALL benchmark-related caches.
        /// Example: "llmpricing:benchmark*"
        /// Use when: Benchmark created, updated, or deleted
        /// </summary>
        public const string AllBenchmarks = InstancePrefix + "benchmark*";

        /// <summary>
        /// Pattern to invalidate ALL QAPS calculation caches.
        /// Example: "llmpricing:qaps:*"
        /// Use when: Model or benchmark data changes affecting QAPS scores
        /// </summary>
        public const string AllQaps = InstancePrefix + "qaps:*";

        /// <summary>
        /// Pattern to invalidate EVERYTHING in this instance.
        /// Example: "llmpricing:*"
        /// Use when: Major data migration, schema changes, or full cache flush needed
        /// WARNING: Use sparingly - clears all caches for this application
        /// </summary>
        public const string Everything = InstancePrefix + "*";
    }
}
