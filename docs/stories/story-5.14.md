# Story 5.14: Optimize Comparison Page Performance

Status: Draft

## Story

As a developer and user,
I want the comparison page optimized for performance,
so that it loads quickly, responds smoothly, and provides an excellent user experience even with complex visualizations.

## Acceptance Criteria

1. Comparison data fetched in single optimized API call (batch fetch by model IDs)
2. React components optimized to prevent unnecessary re-renders
3. Code splitting and lazy loading for below-fold components (charts, tables)
4. Page load time <2 seconds for 5 models (complete render including charts)
5. Smooth interactions with no visual jank when adding/removing models

## Tasks / Subtasks

### Task Group 1: API Optimization - Batch Fetch (AC: #1)
- [ ] Verify GET `/api/models/compare` endpoint design
  - [ ] Already designed in Story 5.2 tech spec
  - [ ] Endpoint: `/api/models/compare?ids=id1,id2,id3`
  - [ ] Returns: ComparisonResponseDto with all model data
  - [ ] Single query fetches all models + benchmarks in one round trip
- [ ] Ensure backend uses batch query (not N+1)
  - [ ] SQL: `SELECT * FROM models WHERE id IN (id1, id2, id3)`
  - [ ] Not: 5 separate queries per model
  - [ ] Backend optimization (Story 5.2 implementation)
- [ ] Implement Redis caching on backend
  - [ ] Already designed in tech spec
  - [ ] Cache key: `cache:comparison:{sorted_model_ids}:v1`
  - [ ] TTL: 30 minutes
  - [ ] Cached response served instantly (<50ms vs ~200-500ms DB query)

### Task Group 2: React Component Memoization (AC: #2, #5)
- [ ] Memoize expensive components with React.memo()
  - [ ] Components to memoize:
    ```typescript
    // Chart components (expensive renders)
    export const BenchmarkBarChart = React.memo(BenchmarkBarChartComponent);
    export const PricingComparisonChart = React.memo(PricingComparisonChartComponent);
    export const CapabilitiesMatrix = React.memo(CapabilitiesMatrixComponent);

    // Table components
    export const ComparisonTable = React.memo(ComparisonTableComponent);
    export const BenchmarkComparisonSection = React.memo(BenchmarkComparisonSectionComponent);

    // UI components
    export const ModelCard = React.memo(ModelCardComponent);
    export const MetricSelector = React.memo(MetricSelectorComponent);
    export const ChartTypeSwitcher = React.memo(ChartTypeSwitcherComponent);
    ```
  - [ ] Custom equality function for complex props:
    ```typescript
    const arePropsEqual = (prevProps: Props, nextProps: Props) => {
      return (
        prevProps.models.length === nextProps.models.length &&
        prevProps.models.every((m, i) => m.id === nextProps.models[i].id)
      );
    };

    export const BenchmarkBarChart = React.memo(BenchmarkBarChartComponent, arePropsEqual);
    ```

### Task Group 3: useMemo and useCallback Optimization (AC: #2)
- [ ] Memoize computed values with useMemo
  - [ ] Chart data transformation:
    ```typescript
    const chartData = useMemo(
      () => buildBenchmarkChartData(models, selectedBenchmarks),
      [models, selectedBenchmarks]
    );

    const chartOptions = useMemo(
      () => mergeChartOptions(defaultOptions, customOptions),
      [customOptions]
    );

    const pricingData = useMemo(
      () => buildPricingChartData(models),
      [models]
    );
    ```
  - [ ] Expensive calculations:
    ```typescript
    const cheapestModel = useMemo(
      () => findCheapestModel(models),
      [models]
    );

    const benchmarkRows = useMemo(
      () => buildBenchmarkRows(models),
      [models]
    );
    ```
- [ ] Memoize callback functions with useCallback
  - [ ] Event handlers passed to child components:
    ```typescript
    const handleModelRemove = useCallback((modelId: string) => {
      const newIds = modelIds.filter(id => id !== modelId);
      navigate(`/compare?models=${newIds.join(',')}`);
    }, [modelIds, navigate]);

    const handleBenchmarkSelect = useCallback((benchmarkIds: string[]) => {
      setSelectedBenchmarks(benchmarkIds);
    }, []);
    ```
  - [ ] Prevents child component re-renders when parent re-renders

