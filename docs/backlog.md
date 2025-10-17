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
| 2025-10-16 | 1.8 | 1 | TechDebt | Medium | DEV | Open | Implement code coverage reporting in backend pipeline. Add coverlet.collector package to LlmTokenPrice.Domain.Tests.csproj + coverage upload to backend-ci.yml. Target: 70% overall, 90% Domain. Related AC#1. |
| 2025-10-16 | 1.8 | 1 | Enhancement | Medium | DEV | Open | Add ESLint security plugins (eslint-plugin-security, eslint-plugin-no-unsanitized) for XSS/injection detection. Files: apps/web/eslint.config.js, package.json. Defer to Epic 3 frontend security hardening. Related AC#2. |
| 2025-10-16 | 1.8 | 1 | TechDebt | Low | DEV | Open | Expand unit test coverage to all domain entities (Capability, Benchmark, BenchmarkScore). Create CapabilityTests.cs, BenchmarkTests.cs, BenchmarkScoreTests.cs following ModelTests.cs pattern. Defer to Story 1.10. Related AC#1. |
| 2025-10-16 | 1.8 | 1 | Enhancement | Low | DEV | Open | Optimize frontend pipeline with node_modules caching for 5-10s improvement. Add cache step to .github/workflows/frontend-ci.yml. Optional performance enhancement. Related AC#2. |
