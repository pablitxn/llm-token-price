using LlmTokenPrice.Application.DTOs;

namespace LlmTokenPrice.Application.Services;

/// <summary>
/// Port interface for authentication services (Hexagonal Architecture)
/// Application layer defines the contract, Infrastructure provides the implementation
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Authenticates a user with username and password
    /// </summary>
    /// <param name="loginDto">Login credentials</param>
    /// <returns>Authentication response indicating success or failure</returns>
    Task<AuthResponseDto> AuthenticateAsync(LoginDto loginDto);

    /// <summary>
    /// Generates a JWT token for an authenticated user
    /// </summary>
    /// <param name="username">The authenticated username</param>
    /// <param name="role">The user's role (e.g., "admin")</param>
    /// <returns>JWT token string</returns>
    string GenerateJwtToken(string username, string role);

    /// <summary>
    /// Validates provided credentials against configured admin credentials
    /// </summary>
    /// <param name="username">Provided username</param>
    /// <param name="password">Provided password</param>
    /// <returns>True if credentials are valid, false otherwise</returns>
    bool ValidateCredentials(string username, string password);
}
