# Epic Alignment Matrix

**Project:** llm-token-price
**Date:** 2025-10-16
**Purpose:** Traceability mapping between epics, stories, architecture components, and implementation readiness

---

## Overview

This matrix provides comprehensive traceability from PRD epics to architectural components, ensuring that every story has clear implementation guidance from the solution architecture.

**Status:** ✅ **100% Epic-Architecture Alignment** (8/8 epics fully mapped)

---

## Epic-to-Architecture Mapping

| Epic | Stories | Frontend Components | Backend Services | Data Models | API Endpoints | Integration Points | Readiness |
|------|---------|---------------------|------------------|-------------|---------------|-------------------|-----------|
| **Epic 1: Foundation** | 10 | App shell, Layout, Routing | Health check, DI setup, DbContext | All tables (migrations) | GET /api/health | PostgreSQL, Redis, Docker Compose | ✅ COMPLETE |
| **Epic 2: Admin CRUD** | 12 | AdminLayout, ModelList, ModelForm, BenchmarkForm | AdminModelService, AdminBenchmarkService | models, capabilities, benchmarks, scores, audit_log | POST/PUT/DELETE /api/admin/models, /api/admin/benchmarks | EF Core repositories, JWT auth | ✅ READY |
| **Epic 3: Public Table** | 15 | HomePage, FilterSidebar, ModelTable, SearchBar, ComparisonBasket | ModelQueryService, FilterService | models (read), capabilities, scores | GET /api/models (cached) | TanStack Table, Redis cache | ✅ READY |
| **Epic 4: Model Detail** | 12 | ModelDetailModal, OverviewTab, BenchmarksTab, PricingTab, CostCalculator | ModelDetailService, CostCalculationService | models (detail), scores (all) | GET /api/models/{id} | React Router modals, Chart.js | ✅ READY |
| **Epic 5: Comparison** | 14 | ComparisonPage, ModelCard, ComparisonTable, BenchmarkChart, CapabilityMatrix | ModelComparisonService, ChartDataService | models (batch fetch), scores | GET /api/models/compare, GET /api/models/chart-data | Chart.js grouped bars | ✅ READY |
| **Epic 6: Smart Filter** | 10 | BestValueButton, QAPSScoreDisplay, ExplanationPanel | QAPSCalculationService, BestValueRankingService | benchmarks (weights), computed_metrics | GET /api/smart-filters/best-value | QAPS algorithm, Redis cache (1hr TTL) | ✅ READY |
| **Epic 7: Data Quality** | 10 | DataFreshnessIndicator, ValidationMessages, AdminDashboard | DataQualityService, ValidationService, AuditLogService | audit_log, models (timestamps) | GET /api/admin/dashboard/metrics | FluentValidation, anomaly detection | ✅ READY |
| **Epic 8: Responsive** | 10 | Mobile adaptations: FilterDrawer, CardLayout, MobileNav | No backend changes | No schema changes | No API changes | TailwindCSS breakpoints, touch optimization | ✅ READY |

**Total Stories:** 83 stories across 8 epics

---

## Epic 1: Project Foundation & Data Infrastructure

**Goal:** Establish development environment, database schema, API skeleton, and CI/CD pipeline

**Stories:** 10 | **Status:** ✅ COMPLETE (100% - all stories implemented and validated)

### Component Mapping

