# Story 5.8: Add Pricing Comparison Visualization

Status: Draft

## Story

As a user,
I want to visualize pricing differences between models,
so that I can quickly identify the most cost-effective options for my use case.

## Acceptance Criteria

1. Pricing comparison chart added to comparison page (below benchmark chart)
2. Shows input price, output price, and total cost for each model
3. Bars color-coded: input price (one color) vs output price (different color)
4. Cheapest model highlighted with visual indicator (border, badge, or checkmark)
5. Cost difference vs cheapest shown as percentage (e.g., "+25% vs cheapest")

## Tasks / Subtasks

### Task Group 1: Create PricingComparisonChart Component (AC: #1, #2, #3)
- [ ] Create component file: `apps/web/src/components/charts/PricingComparisonChart.tsx`
  - [ ] Define `PricingComparisonChartProps` interface:
    ```typescript
    interface PricingComparisonChartProps {
      models: ModelDto[];
      height?: number;
      className?: string;
    }
    ```
  - [ ] Create functional component with TypeScript
  - [ ] Export as named export
- [ ] Component layout structure
  - [ ] Section container: `<section className="mt-8 p-6 bg-white border border-gray-200 rounded-lg">`
  - [ ] Section heading: "Pricing Comparison" (`text-2xl font-bold mb-6`)
  - [ ] Description: "Compare input and output pricing per 1M tokens" (`text-gray-600 mb-4`)
  - [ ] Pricing unit note: "All prices shown per 1 million tokens" (`text-sm text-gray-500 mb-4`)
  - [ ] Chart container: `<div className="relative w-full">`
- [ ] Use BaseChart component from Story 5.5
  - [ ] Import: `import { BaseChart } from '@/components/charts/BaseChart';`
  - [ ] Render: `<BaseChart type="bar" data={chartData} options={chartOptions} height={height || 350} />`
  - [ ] Pass transformed pricing data

### Task Group 2: Transform Pricing Data to Chart Format (AC: #2, #3)
- [ ] Create data transformation utility: `apps/web/src/utils/pricingChartData.ts`
  - [ ] Function: `buildPricingChartData(models: ModelDto[]): ChartData<'bar'>`
  - [ ] Input: Array of ModelDto with pricing fields
  - [ ] Output: Chart.js data format (stacked bar chart)
- [ ] Extract model labels (X-axis)
  - [ ] Labels array: Model names
  - [ ] Example: `['GPT-4 Turbo', 'Claude 3 Opus', 'Gemini Pro']`
  - [ ] Shorten long names if needed: Max 20 characters, truncate with "..."
- [ ] Build datasets (stacked bars)
  - [ ] Dataset 1: Input prices
    ```typescript
    {
      label: 'Input Price',
      data: models.map(m => m.inputPricePer1M),  // [10.00, 15.00, 7.00]
      backgroundColor: '#3b82f6',  // blue-500 (input color)
      borderColor: '#2563eb',      // blue-600
      borderWidth: 2,
      stack: 'pricing',            // Enable stacking
    }
    ```
  - [ ] Dataset 2: Output prices
    ```typescript
    {
      label: 'Output Price',
      data: models.map(m => m.outputPricePer1M),  // [30.00, 75.00, 21.00]
      backgroundColor: '#10b981',  // green-500 (output color)
      borderColor: '#059669',      // green-600
      borderWidth: 2,
      stack: 'pricing',            // Same stack as input (stacked bars)
    }
    ```
  - [ ] Stacked bars: Input price at bottom, output price stacked on top
  - [ ] Total visible = input + output (visual addition)

