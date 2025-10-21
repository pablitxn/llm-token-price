namespace LlmTokenPrice.Application.DTOs;

/// <summary>
/// Result DTO for CSV bulk import operation (Story 2.11 AC#6)
/// Provides summary statistics and detailed error information for failed rows
/// Supports partial success scenario where valid rows are imported even if some fail
/// </summary>
public class CSVImportResultDto
{
    /// <summary>
    /// Total number of rows in the CSV file (excluding header)
    /// </summary>
    public int TotalRows { get; set; }

    /// <summary>
    /// Number of rows successfully imported to database
    /// </summary>
    public int SuccessfulImports { get; set; }

    /// <summary>
    /// Number of rows that failed validation or import
    /// </summary>
    public int FailedImports { get; set; }

    /// <summary>
    /// Number of rows skipped because they already exist (duplicate model+benchmark)
    /// Only counted when import strategy is "skip duplicates"
    /// </summary>
    public int SkippedDuplicates { get; set; }

    /// <summary>
    /// Detailed error information for each failed row
    /// Includes row number, error message, and original row data for correction
    /// </summary>
    public List<FailedRowDto> Errors { get; set; } = new();
}

/// <summary>
/// Detailed error information for a single failed row in CSV import
/// Enables admin to download failed rows, correct errors, and re-import
/// </summary>
public class FailedRowDto
{
    /// <summary>
    /// Row number in the CSV file (1-based index, excluding header)
    /// </summary>
    public int RowNumber { get; set; }

    /// <summary>
    /// Human-readable error message describing why the row failed
    /// Examples:
    /// - "Invalid model_id format (must be UUID)"
    /// - "Model not found: {guid}"
    /// - "Benchmark not found: {name}"
    /// - "Invalid score (must be a number)"
    /// </summary>
    public string Error { get; set; } = string.Empty;

    /// <summary>
    /// Original row data as key-value pairs (column name => value)
    /// Allows admin to see exactly what data failed and correct it
    /// All values are strings as they appear in the CSV
    /// </summary>
    public Dictionary<string, string> Data { get; set; } = new();
}
