# Story 3.1: Create Public Homepage with Basic Layout

Status: Ready

## Story

As a user,
I want to access the platform homepage,
So that I can begin comparing LLM models.

## Acceptance Criteria

### Functional Requirements

1. **Public Homepage Route** - Route `/` created and accessible without authentication
2. **Page Layout Structure** - Layout includes:
   - Header with platform name/logo placeholder
   - Main content area ready for comparison table
   - Footer with basic info (About, Contact links as placeholders)
   - Navigation bar with search input placeholder
3. **Responsive Layout** - Layout works on desktop (1920×1080, 1366×768), tablet (768×1024), and mobile (375×667)
4. **Loading State** - Page displays loading spinner while initializing
5. **Empty State** - If no models available, displays appropriate message
6. **Error State** - If API fails, displays error message with retry button

### Performance Requirements (from AC-P1)

7. **Initial Page Load** - Page loads in <2 seconds (cold cache)
8. **First Contentful Paint** - FCP <1.2s
9. **Largest Contentful Paint** - LCP <2.5s
10. **Cumulative Layout Shift** - CLS <0.1 (no layout jumps during load)

### Quality Requirements (from AC-Q1)

11. **TypeScript Strict Mode** - Zero `any` types in all components
12. **Code Quality** - Zero ESLint errors
13. **Accessibility** - Keyboard navigation works for all interactive elements
14. **ARIA Labels** - All navigation elements have appropriate ARIA labels

### Integration Requirements (from AC-I1)

15. **Frontend Shell Extension** - Extends Epic 1 Story 1.7 frontend shell (does not replace it)
16. **React Router Integration** - Uses existing React Router configuration from Epic 1.7

## Tasks / Subtasks