### Task Group 3: Configure Stacked Bar Chart (AC: #3)
- [ ] Create chart options for stacked bar chart
  - [ ] Start with defaultChartOptions from Story 5.5
  - [ ] Override with stacked-specific options:
    ```typescript
    const customOptions: ChartOptions<'bar'> = {
      indexAxis: 'y',  // Horizontal bars (models on Y-axis, prices on X-axis)
      plugins: {
        title: {
          display: false,  // Use React heading
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
              const label = context.dataset.label;
              const value = context.parsed.x;
              return `${label}: $${value.toFixed(2)}`;
            },
            footer: (tooltipItems) => {
              const total = tooltipItems.reduce((sum, item) => sum + item.parsed.x, 0);
              return `Total: $${total.toFixed(2)}`;
            },
          },
        },
      },
      scales: {
        x: {
          stacked: true,  // Enable stacking on X-axis
          beginAtZero: true,
          grid: {
            color: '#e5e7eb',
            lineWidth: 1,
          },
          ticks: {
            font: { family: 'Inter, sans-serif', size: 11 },
            color: '#6b7280',
            callback: (value) => `$${value}`,  // Add dollar sign
          },
          title: {
            display: true,
            text: 'Price per 1M tokens (USD)',
            font: { family: 'Inter, sans-serif', size: 12, weight: 'bold' },
            color: '#374151',
          },
        },
        y: {
          stacked: true,  // Enable stacking on Y-axis
          grid: { display: false },
          ticks: {
            font: { family: 'Inter, sans-serif', size: 11 },
            color: '#6b7280',
          },
        },
      },
      interaction: {
        mode: 'index',  // Hover shows both input + output
        intersect: false,
      },
    };
    ```
  - [ ] Merge with defaults: `mergeChartOptions(defaultChartOptions, customOptions)`
- [ ] Horizontal bar layout (indexAxis: 'y')
  - [ ] Models on Y-axis (vertical list)
  - [ ] Prices on X-axis (horizontal bars)
  - [ ] Easier to read model names (no rotation needed)
  - [ ] Better for comparing prices visually (align left)

### Task Group 4: Identify Cheapest Model (AC: #4)
- [ ] Calculate total cost for each model
  - [ ] Function: `calculateTotalCost(model: ModelDto): number`
  - [ ] Formula: `totalCost = inputPricePer1M + outputPricePer1M`
  - [ ] Example: GPT-4 Turbo = $10.00 + $30.00 = $40.00
- [ ] Find cheapest model
  - [ ] Function: `findCheapestModel(models: ModelDto[]): ModelDto`
  - [ ] Algorithm: `models.reduce((min, m) => calculateTotalCost(m) < calculateTotalCost(min) ? m : min)`
  - [ ] Handle ties: Return first model with lowest price
- [ ] Add visual indicator to cheapest model
  - [ ] Option A: Add border to cheapest bar
    ```typescript
    borderWidth: isCheapest ? 4 : 2,
    borderColor: isCheapest ? '#22c55e' : '#2563eb',  // Green border for cheapest
    ```
  - [ ] Option B: Add badge below chart (recommended for clarity)
    - [ ] Display below chart: "Cheapest: Claude 3 Opus ($22.00 total)"
    - [ ] Style: `text-sm text-green-600 font-semibold`
    - [ ] Icon: CheckCircle from lucide-react
  - [ ] Option C: Add star icon next to model name in Y-axis labels
    - [ ] Custom plugin to add icon to label (complex)
- [ ] Recommendation: Use Option B (badge below chart) for simplicity

### Task Group 5: Calculate Cost Difference Percentage (AC: #5)
- [ ] Create utility function: `apps/web/src/utils/pricingCalculations.ts`
  - [ ] Function: `calculateCostDifference(modelCost: number, cheapestCost: number): string`
  - [ ] Formula: `((modelCost - cheapestCost) / cheapestCost) * 100`
  - [ ] Example: GPT-4 ($40) vs Claude ($22) = ((40 - 22) / 22) * 100 = +81.8%
  - [ ] Return formatted string: "+81.8% vs cheapest" or "Cheapest"
- [ ] Display cost difference on chart
  - [ ] Option A: Add as data labels on bars (Chart.js datalabels plugin)
    ```typescript
    plugins: {
      datalabels: {
        anchor: 'end',
        align: 'end',
        formatter: (value, context) => {
          const modelIndex = context.dataIndex;
          const model = models[modelIndex];
          const totalCost = calculateTotalCost(model);
          return calculateCostDifference(totalCost, cheapestCost);
        },
      },
    }
    ```
  - [ ] Option B: Add as text below chart (table format)
    - [ ] Table with 3 columns: Model, Total Cost, Difference
    - [ ] Example:
      ```
      | Model          | Total Cost | vs Cheapest |
      |----------------|------------|-------------|
      | GPT-4 Turbo    | $40.00     | +81.8%      |
      | Claude 3 Opus  | $22.00     | Cheapest    |
      | Gemini Pro     | $28.00     | +27.3%      |
      ```
  - [ ] Option C: Tooltip only (show on hover)
    - [ ] Custom tooltip footer: "81.8% more expensive than cheapest"
