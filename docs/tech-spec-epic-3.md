# Technical Specification: Epic 3 - Public Comparison Table Interface

Date: 2025-10-21
Author: Pablo
Epic ID: 3
Status: Draft

---

## Overview

Epic 3 delivers the **primary user-facing interface** of the LLM Cost Comparison Platform: a sophisticated, interactive comparison table that enables users to browse, filter, sort, and select models for deeper analysis. This epic transforms the foundational data infrastructure (Epic 1) and admin-managed content (Epic 2) into a high-performance, information-dense public interface optimized for developer users making data-driven model selection decisions.

**Core Value Proposition:** Users can view 50+ LLM models simultaneously, instantly filter by capabilities (function calling, vision support) and price ranges, sort by any metric (cost, benchmark scores), and select up to 5 models for detailed comparison—all within a sub-2-second initial load time and instant client-side interactions.

**Strategic Positioning:** This epic represents the MVP's **primary value delivery**—the "comparison table" is the platform's headline feature mentioned in the PRD's goals ("eliminate manual research overhead reducing model evaluation time from 3-5 hours to under 15 minutes"). Success here validates the core product hypothesis before investing in advanced features (smart filters, cost calculators, visualizations).

**Technical Complexity:** Epic 3 introduces significant frontend complexity (TanStack Table integration, Zustand state management, TanStack Query caching) and backend optimization requirements (Redis caching, query performance tuning). The multi-layer caching strategy and client-side filtering architecture established here become foundational patterns for subsequent epics.

## Objectives and Scope

### Primary Objectives

1. **Deliver High-Performance Model Browsing** - Implement TanStack Table-powered comparison interface loading in <2s with instant client-side filtering/sorting for 50+ models
2. **Enable Multi-Dimensional Filtering** - Provide provider, capability, and price range filters allowing users to narrow 50+ models to 5-10 relevant options in seconds
3. **Support Model Selection Workflow** - Enable checkbox-based selection of 2-5 models with persistent comparison basket, setting up Epic 5's side-by-side comparison
4. **Optimize Backend Performance** - Implement Redis caching (1hr TTL) for `/api/models` endpoint achieving 80%+ cache hit ratio and <200ms response on cache miss
5. **Establish Frontend Architecture Patterns** - Create reusable patterns for TanStack Query (server state), Zustand (client state), and component composition used across remaining epics

### In-Scope for Epic 3

**Frontend Components:**
- Public homepage layout with header, footer, navigation
- TanStack Table integration with sorting, filtering, column visibility
- Filter sidebar with provider checkboxes, capability toggles, price range slider
- Search functionality (debounced, filters by name/provider)
- Comparison basket UI (floating bar or top section)
- Model selection checkboxes with 5-model limit
- Benchmark score columns (top 3-5 benchmarks in table)
- Responsive table (desktop table view, mobile card view)
- Loading states, empty states, error boundaries

**Backend Services:**
- Redis caching layer for `/api/models` endpoint
- Query optimization (EF Core includes for capabilities and benchmark scores)
- Cache invalidation on admin model updates (domain event → cache bust)
- API response compression (gzip)

**State Management:**
- TanStack Query for server state (5min stale time)
- Zustand store for filter state, comparison basket, view preferences
- URL state synchronization for shareable filtered views

**Performance:**
- Initial page load <2s (measured from route navigation to table render)
- Client-side filter application <100ms
- Search debouncing (300ms) to avoid excessive re-renders
- Virtual scrolling OR pagination (decision based on UX preference)

### Out-of-Scope for Epic 3

**Deferred to Epic 4:**
- Model detail modal (click model name → full specs)
- Cost calculator (embedded in modal or standalone page)

**Deferred to Epic 5:**
- Side-by-side comparison page (/compare route)
- Benchmark visualization charts (Chart.js bar charts)
- Capability matrix comparison table

**Deferred to Epic 6:**
- "Best Value" smart filter (QAPS algorithm)
- Smart filter explanations and ranking

**Deferred to Epic 7:**
- Data freshness indicators ("Updated 3 days ago")
- Validation warnings for outlier data
- Admin dashboard metrics

**Deferred to Epic 8:**
- Advanced mobile optimizations (touch gestures, swipe actions)
- Progressive Web App (PWA) features
- Offline capability

**Explicitly NOT in Epic 3:**
- User accounts or authentication (public read-only interface)
- Saved filters or bookmarked models (requires accounts)
- Export to CSV/PDF (future enhancement)
- Historical price tracking visualization (Phase 2)
- Multi-language support (future enhancement)

### Success Criteria

1. **Performance:** 90% of page loads complete in <2s (Google Analytics measurement)
2. **Usability:** 70%+ first-time users successfully filter models and select 2+ for comparison without instruction (usability testing)
3. **Cache Efficiency:** 80%+ cache hit ratio on `/api/models` endpoint (Redis metrics)
4. **Error Rate:** <1% API error rate under normal load (monitoring)
5. **Adoption:** 60%+ of users who land on homepage engage with filters within first session (analytics)

## System Architecture Alignment

Epic 3 implementation aligns with the hexagonal architecture established in the Solution Architecture Document (`docs/solution-architecture.md`) and builds upon the foundation created in Epic 1 and Epic 2.

### Hexagonal Architecture Layers

**Domain Layer (Unchanged):**
- No new domain entities or services required
- Reuses existing `Model`, `Capability`, `Benchmark`, `BenchmarkScore` entities from Epic 1
- Domain logic remains isolated from presentation concerns

**Application Layer:**
- **New Service:** `ModelQueryService` - Orchestrates fetching models with related data (capabilities, top benchmarks)
- **New Service:** `FilterService` - Applies filtering logic (provider, capabilities, price range) - NOTE: Initially client-side, this service provides server-side filtering capability for future optimization
- **Existing DTOs Enhanced:** `ModelDto` extended to include top 3-5 benchmarks scores in list response

**Infrastructure Layer:**
- **New Adapter:** `RedisCacheService` implementation for `ICacheRepository` port (from Epic 1.5)
- **Repository Enhancement:** `ModelRepository.GetAllWithDetailsAsync()` method with optimized EF Core includes
- **Cache Invalidation:** Domain event handler `ModelUpdatedEvent` → cache bust pattern

**Presentation Layer (React SPA):**
- **New Page:** `HomePage` component at `/` route
- **New Components:** `ModelTable` (TanStack Table), `FilterSidebar`, `ComparisonBasket`
- **State Management:** Zustand store for filter state, TanStack Query for API data

### Multi-Layer Caching Strategy

Epic 3 implements the caching architecture defined in Solution Architecture Section 4:

```
Client (TanStack Query, 5min stale)
  ↓ (if stale/miss)
Redis API Cache (1hr TTL, key: cache:models:list:v1)
  ↓ (if miss)
PostgreSQL (EF Core with includes)
```

**Cache Key Naming Convention (from Architecture Doc):**
- `cache:models:list:v1` - All models with capabilities and top benchmarks
- `cache:model:{id}:v1` - Single model detail (Epic 4)
- `cache:bestvalue:v1` - Smart filter results (Epic 6)

**Cache Invalidation Pattern:**
1. Admin updates model (Epic 2 → Story 2.7) → `ModelUpdatedEvent` published
2. `ModelUpdateHandler` (new in Epic 3) → `_cache.RemovePatternAsync("cache:models:*")`
3. Next user request → cache miss → fresh data fetched → cache repopulated

### API Response Optimization

Epic 3 enhances the `/api/models` endpoint created in Epic 1 (Story 1.10):

**Epic 1 Baseline:**
```csharp
// Story 1.10 - Basic implementation
GET /api/models
// Returns: List<Model> with basic fields
```

**Epic 3 Enhancement:**
```csharp
// Story 3.15 - Optimized implementation
GET /api/models
// Returns: List<ModelDto> with:
//  - Model metadata (name, provider, pricing)
//  - Capabilities (context window, flags)
//  - Top 3-5 benchmark scores (MMLU, HumanEval, GSM8K)
//  - Last updated timestamp
// Performance: <200ms (cache miss), <10ms (cache hit)
// Caching: Redis 1hr TTL, gzip compression
```

### Frontend Architecture Patterns

Epic 3 establishes the **canonical frontend patterns** reused across all subsequent epics:

**Server State Management (TanStack Query):**
```typescript
// Pattern established in Story 3.2
export const useModels = () => {
  return useQuery({
    queryKey: ['models'],
    queryFn: fetchModels,
    staleTime: 5 * 60 * 1000,  // 5 min (from Architecture)
    cacheTime: 30 * 60 * 1000, // 30 min
  });
};
```

**Client State Management (Zustand):**
```typescript
// Pattern established in Story 3.10-3.11
export const useAppStore = create<AppState>((set) => ({
  selectedModels: [],
  filterState: defaultFilters,
  addModel: (model) => set((state) => ({
    selectedModels: [...state.selectedModels, model]
  })),
  // ... other actions
}));
```

**Component Composition:**
```typescript
// Pattern established in Story 3.1-3.3
HomePage
├── Header (navigation, search)
├── FilterSidebar (Zustand → filterState)
├── ModelTable (TanStack Table + TanStack Query)
│   └── ModelRow[] (checkbox selection → Zustand)
└── ComparisonBasket (Zustand → selectedModels)
```

### Dependency on Epic 1 & Epic 2

**Epic 1 Prerequisites:**
- Database schema (models, capabilities, benchmarks, model_benchmark_scores) - Story 1.4
- PostgreSQL connection - Story 1.3
- Redis connection - Story 1.5
- Basic `/api/models` endpoint - Story 1.10
- Frontend application shell - Story 1.7
- TailwindCSS, Vite configuration - Story 1.2

**Epic 2 Prerequisites:**
- Admin panel with model CRUD - Stories 2.4-2.8
- Benchmark score management - Story 2.10
- Timestamp tracking - Story 2.12
- **Database contains 10-15 models with capabilities and benchmark scores** (seeded via admin or SQL scripts)

**Key Assumption:** Epic 3 assumes the database is populated with meaningful model data. If testing with empty database, Story 3.2 will display empty table (acceptable for development, but usability testing requires real data).

## Detailed Design

### Services and Modules

This section details all backend services, frontend components, and their responsibilities within Epic 3.

#### Backend Services

| Service/Module | Layer | Responsibility | Input | Output | Owner |
|---|---|---|---|---|---|
| `ModelQueryService` | Application | Orchestrates fetching all models with related data for public API | None (or filter params in future) | `List<ModelDto>` | Backend |
| `RedisCacheService` | Infrastructure | Implements `ICacheRepository` for Redis operations (Get, Set, Delete, RemovePattern) | Key, Value, TTL | Cached value or null | Backend |
| `ModelRepository.GetAllWithDetailsAsync()` | Infrastructure | Fetches models with EF Core includes for capabilities and top benchmark scores | None | `List<Model>` with includes | Backend |
| `ModelUpdatedEvent` Handler | Application | Listens to domain events and invalidates Redis cache on model updates | `ModelUpdatedEvent` | void (side effect: cache bust) | Backend |
| `CacheMiddleware` (optional) | API | HTTP middleware to check cache before hitting controllers (future optimization) | HTTP Request | Cached response or next() | Backend |