### Task Group 4: Code Splitting with React.lazy (AC: #3)
- [ ] Lazy load chart components (not visible above fold)
  - [ ] Charts appear below ModelCards and ComparisonTable
  - [ ] Split into separate bundles:
    ```typescript
    // In ComparisonPage.tsx
    import { lazy, Suspense } from 'react';

    const BenchmarkBarChart = lazy(() => import('@/components/charts/BenchmarkBarChart'));
    const PricingComparisonChart = lazy(() => import('@/components/charts/PricingComparisonChart'));
    const CapabilitiesMatrix = lazy(() => import('@/components/comparison/CapabilitiesMatrix'));

    // Render with Suspense boundary
    <Suspense fallback={<ChartSkeleton />}>
      <BenchmarkBarChart models={data.models} />
    </Suspense>

    <Suspense fallback={<ChartSkeleton />}>
      <PricingComparisonChart models={data.models} />
    </Suspense>

    <Suspense fallback={<CapabilitiesMatrixSkeleton />}>
      <CapabilitiesMatrix models={data.models} />
    </Suspense>
    ```
  - [ ] Benefits: Smaller initial bundle, faster first paint
- [ ] Create loading skeletons for lazy components
  - [ ] ChartSkeleton: Gray rectangle with `animate-pulse`
  - [ ] CapabilitiesMatrixSkeleton: Table skeleton
  - [ ] Match component dimensions to prevent layout shift

### Task Group 5: Bundle Size Analysis and Optimization (AC: #4)
- [ ] Analyze bundle size with Vite
  - [ ] Build command: `pnpm run build`
  - [ ] Vite outputs bundle sizes automatically
  - [ ] Check: `dist/assets/*.js` file sizes
  - [ ] Target: Main bundle <300KB gzipped
- [ ] Optimize heavy dependencies
  - [ ] Chart.js: Already tree-shaken (Story 5.5)
  - [ ] Lucide icons: Import only used icons
    ```typescript
    // ❌ Don't import all
    import * as Icons from 'lucide-react';

    // ✅ Import specific icons
    import { Download, Plus, Check, X } from 'lucide-react';
    ```
  - [ ] Date-fns: Import only used functions
    ```typescript
    import { format } from 'date-fns';  // Not entire library
    ```
- [ ] Check for duplicate dependencies
  - [ ] Run: `pnpm why <package-name>`
  - [ ] Identify version conflicts
  - [ ] Deduplicate in package.json if needed

### Task Group 6: Image and Asset Optimization (AC: #4)
- [ ] No images in comparison page (charts are canvas/SVG)
  - [ ] Chart.js uses canvas (no image assets)
  - [ ] Icons are SVG (lucide-react)
  - [ ] No optimization needed for MVP
- [ ] Future: Optimize provider logos (if added)
  - [ ] Use WebP format
  - [ ] Lazy load below-fold logos
  - [ ] SVG for simple logos (better than PNG)

### Task Group 7: Virtual Scrolling for Large Lists (Optional) (AC: #5)
- [ ] Benchmark comparison section (many benchmarks)
  - [ ] Current: Render all rows (max ~20 benchmarks)
  - [ ] 20 rows × 5 models = 100 cells (fast, no virtualization needed)
  - [ ] Future: If >100 benchmarks, use react-window for virtualization
- [ ] Model selector modal (many models)
  - [ ] Current: Render all available models (~50 models)
  - [ ] 50 rows × 200px = 10,000px (scrollable, fast)
  - [ ] Future: If >500 models, use react-window
- [ ] Not needed for MVP (dataset too small)

### Task Group 8: TanStack Query Optimization (AC: #1)
- [ ] Verify cache configuration
  - [ ] Already configured in Story 5.2 (useComparisonData hook)
  - [ ] Stale time: 5 minutes (data cached client-side)
  - [ ] Cache key: `['comparison', modelIds.sort().join(',')]`
  - [ ] Deduplication: Multiple components using same IDs = single request
