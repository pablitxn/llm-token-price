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

- [ ] Task 7: Test filtering functionality (AC: #1-6)
  - [ ] Subtask 7.1: Test selecting single provider (verify table filters correctly)
  - [ ] Subtask 7.2: Test selecting multiple providers (verify OR logic - shows all selected)
  - [ ] Subtask 7.3: Test unselecting provider (verify table updates immediately)
  - [ ] Subtask 7.4: Test "Clear Filters" button (verify all selections reset)
  - [ ] Subtask 7.5: Verify filter count badge updates correctly
  - [ ] Subtask 7.6: Test with sorting enabled (Story 3.4 - verify filters + sorting work together)
  - [ ] Subtask 7.7: Test edge cases (all providers selected, no providers selected, single model per provider)
  - [ ] Subtask 7.8: Verify performance <100ms for filter operations with 50+ models

- [ ] Task 8: Manual testing and verification (AC: All)
  - [ ] Subtask 8.1: Test in Chrome DevTools responsive mode (desktop, tablet, mobile)
  - [ ] Subtask 8.2: Verify sidebar collapses/expands on mobile (if implemented)
  - [ ] Subtask 8.3: Verify no console errors or warnings
  - [ ] Subtask 8.4: Verify no TypeScript errors (pnpm run type-check)
  - [ ] Subtask 8.5: Test accessibility - keyboard navigation and screen reader support
  - [ ] Subtask 8.6: Verify visual design matches TailwindCSS theme (consistent with Story 3.3-3.4)

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
1. **Functional Testing:**
   - Select single provider checkbox → verify table shows only that provider's models
   - Select multiple providers → verify table shows models from ALL selected providers (OR logic)
   - Uncheck provider → verify table updates immediately
   - Click "Clear Filters" → verify all checkboxes unchecked and full table restored

2. **Visual Testing:**
   - Verify FilterSidebar displays on left side of table
   - Confirm checkboxes have visible checked/unchecked states
   - Verify filter count badge displays correctly
   - Test responsive layout (sidebar behavior on mobile/tablet/desktop)

3. **Integration Testing:**
   - Apply filter, then sort (Story 3.4) → verify filtered subset sorts correctly
   - Sort, then apply filter → verify sorted order maintained after filtering
   - Test with Story 3.2's useModels hook → verify loading states

4. **Performance Testing:**
   - Measure filter time in Chrome DevTools (target: <100ms)
   - Test with 50+ models (no UI lag)
   - Verify no unnecessary re-renders (React DevTools Profiler)

5. **Accessibility Testing:**
   - Keyboard navigation (Tab key moves between checkboxes, Space toggles)
   - Screen reader announces checkbox labels and states
   - Focus indicators visible on all interactive elements

**Automated Testing (Deferred to Epic 3 Completion):**
- Component tests for FilterSidebar and ProviderFilter rendering
- Zustand store unit tests (toggleProvider, clearFilters logic)
- E2E tests for filter interaction flow

**No Automated Tests Required for Story 3.5** per Epic 1/2 retrospective feedback - manual testing sufficient.

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

**2025-10-24** - Story 3.5 Drafted by Scrum Master Agent
- Created story draft based on Epic 3 requirements (epics.md:549-564)
- Extracted acceptance criteria from epics.md verbatim
- Defined 8 tasks with 49 subtasks covering FilterSidebar component, Zustand store, ProviderFilter component, and TanStack Table integration
- Added comprehensive dev notes with TanStack Table + Zustand architecture patterns, state management strategy, and technical references
- Established Zustand pattern for Stories 3.6-3.7 (capabilities, price range filters)
- Status: Draft (needs review via story-ready workflow)
