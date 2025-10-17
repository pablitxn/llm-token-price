# Story 5.7: Add Metric Selector for Chart

Status: Draft

## Story

As a user,
I want to choose which benchmarks to visualize in the chart,
so that I can focus on the metrics most relevant to my comparison needs.

## Acceptance Criteria

1. Metric selector (multi-select dropdown) displayed above chart
2. Lists all available benchmarks organized by categories (Reasoning, Code, Math, Language, Multimodal)
3. Default selection: top 5 key benchmarks (from Story 5.6 coverage algorithm)
4. Selecting/deselecting benchmarks updates chart in real-time (no page reload)
5. "Select All" / "Deselect All" buttons for bulk actions
6. Selection state persisted in session storage (survives page refresh)

## Tasks / Subtasks

### Task Group 1: Create MetricSelector Component (AC: #1, #2)
- [ ] Create component file: `apps/web/src/components/charts/MetricSelector.tsx`
  - [ ] Define `MetricSelectorProps` interface:
    ```typescript
    interface MetricSelectorProps {
      availableBenchmarks: BenchmarkOption[];
      selectedBenchmarkIds: string[];
      onSelectionChange: (selectedIds: string[]) => void;
      className?: string;
    }

    interface BenchmarkOption {
      id: string;
      name: string;
      category: 'reasoning' | 'code' | 'math' | 'language' | 'multimodal';
    }
    ```
  - [ ] Create functional component with TypeScript
  - [ ] Export as named export
- [ ] Component layout structure
  - [ ] Container: `<div className="mb-6 p-4 bg-gray-50 border border-gray-200 rounded-lg">`
  - [ ] Label: "Select Benchmarks" (`text-sm font-medium text-gray-700 mb-2`)
  - [ ] Flex container for selector + buttons: `flex items-center gap-4`
  - [ ] Multi-select dropdown
  - [ ] Button group: "Select All" + "Deselect All"
- [ ] Use native HTML select with multiple attribute (simpler alternative)
  - [ ] Or custom dropdown with checkboxes (better UX)
  - [ ] Recommendation: Custom dropdown for categorized display

### Task Group 2: Build Custom Multi-Select Dropdown (AC: #2, #4)
- [ ] Implement dropdown with Headless UI (optional) or custom React
  - [ ] Dropdown button: Shows selected count or "Select Benchmarks"
  - [ ] Button text: "{count} benchmarks selected" or "No benchmarks selected"
  - [ ] Dropdown icon: ChevronDown from lucide-react
  - [ ] Button style: `px-4 py-2 bg-white border border-gray-300 rounded-lg hover:bg-gray-50`
- [ ] Dropdown panel (opens on click)
  - [ ] Panel: `absolute top-full mt-2 w-96 bg-white border border-gray-200 rounded-lg shadow-lg z-20`
  - [ ] Max height: `max-h-96 overflow-y-auto`
  - [ ] Scrollable list of checkboxes
- [ ] Group benchmarks by category
  - [ ] Category headers: "Reasoning", "Code", "Math", "Language", "Multimodal"
  - [ ] Category header style: `px-4 py-2 bg-gray-100 text-sm font-semibold text-gray-700`
  - [ ] Benchmarks under each category with checkboxes
- [ ] Checkbox list items
  - [ ] Each item: `<label className="flex items-center px-4 py-2 hover:bg-gray-50 cursor-pointer">`
  - [ ] Checkbox: `<input type="checkbox" checked={isSelected} onChange={handleToggle} />`
  - [ ] Benchmark name: `<span className="ml-3 text-sm text-gray-900">{benchmark.name}</span>`
  - [ ] Checkbox style: TailwindCSS form plugin or custom styled

### Task Group 3: Extract Available Benchmarks from Models (AC: #2)
- [ ] Create utility function: `apps/web/src/utils/benchmarkSelection.ts`
  - [ ] Function: `extractAvailableBenchmarks(models: ModelDto[]): BenchmarkOption[]`
  - [ ] Input: Array of ModelDto with benchmarkScores
  - [ ] Output: Array of unique benchmarks with category metadata
