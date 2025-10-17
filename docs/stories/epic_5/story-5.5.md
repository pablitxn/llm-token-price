# Story 5.5: Integrate Chart.js Library

Status: Draft

## Story

As a developer,
I want Chart.js configured and ready,
so that I can create visualizations for model comparison.

## Acceptance Criteria

1. Chart.js library installed and verified in package.json
2. Basic chart component wrapper created (reusable BaseChart component)
3. Chart configuration defaults set (colors matching TailwindCSS palette, fonts, responsiveness)
4. Test chart renders successfully with sample data
5. Chart responsive and works on mobile (canvas resizing)

## Tasks / Subtasks

### Task Group 1: Verify Chart.js Installation (AC: #1)
- [ ] Verify Chart.js in `apps/web/package.json`
  - [ ] Check `chart.js` version: `^4.5.1`
  - [ ] Check `react-chartjs-2` version: `^5.3.0` (React wrapper)
  - [ ] No installation needed - already in dependencies
- [ ] Verify tree-shaking configuration
  - [ ] Check Vite build config supports Chart.js auto-registration
  - [ ] Verify bundle analyzer shows Chart.js imported (build step)
- [ ] Document Chart.js version and features used
  - [ ] Version 4.5.1 features: Tree-shakable, TypeScript support, ESM
  - [ ] React wrapper: react-chartjs-2 for React 19 compatibility

### Task Group 2: Register Chart.js Components (AC: #2)
- [ ] Create Chart.js registration file: `apps/web/src/lib/chart-setup.ts`
  - [ ] Import required Chart.js components:
    ```typescript
    import {
      Chart as ChartJS,
      CategoryScale,
      LinearScale,
      BarElement,
      LineElement,
      PointElement,
      ArcElement,
      Title,
      Tooltip,
      Legend,
      Filler,
    } from 'chart.js';
    ```
  - [ ] Register components globally:
    ```typescript
    ChartJS.register(
      CategoryScale,
      LinearScale,
      BarElement,
      LineElement,
      PointElement,
      ArcElement,
      Title,
      Tooltip,
      Legend,
      Filler
    );
    ```
  - [ ] Export registration function: `export const setupCharts = () => { /* registration code */ }`
- [ ] Call registration in app entry point
  - [ ] Update `apps/web/src/main.tsx`
  - [ ] Import: `import { setupCharts } from '@/lib/chart-setup';`
  - [ ] Call before React render: `setupCharts();`
  - [ ] Verify registration happens once (no re-registration on HMR)

### Task Group 3: Create BaseChart Wrapper Component (AC: #2, #3)
- [ ] Create component file: `apps/web/src/components/charts/BaseChart.tsx`
  - [ ] Define `BaseChartProps` interface:
    ```typescript
    interface BaseChartProps<T extends ChartType = 'bar'> {
      type: T;
      data: ChartData<T>;
      options?: ChartOptions<T>;
      height?: number;
      className?: string;
    }
    ```
  - [ ] Create generic component with TypeScript
  - [ ] Export as named export
- [ ] Implement chart rendering with react-chartjs-2
  - [ ] Use appropriate react-chartjs-2 component based on type:
    - `type: 'bar'` → `<Bar />` component
    - `type: 'line'` → `<Line />` component
    - `type: 'pie'` → `<Pie />` component
    - `type: 'radar'` → `<Radar />` component
  - [ ] Pass data and options props
  - [ ] Apply className for styling
- [ ] Add container wrapper for responsiveness
  - [ ] Container: `<div className="relative w-full" style={{ height }}>`
  - [ ] Chart canvas inside container
  - [ ] Default height: 400px (`height={400}`)