- [ ] Recommendation: Use Option B (table below chart) for clarity + Option C (tooltip)

### Task Group 6: Add Pricing Summary Table (AC: #5)
- [ ] Create PricingSummaryTable sub-component
  - [ ] Renders below chart
  - [ ] Table structure:
    ```typescript
    <table className="mt-4 w-full text-sm">
      <thead className="bg-gray-50 text-left">
        <tr>
          <th className="px-4 py-2 font-semibold text-gray-700">Model</th>
          <th className="px-4 py-2 font-semibold text-gray-700">Input</th>
          <th className="px-4 py-2 font-semibold text-gray-700">Output</th>
          <th className="px-4 py-2 font-semibold text-gray-700">Total</th>
          <th className="px-4 py-2 font-semibold text-gray-700">vs Cheapest</th>
        </tr>
      </thead>
      <tbody>
        {models.map(model => (
          <tr key={model.id} className={isCheapest(model) ? 'bg-green-50' : ''}>
            <td className="px-4 py-2">{model.name}</td>
            <td className="px-4 py-2">${model.inputPricePer1M.toFixed(2)}</td>
            <td className="px-4 py-2">${model.outputPricePer1M.toFixed(2)}</td>
            <td className="px-4 py-2 font-semibold">${totalCost.toFixed(2)}</td>
            <td className={`px-4 py-2 ${isCheapest(model) ? 'text-green-600 font-semibold' : 'text-gray-600'}`}>
              {isCheapest(model) ? 'Cheapest' : `+${costDiff}%`}
            </td>
          </tr>
        ))}
      </tbody>
    </table>
    ```
- [ ] Highlight cheapest row
  - [ ] Background: `bg-green-50` (light green)
  - [ ] Text: `text-green-600 font-semibold`
  - [ ] Icon: CheckCircle from lucide-react in "vs Cheapest" column
- [ ] Sort by total cost (ascending)
  - [ ] Cheapest at top, most expensive at bottom
  - [ ] Makes it easy to scan pricing hierarchy

### Task Group 7: Color Coding for Input vs Output (AC: #3)
- [ ] Define pricing color scheme
  - [ ] Input price: Blue (`#3b82f6` - blue-500)
  - [ ] Output price: Green (`#10b981` - green-500)
  - [ ] Rationale: Blue = incoming data (input), Green = outgoing data (output)
- [ ] Alternative color scheme (if green confusing with "cheapest" indicator):
  - [ ] Input price: Blue (`#3b82f6`)
  - [ ] Output price: Orange (`#f97316` - orange-500)
  - [ ] Avoids confusion with green = cheapest
  - [ ] Recommendation: Use Blue + Orange
- [ ] Legend clarity
  - [ ] Legend shows: "Input Price" (blue) and "Output Price" (orange)
  - [ ] Position: Top of chart
  - [ ] Ensure color contrast for accessibility

### Task Group 8: Integrate into ComparisonPage (AC: #1)
- [ ] Update `ComparisonPage.tsx` to include PricingComparisonChart
  - [ ] Import component: `import { PricingComparisonChart } from '@/components/charts/PricingComparisonChart';`
  - [ ] Add below BenchmarkBarChart: `<PricingComparisonChart models={data.models} />`
  - [ ] Placement: After benchmark chart, before capabilities section (Story 5.9)
- [ ] Section divider
  - [ ] Add horizontal rule: `<hr className="my-8 border-gray-200" />`
  - [ ] Or use spacing: `<div className="h-8" />`
- [ ] Loading state
  - [ ] Show skeleton while `isLoading`
  - [ ] Skeleton: Gray rectangle with `animate-pulse`, match chart dimensions
  - [ ] `<div className="mt-8 p-6 bg-gray-100 rounded-lg animate-pulse" style={{ height: 350 }}></div>`

### Task Group 9: Handle Edge Cases (AC: #2, #4)
- [ ] Missing pricing data
  - [ ] If model has `inputPricePer1M = null` or `outputPricePer1M = null`:
    - Display "N/A" in table
    - Exclude from chart (or show 0 with note)
  - [ ] If no models have pricing: Show empty state message
    - "Pricing data unavailable for selected models"
