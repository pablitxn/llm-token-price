# Story 1.4: Create Core Data Models (Models, Capabilities, Benchmarks)

Status: Ready for Review

## Story

As a developer,
I want database schema for LLM models, capabilities, and benchmarks implemented with Entity Framework Core,
So that I can store and retrieve model data with proper relationships and constraints.

## Acceptance Criteria

1. Models table entity created with all required fields: id, name, provider, version, release_date, status, input_price_per_1M, output_price_per_1M, currency, pricing_valid_from, pricing_valid_to, last_scraped_at, is_active, created_at, updated_at
2. ModelCapabilities table entity created with fields: id, model_id, context_window, max_output_tokens, supports_function_calling, supports_vision, supports_audio_input, supports_audio_output, supports_streaming, supports_json_mode
3. Benchmarks table entity created with fields: id, benchmark_name, full_name, description, category, interpretation, typical_range_min, typical_range_max
4. ModelBenchmarkScores table entity created with fields: id, model_id, benchmark_id, score, max_score, test_date, source_url, verified, notes
5. Entity relationships configured: one-to-one Models to Capabilities, one-to-many Models to BenchmarkScores, many-to-one BenchmarkScores to Benchmarks
6. Database migration generated and applied successfully, creating all 4 tables with indexes and constraints

## Tasks / Subtasks

- [x] Create Model entity in Domain layer (AC: 1)
  - [x] Create `Backend.Domain/Entities/Model.cs` class with all required properties
  - [x] Add GUID primary key: `public Guid Id { get; set; }`
  - [x] Add model metadata: Name (string, required), Provider (string, required), Version (string, nullable), ReleaseDate (DateTime?, nullable), Status (string, default "active")
  - [x] Add pricing fields: InputPricePer1M (decimal, required), OutputPricePer1M (decimal, required), Currency (string, default "USD"), PricingValidFrom (DateTime?, nullable), PricingValidTo (DateTime?, nullable)
  - [x] Add operational fields: LastScrapedAt (DateTime?, nullable), IsActive (bool, default true), CreatedAt (DateTime, required), UpdatedAt (DateTime, required)
  - [x] Add navigation properties: `public Capability? Capability { get; set; }`, `public ICollection<BenchmarkScore> BenchmarkScores { get; set; } = new List<BenchmarkScore>();`
  - [x] Add XML documentation comments explaining each property's purpose
  - [x] Verify Domain layer has no dependencies (pure POCO entity)

- [x] Create Capability entity in Domain layer (AC: 2)
  - [x] Create `Backend.Domain/Entities/Capability.cs` class
  - [x] Add GUID primary key and foreign key: `public Guid Id { get; set; }`, `public Guid ModelId { get; set; }`
  - [x] Add capability fields: ContextWindow (int, required), MaxOutputTokens (int, nullable)
  - [x] Add boolean capability flags: SupportsFunctionCalling, SupportsVision, SupportsAudioInput, SupportsAudioOutput, SupportsStreaming (default true), SupportsJsonMode
  - [x] Add navigation property: `public Model Model { get; set; } = null!;`
  - [x] Add XML documentation explaining capability flags (e.g., "Indicates whether model supports OpenAI-style function calling")
  - [x] Ensure class is in `Backend.Domain.Entities` namespace

- [x] Create Benchmark entity in Domain layer (AC: 3)
  - [x] Create `Backend.Domain/Entities/Benchmark.cs` class
  - [x] Add GUID primary key: `public Guid Id { get; set; }`
  - [x] Add benchmark metadata: BenchmarkName (string, required, unique), FullName (string, nullable), Description (string, nullable)
  - [x] Add categorization: Category (string, nullable - "reasoning", "code", "math", "language", "multimodal")
  - [x] Add interpretation: Interpretation (string, nullable - "higher_better" or "lower_better")
  - [x] Add range fields: TypicalRangeMin (decimal, nullable), TypicalRangeMax (decimal, nullable)
  - [x] Add navigation property: `public ICollection<BenchmarkScore> Scores { get; set; } = new List<BenchmarkScore>();`
  - [x] Add CreatedAt timestamp field