### Task Group 4: Define Chart Configuration Defaults (AC: #3)
- [ ] Create chart config file: `apps/web/src/config/chartDefaults.ts`
  - [ ] Define default Chart.js options:
    ```typescript
    import type { ChartOptions } from 'chart.js';

    export const defaultChartOptions: ChartOptions<'bar'> = {
      responsive: true,
      maintainAspectRatio: false,
      plugins: {
        legend: {
          display: true,
          position: 'top',
          labels: {
            font: { family: 'Inter, sans-serif', size: 12 },
            color: '#374151', // TailwindCSS gray-700
            padding: 16,
            usePointStyle: true,
          },
        },
        title: {
          display: false, // Let React components handle titles
        },
        tooltip: {
          backgroundColor: '#1f2937', // TailwindCSS gray-800
          titleColor: '#ffffff',
          bodyColor: '#d1d5db', // TailwindCSS gray-300
          borderColor: '#374151',
          borderWidth: 1,
          padding: 12,
          cornerRadius: 8,
          titleFont: { size: 14, weight: 'bold' },
          bodyFont: { size: 13 },
        },
      },
      scales: {
        x: {
          grid: { display: false },
          ticks: {
            font: { family: 'Inter, sans-serif', size: 11 },
            color: '#6b7280', // TailwindCSS gray-500
          },
        },
        y: {
          beginAtZero: true,
          grid: {
            color: '#e5e7eb', // TailwindCSS gray-200
            lineWidth: 1,
          },
          ticks: {
            font: { family: 'Inter, sans-serif', size: 11 },
            color: '#6b7280',
          },
        },
      },
    };
    ```
  - [ ] Export type-specific defaults (bar, line, pie)
  - [ ] Export helper: `mergeChartOptions(defaults, custom)` to deep merge options
- [ ] Define TailwindCSS color palette for charts
  - [ ] Create `chartColors.ts` config:
    ```typescript
    export const CHART_COLORS = {
      primary: '#10b981',    // green-500 (primary brand)
      secondary: '#3b82f6',  // blue-500
      tertiary: '#f59e0b',   // amber-500
      quaternary: '#8b5cf6', // violet-500
      quinary: '#ec4899',    // pink-500
      gray: '#6b7280',       // gray-500
      success: '#22c55e',    // green-500
      warning: '#f59e0b',    // amber-500
      error: '#ef4444',      // red-500
    };

    export const CHART_COLOR_PALETTE = [
      '#10b981', // green-500
      '#3b82f6', // blue-500
      '#f59e0b', // amber-500
      '#8b5cf6', // violet-500
      '#ec4899', // pink-500
      '#06b6d4', // cyan-500
      '#f97316', // orange-500
      '#a855f7', // purple-500
    ];

    // Generate transparent versions for backgrounds
    export const getTransparentColor = (hex: string, alpha: number): string => {
      const r = parseInt(hex.slice(1, 3), 16);
      const g = parseInt(hex.slice(3, 5), 16);
      const b = parseInt(hex.slice(5, 7), 16);
      return `rgba(${r}, ${g}, ${b}, ${alpha})`;
    };
    ```
  - [ ] Export color utilities for dataset styling

### Task Group 5: Create Test Chart Component (AC: #4)
- [ ] Create test component: `apps/web/src/components/charts/TestChart.tsx`
  - [ ] Use BaseChart wrapper
  - [ ] Sample data: 3 models × 3 benchmarks (MMLU, HumanEval, GSM8K)
    ```typescript
    const testData = {
      labels: ['MMLU', 'HumanEval', 'GSM8K'],
      datasets: [
        {
          label: 'GPT-4 Turbo',
          data: [86.4, 67.0, 92.0],
          backgroundColor: getTransparentColor(CHART_COLOR_PALETTE[0], 0.8),
          borderColor: CHART_COLOR_PALETTE[0],
          borderWidth: 2,
        },
        {
          label: 'Claude 3 Opus',
          data: [86.8, 84.9, 95.0],
          backgroundColor: getTransparentColor(CHART_COLOR_PALETTE[1], 0.8),
          borderColor: CHART_COLOR_PALETTE[1],
          borderWidth: 2,
        },
        {
          label: 'Gemini Pro',
          data: [79.1, 74.4, 86.5],
          backgroundColor: getTransparentColor(CHART_COLOR_PALETTE[2], 0.8),
          borderColor: CHART_COLOR_PALETTE[2],
          borderWidth: 2,
        },
      ],
    };
    ```
  - [ ] Render with title: "Benchmark Performance Comparison (Test)"
  - [ ] Use default options from chartDefaults.ts
- [ ] Temporarily add TestChart to ComparisonPage
  - [ ] Import TestChart in `apps/web/src/pages/ComparisonPage/ComparisonPage.tsx`
  - [ ] Render below BenchmarkComparisonSection (Story 5.4)
  - [ ] Wrap in section: `<section className="mt-8">`
  - [ ] This is a temporary test component (will be removed in Story 5.6)