**Service Implementation Details:**

**1. ModelQueryService (`LlmTokenPrice.Application/Services/ModelQueryService.cs`)**

```csharp
public class ModelQueryService
{
    private readonly IModelRepository _modelRepo;
    private readonly ICacheRepository _cache;
    private readonly IMapper _mapper;
    private const string CacheKey = "cache:models:list:v1";
    private static readonly TimeSpan CacheTTL = TimeSpan.FromHours(1);

    public async Task<IEnumerable<ModelDto>> GetAllModelsAsync()
    {
        // 1. Check cache
        var cached = await _cache.GetAsync<IEnumerable<ModelDto>>(CacheKey);
        if (cached != null) return cached;

        // 2. Fetch from database with optimized includes
        var models = await _modelRepo.GetAllWithDetailsAsync();

        // 3. Map to DTOs with top benchmarks
        var modelDtos = _mapper.Map<IEnumerable<ModelDto>>(models);

        // 4. Cache and return
        await _cache.SetAsync(CacheKey, modelDtos, CacheTTL);
        return modelDtos;
    }
}
```

**Key Design Decisions:**
- **Cache-aside pattern:** Check cache → miss → fetch DB → populate cache
- **1-hour TTL:** Balances freshness vs. performance (configurable via `appsettings.json`)
- **Cache entire response:** Serialized `List<ModelDto>` (est. 50 models × 2KB = 100KB total)

**2. Redis Cache Service (`LlmTokenPrice.Infrastructure/Caching/RedisCacheService.cs`)**

```csharp
public class RedisCacheService : ICacheRepository
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _db;

    public async Task<T?> GetAsync<T>(string key)
    {
        var value = await _db.StringGetAsync(key);
        return value.HasValue
            ? JsonSerializer.Deserialize<T>(value)
            : default;
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan ttl)
    {
        var serialized = JsonSerializer.Serialize(value);
        await _db.StringSetAsync(key, serialized, ttl);
    }

    public async Task RemovePatternAsync(string pattern)
    {
        // Use Lua script for pattern-based deletion (e.g., "cache:models:*")
        var keys = _redis.GetServer(_redis.GetEndPoints()[0])
            .Keys(pattern: pattern);
        foreach (var key in keys)
            await _db.KeyDeleteAsync(key);
    }
}
```

**3. Model Repository Enhancement (`LlmTokenPrice.Infrastructure/Repositories/ModelRepository.cs`)**

```csharp
public async Task<List<Model>> GetAllWithDetailsAsync()
{
    return await _context.Models
        .Include(m => m.Capabilities)
        .Include(m => m.BenchmarkScores.OrderByDescending(bs => bs.Benchmark.Weight).Take(5))
            .ThenInclude(bs => bs.Benchmark)
        .Where(m => m.IsActive)
        .AsNoTracking() // Read-only, no change tracking overhead
        .ToListAsync();
}
```

**Performance Optimization:**
- `AsNoTracking()`: 30-40% faster for read-only queries
- `Take(5)` on benchmarks: Fetch only top 5 weighted benchmarks (reduces payload)
- Eager loading (`Include`): Avoids N+1 query problem (1 query vs. 50+ queries)

---

#### Frontend Components

| Component | Responsibility | State Management | Key Props/Hooks |
|---|---|---|---|
| `HomePage` | Main page container, layout coordination | None (stateless container) | None |
| `Header` | Navigation bar, logo, search input | Local state (search query) | `onSearch: (query) => void` |
| `FilterSidebar` | Provider, capability, price range filters | Zustand `filterStore` | `filters: FilterState, updateFilters: (filters) => void` |
| `ModelTable` | TanStack Table with sorting, selection | TanStack Query (`useModels`), Zustand (`selectedModels`) | `models: ModelDto[], onSelectModel: (model) => void` |
| `ModelRow` | Individual table row with checkbox | None (receives props from parent) | `model: ModelDto, isSelected: boolean, onSelect: () => void` |
| `ComparisonBasket` | Floating basket showing selected models | Zustand (`selectedModels`) | `selectedModels: ModelDto[], onRemove: (id) => void` |
| `SearchBar` | Debounced search input (embedded in Header) | Local state + Zustand (`searchQuery`) | `onSearch: (query) => void` |
| `LoadingSpinner` | Loading state UI | None (presentational) | `message?: string` |
| `ErrorBoundary` | Catches React errors, shows fallback UI | Local state (error info) | `fallback: ReactNode` |

**Component File Structure:**

```
apps/web/src/
├── pages/
│   └── HomePage.tsx (Story 3.1)
├── components/
│   ├── layout/
│   │   └── Header.tsx (Story 3.1, 3.8)
│   ├── models/
│   │   ├── ModelTable.tsx (Story 3.3, 3.4)
│   │   └── ModelRow.tsx (Story 3.10)
│   ├── filters/
│   │   ├── FilterSidebar.tsx (Story 3.5)
│   │   ├── ProviderFilter.tsx (Story 3.5)
│   │   ├── CapabilityFilter.tsx (Story 3.6)
│   │   └── PriceRangeFilter.tsx (Story 3.7)
│   ├── comparison/
│   │   └── ComparisonBasket.tsx (Story 3.11)
│   └── ui/ (reusable primitives)
│       ├── LoadingSpinner.tsx (Story 3.2)
│       └── ErrorBoundary.tsx (Story 3.2)
├── hooks/
│   └── useModels.ts (Story 3.2 - TanStack Query hook)
└── store/
    ├── appStore.ts (Story 3.10 - Zustand store)
    └── filterStore.ts (Story 3.5 - Filter state)
```

**Component Interaction Flow:**

```
User types in SearchBar
  ↓
SearchBar updates Zustand filterStore.searchQuery (debounced 300ms)
  ↓
FilterSidebar & ModelTable read filterStore.searchQuery
  ↓
ModelTable applies client-side filter to TanStack Query data
  ↓
Re-render with filtered rows (< 100ms)
```

---

#### State Management Architecture

**TanStack Query (Server State):**

```typescript
// hooks/useModels.ts (Story 3.2)
export const useModels = () => {
  return useQuery<ModelDto[]>({
    queryKey: ['models'],
    queryFn: () => apiClient.get('/api/models').then(res => res.data),
    staleTime: 5 * 60 * 1000,  // 5 minutes
    cacheTime: 30 * 60 * 1000, // 30 minutes
    refetchOnWindowFocus: false, // Don't refetch on tab switch
  });
};
```

**Zustand (Client State):**

```typescript
// store/appStore.ts (Story 3.10-3.11)
interface AppState {
  selectedModels: ModelDto[];
  addModel: (model: ModelDto) => void;
  removeModel: (id: string) => void;
  clearSelection: () => void;
}

export const useAppStore = create<AppState>((set) => ({
  selectedModels: [],
  addModel: (model) => set((state) => {
    if (state.selectedModels.length >= 5) {
      toast.error('Maximum 5 models can be selected');
      return state;
    }
    return { selectedModels: [...state.selectedModels, model] };
  }),
  removeModel: (id) => set((state) => ({
    selectedModels: state.selectedModels.filter(m => m.id !== id)
  })),
  clearSelection: () => set({ selectedModels: [] }),
}));

// store/filterStore.ts (Story 3.5-3.7)
interface FilterState {
  providers: string[];
  capabilities: Record<string, boolean>;
  priceRange: { min: number; max: number };
  searchQuery: string;
}

export const useFilterStore = create<FilterState>((set) => ({
  providers: [],
  capabilities: {},
  priceRange: { min: 0, max: 100 },
  searchQuery: '',
  // ... update actions
}));
```

---

#### Module Dependencies

**Backend Module Graph:**

```
LlmTokenPrice.API
  → LlmTokenPrice.Application
      → LlmTokenPrice.Domain (no new domain code in Epic 3)
  → LlmTokenPrice.Infrastructure
      → LlmTokenPrice.Application (for interfaces)
```

**Frontend Module Graph:**

```
pages/HomePage
  → components/models/ModelTable
      → hooks/useModels (TanStack Query)
      → store/appStore (Zustand)
  → components/filters/FilterSidebar
      → store/filterStore (Zustand)
  → components/comparison/ComparisonBasket
      → store/appStore (Zustand)
```

**Key Architectural Rule:** Components never directly import API client—only through TanStack Query hooks (`useModels`, `useBenchmarks`, etc.)

### Data Models and Contracts

This section defines the data structures used in Epic 3, including API contracts, DTOs, and client-side TypeScript interfaces.

#### Backend DTOs (C#)

**ModelDto (`LlmTokenPrice.Application/DTOs/ModelDto.cs`)**

```csharp
public class ModelDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public string? Version { get; set; }
    public DateTime? ReleaseDate { get; set; }
    public string Status { get; set; } = "active"; // active|deprecated|beta

    // Pricing
    public decimal InputPricePer1M { get; set; }
    public decimal OutputPricePer1M { get; set; }
    public string Currency { get; set; } = "USD";
    public DateTime? PricingValidFrom { get; set; }
    public DateTime? PricingValidTo { get; set; }
    public DateTime? LastScrapedAt { get; set; }

    // Capabilities (flattened from Capabilities entity)
    public int ContextWindow { get; set; }
    public int? MaxOutputTokens { get; set; }
    public bool SupportsFunctionCalling { get; set; }
    public bool SupportsVision { get; set; }
    public bool SupportsAudioInput { get; set; }
    public bool SupportsAudioOutput { get; set; }
    public bool SupportsStreaming { get; set; }
    public bool SupportsJsonMode { get; set; }

    // Top benchmarks (for table display - top 5 by weight)
    public List<BenchmarkScoreDto> TopBenchmarks { get; set; } = new();

    // Metadata
    public DateTime UpdatedAt { get; set; }
}

public class BenchmarkScoreDto
{
    public string BenchmarkName { get; set; } = string.Empty; // e.g., "MMLU"
    public string FullName { get; set; } = string.Empty;      // e.g., "Massive Multitask Language Understanding"
    public string Category { get; set; } = string.Empty;       // reasoning|code|math|language|multimodal
    public decimal Score { get; set; }                         // e.g., 85.2
    public decimal? MaxScore { get; set; }                     // e.g., 100.0
    public decimal? NormalizedScore { get; set; }              // 0-1 range (for future QAPS calculation)
    public DateTime? TestDate { get; set; }
    public string? SourceUrl { get; set; }
    public bool Verified { get; set; }
}
```

**DTO Design Rationale:**

- **Flattened Capabilities:** Instead of nested `CapabilitiesDto`, we inline capability flags directly in `ModelDto` to reduce JSON depth and simplify client-side access (`model.supportsFunctionCalling` vs. `model.capabilities.supportsFunctionCalling`)
- **Selective Benchmarks:** `TopBenchmarks` array contains only 3-5 highest-weighted benchmarks to minimize payload (full benchmarks available in model detail endpoint - Epic 4)
- **Pricing Metadata:** Includes validity dates and last scraped timestamp for future Epic 7 data freshness indicators
- **No Navigation Properties:** No `Capability` or `BenchmarkScore` entity references—DTOs are pure data transfer objects with no EF Core tracking

