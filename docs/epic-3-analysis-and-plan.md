# Epic 3: Public Comparison Table Interface
## Analysis & Implementation Plan

**Author:** Pablo (with Claude Code Analysis)
**Date:** 2025-10-16
**Status:** Ready for Implementation
**Epic Goal:** Deliver core public-facing comparison table with sorting, filtering, search, and model selection capabilities

---

## Executive Summary

This document provides a comprehensive analysis of Epic 3 scope, refined requirements based on current project state, and a detailed step-by-step implementation plan. After analyzing your codebase, all critical dependencies are in place, enabling focus on pure UI/UX implementation.

### Key Findings

✅ **Foundation Ready**: TanStack Table v8, TanStack Query v5, Zustand v5, Chart.js v4 installed
✅ **Path Aliases Configured**: `@/`, `@components/`, `@api/`, `@store/` ready
✅ **Backend Architecture**: Hexagonal structure established (Domain, Application, Infrastructure, API)
⚠️ **Pending**: Frontend folder structure, `/api/models` endpoint, domain entities implementation

### Refined Scope

**Original**: 12-15 stories
**Refined**: 12 stories (consolidated for efficiency)
**Estimated Effort**: 35-42 hours (4-5 days full-time development)

---

## 1. Current Project State Analysis

### ✅ Completed (Epic 1 & 2)

**Backend Infrastructure:**
- Hexagonal architecture with 4 layers (Domain, Application, Infrastructure, API)
- AppDbContext configured for PostgreSQL + TimescaleDB
- Health check endpoint `/api/health` functional
- DI container configured in Program.cs

**Frontend Infrastructure:**
- Vite 7 (Rolldown) + React 19 + TypeScript 5.9
- TailwindCSS 4 configured
- All critical dependencies installed:
  - `@tanstack/react-table` v8.21.3
  - `@tanstack/react-query` v5.90.5
  - `zustand` v5.0.8
  - `chart.js` v4.5.1
  - `react-router-dom` v7.9.4
  - `axios` v1.12.2
- Path aliases configured in vite.config.ts
- API proxy `/api` → `localhost:5000` configured

**Documentation:**
- 22 story files documented (Epic 1: 1.1-1.10, Epic 2: 2.1-2.12)
- PRD.md and epics.md complete

### ⚠️ Pending for Epic 3

**Backend:**
- [ ] Domain entities (Model, Capability, Benchmark, BenchmarkScore)
- [ ] Entity configurations in AppDbContext
- [ ] Database migrations for schema
- [ ] Seed data script with 10+ sample models
- [ ] `GET /api/models` endpoint
- [ ] DTOs: ModelDto, CapabilitiesDto, BenchmarkScoreDto

**Frontend:**
- [ ] Folder structure (`/components`, `/pages`, `/api`, `/store`, `/hooks`, `/types`)
- [ ] Axios client configuration
- [ ] React Router routes setup
- [ ] TailwindCSS design system (colors, spacing)

---

## 2. Refined Epic 3 Scope

### Original Stories (from epics.md)

1. **Story 3.1**: Public Homepage with Basic Layout
2. **Story 3.2**: Fetch and Display Models in Basic Table
3. **Story 3.3**: Integrate TanStack Table for Advanced Features
4. **Story 3.4**: Implement Column Sorting
5. **Story 3.5**: Add Provider Filter
6. **Story 3.6**: Add Capabilities Filters
7. **Story 3.7**: Add Price Range Filter
8. **Story 3.8**: Implement Search Functionality
9. **Story 3.9**: Display Benchmark Scores in Table
10. **Story 3.10**: Add Checkbox Selection for Models
11. **Story 3.11**: Create Comparison Basket UI
12. **Story 3.12**: Implement Table Pagination or Virtual Scrolling
13. **Story 3.13**: Style and Polish Table Interface
14. **Story 3.14**: Add Context Window and Capabilities Icons
15. **Story 3.15**: Optimize API Response and Caching

### 🔄 Scope Refinement Decisions

**Consolidated Stories:**

- **3.12 + 3.13 + 3.15** → **New Story 3.12**: "Pagination & Caching"
  - Rationale: Avoid "polish as separate story" anti-pattern. Styling should be incremental in each story.
  - Combine pagination, API caching, and final optimizations into single story

**Deferred to Epic 5:**

- **Story 3.14** (Capabilities Icons) → **Epic 5: Multi-Model Comparison**
  - Rationale: Icons provide more value in comparison context. Better UX to introduce visual indicators alongside charts.

**Final Epic 3 Scope: 12 Stories**

1. Story 3.1: Public Homepage Layout
2. Story 3.2: Basic Models Table (fetch + display)
3. Story 3.3: TanStack Table Integration
4. Story 3.4: Column Sorting
5. Story 3.5: Provider Filter
6. Story 3.6: Capabilities Filters
7. Story 3.7: Price Range Filter
8. Story 3.8: Search Functionality
9. Story 3.9: Benchmark Scores Display
10. Story 3.10: Checkbox Selection
11. Story 3.11: Comparison Basket UI
12. Story 3.12: Pagination & Caching _(consolidated)_

---

## 3. Technical Specification

### 3.1 Frontend Architecture

```
apps/web/src/
├── components/
│   ├── layout/
│   │   ├── PublicLayout.tsx         # Main layout wrapper (Story 3.1)
│   │   ├── Header.tsx               # Navigation header (Story 3.1)
│   │   └── Footer.tsx               # Footer with links (Story 3.1)
│   ├── models/
│   │   ├── ModelsTable.tsx          # Main table component (Story 3.2-3.4)
│   │   ├── ModelRow.tsx             # Table row component (Story 3.2)
│   │   ├── FilterSidebar.tsx        # Filter container (Story 3.5)
│   │   ├── ProviderFilter.tsx       # Provider checkbox filter (Story 3.5)
│   │   ├── CapabilitiesFilter.tsx   # Capabilities checkboxes (Story 3.6)
│   │   ├── PriceRangeFilter.tsx     # Dual slider filter (Story 3.7)
│   │   ├── SearchBar.tsx            # Search input (Story 3.8)
│   │   └── ComparisonBasket.tsx     # Selection basket (Story 3.11)
│   └── ui/
│       ├── Button.tsx               # Reusable button
│       ├── Checkbox.tsx             # Reusable checkbox (Story 3.10)
│       ├── Input.tsx                # Reusable input
│       └── Slider.tsx               # Range slider (Story 3.7)
├── pages/
│   ├── HomePage.tsx                 # Main public page (Story 3.1)
│   └── ComparePage.tsx              # Comparison page (Epic 4 prep)
├── api/
│   ├── client.ts                    # Axios instance config
│   └── models.ts                    # Models API functions
├── store/
│   ├── comparisonStore.ts           # Zustand: model selection state
│   └── filterStore.ts               # Zustand: filter state
├── hooks/
│   ├── useModels.ts                 # TanStack Query: fetch models
│   ├── useFilters.ts                # Filter logic hook
│   └── useSelection.ts              # Selection logic hook
├── types/
│   ├── model.ts                     # ModelDto TypeScript interface
│   └── filter.ts                    # Filter-related types
└── utils/
    ├── formatters.ts                # Price, date formatting
    └── sorting.ts                   # Sort helper functions
```

