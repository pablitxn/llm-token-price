namespace LlmTokenPrice.Domain.Enums;

/// <summary>
/// Category classification for benchmark grouping and QAPS weighting.
/// Used to organize benchmarks by their primary evaluation focus area.
/// </summary>
/// <remarks>
/// Each category has an associated default weight in the QAPS algorithm:
/// - Reasoning: 0.30 (MMLU, Big-Bench Hard)
/// - Code: 0.25 (HumanEval, MBPP)
/// - Math: 0.20 (GSM8K, MATH)
/// - Language: 0.15 (HellaSwag, TruthfulQA)
/// - Multimodal: 0.10 (MMMU, VQA)
/// </remarks>
public enum BenchmarkCategory
{
    /// <summary>
    /// Reasoning benchmarks (MMLU, Big-Bench Hard).
    /// Default weight: 0.30
    /// </summary>
    Reasoning,

    /// <summary>
    /// Code generation and understanding benchmarks (HumanEval, MBPP).
    /// Default weight: 0.25
    /// </summary>
    Code,

    /// <summary>
    /// Mathematical problem-solving benchmarks (GSM8K, MATH).
    /// Default weight: 0.20
    /// </summary>
    Math,

    /// <summary>
    /// Natural language understanding benchmarks (HellaSwag, TruthfulQA).
    /// Default weight: 0.15
    /// </summary>
    Language,

    /// <summary>
    /// Multimodal benchmarks involving vision, audio, or mixed modalities (MMMU, VQA).
    /// Default weight: 0.10
    /// </summary>
    Multimodal
}
