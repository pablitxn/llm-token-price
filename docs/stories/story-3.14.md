# Story 3.14: Add Context Window and Capabilities Icons

Status: Ready

## Story

As a user,
I want to see capabilities as icons/badges,
So that I can quickly identify supported features.

## Acceptance Criteria

1. Capabilities displayed as icon badges (function icon, eye icon for vision, etc.)
2. Context window displayed with formatting (e.g., "128K tokens")
3. Tooltip on hover explains capability
4. Icons use consistent design system
5. Icons visible in both table and card views

## Tasks / Subtasks

- [ ] Task 1: Create Capability Badge Component (AC: 1, 4)
  - [ ] Create `CapabilityBadge.tsx` component
  - [ ] Import Lucide icons for each capability
  - [ ] Map capability types to icons
  - [ ] Style badges with TailwindCSS (small, rounded, colored)

- [ ] Task 2: Add Capabilities Column to Table (AC: 1, 3)
  - [ ] Add capabilities column to table
  - [ ] Render CapabilityBadge for each capability
  - [ ] Add Tooltip for each badge
  - [ ] Limit visible badges (show top 3 + "more" indicator)

- [ ] Task 3: Add Context Window Column (AC: 2)
  - [ ] Add context window column
  - [ ] Format number with K suffix (128000 → "128K")
  - [ ] Add "tokens" label
  - [ ] Style with monospace font

- [ ] Task 4: Add Tooltips (AC: 3)
  - [ ] Use Tooltip component for each capability icon
  - [ ] Show capability name and description on hover
  - [ ] Test tooltip positioning

- [ ] Task 5: Testing
  - [ ] Test capability icons display correctly
  - [ ] Test context window formatting
  - [ ] Test tooltips show on hover
  - [ ] Test icons visible in mobile/card view
  - [ ] Test color/size consistency

## Dev Notes

### Prerequisites
- Story 3.13 (Styling) complete

### References
- [Source: docs/epics.md#Story 3.14]

## Dev Agent Record

### Context Reference

### Agent Model Used

### Debug Log References

### Completion Notes List

### File List
