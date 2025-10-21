using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace LlmTokenPrice.Tests.E2E;

/// <summary>
/// Custom WebApplicationFactory for E2E tests that configures test authentication.
/// Bypasses JWT authentication by using a test authentication handler that auto-authenticates all requests.
/// </summary>
/// <remarks>
/// Story 2.13 Task 1: Fix E2E test failures by configuring test authentication
/// This approach avoids the complexity of generating valid JWT tokens in tests.
/// </remarks>
public class TestWebApplicationFactory : WebApplicationFactory<LlmTokenPrice.API.Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            // Replace JWT authentication with test authentication scheme
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "TestScheme";
                options.DefaultChallengeScheme = "TestScheme";
            })
            .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("TestScheme", options => { });
        });
    }
}

/// <summary>
/// Test authentication handler that automatically authenticates all requests.
/// Returns a ClaimsPrincipal with admin role for testing admin endpoints.
/// </summary>
public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public TestAuthHandler(
        Microsoft.Extensions.Options.IOptionsMonitor<AuthenticationSchemeOptions> options,
        Microsoft.Extensions.Logging.ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Create claims for test user (admin role)
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "test-admin-user"),
            new Claim(ClaimTypes.Name, "Test Admin"),
            new Claim(ClaimTypes.Role, "Admin")
        };

        var identity = new ClaimsIdentity(claims, "TestScheme");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "TestScheme");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
