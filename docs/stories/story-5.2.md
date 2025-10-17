# Story 5.2: Display Selected Models in Side-by-Side Cards

Status: Draft

## Story

As a user,
I want to see selected models displayed side-by-side in cards,
so that I can visually compare their key attributes at a glance.

## Acceptance Criteria

1. Comparison page displays 2-5 model cards horizontally (desktop) or stacked vertically (mobile <768px)
2. Each card shows: model name, provider, input/output pricing, context window, and key capabilities (function calling, vision, streaming)
3. Cards have equal width for vertical attribute alignment across all models
4. Remove button (X icon) on each card removes model from comparison and updates URL
5. Cards use horizontal scroll if >3 models on smaller screens (tablet: 768-1024px)
6. Empty card placeholder shown if <2 models with "Add Model" prompt

## Tasks / Subtasks

### Task Group 1: Create ModelCard Component (AC: #2, #3)
- [ ] Create component file: `apps/web/src/components/comparison/ModelCard.tsx`
  - [ ] Define `ModelCardProps` interface: `{ model: ModelDto; onRemove: (id: string) => void }`
  - [ ] Create functional component with TypeScript
  - [ ] Export as named export
- [ ] Implement card layout with TailwindCSS
  - [ ] Container: `bg-white border border-gray-200 rounded-lg p-6 shadow-sm`
  - [ ] Fixed width for alignment: `w-80` (320px) or `flex-1 min-w-[320px]`
  - [ ] Sections: Header (name/provider), Pricing, Capabilities, Context Window
  - [ ] Use `space-y-4` for vertical spacing between sections
- [ ] Header section
  - [ ] Model name: `text-xl font-semibold text-gray-900`
  - [ ] Provider: `text-sm text-gray-600` with pill badge
  - [ ] Remove button (X): `absolute top-4 right-4` with hover effect
- [ ] Pricing section
  - [ ] Label: "Pricing" (`text-sm font-medium text-gray-700`)
  - [ ] Input price: `$X.XX per 1M tokens`
  - [ ] Output price: `$X.XX per 1M tokens`
  - [ ] Format with `formatCurrency` utility (2 decimal places)
- [ ] Capabilities section
  - [ ] Label: "Key Capabilities"
  - [ ] Icon list: Function calling ✓, Vision ✓, Streaming ✓
  - [ ] Use checkmark (✓) for true, dash (—) for false
  - [ ] Icons from lucide-react: Check, X
- [ ] Context window section
  - [ ] Label: "Context Window"
  - [ ] Value: `128,000 tokens` (formatted with commas)

### Task Group 2: Create useComparisonData Hook (AC: #1, #4)
- [ ] Create custom hook: `apps/web/src/hooks/useComparisonData.ts`
  - [ ] Use TanStack Query `useQuery` for data fetching
  - [ ] Query key: `['comparison', modelIds.sort().join(',')]`
  - [ ] Query function: `comparisonApi.fetchComparison(modelIds)`
  - [ ] Enabled condition: `modelIds.length >= 2 && modelIds.length <= 5`
  - [ ] Stale time: 5 minutes (300000ms)
  - [ ] Return: `{ data, isLoading, error, refetch }`
- [ ] Handle loading state
  - [ ] Return `isLoading` flag for skeleton rendering
- [ ] Handle error state
  - [ ] Return `error` object for error boundary
  - [ ] Automatic retry: 3 attempts with exponential backoff (TanStack Query default)

### Task Group 3: API Client Function (AC: #1)
- [ ] Create API function: `apps/web/src/api/comparison.ts`
  - [ ] Define `fetchComparison` function
  - [ ] Signature: `async (modelIds: string[]): Promise<ComparisonResponseDto>`
  - [ ] Build URL: `/api/models/compare?ids=${modelIds.join(',')}`
  - [ ] Use axios GET request
  - [ ] Return typed response: `ComparisonResponseDto`
- [ ] Handle API errors
  - [ ] Catch axios errors and rethrow with user-friendly message
  - [ ] Handle 400 (invalid IDs), 404 (models not found), 500 (server error)

