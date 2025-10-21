using System.Security.Claims;

namespace LlmTokenPrice.API.Extensions;

/// <summary>
/// Extension methods for ClaimsPrincipal to extract user information from JWT tokens.
/// </summary>
public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// Extracts the user identifier from the JWT token claims.
    /// Prefers email claim (ClaimTypes.Email) over name claim (ClaimTypes.Name).
    /// </summary>
    /// <param name="user">The ClaimsPrincipal from HttpContext.User.</param>
    /// <returns>The user identifier (email or username), or "unknown" if not found.</returns>
    /// <remarks>
    /// JWT tokens issued by AuthService contain:
    /// - ClaimTypes.Name: Username
    /// - ClaimTypes.Email: Email address
    /// This method prefers email for audit logging as it's more specific.
    /// </remarks>
    public static string GetUserId(this ClaimsPrincipal user)
    {
        // Try to get email claim first (most specific identifier)
        var email = user.FindFirstValue(ClaimTypes.Email);
        if (!string.IsNullOrWhiteSpace(email))
        {
            return email;
        }

        // Fallback to name claim
        var name = user.FindFirstValue(ClaimTypes.Name);
        if (!string.IsNullOrWhiteSpace(name))
        {
            return name;
        }

        // Fallback to "unknown" if no claims found (should never happen with [Authorize])
        return "unknown";
    }

    /// <summary>
    /// Extracts the user's email address from the JWT token claims.
    /// </summary>
    /// <param name="user">The ClaimsPrincipal from HttpContext.User.</param>
    /// <returns>The user's email address, or null if not found.</returns>
    public static string? GetUserEmail(this ClaimsPrincipal user)
    {
        return user.FindFirstValue(ClaimTypes.Email);
    }

    /// <summary>
    /// Extracts the user's name from the JWT token claims.
    /// </summary>
    /// <param name="user">The ClaimsPrincipal from HttpContext.User.</param>
    /// <returns>The user's name, or null if not found.</returns>
    public static string? GetUserName(this ClaimsPrincipal user)
    {
        return user.FindFirstValue(ClaimTypes.Name);
    }
}
