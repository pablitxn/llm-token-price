# Story 4.2: Backend API for Model Detail

Status: Ready

## Story

As a developer,
I want dedicated API endpoint for model details,
so that modal can fetch complete model data efficiently.

## Acceptance Criteria

1. GET `/api/models/{id}` endpoint created
2. Endpoint returns single model with all fields
3. Response includes complete capabilities object
4. Response includes all benchmark scores (not just top 3)
5. Endpoint returns 404 if model not found
6. Response cached in Redis (30 min TTL)

## Tasks / Subtasks

- [ ] **Task 1: Create ModelDetailDto** (AC: #2, #3, #4)
  - [ ] 1.1: Create `ModelDetailDto.cs` in `/services/backend/LlmTokenPrice.Application/DTOs`
  - [ ] 1.2: Add all Model properties (id, name, provider, version, release_date, status)
  - [ ] 1.3: Add pricing properties with breakdowns (per million, per thousand, per token)
  - [ ] 1.4: Add complete `CapabilitiesDto` nested object
  - [ ] 1.5: Add `List<BenchmarkScoreDetailDto>` for ALL benchmark scores
  - [ ] 1.6: Add price comparison properties (comparison_text, percentage_diff, compared_to_model)
  - [ ] 1.7: Add metadata (created_at, updated_at)

- [ ] **Task 2: Create BenchmarkScoreDetailDto** (AC: #4)
  - [ ] 2.1: Create `BenchmarkScoreDetailDto.cs` in DTOs folder
  - [ ] 2.2: Add properties: benchmark_name, full_name, category, score, max_score
  - [ ] 2.3: Add interpretation ("Higher is better"), test_date, source_url
  - [ ] 2.4: Document all properties with XML comments

- [ ] **Task 3: Create PriceComparisonDto** (AC: #2)
  - [ ] 3.1: Create `PriceComparisonDto.cs` in DTOs folder
  - [ ] 3.2: Add properties: comparison_text, percentage_difference, compared_to_model
  - [ ] 3.3: Document calculation logic in comments

- [ ] **Task 4: Implement GetModelByIdAsync service method** (AC: #1, #2, #6)
  - [ ] 4.1: Add `GetModelByIdAsync(Guid id)` method to IModelService interface
  - [ ] 4.2: Implement method in ModelService class
  - [ ] 4.3: Check Redis cache first with key `cache:model:detail:{id}`
  - [ ] 4.4: If cache miss, fetch from repository using `GetByIdWithAllDetailsAsync(id)`
  - [ ] 4.5: Map Model entity to ModelDetailDto (include all fields)
  - [ ] 4.6: Calculate price breakdown fields (per thousand = per million / 1000, etc.)
  - [ ] 4.7: Cache result in Redis with 30-minute TTL
  - [ ] 4.8: Return null if model not found

- [ ] **Task 5: Implement price comparison calculation** (AC: #2)
  - [ ] 5.1: Create `CalculatePriceComparisonAsync(Model model)` method in ModelService
  - [ ] 5.2: Query similar models (same provider OR similar capabilities)
  - [ ] 5.3: Calculate average total price of similar models
  - [ ] 5.4: Calculate percentage difference: ((current - avg) / avg) * 100
  - [ ] 5.5: Determine compared model (cheapest if current is expensive, most expensive if current is cheap)
  - [ ] 5.6: Generate comparison text: "X% cheaper/more expensive than {model_name}"
  - [ ] 5.7: Return PriceComparisonDto or null if no similar models

- [ ] **Task 6: Create GET /api/models/{id} endpoint** (AC: #1, #5)
  - [ ] 6.1: Add `GetModelById(Guid id)` action to ModelsController
  - [ ] 6.2: Add [HttpGet("{id}")] attribute
  - [ ] 6.3: Add [ResponseCache(Duration = 1800)] attribute (30 minutes)
  - [ ] 6.4: Call _modelService.GetModelByIdAsync(id)
  - [ ] 6.5: Return 404 NotFound if model is null
  - [ ] 6.6: Return 200 OK with ApiResponse<ModelDetailDto> wrapper
  - [ ] 6.7: Include meta object with cached flag and timestamp

- [ ] **Task 7: Update repository to include all details** (AC: #3, #4)
  - [ ] 7.1: Create `GetByIdWithAllDetailsAsync(Guid id)` in IModelRepository
  - [ ] 7.2: Implement in ModelRepository with EF Core
  - [ ] 7.3: Use .Include(m => m.Capability) to load capabilities
  - [ ] 7.4: Use .Include(m => m.BenchmarkScores).ThenInclude(bs => bs.Benchmark) to load all benchmarks
  - [ ] 7.5: Return model with all navigation properties loaded
  - [ ] 7.6: Return null if model not found or is_active = false

- [ ] **Task 8: Testing and validation**
  - [ ] 8.1: Write unit tests for GetModelByIdAsync service method (xUnit)
  - [ ] 8.2: Test cache hit scenario (verify no DB query on second call)
  - [ ] 8.3: Test 404 response for non-existent model ID
  - [ ] 8.4: Write integration test for GET /api/models/{id} endpoint (WebApplicationFactory)
  - [ ] 8.5: Test with Postman/curl: verify all fields present in response
  - [ ] 8.6: Verify price comparison calculation is correct
  - [ ] 8.7: Test Redis caching with 30-minute TTL

## Dev Notes

### Architecture Context

**DTO Extension Strategy:**
- `ModelDto` (Epic 3): Lightweight, top 3-5 benchmarks, list view
- `ModelDetailDto` (Epic 4): Complete, ALL benchmarks, detail view
- Separate DTOs avoid over-fetching in list endpoint

**Caching Strategy:**
- List endpoint (`/api/models`): 1 hour TTL (data changes infrequently)
- Detail endpoint (`/api/models/{id}`): 30 min TTL (more dynamic, price comparisons)
- Cache invalidation: On model update in admin panel (Epic 2)

**Price Comparison Logic:**
- "Similar models" = same provider OR similar context window (±20K tokens)
- Comparison shows relative position (cheaper/expensive vs average)
- Future enhancement: Compare within same "tier" (GPT-4 vs Claude Opus, not vs GPT-3.5)

### Project Structure Notes

**Backend Files to Create:**
```
/services/backend/
├── LlmTokenPrice.Application/
│   └── DTOs/
│       ├── ModelDetailDto.cs               # Extended model DTO
│       ├── BenchmarkScoreDetailDto.cs      # Full benchmark info
│       └── PriceComparisonDto.cs           # Price comparison context
├── LlmTokenPrice.Domain/
│   └── Interfaces/
│       └── IModelRepository.cs             # (update) Add GetByIdWithAllDetailsAsync
└── LlmTokenPrice.API/
    └── Controllers/
        └── ModelsController.cs             # (update) Add GetModelById action
```

### Implementation Details

**ModelDetailDto Structure:**
```csharp
// DTOs/ModelDetailDto.cs
public record ModelDetailDto
{
    // Basic info
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Provider { get; init; } = string.Empty;
    public string? Version { get; init; }
    public DateTime? ReleaseDate { get; init; }
    public string Status { get; init; } = "active";

    // Pricing (extended with breakdowns)
    public decimal InputPricePerMillion { get; init; }
    public decimal OutputPricePerMillion { get; init; }
    public decimal InputPricePerThousand { get; init; }   // Calculated: / 1000
    public decimal OutputPricePerThousand { get; init; }  // Calculated: / 1000
    public decimal InputPricePerToken { get; init; }      // Calculated: / 1,000,000
    public decimal OutputPricePerToken { get; init; }     // Calculated: / 1,000,000
    public string Currency { get; init; } = "USD";
    public DateTime? PricingValidFrom { get; init; }
    public DateTime? PricingValidTo { get; init; }

    // Complete capabilities
    public CapabilitiesDto Capabilities { get; init; } = null!;

    // ALL benchmarks (not just top 3-5)
    public List<BenchmarkScoreDetailDto> BenchmarkScores { get; init; } = new();

    // Price comparison context
    public PriceComparisonDto? PriceComparison { get; init; }

    // Metadata
    public DateTime UpdatedAt { get; init; }
    public DateTime CreatedAt { get; init; }
}
```

**Service Implementation:**
```csharp
// Application/Services/ModelService.cs
public async Task<ModelDetailDto?> GetModelByIdAsync(Guid id)
{
    var cacheKey = $"cache:model:detail:{id}";
    var cached = await _cache.GetAsync<ModelDetailDto>(cacheKey);

    if (cached != null) return cached;

    var model = await _repository.GetByIdWithAllDetailsAsync(id);

    if (model == null) return null;

    var modelDto = new ModelDetailDto
    {
        Id = model.Id,
        Name = model.Name,
        Provider = model.Provider,
        // ... map all fields
        InputPricePerThousand = model.InputPricePerMillion / 1000,
        InputPricePerToken = model.InputPricePerMillion / 1_000_000,
        // ... other calculated fields
        BenchmarkScores = model.BenchmarkScores
            .Select(bs => new BenchmarkScoreDetailDto
            {
                BenchmarkName = bs.Benchmark.BenchmarkName,
                FullName = bs.Benchmark.FullName,
                Category = bs.Benchmark.Category,
                Score = bs.Score,
                MaxScore = bs.MaxScore,
                Interpretation = bs.Benchmark.Interpretation,
                TestDate = bs.TestDate,
                SourceUrl = bs.SourceUrl
            })
            .ToList(),
        PriceComparison = await CalculatePriceComparisonAsync(model)
    };

    await _cache.SetAsync(cacheKey, modelDto, TimeSpan.FromMinutes(30));

    return modelDto;
}
```

**Price Comparison Logic:**
```csharp
private async Task<PriceComparisonDto?> CalculatePriceComparisonAsync(Model model)
{
    // Get similar models (same provider or similar context window)
    var similarModels = await _repository.GetSimilarModelsAsync(
        model.Id,
        model.Provider,
        model.Capability?.ContextWindow ?? 0
    );

    if (!similarModels.Any()) return null;

    var avgPrice = similarModels.Average(m => m.InputPricePerMillion + m.OutputPricePerMillion);
    var currentPrice = model.InputPricePerMillion + model.OutputPricePerMillion;
    var percentDiff = ((currentPrice - avgPrice) / avgPrice) * 100;

    var comparedTo = percentDiff < 0
        ? similarModels.OrderByDescending(m => m.InputPricePerMillion + m.OutputPricePerMillion).First()
        : similarModels.OrderBy(m => m.InputPricePerMillion + m.OutputPricePerMillion).First();

    var text = percentDiff < 0
        ? $"{Math.Abs(percentDiff):F0}% cheaper than {comparedTo.Name}"
        : $"{percentDiff:F0}% more expensive than {comparedTo.Name}";

    return new PriceComparisonDto
    {
        ComparisonText = text,
        PercentageDifference = percentDiff,
        ComparedToModel = comparedTo.Name
    };
}
```

**Controller Action:**
```csharp
// API/Controllers/ModelsController.cs
[HttpGet("{id}")]
[ResponseCache(Duration = 1800)] // 30 minutes
public async Task<ActionResult<ApiResponse<ModelDetailDto>>> GetModelById(Guid id)
{
    var model = await _modelService.GetModelByIdAsync(id);

    if (model == null)
    {
        return NotFound(new { Error = "Model not found" });
    }

    return Ok(new ApiResponse<ModelDetailDto>
    {
        Data = model,
        Meta = new { Cached = false, Timestamp = DateTime.UtcNow }
    });
}
```

### References

- [Epic 4 Analysis: docs/epic-4-analysis-and-plan.md#Story 4.2]
- [Solution Architecture: docs/solution-architecture.md#Caching Strategy]
- [Epics Document: docs/epics.md#Story 4.3] (note: 4.3 in original, 4.2 in refined)
- [ADR-016: Model Detail DTO Design]

### Testing Strategy

**Unit Tests:**
- GetModelByIdAsync returns correct ModelDetailDto for valid ID
- Returns null for non-existent ID
- Price breakdown calculations correct (per thousand, per token)
- Price comparison logic calculates percentage correctly
- Cache hit scenario: DB not queried on second call with same ID

**Integration Tests:**
- GET /api/models/{valid-id} returns 200 OK with complete data
- GET /api/models/{invalid-id} returns 404 Not Found
- Response includes all fields (basic info, pricing, capabilities, ALL benchmarks)
- Response cached in Redis with 30-minute TTL
- Cache invalidated when model updated in admin panel

**Performance Tests:**
- Endpoint responds in < 500ms with cache hit
- Endpoint responds in < 2 seconds with cache miss (DB query)
- Verify EF Core includes work correctly (no N+1 queries)

## Dev Agent Record

### Context Reference

<!-- Path(s) to story context XML will be added here by context workflow -->

### Agent Model Used

claude-sonnet-4-5-20250929

### Debug Log References

### Completion Notes List

### File List
