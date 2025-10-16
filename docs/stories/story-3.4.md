# Story 3.4: Implement Column Sorting

Status: Draft

## Story

As a user,
I want to sort models by any column,
So that I can find cheapest models or highest-scoring models.

## Acceptance Criteria

1. Clickable column headers enable sorting
2. Sort indicator (up/down arrow) shows sort direction
3. Sorting works for: name (alphabetical), provider (alphabetical), input price (numeric), output price (numeric), benchmark scores (numeric)
4. Default sort: alphabetical by name
5. Click toggles ascending/descending
6. Sort state persists during session

## Tasks / Subtasks

- [ ] Task 1: Enable Sorting in TanStack Table (AC: 1, 3, 4, 5)
  - [ ] Add getSortedRowModel to table configuration
  - [ ] Set initialState with default sort (name ascending)
  - [ ] Configure column sorting (enableSorting: true)
  - [ ] Add sortingFn for numeric columns (price columns)

- [ ] Task 2: Add Sort Indicators to Headers (AC: 2)
  - [ ] Import Lucide icons (ChevronUp, ChevronDown, ChevronsUpDown)
  - [ ] Add onClick handler to column headers
  - [ ] Render sort icon based on column.getIsSorted()
  - [ ] Style icons with TailwindCSS (gray when not sorted, blue when sorted)

- [ ] Task 3: Implement Session Persistence (AC: 6)
  - [ ] Create Zustand store slice for table state
  - [ ] Save sorting state to store on change
  - [ ] Load sorting state from store on mount
  - [ ] Test: sort, refresh page, verify sort persists

- [ ] Task 4: Testing and Verification
  - [ ] Test name column sorting (alphabetical A-Z, Z-A)
  - [ ] Test provider column sorting
  - [ ] Test input price sorting (lowest to highest, highest to lowest)
  - [ ] Test output price sorting
  - [ ] Test sort indicator changes correctly
  - [ ] Test default sort on first load (name ascending)

## Dev Notes

### Prerequisites
- Story 3.3 (TanStack Table integration) complete

### References
- [Source: docs/epics.md#Story 3.4] - Sorting requirements
- [Source: docs/solution-architecture.md#2.3] - Zustand for client state (sort state)

## Dev Agent Record

### Context Reference

### Agent Model Used

### Debug Log References

### Completion Notes List

### File List
