using System.ComponentModel.DataAnnotations;
using CsvHelper.Configuration.Attributes;

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
    /// Not mapped from CSV - set programmatically
    /// </summary>
    [Ignore]
    public int RowNumber { get; set; }

    /// <summary>
    /// Model unique identifier (UUID format required)
    /// Column: model_id
    /// </summary>
    [Required]
    [Name("model_id")]
    public string ModelId { get; set; } = string.Empty;

    /// <summary>
    /// Benchmark name identifier (e.g., "MMLU", "HumanEval")
    /// Column: benchmark_name
    /// Case-insensitive lookup against database
    /// </summary>
    [Required]
    [Name("benchmark_name")]
    public string BenchmarkName { get; set; } = string.Empty;

    /// <summary>
    /// Benchmark score value (decimal)
    /// Column: score
    /// </summary>
    [Required]
    [Name("score")]
    public string Score { get; set; } = string.Empty;

    /// <summary>
    /// Maximum possible score (optional, decimal)
    /// Column: max_score
    /// If provided, must be >= score
    /// </summary>
    [Name("max_score")]
    public string? MaxScore { get; set; }

    /// <summary>
    /// Date when benchmark test was performed (YYYY-MM-DD format)
    /// Column: test_date
    /// Optional, defaults to current date if not provided
    /// </summary>
    [Name("test_date")]
    public string? TestDate { get; set; }

    /// <summary>
    /// URL source for benchmark results (optional)
    /// Column: source_url
    /// Must be valid URL format if provided
    /// </summary>
    [Name("source_url")]
    public string? SourceUrl { get; set; }

    /// <summary>
    /// Verified status (true/false)
    /// Column: verified
    /// Defaults to false if not provided
    /// </summary>
    [Name("verified")]
    public string? Verified { get; set; }

    /// <summary>
    /// Additional notes about the benchmark score (optional)
    /// Column: notes
    /// Max length validated by FluentValidation
    /// </summary>
    [Name("notes")]
    public string? Notes { get; set; }
}
