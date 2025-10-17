# Epic 4: Model Detail & Cost Calculator
## Analysis & Implementation Plan

**Author:** Pablo (with Claude Code Analysis)
**Date:** 2025-10-16
**Status:** Ready for Implementation
**Epic Goal:** Enable deep-dive model exploration and cost calculation for workload planning through detailed model modals and embedded cost calculator

---

## Executive Summary

Epic 4 transforms your platform from a data browser (Epic 3) into a decision-making tool. Users will explore complete model specifications in a detailed modal and calculate exact monthly costs for their specific workloads. The cost calculator—usable both embedded and standalone—is the killer feature that converts browsing into actionable insights.

### Key Value Proposition

**Epic 3** answers: *"What models are available?"*
**Epic 4** answers: *"Which model should I choose for my workload and what will it cost?"*

### Refined Scope Analysis

**Original**: 12 stories (4.1-4.12)
**Refined**: 11 stories (consolidated for efficiency)
**Estimated Effort**: 28-35 hours (3.5-4.5 days full-time development)

### Critical Dependencies

- ✅ **Epic 3 Complete**: ModelsTable, useModels hook, Header component
- ✅ **Chart.js Installed**: Already in package.json from Epic 3
- ⚠️ **Backend Extension Needed**: GET /api/models/{id} endpoint with full details
- ⚠️ **Calculator Store**: New Zustand store for calculator state

---

## 1. Current State & Dependencies

### ✅ What We Have from Epic 3

**Frontend Components:**
- ModelsTable with clickable model names (Story 3.2-3.3)
- PublicLayout with Header (Story 3.1)
- useModels hook with TanStack Query (Story 3.2)
- Chart.js library installed (Story 3.15)
- React Router configured with URL params support (Story 3.1)

**Backend Infrastructure:**
- GET /api/models endpoint (returns list with top 3-5 benchmarks)
- Domain entities: Model, Capability, Benchmark, BenchmarkScore
- Redis caching infrastructure

**State Management:**
- TanStack Query for server state
- Zustand for client state (filters, comparison)

### ⚠️ What We Need to Build for Epic 4

**Backend Extensions:**
- [ ] GET /api/models/{id} endpoint (single model with ALL benchmarks)
- [ ] Caching strategy for individual models (30 min TTL vs 1 hour for list)
- [ ] Price comparison logic (compare model to similar models)

**Frontend New Components:**
- [ ] Modal system (base Modal component)
- [ ] Tab navigation (reusable Tabs component)
- [ ] Cost calculator logic (pure calculation functions)
- [ ] Calculator UI (sliders, presets, results)
- [ ] Chart integration (Bar chart for cost visualization)

**New State:**
- [ ] Calculator state store (Zustand)
- [ ] Modal state (URL-based with React Router)

---

## 2. Refined Epic 4 Scope

### Original 12 Stories Review

**Group A: Model Detail Modal (4.1-4.5)**
1. Story 4.1: Create Model Detail Modal Component
2. Story 4.2: Add Overview Tab with Model Specifications
3. Story 4.3: Create Backend API for Model Detail
4. Story 4.4: Add Benchmarks Tab with All Scores
5. Story 4.5: Add Pricing Tab with Detailed Breakdown

**Group B: Cost Calculator (4.6-4.9)**
6. Story 4.6: Create Cost Calculator Component
7. Story 4.7: Embed Calculator in Pricing Tab and Create Standalone Page
8. Story 4.8: Add Cost Comparison Table to Standalone Calculator
9. Story 4.9: Add Preset Workload Scenarios

**Group C: Visualization & Optimization (4.10-4.12)**
10. Story 4.10: Add Visualization to Calculator Results
11. Story 4.11: Add Export/Share Calculator Results
12. Story 4.12: Optimize Calculator Performance

### 🔄 Scope Refinement Decisions

**Consolidations:**

- **4.6 + 4.7** → **New 4.6**: "Create Cost Calculator Component (Embedded + Standalone)"
  - Rationale: Building the calculator once for both contexts is more efficient than creating separately then integrating
  - Single implementation with different rendering contexts

- **4.11 + 4.12** → **New 4.10**: "Calculator Sharing & Performance Optimization"
  - Rationale: Share functionality and performance optimization are both polish tasks best done together
  - URL params for sharing naturally leads to testing performance with various parameter values

**Reordering for Logical Flow:**

- Move **4.3** (Backend API) BEFORE **4.2** (Overview Tab)
  - Reason: Frontend needs API first (can use mock data temporarily)

**Final Epic 4 Scope: 11 Stories**

1. **Story 4.1**: Model Detail Modal Component (Base Modal + URL Routing)
2. **Story 4.2**: Backend API for Model Detail (GET /api/models/{id})
3. **Story 4.3**: Overview Tab with Model Specifications
4. **Story 4.4**: Benchmarks Tab with All Scores
5. **Story 4.5**: Pricing Tab with Detailed Breakdown
6. **Story 4.6**: Cost Calculator Component (Embedded + Standalone) _(consolidated 4.6+4.7)_
7. **Story 4.7**: Cost Comparison Table in Calculator _(was 4.8)_
8. **Story 4.8**: Preset Workload Scenarios _(was 4.9)_
9. **Story 4.9**: Cost Visualization with Chart.js _(was 4.10)_
10. **Story 4.10**: Calculator Sharing & Performance _(consolidated 4.11+4.12)_
11. **Story 4.11**: Modal Polish & Responsive Design

---

## 3. Technical Specification

### 3.1 Architecture Overview

```
apps/web/src/
├── components/
│   ├── ui/
│   │   ├── Modal.tsx                    # Story 4.1 - Base modal component
│   │   ├── Tabs.tsx                     # Story 4.3 - Tab navigation
│   │   └── Slider.tsx                   # (from Epic 3, reused)
│   ├── models/
│   │   ├── ModelDetailModal.tsx         # Story 4.1 - Main modal container
│   │   ├── tabs/
│   │   │   ├── OverviewTab.tsx          # Story 4.3
│   │   │   ├── BenchmarksTab.tsx        # Story 4.4
│   │   │   └── PricingTab.tsx           # Story 4.5
│   └── calculator/
│       ├── CostCalculator.tsx           # Story 4.6 - Main calculator component
│       ├── CalculatorInputs.tsx         # Story 4.6 - Volume & ratio inputs
│       ├── CalculatorResults.tsx        # Story 4.7 - Results table
│       ├── WorkloadPresets.tsx          # Story 4.8 - Preset buttons
│       ├── CostChart.tsx                # Story 4.9 - Bar chart visualization
│       └── ShareButton.tsx              # Story 4.10 - Share functionality
├── pages/
│   ├── HomePage.tsx                     # (Epic 3) - Add modal integration
│   └── CalculatorPage.tsx               # Story 4.6 - Standalone calculator page
├── store/
│   └── calculatorStore.ts               # Story 4.6 - Calculator state (Zustand)
├── hooks/
│   ├── useModelDetail.ts                # Story 4.2 - Fetch single model
│   ├── useCostCalculation.ts            # Story 4.6 - Cost calculation logic
│   └── useCalculatorParams.ts           # Story 4.10 - URL params sync
└── utils/
    ├── costCalculations.ts              # Story 4.6 - Pure calculation functions
    └── formatters.ts                    # (Epic 3) - Extend with cost formatting
```

### 3.2 Backend Requirements

#### New Endpoint: GET /api/models/{id}

