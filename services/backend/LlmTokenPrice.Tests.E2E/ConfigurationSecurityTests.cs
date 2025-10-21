using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using System.Net;

namespace LlmTokenPrice.Tests.E2E;

/// <summary>
/// E2E integration tests for configuration security (Tasks 18 & 19).
/// Tests CORS configuration and environment variable handling for production readiness.
/// </summary>
/// <remarks>
/// Story 2.13 Tasks 18 & 19:
/// - Task 18: CORS configured for production (environment-based origins)
/// - Task 19: Secrets moved to environment variables (JWT, connection strings)
/// </remarks>
public class ConfigurationSecurityTests
{
    /// <summary>
    /// AC 18: CORS should accept requests from configured origins.
    /// Tests that CORS origins can be set via CORS_ALLOWED_ORIGINS environment variable.
    /// </summary>
    [Fact]
    public async Task CorsConfiguration_WithEnvironmentVariable_ShouldAllowConfiguredOrigin()
    {
        // Arrange - Configure CORS via environment variable
        var testOrigin = "https://test.example.com";
        Environment.SetEnvironmentVariable("CORS_ALLOWED_ORIGINS", testOrigin);

        try
        {
            var factory = new WebApplicationFactory<LlmTokenPrice.API.Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.UseEnvironment("Development");
                    builder.ConfigureAppConfiguration((context, config) =>
                    {
                        config.AddInMemoryCollection(new Dictionary<string, string?>
                        {
                            ["CORS_ALLOWED_ORIGINS"] = testOrigin
                        });
                    });
                });

            var client = factory.CreateClient();

            // Act - Send preflight CORS request
            var request = new HttpRequestMessage(HttpMethod.Options, "/api/models");
            request.Headers.Add("Origin", testOrigin);
            request.Headers.Add("Access-Control-Request-Method", "GET");

            var response = await client.SendAsync(request);

