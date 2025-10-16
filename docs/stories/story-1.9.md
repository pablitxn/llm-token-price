# Story 1.9: Seed Database with Sample Data

Status: Ready

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

- [ ] Create SampleDataSeeder class in Infrastructure layer (AC: 1, 2)
  - [ ] Create `Backend.Infrastructure/Data/Seeds/SampleDataSeeder.cs` static class
  - [ ] Add `SeedAsync(AppDbContext context)` static method
  - [ ] Implement idempotency check: `if (await context.Models.AnyAsync()) return;` (AC: 3)
  - [ ] Create benchmark definitions: MMLU, HumanEval, GSM8K, HELM, MT-Bench (5 benchmarks) with categories and typical ranges
  - [ ] Create 10 sample models: GPT-4, GPT-3.5-turbo, Claude 3 Opus, Claude 3 Sonnet, Claude 3 Haiku, Gemini 1.5 Pro, Gemini 1.5 Flash, Llama 3 70B, Llama 3 8B, Mistral Large
  - [ ] For each model: set name, provider, pricing (realistic values), capabilities, status
  - [ ] Create capability entities for each model with context windows and feature flags
  - [ ] Create benchmark score entities linking models to benchmarks with realistic scores
  - [ ] Use `context.AddRange()` and `await context.SaveChangesAsync()` to persist

- [ ] Define realistic sample data (AC: 5)
  - [ ] GPT-4: Provider=OpenAI, InputPrice=30.00, OutputPrice=60.00, ContextWindow=128000, SupportsFunctionCalling=true
  - [ ] GPT-3.5-turbo: Provider=OpenAI, InputPrice=0.50, OutputPrice=1.50, ContextWindow=16385
  - [ ] Claude 3 Opus: Provider=Anthropic, InputPrice=15.00, OutputPrice=75.00, ContextWindow=200000, SupportsVision=true
  - [ ] Claude 3 Sonnet: Provider=Anthropic, InputPrice=3.00, OutputPrice=15.00, ContextWindow=200000
  - [ ] Claude 3 Haiku: Provider=Anthropic, InputPrice=0.25, OutputPrice=1.25, ContextWindow=200000
  - [ ] Gemini 1.5 Pro: Provider=Google, InputPrice=3.50, OutputPrice=10.50, ContextWindow=1000000
  - [ ] Gemini 1.5 Flash: Provider=Google, InputPrice=0.35, OutputPrice=1.05, ContextWindow=1000000
  - [ ] Llama 3 70B: Provider=Meta, InputPrice=0.00, OutputPrice=0.00, ContextWindow=8192 (open source, free)
  - [ ] Llama 3 8B: Provider=Meta, InputPrice=0.00, OutputPrice=0.00, ContextWindow=8192
  - [ ] Mistral Large: Provider=Mistral, InputPrice=4.00, OutputPrice=12.00, ContextWindow=32000

- [ ] Create benchmark score data (AC: 2)
  - [ ] MMLU benchmark: category=reasoning, interpretation=higher_better, range 0-100
  - [ ] Assign realistic MMLU scores: GPT-4 (86.4), Claude Opus (86.8), Gemini Pro (83.7), etc.
  - [ ] HumanEval benchmark: category=code, interpretation=higher_better, range 0-100
  - [ ] Assign realistic code scores: GPT-4 (67.0), Claude Sonnet (73.0), etc.
  - [ ] GSM8K benchmark: category=math, interpretation=higher_better, range 0-100
  - [ ] Create at least 3 benchmark scores per model for testing

- [ ] Integrate seeder with application startup (AC: 4)
  - [ ] Open `Backend.API/Program.cs` and locate database initialization section
  - [ ] After `var app = builder.Build();`, create service scope
  - [ ] Get AppDbContext from scope: `var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();`
  - [ ] Call seeder: `await SampleDataSeeder.SeedAsync(context);`
  - [ ] Wrap in development environment check: `if (app.Environment.IsDevelopment()) { ... }`
  - [ ] Add logging: log "Seeding database..." before seeder call, "Database seeded successfully" after

- [ ] Test database seeding (AC: 6)
  - [ ] Delete existing database: `docker-compose down -v && docker-compose up -d` (removes volumes)
  - [ ] Start backend: `dotnet run --project Backend.API`
  - [ ] Verify seeder runs: check logs for "Seeding database..." message
  - [ ] Query database: `psql -h localhost -U llmpricing -d llmpricing_dev -c "SELECT COUNT(*) FROM models;"`  should return 10
  - [ ] Verify benchmarks seeded: `SELECT COUNT(*) FROM benchmarks;` should return 5
  - [ ] Verify scores seeded: `SELECT COUNT(*) FROM model_benchmark_scores;` should return 30+ (3 scores × 10 models minimum)
  - [ ] Verify relationships: Query model with capabilities and scores, confirm joins work
  - [ ] Test idempotency: restart backend, verify no duplicate data created

- [ ] Document seeding process (AC: 1-6)
  - [ ] Update README.md with "Database Seeding" section explaining sample data
  - [ ] List all seeded models with providers and key characteristics
  - [ ] Document how to reset database: `docker-compose down -v` then restart backend
  - [ ] Document how to disable seeding: remove seeder call from Program.cs or set environment variable
  - [ ] Verify all acceptance criteria met

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

<!-- Agent model information will be populated during development -->

### Debug Log References

<!-- Debug logs will be added during development -->

### Completion Notes List

<!-- Completion notes will be added after story implementation -->

### File List

<!-- Modified/created files will be listed here after implementation -->
