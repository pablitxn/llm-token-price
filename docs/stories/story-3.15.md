# Story 3.15: Optimize API Response and Caching

Status: Draft

## Story

As a developer,
I want optimized API response with caching,
So that page loads are fast and reduce database load.

## Acceptance Criteria

1. Backend implements Redis caching for `/api/models` response (1 hour TTL)
2. API response includes only necessary fields (avoid over-fetching)
3. Cache key invalidated when model data updated in admin
4. API response compressed (gzip)
5. Page load time <2 seconds verified
6. Frontend implements React Query for client-side caching

## Tasks / Subtasks

- [ ] Task 1: Implement Redis Caching in Backend (AC: 1)
  - [ ] Add caching middleware or service method
  - [ ] Cache GET /api/models response with key "cache:models:list:v1"
  - [ ] Set TTL to 1 hour (3600 seconds)
  - [ ] Return cached response if exists
  - [ ] Add "cached: true" to response meta

- [ ] Task 2: Optimize API Response (AC: 2)
  - [ ] Review ModelDto - remove unnecessary fields
  - [ ] Include only fields needed for table display
  - [ ] Test response size reduction

- [ ] Task 3: Implement Cache Invalidation (AC: 3)
  - [ ] Add cache invalidation on admin model update
  - [ ] Clear "cache:models:*" pattern on update
  - [ ] Test cache clears when model updated
  - [ ] Test new data fetched after invalidation

- [ ] Task 4: Enable Response Compression (AC: 4)
  - [ ] Add response compression middleware in ASP.NET Core
  - [ ] Enable gzip compression
  - [ ] Test response headers include Content-Encoding: gzip
  - [ ] Measure response size reduction

- [ ] Task 5: Configure React Query Caching (AC: 6)
  - [ ] Verify staleTime: 5 minutes configured
  - [ ] Verify cacheTime: 30 minutes configured
  - [ ] Test client-side cache hit (no network request)
  - [ ] Test background refetch after stale time

- [ ] Task 6: Performance Testing (AC: 5)
  - [ ] Measure page load time with Chrome DevTools
  - [ ] Test cold cache scenario (first visit)
  - [ ] Test warm cache scenario (Redis cache hit)
  - [ ] Test client cache scenario (React Query cache hit)
  - [ ] Verify all scenarios < 2 seconds

## Dev Notes

### Prerequisites
- Story 3.14 (Capability Icons) complete
- Story 1.5 (Redis Cache) complete

### References
- [Source: docs/epics.md#Story 3.15]
- [Source: docs/solution-architecture.md#4] - Multi-layer caching strategy
- [Source: docs/solution-architecture.md#4.2] - Cache key patterns

## Dev Agent Record

### Context Reference

### Agent Model Used

### Debug Log References

### Completion Notes List

### File List
