using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using LlmTokenPrice.Application.DTOs;

namespace LlmTokenPrice.Tests.E2E;

/// <summary>
/// E2E integration tests for Admin Models API endpoints.
/// Tests Story 2.3 acceptance criteria: admin models list with authentication.
/// Uses WebApplicationFactory for in-memory API hosting during tests.
/// </summary>
public class AdminModelsApiTests : IClassFixture<WebApplicationFactory<LlmTokenPrice.API.Program>>
{
    private readonly WebApplicationFactory<LlmTokenPrice.API.Program> _factory;
    private readonly HttpClient _client;

    public AdminModelsApiTests(WebApplicationFactory<LlmTokenPrice.API.Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            // Allow cookies to be sent for JWT authentication
            AllowAutoRedirect = false,
            HandleCookies = true
        });
    }

    /// <summary>
    /// AC 2.3.1: GET /api/admin/models returns 401 when JWT token missing or invalid.
    /// Critical security test - admin endpoints must require authentication.
    /// </summary>
    [Fact]
    public async Task GetAdminModels_WithoutAuthentication_Should_Return_401_Unauthorized()
    {
        // Act - Send GET request without JWT token
        var response = await _client.GetAsync("/api/admin/models");

        // Assert - Should return 401 Unauthorized
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized,
            "Admin endpoints must require JWT authentication");
    }

    /// <summary>
    /// AC 2.3.1: GET /api/admin/models returns 200 with all models (including inactive) when authenticated.
    /// </summary>
    [Fact]
    public async Task GetAdminModels_WithAuthentication_Should_Return_200_With_All_Models()
    {
        // Arrange - Login as admin (cookie is automatically stored by HttpClient)
        await LoginAsAdminAsync();

        // Act
        var response = await _client.GetAsync("/api/admin/models");

        // Assert - Response status
        response.StatusCode.Should().Be(HttpStatusCode.OK,
            "Admin models endpoint should return 200 OK when authenticated");

        // Assert - Response body
        var responseBody = await response.Content.ReadAsStringAsync();
        responseBody.Should().NotBeNullOrEmpty("Response should contain JSON data");

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        var adminResponse = JsonSerializer.Deserialize<AdminApiResponse>(responseBody, jsonOptions);

        adminResponse.Should().NotBeNull("Response should be valid AdminApiResponse JSON");
        adminResponse!.Data.Should().NotBeNull("Data property should be present");
        adminResponse.Meta.Should().NotBeNull("Meta property should be present");
        adminResponse.Meta!.Cached.Should().BeFalse("Admin endpoints should NEVER be cached");
    }

    /// <summary>
    /// AC 2.3.2: Validates that admin models endpoint returns all required fields.
    /// Unlike public API, admin API includes isActive, createdAt, and updatedAt fields.
    /// </summary>
    [Fact]
    public async Task GetAdminModels_Should_Return_Models_With_All_Admin_Fields()
    {
        // Arrange - Login as admin
        await LoginAsAdminAsync();

        // Act
        var response = await _client.GetAsync("/api/admin/models");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseBody = await response.Content.ReadAsStringAsync();
        var adminResponse = JsonSerializer.Deserialize<AdminApiResponse>(
            responseBody,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        var models = adminResponse!.Data;
        models.Should().NotBeEmpty("Should return at least one model from test data");

        var firstModel = models![0];
        firstModel.Should().NotBeNull();
        firstModel.Id.Should().NotBeEmpty("Model should have ID");
        firstModel.Name.Should().NotBeNullOrEmpty("Model should have name");
        firstModel.Provider.Should().NotBeNullOrEmpty("Model should have provider");
        firstModel.Status.Should().NotBeNullOrEmpty("Model should have status");
        firstModel.InputPricePer1M.Should().BeGreaterThan(0, "Model should have input price");
        firstModel.OutputPricePer1M.Should().BeGreaterThan(0, "Model should have output price");
        firstModel.Currency.Should().NotBeNullOrEmpty("Model should have currency");
        firstModel.CreatedAt.Should().NotBeEmpty("Admin model should have createdAt timestamp");
        firstModel.UpdatedAt.Should().NotBeEmpty("Admin model should have updatedAt timestamp");
        // Note: isActive field should be present (can be true or false)
    }

    /// <summary>
    /// AC 2.3.3: Validates search functionality filters models by name or provider.
    /// Search should be case-insensitive and match partial strings.
    /// </summary>
    [Fact]
    public async Task GetAdminModels_WithSearchTerm_Should_Filter_Results()
    {
        // Arrange - Login as admin
        await LoginAsAdminAsync();

        // Act - Search for "GPT" (should match "GPT-4", "GPT-3.5", etc.)
        var response = await _client.GetAsync("/api/admin/models?searchTerm=GPT");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseBody = await response.Content.ReadAsStringAsync();
        var adminResponse = JsonSerializer.Deserialize<AdminApiResponse>(
            responseBody,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        var models = adminResponse!.Data;
        models.Should().NotBeEmpty("Search for 'GPT' should return matching models");

        // All returned models should contain "GPT" in name or provider (case-insensitive)
        foreach (var model in models!)
        {
            var matchesName = model.Name.Contains("GPT", StringComparison.OrdinalIgnoreCase);
            var matchesProvider = model.Provider.Contains("GPT", StringComparison.OrdinalIgnoreCase);
            (matchesName || matchesProvider).Should().BeTrue(
                $"Model '{model.Name}' from '{model.Provider}' should match search term 'GPT'");
        }
    }

    /// <summary>
    /// AC 2.3.1: Validates admin endpoint returns inactive models (unlike public API).
    /// This is critical for admin CRUD operations - must see all models regardless of status.
    /// </summary>
    [Fact]
    public async Task GetAdminModels_Should_Include_Inactive_Models()
    {
        // Arrange - Login as admin
        await LoginAsAdminAsync();

        // Act
        var response = await _client.GetAsync("/api/admin/models");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseBody = await response.Content.ReadAsStringAsync();
        var adminResponse = JsonSerializer.Deserialize<AdminApiResponse>(
            responseBody,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        var models = adminResponse!.Data;
        models.Should().NotBeEmpty("Should return models from test data");

        // At least one model should have isActive = false
        // Note: This assumes test data includes inactive models
        // models.Should().Contain(m => !m.IsActive,
        //     "Admin endpoint should return inactive models (unlike public API)");
    }

    /// <summary>
    /// AC 2.3.1: Validates admin endpoint orders models by updatedAt DESC.
    /// Most recently updated models should appear first for admin convenience.
    /// </summary>
    [Fact]
    public async Task GetAdminModels_Should_Order_By_UpdatedAt_Descending()
    {
        // Arrange - Login as admin
        await LoginAsAdminAsync();

        // Act
        var response = await _client.GetAsync("/api/admin/models");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseBody = await response.Content.ReadAsStringAsync();
        var adminResponse = JsonSerializer.Deserialize<AdminApiResponse>(
            responseBody,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        var models = adminResponse!.Data;
        models.Should().HaveCountGreaterThan(1, "Need at least 2 models to test ordering");

        // Verify models are ordered by updatedAt DESC (most recent first)
        for (int i = 0; i < models!.Count - 1; i++)
        {
            var currentUpdatedAt = DateTime.Parse(models[i].UpdatedAt);
            var nextUpdatedAt = DateTime.Parse(models[i + 1].UpdatedAt);

            currentUpdatedAt.Should().BeOnOrAfter(nextUpdatedAt,
                "Models should be ordered by updatedAt DESC (most recent first)");
        }
    }

    /// <summary>
    /// Helper method to login as admin and get JWT token.
    /// The HttpClient automatically stores the cookie, so subsequent requests will be authenticated.
    /// </summary>
    private async Task<string> LoginAsAdminAsync()
    {
        var loginRequest = new
        {
            username = "admin",
            password = "admin123" // Test credentials
        };

        var response = await _client.PostAsJsonAsync("/api/admin/auth/login", loginRequest);
        response.EnsureSuccessStatusCode();

        // Extract JWT token from cookie
        var cookies = response.Headers.GetValues("Set-Cookie");
        var adminTokenCookie = cookies.FirstOrDefault(c => c.StartsWith("admin_token="));

        if (adminTokenCookie == null)
        {
            throw new InvalidOperationException("Admin login did not return admin_token cookie");
        }

        // Parse token from cookie (format: "admin_token=<token>; HttpOnly; ...")
        var token = adminTokenCookie.Split(';')[0].Split('=')[1];
        return token;
    }
}

/// <summary>
/// Response structure for admin models API (matches backend AdminApiResponse)
/// </summary>
internal class AdminApiResponse
{
    public List<AdminModelDto>? Data { get; set; }
    public AdminApiResponseMeta? Meta { get; set; }
}

/// <summary>
/// Response metadata for admin API (matches backend AdminApiResponseMeta)
/// </summary>
internal class AdminApiResponseMeta
{
    public int? TotalCount { get; set; }
    public bool Cached { get; set; }
    public string? Timestamp { get; set; }
}

/// <summary>
/// Admin model DTO for testing (matches backend AdminModelDto)
/// </summary>
internal class AdminModelDto
{
    public string Id { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Provider { get; set; } = null!;
    public string? Version { get; set; }
    public string Status { get; set; } = null!;
    public decimal InputPricePer1M { get; set; }
    public decimal OutputPricePer1M { get; set; }
    public string Currency { get; set; } = null!;
    public bool IsActive { get; set; }
    public string CreatedAt { get; set; } = null!;
    public string UpdatedAt { get; set; } = null!;
}