**AutoMapper Profile (`LlmTokenPrice.Application/Mapping/MappingProfile.cs`)**

```csharp
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Model, ModelDto>()
            .ForMember(dest => dest.ContextWindow, opt => opt.MapFrom(src => src.Capabilities.ContextWindow))
            .ForMember(dest => dest.MaxOutputTokens, opt => opt.MapFrom(src => src.Capabilities.MaxOutputTokens))
            .ForMember(dest => dest.SupportsFunctionCalling, opt => opt.MapFrom(src => src.Capabilities.SupportsFunctionCalling))
            .ForMember(dest => dest.SupportsVision, opt => opt.MapFrom(src => src.Capabilities.SupportsVision))
            // ... other capability mappings
            .ForMember(dest => dest.TopBenchmarks, opt => opt.MapFrom(src =>
                src.BenchmarkScores
                    .OrderByDescending(bs => bs.Benchmark.WeightInQAPS)
                    .Take(5)
                    .Select(bs => new BenchmarkScoreDto
                    {
                        BenchmarkName = bs.Benchmark.BenchmarkName,
                        FullName = bs.Benchmark.FullName,
                        Category = bs.Benchmark.Category,
                        Score = bs.Score,
                        MaxScore = bs.MaxScore,
                        NormalizedScore = bs.NormalizedScore,
                        TestDate = bs.TestDate,
                        SourceUrl = bs.SourceUrl,
                        Verified = bs.Verified
                    })
            ));

        CreateMap<BenchmarkScore, BenchmarkScoreDto>();
    }
}
```

---

#### Frontend TypeScript Interfaces

**Model Types (`apps/web/src/types/models.ts`)**

```typescript
export interface ModelDto {
  id: string;
  name: string;
  provider: string;
  version?: string;
  releaseDate?: string;
  status: 'active' | 'deprecated' | 'beta';

  // Pricing
  inputPricePer1M: number;
  outputPricePer1M: number;
  currency: string;
  pricingValidFrom?: string;
  pricingValidTo?: string;
  lastScrapedAt?: string;

  // Capabilities
  contextWindow: number;
  maxOutputTokens?: number;
  supportsFunctionCalling: boolean;
  supportsVision: boolean;
  supportsAudioInput: boolean;
  supportsAudioOutput: boolean;
  supportsStreaming: boolean;
  supportsJsonMode: boolean;

  // Benchmarks
  topBenchmarks: BenchmarkScoreDto[];

  // Metadata
  updatedAt: string; // ISO 8601 date string
}

export interface BenchmarkScoreDto {
  benchmarkName: string;
  fullName: string;
  category: 'reasoning' | 'code' | 'math' | 'language' | 'multimodal';
  score: number;
  maxScore?: number;
  normalizedScore?: number;
  testDate?: string;
  sourceUrl?: string;
  verified: boolean;
}
```

**Filter State Types (`apps/web/src/types/filters.ts`)**

```typescript
export interface FilterState {
  providers: string[];        // e.g., ['OpenAI', 'Anthropic']
  capabilities: CapabilityFilters;
  priceRange: PriceRange;
  searchQuery: string;
}

export interface CapabilityFilters {
  functionCalling: boolean;
  vision: boolean;
  audioInput: boolean;
  audioOutput: boolean;
  streaming: boolean;
  jsonMode: boolean;
}

export interface PriceRange {
  min: number;  // Price per 1M tokens (combined avg)
  max: number;
}

// Helper type for filtered/sorted results
export interface TableState {
  sortBy: string | null;      // column ID
  sortDirection: 'asc' | 'desc';
  pageIndex: number;           // For pagination (if used)
  pageSize: number;
}
```

**API Response Types (`apps/web/src/types/api.ts`)**

```typescript
export interface ApiResponse<T> {
  data: T;
  meta?: {
    timestamp: string;
    cached: boolean;
    ttl?: number; // seconds until cache expiry
  };
}

export interface ApiError {
  error: {
    code: string;              // e.g., "VALIDATION_ERROR"
    message: string;
    details?: Record<string, any>;
  };
}
```

---

#### API Contract Specifications

**GET /api/models (Enhanced from Epic 1)**

**Request:**
```http
GET /api/models HTTP/1.1
Host: localhost:5000
Accept: application/json
Accept-Encoding: gzip
```

**Response (200 OK):**
```json
{
  "data": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "name": "GPT-4",
      "provider": "OpenAI",
      "version": "gpt-4-0613",
      "releaseDate": "2023-06-13T00:00:00Z",
      "status": "active",
      "inputPricePer1M": 30.0,
      "outputPricePer1M": 60.0,
      "currency": "USD",
      "pricingValidFrom": "2024-01-01T00:00:00Z",
      "pricingValidTo": null,
      "lastScrapedAt": "2025-10-20T10:30:00Z",
      "contextWindow": 8192,
      "maxOutputTokens": 4096,
      "supportsFunctionCalling": true,
      "supportsVision": false,
      "supportsAudioInput": false,
      "supportsAudioOutput": false,
      "supportsStreaming": true,
      "supportsJsonMode": true,
      "topBenchmarks": [
        {
          "benchmarkName": "MMLU",
          "fullName": "Massive Multitask Language Understanding",
          "category": "reasoning",
          "score": 86.4,
          "maxScore": 100.0,
          "normalizedScore": 0.864,
          "testDate": "2023-06-15T00:00:00Z",
          "sourceUrl": "https://openai.com/research/gpt-4",
          "verified": true
        },
        {
          "benchmarkName": "HumanEval",
          "fullName": "HumanEval Code Completion",
          "category": "code",
          "score": 0.67,
          "maxScore": 1.0,
          "normalizedScore": 0.67,
          "testDate": "2023-06-15T00:00:00Z",
          "sourceUrl": null,
          "verified": true
        },
        {
          "benchmarkName": "GSM8K",
          "fullName": "Grade School Math 8K",
          "category": "math",
          "score": 92.0,
          "maxScore": 100.0,
          "normalizedScore": 0.92,
          "testDate": null,
          "sourceUrl": null,
          "verified": false
        }
      ],
      "updatedAt": "2025-10-20T10:30:00Z"
    }
    // ... additional models (50+ total)
  ],
  "meta": {
    "timestamp": "2025-10-21T14:22:33Z",
    "cached": true,
    "ttl": 3421  // seconds remaining in cache (57 min)
  }
}
```

**Response Headers:**
```http
HTTP/1.1 200 OK
Content-Type: application/json; charset=utf-8
Content-Encoding: gzip
Cache-Control: public, max-age=3600  // 1 hour
ETag: "686897696a7c876b7e"
Content-Length: 15234 (compressed from ~42KB)
```

**Response (304 Not Modified - if ETag matches):**
```http
HTTP/1.1 304 Not Modified
ETag: "686897696a7c876b7e"
Cache-Control: public, max-age=3600
```

**Response (500 Internal Server Error):**
```json
{
  "error": {
    "code": "DATABASE_ERROR",
    "message": "Failed to fetch models from database",
    "details": {
      "innerException": "Connection timeout"
    }
  }
}
```

**Performance Characteristics:**
- **Payload Size:** ~42KB uncompressed (50 models × ~840 bytes avg), ~15KB gzipped (64% compression)
- **Response Time:** <10ms (cache hit), <200ms (cache miss with database query)
- **Cache TTL:** 1 hour (3600 seconds)
- **Update Frequency:** Invalidated immediately on admin model updates

---

#### Data Transformation Pipeline

```
Database (Entity Framework)
  ↓
  Model entities (with Includes for Capabilities, BenchmarkScores)
  ↓
AutoMapper
  ↓
  ModelDto (flattened capabilities, top 5 benchmarks)
  ↓
System.Text.Json Serialization
  ↓
  JSON string (~42KB)
  ↓
gzip Compression
  ↓
  Compressed response (~15KB)
  ↓
HTTP Response
  ↓
Axios Client (Frontend)
  ↓
  Automatic decompression + JSON parsing
  ↓
TypeScript ModelDto[] (strongly typed)
  ↓
TanStack Query Cache (5 min stale time)
  ↓
React Components
```

---

#### Schema Validation

**Backend (FluentValidation):**

```csharp
// No validation needed for GET endpoint (read-only)
// Admin mutation validation already handled in Epic 2
```

**Frontend (Zod - optional runtime validation):**

```typescript
import { z } from 'zod';

export const ModelDtoSchema = z.object({
  id: z.string().uuid(),
  name: z.string().min(1),
  provider: z.string().min(1),
  status: z.enum(['active', 'deprecated', 'beta']),
  inputPricePer1M: z.number().positive(),
  outputPricePer1M: z.number().positive(),
  // ... other fields
});

// Used in development to validate API responses
if (import.meta.env.DEV) {
  const parsed = ModelDtoSchema.array().safeParse(apiResponse.data);
  if (!parsed.success) {
    console.error('Invalid API response:', parsed.error);
  }
}
```

**Runtime Validation Strategy:**
- **Development:** Zod validation enabled to catch API contract mismatches early
- **Production:** Zod validation disabled for performance (trust backend contract)
- **TypeScript:** Compile-time type safety catches most issues

### APIs and Interfaces

This section details all API endpoints, their signatures, and integration patterns used in Epic 3.

#### Backend API Endpoints

**Enhanced `/api/models` Endpoint (from Epic 1 Story 1.10)**

**Controller Implementation (`LlmTokenPrice.API/Controllers/ModelsController.cs`):**

```csharp
[ApiController]
[Route("api/[controller]")]
public class ModelsController : ControllerBase
{
    private readonly ModelQueryService _modelQueryService;
    private readonly ILogger<ModelsController> _logger;

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ModelDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    [ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Any, VaryByHeader = "Accept-Encoding")]
    public async Task<IActionResult> GetAllModels()
    {
        try
        {
            var models = await _modelQueryService.GetAllModelsAsync();

            return Ok(new ApiResponse<IEnumerable<ModelDto>>
            {
                Data = models,
                Meta = new ResponseMeta
                {
                    Timestamp = DateTime.UtcNow,
                    Cached = HttpContext.Items.ContainsKey("FromCache"), // Set by CacheMiddleware
                    Ttl = 3600 // 1 hour
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch models");
            return StatusCode(500, new ApiErrorResponse
            {
                Error = new ErrorDetail
                {
                    Code = "DATABASE_ERROR",
                    Message = "Failed to fetch models from database",
                    Details = new { innerException = ex.Message }
                }
            });
        }
    }

    [HttpGet("providers")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<string>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProviders()
    {
        // Helper endpoint for filter sidebar - returns distinct provider names
        var providers = await _modelQueryService.GetDistinctProvidersAsync();
        return Ok(new ApiResponse<IEnumerable<string>> { Data = providers });
    }
}
```

**Endpoint Specifications:**

| Endpoint | Method | Purpose | Query Params | Response | Cache |
|----------|--------|---------|--------------|----------|-------|
| `/api/models` | GET | Fetch all active models with capabilities and top benchmarks | None | `ApiResponse<ModelDto[]>` | Redis 1hr, HTTP 1hr |
| `/api/models/providers` | GET | Get distinct provider names for filter sidebar | None | `ApiResponse<string[]>` | Redis 1hr |
| `/api/health` | GET | Health check (existing from Epic 1) | None | Health status JSON | None |