- [ ] Free tier models (price = 0)
  - [ ] Handle $0.00 pricing gracefully
  - [ ] Show as "Free" in table instead of "$0.00"
  - [ ] Bar chart shows minimal bar (not invisible)
- [ ] Extremely cheap vs expensive models
  - [ ] Example: Free model ($0) vs Enterprise model ($200)
  - [ ] Use logarithmic scale if difference >100x (optional, advanced)
  - [ ] Or show warning: "Large price variation - review table for details"
- [ ] Tie for cheapest
  - [ ] If multiple models have same total cost: Highlight all
  - [ ] "vs Cheapest" column: Show "Cheapest" for all tied models

### Task Group 10: Responsive Behavior (AC: #1)
- [ ] Desktop layout (≥1024px)
  - [ ] Horizontal bars, full width
  - [ ] Chart height: 350px (fits 5 models comfortably)
  - [ ] Table width: 100%
- [ ] Tablet layout (768-1023px)
  - [ ] Same as desktop, may need horizontal scroll for table
  - [ ] Chart height: 350px
- [ ] Mobile layout (<768px)
  - [ ] Chart height: 300px (shorter to fit screen)
  - [ ] Table: Horizontal scroll or stack columns
  - [ ] Consider hiding "vs Cheapest" column on mobile (show in tooltip)
- [ ] Test with varying model counts
  - [ ] 2 models: Chart height 200px (fits comfortably)
  - [ ] 5 models: Chart height 350px
  - [ ] 8+ models: Chart height 500px or scrollable (rare case)

### Task Group 11: Type Definitions (AC: #2, #5)
- [ ] Update `apps/web/src/types/charts.ts`
  - [ ] Define `PricingComparisonChartProps` interface
  - [ ] Define `PricingDataPoint` type:
    ```typescript
    export interface PricingDataPoint {
      modelId: string;
      modelName: string;
      inputPrice: number;
      outputPrice: number;
      totalCost: number;
      costDifference: string;  // e.g., "+81.8%"
      isCheapest: boolean;
    }
    ```
- [ ] Create `apps/web/src/types/pricing.ts` (optional, for pricing-specific types)
  - [ ] `PricingSummaryRow` interface
  - [ ] `CostComparison` interface

### Task Group 12: Accessibility (AC: #1, #3)
- [ ] Chart accessibility
  - [ ] ARIA: `role="img"`, `aria-label="Horizontal bar chart comparing input and output pricing across models"`
  - [ ] Canvas: `aria-hidden="true"`
  - [ ] Screen reader description: "Pricing comparison showing stacked input and output prices"
- [ ] Table accessibility
  - [ ] Use semantic HTML: `<table>`, `<thead>`, `<tbody>`, `<th>`, `<td>`
  - [ ] Table caption: `<caption className="sr-only">Pricing summary table</caption>`
  - [ ] Scope attributes: `<th scope="col">` for headers
- [ ] Color contrast
  - [ ] Ensure blue and orange meet WCAG AA contrast (4.5:1 against white)
  - [ ] Test with color blindness simulator (blue + orange works for most types)
- [ ] Keyboard navigation
  - [ ] Table: Keyboard navigable by default (browser behavior)
  - [ ] Chart: Not keyboard navigable (canvas limitation, addressed in Story 5.11)

### Task Group 13: Performance Optimization (AC: #1)
- [ ] Memoize chart data
  - [ ] Use `useMemo`:
    ```typescript
    const chartData = useMemo(
      () => buildPricingChartData(models),
      [models]
    );
    ```
  - [ ] Only recalculate when models array changes
- [ ] Memoize pricing calculations
  - [ ] Use `useMemo` for cheapest model, cost differences
  - [ ] Avoid recalculating on every render
- [ ] Optimize component re-renders
  - [ ] Use `React.memo()` on PricingComparisonChart
  - [ ] Prevent re-render when unrelated ComparisonPage state changes
- [ ] Test performance
  - [ ] Chart renders in <300ms with 5 models
  - [ ] Table renders instantly (<50ms)

### Task Group 14: Testing and Verification (AC: #1-5)
- [ ] Write unit test for `buildPricingChartData` utility
  - [ ] Test with 3 models with different prices
  - [ ] Verify input dataset has correct values
  - [ ] Verify output dataset has correct values
  - [ ] Verify stacked configuration (`stack: 'pricing'`)
  - [ ] Use Vitest
