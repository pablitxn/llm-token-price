# Story 4.9: Cost Visualization with Chart.js

Status: Ready

## Story

As a user,
I want visual cost comparison,
so that I can quickly see cost differences.

## Acceptance Criteria

1. Bar chart added below calculator results table
2. Chart shows top 10 models by cost (or all if <10)
3. X-axis: model names, Y-axis: monthly cost
4. Chart updates in real-time with calculator inputs
5. Clicking bar in chart highlights corresponding row in table
6. Chart uses Chart.js library

## Tasks / Subtasks

- [ ] **Task 1: Create CostChart component** (AC: #1, #2, #3)
- [ ] **Task 2: Configure Chart.js** (AC: #6)
- [ ] **Task 3: Implement real-time updates** (AC: #4)
- [ ] **Task 4: Add click interaction** (AC: #5)
- [ ] **Task 5: Style and polish chart**
- [ ] **Task 6: Testing**

## Dev Notes

### CostChart implementation:
```typescript
import { Bar } from 'react-chartjs-2';
import { Chart as ChartJS, CategoryScale, LinearScale, BarElement, Title, Tooltip, Legend } from 'chart.js';

ChartJS.register(CategoryScale, LinearScale, BarElement, Title, Tooltip, Legend);

const data = {
  labels: sortedModels.slice(0, 10).map(m => m.name),
  datasets: [{
    label: 'Monthly Cost',
    data: sortedModels.slice(0, 10).map(m => m.calculatedCost.totalMonthlyCost),
    backgroundColor: 'rgba(59, 130, 246, 0.5)',
  }],
};
```

## References

- [Epic 4 Analysis: docs/epic-4-analysis-and-plan.md#Story 4.9]
- [ADR-019: Chart.js for Cost Visualization]

## Dev Agent Record

### Agent Model Used

claude-sonnet-4-5-20250929
