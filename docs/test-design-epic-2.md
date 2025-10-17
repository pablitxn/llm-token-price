# Test Design: Epic 2 - Model Data Management & Admin CRUD

**Date:** 2025-10-17
**Author:** Pablo
**Status:** Draft

---

## Executive Summary

**Scope:** Full test design for Epic 2 - Admin Panel & Model Data Management

**Risk Summary:**

- Total risks identified: 8
- High-priority risks (≥6): 4 (SEC: 2, DATA: 1, TECH: 1)
- Critical categories: Security (JWT, XSS), Data Integrity (pricing errors, cache consistency)

**Coverage Summary:**

- P0 scenarios: 22 (44 hours)
- P1 scenarios: 29 (29 hours)
- P2 scenarios: 46 (23 hours)
- P3 scenarios: 10 (2.5 hours)
- **Total effort**: 98.5 hours (~13 days)

**Key Focus Areas:**

Epic 2 introduces admin authentication and CRUD operations for model data management. Testing emphasizes security (JWT tokens, XSS prevention), data integrity (pricing validation, audit logging), and cache synchronization between admin mutations and public API. The test strategy balances comprehensive security coverage (100% for SEC category) with pragmatic unit testing for validation logic and integration testing for cache invalidation workflows.

---

## Risk Assessment

### High-Priority Risks (Score ≥6)

| Risk ID | Category | Description | Probability | Impact | Score | Mitigation | Owner | Timeline |
| ------- | -------- | ----------- | ----------- | ------ | ----- | ---------- | ----- | -------- |
| R-001 | SEC | JWT token security compromise (hardcoded credentials in MVP, weak secret generation) | 2 | 3 | 6 | Generate 32+ char secret with cryptographically secure random, store in environment variables only, rotate regularly, monitor for unauthorized 401s, add pre-commit hook to scan for hardcoded secrets | Backend | Before production |
| R-002 | DATA | Admin enters incorrect pricing (typo: $0.01 instead of $0.10 causing 10x error) | 3 | 2 | 6 | Implement duplicate price check (warn if price differs >50% from similar models), audit log tracks changes for rollback, admin dashboard shows recent price changes for review | Backend + QA | Story 2.5 |
| R-003 | TECH | Cache invalidation race condition (admin updates model, public API serves stale data briefly) | 2 | 3 | 6 | Domain events published synchronously before API response, cache bust completes <100ms, acceptable 1s eventual consistency window, integration tests verify cache invalidation | Backend | Story 2.5-2.7 |
| R-004 | SEC | XSS via admin input fields (stored XSS could compromise other admins or leak data) | 2 | 3 | 6 | React escapes by default, no `dangerouslySetInnerHTML` in admin panel, Content-Security-Policy headers configured, input sanitization validation | Frontend + Backend | All stories |

### Medium-Priority Risks (Score 3-4)

| Risk ID | Category | Description | Probability | Impact | Score | Mitigation | Owner |
| ------- | -------- | ----------- | ----------- | ------ | ----- | ---------- | ----- |
| R-005 | OPS | Audit log table unbounded growth (>10GB in 6 months impacts database performance) | 2 | 2 | 4 | Implement retention policy (archive logs >1 year), monitor table size, partition by month (PostgreSQL partitioning) | DBA + Backend |
| R-006 | PERF | CSV import memory overflow (1000+ row uploads rare but possible) | 1 | 3 | 3 | Stream CSV parsing with CsvHelper (no full load in memory), limit file size to 5MB (~10k rows), paginate import results | Backend |
| R-007 | DATA | Soft delete cascading issues (benchmark scores orphaned when model soft-deleted) | 1 | 3 | 3 | Test cascade behavior, ensure foreign keys handle soft-deleted parents, add database constraints to prevent orphans | Backend + DBA |
| R-008 | BUS | CSV import overwrites valid data (admin uploads wrong file) | 2 | 2 | 4 | Confirmation dialog before import ("This will add X scores"), dry-run preview mode (show what will be imported without committing), audit log tracks bulk imports for rollback | Frontend + Backend |

### Low-Priority Risks (Score 1-2)