### 3.2 Backend Requirements

#### New Endpoints

**GET /api/models**

```csharp
// Location: services/backend/LlmTokenPrice.API/Controllers/ModelsController.cs

[ApiController]
[Route("api/[controller]")]
public class ModelsController : ControllerBase
{
    private readonly IModelService _modelService;

    [HttpGet]
    [ResponseCache(Duration = 3600)] // 1 hour cache
    public async Task<ActionResult<ApiResponse<List<ModelDto>>>> GetModels()
    {
        var models = await _modelService.GetAllActiveModelsAsync();
        return Ok(new ApiResponse<List<ModelDto>>
        {
            Data = models,
            Meta = new { Cached = false, Timestamp = DateTime.UtcNow }
        });
    }
}
```

#### DTOs Required

```csharp
// Location: services/backend/LlmTokenPrice.Application/DTOs/ModelDto.cs

public record ModelDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Provider { get; init; } = string.Empty;
    public string? Version { get; init; }
    public decimal InputPricePerMillion { get; init; }
    public decimal OutputPricePerMillion { get; init; }
    public string Currency { get; init; } = "USD";
    public CapabilitiesDto Capabilities { get; init; } = null!;
    public List<BenchmarkScoreDto> TopBenchmarks { get; init; } = new();
    public DateTime UpdatedAt { get; init; }
    public string Status { get; init; } = "active";
}

public record CapabilitiesDto
{
    public int ContextWindow { get; init; }
    public int MaxOutputTokens { get; init; }
    public bool SupportsFunctionCalling { get; init; }
    public bool SupportsVision { get; init; }
    public bool SupportsAudioInput { get; init; }
    public bool SupportsAudioOutput { get; init; }
    public bool SupportsStreaming { get; init; }
    public bool SupportsJsonMode { get; init; }
}

public record BenchmarkScoreDto
{
    public string BenchmarkName { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public decimal Score { get; init; }
    public decimal? MaxScore { get; init; }
}

// Generic API response wrapper
public record ApiResponse<T>
{
    public T Data { get; init; } = default!;
    public object? Meta { get; init; }
}
```

#### Domain Entities

```csharp
// Location: services/backend/LlmTokenPrice.Domain/Entities/Model.cs

public class Model
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public string? Version { get; set; }
    public DateTime? ReleaseDate { get; set; }
    public string Status { get; set; } = "active";

    // Pricing
    public decimal InputPricePerMillion { get; set; }
    public decimal OutputPricePerMillion { get; set; }
    public string Currency { get; set; } = "USD";
    public DateTime? PricingValidFrom { get; set; }
    public DateTime? PricingValidTo { get; set; }

    // Metadata
    public DateTime? LastScrapedAt { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public ModelCapability? Capability { get; set; }
    public List<ModelBenchmarkScore> BenchmarkScores { get; set; } = new();
}

// Similar for ModelCapability, Benchmark, ModelBenchmarkScore...
```

### 3.3 State Management Strategy

#### Server State (TanStack Query)

```typescript
// hooks/useModels.ts
import { useQuery } from '@tanstack/react-query';
import { fetchModels } from '@api/models';

export const useModels = () => {
  return useQuery({
    queryKey: ['models'],
    queryFn: fetchModels,
    staleTime: 5 * 60 * 1000, // 5 minutes
    gcTime: 10 * 60 * 1000,   // 10 minutes
  });
};
```

#### Client State - Filters (Zustand)

```typescript
// store/filterStore.ts
import { create } from 'zustand';

interface FilterState {
  providers: string[];
  capabilities: string[];
  priceRange: [number, number];
  searchQuery: string;

  setProviders: (providers: string[]) => void;
  setCapabilities: (capabilities: string[]) => void;
  setPriceRange: (range: [number, number]) => void;
  setSearchQuery: (query: string) => void;
  clearFilters: () => void;
}

export const useFilterStore = create<FilterState>((set) => ({
  providers: [],
  capabilities: [],
  priceRange: [0, 1000],
  searchQuery: '',

  setProviders: (providers) => set({ providers }),
  setCapabilities: (capabilities) => set({ capabilities }),
  setPriceRange: (priceRange) => set({ priceRange }),
  setSearchQuery: (searchQuery) => set({ searchQuery }),
  clearFilters: () => set({
    providers: [],
    capabilities: [],
    priceRange: [0, 1000],
    searchQuery: '',
  }),
}));
```

#### Client State - Selection (Zustand)

```typescript
// store/comparisonStore.ts
import { create } from 'zustand';

interface ComparisonState {
  selectedModelIds: string[];
  addModel: (id: string) => void;
  removeModel: (id: string) => void;
  clearSelection: () => void;
  canAddMore: () => boolean;
}

export const useComparisonStore = create<ComparisonState>((set, get) => ({
  selectedModelIds: [],

  addModel: (id) => {
    if (get().selectedModelIds.length < 5) {
      set((state) => ({
        selectedModelIds: [...state.selectedModelIds, id]
      }));
    }
  },

  removeModel: (id) => set((state) => ({
    selectedModelIds: state.selectedModelIds.filter(m => m !== id)
  })),

  clearSelection: () => set({ selectedModelIds: [] }),

  canAddMore: () => get().selectedModelIds.length < 5,
}));
```

#### Table State (TanStack Table)

```typescript
// components/models/ModelsTable.tsx
import { useReactTable, getCoreRowModel, getSortedRowModel, getFilteredRowModel } from '@tanstack/react-table';

const table = useReactTable({
  data: models,
  columns,
  getCoreRowModel: getCoreRowModel(),
  getSortedRowModel: getSortedRowModel(),
  getFilteredRowModel: getFilteredRowModel(),
  state: {
    sorting,
    globalFilter: searchQuery,
  },
  onSortingChange: setSorting,
  onGlobalFilterChange: setSearchQuery,
});
```

### 3.4 Filtering Strategy

**Decision: Client-Side Filtering**

**Rationale:**
- Dataset size: 50-100 models (fits in memory)
- Instant filtering response (no network latency)
- Better UX for interactive filtering
- TanStack Table handles filtering efficiently

**Implementation:**

```typescript
// hooks/useFilters.ts
import { useFilterStore } from '@store/filterStore';

export const useFilters = (models: Model[]) => {
  const { providers, capabilities, priceRange, searchQuery } = useFilterStore();

  return useMemo(() => {
    return models.filter(model => {
      // Provider filter
      if (providers.length > 0 && !providers.includes(model.provider)) {
        return false;
      }

      // Capabilities filter (AND logic)
      if (capabilities.length > 0) {
        const hasAllCapabilities = capabilities.every(cap =>
          model.capabilities[cap] === true
        );
        if (!hasAllCapabilities) return false;
      }

      // Price range filter
      const totalPrice = model.inputPricePerMillion + model.outputPricePerMillion;
      if (totalPrice < priceRange[0] || totalPrice > priceRange[1]) {
        return false;
      }

      // Search query
      if (searchQuery) {
        const searchLower = searchQuery.toLowerCase();
        const matchesSearch =
          model.name.toLowerCase().includes(searchLower) ||
          model.provider.toLowerCase().includes(searchLower);
        if (!matchesSearch) return false;
      }

      return true;
    });
  }, [models, providers, capabilities, priceRange, searchQuery]);
};
```