```csharp
// Location: services/backend/LlmTokenPrice.API/Controllers/ModelsController.cs

[HttpGet("{id}")]
[ResponseCache(Duration = 1800)] // 30 minutes (vs 1 hour for list)
public async Task<ActionResult<ApiResponse<ModelDetailDto>>> GetModelById(Guid id)
{
    var model = await _modelService.GetModelByIdAsync(id);

    if (model == null)
    {
        return NotFound(new { Error = "Model not found" });
    }

    return Ok(new ApiResponse<ModelDetailDto>
    {
        Data = model,
        Meta = new { Cached = false, Timestamp = DateTime.UtcNow }
    });
}
```

#### Extended DTO: ModelDetailDto

```csharp
// Location: services/backend/LlmTokenPrice.Application/DTOs/ModelDetailDto.cs

public record ModelDetailDto
{
    // Basic Info (same as ModelDto)
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Provider { get; init; } = string.Empty;
    public string? Version { get; init; }
    public DateTime? ReleaseDate { get; init; }
    public string Status { get; init; } = "active";

    // Pricing (extended with breakdown)
    public decimal InputPricePerMillion { get; init; }
    public decimal OutputPricePerMillion { get; init; }
    public decimal InputPricePerThousand { get; init; }  // Calculated: / 1000
    public decimal OutputPricePerThousand { get; init; } // Calculated: / 1000
    public decimal InputPricePerToken { get; init; }     // Calculated: / 1,000,000
    public decimal OutputPricePerToken { get; init; }    // Calculated: / 1,000,000
    public string Currency { get; init; } = "USD";
    public DateTime? PricingValidFrom { get; init; }
    public DateTime? PricingValidTo { get; init; }

    // Capabilities (complete object)
    public CapabilitiesDto Capabilities { get; init; } = null!;

    // ALL Benchmarks (not just top 3-5)
    public List<BenchmarkScoreDetailDto> BenchmarkScores { get; init; } = new();

    // Metadata
    public DateTime UpdatedAt { get; init; }
    public DateTime CreatedAt { get; init; }

    // Price Comparison Context (calculated server-side)
    public PriceComparisonDto? PriceComparison { get; init; }
}

public record BenchmarkScoreDetailDto
{
    public string BenchmarkName { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public decimal Score { get; init; }
    public decimal? MaxScore { get; init; }
    public string Interpretation { get; init; } = string.Empty; // "Higher is better"
    public DateTime? TestDate { get; init; }
    public string? SourceUrl { get; init; }
}

public record PriceComparisonDto
{
    public string ComparisonText { get; init; } = string.Empty; // "15% cheaper than GPT-4"
    public decimal PercentageDifference { get; init; }
    public string ComparedToModel { get; init; } = string.Empty;
}
```

#### Service Layer Extension

```csharp
// Location: services/backend/LlmTokenPrice.Application/Services/ModelService.cs

public async Task<ModelDetailDto?> GetModelByIdAsync(Guid id)
{
    var cacheKey = $"cache:model:detail:{id}";
    var cached = await _cache.GetAsync<ModelDetailDto>(cacheKey);

    if (cached != null) return cached;

    var model = await _repository.GetByIdWithAllDetailsAsync(id);

    if (model == null) return null;

    var modelDto = MapToDetailDto(model);

    // Calculate price comparison
    modelDto = modelDto with
    {
        PriceComparison = await CalculatePriceComparisonAsync(model)
    };

    await _cache.SetAsync(cacheKey, modelDto, TimeSpan.FromMinutes(30));

    return modelDto;
}

private async Task<PriceComparisonDto?> CalculatePriceComparisonAsync(Model model)
{
    // Get similar models (same provider or similar capabilities)
    var similarModels = await _repository.GetSimilarModelsAsync(model.Id);

    if (!similarModels.Any()) return null;

    var avgPrice = similarModels.Average(m => m.InputPricePerMillion + m.OutputPricePerMillion);
    var currentPrice = model.InputPricePerMillion + model.OutputPricePerMillion;
    var percentDiff = ((currentPrice - avgPrice) / avgPrice) * 100;

    var comparedTo = percentDiff < 0
        ? similarModels.OrderByDescending(m => m.InputPricePerMillion + m.OutputPricePerMillion).First()
        : similarModels.OrderBy(m => m.InputPricePerMillion + m.OutputPricePerMillion).First();

    var text = percentDiff < 0
        ? $"{Math.Abs(percentDiff):F0}% cheaper than {comparedTo.Name}"
        : $"{percentDiff:F0}% more expensive than {comparedTo.Name}";

    return new PriceComparisonDto
    {
        ComparisonText = text,
        PercentageDifference = percentDiff,
        ComparedToModel = comparedTo.Name
    };
}
```

### 3.3 Frontend Architecture Details

#### Modal State Management (URL-Based)

**Pattern**: Modal state lives in URL, not component state
**Rationale**: Shareable URLs, browser back button support, deep linking

```typescript
// hooks/useModalState.ts
import { useSearchParams, useNavigate } from 'react-router-dom';

export const useModalState = () => {
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();

  const modelId = searchParams.get('model');
  const isOpen = modelId !== null;

  const openModal = (id: string) => {
    const params = new URLSearchParams(searchParams);
    params.set('model', id);
    navigate(`?${params.toString()}`, { replace: false });
  };

  const closeModal = () => {
    const params = new URLSearchParams(searchParams);
    params.delete('model');
    navigate(`?${params.toString()}`, { replace: false });
  };

  return { modelId, isOpen, openModal, closeModal };
};
```

**Usage in ModelsTable**:
```typescript
// components/models/ModelsTable.tsx
const { openModal } = useModalState();

<td onClick={() => openModal(model.id)} className="cursor-pointer hover:underline">
  {model.name}
</td>
```

#### Cost Calculator State Management

```typescript
// store/calculatorStore.ts
import { create } from 'zustand';
import { persist } from 'zustand/middleware';

interface CalculatorState {
  // Inputs
  monthlyTokenVolume: number;
  inputOutputRatio: number; // 0-100 (% input tokens)

  // Presets
  activePreset: 'custom' | 'small' | 'medium' | 'large';

  // Actions
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

#### Cost Calculation Logic (Pure Functions)

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

  const totalMonthlyCost = inputTokensCost + outputTokensCost;

  return {
    totalMonthlyCost,
    inputTokensCost,
    outputTokensCost,
    inputTokensVolume,
    outputTokensVolume,
  };
};

export const calculateCostForAllModels = (
  models: Model[],
  volume: number,
  ratio: number
): Array<Model & { calculatedCost: CostCalculationResult }> => {
  return models.map(model => ({
    ...model,
    calculatedCost: calculateMonthlyCost({
      monthlyTokenVolume: volume,
      inputOutputRatio: ratio,
      inputPricePerMillion: model.inputPricePerMillion,
      outputPricePerMillion: model.outputPricePerMillion,
    }),
  }));
};
```

#### Workload Presets Configuration

```typescript
// utils/workloadPresets.ts

export interface WorkloadPreset {
  id: string;
  name: string;
  description: string;
  volume: number;
  ratio: number; // % input tokens
}

export const WORKLOAD_PRESETS: WorkloadPreset[] = [
  {
    id: 'small',
    name: 'Small Project',
    description: 'Personal projects, prototypes (1M tokens/month)',
    volume: 1_000_000,
    ratio: 60, // More input (user queries)
  },
  {
    id: 'medium',
    name: 'Medium Project',
    description: 'Team applications, MVPs (5M tokens/month)',
    volume: 5_000_000,
    ratio: 50, // Balanced
  },
  {
    id: 'large',
    name: 'Large Project',
    description: 'Production apps, high traffic (20M tokens/month)',
    volume: 20_000_000,
    ratio: 70, // Heavy input processing
  },
  {
    id: 'custom',
    name: 'Custom',
    description: 'Define your own parameters',
    volume: 1_000_000,
    ratio: 50,
  },
];
```