| Risk ID | Category | Description | Probability | Impact | Score | Action |
| ------- | -------- | ----------- | ----------- | ------ | ----- | ------ |
| R-009 | OPS | EF Core optimistic concurrency failures (concurrent admin updates) | 1 | 1 | 1 | Implement retry logic with exponential backoff, return 409 Conflict to admin with "Refresh and try again" message |
| R-010 | BUS | Admin accidentally deletes active model (soft delete helps but data loss perceived) | 1 | 2 | 2 | Confirmation modal with model name verification, add "Undo delete" functionality (flip `is_active` back), audit log enables recovery |

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
| ----------- | ---------- | --------- | ---------- | ----- | ----- |
| Admin Login Authentication (AC 2.1.1-2.1.6) | E2E | R-001 | 3 | QA | Valid login → dashboard redirect, invalid credentials → error message, logout clears session |
| JWT Token Security Validation | API | R-001, R-004 | 5 | QA | Token validation, expiration handling, HttpOnly cookie set, unauthorized 401, tampering detection |
| Create Model with Validation (AC 2.4.1-2.4.6, 2.5.1-2.5.6) | API | R-002 | 4 | QA | Valid creation, pricing validation (positive, logical), duplicate detection warning, audit log entry |
| Update Model Pricing (AC 2.7.1-2.7.6) | API | R-002, R-003 | 4 | QA | Valid update, pricing constraints, cache invalidation triggered, audit log captures before/after |
| Cache Invalidation on Admin Mutations (Integration 1-3) | Integration | R-003 | 4 | QA | Create model → cache busted → public API reflects, update pricing → cache busted, benchmark score added → cache busted |
| Soft Delete Preserves Referential Integrity (AC 2.8.3-2.8.4) | Integration | R-007 | 2 | QA | Soft delete sets is_active=false, benchmark scores retained, public API excludes deleted model |

**Total P0**: 22 tests, 44 hours (2 hrs/test for E2E/API security)

**Execution Time**: <10 minutes (critical path validation)

### P1 (High) - Run on PR to main

**Criteria**: Important features + Medium risk (3-4) + Common workflows

| Requirement | Test Level | Risk Link | Test Count | Owner | Notes |
| ----------- | ---------- | --------- | ---------- | ----- | ----- |
| Models List View (AC 2.3.1-2.3.6) | Component | - | 6 | DEV | Table rendering, search filtering (debounced 300ms), column sorting, navigation to edit/delete |
| Model Form Validation (AC 2.4.2-2.4.3) | Component | R-002 | 8 | DEV | Client-side validation (React Hook Form + Zod), required fields, pricing positive numbers, logical constraints (pricingValidFrom < pricingValidTo) |
| Capabilities Section (AC 2.6.1-2.6.6) | API | - | 3 | QA | Capability creation linked to model, pre-population on edit, checkbox values persisted |
| Benchmark Score Entry (AC 2.10.1-2.10.6) | API | - | 4 | QA | Score validation (numeric, within typical range), warning if outside range, optional fields (testDate, sourceUrl, notes) |
| CSV Import Partial Success (AC 2.11.4-2.11.6) | API | R-006, R-008 | 5 | QA | Valid rows imported, invalid rows collected with error details, row-by-row validation (model exists, benchmark exists, score valid) |
| Timestamp Tracking (AC 2.12.1-2.12.4) | API | - | 3 | QA | created_at set on creation, updated_at refreshed on save, timestamps in UTC, public API includes pricingUpdatedAt |

**Total P1**: 29 tests, 29 hours (1 hr/test for API/component)

**Execution Time**: <30 minutes (important feature coverage)

### P2 (Medium) - Run nightly/weekly

**Criteria**: Secondary features + Low risk (1-2) + Edge cases