- [ ] **Task 1: Create Public Homepage Route and Layout Component** (AC: #1, #2, #15, #16)
  - [ ] Subtask 1.1: Create `HomePage.tsx` component in `apps/web/src/pages/`
  - [ ] Subtask 1.2: Add route `/` in `App.tsx` using React Router
  - [ ] Subtask 1.3: Create `PublicLayout.tsx` wrapper component with header/footer structure
  - [ ] Subtask 1.4: Add platform name/logo placeholder in header
  - [ ] Subtask 1.5: Add search input placeholder in navigation bar
  - [ ] Subtask 1.6: Create footer component with About/Contact links (placeholders)
  - [ ] Subtask 1.7: Verify route integration with Epic 1 Story 1.7 frontend shell

- [ ] **Task 2: Implement Responsive Layout** (AC: #3)
  - [ ] Subtask 2.1: Add TailwindCSS responsive breakpoints (mobile: 375px, tablet: 768px, desktop: 1280px+)
  - [ ] Subtask 2.2: Test layout at 375×667 (mobile) - navigation collapses, header stacks vertically
  - [ ] Subtask 2.3: Test layout at 768×1024 (tablet) - navigation horizontal, content area optimized
  - [ ] Subtask 2.4: Test layout at 1920×1080 (desktop) - full layout with max-width constraints
  - [ ] Subtask 2.5: Ensure no horizontal scrollbar on any breakpoint

- [ ] **Task 3: Add Loading, Empty, and Error States** (AC: #4, #5, #6)
  - [ ] Subtask 3.1: Create `LoadingSpinner.tsx` component with centered spinner
  - [ ] Subtask 3.2: Create `EmptyState.tsx` component with message "No models available"
  - [ ] Subtask 3.3: Create `ErrorState.tsx` component with error message and retry button
  - [ ] Subtask 3.4: Implement conditional rendering in `HomePage.tsx` based on data state
  - [ ] Subtask 3.5: Wire retry button to refetch data using TanStack Query

- [ ] **Task 4: Optimize Performance** (AC: #7, #8, #9, #10)
  - [ ] Subtask 4.1: Implement code splitting for HomePage route using React.lazy()
  - [ ] Subtask 4.2: Add Suspense boundary with loading fallback
  - [ ] Subtask 4.3: Optimize images (logo) - use WebP format, lazy loading
  - [ ] Subtask 4.4: Minimize initial bundle size - verify <200KB gzipped (AC-P4)
  - [ ] Subtask 4.5: Run Lighthouse audit - verify Performance Score ≥90
  - [ ] Subtask 4.6: Measure Core Web Vitals: FCP <1.2s, LCP <2.5s, CLS <0.1
  - [ ] Subtask 4.7: Add `<meta>` tags for proper viewport configuration (prevent CLS)

- [ ] **Task 5: Implement TypeScript Strict Mode and Code Quality** (AC: #11, #12)
  - [ ] Subtask 5.1: Verify `tsconfig.json` has `strict: true` enabled
  - [ ] Subtask 5.2: Type all component props with TypeScript interfaces
  - [ ] Subtask 5.3: Ensure zero `any` types - use proper type definitions
  - [ ] Subtask 5.4: Run `pnpm run type-check` - verify zero TypeScript errors
  - [ ] Subtask 5.5: Run `pnpm run lint` - verify zero ESLint errors

- [ ] **Task 6: Implement Accessibility** (AC: #13, #14)
  - [ ] Subtask 6.1: Add `<nav>` semantic HTML for navigation bar
  - [ ] Subtask 6.2: Add `<header>`, `<main>`, `<footer>` semantic HTML tags
  - [ ] Subtask 6.3: Add ARIA labels to all navigation links (`aria-label`)
  - [ ] Subtask 6.4: Test keyboard navigation (Tab key) - verify all interactive elements focusable
  - [ ] Subtask 6.5: Add skip-to-content link for screen readers
  - [ ] Subtask 6.6: Run axe-core automated accessibility tests - verify zero violations

- [ ] **Task 7: Write Tests** (AC: Test Coverage from AC-Q2)
  - [ ] Subtask 7.1: Create `HomePage.test.tsx` with Vitest + Testing Library
  - [ ] Subtask 7.2: Test loading state renders LoadingSpinner
  - [ ] Subtask 7.3: Test empty state renders EmptyState component
  - [ ] Subtask 7.4: Test error state renders ErrorState with retry button
  - [ ] Subtask 7.5: Test retry button triggers refetch
  - [ ] Subtask 7.6: Create E2E test `pageLoad.spec.ts` with Playwright
  - [ ] Subtask 7.7: E2E test verifies homepage loads with header, footer, main content area
  - [ ] Subtask 7.8: E2E test verifies responsive layout at 3 breakpoints

- [ ] **Task 8: Verify Integration with Epic 1** (AC: #15, #16)
  - [ ] Subtask 8.1: Confirm Epic 1 Story 1.7 frontend shell is preserved
  - [ ] Subtask 8.2: Verify React Router configuration from Epic 1 is extended (not replaced)
  - [ ] Subtask 8.3: Test API client from Epic 1 Story 1.7 is functional
  - [ ] Subtask 8.4: Verify TailwindCSS configuration from Epic 1 is used
  - [ ] Subtask 8.5: Run full test suite to ensure no regressions

## Dev Notes

### Project Structure Notes

**Frontend Structure (Hexagonal Architecture - Presentation Layer):**
```
apps/web/src/
├── pages/
│   └── HomePage.tsx             # NEW: Public homepage route component
├── components/
│   ├── layouts/
│   │   └── PublicLayout.tsx     # NEW: Header + main + footer wrapper
│   ├── common/
│   │   ├── LoadingSpinner.tsx   # NEW: Reusable loading state
│   │   ├── EmptyState.tsx       # NEW: Reusable empty state
│   │   └── ErrorState.tsx       # NEW: Reusable error state
│   └── ...                      # Epic 1 components preserved
├── __tests__/
│   └── pages/
│       └── HomePage.test.tsx    # NEW: Component tests
└── e2e/
    └── pageLoad.spec.ts         # NEW: Playwright E2E tests
```

**Alignment with Unified Project Structure:**
- Follows existing `apps/web/` frontend structure from Epic 1
- Uses `@components/*` path alias configured in Epic 1 Story 1.2
- Extends React Router setup from Epic 1 Story 1.7 (no replacement)

**Detected Conflicts/Variances:**
- None detected - Story 3.1 is purely additive to Epic 1 frontend shell

### Architecture Patterns and Constraints

**Hexagonal Architecture Context (Presentation Layer):**
- **Layer:** Presentation (React SPA)
- **Dependencies:** Domain ← Application ← Infrastructure ← **Presentation** (this layer)
- **Pattern:** Presentation components communicate with backend via HTTP (REST API)
- **State Management:** React local state for UI-only concerns (loading, error states)
- **Future Integration:** Story 3.2 will integrate TanStack Query for server state

**Multi-Layer Caching Strategy (Frontend):**
Story 3.1 sets up the UI shell, but does NOT implement caching yet. Future stories will add:
- **Story 3.2:** TanStack Query client-side caching (5min stale time)
- **Story 3.15:** Backend Redis caching (1hr TTL) + cache invalidation

**Performance Architecture:**
- **Code Splitting:** Use React.lazy() for route-level code splitting
- **Bundle Size Target:** <200KB gzipped for initial bundle (AC-P4)
- **Core Web Vitals:** FCP <1.2s, LCP <2.5s, CLS <0.1 (AC-P1)
- **Optimization Techniques:**
  - Lazy load images with `loading="lazy"`
  - Use WebP format for logo
  - Minimize initial CSS with TailwindCSS JIT
  - Defer non-critical JavaScript

**Responsive Design Strategy:**
- **Mobile-First Approach:** Design for 375px first, enhance for larger screens
- **Breakpoints (TailwindCSS):**
  - `sm`: 640px (mobile landscape)
  - `md`: 768px (tablet portrait)
  - `lg`: 1024px (tablet landscape)
  - `xl`: 1280px (desktop)
  - `2xl`: 1536px (large desktop)
- **Layout Pattern:** Stack vertically on mobile, horizontal layout on desktop

**Accessibility Requirements:**
- **WCAG 2.1 Level AA** compliance target
- **Keyboard Navigation:** All interactive elements reachable via Tab
- **ARIA Labels:** Navigation links, buttons, form inputs
- **Semantic HTML:** `<header>`, `<nav>`, `<main>`, `<footer>`
- **Skip Link:** "Skip to main content" for screen reader users

### Testing Standards Summary

**Test Levels (from Tech Spec Section 3.9):**
1. **Component Tests (Vitest + Testing Library):**
   - Test loading, empty, error states
   - Test responsive behavior (window resize simulation)
   - Coverage target: 60%+ for frontend utilities/hooks (AC-Q2)

2. **E2E Tests (Playwright):**
   - Test critical user flow: load homepage → see header/footer
   - Test responsive layout at 3 breakpoints (mobile, tablet, desktop)
   - Run in CI/CD pipeline on every pull request

3. **Accessibility Tests (axe-core):**
   - Automated ARIA label validation
   - Keyboard navigation verification
   - Color contrast checks

**Test Execution:**
```bash
# Component tests
pnpm run test:run src/pages/__tests__/HomePage.test.tsx

# E2E tests
pnpm run test:e2e e2e/pageLoad.spec.ts

# Accessibility tests (integrated into E2E)
# axe-core runs within Playwright tests
```

### References

**Epic 3 Requirements:**
- [Source: docs/tech-spec-epic-3.md#overview] - Epic 3 strategic positioning
- [Source: docs/tech-spec-epic-3.md#acceptance-criteria] - AC-F1, AC-P1, AC-Q1, AC-I1
- [Source: docs/epics.md#story-3.1] - Original story definition

**Architecture Context:**
- [Source: docs/solution-architecture.md#section-2.1] - Hexagonal Architecture pattern
- [Source: docs/solution-architecture.md#section-3.1] - Frontend architecture (React SPA)
- [Source: docs/tech-spec-epic-3.md#system-architecture-alignment] - Multi-layer caching strategy

**Testing Strategy:**
- [Source: docs/tech-spec-epic-3.md#section-3.9] - Test strategy summary
- [Source: docs/test-design-epic-3.md#test-coverage-plan] - P0 test scenarios

**Integration References:**
- [Source: docs/epics.md#story-1.7] - Epic 1 Story 1.7 frontend shell (prerequisite)
- [Source: docs/tech-spec-epic-3.md#epic-dependencies] - Epic 1 integration requirements

## Dev Agent Record

### Context Reference

- [Story Context XML](story-context-3.1.xml) - Generated 2025-10-21

### Agent Model Used

<!-- Agent model name and version will be added during implementation -->

### Debug Log References

<!-- Debug logs and implementation notes will be added here -->

### Completion Notes List

<!-- Post-implementation notes will be added here -->

### File List

<!-- Files created/modified during implementation will be listed here -->