- [ ] Algorithm:
  ```typescript
  // 1. Collect all unique benchmark IDs across all models
  const benchmarkMap = new Map<string, BenchmarkOption>();

  models.forEach(model => {
    model.benchmarkScores.forEach(score => {
      if (!benchmarkMap.has(score.benchmarkId)) {
        benchmarkMap.set(score.benchmarkId, {
          id: score.benchmarkId,
          name: score.benchmarkName,
          category: getCategoryForBenchmark(score.benchmarkName),
        });
      }
    });
  });

  // 2. Convert to array and sort by category, then alphabetically
  const benchmarks = Array.from(benchmarkMap.values());
  benchmarks.sort((a, b) => {
    if (a.category !== b.category) {
      return CATEGORY_ORDER.indexOf(a.category) - CATEGORY_ORDER.indexOf(b.category);
    }
    return a.name.localeCompare(b.name);
  });

  return benchmarks;
  ```
- [ ] Use benchmark category mapping from Story 5.4
  - [ ] Import: `import { getCategoryForBenchmark } from '@/config/benchmarkCategories';`
  - [ ] Fallback category: 'reasoning' (if benchmark not in mapping)
- [ ] Define category display order
  - [ ] Constant: `CATEGORY_ORDER = ['reasoning', 'code', 'math', 'language', 'multimodal']`
  - [ ] Ensures consistent category order in dropdown

### Task Group 4: Default Selection Logic (AC: #3)
- [ ] Initialize with top 5 benchmarks (from Story 5.6)
  - [ ] Use `selectTopBenchmarks(models, 5)` utility
  - [ ] Returns array of benchmark IDs: `['mmlu-id', 'humaneval-id', 'gsm8k-id', ...]`
  - [ ] Set as initial state: `const [selectedIds, setSelectedIds] = useState<string[]>(defaultIds)`
- [ ] Alternative: Use predefined default set
  - [ ] If coverage algorithm unavailable, use hardcoded defaults:
    ```typescript
    const DEFAULT_BENCHMARKS = [
      'mmlu',        // Reasoning
      'humaneval',   // Code
      'gsm8k',       // Math
      'hellaswag',   // Language
      'mmmu',        // Multimodal
    ];
    ```
  - [ ] Filter available benchmarks: `availableBenchmarks.filter(b => DEFAULT_BENCHMARKS.includes(b.id))`
- [ ] Handle case: <5 benchmarks available
  - [ ] If models have <5 total benchmarks: Select all available
  - [ ] Ensures chart always shows something

### Task Group 5: Selection Change Handler (AC: #4)
- [ ] Implement checkbox toggle logic
  - [ ] Function: `handleToggleBenchmark(benchmarkId: string)`
  - [ ] Check if currently selected: `selectedIds.includes(benchmarkId)`
  - [ ] If selected: Remove from array
  - [ ] If not selected: Add to array
  - [ ] Update state: `setSelectedIds(newIds)`
  - [ ] Call parent callback: `onSelectionChange(newIds)`
- [ ] Real-time chart update
  - [ ] Parent component (BenchmarkBarChart) receives new selectedIds via callback
  - [ ] Re-renders chart with new benchmark set
  - [ ] Use `useMemo` to prevent unnecessary recalculations
- [ ] Debouncing (optional, for performance)
  - [ ] If selecting multiple benchmarks rapidly, debounce chart updates
  - [ ] Use lodash.debounce or custom hook: `useDebouncedValue(selectedIds, 200)`
  - [ ] Not needed for <20 benchmarks (fast enough)

### Task Group 6: Select All / Deselect All Buttons (AC: #5)
- [ ] Create "Select All" button
  - [ ] Button text: "Select All"
  - [ ] onClick: `setSelectedIds(availableBenchmarks.map(b => b.id))`
  - [ ] Call callback: `onSelectionChange(allIds)`
  - [ ] Button style: `px-3 py-1.5 text-sm bg-blue-500 text-white rounded hover:bg-blue-600`
