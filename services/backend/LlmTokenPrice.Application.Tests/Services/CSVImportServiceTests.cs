using System.Globalization;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using FluentAssertions;
using LlmTokenPrice.Application.DTOs;
using LlmTokenPrice.Application.Services;
using LlmTokenPrice.Domain.Entities;
using LlmTokenPrice.Domain.Repositories;
using LlmTokenPrice.Domain.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace LlmTokenPrice.Application.Tests.Services;

/// <summary>
/// Unit tests for CSVImportService
/// Story 2.11 - Task 10: Comprehensive testing for CSV bulk import
/// Covers AC#3, AC#4, AC#5, AC#6
/// </summary>
public class CSVImportServiceTests
{
    private readonly Mock<IModelRepository> _modelRepositoryMock;
    private readonly Mock<IBenchmarkRepository> _benchmarkRepositoryMock;
    private readonly BenchmarkNormalizer _benchmarkNormalizer;
    private readonly Mock<ILogger<CSVImportService>> _loggerMock;
    private readonly CSVImportService _service;

    public CSVImportServiceTests()
    {
        _modelRepositoryMock = new Mock<IModelRepository>();
        _benchmarkRepositoryMock = new Mock<IBenchmarkRepository>();
        _benchmarkNormalizer = new BenchmarkNormalizer();
        _loggerMock = new Mock<ILogger<CSVImportService>>();
        _service = new CSVImportService(
            _modelRepositoryMock.Object,
            _benchmarkRepositoryMock.Object,
            _benchmarkNormalizer,
            _loggerMock.Object);
    }

    #region Task 10.1-10.3: CSV Parsing Tests