**Future Endpoints (Out of Scope for Epic 3):**
- `/api/models/{id}` - Model detail (Epic 4)
- `/api/smart-filters/best-value` - QAPS ranking (Epic 6)
- `/api/models/search?q={query}` - Server-side search (future optimization)

---

#### Frontend API Client

**API Client Setup (`apps/web/src/api/client.ts`):**

```typescript
import axios from 'axios';
import type { ApiResponse, ApiError } from '@/types/api';

export const apiClient = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000/api',
  timeout: 10000,
  headers: {
    'Content-Type': 'application/json',
    'Accept': 'application/json',
    'Accept-Encoding': 'gzip',
  },
});

// Response interceptor for error handling
apiClient.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.data?.error) {
      // Backend error response
      const apiError: ApiError = error.response.data;
      console.error('API Error:', apiError.error.message);
      throw new Error(apiError.error.message);
    }
    // Network or other errors
    throw error;
  }
);
```

**Model API Functions (`apps/web/src/api/models.ts`):**

```typescript
import { apiClient } from './client';
import type { ModelDto } from '@/types/models';
import type { ApiResponse } from '@/types/api';

export const modelsApi = {
  getAll: async (): Promise<ModelDto[]> => {
    const response = await apiClient.get<ApiResponse<ModelDto[]>>('/models');
    return response.data.data; // Extract data from envelope
  },

  getProviders: async (): Promise<string[]> => {
    const response = await apiClient.get<ApiResponse<string[]>>('/models/providers');
    return response.data.data;
  },
};
```

**TanStack Query Integration (`apps/web/src/hooks/useModels.ts`):**

```typescript
import { useQuery, UseQueryResult } from '@tanstack/react-query';
import { modelsApi } from '@/api/models';
import type { ModelDto } from '@/types/models';

export const useModels = (): UseQueryResult<ModelDto[], Error> => {
  return useQuery({
    queryKey: ['models'],
    queryFn: modelsApi.getAll,
    staleTime: 5 * 60 * 1000,      // 5 minutes
    cacheTime: 30 * 60 * 1000,     // 30 minutes
    refetchOnWindowFocus: false,
    retry: 2,                       // Retry failed requests twice
    retryDelay: (attemptIndex) => Math.min(1000 * 2 ** attemptIndex, 30000),
  });
};

export const useProviders = () => {
  return useQuery({
    queryKey: ['providers'],
    queryFn: modelsApi.getProviders,
    staleTime: 10 * 60 * 1000,     // 10 minutes (providers change rarely)
  });
};
```

---

#### Client-Side Filtering Logic

**Filter Utility (`apps/web/src/utils/filterModels.ts`):**

```typescript
import type { ModelDto } from '@/types/models';
import type { FilterState } from '@/types/filters';

export const filterModels = (
  models: ModelDto[],
  filters: FilterState
): ModelDto[] => {
  return models.filter((model) => {
    // Provider filter (OR logic)
    if (filters.providers.length > 0 && !filters.providers.includes(model.provider)) {
      return false;
    }

    // Capability filters (AND logic - must have all selected capabilities)
    if (filters.capabilities.functionCalling && !model.supportsFunctionCalling) {
      return false;
    }
    if (filters.capabilities.vision && !model.supportsVision) {
      return false;
    }
    if (filters.capabilities.audioInput && !model.supportsAudioInput) {
      return false;
    }
    if (filters.capabilities.audioOutput && !model.supportsAudioOutput) {
      return false;
    }
    if (filters.capabilities.streaming && !model.supportsStreaming) {
      return false;
    }
    if (filters.capabilities.jsonMode && !model.supportsJsonMode) {
      return false;
    }

    // Price range filter (average of input + output)
    const avgPrice = (model.inputPricePer1M + model.outputPricePer1M) / 2;
    if (avgPrice < filters.priceRange.min || avgPrice > filters.priceRange.max) {
      return false;
    }

    // Search query (case-insensitive, matches name or provider)
    if (filters.searchQuery) {
      const query = filters.searchQuery.toLowerCase();
      const matchesName = model.name.toLowerCase().includes(query);
      const matchesProvider = model.provider.toLowerCase().includes(query);
      if (!matchesName && !matchesProvider) {
        return false;
      }
    }

    return true;
  });
};
```

**Integration in ModelTable Component:**

```typescript
import { useMemo } from 'react';
import { useModels } from '@/hooks/useModels';
import { useFilterStore } from '@/store/filterStore';
import { filterModels } from '@/utils/filterModels';

export const ModelTable = () => {
  const { data: models, isLoading, error } = useModels();
  const filters = useFilterStore((state) => state);

  // Client-side filtering (memoized for performance)
  const filteredModels = useMemo(() => {
    if (!models) return [];
    return filterModels(models, filters);
  }, [models, filters]);

  // ... rest of component
};
```

**Performance Characteristics:**
- **Filter Execution Time:** <10ms for 100 models (JavaScript array filter)
- **Debounce:** Search input debounced 300ms to avoid excessive re-renders
- **Memoization:** `useMemo` prevents re-filtering unless models or filters change

---

#### TanStack Table Configuration

**Column Definitions (`apps/web/src/components/models/columns.tsx`):**

```typescript
import { createColumnHelper } from '@tanstack/react-table';
import type { ModelDto } from '@/types/models';

const columnHelper = createColumnHelper<ModelDto>();

export const columns = [
  columnHelper.display({
    id: 'select',
    header: ({ table }) => (
      <Checkbox
        checked={table.getIsAllRowsSelected()}
        onChange={table.getToggleAllRowsSelectedHandler()}
      />
    ),
    cell: ({ row }) => (
      <Checkbox
        checked={row.getIsSelected()}
        onChange={row.getToggleSelectedHandler()}
        disabled={!row.getCanSelect()}
      />
    ),
  }),
  columnHelper.accessor('name', {
    header: 'Model',
    cell: (info) => <span className="font-medium">{info.getValue()}</span>,
    sortingFn: 'alphanumeric',
  }),
  columnHelper.accessor('provider', {
    header: 'Provider',
    sortingFn: 'alphanumeric',
  }),
  columnHelper.accessor('inputPricePer1M', {
    header: 'Input Price',
    cell: (info) => `$${info.getValue().toFixed(2)}/1M`,
    sortingFn: 'basic',
  }),
  columnHelper.accessor('outputPricePer1M', {
    header: 'Output Price',
    cell: (info) => `$${info.getValue().toFixed(2)}/1M`,
    sortingFn: 'basic',
  }),
  columnHelper.accessor('contextWindow', {
    header: 'Context',
    cell: (info) => `${(info.getValue() / 1000).toFixed(0)}K`,
    sortingFn: 'basic',
  }),
  // Benchmark columns (dynamic based on available benchmarks)
  columnHelper.accessor((row) => row.topBenchmarks.find(b => b.benchmarkName === 'MMLU')?.score, {
    id: 'mmlu',
    header: 'MMLU',
    cell: (info) => info.getValue()?.toFixed(1) ?? 'N/A',
    sortingFn: 'basic',
  }),
  // ... additional benchmark columns
];
```

---

#### State Synchronization Patterns

**URL State Sync (for shareable filtered views):**

```typescript
import { useEffect } from 'react';
import { useSearchParams } from 'react-router-dom';
import { useFilterStore } from '@/store/filterStore';

export const useUrlStateSync = () => {
  const [searchParams, setSearchParams] = useSearchParams();
  const filters = useFilterStore();

  // Sync filters to URL on change
  useEffect(() => {
    const params = new URLSearchParams();

    if (filters.providers.length > 0) {
      params.set('providers', filters.providers.join(','));
    }
    if (filters.searchQuery) {
      params.set('q', filters.searchQuery);
    }
    if (filters.priceRange.min > 0 || filters.priceRange.max < 100) {
      params.set('priceMin', filters.priceRange.min.toString());
      params.set('priceMax', filters.priceRange.max.toString());
    }

    setSearchParams(params, { replace: true });
  }, [filters]);

  // Initialize filters from URL on mount
  useEffect(() => {
    const providers = searchParams.get('providers')?.split(',') || [];
    const searchQuery = searchParams.get('q') || '';
    const priceMin = parseFloat(searchParams.get('priceMin') || '0');
    const priceMax = parseFloat(searchParams.get('priceMax') || '100');

    if (providers.length > 0) {
      filters.setProviders(providers);
    }
    if (searchQuery) {
      filters.setSearchQuery(searchQuery);
    }
    if (priceMin > 0 || priceMax < 100) {
      filters.setPriceRange({ min: priceMin, max: priceMax });
    }
  }, []);
};
```

**Example Shareable URL:**
```
https://llm-price.app/?providers=OpenAI,Anthropic&priceMax=50&q=gpt
```

This URL would pre-filter to OpenAI and Anthropic models under $50/1M with "gpt" in the name.

### Workflows and Sequencing

This section illustrates the key user workflows and technical sequences that occur during Epic 3 interactions.

#### Workflow 1: Initial Page Load & Data Fetch

```
User navigates to "/"
  ↓
1. React Router renders HomePage component
  ↓
2. HomePage mounts → useModels() hook executes
  ↓
3. TanStack Query checks client cache
   ├─ Cache Hit (< 5min stale) → Return cached data (instant render)
   └─ Cache Miss → Proceed to step 4
  ↓
4. HTTP GET request to /api/models
  ↓
5. Backend: ModelsController.GetAllModels()
   ├─ Check Redis cache (key: "cache:models:list:v1")
   │  ├─ Cache Hit → Return JSON (< 10ms)
   │  └─ Cache Miss → Proceed to step 6
   ├─ 6. ModelQueryService.GetAllModelsAsync()
   ├─ 7. ModelRepository.GetAllWithDetailsAsync() (EF Core query)
   │      └─ SQL query with includes (Capabilities, BenchmarkScores)
   ├─ 8. AutoMapper: Model entities → ModelDto
   ├─ 9. Cache result in Redis (1hr TTL)
   └─ 10. Return ApiResponse<ModelDto[]>
  ↓
11. Response gzip compressed (~15KB)
  ↓
12. Frontend receives response → TanStack Query caches result
  ↓
13. ModelTable component receives data → TanStack Table renders
  ↓
14. User sees table with 50+ models (total time: <2s)
```

**Performance Checkpoints:**
- **Cold start (no caches):** 1.5-2s (database query ~150ms + network ~50ms + render ~300ms)
- **Warm Redis cache:** 300-500ms (Redis ~10ms + network ~50ms + render ~300ms)
- **Hot TanStack Query cache:** <100ms (instant render from memory)

---

#### Workflow 2: User Applies Filters

