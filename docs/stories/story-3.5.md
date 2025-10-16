# Story 3.5: Add Provider Filter

Status: Draft

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

- [ ] Task 1: Create FilterSidebar Component (AC: 1)
  - [ ] Create `FilterSidebar.tsx` in `/frontend/src/components/filters/`
  - [ ] Add sidebar container with fixed width (w-64)
  - [ ] Style with TailwindCSS (border-right, padding)
  - [ ] Integrate into HomePage layout (grid with sidebar + main)

- [ ] Task 2: Create Zustand Filter Store (AC: 3, 6)
  - [ ] Create `filterStore.ts` in `/frontend/src/store/`
  - [ ] Define filterState interface (selectedProviders: string[])
  - [ ] Add actions: setProviders, clearFilters
  - [ ] Add computed property: activeFilterCount

- [ ] Task 3: Create ProviderFilter Component (AC: 2, 4)
  - [ ] Create `ProviderFilter.tsx` in `/frontend/src/components/filters/`
  - [ ] Extract unique providers from models data
  - [ ] Render checkbox list with provider names
  - [ ] Handle checkbox onChange to update store
  - [ ] Show provider count (e.g., "OpenAI (5)")

- [ ] Task 4: Implement Table Filtering Logic (AC: 3)
  - [ ] Add getFilteredRowModel to TanStack Table
  - [ ] Implement filterFn for provider column
  - [ ] Connect filter state from Zustand store
  - [ ] Verify table updates in real-time on filter change

- [ ] Task 5: Add Clear Filters Button (AC: 5)
  - [ ] Add button in FilterSidebar header
  - [ ] Call clearFilters action on click
  - [ ] Show button count: "Clear Filters (2)"
  - [ ] Disable button when no filters active

- [ ] Task 6: Testing
  - [ ] Test checking single provider filters table
  - [ ] Test multiple providers (OR logic works)
  - [ ] Test clear filters button resets all
  - [ ] Test active filter count updates
  - [ ] Test filter state persists during sort

## Dev Notes

### Prerequisites
- Story 3.4 (Column Sorting) complete

### References
- [Source: docs/epics.md#Story 3.5] - Provider filter requirements
- [Source: docs/solution-architecture.md#2.3] - Zustand for filter state

## Dev Agent Record

### Context Reference

### Agent Model Used

### Debug Log References

### Completion Notes List

### File List
