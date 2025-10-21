namespace LlmTokenPrice.Application.DTOs;

/// <summary>
/// Real-time progress updates for CSV import operation (Story 2.13 Task 12)
/// Streamed via Server-Sent Events (SSE) to provide live feedback during import
/// Enables progress bar, row counters, and cancellation UI in the admin panel
/// </summary>
public class CSVImportProgressDto
{
    /// <summary>
    /// Current phase of the import process
    /// Values: "Parsing", "Validating", "Importing", "Complete", "Cancelled", "Failed"
    /// </summary>
    public string Phase { get; set; } = string.Empty;

    /// <summary>
    /// Total number of rows detected in the CSV file (excluding header)
    /// Set during parsing phase, used to calculate progress percentage
    /// </summary>
    public int TotalRows { get; set; }

    /// <summary>
    /// Number of rows processed so far (parsed, validated, or imported depending on phase)
    /// Used to calculate: PercentComplete = (ProcessedRows / TotalRows) * 100
    /// </summary>
    public int ProcessedRows { get; set; }

    /// <summary>
    /// Number of rows successfully imported to database
    /// Updates in real-time during "Importing" phase
    /// </summary>
    public int SuccessCount { get; set; }

    /// <summary>
    /// Number of rows that failed validation or import
    /// Updates in real-time during "Validating" and "Importing" phases
    /// </summary>
    public int FailureCount { get; set; }

    /// <summary>
    /// Number of rows skipped due to duplicate model+benchmark combination
    /// Only increments when skipDuplicates=true
    /// </summary>
    public int SkippedCount { get; set; }

    /// <summary>
    /// Progress percentage (0-100)
    /// Calculated as: (ProcessedRows / TotalRows) * 100
    /// Displayed in progress bar UI
    /// </summary>
    public decimal PercentComplete => TotalRows > 0
        ? Math.Round((decimal)ProcessedRows / TotalRows * 100, 1)
        : 0;

    /// <summary>
    /// Human-readable status message for UI display
    /// Examples:
    /// - "Parsing CSV file..."
    /// - "Validating row 45 of 120..."
    /// - "Importing row 100 of 120..."
    /// - "Import complete: 115 successful, 5 failed"
    /// - "Import cancelled by user"
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Final import result (only populated when Phase = "Complete")
    /// Contains detailed error information for failed rows
    /// Null during in-progress phases
    /// </summary>
    public CSVImportResultDto? FinalResult { get; set; }

    /// <summary>
    /// Error message if Phase = "Failed"
    /// Contains exception details for debugging
    /// </summary>
    public string? ErrorMessage { get; set; }
}
