# Story 5.10: Add Chart Type Switcher

Status: Draft

## Story

As a user,
I want to switch between different chart types,
so that I can view comparison data in the format that best suits my analysis needs.

## Acceptance Criteria

1. Chart type selector (segmented control or button group) displayed above benchmark chart
2. Options: "Grouped Bar" (default), "Line Chart" for trend visualization
3. Clicking button re-renders chart in selected type (smooth transition)
4. Chart type preference persists during comparison session (session storage)
5. Different chart types optimized for different insights:
   - Grouped Bar: Best for comparing values across models
   - Line: Best for showing trends across benchmarks

## Tasks / Subtasks

### Task Group 1: Create ChartTypeSwitcher Component (AC: #1, #2)
- [ ] Create component file: `apps/web/src/components/charts/ChartTypeSwitcher.tsx`
  - [ ] Define `ChartTypeSwitcherProps` interface:
    ```typescript
    interface ChartTypeSwitcherProps {
      currentType: 'bar' | 'line';
      onTypeChange: (type: 'bar' | 'line') => void;
      className?: string;
    }
    ```
  - [ ] Create functional component with TypeScript
  - [ ] Export as named export
- [ ] Component layout structure
  - [ ] Container: `<div className="flex items-center gap-2 mb-4">`
  - [ ] Label: "Chart Type:" (`text-sm font-medium text-gray-700`)
  - [ ] Button group: Segmented control pattern
- [ ] Segmented control (button group)
  - [ ] Container: `<div className="inline-flex rounded-lg border border-gray-300 overflow-hidden">`
  - [ ] Button 1: "Grouped Bar"
  - [ ] Button 2: "Line Chart"
  - [ ] Active button style: `bg-blue-500 text-white`
  - [ ] Inactive button style: `bg-white text-gray-700 hover:bg-gray-50`
  - [ ] Example:
    ```typescript
    <div className="inline-flex rounded-lg border border-gray-300 overflow-hidden">
      <button
        onClick={() => onTypeChange('bar')}
        className={cn(
          'px-4 py-2 text-sm font-medium transition-colors',
          currentType === 'bar'
            ? 'bg-blue-500 text-white'
            : 'bg-white text-gray-700 hover:bg-gray-50'
        )}
      >
        <BarChart3 className="w-4 h-4 inline mr-2" />
        Grouped Bar
      </button>
      <button
        onClick={() => onTypeChange('line')}
        className={cn(
          'px-4 py-2 text-sm font-medium border-l border-gray-300 transition-colors',
          currentType === 'line'
            ? 'bg-blue-500 text-white'
            : 'bg-white text-gray-700 hover:bg-gray-50'
        )}
      >
        <LineChart className="w-4 h-4 inline mr-2" />
        Line Chart
      </button>
    </div>
    ```
  - [ ] Icons from lucide-react: `BarChart3`, `LineChart`

### Task Group 2: Define Chart Type Options (AC: #2, #5)
- [ ] Create chart types configuration: `apps/web/src/config/chartTypes.ts`
  - [ ] Define chart type metadata:
    ```typescript
    export type ChartType = 'bar' | 'line';

    export interface ChartTypeOption {
      id: ChartType;
      label: string;
      icon: React.ComponentType;
      description: string;
      bestFor: string;
    }

    export const CHART_TYPE_OPTIONS: ChartTypeOption[] = [
      {
        id: 'bar',
        label: 'Grouped Bar',
        icon: BarChart3,
        description: 'Side-by-side bars for each model',
        bestFor: 'Comparing values across models for each benchmark',
      },
      {
        id: 'line',
        label: 'Line Chart',
        icon: LineChart,
        description: 'Connected lines showing trends',
        bestFor: 'Showing performance trends across multiple benchmarks',
      },
    ];
    ```
  - [ ] Export default chart type: `export const DEFAULT_CHART_TYPE: ChartType = 'bar';`
