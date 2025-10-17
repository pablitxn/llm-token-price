# 📋 Retrospective Action Items - Epic 1

**Generated:** 2025-10-17
**Source:** Epic 1 Retrospective
**Status:** Active

---

## 🔴 CRITICAL PRIORITY (Blocks Epic 2)

These items MUST be completed before Epic 2 Story 2.1 can begin.

### 1. Design Cache Invalidation Strategy
**Owner:** Winston (Architect)
**Priority:** 🔴 HIGH
**Estimate:** 2 hours
**Due By:** Before Story 2.5 (Backend API for Adding Models)
**Status:** ⬜ Not Started

**Description:**
Design comprehensive cache invalidation strategy for Redis layer. Admin mutations (create/update/delete models) will invalidate stale cached data.

**Deliverables:**
- [ ] Document cache key patterns (e.g., `cache:models:*`, `cache:models:{id}:*`, `cache:bestvalue:*`)
- [ ] Design CacheInvalidationService interface in Domain layer
- [ ] Define invalidation triggers (which operations invalidate which keys)
- [ ] Specify TTL strategy per cache type (API responses: 1hr, model details: 30min)
- [ ] Document graceful degradation behavior when cache unavailable

**Why Critical:**
Without this, admin updates will serve stale data to users. Story 2.5 creates first admin mutation endpoint.

**Files to Create:**
- `docs/cache-invalidation-strategy.md`

---

### 2. Create ADR-011: Authentication Approach
**Owner:** Winston (Architect)
**Priority:** 🔴 HIGH
**Estimate:** 2 hours
**Due By:** Before Story 2.1 (Admin Panel Authentication)
**Status:** ⬜ Not Started

**Description:**
Document authentication architecture decision for admin panel. Story 2.1 context is ready but needs ADR foundation.

**Deliverables:**
- [ ] Document decision: JWT + HttpOnly cookies vs Session vs OAuth
- [ ] Justify JWT choice for MVP (stateless, scalable, standard)
- [ ] Define token structure (claims: user_id, role, exp)
- [ ] Define token lifetime (access: 1hr, refresh: 7 days)
- [ ] Document logout flow (clear HttpOnly cookie)
- [ ] Document CORS configuration (`credentials: true` required)
- [ ] Address security concerns (CSRF, XSS, token storage)

**Why Critical:**
Story 2.1 implementation requires clear authentication design. Current context assumes JWT but lacks formal ADR.

**Files to Update:**
- `docs/architecture-decisions.md` (add ADR-011 section)

---

### 3. Automate Story Context XML Generation
**Owner:** Bob (Scrum Master)
**Priority:** 🔴 HIGH
**Estimate:** 1 hour
**Due By:** Before Epic 2 Story 2.1
**Status:** ⬜ Not Started

**Description:**
Modify `story-ready` workflow to automatically generate Story Context XML after approval. Prevents M1/L1 code review findings.

**Deliverables:**
- [ ] Update `bmad/bmm/workflows/4-implementation/story-ready/instructions.md`
- [ ] Add automatic `story-context` workflow invocation after approval
- [ ] Test with Epic 2 Story 2.1 (verify context auto-generates)
- [ ] Document change in workflow status

**Why Critical:**
Every Epic 1 code review flagged missing Story Context XML as M1 or L1 issue. Automation eliminates this recurring problem.

**Files to Update:**
- `bmad/bmm/workflows/4-implementation/story-ready/instructions.md`

---

## 🟡 IMPORTANT PRIORITY (Preparation Sprint)

Complete these during 3-4 hour preparation sprint before Epic 2.

### 4. Create ATDD Checklist Template
**Owner:** Murat (Test Architect)
**Priority:** 🟡 MEDIUM
**Estimate:** 2 hours
**Phase:** Preparation Sprint
**Status:** ⬜ Not Started

**Description:**
Create reusable ATDD checklist template based on Story 1.11 success. Enforce test-first discipline for all stories.

**Deliverables:**
- [ ] Create `docs/templates/atdd-checklist-template.md`
- [ ] Include sections: Story Context, Failing Tests (RED), Passing Tests (GREEN), Refactoring (REFACTOR)
- [ ] Add placeholders for acceptance criteria mapping
- [ ] Document usage instructions
- [ ] Generate ATDD checklist for Story 2.1 as example

**Why Important:**
Story 1.11 ATDD checklist enforced test-first development. Template makes this standard for all stories.

**Files to Create:**
- `docs/templates/atdd-checklist-template.md`

---

### 5. Install Epic 2 Libraries
**Owner:** Amelia (Developer)
**Priority:** 🟡 MEDIUM
**Estimate:** 1 hour
**Phase:** Preparation Sprint
**Status:** ⬜ Not Started

**Description:**
Install all required libraries for Epic 2 stories before starting development.

