# Story: Establish Test Infrastructure and Validation Framework

Status: Ready

## Story

As a **backend developer**,
I want **comprehensive test infrastructure with database isolation, architecture validation, and E2E testing capabilities**,
so that **I can write reliable tests that validate hexagonal architecture boundaries, database integrity, and critical infrastructure components**.

## Acceptance Criteria

1. **xUnit test framework configured** with parallel test execution enabled
2. **TestContainers setup** for PostgreSQL 16 and Redis 7.2 with automatic container lifecycle management
3. **Respawn database cleanup** configured to reset database state between integration tests (<100ms cleanup)
4. **ArchUnitNET tests** enforce hexagonal architecture boundaries (domain layer has zero infrastructure dependencies)
5. **FluentAssertions library** integrated for readable test assertions
6. **Playwright E2E framework** configured with API request context for fast data seeding
7. **DatabaseFixture (xUnit IClassFixture)** provides shared test database instance across test classes
8. **SampleDataSeeder factory** creates valid test entities (Models, Capabilities, Benchmarks, BenchmarkScores)
9. **Integration test** validates PostgreSQL connection, migration execution, and entity creation
10. **Integration test** validates Redis cache Get/Set/Delete operations with connection resilience
11. **Unit test** validates hexagonal architecture boundaries (ArchUnit rules fail if domain depends on infrastructure)
12. **E2E test** validates API health endpoint returns 200 OK with database + Redis status checks
13. **Test execution time** meets targets: Unit tests <10s, Integration tests <30s, E2E smoke tests <5min
14. **CI/CD pipeline** executes all test levels successfully in GitHub Actions workflow

## Tasks / Subtasks

### Setup Phase (AC: #1, #2, #3)

