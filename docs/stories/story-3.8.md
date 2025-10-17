# Story 3.8: Implement Search Functionality

Status: Ready

## Story

As a user,
I want to search for models by name or provider,
So that I can quickly find specific models.

## Acceptance Criteria

1. Search input box in header or above table
2. Typing filters table to matching models (name or provider contains search term)
3. Search is case-insensitive
4. Search updates table in real-time (debounced 300ms)
5. Clear search button (X icon) resets search
6. Search works alongside filters (combined filter logic)

## Tasks / Subtasks

- [ ] Task 1: Update Filter Store (AC: 1, 2)
  - [ ] Add `searchQuery` to filter state
  - [ ] Add `setSearchQuery` action

- [ ] Task 2: Update Header Search Input (AC: 1, 5)
  - [ ] Enable search input in Header.tsx (remove disabled attribute)
  - [ ] Add onChange handler to update searchQuery
  - [ ] Add clear button (X icon) when search has value
  - [ ] Debounce search input (300ms)

- [ ] Task 3: Implement Table Filtering Logic (AC: 2, 3, 6)
  - [ ] Add search filter to table filterFn
  - [ ] Check if model.name or model.provider contains search term
  - [ ] Make search case-insensitive (toLowerCase)
  - [ ] Combine with existing filters (AND logic)

- [ ] Task 4: Implement Search in Real-Time (AC: 4)
  - [ ] Use debounce utility (300ms)
  - [ ] Update table immediately after debounce
  - [ ] Show loading indicator if needed

- [ ] Task 5: Testing
  - [ ] Test search by model name
  - [ ] Test search by provider
  - [ ] Test case-insensitive search
  - [ ] Test debounce (rapid typing doesn't cause multiple updates)
  - [ ] Test clear button resets search
  - [ ] Test search combined with filters

## Dev Notes

### Prerequisites
- Story 3.7 (Price Range Filter) complete

### References
- [Source: docs/epics.md#Story 3.8]

## Dev Agent Record

### Context Reference

### Agent Model Used

### Debug Log References

### Completion Notes List

### File List