### 3.4 Chart.js Integration for Cost Visualization

```typescript
// components/calculator/CostChart.tsx
import { Bar } from 'react-chartjs-2';
import {
  Chart as ChartJS,
  CategoryScale,
  LinearScale,
  BarElement,
  Title,
  Tooltip,
  Legend,
} from 'chart.js';

ChartJS.register(CategoryScale, LinearScale, BarElement, Title, Tooltip, Legend);

interface CostChartProps {
  models: Array<Model & { calculatedCost: CostCalculationResult }>;
  maxModels?: number;
}

export const CostChart = ({ models, maxModels = 10 }: CostChartProps) => {
  const sortedModels = models
    .sort((a, b) => a.calculatedCost.totalMonthlyCost - b.calculatedCost.totalMonthlyCost)
    .slice(0, maxModels);

  const data = {
    labels: sortedModels.map(m => m.name),
    datasets: [
      {
        label: 'Monthly Cost',
        data: sortedModels.map(m => m.calculatedCost.totalMonthlyCost),
        backgroundColor: 'rgba(59, 130, 246, 0.5)',
        borderColor: 'rgb(59, 130, 246)',
        borderWidth: 1,
      },
    ],
  };

  const options = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: { display: false },
      title: {
        display: true,
        text: 'Monthly Cost Comparison (Top 10)',
      },
      tooltip: {
        callbacks: {
          label: (context: any) => {
            const model = sortedModels[context.dataIndex];
            return [
              `Total: $${model.calculatedCost.totalMonthlyCost.toFixed(2)}`,
              `Input: $${model.calculatedCost.inputTokensCost.toFixed(2)}`,
              `Output: $${model.calculatedCost.outputTokensCost.toFixed(2)}`,
            ];
          },
        },
      },
    },
    scales: {
      y: {
        beginAtZero: true,
        ticks: {
          callback: (value: number) => `$${value.toFixed(0)}`,
        },
      },
    },
  };

  return (
    <div className="h-96">
      <Bar data={data} options={options} />
    </div>
  );
};
```

---

## 4. Implementation Plan - Step by Step

### Sprint 1: Modal Foundation (Stories 4.1-4.2)

#### 📋 Story 4.1: Model Detail Modal Component
**Estimated Time:** 3-4 hours
**Prerequisites:** Epic 3 complete (ModelsTable with clickable models)

**Tasks:**

1. **Create base Modal component** (1 hour)
   ```typescript
   // components/ui/Modal.tsx
   import { useEffect } from 'react';

   interface ModalProps {
     isOpen: boolean;
     onClose: () => void;
     children: React.ReactNode;
     title?: string;
   }

   export const Modal = ({ isOpen, onClose, children, title }: ModalProps) => {
     useEffect(() => {
       const handleEsc = (e: KeyboardEvent) => {
         if (e.key === 'Escape') onClose();
       };

       if (isOpen) {
         document.addEventListener('keydown', handleEsc);
         document.body.style.overflow = 'hidden';
       }

       return () => {
         document.removeEventListener('keydown', handleEsc);
         document.body.style.overflow = 'unset';
       };
     }, [isOpen, onClose]);

     if (!isOpen) return null;

     return (
       <div className="fixed inset-0 z-50 flex items-center justify-center">
         {/* Backdrop */}
         <div
           className="absolute inset-0 bg-black/50"
           onClick={onClose}
         />

         {/* Modal Content */}
         <div className="relative bg-white rounded-lg shadow-xl max-w-4xl w-full max-h-[90vh] overflow-hidden">
           {/* Header */}
           <div className="flex items-center justify-between p-6 border-b">
             <h2 className="text-2xl font-bold">{title}</h2>
             <button onClick={onClose} className="text-gray-500 hover:text-gray-700">
               <XIcon className="w-6 h-6" />
             </button>
           </div>

           {/* Body */}
           <div className="overflow-y-auto max-h-[calc(90vh-80px)]">
             {children}
           </div>
         </div>
       </div>
     );
   };
   ```

2. **Create useModalState hook** (30 min)
   - Implement URL-based modal state (see section 3.3)

3. **Create ModelDetailModal component** (1 hour)
   ```typescript
   // components/models/ModelDetailModal.tsx
   export const ModelDetailModal = () => {
     const { modelId, isOpen, closeModal } = useModalState();
     const { data: model, isLoading } = useModelDetail(modelId);

     if (!isOpen) return null;

     return (
       <Modal
         isOpen={isOpen}
         onClose={closeModal}
         title={model?.name || 'Loading...'}
       >
         {isLoading ? (
           <LoadingSpinner />
         ) : model ? (
           <div className="p-6">
             {/* Placeholder content - tabs will be added in Story 4.3 */}
             <p>Model details will appear here</p>
           </div>
         ) : (
           <ErrorMessage message="Model not found" />
         )}
       </Modal>
     );
   };
   ```

4. **Integrate modal in HomePage** (30 min)
   - Add ModelDetailModal to HomePage
   - Make model names in table clickable

5. **Test browser back button** (30 min)
   - Verify back button closes modal
   - Verify URL updates correctly

**Acceptance Criteria:**
- [x] Clicking model name opens modal overlay
- [x] Modal shows model name in header
- [x] Close button works
- [x] Clicking backdrop closes modal
- [x] URL updates with ?model={id}
- [x] Browser back button closes modal
- [x] Escape key closes modal

---

#### 📋 Story 4.2: Backend API for Model Detail
**Estimated Time:** 3 hours
**Prerequisites:** Story 4.1 (frontend can use mock data temporarily)

**Tasks:**

1. **Create ModelDetailDto** (30 min)
   - See section 3.2 for full DTO structure
   - Include price breakdowns (per million, per thousand, per token)

2. **Implement GetModelByIdAsync service** (1 hour)
   ```csharp
   public async Task<ModelDetailDto?> GetModelByIdAsync(Guid id)
   {
       var cacheKey = $"cache:model:detail:{id}";
       var cached = await _cache.GetAsync<ModelDetailDto>(cacheKey);

       if (cached != null) return cached;

       var model = await _repository.GetByIdWithAllDetailsAsync(id);

       if (model == null) return null;

       var modelDto = new ModelDetailDto
       {
           Id = model.Id,
           Name = model.Name,
           // ... map all fields
           InputPricePerThousand = model.InputPricePerMillion / 1000,
           InputPricePerToken = model.InputPricePerMillion / 1_000_000,
           // ... calculate other derived fields
           BenchmarkScores = model.BenchmarkScores.Select(MapToBenchmarkDetailDto).ToList(),
       };

       await _cache.SetAsync(cacheKey, modelDto, TimeSpan.FromMinutes(30));

       return modelDto;
   }
   ```

3. **Create GET /api/models/{id} endpoint** (45 min)
   - Controller action (see section 3.2)
   - 404 handling for missing models

4. **Implement price comparison logic** (45 min)
   - Calculate vs similar models
   - Generate comparison text

5. **Test endpoint** (30 min)
   - Postman/curl tests
   - Verify caching works
   - Verify all benchmarks included

