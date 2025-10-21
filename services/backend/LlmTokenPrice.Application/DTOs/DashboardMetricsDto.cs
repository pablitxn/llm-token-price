namespace LlmTokenPrice.Application.DTOs;

/// <summary>
/// Data Transfer Object for admin dashboard metrics.
/// Story 2.12 + 2.13 Task 15: Comprehensive data quality and freshness metrics.
/// </summary>
/// <remarks>
/// Returned by GET /api/admin/dashboard/metrics endpoint.
/// Cached for 5 minutes to optimize performance under admin dashboard load.
/// </remarks>
public record DashboardMetricsDto
{
    // Story 2.12 - Data Freshness Metrics

    /// <summary>
    /// Total number of active models in the system (IsActive = true).
    /// </summary>
    public required int TotalActiveModels { get; init; }

    /// <summary>
    /// Count of models with UpdatedAt > 7 days ago (stale models).
    /// These models should be reviewed and updated soon.
    /// </summary>
    public required int ModelsNeedingUpdates { get; init; }

    /// <summary>
    /// Count of models with UpdatedAt > 30 days ago (critical staleness).
    /// These models require urgent attention and updates.
    /// </summary>
    public required int CriticalUpdates { get; init; }

    /// <summary>
    /// Count of models updated in the last 7 days (fresh data).
    /// Indicates recently maintained models.
    /// </summary>
    public required int RecentlyUpdated { get; init; }

    /// <summary>
    /// Count of models with pricing data > 30 days old (pricing staleness).
    /// Based on PricingUpdatedAt timestamp, critical for pricing platform accuracy.
    /// </summary>
    public required int PricingNeedingUpdates { get; init; }

    // Story 2.13 Task 15 - Additional Data Quality Metrics

    /// <summary>
    /// Count of models with fewer than 3 benchmark scores (incomplete models).
    /// Minimum 3 benchmarks recommended for meaningful quality comparisons.
    /// </summary>
    public required int IncompleteModels { get; init; }

    /// <summary>
    /// Count of models added in the last 7 days (recent additions).
    /// Based on CreatedAt timestamp, indicates catalog growth.
    /// </summary>
    public required int RecentAdditions { get; init; }

    /// <summary>
    /// Average number of benchmark scores per model across all active models.
    /// Indicates overall benchmark coverage quality. Rounded to 1 decimal place.
    /// </summary>
    public required double AverageBenchmarksPerModel { get; init; }

    /// <summary>
    /// Breakdown of model counts by provider (e.g., OpenAI: 15, Anthropic: 8).
    /// Dictionary ordered by count descending (most models first).
    /// Helps identify which providers dominate the catalog.
    /// </summary>
    public required Dictionary<string, int> ModelsByProvider { get; init; }

    /// <summary>
    /// Timestamp when these metrics were calculated (UTC).
    /// Used to display metric freshness to admins.
    /// </summary>
    public required DateTime CalculatedAt { get; init; }
}
