# Story 3.1: Public Homepage Layout

Status: Ready

## Story

As a user,
I want to access the platform homepage,
so that I can begin comparing LLM models.

## Acceptance Criteria

1. Public homepage route (`/`) created
2. Page layout includes header with platform name/logo
3. Main content area ready for comparison table
4. Footer with basic info (about, contact links placeholder)
5. Navigation bar includes search input placeholder
6. Responsive layout works on desktop, tablet, mobile

## Tasks / Subtasks

- [ ] **Task 1: Create frontend folder structure** (AC: #1)
  - [ ] 1.1: Create directories: `components/`, `pages/`, `api/`, `store/`, `hooks/`, `types/`, `utils/`
  - [ ] 1.2: Create sub-directories: `components/layout/`, `components/models/`, `components/ui/`
  - [ ] 1.3: Verify path aliases work in vite.config.ts and tsconfig.json
  - [ ] 1.4: Test imports with aliases (@/, @components/, @api/, @store/)

- [ ] **Task 2: Create PublicLayout component** (AC: #2, #3, #4)
  - [ ] 2.1: Create `PublicLayout.tsx` in `/apps/web/src/components/layout`
  - [ ] 2.2: Implement flexbox layout: header (sticky), main content (flex-grow), footer
  - [ ] 2.3: Add children prop to render page content in main area
  - [ ] 2.4: Style with TailwindCSS (min-h-screen, flex flex-col)
  - [ ] 2.5: Make header sticky (sticky top-0 z-10)

- [ ] **Task 3: Create Header component** (AC: #2, #5)
  - [ ] 3.1: Create `Header.tsx` in `/apps/web/src/components/layout`
  - [ ] 3.2: Add platform name/logo (text for MVP, image placeholder for logo)
  - [ ] 3.3: Add navigation placeholder (links to: Home, Calculator, About)
  - [ ] 3.4: Add search input placeholder (will be implemented in Story 3.8)
  - [ ] 3.5: Style with TailwindCSS (bg-white, border-b, shadow-sm, padding)
  - [ ] 3.6: Make responsive: logo + nav on desktop, hamburger menu on mobile (button only, functionality in Epic 8)

- [ ] **Task 4: Create Footer component** (AC: #4)
  - [ ] 4.1: Create `Footer.tsx` in `/apps/web/src/components/layout`
  - [ ] 4.2: Add copyright text with current year (dynamic with Date)
  - [ ] 4.3: Add placeholder links: About, Contact, Privacy (# hrefs for now)
  - [ ] 4.4: Style with TailwindCSS (bg-gray-50, border-t, text-center, padding)
  - [ ] 4.5: Make footer stick to bottom (mt-auto in PublicLayout)

- [ ] **Task 5: Create HomePage component** (AC: #1, #3)
  - [ ] 5.1: Create `HomePage.tsx` in `/apps/web/src/pages`
  - [ ] 5.2: Wrap in PublicLayout component
  - [ ] 5.3: Add page title: "LLM Model Comparison" (h1)
  - [ ] 5.4: Add subtitle/description paragraph
  - [ ] 5.5: Add placeholder for comparison table: "Table will appear here in Story 3.2"
  - [ ] 5.6: Style with container mx-auto, padding

- [ ] **Task 6: Setup React Router** (AC: #1)
  - [ ] 6.1: Configure routes in `App.tsx`
  - [ ] 6.2: Add route: `/` → HomePage
  - [ ] 6.3: Add placeholder route: `/calculator` → "Coming soon" page
  - [ ] 6.4: Add placeholder route: `/compare` → "Coming soon" page
  - [ ] 6.5: Test navigation: verify homepage loads at localhost:5173

- [ ] **Task 7: Configure TailwindCSS design tokens** (AC: #6)
  - [ ] 7.1: Extend Tailwind config with project colors (primary, secondary, accent)
  - [ ] 7.2: Add custom spacing if needed (consistent padding/margins)
  - [ ] 7.3: Configure container settings (max-width, padding)
  - [ ] 7.4: Add custom font family if specified in design
  - [ ] 7.5: Test design tokens work (apply custom colors to Header)

- [ ] **Task 8: Apply global styles** (AC: #6)
  - [ ] 8.1: Update `index.css` with base styles
  - [ ] 8.2: Set default font, font-size, line-height
  - [ ] 8.3: Add smooth scrolling behavior (html { scroll-behavior: smooth; })
  - [ ] 8.4: Reset default margins/padding if needed
  - [ ] 8.5: Test global styles apply correctly

- [ ] **Task 9: Test responsive layout** (AC: #6)
  - [ ] 9.1: Test on desktop (1920px, 1280px)
  - [ ] 9.2: Test on tablet (768px, 1024px)
  - [ ] 9.3: Test on mobile (375px, 414px)
  - [ ] 9.4: Verify header, content, footer layout works on all sizes
  - [ ] 9.5: Fix any overflow or layout breaks
  - [ ] 9.6: Take screenshots for documentation

- [ ] **Task 10: Testing**
  - [ ] 10.1: Write unit test for PublicLayout component (Vitest)
  - [ ] 10.2: Test Header renders logo and navigation
  - [ ] 10.3: Test Footer renders links
  - [ ] 10.4: Test HomePage renders within PublicLayout
  - [ ] 10.5: E2E test: navigate to / and verify page loads

## Dev Notes

### Architecture Context

**Layout Pattern:**
- PublicLayout: Shared layout for all public pages (non-admin)
- Sticky header: Always visible for navigation
- Footer: Informational, sticks to bottom if content short

**Responsive Strategy:**
- Mobile-first approach (base styles for mobile, media queries for larger)
- Breakpoints: sm (640px), md (768px), lg (1024px), xl (1280px), 2xl (1536px)
- Header adapts: Full nav on desktop, hamburger icon on mobile (functionality in Epic 8)

**Design Tokens:**
- Colors: Define primary (blue), secondary (gray), accent (green for success states)
- Spacing: Consistent padding (container: px-4 md:px-6 lg:px-8)
- Typography: Clear hierarchy (h1: 3xl, h2: 2xl, body: base)

### Project Structure Notes

**Frontend Files to Create:**
```
/apps/web/src/
├── components/
│   └── layout/
│       ├── PublicLayout.tsx             # Main layout wrapper
│       ├── Header.tsx                   # Navigation header
│       └── Footer.tsx                   # Page footer
├── pages/
│   └── HomePage.tsx                     # Main public page
├── App.tsx                              # (update) Add routes
├── index.css                            # (update) Global styles
└── vite.config.ts                       # (verify) Path aliases
```

**Tailwind Config Extension:**
```javascript
// tailwind.config.js
export default {
  theme: {
    extend: {
      colors: {
        primary: {
          50: '#eff6ff',
          // ... blue shades
          600: '#2563eb',
          // ...
        },
      },
      container: {
        center: true,
        padding: {
          DEFAULT: '1rem',
          md: '1.5rem',
          lg: '2rem',
        },
      },
    },
  },
};
```

### Implementation Details

**PublicLayout Component:**
```typescript
// components/layout/PublicLayout.tsx
import { Header } from './Header';
import { Footer } from './Footer';

interface PublicLayoutProps {
  children: React.ReactNode;
}

export const PublicLayout = ({ children }: PublicLayoutProps) => (
  <div className="min-h-screen flex flex-col">
    <Header />
    <main className="flex-grow container mx-auto px-4 md:px-6 lg:px-8 py-8">
      {children}
    </main>
    <Footer />
  </div>
);
```

**Header Component:**
```typescript
// components/layout/Header.tsx
export const Header = () => (
  <header className="sticky top-0 z-10 bg-white border-b shadow-sm">
    <div className="container mx-auto px-4 md:px-6 lg:px-8">
      <div className="flex items-center justify-between h-16">
        {/* Logo */}
        <div className="flex items-center">
          <h1 className="text-2xl font-bold text-primary-600">
            LLM Price Comparison
          </h1>
        </div>

        {/* Navigation - Desktop */}
        <nav className="hidden md:flex items-center space-x-6">
          <a href="/" className="text-gray-700 hover:text-primary-600">Home</a>
          <a href="/calculator" className="text-gray-700 hover:text-primary-600">Calculator</a>
          <a href="#" className="text-gray-700 hover:text-primary-600">About</a>
        </nav>

        {/* Search Placeholder */}
        <div className="hidden lg:block">
          <input
            type="text"
            placeholder="Search models..."
            className="px-4 py-2 border rounded-lg w-64"
            disabled
          />
        </div>

        {/* Mobile menu button */}
        <button className="md:hidden">
          <svg className="w-6 h-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 6h16M4 12h16M4 18h16" />
          </svg>
        </button>
      </div>
    </div>
  </header>
);
```

**Footer Component:**
```typescript
// components/layout/Footer.tsx
export const Footer = () => {
  const currentYear = new Date().getFullYear();

  return (
    <footer className="mt-auto bg-gray-50 border-t">
      <div className="container mx-auto px-4 py-6">
        <div className="flex flex-col md:flex-row items-center justify-between">
          <p className="text-gray-600 text-sm">
            © {currentYear} LLM Price Comparison. All rights reserved.
          </p>
          <nav className="flex space-x-6 mt-4 md:mt-0">
            <a href="#" className="text-gray-600 hover:text-gray-900 text-sm">About</a>
            <a href="#" className="text-gray-600 hover:text-gray-900 text-sm">Contact</a>
            <a href="#" className="text-gray-600 hover:text-gray-900 text-sm">Privacy</a>
          </nav>
        </div>
      </div>
    </footer>
  );
};
```

**HomePage Component:**
```typescript
// pages/HomePage.tsx
import { PublicLayout } from '@components/layout/PublicLayout';

export const HomePage = () => (
  <PublicLayout>
    <div className="space-y-6">
      <div className="text-center md:text-left">
        <h1 className="text-4xl font-bold text-gray-900 mb-4">
          LLM Model Comparison
        </h1>
        <p className="text-lg text-gray-600 max-w-3xl">
          Compare pricing, capabilities, and performance across 50+ large language models.
          Make data-driven decisions for your AI projects.
        </p>
      </div>

      {/* Placeholder for table */}
      <div className="border-2 border-dashed border-gray-300 rounded-lg p-12 text-center text-gray-500">
        Comparison table will appear here in Story 3.2
      </div>
    </div>
  </PublicLayout>
);
```

**App.tsx Routes:**
```typescript
// App.tsx
import { BrowserRouter, Routes, Route } from 'react-router-dom';
import { HomePage } from './pages/HomePage';

function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<HomePage />} />
        <Route path="/calculator" element={<div>Calculator coming soon</div>} />
        <Route path="/compare" element={<div>Comparison coming soon</div>} />
      </Routes>
    </BrowserRouter>
  );
}

export default App;
```

### References

- [Epic 3 Analysis: docs/epic-3-analysis-and-plan.md#Story 3.1]
- [Epics Document: docs/epics.md#Story 3.1]
- [TailwindCSS Docs](https://tailwindcss.com/docs)
- [React Router Docs](https://reactrouter.com/)

### Testing Strategy

**Unit Tests:**
- PublicLayout renders Header, children, Footer in correct order
- Header displays logo and navigation links
- Footer displays current year and links
- HomePage renders within PublicLayout

**Integration Tests:**
- Navigate to / renders HomePage
- All links render (href attributes present)
- Layout responsive at different breakpoints

**Visual Regression Tests (optional):**
- Screenshot homepage at desktop, tablet, mobile
- Verify layout consistency across browsers

## Dev Agent Record

### Context Reference

<!-- Path(s) to story context XML will be added here by context workflow -->

### Agent Model Used

claude-sonnet-4-5-20250929

### Debug Log References

### Completion Notes List

### File List