---

## 4. Implementation Plan - Step by Step

### Sprint 1: Foundation (Stories 3.1-3.3)

#### 📋 Story 3.1: Public Homepage Layout
**Estimated Time:** 2-3 hours
**Prerequisites:** None

**Tasks:**

1. **Create frontend folder structure** (30 min)
   ```bash
   mkdir -p src/{components/{layout,models,ui},pages,api,store,hooks,types,utils}
   ```

2. **Implement PublicLayout component** (45 min)
   - File: `src/components/layout/PublicLayout.tsx`
   - Responsibilities: Main container, header, content area, footer
   - TailwindCSS: Flexbox layout, sticky header

3. **Implement Header component** (30 min)
   - File: `src/components/layout/Header.tsx`
   - Content: Platform logo/name, navigation placeholder, search bar placeholder
   - Responsive: Mobile menu button (implementation in Epic 8)

4. **Implement Footer component** (15 min)
   - File: `src/components/layout/Footer.tsx`
   - Content: About, Contact links (placeholders), copyright

5. **Setup React Router** (30 min)
   - Configure routes in `App.tsx`
   - Route `/` → `HomePage` wrapped in `PublicLayout`
   - Route `/compare` → Placeholder (Epic 4)

6. **Apply base styling** (30 min)
   - TailwindCSS config: Define design tokens (colors, spacing)
   - Global styles in `index.css`

**Acceptance Criteria:**
- [ ] Homepage accessible at `localhost:5173/`
- [ ] Header, main content area, footer visible
- [ ] Responsive layout (desktop, tablet tested)
- [ ] Navigation placeholder ready for Story 3.8

---

#### 📋 Story 3.2: Basic Models Table
**Estimated Time:** 3-4 hours
**Prerequisites:** Backend `/api/models` endpoint ready

**Backend Tasks (if not done):**

1. **Create Domain Entities** (1 hour)
   - `Model.cs`, `ModelCapability.cs`, `Benchmark.cs`, `ModelBenchmarkScore.cs`
   - Configure in `AppDbContext.cs` (DbSets, relationships)
   - Generate migration: `dotnet ef migrations add AddModelEntities`
   - Apply: `dotnet ef database update`

2. **Create DTOs** (30 min)
   - `ModelDto.cs`, `CapabilitiesDto.cs`, `BenchmarkScoreDto.cs`
   - AutoMapper profiles (or manual mapping)

3. **Implement GET /api/models** (45 min)
   - `ModelsController.cs`
   - Service layer: `IModelService`, `ModelService`
   - Return top 3-5 benchmarks per model

4. **Seed Data** (30 min)
   - Create `DbInitializer.cs` with 10+ sample models
   - Include: GPT-4, Claude 3, Gemini, Llama 3, Mistral, etc.
   - Run seed on app startup (development only)

**Frontend Tasks:**

5. **Configure Axios client** (15 min)
   ```typescript
   // src/api/client.ts
   import axios from 'axios';

   export const apiClient = axios.create({
     baseURL: '/api',
     headers: { 'Content-Type': 'application/json' },
   });
   ```

6. **Create models API functions** (15 min)
   ```typescript
   // src/api/models.ts
   import { apiClient } from './client';
   import { Model } from '@/types/model';

   export const fetchModels = async (): Promise<Model[]> => {
     const { data } = await apiClient.get<ApiResponse<Model[]>>('/models');
     return data.data;
   };
   ```

7. **Define TypeScript types** (20 min)
   ```typescript
   // src/types/model.ts
   export interface Model {
     id: string;
     name: string;
     provider: string;
     inputPricePerMillion: number;
     outputPricePerMillion: number;
     currency: string;
     capabilities: Capabilities;
     topBenchmarks: BenchmarkScore[];
     updatedAt: string;
   }
   // ... rest of types
   ```

8. **Create useModels hook** (20 min)
   ```typescript
   // src/hooks/useModels.ts
   import { useQuery } from '@tanstack/react-query';
   import { fetchModels } from '@api/models';

   export const useModels = () => {
     return useQuery({
       queryKey: ['models'],
       queryFn: fetchModels,
       staleTime: 5 * 60 * 1000,
     });
   };
   ```

9. **Create ModelsTable component** (1 hour)
   - File: `src/components/models/ModelsTable.tsx`
   - Fetch data with `useModels`
   - Render basic HTML table
   - Columns: Name, Provider, Input Price, Output Price
   - Loading spinner, error message

10. **Integrate in HomePage** (15 min)
    ```typescript
    // src/pages/HomePage.tsx
    import { ModelsTable } from '@components/models/ModelsTable';

    export const HomePage = () => (
      <div className="container mx-auto py-8">
        <h1 className="text-3xl font-bold mb-6">LLM Model Comparison</h1>
        <ModelsTable />
      </div>
    );
    ```

**Acceptance Criteria:**
- [ ] GET /api/models returns 10+ models
- [ ] Table displays models with name, provider, pricing
- [ ] Loading state shown while fetching
- [ ] Error message if API fails
- [ ] Data refreshes on page reload

---

#### 📋 Story 3.3: TanStack Table Integration
**Estimated Time:** 4-5 hours
**Prerequisites:** Story 3.2 complete

**Tasks:**

1. **Define column definitions** (1 hour)
   ```typescript
   // src/components/models/columns.tsx
   import { ColumnDef } from '@tanstack/react-table';
   import { Model } from '@/types/model';

   export const columns: ColumnDef<Model>[] = [
     {
       accessorKey: 'name',
       header: 'Model Name',
       cell: ({ row }) => <span className="font-medium">{row.original.name}</span>,
     },
     {
       accessorKey: 'provider',
       header: 'Provider',
     },
     {
       accessorKey: 'inputPricePerMillion',
       header: 'Input Price',
       cell: ({ row }) => `$${row.original.inputPricePerMillion.toFixed(2)}`,
     },
     {
       accessorKey: 'outputPricePerMillion',
       header: 'Output Price',
       cell: ({ row }) => `$${row.original.outputPricePerMillion.toFixed(2)}`,
     },
     // More columns...
   ];
   ```

