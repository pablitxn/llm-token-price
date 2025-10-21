namespace LlmTokenPrice.Domain.Services;

/// <summary>
/// Pure domain service for normalizing benchmark scores to 0-1 range.
/// Used for QAPS (Quality-Adjusted Price per Score) calculation.
/// </summary>
/// <remarks>
/// CRITICAL: This is a pure domain service with ZERO infrastructure dependencies.
/// No EF Core, no HTTP, no framework-specific code. Pure business logic only.
/// 
/// Normalization Formula:
/// normalized_score = (score - typical_range_min) / (typical_range_max - typical_range_min)
/// 
/// Result is clamped to [0, 1] range to handle outliers.
/// Edge case: If min = max, returns 1.0 to avoid division by zero.
/// </remarks>
public class BenchmarkNormalizer
{
    /// <summary>
    /// Normalizes a score to the 0-1 range based on typical min/max values.
    /// </summary>
    /// <param name="score">The actual score achieved.</param>
    /// <param name="typicalMin">Minimum value in typical range.</param>
    /// <param name="typicalMax">Maximum value in typical range.</param>
    /// <returns>Normalized score between 0.0 and 1.0.</returns>
    /// <remarks>
    /// Examples:
    /// - Normalize(75, 0, 100) = 0.75
    /// - Normalize(50, 0, 100) = 0.50
    /// - Normalize(50, 50, 50) = 1.00 (edge case: min = max)
    /// - Normalize(150, 0, 100) = 1.00 (clamped, outlier above max)
    /// - Normalize(-10, 0, 100) = 0.00 (clamped, outlier below min)
    /// </remarks>
    public decimal Normalize(decimal score, decimal typicalMin, decimal typicalMax)
    {
        // Edge case: If min equals max, avoid division by zero
        // Return 1.0 as the score is at the only possible value
        if (typicalMax == typicalMin)
        {
            return 1.0m;
        }

        // Apply normalization formula: (score - min) / (max - min)
        var normalized = (score - typicalMin) / (typicalMax - typicalMin);

        // Clamp result to [0, 1] range to handle outliers
        // Scores outside typical range are capped rather than extrapolated
        return Math.Max(0m, Math.Min(1m, normalized));
    }

    /// <summary>
    /// Checks if a score falls within the typical range.
    /// </summary>
    /// <param name="score">The score to check.</param>
    /// <param name="typicalMin">Minimum value in typical range.</param>
    /// <param name="typicalMax">Maximum value in typical range.</param>
    /// <returns>True if score is within [min, max], false otherwise.</returns>
    /// <remarks>
    /// Used for generating warning flags in the UI when scores are unusual.
    /// Scores outside typical range are still allowed (admin override).
    /// </remarks>
    public bool IsWithinTypicalRange(decimal score, decimal typicalMin, decimal typicalMax)
    {
        return score >= typicalMin && score <= typicalMax;
    }
}
