namespace LlmTokenPrice.Application.DTOs;

/// <summary>
/// Data Transfer Object for admin dashboard metrics.
/// Story 2.12: Provides data freshness metrics for admin monitoring.
/// </summary>
/// <remarks>
/// Returned by GET /api/admin/dashboard/metrics endpoint.
/// Cached for 5 minutes to optimize performance under admin dashboard load.
/// </remarks>
public record DashboardMetricsDto
{
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

    /// <summary>
    /// Timestamp when these metrics were calculated (UTC).
    /// Used to display metric freshness to admins.
    /// </summary>
    public required DateTime CalculatedAt { get; init; }
}
