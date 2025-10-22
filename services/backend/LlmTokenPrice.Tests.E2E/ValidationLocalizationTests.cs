using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using LlmTokenPrice.Application.DTOs;
using LlmTokenPrice.Tests.E2E.Helpers;
using Xunit;

namespace LlmTokenPrice.Tests.E2E;

/// <summary>
/// E2E tests for validation message localization (Story 2.13 Task 13).
/// Verifies that FluentValidation error messages are returned in the user's language
/// based on the Accept-Language HTTP header.
/// </summary>
[Collection("E2E")]
public class ValidationLocalizationTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public ValidationLocalizationTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    #region English (Default) Language Tests

    [Fact]
    public async Task CreateModel_WithInvalidData_ShouldReturnEnglishErrorMessages_WhenNoAcceptLanguageHeader()
    {
        // Given: Authenticated admin client with invalid model data, no Accept-Language header
        var authenticatedClient = await AuthHelper.CreateAuthenticatedAdminClientAsync(_factory);
        var invalidModel = new CreateModelRequest
        {
            Name = "", // Required field empty
            Provider = "",
            InputPricePer1M = -1, // Must be > 0
            OutputPricePer1M = 0.0000001m, // Too many decimal places
            Currency = "INVALID", // Must be USD, EUR, or GBP
            Status = "unknown", // Must be active, deprecated, or beta
            Capabilities = new CreateCapabilityRequest
            {
                ContextWindow = 500 // Below minimum 1,000
            }
        };

        // When: POST request to create model
        var response = await authenticatedClient.PostAsJsonAsync("/api/admin/models", invalidModel);

        // Then: Should return 400 Bad Request with English error messages
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var responseBody = await response.Content.ReadAsStringAsync();

        responseBody.Should().Contain("Model name is required");
        responseBody.Should().Contain("Provider is required");
        responseBody.Should().Contain("Input price must be greater than 0");
        responseBody.Should().Contain("Output price can have maximum 6 decimal places");
        responseBody.Should().Contain("Currency must be one of:");
        responseBody.Should().Contain("Status must be one of:");
        responseBody.Should().Contain("Context window must be at least");
    }

    [Fact]
    public async Task CreateModel_WithInvalidData_ShouldReturnEnglishErrorMessages_WhenAcceptLanguageIsEnglish()
    {
        // Given: Authenticated admin client with invalid model data, Accept-Language: en
        var authenticatedClient = await AuthHelper.CreateAuthenticatedAdminClientAsync(_factory);
        authenticatedClient.DefaultRequestHeaders.AcceptLanguage.Clear();
        authenticatedClient.DefaultRequestHeaders.AcceptLanguage.Add(new StringWithQualityHeaderValue("en"));

        var invalidModel = new CreateModelRequest
        {
            Name = "",
            Provider = "",
            InputPricePer1M = -1,
            OutputPricePer1M = -1,
            Currency = "",
            Status = "",
            Capabilities = new CreateCapabilityRequest { ContextWindow = 100 }
        };

        // When: POST request to create model
        var response = await authenticatedClient.PostAsJsonAsync("/api/admin/models", invalidModel);

        // Then: Should return 400 Bad Request with English error messages
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var responseBody = await response.Content.ReadAsStringAsync();

        responseBody.Should().Contain("Model name is required");
        responseBody.Should().Contain("Provider is required");
        responseBody.Should().Contain("Input price must be greater than 0");
        responseBody.Should().Contain("Output price must be greater than 0");
        responseBody.Should().Contain("Currency is required");
        responseBody.Should().Contain("Status is required");
    }

    #endregion

    #region Spanish Language Tests

    [Fact]
    public async Task CreateModel_WithInvalidData_ShouldReturnSpanishErrorMessages_WhenAcceptLanguageIsSpanish()
    {
        // Given: Authenticated admin client with invalid model data, Accept-Language: es
        var authenticatedClient = await AuthHelper.CreateAuthenticatedAdminClientAsync(_factory);
        authenticatedClient.DefaultRequestHeaders.AcceptLanguage.Clear();
        authenticatedClient.DefaultRequestHeaders.AcceptLanguage.Add(new StringWithQualityHeaderValue("es"));

        var invalidModel = new CreateModelRequest
        {
            Name = "", // "El nombre del modelo es obligatorio"
            Provider = "", // "El proveedor es obligatorio"
            InputPricePer1M = -1, // "El precio de entrada debe ser mayor que 0"
            OutputPricePer1M = 0.0000001m, // "El precio de salida puede tener un máximo de 6 decimales"
            Currency = "INVALID", // "La moneda debe ser una de:"
            Status = "unknown", // "El estado debe ser uno de:"
            Capabilities = new CreateCapabilityRequest
            {
                ContextWindow = 500 // "La ventana de contexto debe ser de al menos"
            }
        };

        // When: POST request to create model
        var response = await authenticatedClient.PostAsJsonAsync("/api/admin/models", invalidModel);

        // Then: Should return 400 Bad Request with Spanish error messages
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var responseBody = await response.Content.ReadAsStringAsync();

        responseBody.Should().Contain("El nombre del modelo es obligatorio");
        responseBody.Should().Contain("El proveedor es obligatorio");
        responseBody.Should().Contain("El precio de entrada debe ser mayor que 0");
        responseBody.Should().Contain("El precio de salida puede tener un máximo de 6 decimales");
        responseBody.Should().Contain("La moneda debe ser una de:");
        responseBody.Should().Contain("El estado debe ser uno de:");
        responseBody.Should().Contain("La ventana de contexto debe ser de al menos");
    }

    [Fact]
    public async Task CreateBenchmark_WithInvalidData_ShouldReturnSpanishErrorMessages_WhenAcceptLanguageIsSpanish()
    {
        // Given: Authenticated admin client with invalid benchmark data, Accept-Language: es
        var authenticatedClient = await AuthHelper.CreateAuthenticatedAdminClientAsync(_factory);
        authenticatedClient.DefaultRequestHeaders.AcceptLanguage.Clear();
        authenticatedClient.DefaultRequestHeaders.AcceptLanguage.Add(new StringWithQualityHeaderValue("es"));

        var invalidBenchmark = new CreateBenchmarkRequest
        {
            BenchmarkName = "", // "El nombre del benchmark es obligatorio"
            FullName = "", // "El nombre completo es obligatorio"
            Description = new string('A', 1001), // "La descripción no puede exceder 1000 caracteres"
            Category = "Invalid", // "La categoría debe ser una de:"
            Interpretation = "Invalid", // "La interpretación debe ser una de:"
            TypicalRangeMin = -100, // Invalid value to trigger validation
            TypicalRangeMax = -50, // Invalid: max less than min
            WeightInQaps = 2.0m // Invalid: exceeds 1.00 maximum
        };

        // When: POST request to create benchmark
        var response = await authenticatedClient.PostAsJsonAsync("/api/admin/benchmarks", invalidBenchmark);

        // Then: Should return 400 Bad Request with Spanish error messages
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var responseBody = await response.Content.ReadAsStringAsync();

        responseBody.Should().Contain("El nombre del benchmark es obligatorio");
        responseBody.Should().Contain("El nombre completo es obligatorio");
        responseBody.Should().Contain("La descripción no puede exceder 1000 caracteres");
        responseBody.Should().Contain("La categoría");
        responseBody.Should().Contain("La interpretación");
        responseBody.Should().Contain("El peso QAPS debe estar entre 0.00 y 1.00");
    }

    [Fact]
    public async Task UpdateBenchmark_WithInvalidWeightInQaps_ShouldReturnSpanishErrorMessage()
    {
        // Given: Authenticated client, existing benchmark, invalid update data, Accept-Language: es
        var authenticatedClient = await AuthHelper.CreateAuthenticatedAdminClientAsync(_factory);

        // First create a valid benchmark
        var createRequest = new CreateBenchmarkRequest
        {
            BenchmarkName = $"Test_Localization_{Guid.NewGuid():N}",
            FullName = "Test Localization Benchmark",
            Description = "Testing Spanish validation messages",
            Category = "Reasoning",
            Interpretation = "HigherBetter",
            TypicalRangeMin = 0,
            TypicalRangeMax = 100,
            WeightInQaps = 0.30m
        };
        var createResponse = await authenticatedClient.PostAsJsonAsync("/api/admin/benchmarks", createRequest);
        createResponse.EnsureSuccessStatusCode();
        var createdBenchmark = await createResponse.Content.ReadFromJsonAsync<BenchmarkResponseDto>();

        // Now update with invalid weight, Accept-Language: es
        authenticatedClient.DefaultRequestHeaders.AcceptLanguage.Clear();
        authenticatedClient.DefaultRequestHeaders.AcceptLanguage.Add(new StringWithQualityHeaderValue("es"));

        var invalidUpdate = new UpdateBenchmarkRequest
        {
            FullName = "Updated Name",
            Description = "Updated Description",
            Category = "Reasoning",
            Interpretation = "HigherBetter",
            TypicalRangeMin = 0,
            TypicalRangeMax = 50,
            WeightInQaps = 1.5m // Invalid: > 1.00 - "El peso QAPS debe estar entre 0.00 y 1.00"
        };

        // When: PUT request to update benchmark
        var response = await authenticatedClient.PutAsJsonAsync(
            $"/api/admin/benchmarks/{createdBenchmark!.Id}",
            invalidUpdate);

        // Then: Should return 400 Bad Request with Spanish error message
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var responseBody = await response.Content.ReadAsStringAsync();

        responseBody.Should().Contain("El peso QAPS debe estar entre 0.00 y 1.00");
    }

    #endregion

    #region Multiple Language Priority Tests

    [Fact]
    public async Task CreateModel_WithInvalidData_ShouldReturnSpanishErrorMessages_WhenAcceptLanguageHasMultipleLanguages()
    {
        // Given: Authenticated client with Accept-Language: es, en;q=0.9 (Spanish preferred)
        var authenticatedClient = await AuthHelper.CreateAuthenticatedAdminClientAsync(_factory);
        authenticatedClient.DefaultRequestHeaders.AcceptLanguage.Clear();
        authenticatedClient.DefaultRequestHeaders.AcceptLanguage.Add(new StringWithQualityHeaderValue("es", 1.0));
        authenticatedClient.DefaultRequestHeaders.AcceptLanguage.Add(new StringWithQualityHeaderValue("en", 0.9));

        var invalidModel = new CreateModelRequest
        {
            Name = "",
            Provider = "",
            InputPricePer1M = -1,
            OutputPricePer1M = -1,
            Currency = "",
            Status = "",
            Capabilities = new CreateCapabilityRequest { ContextWindow = 100 }
        };

        // When: POST request to create model
        var response = await authenticatedClient.PostAsJsonAsync("/api/admin/models", invalidModel);

        // Then: Should return 400 with Spanish messages (highest priority language)
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var responseBody = await response.Content.ReadAsStringAsync();

        responseBody.Should().Contain("El nombre del modelo es obligatorio");
        responseBody.Should().Contain("El proveedor es obligatorio");
    }

    [Fact]
    public async Task CreateModel_WithInvalidData_ShouldReturnEnglishErrorMessages_WhenAcceptLanguageIsUnsupported()
    {
        // Given: Authenticated client with unsupported language (French), should fallback to English
        var authenticatedClient = await AuthHelper.CreateAuthenticatedAdminClientAsync(_factory);
        authenticatedClient.DefaultRequestHeaders.AcceptLanguage.Clear();
        authenticatedClient.DefaultRequestHeaders.AcceptLanguage.Add(new StringWithQualityHeaderValue("fr"));

        var invalidModel = new CreateModelRequest
        {
            Name = "",
            Provider = "",
            InputPricePer1M = -1,
            OutputPricePer1M = -1,
            Currency = "",
            Status = "",
            Capabilities = new CreateCapabilityRequest { ContextWindow = 100 }
        };

        // When: POST request to create model
        var response = await authenticatedClient.PostAsJsonAsync("/api/admin/models", invalidModel);

        // Then: Should return 400 with English messages (default fallback)
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var responseBody = await response.Content.ReadAsStringAsync();

        responseBody.Should().Contain("Model name is required");
        responseBody.Should().Contain("Provider is required");
    }

    #endregion

    #region Parameterized Message Tests

    [Fact]
    public async Task CreateModel_WithTooLongName_ShouldReturnLocalizedMaxLengthMessage()
    {
        // Given: Authenticated clients with English and Spanish languages
        var authenticatedClientEn = await AuthHelper.CreateAuthenticatedAdminClientAsync(_factory);
        authenticatedClientEn.DefaultRequestHeaders.AcceptLanguage.Clear();
        authenticatedClientEn.DefaultRequestHeaders.AcceptLanguage.Add(new StringWithQualityHeaderValue("en"));

        var authenticatedClientEs = await AuthHelper.CreateAuthenticatedAdminClientAsync(_factory);
        authenticatedClientEs.DefaultRequestHeaders.AcceptLanguage.Clear();
        authenticatedClientEs.DefaultRequestHeaders.AcceptLanguage.Add(new StringWithQualityHeaderValue("es"));

        var modelWithLongName = new CreateModelRequest
        {
            Name = new string('A', 300), // Exceeds 255 character limit
            Provider = "TestProvider",
            InputPricePer1M = 1.0m,
            OutputPricePer1M = 2.0m,
            Currency = "USD",
            Status = "active",
            Capabilities = new CreateCapabilityRequest { ContextWindow = 4000 }
        };

        // When: POST requests in both languages
        var responseEn = await authenticatedClientEn.PostAsJsonAsync("/api/admin/models", modelWithLongName);
        var responseEs = await authenticatedClientEs.PostAsJsonAsync("/api/admin/models", modelWithLongName);

        // Then: Should return localized max length messages
        responseEn.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var responseBodyEn = await responseEn.Content.ReadAsStringAsync();
        responseBodyEn.Should().Contain("Model name cannot exceed 255 characters");

        responseEs.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var responseBodyEs = await responseEs.Content.ReadAsStringAsync();
        responseBodyEs.Should().Contain("El nombre del modelo no puede exceder 255 caracteres");
    }

    #endregion
}