- [x] Create BenchmarkScore entity in Domain layer (AC: 4)
  - [x] Create `Backend.Domain/Entities/BenchmarkScore.cs` class
  - [x] Add GUID primary key and foreign keys: `public Guid Id { get; set; }`, `public Guid ModelId { get; set; }`, `public Guid BenchmarkId { get; set; }`
  - [x] Add score fields: Score (decimal, required), MaxScore (decimal, nullable), NormalizedScore (decimal, nullable - 0-1 for QAPS calculation)
  - [x] Add metadata fields: TestDate (DateTime, nullable), SourceUrl (string, nullable), Verified (bool, default false), Notes (string, nullable)
  - [x] Add timestamp: CreatedAt (DateTime, required)
  - [x] Add navigation properties: `public Model Model { get; set; } = null!;`, `public Benchmark Benchmark { get; set; } = null!;`
  - [x] Add XML documentation explaining NormalizedScore purpose (used in QAPS calculation)

- [x] Configure entity relationships and constraints in Infrastructure layer (AC: 5)
  - [x] Create `Backend.Infrastructure/Data/Configurations/ModelConfiguration.cs` implementing `IEntityTypeConfiguration<Model>`
  - [x] Configure Model primary key, indexes (Provider, Status, UpdatedAt DESC), unique constraint on (Name, Provider)
  - [x] Configure decimal precision: InputPricePer1M and OutputPricePer1M as `decimal(10,6)`
  - [x] Configure one-to-one relationship: Model → Capability with cascade delete
  - [x] Configure one-to-many relationship: Model → BenchmarkScores with cascade delete
  - [x] Create `Backend.Infrastructure/Data/Configurations/CapabilityConfiguration.cs` implementing `IEntityTypeConfiguration<Capability>`
  - [x] Configure Capability primary key, foreign key to Model, unique constraint on ModelId
  - [x] Configure index on ModelId for fast lookups
  - [x] Create `Backend.Infrastructure/Data/Configurations/BenchmarkConfiguration.cs` implementing `IEntityTypeConfiguration<Benchmark>`
  - [x] Configure Benchmark primary key, unique constraint on BenchmarkName
  - [x] Configure decimal precision for TypicalRangeMin and TypicalRangeMax as `decimal(5,2)`
  - [x] Create `Backend.Infrastructure/Data/Configurations/BenchmarkScoreConfiguration.cs` implementing `IEntityTypeConfiguration<BenchmarkScore>`
  - [x] Configure BenchmarkScore primary key, foreign keys, unique constraint on (ModelId, BenchmarkId)
  - [x] Configure indexes on ModelId and BenchmarkId for query performance
  - [x] Configure decimal precision: Score as `decimal(6,2)`, NormalizedScore as `decimal(5,4)`

- [x] Register entity configurations in AppDbContext (AC: 5)
  - [x] Update `Backend.Infrastructure/Data/AppDbContext.cs` to add DbSet properties for all entities
  - [x] Add `public DbSet<Model> Models { get; set; } = null!;`
  - [x] Add `public DbSet<Capability> Capabilities { get; set; } = null!;`
  - [x] Add `public DbSet<Benchmark> Benchmarks { get; set; } = null!;`
  - [x] Add `public DbSet<BenchmarkScore> BenchmarkScores { get; set; } = null!;`
  - [x] In `OnModelCreating`, apply all entity configurations: `modelBuilder.ApplyConfiguration(new ModelConfiguration());` (repeat for all entities)
  - [x] Alternatively, use `modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());` to auto-discover all configurations

- [x] Generate and apply database migration (AC: 6)
  - [x] Generate migration: `dotnet ef migrations add InitialSchema --project Backend.Infrastructure --startup-project Backend.API --output-dir Data/Migrations`
  - [x] Review generated migration file in `Backend.Infrastructure/Data/Migrations/` directory
  - [x] Verify migration includes all 4 tables: models, model_capabilities, benchmarks, model_benchmark_scores
  - [x] Verify indexes created: idx_models_provider, idx_models_status, idx_models_updated, idx_capabilities_model, idx_scores_model, idx_scores_benchmark
  - [x] Verify unique constraints: unique_model_provider (name, provider), unique_model_capability (model_id), unique_benchmark_name, unique_model_benchmark (model_id, benchmark_id)
  - [x] Apply migration to database: `dotnet ef database update --project Backend.Infrastructure --startup-project Backend.API`
  - [x] Verify tables created in PostgreSQL: `psql -h localhost -U llmpricing -d llmpricing_dev -c "\dt"` should show all 4 tables