**Acceptance Criteria:**
- [x] GET /api/models/{id} endpoint created
- [x] Returns single model with ALL fields
- [x] Includes complete capabilities object
- [x] Includes ALL benchmark scores (not just top 3-5)
- [x] Returns 404 if model not found
- [x] Response cached in Redis (30 min TTL)
- [x] Price comparison calculated and included

---

### Sprint 2: Modal Tabs (Stories 4.3-4.5)

#### 📋 Story 4.3: Overview Tab with Model Specifications
**Estimated Time:** 3 hours
**Prerequisites:** Story 4.2 (backend API ready)

**Tasks:**

1. **Create Tabs component** (1 hour)
   ```typescript
   // components/ui/Tabs.tsx
   interface TabsProps {
     tabs: Array<{ id: string; label: string }>;
     activeTab: string;
     onChange: (tabId: string) => void;
     children: React.ReactNode;
   }

   export const Tabs = ({ tabs, activeTab, onChange, children }: TabsProps) => (
     <div>
       <div className="border-b">
         <nav className="flex space-x-8">
           {tabs.map(tab => (
             <button
               key={tab.id}
               onClick={() => onChange(tab.id)}
               className={`py-4 px-1 border-b-2 font-medium ${
                 activeTab === tab.id
                   ? 'border-blue-500 text-blue-600'
                   : 'border-transparent text-gray-500 hover:text-gray-700'
               }`}
             >
               {tab.label}
             </button>
           ))}
         </nav>
       </div>
       <div className="py-6">{children}</div>
     </div>
   );
   ```

2. **Create OverviewTab component** (1.5 hours)
   ```typescript
   // components/models/tabs/OverviewTab.tsx
   export const OverviewTab = ({ model }: { model: ModelDetail }) => (
     <div className="space-y-6">
       {/* Basic Info Section */}
       <Section title="Basic Information">
         <InfoRow label="Model Name" value={model.name} />
         <InfoRow label="Provider" value={model.provider} />
         <InfoRow label="Version" value={model.version || 'N/A'} />
         <InfoRow label="Release Date" value={formatDate(model.releaseDate)} />
         <InfoRow label="Status">
           <Badge variant={model.status === 'active' ? 'success' : 'warning'}>
             {model.status}
           </Badge>
         </InfoRow>
       </Section>

       {/* Pricing Section */}
       <Section title="Pricing">
         <InfoRow label="Input (per 1M tokens)" value={`$${model.inputPricePerMillion}`} />
         <InfoRow label="Output (per 1M tokens)" value={`$${model.outputPricePerMillion}`} />
         <InfoRow label="Currency" value={model.currency} />
       </Section>

       {/* Capabilities Section */}
       <Section title="Capabilities">
         <InfoRow label="Context Window" value={`${model.capabilities.contextWindow.toLocaleString()} tokens`} />
         <InfoRow label="Max Output" value={`${model.capabilities.maxOutputTokens.toLocaleString()} tokens`} />
         <div className="grid grid-cols-2 gap-4 mt-4">
           <CapabilityBadge
             label="Function Calling"
             supported={model.capabilities.supportsFunctionCalling}
           />
           <CapabilityBadge
             label="Vision Support"
             supported={model.capabilities.supportsVision}
           />
           <CapabilityBadge
             label="Streaming"
             supported={model.capabilities.supportsStreaming}
           />
           <CapabilityBadge
             label="JSON Mode"
             supported={model.capabilities.supportsJsonMode}
           />
         </div>
       </Section>

       {/* Metadata */}
       <Section title="Metadata">
         <InfoRow label="Last Updated" value={formatRelativeTime(model.updatedAt)} />
       </Section>
     </div>
   );
   ```

3. **Integrate tabs in ModelDetailModal** (30 min)
   - Add tab navigation
   - Default to Overview tab

**Acceptance Criteria:**
- [x] Tab navigation works (Overview, Benchmarks, Pricing)
- [x] Overview tab displays all model info
- [x] Data fetched from GET /api/models/{id}
- [x] Loading state shown while fetching
- [x] Information organized in clear sections

---

#### 📋 Story 4.4: Benchmarks Tab with All Scores
**Estimated Time:** 3 hours
**Prerequisites:** Story 4.3

**Tasks:**

1. **Create BenchmarksTab component** (2 hours)
   ```typescript
   // components/models/tabs/BenchmarksTab.tsx
   export const BenchmarksTab = ({ model }: { model: ModelDetail }) => {
     const benchmarksByCategory = groupBy(model.benchmarkScores, 'category');

     return (
       <div className="space-y-6">
         {Object.entries(benchmarksByCategory).map(([category, benchmarks]) => (
           <Section key={category} title={category}>
             <div className="space-y-3">
               {benchmarks.map(benchmark => (
                 <BenchmarkRow key={benchmark.benchmarkName} benchmark={benchmark} />
               ))}
             </div>
           </Section>
         ))}

         {model.benchmarkScores.length === 0 && (
           <EmptyState message="No benchmark scores available for this model" />
         )}
       </div>
     );
   };

   const BenchmarkRow = ({ benchmark }: { benchmark: BenchmarkScoreDetail }) => (
     <div className="flex items-center justify-between p-3 bg-gray-50 rounded">
       <div className="flex-1">
         <div className="flex items-center gap-2">
           <span className="font-medium">{benchmark.benchmarkName}</span>
           <Tooltip content={benchmark.fullName}>
             <InfoIcon className="w-4 h-4 text-gray-400" />
           </Tooltip>
         </div>
         <p className="text-sm text-gray-500">{benchmark.interpretation}</p>
       </div>

       <div className="text-right">
         <div className="text-lg font-semibold">
           {benchmark.score.toFixed(1)}
           {benchmark.maxScore && `/${benchmark.maxScore}`}
         </div>
         {benchmark.testDate && (
           <p className="text-xs text-gray-500">
             Tested: {formatDate(benchmark.testDate)}
           </p>
         )}
       </div>

       {benchmark.sourceUrl && (
         <a
           href={benchmark.sourceUrl}
           target="_blank"
           rel="noopener noreferrer"
           className="ml-4 text-blue-600 hover:text-blue-800"
         >
           <ExternalLinkIcon className="w-5 h-5" />
         </a>
       )}
     </div>
   );
   ```

2. **Add grouping utility** (30 min)
   ```typescript
   // utils/helpers.ts
   export const groupBy = <T, K extends keyof T>(
     array: T[],
     key: K
   ): Record<string, T[]> => {
     return array.reduce((acc, item) => {
       const group = String(item[key]);
       if (!acc[group]) acc[group] = [];
       acc[group].push(item);
       return acc;
     }, {} as Record<string, T[]>);
   };
   ```

3. **Style and polish** (30 min)
   - Category sections collapsible (optional)
   - Sort benchmarks alphabetically within category

**Acceptance Criteria:**
- [x] Benchmarks tab displays all scores
- [x] Scores organized by category
- [x] Shows: name, score, max_score, interpretation
- [x] Test date and source URL shown if available
- [x] "Not tested" shown for models with no benchmarks
- [x] Scores sorted by category then alphabetically

---

#### 📋 Story 4.5: Pricing Tab with Detailed Breakdown
**Estimated Time:** 2-3 hours
**Prerequisites:** Story 4.4

**Tasks:**