```
User checks "OpenAI" provider filter
  ↓
1. ProviderFilter onChange event fires
  ↓
2. Zustand filterStore.addProvider('OpenAI') executes
  ↓
3. FilterStore state update triggers React re-render
  ↓
4. ModelTable reads updated filters from useFilterStore()
  ↓
5. useMemo detects filters change → re-executes filterModels()
   └─ JavaScript array.filter() on client-side (< 10ms for 50 models)
  ↓
6. TanStack Table re-renders with filtered subset (e.g., 15 models)
  ↓
7. User sees updated table (< 100ms total)
  ↓
8. URL updates via useUrlStateSync()
   └─ New URL: /?providers=OpenAI
```

**Key Performance Factor:** Client-side filtering is **instant** (no server round-trip), enabling real-time interactive filtering experience.

---

#### Workflow 3: User Searches for Model

```
User types "gpt-4" in search input
  ↓
1. SearchBar onChange event fires on each keystroke
  ↓
2. useDebouncedValue hook buffers input (300ms delay)
  ↓
3. After 300ms silence, debounced value updates
  ↓
4. Zustand filterStore.setSearchQuery('gpt-4') executes
  ↓
5. ModelTable useMemo re-executes filterModels()
   └─ Filters by name.includes('gpt-4') OR provider.includes('gpt-4')
  ↓
6. TanStack Table re-renders with matching models (e.g., 3 results)
  ↓
7. User sees filtered results (< 100ms after debounce)
```

**Debounce Rationale:** Prevents excessive re-renders while user is typing, reducing CPU usage and improving perceived performance.

---

#### Workflow 4: User Selects Models for Comparison

```
User clicks checkbox next to "GPT-4"
  ↓
1. ModelRow checkbox onChange fires
  ↓
2. TanStack Table row.getToggleSelectedHandler() executes
  ↓
3. Zustand appStore.addModel(model) validates:
   ├─ Already selected? → Ignore (checkbox disabled)
   ├─ 5 models already selected? → Show toast error, reject
   └─ Valid? → Add to selectedModels array
  ↓
4. appStore state update triggers re-render
  ↓
5. ModelRow re-renders with isSelected=true (highlight row)
  ↓
6. ComparisonBasket detects selectedModels change → displays basket
   └─ Shows mini-card for "GPT-4"
  ↓
7. User repeats for 2-4 more models
  ↓
8. When 2+ selected, "Compare" button becomes enabled
```

**State Persistence:** `selectedModels` stored in Zustand (memory), not persisted across page refreshes (by design for MVP).

---

#### Workflow 5: User Sorts Table by Column

```
User clicks "Input Price" column header
  ↓
1. TanStack Table header onClick fires
  ↓
2. Table sorting state updates:
   - sortBy: 'inputPricePer1M'
   - sortDirection: 'asc'
  ↓
3. TanStack Table internal sorting executes
   └─ Uses 'basic' sortingFn (numeric comparison)
  ↓
4. Table re-renders with sorted rows (cheapest first)
  ↓
5. User sees sorted table (< 50ms)
  ↓
6. User clicks again → sortDirection toggles to 'desc' (most expensive first)
```

**Sorting Performance:** TanStack Table uses efficient QuickSort algorithm, <50ms for 100 rows.

---

#### Workflow 6: Admin Updates Model → Cache Invalidation

```
Admin updates "GPT-4" pricing in admin panel (Epic 2)
  ↓
1. PUT /api/admin/models/{id} request
  ↓
2. AdminModelService.UpdateModelAsync() executes
  ↓
3. Database update committed
  ↓
4. Domain event ModelUpdatedEvent published
  ↓
5. ModelUpdatedEventHandler executes:
   ├─ _cache.RemovePatternAsync("cache:models:*")
   │  └─ Deletes "cache:models:list:v1" from Redis
   └─ Optional: Publish cache invalidation message to Redis pub/sub
  ↓
6. Next public user request to /api/models:
   ├─ Redis cache miss (key deleted)
   └─ Database query fetches fresh data (with updated pricing)
  ↓
7. Fresh data cached and returned
  ↓
8. Users see updated pricing within 5min (TanStack Query stale time)
   └─ Force refresh: User can manually refresh browser
```

**Cache Invalidation Delay:** Up to 5 minutes for users with stale TanStack Query cache. Admin updates are eventually consistent, not immediate.

---

#### Workflow 7: Page Load with URL Filters (Shareable Link)

```
User clicks shared link: /?providers=OpenAI,Anthropic&priceMax=50
  ↓
1. HomePage mounts → useUrlStateSync() hook executes
  ↓
2. Hook reads URL search params:
   - providers: ['OpenAI', 'Anthropic']
   - priceMax: 50
  ↓
3. Zustand filterStore initialized with URL values:
   - setProviders(['OpenAI', 'Anthropic'])
   - setPriceRange({ min: 0, max: 50 })
  ↓
4. ModelTable reads filters → applies filterModels() with URL filters
  ↓
5. User sees pre-filtered table (only OpenAI & Anthropic models under $50)
```

**Use Case:** User shares filtered view with colleague → colleague opens link → sees same filtered results.

---

#### Sequence Diagram: Complete User Journey

```
┌─────┐         ┌──────────┐       ┌─────────┐       ┌──────┐       ┌──────────┐
│User │         │ Browser  │       │ React   │       │ API  │       │ Database │
└──┬──┘         └────┬─────┘       └────┬────┘       └───┬──┘       └────┬─────┘
   │                 │                   │                │                │
   │ Navigate to /   │                   │                │                │
   ├────────────────>│                   │                │                │
   │                 │ Mount HomePage    │                │                │
   │                 ├──────────────────>│                │                │
   │                 │                   │ GET /api/models│                │
   │                 │                   ├───────────────>│                │
   │                 │                   │                │ Query models   │
   │                 │                   │                ├───────────────>│
   │                 │                   │                │<───────────────┤
   │                 │                   │<───────────────┤ Return data    │
   │                 │<──────────────────┤ Render table   │                │
   │<────────────────┤ Display           │                │                │
   │                 │                   │                │                │
   │ Select provider │                   │                │                │
   ├────────────────>│                   │                │                │
   │                 │ Update filters    │                │                │
   │                 ├──────────────────>│ Client-side    │                │
   │                 │                   │ filter (no API)│                │
   │                 │<──────────────────┤ Re-render      │                │
   │<────────────────┤ Show filtered     │                │                │
   │                 │                   │                │                │
   │ Select 3 models │                   │                │                │
   ├────────────────>│                   │                │                │
   │                 │ Update selection  │                │                │
   │                 ├──────────────────>│                │                │
   │                 │<──────────────────┤ Show basket    │                │
   │<────────────────┤                   │                │                │
```

---

#### Error Handling Workflows

**Scenario 1: Database Connection Failure**

```
User navigates to / → API returns 500 error
  ↓
1. Axios interceptor catches error
  ↓
2. TanStack Query retry logic executes (2 retries with exponential backoff)
  ↓
3. All retries fail → useModels() returns error state
  ↓
4. ModelTable renders ErrorBoundary fallback UI:
   "Failed to load models. Please try again later."
  ↓
5. User clicks "Retry" button → TanStack Query refetch()
```

**Scenario 2: Network Timeout**

```
Slow network → API request exceeds 10s timeout
  ↓
1. Axios timeout fires → request aborted
  ↓
2. TanStack Query retry logic (same as above)
  ↓
3. LoadingSpinner shows during retries
  ↓
4. Eventually shows error or succeeds
```

**Scenario 3: Empty Database**

```
Database has 0 active models (development scenario)
  ↓
1. API returns empty array []
  ↓
2. ModelTable detects models.length === 0
  ↓
3. Renders EmptyState component:
   "No models available. Add models via admin panel."
  ↓
4. Shows button linking to /admin
```

## Non-Functional Requirements

### Performance

Epic 3 must meet the following performance targets to deliver the "instant filtering" user experience promised in the PRD.

**Response Time Targets:**

| Metric | Target | Measurement Method | Acceptance Threshold |
|--------|--------|-------------------|---------------------|
| Initial page load (cold cache) | < 2s | Lighthouse Performance Score | 90% of loads < 2s |
| Initial page load (warm Redis) | < 500ms | Server-side timing middleware | 95% of loads < 500ms |
| Filter application (client-side) | < 100ms | React DevTools Profiler | 99% of filter changes < 100ms |
| Search query (debounced) | < 100ms | Console.time() measurement | 100% < 100ms after debounce |
| Column sorting | < 50ms | TanStack Table internal metrics | 100% < 50ms |
| Model selection (checkbox click) | < 50ms | React re-render time | 100% < 50ms |

**Backend API Performance:**

```
GET /api/models Performance Breakdown:

Cache Hit (Redis):
├─ Redis lookup: 5-10ms
├─ Deserialization: 5-10ms
├─ HTTP response: 5-10ms
└─ Total: < 30ms (Target: < 50ms)

Cache Miss (Database):
├─ EF Core query (with includes): 100-150ms
├─ AutoMapper transformation: 20-30ms
├─ Redis cache write: 10-15ms
├─ JSON serialization: 15-20ms
├─ gzip compression: 10-15ms
└─ Total: 155-230ms (Target: < 250ms)
```

**Frontend Performance:**

```
React Component Render Times:

HomePage (initial mount):
├─ Component mount: 10-20ms
├─ useModels() hook initialization: 5ms
├─ TanStack Query cache check: 1-2ms
└─ Total: < 30ms

ModelTable (with 50 models):
├─ TanStack Table initialization: 30-50ms
├─ Column rendering: 20-30ms
├─ Row virtualization setup: 10-15ms
└─ Total: 60-95ms (Target: < 100ms)

FilterSidebar (interaction):
├─ Checkbox state update: 1-2ms
├─ Zustand store update: 2-3ms
├─ React re-render cascade: 10-20ms
└─ Total: 13-25ms (Target: < 50ms)
```

**Caching Efficiency:**

- **Target Cache Hit Ratio:** 80%+ for `/api/models` endpoint
- **Cache Warming Strategy:**
  - Admin panel updates trigger background cache pre-warm
  - Health check endpoint optionally pre-warms cache on deployment
- **Cache Memory Usage:** < 2MB Redis memory for models cache (50 models × 40KB per entry)

**Bundle Size Targets:**

| Asset | Size (gzipped) | Budget |
|-------|---------------|--------|
| Initial JS bundle | < 180KB | 200KB |
| Vendor JS (React, TanStack, etc.) | < 150KB | 180KB |
| CSS (TailwindCSS) | < 15KB | 20KB |
| Total initial payload | < 345KB | 400KB |

**Code Splitting Strategy:**
- Admin panel lazy-loaded (not in initial bundle)
- Chart.js lazy-loaded (Epic 5 comparison charts)
- Model detail modal lazy-loaded (Epic 4)

**Performance Monitoring:**

```typescript
// Performance measurement utility
export const measurePerformance = (label: string, fn: () => void) => {
  if (import.meta.env.DEV) {
    performance.mark(`${label}-start`);
    fn();
    performance.mark(`${label}-end`);
    performance.measure(label, `${label}-start`, `${label}-end`);
    const measure = performance.getEntriesByName(label)[0];
    console.log(`⏱️ ${label}: ${measure.duration.toFixed(2)}ms`);
  } else {
    fn();
  }
};
```

---

### Security

Epic 3 implements security best practices for a public-facing read-only interface.

**Public API Security (No Authentication Required):**

