# Story 5.6: Create Bar Chart for Benchmark Comparison

Status: Draft

## Story

As a user,
I want a bar chart showing benchmark scores,
so that I can visually compare model performance across multiple benchmarks.

## Acceptance Criteria

1. Bar chart added to comparison page below comparison table
2. X-axis: benchmark names, Y-axis: scores (0-100 range)
3. Grouped bars: one bar per model for each benchmark
4. Chart shows top 5-8 benchmarks by default (configurable in Story 5.7)
5. Legend identifies which bar color represents which model
6. Chart responsive and readable on all screen sizes

## Tasks / Subtasks

### Task Group 1: Create BenchmarkBarChart Component (AC: #1, #2, #3)
- [ ] Create component file: `apps/web/src/components/charts/BenchmarkBarChart.tsx`
  - [ ] Define `BenchmarkBarChartProps` interface:
    ```typescript
    interface BenchmarkBarChartProps {
      models: ModelDto[];
      selectedBenchmarks?: string[];  // Benchmark IDs, optional (defaults to top 5)
      height?: number;
      className?: string;
    }
    ```
  - [ ] Create functional component with TypeScript
  - [ ] Export as named export
- [ ] Component layout structure
  - [ ] Section container: `<section className="mt-8 p-6 bg-white border border-gray-200 rounded-lg">`
  - [ ] Section heading: "Benchmark Performance Comparison" (`text-2xl font-bold mb-6`)
  - [ ] Description: "Compare model scores across key benchmarks" (`text-gray-600 mb-4`)
  - [ ] Chart container: `<div className="relative w-full" style={{ height: 400 }}>`
- [ ] Use BaseChart component from Story 5.5
  - [ ] Import BaseChart: `import { BaseChart } from '@/components/charts/BaseChart';`
  - [ ] Render: `<BaseChart type="bar" data={chartData} options={chartOptions} height={400} />`
  - [ ] Pass transformed data and merged options

### Task Group 2: Transform Model Data to Chart Format (AC: #2, #3, #4)
- [ ] Create data transformation utility: `apps/web/src/utils/chartData.ts`
  - [ ] Function: `buildBenchmarkChartData(models: ModelDto[], benchmarkIds?: string[]): ChartData<'bar'>`
  - [ ] Input: Array of ModelDto with benchmarkScores
  - [ ] Output: Chart.js data format (labels + datasets)
- [ ] Extract benchmark labels (X-axis)
  - [ ] If `benchmarkIds` provided: Use specified benchmarks in that order
  - [ ] If not provided: Select top 5 benchmarks by coverage (most models have scores)
  - [ ] Fallback benchmarks: ['MMLU', 'HumanEval', 'GSM8K', 'HellaSwag', 'MATH']
  - [ ] Labels array: `['MMLU', 'HumanEval', 'GSM8K', 'HellaSwag', 'MATH']`
- [ ] Build datasets (one per model)
  - [ ] Iterate over models array
  - [ ] For each model, create dataset object:
    ```typescript
    {
      label: model.name,  // "GPT-4 Turbo"
      data: benchmarkIds.map(id => getScoreForBenchmark(model, id)),  // [86.4, 67.0, 92.0, ...]
      backgroundColor: getTransparentColor(CHART_COLOR_PALETTE[index], 0.8),
      borderColor: CHART_COLOR_PALETTE[index],
      borderWidth: 2,
      borderRadius: 4,  // Rounded bar corners
    }
    ```
  - [ ] Handle missing scores: Return `null` for benchmarks model doesn't have (Chart.js skips null values)
  - [ ] Assign colors from palette: Model 1 → green, Model 2 → blue, Model 3 → amber, etc.
- [ ] Helper function: `getScoreForBenchmark(model: ModelDto, benchmarkId: string): number | null`
  - [ ] Search model.benchmarkScores array for matching benchmarkId
  - [ ] Return score if found, null if not found
  - [ ] Handle edge case: Empty benchmarkScores array

