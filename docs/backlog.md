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