2. **Migrate to TanStack Table** (2 hours)
   ```typescript
   // src/components/models/ModelsTable.tsx
   import { useReactTable, getCoreRowModel, flexRender } from '@tanstack/react-table';
   import { columns } from './columns';

   export const ModelsTable = () => {
     const { data: models = [], isLoading, error } = useModels();

     const table = useReactTable({
       data: models,
       columns,
       getCoreRowModel: getCoreRowModel(),
     });

     if (isLoading) return <Spinner />;
     if (error) return <ErrorMessage />;

     return (
       <table className="w-full">
         <thead>
           {table.getHeaderGroups().map(headerGroup => (
             <tr key={headerGroup.id}>
               {headerGroup.headers.map(header => (
                 <th key={header.id}>
                   {flexRender(header.column.columnDef.header, header.getContext())}
                 </th>
               ))}
             </tr>
           ))}
         </thead>
         <tbody>
           {table.getRowModel().rows.map(row => (
             <tr key={row.id}>
               {row.getVisibleCells().map(cell => (
                 <td key={cell.id}>
                   {flexRender(cell.column.columnDef.cell, cell.getContext())}
                 </td>
               ))}
             </tr>
           ))}
         </tbody>
       </table>
     );
   };
   ```

3. **Add loading states** (30 min)
   - Skeleton loader component
   - Shimmer effect with TailwindCSS

4. **Error handling** (30 min)
   - Error boundary component
   - Retry button on error

5. **Performance testing** (1 hour)
   - Generate 50+ models in seed data
   - Verify render time < 1 second
   - Check React DevTools for unnecessary re-renders

**Acceptance Criteria:**
- [ ] TanStack Table renders all models
- [ ] Columns properly formatted (currency, decimals)
- [ ] Loading skeleton shown during fetch
- [ ] Error handled gracefully with retry
- [ ] Performance verified with 50+ models

---

### Sprint 2: Filters (Stories 3.4-3.7)

#### 📋 Story 3.4: Column Sorting
**Estimated Time:** 2 hours

**Tasks:**

1. **Enable sorting in table** (30 min)
   ```typescript
   const [sorting, setSorting] = useState<SortingState>([]);

   const table = useReactTable({
     // ...
     state: { sorting },
     onSortingChange: setSorting,
     getSortedRowModel: getSortedRowModel(),
   });
   ```

2. **Add sort indicators** (45 min)
   - Up/down arrow icons (lucide-react)
   - Show indicator on active sort column
   - Toggle ascending/descending on click

3. **Style sortable headers** (30 min)
   - Cursor pointer
   - Hover effect
   - Active state styling

4. **Persist sort state** (15 min)
   - Use URL search params: `?sort=price&order=asc`
   - React Router `useSearchParams`

**Acceptance Criteria:**
- [ ] Clicking column header sorts data
- [ ] Arrow indicator shows sort direction
- [ ] Sorting works for: name, provider, input price, output price
- [ ] Sort state persists in URL

---

#### 📋 Story 3.5: Provider Filter
**Estimated Time:** 3 hours

**Tasks:**

1. **Create FilterSidebar component** (1 hour)
   - File: `src/components/models/FilterSidebar.tsx`
   - Layout: Sticky sidebar on left (desktop)
   - Collapsible sections

2. **Create ProviderFilter component** (1 hour)
   ```typescript
   // src/components/models/ProviderFilter.tsx
   import { useFilterStore } from '@store/filterStore';

   export const ProviderFilter = ({ providers }: { providers: string[] }) => {
     const { providers: selected, setProviders } = useFilterStore();

     const toggleProvider = (provider: string) => {
       if (selected.includes(provider)) {
         setProviders(selected.filter(p => p !== provider));
       } else {
         setProviders([...selected, provider]);
       }
     };

     return (
       <div className="space-y-2">
         <h3 className="font-semibold">Provider</h3>
         {providers.map(provider => (
           <Checkbox
             key={provider}
             label={provider}
             checked={selected.includes(provider)}
             onChange={() => toggleProvider(provider)}
           />
         ))}
       </div>
     );
   };
   ```

3. **Create filterStore** (30 min)
   - Zustand store (see section 3.3)

4. **Integrate filter with table** (30 min)
   - Apply filter using `getFilteredRowModel`
   - Show active filter count

**Acceptance Criteria:**
- [ ] Provider filter sidebar visible
- [ ] Checking provider filters table immediately
- [ ] Multiple providers selectable (OR logic)
- [ ] "Clear Filters" button resets selections
- [ ] Filter count badge shows active filters

---

#### 📋 Story 3.6: Capabilities Filters
**Estimated Time:** 3 hours

**Tasks:**

1. **Create CapabilitiesFilter component** (1.5 hours)
   - Checkboxes for: Function Calling, Vision, Audio Input, Audio Output, Streaming, JSON Mode
   - AND logic: model must have ALL selected capabilities

2. **Add tooltips** (45 min)
   - Explain what each capability means
   - Use Headless UI or custom tooltip

3. **Update filter logic** (45 min)
   - Extend `useFilters` hook
   - Handle capabilities AND logic

**Acceptance Criteria:**
- [ ] Capabilities filter section in sidebar
- [ ] Checkboxes for all 6 capabilities
- [ ] Selecting capability filters to models with that feature
- [ ] AND logic: multiple selections require all capabilities
- [ ] Tooltip explains each capability on hover

---

#### 📋 Story 3.7: Price Range Filter
**Estimated Time:** 4 hours

**Tasks:**

1. **Create Slider component** (1.5 hours)
   - Dual-handle range slider (use library or custom)
   - Option: `react-slider` or Headless UI

2. **Calculate price range** (30 min)
   - Compute min/max from loaded models
   - Total price = input + output price

3. **Create PriceRangeFilter component** (1.5 hours)
   ```typescript
   export const PriceRangeFilter = () => {
     const { priceRange, setPriceRange } = useFilterStore();
     const { data: models = [] } = useModels();

     const minPrice = Math.min(...models.map(m => m.inputPricePerMillion + m.outputPricePerMillion));
     const maxPrice = Math.max(...models.map(m => m.inputPricePerMillion + m.outputPricePerMillion));

     return (
       <div>
         <h3>Price Range</h3>
         <Slider
           min={minPrice}
           max={maxPrice}
           value={priceRange}
           onChange={setPriceRange}
         />
         <div className="flex justify-between">
           <span>${priceRange[0]}</span>
           <span>${priceRange[1]}</span>
         </div>
       </div>
     );
   };
   ```

4. **Apply filter** (30 min)
   - Update `useFilters` hook
   - Real-time filtering as slider moves

**Acceptance Criteria:**
- [ ] Dual slider controls min/max price
- [ ] Range displays current selection (e.g., "$10 - $500")
- [ ] Adjusting slider filters table immediately
- [ ] Range defaults to full dataset min/max
- [ ] Total price = input + output per 1M tokens

---

### Sprint 3: Search & Comparison (Stories 3.8-3.11)

#### 📋 Story 3.8: Search Functionality
**Estimated Time:** 2-3 hours

**Tasks:**

