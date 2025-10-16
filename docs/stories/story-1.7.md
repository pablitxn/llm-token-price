# Story 1.7: Setup Frontend Application Shell

Status: Ready

## Story

As a developer,
I want React application initialized with routing, state management, and layout structure,
So that I can begin building UI components and pages.

## Acceptance Criteria

1. React application entry point (main.tsx) configured with React Query and React Router providers
2. App component created with route configuration for HomePage, CalculatorPage, and ComparisonPage placeholder components
3. Layout component created with header, footer, and main content area structure
4. Global styles configured with TailwindCSS imports and custom CSS variables for design system
5. Frontend dev server starts successfully and displays placeholder homepage at http://localhost:5173
6. API client utility created for making HTTP requests to backend with base URL configuration

## Tasks / Subtasks

- [ ] Configure React application entry point with providers (AC: 1)
  - [ ] Create `frontend/src/main.tsx` if not exists (should be from Story 1.1)
  - [ ] Import React, ReactDOM, QueryClient, QueryClientProvider, BrowserRouter
  - [ ] Create QueryClient instance with default options: `staleTime: 5 * 60 * 1000` (5 minutes client cache)
  - [ ] Configure QueryClient with retry logic: `retry: 1`, error handling defaults
  - [ ] Wrap App in providers: React.StrictMode → QueryClientProvider → BrowserRouter → App
  - [ ] Import global styles: `import './styles/globals.css'`
  - [ ] Mount to root element: `ReactDOM.createRoot(document.getElementById('root')!).render(...)`

- [ ] Create App component with route configuration (AC: 2)
  - [ ] Create `frontend/src/App.tsx` with Routes and Route imports from react-router-dom
  - [ ] Import Layout component and placeholder page components
  - [ ] Configure routes: `/` → HomePage, `/calculator` → CalculatorPage, `/compare` → ComparisonPage
  - [ ] Wrap routes in Layout component for consistent header/footer
  - [ ] Add 404 not found route: `<Route path="*" element={<NotFoundPage />}` (create simple placeholder)
  - [ ] Test routing: navigate to each route in browser and verify correct page renders

- [ ] Create Layout component with structure (AC: 3)
  - [ ] Create `frontend/src/components/layout/Layout.tsx` component accepting children prop
  - [ ] Create Header component: `frontend/src/components/layout/Header.tsx` with logo and navigation links
  - [ ] Create Footer component: `frontend/src/components/layout/Footer.tsx` with copyright and links
  - [ ] Layout structure: `<div className="min-h-screen flex flex-col"><Header /><main className="flex-1">{children}</main><Footer /></div>`
  - [ ] Header navigation: links to "/" (Home), "/calculator" (Calculator), "/compare" (Compare) using react-router-dom Link component
  - [ ] Style with TailwindCSS: header with border-bottom, footer with border-top, sticky header optional

- [ ] Create placeholder page components (AC: 2, 5)
  - [ ] Create `frontend/src/pages/HomePage.tsx` with simple content: "LLM Pricing Comparison - Coming Soon"
  - [ ] Create `frontend/src/pages/CalculatorPage.tsx` placeholder
  - [ ] Create `frontend/src/pages/ComparisonPage.tsx` placeholder
  - [ ] Create `frontend/src/pages/NotFoundPage.tsx` with 404 message and link to home
  - [ ] Add basic Tailwind styling: centered content, padding, headings
  - [ ] Verify each page renders correctly when navigating to its route

- [ ] Configure global styles with TailwindCSS (AC: 4)
  - [ ] Create `frontend/src/styles/globals.css` if not exists
  - [ ] Add Tailwind directives: `@tailwind base; @tailwind components; @tailwind utilities;`
  - [ ] Define CSS custom properties for design system colors (from UX spec): `--color-primary`, `--color-secondary`, `--color-neutral-50` through `--color-neutral-900`
  - [ ] Add base styles: body font family (Inter), reset margins, box-sizing
  - [ ] Add utility classes: `.container` for max-width layouts, `.btn` base button styles
  - [ ] Test styles apply: verify fonts load, colors available, Tailwind utilities work