**Backend Libraries:**
```bash
cd services/backend
dotnet add LlmTokenPrice.Application package FluentValidation.AspNetCore --version 11.3.0
dotnet add LlmTokenPrice.API package System.IdentityModel.Tokens.Jwt --version 7.3.1
dotnet add LlmTokenPrice.Infrastructure package BCrypt.Net-Next --version 4.0.3
dotnet add LlmTokenPrice.Infrastructure package CsvHelper --version 30.0.1
```

**Frontend Libraries:**
```bash
cd apps/web
pnpm add react-hook-form zod @hookform/resolvers
```

**Deliverables:**
- [ ] Install backend libraries and verify build
- [ ] Install frontend libraries and verify build
- [ ] Update README.md with new dependencies
- [ ] Test JWT token generation (basic smoke test)
- [ ] Test React Hook Form with Zod validation (basic example)

---

### 6. Implement CacheInvalidationService
**Owner:** Amelia (Developer)
**Priority:** 🟡 MEDIUM
**Estimate:** 3 hours
**Phase:** Story 2.5
**Status:** ⬜ Not Started

**Description:**
Implement cache invalidation service following Winston's design (Item #1). Required before Story 2.5 admin mutations.

**Deliverables:**
- [ ] Create `ICacheInvalidationService` interface in Domain layer
- [ ] Implement `CacheInvalidationService` in Infrastructure layer
- [ ] Methods: `InvalidateModelsCache()`, `InvalidateModelById(Guid id)`, `InvalidateBestValueCache()`
- [ ] Use Redis pattern matching (`KEYS cache:models:*`) with caution (blocking operation)
- [ ] Register service in DI container
- [ ] Add unit tests for invalidation logic
- [ ] Document usage in README

**Dependencies:**
- Requires Item #1 (Cache Invalidation Strategy) complete

**Files to Create:**
- `services/backend/LlmTokenPrice.Domain/Services/ICacheInvalidationService.cs`
- `services/backend/LlmTokenPrice.Infrastructure/Services/CacheInvalidationService.cs`

---

### 7. Create admin_audit_log Database Migration
**Owner:** Amelia (Developer)
**Priority:** 🟡 MEDIUM
**Estimate:** 1 hour
**Phase:** Story 2.9 (Add Audit Log)
**Status:** ⬜ Not Started

**Description:**
Create database migration for admin audit log table. Tracks all admin CRUD operations for compliance.

**Schema:**
```sql
CREATE TABLE admin_audit_log (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    admin_user VARCHAR(255) NOT NULL,
    action VARCHAR(50) NOT NULL, -- 'CREATE', 'UPDATE', 'DELETE'
    entity_type VARCHAR(100) NOT NULL, -- 'Model', 'Benchmark', etc.
    entity_id UUID NOT NULL,
    changes_json JSONB, -- Before/after values
    ip_address VARCHAR(45),
    user_agent VARCHAR(500),
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

CREATE INDEX idx_audit_log_admin_user ON admin_audit_log(admin_user);
CREATE INDEX idx_audit_log_entity ON admin_audit_log(entity_type, entity_id);
CREATE INDEX idx_audit_log_created_at ON admin_audit_log(created_at DESC);
```

**Deliverables:**
- [ ] Create `AdminAuditLog` entity in Domain layer
- [ ] Create EF migration: `dotnet ef migrations add AddAdminAuditLog`
- [ ] Apply migration: `dotnet ef database update`
- [ ] Verify schema in PostgreSQL
- [ ] Document audit log format in README

**Files to Create:**
- `services/backend/LlmTokenPrice.Domain/Entities/AdminAuditLog.cs`
- `services/backend/LlmTokenPrice.Infrastructure/Migrations/{timestamp}_AddAdminAuditLog.cs`

---

### 8. Wireframe Admin Panel Layout
**Owner:** UX/Sally
**Priority:** 🟡 MEDIUM
**Estimate:** 3 hours
**Phase:** Preparation Sprint
**Status:** ⬜ Not Started

**Description:**
Create low-fidelity wireframes for admin panel layout. Guides Story 2.2 (Admin Dashboard Layout) implementation.

**Deliverables:**
- [ ] Wireframe: Admin login page (Story 2.1)
- [ ] Wireframe: Admin dashboard layout with sidebar nav (Story 2.2)
- [ ] Wireframe: Models list table (Story 2.3)
- [ ] Wireframe: Add/Edit model form (Stories 2.4, 2.6, 2.7)
- [ ] Wireframe: Benchmarks management (Story 2.9)
- [ ] Document navigation flow
- [ ] Export wireframes to `docs/wireframes/` directory

**Why Important:**
Clear wireframes prevent design churn during implementation. Story 2.2 needs layout structure before coding.

**Files to Create:**
- `docs/wireframes/admin-login.png`
- `docs/wireframes/admin-dashboard.png`
- `docs/wireframes/admin-models-list.png`
- `docs/wireframes/admin-model-form.png`

---

## 🟢 LOW PRIORITY (Technical Debt - Backlog)

Deferrable items that improve quality but don't block progress.

### 9. Complete Story 1.1 Documentation
**Owner:** Amelia (Developer)
**Priority:** 🟢 LOW
**Estimate:** 1 hour
**Epic:** Backlog
**Status:** ⬜ Not Started