### Task Group 3: Select Default Benchmarks (AC: #4)
- [ ] Implement benchmark selection logic in `chartData.ts`
  - [ ] Function: `selectTopBenchmarks(models: ModelDto[], count: number = 5): string[]`
  - [ ] Strategy: Select benchmarks with highest coverage (most models have scores)
  - [ ] Algorithm:
    ```typescript
    // 1. Collect all unique benchmark IDs across all models
    const allBenchmarks = new Set<string>();
    models.forEach(m => m.benchmarkScores.forEach(b => allBenchmarks.add(b.benchmarkId)));

    // 2. Calculate coverage for each benchmark (how many models have it)
    const coverage = Array.from(allBenchmarks).map(id => ({
      benchmarkId: id,
      count: models.filter(m => m.benchmarkScores.some(b => b.benchmarkId === id)).length,
    }));

    // 3. Sort by coverage (descending), then alphabetically
    coverage.sort((a, b) => b.count - a.count || a.benchmarkId.localeCompare(b.benchmarkId));

    // 4. Return top N benchmark IDs
    return coverage.slice(0, count).map(c => c.benchmarkId);
    ```
  - [ ] Default count: 5 benchmarks
  - [ ] Configurable count (Story 5.7 will add UI control)
- [ ] Fallback for insufficient data
  - [ ] If <5 benchmarks available: Show all available benchmarks
  - [ ] If no benchmarks: Display empty state message

### Task Group 4: Configure Chart Options (AC: #2, #5, #6)
- [ ] Create chart-specific options in BenchmarkBarChart component
  - [ ] Start with defaultChartOptions from Story 5.5
  - [ ] Merge with custom options using `mergeChartOptions` utility
  - [ ] Custom options:
    ```typescript
    const customOptions: ChartOptions<'bar'> = {
      plugins: {
        title: {
          display: false,  // Use React heading instead
        },
        legend: {
          display: true,
          position: 'top',
          labels: {
            font: { family: 'Inter, sans-serif', size: 12 },
            color: '#374151',
            padding: 16,
            usePointStyle: true,
            pointStyle: 'rect',
          },
        },
        tooltip: {
          callbacks: {
            label: (context) => {
              const modelName = context.dataset.label;
              const score = context.parsed.y;
              return `${modelName}: ${score.toFixed(1)}`;
            },
          },
        },
      },
      scales: {
        x: {
          grid: { display: false },
          ticks: {
            font: { family: 'Inter, sans-serif', size: 11 },
            color: '#6b7280',
            maxRotation: 0,  // Horizontal labels
            minRotation: 0,
          },
        },
        y: {
          beginAtZero: true,
          max: 100,  // Benchmark scores are 0-100
          grid: {
            color: '#e5e7eb',
            lineWidth: 1,
          },
          ticks: {
            font: { family: 'Inter, sans-serif', size: 11 },
            color: '#6b7280',
            callback: (value) => `${value}%`,  // Add percentage symbol
          },
        },
      },
      interaction: {
        mode: 'index',  // Hover all bars in same benchmark
        intersect: false,
      },
    };
    ```
  - [ ] Merge: `const options = mergeChartOptions(defaultChartOptions, customOptions);`
- [ ] Mobile-specific options
  - [ ] Detect screen size: `const isMobile = window.innerWidth < 768;`
  - [ ] Adjust options for mobile:
    ```typescript
    if (isMobile) {
      options.plugins.legend.position = 'bottom';
      options.scales.x.ticks.maxRotation = 45;
      options.scales.x.ticks.minRotation = 45;
    }
    ```
  - [ ] Reduce chart height on mobile: `height={isMobile ? 300 : 400}`

### Task Group 5: Integrate into ComparisonPage (AC: #1)
- [ ] Update `ComparisonPage.tsx` to include BenchmarkBarChart
  - [ ] Import component: `import { BenchmarkBarChart } from '@/components/charts/BenchmarkBarChart';`
  - [ ] Remove TestChart component (from Story 5.5)
  - [ ] Add BenchmarkBarChart below BenchmarkComparisonSection (Story 5.4)
  - [ ] Render: `<BenchmarkBarChart models={data.models} />`
  - [ ] Placement: After `<BenchmarkComparisonSection>`, before future charts section