| Story ID | Story Title | Frontend Components | Backend Components | Data/Infrastructure | Architecture Reference |
|----------|-------------|---------------------|-------------------|---------------------|----------------------|
| 1.1 | Initialize Project Repository | N/A (scaffolding) | N/A (scaffolding) | Monorepo structure, .gitignore, docker-compose.yml | Section 2.2 (lines 159-180), Section 8 (lines 862-1088) |
| 1.2 | Configure Build Tools | Vite, TailwindCSS, package.json | .NET solution, NuGet packages | N/A | Section 1.1 (lines 20-52) |
| 1.3 | Setup PostgreSQL Database | N/A | AppDbContext, DbInitializer | PostgreSQL 16 + TimescaleDB, EF migrations | Section 3.1 (lines 289-407) |
| 1.4 | Create Core Data Models | N/A | Domain entities: Model, Capability, Benchmark, BenchmarkScore | 4 tables with relationships | Section 3.1 (lines 289-407), Section 8 (lines 869-891) |
| 1.5 | Setup Redis Cache | N/A | ICacheRepository, RedisCacheService | Redis 7.2 (Upstash) | Section 4 (lines 457-540) |
| 1.6 | Create Basic API Structure | N/A | HealthController, Program.cs DI | Health endpoint with DB/Redis checks | Section 2.4 (lines 239-283) |
| 1.7 | Setup Frontend Application Shell | App.tsx, Layout, Router providers | N/A | React 19, Zustand, TanStack Query | Section 2.3 (lines 182-210) |
| 1.8 | Configure CI/CD Pipeline | N/A | GitHub Actions workflows | Backend + Frontend test automation | Section 11.1 (lines 1370-1434) |
| 1.9 | Seed Database with Sample Data | N/A | SampleDataSeeder | 10 models, 5 benchmarks, 34 scores | Section 3.2 (lines 452-456) |
| 1.10 | Create Basic GET API | N/A | IModelRepository, ModelQueryService, ModelsController | GET /api/models with DTOs | Section 2.4 (lines 241-243), Section 7.1 (lines 689-714) |

**Integration Points:**
- Docker Compose: PostgreSQL + Redis containers
- EF Core: Database migrations and repository pattern
- Vite: Frontend build tool with HMR
- GitHub Actions: Automated CI/CD with test coverage

---

## Epic 2: Model Data Management & Admin CRUD

**Goal:** Admin panel for creating, updating, and managing model data, pricing, and benchmarks

**Stories:** 12 | **Status:** ✅ READY (architecture fully defined, stories pending drafting)

### Component Mapping

| Component Type | Components | Architecture Reference |
|----------------|-----------|----------------------|
| **Frontend Components** | AdminLayout, AdminModelsPage, ModelList, ModelForm (create/edit), BenchmarkForm, CSVImport | Section 2.3 (lines 203-209) |
| **Backend Services** | AdminModelService (CRUD), AdminBenchmarkService, DataQualityService, AuditLogService | Section 2.1 (lines 103-108) |
| **Data Models** | models table, model_capabilities, benchmarks, model_benchmark_scores, admin_audit_log | Section 3.1 (lines 289-415) |
| **API Endpoints** | POST /api/admin/models, PUT /api/admin/models/{id}, DELETE /api/admin/models/{id}, POST /api/admin/benchmarks, POST /api/admin/benchmarks/import-csv | Section 2.4 (lines 250-259), Section 7.2 (lines 793-860) |
| **Validation** | FluentValidation (backend), Zod (frontend), CreateModelValidator, BenchmarkScoreValidator | Section 12.2 (lines 1504-1522) |
| **Authentication** | JWT tokens, HttpOnly cookies, AdminAuthController | ADR-008 (lines 1220-1237) |

**Integration Points:**
- EF Core repositories for CRUD operations
- Cache invalidation on model updates (Redis pub/sub)
- Audit logging for admin actions
- CSV import with bulk validation

**Key Architecture Decisions:**
- ADR-008: JWT for admin auth (no OAuth for MVP)
- Soft deletes via `is_active` flag
- Audit trail in `admin_audit_log` table (JSONB changes)

---

## Epic 3: Public Comparison Table Interface

**Goal:** Primary user interface with sortable/filterable table, search, and comparison basket

**Stories:** 15 | **Status:** ✅ READY (all stories drafted and approved)

### Component Mapping

| Component Type | Components | Architecture Reference |
|----------------|-----------|----------------------|
| **Frontend Components** | HomePage, FilterSidebar, ModelTable (TanStack Table), SearchBar, ComparisonBasket, ModelRow, ProviderFilter, CapabilityFilter, PriceRangeFilter | Section 2.3 (lines 185-210) |
| **Backend Services** | ModelQueryService, FilterService | Section 2.1 (lines 103-108) |
| **Data Models** | models (read-only), capabilities, benchmark_scores (top 3-5 for table display) | Section 3.1 (lines 289-415) |
| **API Endpoints** | GET /api/models (cached 1hr) | Section 2.4 (lines 242), Section 7.1 (lines 689-714) |
| **State Management** | Zustand: filterState, viewPreferences; TanStack Query: models cache (5min stale) | Section 2.3 (lines 213-228) |