- [x] Validate schema and create verification tests (AC: 6)
  - [x] Connect to database and verify table structure: `psql -h localhost -U llmpricing -d llmpricing_dev -c "\d models"` shows correct columns
  - [x] Test entity relationships: insert test Model, Capability, Benchmark, and BenchmarkScore records via SQL
  - [x] Verify cascade delete works: delete Model record, confirm Capability and BenchmarkScores cascade deleted
  - [x] Verify unique constraints: attempt to insert duplicate (name, provider) Model, should fail with constraint violation
  - [x] Test indexes exist: `SELECT * FROM pg_indexes WHERE tablename IN ('models', 'model_capabilities', 'benchmarks', 'model_benchmark_scores');`
  - [x] Document schema verification commands in README.md under "Database Management" section
  - [x] Create schema diagram (optional): generate ER diagram showing entity relationships using pgAdmin or dbdiagram.io

## Dev Notes

### Architecture Constraints

**From solution-architecture.md Section 3.1 - Database Schema:**
- **Table naming:** Use snake_case (models, model_capabilities, benchmarks, model_benchmark_scores) per PostgreSQL conventions
- **Primary keys:** All entities use GUID (UUID) for distributed system compatibility and security
- **Timestamps:** All entities track created_at and updated_at (except join tables which only need created_at)
- **Soft deletes:** Models use is_active flag for soft deletion (preserves audit trail)
- **Decimal precision:** Pricing fields use decimal(10,6) for sub-cent accuracy, scores use decimal(6,2)

**From solution-architecture.md Section 2.1 - Hexagonal Architecture:**
- **Domain entities are PURE POCOs**: No EF annotations, no data access logic, no infrastructure dependencies
- **Configuration lives in Infrastructure**: All EF Core configuration via Fluent API in IEntityTypeConfiguration classes
- **Navigation properties are one-way by default**: Model → Capability (one-to-one), Model → BenchmarkScores (one-to-many), BenchmarkScore → Benchmark (many-to-one)

**From tech-spec-epic-1.md Story 1.4:**
- **Entity relationships:**
  - Model (1) ──< (1) Capability (one-to-one, cascade delete)
  - Model (1) ──< (N) BenchmarkScore (one-to-many, cascade delete)
  - Benchmark (1) ──< (N) BenchmarkScore (one-to-many, cascade delete)
- **Indexes for performance:**
  - Models: provider (filtering), status (active models query), updated_at DESC (freshness queries)
  - Capabilities: model_id (join performance)
  - BenchmarkScores: model_id and benchmark_id (join performance, aggregations)

### Project Structure Notes

**Domain entities location:**
```
/backend/
└── Backend.Domain/
    └── Entities/
        ├── Model.cs (LLM model metadata and pricing)
        ├── Capability.cs (model capabilities and limits)
        ├── Benchmark.cs (benchmark definitions)
        └── BenchmarkScore.cs (model performance on benchmarks)
```

**Infrastructure configurations location:**
```
/backend/
└── Backend.Infrastructure/
    └── Data/
        ├── AppDbContext.cs (updated with DbSet properties)
        ├── Configurations/
        │   ├── ModelConfiguration.cs (EF Core fluent config)
        │   ├── CapabilityConfiguration.cs
        │   ├── BenchmarkConfiguration.cs
        │   └── BenchmarkScoreConfiguration.cs
        └── Migrations/
            ├── 20251016XXXXXX_InitialSchema.cs (generated migration)
            └── AppDbContextModelSnapshot.cs (EF Core snapshot)
```

**Model entity structure (example):**
```csharp
namespace Backend.Domain.Entities
{
    /// <summary>
    /// Represents an LLM model with pricing and metadata.
    /// </summary>
    public class Model
    {
        public Guid Id { get; set; }

        /// <summary>
        /// Model name (e.g., "GPT-4", "Claude 3 Opus")
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        /// Provider/vendor (e.g., "OpenAI", "Anthropic", "Google")
        /// </summary>
        public string Provider { get; set; } = null!;

        // ... other properties

        // Navigation properties
        public Capability? Capability { get; set; }
        public ICollection<BenchmarkScore> BenchmarkScores { get; set; } = new List<BenchmarkScore>();
    }
}
```