1. **Create PricingTab component** (2 hours)
   ```typescript
   // components/models/tabs/PricingTab.tsx
   export const PricingTab = ({ model }: { model: ModelDetail }) => (
     <div className="space-y-6">
       {/* Price Breakdown Section */}
       <Section title="Price Breakdown">
         <div className="grid grid-cols-2 gap-6">
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
           <div className="p-4 bg-blue-50 rounded-lg">
             <p className="text-lg">
               <strong>{model.priceComparison.comparisonText}</strong>
             </p>
             <p className="text-sm text-gray-600 mt-1">
               Compared to {model.priceComparison.comparedToModel}
             </p>
           </div>
         </Section>
       )}

       {/* Pricing Validity */}
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

       {/* Cost Calculator Placeholder - Story 4.6 */}
       <Section title="Cost Calculator">
         <p className="text-gray-500">Calculator will be added in next story</p>
       </Section>
     </div>
   );

   const PriceCard = ({ title, prices }) => (
     <div className="p-4 border rounded-lg">
       <h4 className="font-semibold mb-3">{title}</h4>
       <div className="space-y-2">
         <PriceRow label="Per 1M tokens" value={prices.perMillion} highlight />
         <PriceRow label="Per 1K tokens" value={prices.perThousand} />
         <PriceRow label="Per token" value={prices.perToken} small />
       </div>
     </div>
   );
   ```

2. **Test all tabs** (1 hour)
   - Verify tab switching works
   - Check data displays correctly in all tabs
   - Test with models that have missing data (optional benchmarks, etc.)

**Acceptance Criteria:**
- [x] Pricing tab displays detailed breakdown
- [x] Shows prices per 1M, 1K, and per token
- [x] Currency displayed
- [x] Pricing validity period shown
- [x] Comparison note vs similar models
- [x] Last updated timestamp visible

---

### Sprint 3: Cost Calculator (Stories 4.6-4.8)

#### 📋 Story 4.6: Cost Calculator Component (Embedded + Standalone)
**Estimated Time:** 5-6 hours
**Prerequisites:** Story 4.5

**Tasks:**

1. **Create calculatorStore** (30 min)
   - See section 3.3 for store implementation
   - Persist state in localStorage

2. **Create cost calculation utilities** (1 hour)
   - Pure functions in utils/costCalculations.ts
   - See section 3.3 for implementation

3. **Create CalculatorInputs component** (1.5 hours)
   ```typescript
   // components/calculator/CalculatorInputs.tsx
   export const CalculatorInputs = () => {
     const { monthlyTokenVolume, inputOutputRatio, setVolume, setRatio } = useCalculatorStore();

     return (
       <div className="space-y-6">
         {/* Volume Input */}
         <div>
           <label className="block text-sm font-medium mb-2">
             Monthly Token Volume
           </label>
           <div className="flex items-center gap-4">
             <Slider
               min={100_000}
               max={50_000_000}
               step={100_000}
               value={monthlyTokenVolume}
               onChange={setVolume}
             />
             <input
               type="number"
               value={monthlyTokenVolume}
               onChange={(e) => setVolume(Number(e.target.value))}
               className="w-32 px-3 py-2 border rounded"
             />
           </div>
           <p className="text-sm text-gray-500 mt-1">
             {(monthlyTokenVolume / 1_000_000).toFixed(1)}M tokens/month
           </p>
         </div>

         {/* Ratio Input */}
         <div>
           <label className="block text-sm font-medium mb-2">
             Input/Output Ratio
           </label>
           <div className="flex items-center gap-4">
             <Slider
               min={0}
               max={100}
               value={inputOutputRatio}
               onChange={setRatio}
             />
             <span className="w-32 text-center">
               {inputOutputRatio}% / {100 - inputOutputRatio}%
             </span>
           </div>
           <p className="text-sm text-gray-500 mt-1">
             Input: {inputOutputRatio}% • Output: {100 - inputOutputRatio}%
           </p>
         </div>
       </div>
     );
   };
   ```