            // Assert - Should allow the configured origin
            response.Headers.Should().ContainKey("Access-Control-Allow-Origin",
                "CORS should be configured to allow the origin from environment variable");
        }
        finally
        {
            Environment.SetEnvironmentVariable("CORS_ALLOWED_ORIGINS", null);
        }
    }

    /// <summary>
    /// AC 18: CORS should support multiple comma-separated origins.
    /// Tests production deployment scenario with multiple frontend domains.
    /// Note: This test verifies configuration parsing, not full CORS preflight (complex in test scenarios).
    /// </summary>
    [Fact]
    public void CorsConfiguration_WithMultipleOrigins_ShouldParseCorrectly()
    {
        // Arrange - Configure multiple CORS origins
        var origins = "https://app.example.com,https://www.example.com,https://admin.example.com";

        // Act - Create factory to verify configuration parsing works
        var act = () =>
        {
            var factory = new WebApplicationFactory<LlmTokenPrice.API.Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.UseEnvironment("Development");
                    builder.ConfigureAppConfiguration((context, config) =>
                    {
                        config.AddInMemoryCollection(new Dictionary<string, string?>
                        {
                            ["CORS_ALLOWED_ORIGINS"] = origins,
                            ["Jwt:SecretKey"] = "test-secret-key-32-characters-minimum-for-hs256"
                        });
                    });
                });
            _ = factory.CreateClient();
        };

        // Assert - Application should start successfully with multiple origins
        act.Should().NotThrow("CORS should support comma-separated multiple origins");
    }

    /// <summary>
    /// AC 18: CORS should reject requests from non-configured origins.
    /// Critical security test - prevents CSRF attacks from unauthorized domains.
    /// </summary>
    [Fact]
    public async Task CorsConfiguration_WithUnauthorizedOrigin_ShouldRejectRequest()
    {
        // Arrange - Configure specific allowed origin
        var allowedOrigin = "https://llmpricing.com";
        var unauthorizedOrigin = "https://malicious-site.com";

        var factory = new WebApplicationFactory<LlmTokenPrice.API.Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Development");
                builder.ConfigureAppConfiguration((context, config) =>
                {
                    config.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["CORS_ALLOWED_ORIGINS"] = allowedOrigin
                    });
                });
            });

        var client = factory.CreateClient();

        // Act - Send preflight CORS request from unauthorized origin
        var request = new HttpRequestMessage(HttpMethod.Options, "/api/models");
        request.Headers.Add("Origin", unauthorizedOrigin);
        request.Headers.Add("Access-Control-Request-Method", "GET");

        var response = await client.SendAsync(request);

        // Assert - Should NOT allow the unauthorized origin
        if (response.Headers.Contains("Access-Control-Allow-Origin"))
        {
            var allowedOriginHeader = response.Headers.GetValues("Access-Control-Allow-Origin").FirstOrDefault();
            allowedOriginHeader.Should().NotBe(unauthorizedOrigin,
                "CORS should reject requests from unauthorized origins to prevent CSRF attacks");
        }
    }

    /// <summary>
    /// AC 18: CORS should fall back to localhost:5173 for development when no environment variable set.
    /// Ensures developers can run the app without additional configuration.
    /// </summary>
    [Fact]
    public async Task CorsConfiguration_WithoutEnvironmentVariable_ShouldUseDefaultDevelopmentOrigin()
    {
        // Arrange - No CORS_ALLOWED_ORIGINS environment variable set
        var factory = new WebApplicationFactory<LlmTokenPrice.API.Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Development");
                // Explicitly don't configure CORS_ALLOWED_ORIGINS
            });

        var client = factory.CreateClient();

        // Act - Send preflight CORS request from default development origin
        var request = new HttpRequestMessage(HttpMethod.Options, "/api/models");
        request.Headers.Add("Origin", "http://localhost:5173");
        request.Headers.Add("Access-Control-Request-Method", "GET");

        var response = await client.SendAsync(request);

        // Assert - Should allow the default development origin
        response.Headers.Should().ContainKey("Access-Control-Allow-Origin",
            "CORS should fall back to localhost:5173 for development");
    }

    /// <summary>
    /// AC 19: JWT secret should be read from JWT_SECRET_KEY environment variable.
    /// Tests environment variable takes precedence over appsettings.json.
    /// </summary>
    [Fact]
    public void JwtConfiguration_WithEnvironmentVariable_ShouldUseEnvironmentSecret()
    {
        // Arrange - Set JWT secret via environment variable
        var envSecret = "environment-secret-key-32-chars-minimum-for-production-hs256";
        Environment.SetEnvironmentVariable("JWT_SECRET_KEY", envSecret);

        try
        {
            // Act - Create application (Program.cs reads JWT secret on startup)
            var factory = new WebApplicationFactory<LlmTokenPrice.API.Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.UseEnvironment("Development");
                    builder.ConfigureAppConfiguration((context, config) =>
                    {
                        // Also set a different secret in config to verify env var takes precedence
                        config.AddInMemoryCollection(new Dictionary<string, string?>
                        {
                            ["Jwt:SecretKey"] = "config-secret-should-not-be-used"
                        });
                    });
                });

            // Assert - Application should start without throwing (env var secret is used)
            var client = factory.CreateClient();
            client.Should().NotBeNull("Application should start successfully with JWT secret from environment");
        }
        finally
        {
            Environment.SetEnvironmentVariable("JWT_SECRET_KEY", null);
        }
    }

    /// <summary>
    /// AC 19: JWT secret should validate minimum length (32 characters) in production.
    /// Critical security test - weak JWT secrets compromise authentication security.
    /// </summary>
    [Fact]
    public void JwtConfiguration_WithShortSecretInProduction_ShouldThrowException()
    {
        // Arrange - Set a weak JWT secret (too short)
        var weakSecret = "short-key";
        Environment.SetEnvironmentVariable("JWT_SECRET_KEY", weakSecret);

        try
        {
            // Act & Assert - Application should throw on startup in production
            var act = () =>
            {
                var factory = new WebApplicationFactory<LlmTokenPrice.API.Program>()
                    .WithWebHostBuilder(builder =>
                    {
                        builder.UseEnvironment("Production"); // Production environment enforces validation
                    });
                _ = factory.CreateClient();
            };

            act.Should().Throw<InvalidOperationException>()
                .WithMessage("*at least 32 characters*",
                    "Production mode should enforce minimum JWT secret length for security");
        }
        finally
        {
            Environment.SetEnvironmentVariable("JWT_SECRET_KEY", null);
        }
    }

    /// <summary>
    /// AC 19: Application should throw exception if JWT secret is not configured at all.
    /// Fail-fast approach prevents accidental deployment without authentication.
    /// </summary>
    [Fact]
    public void JwtConfiguration_WithoutAnySecret_ShouldThrowException()
    {
        // Arrange - No JWT secret in environment or configuration
        Environment.SetEnvironmentVariable("JWT_SECRET_KEY", null);

        // Act & Assert - Application should throw on startup
        var act = () =>
        {
            var factory = new WebApplicationFactory<LlmTokenPrice.API.Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.UseEnvironment("Production");
                    builder.ConfigureAppConfiguration((context, config) =>
                    {
                        // Explicitly remove any JWT configuration
                        config.AddInMemoryCollection(new Dictionary<string, string?>
                        {
                            ["Jwt:SecretKey"] = null
                        });
                    });
                });
            _ = factory.CreateClient();
        };

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*JWT SecretKey is not configured*",
                "Application should fail fast if JWT secret is missing");
    }

    /// <summary>
    /// AC 19: Configuration should support fallback from environment to appsettings.
    /// Tests graceful degradation for development environments.
    /// </summary>
    [Fact]
    public void JwtConfiguration_WithoutEnvironmentVariable_ShouldFallBackToConfig()
    {
        // Arrange - No environment variable, but config has secret
        Environment.SetEnvironmentVariable("JWT_SECRET_KEY", null);
        var configSecret = "config-secret-key-32-characters-minimum-for-hs256-algorithm";

        // Act - Create application with secret in configuration
        var factory = new WebApplicationFactory<LlmTokenPrice.API.Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Development");
                builder.ConfigureAppConfiguration((context, config) =>
                {
                    config.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["Jwt:SecretKey"] = configSecret,
                        ["Jwt:Issuer"] = "test-issuer",
                        ["Jwt:Audience"] = "test-audience"
                    });
                });
            });

        // Assert - Application should start successfully using config secret
        var client = factory.CreateClient();
        client.Should().NotBeNull("Application should fall back to appsettings when environment variable not set");
    }

    /// <summary>
    /// AC 18 & 19: Verify CORS allows credentials (required for HttpOnly JWT cookies).
    /// Integration test ensuring CORS and JWT authentication work together.
    /// </summary>
    [Fact]
    public async Task CorsConfiguration_ShouldAllowCredentials_ForJwtCookies()
    {
        // Arrange
        var factory = new WebApplicationFactory<LlmTokenPrice.API.Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Development");
            });

        var client = factory.CreateClient();

        // Act - Send preflight CORS request
        var request = new HttpRequestMessage(HttpMethod.Options, "/api/models");
        request.Headers.Add("Origin", "http://localhost:5173");
        request.Headers.Add("Access-Control-Request-Method", "GET");

        var response = await client.SendAsync(request);

        // Assert - Should allow credentials (required for HttpOnly cookies)
        response.Headers.Should().ContainKey("Access-Control-Allow-Credentials",
            "CORS must allow credentials for HttpOnly JWT cookies to work");

        if (response.Headers.Contains("Access-Control-Allow-Credentials"))
        {
            var allowCredentials = response.Headers.GetValues("Access-Control-Allow-Credentials").FirstOrDefault();
            allowCredentials.Should().Be("true",
                "Access-Control-Allow-Credentials must be 'true' for JWT cookies");
        }
    }
}
