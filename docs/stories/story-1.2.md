# Story 1.2: Configure Build Tools and Package Management

Status: Ready

## Story

As a developer,
I want build tools and package managers configured for both backend and frontend,
So that I can install dependencies, build the application, and maintain consistent development workflows.

## Acceptance Criteria

1. Backend .NET 8 solution file created with proper project references (API → Application → Domain, Infrastructure → Domain, Application → Domain)
2. Frontend Vite configured with TypeScript, React, and TailwindCSS with proper build optimization settings
3. Package.json includes all required frontend dependencies: Zustand 4.4.7, React Query 5.17.0, TanStack Table 8.11.0, Chart.js 4.4.1
4. Backend NuGet packages configured: Entity Framework Core 8.0.0, Npgsql.EntityFrameworkCore.PostgreSQL 8.0.0, StackExchange.Redis 2.7.0
5. Both projects build successfully with `dotnet build` (backend) and `npm run build` (frontend) producing no errors
6. TypeScript type checking passes with `npm run type-check` in frontend project

## Tasks / Subtasks

- [ ] Configure .NET project references and validate dependencies (AC: 1)
  - [ ] Add project reference from Backend.API to Backend.Application: `dotnet add Backend.API/Backend.API.csproj reference Backend.Application/Backend.Application.csproj`
  - [ ] Add project reference from Backend.API to Backend.Infrastructure: `dotnet add Backend.API/Backend.API.csproj reference Backend.Infrastructure/Backend.Infrastructure.csproj`
  - [ ] Add project reference from Backend.Application to Backend.Domain: `dotnet add Backend.Application/Backend.Application.csproj reference Backend.Domain/Backend.Domain.csproj`
  - [ ] Add project reference from Backend.Infrastructure to Backend.Domain: `dotnet add Backend.Infrastructure/Backend.Infrastructure.csproj reference Backend.Domain/Backend.Domain.csproj`
  - [ ] Verify project references in Backend.sln by running `dotnet build` (should succeed with no dependency warnings)
  - [ ] Validate Hexagonal Architecture boundaries: ensure Domain has zero project references

- [ ] Install and configure backend NuGet packages (AC: 4)
  - [ ] Install EF Core in Infrastructure: `dotnet add Backend.Infrastructure package Microsoft.EntityFrameworkCore --version 8.0.0`
  - [ ] Install EF Core Design tools: `dotnet add Backend.Infrastructure package Microsoft.EntityFrameworkCore.Design --version 8.0.0`
  - [ ] Install PostgreSQL provider: `dotnet add Backend.Infrastructure package Npgsql.EntityFrameworkCore.PostgreSQL --version 8.0.0`
  - [ ] Install Redis client: `dotnet add Backend.Infrastructure package StackExchange.Redis --version 2.7.10`
  - [ ] Install Serilog logging: `dotnet add Backend.API package Serilog.AspNetCore --version 8.0.0`
  - [ ] Install Swagger/OpenAPI: `dotnet add Backend.API package Swashbuckle.AspNetCore --version 6.5.0`
  - [ ] Verify all packages restored: `dotnet restore` in backend directory