**Integration Points:**
- TanStack Table for sorting, filtering, pagination
- Redis caching (Layer 2): 1hr TTL for GET /api/models
- Client-side filtering (no server round-trips for 100 models)
- PostgreSQL full-text search for model/provider search

**Performance Strategy:**
- Multi-layer caching: Client (5min) → Redis (1hr) → PostgreSQL
- Client-side filtering faster than server queries
- Virtual scrolling for 100+ rows (TanStack Table)

---

## Epic 4: Model Detail & Cost Calculator

**Goal:** Detailed model view with comprehensive specifications and embedded cost calculator

**Stories:** 12 | **Status:** ✅ READY (architecture fully defined)

### Component Mapping

| Component Type | Components | Architecture Reference |
|----------------|-----------|----------------------|
| **Frontend Components** | ModelDetailModal, OverviewTab, BenchmarksTab, PricingTab, CostCalculatorWidget, CostResultsTable, CostChart | Section 2.3 (lines 193-201) |
| **Backend Services** | ModelDetailService, CostCalculationService, BenchmarkQueryService | Section 2.1 (lines 103-108) |
| **Data Models** | models (full detail), all benchmark_scores, capabilities (complete) | Section 3.1 (lines 289-415) |
| **API Endpoints** | GET /api/models/{id} (cached 30min) | Section 2.4 (lines 243), Section 7.1 (lines 717-764) |
| **Calculations** | CostEstimator domain service (client-side for <100ms response) | Section 2.1 (line 124) |

**Integration Points:**
- React Router for modal navigation
- Chart.js for cost comparison visualization
- TanStack Query for model detail caching (30min TTL)
- Real-time cost calculation (<100ms requirement)

**Key Features:**
- Progressive disclosure: Table → Modal → Detail tabs
- Instant feedback on calculator input
- Cost savings percentage vs. most expensive option

---

## Epic 5: Multi-Model Comparison & Visualization

**Goal:** Side-by-side model comparison with interactive benchmark charts

**Stories:** 14 | **Status:** ✅ READY (detailed tech spec generated 2025-01-17)

### Component Mapping

| Component Type | Components | Architecture Reference |
|----------------|-----------|----------------------|
| **Frontend Components** | ComparisonPage, ModelCard, ComparisonTable, BenchmarkChart, CapabilityMatrix, PricingComparisonChart, ChartTypeSwit cher | Section 2.3 (lines 198-202) |
| **Backend Services** | ModelComparisonService, ChartDataService | Section 2.1 (lines 103-108) |
| **Data Models** | models (batch fetch 2-5), all benchmark_scores for selected models | Section 3.1 (lines 289-415) |
| **API Endpoints** | GET /api/models/compare?ids=x,y,z, GET /api/models/chart-data | Section 7.1 (custom for comparison) |
| **Visualization** | Chart.js: bar charts, radar charts (future), grouped comparisons | Section 1.1 (line 37) |

**Integration Points:**
- Chart.js 4.4.1 for canvas rendering (<1s chart load requirement)
- Zustand comparison basket state (selected models)
- Batch API optimization for 2-5 models
- Export comparison feature (CSV/PNG)

**Performance Targets:**
- Chart rendering: <1 second for up to 10 models
- Comparison page load: <2 seconds total

**Tech Spec:** `docs/tech-spec-epic-5.md` (generated 2025-01-17)

---

## Epic 6: Smart Filters (Best Value - QAPS Algorithm)

**Goal:** Algorithm-driven "Best Value" filter ranking models by quality-adjusted price per score

**Stories:** 10 | **Status:** ✅ READY (QAPS algorithm fully specified)

### Component Mapping

