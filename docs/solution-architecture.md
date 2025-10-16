# Solution Architecture Document

**Project:** llm-token-price
**Date:** 2025-10-16
**Author:** Pablo

## Executive Summary

LLM Cost Comparison Platform is an enterprise-scale web application (Level 4) designed to enable data-driven LLM model selection, targeting 5,000+ MAU. Architecture combines React 18+ SPA with .NET 8 API using hexagonal architecture, optimized for high information density, real-time calculations (<100ms), and multi-layer caching.

**Key architectural characteristics:**
- **Pattern:** SPA + REST API monolith (potential microservices extraction post-MVP)
- **Scale:** Designed for 10K+ MAU, 100+ models, 100 concurrent users
- **Performance:** <2s initial load, <100ms calculations, <1s chart rendering
- **Data:** PostgreSQL + TimescaleDB (time-series pricing), Redis (multi-layer cache)
- **Infrastructure:** Hexagonal architecture enables independent evolution of domain logic, data adapters, and UI

## 1. Technology Stack and Decisions

### 1.1 Technology and Library Decision Table

| Category | Technology | Version | Justification |
|----------|-----------|---------|---------------|
| **Backend Framework** | ASP.NET Core | 8.0 | Modern web API with minimal APIs, native async/await, dependency injection, cross-platform |
| **Backend Language** | C# | 12 | Type-safe, async-first, LINQ for complex data queries, strong tooling |
| **Frontend Framework** | React | 18.2.0 | Concurrent rendering, mature ecosystem, team expertise, component reusability |
| **Frontend Language** | TypeScript | 5.3.0 | Type safety, IDE support, catches errors at compile time, self-documenting APIs |
| **Build Tool** | Vite | 5.0.0 | Fast HMR, optimized production builds, native ESM, better than CRA/Webpack |
| **Database** | PostgreSQL | 16.0 | JSONB support, full-text search, TimescaleDB extension for price history |
| **Database Extension** | TimescaleDB | 2.13.0 | Efficient time-series storage for pricing history (Phase 2), hypertables, continuous aggregates |
| **ORM** | Entity Framework Core | 8.0.0 | Code-first migrations, LINQ queries, change tracking, repository pattern support |
| **Cache** | Redis | 7.2 (Upstash) | Multi-layer caching (API responses, computed QAPS scores), session storage, pub/sub for invalidation |
| **Task Queue** | MassTransit | 8.1.0 | Message-driven architecture, saga support, outbox pattern, future price scraping pipeline |
| **State Management** | Zustand | 4.4.7 | Minimal boilerplate, TypeScript-first, no Provider hell, < 1KB, simpler than Redux/Context |
| **Data Fetching** | TanStack Query | 5.17.0 | Declarative data fetching, caching, optimistic updates, background refetch, devtools |
| **Table Library** | TanStack Table | 8.11.0 | Headless table logic, virtualization, sorting/filtering, column visibility, 50K+ rows support |
| **Charts** | Chart.js | 4.4.1 | Mature, performant canvas rendering, extensive chart types, responsive, accessibility hooks |
| **Styling** | TailwindCSS | 3.4.0 | Utility-first, design system enforcement, tree-shaking, small bundle, JIT compilation |
| **Forms** | React Hook Form | 7.49.0 | Uncontrolled forms, minimal re-renders, schema validation integration, async validation |
| **Validation** | Zod | 3.22.4 | TypeScript-first schema validation, shared BE/FE validation rules, runtime type checking |
| **Routing** | React Router | 6.21.0 | Standard SPA routing, nested routes, loaders/actions pattern, URL state management |
| **HTTP Client** | Axios | 1.6.0 | Interceptors for auth tokens, request/response transformation, TypeScript support, timeout handling |
| **Icons** | Lucide React | 0.300.0 | Tree-shakeable, consistent design, < 1KB per icon, better than Font Awesome/Material |
| **Date Handling** | date-fns | 3.0.0 | Tree-shakeable, immutable, timezone support, modern alternative to Moment.js |
| **Testing (Unit)** | xUnit | 2.6.0 | Backend unit testing, parallel execution, theory tests for data-driven scenarios |
| **Testing (E2E)** | Playwright | 1.40.0 | Cross-browser E2E, parallel execution, trace viewer, component testing support |
| **Mocking** | MSW | 2.0.0 | Mock Service Worker for API mocking in tests, no axios/fetch mocking, realistic testing |
| **Logging** | Serilog | 3.1.0 | Structured logging, sinks (console, file, seq), enrichers, request tracing |
| **Authentication** | JWT Bearer | Manual | Simple JWT tokens for admin auth (no OAuth needed for MVP), HttpOnly cookies for web |
| **API Documentation** | Swagger/OpenAPI | 6.5.0 (Swashbuckle) | Auto-generated API docs, try-it-out functionality, TypeScript client generation |
| **Containerization** | Docker | 24.0.0 | Development consistency, production deployment, PostgreSQL/Redis dev containers |
| **CI/CD** | GitHub Actions | N/A | Automated build/test, branch protection, automated deployments, free for public repos |

### 1.2 Technology Decisions Rationale

**Why React over Next.js/Remix?**
- **No SSR needed**: Platform is data-intensive SPA, not content site (no SEO requirements for comparison tables)
- **API-first**: Clean separation enables future mobile apps, public API, desktop clients
- **Complexity**: SSR adds deployment complexity without value for authenticated dashboards
- **Performance**: Client-side filtering/sorting faster than server round-trips for 100 models

**Why Hexagonal Architecture?**
- **Domain isolation**: Business logic (QAPS calculation, filtering algorithms) independent of infrastructure
- **Testability**: Domain services testable without database/HTTP concerns
- **Maintainability**: Clear boundaries prevent feature creep contamination
- **Future-proof**: Easy extraction to microservices (e.g., scraping service) post-MVP

**Why PostgreSQL + TimescaleDB?**
- **TimescaleDB**: Hypertables optimize price history queries (Phase 2 automated scraping)
- **JSONB**: Store flexible benchmark metadata without schema migrations
- **Full-text search**: Model/provider search without Elasticsearch complexity
- **Proven scale**: Handles 10K+ concurrent users with proper indexing

**Why Redis multi-layer caching?**
- **Layer 1**: API responses (`GET /api/models` - 1hr TTL) - reduces DB load
- **Layer 2**: Computed values (QAPS scores - 1hr TTL) - expensive calculations
- **Layer 3**: Client-side (TanStack Query - 5min stale) - instant UI updates
- **Invalidation**: Pub/sub on model updates triggers cache busts across layers

**Why MassTransit over raw RabbitMQ/Kafka?**
- **Abstraction**: Message broker abstraction (swap Redis → RabbitMQ → AWS SQS)
- **Patterns**: Built-in retry policies, outbox pattern (eventual consistency), saga support
- **Future scraping**: Price scraping pipeline (schedule → fetch → validate → store → cache invalidation)
- **Simpler than**: NServiceBus (paid), raw RabbitMQ (boilerplate)

## 2. Application Architecture

### 2.1 Architecture Pattern: Hexagonal Architecture (Ports & Adapters)

