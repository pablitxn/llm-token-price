# Story 3.2: Fetch and Display Models in Basic Table

Status: Ready

## Story

As a user,
I want to see all available models in a table,
So that I can get overview of options.

## Acceptance Criteria

1. Frontend fetches models from GET `/api/models`
2. Basic HTML table displays models with columns: name, provider, input price, output price
3. Data loads on page mount
4. Loading spinner shown while fetching
5. Error message displayed if API fails
6. Table displays 10+ models with sample data

## Tasks / Subtasks

- [ ] Task 1: Create API Client Function for Models (AC: 1)
  - [ ] Create `models.ts` in `/frontend/src/api/`
  - [ ] Define ModelDto TypeScript interface matching backend DTO
  - [ ] Implement `fetchModels()` function using axios
  - [ ] Add error handling with try-catch
  - [ ] Test API call returns models array

- [ ] Task 2: Create TanStack Query Hook (AC: 1, 3, 4)
  - [ ] Create `useModels.ts` in `/frontend/src/hooks/`
  - [ ] Implement `useModels` hook with useQuery
  - [ ] Configure stale time (5 minutes per architecture)
  - [ ] Configure cache time (30 minutes)
  - [ ] Return data, isLoading, isError, error states

- [ ] Task 3: Create Basic Model Table Component (AC: 2)
  - [ ] Create `ModelTable.tsx` in `/frontend/src/components/models/`
  - [ ] Implement basic HTML <table> structure
  - [ ] Add table headers: Name, Provider, Input Price, Output Price
  - [ ] Map models array to table rows
  - [ ] Format prices with currency symbol and 4 decimals
  - [ ] Style table with TailwindCSS (border, hover states)

- [ ] Task 4: Add Loading State UI (AC: 4)
  - [ ] Create LoadingSpinner component if not exists
  - [ ] Show spinner when `isLoading === true`
  - [ ] Center spinner in content area
  - [ ] Add "Loading models..." text below spinner

- [ ] Task 5: Add Error State UI (AC: 5)
  - [ ] Create error alert component/section
  - [ ] Show error message when `isError === true`
  - [ ] Display user-friendly error text (not raw error)
  - [ ] Add "Retry" button that refetches data
  - [ ] Style error state with red border/background

- [ ] Task 6: Integrate Table into HomePage (AC: 3)
  - [ ] Import ModelTable into HomePage.tsx
  - [ ] Replace loading placeholder with ModelTable
  - [ ] Pass models data from useModels hook
  - [ ] Handle loading and error states in HomePage
  - [ ] Verify table renders on page mount

- [ ] Task 7: Testing and Verification (AC: 6)
  - [ ] Verify backend GET /api/models returns 10+ models (from Story 1.10 seeding)
  - [ ] Test page loads and fetches models automatically
  - [ ] Test loading spinner appears briefly during fetch
  - [ ] Test table displays all model data correctly
  - [ ] Test price formatting (e.g., "$0.0015" for input price)
  - [ ] Test error handling: stop backend, verify error message shows

## Dev Notes

### Project Structure Notes

**Prerequisites:** 
- Story 1.10 (GET API for Models) must be complete
- Backend must be running with seeded data

**File Locations:**
- /frontend/src/api/models.ts (new) - API client functions
- /frontend/src/hooks/useModels.ts (new) - TanStack Query hook
- /frontend/src/components/models/ModelTable.tsx (new) - Table component
- /frontend/src/pages/HomePage.tsx (update) - Integrate table

### References

- [Source: docs/epics.md#Story 3.2] - Original acceptance criteria
- [Source: docs/solution-architecture.md#7.1] - GET /api/models API contract
- [Source: docs/solution-architecture.md#2.3] - TanStack Query configuration (5min stale, 30min cache)

## Dev Agent Record

### Context Reference

### Agent Model Used

### Debug Log References

### Completion Notes List

### File List
