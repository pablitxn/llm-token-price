# Story 1.10: Create Basic GET API for Models List

Status: Ready

## Story

As a developer,
I want a REST API endpoint to retrieve all LLM models with their data,
So that the frontend can display the model list and I can verify the full stack works end-to-end.

## Acceptance Criteria

1. ModelDto record created in Application layer with all required fields mapped from Model entity
2. IModelQueryService interface created in Application layer defining GetAllModelsAsync method
3. ModelQueryService implementation created using repository pattern to query database
4. ModelsController created in API layer with GET /api/models endpoint returning list of ModelDto
5. Endpoint returns 200 OK with JSON array of models including capabilities and top 3 benchmark scores
6. Full stack integration test passes: frontend fetches models from backend API successfully

## Tasks / Subtasks

- [ ] Create DTOs in Application layer (AC: 1)
  - [ ] Create `Backend.Application/DTOs/ModelDto.cs` record with properties: Id, Name, Provider, Version, InputPricePer1M, OutputPricePer1M, Currency, LastUpdated
  - [ ] Create `Backend.Application/DTOs/CapabilityDto.cs` record with: ContextWindow, MaxOutputTokens, SupportsFunctionCalling, SupportsVision, SupportsStreaming, etc.
  - [ ] Create `Backend.Application/DTOs/BenchmarkScoreDto.cs` record with: BenchmarkName, Score, MaxScore
  - [ ] Update ModelDto to include nested DTOs: `CapabilityDto? Capabilities`, `List<BenchmarkScoreDto> TopBenchmarks`
  - [ ] Add XML documentation explaining each DTO's purpose

- [ ] Create repository interface in Domain layer (AC: 2)
  - [ ] Create `Backend.Domain/Repositories/IModelRepository.cs` interface
  - [ ] Define method: `Task<List<Model>> GetAllAsync(CancellationToken cancellationToken = default);`
  - [ ] Define method: `Task<Model?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);`
  - [ ] Add XML documentation explaining repository purpose (port for data access)

- [ ] Implement repository in Infrastructure layer (AC: 3)
  - [ ] Create `Backend.Infrastructure/Repositories/ModelRepository.cs` implementing `IModelRepository`
  - [ ] Inject AppDbContext via constructor
  - [ ] Implement GetAllAsync: `return await _context.Models.Include(m => m.Capability).Include(m => m.BenchmarkScores).ThenInclude(bs => bs.Benchmark).Where(m => m.IsActive).ToListAsync();`
  - [ ] Implement GetByIdAsync with similar includes
  - [ ] Register repository in DI: `builder.Services.AddScoped<IModelRepository, ModelRepository>();` in Program.cs

- [ ] Create query service interface and implementation (AC: 2, 3)
  - [ ] Create `Backend.Application/Services/IModelQueryService.cs` interface
  - [ ] Define method: `Task<List<ModelDto>> GetAllModelsAsync(CancellationToken cancellationToken = default);`
  - [ ] Create `Backend.Application/Services/ModelQueryService.cs` implementing interface
  - [ ] Inject IModelRepository via constructor
  - [ ] Implement GetAllModelsAsync: call repository, map entities to DTOs
  - [ ] Map Model → ModelDto including Capability → CapabilityDto, take top 3 benchmark scores ordered by score DESC
  - [ ] Register service in DI: `builder.Services.AddScoped<IModelQueryService, ModelQueryService>();`

- [ ] Create ModelsController in API layer (AC: 4, 5)
  - [ ] Create `Backend.API/Controllers/ModelsController.cs` with `[ApiController]` and `[Route("api/[controller]")]`
  - [ ] Inject IModelQueryService via constructor
  - [ ] Create GET endpoint: `[HttpGet] public async Task<IActionResult> GetAll(CancellationToken cancellationToken)`
  - [ ] Call service: `var models = await _queryService.GetAllModelsAsync(cancellationToken);`
  - [ ] Return success response: `return Ok(new { data = models, meta = new { count = models.Count, cached = false, timestamp = DateTime.UtcNow } });`
  - [ ] Add response type attributes: `[ProducesResponseType(typeof(ApiResponse<List<ModelDto>>), StatusCodes.Status200OK)]`
  - [ ] Add XML documentation with example response