- [ ] **Install xUnit NuGet packages** (xUnit 2.6.0, xUnit.runner.visualstudio, Microsoft.NET.Test.Sdk) (AC: #1)
- [ ] **Install TestContainers.PostgreSql** and **TestContainers.Redis** NuGet packages (3.6.0+) (AC: #2)
- [ ] **Install Respawn** NuGet package (6.1.0+) for database cleanup (AC: #3)
- [ ] **Configure xUnit parallel execution** in `xunit.runner.json` (maxParallelThreads: auto) (AC: #1)
- [ ] **Create TestContainers configuration** class with PostgreSQL and Redis container initialization (AC: #2)

### Architecture Validation (AC: #4, #11)

- [ ] **Install ArchUnitNET** NuGet package (0.10.0+) (AC: #4)
- [ ] **Create ArchitectureTests.cs** in `LlmTokenPrice.Tests.Unit` project (AC: #11)
- [ ] **Define ArchUnit rules** to enforce:
  - Domain layer has zero dependencies on Infrastructure, Application, or API layers
  - Domain layer only references System namespaces and domain-internal namespaces
  - Repository interfaces (IModelRepository, etc.) live in Domain layer
  - Concrete repository implementations live in Infrastructure layer
- [ ] **Write failing test** that detects if domain depends on infrastructure packages (AC: #11)

### Test Support Infrastructure (AC: #5, #7, #8)

- [ ] **Install FluentAssertions** NuGet package (6.12.0+) (AC: #5)
- [ ] **Create DatabaseFixture.cs** implementing `IAsyncLifetime` for xUnit:
  - Starts TestContainers PostgreSQL instance
  - Runs EF Core migrations
  - Provides shared DbContext across test classes
  - Implements `Respawn` checkpoint for fast cleanup between tests
- [ ] **Create SampleDataSeeder.cs** factory class:
  - `CreateModel()` with Faker-based realistic data
  - `CreateCapability()` linked to model
  - `CreateBenchmark()` with standard definitions (MMLU, HumanEval, etc.)
  - `CreateBenchmarkScore()` with valid score ranges
  - All entities have valid timestamps and relationships

### Integration Tests (AC: #9, #10)

- [ ] **Create `LlmTokenPrice.Tests.Integration` project**
- [ ] **Write PostgreSQL integration test** (`DatabaseIntegrationTests.cs`):
  - Test connection establishment
  - Test migration execution (up/down)
  - Test entity creation with relationships (Model → Capability → BenchmarkScores)
  - Test Respawn cleanup (insert data → cleanup → verify empty)
- [ ] **Write Redis integration test** (`CacheIntegrationTests.cs`):
  - Test connection establishment
  - Test Get/Set/Delete operations
  - Test connection resilience (fallback if Redis unavailable)
  - Test cache TTL expiration

### E2E Tests (AC: #6, #12)

- [ ] **Install Playwright** NuGet package (Microsoft.Playwright 1.40.0+)
- [ ] **Create `LlmTokenPrice.Tests.E2E` project**
- [ ] **Configure Playwright** with API request context for fast data seeding
- [ ] **Write API health endpoint E2E test** (`HealthCheckTests.cs`):
  - Start backend API in test context
  - Send GET request to `/api/health`
  - Assert response status is 200 OK
  - Assert response body contains database status: "healthy"
  - Assert response body contains Redis status: "healthy"

### CI/CD Integration (AC: #13, #14)

- [ ] **Create `.github/workflows/test.yml`** GitHub Actions workflow:
  - Job 1: Unit tests (runs on every push)
  - Job 2: Integration tests (runs on PR to main, uses TestContainers)
  - Job 3: E2E smoke tests (runs on PR to main)
  - Job 4: Test report generation and upload
- [ ] **Configure test timeout limits** (Unit: 1min, Integration: 5min, E2E: 10min)
- [ ] **Add test coverage reporting** (Coverlet + ReportGenerator) with 70%+ target
- [ ] **Validate test execution times** meet acceptance criteria thresholds

### Documentation and Validation (All ACs)

- [ ] **Create `docs/testing-guide.md`** documenting:
  - How to run tests locally
  - TestContainers setup and troubleshooting
  - How to write new integration tests with DatabaseFixture
  - How to use SampleDataSeeder for test data
  - CI/CD pipeline test execution details
- [ ] **Verify all 14 acceptance criteria** are met with passing tests
- [ ] **Run full test suite** and confirm execution time targets achieved

## Dev Notes

### Technical Summary

This story establishes the **critical test infrastructure foundation** for the LLM Token Price platform, enabling reliable testing of:

1. **Hexagonal Architecture Boundaries:** ArchUnitNET enforces zero infrastructure dependencies in domain layer
2. **Database Integrity:** TestContainers + Respawn provide isolated, fast integration tests
3. **Cache Connectivity:** Redis integration tests validate connection resilience and operations
4. **E2E Critical Paths:** Playwright validates API health endpoints and frontend rendering

**Key Technical Decisions:**

- **TestContainers over in-memory databases:** Realistic PostgreSQL/Redis instances prevent "works in test, fails in prod" scenarios
- **Respawn over database recreation:** 10-100x faster cleanup (<100ms vs 5-10s per test)
- **ArchUnitNET over manual reviews:** Automated architecture violation detection catches issues in CI
- **Playwright API context:** Fast data seeding via API (1-2s) instead of UI (10-30s per test)

**Risk Mitigation:**

- **R-001 (Database Schema Flaws):** Integration tests validate entity relationships and migrations
- **R-002 (Architecture Violations):** ArchUnit tests enforce hexagonal boundaries automatically
- **R-005 (Redis Connection Failures):** Integration tests validate connection resilience and fallback

### Project Structure Notes

- **Files to create:**
  - `/tests/LlmTokenPrice.Tests.Unit/ArchitectureTests.cs` (Architecture validation)
  - `/tests/LlmTokenPrice.Tests.Integration/DatabaseIntegrationTests.cs` (PostgreSQL tests)
  - `/tests/LlmTokenPrice.Tests.Integration/CacheIntegrationTests.cs` (Redis tests)
  - `/tests/LlmTokenPrice.Tests.Integration/Fixtures/DatabaseFixture.cs` (xUnit fixture)
  - `/tests/LlmTokenPrice.Tests.Integration/Factories/SampleDataSeeder.cs` (Test data factory)
  - `/tests/LlmTokenPrice.Tests.E2E/HealthCheckTests.cs` (E2E smoke tests)
  - `/.github/workflows/test.yml` (CI/CD test workflow)
  - `/docs/testing-guide.md` (Documentation)

- **NuGet packages to install:**
  - xUnit 2.6.0 + xUnit.runner.visualstudio + Microsoft.NET.Test.Sdk
  - TestContainers.PostgreSql 3.6.0+
  - TestContainers.Redis 3.6.0+
  - Respawn 6.1.0+
  - ArchUnitNET 0.10.0+
  - FluentAssertions 6.12.0+
  - Microsoft.Playwright 1.40.0+
  - Coverlet.collector + ReportGenerator (test coverage)

- **Expected test locations:**
  - `/tests/LlmTokenPrice.Tests.Unit/` (17 test classes, ~50 tests)
  - `/tests/LlmTokenPrice.Tests.Integration/` (15 test classes, ~40 tests)
  - `/tests/LlmTokenPrice.Tests.E2E/` (5 test classes, ~12 tests)

- **Estimated effort:** 5 story points (3-4 days)
  - Day 1: TestContainers + DatabaseFixture + Respawn setup
  - Day 2: ArchUnit tests + SampleDataSeeder factory
  - Day 3: Integration tests (PostgreSQL + Redis)
  - Day 4: E2E tests (Playwright) + CI/CD workflow

### References

- **Test Design:** See `/docs/test-design-epic-1.md` for comprehensive risk assessment and test coverage plan
- **Architecture:** See `/docs/solution-architecture.md` Section 2.1 for hexagonal architecture boundaries
- **Knowledge Base:**
  - `bmad/bmm/testarch/knowledge/test-levels-framework.md` - Unit vs Integration vs E2E test selection
  - `bmad/bmm/testarch/knowledge/test-priorities-matrix.md` - P0-P3 prioritization (this story covers all P0 infrastructure tests)
  - `bmad/bmm/testarch/knowledge/fixture-architecture.md` - DatabaseFixture patterns and best practices

## Dev Agent Record

### Context Reference

- **Story Context XML:** `docs/stories/story-context-1.1.11.xml` (Generated: 2025-10-17)
  - Contains: 27 tasks across 6 groups, 14 acceptance criteria, 6 documentation artifacts, 6 code artifacts, 11 NuGet package dependencies, 12 architectural constraints, 4 interface definitions, and 15 test ideas mapped to ACs

### Agent Model Used

<!-- Will be populated during dev-story execution -->

### Debug Log References

<!-- Will be populated during dev-story execution -->

### Completion Notes List

<!-- Will be populated during dev-story execution -->

### File List

<!-- Will be populated during dev-story execution -->