| Component Type | Components | Architecture Reference |
|----------------|-----------|----------------------|
| **Frontend Components** | BestValueButton, QAPSScoreDisplay, ExplanationPanel, ValueIndicator | Section 2.3 (filters section) |
| **Backend Services** | QAPSCalculationService, BestValueRankingService, BenchmarkNormalizer | Section 2.1 (lines 121-123), Section 5 (lines 541-651) |
| **Data Models** | benchmarks (weights), model_computed_metrics (QAPS cache), normalized_scores | Section 3.1 (lines 370-375) |
| **API Endpoints** | GET /api/smart-filters/best-value?limit=10 (cached 1hr) | Section 2.4 (line 245), Section 7.1 (lines 767-789) |
| **Algorithm** | QAPS = Composite Quality Score / Total Price; Weighted benchmark normalization | Section 5 (lines 541-651) |

**Integration Points:**
- Redis caching for QAPS scores (1hr TTL)
- Domain event-driven cache invalidation (on model/benchmark updates)
- MassTransit (future) for QAPS recalculation background jobs

**Key Architecture:**
- **QAPS Formula:** `QAPS = Σ(Normalized Score × Weight) / Avg Price`
- **Benchmark Weights:** Reasoning 30%, Code 25%, Math 20%, Language 15%, Multimodal 10%
- **Normalization:** `(Score - Min) / (Max - Min)` to 0-1 range
- **Edge Cases:** Free models (rank separately), insufficient data (<3 benchmarks)

**Algorithm Reference:** Section 5.1-5.5 (lines 541-651)

---

## Epic 7: Data Quality & Admin Dashboard

**Goal:** Data validation, freshness indicators, admin metrics, and audit logging

**Stories:** 10 | **Status:** ✅ READY (validation strategy fully defined)

### Component Mapping

| Component Type | Components | Architecture Reference |
|----------------|-----------|----------------------|
| **Frontend Components** | DataFreshnessIndicator, ValidationMessages, AdminDashboard, AuditLog, DataQualityMetrics | Section 2.3 (admin components) |
| **Backend Services** | DataQualityService, ValidationService, AuditLogService | Section 2.1 (line 108) |
| **Data Models** | admin_audit_log (JSONB changes), models (last_scraped_at, pricing_valid_from/to) | Section 3.1 (lines 379-392) |
| **API Endpoints** | GET /api/admin/dashboard/metrics, GET /api/admin/audit-log | Section 2.4 (lines 258-259) |
| **Validation** | FluentValidation (backend), Zod (frontend), anomaly detection for benchmark scores | Section 12.2 (lines 1504-1522) |

**Integration Points:**
- FluentValidation + Zod defense-in-depth (client and server validation)
- Audit logging with JSONB change tracking (before/after snapshots)
- Data freshness timestamps displayed on public interface
- Admin dashboard with data quality metrics

**Validation Strategy:**
- Pricing: Positive numbers, logical input/output relationships
- Benchmarks: Scores within typical ranges, flagged if anomalous
- Timestamps: Automatic tracking on create/update
- Audit trail: All admin actions logged with user, IP, user agent

---

## Epic 8: Responsive Design & Mobile Optimization

**Goal:** Fully responsive interface functional on desktop, tablet, and mobile devices

**Stories:** 10 | **Status:** ✅ READY (responsive patterns fully specified)

### Component Mapping

| Component Type | Components | Architecture Reference |
|----------------|-----------|----------------------|
| **Frontend Components** | FilterDrawer (mobile), CardLayout (mobile table), MobileNav, TouchOptimizedInputs | Section 2.3 (component adaptations) |
| **Backend Changes** | None (purely frontend) | N/A |
| **Data Changes** | None | N/A |
| **API Changes** | None | N/A |
| **Responsive Strategy** | TailwindCSS breakpoints, progressive enhancement, touch optimization | Section 1.1 (line 38), UX Spec responsive patterns |

**Integration Points:**
- TailwindCSS 3.4.0 responsive utilities (sm, md, lg, xl, 2xl)
- Progressive disclosure: Table → Cards on mobile
- Drawer pattern for filters (mobile)
- Touch-friendly hit targets (48px minimum)
- Horizontal scrolling with snap points (mobile tables)

