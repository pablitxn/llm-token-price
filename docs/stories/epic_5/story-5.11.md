# Story 5.11: Implement Chart Interactions (Hover, Click)

Status: Draft

## Story

As a user,
I want interactive charts with hover tooltips and click handlers,
so that I can explore comparison data in detail and control what information is displayed.

## Acceptance Criteria

1. Hovering over chart elements shows detailed tooltip with exact values
2. Tooltip displays: model name, benchmark name, score (with context)
3. Clicking legend items toggles model dataset visibility (hide/show specific models)
4. Smooth animations/transitions when hiding/showing data (Chart.js default)
5. Keyboard navigation support for accessibility (arrow keys, Enter/Space)

## Tasks / Subtasks

### Task Group 1: Enhanced Tooltip Configuration (AC: #1, #2)
- [ ] Update tooltip configuration in chartDefaults.ts
  - [ ] Already have basic tooltips from Story 5.5
  - [ ] Enhance with custom callbacks for richer information
  - [ ] Tooltip options:
    ```typescript
    tooltip: {
      enabled: true,
      mode: 'index',  // Show all datasets at same X position
      intersect: false,  // Trigger on hover anywhere in column
      backgroundColor: '#1f2937',  // gray-800 (dark)
      titleColor: '#ffffff',
      bodyColor: '#d1d5db',  // gray-300
      borderColor: '#374151',  // gray-700
      borderWidth: 1,
      padding: 12,
      cornerRadius: 8,
      titleFont: {
        family: 'Inter, sans-serif',
        size: 14,
        weight: 'bold',
      },
      bodyFont: {
        family: 'Inter, sans-serif',
        size: 13,
      },
      displayColors: true,  // Show colored box next to model name
      boxWidth: 12,
      boxHeight: 12,
      usePointStyle: true,  // Circular color indicators
      callbacks: {
        // Custom title (benchmark name)
        title: (tooltipItems) => {
          return tooltipItems[0].label;  // "MMLU"
        },
        // Custom label (model name + score)
        label: (context) => {
          const modelName = context.dataset.label;
          const score = context.parsed.y;
          return `${modelName}: ${score.toFixed(1)}`;  // "GPT-4 Turbo: 86.4"
        },
        // Optional footer (additional context)
        footer: (tooltipItems) => {
          const scores = tooltipItems.map(item => item.parsed.y);
          const max = Math.max(...scores);
          const maxModel = tooltipItems.find(item => item.parsed.y === max);
          return `Highest: ${maxModel?.dataset.label} (${max.toFixed(1)})`;
        },
      },
    }
    ```
  - [ ] Export as `enhancedTooltipOptions` for reuse across charts

### Task Group 2: Benchmark Chart Tooltip Customization (AC: #2)
- [ ] Create benchmark-specific tooltip callbacks
  - [ ] File: `apps/web/src/config/tooltipCallbacks.ts`
  - [ ] Function: `getBenchmarkTooltipCallbacks()`
  - [ ] Implementation:
    ```typescript
    export const getBenchmarkTooltipCallbacks = () => ({
      title: (tooltipItems: TooltipItem<'bar' | 'line'>[]) => {
        // Show benchmark name
        return tooltipItems[0].label;  // "MMLU"
      },
      label: (context: TooltipItem<'bar' | 'line'>) => {
        const modelName = context.dataset.label;
        const score = context.parsed.y;
        const percentage = `${score.toFixed(1)}%`;
        return `${modelName}: ${percentage}`;  // "GPT-4 Turbo: 86.4%"
      },
      footer: (tooltipItems: TooltipItem<'bar' | 'line'>[]) => {
        // Show highest scorer
        const scores = tooltipItems.map(item => item.parsed.y);
        const maxScore = Math.max(...scores);
        const bestModel = tooltipItems.find(item => item.parsed.y === maxScore);

        if (bestModel) {
          return `Best: ${bestModel.dataset.label}`;
        }
        return '';
      },
    });
    ```
  - [ ] Apply to BenchmarkBarChart and line chart options

