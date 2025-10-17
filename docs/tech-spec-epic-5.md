# Technical Specification: Multi-Model Comparison & Visualization

Date: 2025-01-17
Author: Pablo
Epic ID: 5
Status: Draft

---

## Overview

Epic 5 delivers the **side-by-side model comparison experience** that transforms user selections from the main table into actionable insights through visualization and detailed attribute comparison. This epic implements the `/compare` page where users can analyze 2-5 models simultaneously using synchronized model cards, comparison tables, interactive Chart.js visualizations, and capability matrices.

The comparison page serves as the platform's primary decision-support interface, enabling users to move beyond simple table browsing to deep multi-dimensional analysis. By combining pricing visualizations, benchmark charts with metric selectors, and exportable comparison data, this epic delivers the "informed selection" value promised in the PRD (FR009-FR011).

## Objectives and Scope

**In Scope:**

- `/compare` route with URL state management (`?models=id1,id2,id3`)
- Horizontal model cards (2-5 models) with remove functionality
- Comparison table component with vertical attribute alignment
- Benchmark comparison section with category grouping and highlighting
- Chart.js integration with grouped bar charts for benchmarks
- Metric selector for dynamic chart customization
- Pricing comparison visualization (stacked/grouped bars)
- Capabilities comparison matrix (checkmark grid)
- Chart type switcher (bar chart variations)
- Interactive chart features (hover tooltips, legend toggling)
- CSV export functionality for comparison data
- Add/remove models dynamically from comparison page
- Batch API fetch optimization for selected models
- Performance optimization (<2s load for 5 models, smooth interactions)

**Out of Scope (Future Phases):**

- Radar charts, scatter plots (mentioned as future in Story 5.10)
- Chart zoom/pan (optional for MVP per Story 5.11)
- PNG chart export (future enhancement per Story 5.12)
- Comparison page for >5 models (limit enforced)
- Advanced chart annotations or data point labeling
- Comparison history/saved comparisons (Phase 2 feature)

## System Architecture Alignment

Epic 5 aligns with the **hexagonal architecture** established in `solution-architecture.md`:

**Frontend Components** (Presentation Layer):
- `ComparisonPage` - Main comparison route component
- `ModelCard` - Reusable model summary card
- `ComparisonTable` - Attribute comparison table
- `BenchmarkChart` - Chart.js wrapper for benchmark visualization
- `CapabilityMatrix` - Grid-based capability comparison
- Located in: `apps/web/src/pages/ComparisonPage/` and `apps/web/src/components/comparison/`

**Backend Services** (Application Layer):
- `ModelComparisonService` - Orchestrates batch model fetching and comparison data preparation
- `ChartDataService` - Transforms benchmark data into Chart.js-compatible format
- Located in: `services/backend/LlmTokenPrice.Application/Services/`

**Data Access** (Infrastructure Layer):
- Leverages existing `IModelRepository` for batch fetching
- No new database tables required (reads from `models`, `capabilities`, `benchmarks`, `benchmark_scores`)
- Redis caching for comparison data (30min TTL)

**Integration Points:**
- **Chart.js** (v4.4.1): Client-side rendering, no backend integration
- **React Router** (v6.21.0): URL state management for shareable comparison links
- **Zustand** (v4.4.7): Comparison basket state synchronization with main table
- **TanStack Query** (v5.17.0): Caching comparison API responses

## Detailed Design

### Services and Modules

| Module/Service | Layer | Responsibility | Inputs | Outputs | Owner |
|---------------|-------|----------------|--------|---------|-------|
| **ComparisonPage** | Frontend | Main comparison route, orchestrates all comparison UI components | URL params (model IDs), Zustand basket state | Rendered comparison view | React Component |
| **ModelCard** | Frontend | Displays individual model summary card | `ModelDto` object | Card UI with name, provider, pricing, key capabilities | React Component |
| **ComparisonTable** | Frontend | Vertical attribute comparison table | Array of `ModelDto` objects | Table with aligned attributes, highlighted differences | React Component |
| **BenchmarkChart** | Frontend | Chart.js wrapper for benchmark visualization | `ChartDataDto` (datasets, labels, options) | Interactive bar chart with tooltips, legend | React Component |
| **CapabilityMatrix** | Frontend | Grid-based capability comparison | Array of `ModelDto.capabilities` | Checkmark grid showing feature support | React Component |
| **MetricSelector** | Frontend | Multi-select dropdown for benchmark selection | Available benchmarks, current selection | Selected benchmark IDs, triggers chart update | React Component |
| **ModelComparisonService** | Backend (Application) | Batch fetch models, prepare comparison data | Model IDs (List<Guid>) | `ComparisonResponseDto` with models + metadata | C# Service |
| **ChartDataService** | Backend (Application) | Transform benchmark data for Chart.js | Model IDs, selected benchmark IDs | `ChartDataDto` with datasets, labels, colors | C# Service |
| **useComparisonData** | Frontend | React Query hook for fetching comparison data | Model IDs from URL | `{ data, isLoading, error }` | Custom Hook |
| **comparisonStore** | Frontend | Zustand store for comparison UI state | Actions (add/remove model, toggle chart type) | Current comparison state | Zustand Store |