- [ ] Prefetch comparison data (optional enhancement)
  - [ ] When user hovers over "Compare Selected" button (main table)
  - [ ] Prefetch: `queryClient.prefetchQuery(['comparison', selectedIds])`
  - [ ] Result: Instant load when navigating to comparison page
  - [ ] Not implemented in MVP (marginal gain)
- [ ] Background refetch on window focus
  - [ ] TanStack Query default: Refetch on window focus
  - [ ] Ensures data fresh when returning to tab
  - [ ] No code needed (default behavior)

### Task Group 9: Debouncing and Throttling (AC: #5)
- [ ] Debounce search input (already in Story 5.13)
  - [ ] Model selector search: 300ms debounce
  - [ ] Metric selector search: 300ms debounce (if added)
  - [ ] Prevents excessive filtering on every keystroke
- [ ] Throttle window resize events (for responsive charts)
  - [ ] Chart.js handles this internally
  - [ ] No custom throttling needed
- [ ] Debounce URL updates (optional)
  - [ ] If rapid model add/remove: Debounce navigate() calls
  - [ ] Prevents excessive history entries
  - [ ] Not needed for MVP (user actions are slow enough)

### Task Group 10: Reduce Rendering Work (AC: #2, #5)
- [ ] Avoid inline object/array creation in JSX
  - [ ] ❌ Bad (creates new object every render):
    ```typescript
    <Component style={{ padding: 10 }} />
    <Component items={[1, 2, 3]} />
    ```
  - [ ] ✅ Good (stable reference):
    ```typescript
    const style = { padding: 10 };
    const items = [1, 2, 3];
    <Component style={style} items={items} />
    ```
- [ ] Use keys for lists correctly
  - [ ] Always use stable, unique keys (model.id, not array index)
  - [ ] Helps React identify which items changed
  - [ ] Example:
    ```typescript
    {models.map(model => (
      <ModelCard key={model.id} model={model} />  // ✅ Stable key
      // NOT: key={index}  ❌ Causes re-renders
    ))}
    ```
- [ ] Avoid deep prop drilling
  - [ ] Use context or Zustand for deeply nested state
  - [ ] Already using Zustand for comparison state (Story 5.13)

### Task Group 11: Loading States and Perceived Performance (AC: #4)
- [ ] Show skeleton immediately (no blank screen)
  - [ ] ComparisonPage loading state:
    ```typescript
    if (isLoading) {
      return (
        <div className="max-w-7xl mx-auto px-4 py-8">
          <div className="grid grid-cols-3 gap-6">
            {[1, 2, 3].map(i => <ModelCardSkeleton key={i} />)}
          </div>
          <ComparisonTableSkeleton />
          <ChartSkeleton />
        </div>
      );
    }
    ```
  - [ ] Matches final layout (prevents layout shift)
- [ ] Progressive rendering (show content as it loads)
  - [ ] Cards render first (above fold)
  - [ ] Tables render next
  - [ ] Charts render last (lazy loaded)
- [ ] Optimistic updates (optional)
  - [ ] When adding model: Update UI immediately, refetch in background
  - [ ] If refetch fails: Rollback UI
  - [ ] Not implemented in MVP (adds complexity)

### Task Group 12: Performance Monitoring (AC: #4)
- [ ] Add performance measurement with Web Vitals
  - [ ] Install: `pnpm add web-vitals`
  - [ ] Track Core Web Vitals:
    - LCP (Largest Contentful Paint): <2.5s
    - FID (First Input Delay): <100ms
    - CLS (Cumulative Layout Shift): <0.1
  - [ ] Implementation:
    ```typescript
    // src/lib/reportWebVitals.ts
    import { onCLS, onFID, onLCP } from 'web-vitals';

    export const reportWebVitals = () => {
      onCLS(console.log);  // Or send to analytics
      onFID(console.log);
      onLCP(console.log);
    };

    // In main.tsx
    reportWebVitals();
    ```
- [ ] Measure component render times with React DevTools Profiler
  - [ ] Development: Use React DevTools Profiler extension
  - [ ] Production: Programmatic profiling (optional)
    ```typescript
    <Profiler id="BenchmarkChart" onRender={onRenderCallback}>
      <BenchmarkBarChart />
    </Profiler>
    ```