- [ ] Future chart types (out of MVP scope)
  - [ ] Radar: Multi-dimensional comparison (5+ benchmarks)
  - [ ] Scatter: Correlation analysis (price vs performance)
  - [ ] Note: Documented for future enhancement, not implemented in this story

### Task Group 3: Integrate ChartTypeSwitcher into BenchmarkBarChart (AC: #1, #3)
- [ ] Update `BenchmarkBarChart.tsx` to include chart type selection
  - [ ] Add state: `const [chartType, setChartType] = useSessionStorage<ChartType>('benchmark-chart-type', 'bar');`
  - [ ] Import ChartTypeSwitcher: `import { ChartTypeSwitcher } from './ChartTypeSwitcher';`
  - [ ] Render switcher above MetricSelector (Story 5.7)
    ```typescript
    <div className="flex items-center justify-between mb-4">
      <ChartTypeSwitcher currentType={chartType} onTypeChange={setChartType} />
      <MetricSelector ... />
    </div>
    ```
  - [ ] Pass chartType to BaseChart: `<BaseChart type={chartType} data={chartData} options={chartOptions} />`
- [ ] Conditional chart rendering
  - [ ] If `chartType === 'bar'`: Render grouped bar chart (existing behavior)
  - [ ] If `chartType === 'line'`: Render line chart with adapted data

### Task Group 4: Adapt Chart Data for Line Chart (AC: #3, #5)
- [ ] Update `buildBenchmarkChartData` to support line chart format
  - [ ] Bar chart data: Grouped bars (existing)
    ```typescript
    {
      labels: ['MMLU', 'HumanEval', 'GSM8K'],
      datasets: [
        { label: 'GPT-4', data: [86.4, 67.0, 92.0] },  // One dataset per model
        { label: 'Claude', data: [86.8, 84.9, 95.0] },
      ]
    }
    ```
  - [ ] Line chart data: Same structure, different visualization
    ```typescript
    {
      labels: ['MMLU', 'HumanEval', 'GSM8K'],
      datasets: [
        {
          label: 'GPT-4',
          data: [86.4, 67.0, 92.0],
          borderColor: CHART_COLOR_PALETTE[0],
          backgroundColor: getTransparentColor(CHART_COLOR_PALETTE[0], 0.1),  // Very transparent fill
          fill: false,  // Or true for area chart
          tension: 0.4,  // Curved lines (0 = straight)
        },
        {
          label: 'Claude',
          data: [86.8, 84.9, 95.0],
          borderColor: CHART_COLOR_PALETTE[1],
          backgroundColor: getTransparentColor(CHART_COLOR_PALETTE[1], 0.1),
          fill: false,
          tension: 0.4,
        },
      ]
    }
    ```
  - [ ] No transformation needed, just different dataset properties
- [ ] Line chart visual properties
  - [ ] Border color: Solid color from palette
  - [ ] Border width: 3px (thicker than bar borders)
  - [ ] Point style: Circle
  - [ ] Point radius: 4px (visible but not too large)
  - [ ] Point hover radius: 6px (larger on hover)
  - [ ] Tension: 0.4 (smooth curves, not sharp angles)
  - [ ] Fill: false (no area fill, just lines)

### Task Group 5: Configure Line Chart Options (AC: #5)
- [ ] Create line-specific chart options
  - [ ] Start with defaultChartOptions (Story 5.5)
  - [ ] Override for line chart:
    ```typescript
    const lineChartOptions: ChartOptions<'line'> = {
      ...defaultChartOptions,
      plugins: {
        ...defaultChartOptions.plugins,
        legend: {
          display: true,
          position: 'top',
        },
        tooltip: {
          mode: 'index',  // Show all models at same benchmark
          intersect: false,  // Hover anywhere on vertical line
        },
      },
      scales: {
        x: {
          grid: { display: true, color: '#f3f4f6' },  // Light gray grid
          ticks: {
            font: { family: 'Inter, sans-serif', size: 11 },
            color: '#6b7280',
          },
        },
        y: {
          beginAtZero: true,
          max: 100,
          grid: {
            color: '#e5e7eb',
            lineWidth: 1,
          },
          ticks: {
            font: { family: 'Inter, sans-serif', size: 11 },
            color: '#6b7280',
            callback: (value) => `${value}%`,
          },
        },
      },
      interaction: {
        mode: 'index',  // Hover shows vertical line across all datasets
        intersect: false,
      },
    };
    ```
  - [ ] Key difference from bar chart: Grid lines on X-axis (shows benchmark boundaries)

