# Story 5.1: Create Comparison Page Route and Layout

Status: Draft

## Story

As a user,
I want a dedicated comparison page for selected models,
so that I can focus on comparing my shortlisted options in a structured interface.

## Acceptance Criteria

1. Comparison page route created at `/compare` and accessible via React Router
2. Page layout includes space for model cards section (top) and charts section (below)
3. "Compare Selected" button in comparison basket (Story 3.11) navigates to `/compare` page
4. URL includes selected model IDs as query parameters (e.g., `/compare?models=id1,id2,id3`)
5. Page displays empty state message if <2 models selected: "Select at least 2 models to compare"
6. Back button or "Return to Table" link navigates back to main table (`/`)

## Tasks / Subtasks

### Task Group 1: Create Comparison Page Route (AC: #1, #4)
- [ ] Add `/compare` route to React Router configuration in `App.tsx`
  - [ ] Import `ComparisonPage` component
  - [ ] Add route: `<Route path="/compare" element={<ComparisonPage />} />`
  - [ ] Verify route accessible at `http://localhost:5173/compare`
- [ ] Create `ComparisonPage` component file: `apps/web/src/pages/ComparisonPage/ComparisonPage.tsx`
  - [ ] Initialize basic functional component with TypeScript
  - [ ] Export component as default export
- [ ] Create `useComparisonParams` custom hook to extract model IDs from URL
  - [ ] Use `useSearchParams` from `react-router-dom`
  - [ ] Parse `models` query parameter (comma-separated IDs)
  - [ ] Return array of model ID strings: `{ modelIds: string[] }`
  - [ ] Handle empty/invalid params gracefully (return empty array)

### Task Group 2: Page Layout Structure (AC: #2)
- [ ] Create comparison page layout with TailwindCSS
  - [ ] Container: `max-w-7xl mx-auto px-4 py-8`
  - [ ] Header section with page title "Model Comparison"
  - [ ] Model cards section: `flex flex-row gap-4` (horizontal layout)
  - [ ] Charts section placeholder: `mt-8 space-y-6`
  - [ ] Use responsive breakpoints: `md:`, `lg:` for layout adaptations
- [ ] Add semantic HTML structure
  - [ ] Use `<main>` for page content
  - [ ] Use `<section>` for model cards and charts areas
  - [ ] Add `aria-label` attributes for accessibility

### Task Group 3: Navigation Integration (AC: #3, #6)
- [ ] Update comparison basket component (from Story 3.11) to include "Compare Selected" button
  - [ ] Import `useNavigate` from `react-router-dom`
  - [ ] Add button: "Compare Selected" (enabled when 2-5 models selected)
  - [ ] onClick: `navigate(\`/compare?models=${selectedIds.join(',')}\`)`
  - [ ] Style button with primary color (green-600) and disabled state
- [ ] Add "Back to Table" navigation on comparison page
  - [ ] Create back button/link in header: "← Back to Models"
  - [ ] Use `useNavigate` to navigate to `/`
  - [ ] Style as secondary link (text-gray-600 hover:text-gray-900)

### Task Group 4: Empty State Handling (AC: #5)
- [ ] Implement empty state component
  - [ ] Check if `modelIds.length < 2`
  - [ ] Display centered message: "Select at least 2 models to compare"
  - [ ] Add icon (AlertCircle from lucide-react)
  - [ ] Include "Go to Models Table" button linking to `/`
  - [ ] Style with `text-center text-gray-500 py-12`
- [ ] Add validation message for >5 models
  - [ ] Check if `modelIds.length > 5`
  - [ ] Display warning: "Maximum 5 models can be compared at once"
  - [ ] Auto-trim to first 5 models or redirect to table

### Task Group 5: Component File Structure
- [ ] Create comparison page directory: `apps/web/src/pages/ComparisonPage/`
- [ ] Create component files:
  - [ ] `ComparisonPage.tsx` - Main page component
  - [ ] `ComparisonPage.module.css` (if needed, otherwise use Tailwind)
  - [ ] `useComparisonParams.ts` - Custom hook for URL params
  - [ ] `index.ts` - Barrel export
- [ ] Create placeholder components directory: `apps/web/src/components/comparison/`
  - [ ] This will house ModelCard, ComparisonTable components in future stories

