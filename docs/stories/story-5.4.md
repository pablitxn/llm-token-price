# Story 5.4: Add Benchmark Comparison Section

Status: Draft

## Story

As a user,
I want to see a dedicated benchmark comparison section with all benchmark scores,
so that I can evaluate model performance across multiple dimensions in detail.

## Acceptance Criteria

1. Benchmark comparison section added below the main comparison table
2. Lists all available benchmarks with scores for each selected model
3. Models displayed in columns, benchmarks displayed in rows (consistent with table layout)
4. Highest score in each row highlighted with bold text and green color
5. Missing scores displayed as "N/A" (not blank or 0)
6. Benchmarks grouped by category (Reasoning, Code, Math, Language, Multimodal) with collapsible sections

## Tasks / Subtasks

### Task Group 1: Create BenchmarkComparisonSection Component (AC: #1, #3)
- [ ] Create component file: `apps/web/src/components/comparison/BenchmarkComparisonSection.tsx`
  - [ ] Define `BenchmarkComparisonSectionProps` interface: `{ models: ModelDto[] }`
  - [ ] Create functional component with TypeScript
  - [ ] Export as named export
- [ ] Section layout with TailwindCSS
  - [ ] Container: `mt-8 bg-white border border-gray-200 rounded-lg p-6`
  - [ ] Section heading: "Benchmark Performance" (`text-2xl font-bold mb-6`)
  - [ ] Description: "Compare model performance across standardized benchmarks" (`text-gray-600 mb-4`)
- [ ] Use table structure for consistency
  - [ ] Similar to ComparisonTable (Story 5.3)
  - [ ] First column: Benchmark name + category badge
  - [ ] Model columns: Benchmark scores
  - [ ] Table: `min-w-full border-collapse`

### Task Group 2: Extract and Transform Benchmark Data (AC: #2, #5)
- [ ] Create `buildBenchmarkRows` utility function
  - [ ] Location: `apps/web/src/utils/benchmarkComparison.ts`
  - [ ] Input: `ModelDto[]` (contains `benchmarkScores` array)
  - [ ] Output: `BenchmarkComparisonRow[]`
  - [ ] Define `BenchmarkComparisonRow` type:
    ```typescript
    interface BenchmarkComparisonRow {
      benchmarkId: string;
      benchmarkName: string;
      category: 'reasoning' | 'code' | 'math' | 'language' | 'multimodal';
      scores: Record<string, number | null>; // modelId -> score
      maxScore?: number; // For percentage calculation
      interpretation: 'higher_better' | 'lower_better';
    }
    ```
- [ ] Collect all unique benchmarks across models
  - [ ] Iterate through all models' `benchmarkScores` arrays
  - [ ] Create set of unique benchmark IDs
  - [ ] Sort alphabetically within each category
- [ ] Map scores to each model
  - [ ] For each benchmark, create score map: `{ modelId: score }`
  - [ ] Handle missing scores: Set value to `null` (will display as "N/A")
  - [ ] Preserve score metadata: test date, source URL (if available)

### Task Group 3: Category Grouping (AC: #6)
- [ ] Define benchmark categories mapping
  - [ ] Create `benchmarkCategories.ts` config file
  - [ ] Map benchmark names to categories:
    ```typescript
    const BENCHMARK_CATEGORIES = {
      reasoning: ['MMLU', 'Big-Bench Hard', 'BBH', 'ARC-Challenge'],
      code: ['HumanEval', 'MBPP', 'CodeContests'],
      math: ['GSM8K', 'MATH', 'MathQA'],
      language: ['HellaSwag', 'TruthfulQA', 'ANLI'],
      multimodal: ['MMMU', 'VQA', 'TextVQA']
    };
    ```
  - [ ] Export category labels: `CATEGORY_LABELS = { reasoning: 'Reasoning', code: 'Code Generation', ... }`
- [ ] Group rows by category in `buildBenchmarkRows`
  - [ ] Sort rows by category first, then alphabetically
  - [ ] Add category metadata to each row
