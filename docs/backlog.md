# Engineering Backlog

This backlog collects cross-cutting or future action items that emerge from reviews and planning.

Routing guidance:

- Use this file for non-urgent optimizations, refactors, or follow-ups that span multiple stories/epics.
- Must-fix items to ship a story belong in that story's `Tasks / Subtasks`.
- Same-epic improvements may also be captured under the epic Tech Spec `Post-Review Follow-ups` section.

| Date | Story | Epic | Type | Severity | Owner | Status | Notes |
| ---- | ----- | ---- | ---- | -------- | ----- | ------ | ----- |
| 2025-10-16 | 1.5 | 1 | TechDebt | Medium | DEV | Open | Add automated test coverage for RedisCacheRepository. Defer to Story 1.8 (CI/CD). Related AC#5. Files: Create LlmTokenPrice.Infrastructure.Tests/Caching/RedisCacheRepositoryTests.cs |
| 2025-10-16 | 1.5 | 1 | Enhancement | Medium | TBD | Open | Consider implementing Polly retry logic for transient Redis failures (3 retries with exponential backoff). Defer to Phase 2. Related files: RedisCacheRepository.cs |
| 2025-10-16 | 1.5 | 1 | Enhancement | Low | TBD | Open | Add cache key validation utility for defense-in-depth security. Create CacheKeys.ValidateKey() method with alphanumeric + colon/hyphen validation, max 250 chars. |
| 2025-10-16 | 1.5 | 1 | Bug | Low | Pablo | **Done** | Clean up corrupted cache entries on JSON deserialization failure. RedisCacheRepository.cs:71-73 - Added DeleteAsync call on JsonException. Completed 2025-10-16. |
| 2025-10-16 | 1.5 | 1 | Documentation | Low | Pablo | **Done** | Document CancellationToken limitation in ICacheRepository interface XML comments. Added note to all 4 methods explaining StackExchange.Redis 2.7.10 limitation. Completed 2025-10-16. |
| 2025-10-16 | 1.8 | 1 | Documentation | Medium | SM | Open | Add Story Context XML documentation for CI/CD architectural decisions (service containers, trigger patterns, caching strategy). Create docs/story-context-1.8.xml. Related AC#5, AC#6. |
| 2025-10-16 | 1.8 | 1 | TechDebt | Medium | Pablo | **Done** | Implement code coverage reporting in backend pipeline. Added coverlet.collector v6.0.2 + Codecov upload to backend-ci.yml with XPlat Code Coverage collection. Generates coverage.cobertura.xml. Completed 2025-10-16. Related AC#1. |
| 2025-10-16 | 1.8 | 1 | Enhancement | Medium | Pablo | **Done** | Add ESLint security plugins for XSS/injection detection. Installed eslint-plugin-security v3.0.1 + eslint-plugin-no-unsanitized v4.1.4. Configured 9 security rules in eslint.config.js (detect-unsafe-regex, detect-eval-with-expression, no-unsanitized/method, etc.). Completed 2025-10-16. Related AC#2. |
| 2025-10-16 | 1.8 | 1 | TechDebt | Low | DEV | Open | Expand unit test coverage to all domain entities (Capability, Benchmark, BenchmarkScore). Create CapabilityTests.cs, BenchmarkTests.cs, BenchmarkScoreTests.cs following ModelTests.cs pattern. Defer to Story 1.10. Related AC#1. |
| 2025-10-16 | 1.8 | 1 | Enhancement | Low | Pablo | **Done** | Optimize frontend pipeline with node_modules caching. Added cache step to .github/workflows/frontend-ci.yml with pnpm-lock.yaml hash key. Expected 5-10s improvement on cache hit. Completed 2025-10-16. Related AC#2. |
| 2025-10-16 | 1.9 | 1 | TechDebt | Low | DEV | Open | Add unit tests for SampleDataSeeder. Create SampleDataSeederTests.cs in LlmTokenPrice.Infrastructure.Tests/Data/Seeds with tests for: (1) SeedAsync_WithEmptyDatabase_SeedsCorrectCounts (verify 10 models, 5 benchmarks, 34+ scores), (2) SeedAsync_WithExistingData_SkipsSeeding (idempotency check), (3) SeedAsync_CreatesCorrectRelationships (Model → Capability 1:1, Model → Scores 1:N). Defer to Epic 2. Related AC#1, AC#3. |
| 2025-10-16 | 1.9 | 1 | Enhancement | Low | TBD | Open | Add benchmark score validation helper in SampleDataSeeder. Create CreateScore() method to validate scores fall within TypicalRangeMin/Max. Prevents data quality issues if benchmark definitions change. File: SampleDataSeeder.cs:505. Defer to future enhancement. Related AC#2. |