| Threat | Mitigation | Implementation |
|--------|-----------|----------------|
| DDoS / API abuse | Rate limiting | Nginx: 100 req/min per IP, 429 response on exceed |
| SQL Injection | Parameterized queries | EF Core uses parameters by default, no raw SQL |
| XSS (Cross-Site Scripting) | React escaping | React escapes by default, no `dangerouslySetInnerHTML` |
| CSRF (for future POST endpoints) | SameSite cookies | `SameSite=Strict` for admin cookies (Epic 2) |
| Sensitive data exposure | Minimal data | Public API exposes only necessary fields, no internal IDs exposed as integers |

**Input Validation (Defense in Depth):**

Even though Epic 3 has no user input to backend (read-only API), we validate URL parameters for future features:

```csharp
// Future: GET /api/models?provider=OpenAI validation
public class ModelQueryValidator : AbstractValidator<ModelQueryParams>
{
    public ModelQueryValidator()
    {
        RuleFor(x => x.Provider)
            .MaximumLength(100)
            .WithMessage("Provider name too long");

        RuleFor(x => x.SearchQuery)
            .MaximumLength(200)
            .WithMessage("Search query too long");
    }
}
```

**Frontend Security:**

- **Content Security Policy (CSP):**
  ```http
  Content-Security-Policy: default-src 'self';
    script-src 'self';
    style-src 'self' 'unsafe-inline';
    img-src 'self' data: https:;
    connect-src 'self' https://api.llmprice.app;
  ```

- **HTTP Headers (Production):**
  ```http
  X-Frame-Options: DENY
  X-Content-Type-Options: nosniff
  Referrer-Policy: strict-origin-when-cross-origin
  Permissions-Policy: geolocation=(), microphone=(), camera=()
  ```

**Data Privacy:**

- **No PII Collection:** Epic 3 collects no personal information (no user accounts)
- **Analytics:** If Google Analytics added (future), anonymize IP addresses
- **Cookies:** No cookies set by Epic 3 (admin cookies from Epic 2 only)

**Dependency Security:**

- **Automated Scanning:** Dependabot enabled for npm and NuGet packages
- **Known Vulnerabilities:** Zero tolerance for high/critical CVEs in dependencies
- **Update Policy:** Security patches applied within 7 days of disclosure

---

### Reliability/Availability

Epic 3 targets 99% uptime for the public comparison table interface.

**Availability Targets:**

| Component | Target Uptime | Downtime Allowance |
|-----------|---------------|-------------------|
| Frontend (Vercel/CDN) | 99.9% | < 44 min/month |
| Backend API | 99.0% | < 7.2 hours/month |
| PostgreSQL Database | 99.5% | < 3.6 hours/month |
| Redis Cache | 99.0% | < 7.2 hours/month (graceful degradation) |

**Graceful Degradation Strategy:**

```
Redis Cache Failure:
  ↓
Backend detects Redis connection error
  ↓
Fallback: Query database directly (slower but functional)
  ↓
Log error, alert ops team
  ↓
Users experience slower response (300-500ms vs <50ms) but site remains functional
```

**Error Handling Patterns:**

```typescript
// Frontend error boundary wraps entire app
<ErrorBoundary
  fallback={<ErrorFallback />}
  onError={(error) => {
    // Log to error tracking (Sentry, etc.)
    console.error('React Error:', error);
  }}
>
  <App />
</ErrorBoundary>

// API error handling with fallbacks
export const useModels = () => {
  return useQuery({
    queryKey: ['models'],
    queryFn: modelsApi.getAll,
    retry: 2,
    retryDelay: (attemptIndex) => Math.min(1000 * 2 ** attemptIndex, 30000),
    useErrorBoundary: false, // Handle errors in component
    onError: (error) => {
      toast.error('Failed to load models. Please try again.');
    },
  });
};
```

**Database Resilience:**

- **Connection Pooling:** Max 20 concurrent connections, 30s timeout
- **Retry Logic:** 3 retries with exponential backoff on transient errors
- **Circuit Breaker:** After 5 consecutive failures, open circuit for 30s (prevent cascading failures)

**Deployment Strategy (Zero-Downtime):**

```
Backend Deployment:
1. Deploy new version to staging slot
2. Run smoke tests (health check, sample API call)
3. Swap staging → production (blue-green deployment)
4. Monitor error rates for 5 minutes
5. Rollback if error rate > 1%
```

**Backup & Recovery:**

- **Database Backups:** Daily full backup, 7-day retention
- **Redis Backup:** Not critical (cache can rebuild from DB)
- **RTO (Recovery Time Objective):** < 1 hour
- **RPO (Recovery Point Objective):** < 24 hours (acceptable data loss for read-only cache)

---

### Observability

Epic 3 implements comprehensive logging, metrics, and tracing to enable rapid diagnosis and performance optimization.

**Structured Logging (Serilog):**

```csharp
// Backend logging pattern
_logger.LogInformation(
    "Models fetched: {Count} models, {CacheHit} cache hit, {Duration}ms",
    models.Count(),
    cacheHit,
    stopwatch.ElapsedMilliseconds
);

// Error logging with context
_logger.LogError(
    ex,
    "Failed to fetch models: {ErrorCode}, {DatabaseStatus}",
    "DATABASE_ERROR",
    await _db.CanConnectAsync()
);
```

**Log Levels:**
- **Debug:** Development only (SQL queries, cache operations)
- **Information:** Request/response cycles, cache hits/misses
- **Warning:** Slow queries (>500ms), cache misses on hot paths
- **Error:** Database connection failures, unhandled exceptions
- **Critical:** System-wide failures (Redis down, DB down)

**Metrics Collection:**

| Metric | Type | Purpose | Alert Threshold |
|--------|------|---------|----------------|
| `api.models.requests.total` | Counter | Total API requests | N/A |
| `api.models.cache_hit_ratio` | Gauge | Redis cache efficiency | < 70% |
| `api.models.response_time.p95` | Histogram | 95th percentile latency | > 500ms |
| `api.models.errors.rate` | Counter | Error rate per minute | > 10/min |
| `db.query.duration.avg` | Gauge | Avg database query time | > 200ms |
| `redis.connection.failures` | Counter | Redis connection errors | > 5/hour |

**Application Performance Monitoring (APM):**

```typescript
// Frontend performance tracking (Vercel Analytics or similar)
export const trackPageLoad = () => {
  if ('PerformanceObserver' in window) {
    const observer = new PerformanceObserver((list) => {
      list.getEntries().forEach((entry) => {
        if (entry.entryType === 'navigation') {
          analytics.track('page_load', {
            duration: entry.duration,
            domContentLoaded: entry.domContentLoadedEventEnd - entry.domContentLoadedEventStart,
            resourceCount: performance.getEntriesByType('resource').length,
          });
        }
      });
    });
    observer.observe({ entryTypes: ['navigation'] });
  }
};
```

**Distributed Tracing (Future Enhancement):**

```csharp
// OpenTelemetry tracing (Phase 2)
using var activity = Activity.StartActivity("GetModels");
activity?.SetTag("cache.key", cacheKey);
activity?.SetTag("cache.hit", cacheHit);

// Trace propagation to frontend via headers
response.Headers.Add("X-Trace-Id", Activity.Current?.TraceId.ToString());
```

**Health Checks (Enhanced from Epic 1):**

```csharp
// Detailed health endpoint
[HttpGet("health/detailed")]
public async Task<IActionResult> GetDetailedHealth()
{
    var health = new
    {
        Status = "Healthy",
        Database = await _db.CanConnectAsync() ? "Connected" : "Disconnected",
        Redis = await _cache.PingAsync() ? "Connected" : "Disconnected",
        Timestamp = DateTime.UtcNow,
        Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString(),
        Environment = _env.EnvironmentName,
    };

    return Ok(health);
}
```

**Alerting Rules:**

```yaml
# Example Prometheus alert rules
groups:
  - name: llm-price-api
    rules:
      - alert: HighErrorRate
        expr: rate(api_errors_total[5m]) > 10
        for: 5m
        annotations:
          summary: "High error rate detected"

      - alert: LowCacheHitRatio
        expr: api_cache_hit_ratio < 0.7
        for: 15m
        annotations:
          summary: "Cache hit ratio below 70%"

      - alert: SlowAPIResponse
        expr: histogram_quantile(0.95, api_response_time) > 0.5
        for: 10m
        annotations:
          summary: "95th percentile response time > 500ms"
```

**Monitoring Dashboard (Grafana/similar):**

```
Dashboard Panels:
├─ API Request Rate (requests/sec)
├─ Cache Hit Ratio (%)
├─ Response Time P50/P95/P99 (ms)
├─ Error Rate (errors/min)
├─ Active Database Connections
├─ Redis Memory Usage (MB)
└─ Frontend Page Load Times (s)
```

## Dependencies and Integrations

### External Dependencies

**Backend NuGet Packages:**

| Package | Version | Purpose | Justification |
|---------|---------|---------|---------------|
| `Microsoft.EntityFrameworkCore` | 9.0.x | ORM for PostgreSQL | Core data access layer |
| `Npgsql.EntityFrameworkCore.PostgreSQL` | 9.0.x | PostgreSQL provider | Database connectivity |
| `StackExchange.Redis` | 2.7.x | Redis client | Caching layer |
| `AutoMapper` | 13.0.x | Object mapping | Entity → DTO transformation |
| `Serilog.AspNetCore` | 8.0.x | Structured logging | Observability |
| `Swashbuckle.AspNetCore` | 6.5.x | OpenAPI/Swagger | API documentation |

**Frontend npm Packages:**

| Package | Version | Purpose | Bundle Impact |
|---------|---------|---------|--------------|
| `react` | 19.0.x | UI framework | 45KB (gzipped) |
| `react-dom` | 19.0.x | DOM renderer | Included in react |
| `@tanstack/react-table` | 8.11.x | Table logic | 35KB |
| `@tanstack/react-query` | 5.17.x | Server state | 42KB |
| `zustand` | 4.4.x | Client state | 1.2KB |
| `axios` | 1.6.x | HTTP client | 15KB |
| `tailwindcss` | 4.0.x | CSS framework | 15KB (purged) |
| `lucide-react` | 0.300.x | Icons | 2KB (tree-shaken) |
| `react-router-dom` | 6.21.x | Routing | 18KB |

**Total Frontend Bundle (Epic 3):** ~173KB gzipped (under 200KB budget ✅)

---

### Infrastructure Dependencies

| Service | Provider | Purpose | Fallback Strategy |
|---------|----------|---------|------------------|
| PostgreSQL 16 | Local/Railway/AWS RDS | Primary data store | None (critical path) |
| Redis 7.2 | Upstash/AWS ElastiCache | Caching layer | Fallback to database (graceful degradation) |
| CDN | Vercel/CloudFlare | Static asset delivery | Browser cache (5min TTL) |
| DNS | CloudFlare | Domain resolution | None (infrastructure) |

---

###

 Epic Dependencies

**Prerequisite Epics:**

- **Epic 1 (COMPLETE):** Database schema, API foundation, frontend shell, CI/CD
- **Epic 2 (COMPLETE):** Admin panel, model CRUD, benchmark management