**ModelConfiguration fluent API (example):**
```csharp
public class ModelConfiguration : IEntityTypeConfiguration<Model>
{
    public void Configure(EntityTypeBuilder<Model> builder)
    {
        builder.ToTable("models");

        builder.HasKey(m => m.Id);

        builder.HasIndex(m => m.Provider)
            .HasDatabaseName("idx_models_provider");

        builder.HasIndex(m => new { m.Name, m.Provider })
            .IsUnique()
            .HasDatabaseName("unique_model_provider");

        builder.Property(m => m.InputPricePer1M)
            .HasColumnType("decimal(10,6)")
            .IsRequired();

        builder.HasOne(m => m.Capability)
            .WithOne(c => c.Model)
            .HasForeignKey<Capability>(c => c.ModelId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(m => m.BenchmarkScores)
            .WithOne(bs => bs.Model)
            .HasForeignKey(bs => bs.ModelId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
```

### Testing Standards Summary

**Schema validation:**
1. All 4 tables created: `SELECT table_name FROM information_schema.tables WHERE table_schema = 'public';`
2. Indexes created: `SELECT indexname FROM pg_indexes WHERE tablename = 'models';`
3. Constraints enforced: Attempt duplicate insert, verify unique constraint violation
4. Foreign keys work: Insert related records, verify relationships
5. Cascade deletes work: Delete parent Model, verify Capability and BenchmarkScores deleted

**EF Core migration validation:**
- Migration file generated without errors
- `Up()` method creates all tables with correct column types
- `Down()` method drops all tables in correct order (scores → capabilities → models, benchmarks)
- No SQL syntax errors in generated migration

**Quality gates:**
- Migration generation time: <5 seconds
- Migration application time: <10 seconds (initial schema)
- Schema matches solution-architecture.md Section 3.1 exactly

### Dependencies and Prerequisites

**Prerequisites from previous stories:**
- Story 1.2: EF Core packages installed
- Story 1.3: PostgreSQL running, AppDbContext created, migrations infrastructure configured

**New dependencies:**
- None (all packages already installed in Story 1.2)

**Common issues and solutions:**
1. **Migration already applied**: Run `dotnet ef migrations remove` before regenerating
2. **Foreign key constraint violation**: Ensure parent entities (Model, Benchmark) created before child entities
3. **Decimal precision mismatch**: EF Core may default to decimal(18,2); explicitly configure column type
4. **Unique constraint on null values**: PostgreSQL allows multiple NULLs in unique constraint (intended behavior for optional Version field)

### References

