using System.Net.Http.Json;

namespace LlmTokenPrice.Tests.E2E.Helpers;

/// <summary>
/// Test helper for admin authentication in E2E tests.
/// Story 2.13 Task 8.3: Provides reusable authentication utilities for all admin endpoint tests.
/// </summary>
public static class AuthHelper
{
    /// <summary>
    /// Authenticates as admin user by calling the login endpoint.
    /// The JWT token is automatically stored in the HttpClient's cookie container.
    /// </summary>
    /// <param name="client">HttpClient instance (must have HandleCookies = true)</param>
    /// <returns>JWT token string extracted from the admin_token cookie</returns>
    /// <exception cref="InvalidOperationException">If login fails or cookie is not returned</exception>
    /// <remarks>
    /// After calling this method, all subsequent requests with the same HttpClient
    /// will be authenticated as the admin user.
    /// Test credentials: username=admin, password=admin123 (configured in test database seed)
    /// </remarks>
    public static async Task<string> LoginAsAdminAsync(HttpClient client)
    {
        var loginRequest = new
        {
            username = "admin",
            password = "admin123" // Test credentials from database seed
        };

        var response = await client.PostAsJsonAsync("/api/admin/auth/login", loginRequest);
        response.EnsureSuccessStatusCode();

        // Extract JWT token from Set-Cookie header
        var cookies = response.Headers.GetValues("Set-Cookie");
        var adminTokenCookie = cookies.FirstOrDefault(c => c.StartsWith("admin_token="));

        if (adminTokenCookie == null)
        {
            throw new InvalidOperationException(
                "Admin login did not return admin_token cookie. " +
                "Ensure authentication is configured correctly.");
        }

        // Parse token from cookie (format: "admin_token=<token>; HttpOnly; SameSite=Strict; Path=/")
        var token = adminTokenCookie.Split(';')[0].Split('=')[1];
        return token;
    }

    /// <summary>
    /// Creates a new HttpClient configured for authenticated admin requests.
    /// </summary>
    /// <param name="factory">WebApplicationFactory instance for creating clients</param>
    /// <param name="autoAuthenticate">If true, automatically calls LoginAsAdminAsync before returning</param>
    /// <returns>HttpClient configured with cookie handling and optionally pre-authenticated</returns>
    /// <remarks>
    /// The returned client has cookies enabled and follows redirects disabled for precise testing.
    /// </remarks>
    public static async Task<HttpClient> CreateAuthenticatedAdminClientAsync(
        TestWebApplicationFactory factory,
        bool autoAuthenticate = true)
    {
        var client = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            HandleCookies = true // Required for JWT cookie authentication
        });

        if (autoAuthenticate)
        {
            await LoginAsAdminAsync(client);
        }

        return client;
    }

    /// <summary>
    /// Verifies that an endpoint returns 401 Unauthorized when called without authentication.
    /// Story 2.13 Task 8.5: Security test to ensure [Authorize] attributes are properly configured.
    /// </summary>
    /// <param name="client">Unauthenticated HttpClient instance</param>
    /// <param name="method">HTTP method (GET, POST, PUT, DELETE)</param>
    /// <param name="endpoint">API endpoint path</param>
    /// <param name="requestBody">Optional request body for POST/PUT requests</param>
    /// <returns>True if endpoint correctly returns 401, false otherwise</returns>
    public static async Task<bool> VerifyRequiresAuthenticationAsync(
        HttpClient client,
        HttpMethod method,
        string endpoint,
        object? requestBody = null)
    {
        HttpResponseMessage response;

        if (method == HttpMethod.Get)
        {
            response = await client.GetAsync(endpoint);
        }
        else if (method == HttpMethod.Post)
        {
            response = requestBody != null
                ? await client.PostAsJsonAsync(endpoint, requestBody)
                : await client.PostAsync(endpoint, null);
        }
        else if (method == HttpMethod.Put)
        {
            response = requestBody != null
                ? await client.PutAsJsonAsync(endpoint, requestBody)
                : await client.PutAsync(endpoint, null);
        }
        else if (method == HttpMethod.Delete)
        {
            response = await client.DeleteAsync(endpoint);
        }
        else
        {
            throw new ArgumentException($"Unsupported HTTP method: {method}", nameof(method));
        }

        return response.StatusCode == System.Net.HttpStatusCode.Unauthorized;
    }
}