- [ ] Create "Deselect All" button
  - [ ] Button text: "Deselect All"
  - [ ] onClick: `setSelectedIds([])`
  - [ ] Call callback: `onSelectionChange([])`
  - [ ] Button style: `px-3 py-1.5 text-sm bg-gray-200 text-gray-700 rounded hover:bg-gray-300`
- [ ] Button group layout
  - [ ] Container: `<div className="flex gap-2">`
  - [ ] Position: Right of dropdown or below it
  - [ ] Disabled state: Gray out buttons if no benchmarks available
- [ ] Handle edge case: Deselect all
  - [ ] If selectedIds = [], chart shows empty state or default message
  - [ ] Alternative: Prevent deselect all (require at least 1 selected)
  - [ ] Recommendation: Allow deselect all, show empty chart message

### Task Group 7: Persist Selection in Session Storage (AC: #6)
- [ ] Create session storage hook: `apps/web/src/hooks/useSessionStorage.ts`
  - [ ] Generic hook: `useSessionStorage<T>(key: string, initialValue: T): [T, (value: T) => void]`
  - [ ] Similar to useState, but syncs with sessionStorage
  - [ ] Implementation:
    ```typescript
    export const useSessionStorage = <T>(key: string, initialValue: T) => {
      const [storedValue, setStoredValue] = useState<T>(() => {
        try {
          const item = window.sessionStorage.getItem(key);
          return item ? JSON.parse(item) : initialValue;
        } catch (error) {
          console.warn(`Error reading sessionStorage key "${key}":`, error);
          return initialValue;
        }
      });

      const setValue = (value: T) => {
        try {
          setStoredValue(value);
          window.sessionStorage.setItem(key, JSON.stringify(value));
        } catch (error) {
          console.warn(`Error setting sessionStorage key "${key}":`, error);
        }
      };

      return [storedValue, setValue];
    };
    ```
- [ ] Use hook in MetricSelector or parent component
  - [ ] Key: `comparison-selected-benchmarks`
  - [ ] Value: Array of benchmark IDs
  - [ ] Example: `const [selectedIds, setSelectedIds] = useSessionStorage('comparison-selected-benchmarks', defaultIds);`
- [ ] Session storage behavior
  - [ ] Persists during tab session (survives page refresh)
  - [ ] Cleared when tab closed (not permanent like localStorage)
  - [ ] Per-origin storage (not shared across domains)
- [ ] Handle invalid stored data
  - [ ] If stored IDs don't match available benchmarks: Reset to default
  - [ ] Filter out invalid IDs: `storedIds.filter(id => availableBenchmarks.some(b => b.id === id))`

### Task Group 8: Integrate with BenchmarkBarChart (AC: #4)
- [ ] Update BenchmarkBarChart to accept selected benchmarks
  - [ ] Modify props: `selectedBenchmarks?: string[]` (now controlled by parent)
  - [ ] If provided: Use selectedBenchmarks
  - [ ] If not provided: Use default top 5 (backward compatible)
- [ ] Move MetricSelector into BenchmarkBarChart component
  - [ ] Render MetricSelector above chart
  - [ ] Manage selected state locally in BenchmarkBarChart
  - [ ] Pass selection to buildBenchmarkChartData utility
- [ ] Component structure:
  ```typescript
  export const BenchmarkBarChart = ({ models }: BenchmarkBarChartProps) => {
    const availableBenchmarks = extractAvailableBenchmarks(models);
    const defaultIds = selectTopBenchmarks(models, 5);
    const [selectedIds, setSelectedIds] = useSessionStorage('comparison-selected-benchmarks', defaultIds);

    const chartData = useMemo(
      () => buildBenchmarkChartData(models, selectedIds),
      [models, selectedIds]
    );

    return (
      <section className="mt-8 p-6 bg-white border border-gray-200 rounded-lg">
        <h2 className="text-2xl font-bold mb-6">Benchmark Performance Comparison</h2>

        <MetricSelector
          availableBenchmarks={availableBenchmarks}
          selectedBenchmarkIds={selectedIds}
          onSelectionChange={setSelectedIds}
        />

        {selectedIds.length > 0 ? (
          <BaseChart type="bar" data={chartData} options={chartOptions} height={400} />
        ) : (
          <EmptyState message="Select at least one benchmark to visualize" />
        )}
      </section>
    );
  };
  ```

