# Story 2.10: Create Benchmark Score Entry Form

Status: Ready

## Story

As an administrator,
I want to add benchmark scores for models,
so that performance data is captured for comparisons.

## Acceptance Criteria

1. Benchmark scores section added to model form (or separate page)
2. Form allows selecting model and benchmark from dropdowns
3. Score input field with validation (number within typical range)
4. Test date picker and source URL input
5. POST `/api/admin/models/{id}/benchmarks` endpoint saves score
6. Benchmark scores list displayed for each model

## Tasks / Subtasks

- [ ] **Task 1: Create benchmark score entry form** (AC: #1, #2)
  - [ ] 1.1: Create `BenchmarkScoreForm.tsx` component in `/frontend/src/components/admin`
  - [ ] 1.2: Add model selector dropdown (searchable, shows all models)
  - [ ] 1.3: Add benchmark selector dropdown (shows all available benchmarks)
  - [ ] 1.4: Group benchmarks by category in dropdown
  - [ ] 1.5: Display selected benchmark's typical range as helper text
  - [ ] 1.6: Show interpretation (higher/lower better) below benchmark selector

- [ ] **Task 2: Add score input fields** (AC: #3, #4)
  - [ ] 2.1: Add "Score" number input (required)
  - [ ] 2.2: Add "Max Score" number input (optional, for percentage benchmarks)
  - [ ] 2.3: Add "Test Date" date picker (optional, defaults to today)
  - [ ] 2.4: Add "Source URL" text input (optional, for linking to source)
  - [ ] 2.5: Add "Verified" checkbox (marks score as verified/official)
  - [ ] 2.6: Add "Notes" textarea (optional, for additional context)
  - [ ] 2.7: Format score input with appropriate decimal places

- [ ] **Task 3: Implement score validation** (AC: #3)
  - [ ] 3.1: Create Zod schema for benchmark score
  - [ ] 3.2: Validate score: required, number
  - [ ] 3.3: Validate score within typical range (warn if outside, don't block)
  - [ ] 3.4: Validate max_score >= score if provided
  - [ ] 3.5: Validate source_url is valid URL format if provided
  - [ ] 3.6: Show warning icon if score outside typical range
  - [ ] 3.7: Allow admin to override warning and submit

- [ ] **Task 4: Create POST benchmark score endpoint** (AC: #5)
  - [ ] 4.1: Create POST action in `AdminModelsController.cs` at `/api/admin/models/{id}/benchmarks`
  - [ ] 4.2: Accept `CreateBenchmarkScoreDto` from request body
  - [ ] 4.3: Add [Authorize] attribute for JWT authentication
  - [ ] 4.4: Call `AdminBenchmarkService.AddScoreAsync()`
  - [ ] 4.5: Return 201 Created with new score
  - [ ] 4.6: Return 400 if model or benchmark doesn't exist
  - [ ] 4.7: Return 400 if score already exists for this model+benchmark (prevent duplicates)

- [ ] **Task 5: Implement add score service method** (AC: #5)
  - [ ] 5.1: Create `AddScoreAsync(modelId, CreateBenchmarkScoreDto)` in AdminBenchmarkService
  - [ ] 5.2: Validate model exists
  - [ ] 5.3: Validate benchmark exists
  - [ ] 5.4: Check for existing score (model_id + benchmark_id unique)
  - [ ] 5.5: Calculate normalized_score: (score - min) / (max - min)
  - [ ] 5.6: Create ModelBenchmarkScore entity
  - [ ] 5.7: Save to database
  - [ ] 5.8: Invalidate model detail cache and best value cache
  - [ ] 5.9: Recalculate QAPS score for model (or mark for recalculation)
  - [ ] 5.10: Create audit log entry

- [ ] **Task 6: Display benchmark scores list** (AC: #6)
  - [ ] 6.1: Create `BenchmarkScoresList.tsx` component
  - [ ] 6.2: Display in model detail page or admin model edit page
  - [ ] 6.3: Show table with columns: benchmark name, score, max score, test date, verified
  - [ ] 6.4: Group scores by category
  - [ ] 6.5: Add edit/delete buttons for each score
  - [ ] 6.6: Highlight verified scores with badge
  - [ ] 6.7: Show "No scores yet" message if empty

- [ ] **Task 7: Add benchmark score to model edit page**
  - [ ] 7.1: Add "Benchmark Scores" section to EditModelPage
  - [ ] 7.2: Display existing scores list
  - [ ] 7.3: Add "Add New Score" button
  - [ ] 7.4: Open BenchmarkScoreForm in modal or inline
  - [ ] 7.5: Pre-select current model in form
  - [ ] 7.6: Refresh scores list after adding new score

- [ ] **Task 8: Create DTOs and validators**
  - [ ] 8.1: Create `CreateBenchmarkScoreDto.cs` in `/Backend.Application/DTOs`
  - [ ] 8.2: Create `BenchmarkScoreResponseDto.cs`
  - [ ] 8.3: Create `CreateBenchmarkScoreValidator.cs` using FluentValidation
  - [ ] 8.4: Validate score is number
  - [ ] 8.5: Warn (not error) if score outside typical range
  - [ ] 8.6: Validate max_score >= score if provided
  - [ ] 8.7: Validate source_url is valid URL

- [ ] **Task 9: Implement score normalization**
  - [ ] 9.1: Create `BenchmarkNormalizer` domain service
  - [ ] 9.2: Normalize score: `(score - min) / (max - min)`
  - [ ] 9.3: Handle edge cases: score = min (return 0), score = max (return 1)
  - [ ] 9.4: Handle scores outside range (extrapolate, or cap at 0/1)
  - [ ] 9.5: Store normalized_score in database for QAPS calculation

- [ ] **Task 10: Add edit/delete score functionality**
  - [ ] 10.1: Create PUT `/api/admin/models/{modelId}/benchmarks/{scoreId}` endpoint
  - [ ] 10.2: Create DELETE `/api/admin/models/{modelId}/benchmarks/{scoreId}` endpoint
  - [ ] 10.3: Implement `UpdateScoreAsync` and `DeleteScoreAsync` in service
  - [ ] 10.4: Recalculate normalized score and QAPS on update
  - [ ] 10.5: Invalidate cache on update/delete

- [ ] **Task 11: Add testing**
  - [ ] 11.1: Write component tests for BenchmarkScoreForm (Vitest)
  - [ ] 11.2: Test form validation (score range, max_score validation)
  - [ ] 11.3: Test warning displayed for out-of-range scores
  - [ ] 11.4: Write unit tests for BenchmarkNormalizer
  - [ ] 11.5: Write unit tests for AddScoreAsync service method
  - [ ] 11.6: Write integration tests for POST endpoint
  - [ ] 11.7: Test score persisted to database with normalized value
  - [ ] 11.8: Test duplicate score returns 400
  - [ ] 11.9: Test cache invalidated after adding score

## Dev Notes

### Architecture Context

**Benchmark Score Workflow:**
```
1. Admin selects model + benchmark
2. Enters score data
3. System validates score
4. System calculates normalized_score
5. System saves to model_benchmark_scores table
6. System invalidates cache
7. System triggers QAPS recalculation (or marks stale)
```

**Normalization Purpose:**
- QAPS algorithm needs scores on common scale (0-1)
- Different benchmarks have different ranges (0-100, 0-1, etc.)
- Normalization formula: `(score - min) / (max - min)`
- Enables weighted averaging across benchmarks

**Unique Constraint:**
- One score per model+benchmark combination
- UPDATE existing score if admin re-enters
- Or show error: "Score already exists, use edit instead"

### Project Structure Notes

**Frontend Files to Create:**
```
/frontend/src/components/admin/
  ├── BenchmarkScoreForm.tsx
  ├── BenchmarkScoresList.tsx
  └── BenchmarkScoreRow.tsx

/frontend/src/hooks/
  ├── useAddBenchmarkScore.ts
  ├── useUpdateBenchmarkScore.ts
  └── useDeleteBenchmarkScore.ts

/frontend/src/schemas/
  └── benchmarkScoreSchema.ts

/frontend/src/api/
  └── admin.ts (add benchmark score functions)
```

**Backend Files to Create/Modify:**
```
/backend/src/Backend.API/Controllers/Admin/
  └── AdminModelsController.cs (add benchmark score endpoints)

/backend/src/Backend.Application/Services/
  └── AdminBenchmarkService.cs (add score methods)

/backend/src/Backend.Application/DTOs/
  ├── CreateBenchmarkScoreDto.cs
  ├── UpdateBenchmarkScoreDto.cs
  └── BenchmarkScoreResponseDto.cs

/backend/src/Backend.Application/Validators/
  └── CreateBenchmarkScoreValidator.cs

/backend/src/Backend.Domain/Services/
  └── BenchmarkNormalizer.cs

/backend/src/Backend.Infrastructure/Repositories/
  └── BenchmarkRepository.cs (add score methods)
```

### Implementation Details

**Zod Validation Schema:**
```typescript
import { z } from 'zod'

export const createBenchmarkScoreSchema = z.object({
  modelId: z.string().uuid('Invalid model ID'),
  benchmarkId: z.string().uuid('Invalid benchmark ID'),
  score: z.number()
    .finite('Must be a valid number'),
  maxScore: z.number()
    .finite('Must be a valid number')
    .optional(),
  testDate: z.string().optional(), // ISO date
  sourceUrl: z.string()
    .url('Must be a valid URL')
    .optional()
    .or(z.literal('')),
  verified: z.boolean().default(false),
  notes: z.string().max(500).optional()
}).refine(data => {
  if (data.maxScore !== undefined && data.maxScore !== null) {
    return data.score <= data.maxScore
  }
  return true
}, {
  message: 'Score cannot exceed max score',
  path: ['score']
})
```

**CreateBenchmarkScoreDto:**
```csharp
public class CreateBenchmarkScoreDto
{
    public Guid BenchmarkId { get; set; }
    public decimal Score { get; set; }
    public decimal? MaxScore { get; set; }
    public DateTime? TestDate { get; set; }
    public string SourceUrl { get; set; }
    public bool Verified { get; set; } = false;
    public string Notes { get; set; }
}
```

**FluentValidation:**
```csharp
public class CreateBenchmarkScoreValidator : AbstractValidator<CreateBenchmarkScoreDto>
{
    public CreateBenchmarkScoreValidator()
    {
        RuleFor(x => x.BenchmarkId)
            .NotEmpty().WithMessage("Benchmark is required");

        RuleFor(x => x.Score)
            .NotNull().WithMessage("Score is required");

        When(x => x.MaxScore.HasValue, () =>
        {
            RuleFor(x => x.Score)
                .LessThanOrEqualTo(x => x.MaxScore.Value)
                .WithMessage("Score cannot exceed max score");
        });

        When(x => !string.IsNullOrEmpty(x.SourceUrl), () =>
        {
            RuleFor(x => x.SourceUrl)
                .Must(BeValidUrl)
                .WithMessage("Must be a valid URL");
        });
    }

    private bool BeValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out _);
    }
}
```

**BenchmarkNormalizer Service:**
```csharp
public class BenchmarkNormalizer
{
    public decimal Normalize(decimal score, decimal min, decimal max)
    {
        if (max == min) return 1m; // Edge case: same min/max

        var normalized = (score - min) / (max - min);

        // Clamp to 0-1 range (handle outliers)
        return Math.Max(0m, Math.Min(1m, normalized));
    }

    public bool IsWithinTypicalRange(decimal score, decimal min, decimal max)
    {
        return score >= min && score <= max;
    }
}
```

**AddScoreAsync Service:**
```csharp
public async Task<BenchmarkScoreResponseDto> AddScoreAsync(
    Guid modelId,
    CreateBenchmarkScoreDto dto)
{
    // 1. Validate model and benchmark exist
    var model = await _modelRepo.GetByIdAsync(modelId);
    if (model == null)
        throw new NotFoundException("Model not found");

    var benchmark = await _benchmarkRepo.GetByIdAsync(dto.BenchmarkId);
    if (benchmark == null)
        throw new NotFoundException("Benchmark not found");

    // 2. Check for duplicate
    var existing = await _benchmarkRepo.GetScoreAsync(modelId, dto.BenchmarkId);
    if (existing != null)
        throw new ValidationException($"Score already exists for {benchmark.BenchmarkName}");

    // 3. Calculate normalized score
    var normalizedScore = _normalizer.Normalize(
        dto.Score,
        benchmark.TypicalRangeMin,
        benchmark.TypicalRangeMax
    );

    // 4. Check if score outside typical range (warn, don't block)
    var isOutOfRange = !_normalizer.IsWithinTypicalRange(
        dto.Score,
        benchmark.TypicalRangeMin,
        benchmark.TypicalRangeMax
    );

    // 5. Create score entity
    var score = new ModelBenchmarkScore
    {
        Id = Guid.NewGuid(),
        ModelId = modelId,
        BenchmarkId = dto.BenchmarkId,
        Score = dto.Score,
        MaxScore = dto.MaxScore,
        NormalizedScore = normalizedScore,
        TestDate = dto.TestDate ?? DateTime.UtcNow,
        SourceUrl = dto.SourceUrl,
        Verified = dto.Verified,
        Notes = dto.Notes,
        CreatedAt = DateTime.UtcNow
    };

    // 6. Save
    await _benchmarkRepo.AddScoreAsync(score);
    await _benchmarkRepo.SaveChangesAsync();

    // 7. Invalidate cache
    await _cache.RemoveAsync($"cache:model:{modelId}:v1");
    await _cache.RemovePatternAsync("cache:bestvalue:*");

    // 8. Mark model for QAPS recalculation
    await MarkModelForQAPSRecalculation(modelId);

    // 9. Audit log
    await _auditLog.LogAsync(new AuditLogEntry
    {
        AdminUser = GetCurrentUsername(),
        Action = "CREATE",
        EntityType = "BENCHMARK_SCORE",
        EntityId = score.Id,
        ChangesJson = JsonSerializer.Serialize(new
        {
            after = score,
            warnings = isOutOfRange ? "Score outside typical range" : null
        })
    });

    var response = _mapper.Map<BenchmarkScoreResponseDto>(score);
    response.IsOutOfRange = isOutOfRange;
    return response;
}
```

**POST Endpoint:**
```csharp
[HttpPost("{modelId}/benchmarks")]
[Authorize]
[ProducesResponseType(typeof(BenchmarkScoreResponseDto), StatusCodes.Status201Created)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
public async Task<IActionResult> AddBenchmarkScore(
    Guid modelId,
    [FromBody] CreateBenchmarkScoreDto dto)
{
    var score = await _benchmarkService.AddScoreAsync(modelId, dto);

    return CreatedAtAction(
        nameof(GetModelBenchmarks),
        new { modelId },
        new { data = score, meta = new { message = "Benchmark score added successfully" } }
    );
}
```

**BenchmarkScoreForm Component:**
```typescript
export function BenchmarkScoreForm({ modelId }: { modelId: string }) {
  const { data: benchmarks } = useBenchmarks()
  const addScoreMutation = useAddBenchmarkScore()

  const form = useForm({
    resolver: zodResolver(createBenchmarkScoreSchema),
    defaultValues: { modelId, verified: false }
  })

  const selectedBenchmark = benchmarks?.find(
    b => b.id === form.watch('benchmarkId')
  )

  const onSubmit = (data) => {
    addScoreMutation.mutate(data, {
      onSuccess: () => {
        toast.success('Benchmark score added')
        form.reset()
      }
    })
  }

  return (
    <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
      <Select
        label="Benchmark"
        {...form.register('benchmarkId')}
        options={benchmarks?.map(b => ({
          value: b.id,
          label: `${b.benchmarkName} (${b.category})`,
          group: b.category
        }))}
        error={form.formState.errors.benchmarkId?.message}
      />

      {selectedBenchmark && (
        <div className="text-sm text-gray-600">
          <p>Typical Range: {selectedBenchmark.typicalRangeMin} - {selectedBenchmark.typicalRangeMax}</p>
          <p>Interpretation: {selectedBenchmark.interpretation === 'higher_better' ? 'Higher is better' : 'Lower is better'}</p>
        </div>
      )}

      <div className="grid grid-cols-2 gap-4">
        <Input
          label="Score"
          type="number"
          step="0.01"
          {...form.register('score', { valueAsNumber: true })}
          error={form.formState.errors.score?.message}
        />
        <Input
          label="Max Score (optional)"
          type="number"
          step="0.01"
          {...form.register('maxScore', { valueAsNumber: true })}
          error={form.formState.errors.maxScore?.message}
        />
      </div>

      <div className="grid grid-cols-2 gap-4">
        <DatePicker
          label="Test Date"
          {...form.register('testDate')}
        />
        <Input
          label="Source URL"
          type="url"
          placeholder="https://..."
          {...form.register('sourceUrl')}
          error={form.formState.errors.sourceUrl?.message}
        />
      </div>

      <Checkbox
        label="Verified"
        {...form.register('verified')}
      />

      <Textarea
        label="Notes"
        {...form.register('notes')}
        placeholder="Additional context..."
      />

      <Button type="submit" loading={addScoreMutation.isPending}>
        Add Score
      </Button>
    </form>
  )
}
```

### References

- [Tech Spec Epic 2: docs/tech-spec-epic-2-8-summary.md#Epic 2]
- [Solution Architecture: docs/solution-architecture.md#3.1 Database Schema - model_benchmark_scores]
- [Epics Document: docs/epics.md#Story 2.10]
- [Normalization: docs/solution-architecture.md#5.3 Benchmark Normalization]

### Testing Strategy

**Component Tests:**
- BenchmarkScoreForm renders all fields
- Benchmark selector groups by category
- Typical range displayed for selected benchmark
- Score validation enforces max_score >= score
- Form submission calls add score mutation

**Unit Tests:**
- BenchmarkNormalizer calculates normalized score correctly
- Normalizer handles edge cases (min=max, outliers)
- CreateBenchmarkScoreValidator validates all fields
- AddScoreAsync creates score with normalized value
- Service prevents duplicate scores

**Integration Tests:**
- POST /api/admin/models/{id}/benchmarks returns 201
- Score persisted to database with normalized value
- Duplicate score returns 400
- Non-existent model/benchmark returns 400
- Cache invalidated after adding score
- Audit log entry created

**E2E Test:**
- Admin edits model → adds benchmark score → sees score in list → score appears in public model detail

## Dev Agent Record

### Context Reference

<!-- Path(s) to story context XML will be added here by context workflow -->

### Agent Model Used

claude-sonnet-4-5-20250929

### Debug Log References

### Completion Notes List

### File List
