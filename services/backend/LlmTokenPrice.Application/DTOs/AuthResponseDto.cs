namespace LlmTokenPrice.Application.DTOs;

/// <summary>
/// Data transfer object for authentication responses
/// JWT token is sent via HttpOnly cookie, not in response body
/// </summary>
/// <param name="Success">Whether authentication was successful</param>
/// <param name="Message">Human-readable message describing the result</param>
public record AuthResponseDto(
    bool Success,
    string Message
);
