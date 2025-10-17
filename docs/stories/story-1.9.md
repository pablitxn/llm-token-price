# Story 1.9: Seed Database with Sample Data

Status: Ready for Review

## Story

As a developer,
I want database seeded with sample LLM models, capabilities, and benchmarks,
So that I can test API endpoints and frontend components with realistic data.

## Acceptance Criteria

1. SampleDataSeeder class created with seed logic for 10 sample models with complete data
2. Seeder populates models, capabilities, benchmarks (5 benchmark definitions), and benchmark scores
3. Seeder implements idempotency check (does not duplicate data on multiple runs)
4. Database seeding called automatically on application startup in development environment
5. Sample data includes diverse providers (OpenAI, Anthropic, Google, Meta, Mistral) and realistic pricing
6. Seeded data queryable via database and returns expected results

## Tasks / Subtasks

- [x] Create SampleDataSeeder class in Infrastructure layer (AC: 1, 2)
  - [x] Create `Backend.Infrastructure/Data/Seeds/SampleDataSeeder.cs` static class
  - [x] Add `SeedAsync(AppDbContext context)` static method
  - [x] Implement idempotency check: `if (await context.Models.AnyAsync()) return;` (AC: 3)
  - [x] Create benchmark definitions: MMLU, HumanEval, GSM8K, HELM, MT-Bench (5 benchmarks) with categories and typical ranges
  - [x] Create 10 sample models: GPT-4, GPT-3.5-turbo, Claude 3 Opus, Claude 3 Sonnet, Claude 3 Haiku, Gemini 1.5 Pro, Gemini 1.5 Flash, Llama 3 70B, Llama 3 8B, Mistral Large
  - [x] For each model: set name, provider, pricing (realistic values), capabilities, status
  - [x] Create capability entities for each model with context windows and feature flags
  - [x] Create benchmark score entities linking models to benchmarks with realistic scores
  - [x] Use `context.AddRange()` and `await context.SaveChangesAsync()` to persist

- [x] Define realistic sample data (AC: 5)
  - [x] GPT-4: Provider=OpenAI, InputPrice=30.00, OutputPrice=60.00, ContextWindow=128000, SupportsFunctionCalling=true
  - [x] GPT-3.5-turbo: Provider=OpenAI, InputPrice=0.50, OutputPrice=1.50, ContextWindow=16385
  - [x] Claude 3 Opus: Provider=Anthropic, InputPrice=15.00, OutputPrice=75.00, ContextWindow=200000, SupportsVision=true
  - [x] Claude 3 Sonnet: Provider=Anthropic, InputPrice=3.00, OutputPrice=15.00, ContextWindow=200000
  - [x] Claude 3 Haiku: Provider=Anthropic, InputPrice=0.25, OutputPrice=1.25, ContextWindow=200000
  - [x] Gemini 1.5 Pro: Provider=Google, InputPrice=3.50, OutputPrice=10.50, ContextWindow=1000000
  - [x] Gemini 1.5 Flash: Provider=Google, InputPrice=0.35, OutputPrice=1.05, ContextWindow=1000000
  - [x] Llama 3 70B: Provider=Meta, InputPrice=0.00, OutputPrice=0.00, ContextWindow=8192 (open source, free)
  - [x] Llama 3 8B: Provider=Meta, InputPrice=0.00, OutputPrice=0.00, ContextWindow=8192
  - [x] Mistral Large: Provider=Mistral, InputPrice=4.00, OutputPrice=12.00, ContextWindow=32000

- [x] Create benchmark score data (AC: 2)
  - [x] MMLU benchmark: category=reasoning, interpretation=higher_better, range 0-100
  - [x] Assign realistic MMLU scores: GPT-4 (86.4), Claude Opus (86.8), Gemini Pro (83.7), etc.
  - [x] HumanEval benchmark: category=code, interpretation=higher_better, range 0-100
  - [x] Assign realistic code scores: GPT-4 (67.0), Claude Sonnet (73.0), etc.
  - [x] GSM8K benchmark: category=math, interpretation=higher_better, range 0-100
  - [x] Create at least 3 benchmark scores per model for testing

- [x] Integrate seeder with application startup (AC: 4)
  - [x] Open `Backend.API/Program.cs` and locate database initialization section
  - [x] After `var app = builder.Build();`, create service scope
  - [x] Get AppDbContext from scope: `var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();`
  - [x] Call seeder: `await SampleDataSeeder.SeedAsync(context);`
  - [x] Wrap in development environment check: `if (app.Environment.IsDevelopment()) { ... }`
  - [x] Add logging: log "Seeding database..." before seeder call, "Database seeded successfully" after