1. **Create SearchBar component** (1 hour)
   ```typescript
   import { useDebouncedValue } from '@/hooks/useDebouncedValue';

   export const SearchBar = () => {
     const [query, setQuery] = useState('');
     const debouncedQuery = useDebouncedValue(query, 300);
     const { setSearchQuery } = useFilterStore();

     useEffect(() => {
       setSearchQuery(debouncedQuery);
     }, [debouncedQuery]);

     return (
       <div className="relative">
         <input
           type="text"
           value={query}
           onChange={(e) => setQuery(e.target.value)}
           placeholder="Search models or providers..."
           className="w-full px-4 py-2 border rounded-lg"
         />
         {query && (
           <button onClick={() => setQuery('')} className="absolute right-2 top-2">
             <XIcon />
           </button>
         )}
       </div>
     );
   };
   ```

2. **Implement debounce hook** (30 min)
   ```typescript
   // src/hooks/useDebouncedValue.ts
   export const useDebouncedValue = <T>(value: T, delay: number): T => {
     const [debouncedValue, setDebouncedValue] = useState(value);

     useEffect(() => {
       const timer = setTimeout(() => setDebouncedValue(value), delay);
       return () => clearTimeout(timer);
     }, [value, delay]);

     return debouncedValue;
   };
   ```

3. **Integrate in Header** (30 min)
   - Place SearchBar in Header component
   - Responsive: Full width on mobile

4. **Update filter logic** (30 min)
   - Case-insensitive search
   - Match on name OR provider

**Acceptance Criteria:**
- [ ] Search input in header
- [ ] Typing filters table in real-time (debounced 300ms)
- [ ] Case-insensitive matching
- [ ] Searches model name and provider
- [ ] Clear button (X) resets search
- [ ] Works alongside other filters

---

#### 📋 Story 3.9: Benchmark Scores Display
**Estimated Time:** 3 hours
**Prerequisites:** Backend includes benchmarks in ModelDto

**Tasks:**

1. **Update backend DTO** (30 min)
   - Ensure `ModelDto.TopBenchmarks` includes 3-5 key benchmarks
   - Prioritize: MMLU, HumanEval, GSM8K, HellaSwag, MATH

2. **Add benchmark columns** (1.5 hours)
   ```typescript
   // Add to columns definition
   {
     accessorKey: 'topBenchmarks',
     header: 'MMLU',
     cell: ({ row }) => {
       const benchmark = row.original.topBenchmarks.find(b => b.benchmarkName === 'MMLU');
       return benchmark ? `${benchmark.score.toFixed(1)}%` : 'N/A';
     },
   },
   // Repeat for other benchmarks
   ```

3. **Format scores** (30 min)
   - Decimal formatting (e.g., "85.2")
   - Percentage if applicable
   - "N/A" for missing scores

4. **Add tooltips** (30 min)
   - Show full benchmark name on hover
   - Example: "MMLU - Massive Multitask Language Understanding"

**Acceptance Criteria:**
- [ ] Table columns for 3-5 key benchmarks
- [ ] Scores formatted (decimals or percentages)
- [ ] "N/A" shown for missing scores
- [ ] Benchmark columns sortable
- [ ] Tooltip shows full benchmark name

---

#### 📋 Story 3.10: Checkbox Selection
**Estimated Time:** 3 hours

**Tasks:**

1. **Create Checkbox component** (30 min)
   - Reusable UI component
   - Accessible (keyboard support)

2. **Add selection column** (1 hour)
   ```typescript
   // Add as first column
   {
     id: 'select',
     header: ({ table }) => (
       <Checkbox
         checked={table.getIsAllRowsSelected()}
         onChange={table.getToggleAllRowsSelectedHandler()}
       />
     ),
     cell: ({ row }) => (
       <Checkbox
         checked={row.getIsSelected()}
         onChange={row.getToggleSelectedHandler()}
       />
     ),
   },
   ```

3. **Integrate comparisonStore** (45 min)
   - Create Zustand store (see section 3.3)
   - Sync TanStack Table selection with store

4. **Enforce 5-model limit** (30 min)
   - Disable checkboxes when 5 selected
   - Show warning toast on 6th attempt

5. **Highlight selected rows** (15 min)
   - Background color on selected rows
   - Persist highlight during sort/filter

**Acceptance Criteria:**
- [ ] Checkbox column as first column
- [ ] Clicking checkbox selects/deselects model
- [ ] Selected rows highlighted (background color)
- [ ] "Select All" checkbox in header
- [ ] Max 5 models selectable (warning shown on 6th)
- [ ] Selection persists during filtering/sorting

---

#### 📋 Story 3.11: Comparison Basket UI
**Estimated Time:** 4 hours

**Tasks:**

1. **Create ComparisonBasket component** (2.5 hours)
   ```typescript
   // src/components/models/ComparisonBasket.tsx
   import { useComparisonStore } from '@store/comparisonStore';
   import { useNavigate } from 'react-router-dom';

   export const ComparisonBasket = () => {
     const { selectedModelIds, removeModel, clearSelection } = useComparisonStore();
     const { data: models = [] } = useModels();
     const navigate = useNavigate();

     const selectedModels = models.filter(m => selectedModelIds.includes(m.id));

     if (selectedModels.length === 0) return null;

     const handleCompare = () => {
       const ids = selectedModelIds.join(',');
       navigate(`/compare?models=${ids}`);
     };

     return (
       <div className="fixed bottom-0 left-0 right-0 bg-white border-t shadow-lg p-4">
         <div className="container mx-auto flex items-center justify-between">
           <div className="flex gap-2">
             {selectedModels.map(model => (
               <div key={model.id} className="bg-gray-100 rounded px-3 py-1 flex items-center gap-2">
                 <span className="text-sm">{model.name}</span>
                 <button onClick={() => removeModel(model.id)}>
                   <XIcon className="w-4 h-4" />
                 </button>
               </div>
             ))}
           </div>
           <div className="flex gap-2">
             <Button variant="outline" onClick={clearSelection}>Clear All</Button>
             <Button
               onClick={handleCompare}
               disabled={selectedModels.length < 2}
             >
               Compare Selected ({selectedModels.length})
             </Button>
           </div>
         </div>
       </div>
     );
   };
   ```

2. **Style basket** (1 hour)
   - Sticky bottom bar (fixed position)
   - Elevated shadow
   - Responsive: Stack on mobile

3. **Integrate in layout** (30 min)
   - Add to PublicLayout
   - Ensure doesn't overlap table

**Acceptance Criteria:**
- [ ] Basket appears when models selected
- [ ] Mini-cards show selected models (name, provider)
- [ ] Remove button (X) on each card
- [ ] "Compare Selected" button enabled with 2+ models
- [ ] "Clear All" button removes all selections
- [ ] Basket collapses when empty
- [ ] Navigates to `/compare?models=id1,id2,id3`

---

### Sprint 4: Optimization (Story 3.12)

#### 📋 Story 3.12: Pagination & Caching
**Estimated Time:** 5 hours

**Tasks:**

1. **Implement pagination** (2 hours)
   ```typescript
   const [pagination, setPagination] = useState({ pageIndex: 0, pageSize: 25 });

   const table = useReactTable({
     // ...
     state: { pagination },
     onPaginationChange: setPagination,
     getPaginationRowModel: getPaginationRowModel(),
   });
   ```

