# Story 3.10: Add Checkbox Selection for Models

Status: Draft

## Story

As a user,
I want to select multiple models with checkboxes,
So that I can compare them side-by-side later.

## Acceptance Criteria

1. Checkbox column added as first column in table
2. Clicking checkbox selects/deselects model
3. Selected models highlighted with background color
4. Selected count shown above table ("3 models selected")
5. "Select All" checkbox in header selects/deselects all visible models
6. Selection state persists during filtering/sorting
7. Maximum 5 models selectable (6th click shows warning)

## Tasks / Subtasks

- [ ] Task 1: Create Comparison Store (AC: 2, 3, 6, 7)
  - [ ] Create `comparisonStore.ts` in `/frontend/src/store/`
  - [ ] Add `selectedModels` state (Model[])
  - [ ] Add actions: addModel, removeModel, clearSelection
  - [ ] Add computed: selectedCount, isSelected(id)
  - [ ] Implement max 5 models validation

- [ ] Task 2: Add Checkbox Column to Table (AC: 1, 2)
  - [ ] Add checkbox column as first column
  - [ ] Render checkbox in header and each row
  - [ ] Handle checkbox onChange
  - [ ] Connect to comparison store

- [ ] Task 3: Implement Row Selection UI (AC: 3)
  - [ ] Add conditional bg-blue-50 to selected rows
  - [ ] Add border or other visual indicator
  - [ ] Ensure hover state still visible

- [ ] Task 4: Add Selection Count Display (AC: 4)
  - [ ] Show count above table: "{selectedCount} models selected"
  - [ ] Hide when selectedCount === 0
  - [ ] Style with TailwindCSS

- [ ] Task 5: Implement Select All Checkbox (AC: 5, 6)
  - [ ] Add checkbox in header row
  - [ ] Clicking selects all visible (filtered) models
  - [ ] Handle indeterminate state (some selected)
  - [ ] Respect max 5 limit (show warning if > 5)

- [ ] Task 6: Implement Max Selection Validation (AC: 7)
  - [ ] Check selectedCount before adding model
  - [ ] Show toast/alert when limit reached
  - [ ] Disable unselected checkboxes when limit reached

- [ ] Task 7: Testing
  - [ ] Test selecting/deselecting individual models
  - [ ] Test selected row highlighting
  - [ ] Test selection count updates
  - [ ] Test select all checkbox
  - [ ] Test selection persists during filter/sort
  - [ ] Test max 5 models validation
  - [ ] Test warning message on 6th selection attempt

## Dev Notes

### Prerequisites
- Story 3.9 (Benchmark Scores) complete

### References
- [Source: docs/epics.md#Story 3.10]
- [Source: docs/solution-architecture.md#2.3] - Zustand for comparison basket state

## Dev Agent Record

### Context Reference

### Agent Model Used

### Debug Log References

### Completion Notes List

### File List
