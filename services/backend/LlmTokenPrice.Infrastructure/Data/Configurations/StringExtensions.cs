using System.Text;
using System.Text.RegularExpressions;

namespace LlmTokenPrice.Infrastructure.Data.Configurations;

/// <summary>
/// String extension methods for case conversions used in EF Core value converters.
/// </summary>
internal static class StringExtensions
{
    /// <summary>
    /// Converts PascalCase to snake_case.
    /// Example: "HigherBetter" → "higher_better"
    /// </summary>
    public static string ToSnakeCase(this string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        var sb = new StringBuilder();
        sb.Append(char.ToLowerInvariant(input[0]));

        for (int i = 1; i < input.Length; i++)
        {
            char c = input[i];
            if (char.IsUpper(c))
            {
                sb.Append('_');
                sb.Append(char.ToLowerInvariant(c));
            }
            else
            {
                sb.Append(c);
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Converts snake_case to PascalCase.
    /// Example: "higher_better" → "HigherBetter"
    /// </summary>
    public static string ToPascalCase(this string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        var parts = input.Split('_');
        var sb = new StringBuilder();

        foreach (var part in parts)
        {
            if (part.Length > 0)
            {
                sb.Append(char.ToUpperInvariant(part[0]));
                if (part.Length > 1)
                {
                    sb.Append(part.Substring(1).ToLowerInvariant());
                }
            }
        }

        return sb.ToString();
    }
}
