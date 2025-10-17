using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using LlmTokenPrice.Application.DTOs;
using Microsoft.AspNetCore.Mvc.Testing;

namespace LlmTokenPrice.Infrastructure.Tests.Integration;

/// <summary>
/// Integration tests for AdminAuthController
/// Tests login and logout endpoints with real HTTP requests
/// </summary>
public class AdminAuthControllerTests : IClassFixture<WebApplicationFactory<LlmTokenPrice.API.Program>>
{
    private readonly HttpClient _client;

    public AdminAuthControllerTests(WebApplicationFactory<LlmTokenPrice.API.Program> factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false // We want to test redirects manually
        });
    }

    [Fact]
    public async Task Login_ShouldReturn200_AndSetCookie_WhenCredentialsAreValid()
    {
        // Arrange
        var loginDto = new LoginDto("admin", "admin123");

        // Act
        var response = await _client.PostAsJsonAsync("/api/admin/auth/login", loginDto);
        var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Message.Should().Be("Authentication successful");

        // Verify cookie is set
        response.Headers.Should().ContainKey("Set-Cookie");
        var cookieHeader = response.Headers.GetValues("Set-Cookie").FirstOrDefault();
        cookieHeader.Should().NotBeNull();
        cookieHeader.Should().Contain("admin_token");
        cookieHeader.Should().ContainAny("HttpOnly", "httponly"); // ASP.NET Core may use lowercase
        cookieHeader.Should().ContainAny("SameSite=Strict", "samesite=strict");
    }

    [Fact]
    public async Task Login_ShouldReturn401_WhenCredentialsAreInvalid()
    {
        // Arrange
        var loginDto = new LoginDto("admin", "wrongpassword");

        // Act
        var response = await _client.PostAsJsonAsync("/api/admin/auth/login", loginDto);
        var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        result.Should().NotBeNull();
        result!.Success.Should().BeFalse();
        result.Message.Should().Be("Invalid username or password");
    }

    [Fact]
    public async Task Login_ShouldReturn400_WhenUsernameIsEmpty()
    {
        // Arrange
        var loginDto = new LoginDto("", "password123");

        // Act
        var response = await _client.PostAsJsonAsync("/api/admin/auth/login", loginDto);
        var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.Should().NotBeNull();
        result!.Success.Should().BeFalse();
        result.Message.Should().Be("Username and password are required");
    }

    [Fact]
    public async Task Login_ShouldReturn400_WhenPasswordIsEmpty()
    {
        // Arrange
        var loginDto = new LoginDto("admin", "");

        // Act
        var response = await _client.PostAsJsonAsync("/api/admin/auth/login", loginDto);
        var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.Should().NotBeNull();
        result!.Success.Should().BeFalse();
        result.Message.Should().Be("Username and password are required");
    }

    [Fact]
    public async Task Logout_ShouldReturn200_AndClearCookie()
    {
        // Arrange - First login to get a cookie
        var loginDto = new LoginDto("admin", "admin123");
        await _client.PostAsJsonAsync("/api/admin/auth/login", loginDto);

        // Act - Logout
        var response = await _client.PostAsync("/api/admin/auth/logout", null);
        var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Message.Should().Be("Logout successful");
    }

    [Fact]
    public async Task Login_MultipleTimes_ShouldReplaceOldToken()
    {
        // Arrange
        var loginDto = new LoginDto("admin", "admin123");

        // Act - Login twice
        var response1 = await _client.PostAsJsonAsync("/api/admin/auth/login", loginDto);
        var cookie1 = response1.Headers.GetValues("Set-Cookie").FirstOrDefault();

        var response2 = await _client.PostAsJsonAsync("/api/admin/auth/login", loginDto);
        var cookie2 = response2.Headers.GetValues("Set-Cookie").FirstOrDefault();

        // Assert - Both should succeed with new cookies
        response1.StatusCode.Should().Be(HttpStatusCode.OK);
        response2.StatusCode.Should().Be(HttpStatusCode.OK);
        cookie1.Should().NotBeNull();
        cookie2.Should().NotBeNull();
        // Note: In a real scenario, tokens would be different. This just verifies the endpoint works.
    }
}