2. **Create pagination controls** (1.5 hours)
   - Previous/Next buttons
   - Page numbers (show 5 pages around current)
   - Page size selector (10, 25, 50, 100)

3. **Persist page size** (30 min)
   - localStorage: `pageSize`
   - Load on mount

4. **Backend caching** (1 hour)
   - Redis caching for `/api/models` (1 hour TTL)
   - Cache invalidation on model update (Epic 2 admin actions)

   ```csharp
   public async Task<List<ModelDto>> GetAllActiveModelsAsync()
   {
       var cacheKey = "cache:models:all";
       var cached = await _cache.GetAsync<List<ModelDto>>(cacheKey);

       if (cached != null) return cached;

       var models = await _repository.GetActiveModelsWithDetailsAsync();
       await _cache.SetAsync(cacheKey, models, TimeSpan.FromHours(1));

       return models;
   }
   ```

5. **Frontend caching tuning** (30 min)
   - TanStack Query: staleTime = 5 min, gcTime = 10 min
   - Prefetch on hover (advanced)

**Acceptance Criteria:**
- [ ] Pagination controls (prev/next, page numbers) below table
- [ ] Page size selector (10/25/50/100)
- [ ] Page size persisted in localStorage
- [ ] Backend Redis cache (1hr TTL) for `/api/models`
- [ ] Cache invalidated on admin model updates
- [ ] Frontend TanStack Query cache (5min stale, 10min gc)
- [ ] Page load < 2 seconds with 50+ models

---

## 5. Architectural Decision Records (ADRs)

### ADR-010: State Management Strategy for Epic 3

**Context**: Epic 3 requires managing multiple state types: server data, filters, model selection, and local UI state.

**Decision**:
- **Server State**: TanStack Query for models data, with built-in caching and refetching
- **Client State - Filters**: Zustand `filterStore` (providers, capabilities, price range, search query)
- **Client State - Selection**: Zustand `comparisonStore` (selected model IDs)
- **Local UI State**: React `useState` for modals, tooltips, local component state

**Rationale**:
- Separation of concerns: server state ≠ client state
- TanStack Query optimized for API data (caching, background refetch, stale-while-revalidate)
- Zustand lightweight for client state (no boilerplate, good DevTools)
- Avoids "everything in Redux" anti-pattern

**Consequences**:
- Clear boundaries between state types
- Easy to debug (TanStack Query DevTools, Zustand DevTools)
- Performance: No unnecessary re-renders from global state updates

**Alternatives Considered**:
- Redux Toolkit: Rejected (too heavy for Epic 3 scope)
- Context API: Rejected (performance issues with frequent filter updates)

---

### ADR-011: Client-Side Filtering Strategy

**Context**: Users need to apply multiple filters (provider, capabilities, price range, search) to 50-100 models.

**Decision**: Implement client-side filtering using TanStack Table's `getFilteredRowModel`.

**Rationale**:
- Dataset size: 50-100 models (~50KB JSON) fits easily in browser memory
- Instant filtering response (no network latency)
- Simpler architecture: No need for complex server-side query logic
- TanStack Table handles filtering efficiently with memoization

**Consequences**:
- Excellent UX: Filters respond in <50ms
- No backend changes needed for filter logic
- Scales up to ~500 models before client-side filtering becomes bottleneck

**Alternatives Considered**:
- Server-side filtering: Rejected (unnecessary latency for small dataset)
- Hybrid (some server, some client): Rejected (added complexity without clear benefit)

**Migration Path**: If dataset grows >500 models, migrate to server-side filtering with query params:
```
GET /api/models?provider=OpenAI&capabilities=vision&priceMax=100
```

---

### ADR-012: Selection State Persistence

**Context**: Users select models for comparison. Selection must persist during sorting, filtering, pagination.

**Decision**: Store selected model IDs (not row indices) in Zustand store.