### Task Group 9: Dropdown Open/Close State Management (AC: #1)
- [ ] Implement dropdown toggle
  - [ ] State: `const [isOpen, setIsOpen] = useState(false)`
  - [ ] Button onClick: `setIsOpen(!isOpen)`
  - [ ] Panel visibility: `{isOpen && <DropdownPanel />}`
- [ ] Close dropdown on outside click
  - [ ] Use useEffect with document click listener
  - [ ] Or use Headless UI `<Menu>` component (handles this automatically)
  - [ ] Implementation:
    ```typescript
    useEffect(() => {
      const handleClickOutside = (event: MouseEvent) => {
        if (dropdownRef.current && !dropdownRef.current.contains(event.target as Node)) {
          setIsOpen(false);
        }
      };

      if (isOpen) {
        document.addEventListener('mousedown', handleClickOutside);
      }

      return () => {
        document.removeEventListener('mousedown', handleClickOutside);
      };
    }, [isOpen]);
    ```
  - [ ] Ref: `const dropdownRef = useRef<HTMLDivElement>(null)`
- [ ] Close dropdown on Escape key
  - [ ] Add keydown listener: `if (event.key === 'Escape') setIsOpen(false)`
  - [ ] Accessibility improvement

### Task Group 10: Dropdown Styling and UX (AC: #1, #2)
- [ ] Dropdown button design
  - [ ] Shows count: "5 benchmarks selected" or "No benchmarks"
  - [ ] Icon: ChevronDown rotates 180° when open
  - [ ] Style: `flex items-center justify-between px-4 py-2 bg-white border rounded-lg`
  - [ ] Hover: `hover:bg-gray-50`
  - [ ] Focus: `focus:outline-none focus:ring-2 focus:ring-blue-500`
- [ ] Dropdown panel design
  - [ ] White background with border and shadow
  - [ ] Rounded corners: `rounded-lg`
  - [ ] Shadow: `shadow-lg`
  - [ ] Max height with scroll: `max-h-96 overflow-y-auto`
  - [ ] Z-index: `z-20` (above chart but below modals)
- [ ] Category section styling
  - [ ] Sticky category headers (optional): `sticky top-0 bg-gray-100`
  - [ ] Category name: Bold, uppercase, small text
  - [ ] Category icon (optional): Brain, Code, Calculator, MessageSquare, Image from lucide-react
- [ ] Checkbox styling
  - [ ] Use TailwindCSS Forms plugin: `@tailwindcss/forms`
  - [ ] Or custom checkmark SVG
  - [ ] Checked state: Blue checkmark, blue border
  - [ ] Unchecked state: Gray border
  - [ ] Hover: Background gray-50 on label

### Task Group 11: Empty State Handling (AC: #4)
- [ ] No benchmarks selected
  - [ ] Chart shows empty state message: "Select at least one benchmark to visualize"
  - [ ] Icon: BarChart3 from lucide-react (grayed out)
  - [ ] Style: `text-center text-gray-500 py-12`
  - [ ] Button: "Select Benchmarks" (opens dropdown)
- [ ] No benchmarks available
  - [ ] If models have no benchmarkScores: "No benchmark data available"
  - [ ] Disable MetricSelector (dropdown shows "No benchmarks")
  - [ ] Hide "Select All" / "Deselect All" buttons