- [ ] Create API client utility (AC: 6)
  - [ ] Create `frontend/src/api/client.ts` with axios instance configuration
  - [ ] Set base URL: `baseURL: 'http://localhost:5000/api'` (using Vite proxy from Story 1.2)
  - [ ] Configure timeout: `timeout: 10000` (10 seconds)
  - [ ] Add request interceptor for auth headers (placeholder for future JWT tokens)
  - [ ] Add response interceptor for error handling: log errors, transform error responses
  - [ ] Export configured axios instance as `apiClient`
  - [ ] Create `frontend/src/api/health.ts` with test function: `export const checkHealth = () => apiClient.get('/health')`

- [ ] Test frontend application end-to-end (AC: 5)
  - [ ] Start frontend dev server: `npm run dev` in /frontend directory
  - [ ] Verify app loads at http://localhost:5173 without errors
  - [ ] Test navigation: click header links, verify routes change, back button works
  - [ ] Test API connection: open browser console, run `import { checkHealth } from './api/health'; checkHealth().then(console.log)`
  - [ ] Verify no console errors (check for React warnings, hydration errors, Tailwind issues)
  - [ ] Test hot reload: edit HomePage content, verify changes appear without refresh
  - [ ] Verify responsive: resize browser window, check mobile breakpoints

- [ ] Document frontend structure and verify all components (AC: 1-6)
  - [ ] Update README.md with "Frontend Architecture" section explaining component structure
  - [ ] Document routing structure: list all routes and their purposes
  - [ ] Document state management setup: React Query for server state, Zustand for client state (not yet implemented)
  - [ ] Create troubleshooting section: port 5173 already in use, API connection errors, Tailwind not working
  - [ ] Create frontend verification checklist: dev server starts, routes work, API client configured, styles apply
  - [ ] Verify all acceptance criteria: run through checklist and confirm all 6 criteria met

## Dev Notes

### Architecture Constraints

**From solution-architecture.md Section 2.3 - Client-Side Architecture:**
- **Component hierarchy:** App → Layout → Pages → Components
- **State architecture:**
  - Global state (Zustand) for selected models, filter state, view preferences
  - Server state (TanStack Query) for API data with 5-minute stale time
  - Local component state (useState) for form inputs, modals
- **Routing:** React Router 6.21.0 with standard SPA routing

**From solution-architecture.md Section 1.1 - Technology Stack:**
- **React:** 18.2.0 with concurrent rendering features
- **React Router:** 6.21.0 for client-side routing
- **TanStack Query:** 5.17.0 for server state management
- **Axios:** 1.6.0 for HTTP client
- **TailwindCSS:** 3.4.0 for styling (already configured in Story 1.2)

**From tech-spec-epic-1.md Story 1.7:**
- **main.tsx structure:** QueryClient with 5min staleTime, BrowserRouter wrapper, StrictMode enabled
- **App.tsx routes:** / (HomePage), /calculator (CalculatorPage), /compare (ComparisonPage)
- **Layout component:** Header + main content + Footer structure

### Project Structure Notes

**Frontend directory structure:**
```
/frontend/
├── src/
│   ├── main.tsx (app entry point)
│   ├── App.tsx (route configuration)
│   ├── /components/
│   │   └── /layout/
│   │       ├── Layout.tsx (wraps header + main + footer)
│   │       ├── Header.tsx (navigation bar)
│   │       └── Footer.tsx (footer content)
│   ├── /pages/
│   │   ├── HomePage.tsx (placeholder)
│   │   ├── CalculatorPage.tsx (placeholder)
│   │   ├── ComparisonPage.tsx (placeholder)
│   │   └── NotFoundPage.tsx (404 handler)
│   ├── /api/
│   │   ├── client.ts (axios instance configuration)
│   │   └── health.ts (health check function)
│   └── /styles/
│       └── globals.css (Tailwind imports + custom styles)
└── index.html (entry HTML)
```

**main.tsx implementation:**
```typescript
import React from 'react'
import ReactDOM from 'react-dom/client'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { BrowserRouter } from 'react-router-dom'
import App from './App'
import './styles/globals.css'

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 5 * 60 * 1000, // 5 minutes
      retry: 1,
      refetchOnWindowFocus: false,
    },
  },
})

ReactDOM.createRoot(document.getElementById('root')!).render(
  <React.StrictMode>
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>
        <App />
      </BrowserRouter>
    </QueryClientProvider>
  </React.StrictMode>,
)
```

