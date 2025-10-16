# Story 1.8: Configure CI/CD Pipeline

Status: Ready

## Story

As a developer,
I want automated CI/CD pipelines for backend and frontend,
So that code quality is verified on every commit and pull request.

## Acceptance Criteria

1. Backend CI pipeline created running on push and pull_request events with dotnet test execution
2. Frontend CI pipeline created with npm ci, type-check, lint, and build steps
3. Both pipelines use GitHub Actions with appropriate service containers (PostgreSQL, Redis for backend)
4. Pipelines pass successfully on main branch with current codebase
5. Build status badges added to README.md showing pipeline health
6. Pipeline failures block pull request merging (branch protection configured)

## Tasks / Subtasks

- [ ] Create backend CI/CD pipeline (AC: 1, 3)
  - [ ] Create `.github/workflows/backend-ci.yml` file in project root
  - [ ] Configure trigger: `on: [push, pull_request]`
  - [ ] Set up job running on `ubuntu-latest`
  - [ ] Add PostgreSQL service container: `timescale/timescaledb:2.13.0-pg16`, environment POSTGRES_PASSWORD=test, health check `pg_isready`
  - [ ] Add Redis service container: `redis:7-alpine`
  - [ ] Add steps: checkout code (`actions/checkout@v4`), setup .NET (`actions/setup-dotnet@v4` with version 8.0.x)
  - [ ] Run dotnet restore: `dotnet restore` in backend directory
  - [ ] Run dotnet build: `dotnet build --no-restore --configuration Release`
  - [ ] Run dotnet test: `dotnet test --no-build --verbosity normal --configuration Release`
  - [ ] Test pipeline: push to branch, verify pipeline runs successfully in GitHub Actions tab

- [ ] Create frontend CI/CD pipeline (AC: 2, 3)
  - [ ] Create `.github/workflows/frontend-ci.yml` file
  - [ ] Configure trigger: `on: [push, pull_request]`
  - [ ] Set up job running on `ubuntu-latest`
  - [ ] Add steps: checkout code, setup Node.js (`actions/setup-node@v4` with version 20)
  - [ ] Install dependencies: `npm ci` in frontend directory (faster than npm install)
  - [ ] Run type check: `npm run type-check` (executes `tsc --noEmit`)
  - [ ] Run linter: `npm run lint` (configure ESLint script if not exists)
  - [ ] Run build: `npm run build` to verify production build succeeds
  - [ ] Test pipeline: verify all steps pass in GitHub Actions

- [ ] Add unit test projects and basic tests (AC: 1)
  - [ ] Create `Backend.Domain.Tests` project: `dotnet new xunit -n Backend.Domain.Tests -f net8.0`
  - [ ] Add to solution: `dotnet sln add Backend.Domain.Tests/Backend.Domain.Tests.csproj`
  - [ ] Add project reference to Domain: `dotnet add Backend.Domain.Tests reference Backend.Domain`
  - [ ] Install testing packages: `dotnet add Backend.Domain.Tests package FluentAssertions`
  - [ ] Create placeholder test: `ModelTests.cs` with basic entity creation test
  - [ ] Verify tests run locally: `dotnet test` passes with at least 1 test

- [ ] Configure ESLint for frontend (AC: 2)
  - [ ] Install ESLint: `npm install -D eslint @typescript-eslint/parser @typescript-eslint/eslint-plugin eslint-plugin-react-hooks`
  - [ ] Create `.eslintrc.json` with TypeScript and React rules
  - [ ] Add lint script to package.json: `"lint": "eslint src --ext .ts,.tsx"`
  - [ ] Fix any existing lint errors in placeholder components
  - [ ] Add lint-fix script: `"lint:fix": "eslint src --ext .ts,.tsx --fix"`
  - [ ] Verify lint passes: `npm run lint` exits with code 0

- [ ] Add build status badges to README (AC: 5)
  - [ ] Get GitHub Actions badge URLs: `https://github.com/{user}/{repo}/actions/workflows/backend-ci.yml/badge.svg`
  - [ ] Add badges to README.md header: `![Backend CI](badge-url)` for both backend and frontend pipelines
  - [ ] Verify badges show correct status: click badges, ensure they link to GitHub Actions

