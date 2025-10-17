# Story 4.5: Pricing Tab with Detailed Breakdown

Status: Ready

## Story

As a user,
I want detailed pricing information and calculator,
so that I can understand costs and estimate my usage.

## Acceptance Criteria

1. Pricing tab displays:
   - Input price per 1M tokens (with per-1K and per-token breakdown)
   - Output price per 1M tokens (with per-1K and per-token breakdown)
   - Currency
   - Pricing validity period
   - Last updated timestamp
2. Comparison note showing how price compares to similar models
3. Embedded cost calculator widget (next story)

## Tasks / Subtasks

- [ ] **Task 1: Create PricingTab component** (AC: #1, #2)
  - [ ] 1.1: Create `PricingTab.tsx` in `/apps/web/src/components/models/tabs`
  - [ ] 1.2: Accept model prop (ModelDetail type)
  - [ ] 1.3: Create "Price Breakdown" section with input and output cards
  - [ ] 1.4: Create "Price Comparison" section (if model.priceComparison exists)
  - [ ] 1.5: Create "Pricing Information" section with metadata
  - [ ] 1.6: Create placeholder for cost calculator (Story 4.6)

- [ ] **Task 2: Create PriceCard component** (AC: #1)
  - [ ] 2.1: Create `PriceCard.tsx` component
  - [ ] 2.2: Accept props: title ("Input Tokens" / "Output Tokens"), prices object
  - [ ] 2.3: Display price per 1M tokens (highlight, larger font)
  - [ ] 2.4: Display price per 1K tokens
  - [ ] 2.5: Display price per token (smaller font, scientific notation)
  - [ ] 2.6: Style with border, padding, rounded corners

- [ ] **Task 3: Create PriceRow component** (AC: #1)
  - [ ] 3.1: Create `PriceRow.tsx` component
  - [ ] 3.2: Accept props: label, value, highlight (boolean), small (boolean)
  - [ ] 3.3: Display label on left, value on right (flex justify-between)
  - [ ] 3.4: If highlight=true, use larger font and bold
  - [ ] 3.5: If small=true, use smaller font size and gray color

- [ ] **Task 4: Implement price comparison display** (AC: #2)
  - [ ] 4.1: Check if model.priceComparison exists
  - [ ] 4.2: Display comparison text in blue info box (e.g., "15% cheaper than GPT-4")
  - [ ] 4.3: Show compared model name in smaller text below
  - [ ] 4.4: Add color coding: green if cheaper, red if more expensive
  - [ ] 4.5: If no comparison, hide section entirely

- [ ] **Task 5: Display pricing metadata** (AC: #1)
  - [ ] 5.1: Show currency with currency symbol (USD → $)
  - [ ] 5.2: Display pricing valid from/to dates if available
  - [ ] 5.3: Show last updated timestamp with formatRelativeTime
  - [ ] 5.4: Style metadata section with subtle background

- [ ] **Task 6: Add cost calculator placeholder** (AC: #3)
  - [ ] 6.1: Create "Cost Calculator" section in PricingTab
  - [ ] 6.2: Display placeholder text: "Calculator will be added in next story"
  - [ ] 6.3: Style with dashed border to indicate future content
  - [ ] 6.4: Document where CostCalculator component will be embedded

- [ ] **Task 7: Integrate PricingTab in modal** (AC: #1)
  - [ ] 7.1: Import PricingTab in ModelDetailModal
  - [ ] 7.2: Render when activeTab === 'pricing'
  - [ ] 7.3: Pass model data from useModelDetail hook
  - [ ] 7.4: Test tab switching to Pricing tab works

- [ ] **Task 8: Testing and polish**
  - [ ] 8.1: Write unit tests for PricingTab component (Vitest)
  - [ ] 8.2: Test price breakdown displays all three formats correctly
  - [ ] 8.3: Test price comparison shows when available, hidden when not
  - [ ] 8.4: Test currency symbol formats correctly
  - [ ] 8.5: Test validity dates display when present
  - [ ] 8.6: Verify layout responsive (cards stack on mobile)

## Dev Notes

### Architecture Context

**Price Breakdown Rationale:**
- Per 1M tokens: Primary pricing (matches provider websites)
- Per 1K tokens: Helpful for mental math (divide by 1000)
- Per token: Exact calculation reference
- Backend calculates derived prices to ensure consistency

**Price Comparison Context:**
- Backend calculates vs similar models (same provider or similar capabilities)
- Helps users understand relative positioning
- Future: Compare within same "tier" (GPT-4 vs Claude Opus)

**Calculator Integration (Story 4.6):**
- CostCalculator component will replace placeholder
- Calculator embedded directly in Pricing tab (no separate page yet)
- Uses model.inputPricePerMillion and model.outputPricePerMillion for calculations

### Project Structure Notes

**Frontend Files to Create:**
```
/apps/web/src/
└── components/
    └── models/
        └── tabs/
            ├── PricingTab.tsx               # Main pricing tab
            ├── PriceCard.tsx                # Price breakdown card
            └── PriceRow.tsx                 # Individual price row
```

### Implementation Details

**PricingTab Component:**
```typescript
// components/models/tabs/PricingTab.tsx
import { ModelDetail } from '@/types/modelDetail';
import { Section } from './Section';
import { PriceCard } from './PriceCard';
import { InfoRow } from './InfoRow';
import { formatDate, formatRelativeTime } from '@/utils/formatters';

interface PricingTabProps {
  model: ModelDetail;
}

export const PricingTab = ({ model }: PricingTabProps) => (
  <div className="space-y-6">
    {/* Price Breakdown */}
    <Section title="Price Breakdown">
      <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
        <PriceCard
          title="Input Tokens"
          prices={{
            perMillion: model.inputPricePerMillion,
            perThousand: model.inputPricePerThousand,
            perToken: model.inputPricePerToken,
          }}
        />
        <PriceCard
          title="Output Tokens"
          prices={{
            perMillion: model.outputPricePerMillion,
            perThousand: model.outputPricePerThousand,
            perToken: model.outputPricePerToken,
          }}
        />
      </div>
    </Section>

    {/* Price Comparison */}
    {model.priceComparison && (
      <Section title="Price Comparison">
        <div className={`p-4 rounded-lg ${
          model.priceComparison.percentageDifference < 0 ? 'bg-green-50' : 'bg-red-50'
        }`}>
          <p className="text-lg font-semibold">
            {model.priceComparison.comparisonText}
          </p>
          <p className="text-sm text-gray-600 mt-1">
            Compared to {model.priceComparison.comparedToModel}
          </p>
        </div>
      </Section>
    )}

    {/* Pricing Information */}
    <Section title="Pricing Information">
      <InfoRow label="Currency" value={model.currency} />
      {model.pricingValidFrom && (
        <InfoRow label="Valid From" value={formatDate(model.pricingValidFrom)} />
      )}
      {model.pricingValidTo && (
        <InfoRow label="Valid Until" value={formatDate(model.pricingValidTo)} />
      )}
      <InfoRow label="Last Updated" value={formatRelativeTime(model.updatedAt)} />
    </Section>

    {/* Cost Calculator Placeholder */}
    <Section title="Cost Calculator">
      <div className="border-2 border-dashed border-gray-300 rounded-lg p-8 text-center text-gray-500">
        Calculator will be embedded here in Story 4.6
      </div>
    </Section>
  </div>
);
```

**PriceCard Component:**
```typescript
// components/models/tabs/PriceCard.tsx
interface PriceCardProps {
  title: string;
  prices: {
    perMillion: number;
    perThousand: number;
    perToken: number;
  };
}

export const PriceCard = ({ title, prices }: PriceCardProps) => (
  <div className="p-4 border border-gray-200 rounded-lg">
    <h4 className="font-semibold text-gray-700 mb-3">{title}</h4>
    <div className="space-y-2">
      <PriceRow
        label="Per 1M tokens"
        value={`$${prices.perMillion.toFixed(2)}`}
        highlight
      />
      <PriceRow
        label="Per 1K tokens"
        value={`$${prices.perThousand.toFixed(4)}`}
      />
      <PriceRow
        label="Per token"
        value={`$${prices.perToken.toFixed(8)}`}
        small
      />
    </div>
  </div>
);
```

**PriceRow Component:**
```typescript
// components/models/tabs/PriceRow.tsx
interface PriceRowProps {
  label: string;
  value: string;
  highlight?: boolean;
  small?: boolean;
}

export const PriceRow = ({ label, value, highlight, small }: PriceRowProps) => (
  <div className="flex justify-between items-center">
    <span className={`${small ? 'text-xs' : 'text-sm'} text-gray-600`}>
      {label}
    </span>
    <span className={`${
      highlight ? 'text-lg font-bold text-gray-900' : 
      small ? 'text-xs text-gray-500' : 
      'text-sm text-gray-700'
    }`}>
      {value}
    </span>
  </div>
);
```

### References

- [Epic 4 Analysis: docs/epic-4-analysis-and-plan.md#Story 4.5]
- [Story 4.2: Backend API] (ModelDetailDto with price breakdowns)
- [Story 4.6: Cost Calculator] (will replace placeholder)
- [Epics Document: docs/epics.md#Story 4.5]

### Testing Strategy

**Unit Tests:**
- PricingTab renders all sections correctly
- PriceCard displays three price formats with correct decimals
- Price comparison displays when present, hidden when null
- Currency displays correctly
- Validity dates format correctly

**Integration Tests:**
- Tab switching to Pricing tab loads data
- Price comparison color coding (green if cheaper, red if expensive)
- Responsive layout (cards stack on mobile)

## Dev Agent Record

### Context Reference

### Agent Model Used

claude-sonnet-4-5-20250929

### Debug Log References

### Completion Notes List

### File List