- [ ] Section divider
  - [ ] Add horizontal rule: `<hr className="my-8 border-gray-200" />`
  - [ ] Or use spacing: `<div className="h-8" />`
- [ ] Loading state
  - [ ] Show skeleton while `isLoading`
  - [ ] Skeleton: Gray rectangle with `animate-pulse`, match chart dimensions
  - [ ] `<div className="mt-8 p-6 bg-gray-100 rounded-lg animate-pulse" style={{ height: 400 }}></div>`

### Task Group 6: Handle Empty States (AC: #4)
- [ ] No benchmarks available
  - [ ] Check if all models have empty benchmarkScores arrays
  - [ ] Display message: "No benchmark data available for comparison"
  - [ ] Style: `text-center text-gray-500 py-12` with icon (AlertCircle from lucide-react)
- [ ] Insufficient models
  - [ ] Check if models.length < 2
  - [ ] Display message: "Select at least 2 models to compare benchmarks"
  - [ ] This should already be handled by ComparisonPage (Story 5.1)
- [ ] Missing scores for some models
  - [ ] Chart.js automatically handles null values (skips bars)
  - [ ] Add note below chart: "Missing bars indicate unavailable benchmark scores"
  - [ ] Small gray text: `text-sm text-gray-500 mt-2`

### Task Group 7: Responsive Behavior (AC: #6)
- [ ] Desktop layout (≥1024px)
  - [ ] Chart width: 100% of container
  - [ ] Chart height: 400px
  - [ ] Legend: Top position
  - [ ] Labels: Horizontal (0° rotation)
- [ ] Tablet layout (768-1023px)
  - [ ] Chart width: 100% of container
  - [ ] Chart height: 400px
  - [ ] Legend: Top position
  - [ ] Labels: Horizontal or 45° rotation (if many benchmarks)
- [ ] Mobile layout (<768px)
  - [ ] Chart width: 100% (minus padding)
  - [ ] Chart height: 300px
  - [ ] Legend: Bottom position
  - [ ] Labels: 45° rotation
  - [ ] Font sizes: Reduced (9px for ticks, 10px for legend)
- [ ] Test responsive resizing
  - [ ] Chart auto-resizes on window resize (Chart.js responsive: true)
  - [ ] No layout shift or overflow

### Task Group 8: Legend Configuration (AC: #5)
- [ ] Legend displays model names
  - [ ] Each model gets unique color from CHART_COLOR_PALETTE
  - [ ] Legend items: Model 1 (green rect), Model 2 (blue rect), Model 3 (amber rect), etc.
  - [ ] Use `usePointStyle: true` for rectangular legend markers
- [ ] Legend positioning
  - [ ] Desktop/Tablet: `position: 'top'`, align left
  - [ ] Mobile: `position: 'bottom'`, center aligned
  - [ ] Padding: 16px between legend and chart
- [ ] Legend interactivity (Chart.js default)
  - [ ] Click legend item: Toggle dataset visibility
  - [ ] Hover legend item: Highlight corresponding bars
  - [ ] This is Chart.js default behavior (no custom code needed)