**Responsive Patterns:**
- **Desktop (≥1024px):** Full table with sidebar filters
- **Tablet (768-1023px):** Compressed table, drawer filters
- **Mobile (<768px):** Card layout, hamburger menu, bottom sheet

**Performance:**
- Bundle size optimization for mobile networks
- Lazy loading for off-screen components
- Image optimization (future CDN integration)

---

## Story Readiness Summary

### Implementation Readiness by Epic

| Epic | Stories | Implementable | Blockers | Notes |
|------|---------|---------------|----------|-------|
| Epic 1 | 10/10 | ✅ 10/10 | None | **COMPLETE** - All stories delivered and validated |
| Epic 2 | 12/12 | ✅ 12/12 | None | Architecture complete, stories pending drafting |
| Epic 3 | 15/15 | ✅ 15/15 | None | All stories drafted and approved, ready for development |
| Epic 4 | 12/12 | ✅ 12/12 | None | Architecture complete, clear tech stack and component design |
| Epic 5 | 14/14 | ✅ 14/14 | None | Detailed tech spec generated (2025-01-17) |
| Epic 6 | 10/10 | ✅ 10/10 | None | QAPS algorithm fully specified, caching strategy defined |
| Epic 7 | 10/10 | ✅ 10/10 | None | Validation rules, audit logging, dashboard metrics specified |
| Epic 8 | 10/10 | ✅ 10/10 | None | Responsive patterns, breakpoints, mobile components defined |

**Overall:** ✅ **83/83 stories (100%) ready for implementation**

---

## Cross-Cutting Concerns Mapping

### Caching Strategy

| Layer | Scope | TTL | Architecture Reference |
|-------|-------|-----|----------------------|
| **Layer 1: Client** | TanStack Query cache | 5min stale, 30min cache | Section 4.1 (lines 463-467) |
| **Layer 2: API Response** | Redis cache | GET /api/models: 1hr, GET /api/models/{id}: 30min | Section 4.1 (lines 469-474) |
| **Layer 3: Computed Values** | Redis cache | QAPS scores: 1hr, Dashboard metrics: 30min | Section 4.1 (lines 476-482) |
| **Layer 4: Database** | PostgreSQL | Source of truth | Section 4.1 (lines 484-488) |

**Cache Invalidation:**
- Admin updates model pricing → Invalidate: `cache:models:*`, `cache:bestvalue:*`, `cache:qaps:{id}:*`
- Admin adds benchmark score → Invalidate: `cache:model:{id}:*`, `cache:bestvalue:*`
- Domain event-driven invalidation (Section 4.3, lines 507-532)

### Security Layers

| Concern | Implementation | Architecture Reference |
|---------|---------------|----------------------|
| **Authentication** | JWT tokens (admin), HttpOnly cookies | ADR-008 (lines 1220-1237) |
| **Authorization** | Single admin role (MVP), RBAC (future) | Section 12.1 (lines 1489-1502) |
| **Input Validation** | FluentValidation (backend) + Zod (frontend) defense-in-depth | Section 12.2 (lines 1504-1522) |
| **SQL Injection** | EF Core parameterized queries | Section 12.3 (lines 1524-1528) |
| **XSS Prevention** | React auto-escaping, CSP headers | Section 12.4 (lines 1530-1535) |
| **CSRF Protection** | SameSite cookies, anti-forgery tokens | Section 12.5 (lines 1537-1539) |

### Testing Coverage

