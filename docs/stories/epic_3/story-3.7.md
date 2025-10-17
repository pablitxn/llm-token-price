# Story 3.7: Add Price Range Filter

Status: Ready

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

- [ ] Task 1: Update Filter Store (AC: 1, 3)
  - [ ] Add `priceRange` to filter state: { min: number, max: number }
  - [ ] Add `setPriceRange` action
  - [ ] Calculate default min/max from models data

- [ ] Task 2: Create PriceRangeFilter Component (AC: 2, 3, 6)
  - [ ] Create `PriceRangeFilter.tsx`
  - [ ] Implement dual-range slider (use HTML input range or library)
  - [ ] Show min/max price inputs (number or readonly)
  - [ ] Format prices with currency
  - [ ] Set slider default to data min/max

- [ ] Task 3: Implement Table Filtering Logic (AC: 4, 5)
  - [ ] Add price range filter to table filterFn
  - [ ] Calculate avg price: (inputPrice + outputPrice) / 2
  - [ ] Filter models within selected range
  - [ ] Update table in real-time (debounce slider if needed)

- [ ] Task 4: Testing
  - [ ] Test slider adjusts min/max
  - [ ] Test table filters by price range
  - [ ] Test combined with provider and capability filters
  - [ ] Test clear filters resets price range
  - [ ] Test edge cases (all models filtered out)

## Dev Notes

### Prerequisites
- Story 3.6 (Capabilities Filters) complete

### References
- [Source: docs/epics.md#Story 3.7]

## Dev Agent Record

### Context Reference

### Agent Model Used

### Debug Log References

### Completion Notes List

### File List
