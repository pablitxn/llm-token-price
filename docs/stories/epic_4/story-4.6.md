# Story 4.6: Cost Calculator Component (Embedded + Standalone)

Status: Ready

## Story

As a user,
I want to calculate estimated monthly cost for my workload,
so that I can budget and compare costs.

**Extended:** This story consolidates creation of calculator component for both embedded (in Pricing tab) and standalone (/calculator page) contexts.

## Acceptance Criteria

1. Cost calculator component created with inputs:
   - Monthly token volume (slider or number input, default 1M)
   - Input/output ratio (slider, default 50/50)
2. Calculator displays:
   - Total monthly cost for current model (embedded) or all models (standalone)
   - Cost breakdown (input tokens cost + output tokens cost)
3. Calculation updates in real-time as inputs change
4. Calculation formula: (volume * ratio * input_price) + (volume * (1-ratio) * output_price)
5. Cost displayed with currency symbol and formatting
6. Calculator works embedded in Pricing tab AND as standalone /calculator page
7. State persists in localStorage and syncs between contexts

## Tasks / Subtasks

- [ ] **Task 1: Create calculatorStore (Zustand)** (AC: #1, #7)
  - [ ] 1.1: Create `calculatorStore.ts` in `/apps/web/src/store`
  - [ ] 1.2: Define CalculatorState interface (monthlyTokenVolume, inputOutputRatio, activePreset)
  - [ ] 1.3: Create Zustand store with persist middleware (localStorage)
  - [ ] 1.4: Implement setVolume, setRatio, setPreset actions
  - [ ] 1.5: Default values: volume=1M, ratio=50, preset='custom'
  - [ ] 1.6: Export useCalculatorStore hook

- [ ] **Task 2: Create cost calculation utilities** (AC: #3, #4)
  - [ ] 2.1: Create `costCalculations.ts` in `/apps/web/src/utils`
  - [ ] 2.2: Define CostCalculationInput and CostCalculationResult interfaces
  - [ ] 2.3: Implement calculateMonthlyCost pure function
  - [ ] 2.4: Calculate input tokens cost: (volume * ratio/100) * (inputPrice / 1M)
  - [ ] 2.5: Calculate output tokens cost: (volume * (1-ratio/100)) * (outputPrice / 1M)
  - [ ] 2.6: Return object with totalCost, inputCost, outputCost, inputVolume, outputVolume
  - [ ] 2.7: Add unit tests for calculation logic

- [ ] **Task 3: Create CalculatorInputs component** (AC: #1, #3)
  - [ ] 3.1: Create `CalculatorInputs.tsx` in `/apps/web/src/components/calculator`
  - [ ] 3.2: Connect to calculatorStore (monthlyTokenVolume, inputOutputRatio)
  - [ ] 3.3: Create volume slider (100K - 50M, step 100K)
  - [ ] 3.4: Create volume number input (sync with slider)
  - [ ] 3.5: Display volume in millions (e.g., "2.5M tokens/month")
  - [ ] 3.6: Create ratio slider (0-100, step 1)
  - [ ] 3.7: Display ratio as percentage (e.g., "60% / 40%")
  - [ ] 3.8: Use Slider component from Epic 3 (if exists) or create new

- [ ] **Task 4: Create CostCalculator component (dual context)** (AC: #2, #5, #6)
  - [ ] 4.1: Create `CostCalculator.tsx` in `/apps/web/src/components/calculator`
  - [ ] 4.2: Accept optional modelId prop (if present = embedded mode, if null = standalone)
  - [ ] 4.3: Fetch model data with useModelDetail(modelId) if in embedded mode
  - [ ] 4.4: Get calculator state from useCalculatorStore
  - [ ] 4.5: Calculate cost using calculateMonthlyCost utility
  - [ ] 4.6: Render CalculatorInputs component
  - [ ] 4.7: Display total monthly cost (large, bold, colored)
  - [ ] 4.8: Display cost breakdown (input cost, output cost, volumes)
  - [ ] 4.9: Style with card/border/padding

- [ ] **Task 5: Create CalculatorPage (standalone)** (AC: #6)
  - [ ] 5.1: Create `CalculatorPage.tsx` in `/apps/web/src/pages`
  - [ ] 5.2: Render CostCalculator without modelId (standalone mode)
  - [ ] 5.3: Add page header with title "Cost Calculator"
  - [ ] 5.4: Create grid layout: calculator on left, results on right (placeholder)
  - [ ] 5.5: Placeholder for results table: "Results coming in Story 4.7"

- [ ] **Task 6: Add route for CalculatorPage** (AC: #6)
  - [ ] 6.1: Add `/calculator` route to React Router in App.tsx
  - [ ] 6.2: Route renders CalculatorPage component
  - [ ] 6.3: Add "Calculator" link to Header navigation
  - [ ] 6.4: Test navigation to /calculator works

- [ ] **Task 7: Embed calculator in PricingTab** (AC: #6)
  - [ ] 7.1: Import CostCalculator in PricingTab component
  - [ ] 7.2: Replace placeholder with <CostCalculator modelId={model.id} />
  - [ ] 7.3: Pass model ID to show cost for specific model
  - [ ] 7.4: Test calculator appears in Pricing tab
  - [ ] 7.5: Verify state syncs between embedded and standalone

- [ ] **Task 8: Testing and optimization**
  - [ ] 8.1: Write unit tests for costCalculations utilities
  - [ ] 8.2: Test calculateMonthlyCost with various inputs
  - [ ] 8.3: Write tests for CalculatorInputs component (Vitest)
  - [ ] 8.4: Test calculator state persists in localStorage
  - [ ] 8.5: Test state syncs between embedded and standalone (change in one, see in other)
  - [ ] 8.6: Verify calculation updates in real-time (<100ms)
  - [ ] 8.7: Test slider and number input stay in sync

## Dev Notes

### Architecture Context

**Dual Context Pattern:**
- Single CostCalculator component works in two contexts
- Embedded: modelId provided → calculate for one model
- Standalone: no modelId → calculate for all models (Story 4.7 extends this)
- State shared via Zustand store (changes in embedded reflect in standalone)

**State Persistence:**
- localStorage via Zustand persist middleware
- Key: 'calculator-state'
- Survives page refresh, shared across tabs
- Users return to same settings

**Pure Calculation Logic:**
- All math in pure functions (utils/costCalculations.ts)
- No side effects, 100% deterministic
- Easy to test, easy to debug
- Future: Reuse calculation on server for validation

### Project Structure Notes

**Files to Create:**
```
/apps/web/src/
├── components/
│   └── calculator/
│       ├── CostCalculator.tsx               # Main calculator (dual context)
│       └── CalculatorInputs.tsx             # Volume & ratio inputs
├── pages/
│   └── CalculatorPage.tsx                   # Standalone page at /calculator
├── store/
│   └── calculatorStore.ts                   # Calculator state (Zustand + persist)
├── utils/
│   └── costCalculations.ts                  # Pure calculation functions
└── App.tsx                                  # (update) Add /calculator route
```

### Implementation Details

**calculatorStore:**
```typescript
// store/calculatorStore.ts
import { create } from 'zustand';
import { persist } from 'zustand/middleware';

interface CalculatorState {
  monthlyTokenVolume: number;
  inputOutputRatio: number;
  activePreset: 'custom' | 'small' | 'medium' | 'large';
  
  setVolume: (volume: number) => void;
  setRatio: (ratio: number) => void;
  setPreset: (preset: string, volume: number, ratio: number) => void;
}

export const useCalculatorStore = create<CalculatorState>()(
  persist(
    (set) => ({
      monthlyTokenVolume: 1_000_000,
      inputOutputRatio: 50,
      activePreset: 'custom',

      setVolume: (volume) => set({ monthlyTokenVolume: volume, activePreset: 'custom' }),
      setRatio: (ratio) => set({ inputOutputRatio: ratio, activePreset: 'custom' }),
      setPreset: (preset, volume, ratio) => set({
        activePreset: preset as any,
        monthlyTokenVolume: volume,
        inputOutputRatio: ratio,
      }),
    }),
    {
      name: 'calculator-state',
    }
  )
);
```

**costCalculations utilities:**
```typescript
// utils/costCalculations.ts
export interface CostCalculationInput {
  monthlyTokenVolume: number;
  inputOutputRatio: number; // 0-100
  inputPricePerMillion: number;
  outputPricePerMillion: number;
}

export interface CostCalculationResult {
  totalMonthlyCost: number;
  inputTokensCost: number;
  outputTokensCost: number;
  inputTokensVolume: number;
  outputTokensVolume: number;
}

export const calculateMonthlyCost = (input: CostCalculationInput): CostCalculationResult => {
  const inputRatio = input.inputOutputRatio / 100;
  const outputRatio = 1 - inputRatio;

  const inputTokensVolume = input.monthlyTokenVolume * inputRatio;
  const outputTokensVolume = input.monthlyTokenVolume * outputRatio;

  const inputTokensCost = (inputTokensVolume / 1_000_000) * input.inputPricePerMillion;
  const outputTokensCost = (outputTokensVolume / 1_000_000) * input.outputPricePerMillion;

  return {
    totalMonthlyCost: inputTokensCost + outputTokensCost,
    inputTokensCost,
    outputTokensCost,
    inputTokensVolume,
    outputTokensVolume,
  };
};
```

**CostCalculator component:**
```typescript
// components/calculator/CostCalculator.tsx
import { useCalculatorStore } from '@store/calculatorStore';
import { useModelDetail } from '@hooks/useModelDetail';
import { calculateMonthlyCost } from '@/utils/costCalculations';
import { CalculatorInputs } from './CalculatorInputs';

interface CostCalculatorProps {
  modelId?: string; // If provided = embedded, if null = standalone
}

export const CostCalculator = ({ modelId }: CostCalculatorProps) => {
  const { monthlyTokenVolume, inputOutputRatio } = useCalculatorStore();
  const { data: model } = useModelDetail(modelId || null);

  const cost = model ? calculateMonthlyCost({
    monthlyTokenVolume,
    inputOutputRatio,
    inputPricePerMillion: model.inputPricePerMillion,
    outputPricePerMillion: model.outputPricePerMillion,
  }) : null;

  return (
    <div className="bg-white rounded-lg border p-6 space-y-6">
      <h3 className="text-xl font-bold">Cost Calculator</h3>

      <CalculatorInputs />

      {cost && (
        <div className="border-t pt-6">
          <h4 className="font-semibold mb-4">Estimated Monthly Cost</h4>
          <div className="text-3xl font-bold text-blue-600 mb-4">
            ${cost.totalMonthlyCost.toFixed(2)}
          </div>

          <div className="space-y-2 text-sm">
            <div className="flex justify-between">
              <span>Input tokens ({cost.inputTokensVolume.toLocaleString()})</span>
              <span>${cost.inputTokensCost.toFixed(2)}</span>
            </div>
            <div className="flex justify-between">
              <span>Output tokens ({cost.outputTokensVolume.toLocaleString()})</span>
              <span>${cost.outputTokensCost.toFixed(2)}</span>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};
```

### References

- [Epic 4 Analysis: docs/epic-4-analysis-and-plan.md#Story 4.6]
- [ADR-017: Client-Side Cost Calculation]
- [ADR-018: Dual Calculator Context]
- [Story 4.5: Pricing Tab] (embedding context)

### Testing Strategy

**Unit Tests:**
- calculateMonthlyCost returns correct values for various inputs
- Edge cases: 0 volume, 100% input, 100% output
- calculatorStore persists state correctly
- setVolume/setRatio update state

**Integration Tests:**
- Calculator renders in Pricing tab (embedded)
- Calculator renders on /calculator page (standalone)
- State syncs between both contexts
- Changing slider updates cost immediately
- localStorage persists state across page refresh

**Performance Tests:**
- Calculation completes in <100ms
- No lag when moving sliders
- Memoization prevents unnecessary recalculations

## Dev Agent Record

### Context Reference

### Agent Model Used

claude-sonnet-4-5-20250929

### Debug Log References

### Completion Notes List

### File List
