# Story 3.6: Add Capabilities Filters

Status: Draft

## Story

As a user,
I want to filter models by capabilities,
So that I can find models supporting specific features (function calling, vision, etc.).

## Acceptance Criteria

1. Capabilities filter section added to sidebar
2. Checkboxes for each capability: function calling, vision support, audio support, streaming, JSON mode
3. Checking capability filters to only models with that capability
4. Multiple capabilities use AND logic (must have all selected)
5. Filters update table immediately
6. Tooltip explains what each capability means

## Tasks / Subtasks

- [ ] Task 1: Extend Zustand filter store for capabilities (AC: #3, #4)
  - [ ] Subtask 1.1: Update `apps/web/src/store/filterStore.ts` with selectedCapabilities array
  - [ ] Subtask 1.2: Define CapabilityType enum matching backend ModelCapabilities fields
  - [ ] Subtask 1.3: Add toggleCapability action (add/remove capability from array)
  - [ ] Subtask 1.4: Update clearFilters action to reset capabilities array
  - [ ] Subtask 1.5: Update getActiveFilterCount to include capabilities count
  - [ ] Subtask 1.6: Export capability display names mapping for UI labels

- [ ] Task 2: Create CapabilitiesFilter component (AC: #2, #6)
  - [ ] Subtask 2.1: Create `apps/web/src/components/filters/CapabilitiesFilter.tsx` file
  - [ ] Subtask 2.2: Define capability list with display names and descriptions
  - [ ] Subtask 2.3: Render checkbox list for each capability (alphabetically sorted)
  - [ ] Subtask 2.4: Connect checkboxes to Zustand filterStore.toggleCapability action
  - [ ] Subtask 2.5: Add tooltip component showing capability description on hover
  - [ ] Subtask 2.6: Style tooltips with TailwindCSS (dark background, white text, positioned above)
  - [ ] Subtask 2.7: Add accessible ARIA labels and tooltip IDs for screen readers

- [ ] Task 3: Integrate CapabilitiesFilter into FilterSidebar (AC: #1)
  - [ ] Subtask 3.1: Import CapabilitiesFilter component in FilterSidebar.tsx
  - [ ] Subtask 3.2: Add "Capabilities" section heading with divider
  - [ ] Subtask 3.3: Position below ProviderFilter section (Story 3.5)
  - [ ] Subtask 3.4: Add collapsible/expandable section (optional enhancement)
  - [ ] Subtask 3.5: Maintain consistent spacing and styling with provider filter

- [ ] Task 4: Implement AND logic for capability filtering (AC: #3, #4)
  - [ ] Subtask 4.1: Add capabilities column filter function in ModelTable.tsx
  - [ ] Subtask 4.2: Read selectedCapabilities from Zustand filterStore
  - [ ] Subtask 4.3: Implement AND logic - model must have ALL selected capabilities
  - [ ] Subtask 4.4: Handle capability field variations (supports_function_calling vs function_calling)
  - [ ] Subtask 4.5: Update columnFilters when selectedCapabilities changes (useEffect)
  - [ ] Subtask 4.6: Combine with provider filter logic (both filters active simultaneously)
  - [ ] Subtask 4.7: Verify AND logic works correctly (select 2+ capabilities → only models with all)

- [ ] Task 5: Add tooltip implementation (AC: #6)
  - [ ] Subtask 5.1: Create reusable Tooltip component in `apps/web/src/components/ui/Tooltip.tsx`
  - [ ] Subtask 5.2: Implement hover state detection with onMouseEnter/onMouseLeave
  - [ ] Subtask 5.3: Position tooltip dynamically (above checkbox, centered)
  - [ ] Subtask 5.4: Add tooltip descriptions for each capability:
    - Function Calling: "Model can call external functions/tools during generation"
    - Vision: "Model can process and analyze images"
    - Audio Support: "Model supports audio input or output"
    - Streaming: "Model supports streaming responses for real-time output"
    - JSON Mode: "Model can output structured JSON responses"
  - [ ] Subtask 5.5: Add info icon (ⓘ) next to each capability label
  - [ ] Subtask 5.6: Implement tooltip animations (fade in/out with transition)

- [ ] Task 6: Update API types to include capabilities (AC: #3)
  - [ ] Subtask 6.1: Verify ModelDto in `apps/web/src/types/model.ts` includes capabilities fields
  - [ ] Subtask 6.2: Add TypeScript interface for ModelCapabilities if missing
  - [ ] Subtask 6.3: Ensure useModels hook returns capability data
  - [ ] Subtask 6.4: Verify backend GET /api/models includes ModelCapabilities in response
  - [ ] Subtask 6.5: Add null/undefined handling for models without capability data

- [ ] Task 7: Test capabilities filtering functionality (AC: #1-6)
  - [ ] Subtask 7.1: Test selecting single capability (verify table filters correctly)
  - [ ] Subtask 7.2: Test selecting multiple capabilities (verify AND logic - only models with all)
  - [ ] Subtask 7.3: Test unselecting capability (verify table updates immediately)
  - [ ] Subtask 7.4: Test combined with provider filter (Story 3.5 - both filters active)
  - [ ] Subtask 7.5: Verify "Clear Filters" clears both provider and capability selections
  - [ ] Subtask 7.6: Test tooltip displays on hover for each capability
  - [ ] Subtask 7.7: Verify filter count badge includes capability filters
  - [ ] Subtask 7.8: Test edge cases (all capabilities selected, no matching models, null capabilities)
  - [ ] Subtask 7.9: Verify performance <100ms for filter operations with 50+ models

- [ ] Task 8: Manual testing and verification (AC: All)
  - [ ] Subtask 8.1: Test in Chrome DevTools responsive mode (desktop, tablet, mobile)
  - [ ] Subtask 8.2: Verify tooltips readable on mobile (touch interaction)
  - [ ] Subtask 8.3: Verify no console errors or warnings
  - [ ] Subtask 8.4: Verify no TypeScript errors (pnpm run type-check)
  - [ ] Subtask 8.5: Test accessibility - keyboard navigation, screen reader tooltip announcements
  - [ ] Subtask 8.6: Verify visual consistency with ProviderFilter (Story 3.5)
  - [ ] Subtask 8.7: Test AND vs OR logic difference (Story 3.5 OR, Story 3.6 AND)
  - [ ] Subtask 8.8: Verify filters persist during sorting (Story 3.4)

## Dev Notes

### Architecture Patterns

**AND vs OR Filter Logic:**
Story 3.6 introduces AND logic for capabilities (different from Story 3.5's OR logic for providers):

```typescript
// Story 3.5 Provider Filter (OR logic)
const providerFilterFn = (row, columnId, filterValue) => {
  const provider = row.getValue(columnId);
  return filterValue.length === 0 || filterValue.includes(provider);
};

// Story 3.6 Capabilities Filter (AND logic)
const capabilitiesFilterFn = (row, columnId, filterValue) => {
  const capabilities = row.original.capabilities;
  if (filterValue.length === 0) return true;

  // Model must have ALL selected capabilities
  return filterValue.every(cap => capabilities?.[cap] === true);
};
```

**Why AND logic for capabilities?**
- Users want models that support function calling AND vision AND streaming
- Selecting "vision" + "function calling" means "models that have both features"
- OR logic would be too permissive ("models with vision OR function calling")

**Zustand Store Extension:**
```typescript
// apps/web/src/store/filterStore.ts
type CapabilityType =
  | 'supports_function_calling'
  | 'supports_vision'
  | 'supports_audio_input'
  | 'supports_streaming'
  | 'supports_json_mode';

interface FilterState {
  selectedProviders: string[]; // Story 3.5
  selectedCapabilities: CapabilityType[]; // Story 3.6 - NEW
  toggleProvider: (provider: string) => void;
  toggleCapability: (capability: CapabilityType) => void; // NEW
  clearFilters: () => void;
  getActiveFilterCount: () => number;
}

export const useFilterStore = create<FilterState>((set, get) => ({
  selectedProviders: [],
  selectedCapabilities: [], // NEW
  toggleProvider: (provider) => set((state) => ({
    selectedProviders: state.selectedProviders.includes(provider)
      ? state.selectedProviders.filter(p => p !== provider)
      : [...state.selectedProviders, provider]
  })),
  toggleCapability: (capability) => set((state) => ({ // NEW
    selectedCapabilities: state.selectedCapabilities.includes(capability)
      ? state.selectedCapabilities.filter(c => c !== capability)
      : [...state.selectedCapabilities, capability]
  })),
  clearFilters: () => set({
    selectedProviders: [],
    selectedCapabilities: [] // Updated to clear both
  }),
  getActiveFilterCount: () => {
    const { selectedProviders, selectedCapabilities } = get();
    return selectedProviders.length + selectedCapabilities.length; // Updated
  }
}));
```

**Capability Display Names Mapping:**
```typescript
// In CapabilitiesFilter.tsx
const CAPABILITIES = [
  {
    key: 'supports_function_calling' as const,
    label: 'Function Calling',
    description: 'Model can call external functions/tools during generation',
    icon: '🔧'
  },
  {
    key: 'supports_vision' as const,
    label: 'Vision Support',
    description: 'Model can process and analyze images',
    icon: '👁️'
  },
  {
    key: 'supports_audio_input' as const,
    label: 'Audio Support',
    description: 'Model supports audio input or output',
    icon: '🎤'
  },
  {
    key: 'supports_streaming' as const,
    label: 'Streaming',
    description: 'Model supports streaming responses for real-time output',
    icon: '⚡'
  },
  {
    key: 'supports_json_mode' as const,
    label: 'JSON Mode',
    description: 'Model can output structured JSON responses',
    icon: '{ }'
  }
];
```

**Tooltip Implementation Pattern:**
Reusable tooltip component using Radix UI primitives or custom implementation:

```typescript
// apps/web/src/components/ui/Tooltip.tsx
import { useState } from 'react';

interface TooltipProps {
  content: string;
  children: React.ReactNode;
}

export function Tooltip({ content, children }: TooltipProps) {
  const [isVisible, setIsVisible] = useState(false);

  return (
    <div className="relative inline-block">
      <div
        onMouseEnter={() => setIsVisible(true)}
        onMouseLeave={() => setIsVisible(false)}
        onFocus={() => setIsVisible(true)}
        onBlur={() => setIsVisible(false)}
      >
        {children}
      </div>
      {isVisible && (
        <div className="absolute bottom-full left-1/2 -translate-x-1/2 mb-2 px-3 py-2 bg-gray-900 text-white text-sm rounded shadow-lg whitespace-nowrap z-50 transition-opacity">
          {content}
          <div className="absolute top-full left-1/2 -translate-x-1/2 -mt-1 border-4 border-transparent border-t-gray-900" />
        </div>
      )}
    </div>
  );
}
```

**Combined Filter Logic:**
Multiple filters combine with TanStack Table's columnFilters:

```typescript
// In ModelTable.tsx
const table = useReactTable({
  data,
  columns,
  getCoreRowModel: getCoreRowModel(),
  getSortedRowModel: getSortedRowModel(),
  getFilteredRowModel: getFilteredRowModel(),
  state: {
    sorting,
    columnFilters // Contains both provider and capabilities filters
  },
  filterFns: {
    providerFilter: providerFilterFn,      // Story 3.5 - OR logic
    capabilitiesFilter: capabilitiesFilterFn // Story 3.6 - AND logic
  }
});
```

**Performance Considerations:**
- Capability filtering adds minimal overhead (<10ms) due to boolean checks
- Combined with provider filter: both execute, total <100ms target
- TanStack Table's memoization prevents unnecessary re-filters

### Project Structure Notes

**Files Created:**
```
apps/web/
├── src/
│   ├── components/
│   │   ├── filters/
│   │   │   └── CapabilitiesFilter.tsx (NEW - capabilities checkbox list)
│   │   └── ui/
│   │       └── Tooltip.tsx (NEW - reusable tooltip component)
```

**Files Modified:**
```
apps/web/
├── src/
│   ├── store/
│   │   └── filterStore.ts (MODIFIED - add selectedCapabilities, toggleCapability)
│   ├── components/
│   │   ├── filters/
│   │   │   └── FilterSidebar.tsx (MODIFIED - add CapabilitiesFilter section)
│   │   └── models/
│   │       └── ModelTable.tsx (MODIFIED - add capabilities filter logic)
│   └── types/
│       └── model.ts (MODIFIED - verify ModelCapabilities interface exists)
```

**Alignment with Project Structure:**
- CapabilitiesFilter follows same pattern as ProviderFilter (Story 3.5)
- Tooltip component in `components/ui/` for reusability (future stories 3.7+)
- Zustand store extension maintains single source of truth for all filters

**Detected Conflicts:**
- None - Capabilities filter builds on Story 3.5 FilterSidebar foundation

### References

**Source Documents:**
- [Epic 3 Story 3.6 Definition - epics.md:567-582](file:///home/pablitxn/repos/bmad-method/llm-token-price/docs/epics.md#567) - Acceptance criteria and prerequisites
- [Tech Spec Epic 3 - Filter Sidebar Component - tech-spec-epic-3.md:340-384](file:///home/pablitxn/repos/bmad-method/llm-token-price/docs/tech-spec-epic-3.md#340) - Component architecture
- [Tech Spec Epic 3 - Zustand State Management - tech-spec-epic-3.md:189-200](file:///home/pablitxn/repos/bmad-method/llm-token-price/docs/tech-spec-epic-3.md#189) - State management patterns
- [Solution Architecture - ModelCapabilities Schema - solution-architecture.md](file:///home/pablitxn/repos/bmad-method/llm-token-price/docs/solution-architecture.md) - Database schema for capabilities
- [Story 3.5 Implementation - story-3.5.md:79-147](file:///home/pablitxn/repos/bmad-method/llm-token-price/docs/stories/story-3.5.md#79) - Zustand filter store pattern established
- [Story 1.4 - Core Data Models - epics.md:94-108](file:///home/pablitxn/repos/bmad-method/llm-token-price/docs/epics.md#94) - ModelCapabilities table definition

**Technical References:**
- TanStack Table Filter Functions: https://tanstack.com/table/v8/docs/guide/column-filtering#filter-functions
- TanStack Table Custom Filter Fns: https://tanstack.com/table/v8/docs/api/features/filters#filterfns
- Zustand TypeScript Guide: https://docs.pmnd.rs/zustand/guides/typescript
- TailwindCSS Tooltips: https://tailwindcss.com/docs/hover-focus-and-other-states
- Radix UI Tooltip (alternative): https://www.radix-ui.com/docs/primitives/components/tooltip

**Architecture Constraints:**
- **Client-side only**: No backend changes (filtering happens in browser)
- **AND Logic**: Must implement AND semantics (different from Story 3.5 OR logic)
- **Performance**: Combined filters (provider + capabilities) must complete <100ms (PRD NFR-002)
- **TypeScript Strict Mode**: Zero `any` types - use CapabilityType literal union
- **Zustand Pattern**: Extend existing filterStore (don't create new store)

**Dependencies from Previous Stories:**
- **Story 3.5**: FilterSidebar component, Zustand filterStore, provider filter pattern
- **Story 3.4**: Sorting (filters + sorting compose)
- **Story 3.3**: TanStack Table integration
- **Story 3.2**: useModels hook
- **Story 1.4**: ModelCapabilities schema in database
- **Story 1.10**: GET /api/models returns capabilities data

**Enables Future Stories:**
- **Story 3.7**: Price range filter (extends filterStore, adds to FilterSidebar)
- **Story 3.8**: Search functionality (combines with all filters)
- **Story 3.10**: Checkbox selection (separate concern from filtering)
- **Story 3.14**: Capabilities icons (can reuse CAPABILITIES mapping with icons)

### Testing Strategy

**Manual Testing (Required for Story 3.6):**
1. **Functional Testing:**
   - Select single capability → verify table shows only models with that capability
   - Select multiple capabilities → verify table shows only models with ALL selected (AND logic)
   - Select "Function Calling" + "Vision" → verify only models with both features shown
   - Uncheck capability → verify table updates immediately
   - Test combined filters: Select provider + capability → verify both filters active

2. **Visual Testing:**
   - Verify CapabilitiesFilter section displays below ProviderFilter
   - Confirm checkboxes have consistent styling with provider filter
   - Hover over info icon → verify tooltip displays with description
   - Verify tooltip positioning (centered above, readable)
   - Test tooltip on mobile (touch interaction shows tooltip)

3. **Integration Testing:**
   - Apply provider filter (Story 3.5), then capability filter → verify combined filtering
   - Apply filters, then sort (Story 3.4) → verify sorted subset
   - Click "Clear Filters" → verify both provider and capability selections cleared
   - Test with Story 3.2's useModels hook → verify loading states

4. **Logic Testing:**
   - Verify AND logic vs OR logic difference:
     - Provider: OpenAI OR Anthropic (shows models from either)
     - Capabilities: Vision AND Function Calling (shows models with both)
   - Test edge cases:
     - All capabilities selected → very few/no models
     - Model with null/undefined capabilities → excluded from filtered results
     - No matching models → empty state message

5. **Performance Testing:**
   - Measure filter time in Chrome DevTools (target: <100ms)
   - Test with 50+ models and multiple filters active
   - Verify no unnecessary re-renders (React DevTools Profiler)

6. **Accessibility Testing:**
   - Keyboard navigation (Tab to checkboxes, Space to toggle)
   - Tooltip keyboard access (Focus triggers tooltip)
   - Screen reader announces capability labels and descriptions
   - Focus indicators visible on all interactive elements

**Automated Testing (Deferred to Epic 3 Completion):**
- Component tests for CapabilitiesFilter rendering
- Zustand store unit tests (toggleCapability AND logic)
- E2E tests for combined filter interaction

**No Automated Tests Required for Story 3.6** per Epic 1/2 retrospective feedback - manual testing sufficient.

### Critical Implementation Details

**1. Capability Field Naming:**
Backend uses `supports_*` prefix (e.g., `supports_function_calling`). Frontend should match:

```typescript
interface ModelCapabilities {
  context_window: number;
  max_output_tokens: number;
  supports_function_calling: boolean;
  supports_vision: boolean;
  supports_audio_input: boolean;
  supports_audio_output: boolean;
  supports_streaming: boolean;
  supports_json_mode: boolean;
}
```

**2. Null Safety:**
Models may have `capabilities: null` if not seeded yet. Handle gracefully:

```typescript
const capabilitiesFilterFn = (row, columnId, filterValue) => {
  const capabilities = row.original.capabilities;
  if (!capabilities) return false; // Exclude models without capabilities data
  if (filterValue.length === 0) return true;

  return filterValue.every(cap => capabilities[cap] === true);
};
```

**3. Tooltip Accessibility:**
Ensure tooltips work for both mouse and keyboard users:

```typescript
<Tooltip content={capability.description}>
  <label className="flex items-center gap-2 cursor-pointer">
    <input
      type="checkbox"
      checked={selectedCapabilities.includes(capability.key)}
      onChange={() => toggleCapability(capability.key)}
      aria-describedby={`tooltip-${capability.key}`}
    />
    <span>{capability.label}</span>
    <span className="text-gray-400 text-xs">ⓘ</span>
  </label>
</Tooltip>
```

**4. Filter Count Display:**
Filter badge should show combined count:

```typescript
// In FilterSidebar.tsx
const activeCount = useFilterStore(state => state.getActiveFilterCount());

// Display: "Filters (3)" when provider=1, capabilities=2
```

## Dev Agent Record

### Context Reference

- `docs/stories/story-context-3.6.xml` (to be generated during implementation)

### Agent Model Used

Claude Sonnet 4.5 (claude-sonnet-4-5-20250929)

### Debug Log References

<!-- Debug logs will be added during implementation -->

### Completion Notes List

<!-- Completion notes will be added after implementation -->

### File List

<!-- File list will be added after implementation -->

## Change Log

**2025-10-24** - Story 3.6 Drafted by Product Manager Agent
- Created story draft based on Epic 3 requirements (epics.md:567-582)
- Extracted acceptance criteria from epics.md verbatim
- Defined 8 tasks with 49 subtasks covering CapabilitiesFilter component, Zustand store extension, AND filter logic, and Tooltip component
- Added comprehensive dev notes with AND vs OR logic patterns, capability display names mapping, tooltip implementation, and combined filter architecture
- Documented critical implementation details: capability field naming, null safety, tooltip accessibility, filter count display
- Established Tooltip component pattern for reuse in Stories 3.7+ (price range slider tooltips, etc.)
- Clarified AND logic rationale: users want models with ALL selected capabilities (different from provider OR logic)
- Status: Draft (needs review via story-ready workflow)