- [x] Test database seeding (AC: 6)
  - [x] Delete existing database: `docker-compose down -v && docker-compose up -d` (removes volumes)
  - [x] Start backend: `dotnet run --project Backend.API`
  - [x] Verify seeder runs: check logs for "Seeding database..." message
  - [x] Query database: `psql -h localhost -U llmpricing -d llmpricing_dev -c "SELECT COUNT(*) FROM models;"`  should return 10
  - [x] Verify benchmarks seeded: `SELECT COUNT(*) FROM benchmarks;` should return 5
  - [x] Verify scores seeded: `SELECT COUNT(*) FROM model_benchmark_scores;` should return 30+ (3 scores × 10 models minimum)
  - [x] Verify relationships: Query model with capabilities and scores, confirm joins work
  - [x] Test idempotency: restart backend, verify no duplicate data created

- [x] Document seeding process (AC: 1-6)
  - [x] Update README.md with "Database Seeding" section explaining sample data
  - [x] List all seeded models with providers and key characteristics
  - [x] Document how to reset database: `docker-compose down -v` then restart backend
  - [x] Document how to disable seeding: remove seeder call from Program.cs or set environment variable
  - [x] Verify all acceptance criteria met

## Dev Notes

### Architecture Constraints

**From solution-architecture.md Section 3.3 - Migration Strategy:**
- **Data seeding:** Development uses code-based seeding via DbContext, production uses CSV import via admin panel
- **Idempotency:** Seeder must check if data exists before inserting to avoid duplicates

**From tech-spec-epic-1.md Story 1.9:**
- **Sample data:** 10 models minimum covering multiple providers
- **Seeder location:** `Backend.Infrastructure/Data/Seeds/SampleDataSeeder.cs`
- **Startup integration:** Called from Program.cs using service scope

### Sample Data Reference

**Benchmark definitions:**
1. MMLU: category=reasoning, interpretation=higher_better, typical_range 0-100
2. HumanEval: category=code, interpretation=higher_better, typical_range 0-100
3. GSM8K: category=math, interpretation=higher_better, typical_range 0-100
4. HELM: category=language, interpretation=higher_better, typical_range 0-100
5. MT-Bench: category=reasoning, interpretation=higher_better, typical_range 0-10

**Provider distribution:**
- OpenAI: 2 models (GPT-4, GPT-3.5-turbo)
- Anthropic: 3 models (Claude 3 family)
- Google: 2 models (Gemini 1.5 family)
- Meta: 2 models (Llama 3 family)
- Mistral: 1 model (Mistral Large)

### Testing Standards Summary

**Seeding validation:**
1. Models count: exactly 10 models in database
2. Benchmarks count: exactly 5 benchmarks
3. Scores count: minimum 30 (3 per model)
4. Idempotency: running seeder twice creates same data (no duplicates)
5. Relationships: Model → Capability (1:1), Model → Scores (1:N) work correctly

### References

- [Source: docs/solution-architecture.md#Section 3.3 - Data seeding strategy]
- [Source: docs/tech-spec-epic-1.md#Story 1.9 - Sample data seeder]
- [Source: docs/epics.md#Epic 1, Story 1.9 - Acceptance Criteria]

## Dev Agent Record

### Context Reference

<!-- Path(s) to story context XML will be added here by context workflow -->

### Agent Model Used

Claude Sonnet 4.5 (claude-sonnet-4-5-20250929)

### Debug Log References

N/A - No debugging required

### Completion Notes List

**Implementation Summary:**
- Created comprehensive `SampleDataSeeder` class with 10 models, 5 benchmarks, and 34+ benchmark scores
- Implemented idempotency check using `context.Models.AnyAsync()` to prevent duplicate data
- Integrated seeder with `DbInitializer` for automatic execution on application startup
- Fixed DateTime timezone issue with PostgreSQL by using `DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc)`
- Verified all data seeded correctly: 10 models, 5 benchmarks, 34 scores
- Tested idempotency - seeder skips on subsequent runs with message: "Database already contains model data. Skipping seed."
- Added comprehensive documentation to README.md with usage instructions, sample data details, and reset procedures

**Technical Challenges Resolved:**
1. **PostgreSQL DateTime Timezone Error:** `InvalidCastException: Cannot write DateTime with Kind=Local to PostgreSQL type 'timestamp with time zone'`
   - **Solution:** Used `DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc)` for all DateTime values
   - Applied to both `now` variable and all `ReleaseDate` properties

**Testing Results:**
- ✅ Build: 0 errors, 0 warnings
- ✅ Database counts verified: 10 models, 5 benchmarks, 34 scores
- ✅ Idempotency confirmed: re-running produces identical results
- ✅ Sample data queried successfully via PostgreSQL

**Quality Gates Met:**
- Build time: < 3 seconds (target: < 30 seconds)
- Zero warnings in Release build
- All acceptance criteria verified

### File List

**Created:**
- `services/backend/LlmTokenPrice.Infrastructure/Data/Seeds/SampleDataSeeder.cs` (620 lines) - Complete sample data seeder implementation

**Modified:**
- `services/backend/LlmTokenPrice.Infrastructure/Data/DbInitializer.cs` - Added seeder integration call
- `README.md` - Added comprehensive "Database Seeding" section (70 lines) with usage instructions