- [ ] Render category sections with headers
  - [ ] Section header row: `bg-gray-100 font-semibold py-3`
  - [ ] Category name with icon (Brain, Code, Calculator, MessageSquare, Image from lucide-react)
  - [ ] Benchmark count: "(X benchmarks)"

### Task Group 4: Collapsible Category Sections (AC: #6)
- [ ] Implement collapse state management
  - [ ] Use React state: `const [collapsedCategories, setCollapsedCategories] = useState<Set<string>>(new Set())`
  - [ ] Toggle function: `toggleCategory(category: string)`
  - [ ] Default: all categories expanded
- [ ] Add collapse/expand button to category headers
  - [ ] Icon: ChevronDown (expanded) / ChevronRight (collapsed) from lucide-react
  - [ ] Button: `cursor-pointer hover:bg-gray-200` on header row
  - [ ] Click handler: `onClick={() => toggleCategory(category)}`
- [ ] Conditional rendering of benchmark rows
  - [ ] Check if category is in `collapsedCategories` set
  - [ ] Apply `hidden` class to rows if collapsed
  - [ ] Smooth transition: `transition-all duration-200 ease-in-out`

### Task Group 5: Highlight Best Scores (AC: #4)
- [ ] Implement score highlighting logic
  - [ ] In `buildBenchmarkRows`, identify best score for each benchmark
  - [ ] Consider `interpretation` field (higher_better vs lower_better)
  - [ ] For higher_better: `Math.max(...scores)`
  - [ ] For lower_better: `Math.min(...scores)`
  - [ ] Handle ties: Highlight all models with best score
- [ ] Apply highlighting styles
  - [ ] Best score cells: `text-green-600 font-bold`
  - [ ] Add trophy icon (Trophy from lucide-react) next to best score
  - [ ] Icon size: `w-4 h-4 inline ml-1`
- [ ] Handle edge cases
  - [ ] All scores equal: Highlight all (rare but possible)
  - [ ] Only one model has score: Highlight by default
  - [ ] No scores available: No highlighting

### Task Group 6: Score Display Formatting (AC: #5)
- [ ] Create score formatting function
  - [ ] Location: `apps/web/src/utils/formatters.ts`
  - [ ] Function: `formatBenchmarkScore(score: number | null, maxScore?: number): string`
  - [ ] Handle null: Return "N/A" with gray text
  - [ ] Format number: 1 decimal place (e.g., "86.4")
  - [ ] Optional percentage: If maxScore provided, show "(86.4%)"
- [ ] Display score with context
  - [ ] Score value: Primary text
  - [ ] Max score (if available): Secondary text in gray
  - [ ] Example: "86.4 / 100" or "86.4%"
- [ ] Add tooltip for additional metadata (optional)
  - [ ] Test date: "Tested on: 2025-01-15"
  - [ ] Source: "Source: Official benchmark"
  - [ ] Use lucide-react `Info` icon with hover tooltip

### Task Group 7: Handle Missing Scores (AC: #5)
- [ ] Display "N/A" for missing scores
  - [ ] Text: "N/A" with `text-gray-400` color
  - [ ] Center align in cell
  - [ ] No highlighting for N/A cells
- [ ] Add visual indicator for data completeness
  - [ ] Badge showing "X/Y models tested" below benchmark name
  - [ ] Calculate: `testedCount / totalModels`
  - [ ] Example: "3/5 models" in small gray text
- [ ] Sort benchmarks by completeness (optional)
  - [ ] Benchmarks with most coverage appear first
  - [ ] Within category, sort by completeness then alphabetically

### Task Group 8: Integrate into ComparisonPage (AC: #1)
- [ ] Update `ComparisonPage.tsx` to include BenchmarkComparisonSection
  - [ ] Import component
  - [ ] Place below ComparisonTable: `<section className="mt-8">`
  - [ ] Pass models data: `<BenchmarkComparisonSection models={data.models} />`
