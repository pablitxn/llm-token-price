# Test Design: Epic 3 - Public Comparison Table Interface

**Date:** 2025-10-21
**Author:** Pablo
**Status:** Draft

---

## Executive Summary

**Scope:** Full test design for Epic 3 - Public Comparison Table Interface

**Risk Summary:**
- Total risks identified: 8
- High-priority risks (≥6): 3 (R-001 PERF, R-002 DATA, R-003 TECH)
- Critical categories: PERF (performance), DATA (cache staleness), TECH (Redis availability)

**Coverage Summary:**
- P0 scenarios: 16 tests (32 hours, ~4 days)
- P1 scenarios: 34 tests (34 hours, ~4.5 days)
- P2/P3 scenarios: 26 tests (10.75 hours, ~1.3 days)
- **Total effort**: 76 tests, 76.75 hours (~10 days with 1 QA engineer)

**Test Level Breakdown:**
- E2E: 13 tests (critical paths, cross-browser, accessibility)
- Integration: 11 tests (API + DB + Cache validation)
- Component: 42 tests (filters, table, basket - majority of coverage)
- Unit: 8 tests (utilities, stores, performance)
- Meta: 2 tests (coverage validation, code quality)

---

## Risk Assessment

### High-Priority Risks (Score ≥6)

| Risk ID | Category | Description | Probability | Impact | Score | Mitigation | Owner | Timeline |
|---------|----------|-------------|-------------|--------|-------|------------|-------|----------|
| **R-001** | **PERF** | Client-side filtering degradation with 200+ models (>100ms latency) | 2 (Possible) | 3 (Critical) | **6** | Virtual scrolling implementation + server-side filtering fallback (Epic 6) | DEV | Story 3.12 |
| **R-002** | **DATA** | Cache invalidation delays cause stale pricing (up to 5min lag after admin update) | 3 (Likely) | 2 (Degraded) | **6** | Monitor cache hit ratio <50%, manual refresh button, cache TTL validation | QA | Story 3.15 |
| **R-003** | **TECH** | Redis unavailability cascades to database overload (10x load spike) | 2 (Possible) | 3 (Critical) | **6** | Graceful degradation to DB-only mode, circuit breaker, connection pool limits | OPS | Story 3.15 |

### Medium-Priority Risks (Score 3-4)

| Risk ID | Category | Description | Probability | Impact | Score | Mitigation | Owner |
|---------|----------|-------------|-------------|--------|-------|------------|-------|
| **R-004** | **TECH** | TanStack Table v8 browser compatibility issues (older Safari, Firefox ESR) | 1 (Unlikely) | 3 (Critical) | **3** | Browserslist config, Babel polyfills, manual cross-browser testing | DEV |
| **R-005** | **BUS** | Empty database on first install shows poor UX (no model data to display) | 2 (Possible) | 2 (Degraded) | **4** | Epic 2 seed scripts mandatory, empty state UI with admin link | PM |
| **R-006** | **SEC** | Rate limiting bypass via distributed IP rotation (DDoS potential) | 1 (Unlikely) | 3 (Critical) | **3** | Monitor API abuse patterns, IP fingerprinting, Cloudflare protection | OPS |

### Low-Priority Risks (Score 1-2)

| Risk ID | Category | Description | Probability | Impact | Score | Action |
|---------|----------|-------------|-------------|--------|-------|--------|
| **R-007** | **OPS** | Deployment cache warming adds deployment complexity (30s longer deploy) | 1 (Unlikely) | 2 (Minor) | **2** | Monitor first-request latency, implement if >500ms observed |
| **R-008** | **PERF** | Bundle size exceeds 200KB gzipped budget (slow 3G users affected) | 1 (Unlikely) | 2 (Minor) | **2** | Bundle analyzer in CI/CD, lazy load admin/charts |

### Risk Category Legend
- **TECH**: Technical/Architecture (flaws, integration, scalability)
- **SEC**: Security (access controls, auth, data exposure)
- **PERF**: Performance (SLA violations, degradation, resource limits)
- **DATA**: Data Integrity (loss, corruption, inconsistency)
- **BUS**: Business Impact (UX harm, logic errors, revenue)
- **OPS**: Operations (deployment, config, monitoring)

---

## Test Coverage Plan

### P0 (Critical) - Run on every commit

**Criteria**: Blocks core journey + High risk (≥6) + No workaround

