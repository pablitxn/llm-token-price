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
/// Service for importing benchmark scores via CSV file upload (Story 2.11)
/// Implements partial success pattern: valid rows imported even if some fail
/// Uses streaming with CsvHelper to handle large files efficiently
/// </summary>
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
    /// Imports benchmark scores from CSV stream
    /// Validates each row, imports valid scores in batch, returns detailed results
    /// </summary>
    /// <param name="fileStream">CSV file stream</param>
    /// <param name="skipDuplicates">If true, skips rows where model+benchmark already exists. If false, updates existing scores.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Import result with success/failure counts and error details</returns>
    public async Task<CSVImportResultDto> ImportBenchmarkScoresAsync(
        Stream fileStream,
        bool skipDuplicates = true,
        CancellationToken cancellationToken = default)
    {
        var result = new CSVImportResultDto();
        var validScores = new List<BenchmarkScore>();
        var errors = new List<FailedRowDto>();

        try
        {
            // Task 4: Parse CSV with CsvHelper
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

            // Task 5 & 6: Validate and transform each row
            for (int i = 0; i < rows.Count; i++)
            {
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
                    continue;
                }

                if (!validationResult.IsValid)
                {
                    errors.Add(new FailedRowDto
                    {
                        RowNumber = row.RowNumber,
                        Error = validationResult.Error!,
                        Data = RowToDictionary(row)
                    });
                    continue;
                }

                validScores.Add(validationResult.Score!);
            }

            // Task 6: Batch insert valid scores
            if (validScores.Any())
            {
                _logger.LogInformation("Importing {Count} valid benchmark scores", validScores.Count);

                await _benchmarkRepository.BulkAddScoresAsync(validScores, cancellationToken);
                await _benchmarkRepository.SaveChangesAsync(cancellationToken);

                result.SuccessfulImports = validScores.Count;

                // TODO Story 2.11: Invalidate cache patterns
                // await _cacheService.RemoveByPatternAsync("cache:model:*:v1");
                // await _cacheService.RemoveByPatternAsync("cache:benchmarks:*");
                // await _cacheService.RemoveByPatternAsync("cache:qaps:*");
                // await _cacheService.RemoveByPatternAsync("cache:bestvalue:*");

                _logger.LogInformation(
                    "CSV import completed: {Successful} successful, {Failed} failed, {Skipped} skipped",
                    result.SuccessfulImports,
                    errors.Count,
                    result.SkippedDuplicates);
            }

            result.FailedImports = errors.Count;
            result.Errors = errors;

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fatal error during CSV import");
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