| Epic | Unit Tests | Integration Tests | E2E Tests | Architecture Reference |
|------|-----------|-------------------|-----------|----------------------|
| Epic 1 | Domain services, validators | EF Core repositories, API controllers | Health check flow | Section 10.2-10.3 (lines 1255-1357) |
| Epic 2 | AdminModelService, validators | Admin CRUD APIs, cache invalidation | Login → Create model → Verify public API | Section 10.2 (lines 1255-1312) |
| Epic 3 | Filter logic, formatters | TanStack Table integration, MSW API | Filter → Select → Compare flow | Section 10.3 (lines 1340-1357) |
| Epic 4 | Cost calculator, validators | Model detail API | Open modal → Calculate cost | Section 10.2-10.3 |
| Epic 5 | Chart helpers, comparison logic | Chart.js integration | Select 3 models → Compare → Export | Section 10.3 (lines 1349-1357) |
| Epic 6 | QAPS calculator, normalizer | QAPS API, cache invalidation | Activate best value filter | Section 10.2 (lines 1267-1281) |
| Epic 7 | Data validators, audit logger | Validation rules, audit logging | Admin bulk update → Verify audit log | Section 10.2 |
| Epic 8 | Responsive utils | Mobile component rendering | Mobile navigation flow | Section 10.3 |

**Coverage Goals:** 70%+ overall, 90%+ domain layer (Section 10.4, lines 1359-1365)

---

## Architecture Decision Records (ADRs) Impact

| ADR | Decision | Affected Epics | Architecture Reference |
|-----|----------|----------------|----------------------|
| **ADR-001** | Hexagonal Architecture | All epics | Lines 1103-1115 |
| **ADR-002** | SPA (React) vs SSR (Next.js) | Epics 3-8 (all frontend) | Lines 1117-1133 |
| **ADR-003** | Monorepo vs Polyrepo | Epic 1 (foundation) | Lines 1135-1152 |
| **ADR-004** | TanStack Query + Zustand | Epics 3-8 (state management) | Lines 1154-1170 |
| **ADR-005** | Multi-layer caching (Redis) | Epics 3, 4, 6 (performance) | Lines 1172-1187 |
| **ADR-006** | PostgreSQL + TimescaleDB | Epic 1 (data), Phase 2 (pricing history) | Lines 1189-1203 |
| **ADR-007** | MassTransit (future) | Epic 6 (QAPS recalc), Phase 2 (scraping) | Lines 1205-1218 |
| **ADR-008** | JWT for admin auth (MVP) | Epic 2 (admin panel) | Lines 1220-1237 |

---

## Technology Stack Alignment

| Technology | Version | Primary Epic Usage | Architecture Reference |
|------------|---------|-------------------|----------------------|
| **ASP.NET Core** | 8.0 | Epics 1-7 (backend) | Line 24 |
| **React** | 18.2.0 | Epics 1, 3-8 (frontend) | Line 26 |
| **TypeScript** | 5.3.0 | Epics 1, 3-8 (type safety) | Line 27 |
| **Vite** | 5.0.0 | Epic 1 (build), all frontend | Line 28 |
| **PostgreSQL** | 16.0 | Epic 1 (data), all backend | Line 29 |
| **TimescaleDB** | 2.13.0 | Epic 1 (setup), Phase 2 (pricing history) | Line 30 |
| **Entity Framework Core** | 8.0.0 | Epics 1-7 (data access) | Line 31 |
| **Redis** | 7.2 | Epics 1, 3-7 (caching) | Line 32 |
| **Zustand** | 4.4.7 | Epics 3-8 (client state) | Line 34 |
| **TanStack Query** | 5.17.0 | Epics 3-8 (server state) | Line 35 |
| **TanStack Table** | 8.11.0 | Epic 3 (table), Epic 5 (comparison) | Line 36 |
| **Chart.js** | 4.4.1 | Epics 4-5 (visualization) | Line 37 |
| **TailwindCSS** | 3.4.0 | Epics 1, 3-8 (styling) | Line 38 |
| **FluentValidation** | (via EF) | Epics 2, 7 (validation) | Line 909 |
| **Zod** | 3.22.4 | Epics 2-7 (client validation) | Line 40 |

---

## Functional Requirements Traceability

### FR Coverage by Epic

