# Story 2.2: Admin Dashboard Layout

Status: Ready

## Story

As an administrator,
I want admin dashboard with navigation,
so that I can access different management functions.

## Acceptance Criteria

1. Admin layout component created with sidebar navigation
2. Navigation menu includes: Models, Benchmarks, Dashboard (placeholder)
3. Main content area renders selected section
4. Header shows logged-in admin name and logout button
5. Responsive layout works on desktop and tablet

## Tasks / Subtasks

- [ ] **Task 1: Create admin layout component** (AC: #1, #3)
  - [ ] 1.1: Create `AdminLayout.tsx` in `/frontend/src/components/layout`
  - [ ] 1.2: Implement sidebar with collapsible navigation
  - [ ] 1.3: Create main content area with proper spacing and padding
  - [ ] 1.4: Add slot/children pattern for dynamic content rendering
  - [ ] 1.5: Style with TailwindCSS for modern admin panel aesthetic

- [ ] **Task 2: Implement navigation menu** (AC: #2)
  - [ ] 2.1: Create navigation item components with active state styling
  - [ ] 2.2: Add "Dashboard" menu item (link to `/admin/dashboard`)
  - [ ] 2.3: Add "Models" menu item (link to `/admin/models`)
  - [ ] 2.4: Add "Benchmarks" menu item (link to `/admin/benchmarks`)
  - [ ] 2.5: Add icons for each menu item (using Lucide React)
  - [ ] 2.6: Highlight active menu item based on current route
  - [ ] 2.7: Add tooltip for collapsed sidebar (mobile)

- [ ] **Task 3: Create admin header component** (AC: #4)
  - [ ] 3.1: Create `AdminHeader.tsx` component
  - [ ] 3.2: Display "Admin Panel" title or current section name
  - [ ] 3.3: Add user dropdown showing logged-in admin username
  - [ ] 3.4: Add logout button in user dropdown
  - [ ] 3.5: Connect logout button to auth service from Story 2.1
  - [ ] 3.6: Add breadcrumb navigation for sub-pages (optional)

- [ ] **Task 4: Implement responsive layout** (AC: #5)
  - [ ] 4.1: Make sidebar collapsible with hamburger menu on mobile/tablet
  - [ ] 4.2: Implement mobile drawer pattern (slide-in from left)
  - [ ] 4.3: Test layout on desktop (>1024px), tablet (768-1024px)
  - [ ] 4.4: Ensure content area adjusts when sidebar collapses
  - [ ] 4.5: Add smooth transitions for sidebar collapse/expand

- [ ] **Task 5: Set up admin routes**
  - [ ] 5.1: Configure admin routes in React Router
  - [ ] 5.2: Create placeholder dashboard page (`AdminDashboardPage.tsx`)
  - [ ] 5.3: Create placeholder models page (`AdminModelsPage.tsx`)
  - [ ] 5.4: Create placeholder benchmarks page (`AdminBenchmarksPage.tsx`)
  - [ ] 5.5: Wrap admin routes with `AdminLayout` component
  - [ ] 5.6: Test navigation between sections

- [ ] **Task 6: Add layout testing**
  - [ ] 6.1: Write component tests for `AdminLayout` (Vitest)
  - [ ] 6.2: Test sidebar navigation highlights active item
  - [ ] 6.3: Test responsive behavior (collapsed sidebar on mobile)
  - [ ] 6.4: Test header displays correct admin username
  - [ ] 6.5: Test logout button triggers auth service

## Dev Notes

### Architecture Context

**Layout Pattern:**
- Master-detail layout: Sidebar (navigation) + Main content area
- Responsive: Desktop sidebar → Mobile drawer
- Nested routing: `/admin` layout wraps all admin sub-routes
- Header persistent across all admin pages

**Component Hierarchy:**
```
AdminLayout
├── AdminSidebar
│   └── NavItem[] (Dashboard, Models, Benchmarks)
├── AdminHeader
│   ├── Breadcrumb (optional)
│   └── UserDropdown (username, logout)
└── MainContent (children slot)
```

**Styling Approach:**
- TailwindCSS utility classes for layout
- Consistent spacing: padding-4, gap-6
- Color scheme: Neutral grays for sidebar, white for content area
- Icons: Lucide React (LayoutDashboard, Database, BarChart icons)

### Project Structure Notes

**Files to Create:**
```
/frontend/src/components/layout/
  ├── AdminLayout.tsx
  ├── AdminSidebar.tsx
  ├── AdminHeader.tsx
  └── NavItem.tsx

/frontend/src/pages/admin/
  ├── AdminDashboardPage.tsx (placeholder)
  ├── AdminModelsPage.tsx (placeholder)
  └── AdminBenchmarksPage.tsx (placeholder)

/frontend/src/App.tsx
  └── (configure admin routes)
```

###

 Implementation Details

**Admin Route Structure:**
```typescript
// In App.tsx
<Routes>
  <Route path="/admin" element={
    <ProtectedRoute>
      <AdminLayout />
    </ProtectedRoute>
  }>
    <Route index element={<Navigate to="dashboard" replace />} />
    <Route path="dashboard" element={<AdminDashboardPage />} />
    <Route path="models" element={<AdminModelsPage />} />
    <Route path="benchmarks" element={<AdminBenchmarksPage />} />
  </Route>
</Routes>
```

**Sidebar Navigation Items:**
```typescript
const navItems = [
  {
    name: 'Dashboard',
    icon: LayoutDashboard,
    path: '/admin/dashboard'
  },
  {
    name: 'Models',
    icon: Database,
    path: '/admin/models',
    badge: '23' // optional: show count
  },
  {
    name: 'Benchmarks',
    icon: BarChart,
    path: '/admin/benchmarks'
  }
]
```

**Responsive Breakpoints:**
- Desktop: `lg:` (1024px+) - Sidebar always visible
- Tablet: `md:` (768-1024px) - Collapsible sidebar
- Mobile: `<md` (<768px) - Drawer sidebar

### References

- [Tech Spec Epic 2: docs/tech-spec-epic-2-8-summary.md#Epic 2 - Admin CRUD]
- [Solution Architecture: docs/solution-architecture.md#2.3 Client-Side Architecture]
- [Epics Document: docs/epics.md#Story 2.2]
- [Frontend Structure: docs/solution-architecture.md#8 Proposed Source Tree]

### Testing Strategy

**Component Tests:**
- AdminLayout renders sidebar and content area correctly
- Navigation items highlight active route
- User dropdown displays admin username from auth state
- Logout button calls auth service logout method
- Sidebar collapses on mobile breakpoint

**Integration Tests:**
- Navigating between admin sections updates active menu item
- Protected route wrapper redirects non-authenticated users
- Content area updates when navigating between routes

**Visual Tests (manual for MVP, automated post-MVP):**
- Layout looks correct on desktop, tablet, mobile
- Sidebar transitions are smooth
- Colors and spacing consistent with design system

## Dev Agent Record

### Context Reference

<!-- Path(s) to story context XML will be added here by context workflow -->

### Agent Model Used

claude-sonnet-4-5-20250929

### Debug Log References

### Completion Notes List

### File List
