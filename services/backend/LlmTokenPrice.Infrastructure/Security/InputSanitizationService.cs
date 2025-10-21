using Ganss.Xss;

namespace LlmTokenPrice.Infrastructure.Security;

/// <summary>
/// Service for sanitizing user input to prevent XSS attacks (Story 2.13 Task 17).
/// Uses HtmlSanitizer library to remove malicious HTML and JavaScript from text inputs.
/// </summary>
/// <remarks>
/// Defense-in-depth strategy:
/// 1. Sanitize at INPUT (this service) - remove malicious content before storage
/// 2. Sanitize at OUTPUT (frontend encoding) - encode when displaying to users
/// 3. CSP headers (middleware) - prevent inline scripts from executing
///
/// Applied to all user-provided text fields:
/// - Model names, descriptions
/// - Benchmark names, descriptions, notes
/// - Provider names
/// - Any other free-text input
/// </remarks>
public class InputSanitizationService
{
    private readonly HtmlSanitizer _sanitizer;

    public InputSanitizationService()
    {
        _sanitizer = new HtmlSanitizer();

        // Configure allowed tags (very restrictive - we don't want ANY HTML in our data)
        // For a pricing platform, we only store plain text, no HTML formatting needed
        _sanitizer.AllowedTags.Clear(); // Remove all allowed tags (no HTML at all)
        _sanitizer.AllowedAttributes.Clear(); // Remove all allowed attributes
        _sanitizer.AllowedCssProperties.Clear(); // Remove all allowed CSS
        _sanitizer.AllowedSchemes.Clear(); // Remove all allowed URL schemes

        // Additional security settings
        _sanitizer.KeepChildNodes = true; // Keep text content but remove tags
        _sanitizer.AllowDataAttributes = false; // Block data-* attributes
    }

    /// <summary>
    /// Sanitizes a string by removing all HTML tags and potentially malicious content.
    /// Returns plain text only.
    /// </summary>
    /// <param name="input">The input string to sanitize (may contain HTML/JS)</param>
    /// <returns>Sanitized plain text string, or null if input is null</returns>
    /// <remarks>
    /// Examples:
    /// - Input: "&lt;script&gt;alert('XSS')&lt;/script&gt;Hello" → Output: "Hello"
    /// - Input: "Model &lt;b&gt;GPT-4&lt;/b&gt;" → Output: "Model GPT-4"
    /// - Input: "Normal text" → Output: "Normal text"
    /// </remarks>
    public string? Sanitize(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return input;
        }

        // Sanitize and trim whitespace
        var sanitized = _sanitizer.Sanitize(input);
        return sanitized?.Trim();
    }

    /// <summary>
    /// Sanitizes multiple strings at once (for batch operations like CSV import).
    /// </summary>
    /// <param name="inputs">Array of strings to sanitize</param>
    /// <returns>Array of sanitized strings</returns>
    public string?[] SanitizeBatch(params string?[] inputs)
    {
        return inputs.Select(Sanitize).ToArray();
    }

    /// <summary>
    /// Checks if a string contains potentially malicious content (for validation/logging).
    /// Does NOT sanitize - use Sanitize() method for that.
    /// </summary>
    /// <param name="input">The input string to check</param>
    /// <returns>True if the input contains HTML tags or suspicious patterns</returns>
    /// <remarks>
    /// Useful for logging security events or additional validation.
    /// </remarks>
    public bool ContainsPotentiallyMaliciousContent(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        var sanitized = Sanitize(input);
        // If sanitization changed the string, it contained HTML/malicious content
        return sanitized != input;
    }
}
