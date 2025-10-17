# Story 2.9: Create Benchmark Definitions Management

Status: Ready

## Story

As an administrator,
I want to manage benchmark definitions,
so that I can add new benchmarks for scoring models.

## Acceptance Criteria

1. Benchmarks management page created in admin panel
2. List view shows all benchmark definitions
3. Add benchmark form includes: name, full_name, description, category, interpretation, typical_range
4. POST `/api/admin/benchmarks` endpoint creates benchmark definition
5. Edit and delete functionality for benchmarks
6. Validation ensures benchmark names are unique

## Tasks / Subtasks

- [ ] **Task 1: Create benchmarks management page** (AC: #1, #2)
  - [ ] 1.1: Create `AdminBenchmarksPage.tsx` in `/frontend/src/pages/admin`
  - [ ] 1.2: Create `BenchmarkList.tsx` component in `/frontend/src/components/admin`
  - [ ] 1.3: Fetch benchmarks using `useQuery` hook
  - [ ] 1.4: Display benchmarks in table with columns: name, full_name, category, typical_range, actions
  - [ ] 1.5: Add "Add New Benchmark" button
  - [ ] 1.6: Add search/filter by category
  - [ ] 1.7: Style with TailwindCSS consistent with models page

- [ ] **Task 2: Create add benchmark form** (AC: #3)
  - [ ] 2.1: Create `BenchmarkForm.tsx` component in `/frontend/src/components/admin`
  - [ ] 2.2: Add "Benchmark Name" text input (short name, e.g., "MMLU")
  - [ ] 2.3: Add "Full Name" text input (e.g., "Massive Multitask Language Understanding")
  - [ ] 2.4: Add "Description" textarea (explain what benchmark measures)
  - [ ] 2.5: Add "Category" dropdown (reasoning, code, math, language, multimodal)
  - [ ] 2.6: Add "Interpretation" dropdown (higher_better, lower_better)
  - [ ] 2.7: Add "Typical Range Min" number input
  - [ ] 2.8: Add "Typical Range Max" number input
  - [ ] 2.9: Add "Weight in QAPS" number input (0.00 - 1.00, default 0.00)
  - [ ] 2.10: Implement form validation with Zod schema

- [ ] **Task 3: Create benchmark validation schema** (AC: #6)
  - [ ] 3.1: Create Zod schema for benchmark creation
  - [ ] 3.2: Validate benchmark_name: required, max 50 chars, alphanumeric + underscore
  - [ ] 3.3: Validate full_name: required, max 255 chars
  - [ ] 3.4: Validate category: required, one of enum values
  - [ ] 3.5: Validate typical_range_min < typical_range_max
  - [ ] 3.6: Validate weight: 0.00 to 1.00, max 2 decimal places
  - [ ] 3.7: Add helpful error messages for all validations

- [ ] **Task 4: Create POST benchmark endpoint** (AC: #4, #6)
  - [ ] 4.1: Create `AdminBenchmarksController.cs` in `/backend/src/Backend.API/Controllers/Admin`
  - [ ] 4.2: Add POST action accepting `CreateBenchmarkDto`
  - [ ] 4.3: Add [Authorize] attribute for JWT authentication
  - [ ] 4.4: Call `AdminBenchmarkService.CreateBenchmarkAsync()`
  - [ ] 4.5: Return 201 Created with new benchmark
  - [ ] 4.6: Return 400 if benchmark name already exists
  - [ ] 4.7: Add Swagger/OpenAPI documentation

- [ ] **Task 5: Implement benchmark creation service** (AC: #4, #6)
  - [ ] 5.1: Create `AdminBenchmarkService.cs` in `/Backend.Application/Services`
  - [ ] 5.2: Inject `IBenchmarkRepository`
  - [ ] 5.3: Check for duplicate benchmark name
  - [ ] 5.4: Create Benchmark entity from DTO
  - [ ] 5.5: Save to database
  - [ ] 5.6: Invalidate benchmark list cache
  - [ ] 5.7: Create audit log entry
  - [ ] 5.8: Return created benchmark

- [ ] **Task 6: Add edit benchmark functionality** (AC: #5)
  - [ ] 6.1: Create edit benchmark page/modal
  - [ ] 6.2: Fetch existing benchmark data
  - [ ] 6.3: Pre-populate form with current values
  - [ ] 6.4: Create PUT `/api/admin/benchmarks/{id}` endpoint
  - [ ] 6.5: Implement `UpdateBenchmarkAsync` in service
  - [ ] 6.6: Prevent changing benchmark_name (immutable identifier)
  - [ ] 6.7: Update weight_in_qaps if changed (invalidate QAPS cache)

- [ ] **Task 7: Add delete benchmark functionality** (AC: #5)
  - [ ] 7.1: Add delete button with confirmation dialog
  - [ ] 7.2: Create DELETE `/api/admin/benchmarks/{id}` endpoint
  - [ ] 7.3: Implement `DeleteBenchmarkAsync` in service
  - [ ] 7.4: Check if benchmark has associated scores before deleting
  - [ ] 7.5: Prevent deletion if scores exist (or cascade delete with warning)
  - [ ] 7.6: Soft-delete option: set is_active=false
  - [ ] 7.7: Create audit log entry

- [ ] **Task 8: Create DTOs and validators**
  - [ ] 8.1: Create `CreateBenchmarkDto.cs` in `/Backend.Application/DTOs`
  - [ ] 8.2: Create `BenchmarkResponseDto.cs`
  - [ ] 8.3: Create `CreateBenchmarkValidator.cs` using FluentValidation
  - [ ] 8.4: Validate all fields match client-side Zod schema
  - [ ] 8.5: Add unique name validation in validator
  - [ ] 8.6: Create AutoMapper profile for Benchmark entity

- [ ] **Task 9: Add category and interpretation enums**
  - [ ] 9.1: Create BenchmarkCategory enum (Reasoning, Code, Math, Language, Multimodal)
  - [ ] 9.2: Create BenchmarkInterpretation enum (HigherBetter, LowerBetter)
  - [ ] 9.3: Use enums in entity, DTOs, and validation
  - [ ] 9.4: Ensure frontend and backend enums match

- [ ] **Task 10: Add testing**
  - [ ] 10.1: Write component tests for BenchmarkForm (Vitest)
  - [ ] 10.2: Test form validation (required fields, range validation)
  - [ ] 10.3: Write unit tests for CreateBenchmarkValidator
  - [ ] 10.4: Write unit tests for AdminBenchmarkService
  - [ ] 10.5: Write integration tests for POST endpoint
  - [ ] 10.6: Test duplicate name returns 400
  - [ ] 10.7: Test benchmark persisted to database
  - [ ] 10.8: Test edit and delete endpoints

## Dev Notes

### Architecture Context

**Benchmark Management Flow:**
```
Admin creates benchmark definition
  → Used in model scoring (Story 2.10)
  → Used in QAPS calculation (Epic 6)
  → Weight determines contribution to composite quality score
```

**Benchmark Definition Purpose:**
- Define available benchmarks for scoring models
- Store metadata (category, interpretation, typical range)
- Configure weights for QAPS algorithm
- Validate benchmark scores against typical range

**Category-Weight Mapping (Reference):**
```
Reasoning: 0.30 (MMLU, Big-Bench Hard)
Code: 0.25 (HumanEval, MBPP)
Math: 0.20 (GSM8K, MATH)
Language: 0.15 (HellaSwag, TruthfulQA)
Multimodal: 0.10 (MMMU, VQA)
```

### Project Structure Notes

**Frontend Files to Create:**
```
/frontend/src/pages/admin/
  └── AdminBenchmarksPage.tsx

/frontend/src/components/admin/
  ├── BenchmarkList.tsx
  ├── BenchmarkForm.tsx
  └── BenchmarkRow.tsx

/frontend/src/hooks/
  ├── useBenchmarks.ts
  ├── useCreateBenchmark.ts
  ├── useUpdateBenchmark.ts
  └── useDeleteBenchmark.ts

/frontend/src/schemas/
  └── benchmarkSchema.ts

/frontend/src/api/
  └── admin.ts (add benchmark functions)
```

**Backend Files to Create:**
```
/backend/src/Backend.API/Controllers/Admin/
  └── AdminBenchmarksController.cs

/backend/src/Backend.Application/Services/
  └── AdminBenchmarkService.cs

/backend/src/Backend.Application/DTOs/
  ├── CreateBenchmarkDto.cs
  ├── UpdateBenchmarkDto.cs
  └── BenchmarkResponseDto.cs

/backend/src/Backend.Application/Validators/
  ├── CreateBenchmarkValidator.cs
  └── UpdateBenchmarkValidator.cs

/backend/src/Backend.Domain/Entities/
  └── Benchmark.cs (already exists from Epic 1)

/backend/src/Backend.Domain/Enums/
  ├── BenchmarkCategory.cs
  └── BenchmarkInterpretation.cs

/backend/src/Backend.Infrastructure/Repositories/
  └── BenchmarkRepository.cs
```

### Implementation Details

**Zod Validation Schema:**
```typescript
import { z } from 'zod'

export const benchmarkCategories = [
  'reasoning',
  'code',
  'math',
  'language',
  'multimodal'
] as const

export const createBenchmarkSchema = z.object({
  benchmarkName: z.string()
    .min(1, 'Benchmark name is required')
    .max(50, 'Maximum 50 characters')
    .regex(/^[a-zA-Z0-9_]+$/, 'Only letters, numbers, and underscores allowed'),
  fullName: z.string()
    .min(1, 'Full name is required')
    .max(255, 'Maximum 255 characters'),
  description: z.string()
    .max(1000, 'Maximum 1000 characters')
    .optional(),
  category: z.enum(benchmarkCategories, {
    errorMap: () => ({ message: 'Please select a valid category' })
  }),
  interpretation: z.enum(['higher_better', 'lower_better']),
  typicalRangeMin: z.number()
    .finite('Must be a valid number'),
  typicalRangeMax: z.number()
    .finite('Must be a valid number'),
  weightInQaps: z.number()
    .min(0, 'Weight must be between 0 and 1')
    .max(1, 'Weight must be between 0 and 1')
    .multipleOf(0.01, 'Maximum 2 decimal places')
    .default(0)
}).refine(data => data.typicalRangeMin < data.typicalRangeMax, {
  message: 'Min must be less than max',
  path: ['typicalRangeMax']
})
```

**CreateBenchmarkDto:**
```csharp
public class CreateBenchmarkDto
{
    public string BenchmarkName { get; set; }
    public string FullName { get; set; }
    public string Description { get; set; }
    public BenchmarkCategory Category { get; set; }
    public BenchmarkInterpretation Interpretation { get; set; }
    public decimal TypicalRangeMin { get; set; }
    public decimal TypicalRangeMax { get; set; }
    public decimal WeightInQaps { get; set; }
}

public enum BenchmarkCategory
{
    Reasoning,
    Code,
    Math,
    Language,
    Multimodal
}

public enum BenchmarkInterpretation
{
    HigherBetter,
    LowerBetter
}
```

**FluentValidation:**
```csharp
public class CreateBenchmarkValidator : AbstractValidator<CreateBenchmarkDto>
{
    private readonly IBenchmarkRepository _benchmarkRepo;

    public CreateBenchmarkValidator(IBenchmarkRepository benchmarkRepo)
    {
        _benchmarkRepo = benchmarkRepo;

        RuleFor(x => x.BenchmarkName)
            .NotEmpty().WithMessage("Benchmark name is required")
            .MaximumLength(50).WithMessage("Maximum 50 characters")
            .Matches(@"^[a-zA-Z0-9_]+$").WithMessage("Only letters, numbers, and underscores")
            .MustAsync(BeUniqueName).WithMessage("Benchmark name already exists");

        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required")
            .MaximumLength(255).WithMessage("Maximum 255 characters");

        RuleFor(x => x.Category)
            .IsInEnum().WithMessage("Invalid category");

        RuleFor(x => x.Interpretation)
            .IsInEnum().WithMessage("Invalid interpretation");

        RuleFor(x => x.TypicalRangeMin)
            .LessThan(x => x.TypicalRangeMax)
            .WithMessage("Min must be less than max");

        RuleFor(x => x.WeightInQaps)
            .InclusiveBetween(0m, 1m)
            .WithMessage("Weight must be between 0 and 1")
            .ScalePrecision(2, 3)
            .WithMessage("Maximum 2 decimal places");
    }

    private async Task<bool> BeUniqueName(string name, CancellationToken ct)
    {
        var existing = await _benchmarkRepo.GetByNameAsync(name);
        return existing == null;
    }
}
```

**AdminBenchmarkService:**
```csharp
public class AdminBenchmarkService
{
    private readonly IBenchmarkRepository _benchmarkRepo;
    private readonly ICacheService _cache;
    private readonly IAuditLogRepository _auditLog;

    public async Task<BenchmarkResponseDto> CreateBenchmarkAsync(CreateBenchmarkDto dto)
    {
        // 1. Check for duplicates (also done in validator)
        var existing = await _benchmarkRepo.GetByNameAsync(dto.BenchmarkName);
        if (existing != null)
            throw new ValidationException("Benchmark name already exists");

        // 2. Create benchmark entity
        var benchmark = new Benchmark
        {
            Id = Guid.NewGuid(),
            BenchmarkName = dto.BenchmarkName,
            FullName = dto.FullName,
            Description = dto.Description,
            Category = dto.Category.ToString(),
            Interpretation = dto.Interpretation.ToString(),
            TypicalRangeMin = dto.TypicalRangeMin,
            TypicalRangeMax = dto.TypicalRangeMax,
            WeightInQaps = dto.WeightInQaps,
            CreatedAt = DateTime.UtcNow
        };

        // 3. Save
        await _benchmarkRepo.AddAsync(benchmark);
        await _benchmarkRepo.SaveChangesAsync();

        // 4. Invalidate cache
        await _cache.RemovePatternAsync("cache:benchmarks:*");

        // 5. Audit log
        await _auditLog.LogAsync(new AuditLogEntry
        {
            AdminUser = GetCurrentUsername(),
            Action = "CREATE",
            EntityType = "BENCHMARK",
            EntityId = benchmark.Id,
            ChangesJson = JsonSerializer.Serialize(new { after = benchmark })
        });

        return _mapper.Map<BenchmarkResponseDto>(benchmark);
    }
}
```

**BenchmarkForm Component:**
```typescript
export function BenchmarkForm({ mode = 'create', defaultValues }: BenchmarkFormProps) {
  const form = useForm({
    resolver: zodResolver(createBenchmarkSchema),
    defaultValues
  })

  const createMutation = useCreateBenchmark()

  const onSubmit = (data) => {
    createMutation.mutate(data, {
      onSuccess: () => {
        toast.success('Benchmark created successfully')
        navigate('/admin/benchmarks')
      }
    })
  }

  return (
    <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-6">
      <div className="grid grid-cols-2 gap-4">
        <Input
          label="Benchmark Name"
          placeholder="MMLU"
          {...form.register('benchmarkName')}
          error={form.formState.errors.benchmarkName?.message}
        />
        <Input
          label="Full Name"
          placeholder="Massive Multitask Language Understanding"
          {...form.register('fullName')}
          error={form.formState.errors.fullName?.message}
        />
      </div>

      <Textarea
        label="Description"
        placeholder="Measures model's ability to..."
        {...form.register('description')}
        error={form.formState.errors.description?.message}
      />

      <div className="grid grid-cols-2 gap-4">
        <Select
          label="Category"
          {...form.register('category')}
          options={[
            { value: 'reasoning', label: 'Reasoning' },
            { value: 'code', label: 'Code' },
            { value: 'math', label: 'Math' },
            { value: 'language', label: 'Language' },
            { value: 'multimodal', label: 'Multimodal' }
          ]}
          error={form.formState.errors.category?.message}
        />
        <Select
          label="Interpretation"
          {...form.register('interpretation')}
          options={[
            { value: 'higher_better', label: 'Higher is Better' },
            { value: 'lower_better', label: 'Lower is Better' }
          ]}
          error={form.formState.errors.interpretation?.message}
        />
      </div>

      <div className="grid grid-cols-3 gap-4">
        <Input
          label="Typical Range Min"
          type="number"
          step="0.01"
          {...form.register('typicalRangeMin', { valueAsNumber: true })}
          error={form.formState.errors.typicalRangeMin?.message}
        />
        <Input
          label="Typical Range Max"
          type="number"
          step="0.01"
          {...form.register('typicalRangeMax', { valueAsNumber: true })}
          error={form.formState.errors.typicalRangeMax?.message}
        />
        <Input
          label="Weight in QAPS"
          type="number"
          step="0.01"
          min="0"
          max="1"
          placeholder="0.30"
          {...form.register('weightInQaps', { valueAsNumber: true })}
          error={form.formState.errors.weightInQaps?.message}
        />
      </div>

      <div className="flex gap-4">
        <Button type="submit" loading={createMutation.isPending}>
          {mode === 'create' ? 'Create Benchmark' : 'Save Changes'}
        </Button>
        <Button type="button" variant="secondary" onClick={() => navigate(-1)}>
          Cancel
        </Button>
      </div>
    </form>
  )
}
```

### References

- [Tech Spec Epic 2: docs/tech-spec-epic-2-8-summary.md#Epic 2]
- [Solution Architecture: docs/solution-architecture.md#3.1 Database Schema - benchmarks table]
- [Epics Document: docs/epics.md#Story 2.9]
- [QAPS Algorithm: docs/solution-architecture.md#5 Smart Filter Algorithm - Benchmark Weights]

### Testing Strategy

**Component Tests:**
- BenchmarkForm renders all fields correctly
- Validation shows errors for invalid data
- Form submission calls create mutation
- Category and interpretation dropdowns work
- Range validation enforces min < max
- Weight validation enforces 0-1 range

**Unit Tests:**
- CreateBenchmarkValidator validates all fields
- Unique name validation rejects duplicates
- AdminBenchmarkService creates benchmark
- Service invalidates cache after creation

**Integration Tests:**
- POST /api/admin/benchmarks with valid data returns 201
- Response includes created benchmark
- Benchmark persisted to database
- Duplicate name returns 400
- Invalid category/interpretation returns 400
- Audit log entry created

**E2E Test:**
- Admin logs in → navigates to benchmarks → adds new benchmark → sees in list

## Dev Agent Record

### Context Reference

<!-- Path(s) to story context XML will be added here by context workflow -->

### Agent Model Used

claude-sonnet-4-5-20250929

### Debug Log References

### Completion Notes List

### File List