### Task Group 12: Type Definitions (AC: #1, #2)
- [ ] Update `apps/web/src/types/charts.ts`
  - [ ] Define `MetricSelectorProps` interface
  - [ ] Define `BenchmarkOption` interface:
    ```typescript
    export interface BenchmarkOption {
      id: string;
      name: string;
      category: 'reasoning' | 'code' | 'math' | 'language' | 'multimodal';
      coverage?: number;  // Optional: How many models have this benchmark
    }
    ```
  - [ ] Define `BenchmarkCategory` type (reuse from Story 5.4)
- [ ] Session storage types
  - [ ] `useSessionStorage<T>` generic hook (type-safe)
  - [ ] Stored value: `string[]` (benchmark IDs)

### Task Group 13: Accessibility (AC: #1, #4)
- [ ] Dropdown button accessibility
  - [ ] ARIA: `aria-label="Select benchmarks to visualize"`, `aria-expanded={isOpen}`, `aria-haspopup="true"`
  - [ ] Keyboard: Enter/Space to open/close dropdown
  - [ ] Focus: Visible focus ring (blue-500)
- [ ] Dropdown panel accessibility
  - [ ] ARIA: `role="menu"` or `role="listbox"` (depending on implementation)
  - [ ] Keyboard navigation: Arrow keys to move between checkboxes
  - [ ] Escape key: Close dropdown
- [ ] Checkbox accessibility
  - [ ] Each checkbox: `<label>` wraps input + text (click anywhere to toggle)
  - [ ] ARIA: `aria-label` on checkbox if needed
  - [ ] Keyboard: Tab to navigate, Space to toggle
- [ ] Screen reader support
  - [ ] Announce selected count: "5 of 12 benchmarks selected"
  - [ ] Announce category changes when navigating list
  - [ ] Live region updates when selection changes (optional)

### Task Group 14: Performance Optimization (AC: #4)
- [ ] Memoize available benchmarks
  - [ ] Use `useMemo`:
    ```typescript
    const availableBenchmarks = useMemo(
      () => extractAvailableBenchmarks(models),
      [models]
    );
    ```
  - [ ] Only recalculate when models array changes
- [ ] Memoize chart data
  - [ ] Already done in Story 5.6, but now depends on selectedIds:
    ```typescript
    const chartData = useMemo(
      () => buildBenchmarkChartData(models, selectedIds),
      [models, selectedIds]
    );
    ```
  - [ ] Prevents recalculation on every render
- [ ] Avoid unnecessary re-renders
  - [ ] Use `React.memo()` on MetricSelector
  - [ ] Use `useCallback` for onSelectionChange handler
- [ ] Test performance
  - [ ] Selecting/deselecting benchmarks updates chart in <100ms
  - [ ] No lag when opening dropdown (50+ benchmarks)

### Task Group 15: Testing and Verification (AC: #1-6)
- [ ] Write unit test for `extractAvailableBenchmarks` utility
  - [ ] Test with 3 models, mixed benchmarks (some overlap, some unique)
  - [ ] Verify unique benchmarks extracted
  - [ ] Verify sorted by category then alphabetically
  - [ ] Use Vitest
- [ ] Write unit test for session storage hook
  - [ ] Test initial value load from sessionStorage
  - [ ] Test setValue writes to sessionStorage
  - [ ] Test invalid JSON handling (fallback to initialValue)
  - [ ] Use Vitest with mock sessionStorage
- [ ] Write integration test for MetricSelector component
  - [ ] Render with 10 benchmarks across 3 categories
  - [ ] Test dropdown opens on button click
  - [ ] Test checkbox toggle updates selection
  - [ ] Test "Select All" selects all benchmarks
  - [ ] Test "Deselect All" clears selection
  - [ ] Verify onSelectionChange called with correct IDs
  - [ ] Use Vitest + React Testing Library
