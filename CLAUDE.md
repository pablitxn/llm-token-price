# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

LLM Token Price Comparison Platform - A Level 4 enterprise-scale web application for comparing and analyzing pricing across Large Language Model providers. This platform enables developers to make data-driven decisions about model selection through real-time pricing comparisons, cost calculations, and performance benchmarks.

**Target Scale:** 5,000+ monthly active users by month 6
**Current Phase:** Epic 1 - Project Foundation & Data Infrastructure

## Architecture

This is a **monorepo** using **Hexagonal Architecture** (Ports & Adapters pattern):

### Backend (`services/backend/`)
- **.NET 9** with Hexagonal Architecture (Clean Architecture)
- **PostgreSQL 16** for persistent storage
- **Redis 7.2** for caching
- **ASP.NET Core Web API** for REST endpoints

**Layer Structure:**
```
services/backend/
├── LlmTokenPrice.Domain/         # Pure business logic, NO framework dependencies
│   ├── /Entities                 # Model, Benchmark, BenchmarkScore, etc.
│   ├── /Services                 # QAPSCalculator, BenchmarkNormalizer, CostEstimator
│   └── /Interfaces               # IModelRepository, ICacheRepository (ports)
├── LlmTokenPrice.Application/    # Use cases orchestrating domain services
│   ├── /DTOs                     # ModelDto, BestValueDto
│   ├── /Services                 # ModelQueryService, BestValueService
│   └── /Validators               # FluentValidation
├── LlmTokenPrice.Infrastructure/ # Framework adapters (swappable)
│   ├── /Data                     # EF Core DbContext, Migrations
│   ├── /Repositories             # Concrete implementations of ports
│   └── /Caching                  # RedisCacheService
└── LlmTokenPrice.API/            # HTTP entry point
    ├── Program.cs                # Startup, DI configuration
    └── /Controllers              # Delegate to application services
```

**Critical Hexagonal Principle:** Domain layer NEVER depends on Infrastructure. All dependencies point inward: Infrastructure → Application → Domain.

### Frontend (`apps/web/`)
- **React 19** with **TypeScript** (strict mode, zero `any` types)
- **Vite** (using Rolldown) as build tool
- **TailwindCSS 4** for styling
- **Zustand** for client state (comparison basket, filters)
- **TanStack Query** for server state (API caching, refetch)
- **TanStack Table** for data tables
- **Chart.js** for visualizations

**Path Aliases (tsconfig & vite.config):**
- `@/*` → `./src/*`
- `@components/*` → `./src/components/*`
- `@api/*` → `./src/api/*`
- `@store/*` → `./src/store/*`

**State Architecture:**
- **Server state:** TanStack Query (5min stale time)
- **Client state:** Zustand (comparison basket, filter state, view preferences)
- **Local state:** useState (forms, modals, pagination)

## Development Commands

### Backend (.NET)

```bash
cd services/backend

# Restore dependencies
dotnet restore

# Build (all projects)
dotnet build

# Build in Release mode
dotnet build --configuration Release

# Run API server (http://localhost:5000)
dotnet run --project LlmTokenPrice.API

# Run tests
dotnet test

# Entity Framework migrations
dotnet ef migrations add MigrationName --project LlmTokenPrice.Infrastructure
dotnet ef database update --project LlmTokenPrice.Infrastructure

# Publish for production
dotnet publish -c Release -o ./publish
```

**Quality Gates:**
- Build time: < 30 seconds
- 0 errors, 0 warnings

### Frontend (React + Vite)

```bash
cd apps/web

# Install dependencies
pnpm install

# Start dev server (http://localhost:5173)
pnpm run dev

# Type check (TypeScript strict mode)
pnpm run type-check

# Lint code
pnpm run lint

# Build for production
pnpm run build

# Preview production build
pnpm run preview
```

**Quality Gates:**
- Build time: < 15 seconds
- Bundle size (gzipped): < 500KB
- Zero `any` types in strict mode

### Concurrent Development

Run both backend and frontend simultaneously in separate terminals:

```bash
# Terminal 1
cd services/backend && dotnet run --project LlmTokenPrice.API

# Terminal 2
cd apps/web && pnpm run dev
```