| Requirement | Test Level | Risk Link | Test Count | Owner | Notes |
|-------------|-----------|-----------|------------|-------|-------|
| **AC-F1**: Model Data Display (homepage table loads) | E2E | R-003 | 2 | QA | Redis failure fallback, empty state |
| **AC-P1**: Initial page load <2s (90% loads) | E2E | R-001 | 1 | QA | Lighthouse CI measurement |
| **AC-P2**: API cache hit <50ms, miss <250ms | Integration | R-002, R-003 | 3 | QA | Redis hit, miss, failure scenarios |
| **AC-F2**: Table sorting (all columns, toggle direction) | Component | - | 2 | DEV | Sort ascending/descending, persistence |
| **AC-F3**: Provider filtering (checkboxes, real-time) | Component | R-001 | 3 | DEV | Single provider, multiple (OR logic), clear |
| **AC-Q4**: Error handling (DB failure, timeout, empty) | E2E | R-003 | 3 | QA | DB down, network timeout, retry button |
| **AC-I2**: Admin updates trigger cache invalidation | Integration | R-002 | 2 | QA | Update model → cache bust → fresh data |

**Total P0**: 16 tests, 32 hours (~4 days)

### P1 (High) - Run on PR to main

**Criteria**: Important features + Medium risk (3-4) + Common workflows

| Requirement | Test Level | Risk Link | Test Count | Owner | Notes |
|-------------|-----------|-----------|------------|-------|-------|
| **AC-F4**: Capability filtering (6 checkboxes, AND logic) | Component | R-001 | 6 | DEV | Each capability + combination |
| **AC-F5**: Price range filtering (dual slider) | Component | R-001 | 3 | DEV | Min, max, combined |
| **AC-F6**: Search functionality (debounced 300ms) | Component | R-001 | 4 | DEV | Name search, provider search, clear, combined |
| **AC-F7**: Model selection (checkboxes, 5 max limit) | Component | - | 4 | DEV | Select, deselect, max limit toast, highlight |
| **AC-F8**: Comparison basket (mini-cards, remove, clear) | Component | - | 4 | DEV | Display basket, remove model, clear all, enable button |
| **AC-F9**: URL state sync (shareable filtered views) | Integration | - | 3 | DEV | Filters → URL, URL → filters, browser back/forward |
| **AC-F10**: Responsive design (desktop, tablet, mobile) | E2E | R-004 | 3 | QA | 1920×1080, 768×1024, 375×667 viewports |
| **AC-P3**: Client-side filtering <100ms (99% of interactions) | Unit | R-001 | 3 | DEV | Provider, capability, price filters with performance measurement |
| **AC-Q2**: Test coverage (70%+ backend, 60%+ frontend) | Meta | - | 1 | QA | Coverage report validation |
| **AC-Q3**: Accessibility (keyboard nav, ARIA labels) | E2E | - | 3 | QA | Tab navigation, screen reader, focus indicators |

**Total P1**: 34 tests, 34 hours (~4.5 days)

### P2 (Medium) - Run nightly/weekly

**Criteria**: Secondary features + Low risk (1-2) + Edge cases

| Requirement | Test Level | Risk Link | Test Count | Owner | Notes |
|-------------|-----------|-----------|------------|-------|-------|
| **AC-F9**: Benchmark scores display (top 3-5 in table) | API | - | 2 | QA | Scores present, N/A for missing |
| **AC-P4**: Bundle size <200KB gzipped (total <400KB) | Unit | R-008 | 1 | DEV | Build artifact size check |
| **AC-Q1**: Code quality (zero `any` types, ESLint pass) | Meta | - | 2 | DEV | TypeScript strict check, lint check |
| **AC-I1**: Epic 1 integration (DB schema unchanged) | Integration | - | 1 | QA | Schema migration validation |
| **AC-I3**: Future epic setup (selectedModels Zustand store) | Unit | - | 2 | DEV | Store structure, actions |
| ModelTable pagination/virtual scrolling (50+ rows) | Component | R-001 | 3 | DEV | Scroll performance, page controls |
| FilterStore state management (set, reset, persist) | Unit | - | 4 | DEV | All filter actions |
| API response caching (TanStack Query 5min stale) | Integration | R-002 | 2 | QA | Cache hit, stale-while-revalidate |

**Total P2**: 17 tests, 8.5 hours (~1 day)

### P3 (Low) - Run on-demand

**Criteria**: Nice-to-have + Exploratory + Performance benchmarks

