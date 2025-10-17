# Story 4.10: Calculator Sharing & Performance

Status: Ready

## Story

As a user,
I want to share calculator results and have instant performance,
so that I can discuss costs with team and get immediate feedback.

**Note:** This story consolidates sharing functionality (original 4.11) and performance optimization (original 4.12).

## Acceptance Criteria

1. "Share" button generates shareable URL with calculator parameters (volume, ratio)
2. URL format: `/calculator?volume=5000000&ratio=60`
3. Opening shared URL loads calculator with those parameters
4. "Copy Link" button copies URL to clipboard
5. Success message confirms copy
6. Calculation logic optimized to run in <100ms
7. Input changes debounced (100ms) to avoid excessive re-renders
8. Memoization used for expensive calculations
9. Chart rendering optimized (throttled updates)
10. Performance verified with 100+ models
11. No visible lag when adjusting sliders

## Tasks / Subtasks

- [ ] **Task 1: Implement URL params sync** (AC: #1, #2, #3)
- [ ] **Task 2: Create ShareButton component** (AC: #1, #4, #5)
- [ ] **Task 3: Optimize calculation logic** (AC: #6, #8)
- [ ] **Task 4: Add input debouncing** (AC: #7)
- [ ] **Task 5: Throttle chart updates** (AC: #9)
- [ ] **Task 6: Performance testing** (AC: #10, #11)

## Dev Notes

### URL params sync:
```typescript
export const useCalculatorParams = () => {
  const [searchParams, setSearchParams] = useSearchParams();
  const { setVolume, setRatio } = useCalculatorStore();

  useEffect(() => {
    const volume = searchParams.get('volume');
    const ratio = searchParams.get('ratio');
    if (volume) setVolume(Number(volume));
    if (ratio) setRatio(Number(ratio));
  }, []);

  const shareUrl = useMemo(() => {
    const url = new URL(window.location.href);
    url.searchParams.set('volume', String(useCalculatorStore.getState().monthlyTokenVolume));
    url.searchParams.set('ratio', String(useCalculatorStore.getState().inputOutputRatio));
    return url.toString();
  }, [/* calculator state */]);

  return { shareUrl };
};
```

## References

- [Epic 4 Analysis: docs/epic-4-analysis-and-plan.md#Story 4.10]

## Dev Agent Record

### Agent Model Used

claude-sonnet-4-5-20250929