| Epic | Covered FRs | FR IDs | Notes |
|------|-------------|--------|-------|
| Epic 1 | FR001-FR005 | Model data management | Database schema, entities, repositories |
| Epic 2 | FR018-FR024 | Admin panel (models + benchmarks) | CRUD operations, CSV import, timestamps |
| Epic 3 | FR006-FR011, FR028-FR029 | Public comparison + search | Table, filters, selection, search |
| Epic 4 | FR010, FR012-FR015 | Model detail + cost calculator | Modal, tabs, cost estimation |
| Epic 5 | FR009, FR011 | Multi-model comparison | Charts, capability matrix |
| Epic 6 | FR016-FR017 | Smart filters (Best Value) | QAPS algorithm, ranking, explanations |
| Epic 7 | FR025-FR027 | Data quality | Validation, freshness, anomaly detection |
| Epic 8 | FR030-FR031 | Responsive design | Mobile adaptations, breakpoints |

**Performance FRs (FR032-FR035):** Cross-cutting - addressed via caching strategy (Epics 1, 3-6)

---

## Non-Functional Requirements Alignment

| NFR | Requirement | Primary Epics | Architecture Strategy |
|-----|-------------|---------------|---------------------|
| **NFR001: Performance** | <2s load, <100ms calc, <1s charts | Epics 1, 3-6 | Multi-layer caching, client-side filtering, Chart.js optimization |
| **NFR002: Scalability** | 10K+ MAU, 100+ models | Epics 1, 3, 6 | Redis caching (80%+ hit rate), PostgreSQL indexing, connection pooling |
| **NFR003: Data Accuracy** | 95%+ pricing accuracy | Epics 2, 7 | FluentValidation + Zod, admin verification, data quality metrics |
| **NFR004: Availability** | 99% uptime, graceful degradation | Epic 1 | Health checks, retry policies, error boundaries, fallback data |
| **NFR005: Maintainability** | Hexagonal architecture | All epics | Domain/Application/Infrastructure separation, repository pattern |
| **NFR006: Usability** | 70%+ task completion | Epics 3-6 | Progressive disclosure, smart filters, instant feedback |
| **NFR007: Accessibility** | WCAG 2.1 AA | Epics 3-8 | Semantic HTML, ARIA labels, keyboard navigation, color contrast |

---

## Dependencies and Integration Points

### Epic Dependencies (Sequential)

```
Epic 1 (Foundation)
  ↓
Epic 2 (Admin CRUD) ← Depends on Epic 1 database + API foundation
  ↓
Epic 3 (Public Table) ← Depends on Epic 1 frontend shell + Epic 2 seeded data
  ↓
Epic 4 (Model Detail) ← Depends on Epic 3 model selection
  ↓
Epic 5 (Comparison) ← Depends on Epic 3 comparison basket
  ↓
Epic 6 (Smart Filter) ← Depends on Epic 3 table + Epic 2 benchmark data
  ↓
Epic 7 (Data Quality) ← Depends on Epic 2 admin panel
  ↓
Epic 8 (Responsive) ← Depends on Epics 3-7 desktop components
```

### Parallel Development Opportunities

After Epic 1 completion:
- **Track A:** Epic 2 (Admin) + Epic 3 (Public Table) can be parallelized with coordination
- **Track B:** Epic 4 (Detail) + Epic 5 (Comparison) can be parallelized after Epic 3
- **Track C:** Epic 6 (Smart Filter) + Epic 7 (Quality) can be parallelized after Epic 2-3

---

## File References

**Source Documents:**
- Solution Architecture: `/docs/solution-architecture.md` (section 6.1, lines 655-667)
- Cohesion Check Report: `/docs/cohesion-check-report.md` (section 3, lines 88-112)
- PRD: `/docs/PRD.md`
- Epics: `/docs/epics.md`
- BMM Workflow Status: `/docs/bmm-workflow-status.md`

**Related Documents:**
- Tech Spec Epic 1: `/docs/tech-spec-epic-1.md`
- Tech Spec Epic 5: `/docs/tech-spec-epic-5.md`
- Tech Spec Summary (Epics 2-8): `/docs/tech-spec-epic-2-8-summary.md`
- Architecture Decisions: `/docs/architecture-decisions.md`

---

**Generated:** 2025-10-16
**Status:** ✅ Complete and validated
**Next Update:** When new epics added or architecture significantly revised