- [ ] Manual E2E testing
  - [ ] Navigate to `/compare?models=1,2,3`
  - [ ] Verify MetricSelector displays above chart
  - [ ] Button shows "5 benchmarks selected" (default)
  - [ ] Click button: Dropdown opens with categorized list
  - [ ] Verify categories: Reasoning, Code, Math, Language, Multimodal
  - [ ] Uncheck "MMLU": Chart updates in real-time (MMLU removed)
  - [ ] Check "MATH": Chart updates (MATH added)
  - [ ] Click "Deselect All": Chart shows empty state
  - [ ] Click "Select All": Chart shows all benchmarks
  - [ ] Refresh page: Selection persists (from sessionStorage)
  - [ ] Close tab, reopen: Selection cleared (session ended)

## Dev Notes

### Architecture Alignment
- **Component Separation**: MetricSelector is a reusable UI component (accepts data via props)
- **State Management**: Selection state managed locally in BenchmarkBarChart (no global state needed)
- **Session Persistence**: useSessionStorage hook (reusable, not specific to this feature)
- **Data Extraction**: Utility function extractAvailableBenchmarks (pure function, testable)
- **No Backend Changes**: Frontend-only story, uses existing benchmarkScores data

### Multi-Select Dropdown Pattern
Two implementation approaches:

**Option A: Headless UI (Recommended)**
```typescript
import { Menu } from '@headlessui/react';

<Menu as="div" className="relative">
  <Menu.Button>5 benchmarks selected</Menu.Button>
  <Menu.Items>
    {/* Checkboxes here */}
  </Menu.Items>
</Menu>
```
Benefits: Handles open/close, outside clicks, Escape key, accessibility automatically.

**Option B: Custom Implementation**
```typescript
const [isOpen, setIsOpen] = useState(false);
const dropdownRef = useRef<HTMLDivElement>(null);

// Manual handling of clicks, keyboard, focus
```
Benefits: Full control, no dependency. Drawbacks: More code, manual accessibility.

Recommendation: Use Headless UI if available (check package.json), otherwise custom.

### Session Storage vs. Local Storage
**Session Storage** (used in this story):
- Persists during tab session (survives refresh)
- Cleared when tab closed
- Ideal for temporary preferences

**Local Storage** (not used):
- Persists permanently (survives browser close)
- Cleared only manually
- Ideal for long-term preferences

Rationale for session storage: Benchmark selection is comparison-specific, not a user-wide preference.

### Benchmark Category Grouping
Categories reuse mapping from Story 5.4:
```typescript
import { BENCHMARK_CATEGORIES, getCategoryForBenchmark } from '@/config/benchmarkCategories';

// BENCHMARK_CATEGORIES = {
//   reasoning: ['MMLU', 'Big-Bench Hard', 'BBH', 'ARC-Challenge'],
//   code: ['HumanEval', 'MBPP', 'CodeContests'],
//   math: ['GSM8K', 'MATH', 'MathQA'],
//   language: ['HellaSwag', 'TruthfulQA', 'ANLI'],
//   multimodal: ['MMMU', 'VQA', 'TextVQA']
// };
```

Category display order: Reasoning → Code → Math → Language → Multimodal

### Default Selection Strategy
Two approaches:

**Approach 1: Coverage-based (from Story 5.6)**
- Select top 5 benchmarks by coverage (most models have scores)
- Uses `selectTopBenchmarks(models, 5)` utility
- Dynamic: Adapts to available data

**Approach 2: Predefined defaults**
- Hardcoded list: MMLU, HumanEval, GSM8K, HellaSwag, MMMU
- Static: Same defaults for all comparisons
- Fallback if coverage algorithm fails

Recommendation: Use Approach 1 (coverage-based) as primary, Approach 2 as fallback.

### Real-Time Chart Update Flow
```
User clicks checkbox
  → handleToggleBenchmark(id)
    → setSelectedIds(newIds)
      → sessionStorage.setItem('comparison-selected-benchmarks', newIds)
        → onSelectionChange(newIds) callback
          → Parent re-renders
            → useMemo recalculates chartData with new selectedIds
              → BaseChart re-renders with new data
                → Chart.js updates canvas (smooth animation)
```

Chart.js update animation: ~300ms fade transition (built-in).

