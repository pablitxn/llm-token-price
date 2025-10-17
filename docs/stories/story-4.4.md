# Story 4.4: Benchmarks Tab with All Scores

Status: Ready

## Story

As a user,
I want to see all benchmark scores for a model,
so that I can evaluate performance across multiple dimensions.

## Acceptance Criteria

1. Benchmarks tab displays all available benchmark scores for model
2. Scores organized by category (reasoning, math, code, language, multimodal)
3. Each benchmark shows: name, score, max_score, interpretation
4. Test date and source URL link displayed if available
5. Missing benchmarks show as "Not tested" or excluded
6. Scores sorted by category then alphabetically

## Tasks / Subtasks

- [ ] **Task 1: Create BenchmarksTab component** (AC: #1, #2)
  - [ ] 1.1: Create `BenchmarksTab.tsx` in `/apps/web/src/components/models/tabs`
  - [ ] 1.2: Accept model prop (ModelDetail type)
  - [ ] 1.3: Group benchmarks by category using groupBy utility function
  - [ ] 1.4: Render sections for each category (Reasoning, Math, Code, Language, Multimodal)
  - [ ] 1.5: Sort categories in fixed order (Reasoning → Code → Math → Language → Multimodal)
  - [ ] 1.6: Within each category, sort benchmarks alphabetically by name

- [ ] **Task 2: Create groupBy utility function** (AC: #2)
  - [ ] 2.1: Create or update `helpers.ts` in `/apps/web/src/utils`
  - [ ] 2.2: Implement generic groupBy function: `groupBy<T, K>(array: T[], key: K): Record<string, T[]>`
  - [ ] 2.3: Return object with category names as keys, benchmark arrays as values
  - [ ] 2.4: Add TypeScript generics for type safety

- [ ] **Task 3: Create BenchmarkRow component** (AC: #3, #4)
  - [ ] 3.1: Create `BenchmarkRow.tsx` component
  - [ ] 3.2: Accept benchmark prop (BenchmarkScoreDetail type)
  - [ ] 3.3: Display benchmark name with tooltip showing full name
  - [ ] 3.4: Display score with formatting (e.g., "85.2" or "85.2/100")
  - [ ] 3.5: Display interpretation text ("Higher is better" / "Lower is better")
  - [ ] 3.6: Display test date if available (formatted with date-fns)
  - [ ] 3.7: Display source URL as external link icon if available
  - [ ] 3.8: Style with flexbox for alignment (name left, score right)

- [ ] **Task 4: Create Section component for categories** (AC: #2)
  - [ ] 4.1: Create `Section.tsx` component (reusable, already created in 4.3)
  - [ ] 4.2: Accept title and children props
  - [ ] 4.3: Render title with border-bottom separator
  - [ ] 4.4: Render children with padding
  - [ ] 4.5: Optional: Add collapsible functionality (expand/collapse icon)

- [ ] **Task 5: Handle empty state** (AC: #5)
  - [ ] 5.1: Check if model.benchmarkScores.length === 0
  - [ ] 5.2: Display EmptyState component with message "No benchmark scores available"
  - [ ] 5.3: If category has no benchmarks, show "No {category} benchmarks tested"
  - [ ] 5.4: Style empty state with gray background and centered text

- [ ] **Task 6: Add score visualization** (AC: #3)
  - [ ] 6.1: Create visual progress bar for scores (optional enhancement)
  - [ ] 6.2: Calculate percentage: (score / max_score) * 100
  - [ ] 6.3: Render horizontal bar with fill based on percentage
  - [ ] 6.4: Color code: green (>80%), yellow (60-80%), red (<60%)
  - [ ] 6.5: Display numeric score alongside bar

- [ ] **Task 7: Integrate BenchmarksTab in modal** (AC: #1)
  - [ ] 7.1: Import BenchmarksTab in ModelDetailModal
  - [ ] 7.2: Render when activeTab === 'benchmarks'
  - [ ] 7.3: Pass model data from useModelDetail hook
  - [ ] 7.4: Test tab switching from Overview to Benchmarks works

- [ ] **Task 8: Testing and polish**
  - [ ] 8.1: Write unit tests for BenchmarksTab component (Vitest)
  - [ ] 8.2: Test groupBy utility function with sample data
  - [ ] 8.3: Test renders all categories with correct benchmarks
  - [ ] 8.4: Test empty state displays when no benchmarks
  - [ ] 8.5: Test external link opens source URL in new tab
  - [ ] 8.6: Test sorting: categories in order, benchmarks alphabetical within category
  - [ ] 8.7: Verify tooltips display full benchmark names

## Dev Notes

### Architecture Context

**Benchmark Organization:**
- Categories defined by backend (Benchmark.Category field)
- Frontend displays in consistent order regardless of data order
- Future: User preference for category order (saved in localStorage)

**Score Interpretation:**
- "Higher is better" (most benchmarks): MMLU, HumanEval, GSM8K
- "Lower is better" (rare): Perplexity, Latency benchmarks
- Display interpretation text from backend (don't hardcode)

**External Links Security:**
- Source URLs open in new tab with `target="_blank"`
- Use `rel="noopener noreferrer"` to prevent security issues
- Icon indicator (ExternalLinkIcon) shows it's external

### Project Structure Notes

**Frontend Files to Create/Update:**
```
/apps/web/src/
├── components/
│   └── models/
│       └── tabs/
│           ├── BenchmarksTab.tsx             # Main benchmarks tab
│           ├── BenchmarkRow.tsx              # Individual benchmark display
│           └── Section.tsx                   # (reuse from 4.3)
├── utils/
│   └── helpers.ts                            # (update) Add groupBy function
└── types/
    └── modelDetail.ts                        # (already has BenchmarkScoreDetail interface)
```

### Implementation Details

**BenchmarksTab Component:**
```typescript
// components/models/tabs/BenchmarksTab.tsx
import { ModelDetail } from '@/types/modelDetail';
import { groupBy } from '@/utils/helpers';
import { Section } from './Section';
import { BenchmarkRow } from './BenchmarkRow';

interface BenchmarksTabProps {
  model: ModelDetail;
}

export const BenchmarksTab = ({ model }: BenchmarksTabProps) => {
  if (model.benchmarkScores.length === 0) {
    return (
      <div className="text-center py-12 text-gray-500">
        No benchmark scores available for this model
      </div>
    );
  }

  const benchmarksByCategory = groupBy(model.benchmarkScores, 'category');

  // Define category order
  const categoryOrder = ['Reasoning', 'Code', 'Math', 'Language', 'Multimodal'];
  const sortedCategories = categoryOrder.filter(cat => benchmarksByCategory[cat]);

  return (
    <div className="space-y-6">
      {sortedCategories.map(category => (
        <Section key={category} title={category}>
          <div className="space-y-3">
            {benchmarksByCategory[category]
              .sort((a, b) => a.benchmarkName.localeCompare(b.benchmarkName))
              .map(benchmark => (
                <BenchmarkRow key={benchmark.benchmarkName} benchmark={benchmark} />
              ))}
          </div>
        </Section>
      ))}
    </div>
  );
};
```

**BenchmarkRow Component:**
```typescript
// components/models/tabs/BenchmarkRow.tsx
import { BenchmarkScoreDetail } from '@/types/modelDetail';
import { InfoIcon, ExternalLinkIcon } from 'lucide-react';
import { Tooltip } from '@/components/ui/Tooltip';
import { formatDate } from '@/utils/formatters';

interface BenchmarkRowProps {
  benchmark: BenchmarkScoreDetail;
}

export const BenchmarkRow = ({ benchmark }: BenchmarkRowProps) => (
  <div className="flex items-center justify-between p-3 bg-gray-50 rounded-lg hover:bg-gray-100 transition-colors">
    {/* Left: Name and interpretation */}
    <div className="flex-1">
      <div className="flex items-center gap-2">
        <span className="font-medium">{benchmark.benchmarkName}</span>
        <Tooltip content={benchmark.fullName}>
          <InfoIcon className="w-4 h-4 text-gray-400" />
        </Tooltip>
      </div>
      <p className="text-sm text-gray-500">{benchmark.interpretation}</p>
      {benchmark.testDate && (
        <p className="text-xs text-gray-400 mt-1">
          Tested: {formatDate(benchmark.testDate)}
        </p>
      )}
    </div>

    {/* Right: Score and source */}
    <div className="flex items-center gap-4">
      <div className="text-right">
        <div className="text-lg font-semibold">
          {benchmark.score.toFixed(1)}
          {benchmark.maxScore && `/${benchmark.maxScore}`}
        </div>
      </div>

      {benchmark.sourceUrl && (
        <a
          href={benchmark.sourceUrl}
          target="_blank"
          rel="noopener noreferrer"
          className="text-blue-600 hover:text-blue-800"
          aria-label={`View ${benchmark.benchmarkName} source`}
        >
          <ExternalLinkIcon className="w-5 h-5" />
        </a>
      )}
    </div>
  </div>
);
```

**groupBy Utility Function:**
```typescript
// utils/helpers.ts
export const groupBy = <T extends Record<string, any>>(
  array: T[],
  key: keyof T
): Record<string, T[]> => {
  return array.reduce((acc, item) => {
    const group = String(item[key]);
    if (!acc[group]) acc[group] = [];
    acc[group].push(item);
    return acc;
  }, {} as Record<string, T[]>);
};
```

**Integration in ModelDetailModal:**
```typescript
// In ModelDetailModal.tsx
{activeTab === 'benchmarks' && <BenchmarksTab model={model} />}
```

### References

- [Epic 4 Analysis: docs/epic-4-analysis-and-plan.md#Story 4.4]
- [Story 4.3: Overview Tab] (Tabs component dependency)
- [Epics Document: docs/epics.md#Story 4.4]
- [Benchmark Categories: docs/PRD.md#Benchmark Metadata]

### Testing Strategy

**Unit Tests:**
- groupBy function correctly groups array by key
- BenchmarksTab renders all categories with benchmarks
- Categories sorted in correct order (Reasoning, Code, Math, Language, Multimodal)
- Benchmarks within category sorted alphabetically
- Empty state displays when no benchmarks
- External link has correct attributes (target, rel)

**Integration Tests:**
- Tab switching from Overview to Benchmarks loads data
- Tooltip shows full benchmark name on hover
- Source URL link opens in new tab
- Test date formats correctly

**Accessibility Tests:**
- Screen reader announces benchmark scores
- External link has aria-label
- Tooltip accessible via keyboard (focus)

## Dev Agent Record

### Context Reference

<!-- Path(s) to story context XML will be added here by context workflow -->

### Agent Model Used

claude-sonnet-4-5-20250929

### Debug Log References

### Completion Notes List

### File List