- [ ] Configure Vite build system for frontend (AC: 2)
  - [ ] Update `vite.config.ts` with proxy configuration to backend API (`http://localhost:5000`)
  - [ ] Configure Vite build optimization: set `build.target: 'es2020'`, `build.minify: 'esbuild'`
  - [ ] Add path aliases for cleaner imports: `resolve.alias: { '@': '/src', '@components': '/src/components', '@api': '/src/api', '@store': '/src/store' }`
  - [ ] Configure dev server port: `server.port: 5173`, enable CORS: `server.cors: true`
  - [ ] Add `define` for environment variables: `define: { '__APP_VERSION__': JSON.stringify(process.env.npm_package_version) }`
  - [ ] Test dev server starts: `npm run dev` (should start on http://localhost:5173)

- [ ] Install frontend core dependencies (AC: 3)
  - [ ] Install state management: `npm install zustand@4.4.7`
  - [ ] Install data fetching: `npm install @tanstack/react-query@5.17.0`
  - [ ] Install table library: `npm install @tanstack/react-table@8.11.0`
  - [ ] Install chart library: `npm install chart.js@4.4.1 react-chartjs-2@5.2.0`
  - [ ] Install routing: `npm install react-router-dom@6.21.0`
  - [ ] Install HTTP client: `npm install axios@1.6.5`
  - [ ] Install form handling: `npm install react-hook-form@7.49.0`
  - [ ] Install validation: `npm install zod@3.22.4`
  - [ ] Install icons: `npm install lucide-react@0.300.0`
  - [ ] Install date utilities: `npm install date-fns@3.0.6`
  - [ ] Verify installations: `npm list --depth=0` (check all versions match)

- [ ] Configure TailwindCSS with design system (AC: 2)
  - [ ] Update `tailwind.config.js` content paths: `content: ['./index.html', './src/**/*.{js,ts,jsx,tsx}']`
  - [ ] Configure theme extension with design system colors from UX spec (primary, secondary, neutral, success, warning, error palettes)
  - [ ] Add custom spacing scale: `spacing: { '128': '32rem', '144': '36rem' }`
  - [ ] Configure font families: `fontFamily: { sans: ['Inter', 'system-ui', 'sans-serif'], mono: ['JetBrains Mono', 'monospace'] }`
  - [ ] Enable JIT mode and add safelist for dynamic classes
  - [ ] Create `src/styles/globals.css` and import Tailwind directives: `@tailwind base; @tailwind components; @tailwind utilities;`
  - [ ] Test TailwindCSS compiles: verify styles applied in dev mode

- [ ] Configure TypeScript for type safety (AC: 6)
  - [ ] Update `tsconfig.json` with strict type checking: `strict: true`, `noImplicitAny: true`, `strictNullChecks: true`
  - [ ] Configure path aliases matching Vite config: `paths: { "@/*": ["./src/*"], "@components/*": ["./src/components/*"] }`
  - [ ] Set module resolution: `moduleResolution: "bundler"`, `target: "ES2020"`
  - [ ] Enable JSX support: `jsx: "react-jsx"`, `jsxImportSource: "react"`
  - [ ] Configure type checking script in package.json: `"type-check": "tsc --noEmit"`
  - [ ] Run type check to verify: `npm run type-check` (should pass with no errors)

- [ ] Verify builds and document build commands (AC: 5)
  - [ ] Run backend build: `dotnet build` in /backend directory (verify 0 errors, 0 warnings)
  - [ ] Run frontend build: `npm run build` in /frontend directory (verify dist/ folder created)
  - [ ] Test backend runs: `dotnet run --project Backend.API` (should start on port 5000)
  - [ ] Test frontend dev server: `npm run dev` in /frontend (should start on port 5173)
  - [ ] Document all build commands in README.md under "Development Workflow" section
  - [ ] Create npm script for concurrent development: `"dev:all": "concurrently \"npm:dev\" \"cd ../backend && dotnet watch run\""`

## Dev Notes

### Architecture Constraints

**From solution-architecture.md Section 1.1:**
- **Exact package versions required** - no flexibility for MVP to ensure consistency:
  - Backend: ASP.NET Core 8.0, EF Core 8.0.0, Npgsql 8.0.0, StackExchange.Redis 2.7.10, Serilog 8.0.0
  - Frontend: React 18.2.0, TypeScript 5.3.0, Vite 5.0.0, Zustand 4.4.7, TanStack Query 5.17.0, TanStack Table 8.11.0, Chart.js 4.4.1, TailwindCSS 3.4.0

**From solution-architecture.md Section 2.1 - Hexagonal Architecture:**
- **Project reference constraints are CRITICAL**:
  - Backend.Domain must have ZERO references (pure domain logic)
  - Backend.Application can ONLY reference Backend.Domain
  - Backend.Infrastructure can ONLY reference Backend.Domain
  - Backend.API can reference Application and Infrastructure (composition root)
- **Violation check**: Run `dotnet list reference` on each project to verify boundaries

**From tech-spec-epic-1.md Story 1.2:**
- Vite proxy must forward `/api/*` requests to `http://localhost:5000` for backend API calls
- TailwindCSS must be configured with PostCSS and Autoprefixer for vendor prefix support
- TypeScript strict mode is mandatory (catches type errors before runtime)

### Project Structure Notes

**Backend package locations:**
```
/backend/
├── Backend.Domain/
│   └── Backend.Domain.csproj (NO package references except testing)
├── Backend.Application/
│   └── Backend.Application.csproj (references: Domain)
├── Backend.Infrastructure/
│   └── Backend.Infrastructure.csproj (packages: EF Core, Npgsql, Redis; references: Domain)
└── Backend.API/
    └── Backend.API.csproj (packages: Serilog, Swagger; references: Application, Infrastructure)
```

**Frontend configuration files:**
```
/frontend/
├── package.json (all dependencies listed here)
├── vite.config.ts (build config, proxy, aliases)
├── tsconfig.json (TypeScript compiler options, path aliases)
├── tailwind.config.js (design system colors, fonts, spacing)
├── postcss.config.js (Tailwind + Autoprefixer)
└── /src/
    ├── main.tsx (app entry point)
    ├── App.tsx (root component)
    └── /styles/
        └── globals.css (Tailwind imports + custom global styles)
```

**Vite proxy configuration pattern:**
```typescript
// vite.config.ts
export default defineConfig({
  server: {
    proxy: {
      '/api': {
        target: 'http://localhost:5000',
        changeOrigin: true,
        secure: false
      }
    }
  }
})
```

### Testing Standards Summary

**Build verification tests (this story):**
- Backend: `dotnet build` exits with code 0 (no compilation errors)
- Frontend: `npm run build` creates dist/ folder with index.html and bundled assets
- TypeScript: `npm run type-check` exits with code 0 (no type errors)
- Dependencies: All packages restore/install without version conflicts

**Quality gates:**
- Backend build time: < 30 seconds
- Frontend build time: < 15 seconds
- Frontend bundle size: < 500KB (initial load, gzipped)
- Zero TypeScript `any` types in strict mode

### References

- [Source: docs/solution-architecture.md#Section 1.1 - Technology and Library Decision Table]
- [Source: docs/solution-architecture.md#Section 2.1 - Hexagonal Architecture boundaries]
- [Source: docs/solution-architecture.md#Section 2.3 - Client-Side Architecture with Zustand + React Query]
- [Source: docs/tech-spec-epic-1.md#Story 1.2 - Backend packages & Frontend configuration]
- [Source: docs/epics.md#Epic 1, Story 1.2 - Acceptance Criteria]
- [Source: docs/ux-specification.md#Visual Design System - TailwindCSS theme configuration]
- [Source: docs/PRD.md#NFR005 - Maintainability with clear architectural boundaries]

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
