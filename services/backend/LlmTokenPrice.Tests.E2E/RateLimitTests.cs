using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace LlmTokenPrice.Tests.E2E;

/// <summary>
/// E2E tests for API rate limiting functionality.
/// Story 2.13 Task 7: Verifies that admin endpoints are protected with 100 req/min rate limit.
/// </summary>
public class RateLimitTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public RateLimitTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    /// <summary>
    /// Story 2.13 Task 7.5: Verify that admin endpoints return 429 after exceeding rate limit.
    /// Sends 105 requests and verifies requests 101-105 return 429 Too Many Requests.
    /// </summary>
    [Fact]
    public async Task AdminEndpoint_ExceedingRateLimit_Should_Return429()
    {
        // GIVEN: Admin user is authenticated
        var loginRequest = new { username = "admin", password = "admin123" };
        await _client.PostAsJsonAsync("/api/admin/auth/login", loginRequest);

        // WHEN: Sending 105 requests to admin endpoint (rate limit is 100/minute)
        var endpoint = "/api/admin/benchmarks";
        var responses = new List<HttpResponseMessage>();

        for (int i = 0; i < 105; i++)
        {
            var response = await _client.GetAsync(endpoint);
            responses.Add(response);
        }

        // THEN: First 100 requests should succeed (200 OK)
        var successfulRequests = responses.Take(100).ToList();
        successfulRequests.Should().AllSatisfy(r =>
            r.StatusCode.Should().Be(HttpStatusCode.OK, "First 100 requests should be within rate limit"));

        // THEN: Requests 101-105 should be rate limited (429 Too Many Requests)
        var rateLimitedRequests = responses.Skip(100).ToList();
        rateLimitedRequests.Should().AllSatisfy(r =>
            r.StatusCode.Should().Be((HttpStatusCode)429, "Requests beyond 100/minute should be rate limited"));

        // THEN: Rate limited responses should contain error message
        var rateLimitedResponse = rateLimitedRequests.First();
        var content = await rateLimitedResponse.Content.ReadAsStringAsync();
        content.Should().Contain("RATE_LIMIT_EXCEEDED", "Response should indicate rate limit error");
        content.Should().Contain("API rate limit exceeded", "Response should have user-friendly message");
    }

    /// <summary>
    /// Story 2.13 Task 7.4: Verify that 429 response includes Retry-After header.
    /// </summary>
    [Fact]
    public async Task RateLimitExceeded_Should_IncludeRetryAfterHeader()
    {
        // GIVEN: Admin user is authenticated
        var loginRequest = new { username = "admin", password = "admin123" };
        await _client.PostAsJsonAsync("/api/admin/auth/login", loginRequest);

        // WHEN: Exceeding rate limit by sending 101 requests
        var endpoint = "/api/admin/benchmarks";
        HttpResponseMessage? rateLimitedResponse = null;

        for (int i = 0; i < 101; i++)
        {
            rateLimitedResponse = await _client.GetAsync(endpoint);
        }

        // THEN: 101st request should return 429
        rateLimitedResponse.Should().NotBeNull();
        rateLimitedResponse!.StatusCode.Should().Be((HttpStatusCode)429);

        // THEN: Response should include Retry-After header
        rateLimitedResponse.Headers.Should().ContainKey("Retry-After",
            "Rate limit response should include Retry-After header for client guidance");

        var retryAfter = rateLimitedResponse.Headers.GetValues("Retry-After").FirstOrDefault();
        retryAfter.Should().NotBeNullOrEmpty("Retry-After header should have a value");
    }

    /// <summary>
    /// Verify that public endpoints are NOT rate limited (only admin endpoints).
    /// </summary>
    [Fact]
    public async Task PublicEndpoint_Should_NotBeRateLimited()
    {
        // GIVEN: Public models endpoint
        var endpoint = "/api/models";

        // WHEN: Sending 105 requests to public endpoint
        var responses = new List<HttpResponseMessage>();

        for (int i = 0; i < 105; i++)
        {
            var response = await _client.GetAsync(endpoint);
            responses.Add(response);
        }

        // THEN: All requests should succeed (no rate limiting on public endpoints)
        responses.Should().AllSatisfy(r =>
            r.StatusCode.Should().Be(HttpStatusCode.OK,
                "Public endpoints should not have rate limiting"));
    }

    /// <summary>
    /// Verify that rate limit response has correct JSON structure and content type.
    /// </summary>
    [Fact]
    public async Task RateLimitExceeded_Should_ReturnJsonErrorResponse()
    {
        // GIVEN: Admin user is authenticated
        var loginRequest = new { username = "admin", password = "admin123" };
        await _client.PostAsJsonAsync("/api/admin/auth/login", loginRequest);

        // WHEN: Exceeding rate limit
        var endpoint = "/api/admin/benchmarks";
        HttpResponseMessage? rateLimitedResponse = null;

        for (int i = 0; i < 101; i++)
        {
            rateLimitedResponse = await _client.GetAsync(endpoint);
        }

        // THEN: Response should have application/json content type
        rateLimitedResponse.Should().NotBeNull();
        rateLimitedResponse!.Content.Headers.ContentType?.MediaType.Should().Be("application/json",
            "Rate limit error should be returned as JSON");

        // THEN: Response JSON should have error structure
        var content = await rateLimitedResponse.Content.ReadAsStringAsync();
        var jsonDoc = JsonDocument.Parse(content);
        var root = jsonDoc.RootElement;

        root.TryGetProperty("error", out var errorElement).Should().BeTrue("Response should have 'error' property");
        errorElement.TryGetProperty("code", out var codeElement).Should().BeTrue("Error should have 'code' property");
        errorElement.TryGetProperty("message", out var messageElement).Should().BeTrue("Error should have 'message' property");

        codeElement.GetString().Should().Be("RATE_LIMIT_EXCEEDED");
        messageElement.GetString().Should().Contain("API rate limit exceeded");
    }
}