| Requirement | Test Level | Risk Link | Test Count | Owner | Notes |
| ----------- | ---------- | --------- | ---------- | ----- | ----- |
| AdminModelService Unit Tests | Unit | - | 15 | DEV | Create model with valid data, update model, validation errors thrown, domain events published (ModelCreatedEvent, ModelUpdatedEvent) |
| DataValidator Unit Tests | Unit | R-002 | 12 | DEV | Positive pricing validation, pricing constraints, duplicate detection logic, benchmark score range validation |
| AdminAuthService Unit Tests | Unit | R-001 | 8 | DEV | Valid login → token generated, invalid credentials → null, token validation, token expiration |
| Dashboard Metrics Computation (AC 2.12.3) | API | - | 3 | QA | Stale models count (>7 days old), incomplete benchmarks count (<3 benchmarks), data quality score calculation |
| Benchmark Management CRUD (AC 2.9.1-2.9.6) | API | - | 5 | QA | Create benchmark definition, unique constraint validation (benchmarkName), edit benchmark, delete benchmark (cascade warning) |
| Bulk Operations (Integration) | Integration | R-008 | 3 | QA | Bulk update models (status, currency), bulk cache invalidation, audit log entries for bulk operations |

**Total P2**: 46 tests, 23 hours (0.5 hr/test for unit tests, edge cases)

**Execution Time**: <60 minutes (full regression coverage)

### P3 (Low) - Run on-demand

**Criteria**: Nice-to-have + Exploratory + Performance benchmarks

| Requirement | Test Level | Test Count | Owner | Notes |
| ----------- | ---------- | ---------- | ----- | ----- |
| Admin Panel Layout Responsiveness (AC 2.2.5) | E2E | 2 | QA | Mobile (<768px), tablet (768-1024px), desktop (>1024px) layouts render correctly, sidebar collapsible |
| Audit Log Query and Filtering | API | 3 | QA | Filter by user, action, entity type, date range (from/to), pagination |
| CSV Export Failed Rows | Component | 2 | DEV | Download failed rows as CSV, format validation (CSV content-type) |
| Performance Benchmarks | E2E | 3 | QA | Dashboard load <2s, CRUD operations <500ms, bulk operations (10 models) <2s |

**Total P3**: 10 tests, 2.5 hours (0.25 hr/test for exploratory)

**Execution Time**: On-demand (performance validation, exploratory testing)

---

## Execution Order

### Smoke Tests (<5 min)

**Purpose**: Fast feedback, catch build-breaking issues

- [x] Admin login successful (valid credentials → dashboard) - 30s
- [x] Public API excludes soft-deleted models (integration check) - 45s
- [x] Admin create model → public API reflects immediately (cache invalidation) - 1min

**Total**: 3 scenarios, ~2 minutes

### P0 Tests (<10 min)

**Purpose**: Critical path validation (security + data integrity)

**E2E Tests:**
- [x] Admin login flow (valid credentials, invalid credentials, logout) - 2min

**API Tests:**
- [x] JWT token validation (valid, expired, tampered, unauthorized 401) - 2min
- [x] Create model with validation (valid, pricing constraints, duplicate detection) - 2min
- [x] Update model pricing (valid, pricing constraints, audit log) - 2min

**Integration Tests:**
- [x] Cache invalidation on admin create/update/delete - 2min
- [x] Soft delete referential integrity - 1min

**Total**: 22 tests, ~10 minutes

### P1 Tests (<30 min)

**Purpose**: Important feature coverage (common workflows)

**Component Tests (Vitest):**
- [x] Models list view (rendering, search, sorting, navigation) - 5min
- [x] Model form validation (React Hook Form + Zod, client-side validation) - 8min

**API Tests:**
- [x] Capabilities section CRUD - 3min
- [x] Benchmark score entry validation - 4min
- [x] CSV import partial success handling - 5min
- [x] Timestamp tracking (created_at, updated_at) - 3min

**Total**: 29 tests, ~28 minutes

### P2/P3 Tests (<60 min)

**Purpose**: Full regression coverage (edge cases, unit tests, performance)

**Unit Tests (xUnit):**
- [x] AdminModelService tests (create, update, validation, events) - 10min
- [x] DataValidator tests (pricing, duplicates, benchmark ranges) - 8min
- [x] AdminAuthService tests (token generation, validation, expiration) - 5min

**API Tests:**
- [x] Dashboard metrics computation - 3min
- [x] Benchmark management CRUD - 5min
- [x] Bulk operations - 3min

