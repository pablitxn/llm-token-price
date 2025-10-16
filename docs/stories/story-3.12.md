# Story 3.12: Implement Table Pagination or Virtual Scrolling

Status: Draft

## Story

As a user,
I want to browse large model lists efficiently,
So that performance remains fast with 50+ models.

## Acceptance Criteria

1. Choose pagination (10/25/50 per page) OR virtual scrolling (renders visible rows only)
2. If pagination: page controls (prev/next, page numbers) displayed below table
3. If virtual scrolling: smooth scroll performance with 100+ rows
4. Current approach works with filtering (shows correct subset)
5. User preference saved in session storage

## Tasks / Subtasks

- [ ] Task 1: Evaluate Pagination vs Virtual Scrolling (AC: 1)
  - [ ] Test current performance with 50+ models
  - [ ] If fast (< 500ms render): use pagination (simpler)
  - [ ] If slow: implement virtual scrolling (TanStack Table + @tanstack/react-virtual)
  - [ ] Document decision

- [ ] Task 2: Implement Pagination (AC: 2, 4, 5)
  - [ ] Add getPaginationRowModel to table
  - [ ] Set initial page size (default 25)
  - [ ] Create pagination controls component
  - [ ] Add prev/next buttons
  - [ ] Add page size selector (10, 25, 50)
  - [ ] Save page size to sessionStorage

- [ ] Task 3: Test with Filtering (AC: 4)
  - [ ] Test pagination resets to page 1 when filter changes
  - [ ] Test page controls update correctly
  - [ ] Test filter shows correct total pages

- [ ] Task 4: Testing
  - [ ] Test pagination with 50+ models
  - [ ] Test page size selector
  - [ ] Test prev/next buttons
  - [ ] Test pagination persists (sessionStorage)
  - [ ] Test performance with max dataset

## Dev Notes

### Prerequisites
- Story 3.11 (Comparison Basket) complete

### References
- [Source: docs/epics.md#Story 3.12]
- [Source: docs/solution-architecture.md#1.1] - TanStack Table virtualization support

## Dev Agent Record

### Context Reference

### Agent Model Used

### Debug Log References

### Completion Notes List

### File List