**Blocks Future Epics:**

- **Epic 4:** Model detail modal (uses `selectedModels` from comparison basket - Story 3.11)
- **Epic 5:** Side-by-side comparison (requires `ComparisonBasket` component - Story 3.11)
- **Epic 6:** Best Value smart filter (extends filtering infrastructure - Stories 3.5-3.7)

---

### Third-Party Integrations

**Development Tools:**

- **Vite** - Build tool (no runtime dependency)
- **TypeScript Compiler** - Type checking (compile-time only)
- **ESLint/Prettier** - Code quality (development only)

**Monitoring & Analytics (Future):**

- **Vercel Analytics** - Page load metrics (optional, phase 2)
- **Sentry** - Error tracking (optional, phase 2)
- **PostHog** - Product analytics (optional, phase 2)

**No External API Calls:**
Epic 3 makes **zero external API calls** - all data served from internal database/cache.

---

## Acceptance Criteria (Authoritative)

This section defines the **authoritative acceptance criteria** that must be met for Epic 3 to be considered complete. These criteria supersede individual story acceptance criteria if conflicts arise.

### Functional Acceptance Criteria

**AC-F1: Model Data Display**
- ✅ Homepage displays table with 50+ models loaded from `/api/models`
- ✅ Each row shows: name, provider, input price, output price, context window, top 3-5 benchmarks
- ✅ Models load within 2 seconds on initial page visit (cold cache)
- ✅ Empty state displayed if database contains 0 models

**AC-F2: Table Sorting**
- ✅ All columns sortable (click header → ascending, click again → descending)
- ✅ Sort indicator (arrow icon) shows current sort direction
- ✅ Sorting applies instantly (<100ms) for 50 models
- ✅ Default sort: alphabetical by model name (ascending)

**AC-F3: Provider Filtering**
- ✅ Filter sidebar displays checkboxes for all unique providers
- ✅ Selecting providers filters table in real-time (<100ms)
- ✅ Multiple providers use OR logic (show models from any selected provider)
- ✅ "Clear Filters" button resets all provider selections

**AC-F4: Capability Filtering**
- ✅ Capability checkboxes for: function calling, vision, audio input, audio output, streaming, JSON mode
- ✅ Capability filters use AND logic (must have all selected capabilities)
- ✅ Filters apply instantly (<100ms) to visible table
- ✅ Tooltip explains each capability on hover

**AC-F5: Price Range Filtering**
- ✅ Dual-range slider for min/max price (average of input + output price per 1M)
- ✅ Slider defaults to min/max of available models
- ✅ Adjusting slider filters table in real-time (<100ms)
- ✅ Price range inputs display current selection with currency formatting

**AC-F6: Search Functionality**
- ✅ Search input filters by model name OR provider (case-insensitive, partial match)
- ✅ Search debounced 300ms to avoid excessive re-renders
- ✅ Clear search button (X icon) resets search query
- ✅ Search works alongside other filters (combined logic)

**AC-F7: Model Selection**
- ✅ Checkbox in first column allows selecting models
- ✅ Maximum 5 models selectable (6th checkbox click shows error toast)
- ✅ Selected models highlighted with background color
- ✅ Selection persists during filtering/sorting (same models remain selected)

**AC-F8: Comparison Basket**
- ✅ Basket displays when 1+ models selected (top of page or floating bar)
- ✅ Shows mini-card for each selected model (name, provider)
- ✅ X button on card removes model from selection
- ✅ "Compare Selected" button enabled when 2+ models selected
- ✅ "Clear All" button removes all selections and hides basket

**AC-F9: URL State Sync**
- ✅ Filters reflected in URL query parameters (e.g., `?providers=OpenAI&priceMax=50`)
- ✅ Sharing URL loads page with pre-applied filters
- ✅ Browser back/forward buttons work correctly with filter state

**AC-F10: Responsive Design**
- ✅ Table displays correctly on desktop (1920×1080, 1366×768)
- ✅ Table displays correctly on tablet (768×1024)
- ✅ Table adapts to mobile (375×667) with horizontal scroll or card layout
- ✅ Filter sidebar collapses to drawer/accordion on mobile

---

### Performance Acceptance Criteria

**AC-P1: Initial Page Load**
- ✅ 90% of page loads complete in <2s (Lighthouse Performance Score ≥90)
- ✅ First Contentful Paint (FCP) <1.2s
- ✅ Largest Contentful Paint (LCP) <2.5s
- ✅ Cumulative Layout Shift (CLS) <0.1

**AC-P2: API Performance**
- ✅ `/api/models` cache hit response <50ms (95th percentile)
- ✅ `/api/models` cache miss response <250ms (95th percentile)
- ✅ Redis cache hit ratio >80% after warm-up (measured over 1 hour)

**AC-P3: Client-Side Performance**
- ✅ Filter application <100ms (measured via React DevTools Profiler)
- ✅ Search query filtering <100ms after debounce
- ✅ Column sorting <50ms for 50 rows
- ✅ Model selection (checkbox click) <50ms

**AC-P4: Bundle Size**
- ✅ Initial JS bundle <200KB gzipped
- ✅ Total initial payload <400KB gzipped (HTML + JS + CSS)
- ✅ Admin panel code-split (not in initial bundle)

---

### Quality Acceptance Criteria

**AC-Q1: Code Quality**
- ✅ Zero TypeScript `any` types (strict mode enabled)
- ✅ Zero ESLint errors, warnings allowed only for special cases
- ✅ All public API endpoints have Swagger/OpenAPI documentation
- ✅ All components have TypeScript prop types

**AC-Q2: Test Coverage**
- ✅ Backend: 70%+ unit test coverage for application services
- ✅ Frontend: 60%+ unit test coverage for utilities and hooks
- ✅ E2E tests cover critical user flows (load page, apply filters, select models)
- ✅ All tests pass in CI/CD pipeline

**AC-Q3: Accessibility**
- ✅ Keyboard navigation works for all interactive elements
- ✅ ARIA labels present on all form inputs
- ✅ Table navigable via keyboard (tab through rows)
- ✅ Filter checkboxes accessible via keyboard

**AC-Q4: Error Handling**
- ✅ Database connection failure shows error message with retry button
- ✅ Network timeout handled gracefully (retry logic + user feedback)
- ✅ Empty database shows appropriate empty state
- ✅ All errors logged to console with structured format

---

### Integration Acceptance Criteria

**AC-I1: Epic 1 Integration**
- ✅ Uses database schema from Epic 1 Story 1.4 (no schema changes)
- ✅ Redis connection from Epic 1 Story 1.5 functional
- ✅ Frontend shell from Epic 1 Story 1.7 extended (not replaced)

**AC-I2: Epic 2 Integration**
- ✅ Admin model updates (Epic 2 Story 2.7) trigger cache invalidation
- ✅ Models managed via admin panel (Epic 2) appear in public table within 5min
- ✅ Benchmark scores from Epic 2 Story 2.10 displayed in table columns

**AC-I3: Future Epic Setup**
- ✅ `selectedModels` Zustand store ready for Epic 5 comparison page
- ✅ `ComparisonBasket` component reusable in Epic 5
- ✅ Filter architecture extensible for Epic 6 smart filters

---

## Traceability Mapping

This section maps acceptance criteria to implementation components and test cases, ensuring every requirement is traceable to code and tests.

| Acceptance Criteria | Tech Spec Section | Backend Component | Frontend Component | Story | Test Case |
|---------------------|-------------------|-------------------|-------------------|-------|-----------|
| AC-F1: Model Data Display | 3.2 Data Models | `ModelQueryService`, `/api/models` | `HomePage`, `ModelTable` | 3.1, 3.2 | `ModelTableIntegrationTest`, E2E `pageLoad.spec.ts` |
| AC-F2: Table Sorting | 3.4 APIs & Interfaces | None (client-side) | `ModelTable` (TanStack Table) | 3.4 | `modelTable.test.tsx` |
| AC-F3: Provider Filtering | 3.4 APIs & Interfaces | None (client-side) | `FilterSidebar`, `ProviderFilter` | 3.5 | `filterModels.test.ts`, E2E `filtering.spec.ts` |
| AC-F4: Capability Filtering | 3.4 APIs & Interfaces | None (client-side) | `CapabilityFilter` | 3.6 | `filterModels.test.ts` |
| AC-F5: Price Range Filtering | 3.4 APIs & Interfaces | None (client-side) | `PriceRangeFilter` | 3.7 | `filterModels.test.ts` |
| AC-F6: Search Functionality | 3.4 APIs & Interfaces | None (client-side) | `SearchBar`, `filterModels` | 3.8 | `filterModels.test.ts` |
| AC-F7: Model Selection | 3.1 Services | None (client-side) | `ModelTable`, `appStore` | 3.10 | `appStore.test.ts` |
| AC-F8: Comparison Basket | 3.1 Services | None (client-side) | `ComparisonBasket` | 3.11 | `comparisonBasket.test.tsx` |
| AC-F9: URL State Sync | 3.4 APIs & Interfaces | None (client-side) | `useUrlStateSync` hook | 3.1 | `urlStateSync.test.ts` |
| AC-F10: Responsive Design | 3.3 Workflows | None (CSS) | All components | 3.13 | Visual regression tests (Playwright) |
| AC-P1: Initial Page Load | 3.5 Performance NFRs | `ModelQueryService`, Redis cache | `HomePage`, TanStack Query | 3.2, 3.15 | Lighthouse CI, Performance monitoring |
| AC-P2: API Performance | 3.5 Performance NFRs | `RedisCacheService`, EF Core optimizations | N/A | 3.15 | `ModelQueryServiceTests`, load tests |
| AC-P3: Client-Side Performance | 3.5 Performance NFRs | N/A | `useMemo` optimizations, debouncing | 3.4-3.8 | React DevTools Profiler measurements |
| AC-P4: Bundle Size | 3.5 Performance NFRs | N/A | Vite code splitting, lazy loading | 3.2 | Bundle analyzer in CI/CD |
| AC-Q1: Code Quality | N/A (process) | TypeScript strict mode | TypeScript strict mode | All | ESLint CI check, type-check CI step |
| AC-Q2: Test Coverage | 3.9 Test Strategy | xUnit tests | Vitest tests | All | Coverage report in CI/CD |
| AC-Q3: Accessibility | 3.6 Security NFRs | N/A | ARIA labels, semantic HTML | 3.13, 3.14 | axe-core automated tests, manual audit |
| AC-Q4: Error Handling | 3.3 Workflows | Try-catch with logging | ErrorBoundary, retry logic | 3.2 | Error simulation tests |
| AC-I1: Epic 1 Integration | 3.2 Architecture Alignment | Uses Epic 1 DB schema | Uses Epic 1 frontend shell | N/A | Integration test suite |
| AC-I2: Epic 2 Integration | 3.3 Workflows (WF 6) | `ModelUpdatedEventHandler` | TanStack Query invalidation | 3.15 | Cache invalidation integration test |
| AC-I3: Future Epic Setup | 3.1 Services | N/A | `appStore.selectedModels` | 3.10, 3.11 | State structure validation tests |

---

## Risks, Assumptions, Open Questions

