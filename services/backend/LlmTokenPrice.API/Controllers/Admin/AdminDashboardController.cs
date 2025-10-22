using LlmTokenPrice.Application.DTOs;
using LlmTokenPrice.Domain.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LlmTokenPrice.API.Controllers.Admin;

/// <summary>
/// Admin dashboard controller providing metrics and analytics endpoints.
/// Story 2.12: Dashboard metrics for data freshness monitoring.
/// Story 3.1b Task 3.3: 5-minute Redis cache for dashboard metrics.
/// </summary>
/// <remarks>
/// **Rate Limiting:** This endpoint is rate-limited to 100 requests per minute per IP address.
/// If the limit is exceeded, the API returns HTTP 429 (Too Many Requests) with a Retry-After header.
///
/// **Caching:** Dashboard metrics cached in Redis for 5 minutes (300 seconds).
/// Cache invalidated on model/benchmark create/update/delete via cache key pattern: cache:dashboard:metrics
/// </remarks>
[ApiController]
[Route("api/admin/dashboard")]
[Authorize]  // JWT authentication required for all admin endpoints
public class AdminDashboardController : ControllerBase
{
    private readonly IAdminModelRepository _adminRepository;
    private readonly ICacheRepository _cacheRepository;
    private readonly ILogger<AdminDashboardController> _logger;

    private const string DashboardMetricsCacheKey = "cache:dashboard:metrics";
    private static readonly TimeSpan CacheExpiry = TimeSpan.FromMinutes(5);

    public AdminDashboardController(
        IAdminModelRepository adminRepository,
        ICacheRepository cacheRepository,
        ILogger<AdminDashboardController> logger)
    {
        _adminRepository = adminRepository ?? throw new ArgumentNullException(nameof(adminRepository));
        _cacheRepository = cacheRepository ?? throw new ArgumentNullException(nameof(cacheRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// GET /api/admin/dashboard/metrics
    /// Returns dashboard metrics including model freshness statistics.
    /// Story 2.12: Data freshness metrics for admin monitoring.
    /// Story 3.1b Task 3.3: Cached in Redis for 5 minutes with automatic invalidation.
    /// </summary>
    /// <returns>Dashboard metrics DTO with timestamp-based counts</returns>
    /// <response code="200">Returns dashboard metrics successfully (may be cached)</response>
    /// <response code="401">Unauthorized - JWT token missing or invalid</response>
    [HttpGet("metrics")]
    [ProducesResponseType(typeof(DashboardMetricsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetDashboardMetrics(CancellationToken cancellationToken = default)
    {
        // Try to get cached metrics first
        var cachedMetrics = await _cacheRepository.GetAsync<DashboardMetricsDto>(
            DashboardMetricsCacheKey,
            cancellationToken);

        if (cachedMetrics != null)
        {
            _logger.LogInformation("Dashboard metrics served from cache (age: {Age}s)",
                (DateTime.UtcNow - cachedMetrics.CalculatedAt).TotalSeconds);

            return Ok(new
            {
                data = cachedMetrics,
                meta = new { cached = true, cacheAge = DateTime.UtcNow - cachedMetrics.CalculatedAt }
            });
        }

        // Cache miss - calculate metrics
        _logger.LogInformation("Cache miss - calculating dashboard metrics");

        var metrics = await CalculateDashboardMetricsAsync(cancellationToken);

        // Store in cache with 5-minute expiry
        await _cacheRepository.SetAsync(
            DashboardMetricsCacheKey,
            metrics,
            CacheExpiry,
            cancellationToken);

        _logger.LogInformation("Dashboard metrics cached for {Expiry}",
            CacheExpiry);

        return Ok(new
        {
            data = metrics,
            meta = new { cached = false }
        });
    }

    /// <summary>
    /// Helper method to calculate dashboard metrics from database.
    /// Extracted for testability and cache invalidation scenarios.
    /// </summary>
    private async Task<DashboardMetricsDto> CalculateDashboardMetricsAsync(CancellationToken cancellationToken)
    {
        // Calculate freshness thresholds
        var now = DateTime.UtcNow;
        var sevenDaysAgo = now.AddDays(-7);
        var thirtyDaysAgo = now.AddDays(-30);

        // Fetch all active models
        var models = await _adminRepository.GetAllModelsAsync(
            searchTerm: null,
            provider: null,
            status: "active", // Only count active models in metrics
            cancellationToken);

        // Calculate Story 2.12 metrics (data freshness)
        var totalModels = models.Count;
        var modelsNeedingUpdates = models.Count(m => m.UpdatedAt < sevenDaysAgo);
        var criticalUpdates = models.Count(m => m.UpdatedAt < thirtyDaysAgo);
        var recentlyUpdated = models.Count(m => m.UpdatedAt >= sevenDaysAgo);
        var pricingNeedingUpdates = models.Count(m =>
            m.PricingUpdatedAt.HasValue && m.PricingUpdatedAt < thirtyDaysAgo);

        // Calculate Story 2.13 Task 15 metrics (data quality)
        var incompleteModels = models.Count(m => m.BenchmarkScores.Count < 3);
        var recentAdditions = models.Count(m => m.CreatedAt >= sevenDaysAgo);

        // Average benchmarks per model (avoid division by zero)
        var averageBenchmarks = totalModels > 0
            ? Math.Round(models.Average(m => m.BenchmarkScores.Count), 1)
            : 0.0;

        // Models by provider (group and order by count descending)
        var modelsByProvider = models
            .GroupBy(m => m.Provider)
            .Select(g => new { Provider = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ToDictionary(x => x.Provider, x => x.Count);

        var metrics = new DashboardMetricsDto
        {
            // Story 2.12 metrics
            TotalActiveModels = totalModels,
            ModelsNeedingUpdates = modelsNeedingUpdates,
            CriticalUpdates = criticalUpdates,
            RecentlyUpdated = recentlyUpdated,
            PricingNeedingUpdates = pricingNeedingUpdates,
            // Story 2.13 Task 15 metrics
            IncompleteModels = incompleteModels,
            RecentAdditions = recentAdditions,
            AverageBenchmarksPerModel = averageBenchmarks,
            ModelsByProvider = modelsByProvider,
            CalculatedAt = now
        };

        _logger.LogInformation(
            "Dashboard metrics calculated: {TotalModels} total, {Stale} stale, {Critical} critical, {Incomplete} incomplete, {AvgBenchmarks} avg benchmarks",
            metrics.TotalActiveModels,
            metrics.ModelsNeedingUpdates,
            metrics.CriticalUpdates,
            metrics.IncompleteModels,
            metrics.AverageBenchmarksPerModel);

        return metrics;
    }
}
