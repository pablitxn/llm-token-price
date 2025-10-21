using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using LlmTokenPrice.Application.DTOs;
using LlmTokenPrice.Domain.Entities;
using LlmTokenPrice.Domain.Repositories;
using LlmTokenPrice.Domain.Services;
using Microsoft.Extensions.Logging;

namespace LlmTokenPrice.Application.Services;

/// <summary>
/// Service for importing benchmark scores via CSV file upload (Story 2.11, Story 2.13 Task 6)
/// Implements all-or-nothing transaction pattern: validates ALL rows before importing ANY (atomic operation)
/// Uses streaming with CsvHelper to handle large files efficiently
/// </summary>
/// <remarks>
/// Story 2.13 Task 6: Changed from partial success to all-or-nothing using database transactions.
/// If ANY row fails validation, NO rows are imported (rollback).
/// Uses ITransactionScope from Domain layer to maintain Hexagonal Architecture (no Infrastructure dependency).
/// </remarks>
public class CSVImportService
{
    private readonly IModelRepository _modelRepository;
    private readonly IBenchmarkRepository _benchmarkRepository;
    private readonly BenchmarkNormalizer _normalizer;
    private readonly ILogger<CSVImportService> _logger;

    public CSVImportService(
        IModelRepository modelRepository,
        IBenchmarkRepository benchmarkRepository,
        BenchmarkNormalizer normalizer,
        ILogger<CSVImportService> logger)
    {
        _modelRepository = modelRepository;
        _benchmarkRepository = benchmarkRepository;
        _normalizer = normalizer;
        _logger = logger;
    }