### Risks

**R1: Performance Degradation with Large Model Count**
- **Risk:** Client-side filtering may slow down if model count exceeds 200-300
- **Likelihood:** Medium (database may grow beyond 100 models)
- **Impact:** High (user experience degradation)
- **Mitigation:** Virtual scrolling (TanStack Table supports this), server-side filtering fallback in Epic 6
- **Contingency:** If performance drops below AC-P3 thresholds, implement pagination

**R2: Redis Cache Unavailability**
- **Risk:** Redis downtime causes all requests to hit database, potentially overloading it
- **Likelihood:** Low (99% uptime target)
- **Impact:** Medium (slower responses, not complete failure)
- **Mitigation:** Graceful degradation, database connection pooling, circuit breaker
- **Monitoring:** Alert on cache hit ratio <50%

**R3: Browser Compatibility Issues**
- **Risk:** TanStack Table or TailwindCSS features unsupported in older browsers
- **Likelihood:** Low (target last 2 versions of major browsers)
- **Impact:** Medium (small percentage of users affected)
- **Mitigation:** Browserslist configuration, Babel polyfills for critical features
- **Testing:** Manual testing on Chrome, Firefox, Safari, Edge

**R4: Epic 2 Incomplete or Buggy**
- **Risk:** If Epic 2 admin panel has bugs, model data may be inconsistent
- **Likelihood:** Low (Epic 2 has 42 passing tests)
- **Impact:** High (garbage in, garbage out)
- **Mitigation:** Data validation in Epic 3 (defensive programming), Epic 2 bug triage before Epic 3 start
- **Blocker:** Epic 3 blocked until Story 2.13 (technical debt) complete

---

### Assumptions

**A1: Database Populated with Real Data**
- Epic 3 assumes database contains 10-15 models with capabilities and benchmark scores
- Without data, table displays empty state (acceptable for development)
- **Validation:** Seed scripts from Epic 2 must run successfully

**A2: Network Latency <100ms**
- Performance targets assume typical broadband/4G latency (50-100ms)
- High-latency networks (satellite, rural 3G) may exceed 2s load time target
- **Acceptable:** Performance targets are for 90th percentile users

**A3: Redis Available in Production**
- Architecture assumes Redis deployment alongside backend API
- Fallback to database-only acceptable for MVP, but performance degrades
- **Deployment Note:** Railway/Render provide Redis add-ons for <$10/month

**A4: TanStack Table Stable API**
- Assumes TanStack Table v8 API remains stable (no breaking changes)
- v9 release may require migration, but v8 supported until mid-2025
- **Mitigation:** Pin dependency versions, monitor changelogs

**A5: No Internationalization (i18n) Required**
- Epic 3 assumes English-only interface
- Multi-language support deferred to Phase 2+
- **Future Work:** Extract hard-coded strings to i18n files when needed

---

### Open Questions

**Q1: Pagination vs. Virtual Scrolling?**
- **Question:** Should table use pagination (10/25/50 per page) or virtual scrolling (render visible rows only)?
- **Decision Needed By:** Story 3.12 implementation
- **Recommendation:** Virtual scrolling for seamless UX, but validate performance with 100+ rows
- **Fallback:** Pagination if virtual scrolling causes jank on low-end devices

**Q2: URL State Sync - All Filters or Minimal?**
- **Question:** Should URL include all filter state (providers, capabilities, price, search) or only essential filters?
- **Trade-off:** Comprehensive URLs are shareable but verbose; minimal URLs are cleaner but less complete
- **Current Approach:** Include all filters (Story 3.1 requirement for shareable links)
- **Review:** User feedback on URL length after MVP launch

**Q3: Mobile Table Layout - Scroll or Cards?**
- **Question:** On mobile, should table scroll horizontally or switch to card-based layout?
- **Decision Needed By:** Story 3.13 (responsive design)
- **Recommendation:** Start with horizontal scroll (simpler), add card layout if user testing shows poor UX
- **Epic 8 Scope:** Mobile optimization epic may revisit this

**Q4: Cache Warming on Deployment?**
- **Question:** Should deployment scripts pre-warm Redis cache to avoid cold start latency?
- **Trade-off:** Adds deployment complexity but eliminates first-request slowness
- **Current Approach:** No pre-warming for MVP (acceptable to have first request at 200ms)
- **Future Enhancement:** Health check endpoint could trigger cache warm

**Q5: Error Telemetry - Sentry or CloudWatch?**
- **Question:** Which error tracking service to use for production monitoring?
- **Decision Needed By:** Pre-production deployment (post-Epic 3)
- **Options:** Sentry (feature-rich, $26/mo), CloudWatch (AWS-native, $10/mo), None (MVP acceptable)
- **Recommendation:** Defer to Phase 2 unless critical errors observed in testing

---

## Test Strategy Summary

### Testing Pyramid

```
        ┌─────────────┐
        │  E2E Tests  │  5% - 3 critical user flows (Playwright)
        └─────────────┘
      ┌─────────────────┐
      │Integration Tests│  25% - API + DB + Cache integration (xUnit, MSW)
      └─────────────────┘
    ┌───────────────────────┐
    │     Unit Tests        │  70% - Business logic, utilities, hooks (xUnit, Vitest)
    └───────────────────────┘
```

---

### Backend Testing (xUnit)

**Unit Tests (70% of backend tests):**

| Test Suite | Coverage Target | Key Tests |
|------------|----------------|-----------|
| `ModelQueryServiceTests` | 90%+ | Cache hit, cache miss, AutoMapper, empty database |
| `RedisCacheServiceTests` | 80%+ | Get, Set, Delete, RemovePattern, connection failure |
| `ModelRepositoryTests` | 75%+ | GetAllWithDetailsAsync, optimized includes, AsNoTracking |
| `ModelUpdatedEventHandlerTests` | 90%+ | Cache invalidation trigger, pattern deletion |

**Example Unit Test:**

```csharp
[Fact]
public async Task GetAllModelsAsync_CacheHit_ReturnsCachedData()
{
    // Arrange
    var cachedModels = GetSampleModelDtos();
    _mockCache.Setup(c => c.GetAsync<IEnumerable<ModelDto>>("cache:models:list:v1"))
        .ReturnsAsync(cachedModels);

    var service = new ModelQueryService(_mockRepo.Object, _mockCache.Object, _mockMapper.Object);

    // Act
    var result = await service.GetAllModelsAsync();

    // Assert
    result.Should().BeEquivalentTo(cachedModels);
    _mockRepo.Verify(r => r.GetAllWithDetailsAsync(), Times.Never); // DB not queried
}
```

**Integration Tests (25% of backend tests):**

- `ModelsControllerIntegrationTests`: Full HTTP request cycle (TestServer + TestContainers)
- `CacheIntegrationTests`: Redis operations with real Redis container
- `DatabaseIntegrationTests`: EF Core queries against PostgreSQL container

---

### Frontend Testing (Vitest + Testing Library)

**Unit Tests (70% of frontend tests):**

| Test Suite | Coverage Target | Key Tests |
|------------|----------------|-----------|
| `filterModels.test.ts` | 100% | All filter combinations, edge cases |
| `useModels.test.ts` | 80%+ | TanStack Query states (loading, success, error) |
| `appStore.test.ts` | 90%+ | Model selection logic, 5-model limit, clear |
| `filterStore.test.ts` | 90%+ | Filter state updates, reset |
| `urlStateSync.test.ts` | 85%+ | URL → state, state → URL, edge cases |

**Component Tests (25% of frontend tests):**

- `ModelTable.test.tsx`: Rendering, sorting, selection
- `FilterSidebar.test.tsx`: Checkbox interactions, "Clear Filters"
- `ComparisonBasket.test.tsx`: Display logic, remove model

**Example Component Test:**

```typescript
it('applies filters instantly when provider selected', async () => {
  const { user } = render(<HomePage />);

  // Wait for models to load
  await waitFor(() => screen.getByText('GPT-4'));

  // Select "OpenAI" provider filter
  const checkbox = screen.getByLabelText('OpenAI');
  await user.click(checkbox);

  // Verify only OpenAI models visible
  expect(screen.getByText('GPT-4')).toBeInTheDocument();
  expect(screen.queryByText('Claude 3 Opus')).not.toBeInTheDocument();
});
```

---

### E2E Testing (Playwright - 5% of tests)

**Critical User Flows:**

1. **Page Load → Filter → Select → Compare**
   ```typescript
   test('user can filter models and select for comparison', async ({ page }) => {
     await page.goto('/');

     // Apply provider filter
     await page.click('text=OpenAI');

     // Verify filtered results
     await expect(page.locator('.model-table tbody tr')).toHaveCount(5);

     // Select 2 models
     await page.click('[data-testid="checkbox-gpt-4"]');
     await page.click('[data-testid="checkbox-gpt-3.5"]');

     // Verify comparison basket
     await expect(page.locator('.comparison-basket')).toContainText('GPT-4');
     await expect(page.locator('.comparison-basket')).toContainText('GPT-3.5');

     // Compare button enabled
     await expect(page.locator('button:has-text("Compare")')).toBeEnabled();
   });
   ```

2. **Search Functionality**
   ```typescript
   test('user can search for models', async ({ page }) => {
     await page.goto('/');

     // Type in search
     await page.fill('input[placeholder="Search models..."]', 'claude');

     // Wait for debounce
     await page.waitForTimeout(350);

     // Verify filtered results
     await expect(page.locator('tbody tr')).toHaveCount(3); // 3 Claude models
   });
   ```

3. **Error Handling**
   ```typescript
   test('shows error message when API fails', async ({ page }) => {
     await page.route('**/api/models', (route) => route.abort());
     await page.goto('/');

     // Verify error message
     await expect(page.locator('text=Failed to load models')).toBeVisible();

     // Verify retry button
     await expect(page.locator('button:has-text("Retry")')).toBeVisible();
   });
   ```

---

### Performance Testing

**Lighthouse CI (GitHub Actions):**

```yaml
- name: Run Lighthouse CI
  run: |
    npm install -g @lhci/cli
    lhci autorun --collect.url=http://localhost:5173
  env:
    LHCI_GITHUB_APP_TOKEN: ${{ secrets.LHCI_GITHUB_APP_TOKEN }}
```

**Bundle Size Monitoring:**

```yaml
- name: Check bundle size
  run: |
    npm run build
    npx bundlesize
```

**Load Testing (Future):**
- k6 or Artillery to simulate 100 concurrent users
- Target: <250ms P95 response time under load

---

### Coverage Targets

| Layer | Coverage Target | Current Status | Gap |
|-------|----------------|----------------|-----|
| Backend Application Services | 90% | TBD | Implement in Story 3.15 |
| Backend Infrastructure | 70% | TBD | Implement in Story 3.15 |
| Frontend Utilities/Hooks | 85% | TBD | Implement alongside feature stories |
| Frontend Components | 60% | TBD | Implement alongside feature stories |
| E2E Critical Flows | 100% (3 flows) | TBD | Implement in Story 3.13 |

**Coverage Enforcement:**
- CI/CD fails if coverage drops below targets
- PRs require coverage report comment (automated via GitHub Actions)