- [ ] Configure branch protection rules (AC: 6)
  - [ ] Navigate to GitHub repository Settings → Branches
  - [ ] Add branch protection rule for `main` branch
  - [ ] Enable "Require status checks to pass before merging"
  - [ ] Select required checks: `Backend CI / test`, `Frontend CI / test`
  - [ ] Enable "Require branches to be up to date before merging"
  - [ ] Test protection: create test PR, verify cannot merge until checks pass

- [ ] Document CI/CD pipeline and verify all components (AC: 1-6)
  - [ ] Update README.md with "CI/CD" section explaining pipeline workflows
  - [ ] Document how to run tests locally: backend (`dotnet test`), frontend (`npm run lint && npm run type-check && npm run build`)
  - [ ] Document pipeline triggers: runs on every push and PR
  - [ ] Create troubleshooting section: pipeline failures, service container issues, dependency installation errors
  - [ ] Verify all acceptance criteria: both pipelines exist, pass on main, badges visible, branch protection active

## Dev Notes

### Architecture Constraints

**From solution-architecture.md Section 1.1 - Technology Stack:**
- **Testing (Unit):** xUnit 2.6.0 for backend unit testing
- **Testing (E2E):** Playwright 1.40.0 (Phase 2, not in Epic 1)
- **CI/CD:** GitHub Actions for automated build/test

**From tech-spec-epic-1.md Story 1.8:**
- **Backend CI:** PostgreSQL and Redis service containers required for integration tests
- **Frontend CI:** Node 20, npm ci (not install), type-check + lint + build steps
- **Service health checks:** PostgreSQL needs `pg_isready` health check

### Project Structure Notes

**CI/CD files:**
```
/.github/
└── workflows/
    ├── backend-ci.yml (backend build, test)
    └── frontend-ci.yml (frontend lint, type-check, build)
```

**Test project structure:**
```
/backend/
├── Backend.Domain.Tests/
│   ├── Backend.Domain.Tests.csproj
│   └── ModelTests.cs (placeholder test)
└── Backend.sln (updated to include test project)
```

**backend-ci.yml structure:**
```yaml
name: Backend CI
on: [push, pull_request]
jobs:
  test:
    runs-on: ubuntu-latest
    services:
      postgres:
        image: timescale/timescaledb:2.13.0-pg16
        env:
          POSTGRES_PASSWORD: test
          POSTGRES_DB: llmpricing_test
        ports:
          - 5432:5432
        options: >-
          --health-cmd pg_isready
          --health-interval 10s
          --health-timeout 5s
          --health-retries 5
      redis:
        image: redis:7-alpine
        ports:
          - 6379:6379
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.0.x'
      - run: dotnet restore
        working-directory: ./backend
      - run: dotnet build --no-restore --configuration Release
        working-directory: ./backend
      - run: dotnet test --no-build --verbosity normal --configuration Release
        working-directory: ./backend
```

### Testing Standards Summary

**Backend CI validation:**
1. Pipeline triggers on push/PR
2. Service containers start successfully
3. dotnet restore, build, test all pass
4. Test results reported in GitHub Actions

**Frontend CI validation:**
1. npm ci installs dependencies
2. Type check passes (no TypeScript errors)
3. Lint passes (no ESLint errors)
4. Build succeeds (dist/ folder created)

**Quality gates:**
- All tests pass (100% success rate)
- Build time: <5 minutes total (backend + frontend)
- Zero warnings in build output

### Dependencies and Prerequisites

**Prerequisites from previous stories:**
- Story 1.1: GitHub repository created
- Story 1.2: All packages configured
- Story 1.4: Domain entities exist for unit tests

**New dependencies:**
- xUnit 2.6.0 (backend testing)
- FluentAssertions (backend test assertions)
- ESLint + TypeScript plugins (frontend linting)

### References

- [Source: docs/solution-architecture.md#Section 1.1 - xUnit 2.6.0 for testing]
- [Source: docs/tech-spec-epic-1.md#Story 1.8 - CI/CD pipeline configuration]
- [Source: docs/epics.md#Epic 1, Story 1.8 - Acceptance Criteria]

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