### Task Group 6: Responsive Chart Behavior (AC: #5)
- [ ] Implement responsive container
  - [ ] Set `responsive: true` in default options (already done in Task Group 4)
  - [ ] Set `maintainAspectRatio: false` to allow custom height
  - [ ] Container uses `w-full` for horizontal responsiveness
- [ ] Test responsive behavior
  - [ ] Desktop (≥1024px): Chart width = container width, height = 400px
  - [ ] Tablet (768-1023px): Chart scales proportionally, no horizontal scroll
  - [ ] Mobile (<768px): Chart width = viewport width (minus padding), height = 300px
  - [ ] Use CSS media queries if needed: `@media (max-width: 768px) { height: 300px }`
- [ ] Add mobile-specific optimizations
  - [ ] Reduce font sizes on mobile: `fontSize: window.innerWidth < 768 ? 10 : 12`
  - [ ] Rotate x-axis labels on mobile if needed: `maxRotation: 45, minRotation: 45`
  - [ ] Legend position: `top` on desktop, `bottom` on mobile (optional)

### Task Group 7: Type Definitions (AC: #2, #3)
- [ ] Create chart types file: `apps/web/src/types/charts.ts`
  - [ ] Re-export Chart.js types for convenience:
    ```typescript
    export type {
      ChartType,
      ChartData,
      ChartOptions,
      ChartDataset,
      TooltipItem,
      LegendItem,
    } from 'chart.js';
    ```
  - [ ] Define custom types:
    ```typescript
    export interface BaseChartProps<T extends ChartType = 'bar'> {
      type: T;
      data: ChartData<T>;
      options?: ChartOptions<T>;
      height?: number;
      className?: string;
    }

    export interface ChartColorConfig {
      primary: string;
      secondary: string;
      tertiary: string;
      palette: string[];
    }
    ```
  - [ ] Export type guards if needed (e.g., `isBarChart`, `isLineChart`)
- [ ] Ensure strict TypeScript compliance
  - [ ] No `any` types in chart components
  - [ ] All chart data properly typed with ChartData<T>
  - [ ] All options properly typed with ChartOptions<T>

### Task Group 8: Integration with ComparisonPage (AC: #4)
- [ ] Update `ComparisonPage.tsx` to test chart rendering
  - [ ] Import TestChart component
  - [ ] Add section for test chart: `<section className="mt-8 p-6 bg-white border border-gray-200 rounded-lg">`
  - [ ] Section heading: "Chart Test" (`text-2xl font-bold mb-6`)
  - [ ] Render TestChart component
  - [ ] Add note: "This test chart will be replaced with real benchmark charts in Story 5.6"
- [ ] Verify chart renders without errors
  - [ ] Chart canvas appears in DOM
  - [ ] No console errors
  - [ ] Chart.js plugins registered correctly
  - [ ] Colors match TailwindCSS palette

### Task Group 9: Accessibility (AC: #5)
- [ ] Add ARIA attributes to BaseChart
  - [ ] Chart container: `role="img"` and `aria-label` describing chart
  - [ ] Example: `aria-label="Bar chart comparing benchmark scores across 3 models"`
  - [ ] Canvas: `aria-hidden="true"` (screen readers use description)
- [ ] Provide accessible data alternative
  - [ ] Add visually hidden table with chart data (optional, Story 5.11)
  - [ ] Use `<caption className="sr-only">` for screen readers
  - [ ] Include data in structured format (table or list)
- [ ] Keyboard navigation
  - [ ] Chart.js doesn't support keyboard by default (canvas limitation)
  - [ ] Add note: Keyboard interactions will be added in Story 5.11 (chart interactions)
  - [ ] Ensure chart is skippable via Tab key (no focus trap)

### Task Group 10: Performance Optimization (AC: #4, #5)
- [ ] Optimize chart rendering
  - [ ] Use React.memo() on BaseChart to prevent unnecessary re-renders
  - [ ] Memoize chart data and options with useMemo:
    ```typescript
    const memoizedData = useMemo(() => chartData, [chartData]);
    const memoizedOptions = useMemo(() => chartOptions, [chartOptions]);
    ```
  - [ ] Avoid recreating datasets on every render
- [ ] Lazy load Chart.js components
  - [ ] Use React.lazy() if chart not visible on initial page load (Story 5.14)
  - [ ] For now, import directly (optimization in Story 5.14)
