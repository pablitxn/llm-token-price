# Story 3.9: Display Benchmark Scores in Table

Status: Draft

## Story

As a user,
I want to see key benchmark scores in the table,
So that I can compare model performance at a glance.

## Acceptance Criteria

1. Table columns added for 3-5 key benchmarks (MMLU, HumanEval, GSM8K)
2. Backend API updated to include benchmark scores in models response
3. Scores displayed with formatting (e.g., "85.2" or "85.2%")
4. Missing scores show as "N/A" or "-"
5. Benchmark columns sortable
6. Column headers show benchmark abbreviation with tooltip for full name

## Tasks / Subtasks

- [ ] Task 1: Update Backend ModelDto (AC: 2)
  - [ ] Add `topBenchmarks` property to ModelDto
  - [ ] Update ModelQueryService to include top 3-5 benchmarks
  - [ ] Select benchmarks: MMLU, HumanEval, GSM8K
  - [ ] Test GET /api/models returns benchmark data

- [ ] Task 2: Update Frontend ModelDto Type (AC: 2)
  - [ ] Add `topBenchmarks` to TypeScript ModelDto interface
  - [ ] Define BenchmarkScore type: { benchmarkName, score }

- [ ] Task 3: Add Benchmark Columns to Table (AC: 1, 3, 4)
  - [ ] Add column definitions for MMLU, HumanEval, GSM8K
  - [ ] Format scores (1 decimal place, add "%" if applicable)
  - [ ] Show "N/A" for missing scores
  - [ ] Style benchmark columns (right-aligned numbers)

- [ ] Task 4: Enable Benchmark Column Sorting (AC: 5)
  - [ ] Configure sorting for benchmark columns
  - [ ] Use numeric sortingFn
  - [ ] Handle N/A values (sort to end)

- [ ] Task 5: Add Tooltips to Benchmark Headers (AC: 6)
  - [ ] Add Tooltip component to benchmark column headers
  - [ ] Show full benchmark name on hover
  - [ ] Show benchmark description/interpretation

- [ ] Task 6: Testing
  - [ ] Verify backend returns benchmark scores
  - [ ] Test benchmark columns display correctly
  - [ ] Test score formatting
  - [ ] Test missing scores show as "N/A"
  - [ ] Test benchmark sorting
  - [ ] Test tooltips show full name

## Dev Notes

### Prerequisites
- Story 3.8 (Search Functionality) complete
- Story 1.10 (GET API) may need update for benchmark data

### References
- [Source: docs/epics.md#Story 3.9]
- [Source: docs/solution-architecture.md#7.1] - API contract for models

## Dev Agent Record

### Context Reference

### Agent Model Used

### Debug Log References

### Completion Notes List

### File List
