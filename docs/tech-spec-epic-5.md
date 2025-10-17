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

**Frontend Dependencies (package.json):**

| Package | Version | Purpose | Epic 5 Usage |
|---------|---------|---------|--------------|
| **chart.js** | ^4.5.1 | Chart rendering library | Core dependency - all benchmark and pricing visualizations |
| **react-chartjs-2** | ^5.3.0 | React wrapper for Chart.js | Provides React components: `<Bar />`, `<Line />` |
| **@tanstack/react-query** | ^5.90.5 | Server state management | Caching comparison API responses, automatic refetching |
| **zustand** | ^5.0.8 | Client state management | Comparison basket state, selected metrics, chart type |
| **react-router-dom** | ^7.9.4 | Routing | `/compare` route, URL state management (`?models=...`) |
| **axios** | ^1.12.2 | HTTP client | API calls to `/api/models/compare` |
| **lucide-react** | ^0.546.0 | Icon library | Checkmark icons (CapabilityMatrix), UI controls |
| **date-fns** | ^4.1.0 | Date formatting | Format timestamps in metadata |

**Backend Dependencies (.csproj - Inherited from Epic 1):**

```xml
<ItemGroup>
  <!-- Core Framework -->
  <PackageReference Include="Microsoft.AspNetCore.App" />

  <!-- Database -->
  <PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="8.0.0" />

  <!-- Caching -->
  <PackageReference Include="StackExchange.Redis" Version="2.7.10" />

  <!-- Logging -->
  <PackageReference Include="Serilog.AspNetCore" Version="8.0.0" />
</ItemGroup>
```

**New Dependencies Required for Epic 5:**

None - All required packages already installed in Epic 1/3.

**Integration Points:**

1. **Chart.js Configuration:**
   ```typescript
   // Story 5.5: Chart.js setup
   import {
     Chart as ChartJS,
     CategoryScale,
     LinearScale,
     BarElement,
     Title,
     Tooltip,
     Legend
   } from 'chart.js';

   ChartJS.register(
     CategoryScale,
     LinearScale,
     BarElement,
     Title,
     Tooltip,
     Legend
   );
   ```

2. **React Query Integration:**
   ```typescript
   // src/hooks/useComparisonData.ts
   import { useQuery } from '@tanstack/react-query';

   export const useComparisonData = (modelIds: string[]) => {
     return useQuery({
       queryKey: ['comparison', modelIds.sort().join(',')],
       queryFn: () => comparisonApi.fetchComparison(modelIds),
       enabled: modelIds.length >= 2 && modelIds.length <= 5,
       staleTime: 5 * 60 * 1000, // 5 minutes
       gcTime: 10 * 60 * 1000,   // 10 minutes garbage collection
     });
   };
   ```

3. **Zustand Store Integration:**
   ```typescript
   // src/store/comparisonStore.ts
   import { create } from 'zustand';
   import { persist } from 'zustand/middleware';

   export const useComparisonStore = create(
     persist(
       (set) => ({
         selectedMetrics: ['mmlu', 'humaneval', 'gsm8k', 'hellaswag', 'math'], // Default top 5
         chartType: 'bar' as const,
         toggleMetric: (benchmarkId: string) =>
           set((state) => ({
             selectedMetrics: state.selectedMetrics.includes(benchmarkId)
               ? state.selectedMetrics.filter(id => id !== benchmarkId)
               : [...state.selectedMetrics, benchmarkId]
           })),
         setChartType: (type) => set({ chartType: type }),
       }),
       { name: 'comparison-preferences' }
     )
   );
   ```

4. **CSV Export (Client-Side, No Dependencies):**
   ```typescript
   // src/utils/exportCSV.ts
   export const exportComparisonToCSV = (data: ExportData) => {
     const csv = generateCSV(data);
     const blob = new Blob([csv], { type: 'text/csv;charset=utf-8;' });
     const url = URL.createObjectURL(blob);
     const link = document.createElement('a');
     link.href = url;
     link.download = `comparison-${format(new Date(), 'yyyy-MM-dd')}.csv`;
     link.click();
     URL.revokeObjectURL(url);
   };
   ```

**External Service Dependencies:**

- **Redis (Upstash)**: Caching layer (inherited from Epic 1, Story 1.5)
- **PostgreSQL**: Data source (inherited from Epic 1, Story 1.3)
- No new external services required for Epic 5

## Acceptance Criteria (Authoritative)

**AC-5.1: Comparison Page Route and URL State**
- `/compare` route accessible and renders without errors
- URL accepts `?models=id1,id2,id3` query parameters (2-5 GUIDs)
- Invalid URLs redirect to error state with clear message
- Browser back/forward buttons update comparison state correctly
- URL updates when models added/removed dynamically

