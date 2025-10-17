# Story 2.3: Build Models List View in Admin Panel

Status: Ready

## Story

As an administrator,
I want to view all models in a table,
so that I can see current data and select models to edit.

## Acceptance Criteria

1. Admin models list page displays all models in table
2. Table shows: name, provider, input price, output price, status, last updated
3. Search box filters models by name or provider
4. "Add New Model" button navigates to creation form
5. "Edit" button on each row navigates to edit form
6. "Delete" button on each row triggers confirmation dialog

## Tasks / Subtasks

- [ ] **Task 1: Create admin models list page** (AC: #1, #2)
  - [ ] 1.1: Create `AdminModelsPage.tsx` in `/frontend/src/pages/admin`
  - [ ] 1.2: Create `ModelList.tsx` component in `/frontend/src/components/admin`
  - [ ] 1.3: Fetch models using `useQuery` hook from TanStack Query
  - [ ] 1.4: Display models in HTML table with columns: name, provider, input price, output price, status, last updated
  - [ ] 1.5: Add loading spinner while fetching data
  - [ ] 1.6: Add error message display if fetch fails
  - [ ] 1.7: Format prices with currency symbol and 6 decimal places
  - [ ] 1.8: Format timestamps with "Updated X days ago" using date-fns

- [ ] **Task 2: Implement search functionality** (AC: #3)
  - [ ] 2.1: Add search input field above table
  - [ ] 2.2: Implement client-side filtering by model name or provider
  - [ ] 2.3: Debounce search input (300ms) to avoid excessive re-renders
  - [ ] 2.4: Show "No models found" message when search has no results
  - [ ] 2.5: Add clear search button (X icon) to reset filter
  - [ ] 2.6: Preserve search state in URL query params for shareability

- [ ] **Task 3: Add "Add New Model" button** (AC: #4)
  - [ ] 3.1: Create "Add New Model" button in page header
  - [ ] 3.2: Style button with primary color and plus icon
  - [ ] 3.3: Navigate to `/admin/models/new` on click
  - [ ] 3.4: Create placeholder route for new model form

- [ ] **Task 4: Add action buttons to table rows** (AC: #5, #6)
  - [ ] 4.1: Add "Actions" column to table
  - [ ] 4.2: Create "Edit" button for each row (pencil icon)
  - [ ] 4.3: Navigate to `/admin/models/{id}/edit` on edit button click
  - [ ] 4.4: Create "Delete" button for each row (trash icon)
  - [ ] 4.5: Trigger confirmation dialog on delete button click
  - [ ] 4.6: Style action buttons with appropriate colors (blue for edit, red for delete)

- [ ] **Task 5: Create backend API endpoint for admin models list** (AC: #1)
  - [ ] 5.1: Create `GET /api/admin/models` endpoint in `AdminModelsController.cs`
  - [ ] 5.2: Return all models including inactive (unlike public API)
  - [ ] 5.3: Include full model details, capabilities, and update timestamps
  - [ ] 5.4: Add JWT authentication requirement to endpoint
  - [ ] 5.5: Do NOT cache admin endpoint (needs real-time data)
  - [ ] 5.6: Return models sorted by updated_at DESC (most recently updated first)

- [ ] **Task 6: Add table sorting**
  - [ ] 6.1: Make table headers clickable for sorting
  - [ ] 6.2: Implement client-side sorting by: name, provider, price, status, updated_at
  - [ ] 6.3: Add sort indicator (up/down arrow) to active column
  - [ ] 6.4: Toggle ascending/descending on header click
  - [ ] 6.5: Preserve sort state in URL query params

- [ ] **Task 7: Add pagination or virtual scrolling**
  - [ ] 7.1: Decide on pagination (10/25/50 per page) or virtual scrolling
  - [ ] 7.2: Implement chosen approach with client-side data
  - [ ] 7.3: Add pagination controls if using pagination
  - [ ] 7.4: Show "Showing X-Y of Z models" count
  - [ ] 7.5: Preserve pagination state in URL query params

- [ ] **Task 8: Add testing**
  - [ ] 8.1: Write component tests for ModelList (Vitest)
  - [ ] 8.2: Test search filtering works correctly
  - [ ] 8.3: Test sort functionality on all columns
  - [ ] 8.4: Test action buttons navigate to correct routes
  - [ ] 8.5: Write integration tests for admin models API endpoint (xUnit)
  - [ ] 8.6: Test endpoint requires authentication

## Dev Notes

### Architecture Context

**Admin vs. Public API:**
- Admin endpoint returns ALL models (including inactive, beta, deprecated)
- Public endpoint returns only active models
- Admin endpoint NOT cached (real-time updates needed for data management)
- Admin endpoint requires JWT authentication

**Component Architecture:**
```
AdminModelsPage
├── SearchBar (debounced input)
├── AddNewModelButton
└── ModelList (table)
    └── ModelRow[] (with edit/delete actions)
```

**State Management:**
- Server state: TanStack Query for fetching models
- UI state: Local useState for search, sort, pagination
- URL state: Search, sort, pagination preserved in query params

### Project Structure Notes

**Backend Files to Create:**
```
/backend/src/Backend.API/Controllers/Admin/
  └── AdminModelsController.cs (add GET endpoint)

/backend/src/Backend.Application/Services/
  └── AdminModelService.cs (query service)

/backend/src/Backend.Application/DTOs/
  └── AdminModelListDto.cs
```

**Frontend Files to Create:**
```
/frontend/src/pages/admin/
  └── AdminModelsPage.tsx

/frontend/src/components/admin/
  ├── ModelList.tsx
  ├── ModelRow.tsx
  └── SearchBar.tsx

/frontend/src/hooks/
  └── useAdminModels.ts (TanStack Query hook)

/frontend/src/api/
  └── admin.ts (add getAdminModels function)
```

### Implementation Details

**Admin Models API Contract:**
```typescript
// GET /api/admin/models
Response: {
  data: Array<{
    id: string
    name: string
    provider: string
    version: string | null
    status: 'active' | 'deprecated' | 'beta'
    inputPricePer1M: number
    outputPricePer1M: number
    currency: string
    capabilities: {
      contextWindow: number
      supportsFunctionCalling: boolean
      supportsVision: boolean
      // ... other capabilities
    }
    createdAt: string  // ISO 8601
    updatedAt: string  // ISO 8601
  }>
  meta: {
    totalCount: number
    timestamp: string
  }
}
```

**Table Columns:**
```typescript
const columns = [
  { key: 'name', label: 'Model Name', sortable: true },
  { key: 'provider', label: 'Provider', sortable: true },
  { key: 'inputPricePer1M', label: 'Input Price', sortable: true, format: formatPrice },
  { key: 'outputPricePer1M', label: 'Output Price', sortable: true, format: formatPrice },
  { key: 'status', label: 'Status', sortable: true, format: formatStatus },
  { key: 'updatedAt', label: 'Last Updated', sortable: true, format: formatRelativeTime },
  { key: 'actions', label: 'Actions', sortable: false }
]
```

**Search Filter Logic:**
```typescript
const filteredModels = models.filter(model =>
  model.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
  model.provider.toLowerCase().includes(searchTerm.toLowerCase())
)
```

**Status Badge Styling:**
- `active`: Green badge
- `deprecated`: Red badge
- `beta`: Yellow/orange badge

### References

- [Tech Spec Epic 2: docs/tech-spec-epic-2-8-summary.md#Epic 2]
- [Solution Architecture: docs/solution-architecture.md#7.1 Public API Contracts]
- [Epics Document: docs/epics.md#Story 2.3]
- [Admin API Design: docs/solution-architecture.md#7.2 Admin API Contracts]

### Testing Strategy

**Component Tests:**
- ModelList renders all models correctly
- Search filters models by name and provider
- Sort toggles ascending/descending order
- Edit button navigates to edit route with correct ID
- Delete button triggers confirmation dialog

**Integration Tests:**
- GET /api/admin/models returns all models (including inactive)
- Endpoint requires valid JWT token
- Unauthorized requests return 401
- Response includes all required fields

**E2E Tests (deferred to later story):**
- Admin login → Navigate to models page → See models list → Search/sort works

## Dev Agent Record

### Context Reference

- `docs/stories/story-context-2.3.xml` (Generated: 2025-10-17)

### Agent Model Used

claude-sonnet-4-5-20250929

### Debug Log References

### Completion Notes List

### File List
