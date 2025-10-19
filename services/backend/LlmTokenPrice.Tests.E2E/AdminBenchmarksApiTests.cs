using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using LlmTokenPrice.Application.DTOs;
using Xunit;

namespace LlmTokenPrice.Tests.E2E;

/// <summary>
/// E2E API tests for Admin Benchmarks endpoints
/// Story 2.9 - Task 10.5-10.8: Integration tests for benchmark CRUD operations
/// Priority: P1 (High - API contract testing)
/// </summary>
[Collection("E2E Tests")]
public class AdminBenchmarksApiTests : IClassFixture<ApiTestFixture>
{
    private readonly ApiTestFixture _fixture;
    private readonly HttpClient _client;

    public AdminBenchmarksApiTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
        _client = fixture.CreateClient();
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
}
