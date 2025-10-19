namespace LlmTokenPrice.Domain.Enums;

/// <summary>
/// Interpretation rule for benchmark score comparison.
/// Determines whether higher or lower scores indicate better performance.
/// </summary>
/// <remarks>
/// Used for score normalization in the QAPS (Quality-Adjusted Price per Score) algorithm.
/// Most benchmarks use HigherBetter (accuracy, F1 score), but some use LowerBetter (error rates, latency).
/// Default: HigherBetter
/// </remarks>
public enum BenchmarkInterpretation
{
    /// <summary>
    /// Higher scores indicate better performance (e.g., accuracy, F1 score).
    /// Most common interpretation for evaluation benchmarks.
    /// Example: MMLU score of 0.95 is better than 0.85.
    /// </summary>
    HigherBetter,

    /// <summary>
    /// Lower scores indicate better performance (e.g., error rate, perplexity, latency).
    /// Less common but used for specific benchmark types.
    /// Example: Error rate of 0.05 is better than 0.15.
    /// </summary>
    LowerBetter
}