**E2E Tests:**
- [x] Admin panel layout responsiveness - 5min
- [x] Audit log query and filtering - 3min
- [x] CSV export failed rows - 2min
- [x] Performance benchmarks (dashboard, CRUD, bulk operations) - 8min

**Total**: 56 tests, ~52 minutes

---

## Resource Estimates

### Test Development Effort

| Priority | Count | Hours/Test | Total Hours | Notes |
| -------- | ----- | ---------- | ----------- | ----- |
| P0 | 22 | 2.0 | 44 | Complex setup (TestContainers, JWT auth), security focus |
| P1 | 29 | 1.0 | 29 | Standard coverage (API + component tests) |
| P2 | 46 | 0.5 | 23 | Simple scenarios (unit tests, edge cases) |
| P3 | 10 | 0.25 | 2.5 | Exploratory testing, performance validation |
| **Total** | **107** | **-** | **98.5** | **~13 days (1 QA + 1 Dev)** |

### Prerequisites

**Test Data:**
- `ModelFactory` (Faker-based, auto-generates valid models with pricing and capabilities, auto-cleanup via TestContainers)
- `BenchmarkFactory` (Pre-defined benchmark definitions: MMLU, HumanEval, GSM8K, auto-cleanup)
- `AdminAuthFixture` (Generates valid JWT tokens for test authentication, manages token lifecycle)
- `DatabaseFixture` (PostgreSQL + Redis TestContainers, schema migrations, seed data, disposal)

**Tooling:**
- **Backend**: xUnit (unit tests), WebApplicationFactory (integration tests), TestContainers (PostgreSQL 16 + Redis 7.2), FluentAssertions
- **Frontend**: Vitest (unit/component tests), Testing Library (React), MSW (API mocking)
- **E2E**: Playwright (cross-browser E2E tests), trace viewer, parallel execution
- **CI/CD**: GitHub Actions (automated test execution on PR, branch protection)

**Environment:**
- Local development: Docker Compose (PostgreSQL + Redis containers)
- CI/CD: TestContainers (ephemeral containers per test run)
- Test isolation: Each test uses fresh database via TestContainers transactions
- JWT secret: Test-specific secret key (not production secret)

---

## Quality Gate Criteria

### Pass/Fail Thresholds

- **P0 pass rate**: 100% (no exceptions, release blocker)
- **P1 pass rate**: ≥95% (waivers required for failures, documented in release notes)
- **P2/P3 pass rate**: ≥90% (informational, does not block release)
- **High-risk mitigations**: 100% complete or approved waivers by Tech Lead

### Coverage Targets

- **Critical paths**: ≥80% (admin login → CRUD → cache invalidation)
- **Security scenarios**: 100% (all SEC category risks, JWT validation, XSS prevention)
- **Business logic**: ≥70% (pricing validation, duplicate detection, audit logging)
- **Edge cases**: ≥50% (concurrent updates, soft delete edge cases)

### Non-Negotiable Requirements

- [ ] All P0 tests pass (22 tests, 100% pass rate)
- [ ] No high-risk (≥6) items unmitigated (R-001, R-002, R-003, R-004 verified)
- [ ] Security tests (SEC category) pass 100% (R-001, R-004 covered)
- [ ] Performance targets met (PERF category: dashboard <2s, CRUD <500ms)
- [ ] Cache invalidation verified (integration tests confirm public API freshness within 1s)
- [ ] Audit log entries validated for all CRUD operations (create, update, delete)

---

## Mitigation Plans

### R-001: JWT Token Security Compromise (Score: 6)

**Mitigation Strategy:**
1. Generate JWT secret key with cryptographically secure random (32+ characters)
2. Store secret in environment variables only (NEVER in code or config files)
3. Rotate secret regularly (every 90 days minimum)
4. Monitor for unauthorized 401 errors (spike indicates potential attack)
5. Add pre-commit hook to scan for hardcoded secrets in code
6. Use HttpOnly cookies for token storage (prevents XSS access)
7. Set SameSite=Strict cookie attribute (prevents CSRF attacks)

**Owner:** Backend Lead
**Timeline:** Story 2.1 implementation (before production deployment)
**Status:** Planned
**Verification:**
- Security test: Attempt token tampering → 401 Unauthorized
- Security test: Token expiration after 24 hours → 401 Unauthorized
- Code review: Verify no hardcoded credentials in codebase
- Manual test: Verify HttpOnly cookie set on login response

