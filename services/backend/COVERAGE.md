# Code Coverage Guide

This document explains code coverage setup, targets, and best practices for the LLM Token Price backend.

## Current Coverage Status

As of **October 21, 2025** (Story 2.13 - Task 3):

| Assembly | Line Coverage | Branch Coverage | Target | Status |
|----------|---------------|-----------------|--------|--------|
| **Overall** | **82.0%** | **61.2%** | ≥70% / ≥60% | ✅ PASS |
| Domain | 98.4% | ~80% | ≥90% / ≥80% | ✅ PASS |
| Application | 87.9% | ~75% | ≥85% / ≥75% | ✅ PASS |
| Infrastructure | 91.8% | ~65% | ≥75% / ≥65% | ✅ PASS |
| API | 48.5% | ~40% | ≥50% / ≥40% | ⚠️ CLOSE |

### Key Metrics

- **Total Lines:** 8,222
- **Coverable Lines:** 4,396
- **Covered Lines:** 3,605 (82%)
- **Uncovered Lines:** 791 (18%)
- **Total Methods:** 338
- **Fully Covered Methods:** 274 (81%)

## Coverage Targets

### By Layer (Hexagonal Architecture)

We enforce different coverage targets based on architectural layers:

#### 1. Domain Layer (≥90% line, ≥80% branch)
**Why:** Contains pure business logic - MUST be thoroughly tested
- Entity validation logic
- Business rules and invariants
- Domain services (QAPS calculator, normalizers)
- No framework dependencies → easy to test

#### 2. Application Layer (≥85% line, ≥75% branch)
**Why:** Orchestrates use cases - should be well-tested
- Use case implementations
- DTO mapping logic
- Validators (FluentValidation)
- Service coordination

#### 3. Infrastructure Layer (≥75% line, ≥65% branch)
**Why:** Adapters to external systems - harder to test
- Repositories (EF Core)
- Cache adapters (Redis)
- Database configurations
- Some code tested via integration tests

#### 4. API Layer (≥50% line, ≥40% branch)
**Why:** Thin controllers - tested mostly via E2E tests
- HTTP request/response handling
- Authentication middleware
- Controller actions
- Focus on critical paths

### Overall Project (≥70% line, ≥60% branch)
Minimum acceptable coverage for production-ready code.

## Tools & Configuration

### Coverlet

Coverage collection tool integrated with `dotnet test`:

**Installation:**
Already included in all test projects via:
```xml
<PackageReference Include="coverlet.collector" Version="6.0.2" />
```

**Configuration:**
`coverlet.runsettings` - Defines excludes, formats, and thresholds

### ReportGenerator

Generates human-readable coverage reports:

**Installation:**
```bash
dotnet tool install -g dotnet-reportgenerator-globaltool
```

**Usage:**
```bash
# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage" --settings coverlet.runsettings

# Generate report
reportgenerator \
  -reports:"coverage/**/coverage.cobertura.xml" \
  -targetdir:"coverage-report" \
  -reporttypes:"Html;TextSummary;Badges"
```

### Automated Script

Use `generate-coverage.sh` for convenient local reporting:

```bash
# Generate coverage report
./generate-coverage.sh

# Generate and open in browser
./generate-coverage.sh --open
```

**Features:**
- Runs all tests with coverage
- Generates HTML report
- Prints text summary to console
- Checks thresholds (70% overall, 90% domain)
- Optionally opens report in browser

## CI/CD Integration

### GitHub Actions Workflow

Coverage is automatically collected and reported on every PR:

**Workflow:** `.github/workflows/backend-ci.yml`

**Steps:**
1. Run tests with coverage collection
2. Parse Cobertura XML for metrics
3. Post coverage report to PR comments
4. Upload to Codecov for historical tracking

**PR Comment Example:**
```markdown
### ✅ Backend Test Results & Coverage

#### 🧪 Test Results
| Metric | Value | Target |
|--------|-------|--------|
| Pass Rate | 95.7% | ≥95.0% |

#### 📊 Code Coverage
| Metric | Value | Target |
|--------|-------|--------|
| Line Coverage | 82.0% | ≥70.0% |
| Branch Coverage | 61.2% | ≥60.0% |

✅ Coverage meets target! Code is well-tested.
```

### Codecov

Historical coverage tracking and visualization:

- Dashboard: `https://codecov.io/gh/OWNER/REPO`
- Automated PR comments with diff coverage
- Coverage trends over time
- File-level coverage breakdown

## Low Coverage Areas

Current areas needing improvement (Story 2.13+):

### API Layer (48.5% → 50%+ needed)

**Critical Issues:**
- `AdminDashboardController`: 0% (not implemented yet)
- `AdminModelsController`: 27% (needs E2E tests)
- `ModelsController`: 30.1% (needs E2E tests)

**Action Items:**
- Add E2E tests for model CRUD operations
- Test error handling paths (400, 404, 500)
- Cover authentication/authorization logic

### Infrastructure Layer

**Specific Classes:**
- `RedisCacheRepository`: 47.6% (needs integration tests)
- `BenchmarkRepository`: 53.2% (needs more query coverage)
- `ModelRepository`: 62.5% (needs edge case tests)
- `DbInitializer`: 57.1% (mostly seed data)

**Action Items:**
- Add integration tests with TestContainers
- Test cache hit/miss scenarios
- Cover complex LINQ queries
- Test connection failures (graceful degradation)