    /// <summary>
    /// [P1] Task 10.2: Should parse valid CSV with all rows successfully
    /// Story 2.11 AC#3, AC#4: CSV file upload processed and parsed
    /// </summary>
    [Fact]
    public async Task ImportBenchmarkScoresAsync_WithValidCSV_ShouldParseAllRows()
    {
        // GIVEN: Valid CSV with 3 rows, all models and benchmarks exist
        var csv = CreateValidCSV(3);
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));

        SetupValidModelAndBenchmark();

        // No existing scores (no duplicates)
        _benchmarkRepositoryMock
            .Setup(r => r.GetScoreAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((BenchmarkScore?)null);

        _benchmarkRepositoryMock
            .Setup(r => r.BulkAddScoresAsync(It.IsAny<List<BenchmarkScore>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _benchmarkRepositoryMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // WHEN: Importing CSV
        var result = await _service.ImportBenchmarkScoresAsync(stream);

        // THEN: All 3 rows should be parsed and imported successfully
        result.TotalRows.Should().Be(3);
        result.SuccessfulImports.Should().Be(3);
        result.FailedImports.Should().Be(0);
        result.SkippedDuplicates.Should().Be(0);
        result.Errors.Should().BeEmpty();

        // Verify BulkAddScoresAsync called with 3 scores
        _benchmarkRepositoryMock.Verify(
            r => r.BulkAddScoresAsync(
                It.Is<List<BenchmarkScore>>(scores => scores.Count == 3),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// [P1] Task 10.3: Should handle malformed CSV gracefully without crashing
    /// Story 2.11 AC#4: Malformed CSV handled gracefully
    /// </summary>
    [Fact]
    public async Task ImportBenchmarkScoresAsync_WithMalformedCSV_ShouldCollectErrors()
    {
        // GIVEN: CSV with missing 'score' column (malformed structure)
        // CsvHelper will parse this but leave Score field empty/null
        var modelId1 = Guid.Parse("550e8400-e29b-41d4-a716-446655440000");
        var modelId2 = Guid.Parse("550e8400-e29b-41d4-a716-446655440001");
        var benchmarkId = Guid.NewGuid();

        var csv = @"model_id,benchmark_name
550e8400-e29b-41d4-a716-446655440000,MMLU
550e8400-e29b-41d4-a716-446655440001,HumanEval";

        var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));

        // Setup mocks so validation gets past model/benchmark checks and fails at score validation
        _modelRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken _) => new Model { Id = id, Name = "Test Model" });

        _benchmarkRepositoryMock
            .Setup(r => r.GetByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Benchmark
            {
                Id = benchmarkId,
                BenchmarkName = "MMLU",
                TypicalRangeMin = 0,
                TypicalRangeMax = 100
            });

        // WHEN: Importing malformed CSV
        var result = await _service.ImportBenchmarkScoresAsync(stream);

        // THEN: Should return result with errors (no crash)
        result.TotalRows.Should().Be(2);
        result.SuccessfulImports.Should().Be(0);
        result.FailedImports.Should().Be(2);
        result.Errors.Should().HaveCount(2);

        // All rows should fail due to missing/invalid 'score' field
        // CsvHelper leaves unmapped fields as empty strings, causing "Invalid score" errors
        result.Errors.Should().AllSatisfy(error =>
        {
            error.Error.Should().Be("Invalid score (must be a number)");
        });
    }

    /// <summary>
    /// [P1] Task 10.2: Should parse CSV with whitespace trimming
    /// Story 2.11 AC#4: CSV parsed with CsvHelper TrimOptions
    /// </summary>
    [Fact]
    public async Task ImportBenchmarkScoresAsync_WithWhitespaceInFields_ShouldTrimAndParse()
    {
        // GIVEN: CSV with extra whitespace around fields
        var modelId = Guid.NewGuid();
        var csv = $@"model_id,benchmark_name,score,max_score,test_date,source_url,verified,notes
  {modelId}  ,  MMLU  ,  85.2  ,  100  ,  2025-10-01  ,  https://example.com  ,  true  ,  Test note  ";

        var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));

        SetupValidModelAndBenchmark(modelId);
        _benchmarkRepositoryMock
            .Setup(r => r.GetScoreAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((BenchmarkScore?)null);

        _benchmarkRepositoryMock
            .Setup(r => r.BulkAddScoresAsync(It.IsAny<List<BenchmarkScore>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _benchmarkRepositoryMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // WHEN: Importing CSV with whitespace
        var result = await _service.ImportBenchmarkScoresAsync(stream);

        // THEN: Should trim and parse successfully
        result.SuccessfulImports.Should().Be(1);
        result.FailedImports.Should().Be(0);

        // Verify score was parsed correctly (trimmed "85.2")
        _benchmarkRepositoryMock.Verify(
            r => r.BulkAddScoresAsync(
                It.Is<List<BenchmarkScore>>(scores =>
                    scores.Count == 1 &&
                    scores[0].Score == 85.2m &&
                    scores[0].Notes == "Test note"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region Task 10.4-10.5: Row Validation Tests

    /// <summary>
    /// [P1] Task 10.5: Should reject invalid model_id format
    /// Story 2.11 AC#4: Validation catches invalid model_id format
    /// </summary>
    [Fact]
    public async Task ImportBenchmarkScoresAsync_WithInvalidModelIdFormat_ShouldReturnError()
    {
        // GIVEN: CSV with invalid model_id (not a UUID)
        var csv = @"model_id,benchmark_name,score,max_score,test_date,source_url,verified,notes
invalid-uuid,MMLU,85.2,100,2025-10-01,https://example.com,true,Test";

        var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));

        // WHEN: Importing CSV with invalid model_id
        var result = await _service.ImportBenchmarkScoresAsync(stream);

        // THEN: Should return error for invalid format
        result.TotalRows.Should().Be(1);
        result.SuccessfulImports.Should().Be(0);
        result.FailedImports.Should().Be(1);
        result.Errors.Should().HaveCount(1);
        result.Errors[0].RowNumber.Should().Be(2); // Row 2 in CSV (after header)
        result.Errors[0].Error.Should().Be("Invalid model_id format (must be UUID)");
        result.Errors[0].Data["model_id"].Should().Be("invalid-uuid");
    }

    /// <summary>
    /// [P1] Task 10.5: Should reject model_id that doesn't exist in database
    /// Story 2.11 AC#4: Validation checks model exists
    /// </summary>
    [Fact]
    public async Task ImportBenchmarkScoresAsync_WithNonExistentModelId_ShouldReturnError()
    {
        // GIVEN: CSV with valid UUID format but model doesn't exist
        var modelId = Guid.NewGuid();
        var csv = $@"model_id,benchmark_name,score,max_score,test_date,source_url,verified,notes
{modelId},MMLU,85.2,100,2025-10-01,https://example.com,true,Test";

        var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));

        // Model doesn't exist
        _modelRepositoryMock
            .Setup(r => r.GetByIdAsync(modelId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Model?)null);

        // WHEN: Importing CSV with non-existent model
        var result = await _service.ImportBenchmarkScoresAsync(stream);

        // THEN: Should return error for model not found
        result.TotalRows.Should().Be(1);
        result.SuccessfulImports.Should().Be(0);
        result.FailedImports.Should().Be(1);
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Error.Should().Be($"Model not found: {modelId}");
    }

    /// <summary>
    /// [P1] Task 10.5: Should reject benchmark_name that doesn't exist (case-insensitive)
    /// Story 2.11 AC#4: Validation checks benchmark exists with case-insensitive lookup
    /// </summary>
    [Fact]
    public async Task ImportBenchmarkScoresAsync_WithNonExistentBenchmark_ShouldReturnError()
    {
        // GIVEN: CSV with valid model but non-existent benchmark
        var modelId = Guid.NewGuid();
        var csv = $@"model_id,benchmark_name,score,max_score,test_date,source_url,verified,notes
{modelId},NonExistentBenchmark,85.2,100,2025-10-01,https://example.com,true,Test";

        var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));

        // Model exists
        _modelRepositoryMock
            .Setup(r => r.GetByIdAsync(modelId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Model { Id = modelId, Name = "Test Model" });

        // Benchmark doesn't exist
        _benchmarkRepositoryMock
            .Setup(r => r.GetByNameAsync("NonExistentBenchmark", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Benchmark?)null);

        // WHEN: Importing CSV with non-existent benchmark
        var result = await _service.ImportBenchmarkScoresAsync(stream);

        // THEN: Should return error for benchmark not found
        result.TotalRows.Should().Be(1);
        result.SuccessfulImports.Should().Be(0);
        result.FailedImports.Should().Be(1);
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Error.Should().Be("Benchmark not found: NonExistentBenchmark");
    }

    /// <summary>
    /// [P1] Task 10.5: Should accept case-insensitive benchmark names
    /// Story 2.11 AC#4: Case-insensitive benchmark lookup
    /// </summary>
    [Fact]
    public async Task ImportBenchmarkScoresAsync_WithLowercaseBenchmarkName_ShouldMatch()
    {
        // GIVEN: CSV with lowercase "mmlu" but DB has "MMLU"
        var modelId = Guid.NewGuid();
        var benchmarkId = Guid.NewGuid();
        var csv = $@"model_id,benchmark_name,score,max_score,test_date,source_url,verified,notes
{modelId},mmlu,85.2,100,2025-10-01,https://example.com,true,Test";

        var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));

        // Model exists
        _modelRepositoryMock
            .Setup(r => r.GetByIdAsync(modelId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Model { Id = modelId, Name = "Test Model" });

        // Benchmark exists with different case
        _benchmarkRepositoryMock
            .Setup(r => r.GetByNameAsync("mmlu", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Benchmark
            {
                Id = benchmarkId,
                BenchmarkName = "MMLU",
                TypicalRangeMin = 0,
                TypicalRangeMax = 100
            });

        _benchmarkRepositoryMock
            .Setup(r => r.GetScoreAsync(modelId, benchmarkId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((BenchmarkScore?)null);

        _benchmarkRepositoryMock
            .Setup(r => r.BulkAddScoresAsync(It.IsAny<List<BenchmarkScore>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _benchmarkRepositoryMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // WHEN: Importing CSV with lowercase benchmark name
        var result = await _service.ImportBenchmarkScoresAsync(stream);

        // THEN: Should match case-insensitively and import successfully
        result.SuccessfulImports.Should().Be(1);
        result.FailedImports.Should().Be(0);
    }

    /// <summary>
    /// [P1] Task 10.5: Should reject invalid score format (non-numeric)
    /// Story 2.11 AC#4: Validation catches invalid score
    /// </summary>
    [Fact]
    public async Task ImportBenchmarkScoresAsync_WithInvalidScore_ShouldReturnError()
    {
        // GIVEN: CSV with non-numeric score
        var modelId = Guid.NewGuid();
        var csv = $@"model_id,benchmark_name,score,max_score,test_date,source_url,verified,notes
{modelId},MMLU,abc,100,2025-10-01,https://example.com,true,Test";

        var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));

        SetupValidModelAndBenchmark(modelId);

        // WHEN: Importing CSV with invalid score
        var result = await _service.ImportBenchmarkScoresAsync(stream);

        // THEN: Should return error for invalid score
        result.TotalRows.Should().Be(1);
        result.SuccessfulImports.Should().Be(0);
        result.FailedImports.Should().Be(1);
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Error.Should().Be("Invalid score (must be a number)");
        result.Errors[0].Data["score"].Should().Be("abc");
    }

    /// <summary>
    /// [P1] Task 10.5: Should reject score exceeding max_score
    /// Story 2.11 AC#4: Validation ensures score <= max_score
    /// </summary>
    [Fact]
    public async Task ImportBenchmarkScoresAsync_WithScoreExceedingMaxScore_ShouldReturnError()
    {
        // GIVEN: CSV where score > max_score
        var modelId = Guid.NewGuid();
        var csv = $@"model_id,benchmark_name,score,max_score,test_date,source_url,verified,notes
{modelId},MMLU,105.5,100,2025-10-01,https://example.com,true,Test";

        var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));

        SetupValidModelAndBenchmark(modelId);

        // WHEN: Importing CSV with score > max_score
        var result = await _service.ImportBenchmarkScoresAsync(stream);

        // THEN: Should return error
        result.TotalRows.Should().Be(1);
        result.SuccessfulImports.Should().Be(0);
        result.FailedImports.Should().Be(1);
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Error.Should().Be("Score cannot exceed max_score");
    }

    /// <summary>
    /// [P1] Task 10.5: Should reject invalid max_score format
    /// Story 2.11 AC#4: Validation catches invalid max_score
    /// </summary>
    [Fact]
    public async Task ImportBenchmarkScoresAsync_WithInvalidMaxScore_ShouldReturnError()
    {
        // GIVEN: CSV with non-numeric max_score
        var modelId = Guid.NewGuid();
        var csv = $@"model_id,benchmark_name,score,max_score,test_date,source_url,verified,notes
{modelId},MMLU,85.2,invalid,2025-10-01,https://example.com,true,Test";

        var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));

        SetupValidModelAndBenchmark(modelId);

        // WHEN: Importing CSV with invalid max_score
        var result = await _service.ImportBenchmarkScoresAsync(stream);

        // THEN: Should return error for invalid max_score
        result.TotalRows.Should().Be(1);
        result.SuccessfulImports.Should().Be(0);
        result.FailedImports.Should().Be(1);
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Error.Should().Be("Invalid max_score (must be a number)");
    }

    /// <summary>
    /// [P1] Task 10.5: Should reject invalid test_date format
    /// Story 2.11 AC#4: Validation catches invalid date format
    /// </summary>
    [Fact]
    public async Task ImportBenchmarkScoresAsync_WithInvalidTestDate_ShouldReturnError()
    {
        // GIVEN: CSV with invalid date format
        var modelId = Guid.NewGuid();
        var csv = $@"model_id,benchmark_name,score,max_score,test_date,source_url,verified,notes
{modelId},MMLU,85.2,100,invalid-date,https://example.com,true,Test";

        var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));

        SetupValidModelAndBenchmark(modelId);

        // WHEN: Importing CSV with invalid date
        var result = await _service.ImportBenchmarkScoresAsync(stream);

        // THEN: Should return error for invalid date
        result.TotalRows.Should().Be(1);
        result.SuccessfulImports.Should().Be(0);
        result.FailedImports.Should().Be(1);
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Error.Should().Be("Invalid test_date format (use YYYY-MM-DD)");
    }

    /// <summary>
    /// [P1] Task 10.5: Should reject invalid source_url format
    /// Story 2.11 AC#4: Validation catches invalid URL
    /// </summary>
    [Fact]
    public async Task ImportBenchmarkScoresAsync_WithInvalidSourceUrl_ShouldReturnError()
    {
        // GIVEN: CSV with invalid URL format
        var modelId = Guid.NewGuid();
        var csv = $@"model_id,benchmark_name,score,max_score,test_date,source_url,verified,notes
{modelId},MMLU,85.2,100,2025-10-01,not-a-valid-url,true,Test";

        var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));

        SetupValidModelAndBenchmark(modelId);

        // WHEN: Importing CSV with invalid URL
        var result = await _service.ImportBenchmarkScoresAsync(stream);

        // THEN: Should return error for invalid URL
        result.TotalRows.Should().Be(1);
        result.SuccessfulImports.Should().Be(0);
        result.FailedImports.Should().Be(1);
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Error.Should().Be("Invalid source_url format");
    }

    /// <summary>
    /// [P1] Task 10.5: Should calculate normalized score using BenchmarkNormalizer
    /// Story 2.11 AC#5: Normalized score calculated correctly
    /// </summary>
    [Fact]
    public async Task ImportBenchmarkScoresAsync_ShouldCalculateNormalizedScore()
    {
        // GIVEN: Valid CSV with score 75 and benchmark range 0-100
        var modelId = Guid.NewGuid();
        var benchmarkId = Guid.NewGuid();
        var csv = $@"model_id,benchmark_name,score,max_score,test_date,source_url,verified,notes
{modelId},MMLU,75,100,2025-10-01,https://example.com,true,Test";

        var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));

        _modelRepositoryMock
            .Setup(r => r.GetByIdAsync(modelId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Model { Id = modelId, Name = "Test Model" });

        _benchmarkRepositoryMock
            .Setup(r => r.GetByNameAsync("MMLU", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Benchmark
            {
                Id = benchmarkId,
                BenchmarkName = "MMLU",
                TypicalRangeMin = 0,
                TypicalRangeMax = 100
            });

        _benchmarkRepositoryMock
            .Setup(r => r.GetScoreAsync(modelId, benchmarkId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((BenchmarkScore?)null);

        _benchmarkRepositoryMock
            .Setup(r => r.BulkAddScoresAsync(It.IsAny<List<BenchmarkScore>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _benchmarkRepositoryMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // WHEN: Importing CSV
        var result = await _service.ImportBenchmarkScoresAsync(stream);

        // THEN: Should calculate normalized score as 0.75 (75/100)
        result.SuccessfulImports.Should().Be(1);
        _benchmarkRepositoryMock.Verify(
            r => r.BulkAddScoresAsync(
                It.Is<List<BenchmarkScore>>(scores =>
                    scores.Count == 1 &&
                    scores[0].Score == 75 &&
                    scores[0].NormalizedScore == 0.75m),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region Task 10.9: Duplicate Handling Tests

    /// <summary>
    /// [P2] Task 10.9: Should skip duplicate model+benchmark combination when skipDuplicates=true
    /// Story 2.11 AC#5: Duplicate handling - skip mode
    /// </summary>
    [Fact]
    public async Task ImportBenchmarkScoresAsync_WithDuplicateAndSkipMode_ShouldSkip()
    {
        // GIVEN: CSV with row where model+benchmark already exists
        var modelId = Guid.NewGuid();
        var benchmarkId = Guid.NewGuid();
        var csv = $@"model_id,benchmark_name,score,max_score,test_date,source_url,verified,notes
{modelId},MMLU,85.2,100,2025-10-01,https://example.com,true,Test";

        var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));

        // Model exists
        _modelRepositoryMock
            .Setup(r => r.GetByIdAsync(modelId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Model { Id = modelId, Name = "Test Model" });

        // Benchmark exists
        _benchmarkRepositoryMock
            .Setup(r => r.GetByNameAsync("MMLU", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Benchmark
            {
                Id = benchmarkId,
                BenchmarkName = "MMLU",
                TypicalRangeMin = 0,
                TypicalRangeMax = 100
            });

        // Existing score exists (duplicate)
        _benchmarkRepositoryMock
            .Setup(r => r.GetScoreAsync(modelId, benchmarkId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BenchmarkScore
            {
                Id = Guid.NewGuid(),
                ModelId = modelId,
                BenchmarkId = benchmarkId,
                Score = 80.0m
            });

        // WHEN: Importing with skipDuplicates=true (default)
        var result = await _service.ImportBenchmarkScoresAsync(stream, skipDuplicates: true);

        // THEN: Should skip the duplicate row
        result.TotalRows.Should().Be(1);
        result.SuccessfulImports.Should().Be(0);
        result.FailedImports.Should().Be(0);
        result.SkippedDuplicates.Should().Be(1);

        // BulkAddScoresAsync should NOT be called
        _benchmarkRepositoryMock.Verify(
            r => r.BulkAddScoresAsync(It.IsAny<List<BenchmarkScore>>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    /// <summary>
    /// [P1] Task 10.8: Should handle partial success (some valid, some invalid rows)
    /// Story 2.11 AC#6: Partial success pattern - valid rows import even if some fail
    /// </summary>
    [Fact]
    public async Task ImportBenchmarkScoresAsync_WithPartialSuccess_ShouldImportValidRows()
    {
        // GIVEN: CSV with 5 rows (3 valid, 2 invalid)
        var validModelId = Guid.NewGuid();
        var invalidModelId = Guid.NewGuid();
        var csv = $@"model_id,benchmark_name,score,max_score,test_date,source_url,verified,notes
{validModelId},MMLU,85.2,100,2025-10-01,https://example.com,true,Test1
{validModelId},HumanEval,0.72,1,2025-10-02,https://example.com,false,Test2
invalid-uuid,GSM8K,78.5,100,2025-10-01,https://example.com,true,Test3
{validModelId},MATH,50.5,100,2025-10-03,https://example.com,true,Test4
{invalidModelId},BigBench,88.0,100,2025-10-04,https://example.com,false,Test5";

        var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));

        // Setup: validModelId exists, invalidModelId doesn't
        _modelRepositoryMock
            .Setup(r => r.GetByIdAsync(validModelId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Model { Id = validModelId, Name = "Valid Model" });

        _modelRepositoryMock
            .Setup(r => r.GetByIdAsync(invalidModelId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Model?)null);

        // All benchmarks exist
        _benchmarkRepositoryMock
            .Setup(r => r.GetByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string name, CancellationToken _) => new Benchmark
            {
                Id = Guid.NewGuid(),
                BenchmarkName = name,
                TypicalRangeMin = 0,
                TypicalRangeMax = 100
            });

        // No duplicates
        _benchmarkRepositoryMock
            .Setup(r => r.GetScoreAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((BenchmarkScore?)null);

        _benchmarkRepositoryMock
            .Setup(r => r.BulkAddScoresAsync(It.IsAny<List<BenchmarkScore>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _benchmarkRepositoryMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // WHEN: Importing CSV with partial success
        var result = await _service.ImportBenchmarkScoresAsync(stream);

        // THEN: Should import 3 valid rows and report 2 failures
        result.TotalRows.Should().Be(5);
        result.SuccessfulImports.Should().Be(3); // Rows 1, 2, 4
        result.FailedImports.Should().Be(2); // Rows 3, 5
        result.Errors.Should().HaveCount(2);
        result.Errors[0].RowNumber.Should().Be(4); // Row 3 in CSV (invalid-uuid)
        result.Errors[1].RowNumber.Should().Be(6); // Row 5 in CSV (non-existent model)

        // Verify 3 valid scores were imported
        _benchmarkRepositoryMock.Verify(
            r => r.BulkAddScoresAsync(
                It.Is<List<BenchmarkScore>>(scores => scores.Count == 3),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Creates a valid CSV string with specified number of rows
    /// </summary>
    private string CreateValidCSV(int rowCount)
    {
        var sb = new StringBuilder();
        sb.AppendLine("model_id,benchmark_name,score,max_score,test_date,source_url,verified,notes");

        for (int i = 1; i <= rowCount; i++)
        {
            var modelId = Guid.NewGuid();
            sb.AppendLine($"{modelId},MMLU,85.{i},100,2025-10-0{i},https://example.com,true,Test{i}");
        }

        return sb.ToString();
    }

    /// <summary>
    /// Sets up mock for valid model and benchmark
    /// </summary>
    private void SetupValidModelAndBenchmark(Guid? modelId = null, Guid? benchmarkId = null)
    {
        var mId = modelId ?? Guid.NewGuid();
        var bId = benchmarkId ?? Guid.NewGuid();

        _modelRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Model { Id = mId, Name = "Test Model" });

        _benchmarkRepositoryMock
            .Setup(r => r.GetByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Benchmark
            {
                Id = bId,
                BenchmarkName = "MMLU",
                TypicalRangeMin = 0,
                TypicalRangeMax = 100
            });
    }

    #endregion
}
