# Story 3.3: Integrate TanStack Table for Advanced Features

Status: Draft

## Story

As a developer,
I want TanStack Table integrated to replace the basic HTML table,
So that I can implement advanced features like sorting, filtering, and virtualization efficiently in subsequent stories.

## Acceptance Criteria

1. TanStack Table library (@tanstack/react-table v8.11.0+) installed and configured in the frontend
2. Models data rendered using TanStack Table component, replacing the basic HTML `<table>` from Story 3.2
3. Column definitions created for all model fields displayed in Story 3.2 (name, provider, input price, output price)
4. Table maintains the same visual appearance and functionality as Story 3.2 (data display, loading states, error states)
5. Performance verified with 50+ models - table renders in <500ms with smooth scrolling
6. Virtual scrolling implemented if performance degrades with 100+ models (optional based on testing)

## Tasks / Subtasks

- [ ] Task 1: Install and configure TanStack Table library (AC: #1)
  - [ ] Subtask 1.1: Run `pnpm add @tanstack/react-table` in apps/web directory
  - [ ] Subtask 1.2: Verify package installation in package.json (version @tanstack/react-table: ^8.11.0)
  - [ ] Subtask 1.3: Update TypeScript types if needed (@tanstack/table-core types)

- [ ] Task 2: Create TypeScript interfaces for table data (AC: #2, #3)
  - [ ] Subtask 2.1: Create `apps/web/src/types/table.ts` with ModelTableData interface matching ModelDto from Story 3.2
  - [ ] Subtask 2.2: Define column helper type using TanStack Table's createColumnHelper
  - [ ] Subtask 2.3: Import ModelDto type from api/types (ensure consistency with backend contract)

- [ ] Task 3: Define column definitions for model table (AC: #3)
  - [ ] Subtask 3.1: Create `apps/web/src/components/models/columns.tsx` file for column definitions
  - [ ] Subtask 3.2: Define column for "name" field (accessor: 'name', header: 'Model Name')
  - [ ] Subtask 3.3: Define column for "provider" field (accessor: 'provider', header: 'Provider')
  - [ ] Subtask 3.4: Define column for "inputPricePer1M" field (accessor: 'inputPricePer1M', header: 'Input Price', cell: format as currency)
  - [ ] Subtask 3.5: Define column for "outputPricePer1M" field (accessor: 'outputPricePer1M', header: 'Output Price', cell: format as currency)
  - [ ] Subtask 3.6: Export columns array for use in ModelTable component

- [ ] Task 4: Refactor ModelTable component to use TanStack Table (AC: #2, #4)
  - [ ] Subtask 4.1: Import useReactTable hook from @tanstack/react-table in apps/web/src/components/models/ModelTable.tsx
  - [ ] Subtask 4.2: Replace basic HTML table logic with useReactTable hook (pass data, columns, getCoreRowModel)
  - [ ] Subtask 4.3: Use table.getHeaderGroups() to render table headers
  - [ ] Subtask 4.4: Use table.getRowModel().rows to render table rows
  - [ ] Subtask 4.5: Use flexRender helper to render cells with proper typing
  - [ ] Subtask 4.6: Maintain existing TailwindCSS classes from Story 3.2 (preserve visual appearance)
  - [ ] Subtask 4.7: Ensure LoadingSpinner and ErrorAlert components still display correctly
  - [ ] Subtask 4.8: Verify data from useModels hook (TanStack Query) integrates with table.data

- [ ] Task 5: Test table rendering and functionality (AC: #4, #5)
  - [ ] Subtask 5.1: Verify table displays all models from GET /api/models endpoint
  - [ ] Subtask 5.2: Confirm loading state shows LoadingSpinner during data fetch
  - [ ] Subtask 5.3: Confirm error state shows ErrorAlert if API fails
  - [ ] Subtask 5.4: Test with 10+ models (Story 3.2 baseline) - verify no regression
  - [ ] Subtask 5.5: Test with 50+ models (seed additional test data if needed) - measure render time (<500ms target)
  - [ ] Subtask 5.6: Check browser DevTools Performance panel for render bottlenecks

- [ ] Task 6: Implement virtual scrolling if needed (AC: #6)
  - [ ] Subtask 6.1: Evaluate performance with 100+ models - if render time >1s, proceed with virtual scrolling
  - [ ] Subtask 6.2: Install @tanstack/react-virtual package: `pnpm add @tanstack/react-virtual`
  - [ ] Subtask 6.3: Integrate useVirtualizer hook with TanStack Table rows
  - [ ] Subtask 6.4: Configure virtualizer with estimateSize, overscan settings
  - [ ] Subtask 6.5: Test virtual scrolling with 100+ models - verify smooth scroll and <500ms render
  - [ ] Subtask 6.6: Document virtual scrolling configuration in dev notes

- [ ] Task 7: Manual testing and verification (AC: #4, #5)
  - [ ] Subtask 7.1: Test in Chrome DevTools responsive mode (desktop 1920px, tablet 768px, mobile 375px)
  - [ ] Subtask 7.2: Verify no console errors or warnings
  - [ ] Subtask 7.3: Verify no TypeScript errors in VSCode
  - [ ] Subtask 7.4: Test with browser cache cleared (ensure no stale data issues)
  - [ ] Subtask 7.5: Verify visual appearance matches Story 3.2 (alternating row colors, hover states, header styling)
  - [ ] Subtask 7.6: Test accessibility - keyboard navigation (Tab key navigates table cells)

## Dev Notes

### Architecture Patterns

**TanStack Table Headless Pattern:**
TanStack Table is a **headless UI library**, meaning it provides table logic (state management, data transformation, row/column APIs) without imposing any UI structure. This allows us to:
- Maintain full control over HTML structure and TailwindCSS styling
- Implement custom row rendering (e.g., checkboxes in Story 3.10, capability icons in Story 3.14)
- Add sorting (Story 3.4) and filtering (Stories 3.5-3.7) by simply adding table options, without rewriting table markup

**Component Composition:**
Following the pattern established in Epic 3 tech spec:
```
HomePage (apps/web/src/pages/HomePage.tsx)
└── ModelTable (apps/web/src/components/models/ModelTable.tsx)
    ├── useModels() hook → TanStack Query (server state)
    └── useReactTable() hook → TanStack Table (table state)
```

**State Management:**
- **Server state**: TanStack Query (`useModels` hook from Story 3.2) - handles data fetching, caching (5min stale time)
- **Table state**: TanStack Table (`useReactTable` hook) - handles row/column state, future sorting/filtering state
- **Client state**: Zustand (deferred to Story 3.10 for selection state)

**Performance Considerations:**
- TanStack Table uses memoization internally to avoid unnecessary re-renders
- `getCoreRowModel()` provides basic row rendering without sorting/filtering overhead
- Virtual scrolling (via @tanstack/react-virtual) is **optional** - only implement if testing shows >1s render time with 100+ models
- Current backend caching (Redis 1hr TTL from Epic 2) ensures fast API responses (<200ms)

### Project Structure Notes

**Files Created/Modified:**
```
apps/web/
├── src/
│   ├── components/
│   │   └── models/
│   │       ├── ModelTable.tsx (MODIFIED - refactor to TanStack Table)
│   │       └── columns.tsx (NEW - column definitions)
│   ├── types/
│   │   └── table.ts (NEW - TypeScript interfaces)
│   └── hooks/
│       └── useModels.ts (UNCHANGED - from Story 3.2)
└── package.json (MODIFIED - add @tanstack/react-table dependency)
```

**Alignment with Project Structure:**
- Component organization by domain: `components/models/` (not `components/tables/`)
- Type definitions centralized in `types/` folder
- Hooks follow naming convention: `use[ResourceName]` (e.g., useModels, useReactTable)

**Detected Conflicts:**
- None - TanStack Table is an additive change, replacing basic HTML table without affecting other components

### References

**Source Documents:**
- [Epic 3 Story 3.3 Definition - epics.md:514-527](file:///home/pablitxn/repos/bmad-method/llm-token-price/docs/epics.md#514) - Acceptance criteria and prerequisites
- [Tech Spec Epic 3 - Frontend Components - tech-spec-epic-3.md:340-384](file:///home/pablitxn/repos/bmad-method/llm-token-price/docs/tech-spec-epic-3.md#340) - Component architecture and file structure
- [Solution Architecture - Technology Stack - solution-architecture.md:36](file:///home/pablitxn/repos/bmad-method/llm-token-price/docs/solution-architecture.md#36) - TanStack Table v8.11.0 decision and justification
- [CLAUDE.md - Frontend Stack](file:///home/pablitxn/repos/bmad-method/llm-token-price/CLAUDE.md) - Path aliases (@components/*), state management patterns

**Technical References:**
- TanStack Table v8 Docs: https://tanstack.com/table/v8/docs/guide/introduction
- TanStack Table React Integration: https://tanstack.com/table/v8/docs/framework/react/react-table
- TanStack Virtual (if needed): https://tanstack.com/virtual/v3/docs/introduction
- Column Definitions Guide: https://tanstack.com/table/v8/docs/guide/column-defs

**Architecture Constraints:**
- **Hexagonal Architecture**: TanStack Table is a presentation layer concern - no changes to domain/application layers
- **Performance**: <500ms render target for 50 models, <1s for 100 models (from PRD FR032-FR035)
- **TypeScript Strict Mode**: Zero `any` types - use TanStack Table's generic types for full type safety
- **Component Composition**: Table logic isolated in ModelTable component - header, filter, basket components remain decoupled

**Dependencies from Previous Stories:**
- **Story 3.2**: useModels hook, ModelDto type, LoadingSpinner/ErrorAlert components, basic table structure
- **Story 3.1**: HomePage layout with main content area
- **Story 1.10**: GET /api/models endpoint returning model data
- **Story 1.2**: TailwindCSS configuration, Vite build setup

**Enables Future Stories:**
- **Story 3.4**: Column sorting (add getSortedRowModel to useReactTable config)
- **Story 3.5-3.7**: Filtering (add getFilteredRowModel, filterFns)
- **Story 3.10**: Checkbox selection (add selection column to column definitions)
- **Story 3.12**: Pagination or virtual scrolling (extend useReactTable with pagination state)

### Testing Strategy

**Manual Testing (Required for Story 3.3):**
1. **Functional Testing:**
   - Verify table displays all models from API
   - Confirm loading/error states work correctly
   - Test with different data volumes (10, 50, 100+ models)

2. **Visual Regression Testing:**
   - Compare table appearance before/after TanStack Table migration
   - Verify TailwindCSS classes render correctly (alternating rows, hover states)
   - Test responsive layout (desktop, tablet, mobile breakpoints)

3. **Performance Testing:**
   - Measure initial render time with 50 models (target: <500ms)
   - Test scroll performance (should feel instant, no jank)
   - Check Chrome DevTools Performance panel for layout thrashing

4. **Accessibility Testing:**
   - Keyboard navigation (Tab key should navigate table)
   - Screen reader compatibility (semantic HTML maintained)

**Automated Testing (Deferred to Epic 3 Completion):**
- Component tests for ModelTable (Vitest + Testing Library) - verify table renders with mock data
- E2E tests for table interactions (Playwright) - full user flow from page load to table display
- Visual regression tests (Percy/Chromatic) - catch unintended UI changes

**No Automated Tests Required for Story 3.3** per Epic 1/2 retrospective feedback - manual testing sufficient for foundational component setup. Automated tests will be added in bulk at epic completion.

## Dev Agent Record

### Context Reference

- `docs/stories/story-context-3.3.xml` (generated: 2025-10-21)

### Agent Model Used

<!-- Model name and version will be populated by dev agent -->

### Debug Log References

<!-- Links to debug logs will be added during development -->

### Completion Notes List

<!-- Dev agent will document completion notes here -->

### File List

<!-- Dev agent will list all files created/modified here -->
