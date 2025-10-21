using FluentAssertions;
using LlmTokenPrice.Tests.E2E.Helpers;
using System.Net;
using System.Net.Http.Json;

namespace LlmTokenPrice.Tests.E2E;

/// <summary>
/// E2E tests for authorization and authentication on admin endpoints.
/// Story 2.13 Task 8: Verifies all admin endpoints require authentication.
/// </summary>
/// <remarks>
/// Uses UnauthenticatedWebApplicationFactory to test 401 responses (no auto-authentication).
/// For tests requiring authentication, creates authenticated clients using AuthHelper.
/// </remarks>
public class AuthorizationTests : IClassFixture<UnauthenticatedWebApplicationFactory>, IClassFixture<TestWebApplicationFactory>
{
    private readonly UnauthenticatedWebApplicationFactory _unauthFactory;
    private readonly TestWebApplicationFactory _authFactory;

    public AuthorizationTests(
        UnauthenticatedWebApplicationFactory unauthFactory,
        TestWebApplicationFactory authFactory)
    {
        _unauthFactory = unauthFactory;
        _authFactory = authFactory;
    }

    /// <summary>
    /// Story 2.13 Task 8.5: Verify GET /api/admin/models requires authentication
    /// </summary>
    [Fact]
    public async Task GetAdminModels_WithoutAuth_Should_Return401()
    {
        // GIVEN: Unauthenticated client (no auto-authentication)
        using var client = _unauthFactory.CreateClient();

        // WHEN: Requesting admin models without authentication
        var response = await client.GetAsync("/api/admin/models");

        // THEN: Should return 401 Unauthorized
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized,
            "GET /api/admin/models must require authentication");
    }

    /// <summary>
    /// Story 2.13 Task 8.5: Verify DELETE /api/admin/models/{id} requires authentication
    /// </summary>
    [Fact]
    public async Task DeleteModel_WithoutAuth_Should_Return401()
    {
        // GIVEN: Unauthenticated client (no auto-authentication)
        using var client = _unauthFactory.CreateClient();
        var testId = Guid.NewGuid();

        // WHEN: Attempting to delete without authentication
        var response = await client.DeleteAsync($"/api/admin/models/{testId}");

        // THEN: Should return 401 Unauthorized
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized,
            "DELETE /api/admin/models/{id} must require authentication");
    }

    /// <summary>
    /// Story 2.13 Task 8.5: Verify POST /api/admin/models requires authentication
    /// </summary>
    [Fact]
    public async Task CreateModel_WithoutAuth_Should_Return401()
    {
        // GIVEN: Unauthenticated client (no auto-authentication)
        using var client = _unauthFactory.CreateClient();

        var createRequest = new
        {
            name = "Test Model",
            provider = "TestProvider",
            version = "1.0",
            status = "active",
            inputPricePer1M = 10.0m,
            outputPricePer1M = 20.0m,
            currency = "USD",
            capabilities = new
            {
                contextWindow = 4096,
                maxOutputTokens = 2048
            }
        };

        // WHEN: Attempting to create model without authentication
        var response = await client.PostAsJsonAsync("/api/admin/models", createRequest);

        // THEN: Should return 401 Unauthorized
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized,
            "POST /api/admin/models must require authentication");
    }

    /// <summary>
    /// Story 2.13 Task 8.5: Verify GET /api/admin/benchmarks requires authentication
    /// </summary>
    [Fact]
    public async Task GetAdminBenchmarks_WithoutAuth_Should_Return401()
    {
        // GIVEN: Unauthenticated client (no auto-authentication)
        using var client = _unauthFactory.CreateClient();

        // WHEN: Requesting admin benchmarks without authentication
        var response = await client.GetAsync("/api/admin/benchmarks");

        // THEN: Should return 401 Unauthorized
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized,
            "GET /api/admin/benchmarks must require authentication");
    }

    /// <summary>
    /// Story 2.13 Task 8.5: Verify POST /api/admin/benchmarks requires authentication
    /// </summary>
    [Fact]
    public async Task CreateBenchmark_WithoutAuth_Should_Return401()
    {
        // GIVEN: Unauthenticated client (no auto-authentication)
        using var client = _unauthFactory.CreateClient();

        var createRequest = new
        {
            benchmarkName = "TestBench",
            category = "Reasoning",
            description = "Test",
            typicalRangeMin = 0m,
            typicalRangeMax = 100m
        };

        // WHEN: Attempting to create benchmark without authentication
        var response = await client.PostAsJsonAsync("/api/admin/benchmarks", createRequest);

        // THEN: Should return 401 Unauthorized
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized,
            "POST /api/admin/benchmarks must require authentication");
    }

    /// <summary>
    /// Story 2.13 Task 8.5: Verify DELETE /api/admin/benchmarks/{id} requires authentication
    /// </summary>
    [Fact]
    public async Task DeleteBenchmark_WithoutAuth_Should_Return401()
    {
        // GIVEN: Unauthenticated client (no auto-authentication)
        using var client = _unauthFactory.CreateClient();
        var testId = Guid.NewGuid();

        // WHEN: Attempting to delete without authentication
        var response = await client.DeleteAsync($"/api/admin/benchmarks/{testId}");

        // THEN: Should return 401 Unauthorized
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized,
            "DELETE /api/admin/benchmarks/{id} must require authentication");
    }

    /// <summary>
    /// Story 2.13 Task 8.5: Verify POST /api/admin/benchmarks/import-csv requires authentication
    /// </summary>
    [Fact]
    public async Task ImportBenchmarkCSV_WithoutAuth_Should_Return401()
    {
        // GIVEN: Unauthenticated client (no auto-authentication)
        using var client = _unauthFactory.CreateClient();

        // Create minimal multipart form data
        using var content = new MultipartFormDataContent();
        using var fileContent = new ByteArrayContent(new byte[] { });
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/csv");
        content.Add(fileContent, "file", "test.csv");

        // WHEN: Attempting to import CSV without authentication
        var response = await client.PostAsync("/api/admin/benchmarks/import-csv", content);

        // THEN: Should return 401 Unauthorized
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized,
            "POST /api/admin/benchmarks/import-csv must require authentication");
    }

    /// <summary>
    /// Story 2.13 Task 8.4: Verify authenticated requests work correctly with AuthHelper
    /// </summary>
    [Fact]
    public async Task AdminEndpoints_WithAuthentication_Should_Return200OrOtherValidStatus()
    {
        // GIVEN: Authenticated admin client using AuthHelper (with real JWT authentication)
        using var client = await AuthHelper.CreateAuthenticatedAdminClientAsync(_authFactory);

        // WHEN: Requesting admin endpoints with authentication
        var modelsResponse = await client.GetAsync("/api/admin/models");
        var benchmarksResponse = await client.GetAsync("/api/admin/benchmarks");

        // THEN: Should NOT return 401 (authentication successful)
        modelsResponse.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized,
            "Authenticated requests should not return 401");
        benchmarksResponse.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized,
            "Authenticated requests should not return 401");

        // THEN: Should return success status codes
        modelsResponse.IsSuccessStatusCode.Should().BeTrue(
            "GET /api/admin/models should succeed when authenticated");
        benchmarksResponse.IsSuccessStatusCode.Should().BeTrue(
            "GET /api/admin/benchmarks should succeed when authenticated");
    }

    /// <summary>
    /// Story 2.13 Task 8.4: Verify AuthHelper.VerifyRequiresAuthenticationAsync utility
    /// </summary>
    [Fact]
    public async Task AuthHelperVerify_Should_CorrectlyDetectUnauthorized()
    {
        // GIVEN: Unauthenticated client (no auto-authentication)
        using var client = _unauthFactory.CreateClient();

        // WHEN: Using AuthHelper to verify endpoints require authentication
        var modelsRequiresAuth = await AuthHelper.VerifyRequiresAuthenticationAsync(
            client, HttpMethod.Get, "/api/admin/models");

        var benchmarksRequiresAuth = await AuthHelper.VerifyRequiresAuthenticationAsync(
            client, HttpMethod.Get, "/api/admin/benchmarks");

        // THEN: Both should return true (endpoint requires authentication)
        modelsRequiresAuth.Should().BeTrue(
            "AuthHelper should detect that /api/admin/models requires authentication");
        benchmarksRequiresAuth.Should().BeTrue(
            "AuthHelper should detect that /api/admin/benchmarks requires authentication");
    }

    /// <summary>
    /// Verify public endpoints DO NOT require authentication
    /// </summary>
    [Fact]
    public async Task PublicEndpoints_WithoutAuth_Should_Return200()
    {
        // GIVEN: Unauthenticated client (no auto-authentication)
        using var client = _unauthFactory.CreateClient();

        // WHEN: Requesting public endpoints without authentication
        var modelsResponse = await client.GetAsync("/api/models");

        // THEN: Should return 200 OK (public endpoints don't require auth)
        modelsResponse.StatusCode.Should().Be(HttpStatusCode.OK,
            "Public endpoints should work without authentication");
    }
}