| Requirement | Test Level | Test Count | Owner | Notes |
|-------------|-----------|------------|-------|-------|
| Performance benchmarking (100 models, 200 models) | E2E | 2 | QA | Measure actual latency |
| Visual regression testing (screenshot comparison) | E2E | 3 | QA | Table, filters, basket |
| Cross-browser compatibility (Edge, older Safari) | E2E | 2 | QA | Manual testing |
| Accessibility audit (axe-core automated scan) | E2E | 1 | QA | Full page scan |
| Bundle analysis (tree-shaking verification) | Meta | 1 | DEV | Webpack bundle analyzer |

**Total P3**: 9 tests, 2.25 hours (~0.3 days)

---

**Grand Total**: 76 tests, 76.75 hours (~10 days)

---

## Execution Order

### Smoke Tests (<5 min)

**Purpose**: Fast feedback, catch build-breaking issues

- [ ] Homepage renders with table (2min)
- [ ] API /api/models returns 200 OK (30s)
- [ ] Provider filter applies without error (1min)
- [ ] Model selection checkbox functional (1min)

**Total**: 4 scenarios (~5 min)

### P0 Tests (<10 min)

**Purpose**: Critical path validation

- [ ] Model Data Display - Table loads with 50+ models (E2E)
- [ ] Initial page load <2s - Lighthouse measurement (E2E)
- [ ] API cache hit/miss/failure - Redis scenarios (Integration)
- [ ] Table sorting - Ascending/descending toggle (Component)
- [ ] Provider filtering - Single, multiple, clear (Component)
- [ ] Error handling - DB failure, timeout, retry (E2E)
- [ ] Cache invalidation - Admin update triggers bust (Integration)

**Total**: 16 scenarios (~10 min with parallelization)

### P1 Tests (<30 min)

**Purpose**: Important feature coverage

- [ ] Capability filtering - 6 capabilities + combinations (Component)
- [ ] Price range filtering - Min, max, combined (Component)
- [ ] Search functionality - Name, provider, clear, combined (Component)
- [ ] Model selection - Select, deselect, max limit, highlight (Component)
- [ ] Comparison basket - Display, remove, clear, enable (Component)
- [ ] URL state sync - Filters ↔ URL, browser navigation (Integration)
- [ ] Responsive design - Desktop, tablet, mobile viewports (E2E)
- [ ] Client-side filtering performance - <100ms validation (Unit)
- [ ] Test coverage validation - 70% backend, 60% frontend (Meta)
- [ ] Accessibility - Keyboard nav, ARIA, focus (E2E)

**Total**: 34 scenarios (~30 min)

### P2/P3 Tests (<60 min)

**Purpose**: Full regression coverage

**P2 (Nightly/Weekly):**
- [ ] Benchmark scores display (API)
- [ ] Bundle size validation (Unit)
- [ ] Code quality checks (Meta)
- [ ] Epic 1 integration (Integration)
- [ ] Future epic setup (Unit)
- [ ] Table pagination/virtual scrolling (Component)
- [ ] FilterStore state management (Unit)
- [ ] API response caching (Integration)

**P3 (On-Demand):**
- [ ] Performance benchmarking - 100, 200 models (E2E)
- [ ] Visual regression testing (E2E)
- [ ] Cross-browser compatibility (E2E)
- [ ] Accessibility audit - axe-core (E2E)
- [ ] Bundle analysis (Meta)

**Total**: 26 scenarios (~60 min total, run separately)

---

## Resource Estimates

### Test Development Effort

| Priority | Count | Hours/Test | Total Hours | Notes |
|----------|-------|------------|-------------|-------|
| P0 | 16 | 2.0 | 32 | Complex setup (Redis mocks, DB mocks), error simulation |
| P1 | 34 | 1.0 | 34 | Standard coverage (component testing, integration) |
| P2 | 17 | 0.5 | 8.5 | Simple scenarios (unit tests, API tests) |
| P3 | 9 | 0.25 | 2.25 | Exploratory (manual testing, benchmarks) |
| **Total** | **76** | **-** | **76.75** | **~10 days (1 QA engineer)** |

### Prerequisites

**Test Data:**
- `ModelFactory` - Faker-based factory for Model entities (auto-cleanup via TestContainers Respawn)
- `BenchmarkScoreFactory` - Generates realistic benchmark scores (MMLU, HumanEval, GSM8K)
- `CapabilityFixture` - Setup/teardown for model capabilities test data