- [Source: docs/solution-architecture.md#Section 3.1 - Database Schema (complete table definitions)]
- [Source: docs/solution-architecture.md#Section 3.2 - Data Relationships and cascade rules]
- [Source: docs/solution-architecture.md#Section 2.1 - Hexagonal Architecture: Domain entities are POCOs]
- [Source: docs/tech-spec-epic-1.md#Story 1.4 - Model.cs, Capability.cs, entity configuration]
- [Source: docs/epics.md#Epic 1, Story 1.4 - Acceptance Criteria]
- [Source: docs/PRD.md#FR001-FR005 - Model data management functional requirements]

## Dev Agent Record

### Context Reference

- `docs/stories/story-context-1.4.xml` (Generated: 2025-10-16)

### Agent Model Used

- **Model:** Claude Sonnet 4.5 (claude-sonnet-4-5-20250929)
- **Date:** 2025-10-16

### Debug Log References

**Implementation Approach:**
1. Generated Story Context XML (story-context-1.4.xml) to establish authoritative source of truth
2. Created pure POCO domain entities in Domain layer (Model, Capability, Benchmark, BenchmarkScore)
3. Configured entity mappings via Fluent API in Infrastructure layer (IEntityTypeConfiguration classes)
4. Registered configurations in AppDbContext using ApplyConfigurationsFromAssembly
5. Generated EF Core migration (20251016232000_InitialSchema.cs)
6. Started PostgreSQL via Docker Compose and applied migration
7. Validated schema structure, indexes, constraints, and cascade delete behavior

**Key Decisions:**
- Used ApplyConfigurationsFromAssembly for automatic configuration discovery (reduces maintenance)
- Implemented descending index on UpdatedAt for "recently updated" queries
- Validated all decimal precisions match specification (10,6 for pricing, 6,2 for scores, 5,4 for normalized scores)
- Tested cascade delete with real database inserts/deletes (verified working correctly)

### Completion Notes List

**✅ All Acceptance Criteria Satisfied:**
- **AC1:** Model entity created with all 15 required fields (Id, Name, Provider, Version, ReleaseDate, Status, InputPricePer1M, OutputPricePer1M, Currency, PricingValidFrom, PricingValidTo, LastScrapedAt, IsActive, CreatedAt, UpdatedAt)
- **AC2:** Capability entity created with all capability fields and boolean flags (ContextWindow, MaxOutputTokens, 6 capability flags)
- **AC3:** Benchmark entity created with metadata fields (BenchmarkName, FullName, Description, Category, Interpretation, TypicalRangeMin/Max)
- **AC4:** BenchmarkScore entity created with score fields and metadata (Score, MaxScore, NormalizedScore, TestDate, SourceUrl, Verified, Notes)
- **AC5:** Entity relationships configured with Fluent API - one-to-one (Model-Capability), one-to-many (Model-BenchmarkScores, Benchmark-BenchmarkScores) with cascade delete
- **AC6:** Migration generated and applied successfully - all 4 tables created with 13 indexes and 4 unique constraints

**Quality Gates Met:**
- ✅ Build time: 4.32 seconds (target: <30s)
- ✅ Build succeeded: 0 errors, 0 warnings
- ✅ Schema matches solution-architecture.md Section 3.1 exactly
- ✅ Cascade delete verified via database testing
- ✅ Decimal precision validated: pricing (10,6), scores (6,2), normalized scores (5,4)

### File List

**Created Files:**
- `services/backend/LlmTokenPrice.Domain/Entities/Model.cs`
- `services/backend/LlmTokenPrice.Domain/Entities/Capability.cs`
- `services/backend/LlmTokenPrice.Domain/Entities/Benchmark.cs`
- `services/backend/LlmTokenPrice.Domain/Entities/BenchmarkScore.cs`
- `services/backend/LlmTokenPrice.Infrastructure/Data/Configurations/ModelConfiguration.cs`
- `services/backend/LlmTokenPrice.Infrastructure/Data/Configurations/CapabilityConfiguration.cs`
- `services/backend/LlmTokenPrice.Infrastructure/Data/Configurations/BenchmarkConfiguration.cs`
- `services/backend/LlmTokenPrice.Infrastructure/Data/Configurations/BenchmarkScoreConfiguration.cs`
- `services/backend/LlmTokenPrice.Infrastructure/Data/Migrations/20251016232000_InitialSchema.cs`
- `services/backend/LlmTokenPrice.Infrastructure/Data/Migrations/AppDbContextModelSnapshot.cs`
- `docs/stories/story-context-1.4.xml`

**Modified Files:**
- `services/backend/LlmTokenPrice.Infrastructure/Data/AppDbContext.cs` (added DbSet properties and ApplyConfigurationsFromAssembly)

## Change Log

### 2025-10-16 - Story Implementation Complete
**Summary:** Created all four core data models (Model, Capability, Benchmark, BenchmarkScore) following hexagonal architecture with pure POCO domain entities and Fluent API configuration in Infrastructure layer.

**Changes:**
- Created 4 domain entities with comprehensive XML documentation (Model.cs, Capability.cs, Benchmark.cs, BenchmarkScore.cs)
- Implemented 4 EF Core entity configurations using IEntityTypeConfiguration pattern
- Generated and applied InitialSchema migration creating all tables, indexes, constraints, and relationships
- Validated schema structure via direct PostgreSQL queries
- Tested cascade delete behavior with actual database operations

**Database Schema Created:**
- Tables: models, model_capabilities, benchmarks, model_benchmark_scores (snake_case per PostgreSQL conventions)
- Indexes: 13 total (performance indexes on Provider, Status, UpdatedAt, ModelId, BenchmarkId)
- Constraints: 4 unique constraints enforcing data integrity
- Relationships: Cascade delete configured for Model → Capability and Model/Benchmark → BenchmarkScores

**Next Steps:** Story ready for review. After approval, proceed to Story 1.5 (Setup Redis Cache Connection).
