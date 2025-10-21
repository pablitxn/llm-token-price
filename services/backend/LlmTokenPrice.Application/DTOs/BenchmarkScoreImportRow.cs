using System.ComponentModel.DataAnnotations;

namespace LlmTokenPrice.Application.DTOs;

/// <summary>
/// Represents a single row from the CSV import file (Story 2.11)
/// All fields are strings initially for validation before type conversion
/// Maps to CSV columns: model_id, benchmark_name, score, max_score, test_date, source_url, verified, notes
/// </summary>
public class BenchmarkScoreImportRow
{
    /// <summary>
    /// Row number in CSV file (for error reporting)
    /// </summary>
    public int RowNumber { get; set; }

    /// <summary>
    /// Model unique identifier (UUID format required)
    /// Column: model_id
    /// </summary>
    [Required]
    public string ModelId { get; set; } = string.Empty;

    /// <summary>
    /// Benchmark name identifier (e.g., "MMLU", "HumanEval")
    /// Column: benchmark_name
    /// Case-insensitive lookup against database
    /// </summary>
    [Required]
    public string BenchmarkName { get; set; } = string.Empty;

    /// <summary>
    /// Benchmark score value (decimal)
    /// Column: score
    /// </summary>
    [Required]
    public string Score { get; set; } = string.Empty;

    /// <summary>
    /// Maximum possible score (optional, decimal)
    /// Column: max_score
    /// If provided, must be >= score
    /// </summary>
    public string? MaxScore { get; set; }

    /// <summary>
    /// Date when benchmark test was performed (YYYY-MM-DD format)
    /// Column: test_date
    /// Optional, defaults to current date if not provided
    /// </summary>
    public string? TestDate { get; set; }

    /// <summary>
    /// URL source for benchmark results (optional)
    /// Column: source_url
    /// Must be valid URL format if provided
    /// </summary>
    public string? SourceUrl { get; set; }

    /// <summary>
    /// Verified status (true/false)
    /// Column: verified
    /// Defaults to false if not provided
    /// </summary>
    public string? Verified { get; set; }

    /// <summary>
    /// Additional notes about the benchmark score (optional)
    /// Column: notes
    /// Max length validated by FluentValidation
    /// </summary>
    public string? Notes { get; set; }
}