### Task Group 9: Color Assignment Strategy (AC: #3, #5)
- [ ] Consistent color mapping
  - [ ] Model index determines color: `CHART_COLOR_PALETTE[index % CHART_COLOR_PALETTE.length]`
  - [ ] Model 1 (index 0) → green (#10b981)
  - [ ] Model 2 (index 1) → blue (#3b82f6)
  - [ ] Model 3 (index 2) → amber (#f59e0b)
  - [ ] Model 4 (index 3) → violet (#8b5cf6)
  - [ ] Model 5 (index 4) → pink (#ec4899)
  - [ ] If >5 models: Wrap around palette (model 6 → green again)
- [ ] Color accessibility
  - [ ] Ensure sufficient contrast between bars and background
  - [ ] Border color: Darker shade of fill color
  - [ ] Background color: 80% opacity for visual separation
  - [ ] Test with color blindness simulator (optional, Story 5.11)

### Task Group 10: Type Definitions (AC: #2, #3)
- [ ] Update `apps/web/src/types/charts.ts`
  - [ ] Define `BenchmarkBarChartProps` interface
  - [ ] Define `BenchmarkDataPoint` type:
    ```typescript
    export interface BenchmarkDataPoint {
      benchmarkId: string;
      benchmarkName: string;
      score: number | null;
    }
    ```
  - [ ] Define `ChartDataTransformOptions` type:
    ```typescript
    export interface ChartDataTransformOptions {
      benchmarkIds?: string[];
      maxBenchmarks?: number;
      colorPalette?: string[];
    }
    ```
- [ ] Ensure type safety
  - [ ] No `any` types in chart data transformation
  - [ ] All Chart.js data properly typed with `ChartData<'bar'>`
  - [ ] All options properly typed with `ChartOptions<'bar'>`

### Task Group 11: Performance Optimization (AC: #6)
- [ ] Memoize chart data
  - [ ] Use `useMemo` to prevent recalculation on every render:
    ```typescript
    const chartData = useMemo(
      () => buildBenchmarkChartData(models, selectedBenchmarks),
      [models, selectedBenchmarks]
    );
    ```
  - [ ] Only recalculate when models or selectedBenchmarks change
- [ ] Memoize chart options
  - [ ] Use `useMemo` for options:
    ```typescript
    const chartOptions = useMemo(
      () => mergeChartOptions(defaultChartOptions, customOptions),
      [isMobile]  // Recalculate only when screen size changes
    );
    ```
- [ ] Optimize component re-renders
  - [ ] Use `React.memo()` on BenchmarkBarChart
  - [ ] Prevent re-render when unrelated ComparisonPage state changes
- [ ] Test performance
  - [ ] Chart renders in <500ms with 5 models × 8 benchmarks
  - [ ] No lag on window resize

### Task Group 12: Accessibility (AC: #6)
- [ ] Add ARIA attributes
  - [ ] Chart container: `role="img"` and `aria-label`
  - [ ] Example: `aria-label="Bar chart comparing benchmark performance across 3 models and 5 benchmarks"`
  - [ ] Canvas: `aria-hidden="true"` (screen readers use description)
- [ ] Provide accessible data alternative
  - [ ] Add visually hidden text description:
    ```typescript
    <p className="sr-only">
      Benchmark comparison: GPT-4 Turbo scores 86.4 on MMLU, 67.0 on HumanEval, 92.0 on GSM8K.
      Claude 3 Opus scores 86.8 on MMLU, 84.9 on HumanEval, 95.0 on GSM8K.
    </p>
    ```
  - [ ] Or link to data table: "View data in table format below"
- [ ] Keyboard navigation
  - [ ] Chart.js canvas not keyboard accessible by default
  - [ ] Add note: Keyboard interactions will be added in Story 5.11
  - [ ] Ensure chart is skippable via Tab key

### Task Group 13: Testing and Verification (AC: #1-6)
- [ ] Write unit test for `buildBenchmarkChartData` utility
  - [ ] Test with 2 models, 3 benchmarks (all models have all benchmarks)
  - [ ] Test with 3 models, 5 benchmarks (some models missing scores)
  - [ ] Test with empty models array (should return empty datasets)
  - [ ] Test with models having no benchmarkScores (should return null values)
  - [ ] Verify color assignment: Model 1 → green, Model 2 → blue, etc.
  - [ ] Use Vitest
- [ ] Write unit test for `selectTopBenchmarks` utility
  - [ ] Test coverage calculation: Models with MMLU=3, HumanEval=2, GSM8K=1 → order: MMLU, HumanEval, GSM8K
  - [ ] Test with <5 benchmarks available: Should return all available
  - [ ] Test with no benchmarks: Should return empty array
  - [ ] Use Vitest
- [ ] Write integration test for BenchmarkBarChart component
  - [ ] Render with 3 models × 5 benchmarks
  - [ ] Verify canvas element present in DOM
  - [ ] Verify legend shows 3 model names
  - [ ] Verify chart heading: "Benchmark Performance Comparison"
  - [ ] Test empty state: No benchmarks available
  - [ ] Use Vitest + React Testing Library
- [ ] Manual E2E testing
  - [ ] Navigate to `/compare?models=1,2,3`
  - [ ] Scroll to "Benchmark Performance Comparison" section
  - [ ] Verify bar chart renders with grouped bars (3 bars per benchmark)
  - [ ] Verify X-axis labels: Benchmark names (MMLU, HumanEval, etc.)
  - [ ] Verify Y-axis range: 0-100 with percentage symbols
  - [ ] Verify legend: 3 model names with colored rectangles
  - [ ] Hover over bar: Tooltip shows "Model Name: 86.4"
  - [ ] Click legend item: Toggles dataset visibility
  - [ ] Test responsive: Resize window, verify chart adapts
  - [ ] Test mobile (<768px): Legend at bottom, labels rotated 45°

## Dev Notes

### Architecture Alignment
- **Data Transformation**: Pure utility function `buildBenchmarkChartData` (testable, no side effects)
- **Component Separation**: BenchmarkBarChart focuses on chart rendering, ComparisonPage handles data fetching
- **Reusability**: BaseChart wrapper (Story 5.5) used for all chart types
- **Configuration**: Chart options externalized in chartDefaults.ts, colors in chartColors.ts
- **No Backend Changes**: Uses existing ComparisonResponseDto with benchmarkScores array

### Chart.js Grouped Bar Chart
Chart.js renders grouped bars automatically when multiple datasets share same labels:
```javascript
{
  labels: ['MMLU', 'HumanEval', 'GSM8K'],
  datasets: [
    { label: 'GPT-4', data: [86.4, 67.0, 92.0] },   // Green bars
    { label: 'Claude', data: [86.8, 84.9, 95.0] },  // Blue bars
    { label: 'Gemini', data: [79.1, 74.4, 86.5] },  // Amber bars
  ]
}
```

Result: 3 bars per benchmark (MMLU has 3 bars, HumanEval has 3 bars, etc.)

### Benchmark Selection Strategy
Top 5 benchmarks by **coverage** (not by score):
1. Count how many models have each benchmark
2. Sort benchmarks by count (descending)
3. Select top 5

Example:
- MMLU: 5 models have it → Coverage = 5
- HumanEval: 4 models have it → Coverage = 4
- GSM8K: 3 models have it → Coverage = 3
- MATH: 3 models have it → Coverage = 3
- HellaSwag: 2 models have it → Coverage = 2

Selected: MMLU, HumanEval, GSM8K, MATH, HellaSwag (top 5 by coverage)

This ensures chart has most complete data (fewer missing bars).

### Missing Score Handling
Chart.js skips `null` values:
```javascript
{
  label: 'GPT-4',
  data: [86.4, null, 92.0]  // No bar shown for HumanEval (index 1)
}
```

This creates "gaps" in grouped bars where scores are unavailable, making it visually clear which models lack data.

### Color Assignment Logic
Colors assigned by model index (stable across re-renders):
```typescript
const color = CHART_COLOR_PALETTE[modelIndex % CHART_COLOR_PALETTE.length];
const backgroundColor = getTransparentColor(color, 0.8);  // 80% opacity
const borderColor = color;  // 100% opacity
```

With 8 colors in palette, supports up to 8 models with unique colors. Model 9 wraps to first color (green).

### Responsive Chart Breakpoints
Chart adapts to screen size:
- **Desktop (≥1024px)**: Height 400px, legend top, labels horizontal
- **Tablet (768-1023px)**: Height 400px, legend top, labels horizontal or 45° (if >6 benchmarks)
- **Mobile (<768px)**: Height 300px, legend bottom, labels 45°, smaller fonts

Chart.js `responsive: true` handles canvas resizing automatically.

### Tooltip Customization
Custom tooltip callback formats scores:
```typescript
tooltip: {
  callbacks: {
    label: (context) => {
      const modelName = context.dataset.label;
      const score = context.parsed.y;
      return `${modelName}: ${score.toFixed(1)}`;  // "GPT-4 Turbo: 86.4"
    },
  },
}
```

Tooltip shows on hover, displays model name + score with 1 decimal place.

### Y-Axis Configuration
Y-axis shows percentage values:
```typescript
scales: {
  y: {
    beginAtZero: true,
    max: 100,  // All benchmarks scaled 0-100
    ticks: {
      callback: (value) => `${value}%`,  // 0%, 20%, 40%, 60%, 80%, 100%
    },
  },
}
```

This assumes all benchmarks are normalized to 0-100 scale (standard for most benchmarks: MMLU, HumanEval, GSM8K).

### Interaction Mode
`interaction: { mode: 'index', intersect: false }` enables:
- Hover over any bar → Tooltip shows all models' scores for that benchmark
- Example: Hover MMLU bar → Tooltip shows "GPT-4: 86.4, Claude: 86.8, Gemini: 79.1"

This helps compare models on same benchmark quickly.

### Prerequisites
- **Story 5.5**: BaseChart component, chart-setup.ts, chartDefaults.ts, chartColors.ts
- **Story 5.4**: BenchmarkComparisonSection establishes benchmark data structure
- **Story 5.2**: useComparisonData hook provides models with benchmarkScores
- No new dependencies required

### Quality Gates
- TypeScript strict mode: ✅ Zero `any` types
- Chart renders: ✅ Grouped bar chart displays with 2-5 models × 5-8 benchmarks
- Colors: ✅ Match TailwindCSS palette (green, blue, amber, violet, pink)
- Legend: ✅ Shows all model names with colored rectangles
- Responsive: ✅ Chart adapts to mobile/tablet/desktop
- Performance: ✅ Chart renders in <500ms
- Accessibility: ✅ ARIA labels, screen reader description
- Missing data: ✅ Null values handled (bars skipped)

### Project Structure Notes
```
apps/web/src/
├── components/
│   └── charts/
│       ├── BaseChart.tsx              # From Story 5.5
│       ├── BenchmarkBarChart.tsx      # New component (this story)
│       └── TestChart.tsx              # REMOVED (this story)
├── utils/
│   └── chartData.ts                   # New utility (this story)
│       ├── buildBenchmarkChartData()
│       └── selectTopBenchmarks()
├── types/
│   └── charts.ts                      # Updated with BenchmarkBarChartProps
└── pages/
    └── ComparisonPage/
        └── ComparisonPage.tsx         # Updated: Add BenchmarkBarChart
```

### Performance Considerations
- Chart with 5 models × 8 benchmarks = 40 bars (performant)
- useMemo prevents recalculating chart data on every render
- React.memo prevents re-rendering chart when unrelated state changes
- Chart.js canvas rendering: ~10ms for 40 bars (very fast)

### Data Flow
```
ComparisonPage (useComparisonData)
  → data.models (ModelDto[])
    → BenchmarkBarChart component
      → buildBenchmarkChartData(models, benchmarkIds)
        → ChartData<'bar'> (labels + datasets)
          → BaseChart component
            → react-chartjs-2 <Bar />
              → Chart.js canvas rendering
```

### References
- [Source: docs/tech-spec-epic-5.md#Services and Modules] - BenchmarkBarChart component spec
- [Source: docs/tech-spec-epic-5.md#Data Models and Contracts] - ChartDataDto structure
- [Source: docs/tech-spec-epic-5.md#Acceptance Criteria] - AC-5.6: Bar chart requirements
- [Source: docs/epics.md#Story 5.6] - Original story with 6 acceptance criteria
- [Source: docs/stories/story-5.5.md] - BaseChart wrapper, chart setup
- [Source: docs/stories/story-5.4.md] - BenchmarkComparisonSection (benchmark data structure)
- [Source: docs/solution-architecture.md#Frontend Components] - Chart.js integration pattern

## Dev Agent Record

### Context Reference

<!-- Path(s) to story context XML will be added here by context workflow -->

### Agent Model Used

claude-sonnet-4-5-20250929

### Debug Log References

### Completion Notes List

### File List