### R-002: Admin Enters Incorrect Pricing (Score: 6)

**Mitigation Strategy:**
1. Implement duplicate price check: Warn if price differs >50% from similar models (same provider, similar context window)
2. Audit log tracks all pricing changes with before/after values
3. Admin dashboard shows recent price changes for review (last 7 days)
4. Client-side validation: Pricing must be positive numbers, logical constraints (inputPrice < outputPrice typically)
5. Server-side validation: FluentValidation rules enforce constraints
6. Confirmation dialog for large price changes (>100% increase/decrease)

**Owner:** Backend Lead + QA Lead
**Timeline:** Story 2.5 (validation), Story 2.7 (audit logging)
**Status:** In Progress (validation implemented, audit logging pending)
**Verification:**
- Integration test: Create model with negative price → 400 Bad Request
- Integration test: Update pricing → audit log entry with before/after JSON
- Manual test: Admin dashboard displays recent price changes

### R-003: Cache Invalidation Race Condition (Score: 6)

**Mitigation Strategy:**
1. Domain events published synchronously before API response (ensures event processed before client receives success)
2. Cache bust operation completes <100ms (asynchronous but fast)
3. Acceptable 1s eventual consistency window (users may see stale data briefly)
4. Integration tests verify cache invalidation (admin update → verify public API reflects changes)
5. Redis monitoring: Alert if cache bust operations >200ms (performance degradation)

**Owner:** Backend Lead
**Timeline:** Stories 2.5-2.7 (all admin mutation endpoints)
**Status:** Planned
**Verification:**
- Integration test: Admin creates model → GET /api/models reflects new model within 1s
- Integration test: Admin updates pricing → GET /api/models/{id} reflects new price within 1s
- Integration test: Admin adds benchmark score → cache invalidated, public API updated

### R-004: XSS via Admin Input Fields (Score: 6)

**Mitigation Strategy:**
1. React escapes all user input by default (no additional work needed)
2. No use of `dangerouslySetInnerHTML` in admin panel (code review enforced)
3. Content-Security-Policy headers configured in production (blocks inline scripts)
4. Input sanitization validation: Backend validates all inputs (FluentValidation)
5. SQL injection prevented via EF Core parameterized queries (no raw SQL)

**Owner:** Frontend Lead + Backend Lead
**Timeline:** All stories (continuous enforcement)
**Status:** In Progress (React escaping default, CSP headers pending)
**Verification:**
- Security test: Submit XSS payload in model name → escaped in HTML
- Code review: Verify no `dangerouslySetInnerHTML` usage in admin panel
- Manual test: CSP headers present in production response

---

## Assumptions and Dependencies

### Assumptions

1. **Admin users have technical proficiency** to understand pricing structures (input tokens, output tokens, per-1M pricing)
   - **Validation**: User acceptance testing with actual admins during Story 2.4
   - **Risk if wrong**: Admin confusion → data entry errors → pricing inaccuracies

2. **Redis cache invalidation completes <1 second** (synchronous domain event handling acceptable)
   - **Validation**: Performance test cache bust with 10-20 keys under load
   - **Risk if wrong**: Admin panel feels slow → poor UX → admin frustration

3. **CSV import <1000 rows typical use case** (bulk onboarding 10-50 models with 10 benchmarks each)
   - **Validation**: Interview admins on expected import volumes
   - **Risk if wrong**: Memory overflow, timeouts → failed imports

4. **Single admin concurrent users acceptable** (1-3 admins, rarely editing same model)
   - **Validation**: Monitor admin activity logs post-launch
   - **Risk if wrong**: Optimistic concurrency conflicts → frustration → need pessimistic locking

5. **Hardcoded admin credentials acceptable for MVP** (1-2 admins, short-term 3-6 months)
   - **Validation**: Security review confirms environment variable storage sufficient
   - **Risk if wrong**: Credential leak → unauthorized access → data manipulation