- [ ] Write unit test for `calculateCostDifference` utility
  - [ ] Test: GPT-4 ($40) vs Claude ($22) = "+81.8%"
  - [ ] Test: Cheapest model = "Cheapest"
  - [ ] Test: Tie for cheapest = "Cheapest"
  - [ ] Test: Rounding to 1 decimal place
  - [ ] Use Vitest
- [ ] Write integration test for PricingComparisonChart component
  - [ ] Render with 3 models
  - [ ] Verify chart heading: "Pricing Comparison"
  - [ ] Verify table shows 3 rows
  - [ ] Verify cheapest model highlighted (green background)
  - [ ] Verify cost difference percentages displayed
  - [ ] Use Vitest + React Testing Library
- [ ] Manual E2E testing
  - [ ] Navigate to `/compare?models=1,2,3`
  - [ ] Scroll to "Pricing Comparison" section
  - [ ] Verify horizontal bar chart with stacked bars (blue + orange)
  - [ ] Verify legend: "Input Price" (blue), "Output Price" (orange)
  - [ ] Verify Y-axis: Model names
  - [ ] Verify X-axis: Prices with dollar signs ($0, $20, $40, $60)
  - [ ] Hover over bar: Tooltip shows "Input Price: $10.00", "Output Price: $30.00", "Total: $40.00"
  - [ ] Verify table below chart with 5 columns
  - [ ] Verify cheapest row highlighted in green
  - [ ] Verify "vs Cheapest" column shows percentages (+81.8%, +27.3%, etc.)
  - [ ] Test responsive: Resize window, verify chart adapts

## Dev Notes

### Architecture Alignment
- **Data Transformation**: Pure utility function `buildPricingChartData` (testable, no side effects)
- **Component Separation**: PricingComparisonChart focuses on pricing visualization, ComparisonPage handles data fetching
- **Reusability**: BaseChart wrapper (Story 5.5) used for pricing chart
- **Configuration**: Chart options customized for stacked horizontal bars
- **No Backend Changes**: Uses existing ModelDto with inputPricePer1M, outputPricePer1M fields

### Stacked Bar Chart Pattern
Chart.js creates stacked bars when datasets share the same `stack` property:
```javascript
{
  datasets: [
    { label: 'Input', data: [10, 15, 7], stack: 'pricing' },   // Bottom segment
    { label: 'Output', data: [30, 75, 21], stack: 'pricing' }, // Top segment (stacked on input)
  ]
}
```

Result: Each model has one bar with two segments (input at bottom, output on top).

### Horizontal vs Vertical Bars
**Horizontal bars (`indexAxis: 'y'`)** are better for pricing comparison:
- ✅ Model names readable (no rotation needed)
- ✅ Easier to compare prices visually (align left to right)
- ✅ Better for long model names
- ✅ More intuitive for cost comparison (left = cheaper, right = expensive)

**Vertical bars (`indexAxis: 'x'`)** would require:
- ❌ Rotated labels or truncated names
- ❌ Less intuitive for price comparison

