# Tech Spec: Epic 1 - Project Foundation & Data Infrastructure

**Epic Goal:** Establish development environment, database schema, API skeleton, and CI/CD pipeline with deployable application

**Stories:** 10 | **Estimated Effort:** 2-3 weeks

---

## Architecture Context

**From solution-architecture.md:**
- **Backend:** .NET 8 with Hexagonal Architecture (Domain → Application → Infrastructure → API)
- **Frontend:** React 18 + Vite + TypeScript + TailwindCSS
- **Database:** PostgreSQL 16 + TimescaleDB 2.13
- **Cache:** Redis 7.2 (Upstash)
- **Repository:** Monorepo (`/backend`, `/frontend`)

**Key Files:**
- Backend: `Backend.Domain/Entities/*.cs`, `Backend.Infrastructure/Data/AppDbContext.cs`
- Frontend: `frontend/src/main.tsx`, `frontend/src/App.tsx`
- Infrastructure: `docker-compose.yml`, `.github/workflows/*.yml`

---

## Story 1.1: Initialize Project Repository

**Backend Setup:**
```bash
dotnet new sln -n Backend
dotnet new classlib -n Backend.Domain -f net8.0
dotnet new classlib -n Backend.Application -f net8.0
dotnet new classlib -n Backend.Infrastructure -f net8.0
dotnet new webapi -n Backend.API -f net8.0
dotnet sln add **/*.csproj
```

**Frontend Setup:**
```bash
npm create vite@latest frontend -- --template react-ts
cd frontend
npm install zustand@4.4.7 @tanstack/react-query@5.17.0 @tanstack/react-table@8.11.0
npm install chart.js@4.4.1 react-chartjs-2
npm install -D tailwindcss@3.4.0 postcss autoprefixer
npx tailwindcss init -p
```

**Directory Structure:** See solution-architecture.md Section 8

---

## Story 1.2: Configure Build Tools

**Backend:**
- Add NuGet packages: `Microsoft.EntityFrameworkCore@8.0.0`, `Npgsql.EntityFrameworkCore.PostgreSQL@8.0.0`, `StackExchangeRedis@2.7.0`
- Configure project references: API → Application → Domain, Infrastructure → Domain

**Frontend:**
- `vite.config.ts`: Configure proxy to backend (`http://localhost:5000`)
- `tailwind.config.js`: Add design system colors, spacing from UX spec
- `tsconfig.json`: Strict mode, path aliases (`@/components`, `@/api`)

---

## Story 1.3: Setup PostgreSQL Database

**docker-compose.yml:**
```yaml
version: '3.8'
services:
  postgres:
    image: timescale/timescaledb:2.13.0-pg16
    environment:
      POSTGRES_USER: llmpricing
      POSTGRES_PASSWORD: dev_password
      POSTGRES_DB: llmpricing_dev
    ports:
      - "5432:5432"
    volumes:
      - pgdata:/var/lib/postgresql/data

  redis:
    image: redis:7-alpine
    ports:
      - "6379:6379"
```

**Backend Connection:**
- `appsettings.Development.json`: `"ConnectionStrings": { "DefaultConnection": "Host=localhost;Database=llmpricing_dev;Username=llmpricing;Password=dev_password" }`
- `Backend.Infrastructure/Data/AppDbContext.cs`: Create DbContext with `DbContextOptions`

---

## Story 1.4: Create Core Data Models

**Implementation:** `Backend.Domain/Entities/`

```csharp
// Model.cs
public class Model
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Provider { get; set; } = null!;
    public string? Version { get; set; }
    public DateTime? ReleaseDate { get; set; }
    public string Status { get; set; } = "active";
    public decimal InputPricePer1M { get; set; }
    public decimal OutputPricePer1M { get; set; }
    public string Currency { get; set; } = "USD";
    public DateTime? PricingValidFrom { get; set; }
    public DateTime? PricingValidTo { get; set; }
    public DateTime? LastScrapedAt { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public Capability? Capability { get; set; }
    public ICollection<BenchmarkScore> BenchmarkScores { get; set; } = new List<BenchmarkScore>();
}
```

**EF Core Configuration:** `Backend.Infrastructure/Data/Configurations/ModelConfiguration.cs`
```csharp
public class ModelConfiguration : IEntityTypeConfiguration<Model>
{
    public void Configure(EntityTypeBuilder<Model> builder)
    {
        builder.HasKey(m => m.Id);
        builder.HasIndex(m => m.Provider);
        builder.HasIndex(m => new { m.Name, m.Provider }).IsUnique();
        builder.Property(m => m.InputPricePer1M).HasColumnType("decimal(10,6)");
        // ... other configurations
    }
}
```

**Migration:**
```bash
dotnet ef migrations add InitialSchema --project Backend.Infrastructure --startup-project Backend.API
dotnet ef database update --project Backend.Infrastructure --startup-project Backend.API
```

---

## Story 1.5: Setup Redis Cache Connection

**Backend.Infrastructure/Caching/RedisCacheService.cs:**
```csharp
public class RedisCacheService : ICacheRepository
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _db;

    public RedisCacheService(IConnectionMultiplexer redis)
    {
        _redis = redis;
        _db = redis.GetDatabase();
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        var value = await _db.StringGetAsync(key);
        return value.HasValue ? JsonSerializer.Deserialize<T>(value!) : default;
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
    {
        await _db.StringSetAsync(key, JsonSerializer.Serialize(value), expiry);
    }
}
```