**AC-5.2: Model Cards Display**
- 2-5 model cards render horizontally (or stacked on mobile <768px)
- Each card displays: name, provider, input/output pricing, context window, key capabilities
- Cards have equal width for vertical alignment
- Remove button (X) on each card removes model from comparison and updates URL
- Empty placeholder shown if <2 models selected with "Add Model" prompt

**AC-5.3: Comparison Table Structure**
- Table displays attributes as rows, models as columns
- Rows include: Provider, Input Price, Output Price, Context Window, Max Output, all capability flags, key benchmarks
- Vertical alignment maintained across all columns
- Differences highlighted: cheapest price (green text), highest score (green text)
- Table scrollable if >10 attribute rows

**AC-5.4: Benchmark Comparison Section**
- All available benchmarks listed with scores for each selected model
- Benchmarks grouped by category with collapsible sections (Reasoning, Code, Math, Language, Multimodal)
- Highest score in each row highlighted (bold + green)
- Missing scores displayed as "N/A" (not blank or 0)
- Test date and source URL shown if available

**AC-5.5: Chart.js Integration**
- Chart.js library loaded and initialized without console errors
- Reusable `BenchmarkChart` component created
- Chart responsive (adapts to container width)
- Chart renders on mobile devices without horizontal scroll

**AC-5.6: Benchmark Bar Chart**
- Grouped bar chart displays benchmark scores (X-axis: benchmarks, Y-axis: scores)
- One bar per model for each benchmark (color-coded by model)
- Top 5 benchmarks shown by default
- Legend identifies which color represents which model
- Chart readable on all screen sizes (labels don't overlap)

**AC-5.7: Metric Selector**
- Multi-select dropdown above chart lists all available benchmarks
- Default selection: top 5 key benchmarks (MMLU, HumanEval, GSM8K, HellaSwag, MATH)
- Selecting/deselecting benchmark updates chart in real-time (<300ms)
- "Select All" / "Deselect All" buttons functional
- Selection persisted in sessionStorage

**AC-5.8: Pricing Comparison Visualization**
- Pricing chart shows input price, output price, total for each model
- Bars color-coded (input: blue, output: green)
- Cheapest model highlighted with border or badge
- Cost difference vs cheapest shown as percentage label

**AC-5.9: Capabilities Matrix**
- Grid/table with capabilities as rows, models as columns
- Checkmark icon (✓) if supported, X icon or empty if not
- Green checkmark, gray X for visual clarity
- Capabilities: Function Calling, Vision, Audio, Streaming, JSON Mode

**AC-5.10: Chart Type Switcher**
- Toggle buttons: "Bar Chart" (default), "Grouped Bar"
- Clicking button re-renders chart in selected type
- Chart type persists during session
- Smooth transition animation (<500ms)

**AC-5.11: Chart Interactivity**
- Hovering over bar shows tooltip with exact value (model name, benchmark name, score)
- Clicking legend item toggles model visibility in chart
- Smooth transitions when data changes (300ms)
- Tooltips positioned to avoid screen edges

**AC-5.12: CSV Export Functionality**
- "Export" button triggers CSV download
- CSV format: Header row (Model1, Model2, Model3...), Data rows (Attribute, Value1, Value2, Value3...)
- Filename: `comparison-YYYY-MM-DD.csv`
- Success toast message confirms export
- CSV opens correctly in Excel/Google Sheets

**AC-5.13: Dynamic Model Management**
- "Add Model" button opens searchable modal
- Modal lists all available models (excludes already selected)
- Search filters models by name/provider (case-insensitive)
- Selecting model adds to comparison (max 5 enforced)
- URL updates when model added/removed

**AC-5.14: Performance Optimization**
- Comparison page loads in <2 seconds for 5 models (Lighthouse score >90)
- Chart renders in <1 second after data received
- No visible jank when adding/removing models (60fps maintained)
- API batch fetch completes in <300ms
- Interactions feel instant (debounced updates <100ms)

## Traceability Mapping

| AC | PRD Req | Spec Section | Component/API | Test Idea |
|----|---------|--------------|---------------|-----------|
| **AC-5.1** | FR009 (select 2-5 models for comparison) | Workflows & Sequencing | `ComparisonPage`, React Router | E2E: Navigate from table with 3 selected models, verify URL |
| **AC-5.2** | FR009 | Services & Modules | `ModelCard`, `useComparisonData` | Unit: Render 5 cards, verify equal widths. E2E: Click X, verify model removed |
| **AC-5.3** | FR009 | Data Models | `ComparisonTable`, `ComparisonTableRow` | Integration: Verify vertical alignment, highlight cheapest price |
| **AC-5.4** | FR011 (benchmark visualization) | Services & Modules | `BenchmarkChart`, benchmark grouping logic | Unit: Group benchmarks by category, verify collapsible sections |
| **AC-5.5** | FR011 | Dependencies | Chart.js registration | Unit: Verify Chart.js initializes, no console errors |
| **AC-5.6** | FR011 | APIs & Interfaces | `BenchmarkChart`, `ChartDataDto` | Visual regression: Verify grouped bars, legend colors |
| **AC-5.7** | FR011 | Services & Modules | `MetricSelector`, `useComparisonStore` | Integration: Toggle metrics, verify chart updates <300ms |
| **AC-5.8** | FR009 (pricing comparison) | Services & Modules | Pricing chart component | Unit: Verify cost calculation, cheapest model badge |
| **AC-5.9** | FR009 (capability comparison) | Services & Modules | `CapabilityMatrix` | Visual regression: Verify checkmark grid, color coding |
| **AC-5.10** | FR011 | Services & Modules | Chart type switcher UI | E2E: Toggle chart types, verify persistence |
| **AC-5.11** | FR011 | Dependencies | Chart.js tooltip/legend config | E2E: Hover over bar, click legend, verify interactions |
| **AC-5.12** | (Export enhancement) | Workflows & Sequencing | CSV export utility | Integration: Export CSV, parse file, verify structure |
| **AC-5.13** | FR009 | Workflows & Sequencing | Add Model modal, URL state | E2E: Add model via modal, verify URL updates, max 5 enforced |
| **AC-5.14** | FR032 (page load <2s), FR035 (chart <1s) | NFR Performance | All components, batch API | Performance: Lighthouse audit, chart render timing |

**Requirements Coverage:**

- **FR009 (Model Comparison)**: AC-5.1, AC-5.2, AC-5.3, AC-5.8, AC-5.9, AC-5.13
- **FR011 (Visualization)**: AC-5.4, AC-5.5, AC-5.6, AC-5.7, AC-5.10, AC-5.11
- **FR032 (Load Time)**: AC-5.14
- **FR035 (Chart Rendering)**: AC-5.14
- **Export (Enhancement)**: AC-5.12

**Story-to-AC Mapping:**

| Story | Acceptance Criteria Covered |
|-------|----------------------------|
| Story 5.1 | AC-5.1 (route, URL state) |
| Story 5.2 | AC-5.2 (model cards) |
| Story 5.3 | AC-5.3 (comparison table) |
| Story 5.4 | AC-5.4 (benchmark section) |
| Story 5.5 | AC-5.5 (Chart.js setup) |
| Story 5.6 | AC-5.6 (bar chart) |
| Story 5.7 | AC-5.7 (metric selector) |
| Story 5.8 | AC-5.8 (pricing visualization) |
| Story 5.9 | AC-5.9 (capability matrix) |
| Story 5.10 | AC-5.10 (chart type switcher) |
| Story 5.11 | AC-5.11 (chart interactions) |
| Story 5.12 | AC-5.12 (CSV export) |
| Story 5.13 | AC-5.13 (add/remove models) |
| Story 5.14 | AC-5.14 (performance) |

## Risks, Assumptions, Open Questions

**Risks:**

| ID | Risk | Likelihood | Impact | Mitigation |
|----|------|------------|--------|------------|
| **R-5.1** | Chart.js bundle size increases page load time | Medium | Medium | Tree-shake to include only bar chart module (~30KB vs 180KB). Lazy load chart component below fold. |
| **R-5.2** | Chart rendering slow with many benchmarks (>20) | Low | High | Default to top 5 benchmarks. Implement debounced updates (300ms). Add "Show More" progressive disclosure. |
| **R-5.3** | Mobile chart readability poor with >3 models | Medium | Medium | Horizontal scroll for >3 models on mobile. Reduce font sizes on small screens. Consider card-based layout for mobile. |
| **R-5.4** | CSV export fails in Safari due to Blob API quirks | Low | Low | Test across browsers (Chrome, Firefox, Safari). Use polyfill if needed. Fallback to data URI method. |
| **R-5.5** | URL length exceeds browser limit with 5 model GUIDs | Very Low | Medium | Use short IDs if possible. Base64 encode model IDs. Test with max-length URLs (2048 chars). |
| **R-5.6** | Missing benchmark data creates confusing empty charts | Medium | Medium | Hide benchmarks with >50% missing data. Display "N/A" tooltip. Show data completeness indicator. |

**Assumptions:**

| ID | Assumption | Validation Method |
|----|------------|-------------------|
| **A-5.1** | Users primarily compare 2-3 models (not 5) | Analytics tracking: 80% of comparisons use 2-3 models |
| **A-5.2** | Chart.js performance adequate for 5 models × 5 benchmarks | Performance testing: Chart renders in <1s on mid-tier devices |
| **A-5.3** | All models have at least 3 benchmark scores | Data quality check: 95% of models have ≥3 scores in production data |
| **A-5.4** | Users understand grouped bar charts without tutorial | User testing: 80% of users correctly interpret chart without help |
| **A-5.5** | CSV export sufficient (PNG/PDF not needed for MVP) | User feedback: <20% request image export in first 3 months |
| **A-5.6** | Comparison basket state syncs correctly between table and comparison page | Integration testing: State consistency verified across navigation |

**Open Questions:**

| ID | Question | Owner | Deadline | Resolution |
|----|----------|-------|----------|------------|
| **Q-5.1** | Should comparison page show historical price changes for selected models? | PM | Before Story 5.8 | **Deferred to Phase 2** - Requires TimescaleDB price history (not in Epic 1) |
| **Q-5.2** | Do we need radar charts for capability visualization? | UX Designer | Before Story 5.10 | **No for MVP** - Bar charts sufficient, radar charts added in future if user feedback requests |
| **Q-5.3** | Should users be able to save/bookmark comparisons? | PM | Before Story 5.13 | **Phase 2 feature** - Requires user accounts. MVP uses URL sharing only. |
| **Q-5.4** | What color palette for Chart.js? Match TailwindCSS theme? | UX Designer | Before Story 5.6 | **Use TailwindCSS colors** - Green-500, Blue-500, Purple-500, Orange-500, Pink-500 for consistency |
| **Q-5.5** | Should charts update live if model data changes (admin updates)? | Architect | Before Story 5.14 | **No live updates** - TanStack Query refetch on window focus (5min stale time) is sufficient |

## Test Strategy Summary

**Testing Pyramid for Epic 5:**

```
         /\
        /  \  E2E (5%)
       /____\
      /      \  Integration (25%)
     /________\
    /          \  Unit (70%)
   /____________\
```

**Unit Tests (70% coverage target):**

| Component | Test Cases | Tools |
|-----------|------------|-------|
| **useComparisonData hook** | Query key generation, enabled condition, cache behavior | Vitest, React Testing Library |
| **comparisonStore** | Add/remove model, toggle metric, persist state | Vitest, Zustand testing utilities |
| **CSV export utility** | Header generation, row formatting, special char escaping | Vitest |
| **Chart data transformer** | Transform `ModelDto[]` to `ChartDataDto`, handle missing scores | Vitest |
| **MetricSelector** | Multi-select logic, "Select All" functionality | Vitest, React Testing Library |
| **CapabilityMatrix** | Grid rendering, checkmark logic for true/false/null | Vitest, React Testing Library |

**Example Unit Test:**

```typescript
// src/utils/chartDataTransformer.test.ts
import { describe, it, expect } from 'vitest';
import { transformToChartData } from './chartDataTransformer';

describe('transformToChartData', () => {
  it('creates grouped bar chart data from models and benchmarks', () => {
    const models = [
      { id: '1', name: 'GPT-4', benchmarkScores: [{ benchmarkId: 'mmlu', score: 86.4 }] },
      { id: '2', name: 'Claude 3', benchmarkScores: [{ benchmarkId: 'mmlu', score: 86.8 }] },
    ];
    const selectedBenchmarks = ['mmlu'];

    const result = transformToChartData(models, selectedBenchmarks);

    expect(result.labels).toEqual(['MMLU']);
    expect(result.datasets).toHaveLength(2);
    expect(result.datasets[0].data).toEqual([86.4]);
    expect(result.datasets[1].data).toEqual([86.8]);
  });

  it('handles missing benchmark scores with null', () => {
    const models = [
      { id: '1', name: 'GPT-4', benchmarkScores: [] }, // Missing MMLU
    ];
    const selectedBenchmarks = ['mmlu'];

    const result = transformToChartData(models, selectedBenchmarks);

    expect(result.datasets[0].data).toEqual([null]);
  });
});
```

**Integration Tests (25% coverage target):**

| Scenario | What to Test | Tools |
|----------|--------------|-------|
| **Comparison API integration** | Fetch comparison data, parse response, handle errors | Vitest, MSW (Mock Service Worker) |
| **Chart rendering** | BenchmarkChart receives data, renders canvas, updates on prop change | Vitest, React Testing Library, canvas mock |
| **URL state sync** | Navigate to `/compare?models=1,2`, verify models fetched, add model → URL updates | Vitest, React Router testing utils |
| **Comparison table highlighting** | Cheapest price highlighted green, highest score highlighted | Vitest, screen queries |

**Example Integration Test:**

```typescript
// src/pages/ComparisonPage/ComparisonPage.integration.test.tsx
import { render, screen, waitFor } from '@testing-library/react';
import { server } from '../../mocks/server';
import { http, HttpResponse } from 'msw';
import { ComparisonPage } from './ComparisonPage';

describe('ComparisonPage Integration', () => {
  it('fetches and displays comparison data', async () => {
    server.use(
      http.get('/api/models/compare', () => {
        return HttpResponse.json({
          models: [
            { id: '1', name: 'GPT-4', inputPricePer1M: 10.0 },
            { id: '2', name: 'Claude 3', inputPricePer1M: 15.0 },
          ],
          metadata: { modelCount: 2 }
        });
      })
    );

    render(<ComparisonPage />, { initialRoute: '/compare?models=1,2' });

    await waitFor(() => {
      expect(screen.getByText('GPT-4')).toBeInTheDocument();
      expect(screen.getByText('Claude 3')).toBeInTheDocument();
    });

    // Verify cheapest price highlighted
    const gpt4Price = screen.getByText('$10.00');
    expect(gpt4Price).toHaveClass('text-green-600');
  });
});
```

**E2E Tests (5% coverage target):**

| Critical Flow | Steps | Tool |
|---------------|-------|------|
| **Happy path comparison** | Select 3 models in table → Click Compare → Verify cards/table/chart render → Toggle metric → Export CSV | Playwright |
| **Add/remove models** | Start with 2 models → Add 3rd via modal → Remove 1st model → Verify URL updates | Playwright |
| **Chart interactions** | Hover over bar → Verify tooltip → Click legend → Verify model hidden | Playwright |
| **Performance verification** | Load comparison with 5 models → Measure page load time (<2s) → Measure chart render (<1s) | Playwright + Lighthouse |

**Example E2E Test:**

```typescript
// e2e/comparison.spec.ts
import { test, expect } from '@playwright/test';

test('full comparison workflow', async ({ page }) => {
  await page.goto('/');

  // Select 3 models in main table
  await page.getByTestId('model-checkbox-1').check();
  await page.getByTestId('model-checkbox-2').check();
  await page.getByTestId('model-checkbox-3').check();

  // Navigate to comparison
  await page.getByRole('button', { name: 'Compare Selected' }).click();
  await expect(page).toHaveURL(/\/compare\?models=/);

  // Verify page loaded
  await expect(page.getByTestId('model-card')).toHaveCount(3);
  await expect(page.getByTestId('comparison-table')).toBeVisible();
  await expect(page.getByTestId('benchmark-chart')).toBeVisible();

  // Interact with chart
  await page.getByTestId('metric-selector').click();
  await page.getByRole('option', { name: 'GSM8K' }).click();

  // Wait for chart update (debounced 300ms)
  await page.waitForTimeout(400);
  await expect(page.locator('canvas')).toBeVisible();

  // Export CSV
  await page.getByRole('button', { name: 'Export' }).click();
  const downloadPromise = page.waitForEvent('download');
  const download = await downloadPromise;
  expect(download.suggestedFilename()).toMatch(/comparison-\d{4}-\d{2}-\d{2}\.csv/);
});

test('comparison performance', async ({ page }) => {
  const start = Date.now();
  await page.goto('/compare?models=1,2,3,4,5');
  await page.waitForSelector('[data-testid="benchmark-chart"]');
  const loadTime = Date.now() - start;

  expect(loadTime).toBeLessThan(2000); // AC-5.14: <2 seconds
});
```

**Visual Regression Testing:**

- Tool: Playwright visual comparisons
- Snapshots: Chart rendering (grouped bars, legend, tooltips), Capability matrix (checkmarks)
- Run on: Chrome, Firefox, Safari
- Threshold: 5% pixel difference tolerance

**Coverage Goals:**

- **Overall:** 70%+ code coverage
- **Critical paths:** 90%+ (comparison data fetch, chart rendering, URL state)
- **Edge cases:** Missing data, API errors, >5 models attempted

**Test Execution:**

```bash
# Unit + Integration
pnpm test

# E2E
pnpm test:e2e

# Performance
pnpm test:perf

# Visual regression
pnpm test:visual
```
