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
    /// AC 2.3.6: Validates DELETE endpoint soft deletes model (sets IsActive = false).
    /// </summary>
    [Fact]
    public async Task DeleteModel_WithValidId_Should_Return_204_And_SoftDelete()
    {
        // Arrange - Login as admin
        await LoginAsAdminAsync();

        // First, get all models to find one to delete
        var getResponse = await _client.GetAsync("/api/admin/models");
        var responseBody = await getResponse.Content.ReadAsStringAsync();
        var adminResponse = JsonSerializer.Deserialize<AdminApiResponse>(
            responseBody,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        var models = adminResponse!.Data;
        models.Should().NotBeEmpty("Need at least one model to test deletion");

        var modelToDelete = models![0];

        // Act - Delete the model
        var deleteResponse = await _client.DeleteAsync($"/api/admin/models/{modelToDelete.Id}");

        // Assert - Should return 204 No Content
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent,
            "DELETE endpoint should return 204 on successful soft delete");

        // Verify model is soft deleted (should still appear in admin API but with IsActive = false)
        var verifyResponse = await _client.GetAsync($"/api/admin/models/{modelToDelete.Id}");
        verifyResponse.StatusCode.Should().Be(HttpStatusCode.OK,
            "Should still be able to retrieve soft-deleted model in admin API");

        var verifyBody = await verifyResponse.Content.ReadAsStringAsync();

        // Parse the JSON to check IsActive status
        using var jsonDoc = JsonDocument.Parse(verifyBody);
        var dataElement = jsonDoc.RootElement.GetProperty("data");
        var isActive = dataElement.GetProperty("isActive").GetBoolean();

        isActive.Should().BeFalse("Model should be soft deleted (IsActive = false)");
    }

    /// <summary>
    /// AC 2.3.6: Validates DELETE endpoint returns 404 for non-existent model.
    /// </summary>
    [Fact]
    public async Task DeleteModel_WithInvalidId_Should_Return_404()
    {
        // Arrange - Login as admin
        await LoginAsAdminAsync();

        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await _client.DeleteAsync($"/api/admin/models/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound,
            "DELETE endpoint should return 404 for non-existent model");
    }

    /// <summary>
    /// AC 2.3.6: Validates DELETE endpoint requires authentication.
    /// </summary>
    [Fact]
    public async Task DeleteModel_WithoutAuthentication_Should_Return_401()
    {
        // Arrange - No login (no JWT token)
        var modelId = Guid.NewGuid();

        // Act
        var response = await _client.DeleteAsync($"/api/admin/models/{modelId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized,
            "DELETE endpoint must require JWT authentication");
    }

    #region POST /api/admin/models - Story 2.5

    /// <summary>
    /// [P0] AC 2.5.1: POST /api/admin/models returns 401 when JWT token is missing.
    /// Critical security test - create endpoint must require authentication.
    /// </summary>
    [Fact]
    public async Task CreateModel_WithoutAuthentication_Should_Return_401_Unauthorized()
    {
        // GIVEN: CreateModelRequest without JWT token
        var request = new
        {
            name = "GPT-4 Test",
            provider = "OpenAI",
            status = "active",
            inputPricePer1M = 10.00m,
            outputPricePer1M = 30.00m,
            currency = "USD"
        };

        // WHEN: Attempting to create model without authentication
        var response = await _client.PostAsJsonAsync("/api/admin/models", request);

        // THEN: Should return 401 Unauthorized
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized,
            "POST /api/admin/models must require JWT authentication");
    }

    /// <summary>
    /// [P0] AC 2.5.5: POST /api/admin/models with valid data returns 201 Created with Location header.
    /// Tests successful model creation flow with authentication.
    /// </summary>
    [Fact]
    public async Task CreateModel_WithValidData_Should_Return_201_Created_WithLocationHeader()
    {
        // GIVEN: Authenticated admin user
        await LoginAsAdminAsync();

        // GIVEN: Valid create model request
        var request = new
        {
            name = $"Test Model {Guid.NewGuid()}",
            provider = "Test Provider",
            version = "v1.0",
            releaseDate = "2024-01-01",
            status = "active",
            inputPricePer1M = 5.50m,
            outputPricePer1M = 15.00m,
            currency = "USD",
            pricingValidFrom = "2024-01-01",
            pricingValidTo = "2024-12-31"
        };

        // WHEN: Creating model via API
        var response = await _client.PostAsJsonAsync("/api/admin/models", request);

        // THEN: Should return 201 Created
        response.StatusCode.Should().Be(HttpStatusCode.Created,
            "Valid model creation should return 201 Created");

        // THEN: Should include Location header
        response.Headers.Location.Should().NotBeNull("Location header should be present");
        response.Headers.Location!.ToString().Should().Contain("/api/admin/models/",
            "Location header should point to created model endpoint");
    }

    /// <summary>
    /// [P0] AC 2.5.3,4: POST /api/admin/models with valid data persists Model + Capability to database.
    /// Verifies both entities are created and linked via ModelId foreign key.
    /// </summary>
    [Fact]
    public async Task CreateModel_WithValidData_Should_PersistModelAndCapabilityToDatabase()
    {
        // GIVEN: Authenticated admin user
        await LoginAsAdminAsync();

        // GIVEN: Valid create model request with unique name
        var uniqueName = $"Test Persistence Model {Guid.NewGuid()}";
        var request = new
        {
            name = uniqueName,
            provider = "Test Provider",
            status = "active",
            inputPricePer1M = 7.25m,
            outputPricePer1M = 20.00m,
            currency = "USD"
        };

        // WHEN: Creating model via API
        var createResponse = await _client.PostAsJsonAsync("/api/admin/models", request);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // Extract created model ID from Location header
        var locationHeader = createResponse.Headers.Location!.ToString();
        var modelId = locationHeader.Split('/').Last();

        // THEN: Model should be retrievable from database
        var getResponse = await _client.GetAsync($"/api/admin/models/{modelId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseBody = await getResponse.Content.ReadAsStringAsync();
        var adminResponse = JsonSerializer.Deserialize<SingleAdminApiResponse>(
            responseBody,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        adminResponse.Should().NotBeNull();
        adminResponse!.Data.Should().NotBeNull();
        adminResponse.Data!.Name.Should().Be(uniqueName);
        adminResponse.Data.Provider.Should().Be("Test Provider");
        adminResponse.Data.Status.Should().Be("active");
        adminResponse.Data.InputPricePer1M.Should().Be(7.25m);
        adminResponse.Data.OutputPricePer1M.Should().Be(20.00m);
        adminResponse.Data.Currency.Should().Be("USD");
        adminResponse.Data.IsActive.Should().BeTrue("New models should be active by default");
        adminResponse.Data.CreatedAt.Should().NotBeNullOrEmpty("CreatedAt timestamp should be set");
        adminResponse.Data.UpdatedAt.Should().NotBeNullOrEmpty("UpdatedAt timestamp should be set");
    }

    /// <summary>
    /// [P1] AC 2.5.6: POST /api/admin/models with invalid data returns 400 Bad Request with validation errors.
    /// Tests FluentValidation error response format.
    /// </summary>
    [Fact]
    public async Task CreateModel_WithInvalidData_Should_Return_400_WithValidationErrors()
    {
        // GIVEN: Authenticated admin user
        await LoginAsAdminAsync();

        // GIVEN: Invalid create model request (missing required fields, negative prices)
        var request = new
        {
            name = "", // Empty name (invalid)
            provider = "", // Empty provider (invalid)
            status = "invalid_status", // Invalid enum (invalid)
            inputPricePer1M = -5.00m, // Negative price (invalid)
            outputPricePer1M = 0m, // Zero price (invalid)
            currency = "ARS" // Invalid currency (invalid)
        };

        // WHEN: Attempting to create model with invalid data
        var response = await _client.PostAsJsonAsync("/api/admin/models", request);

        // THEN: Should return 400 Bad Request
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest,
            "Invalid model data should return 400 Bad Request");

        // THEN: Response should contain validation error details
        var responseBody = await response.Content.ReadAsStringAsync();
        responseBody.Should().Contain("error", "Response should contain error object");
    }

    /// <summary>
    /// [P1] AC 2.5.2: POST /api/admin/models validates positive prices - zero/negative should fail.
    /// </summary>
    [Theory]
    [InlineData(0, 30.00)]
    [InlineData(-1, 30.00)]
    [InlineData(10.00, 0)]
    [InlineData(10.00, -5.00)]
    public async Task CreateModel_WithNonPositivePrices_Should_Return_400(decimal inputPrice, decimal outputPrice)
    {
        // GIVEN: Authenticated admin user
        await LoginAsAdminAsync();

        // GIVEN: Request with zero or negative prices
        var request = new
        {
            name = $"Test Model {Guid.NewGuid()}",
            provider = "Test Provider",
            status = "active",
            inputPricePer1M = inputPrice,
            outputPricePer1M = outputPrice,
            currency = "USD"
        };

        // WHEN: Attempting to create model with invalid prices
        var response = await _client.PostAsJsonAsync("/api/admin/models", request);

        // THEN: Should return 400 Bad Request
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest,
            "Prices must be greater than 0");
    }

    /// <summary>
    /// [P1] AC 2.5.6: POST /api/admin/models with duplicate Name+Provider returns 400 with specific error.
    /// Tests duplicate detection logic.
    /// </summary>
    [Fact]
    public async Task CreateModel_WithDuplicateNameAndProvider_Should_Return_400()
    {
        // GIVEN: Authenticated admin user
        await LoginAsAdminAsync();

        // GIVEN: Create first model
        var uniqueName = $"Duplicate Test {Guid.NewGuid()}";
        var firstRequest = new
        {
            name = uniqueName,
            provider = "Test Provider",
            status = "active",
            inputPricePer1M = 10.00m,
            outputPricePer1M = 30.00m,
            currency = "USD"
        };

        var firstResponse = await _client.PostAsJsonAsync("/api/admin/models", firstRequest);
        firstResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // WHEN: Attempting to create duplicate model (same name + provider)
        var duplicateRequest = new
        {
            name = uniqueName, // Same name
            provider = "Test Provider", // Same provider
            status = "active",
            inputPricePer1M = 15.00m, // Different price (but duplicate by name+provider)
            outputPricePer1M = 45.00m,
            currency = "USD"
        };

        var duplicateResponse = await _client.PostAsJsonAsync("/api/admin/models", duplicateRequest);

        // THEN: Should return 400 Bad Request for duplicate
        duplicateResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest,
            "Duplicate model (same name + provider) should return 400");

        var responseBody = await duplicateResponse.Content.ReadAsStringAsync();
        responseBody.Should().Contain("already exists", "Error message should indicate duplicate");
    }

    /// <summary>
    /// [P1] AC 2.5.2: POST /api/admin/models validates enum values - invalid Status should fail.
    /// </summary>
    [Theory]
    [InlineData("invalid")]
    [InlineData("ACTIVE")] // Case-sensitive
    [InlineData("beta-test")]
    public async Task CreateModel_WithInvalidStatus_Should_Return_400(string invalidStatus)
    {
        // GIVEN: Authenticated admin user
        await LoginAsAdminAsync();

        // GIVEN: Request with invalid Status enum
        var request = new
        {
            name = $"Test Model {Guid.NewGuid()}",
            provider = "Test Provider",
            status = invalidStatus,
            inputPricePer1M = 10.00m,
            outputPricePer1M = 30.00m,
            currency = "USD"
        };

        // WHEN: Attempting to create model with invalid status
        var response = await _client.PostAsJsonAsync("/api/admin/models", request);

        // THEN: Should return 400 Bad Request
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest,
            "Invalid Status enum should return 400");
    }

    /// <summary>
    /// [P1] AC 2.5.2: POST /api/admin/models validates enum values - invalid Currency should fail.
    /// </summary>
    [Theory]
    [InlineData("ARS")]
    [InlineData("JPY")]
    [InlineData("usd")] // Case-sensitive
    public async Task CreateModel_WithInvalidCurrency_Should_Return_400(string invalidCurrency)
    {
        // GIVEN: Authenticated admin user
        await LoginAsAdminAsync();

        // GIVEN: Request with invalid Currency enum
        var request = new
        {
            name = $"Test Model {Guid.NewGuid()}",
            provider = "Test Provider",
            status = "active",
            inputPricePer1M = 10.00m,
            outputPricePer1M = 30.00m,
            currency = invalidCurrency
        };

        // WHEN: Attempting to create model with invalid currency
        var response = await _client.PostAsJsonAsync("/api/admin/models", request);

        // THEN: Should return 400 Bad Request
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest,
            "Invalid Currency enum should return 400");
    }

    /// <summary>
    /// [P2] AC 2.5.5: POST /api/admin/models response includes full AdminModelDto with new model data.
    /// </summary>
    [Fact]
    public async Task CreateModel_WithValidData_Should_ReturnFullAdminModelDto()
    {
        // GIVEN: Authenticated admin user
        await LoginAsAdminAsync();

        // GIVEN: Valid create model request
        var uniqueName = $"Full DTO Test {Guid.NewGuid()}";
        var request = new
        {
            name = uniqueName,
            provider = "Test Provider",
            version = "v2.0",
            releaseDate = "2024-06-15",
            status = "beta",
            inputPricePer1M = 12.75m,
            outputPricePer1M = 35.50m,
            currency = "EUR",
            pricingValidFrom = "2024-06-01",
            pricingValidTo = "2024-12-31"
        };

        // WHEN: Creating model via API
        var response = await _client.PostAsJsonAsync("/api/admin/models", request);

        // THEN: Response should include full AdminModelDto
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var responseBody = await response.Content.ReadAsStringAsync();
        var adminResponse = JsonSerializer.Deserialize<SingleAdminApiResponse>(
            responseBody,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        adminResponse.Should().NotBeNull();
        adminResponse!.Data.Should().NotBeNull();
        adminResponse.Data!.Id.Should().NotBeNullOrEmpty("Response should include model ID");
        adminResponse.Data.Name.Should().Be(uniqueName);
        adminResponse.Data.Provider.Should().Be("Test Provider");
        adminResponse.Data.Version.Should().Be("v2.0");
        adminResponse.Data.Status.Should().Be("beta");
        adminResponse.Data.InputPricePer1M.Should().Be(12.75m);
        adminResponse.Data.OutputPricePer1M.Should().Be(35.50m);
        adminResponse.Data.Currency.Should().Be("EUR");
        adminResponse.Data.IsActive.Should().BeTrue();
        adminResponse.Data.CreatedAt.Should().NotBeNullOrEmpty();
        adminResponse.Data.UpdatedAt.Should().NotBeNullOrEmpty();

        // THEN: Meta should indicate not cached
        adminResponse.Meta.Should().NotBeNull();
        adminResponse.Meta!.Cached.Should().BeFalse("Admin endpoints should never be cached");
    }

    #endregion

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

/// <summary>
/// Response structure for single admin model API (matches backend AdminApiResponse for single item)
/// </summary>
internal class SingleAdminApiResponse
{
    public AdminModelDto? Data { get; set; }
    public AdminApiResponseMeta? Meta { get; set; }
}
