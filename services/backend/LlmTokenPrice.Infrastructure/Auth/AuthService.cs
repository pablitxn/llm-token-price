using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using LlmTokenPrice.Application.DTOs;
using LlmTokenPrice.Application.Services;

namespace LlmTokenPrice.Infrastructure.Auth;

/// <summary>
/// Infrastructure adapter implementing authentication logic
/// Handles JWT generation and credential validation
/// </summary>
public class AuthService : IAuthService
{
    private readonly IConfiguration _configuration;

    public AuthService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// Authenticates admin user with provided credentials
    /// </summary>
    public async Task<AuthResponseDto> AuthenticateAsync(LoginDto loginDto)
    {
        // Input validation
        if (string.IsNullOrWhiteSpace(loginDto.Username) || string.IsNullOrWhiteSpace(loginDto.Password))
        {
            return new AuthResponseDto(
                Success: false,
                Message: "Username and password are required"
            );
        }

        // Validate credentials
        var isValid = ValidateCredentials(loginDto.Username, loginDto.Password);

        if (!isValid)
        {
            // Simulate async operation for consistency
            await Task.Delay(500); // Timing attack mitigation
            return new AuthResponseDto(
                Success: false,
                Message: "Invalid username or password"
            );
        }

        return new AuthResponseDto(
            Success: true,
            Message: "Authentication successful"
        );
    }

    /// <summary>
    /// Generates JWT token with user claims
    /// Token includes: sub (username), role, exp (expiration), iat (issued at)
    /// </summary>
    public string GenerateJwtToken(string username, string role)
    {
        var jwtSecret = _configuration["Jwt:SecretKey"]
            ?? throw new InvalidOperationException("JWT secret key is not configured");

        var jwtExpirationHours = int.Parse(_configuration["Jwt:ExpirationHours"] ?? "24");

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, username),
            new Claim(ClaimTypes.Role, role),
            new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                ClaimValueTypes.Integer64)
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(jwtExpirationHours),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Validates credentials against hardcoded admin credentials (MVP implementation)
    /// TODO: Replace with database lookup and bcrypt hashing post-MVP
    /// </summary>
    public bool ValidateCredentials(string username, string password)
    {
        var configuredUsername = _configuration["Admin:Username"];
        var configuredPassword = _configuration["Admin:Password"];

        if (string.IsNullOrWhiteSpace(configuredUsername) || string.IsNullOrWhiteSpace(configuredPassword))
        {
            throw new InvalidOperationException("Admin credentials are not configured");
        }

        // Simple string comparison for MVP (SECURITY: Use bcrypt for production)
        return username == configuredUsername && password == configuredPassword;
    }
}
