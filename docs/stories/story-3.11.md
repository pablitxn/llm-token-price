# Story 3.11: Create Comparison Basket UI

Status: Draft

## Story

As a user,
I want to see selected models in a comparison basket,
So that I can review my selections and proceed to comparison.

## Acceptance Criteria

1. Comparison basket component displayed when models selected (top of page or floating bottom bar)
2. Basket shows mini-cards of selected models (name, provider)
3. "X" button on each card removes from selection
4. "Compare Selected" button (disabled if <2 models selected)
5. "Clear All" button removes all selections
6. Basket collapses when empty

## Tasks / Subtasks

- [ ] Task 1: Create ComparisonBasket Component (AC: 1, 6)
  - [ ] Create `ComparisonBasket.tsx` in `/frontend/src/components/comparison/`
  - [ ] Conditionally render when selectedModels.length > 0
  - [ ] Position as floating bottom bar (fixed bottom)
  - [ ] Style with TailwindCSS (shadow, border, padding)

- [ ] Task 2: Render Selected Model Cards (AC: 2, 3)
  - [ ] Map selectedModels to mini cards
  - [ ] Show model name and provider
  - [ ] Add remove button (X icon) for each card
  - [ ] Handle remove button click (call removeModel action)
  - [ ] Style cards with horizontal layout

- [ ] Task 3: Add Compare Button (AC: 4)
  - [ ] Add "Compare Selected" button
  - [ ] Disable when selectedModels.length < 2
  - [ ] Navigate to /compare page on click
  - [ ] Style as primary button (bg-blue-600)

- [ ] Task 4: Add Clear All Button (AC: 5)
  - [ ] Add "Clear All" button
  - [ ] Call clearSelection action on click
  - [ ] Style as secondary button

- [ ] Task 5: Integrate into HomePage (AC: 1)
  - [ ] Import ComparisonBasket into HomePage
  - [ ] Position above footer (fixed bottom positioning)
  - [ ] Test visibility when models selected

- [ ] Task 6: Testing
  - [ ] Test basket appears when model selected
  - [ ] Test basket shows all selected models
  - [ ] Test remove button removes model
  - [ ] Test Clear All button clears all
  - [ ] Test Compare button disabled when < 2 selected
  - [ ] Test basket collapses when empty
  - [ ] Test responsive behavior on mobile

## Dev Notes

### Prerequisites
- Story 3.10 (Checkbox Selection) complete

### References
- [Source: docs/epics.md#Story 3.11]

## Dev Agent Record

### Context Reference

### Agent Model Used

### Debug Log References

### Completion Notes List

### File List
