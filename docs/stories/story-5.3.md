# Story 5.3: Create Comparison Table Component

Status: Draft

## Story

As a user,
I want to see a detailed comparison table with models as columns and attributes as rows,
so that I can easily compare specific attributes across all selected models.

## Acceptance Criteria

1. Table displays attributes as rows and models as columns (vertical alignment)
2. Rows include: Provider, Input Price, Output Price, Context Window, Max Output, all capability flags (function calling, vision, audio, streaming, JSON mode), and key benchmarks
3. Vertical alignment maintained across all columns for easy scanning
4. Differences highlighted: cheapest price in green text, highest benchmark score in green text
5. Table is scrollable horizontally if content exceeds viewport width
6. Toggle to show/hide attribute categories (e.g., collapse "Capabilities" section)

## Tasks / Subtasks

### Task Group 1: Create ComparisonTable Component (AC: #1, #2, #3)
- [ ] Create component file: `apps/web/src/components/comparison/ComparisonTable.tsx`
  - [ ] Define `ComparisonTableProps` interface: `{ models: ModelDto[] }`
  - [ ] Create functional component with TypeScript
  - [ ] Export as named export
- [ ] Implement table structure with semantic HTML
  - [ ] Use `<table>` with `<thead>`, `<tbody>` elements
  - [ ] Table container: `overflow-x-auto` for horizontal scroll
  - [ ] Table: `min-w-full border-collapse`
  - [ ] Sticky first column (attribute names): `sticky left-0 bg-white z-10`
- [ ] Header row (model names)
  - [ ] First cell: "Attribute" label (attribute column header)
  - [ ] Model cells: Model name + provider badge
  - [ ] Cell styling: `px-6 py-4 text-left text-sm font-semibold text-gray-900`
  - [ ] Border: `border-b border-gray-200`
- [ ] Data rows structure
  - [ ] First cell: Attribute name (bold)
  - [ ] Model cells: Attribute values
  - [ ] Zebra striping: `even:bg-gray-50` for readability
  - [ ] Cell padding: `px-6 py-4`

### Task Group 2: Build Comparison Rows Data Structure (AC: #2)
- [ ] Create `buildComparisonRows` utility function
  - [ ] Location: `apps/web/src/utils/comparisonTable.ts`
  - [ ] Input: `ModelDto[]`
  - [ ] Output: `ComparisonTableRow[]`
  - [ ] Define `ComparisonTableRow` type:
    ```typescript
    interface ComparisonTableRow {
      attribute: string;
      category: 'pricing' | 'capabilities' | 'metadata' | 'benchmarks';
      values: Record<string, string | number | boolean>;
      highlightBest?: boolean;
    }
    ```
- [ ] Generate rows for each category:
  - [ ] **Metadata**: Provider, Release Date, Status
  - [ ] **Pricing**: Input Price per 1M, Output Price per 1M
  - [ ] **Context**: Context Window, Max Output Tokens
  - [ ] **Capabilities**: Function Calling, Vision, Audio, Streaming, JSON Mode
  - [ ] **Benchmarks**: MMLU, HumanEval, GSM8K (top 3 initially)
- [ ] Map model data to row values
  - [ ] Key: model.id, Value: attribute value
  - [ ] Handle missing data: Display "N/A" or "—"
  - [ ] Format numbers: Prices with $, context with commas

### Task Group 3: Highlight Best Values (AC: #4)
- [ ] Implement highlighting logic in `buildComparisonRows`
  - [ ] For pricing rows: Find minimum value, mark as best
  - [ ] For benchmark rows: Find maximum score, mark as best
  - [ ] For context window: Find maximum, mark as best
  - [ ] Add `isBest: true` flag to value metadata
- [ ] Apply highlighting styles in ComparisonTable
  - [ ] Best price: `text-green-600 font-semibold`
  - [ ] Best score: `text-green-600 font-semibold`
  - [ ] Add checkmark icon (✓) next to best values
  - [ ] Use lucide-react `Check` icon with `w-4 h-4 inline`
- [ ] Handle ties (multiple models with same best value)
  - [ ] Highlight all tied models
  - [ ] No special indicator for ties

### Task Group 4: Category Sections with Collapse (AC: #6)
- [ ] Create category section headers
  - [ ] Row with category name (e.g., "Pricing", "Capabilities")
  - [ ] Style: `bg-gray-100 font-semibold text-gray-700 py-2`
  - [ ] Collapse/expand icon (ChevronDown/ChevronUp from lucide-react)
  - [ ] Click handler toggles section visibility