**Registration:** `Backend.API/Program.cs`
```csharp
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis")!));
builder.Services.AddScoped<ICacheRepository, RedisCacheService>();
```

---

## Story 1.6: Create Basic API Structure

**Backend.API/Controllers/HealthController.cs:**
```csharp
[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IConnectionMultiplexer _redis;

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var dbHealth = await _context.Database.CanConnectAsync();
        var redisHealth = _redis.IsConnected;

        return Ok(new
        {
            status = dbHealth && redisHealth ? "healthy" : "degraded",
            services = new
            {
                database = dbHealth ? "ok" : "error",
                redis = redisHealth ? "ok" : "error"
            },
            timestamp = DateTime.UtcNow
        });
    }
}
```

**CORS:** `Program.cs`
```csharp
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod());
});
```

---

## Story 1.7: Setup Frontend Application Shell

**frontend/src/main.tsx:**
```tsx
import React from 'react'
import ReactDOM from 'react-dom/client'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { BrowserRouter } from 'react-router-dom'
import App from './App'
import './styles/globals.css'

const queryClient = new QueryClient({
  defaultOptions: {
    queries: { staleTime: 5 * 60 * 1000 } // 5min client cache
  }
})

ReactDOM.createRoot(document.getElementById('root')!).render(
  <React.StrictMode>
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>
        <App />
      </BrowserRouter>
    </QueryClientProvider>
  </React.StrictMode>
)
```

**frontend/src/App.tsx:**
```tsx
import { Routes, Route } from 'react-router-dom'
import HomePage from './pages/HomePage'
import CalculatorPage from './pages/CalculatorPage'
import ComparisonPage from './pages/ComparisonPage'
import Layout from './components/layout/Layout'

export default function App() {
  return (
    <Layout>
      <Routes>
        <Route path="/" element={<HomePage />} />
        <Route path="/calculator" element={<CalculatorPage />} />
        <Route path="/compare" element={<ComparisonPage />} />
      </Routes>
    </Layout>
  )
}
```

---

## Story 1.8: Configure CI/CD Pipeline

**.github/workflows/backend-ci.yml:**
```yaml
name: Backend CI
on: [push, pull_request]
jobs:
  test:
    runs-on: ubuntu-latest
    services:
      postgres:
        image: timescale/timescaledb:2.13.0-pg16
        env:
          POSTGRES_PASSWORD: test
        options: >-
          --health-cmd pg_isready
          --health-interval 10s
      redis:
        image: redis:7-alpine
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.0.x'
      - run: dotnet test --verbosity normal
```

**.github/workflows/frontend-ci.yml:**
```yaml
name: Frontend CI
on: [push, pull_request]
jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-node@v4
        with:
          node-version: '20'
      - run: npm ci
      - run: npm run type-check
      - run: npm run lint
      - run: npm run build
```

---

## Story 1.9: Seed Database with Sample Data

**Backend.Infrastructure/Data/Seeds/SampleDataSeeder.cs:**
```csharp
public static class SampleDataSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        if (await context.Models.AnyAsync()) return;

        var models = new[]
        {
            new Model
            {
                Name = "GPT-4",
                Provider = "OpenAI",
                InputPricePer1M = 30.00m,
                OutputPricePer1M = 60.00m,
                // ... other properties
            },
            // Add 9 more sample models
        };

        context.Models.AddRange(models);
        await context.SaveChangesAsync();
    }
}
```

**Call from Program.cs:**
```csharp
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await SampleDataSeeder.SeedAsync(context);
}
```

---

## Story 1.10: Create Basic GET API for Models List

**Backend.Application/DTOs/ModelDto.cs:**
```csharp
public record ModelDto(
    Guid Id,
    string Name,
    string Provider,
    decimal InputPricePer1M,
    decimal OutputPricePer1M,
    string Currency,
    CapabilityDto Capabilities,
    List<BenchmarkScoreDto> TopBenchmarks,
    DateTime LastUpdated
);
```

**Backend.API/Controllers/ModelsController.cs:**
```csharp
[ApiController]
[Route("api/[controller]")]
public class ModelsController : ControllerBase
{
    private readonly IModelQueryService _queryService;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var models = await _queryService.GetAllModelsAsync();
        return Ok(new
        {
            data = models,
            meta = new { cached = false, timestamp = DateTime.UtcNow }
        });
    }
}
```

---

## Testing Strategy

**Unit Tests (Backend.Domain.Tests):**
- Domain entity creation
- Value object validation

**Integration Tests (Backend.API.Tests):**
- Health endpoint returns 200 OK
- Models endpoint returns seeded data
- Database connection verified
- Redis connection verified

**Frontend Tests:**
- App renders without crashing
- Routes configured correctly
- API client initialization

---

## Acceptance Criteria Summary

✅ Monorepo structure with /backend and /frontend
✅ .NET 8 solution with 4 projects (Domain, Application, Infrastructure, API)
✅ React app with Vite, TypeScript, TailwindCSS configured
✅ PostgreSQL + Redis running via Docker Compose
✅ Database schema with 7 tables migrated
✅ Health check endpoint validates DB + Redis
✅ 10 sample models seeded
✅ GET /api/models returns data
✅ CI/CD pipelines test on every push
✅ Documentation: README with setup instructions

---

**Next Epic:** Epic 2 - Model Data Management & Admin CRUD