- [ ] Add section divider
  - [ ] Horizontal rule: `<hr className="my-8 border-gray-200" />`
  - [ ] Or use spacing with background color change
- [ ] Loading state
  - [ ] Show skeleton while `isLoading`
  - [ ] Skeleton: Gray rows with `animate-pulse`, match table structure

### Task Group 9: Type Definitions (AC: #2, #3)
- [ ] Update `apps/web/src/types/comparison.ts`
  - [ ] Define `BenchmarkComparisonSectionProps` interface
  - [ ] Define `BenchmarkComparisonRow` interface
  - [ ] Define `BenchmarkCategory` type: `'reasoning' | 'code' | 'math' | 'language' | 'multimodal'`
  - [ ] Define `ScoreInterpretation` type: `'higher_better' | 'lower_better'`
- [ ] Ensure `ModelDto` includes `benchmarkScores`
  - [ ] Verify `BenchmarkScoreDto` structure:
    ```typescript
    interface BenchmarkScoreDto {
      benchmarkId: string;
      benchmarkName: string;
      score: number;
      maxScore?: number;
      category: string;
      testDate?: string;
      sourceUrl?: string;
    }
    ```

### Task Group 10: Accessibility (AC: #3, #6)
- [ ] Add ARIA attributes
  - [ ] Table caption: `<caption className="sr-only">Benchmark comparison by category</caption>`
  - [ ] Section heading: `<h2 id="benchmark-section">Benchmark Performance</h2>`
  - [ ] Category headers: `role="button"` and `aria-expanded` for collapse buttons
- [ ] Keyboard navigation
  - [ ] Collapse buttons: `tabindex="0"` and Enter/Space key handlers
  - [ ] Table: Keyboard navigable (browser default)
- [ ] Screen reader support
  - [ ] Announce best scores: `aria-label="Best score: 86.4"`
  - [ ] N/A cells: `aria-label="Score not available"`
  - [ ] Category state: `aria-expanded="true"` or `"false"`

### Task Group 11: Testing and Verification (AC: #1-6)
- [ ] Write unit test for `buildBenchmarkRows` utility
  - [ ] Test row generation from 2 models with different benchmarks
  - [ ] Test missing score handling (null values)
  - [ ] Test best score identification (higher_better and lower_better)
  - [ ] Test category grouping
  - [ ] Use Vitest
- [ ] Write integration test for BenchmarkComparisonSection
  - [ ] Render with mock ModelDto array containing benchmarkScores
  - [ ] Verify all benchmarks displayed
  - [ ] Test highlighting applied to best scores
  - [ ] Test collapse/expand functionality per category
  - [ ] Verify N/A display for missing scores
  - [ ] Use Vitest + React Testing Library
- [ ] Manual E2E testing
  - [ ] Navigate to `/compare?models=1,2,3`
  - [ ] Scroll to benchmark section below main table
  - [ ] Verify all benchmark categories present (Reasoning, Code, Math, Language, Multimodal)
  - [ ] Check best scores highlighted in green with trophy icon
  - [ ] Verify missing scores show as "N/A"
  - [ ] Test collapse/expand each category
  - [ ] Verify responsive behavior (horizontal scroll on mobile)

## Dev Notes

### Architecture Alignment
- **Component**: `BenchmarkComparisonSection` is a specialized table component (extends ComparisonTable pattern)
- **Data Transformation**: `buildBenchmarkRows` utility (pure function, testable)
- **Configuration**: `benchmarkCategories.ts` config file (easy to extend with new benchmarks)
- **No Direct API Calls**: Uses data from `useComparisonData` hook (Story 5.2)

### Benchmark Category Mapping
Story assumes benchmark categories are defined in backend (part of `BenchmarkDto`). If not available from API, use frontend mapping:

