using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using LlmTokenPrice.Application.DTOs;
using Microsoft.AspNetCore.Mvc.Testing;

namespace LlmTokenPrice.Tests.E2E;

/// <summary>
/// E2E security tests for XSS and SQL injection protection (Story 2.13 Task 17).
/// Verifies that malicious input is properly sanitized and rejected.
/// </summary>
public class SecurityTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public SecurityTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    /// <summary>
    /// AC 18: XSS Attack - Script tag in model name should be sanitized.
    /// </summary>
    [Fact]
    public async Task CreateModel_WithXSSScriptTag_Should_Sanitize_And_Reject()
    {
        // Arrange - Malicious input with <script> tag
        var maliciousRequest = new CreateModelRequest
        {
            Name = "<script>alert('XSS')</script>GPT-4",
            Provider = "OpenAI",
            Version = "1.0",
            Status = "active",
            InputPricePer1M = 10.0m,
            OutputPricePer1M = 30.0m,
            Currency = "USD",
            Capabilities = new CreateCapabilityRequest
            {
                ContextWindow = 8192,
                MaxOutputTokens = 4096,
                SupportsVision = false,
                SupportsAudioInput = false,
                SupportsAudioOutput = false
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/admin/models", maliciousRequest);

        // Assert - Should either sanitize (201) or reject (400), but NEVER store malicious content
        if (response.StatusCode == HttpStatusCode.Created)
        {
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotContain("<script>", "Malicious script tags should be removed");
            content.Should().NotContain("alert(", "JavaScript code should be removed");
        }
        else
        {
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest, "Should reject obviously malicious input");
        }
    }

    /// <summary>
    /// AC 18: XSS Attack - HTML tags in model name should be stripped.
    /// </summary>
    [Fact]
    public async Task CreateModel_WithHTMLTags_Should_Sanitize()
    {
        // Arrange - Input with HTML formatting
        var requestWithHTML = new CreateModelRequest
        {
            Name = "Model <b>GPT-4</b> with <i>formatting</i>",
            Provider = "OpenAI<br/>",
            Version = "1.0",
            Status = "active",
            InputPricePer1M = 10.0m,
            OutputPricePer1M = 30.0m,
            Currency = "USD",
            Capability = new CreateCapabilityRequest
            {
                SupportsTextInput = true,
                SupportsImageInput = false,
                SupportsAudioInput = false,
                SupportsVideoInput = false,
                MaxContextWindow = 8192,
                MaxOutputTokens = 4096
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/admin/models", requestWithHTML);

        // Assert - Should sanitize by removing HTML tags
        if (response.StatusCode == HttpStatusCode.Created)
        {
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotContain("<b>", "HTML bold tags should be removed");
            content.Should().NotContain("<i>", "HTML italic tags should be removed");
            content.Should().NotContain("<br/>", "HTML line breaks should be removed");
            content.Should().Contain("GPT-4", "Plain text content should remain");
        }
    }

    /// <summary>
    /// AC 18: XSS Attack - Event handlers in input should be sanitized.
    /// </summary>
    [Fact]
    public async Task CreateModel_WithEventHandlers_Should_Sanitize()
    {
        // Arrange - Malicious input with event handlers
        var maliciousRequest = new CreateModelRequest
        {
            Name = "GPT-4<img src=x onerror=alert('XSS')>",
            Provider = "OpenAI",
            Version = "1.0",
            Status = "active",
            InputPricePer1M = 10.0m,
            OutputPricePer1M = 30.0m,
            Currency = "USD",
            Capability = new CreateCapabilityRequest
            {
                SupportsTextInput = true,
                SupportsImageInput = false,
                SupportsAudioInput = false,
                SupportsVideoInput = false,
                MaxContextWindow = 8192,
                MaxOutputTokens = 4096
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/admin/models", maliciousRequest);

        // Assert
        if (response.StatusCode == HttpStatusCode.Created)
        {
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotContain("onerror", "Event handlers should be removed");
            content.Should().NotContain("alert(", "JavaScript code should be removed");
        }
    }

    /// <summary>
    /// AC 18: SQL Injection - Single quotes should not break queries (EF Core protects).
    /// </summary>
    [Fact]
    public async Task GetModels_WithSQLInjectionAttempt_Should_Not_Break()
    {
        // Arrange - SQL injection attempt in search parameter
        var sqlInjectionAttempt = "' OR '1'='1"; // Classic SQL injection

        // Act - This should be safely handled by EF Core parameterized queries
        var response = await _client.GetAsync($"/api/admin/models?searchTerm={Uri.EscapeDataString(sqlInjectionAttempt)}");

        // Assert - Should return 200 (empty results or normal behavior), NOT 500 (SQL error)
        response.StatusCode.Should().Be(HttpStatusCode.OK,
            "EF Core should protect against SQL injection via parameterized queries");

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotContain("SQL", "SQL error messages should not be exposed");
        content.Should().NotContain("Exception", "Exception details should not be exposed");
    }

    /// <summary>
    /// AC 17: Security headers should be present in all responses.
    /// </summary>
    [Fact]
    public async Task AllResponses_Should_Have_Security_Headers()
    {
        // Act
        var response = await _client.GetAsync("/api/models");

        // Assert - Content-Security-Policy
        response.Headers.Should().ContainKey("Content-Security-Policy",
            "CSP header should be present to prevent XSS");

        // Assert - X-Content-Type-Options
        response.Headers.Should().ContainKey("X-Content-Type-Options",
            "X-Content-Type-Options header should prevent MIME sniffing");
        response.Headers.GetValues("X-Content-Type-Options").First().Should().Be("nosniff");

        // Assert - X-Frame-Options
        response.Headers.Should().ContainKey("X-Frame-Options",
            "X-Frame-Options header should prevent clickjacking");
        response.Headers.GetValues("X-Frame-Options").First().Should().Be("DENY");

        // Assert - X-XSS-Protection
        response.Headers.Should().ContainKey("X-XSS-Protection",
            "X-XSS-Protection header should be present for legacy browser support");
    }

    /// <summary>
    /// AC 17: CSP should block inline scripts.
    /// </summary>
    [Fact]
    public async Task ContentSecurityPolicy_Should_Block_Inline_Scripts()
    {
        // Act
        var response = await _client.GetAsync("/api/models");

        // Assert - CSP should contain script-src 'self' (no 'unsafe-inline')
        response.Headers.Should().ContainKey("Content-Security-Policy");
        var cspHeader = response.Headers.GetValues("Content-Security-Policy").First();

        cspHeader.Should().Contain("script-src 'self'",
            "CSP should only allow scripts from same origin");
        cspHeader.Should().NotContain("'unsafe-inline'",
            "CSP should NOT allow unsafe-inline scripts (except for styles in Swagger)");
    }

    /// <summary>
    /// AC 18: Unicode and special characters should be handled safely.
    /// </summary>
    [Fact]
    public async Task CreateModel_WithUnicodeAndSpecialChars_Should_Handle_Safely()
    {
        // Arrange - Input with unicode and special characters
        var requestWithSpecialChars = new CreateModelRequest
        {
            Name = "Model™ with émojis 🚀 and spëcial çhars",
            Provider = "Company®",
            Version = "1.0-αβγ",
            Status = "active",
            InputPricePer1M = 10.0m,
            OutputPricePer1M = 30.0m,
            Currency = "€UR",
            Capability = new CreateCapabilityRequest
            {
                SupportsTextInput = true,
                SupportsImageInput = false,
                SupportsAudioInput = false,
                SupportsVideoInput = false,
                MaxContextWindow = 8192,
                MaxOutputTokens = 4096
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/admin/models", requestWithSpecialChars);

        // Assert - Should handle unicode safely (201 or 400, but not 500)
        response.StatusCode.Should().NotBe(HttpStatusCode.InternalServerError,
            "Unicode characters should not cause server errors");
    }
}
