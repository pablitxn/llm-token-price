# Story 1.3: Setup PostgreSQL Database and Connection

Status: Done

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

- [x] Create Docker Compose configuration for local development infrastructure (AC: 1, 6)
  - [x] Create `docker-compose.yml` in project root with PostgreSQL and Redis services
  - [x] Configure PostgreSQL service: image `timescale/timescaledb:2.13.0-pg16`, port `5434`, environment variables (POSTGRES_USER: llmpricing, POSTGRES_PASSWORD: dev_password, POSTGRES_DB: llmpricing_dev)
  - [x] Add volume mount for PostgreSQL data persistence: `pgdata:/var/lib/postgresql/data`
  - [x] Configure Redis service: image `redis:7-alpine`, port `6379`, no authentication for local dev
  - [x] Add health checks for both services to ensure they're ready before backend starts
  - [x] Start services: `docker compose up -d` and verify both containers running with `docker compose ps`
  - [x] Test PostgreSQL connection manually via Docker: `docker exec llmpricing_postgres pg_isready -U llmpricing`

- [x] Configure database connection string in backend (AC: 2)
  - [x] Create `appsettings.Development.json` in LlmTokenPrice.API project
  - [x] Add ConnectionStrings section: `"DefaultConnection": "Host=localhost;Port=5434;Database=llmpricing_dev;Username=llmpricing;Password=dev_password"`
  - [x] Add ConnectionStrings for Redis: `"Redis": "localhost:6379,abortConnect=false"`
  - [x] Configure appsettings.json with production placeholders (environment variables for connection strings)
  - [x] Add connection string notes to README.md: document that dev_password is for local development only
  - [x] Verify appsettings.Development.json excluded from git via .gitignore (confirmed excluded via pattern)

- [x] Create Entity Framework Core DbContext in Infrastructure layer (AC: 3, 5)
  - [x] Create `LlmTokenPrice.Infrastructure/Data/AppDbContext.cs` class inheriting from `DbContext`
  - [x] Add constructor accepting `DbContextOptions<AppDbContext>` parameter
  - [x] Override `OnModelCreating` method (will be populated with entity configurations in Story 1.4)
  - [x] Create placeholder `DbSet<T>` properties (will add Model, Benchmark entities in Story 1.4)
  - [x] Add XML documentation comments explaining DbContext purpose and usage
  - [x] Ensure AppDbContext is in correct namespace: `LlmTokenPrice.Infrastructure.Data`

- [x] Configure DbContext dependency injection in API startup (AC: 3)
  - [x] Open `LlmTokenPrice.API/Program.cs` and create complete ASP.NET Core setup
  - [x] Register DbContext with Npgsql provider: `builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")))`
  - [x] Configure connection pooling: `npgsqlOptions.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(5))`
  - [x] Add connection timeout configuration: `CommandTimeout(30)` in Npgsql options
  - [x] Register DbContext with scoped lifetime (default behavior)
  - [x] Test API starts without database connection errors: verified with `ASPNETCORE_ENVIRONMENT=Development dotnet run`

- [x] Implement database connection health check (AC: 4)
  - [x] Create `LlmTokenPrice.API/Controllers/HealthController.cs`
  - [x] Inject `AppDbContext` into HealthController constructor
  - [x] In health check endpoint, call `await _context.Database.CanConnectAsync()` to test database connection
  - [x] Return health check status JSON: `{ "status": "healthy/degraded", "services": { "database": "ok/error", "redis": "pending" } }`
  - [x] Test health endpoint: `curl http://localhost:5000/api/health` returned `{"status":"degraded","services":{"database":"ok","redis":"pending"}}`
  - [x] Add error handling: if database connection fails, return 503 Service Unavailable with error details

