# Story 1.3: Setup PostgreSQL Database and Connection

Status: Ready

## Story

As a developer,
I want PostgreSQL database configured and connected to the backend application,
So that I can persist application data and begin building data models.

## Acceptance Criteria

1. PostgreSQL 16 with TimescaleDB 2.13 extension running locally via Docker Compose
2. Database connection string configured in backend `appsettings.Development.json` with correct credentials
3. Entity Framework Core DbContext created with proper dependency injection configuration
4. Database connection health check passes successfully (backend can connect to PostgreSQL)
5. Entity Framework migrations infrastructure configured and ready to generate migrations
6. Redis 7.2 cache service running alongside PostgreSQL in Docker Compose for future caching needs

## Tasks / Subtasks

- [ ] Create Docker Compose configuration for local development infrastructure (AC: 1, 6)
  - [ ] Create `docker-compose.yml` in project root with PostgreSQL and Redis services
  - [ ] Configure PostgreSQL service: image `timescale/timescaledb:2.13.0-pg16`, port `5432`, environment variables (POSTGRES_USER: llmpricing, POSTGRES_PASSWORD: dev_password, POSTGRES_DB: llmpricing_dev)
  - [ ] Add volume mount for PostgreSQL data persistence: `pgdata:/var/lib/postgresql/data`
  - [ ] Configure Redis service: image `redis:7-alpine`, port `6379`, no authentication for local dev
  - [ ] Add health checks for both services to ensure they're ready before backend starts
  - [ ] Start services: `docker-compose up -d` and verify both containers running with `docker-compose ps`
  - [ ] Test PostgreSQL connection manually: `psql -h localhost -U llmpricing -d llmpricing_dev` (should connect successfully)

- [ ] Configure database connection string in backend (AC: 2)
  - [ ] Create `appsettings.Development.json` in Backend.API project if not exists
  - [ ] Add ConnectionStrings section: `"DefaultConnection": "Host=localhost;Port=5432;Database=llmpricing_dev;Username=llmpricing;Password=dev_password"`
  - [ ] Add ConnectionStrings for Redis: `"Redis": "localhost:6379"`
  - [ ] Configure appsettings.json with production placeholders (environment variables for connection strings)
  - [ ] Add connection string notes to README.md: document that dev_password is for local development only
  - [ ] Verify appsettings.Development.json excluded from git via .gitignore (should already be excluded from Story 1.1)

- [ ] Create Entity Framework Core DbContext in Infrastructure layer (AC: 3, 5)
  - [ ] Create `Backend.Infrastructure/Data/AppDbContext.cs` class inheriting from `DbContext`
  - [ ] Add constructor accepting `DbContextOptions<AppDbContext>` parameter
  - [ ] Override `OnModelCreating` method (will be populated with entity configurations in Story 1.4)
  - [ ] Create placeholder `DbSet<T>` properties (will add Model, Benchmark entities in Story 1.4)
  - [ ] Add XML documentation comments explaining DbContext purpose and usage
  - [ ] Ensure AppDbContext is in correct namespace: `Backend.Infrastructure.Data`

- [ ] Configure DbContext dependency injection in API startup (AC: 3)
  - [ ] Open `Backend.API/Program.cs` and locate service registration section
  - [ ] Register DbContext with Npgsql provider: `builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")))`
  - [ ] Configure connection pooling: `options.UseNpgsql(..., npgsqlOptions => npgsqlOptions.EnableRetryOnFailure(maxRetryCount: 3))`
  - [ ] Add connection timeout configuration: `CommandTimeout = 30` in Npgsql options
  - [ ] Register DbContext with scoped lifetime (default behavior, but verify)
  - [ ] Test API starts without database connection errors: `dotnet run --project Backend.API`

- [ ] Implement database connection health check (AC: 4)
  - [ ] Update `Backend.API/Controllers/HealthController.cs` (created in Story 1.6, but using placeholder now)
  - [ ] Inject `AppDbContext` into HealthController constructor
  - [ ] In health check endpoint, call `await _context.Database.CanConnectAsync()` to test database connection
  - [ ] Return health check status JSON: `{ "status": "healthy/degraded", "services": { "database": "ok/error", "redis": "pending" } }`
  - [ ] Test health endpoint: `curl http://localhost:5000/api/health` should return database: ok
  - [ ] Add error handling: if database connection fails, return 503 Service Unavailable with error details

- [ ] Configure Entity Framework migrations infrastructure (AC: 5)
  - [ ] Install EF Core CLI tools globally: `dotnet tool install --global dotnet-ef` (if not already installed)
  - [ ] Verify EF tools installed: `dotnet ef --version` should show version 8.0.x
  - [ ] Add EF Design package to Infrastructure project (should already be added in Story 1.2)
  - [ ] Create migrations directory: `Backend.Infrastructure/Data/Migrations/`
  - [ ] Test migration generation (will create actual migration in Story 1.4): `dotnet ef migrations add TestMigration --project Backend.Infrastructure --startup-project Backend.API`
  - [ ] Remove test migration: `dotnet ef migrations remove --project Backend.Infrastructure --startup-project Backend.API`
  - [ ] Document migration commands in README.md under "Database Management" section

- [ ] Create database initialization and seeding infrastructure (AC: 5)
  - [ ] Create `Backend.Infrastructure/Data/DbInitializer.cs` class with static `InitializeAsync(AppDbContext context)` method
  - [ ] Implement database creation: `await context.Database.EnsureCreatedAsync()` (for dev environments only)
  - [ ] Add migration check: `await context.Database.MigrateAsync()` (applies pending migrations automatically)
  - [ ] Create placeholder for seed data (will populate in Story 1.9)
  - [ ] Call DbInitializer from Program.cs using service scope after app build
  - [ ] Test database initialization: start backend, verify llmpricing_dev database created in PostgreSQL