- [ ] Implement collapse state management
  - [ ] Use React state: `const [collapsed, setCollapsed] = useState<Record<string, boolean>>({})`
  - [ ] Key: category name, Value: is collapsed
  - [ ] Default: all expanded
  - [ ] Toggle function: `toggleCategory(category: string)`
- [ ] Conditional rendering of category rows
  - [ ] Check if category is collapsed
  - [ ] Hide rows with `hidden` class if collapsed
  - [ ] Smooth transition with TailwindCSS: `transition-all duration-200`

### Task Group 5: Responsive Horizontal Scroll (AC: #5)
- [ ] Implement scroll container
  - [ ] Wrapper div: `overflow-x-auto`
  - [ ] Table: `min-w-max` (content-driven width)
  - [ ] Scroll behavior: `scroll-smooth`
- [ ] Add scroll indicators (optional enhancement)
  - [ ] Shadow gradient at left/right edges when scrollable
  - [ ] CSS: `before:` and `after:` pseudo-elements with gradient
  - [ ] Only show when content overflows
- [ ] Test with 2-5 models
  - [ ] 2 models: No scroll needed
  - [ ] 5 models: Horizontal scroll on smaller screens

### Task Group 6: Value Formatting and Display (AC: #2)
- [ ] Create formatting utilities in `apps/web/src/utils/formatters.ts`
  - [ ] `formatPrice(value: number): string` - "$10.50"
  - [ ] `formatNumber(value: number): string` - "128,000"
  - [ ] `formatBoolean(value: boolean): string` - "✓" or "—"
  - [ ] `formatPercentage(value: number): string` - "86.4%"
- [ ] Apply formatting in table cells
  - [ ] Pricing: Currency format with 2 decimals
  - [ ] Context window: Number with thousands separator
  - [ ] Capabilities: Checkmark (✓) or dash (—)
  - [ ] Benchmarks: Decimal score (e.g., "86.4")
- [ ] Handle edge cases
  - [ ] Null/undefined values: Display "N/A"
  - [ ] Zero values: Display "0" (not empty)
  - [ ] Very large numbers: Use abbreviations (e.g., "1.2M")

### Task Group 7: Integrate into ComparisonPage (AC: #1)
- [ ] Update `ComparisonPage.tsx` to include ComparisonTable
  - [ ] Import ComparisonTable component
  - [ ] Place below ModelCard section: `<section className="mt-8">`
  - [ ] Pass models data: `<ComparisonTable models={data.models} />`
- [ ] Add section header
  - [ ] Heading: "Detailed Comparison" (`text-2xl font-bold mb-4`)
  - [ ] Optional description: "Compare all attributes side-by-side"
- [ ] Loading state
  - [ ] Show table skeleton while `isLoading`
  - [ ] Skeleton: Gray rows with `animate-pulse`

### Task Group 8: Type Definitions (AC: #2)
- [ ] Update `apps/web/src/types/comparison.ts`
  - [ ] Define `ComparisonTableProps` interface
  - [ ] Define `ComparisonTableRow` interface
  - [ ] Define `ComparisonCategory` type: `'pricing' | 'capabilities' | 'metadata' | 'benchmarks'`
  - [ ] Define `CellValueMetadata` type for highlight flags
- [ ] Ensure type safety
  - [ ] No `any` types in table logic
  - [ ] All row values properly typed
  - [ ] Model data access type-checked

### Task Group 9: Accessibility (AC: #3, #5)
- [ ] Add ARIA attributes
  - [ ] Table caption: `<caption className="sr-only">Model comparison table</caption>`
  - [ ] Scope attributes: `<th scope="col">` for headers, `<th scope="row">` for row labels
  - [ ] ARIA labels: `aria-label="Comparison table, scroll horizontally to view all models"`
- [ ] Keyboard navigation
  - [ ] Table focusable: `tabindex="0"` on scroll container
  - [ ] Arrow key scrolling (optional, browser default)
  - [ ] Collapse buttons keyboard accessible
- [ ] Screen reader support
  - [ ] Announce best values: `aria-label="Best value"`
  - [ ] Describe table structure with caption

### Task Group 10: Testing and Verification (AC: #1-6)
- [ ] Write unit test for `buildComparisonRows` utility
  - [ ] Test row generation from 2 models
  - [ ] Test highlighting logic (best price, best score)
  - [ ] Test missing data handling (N/A display)
  - [ ] Test tie scenarios (multiple best values)
  - [ ] Use Vitest
- [ ] Write integration test for ComparisonTable component
  - [ ] Render with mock ModelDto array
  - [ ] Verify all rows displayed
  - [ ] Test collapse/expand functionality
  - [ ] Verify highlighting applied correctly
  - [ ] Use Vitest + React Testing Library