- [ ] Test API endpoint (AC: 5)
  - [ ] Start backend: `dotnet run --project Backend.API`
  - [ ] Test endpoint: `curl http://localhost:5000/api/models` returns JSON array of 10 models
  - [ ] Verify response structure: `{ "data": [...], "meta": { "count": 10, "timestamp": "..." } }`
  - [ ] Verify model structure: each model has id, name, provider, pricing, capabilities object, topBenchmarks array
  - [ ] Verify camelCase JSON: properties like `inputPricePer1M`, not `InputPricePer1M`
  - [ ] Test via Swagger: open http://localhost:5000/swagger, execute GET /api/models, verify 200 OK

- [ ] Test frontend integration (AC: 6)
  - [ ] Start both backend (port 5000) and frontend (port 5173)
  - [ ] Create `frontend/src/api/models.ts` with `export const fetchModels = () => apiClient.get('/models')`
  - [ ] Update HomePage to fetch and display models using React Query
  - [ ] Add `useQuery` hook: `const { data, isLoading, error } = useQuery({ queryKey: ['models'], queryFn: fetchModels })`
  - [ ] Display model count or simple list in HomePage
  - [ ] Verify data flows: backend → API → frontend, models display in browser
  - [ ] Check browser Network tab: verify /api/models request succeeds, CORS headers present
  - [ ] Test end-to-end: refresh page, models load automatically

- [ ] Document API endpoint (AC: 1-6)
  - [ ] Update README.md with "API Endpoints" section listing GET /api/models
  - [ ] Document response format with example JSON
  - [ ] Document that endpoint returns only active models (is_active=true)
  - [ ] Update Swagger documentation with detailed descriptions
  - [ ] Verify all acceptance criteria met

## Dev Notes

### Architecture Constraints

**From solution-architecture.md Section 2.4 - API Structure:**
- **API endpoint pattern:** GET /api/models returns list of models
- **Response format:** `{ "data": {...}, "meta": {...} }` wrapper
- **Caching:** Not implemented in Epic 1, added in Epic 2

**From solution-architecture.md Section 2.1 - Hexagonal Architecture:**
- **Repository pattern:** IModelRepository (Domain) → ModelRepository (Infrastructure)
- **Application services:** IModelQueryService orchestrates use cases, maps entities to DTOs
- **Controllers:** Thin layer, delegates to application services

**From tech-spec-epic-1.md Story 1.10:**
- **ModelDto structure:** Includes nested CapabilityDto and top 3 BenchmarkScoreDto entries
- **Query optimization:** Use EF Core Include() to eagerly load relationships (avoid N+1 queries)

### Project Structure Notes

**Application layer:**
```
Backend.Application/
├── DTOs/
│   ├── ModelDto.cs
│   ├── CapabilityDto.cs
│   └── BenchmarkScoreDto.cs
└── Services/
    ├── IModelQueryService.cs
    └── ModelQueryService.cs
```

**Infrastructure repository:**
```
Backend.Infrastructure/
└── Repositories/
    └── ModelRepository.cs
```

**API controller:**
```
Backend.API/
└── Controllers/
    └── ModelsController.cs
```

### Testing Standards Summary

**API endpoint validation:**
1. Returns 200 OK
2. JSON structure: `{ "data": [...], "meta": {...} }`
3. Data array contains 10 models
4. Each model has all required fields
5. Capabilities nested correctly
6. Top 3 benchmarks included per model

**Frontend integration validation:**
1. API call succeeds (status 200)
2. Data deserialized correctly
3. No CORS errors
4. Models display in UI

### References

- [Source: docs/solution-architecture.md#Section 2.4 - API Structure and response format]
- [Source: docs/solution-architecture.md#Section 2.1 - Hexagonal Architecture with repositories]
- [Source: docs/tech-spec-epic-1.md#Story 1.10 - GET API for models list]
- [Source: docs/epics.md#Epic 1, Story 1.10 - Acceptance Criteria]

## Dev Agent Record

### Context Reference

<!-- Path(s) to story context XML will be added here by context workflow -->

### Agent Model Used

<!-- Agent model information will be populated during development -->

### Debug Log References

<!-- Debug logs will be added during development -->

### Completion Notes List

<!-- Completion notes will be added after story implementation -->

### File List

<!-- Modified/created files will be listed here after implementation -->
