using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using LlmTokenPrice.Application.DTOs;

namespace LlmTokenPrice.Tests.E2E;

/// <summary>
/// E2E tests for models API caching behavior.
/// Story 2.13 Task 4.7: Verifies cache hit/miss reporting via Meta.Cached field.
/// </summary>
public class ModelsCacheTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public ModelsCacheTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    /// <summary>
    /// Story 2.13 Task 4.7: Consecutive requests to GET /api/models should demonstrate caching.
    /// Second request should return Meta.Cached=true (cache hit).
    /// </summary>
    [Fact]
    public async Task GetModels_ConsecutiveRequests_Should_Use_Cache()
    {
        // GIVEN: API is available
        var endpoint = "/api/models";

        // WHEN: First request to GET /api/models
        var firstResponse = await _client.GetAsync(endpoint);

        // THEN: Should return 200 OK
        firstResponse.StatusCode.Should().Be(HttpStatusCode.OK,
            "GET /api/models should return 200 OK");

        var firstJson = await firstResponse.Content.ReadAsStringAsync();
        var firstResult = JsonSerializer.Deserialize<JsonElement>(firstJson, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        // Note: We don't assert firstCached value because test execution order is non-deterministic
        // Other tests may have already populated the cache

        // WHEN: Second request to same endpoint (should be cache hit)
        var secondResponse = await _client.GetAsync(endpoint);

        // THEN: Should return 200 OK
        secondResponse.StatusCode.Should().Be(HttpStatusCode.OK,
            "Second GET /api/models should also return 200 OK");

        var secondJson = await secondResponse.Content.ReadAsStringAsync();
        var secondResult = JsonSerializer.Deserialize<JsonElement>(secondJson, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        // THEN: Second request MUST be from cache (guaranteed by consecutive requests)
        var secondCached = secondResult.GetProperty("meta").GetProperty("cached").GetBoolean();
        secondCached.Should().BeTrue(
            "Second consecutive request should be a cache HIT (data retrieved from Redis)");

        // THEN: Data should be identical (cache consistency)
        var firstDataString = firstResult.GetProperty("data").ToString();
        var secondDataString = secondResult.GetProperty("data").ToString();
        secondDataString.Should().Be(firstDataString,
            "Cached data should match the original data");
    }

    /// <summary>
    /// Story 2.13 Task 4.7: Paginated requests should also support cache hit/miss reporting.
    /// </summary>
    [Fact]
    public async Task GetModelsPaginated_ConsecutiveRequests_Should_Use_Cache()
    {
        // GIVEN: Paginated endpoint
        var endpoint = "/api/models?page=1&pageSize=10";

        // WHEN: First paginated request
        var firstResponse = await _client.GetAsync(endpoint);

        // THEN: Should return 200 OK
        firstResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var firstJson = await firstResponse.Content.ReadAsStringAsync();
        var firstResult = JsonSerializer.Deserialize<JsonElement>(firstJson, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        // WHEN: Second consecutive paginated request (should hit cache)
        var secondResponse = await _client.GetAsync(endpoint);

        // THEN: Should return 200 OK
        secondResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var secondJson = await secondResponse.Content.ReadAsStringAsync();
        var secondResult = JsonSerializer.Deserialize<JsonElement>(secondJson, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        // THEN: Second request MUST be from cache
        var secondCached = secondResult.GetProperty("meta").GetProperty("cached").GetBoolean();
        secondCached.Should().BeTrue(
            "Second consecutive paginated request should be a cache HIT");

        // THEN: Pagination metadata should be consistent
        var firstPagination = firstResult.GetProperty("data").GetProperty("pagination").GetProperty("totalItems").GetInt32();
        var secondPagination = secondResult.GetProperty("data").GetProperty("pagination").GetProperty("totalItems").GetInt32();
        secondPagination.Should().Be(firstPagination,
            "Pagination metadata should be identical between requests");
    }

    /// <summary>
    /// Story 2.13 Task 4.5: Cache should be invalidated after admin model create/update/delete.
    /// After admin mutation, next GET /api/models should be cache MISS again.
    /// </summary>
    [Fact]
    public async Task GetModels_AfterAdminMutation_Should_Invalidate_Cache()
    {
        // GIVEN: Login as admin
        var loginRequest = new { username = "admin", password = "admin123" };
        await _client.PostAsJsonAsync("/api/admin/auth/login", loginRequest);

        // GIVEN: First request to cache models
        var firstResponse = await _client.GetAsync("/api/models");
        var firstJson = await firstResponse.Content.ReadAsStringAsync();
        var firstResult = JsonSerializer.Deserialize<JsonElement>(firstJson, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        // Cache should be populated (either from this request or previous tests)
        // We don't assert firstCached because test order is non-deterministic

        // WHEN: Admin creates a new model (cache invalidation should occur)
        var createRequest = new
        {
            name = $"Test Model {Guid.NewGuid()}",
            provider = "TestProvider",
            version = "1.0",
            status = "active",
            inputPricePer1M = 10.0m,
            outputPricePer1M = 20.0m,
            currency = "USD",
            capabilities = new
            {
                contextWindow = 4096,
                maxOutputTokens = 2048,
                supportsFunctionCalling = true,
                supportsVision = false,
                supportsAudioInput = false,
                supportsAudioOutput = false,
                supportsStreaming = true,
                supportsJsonMode = true
            }
        };

        var createResponse = await _client.PostAsJsonAsync("/api/admin/models", createRequest);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created,
            "Admin should be able to create models");

        // WHEN: Next GET /api/models request after admin mutation
        var afterMutationResponse = await _client.GetAsync("/api/models");

        // THEN: Should be cache MISS (cache was invalidated)
        var afterMutationJson = await afterMutationResponse.Content.ReadAsStringAsync();
        var afterMutationResult = JsonSerializer.Deserialize<JsonElement>(afterMutationJson, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        var afterMutationCached = afterMutationResult.GetProperty("meta").GetProperty("cached").GetBoolean();
        afterMutationCached.Should().BeFalse(
            "After admin model creation, cache should be invalidated and next request should be a MISS");
    }
}