- [ ] Manual E2E testing
  - [ ] Navigate to `/compare?models=1,2,3`
  - [ ] Verify table renders below cards
  - [ ] Check vertical alignment of values
  - [ ] Verify cheapest price highlighted in green
  - [ ] Verify highest score highlighted in green
  - [ ] Test collapse/expand categories
  - [ ] Test horizontal scroll with 5 models
  - [ ] Verify responsive behavior on mobile

## Dev Notes

### Architecture Alignment
- **Component**: `ComparisonTable` is a presentational component (receives data via props)
- **Data Transformation**: `buildComparisonRows` utility function (pure function, easy to test)
- **No Direct API Calls**: Uses data from `useComparisonData` hook (Story 5.2)
- **Separation of Concerns**: Formatting logic isolated in `utils/formatters.ts`

### Table Layout Pattern
Story uses **attribute-as-rows** pattern (not model-as-rows):
```
| Attribute       | Model 1 | Model 2 | Model 3 |
|-----------------|---------|---------|---------|
| Provider        | OpenAI  | Anthropic | Google |
| Input Price     | $10.00  | $15.00  | $7.00   |
| Vision Support  | ✓       | ✓       | —       |
```

This pattern is superior for comparison because:
- Users scan vertically to compare single attribute across models
- Horizontal scrolling shows more models without vertical scroll
- Attribute names always visible (sticky left column)

### Highlighting Logic
Best value determination:
```typescript
const findBestValue = (values: number[], type: 'min' | 'max'): Set<number> => {
  const best = type === 'min' ? Math.min(...values) : Math.max(...values);
  return new Set(values.filter(v => v === best));
};

// Example: Prices [10, 15, 7, 7] → best = 7, highlights indices 2 and 3
```

### Collapse State Management
Category collapse uses object-based state:
```typescript
const [collapsed, setCollapsed] = useState<Record<string, boolean>>({
  pricing: false,
  capabilities: false,
  metadata: false,
  benchmarks: false,
});

const toggleCategory = (category: string) => {
  setCollapsed(prev => ({ ...prev, [category]: !prev[category] }));
};
```

Benefits:
- Independent control of each category
- Persists state during re-renders
- Easy to extend with new categories

### Sticky First Column
CSS for sticky attribute column:
```css
/* First column (attribute names) stays visible during horizontal scroll */
th:first-child,
td:first-child {
  position: sticky;
  left: 0;
  background-color: white;
  z-index: 10;
  box-shadow: 2px 0 4px rgba(0,0,0,0.05); /* Subtle shadow for depth */
}
```

### Prerequisites
- **Story 5.2**: `useComparisonData` hook must provide `data.models`
- **Story 1.10**: `ModelDto` structure with all required fields
- No backend changes required (uses existing data)

### Quality Gates
- TypeScript strict mode: ✅ Zero `any` types
- Table renders: ✅ All attributes displayed correctly
- Highlighting: ✅ Best values marked in green
- Alignment: ✅ Columns vertically aligned
- Responsive: ✅ Horizontal scroll works on mobile
- Accessibility: ✅ ARIA labels, keyboard navigation
- Collapse: ✅ Categories expand/collapse smoothly

### Project Structure Notes
```
apps/web/src/
├── components/
│   └── comparison/
│       ├── ComparisonTable.tsx      # New component (this story)
│       └── ModelCard.tsx            # From Story 5.2
├── utils/
│   ├── comparisonTable.ts           # buildComparisonRows utility (this story)
│   └── formatters.ts                # Formatting functions (this story)
└── types/
    └── comparison.ts                # Updated with table types (this story)
```

### Performance Considerations
- Table with 5 models × 15 attributes = 75 cells
- Avoid re-rendering entire table on collapse (use React.memo for rows)
- Memoize `buildComparisonRows` result to prevent recalculation
- Virtual scrolling NOT needed for <100 rows

### References
- [Source: docs/tech-spec-epic-5.md#Services and Modules] - ComparisonTable component specification
- [Source: docs/tech-spec-epic-5.md#Acceptance Criteria] - AC-5.3: Comparison table requirements
- [Source: docs/epics.md#Story 5.3] - Original story with 6 acceptance criteria
- [Source: docs/ux-specification.md#Component Specifications] - Table component design patterns
- [Source: docs/solution-architecture.md#Frontend Components] - Component organization

## Dev Agent Record

### Context Reference

<!-- Path(s) to story context XML will be added here by context workflow -->

### Agent Model Used

claude-sonnet-4-5-20250929

### Debug Log References

### Completion Notes List

### File List