**Rationale**:
- IDs are stable identifiers (won't change with sorting/filtering)
- Row indices change when table data is filtered/sorted
- Simpler logic: Check if ID exists in selectedModelIds array

**Implementation**:
```typescript
// Store IDs, not indices
selectedModelIds: ['uuid-1', 'uuid-2', 'uuid-3']

// Check selection
const isSelected = selectedModelIds.includes(model.id);
```

**Consequences**:
- Selection survives filtering (models remain selected even if filtered out temporarily)
- Clear All button easily resets: `selectedModelIds = []`

**Alternatives Considered**:
- Store row indices: Rejected (breaks on sort/filter)
- Store full model objects: Rejected (memory overhead, stale data issues)

---

### ADR-013: Styling and Component Library Strategy

**Context**: Need consistent, reusable UI components for Epic 3 (buttons, checkboxes, inputs).

**Decision**:
- Build minimal reusable components with TailwindCSS
- Use composition pattern for variants
- NO external component library (Shadcn, Radix, Material-UI, Chakra)

**Rationale**:
- TailwindCSS already installed and configured
- Epic 3 needs only ~5 reusable components (Button, Checkbox, Input, Slider, Tooltip)
- Component libraries add 50-200KB bundle size
- Full control over styling and behavior

**Example**:
```typescript
// src/components/ui/Button.tsx
interface ButtonProps {
  variant?: 'primary' | 'outline' | 'ghost';
  size?: 'sm' | 'md' | 'lg';
  children: React.ReactNode;
  onClick?: () => void;
}

export const Button = ({ variant = 'primary', size = 'md', ...props }: ButtonProps) => {
  const baseClasses = 'rounded font-medium transition-colors';
  const variantClasses = {
    primary: 'bg-blue-600 text-white hover:bg-blue-700',
    outline: 'border border-gray-300 hover:bg-gray-50',
    ghost: 'hover:bg-gray-100',
  };
  const sizeClasses = {
    sm: 'px-3 py-1 text-sm',
    md: 'px-4 py-2',
    lg: 'px-6 py-3 text-lg',
  };

  return (
    <button
      className={`${baseClasses} ${variantClasses[variant]} ${sizeClasses[size]}`}
      {...props}
    />
  );
};
```

**Consequences**:
- Smaller bundle size (~50KB for custom components vs 200KB+ for libraries)
- Learning curve: Team learns component patterns instead of library APIs
- Maintenance: Custom components = our responsibility

**Migration Path**: If Epic 8 (responsive/mobile) reveals gaps, consider Headless UI for accessible primitives (modals, dropdowns).

---

### ADR-014: Pagination Strategy

**Context**: Table may have 50-100+ models. Need to balance UX and performance.

**Decision**: Basic pagination (10/25/50/100 per page) for MVP. Defer virtual scrolling to post-MVP.

**Rationale**:
- TanStack Table's `getPaginationRowModel` is built-in and simple
- Pagination works with filtering/sorting naturally
- Virtual scrolling adds complexity (requires fixed row heights, more testing)

**Implementation**:
```typescript
const table = useReactTable({
  getPaginationRowModel: getPaginationRowModel(),
  state: { pagination: { pageIndex: 0, pageSize: 25 } },
});
```

**Consequences**:
- Simpler implementation (2 hours vs 5+ for virtual scrolling)
- Good UX for current dataset size (50-100 models)
- Clear upgrade path to virtual scrolling if dataset grows >500 models

**When to Migrate to Virtual Scrolling**:
- Dataset exceeds 500 models
- Users report slow filtering/sorting
- Performance metrics show >100ms interaction latency

---

## 6. Pre-Implementation Checklist

Before starting Story 3.1, ensure these prerequisites are complete:

### Backend Checklist

- [ ] **Domain Entities Created**
  - [ ] `Model.cs` with all properties (name, provider, pricing, metadata)
  - [ ] `ModelCapability.cs` with capability flags
  - [ ] `Benchmark.cs` with benchmark definitions
  - [ ] `ModelBenchmarkScore.cs` with score data

- [ ] **Database Configuration**
  - [ ] DbContext configured with DbSets for all entities
  - [ ] Entity relationships configured (one-to-one, one-to-many)
  - [ ] Migration generated: `dotnet ef migrations add AddModelEntities`
  - [ ] Migration applied: `dotnet ef database update`

- [ ] **Seed Data**
  - [ ] Seed script created with 10+ models (GPT-4, Claude 3, Gemini, Llama 3, Mistral, etc.)
  - [ ] Each model has pricing, capabilities, and 3-5 benchmark scores
  - [ ] Seed runs on app startup (development environment only)

- [ ] **API Endpoint**
  - [ ] `ModelsController.cs` created
  - [ ] `GET /api/models` endpoint implemented
  - [ ] DTOs defined: `ModelDto`, `CapabilitiesDto`, `BenchmarkScoreDto`
  - [ ] Service layer: `IModelService`, `ModelService` implemented
  - [ ] Endpoint tested with Postman/curl (returns 200 OK with model data)

- [ ] **Redis Setup**
  - [ ] Redis running locally or Upstash configured
  - [ ] Cache service abstraction (`ICacheService`) created
  - [ ] Connection verified in health check

### Frontend Checklist

- [ ] **Folder Structure**
  - [ ] Create directories: `components/`, `pages/`, `api/`, `store/`, `hooks/`, `types/`, `utils/`
  - [ ] Sub-directories: `components/layout/`, `components/models/`, `components/ui/`

- [ ] **Configuration**
  - [ ] Path aliases verified in `tsconfig.json` and `vite.config.ts`
  - [ ] TailwindCSS config extended with design tokens (colors, spacing, fonts)
  - [ ] Environment variables configured (API base URL if needed)

- [ ] **Dependencies Verified**
  - [ ] `@tanstack/react-table` v8+ installed
  - [ ] `@tanstack/react-query` v5+ installed
  - [ ] `zustand` v5+ installed
  - [ ] `react-router-dom` v7+ installed
  - [ ] `axios` installed
  - [ ] `lucide-react` (or other icon library) installed

### Infrastructure Checklist

- [ ] **Development Environment**
  - [ ] Backend API running at `http://localhost:5000`
  - [ ] Frontend dev server running at `http://localhost:5173`
  - [ ] API proxy `/api` → `localhost:5000` verified
  - [ ] CORS configured in backend for frontend origin

- [ ] **Data**
  - [ ] PostgreSQL database accessible
  - [ ] Redis accessible (local or Upstash)
  - [ ] Seed data loaded (verify with database client)

- [ ] **Testing Readiness**
  - [ ] Vitest configured for frontend unit tests
  - [ ] React Testing Library installed
  - [ ] Backend testing framework (xUnit) ready

---

## 7. Success Metrics for Epic 3

Upon completing all 12 stories, the platform should meet these criteria:

### Functional Metrics

- ✅ **Data Display**: Table shows 10+ models with pricing, capabilities, and benchmark scores
- ✅ **Sorting**: Users can sort by 8+ columns (name, provider, prices, benchmarks)
- ✅ **Filtering**: 4+ filters functional (provider, capabilities, price range, search)
- ✅ **Selection**: Users can select up to 5 models with visual feedback
- ✅ **Comparison Basket**: Basket UI appears with selected models, navigates to `/compare`

### Technical Metrics

- ✅ **Performance**:
  - Initial page load: < 2 seconds (with 50 models)
  - Filter/sort interactions: < 100ms response time
  - Table render: < 1 second (50 models)

- ✅ **Caching**:
  - Backend Redis cache hit ratio: > 80%
  - Frontend TanStack Query cache working (5min stale, 10min gc)

- ✅ **Code Quality**:
  - Zero TypeScript `any` types (strict mode enforced)
  - All components typed with explicit interfaces
  - ESLint/Prettier passing

- ✅ **Bundle Size**:
  - Initial bundle (gzipped): < 300KB
  - Lazy-loaded routes: < 100KB each

### UX Metrics

- ✅ **Responsiveness**: Layout adapts to desktop, tablet (mobile polish in Epic 8)
- ✅ **Feedback**:
  - Loading states visible for operations >200ms
  - Error messages clear and actionable
  - Success confirmations for actions (e.g., "5 models selected")

- ✅ **Accessibility** (WCAG 2.1 AA basics):
  - Keyboard navigation functional (tab, enter, arrows)
  - Color contrast ratios meet standards
  - Screen reader compatible (ARIA labels where needed)

### User Flow Validation

- ✅ **Journey 1 Replica**: A user can:
  1. Land on homepage
  2. View all models in table
  3. Sort by price (ascending)
  4. Filter by "supports function calling"
  5. Search for "Claude"
  6. Select 3 models
  7. See comparison basket
  8. Click "Compare Selected" → Navigate to `/compare?models=id1,id2,id3`

---

## 8. Risk Mitigation

### Identified Risks

**Risk 1: Backend Delays**
- **Impact**: Frontend can't start Story 3.2 without `/api/models` endpoint
- **Mitigation**:
  - Backend team completes Story 1.4 (entities) and 1.10 (GET /api/models) as priority
  - Frontend uses mock data for Story 3.1 if needed
  - Parallel work: Frontend builds UI components while backend finalizes API

**Risk 2: Performance Degradation**
- **Impact**: Table lags with 100+ models
- **Mitigation**:
  - Story 3.12 includes performance testing with stress test (200 models)
  - Virtual scrolling as fallback (defer if not needed)
  - Memoization enforced in React components (useMemo, React.memo)

**Risk 3: State Management Complexity**
- **Impact**: Filters + selection + sorting = complex state interactions
- **Mitigation**:
  - ADR-010 clearly defines state boundaries
  - TanStack Table handles sorting/filtering internally (less custom state)
  - Unit tests for filter logic (isolation testing)

**Risk 4: Scope Creep**
- **Impact**: "While we're at it" additions bloat Epic 3
- **Mitigation**:
  - Strict adherence to 12-story scope
  - Defer enhancements to Epic 4/5 (e.g., icons, advanced charts)
  - Story acceptance criteria = exit criteria (no extras)

---

## 9. Next Steps

### Immediate Actions (Before Story 3.1)

1. **Backend Team**:
   - [ ] Complete domain entities (Model, Capability, Benchmark)
   - [ ] Run migration: `dotnet ef migrations add AddModelEntities`
   - [ ] Create seed data script (10+ models)
   - [ ] Implement `GET /api/models` endpoint
   - [ ] Test endpoint returns correct data

2. **Frontend Team**:
   - [ ] Create folder structure (`components/`, `pages/`, etc.)
   - [ ] Configure Axios client with baseURL
   - [ ] Extend TailwindCSS config (design tokens)
   - [ ] Verify all dependencies installed and importing correctly

3. **DevOps/Infrastructure**:
   - [ ] Verify Redis accessible
   - [ ] Verify PostgreSQL accessible
   - [ ] Ensure backend/frontend running concurrently
   - [ ] Confirm API proxy working

### Story Kickoff Process

For each story:
1. **Pre-Story**:
   - Review story acceptance criteria
   - Check prerequisites complete
   - Assign story to developer(s)

2. **During Story**:
   - Follow task checklist
   - Write unit tests alongside implementation
   - Commit frequently (small, logical commits)
   - Update story markdown with completion notes

3. **Post-Story**:
   - Demo to team (show acceptance criteria met)
   - Code review (PR review required)
   - Merge to main
   - Mark story as complete in docs

### Documentation Updates

As Epic 3 progresses:
- [ ] Create `tech-spec-epic-3.md` (comprehensive technical spec)
- [ ] Document each story implementation in `/docs/stories/story-3.X.md`
- [ ] Update architecture diagrams with Epic 3 components
- [ ] Create API documentation for `/api/models` endpoint

---

## 10. Appendix

### A. Example Seed Data

```csharp
// services/backend/LlmTokenPrice.Infrastructure/Data/DbInitializer.cs

public static class DbInitializer
{
    public static void Initialize(AppDbContext context)
    {
        // Benchmarks
        var mmlu = new Benchmark
        {
            Id = Guid.NewGuid(),
            BenchmarkName = "MMLU",
            FullName = "Massive Multitask Language Understanding",
            Category = "Reasoning",
            Interpretation = "Higher is better",
            TypicalRangeMin = 0,
            TypicalRangeMax = 100
        };

        var humanEval = new Benchmark
        {
            Id = Guid.NewGuid(),
            BenchmarkName = "HumanEval",
            FullName = "HumanEval Code Generation",
            Category = "Code",
            Interpretation = "Higher is better",
            TypicalRangeMin = 0,
            TypicalRangeMax = 100
        };

        context.Benchmarks.AddRange(mmlu, humanEval);

        // Models
        var gpt4 = new Model
        {
            Id = Guid.NewGuid(),
            Name = "GPT-4 Turbo",
            Provider = "OpenAI",
            Version = "gpt-4-turbo-2024-04-09",
            InputPricePerMillion = 10.00m,
            OutputPricePerMillion = 30.00m,
            Currency = "USD",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Capability = new ModelCapability
            {
                Id = Guid.NewGuid(),
                ContextWindow = 128000,
                MaxOutputTokens = 4096,
                SupportsFunctionCalling = true,
                SupportsVision = true,
                SupportsStreaming = true,
                SupportsJsonMode = true
            },
            BenchmarkScores = new List<ModelBenchmarkScore>
            {
                new() { Id = Guid.NewGuid(), BenchmarkId = mmlu.Id, Score = 86.4m, TestDate = DateTime.UtcNow },
                new() { Id = Guid.NewGuid(), BenchmarkId = humanEval.Id, Score = 87.0m, TestDate = DateTime.UtcNow }
            }
        };

        var claude3 = new Model
        {
            Id = Guid.NewGuid(),
            Name = "Claude 3.5 Sonnet",
            Provider = "Anthropic",
            Version = "claude-3-5-sonnet-20241022",
            InputPricePerMillion = 3.00m,
            OutputPricePerMillion = 15.00m,
            Currency = "USD",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Capability = new ModelCapability
            {
                Id = Guid.NewGuid(),
                ContextWindow = 200000,
                MaxOutputTokens = 4096,
                SupportsFunctionCalling = true,
                SupportsVision = true,
                SupportsStreaming = true,
                SupportsJsonMode = false
            },
            BenchmarkScores = new List<ModelBenchmarkScore>
            {
                new() { Id = Guid.NewGuid(), BenchmarkId = mmlu.Id, Score = 88.7m, TestDate = DateTime.UtcNow },
                new() { Id = Guid.NewGuid(), BenchmarkId = humanEval.Id, Score = 92.0m, TestDate = DateTime.UtcNow }
            }
        };

        context.Models.AddRange(gpt4, claude3);
        context.SaveChanges();
    }
}
```

### B. TypeScript Type Definitions

```typescript
// src/types/model.ts

export interface Model {
  id: string;
  name: string;
  provider: string;
  version?: string;
  inputPricePerMillion: number;
  outputPricePerMillion: number;
  currency: string;
  capabilities: Capabilities;
  topBenchmarks: BenchmarkScore[];
  updatedAt: string;
  status: 'active' | 'deprecated';
}

export interface Capabilities {
  contextWindow: number;
  maxOutputTokens: number;
  supportsFunctionCalling: boolean;
  supportsVision: boolean;
  supportsAudioInput: boolean;
  supportsAudioOutput: boolean;
  supportsStreaming: boolean;
  supportsJsonMode: boolean;
}

export interface BenchmarkScore {
  benchmarkName: string;
  category: string;
  score: number;
  maxScore?: number;
}

export interface ApiResponse<T> {
  data: T;
  meta?: {
    timestamp: string;
    cached: boolean;
  };
}
```

### C. Useful Commands Reference

```bash
# Backend
cd services/backend

# Create migration
dotnet ef migrations add AddModelEntities --project LlmTokenPrice.Infrastructure

# Apply migration
dotnet ef database update --project LlmTokenPrice.Infrastructure

# Run API
dotnet run --project LlmTokenPrice.API

# Run tests
dotnet test

# Frontend
cd apps/web

# Install dependencies
pnpm install

# Run dev server
pnpm run dev

# Type check
pnpm run type-check

# Lint
pnpm run lint

# Build
pnpm run build

# Both (concurrent)
# Terminal 1
cd services/backend && dotnet run --project LlmTokenPrice.API

# Terminal 2
cd apps/web && pnpm run dev
```

### D. Recommended VS Code Extensions

- **Backend**:
  - C# Dev Kit
  - C# Extensions
  - NuGet Package Manager

- **Frontend**:
  - ESLint
  - Prettier
  - Tailwind CSS IntelliSense
  - TypeScript + JavaScript
  - Error Lens

- **General**:
  - GitLens
  - Thunder Client (API testing)
  - TODO Highlight

---

**Document Version:** 1.0
**Last Updated:** 2025-10-16
**Maintained By:** Pablo (with Claude Code assistance)

---

_This document serves as the definitive implementation guide for Epic 3. Refer to this as the single source of truth during development._