### Task Group 3: Pricing Chart Tooltip Customization (AC: #2)
- [ ] Create pricing-specific tooltip callbacks
  - [ ] Function: `getPricingTooltipCallbacks()` in `tooltipCallbacks.ts`
  - [ ] Implementation:
    ```typescript
    export const getPricingTooltipCallbacks = () => ({
      title: (tooltipItems: TooltipItem<'bar'>[]) => {
        // Show model name (from Y-axis in horizontal chart)
        return tooltipItems[0].label;  // "GPT-4 Turbo"
      },
      label: (context: TooltipItem<'bar'>) => {
        const priceType = context.dataset.label;  // "Input Price" or "Output Price"
        const price = context.parsed.x;  // Horizontal bar chart uses x for values
        return `${priceType}: $${price.toFixed(2)}`;  // "Input Price: $10.00"
      },
      footer: (tooltipItems: TooltipItem<'bar'>[]) => {
        // Show total price (sum of input + output)
        const total = tooltipItems.reduce((sum, item) => sum + item.parsed.x, 0);
        return `Total: $${total.toFixed(2)} per 1M tokens`;
      },
    });
    ```
  - [ ] Apply to PricingComparisonChart options

### Task Group 4: Legend Click Interaction (AC: #3)
- [ ] Enable legend click to toggle datasets
  - [ ] Chart.js has this built-in (enabled by default)
  - [ ] Verify in chartDefaults.ts:
    ```typescript
    plugins: {
      legend: {
        display: true,
        position: 'top',
        onClick: (e, legendItem, legend) => {
          // Chart.js default behavior: toggle dataset visibility
          const index = legendItem.datasetIndex;
          const chart = legend.chart;
          const meta = chart.getDatasetMeta(index);

          // Toggle visibility
          meta.hidden = meta.hidden === null ? !chart.data.datasets[index].hidden : null;
          chart.update();  // Re-render chart
        },
        labels: {
          usePointStyle: true,
          padding: 16,
          generateLabels: (chart) => {
            // Custom legend labels with strike-through for hidden datasets
            return chart.data.datasets.map((dataset, i) => {
              const meta = chart.getDatasetMeta(i);
              const hidden = meta.hidden;

              return {
                text: dataset.label,
                fillStyle: dataset.backgroundColor,
                strokeStyle: dataset.borderColor,
                lineWidth: dataset.borderWidth,
                hidden: hidden,
                datasetIndex: i,
                fontColor: hidden ? '#9ca3af' : '#374151',  // Gray if hidden
                textDecoration: hidden ? 'line-through' : '',  // Strike-through
              };
            });
          },
        },
      },
    }
    ```
  - [ ] No custom code needed for basic toggle (Chart.js default works well)

### Task Group 5: Visual Feedback for Hidden Datasets (AC: #3, #4)
- [ ] Legend item styling for hidden datasets
  - [ ] Hidden dataset: Gray text color (`#9ca3af`)
  - [ ] Hidden dataset: Lower opacity (0.5)
  - [ ] Hidden dataset: Strike-through text decoration (optional)
  - [ ] Active dataset: Normal text color (`#374151`)
  - [ ] Active dataset: Full opacity (1.0)
- [ ] Smooth transition animation
  - [ ] Chart.js animates dataset hide/show automatically
  - [ ] Default animation duration: 300ms
  - [ ] Fade out hidden dataset, fade in shown dataset
  - [ ] No custom animation code needed
- [ ] Chart rescaling on hide/show
  - [ ] Y-axis auto-rescales when datasets hidden
  - [ ] Example: Hide highest scorer → Y-axis max reduces to next highest
  - [ ] Smooth transition (Chart.js default)

### Task Group 6: Tooltip Positioning (AC: #1)
- [ ] Smart tooltip positioning
  - [ ] Chart.js auto-positions tooltips to stay within canvas
  - [ ] Default: Above cursor if space, below if not
  - [ ] Custom positioning (optional):
    ```typescript
    tooltip: {
      position: 'nearest',  // Position near cursor
      yAlign: 'bottom',  // Prefer below cursor
      xAlign: 'center',  // Center horizontally
    }
    ```
  - [ ] No custom positioning needed (default works well)
- [ ] Tooltip scrolling (for long content)
  - [ ] Max height: 300px
  - [ ] Overflow: `overflow-y: auto` (if many datasets)
  - [ ] Not applicable for 2-5 models (fits comfortably)