**App.tsx implementation:**
```typescript
import { Routes, Route } from 'react-router-dom'
import Layout from './components/layout/Layout'
import HomePage from './pages/HomePage'
import CalculatorPage from './pages/CalculatorPage'
import ComparisonPage from './pages/ComparisonPage'
import NotFoundPage from './pages/NotFoundPage'

export default function App() {
  return (
    <Layout>
      <Routes>
        <Route path="/" element={<HomePage />} />
        <Route path="/calculator" element={<CalculatorPage />} />
        <Route path="/compare" element={<ComparisonPage />} />
        <Route path="*" element={<NotFoundPage />} />
      </Routes>
    </Layout>
  )
}
```

**Layout.tsx implementation:**
```typescript
import { ReactNode } from 'react'
import Header from './Header'
import Footer from './Footer'

interface LayoutProps {
  children: ReactNode
}

export default function Layout({ children }: LayoutProps) {
  return (
    <div className="min-h-screen flex flex-col bg-neutral-50">
      <Header />
      <main className="flex-1 container mx-auto px-4 py-8">
        {children}
      </main>
      <Footer />
    </div>
  )
}
```

**API client.ts implementation:**
```typescript
import axios from 'axios'

export const apiClient = axios.create({
  baseURL: '/api', // Vite proxy forwards to http://localhost:5000/api
  timeout: 10000,
  headers: {
    'Content-Type': 'application/json',
  },
})

// Request interceptor (placeholder for future auth)
apiClient.interceptors.request.use(
  (config) => {
    // Add auth token here when implemented
    return config
  },
  (error) => Promise.reject(error)
)

// Response interceptor (error handling)
apiClient.interceptors.response.use(
  (response) => response,
  (error) => {
    console.error('API Error:', error.response?.data || error.message)
    return Promise.reject(error)
  }
)
```

### Testing Standards Summary

**Frontend application validation:**
1. Dev server test: `npm run dev` starts without errors on port 5173
2. Homepage test: Navigate to http://localhost:5173, verify placeholder content renders
3. Routing test: Click navigation links, verify URL changes and correct page loads
4. API connectivity test: Browser console fetch to `/api/health` succeeds
5. Hot reload test: Edit component, verify changes appear immediately
6. No console errors: Check for React warnings, network errors, Tailwind issues

**Component rendering validation:**
1. Layout renders: Header visible at top, Footer at bottom, main content in between
2. Navigation works: Links change routes without full page reload
3. 404 handling: Navigate to invalid route, verify NotFoundPage renders
4. Tailwind styles apply: Verify fonts, colors, spacing classes work

**Quality gates:**
- Initial page load: <2 seconds
- Route navigation: <100ms
- Dev server HMR: <500ms
- No React strict mode warnings
- Zero TypeScript compilation errors

### Dependencies and Prerequisites

**Prerequisites from previous stories:**
- Story 1.1: Frontend directory created with Vite + React + TypeScript
- Story 1.2: All frontend packages installed (React Router, React Query, axios, TailwindCSS)
- Story 1.2: Vite configured with proxy to backend

**No new dependencies** (all packages already installed in Story 1.2)

**Common issues and solutions:**
1. **Port 5173 already in use**: Kill existing Vite process or change port in vite.config.ts
2. **API connection fails**: Verify backend running on port 5000, check Vite proxy configuration
3. **Tailwind classes not working**: Verify globals.css imported in main.tsx
4. **React Router 404 on refresh**: Dev server should handle this automatically (Vite config)
5. **TypeScript errors on components**: Verify tsconfig.json configured correctly from Story 1.2

### References

- [Source: docs/solution-architecture.md#Section 2.3 - Client-Side Architecture with component hierarchy]
- [Source: docs/solution-architecture.md#Section 2.3 - State Architecture (Zustand + React Query)]
- [Source: docs/solution-architecture.md#Section 1.1 - React 18.2.0, React Router 6.21.0, TanStack Query 5.17.0]
- [Source: docs/tech-spec-epic-1.md#Story 1.7 - Frontend application shell with routing]
- [Source: docs/epics.md#Epic 1, Story 1.7 - Acceptance Criteria]
- [Source: docs/ux-specification.md#Information Architecture - route structure and navigation]

## Dev Agent Record

### Context Reference

<!-- Path(s) to story context XML will be added here by context workflow -->

### Agent Model Used

<!-- Agent model information will be populated during development -->

### Debug Log References

<!-- Debug logs will be added during development -->

### Completion Notes List

<!-- Completion notes will be added after story implementation -->

### File List

<!-- Modified/created files will be listed here after implementation -->
