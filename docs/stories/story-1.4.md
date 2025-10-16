# Story 1.4: Create Core Data Models (Models, Capabilities, Benchmarks)

Status: Ready

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

- [ ] Create Model entity in Domain layer (AC: 1)
  - [ ] Create `Backend.Domain/Entities/Model.cs` class with all required properties
  - [ ] Add GUID primary key: `public Guid Id { get; set; }`
  - [ ] Add model metadata: Name (string, required), Provider (string, required), Version (string, nullable), ReleaseDate (DateTime?, nullable), Status (string, default "active")
  - [ ] Add pricing fields: InputPricePer1M (decimal, required), OutputPricePer1M (decimal, required), Currency (string, default "USD"), PricingValidFrom (DateTime?, nullable), PricingValidTo (DateTime?, nullable)
  - [ ] Add operational fields: LastScrapedAt (DateTime?, nullable), IsActive (bool, default true), CreatedAt (DateTime, required), UpdatedAt (DateTime, required)
  - [ ] Add navigation properties: `public Capability? Capability { get; set; }`, `public ICollection<BenchmarkScore> BenchmarkScores { get; set; } = new List<BenchmarkScore>();`
  - [ ] Add XML documentation comments explaining each property's purpose
  - [ ] Verify Domain layer has no dependencies (pure POCO entity)

- [ ] Create Capability entity in Domain layer (AC: 2)
  - [ ] Create `Backend.Domain/Entities/Capability.cs` class
  - [ ] Add GUID primary key and foreign key: `public Guid Id { get; set; }`, `public Guid ModelId { get; set; }`
  - [ ] Add capability fields: ContextWindow (int, required), MaxOutputTokens (int, nullable)
  - [ ] Add boolean capability flags: SupportsFunctionCalling, SupportsVision, SupportsAudioInput, SupportsAudioOutput, SupportsStreaming (default true), SupportsJsonMode
  - [ ] Add navigation property: `public Model Model { get; set; } = null!;`
  - [ ] Add XML documentation explaining capability flags (e.g., "Indicates whether model supports OpenAI-style function calling")
  - [ ] Ensure class is in `Backend.Domain.Entities` namespace

- [ ] Create Benchmark entity in Domain layer (AC: 3)
  - [ ] Create `Backend.Domain/Entities/Benchmark.cs` class
  - [ ] Add GUID primary key: `public Guid Id { get; set; }`
  - [ ] Add benchmark metadata: BenchmarkName (string, required, unique), FullName (string, nullable), Description (string, nullable)
  - [ ] Add categorization: Category (string, nullable - "reasoning", "code", "math", "language", "multimodal")
  - [ ] Add interpretation: Interpretation (string, nullable - "higher_better" or "lower_better")
  - [ ] Add range fields: TypicalRangeMin (decimal, nullable), TypicalRangeMax (decimal, nullable)
  - [ ] Add navigation property: `public ICollection<BenchmarkScore> Scores { get; set; } = new List<BenchmarkScore>();`
  - [ ] Add CreatedAt timestamp field

- [ ] Create BenchmarkScore entity in Domain layer (AC: 4)
  - [ ] Create `Backend.Domain/Entities/BenchmarkScore.cs` class
  - [ ] Add GUID primary key and foreign keys: `public Guid Id { get; set; }`, `public Guid ModelId { get; set; }`, `public Guid BenchmarkId { get; set; }`
  - [ ] Add score fields: Score (decimal, required), MaxScore (decimal, nullable), NormalizedScore (decimal, nullable - 0-1 for QAPS calculation)
  - [ ] Add metadata fields: TestDate (DateTime, nullable), SourceUrl (string, nullable), Verified (bool, default false), Notes (string, nullable)
  - [ ] Add timestamp: CreatedAt (DateTime, required)
  - [ ] Add navigation properties: `public Model Model { get; set; } = null!;`, `public Benchmark Benchmark { get; set; } = null!;`
  - [ ] Add XML documentation explaining NormalizedScore purpose (used in QAPS calculation)