```
┌─────────────────────────────────────────────────────────────────────┐
│                        PRESENTATION LAYER                            │
│  ┌─────────────────┐                  ┌─────────────────┐           │
│  │   React SPA     │                  │  ASP.NET Core   │           │
│  │   (Vite)        │ ◄──── HTTP ────► │  Web API        │           │
│  │  Zustand/Query  │                  │  Controllers    │           │
│  └─────────────────┘                  └────────┬────────┘           │
│                                                 │                     │
└─────────────────────────────────────────────────┼─────────────────────┘
                                                  │
┌─────────────────────────────────────────────────▼─────────────────────┐
│                        APPLICATION LAYER                              │
│  ┌───────────────────────────────────────────────────────────────┐   │
│  │  Application Services (Use Cases)                             │   │
│  │  • ModelQueryService        • BestValueCalculationService     │   │
│  │  • CostCalculationService   • AdminModelManagementService     │   │
│  │  • BenchmarkQueryService    • DataQualityService              │   │
│  └───────────────────────────────────────────────────────────────┘   │
│                                 │                                     │
└─────────────────────────────────┼─────────────────────────────────────┘
                                  │
┌─────────────────────────────────▼─────────────────────────────────────┐
│                         DOMAIN LAYER (CORE)                           │
│  ┌───────────────────────────────────────────────────────────────┐   │
│  │  Domain Entities                                              │   │
│  │  • Model           • Capability        • BenchmarkScore       │   │
│  │  • Benchmark       • Provider          • PricingHistory       │   │
│  └───────────────────────────────────────────────────────────────┘   │
│  ┌───────────────────────────────────────────────────────────────┐   │
│  │  Domain Services (Business Logic)                            │   │
│  │  • QAPSCalculator     • PriceComparisonEngine                │   │
│  │  • ModelFilterer      • BenchmarkNormalizer                  │   │
│  │  • CostEstimator      • DataValidator                        │   │
│  └───────────────────────────────────────────────────────────────┘   │
│  ┌───────────────────────────────────────────────────────────────┐   │
│  │  Repository Interfaces (Ports)                               │   │
│  │  • IModelRepository      • IBenchmarkRepository              │   │
│  │  • ICacheRepository      • IAuditLogRepository               │   │
│  └───────────────────────────────────────────────────────────────┘   │
└───────────────────────────────────────────────────────────────────────┘
                                  │
┌─────────────────────────────────▼─────────────────────────────────────┐
│                      INFRASTRUCTURE LAYER                             │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐               │
│  │  PostgreSQL  │  │    Redis     │  │  MassTransit │               │
│  │  Repository  │  │  Cache Impl  │  │  Messaging   │               │
│  │  (EF Core)   │  │  (Upstash)   │  │  (Future)    │               │
│  └──────────────┘  └──────────────┘  └──────────────┘               │
└───────────────────────────────────────────────────────────────────────┘
```

**Hexagonal Boundaries:**

**Inbound Ports (Driving Side):**
- API Controllers → Application Services (HTTP adapter)
- Future: GraphQL resolvers, CLI commands, Webhooks

**Outbound Ports (Driven Side):**
- `IModelRepository` → EF Core PostgreSQL implementation
- `ICacheRepository` → Redis implementation
- `IMessageBus` → MassTransit implementation (future)
- `IExternalApiClient` → HTTP client for price scraping (Phase 2)

**Benefits:**
- Swap PostgreSQL → MongoDB without touching domain logic
- Test domain services with in-memory repositories (fast unit tests)
- Add gRPC API alongside REST without modifying use cases

### 2.2 Repository Strategy: Monorepo

```
/llm-token-price
├── /backend              # .NET 8 API
├── /frontend             # React SPA
├── /docs                 # Architecture, PRD, UX spec
├── /.github/workflows    # CI/CD pipelines
└── /docker-compose.yml   # Local dev stack (PG + Redis)
```