### Task Group 6: Type Definitions
- [ ] Create comparison types file: `apps/web/src/types/comparison.ts`
  - [ ] Define `ComparisonPageProps` interface (if props needed)
  - [ ] Define `ComparisonParams` type: `{ modelIds: string[] }`
  - [ ] Export types for use in other components
- [ ] Ensure strict TypeScript compliance
  - [ ] No `any` types
  - [ ] All props typed with interfaces
  - [ ] URL params properly typed

### Task Group 7: Testing and Verification
- [ ] Write unit test for `useComparisonParams` hook
  - [ ] Test parsing comma-separated IDs from URL
  - [ ] Test empty URL parameters
  - [ ] Test invalid/malformed input
  - [ ] Use Vitest + React Testing Library
- [ ] Write integration test for ComparisonPage
  - [ ] Test route accessibility (`/compare`)
  - [ ] Test empty state (<2 models)
  - [ ] Test navigation from basket button
  - [ ] Test back button navigation
- [ ] Manual verification
  - [ ] Navigate to `/compare?models=1,2,3` - page renders
  - [ ] Navigate to `/compare` (no params) - empty state shows
  - [ ] Click "Compare Selected" from table - navigates with IDs in URL
  - [ ] Click "Back to Table" - returns to `/`

## Dev Notes

### Architecture Alignment
- **Component Location**: `apps/web/src/pages/ComparisonPage/ComparisonPage.tsx` (page-level component)
- **Routing**: Uses React Router v7.9.4 (already configured in Story 1.7)
- **State Management**: URL state via query parameters (shareable links)
- **Styling**: TailwindCSS 4 utility classes (consistent with design system)

### React Router URL State Pattern
Story uses URL-driven state (not Zustand) for shareability:
```typescript
// Read from URL
const [searchParams] = useSearchParams();
const ids = searchParams.get('models')?.split(',') || [];

// Write to URL
const navigate = useNavigate();
navigate(`/compare?models=${ids.join(',')}`);
```

This enables:
- Shareable comparison links (FR requirement)
- Browser back/forward button support
- Bookmark-able comparisons

### Empty State Design
Following UX specification (docs/ux-specification.md):
- Centered layout with icon
- Clear action-oriented message
- Primary CTA button to guide user back to table
- Consistent with other empty states (404, search no results)

### Prerequisites
- **Story 3.11** must be complete (comparison basket UI with selected models)
- **Story 1.7** already implemented React Router setup
- No backend changes required for this story

### Quality Gates
- TypeScript strict mode: ✅ Zero `any` types
- Route accessible: ✅ `/compare` loads without errors
- URL state: ✅ Query params parse correctly
- Navigation: ✅ Back button works, basket button navigates
- Empty state: ✅ Shows when <2 models
- Responsive: ✅ Layout adapts to mobile/tablet/desktop

### Project Structure Notes
```
apps/web/src/
├── pages/
│   └── ComparisonPage/
│       ├── ComparisonPage.tsx        # Main page (this story)
│       ├── useComparisonParams.ts    # URL parsing hook (this story)
│       └── index.ts
├── components/
│   └── comparison/                   # Future: ModelCard, ComparisonTable (Stories 5.2-5.3)
└── types/
    └── comparison.ts                 # Type definitions (this story)
```

Aligns with hexagonal architecture:
- **Presentation Layer**: ComparisonPage is a page-level component
- **No Business Logic**: Pure UI layout, delegates data fetching to hooks (Story 5.2)
- **Separation of Concerns**: URL parsing isolated in custom hook

### References
- [Source: docs/tech-spec-epic-5.md#APIs and Interfaces] - URL format: `/compare?models=id1,id2,id3`
- [Source: docs/tech-spec-epic-5.md#Acceptance Criteria] - AC-5.1: Route and URL state requirements
- [Source: docs/epics.md#Story 5.1] - Original story definition with 6 acceptance criteria
- [Source: docs/solution-architecture.md#Frontend Components] - React Router v7.9.4, component structure
- [Source: docs/ux-specification.md#Component Specifications] - Empty state patterns, navigation design
- [Source: docs/PRD.md#FR009] - Functional requirement: Select 2-5 models for comparison

## Dev Agent Record

### Context Reference

<!-- Path(s) to story context XML will be added here by context workflow -->

### Agent Model Used

claude-sonnet-4-5-20250929

### Debug Log References

### Completion Notes List

### File List