### Task Group 7: Hover Interaction Modes (AC: #1)
- [ ] Configure interaction modes for different chart types
  - [ ] **Benchmark bar chart**: `mode: 'index'`, `intersect: false`
    - Hover anywhere in column → Show all models for that benchmark
    - Makes comparison easy (see all models at once)
  - [ ] **Benchmark line chart**: `mode: 'index'`, `intersect: false`
    - Hover anywhere → Vertical line shows all models
    - Tooltip shows all values at that X position
  - [ ] **Pricing chart**: `mode: 'index'`, `intersect: false`
    - Hover anywhere in bar → Show both input and output prices
    - Tooltip footer shows total
- [ ] Interaction configuration:
  ```typescript
  interaction: {
    mode: 'index',  // Show all datasets at same index
    intersect: false,  // Don't require hovering directly on element
    axis: 'x',  // Group by X-axis (benchmark/model)
  }
  ```

### Task Group 8: Keyboard Navigation (AC: #5)
- [ ] Chart.js canvas not keyboard accessible by default (limitation)
  - [ ] Canvas doesn't support Tab navigation
  - [ ] Solution: Add keyboard controls to chart container
- [ ] Implement custom keyboard navigation wrapper
  - [ ] Create component: `apps/web/src/components/charts/KeyboardNavigableChart.tsx`
  - [ ] Wrapper around BaseChart
  - [ ] Add tabindex: `<div tabIndex={0} onKeyDown={handleKeyDown}>`
  - [ ] Keyboard handlers:
    ```typescript
    const handleKeyDown = (e: React.KeyboardEvent) => {
      switch (e.key) {
        case 'ArrowRight':
          // Move to next data point
          highlightNextDataPoint();
          break;
        case 'ArrowLeft':
          // Move to previous data point
          highlightPreviousDataPoint();
          break;
        case 'Enter':
        case ' ':
          // Toggle legend item (simulate click)
          toggleDataset();
          break;
        case 'Escape':
          // Clear highlight
          clearHighlight();
          break;
      }
    };
    ```
  - [ ] Not implemented in MVP (complex, limited value)
  - [ ] Alternative: Focus on legend buttons (already keyboard accessible)

### Task Group 9: Accessible Chart Interactions (AC: #5)
- [ ] Ensure legend items are keyboard accessible
  - [ ] Legend items are `<button>` elements (focusable)
  - [ ] Tab through legend items
  - [ ] Enter/Space to toggle dataset visibility
  - [ ] Already handled by Chart.js (generates accessible HTML)
- [ ] Add ARIA labels to chart container
  - [ ] Chart container: `role="img"`, `aria-label="Interactive bar chart..."`
  - [ ] Live region for announcements (optional):
    ```typescript
    <div aria-live="polite" aria-atomic="true" className="sr-only">
      {announcement}
    </div>
    ```
  - [ ] Announce legend toggles: "GPT-4 Turbo dataset hidden"
- [ ] Screen reader support
  - [ ] Provide text alternative: Link to data table
  - [ ] Example: "View data in table format below chart"
  - [ ] Data table from Story 5.3 serves as accessible alternative