### Data Models and Contracts

**Backend DTOs (C#):**

```csharp
// Application/DTOs/ComparisonResponseDto.cs
public record ComparisonResponseDto
{
    public List<ModelDto> Models { get; init; } = new();
    public ComparisonMetadataDto Metadata { get; init; } = new();
}

public record ComparisonMetadataDto
{
    public int ModelCount { get; init; }
    public DateTime FetchedAt { get; init; }
    public bool AllModelsActive { get; init; }
    public List<string> AvailableBenchmarks { get; init; } = new();
}

// Application/DTOs/ChartDataDto.cs
public record ChartDataDto
{
    public List<string> Labels { get; init; } = new();  // Benchmark names
    public List<DatasetDto> Datasets { get; init; } = new();  // One per model
}

public record DatasetDto
{
    public string Label { get; init; } = string.Empty;  // Model name
    public List<decimal?> Data { get; init; } = new();  // Scores (nullable for missing)
    public string BackgroundColor { get; init; } = string.Empty;  // Hex color
    public string BorderColor { get; init; } = string.Empty;
}
```

**Frontend Types (TypeScript):**

```typescript
// src/types/comparison.ts
export interface ComparisonState {
  selectedModels: string[];  // Model IDs
  chartType: 'bar' | 'groupedBar';
  selectedMetrics: string[];  // Benchmark IDs
  addModel: (modelId: string) => void;
  removeModel: (modelId: string) => void;
  setChartType: (type: 'bar' | 'groupedBar') => void;
  toggleMetric: (benchmarkId: string) => void;
}

export interface ComparisonTableRow {
  attribute: string;
  category: 'pricing' | 'capabilities' | 'metadata';
  values: Record<string, string | number | boolean>;  // modelId -> value
  highlightBest?: boolean;
}

export interface ExportData {
  timestamp: string;
  models: ModelDto[];
  comparisonRows: ComparisonTableRow[];
}
```

**Database Models (No Changes):**

Epic 5 reads from existing tables:
- `models` - Model metadata
- `capabilities` - Model capabilities (1:1 with models)
- `benchmarks` - Benchmark definitions
- `benchmark_scores` - Scores (N:1 with models, N:1 with benchmarks)

### APIs and Interfaces

**Backend API Endpoints:**

```http
GET /api/models/compare?ids=guid1,guid2,guid3
```

**Purpose:** Batch fetch multiple models with all related data for comparison

**Request:**
- Query parameter: `ids` (comma-separated GUIDs, 2-5 models)
- Optional: `includeInactive=false` (default: false)

**Response (200 OK):**
```json
{
  "models": [
    {
      "id": "...",
      "name": "GPT-4 Turbo",
      "provider": "OpenAI",
      "inputPricePer1M": 10.00,
      "outputPricePer1M": 30.00,
      "capabilities": {
        "contextWindow": 128000,
        "maxOutput": 4096,
        "functionCalling": true,
        "vision": true,
        "streaming": true
      },
      "benchmarkScores": [
        { "benchmarkId": "...", "benchmarkName": "MMLU", "score": 86.4, "category": "reasoning" },
        { "benchmarkId": "...", "benchmarkName": "HumanEval", "score": 67.0, "category": "code" }
      ]
    }
  ],
  "metadata": {
    "modelCount": 3,
    "fetchedAt": "2025-01-17T10:30:00Z",
    "allModelsActive": true,
    "availableBenchmarks": ["MMLU", "HumanEval", "GSM8K", "HellaSwag", "MATH"]
  }
}
```

**Error Responses:**
- `400 Bad Request`: Invalid model IDs or >5 models requested
- `404 Not Found`: One or more model IDs not found
- `500 Internal Server Error`: Database/cache failure

**Caching Strategy:**
- Redis key: `cache:comparison:{sorted_model_ids}:v1`
- TTL: 30 minutes
- Invalidation: On any model update (admin panel)

---

```http
GET /api/models/chart-data?ids=guid1,guid2&benchmarks=id1,id2,id3
```

**Purpose:** Get Chart.js-formatted data for selected models and benchmarks

**Request:**
- Query parameter: `ids` (comma-separated model GUIDs)
- Query parameter: `benchmarks` (comma-separated benchmark IDs, optional - defaults to top 5)

**Response (200 OK):**
```json
{
  "labels": ["MMLU", "HumanEval", "GSM8K"],
  "datasets": [
    {
      "label": "GPT-4 Turbo",
      "data": [86.4, 67.0, 92.0],
      "backgroundColor": "#10b981",
      "borderColor": "#059669"
    },
    {
      "label": "Claude 3 Opus",
      "data": [86.8, 84.9, 95.0],
      "backgroundColor": "#3b82f6",
      "borderColor": "#2563eb"
    }
  ]
}
```

**Frontend API Client:**

```typescript
// src/api/comparison.ts
export const comparisonApi = {
  fetchComparison: async (modelIds: string[]): Promise<ComparisonResponseDto> => {
    const ids = modelIds.join(',');
    const response = await axios.get(`/api/models/compare?ids=${ids}`);
    return response.data;
  },

  fetchChartData: async (modelIds: string[], benchmarkIds?: string[]): Promise<ChartDataDto> => {
    const ids = modelIds.join(',');
    const benchmarks = benchmarkIds ? `&benchmarks=${benchmarkIds.join(',')}` : '';
    const response = await axios.get(`/api/models/chart-data?ids=${ids}${benchmarks}`);
    return response.data;
  }
};
```

### Workflows and Sequencing

**User Flow: Navigate to Comparison Page**

```
1. User selects 2-5 models in main table (Epic 3)
2. User clicks "Compare Selected" button in comparison basket
3. Navigation: Router pushes to `/compare?models=id1,id2,id3`
4. ComparisonPage component mounts
5. useComparisonData hook extracts model IDs from URL
6. TanStack Query fetches: GET /api/models/compare?ids=...
7. Page renders loading skeleton
8. On data received:
   - ModelCard components render horizontally
   - ComparisonTable populates with attributes
   - BenchmarkChart renders with default top 5 benchmarks
   - CapabilityMatrix displays checkmark grid
9. User can interact: change metrics, toggle chart types, add/remove models
```

**Sequence Diagram: Fetch Comparison Data**

```
User → ComparisonPage: Navigate to /compare?models=1,2,3
ComparisonPage → useComparisonData: Extract model IDs from URL
useComparisonData → TanStack Query: Initiate fetch
TanStack Query → Axios: GET /api/models/compare?ids=1,2,3
Axios → Backend API: HTTP Request
Backend API → Redis Cache: Check cache:comparison:1,2,3:v1
Redis Cache --X Backend API: Cache miss
Backend API → ModelComparisonService: FetchComparisonData(ids)
ModelComparisonService → ModelRepository: GetByIdsAsync([1,2,3])
ModelRepository → PostgreSQL: SELECT * FROM models WHERE id IN (...)
PostgreSQL --> ModelRepository: Model rows
ModelRepository --> ModelComparisonService: List<Model>
ModelComparisonService → ChartDataService: PrepareChartData(models)
ChartDataService --> ModelComparisonService: ChartDataDto
ModelComparisonService --> Backend API: ComparisonResponseDto
Backend API → Redis Cache: SET cache:comparison:1,2,3:v1 (TTL 30min)
Backend API --> Axios: JSON Response
Axios --> TanStack Query: Response data
TanStack Query --> useComparisonData: Cached data
useComparisonData --> ComparisonPage: Render components
```

**Data Flow: Add Model to Comparison**

```
1. User clicks "Add Model" button on comparison page
2. Modal opens with searchable model list
3. User selects model (e.g., model ID 4)
4. comparisonStore.addModel(4) called
5. Router updates URL: /compare?models=1,2,3,4
6. React Router detects URL change
7. ComparisonPage re-renders
8. useComparisonData hook detects new IDs
9. TanStack Query fetches updated data
10. Components re-render with 4 models
```

**Export Workflow:**

```
1. User clicks "Export" button
2. Frontend compiles ComparisonTableRow[] from current state
3. CSV generation (client-side):
   - Header: Model1, Model2, Model3
   - Rows: Attribute, Value1, Value2, Value3
4. Blob creation with MIME type text/csv
5. Download triggered with filename: comparison-2025-01-17.csv
6. Success toast displayed
```

## Non-Functional Requirements

### Performance

**Target Metrics (from PRD FR032-FR035):**

| Metric | Target | Measurement Method |
|--------|--------|-------------------|
| **Page Load Time** | <2 seconds for 5 models | Lighthouse Performance Score >90 |
| **Chart Rendering** | <1 second | Performance.now() delta from data received to chart painted |
| **Smooth Interactions** | No visible jank when adding/removing models | Maintain 60fps during transitions |
| **API Response Time** | <300ms for batch fetch | Server-side logging + APM monitoring |
| **Cache Hit Ratio** | >80% for comparison requests | Redis metrics tracking |

**Performance Optimization Strategies:**

1. **Batch API Fetching:** Single request for multiple models (`/api/models/compare?ids=1,2,3`) instead of N individual requests
2. **Client-Side Caching:** TanStack Query with 5min stale time prevents unnecessary refetches
3. **Server-Side Caching:** Redis cache with 30min TTL for comparison data
4. **Lazy Loading:** Chart components render below fold, only mount when visible (Intersection Observer)
5. **Debounced Chart Updates:** Metric selector changes debounced 300ms to prevent excessive re-renders
6. **Memoization:** React.memo() on ModelCard, ComparisonTable row components
7. **Virtual Scrolling:** If comparison table exceeds 50 rows (unlikely for 5 models), implement react-window
8. **Bundle Optimization:** Chart.js tree-shaken to include only bar chart module (~30KB vs full 180KB)

**Performance Testing:**

```typescript
// Story 5.14 acceptance criteria verification
describe('Comparison Page Performance', () => {
  it('loads 5 models in <2 seconds', async () => {
    const start = performance.now();
    await page.goto('/compare?models=1,2,3,4,5');
    await page.waitForSelector('[data-testid="comparison-chart"]');
    const duration = performance.now() - start;
    expect(duration).toBeLessThan(2000);
  });

  it('renders chart in <1 second', async () => {
    // Measure from data received to canvas painted
    const chartRenderTime = await page.evaluate(() => {
      return window.chartRenderDuration; // Set by BenchmarkChart component
    });
    expect(chartRenderTime).toBeLessThan(1000);
  });
});

### Security

**Threat Model:**

Epic 5 is a **read-only public feature** with minimal security concerns. No authentication required, no user data stored.

**Security Considerations:**

1. **Input Validation:**
   - Model IDs validated as GUIDs (reject malformed input)
   - Maximum 5 models enforced server-side (prevent resource exhaustion)
   - Benchmark IDs validated against whitelist (prevent injection)

2. **Rate Limiting:**
   - API endpoint rate-limited: 100 requests/minute per IP (prevent abuse)
   - Implemented via ASP.NET Core middleware (AspNetCoreRateLimit package)

3. **XSS Prevention:**
   - React auto-escapes all rendered data (model names, providers)
   - Chart.js labels sanitized (no HTML rendering)
   - CSV export uses proper escaping (=, +, -, @ prefixes handled)

4. **CORS Configuration:**
   - Comparison API follows same CORS policy as main table API (Epic 1)
   - Allow frontend origin only (no wildcard)

5. **Cache Poisoning Prevention:**
   - Redis cache keys include version suffix `:v1` (invalidate on schema changes)
   - Cache keys sorted by model ID (prevent duplicate cache entries)

**Security Testing:**

```csharp
[Fact]
public async Task CompareEndpoint_RejectsMoreThan5Models()
{
    var ids = string.Join(',', Enumerable.Range(1, 6).Select(_ => Guid.NewGuid()));
    var response = await _client.GetAsync($"/api/models/compare?ids={ids}");

    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    var error = await response.Content.ReadAsAsync<ErrorDto>();
    Assert.Contains("Maximum 5 models", error.Message);
}

[Fact]
public async Task CompareEndpoint_ValidatesGuidFormat()
{
    var response = await _client.GetAsync("/api/models/compare?ids=invalid,also-invalid");

    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
}
```

### Reliability/Availability

**Availability Target:** 99.5% uptime (inline with overall platform SLA)

**Graceful Degradation Strategy:**

1. **Cache Failure:**
   - If Redis unavailable → Fall back to direct database queries
   - Log warning, continue serving requests (slower but functional)
   - Display "Reduced performance mode" banner (optional)

2. **Partial Model Data:**
   - If 1 of 5 models fails to load → Display 4 models with warning message
   - If >50% fail → Show error page with "Try fewer models" suggestion

3. **Chart Rendering Failure:**
   - Catch Chart.js errors → Display comparison table only
   - Show "Chart unavailable" message with fallback to table view

4. **Missing Benchmark Data:**
   - Display "N/A" for missing scores in table
   - Exclude missing data points from charts (don't render as 0)
   - Show tooltip: "Model X not tested on Benchmark Y"

**Error Handling:**

```typescript
// src/pages/ComparisonPage/ComparisonPage.tsx
export const ComparisonPage = () => {
  const { modelIds } = useComparisonParams();
  const { data, isLoading, error } = useComparisonData(modelIds);

  if (error) {
    return (
      <ErrorState
        title="Failed to load comparison"
        message={error.message}
        actions={[
          { label: 'Retry', onClick: () => queryClient.invalidateQueries(['comparison']) },
          { label: 'Back to Table', onClick: () => navigate('/') }
        ]}
      />
    );
  }

  // Partial success: Some models loaded
  if (data && data.models.length < modelIds.length) {
    toast.warning(`Only ${data.models.length} of ${modelIds.length} models loaded`);
  }

  return <>{/* Render comparison */}</>;
};
```

**Recovery Mechanisms:**

- TanStack Query automatic retry (3 attempts with exponential backoff)
- Browser back button returns to stable state (main table)
- URL manipulation errors redirect to `/compare` with error message

### Observability

**Logging Requirements:**

```csharp
// Backend: Structured logging with Serilog
public class ModelComparisonService
{
    private readonly ILogger<ModelComparisonService> _logger;

    public async Task<ComparisonResponseDto> FetchComparisonDataAsync(List<Guid> modelIds)
    {
        _logger.LogInformation(
            "Fetching comparison data for {ModelCount} models: {ModelIds}",
            modelIds.Count,
            string.Join(",", modelIds)
        );

        var sw = Stopwatch.StartNew();

        // ... fetch logic ...

        _logger.LogInformation(
            "Comparison data fetched in {ElapsedMs}ms. Cache hit: {CacheHit}",
            sw.ElapsedMilliseconds,
            cacheHit
        );

        return result;
    }
}
```

**Metrics to Track:**

| Metric | Type | Purpose | Alert Threshold |
|--------|------|---------|-----------------|
| `comparison.requests.total` | Counter | Total comparison requests | N/A |
| `comparison.requests.duration` | Histogram | API response time distribution | p95 > 500ms |
| `comparison.cache.hit_ratio` | Gauge | Redis cache effectiveness | < 70% |
| `comparison.models.per_request` | Histogram | Distribution of model counts | N/A |
| `comparison.chart.render_time` | Histogram | Client-side chart rendering | p95 > 2s |
| `comparison.export.total` | Counter | CSV export usage | N/A |

**Frontend Monitoring:**

```typescript
// src/hooks/useComparisonData.ts
export const useComparisonData = (modelIds: string[]) => {
  return useQuery({
    queryKey: ['comparison', modelIds.sort().join(',')],
    queryFn: async () => {
      const start = performance.now();
      const data = await comparisonApi.fetchComparison(modelIds);
      const duration = performance.now() - start;

      // Send to analytics
      analytics.track('comparison_loaded', {
        model_count: modelIds.length,
        duration_ms: duration,
        cache_hit: data.metadata.fromCache,
      });

      return data;
    },
    staleTime: 5 * 60 * 1000, // 5 minutes
  });
};
```

**Tracing:**

- Distributed tracing with correlation IDs (inherit from Epic 1 setup)
- Trace spans: `API Request → Service → Repository → Database`
- Chart rendering tracked with User Timing API:

```typescript
performance.mark('chart-start');
// Chart.js render
performance.mark('chart-end');
performance.measure('chart-render', 'chart-start', 'chart-end');
```

## Dependencies and Integrations

{{dependencies_integrations}}

## Acceptance Criteria (Authoritative)

{{acceptance_criteria}}

## Traceability Mapping

{{traceability_mapping}}

## Risks, Assumptions, Open Questions

{{risks_assumptions_questions}}

## Test Strategy Summary

{{test_strategy}}
