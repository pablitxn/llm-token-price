using Microsoft.AspNetCore.Mvc;
using LlmTokenPrice.Application.DTOs;
using LlmTokenPrice.Application.Services;

namespace LlmTokenPrice.API.Controllers.Admin;

/// <summary>
/// Controller handling admin authentication endpoints
/// Provides login and logout functionality with JWT-based authentication
/// </summary>
[ApiController]
[Route("api/admin/auth")]
[Produces("application/json")]
public class AdminAuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AdminAuthController> _logger;

    public AdminAuthController(IAuthService authService, ILogger<AdminAuthController> _logger)
    {
        _authService = authService;
        this._logger = _logger;
    }

    /// <summary>
    /// Authenticates admin user and sets JWT token in HttpOnly cookie
    /// </summary>
    /// <param name="loginDto">Login credentials (username and password)</param>
    /// <returns>Authentication response with success status and message</returns>
    /// <response code="200">Authentication successful, JWT cookie set</response>
    /// <response code="401">Invalid credentials</response>
    /// <response code="400">Invalid request format or missing fields</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto loginDto)
    {
        _logger.LogInformation("Admin login attempt for username: {Username}", loginDto.Username);

        // Validate input
        if (string.IsNullOrWhiteSpace(loginDto.Username) || string.IsNullOrWhiteSpace(loginDto.Password))
        {
            _logger.LogWarning("Login attempt with empty username or password");
            return BadRequest(new AuthResponseDto(
                Success: false,
                Message: "Username and password are required"
            ));
        }

        // Authenticate user
        var authResponse = await _authService.AuthenticateAsync(loginDto);

        if (!authResponse.Success)
        {
            _logger.LogWarning("Failed login attempt for username: {Username}", loginDto.Username);
            return Unauthorized(authResponse);
        }

        // Generate JWT token
        var token = _authService.GenerateJwtToken(loginDto.Username, "admin");

        // Set JWT in HttpOnly cookie
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true, // Prevents JavaScript access (XSS protection)
            Secure = !HttpContext.Request.IsLocal(), // HTTPS only in production
            SameSite = SameSiteMode.Strict, // CSRF protection
            Expires = DateTimeOffset.UtcNow.AddHours(24), // 24-hour expiration
            Path = "/"
        };

        Response.Cookies.Append("admin_token", token, cookieOptions);

        _logger.LogInformation("Successful admin login for username: {Username}", loginDto.Username);

        return Ok(authResponse);
    }

    /// <summary>
    /// Logs out admin user by clearing the JWT cookie
    /// </summary>
    /// <returns>Logout confirmation</returns>
    /// <response code="200">Logout successful, JWT cookie cleared</response>
    [HttpPost("logout")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    public ActionResult<AuthResponseDto> Logout()
    {
        _logger.LogInformation("Admin logout request");

        // Clear JWT cookie
        Response.Cookies.Delete("admin_token");

        return Ok(new AuthResponseDto(
            Success: true,
            Message: "Logout successful"
        ));
    }
}

/// <summary>
/// Extension method to check if HTTP request is local (for development)
/// </summary>
public static class HttpRequestExtensions
{
    public static bool IsLocal(this HttpRequest request)
    {
        var connection = request.HttpContext.Connection;
        if (connection.RemoteIpAddress != null)
        {
            return connection.LocalIpAddress != null
                ? connection.RemoteIpAddress.Equals(connection.LocalIpAddress)
                : System.Net.IPAddress.IsLoopback(connection.RemoteIpAddress);
        }

        // Handle forwarded headers for reverse proxies
        return true; // Default to secure=false in development if IP detection fails
    }
}