4. **Create CostCalculator component** (2 hours)
   ```typescript
   // components/calculator/CostCalculator.tsx
   export const CostCalculator = ({ modelId }: { modelId?: string }) => {
     const { monthlyTokenVolume, inputOutputRatio } = useCalculatorStore();
     const { data: model } = useModelDetail(modelId);

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

5. **Create CalculatorPage** (1 hour)
   ```typescript
   // pages/CalculatorPage.tsx
   export const CalculatorPage = () => (
     <div className="container mx-auto py-8">
       <h1 className="text-3xl font-bold mb-6">Cost Calculator</h1>

       <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
         <div className="lg:col-span-1">
           <CostCalculator />
         </div>

         <div className="lg:col-span-2">
           {/* Results table - Story 4.7 */}
           <p className="text-gray-500">Results table coming in next story</p>
         </div>
       </div>
     </div>
   );
   ```

6. **Embed in PricingTab** (30 min)
   - Replace placeholder with CostCalculator component

7. **Add route and navigation** (30 min)
   - Add /calculator route to React Router
   - Add link in Header navigation

**Acceptance Criteria:**
- [x] Calculator component with volume and ratio inputs
- [x] Real-time cost calculation
- [x] Displays total cost and breakdown
- [x] Embedded in Pricing tab of modal
- [x] Standalone page at /calculator
- [x] Navigation link in header
- [x] State persists in localStorage

---

#### 📋 Story 4.7: Cost Comparison Table in Calculator
**Estimated Time:** 3 hours
**Prerequisites:** Story 4.6

**Tasks:**

1. **Create CalculatorResults component** (2 hours)
   ```typescript
   // components/calculator/CalculatorResults.tsx
   export const CalculatorResults = () => {
     const { monthlyTokenVolume, inputOutputRatio } = useCalculatorStore();
     const { data: models = [] } = useModels();

     const modelsWithCost = useMemo(
       () => calculateCostForAllModels(models, monthlyTokenVolume, inputOutputRatio),
       [models, monthlyTokenVolume, inputOutputRatio]
     );

     const sortedModels = modelsWithCost.sort(
       (a, b) => a.calculatedCost.totalMonthlyCost - b.calculatedCost.totalMonthlyCost
     );

     const maxCost = sortedModels[sortedModels.length - 1]?.calculatedCost.totalMonthlyCost || 0;

     return (
       <div className="bg-white rounded-lg border">
         <div className="p-4 border-b">
           <h3 className="text-lg font-semibold">Cost Comparison</h3>
           <p className="text-sm text-gray-500">
             Showing all models sorted by monthly cost (lowest first)
           </p>
         </div>

         <div className="overflow-x-auto">
           <table className="w-full">
             <thead className="bg-gray-50">
               <tr>
                 <th className="px-4 py-3 text-left">Model</th>
                 <th className="px-4 py-3 text-left">Provider</th>
                 <th className="px-4 py-3 text-right">Monthly Cost</th>
                 <th className="px-4 py-3 text-right">Input Cost</th>
                 <th className="px-4 py-3 text-right">Output Cost</th>
                 <th className="px-4 py-3 text-right">Savings</th>
               </tr>
             </thead>
             <tbody>
               {sortedModels.map((model, index) => {
                 const savingsPercent =
                   ((maxCost - model.calculatedCost.totalMonthlyCost) / maxCost) * 100;

                 return (
                   <tr key={model.id} className={index === 0 ? 'bg-green-50' : ''}>
                     <td className="px-4 py-3">
                       <div className="flex items-center gap-2">
                         {model.name}
                         {index === 0 && (
                           <Badge variant="success">Cheapest</Badge>
                         )}
                       </div>
                     </td>
                     <td className="px-4 py-3">{model.provider}</td>
                     <td className="px-4 py-3 text-right font-semibold">
                       ${model.calculatedCost.totalMonthlyCost.toFixed(2)}
                     </td>
                     <td className="px-4 py-3 text-right text-sm text-gray-600">
                       ${model.calculatedCost.inputTokensCost.toFixed(2)}
                     </td>
                     <td className="px-4 py-3 text-right text-sm text-gray-600">
                       ${model.calculatedCost.outputTokensCost.toFixed(2)}
                     </td>
                     <td className="px-4 py-3 text-right">
                       {savingsPercent > 0 ? (
                         <span className="text-green-600">
                           {savingsPercent.toFixed(0)}% saved
                         </span>
                       ) : (
                         <span className="text-gray-400">—</span>
                       )}
                     </td>
                   </tr>
                 );
               })}
             </tbody>
           </table>
         </div>
       </div>
     );
   };
   ```

2. **Integrate in CalculatorPage** (30 min)
   - Replace placeholder with CalculatorResults

3. **Add memoization** (30 min)
   - Ensure calculations only run when inputs change
   - Use useMemo for expensive operations

**Acceptance Criteria:**
- [x] Results table shows all models
- [x] Table sorted by cost (cheapest first)
- [x] Columns: name, provider, monthly cost, breakdown
- [x] Savings % vs most expensive shown
- [x] Cheapest model highlighted with badge
- [x] Table updates in real-time with input changes

---

#### 📋 Story 4.8: Preset Workload Scenarios
**Estimated Time:** 2 hours
**Prerequisites:** Story 4.7

**Tasks:**

1. **Create WorkloadPresets component** (1.5 hours)
   ```typescript
   // components/calculator/WorkloadPresets.tsx
   import { WORKLOAD_PRESETS } from '@/utils/workloadPresets';

   export const WorkloadPresets = () => {
     const { activePreset, setPreset } = useCalculatorStore();

     return (
       <div className="space-y-4">
         <h4 className="font-semibold">Workload Presets</h4>

         <div className="grid grid-cols-2 lg:grid-cols-4 gap-3">
           {WORKLOAD_PRESETS.map(preset => (
             <button
               key={preset.id}
               onClick={() => setPreset(preset.id, preset.volume, preset.ratio)}
               className={`p-4 border-2 rounded-lg text-left transition-colors ${
                 activePreset === preset.id
                   ? 'border-blue-500 bg-blue-50'
                   : 'border-gray-200 hover:border-gray-300'
               }`}
             >
               <div className="font-semibold mb-1">{preset.name}</div>
               <p className="text-xs text-gray-600">{preset.description}</p>

               {preset.id !== 'custom' && (
                 <div className="mt-2 text-xs text-gray-500">
                   {(preset.volume / 1_000_000).toFixed(1)}M tokens • {preset.ratio}/{100 - preset.ratio}
                 </div>
               )}
             </button>
           ))}
         </div>
       </div>
     );
   };
   ```

2. **Integrate in Calculator** (30 min)
   - Add WorkloadPresets above CalculatorInputs
   - Verify preset selection updates inputs correctly

**Acceptance Criteria:**
- [x] Preset buttons: Small, Medium, Large, Custom
- [x] Clicking preset sets calculator inputs
- [x] Custom mode allows manual input
- [x] Active preset highlighted
- [x] Tooltip/description explains each preset

---

### Sprint 4: Visualization & Polish (Stories 4.9-4.11)

#### 📋 Story 4.9: Cost Visualization with Chart.js
**Estimated Time:** 3 hours
**Prerequisites:** Story 4.8

**Tasks:**

1. **Create CostChart component** (2 hours)
   - See section 3.4 for full implementation
   - Bar chart with top 10 models by cost

2. **Integrate in CalculatorPage** (30 min)
   - Add below results table
   - Ensure chart updates with input changes

3. **Add interactivity** (30 min)
   - Click bar to highlight row in table
   - Tooltip shows cost breakdown

**Acceptance Criteria:**
- [x] Bar chart shows top 10 models by cost
- [x] X-axis: model names, Y-axis: monthly cost
- [x] Chart updates in real-time with inputs
- [x] Clicking bar highlights table row
- [x] Tooltip shows cost breakdown
- [x] Chart uses Chart.js library

---

#### 📋 Story 4.10: Calculator Sharing & Performance
**Estimated Time:** 3 hours
**Prerequisites:** Story 4.9

**Tasks:**

1. **Implement URL params sync** (1.5 hours)
   ```typescript
   // hooks/useCalculatorParams.ts
   export const useCalculatorParams = () => {
     const [searchParams, setSearchParams] = useSearchParams();
     const { setVolume, setRatio } = useCalculatorStore();

     useEffect(() => {
       const volume = searchParams.get('volume');
       const ratio = searchParams.get('ratio');

       if (volume) setVolume(Number(volume));
       if (ratio) setRatio(Number(ratio));
     }, []);

     const shareUrl = useMemo(() => {
       const url = new URL(window.location.href);
       url.searchParams.set('volume', String(useCalculatorStore.getState().monthlyTokenVolume));
       url.searchParams.set('ratio', String(useCalculatorStore.getState().inputOutputRatio));
       return url.toString();
     }, [useCalculatorStore.getState().monthlyTokenVolume, useCalculatorStore.getState().inputOutputRatio]);

     return { shareUrl };
   };
   ```

2. **Create ShareButton component** (1 hour)
   ```typescript
   // components/calculator/ShareButton.tsx
   export const ShareButton = () => {
     const { shareUrl } = useCalculatorParams();
     const [copied, setCopied] = useState(false);

     const copyToClipboard = async () => {
       await navigator.clipboard.writeText(shareUrl);
       setCopied(true);
       setTimeout(() => setCopied(false), 2000);
     };

     return (
       <button
         onClick={copyToClipboard}
         className="flex items-center gap-2 px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700"
       >
         {copied ? (
           <>
             <CheckIcon className="w-5 h-5" />
             <span>Copied!</span>
           </>
         ) : (
           <>
             <ShareIcon className="w-5 h-5" />
             <span>Share Calculator</span>
           </>
         )}
       </button>
     );
   };
   ```

3. **Performance optimization** (30 min)
   - Debounce slider inputs (100ms)
   - Memoize calculation results
   - Throttle chart updates
   - Test with 100+ models

**Acceptance Criteria:**
- [x] Share button generates URL with volume & ratio
- [x] URL format: /calculator?volume=5000000&ratio=60
- [x] Opening shared URL loads parameters
- [x] Copy to clipboard works
- [x] Success message shown
- [x] Calculations run in <100ms
- [x] No lag when adjusting sliders

---

#### 📋 Story 4.11: Modal Polish & Responsive Design
**Estimated Time:** 2-3 hours
**Prerequisites:** Story 4.10

**Tasks:**

1. **Mobile modal optimization** (1.5 hours)
   - Full-screen modal on mobile (<768px)
   - Scrollable tabs (horizontal overflow)
   - Touch-friendly close button

2. **Accessibility improvements** (1 hour)
   - Focus trap in modal
   - ARIA labels
   - Keyboard navigation (Tab, Escape, Enter)

3. **Visual polish** (30 min)
   - Smooth transitions (modal entrance/exit)
   - Loading skeletons for tabs
   - Empty states for missing data

**Acceptance Criteria:**
- [x] Modal full-screen on mobile
- [x] Tabs scrollable on small screens
- [x] Close button accessible (44px touch target)
- [x] Focus trap prevents tabbing outside modal
- [x] Keyboard navigation works
- [x] Smooth transitions

---

## 5. Architectural Decision Records (ADRs)

### ADR-015: URL-Based Modal State Management

**Context**: Model detail modal needs shareable URLs and browser back button support.

**Decision**: Store modal state in URL search params (`?model={id}`) instead of component state.

**Rationale**:
- **Shareable**: Users can copy URL and share specific model details
- **Browser navigation**: Back button closes modal naturally
- **Bookmarkable**: Users can bookmark model detail pages
- **Deep linking**: External links can open specific models directly

**Implementation**:
```typescript
// URL: /?model=abc-123 → Modal open with model abc-123
// URL: / → Modal closed
```

**Consequences**:
- Slightly more complex than useState (requires React Router)
- Better UX: Users expect back button to work
- SEO-friendly: Crawlers can index model detail "pages"

**Alternatives Considered**:
- Component state (useState): Rejected (no shareable URLs, back button doesn't work)
- Separate routes (/models/{id}): Rejected (would require full page navigation, losing table context)

---

### ADR-016: Cost Calculator State Persistence

**Context**: Users adjust calculator parameters frequently. Losing inputs on page refresh is frustrating.

**Decision**: Persist calculator state (volume, ratio, preset) in localStorage via Zustand middleware.

**Rationale**:
- Users often compare multiple models with same parameters
- Returning users expect their last settings preserved
- localStorage is synchronous and works offline

**Implementation**:
```typescript
export const useCalculatorStore = create<CalculatorState>()(
  persist(
    (set) => ({ /* state */ }),
    { name: 'calculator-state' }
  )
);
```

**Consequences**:
- State survives page refresh
- Works across browser tabs (shared localStorage)
- 5-10KB storage footprint (negligible)

**Migration Path**: If users report issues (e.g., stale data), add version key and invalidate on app update.

---

### ADR-017: Client-Side Cost Calculation

**Context**: Calculator needs instant feedback as users adjust sliders.

**Decision**: Calculate costs client-side using pure functions, not server API.

**Rationale**:
- **Instant**: No network latency (<10ms calculation time)
- **Simple formula**: `(volume * ratio * price)` - no complex logic requiring server
- **Scalability**: No API calls = no server load for millions of calculations
- **Offline**: Works even if API is down

**Implementation**:
```typescript
export const calculateMonthlyCost = (input: CostCalculationInput): CostCalculationResult => {
  // Pure function, no side effects, 100% deterministic
};
```

**Consequences**:
- Frontend bundles calculation logic (~1KB)
- Formula changes require frontend deployment (acceptable for MVP)
- No server-side analytics on calculator usage (can add later via events)

**Alternatives Considered**:
- Server-side calculation: Rejected (unnecessary latency, server load)
- Web Worker: Rejected (overkill for simple math)

---

### ADR-018: Dual Calculator Context (Embedded vs Standalone)

**Context**: Calculator needs to work in two places:
1. Embedded in model detail modal (single model)
2. Standalone page (all models)

**Decision**: Build single `CostCalculator` component with optional `modelId` prop.

**Rationale**:
- **DRY**: One implementation, two contexts
- **Consistent UX**: Same inputs/UI in both places
- **State sharing**: Zustand store works in both contexts

**Implementation**:
```typescript
// Embedded: <CostCalculator modelId="abc-123" />
// Standalone: <CostCalculator /> (no modelId = calculate for all)
```

**Consequences**:
- Component slightly more complex (conditional rendering)
- Better maintainability (single source of truth)
- State syncs between embedded and standalone

---

### ADR-019: Chart.js for Cost Visualization

**Context**: Need interactive cost comparison chart.

**Decision**: Use Chart.js (already installed from Epic 3) instead of alternatives.

**Rationale**:
- **Already installed**: Zero additional bundle size
- **Simple API**: Bar chart in <50 lines of code
- **Interactive**: Built-in tooltips, hover effects
- **Performant**: Canvas-based rendering handles 100+ bars

**Alternatives Considered**:
- D3.js: Rejected (70KB+ bundle, steeper learning curve)
- Recharts: Rejected (React-specific, larger bundle than Chart.js)
- Canvas from scratch: Rejected (reinventing the wheel)

**Consequences**:
- Chart.js conventions (not React-native patterns)
- Limited customization vs D3 (acceptable for bar charts)
- Good documentation and community support

---

## 6. Pre-Implementation Checklist

Before starting Story 4.1:

### Backend Prerequisites

- [x] **Epic 3 Backend Complete**
  - [x] GET /api/models endpoint working
  - [x] Domain entities created (Model, Capability, Benchmark)
  - [x] Seed data loaded

- [ ] **Story 4.2 Ready to Start**
  - [ ] Plan ModelDetailDto structure
  - [ ] Design price comparison algorithm
  - [ ] Confirm Redis caching strategy (30 min TTL)

### Frontend Prerequisites

- [x] **Epic 3 Frontend Complete**
  - [x] ModelsTable with clickable model names
  - [x] useModels hook functional
  - [x] Header component with navigation
  - [x] Chart.js installed

- [ ] **Modal Infrastructure**
  - [ ] Plan modal component API (props, variants)
  - [ ] Design tab navigation pattern
  - [ ] Confirm URL-based state management approach

- [ ] **Calculator Planning**
  - [ ] Review calculation formulas
  - [ ] Design calculator store schema
  - [ ] Plan preset configurations

### Infrastructure

- [x] **Development Environment**
  - [x] Backend running at localhost:5000
  - [x] Frontend running at localhost:5173
  - [x] Redis accessible

- [ ] **Testing Setup**
  - [ ] Modal testing strategy (React Testing Library)
  - [ ] Calculator unit tests planned
  - [ ] Chart.js mock for tests

---

## 7. Success Metrics for Epic 4

Upon completing all 11 stories:

### Functional Metrics

- ✅ **Model Detail Modal**:
  - Users can click any model to see full details
  - 3 tabs functional: Overview, Benchmarks, Pricing
  - URL updates with model ID (shareable)
  - Browser back button closes modal

- ✅ **Cost Calculator**:
  - Embedded calculator in Pricing tab
  - Standalone page at /calculator
  - 4 workload presets functional
  - Real-time cost calculation for all models
  - Cost visualization chart

- ✅ **Sharing**:
  - Calculator URLs shareable with parameters
  - Copy to clipboard works

### Technical Metrics

- ✅ **Performance**:
  - Modal opens in <500ms
  - Calculator responds to inputs in <100ms
  - Chart renders in <1 second (100 models)
  - GET /api/models/{id} cached (30 min TTL)

- ✅ **Code Quality**:
  - Zero TypeScript `any` types
  - Pure functions for calculations (testable)
  - Memoization prevents unnecessary re-renders

- ✅ **Bundle Size**:
  - Modal + Calculator: <50KB gzipped
  - Chart.js already counted in Epic 3 budget

### UX Metrics

- ✅ **Usability**:
  - Modal dismissable 3 ways (close button, backdrop, Escape)
  - Calculator inputs responsive (slider + number input)
  - Preset buttons provide quick starts

- ✅ **Responsiveness**:
  - Modal full-screen on mobile
  - Calculator layout adapts to tablet/mobile
  - Charts readable on small screens

- ✅ **Accessibility**:
  - Focus trap in modal
  - Keyboard navigation works
  - ARIA labels on interactive elements

### User Journey Validation

✅ **Journey 2 Replica** (from PRD): A developer can:
1. Open platform
2. Click model (e.g., "Claude Haiku")
3. See complete specifications in modal
4. Switch to Pricing tab
5. Use embedded calculator: Set 2M tokens/month, 70/30 ratio
6. See estimated cost: $140/month
7. Navigate to standalone calculator
8. Compare costs for all models with same parameters
9. Identify cheapest option
10. Share calculator URL with team

---

## 8. Risk Mitigation

### Identified Risks

**Risk 1: Modal Performance with Heavy Content**
- **Impact**: Modal lags when loading model with 20+ benchmarks
- **Mitigation**:
  - Lazy load tab content (render only active tab)
  - Virtualize benchmark list if >50 scores
  - Cache API responses aggressively (30 min TTL)

**Risk 2: Calculator Calculation Drift**
- **Impact**: Client-side calculation differs from server (if server calculates later)
- **Mitigation**:
  - Use same formula on client and server (shared logic via tests)
  - Document calculation formula in both places
  - Add server-side validation endpoint for reconciliation (Epic 6+)

**Risk 3: Chart.js Version Incompatibility**
- **Impact**: Chart.js update breaks visualization
- **Mitigation**:
  - Pin Chart.js version in package.json
  - Test chart rendering in CI/CD
  - Document Chart.js config for future maintainers

**Risk 4: URL Param Pollution**
- **Impact**: URL becomes messy with multiple params (?model=x&volume=y&ratio=z)
- **Mitigation**:
  - Use semantic param names (clear, short)
  - Limit to 2-3 params max
  - Consider base64 encoding if >5 params needed (Epic 5+)

---

## 9. Next Steps

### Immediate Actions (Before Story 4.1)

1. **Backend Team**:
   - [ ] Review ModelDetailDto structure (section 3.2)
   - [ ] Plan price comparison algorithm
   - [ ] Prepare for Story 4.2 implementation

2. **Frontend Team**:
   - [ ] Review modal architecture (section 3.3)
   - [ ] Study URL-based state pattern (ADR-015)
   - [ ] Prepare Modal and Tabs UI components

3. **Design Review** (Optional):
   - [ ] Review modal tabs layout
   - [ ] Approve calculator UI mockups
   - [ ] Confirm chart visualization style

### Story Execution Process

For each story:
1. **Pre-Story Review**:
   - Read story acceptance criteria
   - Verify prerequisites complete
   - Assign developer(s)

2. **During Story**:
   - Follow task checklist in this document
   - Write tests alongside code
   - Commit frequently with clear messages

3. **Post-Story**:
   - Demo to team (show AC met)
   - Code review (PR required)
   - Merge to main
   - Update story documentation

### Epic 4 Completion

After Story 4.11:
- [ ] Full epic demo to stakeholders
- [ ] Performance testing (100+ models)
- [ ] User acceptance testing
- [ ] Create `tech-spec-epic-4.md` (technical deep-dive)
- [ ] Document lessons learned for Epic 5

---

## 10. Appendix

### A. Example API Response

**GET /api/models/abc-123**

```json
{
  "data": {
    "id": "abc-123",
    "name": "Claude 3.5 Sonnet",
    "provider": "Anthropic",
    "version": "claude-3-5-sonnet-20241022",
    "releaseDate": "2024-10-22T00:00:00Z",
    "status": "active",
    "inputPricePerMillion": 3.00,
    "outputPricePerMillion": 15.00,
    "inputPricePerThousand": 0.003,
    "outputPricePerThousand": 0.015,
    "inputPricePerToken": 0.000003,
    "outputPricePerToken": 0.000015,
    "currency": "USD",
    "pricingValidFrom": "2024-10-22T00:00:00Z",
    "pricingValidTo": null,
    "capabilities": {
      "contextWindow": 200000,
      "maxOutputTokens": 4096,
      "supportsFunctionCalling": true,
      "supportsVision": true,
      "supportsAudioInput": false,
      "supportsAudioOutput": false,
      "supportsStreaming": true,
      "supportsJsonMode": false
    },
    "benchmarkScores": [
      {
        "benchmarkName": "MMLU",
        "fullName": "Massive Multitask Language Understanding",
        "category": "Reasoning",
        "score": 88.7,
        "maxScore": 100,
        "interpretation": "Higher is better",
        "testDate": "2024-10-22T00:00:00Z",
        "sourceUrl": "https://example.com/benchmarks"
      },
      // ... more benchmarks
    ],
    "priceComparison": {
      "comparisonText": "15% cheaper than GPT-4 Turbo",
      "percentageDifference": -15.0,
      "comparedToModel": "GPT-4 Turbo"
    },
    "updatedAt": "2024-10-22T12:00:00Z",
    "createdAt": "2024-10-22T00:00:00Z"
  },
  "meta": {
    "cached": false,
    "timestamp": "2024-10-22T12:00:00Z"
  }
}
```

### B. TypeScript Interfaces

```typescript
// types/modelDetail.ts

