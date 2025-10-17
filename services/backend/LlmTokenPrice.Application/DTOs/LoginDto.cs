namespace LlmTokenPrice.Application.DTOs;

/// <summary>
/// Data transfer object for admin authentication login requests
/// Used for HTTP POST /api/admin/auth/login endpoint
/// </summary>
/// <param name="Username">Admin username (minimum 3 characters)</param>
/// <param name="Password">Admin password (minimum 6 characters)</param>
public record LoginDto(
    string Username,
    string Password
);
