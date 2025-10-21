# CI/CD Configuration

This document explains the Continuous Integration and Continuous Deployment (CI/CD) setup for the LLM Token Price Comparison Platform.

## Overview

The project uses **GitHub Actions** for automated testing, quality enforcement, and deployment pipelines. All workflows are located in `.github/workflows/`.

## Workflows

### 1. Backend CI (`backend-ci.yml`)

**Triggers:**
- Push to `main` branch (when backend files change)
- Pull requests to `main` (when backend files change)

**Infrastructure:**
- PostgreSQL 16 (TimescaleDB) - Port 5432
- Redis 7 - Port 6379
- Ubuntu latest runner

**Pipeline Steps:**

1. **Checkout & Setup**
   - Checkout code
   - Setup .NET 9.0
   - Restore dependencies

2. **Build**
   - Build in Release configuration
   - Ensure 0 errors, 0 warnings

3. **Test Execution**
   - Run all test suites:
     - `LlmTokenPrice.Domain.Tests` (43 tests)
     - `LlmTokenPrice.Application.Tests` (135 tests)
     - `LlmTokenPrice.Infrastructure.Tests` (25 tests)
     - `LlmTokenPrice.Tests.E2E` (51 tests)
   - Collect code coverage (XPlat Code Coverage)
   - Generate TRX test results

4. **Test Result Parsing**
   - Parse TRX files to extract metrics
   - Calculate pass rate
   - Output: total, passed, failed, skipped, pass_rate

5. **PR Reporting** (PRs only)
   - Post/update comment on PR with test results
   - Show metrics table with pass rate
   - Link to coverage reports
   - Visual indicators (✅/❌)

6. **Quality Gate Enforcement**
   - **Required pass rate: 95.0%**
   - Fail job if pass rate < 95.0%
   - Fail job if any tests failed (even if skipped count high)
   - Block PR merge if enforcement fails

7. **Coverage Upload**
   - Upload coverage to Codecov
   - Flag: `backend`
   - Always run (even on test failure)

**Quality Gates:**
```
✅ Pass Rate >= 95.0%
✅ 0 Failed tests
✅ Build succeeds
✅ 0 Compiler warnings
```

**Example PR Comment:**
```markdown
### ✅ Backend Test Results

| Metric | Value |
|--------|-------|
| **Total Tests** | 254 |
| **Passed** | 243 ✅ |
| **Failed** | 0 ❌ |
| **Skipped** | 11 ⏭️ |
| **Pass Rate** | **95.7%** |
| **Required** | 95.0% |

✅ **All tests passed!** Test suite is production-ready.

<details>
<summary>Coverage Report</summary>

Coverage reports will be available on Codecov
</details>

---
*Workflow run: #123*
```

### 2. Frontend CI (`frontend-ci.yml`)

**Triggers:**
- Push to `main` branch (when frontend files change)
- Pull requests to `main` (when frontend files change)

**Infrastructure:**
- Node.js 20
- pnpm 10
- Ubuntu latest runner

**Pipeline Steps:**

1. **Checkout & Setup**
   - Checkout code
   - Setup Node.js 20
   - Setup pnpm 10
   - Cache pnpm store and node_modules

2. **Install Dependencies**
   - `pnpm install --frozen-lockfile`
   - Use cached dependencies when possible

3. **Type Check**
   - `pnpm run type-check`
   - Enforce TypeScript strict mode
   - Fail on any type errors

4. **Linting**
   - `pnpm run lint`
   - Enforce code style consistency
   - Fail on linting errors

5. **Test Execution** (when tests exist)
   - Check if test script exists in package.json
   - Run `pnpm run test:run` or `pnpm test`
   - Currently skipped (tests added in Epic 3+)

6. **Build**
   - `pnpm run build`
   - Vite production build
   - Output to `dist/` folder

7. **Bundle Size Check**
   - Report final bundle size
   - Verify dist folder exists
   - Target: < 500KB gzipped

**Quality Gates:**
```
✅ 0 Type errors
✅ 0 Linting errors
✅ Build succeeds
✅ Bundle size reasonable
```

## Environment Variables

### Backend CI

Required for test execution:

```yaml
ConnectionStrings__DefaultConnection: "Host=localhost;Port=5432;Database=llmpricing_test;Username=postgres;Password=test"
ConnectionStrings__RedisConnection: "localhost:6379"
```

Optional (Codecov):

```yaml
CODECOV_TOKEN: ${{ secrets.CODECOV_TOKEN }}
```

### Frontend CI

No environment variables required for current pipeline.

## Local Testing

### Backend

To run tests locally matching CI environment:

```bash
cd services/backend

# Start dependencies
docker-compose up -d

# Run tests with coverage
dotnet test --configuration Release \
  --collect:"XPlat Code Coverage" \
  --results-directory ./coverage \
  --logger "trx;LogFileName=test-results.trx"

# Check pass rate
dotnet test --no-build --configuration Release
```

### Frontend

To run checks locally matching CI environment:

```bash
cd apps/web

# Install dependencies
pnpm install --frozen-lockfile

# Run type check
pnpm run type-check

# Run linter
pnpm run lint

# Run build
pnpm run build

# Check bundle size
du -sh dist
```

## Coverage Reports

Code coverage is tracked using **Codecov**:

- **Backend:** XPlat Code Coverage (Cobertura format)
- **Target:** 70%+ overall, 90%+ domain layer
- **Reports:** Available on Codecov dashboard

### Viewing Coverage

1. Visit: `https://codecov.io/gh/OWNER/REPO`
2. Select branch/PR
3. View detailed coverage by file/folder

## Troubleshooting

### Backend Tests Failing in CI

1. **Check PostgreSQL connection:**
   - Ensure service is healthy (health checks pass)
   - Verify connection string matches service config

2. **Check Redis connection:**
   - Ensure service is healthy
   - Verify port 6379 is accessible

3. **Check test data:**
   - Some E2E tests require test data fixtures
   - Skipped tests are expected (CSV imports, auth legacy)

### Frontend Build Failing in CI

1. **Type errors:**
   - Run `pnpm run type-check` locally
   - Fix all type errors before pushing

2. **Linting errors:**
   - Run `pnpm run lint` locally
   - Use `pnpm run lint --fix` to auto-fix

3. **Build errors:**
   - Delete `node_modules` and `dist`
   - Run `pnpm install --frozen-lockfile`
   - Run `pnpm run build`

## Future Enhancements

### Planned (Story 2.13)

- ✅ Test result parsing and reporting
- ✅ Pass rate enforcement (95%)
- ✅ PR comments with test results
- ⏳ Code coverage thresholds enforcement
- ⏳ Performance regression testing

### Future Epics

- **Epic 3:** Frontend test coverage
- **Epic 4:** E2E tests with Playwright
- **Epic 5:** Performance monitoring
- **Epic 6:** Deployment automation
- **Epic 7:** Load testing in CI

## Maintenance

### Updating Workflows

When modifying workflows:

1. Edit `.github/workflows/*.yml`
2. Test locally if possible (act, nektos/act)
3. Create PR to test in real CI environment
4. Verify PR checks pass
5. Document changes in this file

### Adding New Tests

When adding tests:

1. Follow existing test structure
2. Ensure tests run in CI environment
3. Update pass rate calculations if needed
4. Document any new test data requirements

---

**Last Updated:** 2025-10-21
**Story:** Story 2.13 - Technical Debt Resolution
**Task:** Task 2 - CI/CD Test Enforcement