- [ ] Set performance budgets
  - [ ] Initial load: <2s (complete render)
  - [ ] API response: <500ms (backend + network)
  - [ ] Chart render: <500ms (Chart.js + React)
  - [ ] User interaction: <100ms (button click to state update)

### Task Group 13: Network Optimization (AC: #1, #4)
- [ ] HTTP/2 multiplexing (automatic with modern browsers)
  - [ ] Multiple requests in parallel over single connection
  - [ ] No code changes needed (browser + server support)
- [ ] Compression (Gzip/Brotli)
  - [ ] Vite build: Automatically generates compressed assets
  - [ ] Server: Serve pre-compressed files
  - [ ] Reduces bundle size by ~70%
- [ ] CDN for static assets (production)
  - [ ] Host JS/CSS on CDN (e.g., Cloudflare, AWS CloudFront)
  - [ ] Reduces latency (geographic distribution)
  - [ ] Not configured in MVP (local dev only)

### Task Group 14: Prevent Memory Leaks (AC: #5)
- [ ] Clean up subscriptions and listeners
  - [ ] Chart.js cleanup: react-chartjs-2 handles automatically
  - [ ] TanStack Query: Cleans up on unmount
  - [ ] Event listeners: Remove in useEffect cleanup
    ```typescript
    useEffect(() => {
      const handleResize = () => { /* ... */ };
      window.addEventListener('resize', handleResize);

      return () => {
        window.removeEventListener('resize', handleResize);  // Cleanup
      };
    }, []);
    ```
- [ ] Avoid creating closures in loops
  - [ ] ✅ Good: Use map with arrow functions
  - [ ] ❌ Bad: Traditional for loop with function creation
- [ ] Test for memory leaks
  - [ ] Chrome DevTools: Memory profiler
  - [ ] Navigate to comparison page → Take heap snapshot
  - [ ] Navigate away → Take heap snapshot
  - [ ] Compare: Detached DOM nodes should be minimal

### Task Group 15: Testing and Verification (AC: #1-5)
- [ ] Performance testing with Lighthouse
  - [ ] Run: Chrome DevTools → Lighthouse → Performance audit
  - [ ] Target scores:
    - Performance: >90
    - First Contentful Paint: <1.5s
    - Largest Contentful Paint: <2.0s
    - Total Blocking Time: <200ms
    - Cumulative Layout Shift: <0.1
- [ ] Load time testing with different network speeds
  - [ ] Chrome DevTools: Network throttling
  - [ ] Fast 3G: Page loads in <3s
  - [ ] Slow 3G: Page loads in <5s (acceptable)
  - [ ] Desktop: Page loads in <1s
- [ ] Bundle size verification
  - [ ] Check `dist/` folder after build
  - [ ] Main bundle: <300KB gzipped
  - [ ] Chart bundle (lazy): <150KB gzipped
  - [ ] Total page weight: <600KB gzipped
- [ ] React DevTools Profiler testing
  - [ ] Profile "Add Model" interaction
  - [ ] Verify: Only affected components re-render
  - [ ] Example: Adding model should NOT re-render ExportButton
- [ ] Manual E2E performance testing
  - [ ] Navigate to `/compare?models=1,2,3,4,5`
  - [ ] Measure time to interactive: <2s (stopwatch)
  - [ ] Add model: UI updates in <100ms (feels instant)
  - [ ] Remove model: UI updates in <100ms
  - [ ] Scroll page: Smooth 60fps (no jank)
  - [ ] Interact with chart: Tooltip appears instantly (<50ms)

## Dev Notes

### Architecture Alignment
- **Single API Call**: GET `/api/models/compare` fetches all data in one round trip
- **React Optimization**: Memoization, code splitting, lazy loading
- **Caching Strategy**: TanStack Query (client) + Redis (server) = Fast repeat visits
- **Progressive Enhancement**: Above-fold content loads first, below-fold lazy loads
- **No Premature Optimization**: Focus on low-hanging fruit (memoization, code splitting)

### Performance Budget

