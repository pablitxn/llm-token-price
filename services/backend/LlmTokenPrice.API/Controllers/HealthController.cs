using LlmTokenPrice.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LlmTokenPrice.API.Controllers;

/// <summary>
/// Health check endpoint for monitoring service availability.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<HealthController> _logger;

    public HealthController(
        AppDbContext context,
        ILogger<HealthController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Check health of database and cache services.
    /// </summary>
    /// <returns>Health status with service details</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Get()
    {
        var dbHealth = false;
        try
        {
            dbHealth = await _context.Database.CanConnectAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database health check failed");
        }

        // Redis health check will be added in Story 1.5
        var redisHealth = false; // Placeholder for Story 1.5

        var status = dbHealth && redisHealth ? "healthy"
                   : dbHealth ? "degraded"
                   : "unhealthy";

        var response = new
        {
            status,
            services = new
            {
                database = dbHealth ? "ok" : "error",
                redis = "pending" // Will be updated in Story 1.5
            },
            timestamp = DateTime.UtcNow
        };

        return dbHealth
            ? Ok(response)
            : StatusCode(StatusCodes.Status503ServiceUnavailable, response);
    }
}