- [x] Configure Entity Framework migrations infrastructure (AC: 5)
  - [x] Verify EF Core CLI tools installed: `dotnet ef --version` showed version 9.0.4
  - [x] Add EF Design package to API project (added `Microsoft.EntityFrameworkCore.Design` version 9.0.10)
  - [x] Create migrations directory: `LlmTokenPrice.Infrastructure/Data/Migrations/`
  - [x] Test migration generation: `dotnet ef migrations add TestMigration --project LlmTokenPrice.Infrastructure --startup-project LlmTokenPrice.API` succeeded
  - [x] Remove test migration: `dotnet ef migrations remove --project LlmTokenPrice.Infrastructure --startup-project LlmTokenPrice.API` succeeded
  - [x] Document migration commands in README.md under "Database Management" section

- [x] Create database initialization and seeding infrastructure (AC: 5)
  - [x] Create `LlmTokenPrice.Infrastructure/Data/DbInitializer.cs` class with static `InitializeAsync(AppDbContext context, ILogger logger)` method
  - [x] Implement migration check: `await context.Database.MigrateAsync()` (applies pending migrations automatically)
  - [x] Create placeholder for seed data (will populate in Story 1.9)
  - [x] Call DbInitializer from Program.cs using service scope after app build
  - [x] Test database initialization: start backend, verified llmpricing_dev database created in PostgreSQL

- [x] Document database setup and verify all components (AC: 1-6)
  - [x] Update README.md with "Database Setup" section covering Docker Compose, connection strings, migrations
  - [x] Add troubleshooting section: common issues (port already in use, PostgreSQL not starting, connection refused, password authentication failed)
  - [x] Document how to reset database: `docker compose down -v` (removes volumes), then `docker compose up -d`
  - [x] Create database verification checklist and Database Management section with EF migration commands
  - [x] Verify all acceptance criteria: All 6 ACs confirmed met
  - [x] Test end-to-end: `docker compose up -d && ASPNETCORE_ENVIRONMENT=Development dotnet run` starts successfully with database connected

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

claude-sonnet-4-5-20250929

### Debug Log References

<!-- Debug logs will be added during development -->

### Completion Notes List

- **PostgreSQL Setup**: Configured TimescaleDB 2.13.0-pg16 on port 5434 (port 5432 was occupied by existing containers)
- **Redis Setup**: Configured Redis 7.2-alpine on standard port 6379
- **Project Structure Adaptation**: Adapted to existing `LlmTokenPrice.*` namespace instead of expected `Backend.*` naming
- **SDK Configuration**: Changed API project from `Microsoft.NET.Sdk` to `Microsoft.NET.Sdk.Web` to enable ASP.NET Core features
- **EF Core Design**: Added `Microsoft.EntityFrameworkCore.Design` package to API project for migrations support
- **Database Initialization**: Implemented DbInitializer with automatic migration application on startup
- **Health Check**: Created HealthController with database connectivity verification (returns "degraded" status since Redis check is placeholder for Story 1.5)
- **Documentation**: Updated README.md with comprehensive database setup, management commands, and troubleshooting guide
- **Environment Variable**: Added `ASPNETCORE_ENVIRONMENT=Development` requirement to running instructions for proper configuration loading

### File List

**Created:**
- `docker-compose.yml` - Docker Compose configuration for PostgreSQL + TimescaleDB and Redis
- `services/backend/LlmTokenPrice.API/appsettings.json` - Production configuration template
- `services/backend/LlmTokenPrice.API/appsettings.Development.json` - Development configuration with connection strings
- `services/backend/LlmTokenPrice.Infrastructure/Data/AppDbContext.cs` - Entity Framework Core database context
- `services/backend/LlmTokenPrice.Infrastructure/Data/DbInitializer.cs` - Database initialization and migration handler
- `services/backend/LlmTokenPrice.API/Controllers/HealthController.cs` - Health check endpoint

**Modified:**
- `services/backend/LlmTokenPrice.API/LlmTokenPrice.API.csproj` - Changed SDK to Web, added EF Core Design package
- `services/backend/LlmTokenPrice.API/Program.cs` - Configured ASP.NET Core, DbContext DI, and database initialization
- `README.md` - Added database setup, configuration, management, and troubleshooting documentation