**Load Time Budget:**
- Initial HTML: <200ms
- JavaScript download: <500ms
- JavaScript parse/execute: <300ms
- API call: <500ms
- First paint: <1s
- **Complete render: <2s** ✅ Target met

**Bundle Size Budget:**
- Main bundle: <300KB gzipped
- Chart bundle (lazy): <150KB gzipped
- Total: <600KB gzipped
- Comparison: Next.js baseline ~250KB, React ~40KB

**Runtime Budget:**
- Component render: <16ms (60fps)
- User interaction response: <100ms
- Chart render: <500ms
- Smooth scrolling: 60fps (no frame drops)

### React.memo Best Practices

**When to use React.memo:**
- ✅ Expensive components (charts, tables)
- ✅ Components that receive same props frequently
- ✅ Components deep in tree (prevent cascade re-renders)

**When NOT to use React.memo:**
- ❌ Simple components (div, span, button)
- ❌ Components that always receive new props
- ❌ Premature optimization (profile first)

**Custom comparison function:**
```typescript
const areEqual = (prevProps, nextProps) => {
  // Only compare what matters
  return (
    prevProps.models.length === nextProps.models.length &&
    prevProps.selectedBenchmarks === nextProps.selectedBenchmarks
  );
};

export const BenchmarkChart = React.memo(BenchmarkChartComponent, areEqual);
```

### useMemo vs useCallback

**useMemo:**
- Memoizes return value (computed result)
- Use for: Expensive calculations, data transformations
- Example: `useMemo(() => models.filter(...), [models])`

**useCallback:**
- Memoizes function reference (stable identity)
- Use for: Event handlers passed to child components
- Example: `useCallback((id) => removeModel(id), [removeModel])`

**When to use:**
- useMemo: Computation takes >5ms
- useCallback: Function passed to memoized child component
- Neither: Simple operations (<1ms)

### Code Splitting Strategy

**What to split:**
- ✅ Below-fold components (charts, tables)
- ✅ Heavy dependencies (Chart.js)
- ✅ Rarely used features (export modal)

**What NOT to split:**
- ❌ Above-fold content (ModelCards)
- ❌ Small components (<10KB)
- ❌ Components used on every page

**Lazy loading example:**
```typescript
// ComparisonPage.tsx
const BenchmarkChart = lazy(() => import('./BenchmarkBarChart'));

<Suspense fallback={<Skeleton />}>
  <BenchmarkChart models={models} />
</Suspense>
```

Result: Chart code loaded only when needed (user scrolls to chart).

### TanStack Query Caching Benefits

**Cache hit scenario:**
1. User visits `/compare?models=1,2,3`
2. Data fetched, cached with key `['comparison', '1,2,3']`
3. User navigates away, returns
4. Cache hit: Data served instantly (<5ms, no API call)

**Cache miss scenario:**
1. User visits `/compare?models=1,2,4` (different IDs)
2. Cache miss: Different key
3. Data fetched from API (~500ms)
4. New cache entry created

**Deduplication:**
1. Component A requests `['comparison', '1,2,3']`
2. Component B requests `['comparison', '1,2,3']` (same key)
3. TanStack Query: Single API call, shared result

### Web Vitals Targets

**Core Web Vitals:**
- **LCP (Largest Contentful Paint)**: <2.5s
  - Largest element on page (likely BenchmarkBarChart)
  - Optimized with code splitting + lazy loading
- **FID (First Input Delay)**: <100ms
  - Time from user click to browser response
  - Optimized with React.memo (reduces main thread work)
- **CLS (Cumulative Layout Shift)**: <0.1
  - Layout stability (no sudden jumps)
  - Optimized with skeleton screens (reserve space)

**Other Metrics:**
- **FCP (First Contentful Paint)**: <1.5s
- **TTI (Time to Interactive)**: <3.5s
- **TBT (Total Blocking Time)**: <200ms

### Bundle Analysis

Vite build output example:
```
dist/assets/index-abc123.js       120.5 kB │ gzip: 45.2 kB
dist/assets/BenchmarkChart-def456.js  85.3 kB │ gzip: 32.1 kB (lazy)
dist/assets/vendor-ghi789.js      180.2 kB │ gzip: 65.5 kB

Total: 386 kB │ gzip: 142.8 kB ✅ Under 600KB budget
```