### Application Layer

**Specific Classes:**
- `CreateBenchmarkScoreValidator`: 0% (unused or not tested)
- `AdminModelService`: 68.6% (needs edge case coverage)
- `DashboardMetricsDto`: 0% (not implemented)

**Action Items:**
- Add validator tests (or remove if unused)
- Test service error paths
- Cover all DTO mapping scenarios

## Best Practices

### 1. Test Important Code First

**Priority Order:**
1. Domain services (QAPS, normalization) - ✅ 98.4%
2. Validators (data integrity) - ✅ 100%
3. Repository queries (data access) - ⚠️ 53-87%
4. Controllers (E2E integration) - ⚠️ 27-48%

### 2. Don't Aim for 100%

**Acceptable Exclusions:**
- Auto-generated code (migrations, scaffolding)
- Auto-properties (excluded via `SkipAutoProps`)
- Program.cs startup configuration
- Trivial DTOs with no logic

### 3. Focus on Business Logic

**High-value tests:**
- ✅ `BenchmarkNormalizer`: 100% (critical calculation)
- ✅ Validator rules: 97-100% (data integrity)
- ✅ Service orchestration: 87-92%

**Lower-value tests:**
- ⚠️ HTTP request parsing (E2E covers this)
- ⚠️ DTO property getters/setters
- ⚠️ Configuration classes

### 4. Use Integration Tests Wisely

Some code is better tested via integration:
- Database queries (EF Core translation)
- Redis caching (connection pooling)
- Authentication flow (JWT validation)

### 5. Track Coverage Trends

- Use Codecov to monitor changes over time
- Require coverage to not decrease on PRs
- Celebrate coverage improvements

## Running Coverage Locally

### Quick Check (Console Output)

```bash
cd services/backend
dotnet test --collect:"XPlat Code Coverage" --settings coverlet.runsettings
```

### Full HTML Report

```bash
# One-liner using script
./generate-coverage.sh --open

# Manual steps
dotnet test --collect:"XPlat Code Coverage" --settings coverlet.runsettings
reportgenerator -reports:"coverage/**/coverage.cobertura.xml" \
                 -targetdir:"coverage-report" \
                 -reporttypes:"Html;TextSummary"
xdg-open coverage-report/index.html  # Linux
```

### Per-Assembly Breakdown

```bash
# Generate report with assembly breakdown
reportgenerator -reports:"coverage/**/coverage.cobertura.xml" \
                 -targetdir:"coverage-report" \
                 -reporttypes:"Html;TextSummary" \
                 -classfilters:"+LlmTokenPrice.Domain.*;+LlmTokenPrice.Application.*"
```

## Interpreting Coverage Metrics

### Line Coverage
- **Green (>80%)**: Well-tested code
- **Yellow (60-80%)**: Acceptable, could improve
- **Red (<60%)**: Needs attention

### Branch Coverage
- **Target**: 60%+ overall
- Measures if/else, switch cases, ternary operators
- Lower than line coverage is normal
- Focus on critical decision points

### Method Coverage
- **Full method coverage**: 81% (274/338 methods)
- Some methods partially covered (entry but not all paths)
- Aim for full coverage on public APIs

## Troubleshooting

### Coverage Report Shows 0%

**Cause:** No coverage files found
**Fix:**
```bash
# Ensure coverlet.collector is installed
dotnet add package coverlet.collector

# Run with explicit collector
dotnet test --collect:"XPlat Code Coverage"
```

### Coverage Doesn't Match Tests

**Cause:** Debug build instead of Release
**Fix:**
```bash
dotnet test --configuration Release --collect:"XPlat Code Coverage"
```

### ReportGenerator Not Found

**Cause:** Tool not installed globally
**Fix:**
```bash
dotnet tool install -g dotnet-reportgenerator-globaltool
```

### Coverage Files in Wrong Location

**Cause:** Results directory mismatch
**Fix:**
```bash
# Explicitly set results directory
dotnet test --collect:"XPlat Code Coverage" --results-directory ./coverage
reportgenerator -reports:"./coverage/**/coverage.cobertura.xml" -targetdir:"coverage-report"
```

## Future Enhancements

### Planned Improvements (Story 2.13+)

- [ ] **Coverage threshold enforcement in CI**
  - Fail build if coverage drops below targets
  - Prevent merges that decrease coverage

- [ ] **Per-PR coverage diff**
  - Show coverage change for modified files
  - Highlight new uncovered code

- [ ] **Coverage badges**
  - Add badges to README.md
  - Visual indicators on project homepage

- [ ] **Coverage by file type**
  - Separate targets for repositories vs services
  - Track controller coverage independently

- [ ] **Historical coverage tracking**
  - Store coverage metrics in database
  - Generate trend charts
  - Alert on coverage degradation

## Resources

- **Coverlet Documentation:** https://github.com/coverlet-coverage/coverlet
- **ReportGenerator Docs:** https://github.com/danielpalme/ReportGenerator
- **Codecov Guide:** https://docs.codecov.com/docs
- **xUnit Coverage:** https://xunit.net/docs/code-coverage

---

**Last Updated:** 2025-10-21
**Story:** Story 2.13 - Technical Debt Resolution
**Task:** Task 3 - Code Coverage Reporting
**Maintainer:** Backend Team
