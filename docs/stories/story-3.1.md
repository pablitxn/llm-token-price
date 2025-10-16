# Story 3.1: Create Public Homepage with Basic Layout

Status: Draft

## Story

As a user,
I want to access the platform homepage,
So that I can begin comparing LLM models.

## Acceptance Criteria

1. Public homepage route (`/`) created
2. Page layout includes header with platform name/logo
3. Main content area ready for comparison table
4. Footer with basic info (about, contact links placeholder)
5. Navigation bar includes search input placeholder
6. Responsive layout works on desktop, tablet, mobile

## Tasks / Subtasks

- [ ] Task 1: Create HomePage Component (AC: 1, 3)
  - [ ] Create `HomePage.tsx` in `/frontend/src/pages/`
  - [ ] Implement component structure with main content container
  - [ ] Add semantic HTML structure (`<main>`, `<article>`)
  - [ ] Configure route in App.tsx for path `/`
  - [ ] Verify route renders and is accessible

- [ ] Task 2: Create Header Component with Platform Branding (AC: 2, 5)
  - [ ] Create `Header.tsx` in `/frontend/src/components/layout/`
  - [ ] Add platform name/logo container (use text logo for MVP)
  - [ ] Implement navigation bar structure
  - [ ] Add search input placeholder (disabled/styling only, no functionality)
  - [ ] Style with TailwindCSS following design system
  - [ ] Make header sticky on scroll

- [ ] Task 3: Create Footer Component (AC: 4)
  - [ ] Create `Footer.tsx` in `/frontend/src/components/layout/`
  - [ ] Add "About" link placeholder (#about)
  - [ ] Add "Contact" link placeholder (#contact)
  - [ ] Add copyright notice with current year
  - [ ] Style with TailwindCSS (subtle background, border-top)
  - [ ] Position at bottom of viewport

- [ ] Task 4: Update Layout Component Integration (AC: 1-4)
  - [ ] Import Header and Footer into Layout component
  - [ ] Ensure Layout wraps all routes
  - [ ] Test header appears on all pages
  - [ ] Test footer appears on all pages
  - [ ] Verify layout spacing (header height, footer placement)

- [ ] Task 5: Implement Responsive Design (AC: 6)
  - [ ] Add Tailwind responsive breakpoints (sm:, md:, lg:)
  - [ ] Test header on mobile (hamburger menu not required for MVP, simplified nav)
  - [ ] Test content area responsiveness (max-width, padding)
  - [ ] Test footer on mobile devices
  - [ ] Verify layout doesn't break on tablet (768px-1024px)
  - [ ] Test on mobile (375px minimum width)

- [ ] Task 6: Add Main Content Placeholder for Table (AC: 3)
  - [ ] Create placeholder div for comparison table
  - [ ] Add loading state component (spinner or skeleton)
  - [ ] Center placeholder in main content area
  - [ ] Add helpful message: "Loading model data..."
  - [ ] Verify spacing and alignment

- [ ] Task 7: Testing and Verification
  - [ ] Manual test: Navigate to `/` and verify page renders
  - [ ] Manual test: Check header, footer, content area visible
  - [ ] Manual test: Verify responsive behavior at 375px, 768px, 1024px, 1920px
  - [ ] Browser test: Chrome, Firefox, Safari (if available)
  - [ ] Accessibility: Verify semantic HTML structure
  - [ ] Accessibility: Test keyboard navigation through header links

## Dev Notes

### Project Structure Notes

**Prerequisites:** Story 1.7 (Frontend Application Shell) must be complete

**File Locations:**
- /frontend/src/pages/HomePage.tsx (new)
- /frontend/src/components/layout/Header.tsx (new)
- /frontend/src/components/layout/Footer.tsx (new)
- /frontend/src/components/layout/Layout.tsx (update)
- /frontend/src/App.tsx (update route configuration)

### References

- [Source: docs/epics.md#Story 3.1] - Original acceptance criteria
- [Source: docs/ux-specification.md#Screen Layouts] - Design system colors, typography, spacing
- [Source: docs/solution-architecture.md#2.3] - Component hierarchy and routing structure

## Dev Agent Record

### Context Reference

<!-- Path(s) to story context XML will be added here by context workflow -->

### Agent Model Used

### Debug Log References

### Completion Notes List

### File List
