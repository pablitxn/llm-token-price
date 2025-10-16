# Story 3.3: Integrate TanStack Table for Advanced Features

Status: Draft

## Story

As a developer,
I want TanStack Table integrated,
So that I can implement sorting, filtering, and virtualization efficiently.

## Acceptance Criteria

1. TanStack Table library installed and configured
2. Models data rendered using TanStack Table component
3. Column definitions created for all model fields
4. Table replaces basic HTML table from Story 3.2
5. Performance verified with 50+ models (virtual scrolling if needed)

## Tasks / Subtasks

- [ ] Task 1: Install TanStack Table (AC: 1)
  - [ ] Run `pnpm add @tanstack/react-table`
  - [ ] Verify version 8.11.0+ installed (from solution-architecture.md)
  - [ ] Import in ModelTable.tsx to verify no errors

- [ ] Task 2: Define Column Configuration (AC: 3)
  - [ ] Create column definitions array with accessor functions
  - [ ] Define columns: name, provider, inputPricePer1M, outputPricePer1M
  - [ ] Add header labels for each column
  - [ ] Add cell formatting (price formatter)
  - [ ] Configure column sizing and min-width

- [ ] Task 3: Implement TanStack Table Hook (AC: 2)
  - [ ] Use `useReactTable` hook with models data
  - [ ] Configure table options (columns, data, getCoreRowModel)
  - [ ] Get table instance methods (getHeaderGroups, getRowModel)
  - [ ] Prepare for sorting (getSortedRowModel) - configure but don't enable yet

- [ ] Task 4: Replace HTML Table with TanStack Table Rendering (AC: 4)
  - [ ] Update ModelTable component to use table.getHeaderGroups()
  - [ ] Render headers using flexRender
  - [ ] Render rows using table.getRowModel().rows
  - [ ] Render cells using flexRender
  - [ ] Apply TailwindCSS styling to match previous table design

- [ ] Task 5: Test Performance with Large Dataset (AC: 5)
  - [ ] Create test dataset with 50+ models (duplicate seed data)
  - [ ] Measure render time in browser DevTools (should be < 500ms)
  - [ ] Test scrolling performance (should be smooth)
  - [ ] Verify no console warnings or errors
  - [ ] Document if virtual scrolling needed (defer to Story 3.12 if performance issue)

- [ ] Task 6: Verification and Cleanup
  - [ ] Verify table displays same data as Story 3.2
  - [ ] Test loading state still works
  - [ ] Test error state still works
  - [ ] Remove old HTML table code
  - [ ] Update tests if any exist

## Dev Notes

### Prerequisites
- Story 3.2 (Basic HTML table) must be complete
- TanStack Table v8.11.0 specified in solution-architecture.md

### References
- [Source: docs/solution-architecture.md#1.1] - TanStack Table v8.11.0, virtualization support
- [Source: docs/epics.md#Story 3.3] - Performance requirement: 50K+ rows support

## Dev Agent Record

### Context Reference

### Agent Model Used

### Debug Log References

### Completion Notes List

### File List