- [ ] Configure entity relationships and constraints in Infrastructure layer (AC: 5)
  - [ ] Create `Backend.Infrastructure/Data/Configurations/ModelConfiguration.cs` implementing `IEntityTypeConfiguration<Model>`
  - [ ] Configure Model primary key, indexes (Provider, Status, UpdatedAt DESC), unique constraint on (Name, Provider)
  - [ ] Configure decimal precision: InputPricePer1M and OutputPricePer1M as `decimal(10,6)`
  - [ ] Configure one-to-one relationship: Model → Capability with cascade delete
  - [ ] Configure one-to-many relationship: Model → BenchmarkScores with cascade delete
  - [ ] Create `Backend.Infrastructure/Data/Configurations/CapabilityConfiguration.cs` implementing `IEntityTypeConfiguration<Capability>`
  - [ ] Configure Capability primary key, foreign key to Model, unique constraint on ModelId
  - [ ] Configure index on ModelId for fast lookups
  - [ ] Create `Backend.Infrastructure/Data/Configurations/BenchmarkConfiguration.cs` implementing `IEntityTypeConfiguration<Benchmark>`
  - [ ] Configure Benchmark primary key, unique constraint on BenchmarkName
  - [ ] Configure decimal precision for TypicalRangeMin and TypicalRangeMax as `decimal(5,2)`
  - [ ] Create `Backend.Infrastructure/Data/Configurations/BenchmarkScoreConfiguration.cs` implementing `IEntityTypeConfiguration<BenchmarkScore>`
  - [ ] Configure BenchmarkScore primary key, foreign keys, unique constraint on (ModelId, BenchmarkId)
  - [ ] Configure indexes on ModelId and BenchmarkId for query performance
  - [ ] Configure decimal precision: Score as `decimal(6,2)`, NormalizedScore as `decimal(5,4)`

- [ ] Register entity configurations in AppDbContext (AC: 5)
  - [ ] Update `Backend.Infrastructure/Data/AppDbContext.cs` to add DbSet properties for all entities
  - [ ] Add `public DbSet<Model> Models { get; set; } = null!;`
  - [ ] Add `public DbSet<Capability> Capabilities { get; set; } = null!;`
  - [ ] Add `public DbSet<Benchmark> Benchmarks { get; set; } = null!;`
  - [ ] Add `public DbSet<BenchmarkScore> BenchmarkScores { get; set; } = null!;`
  - [ ] In `OnModelCreating`, apply all entity configurations: `modelBuilder.ApplyConfiguration(new ModelConfiguration());` (repeat for all entities)
  - [ ] Alternatively, use `modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());` to auto-discover all configurations

- [ ] Generate and apply database migration (AC: 6)
  - [ ] Generate migration: `dotnet ef migrations add InitialSchema --project Backend.Infrastructure --startup-project Backend.API --output-dir Data/Migrations`
  - [ ] Review generated migration file in `Backend.Infrastructure/Data/Migrations/` directory
  - [ ] Verify migration includes all 4 tables: models, model_capabilities, benchmarks, model_benchmark_scores
  - [ ] Verify indexes created: idx_models_provider, idx_models_status, idx_models_updated, idx_capabilities_model, idx_scores_model, idx_scores_benchmark
  - [ ] Verify unique constraints: unique_model_provider (name, provider), unique_model_capability (model_id), unique_benchmark_name, unique_model_benchmark (model_id, benchmark_id)
  - [ ] Apply migration to database: `dotnet ef database update --project Backend.Infrastructure --startup-project Backend.API`
  - [ ] Verify tables created in PostgreSQL: `psql -h localhost -U llmpricing -d llmpricing_dev -c "\dt"` should show all 4 tables

- [ ] Validate schema and create verification tests (AC: 6)
  - [ ] Connect to database and verify table structure: `psql -h localhost -U llmpricing -d llmpricing_dev -c "\d models"` shows correct columns
  - [ ] Test entity relationships: insert test Model, Capability, Benchmark, and BenchmarkScore records via SQL
  - [ ] Verify cascade delete works: delete Model record, confirm Capability and BenchmarkScores cascade deleted
  - [ ] Verify unique constraints: attempt to insert duplicate (name, provider) Model, should fail with constraint violation
  - [ ] Test indexes exist: `SELECT * FROM pg_indexes WHERE tablename IN ('models', 'model_capabilities', 'benchmarks', 'model_benchmark_scores');`
  - [ ] Document schema verification commands in README.md under "Database Management" section
  - [ ] Create schema diagram (optional): generate ER diagram showing entity relationships using pgAdmin or dbdiagram.io

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

<!-- Path(s) to story context XML will be added here by context workflow -->

### Agent Model Used

<!-- Agent model information will be populated during development -->

### Debug Log References

<!-- Debug logs will be added during development -->

### Completion Notes List

<!-- Completion notes will be added after story implementation -->

### File List

<!-- Modified/created files will be listed here after implementation -->