6. **Soft delete sufficient for audit trail** (no hard delete needed in MVP)
   - **Validation**: Legal/compliance review confirms soft delete meets retention policy
   - **Risk if wrong**: Regulatory requirements mandate hard delete → schema redesign

### Dependencies

1. **PostgreSQL 16 with JSONB support** (admin_audit_log.changes_json column) - Required by Story 2.7
2. **Redis 7.2 for pattern-based cache deletion** (`cache:models:*`) - Required by all mutation stories
3. **TestContainers support in CI/CD** (ephemeral PostgreSQL + Redis) - Required by integration tests
4. **React Hook Form + Zod compatibility** (client-side validation) - Required by Story 2.4
5. **Epic 1 completion** (database schema, repository pattern, API structure, CI/CD pipeline) - Required before Epic 2 begins

### Risks to Plan

- **Risk**: TestContainers initialization time in CI/CD (>2 min container startup)
  - **Impact**: Slow CI/CD pipeline, developer feedback loop >5 min
  - **Contingency**: Use persistent test database in CI/CD (trade-off: test isolation vs speed), cache Docker images

- **Risk**: Admin availability for UAT during Sprint 2 (admin schedule conflicts)
  - **Impact**: Delayed acceptance criteria validation, potential rework
  - **Contingency**: Record demo videos for async review, schedule UAT before sprint ends

- **Risk**: Security review bottleneck (external security team availability)
  - **Impact**: Delayed production deployment, blocked release
  - **Contingency**: Schedule security review early (Sprint 1 end), use automated security scanning (OWASP ZAP, Snyk)

---

## Approval

**Test Design Approved By:**

- [ ] Product Manager: **_____** Date: **_____**
- [ ] Tech Lead: **_____** Date: **_____**
- [ ] QA Lead: **_____** Date: **_____**

**Comments:**

---

## Appendix

### Knowledge Base References

- `risk-governance.md` - Risk classification framework (6 categories: TECH, SEC, PERF, DATA, BUS, OPS), automated scoring, gate decision engine
- `probability-impact.md` - Probability × impact matrix, automated classification thresholds (≥6 high-priority), dynamic re-assessment
- `test-levels-framework.md` - E2E vs API vs Component vs Unit decision framework with characteristics matrix, when to use each
- `test-priorities-matrix.md` - P0-P3 automated priority calculation, risk-based mapping, tagging strategy, time budgets

### Related Documents

- **PRD**: `/home/pablitxn/repos/bmad-method/llm-token-price/docs/PRD.md`
- **Epic 2 Tech Spec**: `/home/pablitxn/repos/bmad-method/llm-token-price/docs/epics/epic_2/tech-spec-epic-2.md`
- **Architecture**: `/home/pablitxn/repos/bmad-method/llm-token-price/docs/solution-architecture.md`
- **Epic 2 Stories**: `/home/pablitxn/repos/bmad-method/llm-token-price/docs/stories/epic_2/` (Stories 2.1-2.12)

### Test Strategy Highlights

**Testing Pyramid Distribution:**
```
           E2E (5%)
         ┌──────────┐
        │ 5 tests   │ - Critical admin workflows
       └────────────┘
          ↓
    Integration (25%)
   ┌──────────────────┐
  │ 27 tests          │ - API + Cache invalidation
 └──────────────────────┘
    ↓
  Unit (70%)
 ┌────────────────────┐
│ 75 tests            │ - Domain + Application layer
└────────────────────┘
```

**Coverage Targets Achieved:**
- Domain/Application layer: 90%+ (AdminModelService, DataValidator, AdminAuthService)
- Integration layer: 80%+ (API endpoints, cache invalidation, database persistence)
- E2E layer: Critical paths only (admin login → CRUD workflow)

**CI/CD Integration:**
- P0 tests run on every commit (GitHub Actions, <10 min execution)
- P1 tests run on PR to main (branch protection, <30 min execution)
- P2/P3 tests run nightly (full regression, <60 min execution)
- Test failures block merge to main (P0 + P1 must pass 100%)

---

**Generated by**: BMad TEA Agent - Test Architect Module
**Workflow**: `bmad/bmm/testarch/test-design`
**Version**: 4.0 (BMad v6)
**Date**: 2025-10-17
