# Story 5.13: Add Comparison Page Navigation and State Management

Status: Draft

## Story

As a user,
I want to dynamically add or remove models from the comparison without leaving the page,
so that I can refine my comparison interactively and maintain my analysis context.

## Acceptance Criteria

1. "Add Model" button displayed on comparison page (below cards or top toolbar)
2. Clicking button opens model selector modal/dropdown showing all available models (searchable)
3. Selecting model adds to comparison (max 5 models enforced)
4. URL updates when models added/removed (maintains shareable link)
5. Browser back/forward buttons navigate through comparison history correctly
6. Comparison state syncs with main table selection (Zustand store integration)

## Tasks / Subtasks

### Task Group 1: Create AddModelButton Component (AC: #1)
- [ ] Create component file: `apps/web/src/components/comparison/AddModelButton.tsx`
  - [ ] Define `AddModelButtonProps` interface:
    ```typescript
    interface AddModelButtonProps {
      currentModelIds: string[];
      onModelAdd: (modelId: string) => void;
      maxModels?: number;  // Default: 5
      className?: string;
    }
    ```
  - [ ] Create functional component with TypeScript
  - [ ] Export as named export
- [ ] Component layout structure
  - [ ] Button with Plus icon
  - [ ] Text: "Add Model to Comparison"
  - [ ] Disabled if max models reached (5)
  - [ ] Example:
    ```typescript
    <button
      onClick={() => setIsModalOpen(true)}
      disabled={currentModelIds.length >= maxModels}
      className={cn(
        'px-4 py-2 bg-white border border-gray-300 rounded-lg hover:bg-gray-50',
        'flex items-center gap-2 transition-colors',
        currentModelIds.length >= maxModels && 'opacity-50 cursor-not-allowed'
      )}
    >
      <Plus className="w-4 h-4" />
      Add Model
      {currentModelIds.length >= maxModels && (
        <span className="text-xs text-gray-500">(Max 5)</span>
      )}
    </button>
    ```
  - [ ] Icons from lucide-react: `Plus`

### Task Group 2: Create ModelSelectorModal Component (AC: #2)
- [ ] Create component file: `apps/web/src/components/comparison/ModelSelectorModal.tsx`
  - [ ] Define `ModelSelectorModalProps` interface:
    ```typescript
    interface ModelSelectorModalProps {
      isOpen: boolean;
      onClose: () => void;
      onModelSelect: (modelId: string) => void;
      excludeModelIds: string[];  // Already selected models
    }
    ```
  - [ ] Create functional component with TypeScript
  - [ ] Export as named export
- [ ] Modal layout structure
  - [ ] Overlay: `fixed inset-0 bg-black bg-opacity-50 z-50`
  - [ ] Modal container: `fixed inset-0 flex items-center justify-center p-4`
  - [ ] Modal content: `bg-white rounded-lg shadow-xl max-w-2xl w-full max-h-[80vh] overflow-hidden`
  - [ ] Header: "Add Model to Comparison" with close button (X)
  - [ ] Search bar: Filter models by name or provider
  - [ ] Model list: Scrollable list of available models
  - [ ] Footer: "Cancel" button

### Task Group 3: Fetch Available Models (AC: #2)
- [ ] Use existing GET `/api/models` endpoint
  - [ ] Import useModels hook from Story 1.10 (if exists)
  - [ ] Or create new TanStack Query hook:
    ```typescript
    const useAvailableModels = () => {
      return useQuery({
        queryKey: ['models'],
        queryFn: async () => {
          const response = await axios.get('/api/models');
          return response.data;
        },
        staleTime: 5 * 60 * 1000,  // 5 minutes
      });
    };
    ```
  - [ ] Return: `{ data, isLoading, error }`
- [ ] Filter out already-selected models
  - [ ] Available models = all models - current comparison models
  - [ ] Filter: `availableModels.filter(m => !excludeModelIds.includes(m.id))`
  - [ ] Display count: "Select from X available models"

### Task Group 4: Implement Search Functionality (AC: #2)
- [ ] Add search input to modal
  - [ ] Input field: `<input type="text" placeholder="Search models by name or provider..." />`
  - [ ] Icon: Search from lucide-react
  - [ ] Input style: `px-4 py-2 border border-gray-300 rounded-lg w-full`
  - [ ] Debounced search (300ms delay)