### Task Group 4: Integrate Cards into ComparisonPage (AC: #1, #5)
- [ ] Update `ComparisonPage.tsx` to fetch and display cards
  - [ ] Import `useComparisonData` hook
  - [ ] Extract `modelIds` from URL using `useComparisonParams`
  - [ ] Call `useComparisonData(modelIds)`
  - [ ] Render loading skeleton while `isLoading`
  - [ ] Render error state if `error` exists
  - [ ] Map over `data.models` and render `ModelCard` components
- [ ] Cards container layout
  - [ ] Desktop (≥1024px): `flex flex-row gap-6 justify-start`
  - [ ] Tablet (768-1023px): `flex flex-row gap-4 overflow-x-auto pb-4`
  - [ ] Mobile (<768px): `flex flex-col gap-4`
  - [ ] Add scroll behavior: `scroll-smooth snap-x snap-mandatory` (tablet)
- [ ] Loading skeleton
  - [ ] Render 2-5 skeleton cards matching card dimensions
  - [ ] Use `animate-pulse` class
  - [ ] Gray rectangles for sections: `bg-gray-200 h-6 rounded`

### Task Group 5: Remove Model Functionality (AC: #4)
- [ ] Implement `handleRemoveModel` function in ComparisonPage
  - [ ] Accept model ID as parameter
  - [ ] Filter out removed ID from current `modelIds`
  - [ ] Update URL with new model list: `navigate(\`/compare?models=${newIds.join(',')}\`)`
  - [ ] TanStack Query will auto-refetch with new IDs
- [ ] Pass `onRemove` handler to ModelCard
  - [ ] Bind handler: `onRemove={() => handleRemoveModel(model.id)}`
- [ ] Add confirmation (optional, user testing)
  - [ ] Consider toast notification: "Model removed from comparison"

### Task Group 6: Empty Placeholder (AC: #6)
- [ ] Create empty card placeholder component
  - [ ] Same dimensions as ModelCard: `w-80 min-w-[320px]`
  - [ ] Dashed border: `border-2 border-dashed border-gray-300`
  - [ ] Centered content with icon (Plus from lucide-react)
  - [ ] Text: "Add Model to Compare"
  - [ ] Click handler: Opens model selector modal (Story 5.13 dependency)
- [ ] Show placeholder when <2 models
  - [ ] Render alongside existing cards
  - [ ] Make clickable with hover effect
  - [ ] Style: `hover:border-gray-400 hover:bg-gray-50 cursor-pointer`

### Task Group 7: Responsive Behavior (AC: #1, #5)
- [ ] Test desktop layout (≥1024px)
  - [ ] Cards displayed horizontally
  - [ ] Equal widths maintained
  - [ ] No horizontal scroll (unless >4 models)
- [ ] Test tablet layout (768-1023px)
  - [ ] Horizontal scroll enabled for >3 models
  - [ ] Snap scroll to card boundaries
  - [ ] Scroll indicators visible
- [ ] Test mobile layout (<768px)
  - [ ] Cards stack vertically
  - [ ] Full width on small screens
  - [ ] No horizontal scroll

### Task Group 8: Type Definitions (AC: #2)
- [ ] Update `apps/web/src/types/comparison.ts`
  - [ ] Define `ModelCardProps` interface
  - [ ] Ensure `ModelDto` imported from shared types
  - [ ] Define `ComparisonResponseDto` type matching backend
- [ ] Backend DTO reference (from tech-spec-epic-5.md):
  ```typescript
  interface ComparisonResponseDto {
    models: ModelDto[];
    metadata: ComparisonMetadataDto;
  }
  ```

### Task Group 9: Testing and Verification (AC: #1-6)
- [ ] Write unit test for ModelCard component
  - [ ] Render with mock ModelDto
  - [ ] Verify name, provider, pricing displayed
  - [ ] Test remove button click triggers onRemove
  - [ ] Test capability icons render correctly (checkmark vs dash)
  - [ ] Use Vitest + React Testing Library
- [ ] Write integration test for useComparisonData hook
  - [ ] Mock API response with MSW (Mock Service Worker)
  - [ ] Test successful data fetch
  - [ ] Test loading state
  - [ ] Test error handling (404, 500)
  - [ ] Verify TanStack Query caching behavior
