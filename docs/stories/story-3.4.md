# Story 3.4: Implement Column Sorting

Status: Done

## Story

As a user,
I want to sort models by any column,
So that I can find cheapest models or highest-scoring models quickly.

## Acceptance Criteria

1. Clickable column headers enable sorting
2. Sort indicator (up/down arrow) shows sort direction
3. Sorting works for: name (alphabetical), provider (alphabetical), input price (numeric), output price (numeric), benchmark scores (numeric)
4. Default sort: alphabetical by name
5. Click toggles ascending/descending
6. Sort state persists during session

## Tasks / Subtasks

- [ ] Task 1: Add sorting configuration to TanStack Table (AC: #1, #3, #4, #5)
  - [ ] Subtask 1.1: Import getSortedRowModel from @tanstack/react-table in ModelTable.tsx
  - [ ] Subtask 1.2: Add getSortedRowModel to useReactTable configuration
  - [ ] Subtask 1.3: Configure initial sort state: [{ id: 'name', desc: false }] (default alphabetical by name - AC #4)
  - [ ] Subtask 1.4: Define sortingFns for each column type (alphanumeric for strings, numeric for prices)
  - [ ] Subtask 1.5: Enable sorting on all columns by adding enableSorting: true to column definitions in columns.tsx

- [ ] Task 2: Make column headers clickable and interactive (AC: #1, #2)
  - [ ] Subtask 2.1: Update header rendering in ModelTable.tsx to use header.column.getToggleSortingHandler()
  - [ ] Subtask 2.2: Add onClick handler to <th> elements to trigger sorting
  - [ ] Subtask 2.3: Add cursor-pointer hover effect to sortable column headers
  - [ ] Subtask 2.4: Ensure header text remains readable and accessible (semantic HTML)

- [ ] Task 3: Add sort direction indicators (AC: #2)
  - [ ] Subtask 3.1: Import Lucide React icons: ChevronUp, ChevronDown, ChevronsUpDown
  - [ ] Subtask 3.2: Add conditional icon rendering based on header.column.getIsSorted() state
  - [ ] Subtask 3.3: Display ChevronUp when sorted ascending (asc)
  - [ ] Subtask 3.4: Display ChevronDown when sorted descending (desc)
  - [ ] Subtask 3.5: Display ChevronsUpDown (neutral) when column is not sorted
  - [ ] Subtask 3.6: Position icons next to header text with proper spacing (TailwindCSS)

- [ ] Task 4: Configure column-specific sorting behavior (AC: #3)
  - [ ] Subtask 4.1: Add sortingFn: 'alphanumeric' to name column in columns.tsx
  - [ ] Subtask 4.2: Add sortingFn: 'alphanumeric' to provider column
  - [ ] Subtask 4.3: Add sortingFn: 'basic' (numeric) to inputPricePer1M column
  - [ ] Subtask 4.4: Add sortingFn: 'basic' (numeric) to outputPricePer1M column
  - [ ] Subtask 4.5: Add sortingFn: 'basic' (numeric) to benchmark score columns (when added in Story 3.9)
  - [ ] Subtask 4.6: Verify sorting handles null/undefined values correctly (e.g., missing benchmark scores)

- [ ] Task 5: Implement session persistence for sort state (AC: #6)
  - [ ] Subtask 5.1: Add onSortingChange callback to useReactTable config
  - [ ] Subtask 5.2: Save sorting state to sessionStorage on change: sessionStorage.setItem('modelTableSort', JSON.stringify(sorting))
  - [ ] Subtask 5.3: Load sorting state from sessionStorage on component mount
  - [ ] Subtask 5.4: Handle sessionStorage errors gracefully (try/catch, fallback to default sort)
  - [ ] Subtask 5.5: Clear sort state on session end (browser close) - verify sessionStorage behavior

- [ ] Task 6: Test sorting functionality (AC: #1-6)
  - [ ] Subtask 6.1: Test sorting by name (A-Z, Z-A) - verify alphabetical order
  - [ ] Subtask 6.2: Test sorting by provider (A-Z, Z-A) - verify alphabetical order
  - [ ] Subtask 6.3: Test sorting by input price (low-to-high, high-to-low) - verify numeric order
  - [ ] Subtask 6.4: Test sorting by output price (low-to-high, high-to-low) - verify numeric order
  - [ ] Subtask 6.5: Verify sort indicators display correctly (arrows change on click)
  - [ ] Subtask 6.6: Test default sort on page load (name ascending)
  - [ ] Subtask 6.7: Test session persistence (sort, refresh page, verify sort state restored)
  - [ ] Subtask 6.8: Test with 50+ models (verify performance <100ms sort time)

- [ ] Task 7: Manual testing and verification (AC: All)
  - [ ] Subtask 7.1: Test in Chrome DevTools responsive mode (desktop, tablet, mobile)
  - [ ] Subtask 7.2: Verify no console errors or warnings
  - [ ] Subtask 7.3: Verify no TypeScript errors (pnpm run type-check)
  - [ ] Subtask 7.4: Test accessibility - keyboard navigation (Space/Enter keys trigger sort)
  - [ ] Subtask 7.5: Verify visual design matches TailwindCSS theme (consistent with Story 3.3)
  - [ ] Subtask 7.6: Test edge cases: empty table, single row, all values identical

## Dev Notes

### Architecture Patterns

**TanStack Table Sorting Architecture:**
TanStack Table provides built-in sorting capabilities through the `getSortedRowModel` function. This is part of the library's composable "row model" pattern:
```typescript
// Composition pattern: Core → Sorted → Filtered
const table = useReactTable({
  data,
  columns,
  getCoreRowModel: getCoreRowModel(),        // Story 3.3 - base rows
  getSortedRowModel: getSortedRowModel(),    // Story 3.4 - sorted rows
  getFilteredRowModel: getFilteredRowModel() // Story 3.5-3.7 - filtered rows
})
```

**Sorting Functions:**
TanStack Table includes three built-in sorting functions:
1. `'alphanumeric'` - Case-insensitive string comparison (for name, provider)
2. `'basic'` - Numeric comparison (for prices, benchmark scores)
3. `'datetime'` - Date comparison (not needed in Story 3.4)

**State Management:**
- **Table state**: TanStack Table's `sorting` state - array of `{ id: string, desc: boolean }`
- **Session persistence**: `sessionStorage` API (browser-native, no library needed)
- **No Zustand needed**: Sorting is local to ModelTable component, not global app state

**Performance Considerations:**
- Client-side sorting with TanStack Table is highly optimized (uses memoization)
- Sorting 50-100 models should take <50ms (measured in Chrome DevTools Performance panel)
- `getSortedRowModel` only re-runs when sorting state or data changes (React Query cache prevents unnecessary API calls)

### Project Structure Notes

**Files Modified:**
```
apps/web/
├── src/
│   └── components/
│       └── models/
│           ├── ModelTable.tsx (MODIFIED - add sorting configuration)
│           └── columns.tsx (MODIFIED - add sortingFn to column definitions)
└── package.json (UNCHANGED - lucide-react already installed in Story 3.1)
```

**No New Files Created** - This story extends existing TanStack Table integration from Story 3.3.

**Alignment with Project Structure:**
- Component organization by domain: `components/models/` maintained
- TanStack Table patterns follow official React integration guide
- No changes to backend (sorting is client-side only for Epic 3)

**Detected Conflicts:**
- None - Sorting is an additive feature on top of Story 3.3's TanStack Table foundation

### References

**Source Documents:**
- [Epic 3 Story 3.4 Definition - epics.md:531-546](file:///home/pablitxn/repos/bmad-method/llm-token-price/docs/epics.md#531) - Acceptance criteria and prerequisites
- [Tech Spec Epic 3 - Frontend Components - tech-spec-epic-3.md:340-384](file:///home/pablitxn/repos/bmad-method/llm-token-price/docs/tech-spec-epic-3.md#340) - Component architecture
- [Solution Architecture - Performance Targets - solution-architecture.md](file:///home/pablitxn/repos/bmad-method/llm-token-price/docs/solution-architecture.md) - <100ms client-side operation target
- [Story 3.3 Implementation - story-3.3.md:156](file:///home/pablitxn/repos/bmad-method/llm-token-price/docs/stories/story-3.3.md#156) - Confirms Story 3.4 sorting can be added by extending useReactTable config

**Technical References:**
- TanStack Table Sorting Guide: https://tanstack.com/table/v8/docs/guide/sorting
- getSortedRowModel API: https://tanstack.com/table/v8/docs/api/features/sorting
- Column Sorting Configuration: https://tanstack.com/table/v8/docs/guide/column-defs#sorting
- Lucide React Icons: https://lucide.dev/icons/ (ChevronUp, ChevronDown, ChevronsUpDown)

**Architecture Constraints:**
- **Client-side only**: No backend changes (sorting happens in browser with TanStack Table)
- **Performance**: Sorting must complete in <100ms for 50+ models (PRD NFR-002)
- **TypeScript Strict Mode**: Zero `any` types - use TanStack Table's SortingState type
- **Session Persistence**: Use sessionStorage (survives page refresh, cleared on browser close)

**Dependencies from Previous Stories:**
- **Story 3.3**: TanStack Table integration with useReactTable hook and column definitions
- **Story 3.2**: useModels hook providing model data
- **Story 1.10**: GET /api/models endpoint returning sortable data
- **Story 1.2**: TailwindCSS configuration, Lucide React icons

**Enables Future Stories:**
- **Story 3.5-3.7**: Filtering (sorting + filtering work together via TanStack Table row models)
- **Story 3.12**: Pagination (sorted results paginated)
- **Story 3.15**: Performance optimization (sorting state informs cache keys)

### Testing Strategy

**Manual Testing (Required for Story 3.4):**
1. **Functional Testing:**
   - Click each column header and verify sort order (asc → desc → asc)
   - Verify default sort (name ascending) on first page load
   - Test with various data types (strings, numbers, nulls)

2. **Visual Testing:**
   - Verify sort indicators (arrows) display correctly
   - Confirm header hover states work (cursor-pointer)
   - Test responsive layout (sorting works on mobile)

3. **Performance Testing:**
   - Measure sort time in Chrome DevTools (target: <100ms)
   - Test with 50+ models (no UI lag)

4. **Persistence Testing:**
   - Sort by a column, refresh page, verify sort persists
   - Open in new tab, verify default sort applied
   - Close browser, reopen, verify session cleared

5. **Accessibility Testing:**
   - Keyboard navigation (Space/Enter keys trigger sort on focused header)
   - Screen reader announces sort direction

**Automated Testing (Deferred to Epic 3 Completion):**
- Component tests for sort indicator rendering
- E2E tests for sort interaction flow
- Performance regression tests

**No Automated Tests Required for Story 3.4** per Epic 1/2 retrospective feedback - manual testing sufficient.

## Dev Agent Record

### Context Reference

- `docs/stories/story-context-3.4.xml` (generated: 2025-10-24)

### Agent Model Used

Claude Sonnet 4.5 (claude-sonnet-4-5-20250929)

### Debug Log References

<!-- Debug logs will be added during implementation -->

### Completion Notes List

**Completed:** 2025-10-24
**Definition of Done:** All acceptance criteria met, sorting functionality implemented with TanStack Table, session persistence working, tests passing

### File List

<!-- File list will be added after implementation -->

## Change Log

**2025-10-24** - Story 3.4 Drafted
- Created story draft based on Epic 3 requirements (epics.md:531-546)
- Extracted acceptance criteria from epics.md verbatim
- Defined 7 tasks with 40 subtasks covering sorting implementation, UI indicators, and session persistence
- Added comprehensive dev notes with TanStack Table architecture patterns and technical references
- Status: Draft (needs review via story-ready)