- [ ] Filter models by search query
  - [ ] Search fields: Model name, provider
  - [ ] Case-insensitive: `model.name.toLowerCase().includes(query.toLowerCase())`
  - [ ] Highlight matching text (optional enhancement)
  - [ ] Implementation:
    ```typescript
    const [searchQuery, setSearchQuery] = useState('');

    const filteredModels = useMemo(() => {
      if (!searchQuery) return availableModels;

      const query = searchQuery.toLowerCase();
      return availableModels.filter(model =>
        model.name.toLowerCase().includes(query) ||
        model.provider.toLowerCase().includes(query)
      );
    }, [availableModels, searchQuery]);
    ```
- [ ] Show "No models found" if filtered list empty
  - [ ] Empty state: "No models match your search"
  - [ ] Suggestion: "Try different keywords"

### Task Group 5: Model List Display (AC: #2)
- [ ] Render scrollable model list
  - [ ] Container: `overflow-y-auto max-h-96` (scrollable)
  - [ ] Each model: Card with name, provider, key specs
  - [ ] Example:
    ```typescript
    <div className="overflow-y-auto max-h-96 border-y border-gray-200">
      {filteredModels.map(model => (
        <button
          key={model.id}
          onClick={() => handleModelSelect(model.id)}
          className="w-full px-6 py-4 text-left hover:bg-gray-50 border-b border-gray-200 transition-colors"
        >
          <div className="flex items-center justify-between">
            <div>
              <div className="font-semibold text-gray-900">{model.name}</div>
              <div className="text-sm text-gray-600">{model.provider}</div>
            </div>
            <div className="text-right text-sm text-gray-500">
              <div>${model.inputPricePer1M.toFixed(2)} / ${model.outputPricePer1M.toFixed(2)}</div>
              <div>{model.contextWindow.toLocaleString()} tokens</div>
            </div>
          </div>
        </button>
      ))}
    </div>
    ```
- [ ] Loading state
  - [ ] While fetching: Show skeleton list
  - [ ] Skeleton: Gray rectangles with `animate-pulse`
- [ ] Error state
  - [ ] If API fails: Show error message
  - [ ] "Failed to load models. Please try again."

### Task Group 6: Add Model to Comparison (AC: #3, #4)
- [ ] Handle model selection
  - [ ] Function: `handleModelSelect(modelId: string)`
  - [ ] Check max models: `if (currentModelIds.length >= 5) return;`
  - [ ] Add to current list: `const newIds = [...currentModelIds, modelId];`
  - [ ] Update URL: `navigate(\/compare?models=${newIds.join(',')});`
  - [ ] Close modal: `setIsModalOpen(false);`
  - [ ] Show toast: `toast.success('Model added to comparison');`
- [ ] URL update triggers data refetch
  - [ ] useComparisonParams hook (Story 5.1) extracts new model IDs
  - [ ] useComparisonData hook (Story 5.2) refetches with new IDs
  - [ ] Charts and tables re-render automatically with new data

### Task Group 7: Remove Model from Comparison (AC: #4)
- [ ] Already implemented in Story 5.2 (ModelCard remove button)
  - [ ] X button on each model card
  - [ ] onClick: Remove model ID from URL
  - [ ] Navigate: `/compare?models=${remainingIds.join(',')}`
- [ ] Ensure consistency across components
  - [ ] ModelCard remove button updates URL
  - [ ] URL change triggers data refetch
  - [ ] All components re-render with updated models

### Task Group 8: URL State Management (AC: #4, #5)
- [ ] Use React Router's useNavigate and useSearchParams
  - [ ] Already implemented in Story 5.1 (useComparisonParams hook)
  - [ ] Read: `const [searchParams] = useSearchParams();`
  - [ ] Write: `navigate(\`/compare?models=${ids.join(',')}\`);`
- [ ] URL as single source of truth
  - [ ] Model IDs stored in URL: `/compare?models=id1,id2,id3`
  - [ ] No duplicate state (URL only)
  - [ ] All components read from URL
- [ ] Update URL without page reload
  - [ ] React Router navigate() pushes new history entry
  - [ ] Enables browser back/forward navigation
  - [ ] No full page reload (SPA behavior)

### Task Group 9: Browser History Navigation (AC: #5)
- [ ] Verify back/forward button functionality
  - [ ] Browser back: Returns to previous model selection
  - [ ] Browser forward: Advances to next model selection
  - [ ] React Router handles this automatically (history API)
- [ ] History entry format
  - [ ] Each model add/remove creates new history entry
  - [ ] Example history:
    ```
    1. /compare?models=1,2        (Initial)
    2. /compare?models=1,2,3      (Added model 3) ← Current
    3. /compare?models=1,2,3,4    (Added model 4)

    Back button → Entry 1 (/compare?models=1,2)
    Forward button → Entry 3 (/compare?models=1,2,3,4)
    ```