**Tooling:**
- **Playwright** - E2E testing framework (already configured in Epic 1)
- **TestContainers** - PostgreSQL 16 + Redis 7.2 containers (from Epic 1)
- **MSW** (Mock Service Worker) - API mocking for frontend component tests
- **Vitest** - Unit testing for frontend utilities and hooks
- **xUnit + Moq** - Backend unit testing with mocking
- **Lighthouse CI** - Performance measurement automation
- **axe-core** - Accessibility testing automation

**Environment:**
- Development: Local PostgreSQL + Redis (Docker Compose)
- CI/CD: TestContainers with GitHub Actions (parallel test execution)
- Staging: Full stack deployment for E2E tests (post-deployment smoke tests)

---

## Quality Gate Criteria

### Pass/Fail Thresholds

- **P0 pass rate**: 100% (no exceptions - blocks deployment)
- **P1 pass rate**: ≥95% (waivers required for failures, approved by Tech Lead + QA Lead)
- **P2/P3 pass rate**: ≥90% (informational, does not block release)
- **High-risk mitigations**: 100% complete or approved waivers (R-001, R-002, R-003 must be addressed)

### Coverage Targets

- **Critical paths** (P0 scenarios): ≥80% code coverage
- **Security scenarios** (SEC category risks): 100% coverage (no compromises)
- **Business logic** (API + domain services): ≥70% code coverage
- **Edge cases** (P2/P3 scenarios): ≥50% code coverage

### Non-Negotiable Requirements

- [ ] All P0 tests pass (16/16)
- [ ] No high-risk (score ≥6) items unmitigated (R-001, R-002, R-003 resolved or accepted)
- [ ] Security tests (R-006 rate limiting) pass 100%
- [ ] Performance targets met:
  - [ ] Initial page load <2s (AC-P1)
  - [ ] API cache hit <50ms, miss <250ms (AC-P2)
  - [ ] Client-side filtering <100ms (AC-P3)
  - [ ] Bundle size <200KB gzipped (AC-P4)
- [ ] Lighthouse Performance Score ≥90
- [ ] Zero ESLint errors, zero TypeScript `any` types (AC-Q1)
- [ ] Test coverage ≥70% backend, ≥60% frontend (AC-Q2)
- [ ] Accessibility: WCAG 2.1 AA compliance (AC-Q3)

### Gate Decision Matrix

| Condition | Action |
|-----------|--------|
| All P0 pass + High risks mitigated + Coverage ≥70% | **✅ APPROVE** - Deploy to production |
| P0 pass rate 95-99% + Waiver approved | **⚠️ CONDITIONAL APPROVE** - Deploy with monitoring |
| P0 pass rate <95% OR High risk unmitigated | **❌ REJECT** - Block deployment, fix and retest |
| Security test failure (R-006) | **❌ CRITICAL REJECT** - No deployment until resolved |

---

## Mitigation Plans

### R-001: Client-side filtering degradation with 200+ models (Score: 6)

**Mitigation Strategy:**
1. **Immediate** (Story 3.12): Implement virtual scrolling with TanStack Table's virtualization API
2. **Fallback** (Epic 6): Add server-side filtering option if virtual scrolling insufficient
3. **Monitoring**: Track 99th percentile filtering time in development with React DevTools Profiler

**Owner:** DEV (Frontend Engineer)

**Timeline:** Story 3.12 implementation (within Epic 3)

**Status:** Planned

**Verification:**
- [ ] Performance test with 200 models shows <100ms filtering (P3 benchmark test)
- [ ] Virtual scrolling renders only visible rows (measured via React DevTools)
- [ ] Fallback API endpoint `/api/models?provider=X` available (Epic 6)

---

### R-002: Cache invalidation delays cause stale pricing (Score: 6)

**Mitigation Strategy:**
1. **Immediate** (Story 3.15): Implement cache invalidation via domain events (`ModelUpdatedEvent` → cache bust)
2. **Monitoring**: Add Prometheus metric `api_cache_staleness_seconds` to track TTL
3. **UX Enhancement**: Add manual refresh button with "Last updated: X min ago" indicator
4. **Alert**: Trigger alert if cache hit ratio drops below 50% (indicates cache thrashing)

**Owner:** QA (Integration Testing) + OPS (Monitoring)

**Timeline:** Story 3.15 (cache optimization)

**Status:** Planned

