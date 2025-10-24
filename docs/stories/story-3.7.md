# Story 3.7: Add Price Range Filter

Status: Draft

## Story

As a user,
I want to filter models by price range,
So that I can exclude models outside my budget.

## Acceptance Criteria

1. Price range filter section added to sidebar
2. Dual-range slider for min/max price (combined input+output price or separate)
3. Price range inputs display current selection
4. Adjusting slider filters table in real-time
5. Price range based on total cost per 1M tokens (input + output avg)
6. Range defaults to min/max of available models

## Tasks / Subtasks

- [ ] Task 1: Extend Zustand filter store for price range (AC: #3, #4)
  - [ ] Subtask 1.1: Update `apps/web/src/store/filterStore.ts` with priceRange state
  - [ ] Subtask 1.2: Define PriceRange interface with min and max properties
  - [ ] Subtask 1.3: Add setPriceRange action to update both min and max values
  - [ ] Subtask 1.4: Add resetPriceRange action to restore defaults (min/max of all models)
  - [ ] Subtask 1.5: Update clearFilters action to reset price range to defaults
  - [ ] Subtask 1.6: Update getActiveFilterCount to include price range (if not default)
  - [ ] Subtask 1.7: Export price formatting utility for consistent display ($X.XX per 1M)

- [ ] Task 2: Create price calculation utility (AC: #5, #6)
  - [ ] Subtask 2.1: Create `apps/web/src/utils/priceCalculations.ts` file
  - [ ] Subtask 2.2: Implement calculateAveragePrice function (input + output) / 2
  - [ ] Subtask 2.3: Implement getMinMaxPrices function to find range from model array
  - [ ] Subtask 2.4: Add null/undefined safety for models without pricing data
  - [ ] Subtask 2.5: Export formatPrice function for currency formatting ($X.XX)
  - [ ] Subtask 2.6: Add unit tests for price calculations (edge cases: $0, very high prices)

- [ ] Task 3: Create PriceRangeFilter component with dual-range slider (AC: #2, #3, #4)
  - [ ] Subtask 3.1: Create `apps/web/src/components/filters/PriceRangeFilter.tsx` file
  - [ ] Subtask 3.2: Research and select dual-range slider library (rc-slider, react-range, or custom)
  - [ ] Subtask 3.3: Install selected slider library (e.g., `pnpm add rc-slider`)
  - [ ] Subtask 3.4: Implement dual-range slider with min and max thumb controls
  - [ ] Subtask 3.5: Connect slider to Zustand filterStore.priceRange state
  - [ ] Subtask 3.6: Add onChange handler that calls setPriceRange with debouncing (100ms)
  - [ ] Subtask 3.7: Display current min/max values above slider with currency formatting
  - [ ] Subtask 3.8: Add input fields for manual min/max entry (sync with slider)
  - [ ] Subtask 3.9: Implement input validation (min < max, positive numbers)
  - [ ] Subtask 3.10: Style slider with TailwindCSS (track, thumbs, active range)
  - [ ] Subtask 3.11: Add accessible ARIA labels for slider and inputs
  - [ ] Subtask 3.12: Display price per 1M tokens label with tooltip explanation

- [ ] Task 4: Integrate PriceRangeFilter into FilterSidebar (AC: #1)
  - [ ] Subtask 4.1: Import PriceRangeFilter component in FilterSidebar.tsx
  - [ ] Subtask 4.2: Add "Price Range" section heading with divider
  - [ ] Subtask 4.3: Position below CapabilitiesFilter section (Story 3.6)
  - [ ] Subtask 4.4: Add collapsible/expandable section (optional enhancement)
  - [ ] Subtask 4.5: Maintain consistent spacing and styling with other filters
  - [ ] Subtask 4.6: Add info tooltip explaining average price calculation

- [ ] Task 5: Initialize price range defaults from model data (AC: #6)
  - [ ] Subtask 5.1: In ModelTable or HomePage, calculate min/max prices from all models
  - [ ] Subtask 5.2: Call useFilterStore setPriceRange with calculated defaults on mount
  - [ ] Subtask 5.3: Use useModels hook data to get pricing bounds
  - [ ] Subtask 5.4: Handle loading state (show placeholder range while fetching)
  - [ ] Subtask 5.5: Update range when new models loaded (refetch scenario)
  - [ ] Subtask 5.6: Store default range separately from current range (for reset)

- [ ] Task 6: Implement price range filtering logic (AC: #4, #5)
  - [ ] Subtask 6.1: Add price range filter function in ModelTable.tsx
  - [ ] Subtask 6.2: Read priceRange from Zustand filterStore
  - [ ] Subtask 6.3: Calculate average price per model: (inputPrice + outputPrice) / 2
  - [ ] Subtask 6.4: Filter models where avgPrice >= min AND avgPrice <= max
  - [ ] Subtask 6.5: Update columnFilters when priceRange changes (useEffect)
  - [ ] Subtask 6.6: Combine with provider and capability filters (all filters active)
  - [ ] Subtask 6.7: Verify real-time filtering (target: <100ms)
  - [ ] Subtask 6.8: Handle edge cases (free models $0, very expensive models)

- [ ] Task 7: Add URL state sync for price range (AC: #3)
  - [ ] Subtask 7.1: Extend URL state sync logic to include priceMin and priceMax params
  - [ ] Subtask 7.2: Update URL when slider changes (debounced to avoid URL spam)
  - [ ] Subtask 7.3: Read priceMin/priceMax from URL on page load
  - [ ] Subtask 7.4: Initialize filterStore with URL values if present
  - [ ] Subtask 7.5: Test shareable URLs with price filters
  - [ ] Subtask 7.6: Handle invalid URL values (negative, non-numeric, min > max)

- [ ] Task 8: Test price range filtering functionality (AC: #1-6)
  - [ ] Subtask 8.1: Test slider drag to adjust min/max (verify table filters)
  - [ ] Subtask 8.2: Test manual input entry for min/max values
  - [ ] Subtask 8.3: Test validation (min < max constraint)
  - [ ] Subtask 8.4: Test reset to default range (Clear Filters button)
  - [ ] Subtask 8.5: Test combined with provider + capability filters
  - [ ] Subtask 8.6: Verify filter count badge includes price range (if not default)
  - [ ] Subtask 8.7: Test edge cases (narrow range with no models, full range)
  - [ ] Subtask 8.8: Test price calculation accuracy (avg of input + output)
  - [ ] Subtask 8.9: Verify performance <100ms with 50+ models
  - [ ] Subtask 8.10: Test URL sharing with price range parameters

- [ ] Task 9: Manual testing and verification (AC: All)
  - [ ] Subtask 9.1: Test in Chrome DevTools responsive mode (desktop, tablet, mobile)
  - [ ] Subtask 9.2: Verify slider usable on touch devices (large enough thumbs)
  - [ ] Subtask 9.3: Verify no console errors or warnings
  - [ ] Subtask 9.4: Verify no TypeScript errors (pnpm run type-check)
  - [ ] Subtask 9.5: Test accessibility - keyboard navigation for slider
  - [ ] Subtask 9.6: Verify visual consistency with ProviderFilter and CapabilitiesFilter
  - [ ] Subtask 9.7: Test slider animations smooth and responsive
  - [ ] Subtask 9.8: Verify currency formatting consistent ($X.XX per 1M)

## Dev Notes

### Architecture Patterns

**Price Range Filtering - Continuous Range vs Discrete Options:**

Story 3.7 introduces continuous range filtering (slider) compared to discrete checkbox filtering (Stories 3.5, 3.6):

```typescript
// Story 3.5 Provider Filter (OR logic, discrete checkboxes)
const providerFilterFn = (row, columnId, filterValue) => {
  const provider = row.getValue(columnId);
  return filterValue.length === 0 || filterValue.includes(provider);
};

// Story 3.6 Capabilities Filter (AND logic, discrete checkboxes)
const capabilitiesFilterFn = (row, columnId, filterValue) => {
  const capabilities = row.original.capabilities;
  if (filterValue.length === 0) return true;
  return filterValue.every(cap => capabilities?.[cap] === true);
};

// Story 3.7 Price Range Filter (Range logic, continuous slider)
const priceRangeFilterFn = (row, columnId, filterValue) => {
  const { min, max } = filterValue;
  const model = row.original;
  const avgPrice = (model.inputPricePer1M + model.outputPricePer1M) / 2;

  return avgPrice >= min && avgPrice <= max;
};
```

**Why Average Price (Input + Output) / 2?**

Per Tech Spec Epic 3 (line 1093):
- Users care about "typical cost" not just input or output price separately
- Average provides single comparable metric across models
- Matches PRD requirement: "total cost per 1M tokens (input + output avg)"
- Alternative considered: Weighted average based on typical 50/50 ratio → deferred to Epic 4 calculator

**Zustand Store Extension:**

```typescript
// apps/web/src/store/filterStore.ts
export interface PriceRange {
  min: number;  // Price per 1M tokens (USD)
  max: number;
}

interface FilterState {
  selectedProviders: string[];       // Story 3.5
  selectedCapabilities: CapabilityType[]; // Story 3.6
  priceRange: PriceRange;           // Story 3.7 - NEW
  defaultPriceRange: PriceRange;    // Story 3.7 - NEW (for reset)
  toggleProvider: (provider: string) => void;
  toggleCapability: (capability: CapabilityType) => void;
  setPriceRange: (range: PriceRange) => void; // NEW
  resetPriceRange: () => void;                // NEW
  clearFilters: () => void;
  getActiveFilterCount: () => number;
}

export const useFilterStore = create<FilterState>((set, get) => ({
  selectedProviders: [],
  selectedCapabilities: [],
  priceRange: { min: 0, max: 100 },        // Default placeholder
  defaultPriceRange: { min: 0, max: 100 }, // Will be set from actual data

  setPriceRange: (range) => set({ priceRange: range }),

  resetPriceRange: () => set((state) => ({
    priceRange: state.defaultPriceRange
  })),

  clearFilters: () => set((state) => ({
    selectedProviders: [],
    selectedCapabilities: [],
    priceRange: state.defaultPriceRange // Reset to defaults, not hardcoded 0-100
  })),

  getActiveFilterCount: () => {
    const { selectedProviders, selectedCapabilities, priceRange, defaultPriceRange } = get();
    let count = selectedProviders.length + selectedCapabilities.length;

    // Add 1 if price range is not default
    if (priceRange.min !== defaultPriceRange.min || priceRange.max !== defaultPriceRange.max) {
      count += 1;
    }

    return count;
  }
}));
```

**Price Calculation Utility:**

```typescript
// apps/web/src/utils/priceCalculations.ts
import type { ModelDto } from '@/types/model';

export function calculateAveragePrice(model: ModelDto): number {
  if (!model.inputPricePer1M || !model.outputPricePer1M) {
    return 0; // Handle models without pricing data
  }
  return (model.inputPricePer1M + model.outputPricePer1M) / 2;
}

export function getMinMaxPrices(models: ModelDto[]): PriceRange {
  if (models.length === 0) {
    return { min: 0, max: 100 }; // Fallback
  }

  const prices = models
    .map(calculateAveragePrice)
    .filter(price => price > 0); // Exclude free models from range calc

  if (prices.length === 0) {
    return { min: 0, max: 100 };
  }

  return {
    min: Math.floor(Math.min(...prices)),
    max: Math.ceil(Math.max(...prices))
  };
}

export function formatPrice(price: number): string {
  return new Intl.NumberFormat('en-US', {
    style: 'currency',
    currency: 'USD',
    minimumFractionDigits: 2,
    maximumFractionDigits: 2
  }).format(price);
}
```

**Dual-Range Slider Component Pattern:**

Recommended library: **rc-slider** (27K+ GitHub stars, actively maintained, accessible)

Alternative: **react-range** (lighter, more customizable, but more complex API)

```typescript
// apps/web/src/components/filters/PriceRangeFilter.tsx
import Slider from 'rc-slider';
import 'rc-slider/assets/index.css'; // Base styles
import { useFilterStore } from '@/store/filterStore';
import { formatPrice } from '@/utils/priceCalculations';
import { useMemo, useState } from 'react';

export function PriceRangeFilter() {
  const priceRange = useFilterStore(state => state.priceRange);
  const defaultPriceRange = useFilterStore(state => state.defaultPriceRange);
  const setPriceRange = useFilterStore(state => state.setPriceRange);

  // Local state for slider to avoid excessive Zustand updates
  const [localRange, setLocalRange] = useState(priceRange);

  const handleSliderChange = (values: number | number[]) => {
    const [min, max] = values as number[];
    setLocalRange({ min, max });
  };

  // Debounced commit to Zustand (fires on slider release)
  const handleSliderAfterChange = (values: number | number[]) => {
    const [min, max] = values as number[];
    setPriceRange({ min, max });
  };

  return (
    <div className="space-y-4">
      <h3 className="text-sm font-medium text-gray-700">Price Range</h3>

      {/* Current values display */}
      <div className="flex justify-between text-xs text-gray-600">
        <span>{formatPrice(localRange.min)} per 1M</span>
        <span>{formatPrice(localRange.max)} per 1M</span>
      </div>

      {/* Dual-range slider */}
      <Slider
        range
        min={defaultPriceRange.min}
        max={defaultPriceRange.max}
        value={[localRange.min, localRange.max]}
        onChange={handleSliderChange}
        onAfterChange={handleSliderAfterChange}
        className="px-1"
        aria-label="Price range filter"
      />

      {/* Manual input fields (optional enhancement) */}
      <div className="grid grid-cols-2 gap-2">
        <div>
          <label htmlFor="price-min" className="text-xs text-gray-600">Min</label>
          <input
            id="price-min"
            type="number"
            value={localRange.min}
            onChange={(e) => {
              const min = parseFloat(e.target.value) || 0;
              if (min < localRange.max) {
                const newRange = { min, max: localRange.max };
                setLocalRange(newRange);
                setPriceRange(newRange);
              }
            }}
            className="w-full px-2 py-1 text-sm border rounded"
          />
        </div>
        <div>
          <label htmlFor="price-max" className="text-xs text-gray-600">Max</label>
          <input
            id="price-max"
            type="number"
            value={localRange.max}
            onChange={(e) => {
              const max = parseFloat(e.target.value) || 0;
              if (max > localRange.min) {
                const newRange = { min: localRange.min, max };
                setLocalRange(newRange);
                setPriceRange(newRange);
              }
            }}
            className="w-full px-2 py-1 text-sm border rounded"
          />
        </div>
      </div>
    </div>
  );
}
```

**Default Range Initialization:**

Must happen after models loaded:

```typescript
// In HomePage.tsx or ModelTable.tsx
import { useModels } from '@/api/modelsApi';
import { useFilterStore } from '@/store/filterStore';
import { getMinMaxPrices } from '@/utils/priceCalculations';
import { useEffect } from 'react';

export function HomePage() {
  const { data: models, isLoading } = useModels();
  const setPriceRange = useFilterStore(state => state.setPriceRange);
  const defaultPriceRange = useFilterStore(state => state.defaultPriceRange);

  // Initialize default price range from actual model data
  useEffect(() => {
    if (models && models.length > 0) {
      const range = getMinMaxPrices(models);

      // Set both default and current range (only on first load)
      if (defaultPriceRange.min === 0 && defaultPriceRange.max === 100) {
        setPriceRange(range);
        // Also update defaultPriceRange in store (add setter action)
      }
    }
  }, [models]);

  // ... rest of component
}
```

**Performance Considerations:**

- **Debouncing slider changes:** Update Zustand only on `onAfterChange` (slider release), not every `onChange` (every pixel)
- **Local state optimization:** Use local React state for slider position during drag, commit to Zustand after
- **Filter memoization:** TanStack Table's `getFilteredRowModel` memoizes, preventing unnecessary re-filters
- **Price calculation caching:** Calculate avg price once per model, cache in useMemo
- **Target: <100ms** for filter execution with 50+ models (same as Stories 3.5, 3.6)

### Project Structure Notes

**Files Created:**

```
apps/web/
├── src/
│   ├── components/
│   │   └── filters/
│   │       └── PriceRangeFilter.tsx (NEW - dual-range slider component)
│   └── utils/
│       └── priceCalculations.ts (NEW - price calculation utilities)
```

**Files Modified:**

```
apps/web/
├── src/
│   ├── store/
│   │   └── filterStore.ts (MODIFIED - add priceRange, setPriceRange, resetPriceRange)
│   ├── components/
│   │   ├── filters/
│   │   │   └── FilterSidebar.tsx (MODIFIED - add PriceRangeFilter section)
│   │   └── models/
│   │       └── ModelTable.tsx (MODIFIED - add price range filter logic)
│   └── pages/
│       └── HomePage.tsx (MODIFIED - initialize default price range from model data)
```

**Dependencies Added:**

```bash
pnpm add rc-slider
pnpm add -D @types/rc-slider
```

**Alignment with Project Structure:**

- PriceRangeFilter follows same pattern as ProviderFilter (Story 3.5) and CapabilitiesFilter (Story 3.6)
- priceCalculations.ts utility for reusability in Epic 4 (cost calculator), Epic 6 (best value QAPS)
- Zustand store continues as single source of truth for all filter state

**Detected Conflicts:**

- None - Price range filter builds on Stories 3.5-3.6 FilterSidebar foundation

### References

**Source Documents:**

- [Epic 3 Story 3.7 Definition - epics.md:584-600](file:///home/pablitxn/repos/bmad-method/llm-token-price/docs/epics.md#584) - Acceptance criteria and prerequisites
- [Tech Spec Epic 3 - Price Range Filter - tech-spec-epic-3.md:2060-2064](file:///home/pablitxn/repos/bmad-method/llm-token-price/docs/tech-spec-epic-3.md#2060) - Dual-range slider spec, average price calculation
- [Tech Spec Epic 3 - Zustand State Management - tech-spec-epic-3.md:441-456](file:///home/pablitxn/repos/bmad-method/llm-token-price/docs/tech-spec-epic-3.md#441) - FilterState interface with priceRange
- [Tech Spec Epic 3 - Filter Logic - tech-spec-epic-3.md:1092-1096](file:///home/pablitxn/repos/bmad-method/llm-token-price/docs/tech-spec-epic-3.md#1092) - Price range filter implementation
- [Tech Spec Epic 3 - URL State Sync - tech-spec-epic-3.md:1229-1232](file:///home/pablitxn/repos/bmad-method/llm-token-price/docs/tech-spec-epic-3.md#1229) - Price range in URL params
- [Solution Architecture - Pricing Schema - solution-architecture.md](file:///home/pablitxn/repos/bmad-method/llm-token-price/docs/solution-architecture.md) - input_price_per_1M, output_price_per_1M fields

**Technical References:**

- rc-slider Documentation: https://slider-react-component.vercel.app/
- rc-slider GitHub: https://github.com/react-component/slider
- react-range (Alternative): https://github.com/tajo/react-range
- TanStack Table Custom Filters: https://tanstack.com/table/v8/docs/guide/column-filtering#custom-filter-functions
- Zustand TypeScript Guide: https://docs.pmnd.rs/zustand/guides/typescript
- Intl.NumberFormat (Currency): https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Intl/NumberFormat

**Architecture Constraints:**

- **Client-side only**: No backend changes (filtering happens in browser with TanStack Table)
- **Average price metric**: (input + output) / 2 per Tech Spec Epic 3
- **Performance**: Price range filter must complete <100ms combined with other filters (PRD NFR-002)
- **TypeScript Strict Mode**: Zero `any` types - use PriceRange interface
- **Zustand Pattern**: Extend existing filterStore (don't create new store)
- **Default range from data**: Min/max must be calculated from actual model prices, not hardcoded

**Dependencies from Previous Stories:**

- **Story 3.6**: CapabilitiesFilter (PriceRangeFilter added below in sidebar)
- **Story 3.5**: FilterSidebar component, Zustand filterStore pattern
- **Story 3.4**: Sorting (filters + sorting compose)
- **Story 3.3**: TanStack Table integration with getFilteredRowModel
- **Story 3.2**: useModels hook provides model data for price range calculation
- **Story 1.4**: Models table schema with input_price_per_1M, output_price_per_1M
- **Story 1.10**: GET /api/models returns pricing data

**Enables Future Stories:**

- **Story 3.8**: Search functionality (combines with all filters including price)
- **Story 4.6**: Cost calculator (reuses priceCalculations.ts utilities)
- **Story 6.2**: QAPS calculation (reuses average price logic)
- **Epic 8**: Mobile optimization (slider needs touch-friendly sizing)

### Testing Strategy

**Manual Testing (Required for Story 3.7):**

1. **Functional Testing:**
   - Drag min slider thumb → verify table filters to models >= min price
   - Drag max slider thumb → verify table filters to models <= max price
   - Drag both thumbs to narrow range → verify only models in range shown
   - Type value in min input → verify slider updates and table filters
   - Type value in max input → verify slider updates and table filters
   - Set invalid range (min > max) → verify validation prevents or corrects
   - Test combined filters: Provider + Capabilities + Price → all active

2. **Visual Testing:**
   - Verify PriceRangeFilter section displays below CapabilitiesFilter
   - Confirm slider track, thumbs, and active range styled consistently
   - Verify current min/max values display above slider with currency formatting
   - Test slider on mobile (large enough touch targets for thumbs)
   - Verify manual input fields styled consistently with filter sidebar

3. **Integration Testing:**
   - Apply provider filter (Story 3.5), capability filter (Story 3.6), then price filter
   - Verify combined filtering works correctly (all filters ANDed)
   - Apply filters, then sort (Story 3.4) → verify sorted subset
   - Click "Clear Filters" → verify price range resets to default (not 0-100)
   - Test with Story 3.2's useModels hook → verify loading states

4. **Range Initialization Testing:**
   - Load page with fresh data → verify range defaults to actual min/max of models
   - Test with models priced $0 (free) → verify excluded from range calculation
   - Test with very expensive models → verify ceiling set correctly
   - Verify default range stored separately from current range (for reset)

5. **Performance Testing:**
   - Measure filter time in Chrome DevTools (target: <100ms)
   - Test with 50+ models and all filters active (provider + capabilities + price)
   - Verify slider drag is smooth (no jank during onChange updates)
   - Verify debouncing prevents excessive Zustand updates (<10 updates during drag)
   - React DevTools Profiler: verify no unnecessary re-renders

6. **URL State Sync Testing:**
   - Adjust price range → verify URL updates with priceMin and priceMax params
   - Copy URL and paste in new tab → verify price filter restored
   - Test with invalid URL params (negative, non-numeric, min > max)
   - Verify debouncing prevents URL spam during slider drag

7. **Accessibility Testing:**
   - Keyboard navigation: Tab to slider, Arrow keys to adjust thumbs
   - Focus indicators visible on slider thumbs and input fields
   - Screen reader announces current price range values
   - ARIA labels correct for slider and input fields
   - Manual inputs have proper labels for screen readers

8. **Edge Cases Testing:**
   - Set price range to exclude all models → verify empty state message
   - Set price range to include all models → verify no filtering applied
   - Test with models missing pricing data → verify excluded from results
   - Test with very narrow range (e.g., $0.50 - $0.60) → verify precision

**Automated Testing (Deferred to Epic 3 Completion):**

- Component tests for PriceRangeFilter rendering and slider interactions
- Unit tests for priceCalculations.ts utilities
- Zustand store unit tests (setPriceRange, resetPriceRange, filter count)
- Integration tests for combined filter logic
- E2E tests for URL state sync with price params

**No Automated Tests Required for Story 3.7** per Epic 1/2 retrospective feedback - manual testing sufficient for MVP.

### Critical Implementation Details

**1. Average Price Calculation:**

Backend provides separate `inputPricePer1M` and `outputPricePer1M`. Frontend calculates average:

```typescript
interface ModelDto {
  id: string;
  name: string;
  provider: string;
  inputPricePer1M: number;  // USD per 1M input tokens
  outputPricePer1M: number; // USD per 1M output tokens
  // ... other fields
}

// In priceCalculations.ts
export function calculateAveragePrice(model: ModelDto): number {
  if (!model.inputPricePer1M || !model.outputPricePer1M) {
    return 0; // Free models or missing data
  }
  return (model.inputPricePer1M + model.outputPricePer1M) / 2;
}
```

**2. Default Range Calculation:**

Must exclude free models ($0) to avoid skewing range:

```typescript
export function getMinMaxPrices(models: ModelDto[]): PriceRange {
  const prices = models
    .map(calculateAveragePrice)
    .filter(price => price > 0); // Exclude free models

  if (prices.length === 0) {
    return { min: 0, max: 100 }; // Fallback if all models free
  }

  return {
    min: Math.floor(Math.min(...prices)),
    max: Math.ceil(Math.max(...prices))
  };
}
```

**3. Slider Library Configuration:**

rc-slider provides accessible, customizable dual-range slider:

```bash
# Install rc-slider
pnpm add rc-slider

# Import in component
import Slider from 'rc-slider';
import 'rc-slider/assets/index.css'; // Required base styles
```

TailwindCSS customization:

```css
/* In global CSS or component */
.rc-slider-track {
  @apply bg-blue-500;
}

.rc-slider-handle {
  @apply border-blue-500;
}

.rc-slider-handle:focus {
  @apply ring-2 ring-blue-300;
}
```

**4. Debouncing Strategy:**

Use local state during drag, commit to Zustand on release:

```typescript
const [localRange, setLocalRange] = useState(priceRange);

// Fires every pixel during drag (use for local state only)
const handleSliderChange = (values: number | number[]) => {
  const [min, max] = values as number[];
  setLocalRange({ min, max }); // Local state only
};

// Fires once on slider release (use for Zustand commit)
const handleSliderAfterChange = (values: number | number[]) => {
  const [min, max] = values as number[];
  setPriceRange({ min, max }); // Commit to Zustand
};
```

**5. Filter Count Logic:**

Only count price filter if different from default:

```typescript
getActiveFilterCount: () => {
  const { selectedProviders, selectedCapabilities, priceRange, defaultPriceRange } = get();
  let count = selectedProviders.length + selectedCapabilities.length;

  // Add 1 if price range is not default
  if (priceRange.min !== defaultPriceRange.min || priceRange.max !== defaultPriceRange.max) {
    count += 1;
  }

  return count;
}
```

**6. URL State Sync:**

Extend existing URL sync logic:

```typescript
// In HomePage.tsx or ModelTable.tsx
useEffect(() => {
  const params = new URLSearchParams();

  // Existing filters
  if (selectedProviders.length > 0) {
    params.set('providers', selectedProviders.join(','));
  }

  // Price range (NEW)
  if (priceRange.min !== defaultPriceRange.min || priceRange.max !== defaultPriceRange.max) {
    params.set('priceMin', priceRange.min.toString());
    params.set('priceMax', priceRange.max.toString());
  }

  setSearchParams(params, { replace: true });
}, [priceRange, selectedProviders]);
```

## Dev Agent Record

### Context Reference

- `docs/stories/story-context-3.7.xml` (to be generated during implementation)

### Agent Model Used

Claude Sonnet 4.5 (claude-sonnet-4-5-20250929)

### Debug Log References

<!-- Debug logs will be added during implementation -->

### Completion Notes List

<!-- Completion notes will be added after implementation -->

### File List

<!-- File list will be added after implementation -->

## Change Log

**2025-10-24** - Story 3.7 Drafted by Scrum Master Agent

- Created story draft based on Epic 3 requirements (epics.md:584-600)
- Extracted acceptance criteria from epics.md verbatim
- Defined 9 tasks with 53 subtasks covering PriceRangeFilter component, Zustand store extension, price calculation utilities, dual-range slider integration, and URL state sync
- Added comprehensive dev notes with average price calculation rationale, rc-slider integration pattern, debouncing strategy, and default range initialization
- Documented critical implementation details: average price calculation, default range from data (excluding free models), slider library configuration, performance optimization
- Established priceCalculations.ts utility pattern for reuse in Epic 4 (cost calculator) and Epic 6 (QAPS)
- Clarified continuous range filtering vs discrete checkbox filtering (Stories 3.5-3.6)
- Status: Draft (needs review via story-ready workflow)