### Task Group 6: Smooth Transition Between Chart Types (AC: #3)
- [ ] Chart.js automatically animates chart type changes
  - [ ] Default animation: 300ms fade + morph
  - [ ] No custom animation code needed
- [ ] Optional: Add custom transition
  - [ ] Wrap BaseChart in `<div>` with CSS transition
  - [ ] Fade out old chart, fade in new chart
  - [ ] Example:
    ```typescript
    <div
      key={chartType}  // Force re-render on type change
      className="transition-opacity duration-300"
    >
      <BaseChart type={chartType} data={chartData} options={chartOptions} />
    </div>
    ```
  - [ ] Not necessary for MVP (Chart.js handles it well)
- [ ] Test transition smoothness
  - [ ] Click "Grouped Bar" → "Line Chart": Should animate smoothly
  - [ ] No layout shift or jank

### Task Group 7: Persist Chart Type Selection (AC: #4)
- [ ] Use session storage hook from Story 5.7
  - [ ] Import: `import { useSessionStorage } from '@/hooks/useSessionStorage';`
  - [ ] State: `const [chartType, setChartType] = useSessionStorage<ChartType>('benchmark-chart-type', 'bar');`
  - [ ] Key: `benchmark-chart-type`
  - [ ] Default value: `'bar'` (grouped bar chart)
- [ ] Session storage behavior
  - [ ] Persists during tab session (survives page refresh)
  - [ ] Cleared when tab closed (not permanent)
  - [ ] Per-origin storage (not shared across domains)
- [ ] Handle invalid stored values
  - [ ] If stored type not in CHART_TYPE_OPTIONS: Reset to default ('bar')
  - [ ] Filter: `storedType === 'bar' || storedType === 'line' ? storedType : 'bar'`

### Task Group 8: Chart Type Context (Optional Enhancement) (AC: #5)
- [ ] Consider different optimal chart types per section
  - [ ] Benchmark comparison: Grouped bar (default) or Line
  - [ ] Pricing comparison: Horizontal stacked bar (no switcher needed)
  - [ ] Capabilities: Matrix table (no chart)
- [ ] Switcher only on benchmark chart
  - [ ] Benchmark chart benefits from both bar and line views
  - [ ] Pricing chart: Stacked bar is optimal (no alternative)
  - [ ] Don't add switcher to pricing chart (confusing)
- [ ] Future: Context-aware defaults
  - [ ] Many benchmarks (>8): Default to line chart (less cluttered)
  - [ ] Few benchmarks (≤5): Default to grouped bar (easier comparison)
  - [ ] Not implemented in MVP (always default to bar)

### Task Group 9: Accessibility (AC: #1, #3)
- [ ] Button group accessibility
  - [ ] ARIA: `role="group"`, `aria-label="Chart type selector"`
  - [ ] Active button: `aria-pressed="true"` or `aria-current="true"`
  - [ ] Inactive button: `aria-pressed="false"`
  - [ ] Example:
    ```typescript
    <button
      onClick={() => onTypeChange('bar')}
      aria-pressed={currentType === 'bar'}
      aria-label="Switch to grouped bar chart"
    >
      Grouped Bar
    </button>
    ```
  - [ ] Keyboard navigation: Tab between buttons, Enter/Space to activate
- [ ] Visual focus indicator
  - [ ] Focus ring: `focus:ring-2 focus:ring-blue-500 focus:outline-none`
  - [ ] Visible on keyboard navigation
- [ ] Screen reader announcements
  - [ ] Announce chart type change: "Chart type changed to line chart"
  - [ ] Use live region (optional): `<div aria-live="polite" className="sr-only">`