    /// <summary>
    /// Imports benchmark scores from CSV stream using all-or-nothing transaction pattern
    /// Story 2.13 Task 6: Validates ALL rows first, only imports if ALL are valid (atomic operation)
    /// Story 2.13 Task 12: Reports real-time progress via IProgress for SSE streaming
    /// </summary>
    /// <param name="fileStream">CSV file stream</param>
    /// <param name="skipDuplicates">If true, skips rows where model+benchmark already exists. If false, updates existing scores.</param>
    /// <param name="progress">Optional progress reporter for real-time updates (Task 12)</param>
    /// <param name="cancellationToken">Cancellation token for operation cancellation (Task 12.6)</param>
    /// <returns>Import result with success/failure counts and error details</returns>
    /// <remarks>
    /// IMPORTANT: This method uses all-or-nothing pattern. If ANY row fails validation,
    /// NO rows are imported. The entire transaction is rolled back.
    /// Progress updates: Parsing → Validating (per row) → Importing → Complete
    /// Supports cancellation via CancellationToken for user-initiated abort
    /// </remarks>
    public async Task<CSVImportResultDto> ImportBenchmarkScoresAsync(
        Stream fileStream,
        bool skipDuplicates = true,
        IProgress<CSVImportProgressDto>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var result = new CSVImportResultDto();
        var validScores = new List<BenchmarkScore>();
        var errors = new List<FailedRowDto>();

        try
        {
            // PHASE 1: Parse and validate ALL rows (no database changes yet)

            // Task 12.2: Report parsing phase start
            progress?.Report(new CSVImportProgressDto
            {
                Phase = "Parsing",
                TotalRows = 0,
                ProcessedRows = 0,
                Message = "Parsing CSV file..."
            });

            using var reader = new StreamReader(fileStream);
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HeaderValidated = null, // Don't throw on missing headers
                MissingFieldFound = null, // Don't throw on missing fields
                TrimOptions = TrimOptions.Trim, // Auto-trim whitespace
            };

            using var csv = new CsvReader(reader, config);

            // Read all rows (CsvHelper handles header row automatically)
            var rows = csv.GetRecords<BenchmarkScoreImportRow>().ToList();
            result.TotalRows = rows.Count;

            _logger.LogInformation("Parsing CSV: {RowCount} rows found", rows.Count);

            // Task 12.2: Report total rows detected
            progress?.Report(new CSVImportProgressDto
            {
                Phase = "Validating",
                TotalRows = rows.Count,
                ProcessedRows = 0,
                Message = $"Validating {rows.Count} rows..."
            });

            // Validate ALL rows first (Task 6.2: collect all errors before importing)
            for (int i = 0; i < rows.Count; i++)
            {
                // Task 12.6: Check for cancellation request
                cancellationToken.ThrowIfCancellationRequested();

                var row = rows[i];
                row.RowNumber = i + 2; // +2 because CSV is 1-indexed and has header row

                var validationResult = await ValidateAndTransformRow(
                    row,
                    skipDuplicates,
                    cancellationToken);

                // Check WasSkipped BEFORE IsValid (duplicates have IsValid=false by default)
                if (validationResult.WasSkipped)
                {
                    result.SkippedDuplicates++;
                }
                else if (!validationResult.IsValid)
                {
                    errors.Add(new FailedRowDto
                    {
                        RowNumber = row.RowNumber,
                        Error = validationResult.Error!,
                        Data = RowToDictionary(row)
                    });
                }
                else
                {
                    validScores.Add(validationResult.Score!);
                }

                // Task 12.2: Report validation progress every 10 rows or on last row
                if ((i + 1) % 10 == 0 || i == rows.Count - 1)
                {
                    progress?.Report(new CSVImportProgressDto
                    {
                        Phase = "Validating",
                        TotalRows = rows.Count,
                        ProcessedRows = i + 1,
                        SuccessCount = validScores.Count,
                        FailureCount = errors.Count,
                        SkippedCount = result.SkippedDuplicates,
                        Message = $"Validating row {i + 1} of {rows.Count}..."
                    });
                }
            }

            // Task 6.2: If ANY row failed validation, return errors WITHOUT importing anything
            if (errors.Any())
            {
                result.FailedImports = errors.Count;
                result.SuccessfulImports = 0;
                result.Errors = errors;

                _logger.LogWarning(
                    "CSV import aborted due to validation errors: {Failed} failed rows out of {Total} total rows. " +
                    "All-or-nothing policy: NO rows were imported.",
                    errors.Count,
                    result.TotalRows);

                // Task 12.2: Report validation failure
                progress?.Report(new CSVImportProgressDto
                {
                    Phase = "Complete",
                    TotalRows = result.TotalRows,
                    ProcessedRows = result.TotalRows,
                    SuccessCount = 0,
                    FailureCount = errors.Count,
                    SkippedCount = result.SkippedDuplicates,
                    Message = $"Validation failed: {errors.Count} rows with errors. No rows imported.",
                    FinalResult = result
                });

                return result;
            }

            // PHASE 2: All rows valid - import using database transaction (Task 6.1 & 6.3)
            if (validScores.Any())
            {
                _logger.LogInformation(
                    "All {Count} rows validated successfully. Starting transactional import...",
                    validScores.Count);

                // Task 12.2: Report import phase start
                progress?.Report(new CSVImportProgressDto
                {
                    Phase = "Importing",
                    TotalRows = result.TotalRows,
                    ProcessedRows = result.TotalRows,
                    SuccessCount = validScores.Count,
                    FailureCount = 0,
                    SkippedCount = result.SkippedDuplicates,
                    Message = $"Importing {validScores.Count} validated rows to database..."
                });

                // Task 6.1: Wrap import in database transaction (all-or-nothing)
                // Uses ITransactionScope from Domain layer (Hexagonal Architecture)
                await using var transaction = await _benchmarkRepository.BeginTransactionAsync(cancellationToken);

                try
                {
                    // Task 12.6: Check for cancellation before database operation
                    cancellationToken.ThrowIfCancellationRequested();

                    await _benchmarkRepository.BulkAddScoresAsync(validScores, cancellationToken);
                    await _benchmarkRepository.SaveChangesAsync(cancellationToken);

                    // Task 6.3: Only commit if all rows successfully imported
                    await transaction.CommitAsync(cancellationToken);

                    result.SuccessfulImports = validScores.Count;
                    result.FailedImports = 0;

                    _logger.LogInformation(
                        "CSV import transaction committed successfully: {Successful} rows imported, {Skipped} duplicates skipped",
                        result.SuccessfulImports,
                        result.SkippedDuplicates);

                    // Task 12.2: Report successful completion
                    progress?.Report(new CSVImportProgressDto
                    {
                        Phase = "Complete",
                        TotalRows = result.TotalRows,
                        ProcessedRows = result.TotalRows,
                        SuccessCount = result.SuccessfulImports,
                        FailureCount = 0,
                        SkippedCount = result.SkippedDuplicates,
                        Message = $"Import complete: {result.SuccessfulImports} rows imported, {result.SkippedDuplicates} duplicates skipped",
                        FinalResult = result
                    });
                }
                catch (Exception ex)
                {
                    // Task 6.2: Rollback transaction on any database error
                    await transaction.RollbackAsync(cancellationToken);

                    _logger.LogError(ex,
                        "CSV import transaction rolled back due to database error. NO rows were imported.");

                    throw; // Re-throw to be caught by outer catch block
                }
            }

            result.Errors = errors;
            return result;
        }
        catch (OperationCanceledException)
        {
            // Task 12.6: Handle user-initiated cancellation
            _logger.LogWarning("CSV import cancelled by user");

            progress?.Report(new CSVImportProgressDto
            {
                Phase = "Cancelled",
                TotalRows = result.TotalRows,
                ProcessedRows = 0,
                Message = "Import cancelled by user. No rows were imported.",
                FinalResult = result
            });

            throw; // Re-throw to signal cancellation to caller
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fatal error during CSV import");

            // Task 12.2: Report fatal error
            progress?.Report(new CSVImportProgressDto
            {
                Phase = "Failed",
                TotalRows = result.TotalRows,
                ProcessedRows = 0,
                Message = "Import failed due to an unexpected error",
                ErrorMessage = ex.Message,
                FinalResult = result
            });

            throw;
        }
    }

    /// <summary>
    /// Validates a single CSV row and transforms it into a BenchmarkScore entity
    /// Task 5: Row validation logic
    /// </summary>
    private async Task<RowValidationResult> ValidateAndTransformRow(
        BenchmarkScoreImportRow row,
        bool skipDuplicates,
        CancellationToken cancellationToken)
    {
        var result = new RowValidationResult { RowNumber = row.RowNumber };

        // Validate model_id format
        if (!Guid.TryParse(row.ModelId, out var modelId))
        {
            result.Error = "Invalid model_id format (must be UUID)";
            return result;
        }

        // Validate model exists
        var model = await _modelRepository.GetByIdAsync(modelId, cancellationToken);
        if (model == null)
        {
            result.Error = $"Model not found: {row.ModelId}";
            return result;
        }

        // Validate benchmark exists (case-insensitive)
        var benchmark = await _benchmarkRepository.GetByNameAsync(row.BenchmarkName, cancellationToken);
        if (benchmark == null)
        {
            result.Error = $"Benchmark not found: {row.BenchmarkName}";
            return result;
        }

        // Validate score is decimal
        if (!decimal.TryParse(row.Score, out var score))
        {
            result.Error = "Invalid score (must be a number)";
            return result;
        }

        // Validate max_score if provided
        decimal? maxScore = null;
        if (!string.IsNullOrWhiteSpace(row.MaxScore))
        {
            if (!decimal.TryParse(row.MaxScore, out var parsedMaxScore))
            {
                result.Error = "Invalid max_score (must be a number)";
                return result;
            }

            if (score > parsedMaxScore)
            {
                result.Error = "Score cannot exceed max_score";
                return result;
            }

            maxScore = parsedMaxScore;
        }

        // Validate test_date if provided
        DateTime? testDate = null;
        if (!string.IsNullOrWhiteSpace(row.TestDate))
        {
            if (!DateTime.TryParse(row.TestDate, out var parsedTestDate))
            {
                result.Error = "Invalid test_date format (use YYYY-MM-DD)";
                return result;
            }

            testDate = DateTime.SpecifyKind(parsedTestDate, DateTimeKind.Utc);
        }

        // Validate source_url if provided
        if (!string.IsNullOrWhiteSpace(row.SourceUrl))
        {
            if (!Uri.TryCreate(row.SourceUrl, UriKind.Absolute, out _))
            {
                result.Error = "Invalid source_url format";
                return result;
            }
        }

        // Parse verified (default false)
        var verified = bool.TryParse(row.Verified, out var parsedVerified) && parsedVerified;

        // Task 5: Check for duplicate (model + benchmark combination)
        var existingScore = await _benchmarkRepository.GetScoreAsync(
            modelId,
            benchmark.Id,
            cancellationToken);

        if (existingScore != null)
        {
            if (skipDuplicates)
            {
                result.WasSkipped = true;
                return result;
            }
            // TODO: Implement update logic for "update duplicates" mode
            // For now, skip duplicates even if skipDuplicates is false
            result.WasSkipped = true;
            return result;
        }

        // Task 6: Calculate normalized score using BenchmarkNormalizer
        var normalizedScore = _normalizer.Normalize(
            score,
            benchmark.TypicalRangeMin ?? 0,
            benchmark.TypicalRangeMax ?? 100);

        // Create entity
        var benchmarkScore = new BenchmarkScore
        {
            Id = Guid.NewGuid(),
            ModelId = modelId,
            BenchmarkId = benchmark.Id,
            Score = score,
            MaxScore = maxScore,
            NormalizedScore = normalizedScore,
            TestDate = testDate ?? DateTime.UtcNow,
            SourceUrl = row.SourceUrl,
            Verified = verified,
            Notes = row.Notes,
            CreatedAt = DateTime.UtcNow
        };

        result.IsValid = true;
        result.Score = benchmarkScore;
        return result;
    }

    /// <summary>
    /// Converts a BenchmarkScoreImportRow to dictionary for error reporting
    /// </summary>
    private static Dictionary<string, string> RowToDictionary(BenchmarkScoreImportRow row)
    {
        return new Dictionary<string, string>
        {
            ["model_id"] = row.ModelId,
            ["benchmark_name"] = row.BenchmarkName,
            ["score"] = row.Score,
            ["max_score"] = row.MaxScore ?? string.Empty,
            ["test_date"] = row.TestDate ?? string.Empty,
            ["source_url"] = row.SourceUrl ?? string.Empty,
            ["verified"] = row.Verified ?? string.Empty,
            ["notes"] = row.Notes ?? string.Empty
        };
    }

    /// <summary>
    /// Internal result of row validation and transformation
    /// </summary>
    private class RowValidationResult
    {
        public int RowNumber { get; set; }
        public bool IsValid { get; set; }
        public bool WasSkipped { get; set; }
        public string? Error { get; set; }
        public BenchmarkScore? Score { get; set; }
    }
}
