namespace LlmTokenPrice.Domain.Services;

/// <summary>
/// Domain service for normalizing benchmark scores to a common 0-1 scale.
/// Pure business logic with zero infrastructure dependencies (Hexagonal Architecture).
/// </summary>
/// <remarks>
/// The QAPS (Quality-Adjusted Price per Score) algorithm requires all benchmark scores
/// on a common scale for weighted averaging. Different benchmarks have different ranges:
/// - Some use 0-100 (e.g., MMLU: 0-100%)
/// - Others use 0-1 (e.g., accuracy metrics)
/// - Some have custom ranges (e.g., perplexity scores)
///
/// Normalization formula: (score - min) / (max - min)
/// Result is clamped to [0, 1] to handle outliers gracefully.
/// </remarks>
public class BenchmarkNormalizer
{
    /// <summary>
    /// Normalizes a benchmark score to the 0-1 range using the typical min/max values.
    /// </summary>
    /// <param name="score">The raw benchmark score to normalize</param>
    /// <param name="min">Minimum value of the typical range for this benchmark</param>
    /// <param name="max">Maximum value of the typical range for this benchmark</param>
    /// <returns>Normalized score between 0.0 and 1.0 (clamped for outliers)</returns>
    /// <example>
    /// // MMLU benchmark: score=75, range 0-100
    /// var normalized = normalizer.Normalize(75m, 0m, 100m); // Returns 0.75
    /// </example>
    public decimal Normalize(decimal score, decimal min, decimal max)
    {
        // Edge case: if min equals max, any score is considered "perfect"
        // This avoids division by zero and handles single-value benchmarks
        if (max == min)
        {
            return 1.0m;
        }

        // Standard normalization formula: (score - min) / (max - min)
        var normalized = (score - min) / (max - min);

        // Clamp to [0, 1] range to handle outliers gracefully
        // - Scores below min normalize to 0.0 (worst possible)
        // - Scores above max normalize to 1.0 (best possible)
        // This prevents negative normalized scores or values > 1.0 from skewing QAPS
        return Math.Max(0m, Math.Min(1m, normalized));
    }

    /// <summary>
    /// Checks if a score falls within the typical range for a benchmark.
    /// Used to show warnings in the admin UI when scores are unusual.
    /// </summary>
    /// <param name="score">The raw benchmark score to validate</param>
    /// <param name="min">Minimum value of the typical range</param>
    /// <param name="max">Maximum value of the typical range</param>
    /// <returns>True if score is within [min, max], false if it's an outlier</returns>
    /// <remarks>
    /// Out-of-range scores are NOT rejected (admin can override), but the UI shows
    /// a warning to help catch data entry errors.
    /// </remarks>
    public bool IsWithinTypicalRange(decimal score, decimal min, decimal max)
    {
        return score >= min && score <= max;
    }
}