- [ ] Test browser navigation
  - [ ] Add model 3 → Back button → Should show 2 models
  - [ ] Forward button → Should show 3 models again

### Task Group 10: Zustand Store Integration (AC: #6)
- [ ] Create comparison store: `apps/web/src/store/comparisonStore.ts`
  - [ ] Store selected model IDs (synced with URL)
  - [ ] Store interface:
    ```typescript
    interface ComparisonState {
      selectedModelIds: string[];
      setSelectedModelIds: (ids: string[]) => void;
      addModel: (id: string) => void;
      removeModel: (id: string) => void;
    }

    export const useComparisonStore = create<ComparisonState>((set) => ({
      selectedModelIds: [],
      setSelectedModelIds: (ids) => set({ selectedModelIds: ids }),
      addModel: (id) =>
        set((state) => ({
          selectedModelIds: [...state.selectedModelIds, id].slice(0, 5),  // Max 5
        })),
      removeModel: (id) =>
        set((state) => ({
          selectedModelIds: state.selectedModelIds.filter((m) => m !== id),
        })),
    }));
    ```
- [ ] Sync store with URL on mount
  - [ ] Read model IDs from URL
  - [ ] Update store: `setSelectedModelIds(modelIdsFromURL)`
  - [ ] useEffect in ComparisonPage:
    ```typescript
    const { modelIds } = useComparisonParams();
    const { setSelectedModelIds } = useComparisonStore();

    useEffect(() => {
      setSelectedModelIds(modelIds);
    }, [modelIds, setSelectedModelIds]);
    ```
- [ ] Sync main table selection (optional, out of MVP scope)
  - [ ] Main table checkboxes (Epic 3) use same store
  - [ ] Selecting models in table adds to comparison
  - [ ] Comparison page changes reflect in table checkboxes
  - [ ] Not implemented in MVP (separate state acceptable)

### Task Group 11: Max Models Validation (AC: #3)
- [ ] Enforce 5-model limit
  - [ ] Disable "Add Model" button if 5 models already selected
  - [ ] Modal: Filter shows only non-selected models
  - [ ] handleModelSelect: Check `if (currentModelIds.length >= 5) return;`
  - [ ] Show warning toast: `toast.warning('Maximum 5 models can be compared');`
- [ ] Visual feedback for max limit
  - [ ] Button disabled state: Grayed out
  - [ ] Tooltip: "Maximum 5 models reached"
  - [ ] Badge: "5/5 models" near Add button

### Task Group 12: Modal Accessibility (AC: #2)
- [ ] Implement accessible modal
  - [ ] Use Headless UI Dialog component (recommended)
    - Already a dependency (if used elsewhere)
    - Handles focus trap, Escape key, ARIA automatically
  - [ ] Or custom implementation:
    - Focus trap: Keep Tab key within modal
    - Escape key: Close modal
    - Overlay click: Close modal
- [ ] ARIA attributes
  - [ ] Modal: `role="dialog"`, `aria-labelledby="modal-title"`, `aria-modal="true"`
  - [ ] Title: `<h2 id="modal-title">Add Model to Comparison</h2>`
  - [ ] Close button: `aria-label="Close modal"`
- [ ] Keyboard navigation
  - [ ] Tab through model list
  - [ ] Enter to select model
  - [ ] Escape to close modal
  - [ ] Focus returns to "Add Model" button on close

### Task Group 13: Empty State Handling (AC: #4, #5)
- [ ] URL with <2 models
  - [ ] Already handled in Story 5.1 (ComparisonPage empty state)
  - [ ] Show: "Select at least 2 models to compare"
  - [ ] "Add Model" button prominent (main CTA)
- [ ] URL with >5 models (edge case)
  - [ ] Trim to first 5: `modelIds.slice(0, 5)`
  - [ ] Show warning: "Only first 5 models shown (limit exceeded)"
  - [ ] Redirect to valid URL: `/compare?models=${first5Ids.join(',')}`

### Task Group 14: Loading and Error States (AC: #2)
- [ ] Loading state while fetching models
  - [ ] Modal content: Skeleton list
  - [ ] Search bar disabled
  - [ ] Text: "Loading models..."
- [ ] Error state if API fails
  - [ ] Error message: "Failed to load models"
  - [ ] Retry button: "Try Again"
  - [ ] Close button: "Cancel"