### Dropdown Positioning
Dropdown panel positions below button:
```css
.dropdown-panel {
  position: absolute;
  top: 100%;  /* Below button */
  left: 0;
  margin-top: 0.5rem;
  width: 24rem;  /* 384px */
  z-index: 20;
}
```

On small screens, consider full-width dropdown or modal (mobile-friendly).

### Checkbox State Persistence
Session storage stores IDs only (not full objects):
```json
// sessionStorage['comparison-selected-benchmarks']
["mmlu-id", "humaneval-id", "gsm8k-id"]
```

On load:
1. Read IDs from sessionStorage
2. Match against availableBenchmarks
3. Filter out invalid IDs (benchmarks no longer available)
4. Set as initial state

### Empty State Message Options
When selectedIds = []:
- **Option A**: Show empty chart message + "Select Benchmarks" button
- **Option B**: Automatically select default benchmarks (prevent empty state)
- **Option C**: Show placeholder chart with grayed-out bars

Recommendation: Option A (explicit user control).

### Prerequisites
- **Story 5.6**: BenchmarkBarChart component with buildBenchmarkChartData utility
- **Story 5.4**: Benchmark category mapping (benchmarkCategories.ts)
- No new dependencies required (unless using Headless UI)

### Quality Gates
- TypeScript strict mode: ✅ Zero `any` types
- Dropdown renders: ✅ Categorized list of benchmarks with checkboxes
- Selection works: ✅ Toggling checkboxes updates chart in real-time
- Persistence: ✅ Selection survives page refresh (sessionStorage)
- Bulk actions: ✅ "Select All" / "Deselect All" buttons work
- Responsive: ✅ Dropdown usable on mobile
- Accessibility: ✅ ARIA labels, keyboard navigation, focus management
- Performance: ✅ Chart updates in <100ms on selection change

### Project Structure Notes
```
apps/web/src/
├── components/
│   └── charts/
│       ├── BenchmarkBarChart.tsx      # Updated: Integrate MetricSelector
│       └── MetricSelector.tsx         # New component (this story)
├── hooks/
│   └── useSessionStorage.ts           # New hook (this story)
├── utils/
│   └── benchmarkSelection.ts          # New utility (this story)
│       └── extractAvailableBenchmarks()
├── config/
│   └── benchmarkCategories.ts         # From Story 5.4 (reused)
└── types/
    └── charts.ts                      # Updated with MetricSelectorProps
```

### Performance Considerations
- Dropdown with 50 benchmarks: Renders in <50ms (fast)
- Checkbox toggle: Updates chart in <100ms (useMemo prevents full recalculation)
- Session storage read/write: <5ms (negligible)
- Chart.js update animation: ~300ms (smooth visual feedback)

### Alternative: Simple Multi-Select (HTML Native)
For simpler UX, use native HTML select:
```html
<select multiple size="10" value={selectedIds} onChange={handleChange}>
  <optgroup label="Reasoning">
    <option value="mmlu">MMLU</option>
  </optgroup>
</select>
```

Pros: No custom code, accessible by default
Cons: Less visually appealing, no checkboxes, harder to style

Recommendation: Custom dropdown with checkboxes for better UX.

### References
- [Source: docs/tech-spec-epic-5.md#Services and Modules] - MetricSelector component spec
- [Source: docs/tech-spec-epic-5.md#Acceptance Criteria] - AC-5.7: Metric selector requirements
- [Source: docs/epics.md#Story 5.7] - Original story with 6 acceptance criteria
- [Source: docs/stories/story-5.6.md] - BenchmarkBarChart component, selectTopBenchmarks utility
- [Source: docs/stories/story-5.4.md] - Benchmark categories mapping
- [Source: docs/solution-architecture.md#State Management] - Session storage strategy

## Dev Agent Record

### Context Reference

<!-- Path(s) to story context XML will be added here by context workflow -->

### Agent Model Used

claude-sonnet-4-5-20250929

### Debug Log References

### Completion Notes List

### File List
