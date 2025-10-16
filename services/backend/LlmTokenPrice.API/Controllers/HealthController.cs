using LlmTokenPrice.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace LlmTokenPrice.API.Controllers;

/// <summary>
/// Health check endpoint for monitoring service availability.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IConnectionMultiplexer? _redis;
    private readonly ILogger<HealthController> _logger;

    public HealthController(
        AppDbContext context,
        IConnectionMultiplexer? redis,
        ILogger<HealthController> logger)
    {
        _context = context;
        _redis = redis;
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
        var dbLatencyMs = 0.0;
        try
        {
            var startTime = DateTime.UtcNow;
            dbHealth = await _context.Database.CanConnectAsync();
            dbLatencyMs = (DateTime.UtcNow - startTime).TotalMilliseconds;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database health check failed");
        }

        // Redis health check
        var redisHealth = false;
        var redisLatencyMs = 0.0;
        try
        {
            if (_redis != null)
            {
                redisHealth = _redis.IsConnected;

                if (redisHealth)
                {
                    // Optional: Ping Redis to measure latency
                    var db = _redis.GetDatabase();
                    var startTime = DateTime.UtcNow;
                    await db.PingAsync();
                    redisLatencyMs = (DateTime.UtcNow - startTime).TotalMilliseconds;
                }
            }
            else
            {
                _logger.LogDebug("Redis connection not configured");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Redis health check failed");
        }

        // Determine overall status
        // - healthy: Both database and Redis connected
        // - degraded: Only database connected (cache unavailable but app functional)
        // - unhealthy: Database failed (app cannot function)
        var status = dbHealth && redisHealth ? "healthy"
                   : dbHealth ? "degraded"
                   : "unhealthy";

        var response = new
        {
            status,
            services = new
            {
                database = new
                {
                    status = dbHealth ? "ok" : "error",
                    latencyMs = Math.Round(dbLatencyMs, 2)
                },
                redis = new
                {
                    status = redisHealth ? "ok" : "error",
                    latencyMs = redisHealth ? Math.Round(redisLatencyMs, 2) : 0
                }
            },
            timestamp = DateTime.UtcNow
        };

        return dbHealth
            ? Ok(response)
            : StatusCode(StatusCodes.Status503ServiceUnavailable, response);
    }
}