Vite dev server automatically proxies `/api/*` requests to `http://localhost:5000`.

### Database Setup

**Option A: Docker Compose (Recommended)**
```bash
cd services/backend/LlmTokenPrice.API
docker-compose up -d
```

**Option B: Manual Setup**
1. Start PostgreSQL 16 server
2. Create database: `CREATE DATABASE llm_token_price;`
3. Start Redis 7.2 server: `redis-server`

Then run EF migrations: `dotnet ef database update --project LlmTokenPrice.Infrastructure`

## Key Architectural Patterns

### 1. QAPS Algorithm (Quality-Adjusted Price per Score)

The "Best Value" smart filter uses this algorithm:

```
QAPS = Composite Quality Score / Total Price

Where:
  Composite Quality Score = Σ (Normalized Benchmark Score × Weight)
  Normalized Score = (Score - Min) / (Max - Min)

Benchmark Weights:
  - Reasoning: 30% (MMLU, Big-Bench Hard)
  - Code: 25% (HumanEval, MBPP)
  - Math: 20% (GSM8K, MATH)
  - Language: 15% (HellaSwag, TruthfulQA)
  - Multimodal: 10% (MMMU, VQA)
```

QAPS scores are cached in Redis (1hr TTL) and invalidated on model updates.

### 2. Multi-Layer Caching Strategy

```
Client (TanStack Query, 5min)
  → Redis API Cache (1hr TTL)
    → Redis Computed Values (QAPS, 1hr TTL)
      → PostgreSQL (source of truth)
```

**Cache Invalidation:** Admin updates → Domain events → Cache bust via patterns (`cache:models:*`, `cache:bestvalue:*`)

### 3. Database Schema Patterns

- **UUIDs** as primary keys (distributed-friendly, non-sequential)
- **Soft deletes** via `is_active` flag (audit trail preservation)
- **TimescaleDB** extension for price history (Phase 2)
- **JSONB** for flexible metadata (benchmark changes, audit logs)

### 4. API Contract Format

**Success Response:**
```json
{
  "data": { /* entity */ },
  "meta": { "timestamp": "...", "cached": true }
}
```

**Error Response:**
```json
{
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "Input price must be positive",
    "details": { "field": "inputPrice", "value": -1 }
  }
}
```

## Critical Design Decisions (ADRs)

These architectural decision records (ADRs) guide implementation choices:

1. **Hexagonal Architecture:** Domain logic isolated from frameworks (testable without EF/HTTP)
2. **SPA over SSR:** Client-side filtering faster than server round-trips for 100+ models
3. **Monorepo:** Atomic commits across frontend/backend, single CI/CD pipeline
4. **TanStack Query + Zustand:** Separate server state (API) from client state (UI)
5. **Multi-layer Caching:** 80%+ cache hit ratio target to scale to 10K+ users
6. **PostgreSQL + TimescaleDB:** Unified database for OLTP + time-series pricing data
7. **JWT for Admin:** Simple token auth (HttpOnly cookies), no OAuth for MVP

See `docs/architecture-decisions.md` for complete ADR details.

## Testing Strategy

**Backend Testing:**
- **Unit Tests (70%):** Domain services, QAPS calculation, validators
  - Tools: xUnit, Moq, FluentAssertions
- **Integration Tests (25%):** EF Core repositories, API controllers, Redis caching
  - Tools: xUnit, TestContainers (PostgreSQL/Redis), WebApplicationFactory
- **E2E Tests (5%):** Critical user flows (comparison, admin CRUD)
  - Tools: Playwright

**Frontend Testing:**
- **Unit Tests (70%):** Utility functions, custom hooks, Zustand stores
  - Tools: Vitest, Testing Library
- **Integration Tests (25%):** Component integration, MSW-mocked API
  - Tools: Vitest, MSW, Testing Library
- **E2E Tests (5%):** Same Playwright tests as backend

**Coverage Goals:** 70%+ overall, 90%+ domain layer

## Naming Conventions