**Why monorepo:**
- Shared TypeScript types (generate from C# DTOs via NSwag/OpenAPI)
- Atomic commits across frontend/backend
- Single CI/CD pipeline
- Coordinated versioning
- Simpler for 1-2 developer team

**Polyrepo consideration (post-MVP):**
- If mobile app added → separate repo (different release cycle)
- If scraping service extracted → microservice repo
- If API opens to public → separate API versioning

### 2.3 Client-Side Architecture (React SPA)

**Component Hierarchy:**
```
App
├── Layout (Header, Footer)
├── HomePage (/)
│   ├── FilterSidebar
│   ├── ModelTable (TanStack Table)
│   │   └── ModelRow[]
│   ├── ComparisonBasket
│   └── ModelDetailModal (lazy)
├── CalculatorPage (/calculator)
│   ├── CostCalculatorWidget
│   ├── CostResultsTable
│   └── CostChart
├── ComparisonPage (/compare)
│   ├── ModelCard[]
│   ├── ComparisonTable
│   ├── BenchmarkChart
│   └── CapabilityMatrix
└── AdminPanel (/admin)
    ├── AdminLayout
    ├── ModelManagement
    │   ├── ModelList
    │   ├── ModelForm
    │   └── BulkOperations
    └── BenchmarkManagement
```

**State Architecture:**
```typescript
// Global state (Zustand)
interface AppState {
  selectedModels: Model[]           // Comparison basket
  filterState: FilterCriteria       // Active filters
  viewPreferences: ViewPrefs        // Column visibility, theme
}

// Server state (TanStack Query)
useQuery('models')                  // GET /api/models (cached 5min)
useQuery('model-detail', id)        // GET /api/models/{id}
useQuery('best-value')              // GET /api/smart-filters/best-value
useMutation('update-model')         // PUT /api/admin/models/{id}

// Local component state (useState)
// Form inputs, modal open/close, table pagination
```

**Data Flow:**
1. Component mounts → TanStack Query fetches data → updates cache
2. User filters table → Zustand updates filter state → Table re-renders (client-side filter)
3. User selects model → Zustand adds to basket → ComparisonBasket re-renders
4. Admin updates pricing → Mutation → Cache invalidation → Refetch → Redis bust

### 2.4 API Structure

**RESTful endpoints:**
```
Public API:
  GET    /api/models                      # List all models (cached 1hr)
  GET    /api/models/{id}                 # Model detail (cached 30min)
  GET    /api/models/{id}/benchmarks      # All benchmarks for model
  GET    /api/smart-filters/best-value    # Best value ranked models (cached 1hr)
  GET    /api/benchmarks                  # Benchmark definitions
  GET    /api/health                      # Health check (DB + Redis)

Admin API (JWT auth):
  POST   /api/admin/auth/login            # JWT token generation
  GET    /api/admin/models                # Admin list (no cache)
  POST   /api/admin/models                # Create model
  PUT    /api/admin/models/{id}           # Update model
  DELETE /api/admin/models/{id}           # Soft-delete model
  POST   /api/admin/models/bulk-update    # Bulk operations
  POST   /api/admin/models/{id}/benchmarks # Add benchmark score
  POST   /api/admin/benchmarks/import-csv # Bulk import benchmarks
  GET    /api/admin/dashboard/metrics     # Data quality metrics
  GET    /api/admin/audit-log             # Admin action history
```

**Response format:**
```json
// Standard success
{
  "data": { ... },
  "meta": { "timestamp": "2025-10-16T...", "cached": true }
}

// Error (4xx/5xx)
{
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "Input price must be positive",
    "details": { "field": "inputPrice", "value": -1 }
  }
}
```

**Versioning strategy (future):**
- URL versioning: `/api/v2/models` when breaking changes introduced
- MVP: No versioning, API is internal
- Public API (Phase 3): Semantic versioning, deprecation warnings

## 3. Data Architecture

### 3.1 Database Schema (PostgreSQL + TimescaleDB)

```sql
-- Core entities (OLTP workload)

CREATE TABLE models (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  name VARCHAR(255) NOT NULL,
  provider VARCHAR(100) NOT NULL,
  version VARCHAR(50),
  release_date DATE,
  status VARCHAR(20) DEFAULT 'active', -- active|deprecated|beta
  input_price_per_1m DECIMAL(10,6) NOT NULL,
  output_price_per_1m DECIMAL(10,6) NOT NULL,
  currency VARCHAR(3) DEFAULT 'USD',
  pricing_valid_from TIMESTAMP,
  pricing_valid_to TIMESTAMP,
  last_scraped_at TIMESTAMP,          -- Future: automated scraping
  is_active BOOLEAN DEFAULT true,
  created_at TIMESTAMP DEFAULT now(),
  updated_at TIMESTAMP DEFAULT now(),
  CONSTRAINT unique_model_provider UNIQUE(name, provider)
);

CREATE INDEX idx_models_provider ON models(provider);
CREATE INDEX idx_models_status ON models(status) WHERE is_active = true;
CREATE INDEX idx_models_updated ON models(updated_at DESC);

-- Capabilities (1:1 with models)

CREATE TABLE model_capabilities (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  model_id UUID NOT NULL REFERENCES models(id) ON DELETE CASCADE,
  context_window INTEGER NOT NULL,
  max_output_tokens INTEGER,
  supports_function_calling BOOLEAN DEFAULT false,
  supports_vision BOOLEAN DEFAULT false,
  supports_audio_input BOOLEAN DEFAULT false,
  supports_audio_output BOOLEAN DEFAULT false,
  supports_streaming BOOLEAN DEFAULT true,
  supports_json_mode BOOLEAN DEFAULT false,
  CONSTRAINT unique_model_capability UNIQUE(model_id)
);

CREATE INDEX idx_capabilities_model ON model_capabilities(model_id);

-- Benchmarks (definitions)

CREATE TABLE benchmarks (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  benchmark_name VARCHAR(50) NOT NULL UNIQUE, -- MMLU, HumanEval, etc.
  full_name VARCHAR(255),
  description TEXT,
  category VARCHAR(50),              -- reasoning|code|math|language|multimodal
  interpretation VARCHAR(20),        -- higher_better|lower_better
  typical_range_min DECIMAL(5,2),
  typical_range_max DECIMAL(5,2),
  weight_in_qaps DECIMAL(3,2),       -- 0.30 for reasoning, 0.25 for code, etc.
  created_at TIMESTAMP DEFAULT now()
);

-- Benchmark scores (many-to-many: models ↔ benchmarks)

CREATE TABLE model_benchmark_scores (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  model_id UUID NOT NULL REFERENCES models(id) ON DELETE CASCADE,
  benchmark_id UUID NOT NULL REFERENCES benchmarks(id) ON DELETE CASCADE,
  score DECIMAL(6,2) NOT NULL,
  max_score DECIMAL(6,2),
  normalized_score DECIMAL(5,4),    -- Normalized 0-1 for QAPS calculation
  test_date DATE,
  source_url TEXT,
  verified BOOLEAN DEFAULT false,
  notes TEXT,
  created_at TIMESTAMP DEFAULT now(),
  CONSTRAINT unique_model_benchmark UNIQUE(model_id, benchmark_id)
);

CREATE INDEX idx_scores_model ON model_benchmark_scores(model_id);
CREATE INDEX idx_scores_benchmark ON model_benchmark_scores(benchmark_id);

-- Computed values cache (avoid recalculating QAPS on every request)

CREATE TABLE model_computed_metrics (
  model_id UUID PRIMARY KEY REFERENCES models(id) ON DELETE CASCADE,
  composite_quality_score DECIMAL(6,2),  -- Weighted avg of normalized benchmarks
  qaps_score DECIMAL(10,6),              -- Quality / Price
  last_computed_at TIMESTAMP DEFAULT now()
);

-- Admin audit log

CREATE TABLE admin_audit_log (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  admin_user VARCHAR(100) NOT NULL,
  action VARCHAR(50) NOT NULL,        -- CREATE|UPDATE|DELETE|BULK_UPDATE
  entity_type VARCHAR(50) NOT NULL,   -- MODEL|BENCHMARK|SCORE
  entity_id UUID,
  changes_json JSONB,                 -- {before: {...}, after: {...}}
  ip_address INET,
  user_agent TEXT,
  created_at TIMESTAMP DEFAULT now()
);

CREATE INDEX idx_audit_log_user ON admin_audit_log(admin_user);
CREATE INDEX idx_audit_log_created ON admin_audit_log(created_at DESC);

-- Time-series pricing history (Phase 2 - TimescaleDB hypertable)

CREATE TABLE model_pricing_history (
  time TIMESTAMPTZ NOT NULL,
  model_id UUID NOT NULL REFERENCES models(id) ON DELETE CASCADE,
  input_price_per_1m DECIMAL(10,6),
  output_price_per_1m DECIMAL(10,6),
  source VARCHAR(50),                 -- scraper|manual|api
  PRIMARY KEY (time, model_id)
);

-- Convert to hypertable (TimescaleDB)
-- SELECT create_hypertable('model_pricing_history', 'time');
```

**Schema design decisions:**

- **UUIDs**: Better for distributed systems, non-sequential IDs (security), easier merge conflicts
- **Soft deletes**: `is_active` flag preserves data for audit trail
- **JSONB**: `changes_json` stores audit details without schema migrations for every field
- **Normalization**: 3NF for transactional integrity, denormalized computed metrics table for performance
- **TimescaleDB** (Phase 2): Hypertables compress historical pricing, continuous aggregates for "average price over 30 days"

### 3.2 Data Relationships

```
models (1) ──< (1) model_capabilities
models (1) ──< (N) model_benchmark_scores >── (1) benchmarks
models (1) ──< (1) model_computed_metrics
models (1) ──< (N) model_pricing_history  [Phase 2]
```

**Cascade rules:**
- Delete model → cascade delete capabilities, benchmark scores, computed metrics
- Delete benchmark → cascade delete associated scores (admin confirms)
- Soft-delete model → keep all related data, just hide from public API

### 3.3 Migration Strategy

**EF Core Code-First Migrations:**

```bash
# Generate migration
dotnet ef migrations add InitialSchema --project Backend.Infrastructure

# Apply to dev database
dotnet ef database update --project Backend.Infrastructure

# Rollback (if needed)
dotnet ef database update PreviousMigrationName
```

**Production migration workflow:**
1. Generate migration in development
2. Review SQL (via `dotnet ef migrations script`)
3. Test on staging database
4. Apply to production during maintenance window
5. Backup database before migration

**Data seeding:**
- Development: Seed 10-15 sample models via `DbContext.OnModelCreating`
- Production: CSV import via admin panel (not code-based seeds)

## 4. Caching Strategy (Multi-Layer)

### 4.1 Cache Architecture

```
┌─────────────────────────────────────────────────────────────┐
│  Layer 1: Client-Side (TanStack Query)                      │
│  • Stale time: 5 minutes                                    │
│  • Cache time: 30 minutes                                   │
│  • Instant UI updates, background refetch                   │
└────────────────────────┬────────────────────────────────────┘
                         │ (HTTP request if stale/miss)
┌────────────────────────▼────────────────────────────────────┐
│  Layer 2: API Response Cache (Redis)                        │
│  • GET /api/models: 1 hour TTL                              │
│  • GET /api/models/{id}: 30 min TTL                         │
│  • GET /api/smart-filters/best-value: 1 hour TTL           │
└────────────────────────┬────────────────────────────────────┘
                         │ (DB query if miss)
┌────────────────────────▼────────────────────────────────────┐
│  Layer 3: Computed Values Cache (Redis)                     │
│  • QAPS scores: 1 hour TTL                                  │
│  • Normalized benchmark scores: 1 hour TTL                  │
│  • Aggregated stats (dashboard): 30 min TTL                 │
└────────────────────────┬────────────────────────────────────┘
                         │ (Compute from DB)
┌────────────────────────▼────────────────────────────────────┐
│  Layer 4: Database (PostgreSQL)                             │
│  • Source of truth                                          │
│  • Query cache (PostgreSQL internal)                        │
└─────────────────────────────────────────────────────────────┘
```

### 4.2 Cache Keys Design

```
# API responses
cache:models:list:v1                          # All models
cache:model:{id}:v1                           # Single model detail
cache:bestvalue:v1                            # Best value filter
cache:benchmarks:list:v1                      # Benchmark definitions

# Computed values
cache:qaps:{model_id}:v1                      # QAPS score
cache:dashboard:metrics:v1                    # Admin dashboard stats
```

**Versioning:** `v1` suffix allows cache invalidation on schema changes (bump to `v2`)

### 4.3 Cache Invalidation Strategy

**Triggers:**
1. Admin updates model pricing → Invalidate: `cache:models:*`, `cache:bestvalue:*`, `cache:qaps:{id}:*`
2. Admin adds benchmark score → Invalidate: `cache:model:{id}:*`, `cache:bestvalue:*`
3. Admin creates/deletes model → Invalidate: `cache:models:*`, `cache:bestvalue:*`

**Implementation:**
```csharp
public class ModelUpdateHandler : INotificationHandler<ModelUpdatedEvent>
{
    private readonly ICacheService _cache;

    public async Task Handle(ModelUpdatedEvent evt, CancellationToken ct)
    {
        // Invalidate specific model
        await _cache.RemoveAsync($"cache:model:{evt.ModelId}:v1");

        // Invalidate list caches
        await _cache.RemovePatternAsync("cache:models:*");
        await _cache.RemovePatternAsync("cache:bestvalue:*");

        // Recompute QAPS (background job)
        await _qapsCalculator.RecalculateAsync(evt.ModelId);
    }
}
```

### 4.4 Cache Performance Targets

- Cache hit ratio: >80% for public API endpoints
- Cache miss penalty: <200ms (DB query + computation + cache write)
- Redis latency: <10ms (Upstash or local Redis)
- Memory budget: <500MB Redis memory for MVP (100 models × 50KB avg)

## 5. Smart Filter Algorithm: QAPS (Quality-Adjusted Price per Score)

### 5.1 QAPS Calculation Formula

```
QAPS = Composite Quality Score / Total Price

Where:
  Composite Quality Score = Σ (Normalized Benchmark Score × Weight)
  Total Price = (Input Price + Output Price) / 2  [simplified avg]
```

### 5.2 Benchmark Weights (Configurable)

```csharp
public static class BenchmarkWeights
{
    public const decimal Reasoning = 0.30m;    // MMLU, Big-Bench Hard
    public const decimal Code = 0.25m;         // HumanEval, MBPP
    public const decimal Math = 0.20m;         // GSM8K, MATH
    public const decimal Language = 0.15m;     // HellaSwag, TruthfulQA
    public const decimal Multimodal = 0.10m;   // MMMU, VQA (if applicable)
}
```

### 5.3 Benchmark Normalization

**Problem:** Benchmarks have different scales (0-100, 0-1, 0-10)

**Solution:** Normalize all scores to 0-1 range
```
Normalized Score = (Score - Min) / (Max - Min)

Where:
  Min = typical_range_min from benchmarks table
  Max = typical_range_max from benchmarks table
```

**Example:**
- MMLU: 85.2 → (85.2 - 0) / 100 = 0.852
- HumanEval: 0.72 → (0.72 - 0) / 1 = 0.72

### 5.4 QAPS Calculation Service

```csharp
public class QAPSCalculationService
{
    public async Task<decimal> CalculateQAPSAsync(Guid modelId)
    {
        // 1. Fetch model pricing
        var model = await _modelRepo.GetByIdAsync(modelId);
        var avgPrice = (model.InputPricePer1M + model.OutputPricePer1M) / 2;

        // 2. Fetch benchmark scores with weights
        var scores = await _benchmarkRepo.GetModelScoresWithWeightsAsync(modelId);

        // 3. Calculate weighted quality score
        decimal qualityScore = 0m;
        foreach (var score in scores)
        {
            var normalizedScore = NormalizeBenchmarkScore(score);
            qualityScore += normalizedScore * score.Benchmark.WeightInQAPS;
        }

        // 4. Handle edge cases
        if (avgPrice == 0) return decimal.MaxValue;  // Free models (rank separately)
        if (scores.Count < 3) return 0;              // Insufficient data

        // 5. Calculate QAPS
        var qaps = qualityScore / avgPrice;

        // 6. Cache result (1 hour TTL)
        await _cache.SetAsync($"cache:qaps:{modelId}:v1", qaps, TimeSpan.FromHours(1));

        return qaps;
    }
}
```

### 5.5 Best Value Ranking

**Endpoint:** `GET /api/smart-filters/best-value?limit=10`

**Query logic:**
1. Calculate QAPS for all active models (use cached values if available)
2. Filter models with ≥3 benchmark scores (minimum data quality)
3. Sort by QAPS descending
4. Return top N models with scores and explanation

**Response:**
```json
{
  "data": [
    {
      "model": { "id": "...", "name": "Claude Haiku", "provider": "Anthropic" },
      "qapsScore": 245.67,
      "qualityScore": 0.82,
      "avgPrice": 0.00334,
      "rank": 1,
      "explanation": "Scores 82% on composite quality benchmarks at $0.003/1M avg price"
    }
  ],
  "meta": {
    "algorithm": "QAPS v1.0",
    "benchmarkWeights": { "reasoning": 0.30, "code": 0.25, ... },
    "modelsEvaluated": 54,
    "modelsExcluded": 8,
    "exclusionReasons": { "insufficientData": 5, "inactive": 3 }
  }
}
```

## 6. Component Integration Overview

### 6.1 Epic-to-Component Mapping

| Epic | Frontend Components | Backend Services | Database Tables |
|------|-------------------|------------------|-----------------|
| **Epic 1: Foundation** | App shell, Layout, Routing | Health check, DI setup, DbContext | All tables (migrations) |
| **Epic 2: Admin CRUD** | AdminLayout, ModelList, ModelForm, BenchmarkForm | AdminModelService, AdminBenchmarkService | models, capabilities, benchmarks, scores, audit_log |
| **Epic 3: Public Table** | HomePage, FilterSidebar, ModelTable, SearchBar, ComparisonBasket | ModelQueryService, FilterService | models (read), capabilities, scores |
| **Epic 4: Model Detail** | ModelDetailModal, OverviewTab, BenchmarksTab, PricingTab, CostCalculator | ModelDetailService, CostCalculationService | models (detail), scores (all) |
| **Epic 5: Comparison** | ComparisonPage, ModelCard, ComparisonTable, BenchmarkChart, CapabilityMatrix | ModelComparisonService, ChartDataService | models (batch fetch), scores |
| **Epic 6: Smart Filter** | BestValueButton, QAPSScoreDisplay, ExplanationPanel | QAPSCalculationService, BestValueRankingService | benchmarks (weights), scores, computed_metrics |
| **Epic 7: Data Quality** | DataFreshnessIndicator, ValidationMessages, AdminDashboard | DataQualityService, ValidationService, AuditLogService | audit_log, models (timestamps) |
| **Epic 8: Responsive** | Mobile adaptations (FilterDrawer, CardLayout, MobileNav) | No backend changes | No schema changes |

### 6.2 Third-Party Integrations

| Integration | Purpose | When | Notes |
|------------|---------|------|-------|
| **Upstash Redis** | Cache (API + computed values) | Epic 3 (performance optimization) | Free tier: 10K commands/day sufficient for MVP |
| **Chart.js** | Visualizations (bar charts) | Epic 5 (comparison charts) | Client-side rendering, no backend integration |
| **OpenAPI/Swagger** | API documentation | Epic 1 (foundation) | Auto-generated from C# attributes |
| **GitHub Actions** | CI/CD pipeline | Epic 1 (foundation) | Automated build/test on PR, deploy on merge |
| **Docker** | Dev environment (PostgreSQL, Redis) | Epic 1 (foundation) | `docker-compose.yml` for local dev consistency |
| **Playwright** | E2E testing | Epic 7 (quality) | Test critical flows (comparison, calculator, admin CRUD) |

**Future integrations (Phase 2+):**
- **Puppeteer/Playwright**: Price scraping (provider websites)
- **AWS S3/CloudFront**: Static asset CDN (chart images, exports)
- **Sentry/Seq**: Error tracking and structured logging
- **PostHog/Plausible**: Privacy-focused analytics

## 7. API Contract Specifications

### 7.1 Public API Contracts

**GET /api/models**
```typescript
// Response
{
  data: Array<{
    id: string
    name: string
    provider: string
    inputPricePer1M: number
    outputPricePer1M: number
    currency: string
    capabilities: {
      contextWindow: number
      supportsFunctionCalling: boolean
      supportsVision: boolean
      // ... other flags
    }
    topBenchmarks: Array<{  // Top 3-5 for table display
      benchmarkName: string
      score: number
    }>
    lastUpdated: string  // ISO 8601
  }>
  meta: { cached: boolean, timestamp: string }
}
```

**GET /api/models/{id}**
```typescript
// Response
{
  data: {
    id: string
    name: string
    provider: string
    version: string
    releaseDate: string
    status: 'active' | 'deprecated' | 'beta'
    pricing: {
      inputPricePer1M: number
      outputPricePer1M: number
      currency: string
      validFrom: string
      validTo: string | null
      lastScraped: string | null
    }
    capabilities: {
      contextWindow: number
      maxOutputTokens: number
      supportsFunctionCalling: boolean
      supportsVision: boolean
      supportsAudioInput: boolean
      supportsAudioOutput: boolean
      supportsStreaming: boolean
      supportsJsonMode: boolean
    }
    benchmarks: Array<{
      id: string
      benchmarkName: string
      fullName: string
      category: string
      score: number
      maxScore: number
      normalizedScore: number
      testDate: string
      sourceUrl: string | null
      verified: boolean
    }>
    computedMetrics: {
      compositeQualityScore: number
      qapsScore: number
      rank: number  // Rank in best value (if applicable)
    }
  }
}
```

**GET /api/smart-filters/best-value**
```typescript
// Request
GET /api/smart-filters/best-value?limit=10

// Response
{
  data: Array<{
    model: { /* same as GET /api/models item */ }
    qapsScore: number
    qualityScore: number
    avgPrice: number
    rank: number
    explanation: string
  }>
  meta: {
    algorithm: string          // "QAPS v1.0"
    benchmarkWeights: Record<string, number>
    modelsEvaluated: number
    modelsExcluded: number
    exclusionReasons: Record<string, number>
  }
}
```

### 7.2 Admin API Contracts

**POST /api/admin/models**
```typescript
// Request
{
  name: string
  provider: string
  version?: string
  releaseDate?: string
  status: 'active' | 'deprecated' | 'beta'
  pricing: {
    inputPricePer1M: number
    outputPricePer1M: number
    currency: string
    validFrom?: string
  }
  capabilities: {
    contextWindow: number
    maxOutputTokens?: number
    supportsFunctionCalling: boolean
    supportsVision: boolean
    // ... other flags
  }
}

// Response
{
  data: { id: string, /* created model */ }
  meta: { message: "Model created successfully" }
}
```

**PUT /api/admin/models/{id}**
```typescript
// Request: Same as POST (partial updates allowed)

// Response
{
  data: { /* updated model */ }
  meta: { message: "Model updated successfully", cacheInvalidated: true }
}
```

**POST /api/admin/benchmarks/import-csv**
```typescript
// Request
Content-Type: multipart/form-data
file: <CSV file>

// CSV format:
// model_id,benchmark_name,score,test_date,source_url
// uuid-1,MMLU,85.2,2025-10-01,https://...
// uuid-1,HumanEval,0.72,2025-10-01,https://...

// Response
{
  data: {
    totalRows: number
    successfulImports: number
    failedImports: number
    errors: Array<{
      row: number
      error: string
      details: any
    }>
  }
  meta: { message: "Import completed" }
}
```

## 8. Proposed Source Tree

```
/llm-token-price
├── /backend
│   ├── Backend.sln
│   ├── /src
│   │   ├── /Backend.Domain                    # Core domain (no dependencies)
│   │   │   ├── /Entities
│   │   │   │   ├── Model.cs
│   │   │   │   ├── Capability.cs
│   │   │   │   ├── Benchmark.cs
│   │   │   │   ├── BenchmarkScore.cs
│   │   │   │   └── AuditLog.cs
│   │   │   ├── /Services                      # Domain services (business logic)
│   │   │   │   ├── QAPSCalculator.cs
│   │   │   │   ├── BenchmarkNormalizer.cs
│   │   │   │   ├── CostEstimator.cs
│   │   │   │   ├── ModelFilterer.cs
│   │   │   │   └── DataValidator.cs
│   │   │   ├── /Interfaces                    # Repository ports
│   │   │   │   ├── IModelRepository.cs
│   │   │   │   ├── IBenchmarkRepository.cs
│   │   │   │   ├── ICacheRepository.cs
│   │   │   │   └── IAuditLogRepository.cs
│   │   │   └── /ValueObjects
│   │   │       ├── Price.cs
│   │   │       ├── DateRange.cs
│   │   │       └── QAPSScore.cs
│   │   │
│   │   ├── /Backend.Application               # Use cases (orchestration)
│   │   │   ├── /DTOs
│   │   │   │   ├── ModelDto.cs
│   │   │   │   ├── ModelDetailDto.cs
│   │   │   │   ├── BestValueDto.cs
│   │   │   │   └── AdminModelDto.cs
│   │   │   ├── /Services
│   │   │   │   ├── ModelQueryService.cs       # Public queries
│   │   │   │   ├── BestValueService.cs        # Smart filter logic
│   │   │   │   ├── CostCalculationService.cs  # Cost calculator
│   │   │   │   ├── AdminModelService.cs       # Admin CRUD
│   │   │   │   ├── AdminBenchmarkService.cs   # Benchmark management
│   │   │   │   └── DataQualityService.cs      # Validation, metrics
│   │   │   ├── /Mapping
│   │   │   │   └── MappingProfile.cs          # AutoMapper config
│   │   │   └── /Validators
│   │   │       ├── CreateModelValidator.cs    # FluentValidation
│   │   │       └── BenchmarkScoreValidator.cs
│   │   │
│   │   ├── /Backend.Infrastructure            # External adapters
│   │   │   ├── /Data
│   │   │   │   ├── AppDbContext.cs            # EF Core DbContext
│   │   │   │   ├── /Configurations
│   │   │   │   │   ├── ModelConfiguration.cs  # Fluent API config
│   │   │   │   │   └── BenchmarkConfiguration.cs
│   │   │   │   ├── /Migrations
│   │   │   │   │   └── [EF generated migrations]
│   │   │   │   └── /Seeds
│   │   │   │       └── SampleDataSeeder.cs
│   │   │   ├── /Repositories                  # Adapter implementations
│   │   │   │   ├── ModelRepository.cs
│   │   │   │   ├── BenchmarkRepository.cs
│   │   │   │   └── AuditLogRepository.cs
│   │   │   ├── /Caching
│   │   │   │   ├── RedisCacheService.cs       # Redis adapter
│   │   │   │   └── CacheKeys.cs
│   │   │   ├── /Messaging                     # Future: MassTransit
│   │   │   │   └── [message consumers]
│   │   │   └── /ExternalApis                  # Future: scraping
│   │   │       └── [HTTP clients]
│   │   │
│   │   └── /Backend.API                       # Web API entry point
│   │       ├── Program.cs                     # Startup, DI configuration
│   │       ├── appsettings.json
│   │       ├── appsettings.Development.json
│   │       ├── /Controllers
│   │       │   ├── ModelsController.cs        # Public API
│   │       │   ├── SmartFiltersController.cs  # Best value
│   │       │   ├── BenchmarksController.cs    # Public benchmarks
│   │       │   └── /Admin
│   │       │       ├── AdminModelsController.cs
│   │       │       ├── AdminBenchmarksController.cs
│   │       │       ├── AdminAuthController.cs
│   │       │       └── AdminDashboardController.cs
│   │       ├── /Middleware
│   │       │   ├── ErrorHandlingMiddleware.cs
│   │       │   ├── RequestLoggingMiddleware.cs
│   │       │   └── CacheMiddleware.cs
│   │       ├── /Filters
│   │       │   └── ValidateModelFilter.cs
│   │       └── /Extensions
│   │           └── ServiceCollectionExtensions.cs
│   │
│   └── /tests
│       ├── /Backend.Domain.Tests              # Domain logic unit tests
│       │   ├── QAPSCalculatorTests.cs
│       │   ├── ModelFiltererTests.cs
│       │   └── CostEstimatorTests.cs
│       ├── /Backend.Application.Tests         # Use case unit tests
│       │   ├── BestValueServiceTests.cs
│       │   └── CostCalculationServiceTests.cs
│       ├── /Backend.Infrastructure.Tests      # Adapter integration tests
│       │   ├── ModelRepositoryTests.cs
│       │   └── RedisCacheServiceTests.cs
│       └── /Backend.API.Tests                 # API integration tests
│           ├── ModelsControllerTests.cs
│           └── SmartFiltersControllerTests.cs
│
├── /frontend
│   ├── package.json
│   ├── tsconfig.json
│   ├── vite.config.ts
│   ├── tailwind.config.js
│   ├── index.html
│   ├── /src
│   │   ├── main.tsx                           # App entry point
│   │   ├── App.tsx                            # Root component
│   │   ├── /api
│   │   │   ├── client.ts                      # Axios instance
│   │   │   ├── models.ts                      # Model API calls
│   │   │   ├── smartFilters.ts                # Smart filter calls
│   │   │   ├── admin.ts                       # Admin API calls
│   │   │   └── types.ts                       # API response types
│   │   ├── /components
│   │   │   ├── /ui                            # Reusable UI primitives
│   │   │   │   ├── Button.tsx
│   │   │   │   ├── Modal.tsx
│   │   │   │   ├── Input.tsx
│   │   │   │   ├── Badge.tsx
│   │   │   │   ├── Tooltip.tsx
│   │   │   │   └── LoadingSpinner.tsx
│   │   │   ├── /layout
│   │   │   │   ├── Header.tsx
│   │   │   │   ├── Footer.tsx
│   │   │   │   └── AdminLayout.tsx
│   │   │   ├── /models
│   │   │   │   ├── ModelTable.tsx             # TanStack Table
│   │   │   │   ├── ModelRow.tsx
│   │   │   │   ├── ModelCard.tsx              # Mobile card view
│   │   │   │   ├── ModelDetailModal.tsx
│   │   │   │   ├── OverviewTab.tsx
│   │   │   │   ├── BenchmarksTab.tsx
│   │   │   │   └── PricingTab.tsx
│   │   │   ├── /filters
│   │   │   │   ├── FilterSidebar.tsx
│   │   │   │   ├── ProviderFilter.tsx
│   │   │   │   ├── CapabilityFilter.tsx
│   │   │   │   ├── PriceRangeFilter.tsx
│   │   │   │   └── BestValueButton.tsx
│   │   │   ├── /comparison
│   │   │   │   ├── ComparisonBasket.tsx
│   │   │   │   ├── ComparisonTable.tsx
│   │   │   │   ├── BenchmarkChart.tsx         # Chart.js wrapper
│   │   │   │   ├── CapabilityMatrix.tsx
│   │   │   │   └── PricingComparisonChart.tsx
│   │   │   ├── /calculator
│   │   │   │   ├── CostCalculatorWidget.tsx
│   │   │   │   ├── CostResultsTable.tsx
│   │   │   │   ├── CostChart.tsx
│   │   │   │   └── PresetButtons.tsx
│   │   │   └── /admin
│   │   │       ├── ModelList.tsx
│   │   │       ├── ModelForm.tsx
│   │   │       ├── BenchmarkForm.tsx
│   │   │       ├── CSVImport.tsx
│   │   │       ├── AdminDashboard.tsx
│   │   │       └── AuditLog.tsx
│   │   ├── /pages
│   │   │   ├── HomePage.tsx                   # Main comparison table
│   │   │   ├── CalculatorPage.tsx             # Standalone calculator
│   │   │   ├── ComparisonPage.tsx             # Side-by-side comparison
│   │   │   └── /admin
│   │   │       ├── AdminLoginPage.tsx
│   │   │       ├── AdminModelsPage.tsx
│   │   │       └── AdminBenchmarksPage.tsx
│   │   ├── /hooks
│   │   │   ├── useModels.ts                   # TanStack Query hooks
│   │   │   ├── useModelDetail.ts
│   │   │   ├── useBestValue.ts
│   │   │   ├── useCostCalculation.ts
│   │   │   └── useAuth.ts                     # Admin auth
│   │   ├── /store
│   │   │   ├── appStore.ts                    # Zustand store
│   │   │   ├── filterStore.ts
│   │   │   └── comparisonStore.ts
│   │   ├── /utils
│   │   │   ├── formatters.ts                  # Price, date formatting
│   │   │   ├── validators.ts                  # Client-side validation
│   │   │   └── chartHelpers.ts                # Chart.js config
│   │   ├── /styles
│   │   │   └── globals.css                    # Tailwind imports
│   │   └── /types
│   │       ├── models.ts                      # Domain types
│   │       ├── filters.ts
│   │       └── api.ts
│   │
│   └── /tests
│       ├── /unit
│       │   ├── formatters.test.ts
│       │   └── validators.test.ts
│       ├── /integration
│       │   └── api.test.ts                    # MSW mocked API tests
│       └── /e2e
│           ├── comparison.spec.ts             # Playwright E2E
│           ├── calculator.spec.ts
│           └── admin.spec.ts
│
├── /docs
│   ├── PRD.md
│   ├── epics.md
│   ├── ux-specification.md
│   ├── solution-architecture.md               # This document
│   ├── architecture-decisions.md              # ADRs
│   └── /tech-specs
│       ├── tech-spec-epic-1.md
│       ├── tech-spec-epic-2.md
│       └── [... epic 3-8]
│
├── /.github
│   └── /workflows
│       ├── backend-ci.yml                     # .NET build/test
│       ├── frontend-ci.yml                    # React build/test
│       └── e2e-tests.yml                      # Playwright E2E
│
├── docker-compose.yml                         # Local dev (PG + Redis)
├── .gitignore
└── README.md
```

**Critical folders explained:**

- **Backend.Domain**: Business logic isolated from frameworks (pure C#, testable without EF/HTTP)
- **Backend.Application**: Use cases orchestrate domain services, map to DTOs
- **Backend.Infrastructure**: Adapters for databases, caching, messaging (swappable implementations)
- **Backend.API**: HTTP entry point, controllers delegate to application services
- **frontend/src/components**: Organized by domain (models, filters, comparison) not by type (buttons, forms)
- **frontend/src/api**: API client abstraction, typed responses, centralized error handling
- **frontend/src/store**: Global state (Zustand) for cross-component concerns (comparison basket, filters)

## 9. Architecture Decision Records (ADRs)

### ADR-001: Use Hexagonal Architecture for Backend

**Context:** Need maintainable architecture for 83-story project with potential future complexity (scraping, microservices).

**Decision:** Implement hexagonal architecture (ports & adapters) with clear domain/application/infrastructure separation.

**Consequences:**
- ✅ Domain logic testable without database/HTTP mocks
- ✅ Easy to swap PostgreSQL for MongoDB or add GraphQL API
- ✅ Clear team boundaries (domain experts vs. infrastructure)
- ❌ More boilerplate than CRUD scaffolding (mitigated by code generation)
- ❌ Learning curve for junior developers (mitigated by clear examples)

### ADR-002: Use SPA (React) Instead of SSR (Next.js)

**Context:** Need to choose frontend architecture for data-intensive comparison platform.

**Decision:** Build React SPA with separate .NET API, not Next.js SSR monolith.

**Rationale:**
- No SEO requirements (not content marketing site)
- Client-side filtering/sorting faster than server round-trips for 100 models
- Clear API boundary enables future mobile apps
- Team expertise in React + .NET

**Consequences:**
- ✅ Faster time-to-interactive for filtering (no server latency)
- ✅ API reusable for mobile/desktop clients
- ❌ Initial bundle size larger (mitigated by code splitting)
- ❌ No SSR for first paint (acceptable for data tool)

### ADR-003: Use Monorepo for Frontend + Backend

**Context:** Need to decide repository structure for coordinated frontend/backend development.

**Decision:** Single monorepo with `/frontend` and `/backend` folders.

**Rationale:**
- Atomic commits across frontend/backend (e.g., API contract change + React hook update)
- Shared CI/CD pipeline
- Easier for 1-2 developer team
- Coordinated versioning

**Consequences:**
- ✅ Single git clone for entire project
- ✅ No version drift between frontend/backend
- ❌ Larger repo (mitigated by .gitignore, sparse checkout)
- ❌ Requires mono-repo tooling if scaling (Nx, Turborepo)

**Future reconsideration:** If team grows >5 or mobile app added, evaluate polyrepo.

### ADR-004: Use TanStack Query Over Redux for Server State

**Context:** Need state management for API data (models, benchmarks).

**Decision:** Use TanStack Query for server state, Zustand for client state.

**Rationale:**
- Server state (API responses) has different concerns than client state (UI toggles)
- TanStack Query handles caching, background refetch, optimistic updates out-of-box
- Redux overkill for this use case (no complex app-wide state machines)

**Consequences:**
- ✅ Less boilerplate (no actions/reducers for API calls)
- ✅ Automatic cache invalidation and refetching
- ✅ Optimistic updates for admin forms
- ❌ Two state management libraries (mitigated by clear separation: TQ for server, Zustand for client)

### ADR-005: Use Redis for Multi-Layer Caching

**Context:** NFR002 requires scaling to 10K+ users, <2s page load.

**Decision:** Implement multi-layer caching: client (TanStack Query) → Redis (API) → PostgreSQL.

**Rationale:**
- Expensive operations: QAPS calculation (iterate all benchmarks), cost calculations
- High read:write ratio (1000:1 reads vs. admin updates)
- Redis TTL + pub/sub enables cache invalidation on admin updates

**Consequences:**
- ✅ 80%+ cache hit rate reduces DB load 5x
- ✅ <2s page load even with 100+ models
- ❌ Cache invalidation complexity (mitigated by domain events)
- ❌ Redis cost (mitigated by Upstash free tier)

### ADR-006: Use PostgreSQL + TimescaleDB for Time-Series Pricing

**Context:** Phase 2 requires price history tracking (time-series data).

**Decision:** Use TimescaleDB extension on PostgreSQL rather than separate time-series DB.

**Rationale:**
- Single database simplifies operations (no multi-DB transactions)
- TimescaleDB hypertables optimize time-series queries (continuous aggregates, compression)
- PostgreSQL already chosen for transactional data

**Consequences:**
- ✅ Unified database for OLTP + time-series
- ✅ Continuous aggregates for "avg price over 30 days"
- ❌ Less specialized than InfluxDB/Prometheus (acceptable for price data, not IoT metrics)

### ADR-007: Use MassTransit for Future Message-Driven Architecture

**Context:** Phase 2 automated scraping requires job queues, saga coordination.

**Decision:** Integrate MassTransit with Redis transport (MVP), prepare for RabbitMQ/AWS SQS.

**Rationale:**
- Scraping workflow: Schedule job → fetch URLs → validate data → store → cache invalidation
- Saga pattern for multi-step workflows (fetch → validate → store with rollback)
- Abstraction enables broker swaps (Redis → RabbitMQ)

**Consequences:**
- ✅ Future-proof for event-driven architecture
- ✅ Outbox pattern for eventual consistency
- ❌ Complexity for MVP (mitigated by deferring to Epic 2 or Phase 2)

### ADR-008: Use JWT for Admin Authentication (MVP)

**Context:** Admin panel needs authentication, but no user accounts for public site.

**Decision:** Simple JWT tokens for admin auth, no OAuth/OIDC for MVP.

**Rationale:**
- Admin auth is low complexity (1-3 admin users)
- No need for social login, MFA, or user management
- HttpOnly cookies + JWT sufficient for MVP

**Consequences:**
- ✅ Minimal implementation (1 story)
- ✅ No third-party dependency (Auth0, Okta)
- ❌ Manual user management (acceptable for 1-3 admins)
- ❌ No MFA (defer to post-MVP if needed)

**Future:** If user accounts added (Phase 2+), evaluate Auth0/Firebase Auth.

## 10. Testing Strategy

### 10.1 Testing Pyramid

```
        ┌─────────────┐
        │  E2E Tests  │  5% - Critical user flows (Playwright)
        └─────────────┘
      ┌─────────────────┐
      │Integration Tests│  25% - API + DB integration (xUnit + TestContainers)
      └─────────────────┘
    ┌───────────────────────┐
    │     Unit Tests        │  70% - Domain logic, services (xUnit, Jest)
    └───────────────────────┘
```

### 10.2 Backend Testing

**Unit Tests (70%):**
- **Domain services**: QAPS calculation, benchmark normalization, cost estimation
- **Application services**: Query orchestration, filtering logic
- **Validators**: Input validation rules

**Tools:** xUnit, Moq, FluentAssertions

**Example:**
```csharp
[Fact]
public async Task CalculateQAPS_ValidModel_ReturnsCorrectScore()
{
    // Arrange
    var calculator = new QAPSCalculator(_mockBenchmarkRepo.Object);
    var modelId = Guid.NewGuid();
    _mockBenchmarkRepo
        .Setup(r => r.GetModelScoresWithWeightsAsync(modelId))
        .ReturnsAsync(GetSampleScores());

    // Act
    var qaps = await calculator.CalculateQAPSAsync(modelId);

    // Assert
    qaps.Should().BeApproximately(245.67m, 0.01m);
}
```

**Integration Tests (25%):**
- **Repositories**: EF Core queries, transactions
- **API controllers**: HTTP request/response, validation
- **Caching**: Redis integration

**Tools:** xUnit, TestContainers (PostgreSQL/Redis), WebApplicationFactory

**Example:**
```csharp
[Fact]
public async Task GetModels_CacheHit_ReturnsCachedData()
{
    // Arrange
    var factory = new CustomWebApplicationFactory<Program>();
    var client = factory.CreateClient();

    // Seed cache
    await SeedRedisCache("cache:models:list:v1", GetSampleModels());

    // Act
    var response = await client.GetAsync("/api/models");
    var content = await response.Content.ReadFromJsonAsync<ModelListResponse>();

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.OK);
    content.Meta.Cached.Should().BeTrue();
}
```

**E2E Tests (5%):**
- **Critical flows**: Comparison table → filter → select models → comparison view
- **Admin flows**: Login → create model → update pricing → verify public API

**Tools:** Playwright

**Example:**
```csharp
[Test]
public async Task UserCanCompareModels()
{
    await Page.GotoAsync("http://localhost:3000");

    // Select 3 models
    await Page.ClickAsync("[data-testid='model-checkbox-1']");
    await Page.ClickAsync("[data-testid='model-checkbox-2']");
    await Page.ClickAsync("[data-testid='model-checkbox-3']");

    // Navigate to comparison
    await Page.ClickAsync("[data-testid='compare-button']");

    // Verify comparison page
    await Expect(Page.Locator(".comparison-table")).ToBeVisibleAsync();
    await Expect(Page.Locator(".model-card")).ToHaveCountAsync(3);
}
```

### 10.3 Frontend Testing

**Unit Tests (70%):**
- **Utility functions**: Formatters, validators, chart helpers
- **Custom hooks**: `useCostCalculation`, `useModelFilter`
- **Store logic**: Zustand state updates

**Tools:** Vitest, Testing Library

**Integration Tests (25%):**
- **API integration**: MSW mocked API responses
- **Component integration**: Filter sidebar → table update

**Tools:** Vitest, MSW, Testing Library

**E2E Tests (5%):**
- Same as backend E2E (Playwright tests both frontend + API)

### 10.4 Coverage Goals

- **Overall coverage**: 70%+
- **Domain layer**: 90%+ (critical business logic)
- **Application layer**: 80%+
- **Infrastructure layer**: 60%+ (repository patterns, external adapters)
- **UI components**: 50%+ (focus on business logic, not styling)

## 11. DevOps and Deployment

### 11.1 CI/CD Pipeline (GitHub Actions)

**Backend Pipeline (`backend-ci.yml`):**
```yaml
name: Backend CI
on: [push, pull_request]
jobs:
  build-and-test:
    runs-on: ubuntu-latest
    services:
      postgres:
        image: timescale/timescaledb:2.13.0-pg16
        env:
          POSTGRES_PASSWORD: test
      redis:
        image: redis:7-alpine
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.0.x'
      - run: dotnet restore
      - run: dotnet build --no-restore
      - run: dotnet test --no-build --verbosity normal
      - run: dotnet publish -c Release -o ./publish
```

**Frontend Pipeline (`frontend-ci.yml`):**
```yaml
name: Frontend CI
on: [push, pull_request]
jobs:
  build-and-test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-node@v4
        with:
          node-version: '20'
      - run: npm ci
      - run: npm run lint
      - run: npm run type-check  # TypeScript
      - run: npm run test        # Vitest
      - run: npm run build       # Vite production build
```

**E2E Pipeline (`e2e-tests.yml`):**
```yaml
name: E2E Tests
on: [pull_request]
jobs:
  e2e:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
      - uses: actions/setup-node@v4
      - run: docker-compose up -d      # Start PG + Redis
      - run: dotnet run --project Backend.API &  # Start API
      - run: npm run dev &              # Start Vite dev server
      - run: npx playwright test        # Run E2E tests
      - uses: actions/upload-artifact@v4
        if: failure()
        with:
          name: playwright-traces
          path: test-results/
```

### 11.2 Deployment Strategy (TBD - Placeholder)

**MVP Deployment Options:**

**Option 1: Single VPS (DigitalOcean/Linode)**
- Docker Compose on single VPS ($20-50/mo)
- Nginx reverse proxy (frontend static + API)
- PostgreSQL + Redis in Docker containers
- Pros: Simple, low cost
- Cons: No auto-scaling, manual backups

**Option 2: Vercel + Railway**
- Frontend: Vercel (free/hobby tier)
- Backend: Railway ($5-20/mo)
- Database: Railway PostgreSQL + Redis
- Pros: Auto-deploy on git push, zero downtime
- Cons: Higher cost at scale

**Option 3: AWS/Azure (Future)**
- Frontend: S3 + CloudFront
- Backend: ECS/App Service
- Database: RDS PostgreSQL + ElastiCache Redis
- Pros: Enterprise-grade, auto-scaling
- Cons: Complex, expensive

**Recommendation:** Start with Option 2 (Vercel + Railway) for frictionless deployment, migrate to Option 3 post-MVP if >5K MAU.

### 11.3 Monitoring and Observability (Post-MVP)

**Logging:**
- Serilog → Console (dev), File (staging), Seq/Datadog (prod)
- Structured logging: `Log.Information("Model created {@Model}", model)`

**Metrics:**
- API response times (P50, P95, P99)
- Cache hit rates (Redis)
- Database query latency
- QAPS calculation duration

**Error Tracking:**
- Sentry (frontend + backend)
- Automatic error grouping
- User context (model being viewed, filters applied)

**Analytics:**
- PostHog or Plausible (privacy-focused)
- Track: Page views, filter usage, comparison counts, calculator usage
- No PII tracking

## 12. Security Considerations

### 12.1 Authentication and Authorization

**Public API:**
- No authentication (read-only)
- Rate limiting: 100 req/min per IP (Nginx/CloudFlare)

**Admin API:**
- JWT tokens (HttpOnly cookies)
- Token expiration: 24 hours
- Refresh token: 7 days (future)
- HTTPS only (enforce in prod)

**Authorization:**
- Single admin role (MVP)
- Future: Role-based access (admin, editor, viewer)

### 12.2 Input Validation

**Defense in depth:**
1. Client-side: Zod schemas (UX feedback)
2. Server-side: FluentValidation (security boundary)
3. Database: Check constraints, NOT NULL (last line of defense)

**Example:**
```csharp
public class CreateModelValidator : AbstractValidator<CreateModelDto>
{
    public CreateModelValidator()
    {
        RuleFor(m => m.Name).NotEmpty().MaximumLength(255);
        RuleFor(m => m.InputPricePer1M).GreaterThan(0);
        RuleFor(m => m.OutputPricePer1M).GreaterThan(0);
        RuleFor(m => m.Provider).NotEmpty().MaximumLength(100);
    }
}
```

### 12.3 SQL Injection Prevention

- **Parameterized queries**: EF Core uses parameters by default
- **Never raw SQL**: Avoid `FromSqlRaw` unless sanitized
- **Input validation**: Reject malicious input before DB query

### 12.4 XSS Prevention

- **React escaping**: React escapes by default (use `dangerouslySetInnerHTML` only for trusted content)
- **CSP headers**: Content-Security-Policy in production
- **Sanitize user input**: DOMPurify for rich text (if added)

### 12.5 CSRF Prevention

- **SameSite cookies**: `SameSite=Strict` for admin auth
- **CSRF tokens**: Anti-forgery tokens for admin mutations (ASP.NET Core built-in)

### 12.6 Secrets Management

**Development:**
- `appsettings.Development.json` (git-ignored)
- `docker-compose.yml` environment variables

**Production:**
- Environment variables (Railway, Vercel)
- Azure Key Vault / AWS Secrets Manager (future)
- Never commit secrets to git (use .gitignore, git-secrets)

## 13. Implementation Guidance

### 13.1 Development Workflow

**Local development setup:**
```bash
# 1. Clone repo
git clone https://github.com/pablitxn/llm-token-price.git
cd llm-token-price

# 2. Start dependencies (PostgreSQL + Redis)
docker-compose up -d

# 3. Backend setup
cd backend
dotnet restore
dotnet ef database update          # Apply migrations
dotnet run --project Backend.API   # Start API (http://localhost:5000)

# 4. Frontend setup (separate terminal)
cd frontend
npm install
npm run dev                         # Start Vite (http://localhost:5173)
```

**Branch strategy:**
- `main`: Production-ready code
- `develop`: Integration branch (optional for multi-dev team)
- Feature branches: `feature/epic-3-model-table`, `fix/qaps-calculation`

**Commit conventions:**
```
feat(epic-3): Add model table with TanStack Table
fix(qaps): Correct normalization formula for MATH benchmark
docs(architecture): Add ADR for hexagonal architecture
test(calculator): Add unit tests for cost estimation
```

### 13.2 Naming Conventions

**C# Backend:**
- PascalCase: Classes, methods, properties (`ModelRepository`, `CalculateQAPSAsync`)
- camelCase: Local variables, parameters (`var modelId`, `decimal avgPrice`)
- Interfaces: `I` prefix (`IModelRepository`)
- Async methods: `Async` suffix (`GetModelsAsync`)

**TypeScript Frontend:**
- PascalCase: Components, types (`ModelTable`, `ModelDto`)
- camelCase: Functions, variables (`formatPrice`, `selectedModels`)
- UPPER_SNAKE_CASE: Constants (`API_BASE_URL`, `CACHE_TTL`)
- Hooks: `use` prefix (`useModels`, `useCostCalculation`)

**Database:**
- snake_case: Tables, columns (`model_benchmark_scores`, `input_price_per_1m`)
- Plural table names (`models`, `benchmarks`)
- Foreign keys: `{table}_id` (`model_id`, `benchmark_id`)

**Files:**
- PascalCase: Components (`ModelTable.tsx`, `FilterSidebar.tsx`)
- camelCase: Utilities (`formatters.ts`, `validators.ts`)
- kebab-case: URLs (`/compare`, `/cost-calculator`)

### 13.3 Code Organization Principles

**Backend:**
- **Domain layer**: Pure business logic, no framework dependencies
- **Application layer**: Orchestration, DTOs, no HTTP/DB concerns
- **Infrastructure layer**: Framework-specific code (EF Core, ASP.NET)
- **No circular dependencies**: Domain ← Application ← Infrastructure → Domain (✅)

**Frontend:**
- **Components**: Organized by domain, not by type (`/components/models`, not `/components/buttons`)
- **Hooks**: One hook per API resource (`useModels`, `useModelDetail`)
- **Store**: Minimal global state (comparison basket, filters), prefer local state
- **API layer**: Centralized client (`api/client.ts`), typed responses

### 13.4 Best Practices

**Backend:**
- Async all the way: Use `async/await` for I/O operations (DB, Redis, HTTP)
- Dependency injection: Constructor injection, interfaces for testability
- Error handling: Use `Result<T>` or exceptions for failures (document in ADR)
- Logging: Structured logging (`Log.Information("{Action} {@Model}", "Create", model)`)

**Frontend:**
- Immutability: Use spread operators, avoid mutations (`[...models, newModel]`)
- Accessibility: Semantic HTML, ARIA labels, keyboard navigation
- Performance: Code splitting (`React.lazy`), memoization (`useMemo`, `React.memo`)
- Error boundaries: Catch errors in component tree, show fallback UI

**Database:**
- Indexes: Add indexes for foreign keys, frequently filtered columns
- Transactions: Use for multi-table updates (EF Core `SaveChangesAsync` auto-transaction)
- Migrations: One migration per feature, review SQL before apply

**Testing:**
- AAA pattern: Arrange, Act, Assert
- Test behavior, not implementation: Mock external dependencies (DB, HTTP), not internal methods
- Fast tests: In-memory repositories for domain tests, TestContainers for integration tests

## 14. Specialist Sections Handoff

### 14.1 DevOps Specialist (Post-MVP)

**Scope:** Enterprise-grade CI/CD, monitoring, scaling

**Deliverables:**
- Production deployment architecture (AWS/Azure)
- Infrastructure as Code (Terraform/Pulumi)
- Monitoring dashboards (Datadog, Grafana)
- Auto-scaling policies (ECS, Kubernetes)
- Disaster recovery plan

**Handoff:** Engage after MVP launch, when scaling beyond 5K MAU or multi-region deployment needed.

### 14.2 Security Specialist (Post-MVP)

**Scope:** Penetration testing, compliance, advanced auth

**Deliverables:**
- Security audit (OWASP Top 10)
- OAuth/OIDC implementation (if user accounts added)
- MFA for admin panel
- GDPR compliance (if EU users)
- Secrets rotation strategy

**Handoff:** Engage before public launch or if enterprise customers require compliance (SOC2, ISO 27001).

### 14.3 Testing Specialist (Optional)

**Scope:** Comprehensive test coverage, performance testing

**Deliverables:**
- Increase test coverage to 80%+ (unit + integration)
- Load testing (JMeter, K6) - 10K concurrent users
- Visual regression testing (Percy, Chromatic)
- Accessibility auditing (axe, Pa11y)
- Test automation for all 83 stories

**Handoff:** Engage if test coverage drops below 60% or performance issues arise (P95 latency >2s).

---

## 15. Next Steps

**Immediate (Phase 3 - Solutioning):**
1. ✅ Solution architecture review (this document)
2. ⏳ Run cohesion check (validate architecture against PRD requirements)
3. ⏳ Generate tech specs per epic (8 detailed implementation guides)
4. ⏳ Populate story backlog (83 stories in workflow status)

**Near-term (Phase 4 - Implementation):**
1. Load SM agent → Draft story 1.1 (Initialize project repository)
2. Implement Epic 1: Foundation (8-10 stories)
3. Validate with E2E test: Health check → Seed data → API call succeeds
4. Continue epic-by-epic implementation (Epic 2 → 3 → ... → 8)

**Post-MVP (Phase 5+):**
1. Launch MVP, gather user feedback
2. Implement Phase 2 features (automated scraping, advanced filters)
3. Engage specialist agents (DevOps, Security) for scaling
4. Evaluate microservices extraction (scraping service, admin API)

---

_Generated using BMad Method Solution Architecture workflow_
_Expert mode: Concise, decision-focused architecture for Level 4 enterprise web application_