```typescript
// apps/web/src/config/benchmarkCategories.ts
export const BENCHMARK_CATEGORIES: Record<string, BenchmarkCategory> = {
  'MMLU': 'reasoning',
  'Big-Bench Hard': 'reasoning',
  'HumanEval': 'code',
  'MBPP': 'code',
  'GSM8K': 'math',
  'MATH': 'math',
  'HellaSwag': 'language',
  'TruthfulQA': 'language',
  'MMMU': 'multimodal',
  'VQA': 'multimodal',
};

export const getCategoryForBenchmark = (name: string): BenchmarkCategory => {
  return BENCHMARK_CATEGORIES[name] || 'reasoning'; // Default fallback
};
```

### Score Interpretation
Different benchmarks use different scales:
- **MMLU**: 0-100 (percentage), higher is better
- **HumanEval**: 0-100 (percentage), higher is better
- **Perplexity**: 0-∞ (lower is better) - RARE, not used in this platform

Default assumption: **Higher is better** (covers 95%+ of benchmarks)

### Best Score Highlighting Logic
```typescript
const findBestScore = (
  scores: Record<string, number | null>,
  interpretation: ScoreInterpretation
): number => {
  const validScores = Object.values(scores).filter((s): s is number => s !== null);

  if (validScores.length === 0) return NaN; // No valid scores

  return interpretation === 'higher_better'
    ? Math.max(...validScores)
    : Math.min(...validScores);
};
```

### Collapsible Sections Performance
With 5 categories × 4 benchmarks avg = 20 benchmark rows:
- Collapse hides ~4 rows per category
- No performance concerns (< 100 DOM elements)
- No need for virtualization

### Data Completeness Indicator
Visual indicator helps users understand data quality:
```
MMLU                  [3/5 models]
 Model1  Model2  Model3
  86.4    86.8    N/A
```

Shows that only 3 out of 5 models have been tested on MMLU.

### Prerequisites
- **Story 5.2**: `useComparisonData` hook provides models with `benchmarkScores`
- **Story 5.3**: `ComparisonTable` establishes table layout pattern
- **Backend**: `ModelDto` includes `benchmarkScores: BenchmarkScoreDto[]` array
- No new API endpoints required

### Quality Gates
- TypeScript strict mode: ✅ Zero `any` types
- Section renders: ✅ All benchmarks displayed grouped by category
- Highlighting: ✅ Best scores marked correctly
- Missing data: ✅ N/A displayed for null scores
- Collapse: ✅ Categories expand/collapse smoothly
- Responsive: ✅ Horizontal scroll on small screens
- Accessibility: ✅ ARIA labels, keyboard navigation

### Project Structure Notes
```
apps/web/src/
├── components/
│   └── comparison/
│       ├── BenchmarkComparisonSection.tsx   # New component (this story)
│       ├── ComparisonTable.tsx              # From Story 5.3
│       └── ModelCard.tsx                    # From Story 5.2
├── utils/
│   └── benchmarkComparison.ts               # buildBenchmarkRows utility (this story)
├── config/
│   └── benchmarkCategories.ts               # Category mapping (this story)
└── types/
    └── comparison.ts                        # Updated with benchmark types (this story)
```

### Performance Optimization
- Memoize `buildBenchmarkRows` result to prevent recalculation on every render
- Use React.memo() on BenchmarkComparisonSection if parent re-renders frequently
- Collapse state stored in Set for O(1) lookup

### References
- [Source: docs/tech-spec-epic-5.md#Detailed Design] - Benchmark comparison section specification
- [Source: docs/tech-spec-epic-5.md#Acceptance Criteria] - AC-5.4: Benchmark comparison requirements
- [Source: docs/epics.md#Story 5.4] - Original story with 6 acceptance criteria
- [Source: docs/solution-architecture.md#Database Schema] - Benchmarks table structure
- [Source: docs/PRD.md#FR004] - Benchmark scores storage and metadata

## Dev Agent Record

### Context Reference

<!-- Path(s) to story context XML will be added here by context workflow -->

### Agent Model Used

claude-sonnet-4-5-20250929

### Debug Log References

### Completion Notes List

### File List
