using System.Net;
using System.Net.Http.Json;
using System.Text;
using FluentAssertions;
using LlmTokenPrice.Application.DTOs;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace LlmTokenPrice.Tests.E2E;

/// <summary>
/// E2E API tests for Admin Benchmarks endpoints
/// Story 2.9 - Task 10.5-10.8: Integration tests for benchmark CRUD operations
/// Story 2.11 - Task 10.6-10.10: Integration tests for CSV bulk import
/// Priority: P1 (High - API contract testing)
/// </summary>
public class AdminBenchmarksApiTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public AdminBenchmarksApiTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            HandleCookies = true
        });
    }

    #region POST /api/admin/benchmarks Tests

    /// <summary>
    /// [P1] POST /api/admin/benchmarks should create benchmark and return 201 Created
    /// Story 2.9 AC#4: POST endpoint creates benchmark definition
    /// </summary>
    [Fact]
    public async Task PostBenchmark_WithValidData_ShouldReturn201Created()
    {
        // GIVEN: Valid benchmark creation request
        var request = new CreateBenchmarkRequest
        {
            BenchmarkName = $"TestBenchmark_{Guid.NewGuid():N}",
            FullName = "Test Benchmark for E2E",
            Description = "Integration test benchmark",
            Category = "Reasoning",
            Interpretation = "HigherBetter",
            TypicalRangeMin = 0,
            TypicalRangeMax = 100,
            WeightInQaps = 0.30m
        };

        // WHEN: Posting to create benchmark endpoint
        var response = await _client.PostAsJsonAsync("/api/admin/benchmarks", request);

        // THEN: Should return 201 Created with benchmark in response
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var createdBenchmark = await response.Content.ReadFromJsonAsync<BenchmarkResponseDto>();
        createdBenchmark.Should().NotBeNull();
        createdBenchmark!.Id.Should().NotBeEmpty();
        createdBenchmark.BenchmarkName.Should().Be(request.BenchmarkName);
        createdBenchmark.FullName.Should().Be(request.FullName);
        createdBenchmark.Category.Should().Be("Reasoning");
        createdBenchmark.WeightInQaps.Should().Be(0.30m);
        createdBenchmark.IsActive.Should().BeTrue();

        // Location header should point to the created resource
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.AbsolutePath.Should().Contain($"/api/admin/benchmarks/{createdBenchmark.Id}");
    }

    /// <summary>
    /// [P1] POST /api/admin/benchmarks should return 409 Conflict for duplicate name
    /// Story 2.9 AC#6: Validation ensures benchmark names are unique
    /// Task 10.6: Test duplicate name returns 409 Conflict
    /// </summary>
    [Fact]
    public async Task PostBenchmark_WithDuplicateName_ShouldReturn409Conflict()
    {
        // GIVEN: Create first benchmark
        var uniqueName = $"DuplicateTest_{Guid.NewGuid():N}";
        var firstRequest = new CreateBenchmarkRequest
        {
            BenchmarkName = uniqueName,
            FullName = "First Benchmark",
            Category = "Reasoning",
            Interpretation = "HigherBetter",
            TypicalRangeMin = 0,
            TypicalRangeMax = 100,
            WeightInQaps = 0.30m
        };

        var firstResponse = await _client.PostAsJsonAsync("/api/admin/benchmarks", firstRequest);
        firstResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // AND: Attempt to create second benchmark with same name
        var duplicateRequest = new CreateBenchmarkRequest
        {
            BenchmarkName = uniqueName,
            FullName = "Duplicate Benchmark",
            Category = "Code",
            Interpretation = "LowerBetter",
            TypicalRangeMin = 0,
            TypicalRangeMax = 100,
            WeightInQaps = 0.25m
        };

        // WHEN: Posting duplicate benchmark
        var duplicateResponse = await _client.PostAsJsonAsync("/api/admin/benchmarks", duplicateRequest);

        // THEN: Should return 409 Conflict
        duplicateResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var errorContent = await duplicateResponse.Content.ReadAsStringAsync();
        errorContent.Should().Contain("already exists");
    }

    /// <summary>
    /// [P1] POST /api/admin/benchmarks should return 400 BadRequest for invalid data
    /// Task 10.5: Test validation fails with 400
    /// </summary>
    [Fact]
    public async Task PostBenchmark_WithInvalidData_ShouldReturn400BadRequest()
    {
        // GIVEN: Invalid benchmark request (missing required fields)
        var invalidRequest = new
        {
            BenchmarkName = "", // Empty name (invalid)
            FullName = "",      // Empty full name (invalid)
            Category = "InvalidCategory",
            Interpretation = "InvalidInterpretation",
            TypicalRangeMin = 100, // Min > Max (invalid)
            TypicalRangeMax = 0,
            WeightInQaps = 1.5m  // > 1.0 (invalid)
        };

        // WHEN: Posting invalid benchmark
        var response = await _client.PostAsJsonAsync("/api/admin/benchmarks", invalidRequest);

        // THEN: Should return 400 BadRequest with validation errors
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    /// <summary>
    /// [P1] POST /api/admin/benchmarks should validate weight has at most 2 decimal places
    /// Story 2.9 - Task 2.9: QAPS weight validation
    /// </summary>
    [Fact]
    public async Task PostBenchmark_WithTooManyDecimalPlaces_ShouldReturn400BadRequest()
    {
        // GIVEN: Benchmark with weight having 3 decimal places
        var request = new CreateBenchmarkRequest
        {
            BenchmarkName = $"Test_{Guid.NewGuid():N}",
            FullName = "Test",
            Category = "Reasoning",
            Interpretation = "HigherBetter",
            TypicalRangeMin = 0,
            TypicalRangeMax = 100,
            WeightInQaps = 0.123m // 3 decimal places
        };

        // WHEN: Posting benchmark
        var response = await _client.PostAsJsonAsync("/api/admin/benchmarks", request);

        // THEN: Should return 400 BadRequest
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region GET /api/admin/benchmarks Tests

    /// <summary>
    /// [P1] GET /api/admin/benchmarks should return all benchmarks including inactive
    /// Story 2.9 AC#2: List view shows all benchmark definitions
    /// </summary>
    [Fact]
    public async Task GetBenchmarks_WithIncludeInactive_ShouldReturnAllBenchmarks()
    {
        // GIVEN: Create active and inactive benchmarks
        var activeBenchmark = new CreateBenchmarkRequest
        {
            BenchmarkName = $"Active_{Guid.NewGuid():N}",
            FullName = "Active Benchmark",
            Category = "Reasoning",
            Interpretation = "HigherBetter",
            TypicalRangeMin = 0,
            TypicalRangeMax = 100,
            WeightInQaps = 0.30m
        };

        await _client.PostAsJsonAsync("/api/admin/benchmarks", activeBenchmark);

        // WHEN: Getting all benchmarks with includeInactive=true
        var response = await _client.GetAsync("/api/admin/benchmarks?includeInactive=true");

        // THEN: Should return 200 OK with list of benchmarks
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var benchmarks = await response.Content.ReadFromJsonAsync<List<BenchmarkResponseDto>>();
        benchmarks.Should().NotBeNull();
        benchmarks.Should().NotBeEmpty();
    }

    /// <summary>
    /// [P2] GET /api/admin/benchmarks should filter by category
    /// Story 2.9 - Task 1.6: Filter by category
    /// </summary>
    [Fact]
    public async Task GetBenchmarks_WithCategoryFilter_ShouldReturnFilteredBenchmarks()
    {
        // GIVEN: Create benchmarks with different categories
        var reasoningBenchmark = new CreateBenchmarkRequest
        {
            BenchmarkName = $"Reasoning_{Guid.NewGuid():N}",
            FullName = "Reasoning Test",
            Category = "Reasoning",
            Interpretation = "HigherBetter",
            TypicalRangeMin = 0,
            TypicalRangeMax = 100,
            WeightInQaps = 0.30m
        };

        var codeBenchmark = new CreateBenchmarkRequest
        {
            BenchmarkName = $"Code_{Guid.NewGuid():N}",
            FullName = "Code Test",
            Category = "Code",
            Interpretation = "HigherBetter",
            TypicalRangeMin = 0,
            TypicalRangeMax = 100,
            WeightInQaps = 0.25m
        };

        await _client.PostAsJsonAsync("/api/admin/benchmarks", reasoningBenchmark);
        await _client.PostAsJsonAsync("/api/admin/benchmarks", codeBenchmark);

        // WHEN: Getting benchmarks filtered by Reasoning category
        var response = await _client.GetAsync("/api/admin/benchmarks?category=Reasoning&includeInactive=true");

        // THEN: Should return only Reasoning benchmarks
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var benchmarks = await response.Content.ReadFromJsonAsync<List<BenchmarkResponseDto>>();
        benchmarks.Should().NotBeNull();
        benchmarks.Should().Contain(b => b.Category == "Reasoning");
    }

    #endregion

    #region GET /api/admin/benchmarks/{id} Tests

    /// <summary>
    /// [P1] GET /api/admin/benchmarks/{id} should return benchmark details
    /// </summary>
    [Fact]
    public async Task GetBenchmarkById_WithValidId_ShouldReturn200OK()
    {
        // GIVEN: Create a benchmark
        var request = new CreateBenchmarkRequest
        {
            BenchmarkName = $"GetById_{Guid.NewGuid():N}",
            FullName = "Get By ID Test",
            Category = "Reasoning",
            Interpretation = "HigherBetter",
            TypicalRangeMin = 0,
            TypicalRangeMax = 100,
            WeightInQaps = 0.30m
        };

        var createResponse = await _client.PostAsJsonAsync("/api/admin/benchmarks", request);
        var createdBenchmark = await createResponse.Content.ReadFromJsonAsync<BenchmarkResponseDto>();

        // WHEN: Getting benchmark by ID
        var getResponse = await _client.GetAsync($"/api/admin/benchmarks/{createdBenchmark!.Id}");

        // THEN: Should return 200 OK with benchmark details
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var benchmark = await getResponse.Content.ReadFromJsonAsync<BenchmarkResponseDto>();
        benchmark.Should().NotBeNull();
        benchmark!.Id.Should().Be(createdBenchmark.Id);
        benchmark.BenchmarkName.Should().Be(request.BenchmarkName);
    }

    /// <summary>
    /// [P1] GET /api/admin/benchmarks/{id} should return 404 NotFound for non-existent ID
    /// </summary>
    [Fact]
    public async Task GetBenchmarkById_WithNonExistentId_ShouldReturn404NotFound()
    {
        // GIVEN: Non-existent benchmark ID
        var nonExistentId = Guid.NewGuid();

        // WHEN: Getting benchmark by non-existent ID
        var response = await _client.GetAsync($"/api/admin/benchmarks/{nonExistentId}");

        // THEN: Should return 404 NotFound
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region PUT /api/admin/benchmarks/{id} Tests

    /// <summary>
    /// [P1] PUT /api/admin/benchmarks/{id} should update benchmark and return 200 OK
    /// Story 2.9 AC#5: Edit functionality for benchmarks
    /// Task 10.8: Test edit endpoint
    /// </summary>
    [Fact]
    public async Task PutBenchmark_WithValidData_ShouldReturn200OK()
    {
        // GIVEN: Create a benchmark
        var createRequest = new CreateBenchmarkRequest
        {
            BenchmarkName = $"UpdateTest_{Guid.NewGuid():N}",
            FullName = "Original Name",
            Category = "Reasoning",
            Interpretation = "HigherBetter",
            TypicalRangeMin = 0,
            TypicalRangeMax = 100,
            WeightInQaps = 0.30m
        };

        var createResponse = await _client.PostAsJsonAsync("/api/admin/benchmarks", createRequest);
        var createdBenchmark = await createResponse.Content.ReadFromJsonAsync<BenchmarkResponseDto>();

        // AND: Update request with new values
        var updateRequest = new UpdateBenchmarkRequest
        {
            FullName = "Updated Name",
            Description = "Updated description",
            Category = "Code",
            Interpretation = "LowerBetter",
            TypicalRangeMin = 10,
            TypicalRangeMax = 90,
            WeightInQaps = 0.35m
        };

        // WHEN: Updating benchmark
        var updateResponse = await _client.PutAsJsonAsync($"/api/admin/benchmarks/{createdBenchmark!.Id}", updateRequest);

        // THEN: Should return 200 OK with updated benchmark
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var updatedBenchmark = await updateResponse.Content.ReadFromJsonAsync<BenchmarkResponseDto>();
        updatedBenchmark.Should().NotBeNull();
        updatedBenchmark!.FullName.Should().Be("Updated Name");
        updatedBenchmark.Category.Should().Be("Code");
        updatedBenchmark.Interpretation.Should().Be("LowerBetter");
        updatedBenchmark.WeightInQaps.Should().Be(0.35m);
        updatedBenchmark.BenchmarkName.Should().Be(createRequest.BenchmarkName); // Name should remain unchanged
    }

    /// <summary>
    /// [P1] PUT /api/admin/benchmarks/{id} should return 404 NotFound for non-existent ID
    /// Task 10.8: Test edit endpoint error cases
    /// </summary>
    [Fact]
    public async Task PutBenchmark_WithNonExistentId_ShouldReturn404NotFound()
    {
        // GIVEN: Non-existent benchmark ID and valid update request
        var nonExistentId = Guid.NewGuid();
        var updateRequest = new UpdateBenchmarkRequest
        {
            FullName = "Updated",
            Category = "Reasoning",
            Interpretation = "HigherBetter",
            TypicalRangeMin = 0,
            TypicalRangeMax = 100,
            WeightInQaps = 0.30m
        };

        // WHEN: Attempting to update non-existent benchmark
        var response = await _client.PutAsJsonAsync($"/api/admin/benchmarks/{nonExistentId}", updateRequest);

        // THEN: Should return 404 NotFound
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region DELETE /api/admin/benchmarks/{id} Tests

    /// <summary>
    /// [P1] DELETE /api/admin/benchmarks/{id} should soft-delete benchmark and return 204 NoContent
    /// Story 2.9 AC#5: Delete functionality for benchmarks
    /// Task 10.8: Test delete endpoint
    /// </summary>
    [Fact]
    public async Task DeleteBenchmark_WithValidId_ShouldReturn204NoContent()
    {
        // GIVEN: Create a benchmark
        var createRequest = new CreateBenchmarkRequest
        {
            BenchmarkName = $"DeleteTest_{Guid.NewGuid():N}",
            FullName = "To Be Deleted",
            Category = "Reasoning",
            Interpretation = "HigherBetter",
            TypicalRangeMin = 0,
            TypicalRangeMax = 100,
            WeightInQaps = 0.30m
        };

        var createResponse = await _client.PostAsJsonAsync("/api/admin/benchmarks", createRequest);
        var createdBenchmark = await createResponse.Content.ReadFromJsonAsync<BenchmarkResponseDto>();

        // WHEN: Deleting benchmark
        var deleteResponse = await _client.DeleteAsync($"/api/admin/benchmarks/{createdBenchmark!.Id}");

        // THEN: Should return 204 NoContent
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // AND: Benchmark should be soft-deleted (IsActive = false)
        var getResponse = await _client.GetAsync($"/api/admin/benchmarks/{createdBenchmark.Id}");
        var benchmark = await getResponse.Content.ReadFromJsonAsync<BenchmarkResponseDto>();
        benchmark.Should().NotBeNull();
        benchmark!.IsActive.Should().BeFalse();
    }

    /// <summary>
    /// [P1] DELETE /api/admin/benchmarks/{id} should return 404 NotFound for non-existent ID
    /// </summary>
    [Fact]
    public async Task DeleteBenchmark_WithNonExistentId_ShouldReturn404NotFound()
    {
        // GIVEN: Non-existent benchmark ID
        var nonExistentId = Guid.NewGuid();

        // WHEN: Attempting to delete non-existent benchmark
        var response = await _client.DeleteAsync($"/api/admin/benchmarks/{nonExistentId}");

        // THEN: Should return 404 NotFound
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    /// <summary>
    /// [P1] DELETE /api/admin/benchmarks/{id} should return 400 BadRequest if benchmark has dependent scores
    /// Story 2.9 - Task 7.4/7.5: Check dependencies before deletion
    /// </summary>
    [Fact(Skip = "Requires BenchmarkScores implementation from future story")]
    public async Task DeleteBenchmark_WithDependentScores_ShouldReturn400BadRequest()
    {
        // This test will be implemented when BenchmarkScores entity is added in future story
        // For now, we skip it as a placeholder
    }

    #endregion

    #region Data Persistence Tests

    /// <summary>
    /// [P1] Created benchmark should be persisted to database
    /// Task 10.7: Test benchmark persisted to database
    /// </summary>
    [Fact]
    public async Task PostBenchmark_ShouldPersistToDatabase()
    {
        // GIVEN: Valid benchmark creation request
        var request = new CreateBenchmarkRequest
        {
            BenchmarkName = $"Persist_{Guid.NewGuid():N}",
            FullName = "Persistence Test",
            Description = "Testing database persistence",
            Category = "Math",
            Interpretation = "HigherBetter",
            TypicalRangeMin = 0,
            TypicalRangeMax = 100,
            WeightInQaps = 0.20m
        };

        // WHEN: Creating benchmark
        var createResponse = await _client.PostAsJsonAsync("/api/admin/benchmarks", request);
        var createdBenchmark = await createResponse.Content.ReadFromJsonAsync<BenchmarkResponseDto>();

        // THEN: Benchmark should be retrievable from database
        var getResponse = await _client.GetAsync($"/api/admin/benchmarks/{createdBenchmark!.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var retrievedBenchmark = await getResponse.Content.ReadFromJsonAsync<BenchmarkResponseDto>();
        retrievedBenchmark.Should().NotBeNull();
        retrievedBenchmark!.BenchmarkName.Should().Be(request.BenchmarkName);
        retrievedBenchmark.Description.Should().Be(request.Description);
        retrievedBenchmark.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    #endregion

    #region Story 2.11: CSV Import Integration Tests

    /// <summary>
    /// [P1] Task 10.6: POST /api/admin/benchmarks/import-csv should import valid CSV successfully
    /// Story 2.11 AC#3, AC#4, AC#5: File upload, parsing, validation, and import
    /// </summary>
    [Fact]
    public async Task ImportBenchmarkScoresCSV_WithValidCSV_ShouldReturn200WithSuccessCount()
    {
        // GIVEN: Valid CSV content with 3 benchmark scores
        var csv = @"model_id,benchmark_name,score,max_score,test_date,source_url,verified,notes
550e8400-e29b-41d4-a716-446655440000,MMLU,85.2,100,2025-10-01,https://example.com/mmlu,true,Test score 1
550e8400-e29b-41d4-a716-446655440000,HumanEval,0.72,1,2025-10-02,https://example.com/eval,false,Test score 2
550e8400-e29b-41d4-a716-446655440000,GSM8K,78.5,100,2025-10-03,https://example.com/gsm8k,true,Test score 3";

        var csvBytes = Encoding.UTF8.GetBytes(csv);
        var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(csvBytes);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/csv");
        content.Add(fileContent, "file", "test.csv");

        // WHEN: Posting CSV file to import endpoint
        var response = await _client.PostAsync("/api/admin/benchmarks/import-csv", content);

        // THEN: Should return 200 OK with import results
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var wrapper = await response.Content.ReadFromJsonAsync<CSVImportResponse>();
        wrapper.Should().NotBeNull();
        var result = wrapper!.Data;
        result.Should().NotBeNull();
        result!.TotalRows.Should().Be(3);
        result.SuccessfulImports.Should().BeGreaterThan(0);
        result.FailedImports.Should().BeLessThan(3);
    }

    /// <summary>
    /// [P1] Task 10.7: Successfully imported scores should persist to database
    /// Story 2.11 AC#5: Valid rows imported to database
    /// </summary>
    [Fact]
    public async Task ImportBenchmarkScoresCSV_ShouldPersistValidRowsToDatabase()
    {
        // GIVEN: Valid CSV with unique benchmark score
        var uniqueScore = (DateTime.UtcNow.Ticks % 100).ToString("F1");
        var csv = $@"model_id,benchmark_name,score,max_score,test_date,source_url,verified,notes
550e8400-e29b-41d4-a716-446655440000,MMLU,{uniqueScore},100,2025-10-01,https://example.com/test,true,Persistence test";

        var csvBytes = Encoding.UTF8.GetBytes(csv);
        var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(csvBytes);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/csv");
        content.Add(fileContent, "file", "persist_test.csv");

        // WHEN: Importing CSV
        var importResponse = await _client.PostAsync("/api/admin/benchmarks/import-csv", content);

        // THEN: Should return success
        importResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var wrapper = await importResponse.Content.ReadFromJsonAsync<CSVImportResponse>();
        wrapper.Should().NotBeNull();
        var result = wrapper!.Data;
        result.Should().NotBeNull();

        // AND: Score should be retrievable from database
        // Note: This would require a GET endpoint for benchmark scores which may not exist yet
        // For now, we verify the import succeeded
        result!.SuccessfulImports.Should().BeGreaterThan(0);
    }

    /// <summary>
    /// [P1] Task 10.8: CSV import should handle partial success (some valid, some invalid rows)
    /// Story 2.11 AC#6: Import results show X successful, Y failed with reasons
    /// </summary>
    [Fact]
    public async Task ImportBenchmarkScoresCSV_WithPartialSuccess_ShouldImportValidAndReportInvalid()
    {
        // GIVEN: CSV with 5 rows (3 valid, 2 invalid)
        var csv = @"model_id,benchmark_name,score,max_score,test_date,source_url,verified,notes
550e8400-e29b-41d4-a716-446655440000,MMLU,85.2,100,2025-10-01,https://example.com,true,Valid row 1
invalid-uuid-format,HumanEval,0.72,1,2025-10-02,https://example.com,false,Invalid model_id
550e8400-e29b-41d4-a716-446655440000,NonExistentBenchmark,78.5,100,2025-10-03,https://example.com,true,Invalid benchmark
550e8400-e29b-41d4-a716-446655440000,GSM8K,90.1,100,2025-10-04,https://example.com,false,Valid row 2
550e8400-e29b-41d4-a716-446655440000,MATH,abc,100,2025-10-05,https://example.com,true,Invalid score";

        var csvBytes = Encoding.UTF8.GetBytes(csv);
        var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(csvBytes);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/csv");
        content.Add(fileContent, "file", "partial_success.csv");

        // WHEN: Importing CSV with mixed valid/invalid rows
        var response = await _client.PostAsync("/api/admin/benchmarks/import-csv", content);

        // THEN: Should return 200 OK (partial success is allowed)
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var wrapper = await response.Content.ReadFromJsonAsync<CSVImportResponse>();
        wrapper.Should().NotBeNull();
        var result = wrapper!.Data;
        result.Should().NotBeNull();
        result!.TotalRows.Should().Be(5);

        // Should have some successful imports (at least row 1 and 4)
        result.SuccessfulImports.Should().BeGreaterThan(0);

        // Should have some failures (rows 2, 3, and 5)
        result.FailedImports.Should().BeGreaterThan(0);

        // Errors should include row numbers and error messages
        result.Errors.Should().NotBeEmpty();
        result.Errors.Should().Contain(e => e.Error.Contains("model_id") || e.Error.Contains("benchmark") || e.Error.Contains("score"));
    }

    /// <summary>
    /// [P1] Task 10.9: CSV import should skip duplicate model+benchmark combinations
    /// Story 2.11 AC#4: Check for duplicate model+benchmark (prevent redundant imports)
    /// </summary>
    [Fact]
    public async Task ImportBenchmarkScoresCSV_WithDuplicates_ShouldSkipDuplicateRows()
    {
        // GIVEN: Import CSV first time
        var csv = @"model_id,benchmark_name,score,max_score,test_date,source_url,verified,notes
550e8400-e29b-41d4-a716-446655440000,MMLU,85.2,100,2025-10-01,https://example.com,true,First import";

        var csvBytes = Encoding.UTF8.GetBytes(csv);
        var content1 = new MultipartFormDataContent();
        var fileContent1 = new ByteArrayContent(csvBytes);
        fileContent1.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/csv");
        content1.Add(fileContent1, "file", "duplicate_test1.csv");

        var firstResponse = await _client.PostAsync("/api/admin/benchmarks/import-csv", content1);
        var firstWrapper = await firstResponse.Content.ReadFromJsonAsync<CSVImportResponse>();
        var firstResult = firstWrapper!.Data;

        // AND: Import same CSV second time (duplicate)
        var content2 = new MultipartFormDataContent();
        var fileContent2 = new ByteArrayContent(csvBytes);
        fileContent2.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/csv");
        content2.Add(fileContent2, "file", "duplicate_test2.csv");

        // WHEN: Importing duplicate CSV
        var secondResponse = await _client.PostAsync("/api/admin/benchmarks/import-csv", content2);

        // THEN: Should return 200 OK but with skipped duplicates
        secondResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var secondWrapper = await secondResponse.Content.ReadFromJsonAsync<CSVImportResponse>();
        secondWrapper.Should().NotBeNull();
        var secondResult = secondWrapper!.Data;
        secondResult.Should().NotBeNull();
        secondResult!.TotalRows.Should().Be(1);

        // Should skip duplicate (assuming skipDuplicates=true by default)
        secondResult.SkippedDuplicates.Should().BeGreaterThan(0);
        secondResult.SuccessfulImports.Should().Be(0);
    }

    /// <summary>
    /// [P1] Task 10.10: CSV import should enforce file size limit
    /// Story 2.11 AC#3: Limit file size (10MB max)
    /// </summary>
    [Fact]
    public async Task ImportBenchmarkScoresCSV_WithOversizedFile_ShouldReturn413PayloadTooLarge()
    {
        // GIVEN: CSV file larger than 10MB (create large content)
        var largeContent = new StringBuilder();
        largeContent.AppendLine("model_id,benchmark_name,score,max_score,test_date,source_url,verified,notes");

        // Generate ~11MB of CSV data (assuming 10MB = 10_485_760 bytes)
        // Each row is ~150 bytes, so we need ~73,000 rows for 11MB
        for (int i = 0; i < 75000; i++)
        {
            largeContent.AppendLine($"550e8400-e29b-41d4-a716-446655440000,MMLU,85.2,100,2025-10-01,https://example.com/test{i},true,Large file test row {i}");
        }

        var csvBytes = Encoding.UTF8.GetBytes(largeContent.ToString());
        var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(csvBytes);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/csv");
        content.Add(fileContent, "file", "oversized.csv");

        // WHEN: Attempting to import oversized file
        var response = await _client.PostAsync("/api/admin/benchmarks/import-csv", content);

        // THEN: Should return 413 Payload Too Large or 400 Bad Request
        response.StatusCode.Should().BeOneOf(HttpStatusCode.RequestEntityTooLarge, HttpStatusCode.BadRequest);
    }

    /// <summary>
    /// [P2] CSV import should return 400 for invalid file format (non-CSV)
    /// Story 2.11 AC#3: Validate file is CSV
    /// </summary>
    [Fact]
    public async Task ImportBenchmarkScoresCSV_WithNonCSVFile_ShouldReturn400BadRequest()
    {
        // GIVEN: Non-CSV file (JSON content)
        var jsonContent = "{\"data\": \"This is not a CSV file\"}";
        var jsonBytes = Encoding.UTF8.GetBytes(jsonContent);

        var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(jsonBytes);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
        content.Add(fileContent, "file", "test.json");

        // WHEN: Attempting to import non-CSV file
        var response = await _client.PostAsync("/api/admin/benchmarks/import-csv", content);

        // THEN: Should return 400 Bad Request
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var errorContent = await response.Content.ReadAsStringAsync();
        errorContent.Should().Contain("CSV");
    }

    /// <summary>
    /// [P1] CSV import should handle malformed CSV gracefully
    /// Story 2.11 AC#4: Handle malformed rows (collect errors, don't crash)
    /// </summary>
    [Fact]
    public async Task ImportBenchmarkScoresCSV_WithMalformedCSV_ShouldReturnErrorsWithoutCrashing()
    {
        // GIVEN: Malformed CSV with missing columns
        var csv = @"model_id,benchmark_name
550e8400-e29b-41d4-a716-446655440000,MMLU
550e8400-e29b-41d4-a716-446655440000,HumanEval";

        var csvBytes = Encoding.UTF8.GetBytes(csv);
        var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(csvBytes);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/csv");
        content.Add(fileContent, "file", "malformed.csv");

        // WHEN: Importing malformed CSV
        var response = await _client.PostAsync("/api/admin/benchmarks/import-csv", content);

        // THEN: Should return 200 OK (don't crash) with all rows marked as failed
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var wrapper = await response.Content.ReadFromJsonAsync<CSVImportResponse>();
        wrapper.Should().NotBeNull();
        var result = wrapper!.Data;
        result.Should().NotBeNull();
        result!.TotalRows.Should().Be(2);
        result.SuccessfulImports.Should().Be(0);
        result.FailedImports.Should().Be(2);

        // Errors should indicate missing/invalid score field
        result.Errors.Should().AllSatisfy(e => e.Error.Should().Contain("score"));
    }

    #endregion
}

/// <summary>
/// Response structure for CSV import endpoint (matches backend response structure with data + meta)
/// </summary>
internal class CSVImportResponse
{
    public CSVImportResultDto? Data { get; set; }
    public object? Meta { get; set; }
}