Breakdown:
- Main bundle: React, Router, TanStack Query, UI components
- Chart bundle (lazy): Chart.js, chart components
- Vendor: Dependencies (React, Zustand, axios)

### Memory Leak Prevention Checklist

✅ Remove event listeners in useEffect cleanup
✅ Cancel pending API requests on unmount (TanStack Query handles this)
✅ Destroy Chart.js instances on unmount (react-chartjs-2 handles this)
✅ Clear timers (setTimeout, setInterval)
✅ Unsubscribe from Zustand stores (automatic in hooks)

Common leak sources:
- ❌ Forgetting to remove window event listeners
- ❌ Creating closures in loops that capture large objects
- ❌ Storing large data in global state unnecessarily

### Prerequisites
- **Story 5.13**: Comparison page complete (all components implemented)
- **Story 5.2**: useComparisonData hook with TanStack Query
- **Story 5.5**: Chart.js setup and configuration
- All Epic 5 stories (5.1-5.13) completed
- No new dependencies required (optional: web-vitals)

### Quality Gates
- TypeScript strict mode: ✅ Zero `any` types
- Bundle size: ✅ <600KB gzipped total
- Load time: ✅ <2s complete render (5 models)
- Lighthouse Performance: ✅ >90 score
- No unnecessary re-renders: ✅ Verified with React DevTools Profiler
- Smooth interactions: ✅ 60fps, <100ms response
- No memory leaks: ✅ Verified with Chrome DevTools Memory profiler
- Code splitting: ✅ Charts lazy loaded below fold

### Project Structure Notes
```
apps/web/src/
├── components/
│   └── comparison/
│       └── *.tsx                          # All memoized with React.memo()
├── lib/
│   └── reportWebVitals.ts                 # New file (this story)
├── hooks/
│   └── useComparisonData.ts               # Already optimized with TanStack Query
└── main.tsx                               # Updated: Call reportWebVitals()
```

### Performance Considerations
- Single API call: ~500ms (batch fetch)
- TanStack Query cache hit: <5ms (instant)
- React component render: <16ms (60fps)
- Chart.js render: ~200-500ms (first render), <100ms (updates)
- Code splitting saves: ~150KB initial bundle (charts lazy loaded)

### Data Flow (Optimized)
```
User navigates to /compare?models=1,2,3
  → TanStack Query checks cache (key: ['comparison', '1,2,3'])
    → Cache miss: Fetch from API
      → GET /api/models/compare?ids=1,2,3
        → Backend checks Redis (key: cache:comparison:1,2,3:v1)
          → Cache hit: Return cached data (<50ms)
          → Cache miss: Query database, cache result, return (500ms)
      → TanStack Query caches response (5min stale time)
    → React renders:
      → ModelCards (immediate)
      → ComparisonTable (immediate)
      → <Suspense> triggers lazy load:
        → Download chart bundle (~150KB, 100-300ms)
        → BenchmarkBarChart renders (500ms)
        → PricingComparisonChart renders (300ms)
        → CapabilitiesMatrix renders (50ms)

Total time: ~1.5-2s (meets <2s target)

Repeat visit (cache hit):
  → TanStack Query cache: <5ms
  → React renders: ~500ms
  → Total: <1s ✅ Fast
```

### References
- [Source: docs/tech-spec-epic-5.md#Non-Functional Requirements] - Performance targets
- [Source: docs/tech-spec-epic-5.md#Acceptance Criteria] - AC-5.14: Performance requirements
- [Source: docs/epics.md#Story 5.14] - Original story with 5 acceptance criteria
- [Source: docs/solution-architecture.md#Performance Targets] - Page load <2s, 60fps
- [Source: docs/PRD.md#NFR-Performance] - Load time, response time targets

## Dev Agent Record

### Context Reference

<!-- Path(s) to story context XML will be added here by context workflow -->

### Agent Model Used

claude-sonnet-4-5-20250929

### Debug Log References

### Completion Notes List

### File List
