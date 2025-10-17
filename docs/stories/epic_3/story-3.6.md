# Story 3.6: Add Capabilities Filters

Status: Ready

## Story

As a user,
I want to filter models by capabilities,
So that I can find models supporting specific features (function calling, vision, etc.).

## Acceptance Criteria

1. Capabilities filter section added to sidebar
2. Checkboxes for each capability: function calling, vision support, audio support, streaming, JSON mode
3. Checking capability filters to only models with that capability
4. Multiple capabilities use AND logic (must have all selected)
5. Filters update table immediately
6. Tooltip explains what each capability means

## Tasks / Subtasks

- [ ] Task 1: Update Filter Store (AC: 1, 2, 3, 4)
  - [ ] Add `selectedCapabilities` to filter state
  - [ ] Add `setCapabilities` action
  - [ ] Implement AND logic for capability filtering

- [ ] Task 2: Create CapabilityFilter Component (AC: 1, 2, 6)
  - [ ] Create `CapabilityFilter.tsx`
  - [ ] List capabilities: function_calling, vision, audio, streaming, json_mode
  - [ ] Render checkbox for each capability
  - [ ] Add tooltip component for each capability explanation
  - [ ] Handle checkbox change to update store

- [ ] Task 3: Implement Table Filtering Logic (AC: 3, 4, 5)
  - [ ] Update table filterFn to include capability filtering
  - [ ] Implement AND logic (model must have ALL selected capabilities)
  - [ ] Combine with provider filter (AND between filter types)
  - [ ] Test immediate table update

- [ ] Task 4: Add Tooltips (AC: 6)
  - [ ] Create Tooltip component if not exists
  - [ ] Add tooltip for each capability with description
  - [ ] Style tooltip with TailwindCSS

- [ ] Task 5: Testing
  - [ ] Test single capability filter
  - [ ] Test multiple capabilities (AND logic)
  - [ ] Test combined provider + capability filters
  - [ ] Test tooltip shows on hover
  - [ ] Test clear filters resets capabilities

## Dev Notes

### Prerequisites
- Story 3.5 (Provider Filter) complete

### References
- [Source: docs/epics.md#Story 3.6]

## Dev Agent Record

### Context Reference

### Agent Model Used

### Debug Log References

### Completion Notes List

### File List
