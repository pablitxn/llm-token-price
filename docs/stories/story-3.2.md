# Story 3.2: Fetch and Display Models in Basic Table

Status: Draft

## Story

As a user,
I want to see all available models in a table,
So that I can get an overview of options.

## Acceptance Criteria

1. Frontend fetches models from GET `/api/models` endpoint
2. Basic HTML table displays models with columns: name, provider, input price, output price
3. Data loads automatically on page mount
4. Loading spinner shown while fetching data
5. Error message displayed if API fails
6. Table displays 10+ models with sample data

## Tasks / Subtasks

### Task 1: Implement TanStack Query Hook for Models Data (AC: #1, #3)

- [ ] Install TanStack Query (@tanstack/react-query) if not already present
- [ ] Create API client function `fetchModels()` in `src/api/models.ts` to call GET `/api/models`
- [ ] Create custom hook `useModels()` in `src/hooks/useModels.ts` using `useQuery`
- [ ] Configure query with 5min staleTime and 30min cacheTime (per Architecture)
- [ ] Export hook for use in components

### Task 2: Create Basic Table Component (AC: #2, #6)

- [ ] Create `ModelTable.tsx` component in `src/components/models/`
- [ ] Define table columns: name, provider, input_price_per_1m, output_price_per_1m
- [ ] Render basic HTML `<table>` with proper semantic markup (`<thead>`, `<tbody>`)
- [ ] Map over models data and render rows
- [ ] Format pricing values with currency symbol and proper decimal places
- [ ] Add basic TailwindCSS styling for readability (borders, padding, alternating rows)

### Task 3: Integrate Table into HomePage (AC: #3)

- [ ] Import `ModelTable` component in `HomePage.tsx`
- [ ] Call `useModels()` hook to fetch data
- [ ] Pass `models` data to `ModelTable` component
- [ ] Ensure table renders in main content area defined in Story 3.1

### Task 4: Implement Loading State (AC: #4)

- [ ] Create `LoadingSpinner` component in `src/components/ui/` (or reuse if exists)
- [ ] Check `isLoading` state from `useModels()` hook
- [ ] Conditionally render `LoadingSpinner` when `isLoading === true`
- [ ] Hide table while loading
- [ ] Add message: "Loading models..."

### Task 5: Implement Error Handling (AC: #5)

- [ ] Create `ErrorMessage` component in `src/components/ui/` for displaying errors
- [ ] Check `isError` and `error` states from `useModels()` hook
- [ ] Conditionally render `ErrorMessage` when `isError === true`
- [ ] Display user-friendly error message: "Failed to load models. Please try again later."
- [ ] Add retry button that calls `refetch()` from TanStack Query
- [ ] Log detailed error to console for debugging

### Task 6: Verify Backend Endpoint (AC: #1, #6)

- [ ] Confirm GET `/api/models` endpoint exists from Story 1.10
- [ ] Test endpoint returns JSON array with at least 10 models
- [ ] Verify response includes: id, name, provider, input_price_per_1m, output_price_per_1m
- [ ] Check CORS is configured to allow frontend origin (localhost:5173 in dev)
- [ ] If endpoint missing required fields, update backend to include them

### Task 7: Test End-to-End Flow (All ACs)