export interface ModelDetail {
  id: string;
  name: string;
  provider: string;
  version?: string;
  releaseDate?: string;
  status: 'active' | 'deprecated';

  // Pricing
  inputPricePerMillion: number;
  outputPricePerMillion: number;
  inputPricePerThousand: number;
  outputPricePerThousand: number;
  inputPricePerToken: number;
  outputPricePerToken: number;
  currency: string;
  pricingValidFrom?: string;
  pricingValidTo?: string;

  capabilities: Capabilities;
  benchmarkScores: BenchmarkScoreDetail[];
  priceComparison?: PriceComparison;

  updatedAt: string;
  createdAt: string;
}

export interface BenchmarkScoreDetail {
  benchmarkName: string;
  fullName: string;
  category: string;
  score: number;
  maxScore?: number;
  interpretation: string;
  testDate?: string;
  sourceUrl?: string;
}

export interface PriceComparison {
  comparisonText: string;
  percentageDifference: number;
  comparedToModel: string;
}
```

### C. Component File Checklist

**Files to Create**:

```
✅ components/ui/Modal.tsx
✅ components/ui/Tabs.tsx
✅ components/models/ModelDetailModal.tsx
✅ components/models/tabs/OverviewTab.tsx
✅ components/models/tabs/BenchmarksTab.tsx
✅ components/models/tabs/PricingTab.tsx
✅ components/calculator/CostCalculator.tsx
✅ components/calculator/CalculatorInputs.tsx
✅ components/calculator/CalculatorResults.tsx
✅ components/calculator/WorkloadPresets.tsx
✅ components/calculator/CostChart.tsx
✅ components/calculator/ShareButton.tsx
✅ pages/CalculatorPage.tsx
✅ store/calculatorStore.ts
✅ hooks/useModalState.ts
✅ hooks/useModelDetail.ts
✅ hooks/useCostCalculation.ts
✅ hooks/useCalculatorParams.ts
✅ utils/costCalculations.ts
✅ utils/workloadPresets.ts
```

### D. Useful Commands

```bash
# Run backend
cd services/backend
dotnet run --project LlmTokenPrice.API

# Run frontend
cd apps/web
pnpm run dev

# Test API endpoint
curl http://localhost:5000/api/models/{model-id}

# Type check
pnpm run type-check

# Lint
pnpm run lint

# Build
pnpm run build
```

---

**Document Version:** 1.0
**Last Updated:** 2025-10-16
**Maintained By:** Pablo (with Claude Code assistance)

---

_This document serves as the definitive implementation guide for Epic 4. Refer to this as the single source of truth during development._