### Task Group 10: Type Definitions (AC: #2)
- [ ] Update `apps/web/src/types/charts.ts`
  - [ ] Define `ChartTypeSwitcherProps` interface
  - [ ] Define `ChartType` type: `'bar' | 'line'`
  - [ ] Define `ChartTypeOption` interface (already in config)
- [ ] Ensure BaseChart supports both types
  - [ ] BaseChart `type` prop: `'bar' | 'line' | 'pie' | 'radar'`
  - [ ] Type parameter: `<T extends ChartType>`
  - [ ] Already flexible (no changes needed)

### Task Group 11: Responsive Behavior (AC: #1)
- [ ] Desktop layout (≥1024px)
  - [ ] Button group full size: `px-4 py-2`
  - [ ] Icons visible: `w-4 h-4`
  - [ ] Text visible: Full labels
- [ ] Tablet layout (768-1023px)
  - [ ] Same as desktop
  - [ ] Button group may wrap if combined with MetricSelector
- [ ] Mobile layout (<768px)
  - [ ] Button group smaller: `px-3 py-1.5`
  - [ ] Icons only (hide text): `<BarChart3 className="w-5 h-5" />`
  - [ ] Or stack vertically: Switcher above MetricSelector
  - [ ] Recommendation: Keep text, reduce padding

### Task Group 12: Handle Chart Type Incompatibilities (AC: #5)
- [ ] Bar chart: Works with any number of benchmarks
  - [ ] 2 benchmarks: Wide bars, easy comparison
  - [ ] 10 benchmarks: Narrow bars, still readable
- [ ] Line chart: Better for many benchmarks
  - [ ] 2 benchmarks: Only 2 points, not much of a trend
  - [ ] 10 benchmarks: Clear trend line, better visualization
- [ ] Conditional display (optional)
  - [ ] If <3 benchmarks selected: Hide "Line Chart" button (not useful)
  - [ ] Show tooltip: "Select at least 3 benchmarks to use line chart"
  - [ ] Not implemented in MVP (allow all chart types)
- [ ] Empty state
  - [ ] If no benchmarks selected: Gray out switcher (disabled)
  - [ ] No chart to switch between

### Task Group 13: Performance Optimization (AC: #3)
- [ ] Memoize chart options per type
  - [ ] Use `useMemo`:
    ```typescript
    const chartOptions = useMemo(() => {
      return chartType === 'bar' ? barChartOptions : lineChartOptions;
    }, [chartType]);
    ```
  - [ ] Only recalculate when chartType changes
- [ ] Avoid re-creating chart instance
  - [ ] Chart.js updates existing chart on type change (no destroy + create)
  - [ ] React-chartjs-2 handles this automatically
  - [ ] No performance concern
- [ ] Test transition performance
  - [ ] Chart type switch: <300ms (smooth)
  - [ ] No lag or freeze

### Task Group 14: Testing and Verification (AC: #1-5)
- [ ] Write unit test for ChartTypeSwitcher component
  - [ ] Render with currentType='bar'
  - [ ] Verify "Grouped Bar" button active (blue background)
  - [ ] Verify "Line Chart" button inactive (white background)
  - [ ] Click "Line Chart" button
  - [ ] Verify onTypeChange called with 'line'
  - [ ] Use Vitest + React Testing Library
- [ ] Write integration test for chart type switching
  - [ ] Render BenchmarkBarChart with default type 'bar'
  - [ ] Verify grouped bar chart displayed
  - [ ] Click "Line Chart" button
  - [ ] Verify chart re-renders as line chart
  - [ ] Verify session storage updated
  - [ ] Use Vitest + React Testing Library
- [ ] Manual E2E testing
  - [ ] Navigate to `/compare?models=1,2,3`
  - [ ] Scroll to "Benchmark Performance Comparison" section
  - [ ] Verify chart type switcher above chart (left or right of MetricSelector)
  - [ ] Default: "Grouped Bar" button active (blue)
  - [ ] Click "Line Chart": Chart smoothly transitions to line chart
  - [ ] Verify line chart: Connected lines, points visible, curved (tension)
  - [ ] Hover over line: Tooltip shows all models at that benchmark
  - [ ] Click "Grouped Bar": Chart transitions back to grouped bar
  - [ ] Refresh page: Chart type persists (session storage)
  - [ ] Close tab, reopen: Chart type resets to default (session ended)