**Description:**
Address code review findings from Story 1.1. Missing CONTRIBUTING.md and LICENSE files.

**Deliverables:**
- [ ] Create `CONTRIBUTING.md` with:
  - Development workflow (branch strategy, commit conventions)
  - Story workflow (create-story → story-ready → dev-story → review-story → story-approved)
  - Testing standards (TDD, ATDD checklists)
  - Code review process
  - Pull request template
- [ ] Create `LICENSE` file (specify license type with Pablo)
- [ ] Update README.md to reference CONTRIBUTING.md

**Files to Create:**
- `CONTRIBUTING.md`
- `LICENSE`

---

### 10. Add Environment Variable Support
**Owner:** Amelia (Developer)
**Priority:** 🟢 MEDIUM
**Estimate:** 2 hours
**Epic:** Epic 2 or 3
**Status:** ⬜ Not Started

**Description:**
Replace hardcoded development credentials with environment variables. Security hardening for production.

**Deliverables:**
- [ ] Create `.env.example` with template
- [ ] Update `appsettings.Development.json` to use env vars: `${DB_PASSWORD}`, `${REDIS_PASSWORD}`
- [ ] Update `docker-compose.yml` to load from `.env` file
- [ ] Document environment variable setup in README
- [ ] Add `.env` to `.gitignore` (verify not committed)
- [ ] Test with environment variables set

**Why Medium Priority:**
Development credentials acceptable for Epic 1, but production deployment requires env vars.

**Files to Create:**
- `.env.example`

**Files to Update:**
- `services/backend/LlmTokenPrice.API/appsettings.Development.json`
- `docker-compose.yml`
- `README.md`

---

### 11. Implement Observability Stack
**Owner:** Winston (Architect)
**Priority:** 🟢 LOW
**Estimate:** 8 hours
**Epic:** Post-MVP
**Status:** ⬜ Not Started

**Description:**
Implement comprehensive observability stack for production monitoring. Deferred to Phase 2 (post-MVP).

**Deliverables:**
- [ ] Configure Application Insights or Prometheus + Grafana
- [ ] Add custom metrics (request latency, error rates, cache hit ratio)
- [ ] Configure structured logging exports
- [ ] Create monitoring dashboards
- [ ] Set up alerting rules (error rate thresholds, latency SLAs)
- [ ] Document monitoring setup in README

**Why Low Priority:**
Structured logging (Serilog) already configured. Full observability stack not needed for MVP development.

**Deferred Until:** After MVP launch, Phase 2

---

## 📊 ACTION ITEMS SUMMARY

### By Priority
- 🔴 **Critical:** 3 items (6 hours) - **MUST complete before Epic 2**
- 🟡 **Important:** 5 items (12 hours) - Complete during prep sprint
- 🟢 **Low Priority:** 3 items (11 hours) - Backlog

### By Owner
- **Winston (Architect):** 3 items (12 hours)
- **Amelia (Developer):** 5 items (8 hours)
- **Bob (Scrum Master):** 1 item (1 hour)
- **Murat (Test Architect):** 1 item (2 hours)
- **UX/Sally:** 1 item (3 hours)

### By Phase
- **Before Epic 2:** 3 items (critical path)
- **Preparation Sprint:** 5 items (prep work)
- **During Epic 2:** 2 items (Story 2.5, 2.9)
- **Backlog:** 3 items (deferred)

---

## ✅ COMPLETION CHECKLIST

### Critical Path (Must Complete First)
- [ ] Item #1: Cache Invalidation Strategy (Winston, 2h)
- [ ] Item #2: ADR-011 Authentication (Winston, 2h)
- [ ] Item #3: Story Context XML Automation (Bob, 1h)

### Preparation Sprint (Before Starting Epic 2)
- [ ] Item #4: ATDD Checklist Template (Murat, 2h)
- [ ] Item #5: Install Epic 2 Libraries (Amelia, 1h)
- [ ] Item #8: Wireframe Admin Panel (Sally, 3h)

### During Epic 2
- [ ] Item #6: CacheInvalidationService (Amelia, 3h) - Story 2.5
- [ ] Item #7: admin_audit_log Migration (Amelia, 1h) - Story 2.9

### Backlog (Defer)
- [ ] Item #9: Story 1.1 Docs (Amelia, 1h)
- [ ] Item #10: Environment Variables (Amelia, 2h)
- [ ] Item #11: Observability Stack (Winston, 8h)

---

## 📝 NEXT STEPS

1. **Review with team** - Ensure all action items have clear owners and estimates
2. **Schedule prep sprint** - Block 3-4 hours for critical path + preparation items
3. **Track progress** - Update this document as items complete
4. **Epic 2 readiness check** - Verify all critical items done before Story 2.1

---

**Document Status:** ✅ Active
**Last Updated:** 2025-10-17
**Next Review:** After Epic 2 completion

---

*Generated from Epic 1 Retrospective (docs/retrospectives/epic-1-retro-2025-10-17.md)*
