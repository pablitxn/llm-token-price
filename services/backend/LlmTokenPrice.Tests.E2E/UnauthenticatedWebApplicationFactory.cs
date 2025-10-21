using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace LlmTokenPrice.Tests.E2E;

/// <summary>
/// WebApplicationFactory for testing unauthenticated requests (401 Unauthorized tests).
/// Story 2.13 Task 8.5: Does NOT use TestAuthHandler - requires real JWT authentication.
/// </summary>
/// <remarks>
/// Use this factory when testing that endpoints correctly return 401 Unauthorized.
/// For authenticated tests, use the regular TestWebApplicationFactory which auto-authenticates.
/// </remarks>
public class UnauthenticatedWebApplicationFactory : WebApplicationFactory<LlmTokenPrice.API.Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // No test authentication handler configured
        // Endpoints will use real JWT authentication and return 401 if no token provided
        base.ConfigureWebHost(builder);
    }
}