## Dev Notes

### Architecture Alignment
- **Component Separation**: ChartTypeSwitcher is a reusable UI component (could be used for other charts)
- **State Management**: Chart type stored in session storage (persists during session, not permanent)
- **Configuration**: Chart type options in `config/chartTypes.ts` (easy to extend)
- **No Backend Changes**: Frontend-only story, uses existing chart data
- **Reusability**: BaseChart already supports multiple chart types (no changes needed)

### Segmented Control Pattern
Segmented control (button group) is standard UI pattern for mutually exclusive options:
```
┌─────────────────────────────────────────┐
│ ■ Grouped Bar  │  Line Chart          │  ← Bar active
└─────────────────────────────────────────┘

┌─────────────────────────────────────────┐
│  Grouped Bar   │ ■ Line Chart          │  ← Line active
└─────────────────────────────────────────┘
```

Benefits:
- Clear visual indication of active option (blue background)
- Easy to understand (one active, one inactive)
- Compact (fits in toolbar)
- Familiar pattern (used in iOS, macOS, web apps)

### Bar Chart vs Line Chart Use Cases

**Grouped Bar Chart:**
- ✅ Comparing values across models for each benchmark
- ✅ Discrete comparisons (which model scored higher?)
- ✅ Small number of benchmarks (2-8)
- ✅ Emphasizes individual values
- ❌ Many benchmarks (>10) gets cluttered

**Line Chart:**
- ✅ Showing trends across multiple benchmarks
- ✅ Continuous patterns (is model improving across benchmarks?)
- ✅ Large number of benchmarks (8+)
- ✅ Emphasizes overall pattern
- ❌ Small number of benchmarks (<3) not useful

### Chart.js Transition Animation
Chart.js automatically animates chart type changes:
- 300ms default animation duration
- Morph animation: Bars → Points/Lines
- Smooth color transitions
- No custom code needed

Example: Grouped bar → Line chart
1. Bars shrink to points (300ms)
2. Points connect with lines (fade in)
3. Colors transition smoothly

### Session Storage Strategy
Chart type persists during session but resets on tab close:
- **Rationale**: Chart type is a viewing preference, not a user-wide setting
- **Session scoped**: Lasts while tab open
- **Cleared**: When tab/browser closed
- **Per-comparison**: Different comparisons can have different chart types

Alternative: Local storage (permanent) - Not used because:
- User might prefer different chart types for different comparisons
- Avoids stale preferences

### Line Chart Visual Properties
Optimized for readability:
```typescript
{
  borderColor: '#10b981',       // Green (solid line)
  borderWidth: 3,               // Thicker than bar borders (2px)
  backgroundColor: 'rgba(16, 185, 129, 0.1)',  // Very transparent fill
  fill: false,                  // No area fill (cleaner)
  tension: 0.4,                 // Smooth curves (0 = straight lines, 1 = very curved)
  pointRadius: 4,               // Visible points
  pointHoverRadius: 6,          // Larger on hover
  pointBackgroundColor: '#10b981',  // Match line color
  pointBorderColor: '#ffffff',  // White border (stands out)
  pointBorderWidth: 2,          // Visible border
}
```

Curved lines (`tension: 0.4`) look more professional than jagged straight lines.

### Chart Type Switcher Position
Two layout options:

**Option A: Left of MetricSelector**
```
┌──────────────────────────────────────────────┐
│ Chart Type: [■ Bar] [Line]   Benchmarks: [▼]│
└──────────────────────────────────────────────┘
```
Pros: Logical left-to-right flow (type → data)
Cons: May wrap on mobile