- [ ] Empty state if no models available
  - [ ] Message: "No models available to add"
  - [ ] This is rare (implies all models already selected)

### Task Group 15: Testing and Verification (AC: #1-6)
- [ ] Write unit test for useComparisonParams hook
  - [ ] Test URL parsing: `/compare?models=1,2,3` → `['1', '2', '3']`
  - [ ] Test empty URL: `/compare` → `[]`
  - [ ] Test invalid URL: `/compare?models=` → `[]`
  - [ ] Use Vitest
- [ ] Write integration test for AddModelButton + Modal
  - [ ] Render AddModelButton
  - [ ] Click button: Modal opens
  - [ ] Search for model: Filtered list updates
  - [ ] Select model: onModelAdd called with correct ID
  - [ ] Modal closes
  - [ ] Use Vitest + React Testing Library
- [ ] Manual E2E testing
  - [ ] Navigate to `/compare?models=1,2`
  - [ ] Click "Add Model" button: Modal opens
  - [ ] See list of available models (excluding model 1 and 2)
  - [ ] Search "GPT": List filters to GPT models only
  - [ ] Click "GPT-4 Turbo": Modal closes
  - [ ] URL updates: `/compare?models=1,2,<gpt4-id>`
  - [ ] Page re-renders with 3 models
  - [ ] Click X on model card: Model removed
  - [ ] URL updates: `/compare?models=1,2`
  - [ ] Browser back button: Returns to 3 models
  - [ ] Browser forward button: Returns to 2 models
  - [ ] Add 3 more models: "Add Model" button disabled (max 5)
  - [ ] Verify toast notifications for add/remove

## Dev Notes

### Architecture Alignment
- **URL as Source of Truth**: Model IDs stored in URL only (no duplicate state)
- **React Router Integration**: useNavigate for URL updates, useSearchParams for reading
- **Zustand Store**: Optional state sync with main table (out of MVP scope)
- **Modal Pattern**: Headless UI Dialog for accessibility (or custom implementation)
- **No Backend Changes**: Uses existing GET `/api/models` endpoint

### URL State Pattern Benefits

**Why URL as Single Source of Truth:**
- ✅ Shareable links: Copy URL to share exact comparison
- ✅ Browser navigation: Back/forward buttons work naturally
- ✅ Bookmarkable: Save comparison for later
- ✅ Deep linking: Direct access to specific comparison
- ✅ No state sync issues: URL is always correct

**Alternative (Not Used):**
- ❌ Zustand only: Not shareable, no browser history
- ❌ Both URL + Zustand: Sync complexity, potential conflicts

### Browser History Behavior

React Router's navigate() creates history entries:

```
User journey:
1. Starts with /compare?models=1,2
2. Adds model 3 → navigate() → /compare?models=1,2,3
3. Adds model 4 → navigate() → /compare?models=1,2,3,4
4. Removes model 2 → navigate() → /compare?models=1,3,4

History stack:
[/compare?models=1,2] ← Back button
[/compare?models=1,2,3]
[/compare?models=1,2,3,4]
[/compare?models=1,3,4] ← Current

Back button → /compare?models=1,2,3,4
Back again → /compare?models=1,2,3
Forward button → /compare?models=1,2,3,4
```

Each URL change = New history entry (standard SPA behavior).

### Model Selector Modal vs Dropdown

**Modal (Recommended):**
- ✅ More space for search + list
- ✅ Focused user attention (overlay)
- ✅ Better on mobile (fullscreen)
- ❌ More code (modal component)

**Dropdown:**
- ✅ Simpler implementation
- ✅ Less visual disruption
- ❌ Limited space (scrolling required)
- ❌ Hard to search on mobile

Recommendation: Modal for better UX.

### Max Models Enforcement

5-model limit enforced at multiple levels:

**Client-side validation:**
1. Disable "Add Model" button if 5 models
2. Check in handleModelSelect: `if (currentModelIds.length >= 5) return;`
3. Zustand store: `slice(0, 5)` (trim excess)
4. URL parsing: `modelIds.slice(0, 5)` (trim invalid URLs)

**No backend validation needed:**
- Backend accepts any number of IDs
- Frontend responsible for UX limits
- Backend may have different limit (e.g., 10 for API flexibility)

### Zustand Store Purpose

**In MVP:**
- Store syncs with URL (redundant but useful for other components)
- Provides helper functions: `addModel()`, `removeModel()`
- Optional: Not strictly needed if URL is source of truth

**Future use:**
- Main table checkbox state (Epic 3 integration)
- Comparison basket state (Story 3.11)
- Persist selected models across page navigation