**Verification:**
- [ ] Admin model update triggers `ModelUpdatedEvent` (integration test)
- [ ] Cache key `cache:models:list:v1` deleted on event (integration test)
- [ ] Next API request fetches fresh data from DB (integration test)
- [ ] Cache hit ratio >80% after warm-up period (monitoring dashboard)

---

### R-003: Redis unavailability cascades to database overload (Score: 6)

**Mitigation Strategy:**
1. **Graceful Degradation**: Catch Redis connection errors → fallback to direct DB queries
2. **Circuit Breaker**: After 5 consecutive Redis failures, open circuit for 30s (prevent cascade)
3. **Connection Pooling**: Limit DB connections to max 20 concurrent (prevent overload)
4. **Monitoring**: Alert on Redis connection failures >5/hour

**Owner:** OPS (Infrastructure) + DEV (Backend)

**Timeline:** Story 3.15 (resilience patterns)

**Status:** Planned

**Verification:**
- [ ] Redis container stop triggers fallback to DB (integration test)
- [ ] Response time degrades to ~300ms but remains functional (performance test)
- [ ] Circuit breaker opens after 5 failures (unit test)
- [ ] DB connection pool limits enforced (integration test)
- [ ] Alert triggered on Redis failure (monitoring validation)

---

## Assumptions and Dependencies

### Assumptions

1. **Database Populated**: Epic 2 seed scripts provide 10-15 models with capabilities and benchmarks (validates AC-F1)
2. **Network Latency**: <100ms typical latency for 90th percentile users (performance targets achievable)
3. **Redis Available**: Production deployment includes Redis 7.2 (Architecture assumes caching layer)
4. **TanStack Table Stability**: v8 API remains stable through Epic 3 development (no breaking changes)
5. **Test Infrastructure**: Epic 1 test framework (TestContainers, Playwright) functional and passing

### Dependencies

1. **Epic 1 Complete** - Required by: Epic 3 start
   - Database schema (models, capabilities, benchmarks, benchmark_scores)
   - PostgreSQL + Redis connections validated
   - Frontend shell with TailwindCSS, Vite configured
   - CI/CD pipeline functional

2. **Epic 2 Complete** - Required by: Epic 3 start
   - Admin panel with model CRUD operations
   - Benchmark score management functional
   - Database seeded with 10-15 models (minimum data for testing)
   - **Story 2.13 (Technical Debt)** - BLOCKS Epic 3 (27 E2E tests failing, must fix first)

3. **Test Data Factories** - Required by: P0 test development (Story 3.2)
   - `ModelFactory` with Faker integration
   - `BenchmarkScoreFactory` for realistic test data
   - TestContainers Respawn for auto-cleanup

4. **Monitoring Infrastructure** - Required by: Story 3.15 (cache optimization)
   - Prometheus metrics collection
   - Grafana dashboard templates
   - Alert manager configuration

### Risks to Test Plan

- **Risk**: Story 2.13 (Epic 2 technical debt) not complete by Epic 3 start
  - **Impact**: 27 E2E tests failing blocks baseline establishment
  - **Contingency**: Defer Epic 3 start by 1 sprint, prioritize Story 2.13 fix

- **Risk**: Test data factories not ready for P0 test development
  - **Impact**: Delays P0 test implementation by 2-3 days
  - **Contingency**: Use manual test data setup temporarily, refactor to factories later

- **Risk**: Redis unavailable in CI/CD environment (TestContainers issue)
  - **Impact**: Integration tests fail, can't validate cache behavior
  - **Contingency**: Mock Redis in CI, run full integration tests in staging only

---

## Approval

**Test Design Approved By:**

- [ ] Product Manager: _________ Date: _________
- [ ] Tech Lead: _________ Date: _________
- [ ] QA Lead: _________ Date: _________

**Comments:**

---

## Appendix

### Knowledge Base References
- `risk-governance.md` - Risk classification framework
- `probability-impact.md` - Risk scoring methodology
- `test-levels-framework.md` - Test level selection
- `test-priorities-matrix.md` - P0-P3 prioritization

### Related Documents
- PRD: `/docs/PRD.md`
- Epic: `/docs/epics.md` (Epic 3)
- Architecture: `/docs/solution-architecture.md`
- Tech Spec: `/docs/tech-spec-epic-3.md`

---

**Generated by**: BMad TEA Agent - Test Architect Module
**Workflow**: `bmad/bmm/testarch/test-design`
**Version**: 4.0 (BMad v6)