**Option B: Above MetricSelector**
```
┌──────────────────────────────────────────────┐
│ Chart Type: [■ Bar] [Line]                   │
│ Benchmarks: [▼]                              │
└──────────────────────────────────────────────┘
```
Pros: Never wraps, more vertical space
Cons: More vertical space used

Recommendation: Option A (same row) for desktop, Option B (stack) for mobile.

### Future Chart Types (Out of Scope)
Documented for reference but not implemented in this story:

**Radar Chart:**
- Use case: Multi-dimensional comparison (5+ benchmarks)
- Visual: Pentagon/hexagon with model lines
- Best for: Holistic performance view
- Chart.js support: `type: 'radar'`

**Scatter Plot:**
- Use case: Correlation analysis (price vs performance)
- Visual: X=price, Y=performance, points=models
- Best for: Value analysis (QAPS-like)
- Chart.js support: `type: 'scatter'`

**Area Chart:**
- Use case: Line chart with filled area under lines
- Visual: Similar to line, but with gradient fill
- Best for: Emphasizing magnitude
- Implementation: `fill: true` on line chart datasets

### Prerequisites
- **Story 5.7**: MetricSelector component establishes toolbar pattern above chart
- **Story 5.6**: BenchmarkBarChart with grouped bar chart
- **Story 5.5**: BaseChart wrapper supports multiple chart types
- No new dependencies required

### Quality Gates
- TypeScript strict mode: ✅ Zero `any` types
- Switcher renders: ✅ Button group with 2 options
- Chart types work: ✅ Bar and Line charts render correctly
- Smooth transition: ✅ Chart animates on type change (<300ms)
- Persistence: ✅ Chart type survives page refresh (session storage)
- Responsive: ✅ Switcher adapts to mobile/tablet/desktop
- Accessibility: ✅ ARIA labels, keyboard navigation, focus indicators
- Performance: ✅ Type switch in <300ms

### Project Structure Notes
```
apps/web/src/
├── components/
│   └── charts/
│       ├── BenchmarkBarChart.tsx          # Updated: Add ChartTypeSwitcher
│       ├── ChartTypeSwitcher.tsx          # New component (this story)
│       └── BaseChart.tsx                  # From Story 5.5 (no changes)
├── config/
│   └── chartTypes.ts                      # New config (this story)
│       ├── CHART_TYPE_OPTIONS
│       └── DEFAULT_CHART_TYPE
└── types/
    └── charts.ts                          # Updated: ChartTypeSwitcherProps
```

### Performance Considerations
- Chart type switch: <300ms (Chart.js animation)
- Session storage read/write: <5ms (negligible)
- No performance impact on chart rendering (same data, different visualization)
- React.memo on ChartTypeSwitcher prevents unnecessary re-renders

### Data Flow
```
User clicks "Line Chart" button
  → ChartTypeSwitcher
    → onTypeChange('line')
      → setChartType('line')  // useSessionStorage
        → sessionStorage.setItem('benchmark-chart-type', 'line')
          → BenchmarkBarChart re-renders
            → chartOptions = lineChartOptions (useMemo)
            → <BaseChart type="line" data={chartData} options={lineChartOptions} />
              → react-chartjs-2 <Line />
                → Chart.js animates bar → line transition (300ms)
```

### References
- [Source: docs/tech-spec-epic-5.md#Services and Modules] - ChartTypeSwitcher component spec
- [Source: docs/tech-spec-epic-5.md#Acceptance Criteria] - AC-5.10: Chart type switcher requirements
- [Source: docs/epics.md#Story 5.10] - Original story with 5 acceptance criteria
- [Source: docs/stories/story-5.7.md] - MetricSelector component (toolbar pattern reference)
- [Source: docs/stories/story-5.6.md] - BenchmarkBarChart (integration point)
- [Source: docs/stories/story-5.5.md] - BaseChart wrapper (supports multiple types)

## Dev Agent Record

### Context Reference

<!-- Path(s) to story context XML will be added here by context workflow -->

### Agent Model Used

claude-sonnet-4-5-20250929

### Debug Log References

### Completion Notes List

### File List