### Task Group 10: Custom Tooltip Styling (AC: #2)
- [ ] Match TailwindCSS design system
  - [ ] Background: `gray-800` (#1f2937)
  - [ ] Text: White (#ffffff) for title, `gray-300` (#d1d5db) for body
  - [ ] Border: `gray-700` (#374151)
  - [ ] Padding: 12px
  - [ ] Corner radius: 8px (rounded-lg)
  - [ ] Font: Inter, sans-serif
- [ ] Color indicators in tooltip
  - [ ] `displayColors: true` - Show colored box next to model name
  - [ ] `usePointStyle: true` - Circular indicators (match chart points)
  - [ ] Box size: 12x12px
- [ ] Multi-line tooltip formatting
  - [ ] Title: Benchmark name (bold, 14px)
  - [ ] Body: Model scores (13px, one per line)
  - [ ] Footer: Best model (12px, gray)
  - [ ] Separator line between body and footer (optional)

### Task Group 11: Tooltip HTML Content (Advanced, Optional) (AC: #2)
- [ ] Chart.js tooltips are canvas-rendered (not HTML)
  - [ ] Limitation: Can't use HTML elements (no links, buttons, images)
  - [ ] Alternative: chartjs-plugin-htmllegend for HTML tooltips
- [ ] Custom HTML tooltip plugin (optional, out of MVP scope)
  - [ ] Plugin: `chartjs-plugin-datalabels` or custom implementation
  - [ ] Allows rich HTML in tooltips
  - [ ] Example: Show model icon, provider logo, link to model details
  - [ ] Not implemented in MVP (canvas tooltips sufficient)

### Task Group 12: Touch Device Interactions (AC: #1, #3)
- [ ] Touch events for mobile/tablet
  - [ ] Tap bar/point → Show tooltip (Chart.js default)
  - [ ] Tap legend → Toggle dataset (Chart.js default)
  - [ ] Long press → Persistent tooltip (not supported, out of scope)
- [ ] Touch-optimized tooltip
  - [ ] Larger tap targets (Chart.js handles this)
  - [ ] Tooltip positioning: Avoid finger occlusion
  - [ ] Chart.js auto-positions tooltip above tap point
- [ ] Test on touch devices
  - [ ] Tablet: Verify tap interactions work
  - [ ] Mobile: Verify tooltips readable, legend toggles work

### Task Group 13: Animate Data Changes (AC: #4)
- [ ] Chart.js animation configuration
  - [ ] Already configured in chartDefaults.ts (Story 5.5)
  - [ ] Default animation: 300ms ease-out
  - [ ] Animates: Dataset hide/show, data updates, chart type changes
- [ ] Smooth transitions for:
  - [ ] Hiding dataset: Fade out + rescale (300ms)
  - [ ] Showing dataset: Fade in + rescale (300ms)
  - [ ] Changing benchmarks: Morph bars/lines (300ms)
  - [ ] Switching chart types: Transform shape (300ms)
- [ ] Custom animation duration (optional)
  - [ ] Faster animations: 200ms (snappier, less smooth)
  - [ ] Slower animations: 500ms (smoother, feels slow)
  - [ ] Recommendation: Keep default 300ms

### Task Group 14: Zoom and Pan (Optional, Out of MVP Scope) (AC: #5)
- [ ] Chart.js zoom plugin: `chartjs-plugin-zoom`
  - [ ] Allows mouse wheel zoom, pinch-to-zoom, pan
  - [ ] Install: `pnpm add chartjs-plugin-zoom`
  - [ ] Register plugin in chart-setup.ts
  - [ ] Configuration:
    ```typescript
    import zoomPlugin from 'chartjs-plugin-zoom';
    ChartJS.register(zoomPlugin);

    // In chart options:
    plugins: {
      zoom: {
        zoom: {
          wheel: { enabled: true },  // Mouse wheel zoom
          pinch: { enabled: true },  // Touch pinch zoom
          mode: 'xy',  // Zoom both axes
        },
        pan: {
          enabled: true,
          mode: 'xy',  // Pan both axes
        },
        limits: {
          x: { min: 0, max: 100 },
          y: { min: 0, max: 100 },
        },
      },
    }
    ```
  - [ ] Not implemented in MVP (low priority)
  - [ ] Document for future enhancement

### Task Group 15: Testing and Verification (AC: #1-5)
- [ ] Write unit tests for tooltip callbacks
  - [ ] Test `getBenchmarkTooltipCallbacks`: Returns correct title, label, footer
  - [ ] Test `getPricingTooltipCallbacks`: Returns correct pricing format
  - [ ] Mock TooltipItem objects
  - [ ] Use Vitest
- [ ] Write integration tests for chart interactions
  - [ ] Render BenchmarkBarChart
  - [ ] Simulate hover over bar (fireEvent.mouseOver)
  - [ ] Verify tooltip appears (check for tooltip element)
  - [ ] Simulate legend click (fireEvent.click)
  - [ ] Verify dataset hidden/shown
  - [ ] Use Vitest + React Testing Library + @testing-library/user-event
- [ ] Manual E2E testing
  - [ ] Navigate to `/compare?models=1,2,3`
  - [ ] **Hover interactions:**
    - Hover over benchmark bar: Tooltip shows all 3 models' scores
    - Tooltip displays: Benchmark name (title), Model: Score (body), Best: Model (footer)
    - Verify tooltip styled correctly (dark background, white text)
  - [ ] **Legend interactions:**
    - Click "GPT-4 Turbo" in legend: Dataset fades out (300ms)
    - Verify GPT-4 bars hidden, Y-axis rescales
    - Verify legend item grayed out or strike-through
    - Click again: Dataset fades in (300ms)
  - [ ] **Keyboard interactions:**
    - Tab to legend items: Verify focus ring visible
    - Press Enter on legend item: Toggles dataset
    - Verify smooth animations throughout
  - [ ] **Pricing chart:**
    - Hover over stacked bar: Tooltip shows input + output + total
    - Verify footer displays total price
  - [ ] **Mobile/tablet:**
    - Tap bar: Tooltip appears
    - Tap legend: Dataset toggles
    - Verify tooltips readable on small screen

## Dev Notes

### Architecture Alignment
- **Tooltip Configuration**: Centralized in `config/tooltipCallbacks.ts` (reusable across charts)
- **Chart.js Native Features**: Leverage built-in legend toggle, animations (no custom code)
- **Accessibility**: Focus on keyboard-accessible legend, screen reader support
- **No Backend Changes**: Frontend-only enhancements
- **Gradual Enhancement**: Basic interactions in MVP, zoom/pan in future

### Tooltip Interaction Modes

**Mode: 'index', Intersect: false (Recommended)**
- Hover anywhere in column → Show all datasets
- Example: Hover near "MMLU" → See GPT-4, Claude, Gemini scores
- Best for comparison (see all models at once)

**Mode: 'point', Intersect: true (Alternative)**
- Hover directly on bar/point → Show only that dataset
- Example: Hover directly on GPT-4 bar → See only GPT-4 score
- Less useful for comparison

**Mode: 'nearest', Intersect: false**
- Hover near any element → Show nearest dataset
- Good for line charts (show closest point)

Recommendation: `mode: 'index'` for all comparison charts.

### Legend Toggle Behavior
Chart.js built-in legend click:
1. User clicks "GPT-4 Turbo" in legend
2. Chart.js finds dataset index (0)
3. Gets dataset metadata: `chart.getDatasetMeta(0)`
4. Toggles `meta.hidden` property
5. Calls `chart.update()` to re-render
6. Animation: 300ms fade out + Y-axis rescale

No custom code needed (works out of the box).

### Tooltip Callback Types
Chart.js provides three tooltip callbacks:

**title(tooltipItems):**
- Purpose: Tooltip header
- Usage: Show benchmark name or model name
- Return: String (e.g., "MMLU")

**label(context):**
- Purpose: Tooltip body (one line per dataset)
- Usage: Show model name + score
- Return: String (e.g., "GPT-4 Turbo: 86.4%")

**footer(tooltipItems):**
- Purpose: Tooltip footer (summary/context)
- Usage: Show best model or total price
- Return: String (e.g., "Best: Claude 3 Opus")

All callbacks receive TooltipItem objects with full context (dataset, parsed values, chart instance).

### Keyboard Navigation Challenges
Canvas elements (used by Chart.js) are not keyboard accessible by default:
- ❌ Can't Tab to chart
- ❌ Can't use arrow keys to navigate data points
- ❌ Can't press Enter to interact with chart elements

**Solution 1: Make legend keyboard accessible** (implemented)
- Legend items are HTML buttons (already focusable)
- Tab through legend, Enter to toggle
- Sufficient for MVP

**Solution 2: Custom keyboard wrapper** (out of scope)
- Wrap chart in focusable div
- Implement arrow key navigation
- Complex, limited ROI for comparison use case

**Solution 3: Accessible data table** (already done)
- ComparisonTable from Story 5.3 serves as accessible alternative
- Fully keyboard navigable
- Screen readers can read all data

### Tooltip Positioning Strategy
Chart.js auto-positions tooltips:
- Prefers above cursor (doesn't block view)
- Falls back to below if no space above
- Centers horizontally on cursor
- Avoids canvas edges (stays within bounds)

Custom positioning rarely needed (default works well).

### Animation Performance
Chart.js animations use requestAnimationFrame (smooth 60fps):
- Dataset hide/show: <50ms processing + 300ms animation
- Data update: <100ms processing + 300ms animation
- Chart type change: <200ms processing + 300ms animation

Performance targets:
- ✅ <100ms: Chart responds to interaction
- ✅ 300ms: Animation duration (feels snappy, not sluggish)
- ✅ 60fps: Smooth animation (no dropped frames)

### Touch vs Mouse Interactions
Chart.js handles both automatically:

**Mouse:**
- Hover → Show tooltip
- Click legend → Toggle dataset
- Move → Tooltip follows cursor

**Touch:**
- Tap → Show tooltip (persists briefly)
- Tap legend → Toggle dataset
- Drag → Pan (if zoom plugin enabled)

No custom code needed (Chart.js detects input type).

### Prerequisites
- **Story 5.8**: PricingComparisonChart (integration point for pricing tooltips)
- **Story 5.6**: BenchmarkBarChart (integration point for benchmark tooltips)
- **Story 5.5**: BaseChart wrapper, chartDefaults.ts (tooltip configuration)
- **Story 5.3**: ComparisonTable (accessible alternative to charts)
- No new dependencies required (Chart.js built-in features)

### Quality Gates
- TypeScript strict mode: ✅ Zero `any` types in tooltip callbacks
- Tooltips work: ✅ Hover shows detailed information
- Tooltips styled: ✅ Match TailwindCSS design (dark background, white text)
- Legend toggle: ✅ Click hides/shows datasets with animation
- Smooth animations: ✅ 300ms transitions, no jank
- Keyboard accessible: ✅ Legend items focusable, Enter/Space works
- Touch support: ✅ Tap interactions work on mobile/tablet
- Performance: ✅ Interactions respond in <100ms

### Project Structure Notes
```
apps/web/src/
├── config/
│   ├── chartDefaults.ts                   # Updated: Enhanced tooltip config
│   └── tooltipCallbacks.ts                # New file (this story)
│       ├── getBenchmarkTooltipCallbacks()
│       └── getPricingTooltipCallbacks()
├── components/
│   └── charts/
│       ├── BenchmarkBarChart.tsx          # Updated: Apply tooltip callbacks
│       ├── PricingComparisonChart.tsx     # Updated: Apply tooltip callbacks
│       └── BaseChart.tsx                  # No changes
└── types/
    └── charts.ts                          # Updated: Tooltip callback types
```

### Performance Considerations
- Tooltip rendering: <5ms (canvas drawing)
- Legend click: <50ms processing + 300ms animation
- Dataset hide/show: No performance impact (same number of DOM elements)
- Chart.js optimizations: Only redraws affected portions of canvas

### Data Flow
```
User hovers over bar
  → Chart.js detects mouse position
    → Finds tooltip items at cursor (mode: 'index')
      → Calls tooltip.callbacks.title(tooltipItems)
        → Returns "MMLU"
      → Calls tooltip.callbacks.label(context) for each dataset
        → Returns ["GPT-4: 86.4%", "Claude: 86.8%", "Gemini: 79.1%"]
      → Calls tooltip.callbacks.footer(tooltipItems)
        → Returns "Best: Claude 3 Opus"
      → Renders tooltip on canvas (dark box, white text)
        → Positions above cursor (or below if no space)

User clicks legend item "GPT-4"
  → Chart.js legend.onClick handler
    → Gets dataset meta: chart.getDatasetMeta(0)
      → Toggles meta.hidden
        → Calls chart.update()
          → Re-renders chart with GPT-4 dataset hidden (300ms fade)
            → Y-axis rescales to new max (300ms)
```

### References
- [Source: docs/tech-spec-epic-5.md#Services and Modules] - Chart interactions specification
- [Source: docs/tech-spec-epic-5.md#Acceptance Criteria] - AC-5.11: Chart interaction requirements
- [Source: docs/epics.md#Story 5.11] - Original story with 5 acceptance criteria
- [Source: docs/stories/story-5.8.md] - PricingComparisonChart (pricing tooltip integration)
- [Source: docs/stories/story-5.6.md] - BenchmarkBarChart (benchmark tooltip integration)
- [Source: docs/stories/story-5.5.md] - Chart.js setup, chartDefaults.ts

## Dev Agent Record

### Context Reference

<!-- Path(s) to story context XML will be added here by context workflow -->

### Agent Model Used

claude-sonnet-4-5-20250929

### Debug Log References

### Completion Notes List

### File List
