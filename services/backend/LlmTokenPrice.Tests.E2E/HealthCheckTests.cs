using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Playwright;
using System.Text.Json;

namespace LlmTokenPrice.Tests.E2E;

/// <summary>
/// AC#6, AC#12: E2E tests for API health endpoint using Playwright API request context.
/// Validates /api/health returns 200 OK with database + Redis status checks.
/// Uses WebApplicationFactory for hosting the API in-memory during tests.
/// </summary>
public class HealthCheckTests : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
{
    private readonly WebApplicationFactory<Program> _factory;
    private IAPIRequestContext? _apiContext;

    public HealthCheckTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    /// <summary>
    /// AC#6: Initializes Playwright API request context for fast data seeding and API calls.
    /// Playwright API context is faster than browser automation (1-2s vs 10-30s per test).
    /// </summary>
    public async Task InitializeAsync()
    {
        // Install Playwright browsers if not already installed
        Program.Main(new[] { "install", "--with-deps" });

        var playwright = await Playwright.CreateAsync();

        // AC#6: Use API request context (not browser) for fast API testing
        _apiContext = await playwright.APIRequest.NewContextAsync(new()
        {
            BaseURL = _factory.Server.BaseAddress.ToString(),
            IgnoreHTTPSErrors = true
        });
    }

    /// <summary>
    /// AC#12: Validates /api/health endpoint returns 200 OK with all services healthy.
    /// Critical smoke test that ensures API, database, and Redis are operational.
    /// </summary>
    [Fact]
    public async Task HealthEndpoint_Should_Return_200_OK_With_All_Services_Healthy()
    {
        // Arrange
        if (_apiContext == null)
        {
            throw new InvalidOperationException("API context not initialized");
        }

        // Act - Send GET request to /api/health
        var response = await _apiContext.GetAsync("/api/health");

        // Assert - Response status
        response.Status.Should().Be(200, "Health endpoint should return 200 OK when all services are healthy");

        // Assert - Response body
        var responseBody = await response.TextAsync();
        responseBody.Should().NotBeNullOrEmpty("Health endpoint should return JSON response");

        var healthResponse = JsonSerializer.Deserialize<HealthResponse>(responseBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        healthResponse.Should().NotBeNull("Health response should be valid JSON");
        healthResponse!.Database.Should().NotBeNull("Database health should be present");
        healthResponse.Database!.Status.Should().Be("healthy", "Database should be healthy");

        healthResponse.Redis.Should().NotBeNull("Redis health should be present");
        healthResponse.Redis!.Status.Should().Be("healthy", "Redis should be healthy");
    }

    /// <summary>
    /// AC#12: Validates health endpoint includes latency measurements for monitoring.
    /// Latency metrics help identify performance degradation in production.
    /// </summary>
    [Fact]
    public async Task HealthEndpoint_Should_Include_Latency_Measurements()
    {
        // Arrange
        if (_apiContext == null)
        {
            throw new InvalidOperationException("API context not initialized");
        }

        // Act
        var response = await _apiContext.GetAsync("/api/health");

        // Assert
        response.Status.Should().Be(200);

        var responseBody = await response.TextAsync();
        var healthResponse = JsonSerializer.Deserialize<HealthResponse>(responseBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        healthResponse!.Database!.LatencyMs.Should().BeGreaterThanOrEqualTo(0,
            "Database latency should be non-negative");

        healthResponse.Database.LatencyMs.Should().BeLessThan(500,
            "Database latency should be <500ms (NFR target)");

        healthResponse.Redis!.LatencyMs.Should().BeGreaterThanOrEqualTo(0,
            "Redis latency should be non-negative");

        healthResponse.Redis.LatencyMs.Should().BeLessThan(100,
            "Redis latency should be <100ms for optimal performance");
    }

    /// <summary>
    /// AC#13: Validates E2E test execution completes within 5 minutes target.
    /// This test itself should complete in <10 seconds as a smoke test.
    /// </summary>
    [Fact]
    public async Task HealthEndpoint_Should_Respond_Quickly()
    {
        // Arrange
        if (_apiContext == null)
        {
            throw new InvalidOperationException("API context not initialized");
        }

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var response = await _apiContext.GetAsync("/api/health");

        stopwatch.Stop();

        // Assert
        response.Status.Should().Be(200);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(2000,
            "Health endpoint should respond in <2s for smoke test performance target");
    }

    /// <summary>
    /// AC#12: Validates health endpoint returns correct Content-Type header.
    /// </summary>
    [Fact]
    public async Task HealthEndpoint_Should_Return_JSON_ContentType()
    {
        // Arrange
        if (_apiContext == null)
        {
            throw new InvalidOperationException("API context not initialized");
        }

        // Act
        var response = await _apiContext.GetAsync("/api/health");

        // Assert
        response.Status.Should().Be(200);
        response.Headers["content-type"].Should().Contain("application/json",
            "Health endpoint should return JSON content type");
    }

    public async Task DisposeAsync()
    {
        if (_apiContext != null)
        {
            await _apiContext.DisposeAsync();
        }
    }

    // Response models matching API contract
    private class HealthResponse
    {
        public ServiceHealth? Database { get; set; }
        public ServiceHealth? Redis { get; set; }
    }

    private class ServiceHealth
    {
        public string Status { get; set; } = string.Empty;
        public int LatencyMs { get; set; }
    }
}
