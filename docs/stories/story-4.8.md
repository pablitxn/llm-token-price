# Story 4.8: Preset Workload Scenarios

Status: Ready

## Story

As a user,
I want preset scenarios for common workloads,
so that I can quickly see costs without manual input.

## Acceptance Criteria

1. Preset buttons above calculator: "Small Project" (1M tokens, 60/40), "Medium Project" (5M tokens, 50/50), "Large Project" (20M tokens, 70/30), "Custom"
2. Clicking preset sets calculator inputs automatically
3. Custom mode (default) allows manual input
4. Selected preset highlighted
5. Tooltip explains what each preset represents

## Tasks / Subtasks

- [ ] **Task 1: Create workload presets configuration** (AC: #1, #5)
- [ ] **Task 2: Create WorkloadPresets component** (AC: #1, #2, #4)
- [ ] **Task 3: Integrate with calculatorStore** (AC: #2, #3)
- [ ] **Task 4: Add preset button styling** (AC: #4)
- [ ] **Task 5: Add tooltips** (AC: #5)
- [ ] **Task 6: Testing**

## Dev Notes

### workloadPresets configuration:
```typescript
export const WORKLOAD_PRESETS = [
  { id: 'small', name: 'Small Project', description: 'Personal projects, prototypes (1M tokens/month)', volume: 1_000_000, ratio: 60 },
  { id: 'medium', name: 'Medium Project', description: 'Team applications, MVPs (5M tokens/month)', volume: 5_000_000, ratio: 50 },
  { id: 'large', name: 'Large Project', description: 'Production apps, high traffic (20M tokens/month)', volume: 20_000_000, ratio: 70 },
  { id: 'custom', name: 'Custom', description: 'Define your own parameters', volume: 1_000_000, ratio: 50 },
];
```

## References

- [Epic 4 Analysis: docs/epic-4-analysis-and-plan.md#Story 4.8]

## Dev Agent Record

### Agent Model Used

claude-sonnet-4-5-20250929
