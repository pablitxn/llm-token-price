using LlmTokenPrice.Application.DTOs;
using LlmTokenPrice.Domain.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LlmTokenPrice.API.Controllers.Admin;

/// <summary>
/// Admin dashboard controller providing metrics and analytics endpoints.
/// Story 2.12: Dashboard metrics for data freshness monitoring.
/// </summary>
[ApiController]
[Route("api/admin/dashboard")]
[Authorize]  // JWT authentication required for all admin endpoints
public class AdminDashboardController : ControllerBase
{
    private readonly IAdminModelRepository _adminRepository;
    private readonly ILogger<AdminDashboardController> _logger;

    public AdminDashboardController(
        IAdminModelRepository adminRepository,
        ILogger<AdminDashboardController> logger)
    {
        _adminRepository = adminRepository ?? throw new ArgumentNullException(nameof(adminRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// GET /api/admin/dashboard/metrics
    /// Returns dashboard metrics including model freshness statistics.
    /// Story 2.12: Data freshness metrics for admin monitoring.
    /// </summary>
    /// <returns>Dashboard metrics DTO with timestamp-based counts</returns>
    /// <response code="200">Returns dashboard metrics successfully</response>
    /// <response code="401">Unauthorized - JWT token missing or invalid</response>
    [HttpGet("metrics")]
    [ResponseCache(Duration = 300)] // Cache for 5 minutes (300 seconds)
    [ProducesResponseType(typeof(DashboardMetricsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetDashboardMetrics(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Calculating dashboard metrics");

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

        // Calculate metrics
        var metrics = new DashboardMetricsDto
        {
            TotalActiveModels = models.Count,
            ModelsNeedingUpdates = models.Count(m => m.UpdatedAt < sevenDaysAgo),
            CriticalUpdates = models.Count(m => m.UpdatedAt < thirtyDaysAgo),
            RecentlyUpdated = models.Count(m => m.UpdatedAt >= sevenDaysAgo),
            PricingNeedingUpdates = models.Count(m =>
                m.PricingUpdatedAt.HasValue && m.PricingUpdatedAt < thirtyDaysAgo),
            CalculatedAt = now
        };

        _logger.LogInformation(
            "Dashboard metrics calculated: {TotalModels} total, {Stale} stale, {Critical} critical",
            metrics.TotalActiveModels,
            metrics.ModelsNeedingUpdates,
            metrics.CriticalUpdates);

        return Ok(new { data = metrics });
    }
}
