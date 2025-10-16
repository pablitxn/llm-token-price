# Tech Specs Summary: Epics 2-8

## Epic 2: Model Data Management & Admin CRUD (12 stories)

**Key Components:**
- JWT authentication (`AdminAuthController`, HttpOnly cookies)
- Admin CRUD services (`AdminModelService`, `AdminBenchmarkService`)
- Admin UI (`AdminLayout`, `ModelForm`, `BenchmarkForm`, `CSVImport`)
- Audit logging (`AuditLogRepository`, admin_audit_log table)

**Critical Implementations:**
- FluentValidation for admin forms (pricing >0, unique model names)
- Bulk operations (checkbox selection, bulk update/delete)
- CSV import with validation and error reporting
- Timestamp tracking (created_at, updated_at on all entities)

---

## Epic 3: Public Comparison Table Interface (15 stories)

**Key Components:**
- TanStack Table (`ModelTable`, column definitions, virtualization)
- Filtering (`FilterSidebar`, `ProviderFilter`, `CapabilityFilter`, `PriceRangeFilter`)
- Search (`SearchBar`, debounced 300ms, PostgreSQL full-text)
- Comparison basket (`ComparisonBasket`, Zustand state, max 5 models)

**Critical Implementations:**
- Client-side sorting/filtering (no server round-trips after initial load)
- Redis caching (`GET /api/models`, 1hr TTL, invalidate on admin update)
- Responsive table → card layout on mobile (<768px)
- Performance: <2s page load with 100+ models

---

## Epic 4: Model Detail & Cost Calculator (12 stories)

**Key Components:**
- Modal system (`ModelDetailModal`, tabbed interface, URL state)
- Tabs (`OverviewTab`, `BenchmarksTab`, `PricingTab`)
- Cost calculator (`CostCalculatorWidget`, `CostCalculationService`)
- Standalone calculator page (`/calculator`, all models, preset scenarios)

**Critical Implementations:**
- Cost formula: `(volume × ratio × inputPrice) + (volume × (1-ratio) × outputPrice)`
- Real-time calculations (<100ms, debounced input)
- Shareable URLs (`/calculator?volume=5000000&ratio=60`)
- Chart.js bar chart for cost comparison

---

## Epic 5: Multi-Model Comparison & Visualization (14 stories)

**Key Components:**
- Comparison page (`/compare?models=1,2,3`)
- Side-by-side cards (`ModelCard`, horizontal scroll for >3)
- Comparison table (`ComparisonTable`, attributes aligned vertically)
- Charts (`BenchmarkChart` grouped bars, `PricingComparisonChart`)
- Capability matrix (`CapabilityMatrix`, checkmark icons)

**Critical Implementations:**
- Chart.js grouped bar charts (models × benchmarks)
- Metric selector (choose which benchmarks to visualize)
- Interactive tooltips (hover for exact values)
- Export comparison as CSV

---

## Epic 6: Smart Filter - Best Value Algorithm (10 stories)

**Key Components:**
- QAPS algorithm (`QAPSCalculationService`, `BenchmarkNormalizer`)
- Best value endpoint (`GET /api/smart-filters/best-value`)
- UI (`BestValueButton`, `QAPSScoreDisplay`, `ExplanationPanel`)
- Quality score breakdown (weighted benchmarks visualization)

**Critical Implementations:**
- QAPS = Composite Quality Score / Total Price
- Benchmark normalization: `(score - min) / (max - min)`
- Weights: Reasoning 30%, Code 25%, Math 20%, Language 15%, Multimodal 10%
- Redis cache (1hr TTL, invalidate on score/price updates)
- Edge cases: Free models (separate ranking), <3 benchmarks (exclude)

---

## Epic 7: Data Quality & Admin Enhancements (10 stories)

**Key Components:**
- Validation (`PricingValidator`, `BenchmarkScoreValidator`)
- Data freshness (`DataFreshnessIndicator`, color coding: green<7d, yellow<14d, red>14d)
- Admin dashboard (`AdminDashboard`, quality metrics, stale models)
- Bulk operations (`BulkUpdateForm`, checkbox selection)
- Audit log (`AuditLogViewer`, filterable by user/action/date)

**Critical Implementations:**
- Defense-in-depth validation (client Zod → server FluentValidation → DB constraints)
- Anomaly detection (scores outside typical_range flagged)
- Duplicate detection (fuzzy matching on model name + provider)
- CSV import error handling (partial success, download failed rows)

---

## Epic 8: Responsive Design & Mobile Optimization (10 stories)

**Key Components:**
- Mobile table (`ModelCard` layout, vertical stack)
- Filter drawer (`FilterDrawer`, slide-up from bottom)
- Mobile modal (full-screen on <768px)
- Touch interactions (44px touch targets, swipe gestures)

**Critical Implementations:**
- Breakpoints: mobile <768px, tablet 768-1024px, desktop >1024px
- Card layout preserves key info (name, provider, pricing, top capabilities)
- Filters accessible via drawer (button shows active count badge)
- Performance: <3s load on 3G, Lighthouse mobile score >80

---

## Implementation Notes

**All Epics:**
- Follow hexagonal architecture (Domain → Application → Infrastructure)
- Maintain test coverage (70% overall, 90% domain layer)
- Use TypeScript strict mode (no `any` types)
- Document architectural decisions (add ADRs when deviating)

**Tech Stack Consistency:**
- Backend: C# 12, .NET 8, EF Core 8.0, FluentValidation, Serilog
- Frontend: React 18, TypeScript 5.3, Zustand 4.4.7, TanStack Query 5.17, Chart.js 4.4.1
- Testing: xUnit (backend), Vitest (frontend), Playwright (E2E)

**Performance Benchmarks:**
- Page load: <2s (Epic 3)
- Calculations: <100ms (Epic 4, 6)
- Chart rendering: <1s (Epic 5)
- API response: <500ms (with caching)

---

_Detailed per-story implementation in full tech specs (generated for Epic 1)_
_For Epics 2-8: Follow same structure (Architecture Context → Story-by-Story Implementation → Testing → Acceptance Criteria)_