- [ ] Start backend API server (dotnet run)
- [ ] Start frontend dev server (pnpm run dev)
- [ ] Navigate to homepage (http://localhost:5173)
- [ ] Verify loading spinner appears briefly
- [ ] Verify table renders with 10+ models
- [ ] Verify all 4 columns display correctly
- [ ] Test error handling by stopping backend → verify error message appears
- [ ] Test retry button → verify data reloads when backend restarted

## Dev Notes

### Architecture Patterns

**Server State Management:**
This story establishes the canonical TanStack Query pattern reused across all epics:
```typescript
export const useModels = () => {
  return useQuery({
    queryKey: ['models'],
    queryFn: fetchModels,
    staleTime: 5 * 60 * 1000,  // 5 min (from Architecture)
    cacheTime: 30 * 60 * 1000, // 30 min
  });
};
```

**Component Composition:**
```
HomePage
├── (Loading state from useModels)
├── (Error state from useModels)
└── ModelTable (receives models data)
```

**API Client Pattern:**
```typescript
// src/api/models.ts
export const fetchModels = async (): Promise<ModelDto[]> => {
  const response = await fetch(`${API_BASE_URL}/api/models`);
  if (!response.ok) throw new Error('Failed to fetch models');
  return response.json();
};
```

### Backend Context

- **Endpoint:** GET `/api/models` (created in Epic 1, Story 1.10)
- **Response Format:** JSON array of model objects
- **Expected Fields:** id, name, provider, input_price_per_1m, output_price_per_1m, currency (optional for now)
- **Performance:** Should complete in <2s (Epic 1 measured 306ms)

### Frontend Technologies

- **TanStack Query v5** - Server state management
- **React 19** - Component framework
- **TypeScript (strict mode)** - Zero `any` types allowed
- **TailwindCSS 4** - Styling
- **Vite** - Build tool with HMR

### Data Flow

1. `HomePage` mounts
2. `useModels()` hook triggers
3. TanStack Query checks cache (miss on first load)
4. Calls `fetchModels()` API client
5. API request to backend `/api/models`
6. Backend returns JSON (from database or Redis cache)
7. TanStack Query caches response (5min stale time)
8. Component receives `models` data
9. `ModelTable` renders rows

### Error Scenarios to Handle

- **Network Error:** API unreachable → Show error message with retry
- **500 Server Error:** Backend crash → Show error message
- **Empty Response:** No models in database → Show "No models available" message
- **Malformed Response:** Invalid JSON → Caught by fetchModels error handling

### Testing Strategy

**Manual Testing (Required for Story Approval):**
1. Happy path: Data loads and displays correctly
2. Loading state: Spinner visible during fetch
3. Error handling: Error message on API failure
4. Retry functionality: Retry button reloads data
5. Data accuracy: Verify pricing values display correctly

**Future Automated Tests (Post-Story):**
- Unit test: `useModels` hook with MSW mock
- Integration test: `ModelTable` renders with sample data
- E2E test: Full page load with Playwright (Epic 1 test framework)

### Performance Considerations

- **Initial Load Target:** <2s (includes API call + render)
- **TanStack Query Caching:** Reduces repeated API calls
- **Basic HTML Table:** Fast render for 10-20 models (Story 3.3 upgrades to TanStack Table for virtualization)

### Project Structure Notes

**New Files Created:**
- `apps/web/src/hooks/useModels.ts` - TanStack Query hook
- `apps/web/src/api/models.ts` - API client for models endpoint
- `apps/web/src/components/models/ModelTable.tsx` - Table component
- `apps/web/src/components/ui/LoadingSpinner.tsx` - Loading state UI
- `apps/web/src/components/ui/ErrorMessage.tsx` - Error state UI

**Modified Files:**
- `apps/web/src/pages/HomePage.tsx` - Integrate ModelTable and data fetching

**Alignment with Unified Structure:**
- Components organized by domain (`models/`, `ui/`)
- Hooks in dedicated `hooks/` directory
- API client functions in `api/` directory
- Follows path aliases: `@components/*`, `@api/*` (defined in vite.config.ts)

### References

- [Source: docs/epics.md#Story 3.2 - Lines 496-510]
- [Source: docs/tech-spec-epic-3.md - Frontend Components Section]
- [Source: docs/solution-architecture.md#Multi-Layer Caching Strategy]
- [Source: docs/PRD.md#FR006, FR032]

### Dependencies from Previous Stories

- **Story 1.10:** GET `/api/models` endpoint must exist and return valid data
- **Story 1.7:** Frontend application shell with React Router configured
- **Story 3.1:** HomePage layout with header and main content area

### Constraints

- **TypeScript Strict Mode:** All types must be explicitly defined, no `any` types
- **Architectural:** Frontend cannot directly access database - all data via REST API
- **Performance:** Table must render in <100ms after data received (Story 3.12 adds virtualization if needed)
- **Responsive:** Basic table should be readable on desktop (mobile card view in Story 8.2)

### Security Considerations

- **XSS Protection:** Pricing values are numeric (safe), but model names/providers from user input could contain HTML - ensure React's default escaping is used
- **API Authentication:** Not required for public read-only endpoint (admin endpoints have auth from Epic 2)
- **CORS:** Ensure backend allows frontend origin in development

## Dev Agent Record

### Context Reference

<!-- Path to story context XML will be added here by context workflow -->

### Agent Model Used

<!-- To be filled during implementation -->

### Debug Log References

<!-- To be added during development -->

### Completion Notes List

<!-- To be added when story is completed -->

### File List

<!-- To be added with actual file paths created/modified -->