- [ ] Document database setup and verify all components (AC: 1-6)
  - [ ] Update README.md with "Database Setup" section covering Docker Compose, connection strings, migrations
  - [ ] Add troubleshooting section: common issues (port 5432 already in use, PostgreSQL not starting, connection refused)
  - [ ] Document how to reset database: `docker-compose down -v` (removes volumes), then `docker-compose up -d`
  - [ ] Create database verification checklist: PostgreSQL running, Redis running, backend connects, health check passes, migrations infrastructure ready
  - [ ] Verify all acceptance criteria: run through checklist and confirm all 6 criteria met
  - [ ] Test end-to-end: `docker-compose up -d && dotnet run --project Backend.API` should start successfully with database connected

## Dev Notes

### Architecture Constraints

**From solution-architecture.md Section 3.1:**
- **Database:** PostgreSQL 16.0 with TimescaleDB 2.13.0 extension
  - TimescaleDB enables time-series optimizations for `model_pricing_history` table (Phase 2 feature)
  - Use `timescale/timescaledb:2.13.0-pg16` Docker image (includes both PostgreSQL 16 and TimescaleDB extension)
- **Connection pooling:** Npgsql connection pool enabled by default (max 100 connections)
- **Retry logic:** EnableRetryOnFailure with max 3 retries for transient failures

**From tech-spec-epic-1.md Story 1.3:**
- Docker Compose must define both PostgreSQL and Redis (even though Redis used in Story 1.5) to avoid multiple restarts
- PostgreSQL environment variables: POSTGRES_USER, POSTGRES_PASSWORD, POSTGRES_DB
- Volume mount for data persistence: prevents data loss when containers restart
- Health check endpoint must verify database connectivity before reporting "healthy"

**From solution-architecture.md Section 2.1 - Hexagonal Architecture:**
- DbContext belongs in **Infrastructure layer** (adapts domain repositories to EF Core)
- Domain entities defined in **Domain layer** (Story 1.4)
- Repository interfaces defined in **Domain layer**, implementations in **Infrastructure layer**
- DbContext is an infrastructure concern, never referenced directly in Domain or Application layers

### Project Structure Notes

**Docker Compose file location:**
```
/llm-token-price/
├── docker-compose.yml (root level, manages local dev dependencies)
└── /backend/
    ├── /Backend.Infrastructure/
    │   └── /Data/
    │       ├── AppDbContext.cs (EF Core context)
    │       ├── DbInitializer.cs (database initialization logic)
    │       └── /Migrations/ (EF migrations generated here)
    └── /Backend.API/
        ├── appsettings.json (production config)
        └── appsettings.Development.json (local dev config with connection strings)
```

**AppDbContext structure:**
```csharp
namespace Backend.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        // DbSet properties added in Story 1.4
        // public DbSet<Model> Models { get; set; }
        // public DbSet<Benchmark> Benchmarks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Entity configurations added in Story 1.4
        }
    }
}
```

**Program.cs service registration pattern:**
```csharp
// Database configuration
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsqlOptions =>
        {
            npgsqlOptions.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(5),
                errorCodesToAdd: null);
            npgsqlOptions.CommandTimeout(30);
        }
    )
);

// After app.Build(), initialize database
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await DbInitializer.InitializeAsync(context);
}
```

### Testing Standards Summary

**Database connection verification:**
1. Manual test: `psql -h localhost -U llmpricing -d llmpricing_dev` connects successfully
2. Backend test: `dotnet run` starts without database errors
3. Health check test: `curl http://localhost:5000/api/health` returns `{ "services": { "database": "ok" } }`
4. Migration test: `dotnet ef migrations add TestMigration` generates migration successfully

**Quality gates:**
- Database startup time: <10 seconds (Docker Compose up)
- Backend connection time: <2 seconds (first connection may take longer due to pooling initialization)
- Health check response time: <500ms

**Docker Compose health checks:**
```yaml
healthcheck:
  test: ["CMD-SHELL", "pg_isready -U llmpricing"]
  interval: 10s
  timeout: 5s
  retries: 5
```

### Dependencies and Prerequisites

**Prerequisites from previous stories:**
- Story 1.1: Monorepo structure created
- Story 1.2: EF Core packages installed (Microsoft.EntityFrameworkCore, Npgsql.EntityFrameworkCore.PostgreSQL)

**Dependency check:**
- Docker Desktop or Docker Engine installed and running
- Port 5432 available (not used by another PostgreSQL instance)
- Port 6379 available (for Redis)

**Common issues and solutions:**
1. **Port 5432 already in use**: Stop existing PostgreSQL instances or change port in docker-compose.yml
2. **Permission denied**: Ensure Docker has permission to create volumes
3. **Connection refused**: Wait for PostgreSQL health check to pass before starting backend
4. **Password authentication failed**: Verify POSTGRES_PASSWORD matches connection string

### References

- [Source: docs/solution-architecture.md#Section 3.1 - Database Schema (PostgreSQL + TimescaleDB)]
- [Source: docs/solution-architecture.md#Section 2.1 - Hexagonal Architecture: Infrastructure Layer]
- [Source: docs/solution-architecture.md#Section 3.3 - EF Core Code-First Migrations]
- [Source: docs/tech-spec-epic-1.md#Story 1.3 - Docker Compose configuration and connection setup]
- [Source: docs/epics.md#Epic 1, Story 1.3 - Acceptance Criteria]
- [Source: docs/PRD.md#NFR004 - Availability: graceful degradation if external services fail]

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