**Decision:** Include in MVP for consistency with main table state (Story 3.11).

### Search Debouncing

Debounce search input to avoid excessive filtering:

```typescript
import { useDebouncedValue } from '@/hooks/useDebouncedValue';

const [searchQuery, setSearchQuery] = useState('');
const debouncedQuery = useDebouncedValue(searchQuery, 300);  // 300ms delay

const filteredModels = useMemo(() => {
  // Filter using debouncedQuery (not searchQuery)
}, [availableModels, debouncedQuery]);
```

Benefits:
- Avoids filtering on every keystroke
- Smoother UX (no lag)
- Better performance with large model lists

### Modal Focus Management

Proper focus handling:

**On open:**
1. Save current focus (Add Model button)
2. Focus first focusable element in modal (search input)
3. Trap Tab key within modal

**On close:**
1. Restore focus to Add Model button
2. User can continue keyboard navigation

Headless UI Dialog handles this automatically.

### Prerequisites
- **Story 5.11**: Comparison page layout complete (integration point)
- **Story 5.2**: ModelCard with remove button (URL update pattern)
- **Story 5.1**: useComparisonParams hook (URL state management)
- **Story 1.10**: GET `/api/models` endpoint (model list)
- **Story 3.11**: Comparison basket Zustand store (optional integration)
- No new dependencies (optional: Headless UI for modal)

### Quality Gates
- TypeScript strict mode: ✅ Zero `any` types
- Add model: ✅ Button opens modal, selecting model adds to URL
- Modal search: ✅ Filters model list in real-time
- Max models: ✅ 5-model limit enforced
- URL updates: ✅ Navigate() creates new history entry
- Browser history: ✅ Back/forward buttons work correctly
- Zustand sync: ✅ Store updates when URL changes
- Accessibility: ✅ Modal keyboard navigable, ARIA labels
- Performance: ✅ Modal opens in <100ms, search debounced

### Project Structure Notes
```
apps/web/src/
├── components/
│   └── comparison/
│       ├── AddModelButton.tsx             # New component (this story)
│       ├── ModelSelectorModal.tsx         # New component (this story)
│       └── ComparisonPage.tsx             # Updated: Add AddModelButton
├── hooks/
│   ├── useComparisonParams.ts             # From Story 5.1 (no changes)
│   └── useDebouncedValue.ts               # New hook (this story)
├── store/
│   └── comparisonStore.ts                 # New store (this story)
└── types/
    └── comparison.ts                      # Updated: Modal props
```

### Performance Considerations
- Modal render: <100ms (simple component)
- Model list: ~50 models × 100 bytes = 5KB (instant render)
- Search filtering: <10ms (memoized, debounced)
- URL update: <5ms (navigate() is fast)
- Data refetch: ~200-500ms (API call, depends on network)

### Data Flow
```
User clicks "Add Model" button
  → AddModelButton.onClick()
    → setIsModalOpen(true)
      → ModelSelectorModal renders
        → useAvailableModels() fetches all models
          → Filter out currentModelIds
            → Display searchable list

User selects model
  → handleModelSelect(modelId)
    → newIds = [...currentModelIds, modelId]
      → navigate(`/compare?models=${newIds.join(',')}`)
        → URL changes: /compare?models=1,2,3
          → useComparisonParams() extracts new IDs
            → useComparisonData() refetches comparison data
              → All components re-render with new models
        → setIsModalOpen(false)
          → Modal closes
        → toast.success('Model added')

Browser back button
  → React Router pops history
    → URL reverts: /compare?models=1,2
      → useComparisonParams() extracts old IDs
        → useComparisonData() refetches with 2 models
          → All components re-render with 2 models
```

### References
- [Source: docs/tech-spec-epic-5.md#Services and Modules] - Navigation and state management spec
- [Source: docs/tech-spec-epic-5.md#Acceptance Criteria] - AC-5.13: Navigation requirements
- [Source: docs/epics.md#Story 5.13] - Original story with 6 acceptance criteria
- [Source: docs/stories/story-5.1.md] - useComparisonParams hook, URL state pattern
- [Source: docs/stories/story-5.2.md] - ModelCard remove button, URL update pattern
- [Source: docs/stories/story-3.11.md] - Comparison basket Zustand store (potential integration)

## Dev Agent Record

### Context Reference

<!-- Path(s) to story context XML will be added here by context workflow -->

### Agent Model Used

claude-sonnet-4-5-20250929

### Debug Log References

### Completion Notes List

### File List
