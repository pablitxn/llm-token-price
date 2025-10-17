# Story 4.7: Cost Comparison Table in Calculator

Status: Ready

## Story

As a user,
I want to see costs for all models in calculator,
so that I can compare which is cheapest for my workload.

## Acceptance Criteria

1. Calculator page displays results table below inputs
2. Table shows all models with columns: name, provider, monthly cost, cost breakdown
3. Table sorted by cost ascending (cheapest first)
4. Cost savings column shows % saved vs most expensive
5. Table updates in real-time as calculator inputs change
6. Highlight cheapest model with badge or color

## Tasks / Subtasks

- [ ] **Task 1: Create CalculatorResults component** (AC: #1, #2, #3)
- [ ] **Task 2: Fetch all models and calculate costs** (AC: #2, #5)
- [ ] **Task 3: Implement sorting by cost** (AC: #3)
- [ ] **Task 4: Calculate savings percentage** (AC: #4)
- [ ] **Task 5: Highlight cheapest model** (AC: #6)
- [ ] **Task 6: Add memoization for performance** (AC: #5)
- [ ] **Task 7: Integrate in CalculatorPage** (AC: #1)
- [ ] **Task 8: Testing**

## Dev Notes

### Implementation

**CalculatorResults component:**
```typescript
const { monthlyTokenVolume, inputOutputRatio } = useCalculatorStore();
const { data: models = [] } = useModels();

const modelsWithCost = useMemo(
  () => calculateCostForAllModels(models, monthlyTokenVolume, inputOutputRatio),
  [models, monthlyTokenVolume, inputOutputRatio]
);

const sortedModels = modelsWithCost.sort(
  (a, b) => a.calculatedCost.totalMonthlyCost - b.calculatedCost.totalMonthlyCost
);
```

## References

- [Epic 4 Analysis: docs/epic-4-analysis-and-plan.md#Story 4.7]
- [Story 4.6: Cost Calculator Component]

## Dev Agent Record

### Context Reference

### Agent Model Used

claude-sonnet-4-5-20250929
