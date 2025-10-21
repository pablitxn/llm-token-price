using LlmTokenPrice.Domain.Caching;

namespace LlmTokenPrice.Infrastructure.Caching;

/// <summary>
/// Infrastructure adapter for Domain CacheConfiguration.
/// This class exists for backward compatibility with Infrastructure code that references CacheKeys.
/// New code should use LlmTokenPrice.Domain.Caching.CacheConfiguration directly.
/// </summary>
/// <remarks>
/// Hexagonal Architecture: Cache keys moved to Domain layer (business concepts).
/// This adapter allows Infrastructure code to continue working without breaking changes.
/// Eventually this can be removed once all Infrastructure code references are updated.
/// </remarks>
[Obsolete("Use LlmTokenPrice.Domain.Caching.CacheConfiguration instead")]
public static class CacheKeys
{
    // Delegate all members to Domain layer CacheConfiguration
    public const string InstancePrefix = CacheConfiguration.InstancePrefix;
    public const string ModelListKey = CacheConfiguration.ModelListKey;
    public const string ModelDetailKeyPattern = CacheConfiguration.ModelDetailKeyPattern;
    public const string QapsKeyPattern = CacheConfiguration.QapsKeyPattern;
    public const string BenchmarkListKey = CacheConfiguration.BenchmarkListKey;

    public static string BuildModelDetailKey(Guid modelId) =>
        CacheConfiguration.BuildModelDetailKey(modelId);

    public static string BuildQapsKey(string filterName) =>
        CacheConfiguration.BuildQapsKey(filterName);

    public static class DefaultTtl
    {
        public static readonly TimeSpan ApiResponses = CacheConfiguration.DefaultTtl.ApiResponses;
        public static readonly TimeSpan ModelDetail = CacheConfiguration.DefaultTtl.ModelDetail;
        public static readonly TimeSpan ComputedValues = CacheConfiguration.DefaultTtl.ComputedValues;
    }

    public static class InvalidationPatterns
    {
        public const string AllModels = CacheConfiguration.InvalidationPatterns.AllModels;
        public const string ModelLists = CacheConfiguration.InvalidationPatterns.ModelLists;
        public const string AllBenchmarks = CacheConfiguration.InvalidationPatterns.AllBenchmarks;
        public const string AllQaps = CacheConfiguration.InvalidationPatterns.AllQaps;
        public const string Everything = CacheConfiguration.InvalidationPatterns.Everything;
    }
}