- [ ] Test chart performance
  - [ ] Chart renders in < 500ms with 5 models × 5 benchmarks
  - [ ] Canvas resizing on window resize is smooth (throttled)
  - [ ] No memory leaks on component unmount (Chart.js cleanup)

### Task Group 11: Testing and Verification (AC: #1-5)
- [ ] Write unit test for chart-setup.ts
  - [ ] Test Chart.js component registration
  - [ ] Verify all required components registered (CategoryScale, LinearScale, BarElement, etc.)
  - [ ] Use Vitest
- [ ] Write integration test for BaseChart component
  - [ ] Render with sample data (3 models × 3 benchmarks)
  - [ ] Verify canvas element present in DOM
  - [ ] Test responsive behavior (mock window.innerWidth)
  - [ ] Verify default options applied
  - [ ] Use Vitest + React Testing Library
- [ ] Manual E2E testing
  - [ ] Navigate to `/compare?models=1,2,3`
  - [ ] Scroll to "Chart Test" section
  - [ ] Verify bar chart renders with 3 models × 3 benchmarks
  - [ ] Verify colors match TailwindCSS palette (green, blue, amber)
  - [ ] Test responsive behavior: Resize window, verify chart scales
  - [ ] Test mobile layout (<768px): Chart width = viewport width, reduced height
  - [ ] Verify tooltip shows on hover (Chart.js default behavior)
  - [ ] Verify legend shows at top with model names

## Dev Notes

### Architecture Alignment
- **Chart.js Setup**: Global registration in `main.tsx` (one-time setup)
- **BaseChart Component**: Reusable wrapper for all chart types (DRY principle)
- **Configuration Separation**: Chart defaults in `config/chartDefaults.ts` (not hardcoded)
- **Color System**: TailwindCSS color palette for consistency with design system
- **No Backend Changes**: Frontend-only story, uses existing ComparisonResponseDto data

### Chart.js 4.x Features Used
This story uses Chart.js 4.5.1 features:
- **Tree-shakable**: Only import required components (reduces bundle size by ~40%)
- **TypeScript Support**: Full type definitions for data, options, plugins
- **ESM Modules**: Native ES module support (Vite compatibility)
- **Responsive by Default**: Auto-resize on window resize (no manual listeners needed)
- **Canvas Rendering**: Better performance than SVG for large datasets

### React-ChartJS-2 Integration
react-chartjs-2 v5.3.0 provides React components for Chart.js:
```typescript
import { Bar, Line, Pie, Radar } from 'react-chartjs-2';

// Usage:
<Bar data={chartData} options={chartOptions} />
```

Benefits:
- React 19 compatibility (uses React.forwardRef)
- Auto-cleanup on unmount (no manual destroy)
- Props validation with TypeScript
- Re-render optimization (only updates on data/options change)

### Chart.js Registration Pattern
Chart.js 4.x requires explicit component registration (tree-shaking):
```typescript
import {
  Chart as ChartJS,
  CategoryScale,  // X-axis for bar charts
  LinearScale,    // Y-axis for numeric data
  BarElement,     // Bar chart bars
  LineElement,    // Line chart lines
  PointElement,   // Line chart points
  ArcElement,     // Pie/Doughnut chart arcs
  Title,          // Chart title plugin
  Tooltip,        // Hover tooltips
  Legend,         // Legend plugin
  Filler,         // Area fill for line charts
} from 'chart.js';

ChartJS.register(
  CategoryScale,
  LinearScale,
  BarElement,
  LineElement,
  PointElement,
  ArcElement,
  Title,
  Tooltip,
  Legend,
  Filler
);
```

This registration happens **once** in `main.tsx` before React renders.

### TailwindCSS Color Palette Mapping
Chart colors align with TailwindCSS design system:
```
Model 1 → green-500 (#10b981)   - Primary brand color
Model 2 → blue-500 (#3b82f6)    - Secondary color
Model 3 → amber-500 (#f59e0b)   - Tertiary color
Model 4 → violet-500 (#8b5cf6)  - Quaternary color
Model 5 → pink-500 (#ec4899)    - Quinary color
```

Background colors use 80% opacity: `rgba(16, 185, 129, 0.8)`

