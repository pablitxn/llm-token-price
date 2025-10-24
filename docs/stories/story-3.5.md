# Story 3.5: Add Provider Filter

Status: Ready

## Story

As a user,
I want to filter models by provider,
So that I can focus on specific vendors (OpenAI, Anthropic, Google, etc.).

## Acceptance Criteria

1. Filter sidebar created on left side of table
2. Provider filter section displays list of all providers with checkboxes
3. Checking/unchecking provider filters table in real-time
4. Multiple providers selectable (OR logic)
5. "Clear Filters" button resets all selections
6. Filter state shows count of active filters

## Tasks / Subtasks

- [ ] Task 1: Create FilterSidebar component structure (AC: #1)
  - [ ] Subtask 1.1: Create `apps/web/src/components/filters/FilterSidebar.tsx` component file
  - [ ] Subtask 1.2: Add basic layout with TailwindCSS (sidebar container, heading, filter sections)
  - [ ] Subtask 1.3: Position sidebar on left side of table using CSS Grid or Flexbox layout
  - [ ] Subtask 1.4: Make sidebar responsive (collapsible on mobile, visible on desktop)
  - [ ] Subtask 1.5: Import and integrate FilterSidebar into HomePage component

- [ ] Task 2: Create Zustand filter store for client state (AC: #3, #4)
  - [ ] Subtask 2.1: Create `apps/web/src/store/filterStore.ts` file
  - [ ] Subtask 2.2: Define FilterState interface with selectedProviders array
  - [ ] Subtask 2.3: Initialize Zustand store with create() function
  - [ ] Subtask 2.4: Add toggleProvider action (add/remove provider from array)
  - [ ] Subtask 2.5: Add clearFilters action (reset to empty array)
  - [ ] Subtask 2.6: Add getActiveFilterCount selector

- [ ] Task 3: Create ProviderFilter component (AC: #2, #4)
  - [ ] Subtask 3.1: Create `apps/web/src/components/filters/ProviderFilter.tsx` file
  - [ ] Subtask 3.2: Extract unique providers from model data (useModels hook)
  - [ ] Subtask 3.3: Render checkbox list with provider names (alphabetically sorted)
  - [ ] Subtask 3.4: Connect checkboxes to Zustand filterStore.toggleProvider action
  - [ ] Subtask 3.5: Display checkmark icon or checked state visually
  - [ ] Subtask 3.6: Add accessible labels and ARIA attributes for screen readers

- [ ] Task 4: Implement real-time table filtering (AC: #3)
  - [ ] Subtask 4.1: Import getFilteredRowModel from @tanstack/react-table in ModelTable.tsx
  - [ ] Subtask 4.2: Add getFilteredRowModel to useReactTable configuration
  - [ ] Subtask 4.3: Read selectedProviders from Zustand filterStore in ModelTable
  - [ ] Subtask 4.4: Add columnFilters state to TanStack Table
  - [ ] Subtask 4.5: Update columnFilters when selectedProviders changes (useEffect hook)
  - [ ] Subtask 4.6: Define provider column filter function (checks if model.provider in selectedProviders)
  - [ ] Subtask 4.7: Verify filtered rows update in <100ms (performance target)

- [ ] Task 5: Add "Clear Filters" button (AC: #5)
  - [ ] Subtask 5.1: Add "Clear Filters" button to FilterSidebar component
  - [ ] Subtask 5.2: Connect button onClick to filterStore.clearFilters action
  - [ ] Subtask 5.3: Style button with TailwindCSS (secondary button style)
  - [ ] Subtask 5.4: Disable button when no filters active (conditional rendering)
  - [ ] Subtask 5.5: Add hover and focus states for accessibility

- [ ] Task 6: Display active filter count (AC: #6)
  - [ ] Subtask 6.1: Add filter count badge to FilterSidebar heading
  - [ ] Subtask 6.2: Use filterStore.getActiveFilterCount selector
  - [ ] Subtask 6.3: Display count as badge (e.g., "Filters (2)")
  - [ ] Subtask 6.4: Hide badge when count is 0
  - [ ] Subtask 6.5: Style badge with TailwindCSS (pill style, primary color)

- [ ] Task 7: Test Zustand filter store behavior (AC: #3, #4, #5) - CRITICAL for Stories 3.6-3.7
  - [ ] Subtask 7.1: Test toggleProvider adds provider to selectedProviders array when not present
  - [ ] Subtask 7.2: Test toggleProvider removes provider from selectedProviders array when already present
  - [ ] Subtask 7.3: Test clearFilters resets selectedProviders to empty array []
  - [ ] Subtask 7.4: Test getActiveFilterCount returns correct count (0 when empty, 1, 2, 3 when populated)
  - [ ] Subtask 7.5: Test Zustand store persists across FilterSidebar component remounts (navigate away, return)
  - [ ] Subtask 7.6: Test multiple components can read same filterStore state (verify singleton pattern)
  - [ ] Subtask 7.7: Test store state updates trigger re-renders in subscribed components
  - [ ] Subtask 7.8: Test initial state is empty array (no filters active on first load)

- [ ] Task 8: Test filtering functionality (AC: #1-6)
  - [ ] Subtask 8.1: Test selecting single provider (verify table filters correctly)
  - [ ] Subtask 8.2: Test selecting multiple providers (verify OR logic - shows all selected)
  - [ ] Subtask 8.3: Test unselecting provider (verify table updates immediately)
  - [ ] Subtask 8.4: Test "Clear Filters" button (verify all selections reset)
  - [ ] Subtask 8.5: Verify filter count badge updates correctly
  - [ ] Subtask 8.6: Test with sorting enabled (Story 3.4 - verify filters + sorting work together)
  - [ ] Subtask 8.7: Test edge case: all providers selected (verify table shows all models, equivalent to no filter)
  - [ ] Subtask 8.8: Test edge case: no providers selected (verify table shows all models)
  - [ ] Subtask 8.9: Test edge case: single model per provider (verify filter shows exactly 1 model)
  - [ ] Subtask 8.10: Test edge case: provider with zero models (verify empty state handling)
  - [ ] Subtask 8.11: Verify performance <100ms for filter operations with 50+ models

- [ ] Task 9: Test error handling and resilience (AC: All)
  - [ ] Subtask 9.1: Test FilterSidebar behavior if useModels hook returns error (verify graceful degradation)
  - [ ] Subtask 9.2: Test FilterSidebar behavior if useModels returns empty array (verify "No providers available" message)
  - [ ] Subtask 9.3: Test ProviderFilter handles undefined provider names gracefully
  - [ ] Subtask 9.4: Test filterStore handles rapid toggleProvider calls (stress test for race conditions)
  - [ ] Subtask 9.5: Test filter application when TanStack Table is still loading data

- [ ] Task 10: Manual testing and verification (AC: All)
  - [ ] Subtask 10.1: Test in Chrome DevTools responsive mode (desktop, tablet, mobile)
  - [ ] Subtask 10.2: Verify sidebar collapses/expands on mobile (if implemented)
  - [ ] Subtask 10.3: Verify no console errors or warnings
  - [ ] Subtask 10.4: Verify no TypeScript errors (pnpm run type-check)
  - [ ] Subtask 10.5: Test accessibility - keyboard navigation and screen reader support
  - [ ] Subtask 10.6: Verify visual design matches TailwindCSS theme (consistent with Story 3.3-3.4)

## Dev Notes

### Architecture Patterns

**TanStack Table Filtering Architecture:**
TanStack Table provides built-in filtering through the `getFilteredRowModel` function. This composes with sorting from Story 3.4:

```typescript
// Composable row models: Core → Sorted → Filtered
const table = useReactTable({
  data,
  columns,
  getCoreRowModel: getCoreRowModel(),        // Story 3.3 - base rows
  getSortedRowModel: getSortedRowModel(),    // Story 3.4 - sorted rows
  getFilteredRowModel: getFilteredRowModel(), // Story 3.5 - filtered rows
  state: {
    sorting,
    columnFilters // NEW in Story 3.5
  }
})
```

**Zustand Filter Store Pattern:**
Story 3.5 introduces Zustand for client-side filter state management. This is the first global state in Epic 3:

```typescript
// apps/web/src/store/filterStore.ts
interface FilterState {
  selectedProviders: string[];
  toggleProvider: (provider: string) => void;
  clearFilters: () => void;
  getActiveFilterCount: () => number;
}

export const useFilterStore = create<FilterState>((set, get) => ({
  selectedProviders: [],
  toggleProvider: (provider) => set((state) => ({
    selectedProviders: state.selectedProviders.includes(provider)
      ? state.selectedProviders.filter(p => p !== provider)
      : [...state.selectedProviders, provider]
  })),
  clearFilters: () => set({ selectedProviders: [] }),
  getActiveFilterCount: () => get().selectedProviders.length
}));
```

**Filter Logic (OR Semantics):**
Multiple provider selections use OR logic (show models from ANY selected provider):

```typescript
// In ModelTable.tsx
const filterFn = (row, columnId, filterValue) => {
  const provider = row.getValue(columnId);
  return filterValue.length === 0 || filterValue.includes(provider);
};
```

**State Management Strategy:**
- **Zustand**: Filter selections (client-side, persists across component remounts)
- **TanStack Table**: Filtered row state (derived from Zustand + table data)
- **TanStack Query**: Server data (unchanged - no server-side filtering in Epic 3)

**Performance Considerations:**
- Client-side filtering with TanStack Table is optimized (memoization)
- Filtering 50-100 models should take <50ms
- `getFilteredRowModel` only re-runs when columnFilters or data changes
- No API calls on filter changes (pure client-side)

### Project Structure Notes

**Files Created:**
```
apps/web/
├── src/
│   ├── components/
│   │   └── filters/
│   │       ├── FilterSidebar.tsx (NEW - main filter container)
│   │       └── ProviderFilter.tsx (NEW - provider checkbox list)
│   └── store/
│       └── filterStore.ts (NEW - Zustand filter state)
```

**Files Modified:**
```
apps/web/
├── src/
│   ├── pages/
│   │   └── HomePage.tsx (MODIFIED - integrate FilterSidebar)
│   └── components/
│       └── models/
│           └── ModelTable.tsx (MODIFIED - add getFilteredRowModel, columnFilters state)
```

**Alignment with Project Structure:**
- Component organization by domain: `components/filters/` follows pattern from `components/models/`
- Zustand store in `store/` directory (new directory, follows standard React project structure)
- Filter sidebar positioned via CSS Grid in HomePage (maintains responsive layout)

**Detected Conflicts:**
- None - Filtering is an additive feature on top of Story 3.3-3.4 foundation

### References

**Source Documents:**
- [Epic 3 Story 3.5 Definition - epics.md:549-564](file:///home/pablitxn/repos/bmad-method/llm-token-price/docs/epics.md#549) - Acceptance criteria and prerequisites
- [Tech Spec Epic 3 - Filter Sidebar Component - tech-spec-epic-3.md:340-384](file:///home/pablitxn/repos/bmad-method/llm-token-price/docs/tech-spec-epic-3.md#340) - Component architecture
- [Tech Spec Epic 3 - Zustand State Management - tech-spec-epic-3.md:189-200](file:///home/pablitxn/repos/bmad-method/llm-token-price/docs/tech-spec-epic-3.md#189) - State management patterns
- [Solution Architecture - Performance Targets - solution-architecture.md](file:///home/pablitxn/repos/bmad-method/llm-token-price/docs/solution-architecture.md) - <100ms client-side operation target
- [Story 3.4 Implementation - story-3.4.md:79-108](file:///home/pablitxn/repos/bmad-method/llm-token-price/docs/stories/story-3.4.md#79) - Confirms filtering can be added via getFilteredRowModel

**Technical References:**
- TanStack Table Filtering Guide: https://tanstack.com/table/v8/docs/guide/column-filtering
- getFilteredRowModel API: https://tanstack.com/table/v8/docs/api/features/column-filtering
- Column Filter Functions: https://tanstack.com/table/v8/docs/guide/column-filtering#filter-functions
- Zustand Documentation: https://zustand-demo.pmnd.rs/
- Zustand TypeScript Guide: https://docs.pmnd.rs/zustand/guides/typescript

**Architecture Constraints:**
- **Client-side only**: No backend changes (filtering happens in browser with TanStack Table)
- **Performance**: Filtering must complete in <100ms for 50+ models (PRD NFR-002)
- **TypeScript Strict Mode**: Zero `any` types - use TanStack Table's ColumnFiltersState type
- **Zustand Best Practices**: Use selectors to avoid unnecessary re-renders

**Dependencies from Previous Stories:**
- **Story 3.4**: Sorting implementation (filtering + sorting compose via row models)
- **Story 3.3**: TanStack Table integration with useReactTable hook
- **Story 3.2**: useModels hook providing model data
- **Story 1.10**: GET /api/models endpoint returning provider field
- **Story 1.2**: TailwindCSS configuration, Zustand package installed

**Enables Future Stories:**
- **Story 3.6**: Capabilities filters (adds more filter sections to FilterSidebar)
- **Story 3.7**: Price range filter (uses same Zustand filterStore pattern)
- **Story 3.8**: Search functionality (combines with provider/capability filters)
- **Story 3.11**: Comparison basket (uses Zustand pattern established here)

### Testing Strategy

**Manual Testing (Required for Story 3.5):**

**1. Zustand Store Behavior Testing (Task 7 - CRITICAL):**
   - **Purpose:** Validate foundational state management pattern for Stories 3.6-3.7 and 3.11
   - Test toggleProvider adds/removes providers correctly (Subtask 7.1-7.2)
   - Test clearFilters resets to empty state (Subtask 7.3)
   - Test getActiveFilterCount returns accurate counts (Subtask 7.4)
   - Test store persistence across component remounts (Subtask 7.5)
   - Test singleton pattern with multiple components (Subtask 7.6)
   - Test reactivity triggers re-renders (Subtask 7.7)
   - Verify initial state is empty (Subtask 7.8)

   **Tool:** Browser Console → `window.__ZUSTAND_DEVTOOLS__` or React DevTools

**2. Functional Testing (Task 8):**
   - Select single provider checkbox → verify table shows only that provider's models (Subtask 8.1)
   - Select multiple providers → verify table shows models from ALL selected providers - OR logic (Subtask 8.2)
   - Uncheck provider → verify table updates immediately (Subtask 8.3)
   - Click "Clear Filters" → verify all checkboxes unchecked and full table restored (Subtask 8.4)
   - Verify filter count badge displays correct count (Subtask 8.5)
   - Test with sorting enabled (Story 3.4) → verify filtered subset sorts correctly (Subtask 8.6)

   **Edge Cases (Subtasks 8.7-8.10):**
   - All providers selected → should show all models (equivalent to no filter)
   - No providers selected → should show all models (initial state)
   - Single model per provider → verify filter shows exactly 1 model
   - Provider with zero models → verify empty state handling

**3. Visual Testing (Task 10.1-10.2):**
   - Verify FilterSidebar displays on left side of table
   - Confirm checkboxes have visible checked/unchecked states
   - Verify filter count badge displays correctly (shows/hides based on count)
   - Test responsive layout (sidebar behavior on mobile/tablet/desktop)
   - Verify sidebar collapse/expand behavior on mobile breakpoints

**4. Integration Testing (Story 3.4 Composition):**
   - Apply filter, then sort → verify filtered subset sorts correctly
   - Sort, then apply filter → verify sorted order maintained after filtering
   - Test with Story 3.2's useModels hook → verify loading states during data fetch

   **Critical Test:** Verify TanStack Table row model chain: `getCoreRowModel → getSortedRowModel → getFilteredRowModel`

**5. Performance Testing (Task 8.11):**
   - **Target:** <100ms filter time (PRD NFR-002)
   - **Tool:** Chrome DevTools Performance tab → measure filter application time
   - Test with 50+ models (no UI lag or perceived delay)
   - Verify no unnecessary re-renders (React DevTools Profiler → Flamegraph)
   - Test rapid filter toggles (10 clicks/second) → should remain responsive

**6. Error Handling & Resilience (Task 9):**
   - Test FilterSidebar if useModels hook returns error → graceful degradation (Subtask 9.1)
   - Test FilterSidebar if useModels returns empty array → "No providers available" message (Subtask 9.2)
   - Test ProviderFilter with undefined provider names (Subtask 9.3)
   - Test filterStore with rapid toggleProvider calls → race condition stress test (Subtask 9.4)
   - Test filter application while TanStack Table loading data (Subtask 9.5)

**7. Accessibility Testing (Task 10.5 - WCAG 2.1 AA):**
   - **Keyboard Navigation:**
     - Tab key moves between checkboxes sequentially
     - Space key toggles checkbox when focused
     - Enter key on "Clear Filters" button activates action
   - **Screen Reader:**
     - Checkbox labels announced correctly ("OpenAI", "Anthropic", etc.)
     - Checkbox states announced ("checked", "unchecked")
     - Filter count badge announced ("2 active filters")
     - Clear button state announced (enabled/disabled)
   - **Focus Management:**
     - Focus indicators visible on all interactive elements
     - Focus order follows logical reading order (top to bottom)
     - No focus traps in FilterSidebar

**Automated Testing (Deferred to Epic 3 Completion):**

**Priority Test Files for Epic 3 Test Suite:**
1. **`filterStore.test.ts`** (CRITICAL - 10 tests estimated)
   - Unit tests for toggleProvider, clearFilters, getActiveFilterCount
   - Store persistence and reactivity tests
   - Initial state verification
   - Singleton pattern validation

2. **`FilterSidebar.test.tsx`** (HIGH - 15 tests estimated)
   - Component rendering with/without providers
   - Filter count badge display logic
   - Clear button enable/disable state
   - Responsive behavior tests

3. **`ProviderFilter.test.tsx`** (MEDIUM - 10 tests estimated)
   - Checkbox list rendering
   - Checkbox state management (checked/unchecked)
   - Provider sorting (alphabetical)
   - Click handler integration with filterStore

4. **`ModelTable.filtering.test.tsx`** (HIGH - 12 tests estimated)
   - getFilteredRowModel integration
   - columnFilters state management
   - Filter + Sort composition (critical composability test)
   - Performance regression tests (<100ms assertion)

5. **`FilterSidebar.a11y.test.tsx`** (MEDIUM - 8 tests estimated)
   - vitest-axe automated WCAG 2.1 AA checks
   - Keyboard navigation flow tests
   - ARIA attributes validation

**Estimated Total:** 55 automated tests for Story 3.5 (following Story 3.3's test density pattern)

**Test Framework Stack:**
- Vitest + Testing Library (component tests)
- vitest-axe (accessibility automation)
- MSW (mock useModels hook for error scenarios)
- React DevTools Profiler API (performance assertions)

**No Automated Tests Required for Story 3.5** per Epic 1/2 retrospective feedback - manual testing sufficient for MVP delivery. Automated test suite will be implemented as Epic 3 completion task for comprehensive regression coverage.

## Dev Agent Record

### Context Reference

- `docs/stories/story-context-3.5.xml` (generated: 2025-10-24)

### Agent Model Used

Claude Sonnet 4.5 (claude-sonnet-4-5-20250929)

### Debug Log References

<!-- Debug logs will be added during implementation -->

### Completion Notes List

<!-- Completion notes will be added after implementation -->

### File List

<!-- File list will be added after implementation -->

## Change Log

**2025-10-24** - Story 3.5 Testing Strategy Enhanced by Master Test Architect Agent
- **CRITICAL ADDITION:** Added Task 7 (8 subtasks) for Zustand filter store behavior testing - validates foundational state management pattern for Stories 3.6-3.7 and 3.11
- **EDGE CASES:** Expanded Task 8 with 4 additional edge case subtasks (8.7-8.10) - all providers selected, no providers, single model per provider, zero models
- **ERROR HANDLING:** Added Task 9 (5 subtasks) for error handling and resilience testing - useModels errors, empty data, race conditions
- **ENHANCED TESTING STRATEGY:** Rewrote Testing Strategy section with 7 detailed test categories (previously 5)
- **AUTOMATED TEST PREP:** Added Epic 3 test suite preparation guidance - 55 estimated tests across 5 test files (filterStore.test.ts, FilterSidebar.test.tsx, etc.)
- **TOTAL SUBTASKS:** Increased from 49 to 64 subtasks (+15) across 10 tasks (previously 8)
- **RATIONALE:** Story 3.5 introduces first global Zustand store in Epic 3 - must validate correctness for pattern reuse in 4 future stories (3.6, 3.7, 3.11, plus Story 3.8 integration)
- Status: Ready (approved with enhanced testing coverage)

**2025-10-24** - Story 3.5 Drafted by Scrum Master Agent
- Created story draft based on Epic 3 requirements (epics.md:549-564)
- Extracted acceptance criteria from epics.md verbatim
- Defined 8 tasks with 49 subtasks covering FilterSidebar component, Zustand store, ProviderFilter component, and TanStack Table integration
- Added comprehensive dev notes with TanStack Table + Zustand architecture patterns, state management strategy, and technical references
- Established Zustand pattern for Stories 3.6-3.7 (capabilities, price range filters)
- Status: Draft (marked ready via story-ready workflow)