**C# Backend:**
- PascalCase: Classes, methods, properties (`ModelRepository`, `CalculateQAPSAsync`)
- Interfaces: `I` prefix (`IModelRepository`)
- Async methods: `Async` suffix
- camelCase: Local variables, parameters

**TypeScript Frontend:**
- PascalCase: Components, types (`ModelTable`, `ModelDto`)
- camelCase: Functions, variables (`formatPrice`, `selectedModels`)
- UPPER_SNAKE_CASE: Constants (`API_BASE_URL`)
- Hooks: `use` prefix (`useModels`, `useCostCalculation`)

**Database:**
- snake_case: Tables, columns (`model_benchmark_scores`, `input_price_per_1m`)
- Plural table names (`models`, `benchmarks`)
- Foreign keys: `{table}_id` (`model_id`, `benchmark_id`)

## Component Organization Principles

**Backend:**
- Organize by layer, not feature
- Domain → Application → Infrastructure dependency flow
- No circular dependencies (enforce with architecture tests)

**Frontend:**
- Organize components by domain, NOT by type
  - ✅ `/components/models/ModelTable.tsx`
  - ❌ `/components/tables/ModelTable.tsx`
- One TanStack Query hook per API resource
- Minimal global state (prefer local state + server state)

## Important Implementation Notes

### When Adding New Features

1. **Backend:** Start in Domain layer (entities, domain services), work outward to Application (use cases), then Infrastructure (adapters)
2. **Frontend:** Define TypeScript types first (match backend DTOs), create API client functions, then build components
3. **Always update cache invalidation** when adding mutations (domain events → cache bust patterns)

### Code Quality Standards

- **Zero `any` types** in TypeScript (strict mode enforced)
- **No framework dependencies** in Domain layer (pure C#)
- **Async all the way:** Use `async/await` for I/O (DB, Redis, HTTP)
- **Structured logging:** Serilog with semantic properties (`Log.Information("{Action} {@Model}", "Create", model)`)

### Performance Targets

- Page load: < 2 seconds
- API calculations: < 100ms
- Chart rendering: < 1 second
- Cache hit ratio: > 80%

## Documentation

Key documentation files in `docs/`:

- **PRD.md** - Product Requirements Document (FRs, NFRs)
- **epics.md** - Epic breakdown (8 epics, 83+ stories)
- **solution-architecture.md** - Detailed architecture (this summary's source)
- **architecture-decisions.md** - All ADRs with rationale
- **tech-spec-epic-*.md** - Detailed technical specifications per epic

## Tech Stack Summary

| Layer | Technology | Version |
|-------|------------|---------|
| Backend Framework | ASP.NET Core | .NET 9 |
| Frontend Framework | React | 19.x |
| Build Tool | Vite (Rolldown) | 7.x |
| Language (Frontend) | TypeScript | 5.9.x |
| State Management | Zustand | 5.x |
| Data Fetching | TanStack Query | 5.x |
| Tables | TanStack Table | 8.x |
| Charts | Chart.js | 4.x |
| Styling | TailwindCSS | 4.x |
| Database | PostgreSQL | 16 |
| Cache | Redis | 7.2 |
| ORM | Entity Framework Core | 9.x |
| Package Manager | pnpm | 10.x |

## Git Workflow

**Branch Strategy:**
- `main` - Production-ready code
- Feature branches: `feature/epic-N-description`
- Bug fixes: `fix/description`

**Commit Conventions:**
```
feat(epic-3): Add model table with TanStack Table
fix(qaps): Correct normalization formula for MATH benchmark
docs(architecture): Add ADR for hexagonal architecture
test(calculator): Add unit tests for cost estimation
```

## Health Check & Verification

**Backend Health:**
```bash
curl http://localhost:5000/api/health
# Should return: 200 OK with database + Redis status
```

**Frontend Verification:**
- Visit `http://localhost:5173`
- Hot Module Replacement (HMR) should be active
- Console should show no errors

**Database Connection:**
```bash
# Check PostgreSQL
psql -h localhost -U your_user -d llm_token_price -c "SELECT 1;"

# Check Redis
redis-cli ping
# Should return: PONG
```

---

**Last Updated:** 2025-10-16
**Generated by:** /init command (BMad Method workflow)