### Responsive Chart Strategy
Charts adapt to screen size:
- **Desktop (≥1024px)**: Height = 400px, full width
- **Tablet (768-1023px)**: Height = 400px, proportional scaling
- **Mobile (<768px)**: Height = 300px, reduced font sizes

CSS:
```css
@media (max-width: 768px) {
  .chart-container {
    height: 300px;
  }
}
```

JavaScript (optional):
```typescript
const isMobile = window.innerWidth < 768;
const chartHeight = isMobile ? 300 : 400;
```

### Default Chart Options Structure
Chart.js options hierarchy:
```typescript
{
  responsive: true,              // Auto-resize on window resize
  maintainAspectRatio: false,    // Allow custom height
  plugins: {                     // Plugin configuration
    legend: { /* ... */ },       // Legend settings
    tooltip: { /* ... */ },      // Tooltip settings
  },
  scales: {                      // Axis configuration
    x: { /* ... */ },            // X-axis settings
    y: { /* ... */ },            // Y-axis settings
  },
}
```

Options are **deep merged** with component-specific options (custom overrides defaults).

### Test Chart Data Format
Sample data structure:
```json
{
  "labels": ["MMLU", "HumanEval", "GSM8K"],
  "datasets": [
    {
      "label": "GPT-4 Turbo",
      "data": [86.4, 67.0, 92.0],
      "backgroundColor": "rgba(16, 185, 129, 0.8)",
      "borderColor": "#10b981",
      "borderWidth": 2
    }
  ]
}
```

This matches the ChartDataDto format from backend (tech-spec-epic-5.md).

### Prerequisites
- **Story 5.2**: `useComparisonData` hook provides model data
- **Story 5.4**: BenchmarkComparisonSection establishes benchmark display pattern
- **Chart.js + react-chartjs-2**: Already installed in package.json (verified)
- No new dependencies required

### Quality Gates
- TypeScript strict mode: ✅ Zero `any` types
- Chart renders: ✅ Test chart displays with 3 models × 3 benchmarks
- Colors: ✅ Match TailwindCSS palette (green, blue, amber)
- Responsive: ✅ Chart scales on mobile/tablet/desktop
- Performance: ✅ Chart renders in < 500ms
- Accessibility: ✅ ARIA labels, role="img"
- Registration: ✅ Chart.js components registered without errors

### Project Structure Notes
```
apps/web/src/
├── lib/
│   └── chart-setup.ts                 # Chart.js registration (this story)
├── components/
│   └── charts/
│       ├── BaseChart.tsx              # Reusable chart wrapper (this story)
│       └── TestChart.tsx              # Test chart component (this story, temporary)
├── config/
│   ├── chartDefaults.ts               # Default Chart.js options (this story)
│   └── chartColors.ts                 # TailwindCSS color palette (this story)
├── types/
│   └── charts.ts                      # Chart TypeScript types (this story)
└── main.tsx                           # Updated: Call setupCharts() (this story)
```

### Performance Considerations
- Chart.js 4.x bundle size: ~160KB (minified)
- Tree-shaking reduces to ~80KB (only used components)
- Canvas rendering: Better performance than SVG for 5 models × 10 benchmarks
- React.memo prevents re-renders on unrelated state changes
- useMemo caches chart data/options to avoid recalculation

### Chart.js Cleanup on Unmount
react-chartjs-2 handles cleanup automatically:
- Calls `chart.destroy()` on component unmount
- Removes canvas event listeners
- Prevents memory leaks

No manual cleanup required in useEffect.

### References
- [Source: docs/tech-spec-epic-5.md#Services and Modules] - ChartService client-side logic
- [Source: docs/tech-spec-epic-5.md#APIs and Interfaces] - GET /api/models/chart-data endpoint
- [Source: docs/tech-spec-epic-5.md#Acceptance Criteria] - AC-5.5: Chart.js integration requirements
- [Source: docs/epics.md#Story 5.5] - Original story with 5 acceptance criteria
- [Source: docs/solution-architecture.md#Frontend Components] - Chart.js 4.5.1 in tech stack
- [Source: apps/web/package.json] - chart.js@4.5.1, react-chartjs-2@5.3.0 already installed

## Dev Agent Record

### Context Reference

<!-- Path(s) to story context XML will be added here by context workflow -->

### Agent Model Used

claude-sonnet-4-5-20250929

### Debug Log References

### Completion Notes List

### File List
