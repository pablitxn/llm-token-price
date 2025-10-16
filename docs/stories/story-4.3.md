# Story 4.3: Overview Tab with Model Specifications

Status: Ready

## Story

As a user,
I want to see complete model specifications,
so that I understand all model details.

## Acceptance Criteria

1. Tabbed interface created in modal (Overview, Benchmarks, Pricing tabs)
2. Overview tab displays:
   - Full model name, provider, version, release date, status
   - Pricing: input price/1M, output price/1M, currency
   - Capabilities: context window, max output, all capability flags
   - Last updated timestamp
3. Data fetched from GET `/api/models/{id}` endpoint
4. Loading state shown while fetching
5. Information organized in clear sections with labels

## Tasks / Subtasks

- [ ] **Task 1: Create reusable Tabs component** (AC: #1)
  - [ ] 1.1: Create `Tabs.tsx` in `/apps/web/src/components/ui`
  - [ ] 1.2: Accept props: tabs array (id, label), activeTab, onChange, children
  - [ ] 1.3: Render tab navigation with buttons for each tab
  - [ ] 1.4: Style active tab with border-bottom and color change
  - [ ] 1.5: Render children (tab content) below navigation
  - [ ] 1.6: Make tabs keyboard accessible (arrow keys navigation)

- [ ] **Task 2: Create useModelDetail hook** (AC: #3, #4)
  - [ ] 2.1: Create `useModelDetail.ts` in `/apps/web/src/hooks`
  - [ ] 2.2: Use TanStack Query's useQuery hook
  - [ ] 2.3: Query key: `['model', modelId]`
  - [ ] 2.4: Query function: fetch from `/api/models/${modelId}`
  - [ ] 2.5: Configure staleTime: 5 minutes, gcTime: 10 minutes
  - [ ] 2.6: Return { data, isLoading, error }
  - [ ] 2.7: Handle case where modelId is null/undefined (don't fetch)

- [ ] **Task 3: Create TypeScript interface for ModelDetail** (AC: #2)
  - [ ] 3.1: Create `modelDetail.ts` in `/apps/web/src/types`
  - [ ] 3.2: Define ModelDetail interface matching backend ModelDetailDto
  - [ ] 3.3: Include all pricing fields (per million, per thousand, per token)
  - [ ] 3.4: Include complete Capabilities interface
  - [ ] 3.5: Include BenchmarkScoreDetail[] array
  - [ ] 3.6: Include PriceComparison interface (optional)

- [ ] **Task 4: Create OverviewTab component** (AC: #2, #5)
  - [ ] 4.1: Create `OverviewTab.tsx` in `/apps/web/src/components/models/tabs`
  - [ ] 4.2: Accept model prop (ModelDetail type)
  - [ ] 4.3: Create Section component for grouping info (title + children)
  - [ ] 4.4: Create InfoRow component for label-value pairs
  - [ ] 4.5: Implement Basic Information section (name, provider, version, release date, status)
  - [ ] 4.6: Implement Pricing section (input/output prices per 1M, currency)
  - [ ] 4.7: Implement Capabilities section (context window, max output, capability flags)
  - [ ] 4.8: Implement Metadata section (last updated timestamp)

- [ ] **Task 5: Create CapabilityBadge component** (AC: #2)
  - [ ] 5.1: Create `CapabilityBadge.tsx` component
  - [ ] 5.2: Accept props: label (string), supported (boolean)
  - [ ] 5.3: Render checkmark icon if supported, X icon if not
  - [ ] 5.4: Style with green background if supported, gray if not
  - [ ] 5.5: Use in Capabilities section of OverviewTab

- [ ] **Task 6: Integrate Tabs in ModelDetailModal** (AC: #1)
  - [ ] 6.1: Import Tabs component in ModelDetailModal
  - [ ] 6.2: Create local state for activeTab (default: 'overview')
  - [ ] 6.3: Define tabs array: [{ id: 'overview', label: 'Overview' }, { id: 'benchmarks', label: 'Benchmarks' }, { id: 'pricing', label: 'Pricing' }]
  - [ ] 6.4: Render Tabs component with tabs, activeTab, and onChange handler
  - [ ] 6.5: Render OverviewTab when activeTab === 'overview'
  - [ ] 6.6: Render placeholders for Benchmarks and Pricing tabs

- [ ] **Task 7: Add loading and error states** (AC: #4)
  - [ ] 7.1: Show skeleton loader while isLoading (shimmer effect)
  - [ ] 7.2: Create LoadingSkeleton component for OverviewTab
  - [ ] 7.3: Show error message if fetch fails (with retry button)
  - [ ] 7.4: Show "Model not found" if data is null after loading

- [ ] **Task 8: Formatting and polish** (AC: #2, #5)
  - [ ] 8.1: Format dates with date-fns (formatRelativeTime for "Updated 3 days ago")
  - [ ] 8.2: Format numbers with toLocaleString (e.g., "128,000 tokens")
  - [ ] 8.3: Display status as Badge component (green for "active", yellow for "deprecated")
  - [ ] 8.4: Add tooltips to capability flags explaining what they mean
  - [ ] 8.5: Ensure all sections have clear visual separation (borders, spacing)

- [ ] **Task 9: Testing**
  - [ ] 9.1: Write unit tests for OverviewTab component (Vitest + Testing Library)
  - [ ] 9.2: Test renders all sections correctly with mock data
  - [ ] 9.3: Test displays "N/A" for missing optional fields (version, release date)
  - [ ] 9.4: Write tests for useModelDetail hook (mock API response)
  - [ ] 9.5: Test tab switching works correctly
  - [ ] 9.6: Test loading state displays skeleton
  - [ ] 9.7: Test error state displays error message

## Dev Notes

### Architecture Context

**Tab Navigation Pattern:**
- Use local component state for activeTab (no URL params needed here)
- Tab content rendered conditionally based on activeTab
- Future: Lazy load tab content (only render active tab)

**Data Fetching Strategy:**
- TanStack Query handles caching automatically
- 5-minute stale time = data refreshed if >5min old
- 10-minute garbage collection = cached data removed after 10min of non-use
- Query key `['model', modelId]` ensures separate cache per model

**Responsive Design:**
- Tabs scroll horizontally on mobile if labels too long
- Sections stack vertically, always readable
- Touch-friendly tab buttons (min 44px height)

### Project Structure Notes

**Frontend Files to Create:**
```
/apps/web/src/
├── components/
│   ├── ui/
│   │   └── Tabs.tsx                         # Reusable tab navigation
│   └── models/
│       ├── ModelDetailModal.tsx             # (update) Add tab navigation
│       └── tabs/
│           ├── OverviewTab.tsx              # Overview tab content
│           ├── Section.tsx                  # Section wrapper component
│           ├── InfoRow.tsx                  # Label-value row component
│           └── CapabilityBadge.tsx          # Capability indicator
├── hooks/
│   └── useModelDetail.ts                    # Fetch model detail with TanStack Query
└── types/
    └── modelDetail.ts                       # TypeScript interfaces for model detail
```

### Implementation Details

**Tabs Component:**
```typescript
// components/ui/Tabs.tsx
interface Tab {
  id: string;
  label: string;
}

interface TabsProps {
  tabs: Tab[];
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
            className={`py-4 px-1 border-b-2 font-medium transition-colors ${
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

**useModelDetail Hook:**
```typescript
// hooks/useModelDetail.ts
import { useQuery } from '@tanstack/react-query';
import { apiClient } from '@api/client';
import { ModelDetail } from '@/types/modelDetail';

export const useModelDetail = (modelId: string | null) => {
  return useQuery({
    queryKey: ['model', modelId],
    queryFn: async () => {
      if (!modelId) return null;
      const { data } = await apiClient.get<ApiResponse<ModelDetail>>(`/models/${modelId}`);
      return data.data;
    },
    enabled: !!modelId, // Only fetch if modelId exists
    staleTime: 5 * 60 * 1000, // 5 minutes
    gcTime: 10 * 60 * 1000,   // 10 minutes
  });
};
```

**OverviewTab Component:**
```typescript
// components/models/tabs/OverviewTab.tsx
import { ModelDetail } from '@/types/modelDetail';
import { Section } from './Section';
import { InfoRow } from './InfoRow';
import { CapabilityBadge } from './CapabilityBadge';
import { formatDate, formatRelativeTime } from '@/utils/formatters';

interface OverviewTabProps {
  model: ModelDetail;
}

export const OverviewTab = ({ model }: OverviewTabProps) => (
  <div className="space-y-6">
    {/* Basic Information */}
    <Section title="Basic Information">
      <InfoRow label="Model Name" value={model.name} />
      <InfoRow label="Provider" value={model.provider} />
      <InfoRow label="Version" value={model.version || 'N/A'} />
      <InfoRow label="Release Date" value={model.releaseDate ? formatDate(model.releaseDate) : 'N/A'} />
      <InfoRow label="Status">
        <Badge variant={model.status === 'active' ? 'success' : 'warning'}>
          {model.status}
        </Badge>
      </InfoRow>
    </Section>

    {/* Pricing */}
    <Section title="Pricing">
      <InfoRow label="Input (per 1M tokens)" value={`$${model.inputPricePerMillion.toFixed(2)}`} />
      <InfoRow label="Output (per 1M tokens)" value={`$${model.outputPricePerMillion.toFixed(2)}`} />
      <InfoRow label="Currency" value={model.currency} />
    </Section>

    {/* Capabilities */}
    <Section title="Capabilities">
      <InfoRow label="Context Window" value={`${model.capabilities.contextWindow.toLocaleString()} tokens`} />
      <InfoRow label="Max Output" value={`${model.capabilities.maxOutputTokens.toLocaleString()} tokens`} />

      <div className="grid grid-cols-2 gap-4 mt-4">
        <CapabilityBadge label="Function Calling" supported={model.capabilities.supportsFunctionCalling} />
        <CapabilityBadge label="Vision Support" supported={model.capabilities.supportsVision} />
        <CapabilityBadge label="Streaming" supported={model.capabilities.supportsStreaming} />
        <CapabilityBadge label="JSON Mode" supported={model.capabilities.supportsJsonMode} />
      </div>
    </Section>

    {/* Metadata */}
    <Section title="Metadata">
      <InfoRow label="Last Updated" value={formatRelativeTime(model.updatedAt)} />
    </Section>
  </div>
);
```

**Integration in ModelDetailModal:**
```typescript
// components/models/ModelDetailModal.tsx
export const ModelDetailModal = () => {
  const { modelId, isOpen, closeModal } = useModalState();
  const { data: model, isLoading, error } = useModelDetail(modelId);
  const [activeTab, setActiveTab] = useState('overview');

  if (!isOpen) return null;

  const tabs = [
    { id: 'overview', label: 'Overview' },
    { id: 'benchmarks', label: 'Benchmarks' },
    { id: 'pricing', label: 'Pricing' },
  ];

  return (
    <Modal isOpen={isOpen} onClose={closeModal} title={model?.name || 'Loading...'}>
      {isLoading ? (
        <LoadingSkeleton />
      ) : error ? (
        <ErrorMessage message="Failed to load model details" />
      ) : model ? (
        <Tabs tabs={tabs} activeTab={activeTab} onChange={setActiveTab}>
          {activeTab === 'overview' && <OverviewTab model={model} />}
          {activeTab === 'benchmarks' && <p>Benchmarks tab coming in Story 4.4</p>}
          {activeTab === 'pricing' && <p>Pricing tab coming in Story 4.5</p>}
        </Tabs>
      ) : (
        <p>Model not found</p>
      )}
    </Modal>
  );
};
```

### References

- [Epic 4 Analysis: docs/epic-4-analysis-and-plan.md#Story 4.3]
- [Story 4.2: Backend API for Model Detail] (dependency)
- [Epics Document: docs/epics.md#Story 4.2]
- [TanStack Query Docs](https://tanstack.com/query/latest/docs/react/overview)

### Testing Strategy

**Unit Tests:**
- OverviewTab renders all sections with complete data
- Displays "N/A" for missing optional fields
- CapabilityBadge shows correct icon/color for supported/unsupported
- InfoRow formats label and value correctly

**Integration Tests:**
- useModelDetail hook fetches data from /api/models/{id}
- Tab switching updates displayed content
- Loading state shows skeleton while fetching
- Error state shows error message with retry option

**Accessibility Tests:**
- Tab navigation keyboard accessible (Tab key, arrow keys)
- Focus visible on active tab
- Screen reader announces tab changes

## Dev Agent Record

### Context Reference

<!-- Path(s) to story context XML will be added here by context workflow -->

### Agent Model Used

claude-sonnet-4-5-20250929

### Debug Log References

### Completion Notes List

### File List