- [ ] Manual E2E testing
  - [ ] Navigate to `/compare?models=1,2,3`
  - [ ] Verify 3 cards render horizontally (desktop)
  - [ ] Click X on card 2, verify URL updates to `?models=1,3`
  - [ ] Test with 5 models, verify horizontal scroll (tablet)
  - [ ] Test mobile layout (stack vertically)
  - [ ] Verify empty placeholder shows when <2 models

## Dev Notes

### Architecture Alignment
- **Component**: `ModelCard` is a presentational component (dumb component pattern)
- **Data Fetching**: `useComparisonData` hook abstracts TanStack Query logic (smart hook pattern)
- **API Layer**: `comparisonApi.fetchComparison` aligns with hexagonal architecture (outbound adapter)
- **Backend Endpoint**: Uses GET `/api/models/compare?ids=1,2,3` (defined in tech-spec-epic-5.md)

### TanStack Query Integration
Story introduces the first TanStack Query hook for Epic 5:
```typescript
// apps/web/src/hooks/useComparisonData.ts
export const useComparisonData = (modelIds: string[]) => {
  return useQuery({
    queryKey: ['comparison', modelIds.sort().join(',')],
    queryFn: () => comparisonApi.fetchComparison(modelIds),
    enabled: modelIds.length >= 2 && modelIds.length <= 5,
    staleTime: 5 * 60 * 1000, // 5 minutes
  });
};
```

Benefits:
- Automatic caching (5min stale time)
- Deduplication (multiple components using same IDs = single request)
- Background refetch on window focus
- Retry logic (3 attempts)

### Card Design Specification
Following UX specification (docs/ux-specification.md#Component Specifications):
- **Width**: 320px fixed (`w-80`) for alignment
- **Padding**: 24px (`p-6`)
- **Border**: 1px solid gray-200
- **Shadow**: Subtle shadow (`shadow-sm`)
- **Sections**: Header, Pricing, Capabilities, Context Window
- **Typography**: Inter font, size scale (xl, sm)

### Pricing Formatting
Use consistent currency formatting:
```typescript
const formatPrice = (price: number) => {
  return new Intl.NumberFormat('en-US', {
    style: 'currency',
    currency: 'USD',
    minimumFractionDigits: 2,
    maximumFractionDigits: 2
  }).format(price);
};

// Example: formatPrice(10.5) => "$10.50"
```

### Responsive Breakpoints
TailwindCSS breakpoints used:
- `sm`: 640px (not used in this story)
- `md`: 768px - Tablet breakpoint (horizontal scroll)
- `lg`: 1024px - Desktop breakpoint (no scroll)

### Prerequisites
- **Story 5.1**: Comparison page route must exist
- **Story 1.10**: GET `/api/models` endpoint (ModelDto structure exists)
- **Backend Epic 5 API**: GET `/api/models/compare` endpoint (will be implemented in parallel)

### Quality Gates
- TypeScript strict mode: ✅ Zero `any` types
- Cards render: ✅ 2-5 cards display horizontally (desktop)
- Remove functionality: ✅ Clicking X updates URL and refetches
- Responsive: ✅ Layout adapts to mobile/tablet/desktop
- Loading state: ✅ Skeleton shown during fetch
- Error handling: ✅ Error boundary catches API failures

### Project Structure Notes
```
apps/web/src/
├── components/
│   └── comparison/
│       └── ModelCard.tsx           # New component (this story)
├── hooks/
│   └── useComparisonData.ts        # New hook (this story)
├── api/
│   └── comparison.ts               # New API client (this story)
└── utils/
    └── formatters.ts               # Currency/number formatting (this story)
```

### References
- [Source: docs/tech-spec-epic-5.md#Data Models and Contracts] - ModelDto interface structure
- [Source: docs/tech-spec-epic-5.md#APIs and Interfaces] - GET `/api/models/compare` endpoint spec
- [Source: docs/tech-spec-epic-5.md#Acceptance Criteria] - AC-5.2: Model cards display requirements
- [Source: docs/epics.md#Story 5.2] - Original story with 6 acceptance criteria
- [Source: docs/solution-architecture.md#State Management] - TanStack Query for server state
- [Source: docs/ux-specification.md#Component Specifications] - Card component design system

## Dev Agent Record

### Context Reference

<!-- Path(s) to story context XML will be added here by context workflow -->

### Agent Model Used

claude-sonnet-4-5-20250929

### Debug Log References

### Completion Notes List

### File List