### Color Scheme Rationale
**Input vs Output colors:**
- Input: Blue (#3b82f6) - Represents incoming/consumed tokens
- Output: Orange (#f97316) - Represents outgoing/generated tokens

**Avoids green for output** because:
- Green already used for "cheapest" indicator
- Would create confusion (is green = output or cheapest?)

**Accessibility:**
- Blue + Orange works for most color blindness types (deuteranopia, protanopia)
- Sufficient contrast against white background (WCAG AA)

### Cost Difference Calculation
Formula: `((modelCost - cheapestCost) / cheapestCost) * 100`

Examples:
- GPT-4 ($40) vs Claude ($22 cheapest): (40-22)/22 * 100 = **+81.8%**
- Gemini ($28) vs Claude ($22 cheapest): (28-22)/22 * 100 = **+27.3%**
- Claude ($22) vs Claude ($22 cheapest): **Cheapest**

Interpretation: "+81.8%" means GPT-4 is 81.8% more expensive than the cheapest option.

### Cheapest Model Highlighting
Three approaches evaluated:

**Approach A: Border on chart bar**
- Pro: Visual on chart itself
- Con: Hard to see, especially with stacked bars

**Approach B: Badge/text below chart**
- Pro: Clear and readable
- Con: Requires looking below chart
- **Selected for implementation**

**Approach C: Table row highlight**
- Pro: Integrated with pricing details
- Con: Table may not be visible initially
- **Also implemented (complements Approach B)**

### Pricing Summary Table Benefits
Table provides:
1. Exact numeric values (chart shows visual proportions)
2. Side-by-side comparison of input, output, total
3. Cost difference percentages
4. Sortable rows (cheapest first)
5. Accessible alternative to chart (screen readers, keyboard navigation)

### Stacked Bar Tooltip
Custom tooltip shows breakdown:
```
Input Price: $10.00
Output Price: $30.00
──────────────────────
Total: $40.00
```

Tooltip footer calculates total (sum of both datasets at same index).

### Chart Height Considerations
Height adjusts based on model count:
- 2 models: 200px (compact)
- 3-5 models: 350px (default)
- 6-8 models: 500px (taller)

Formula: `height = Math.max(200, modelCount * 60)` (60px per model bar)

### Edge Case: Free Tier Models
Models with $0.00 pricing:
- Chart shows minimal bar (1px, visible)
- Table shows "Free" instead of "$0.00"
- "vs Cheapest" column: "Cheapest" (since $0 is always cheapest)

### Prerequisites
- **Story 5.5**: BaseChart component, chart-setup.ts, chartDefaults.ts
- **Story 5.6**: Chart data transformation pattern (buildBenchmarkChartData reference)
- **Story 5.2**: useComparisonData hook provides models with pricing data
- No new dependencies required

### Quality Gates
- TypeScript strict mode: ✅ Zero `any` types
- Chart renders: ✅ Stacked horizontal bar chart with input + output
- Colors: ✅ Blue (input), Orange (output)
- Cheapest highlighted: ✅ Green row in table, badge below chart
- Cost difference: ✅ Percentages displayed in table and tooltip
- Responsive: ✅ Chart adapts to mobile/tablet/desktop
- Performance: ✅ Chart + table render in <300ms
- Accessibility: ✅ ARIA labels, semantic table, keyboard navigation

### Project Structure Notes
```
apps/web/src/
├── components/
│   └── charts/
│       ├── PricingComparisonChart.tsx     # New component (this story)
│       ├── BenchmarkBarChart.tsx          # From Story 5.6
│       └── BaseChart.tsx                  # From Story 5.5
├── utils/
│   ├── pricingChartData.ts                # New utility (this story)
│   │   └── buildPricingChartData()
│   └── pricingCalculations.ts             # New utility (this story)
│       ├── calculateTotalCost()
│       ├── findCheapestModel()
│       └── calculateCostDifference()
└── types/
    └── charts.ts                          # Updated: PricingComparisonChartProps
```

### Performance Considerations
- Chart with 5 models × 2 datasets = 10 bars (very fast)
- Stacked bars: No performance difference vs grouped bars
- Table with 5 rows: Renders instantly (<10ms)
- useMemo prevents recalculating chart data on every render

### Data Flow
```
ComparisonPage (useComparisonData)
  → data.models (ModelDto[])
    → PricingComparisonChart component
      → buildPricingChartData(models)
        → ChartData<'bar'> (stacked datasets)
          → BaseChart component
            → react-chartjs-2 <Bar />
              → Chart.js canvas rendering
      → PricingSummaryTable
        → calculateTotalCost, findCheapestModel, calculateCostDifference
          → Render table with highlighted cheapest row
```

### References
- [Source: docs/tech-spec-epic-5.md#Services and Modules] - PricingComparisonChart component spec
- [Source: docs/tech-spec-epic-5.md#Acceptance Criteria] - AC-5.8: Pricing chart requirements
- [Source: docs/epics.md#Story 5.8] - Original story with 5 acceptance criteria
- [Source: docs/stories/story-5.5.md] - BaseChart wrapper, chart setup
- [Source: docs/stories/story-5.6.md] - Chart data transformation pattern
- [Source: docs/solution-architecture.md#Frontend Components] - Chart.js integration

## Dev Agent Record

### Context Reference

<!-- Path(s) to story context XML will be added here by context workflow -->

### Agent Model Used

claude-sonnet-4-5-20250929

### Debug Log References

### Completion Notes List

### File List
