# Story 4.1: Model Detail Modal Component

Status: Ready

## Story

As a user,
I want to click a model to see full details,
so that I can explore specifications beyond table summary.

## Acceptance Criteria

1. Clicking model name in table opens modal overlay
2. Modal component created with header (model name, provider), close button
3. Modal body displays placeholder content
4. Modal dismissable by clicking close button or clicking outside
5. URL updates with model ID (e.g., `/?model=gpt-4`) for shareable links
6. Browser back button closes modal

## Tasks / Subtasks

- [ ] **Task 1: Create base Modal UI component** (AC: #1, #2, #3)
  - [ ] 1.1: Create `Modal.tsx` in `/apps/web/src/components/ui`
  - [ ] 1.2: Implement modal overlay with backdrop (fixed positioning, z-index: 50)
  - [ ] 1.3: Add modal content container (white background, rounded, shadow, max-width: 4xl)
  - [ ] 1.4: Create header section with title prop and close button (X icon from lucide-react)
  - [ ] 1.5: Add body section with scroll overflow for content
  - [ ] 1.6: Style with TailwindCSS (responsive, centered, max-height: 90vh)

- [ ] **Task 2: Implement modal dismissal logic** (AC: #4)
  - [ ] 2.1: Add onClick handler to backdrop to close modal
  - [ ] 2.2: Add Escape key listener with useEffect hook
  - [ ] 2.3: Prevent body scroll when modal is open (document.body.style.overflow = 'hidden')
  - [ ] 2.4: Cleanup event listeners on unmount
  - [ ] 2.5: Test all dismissal methods (backdrop, close button, Escape key)

- [ ] **Task 3: Create URL-based modal state management** (AC: #5, #6)
  - [ ] 3.1: Create `useModalState` hook in `/apps/web/src/hooks`
  - [ ] 3.2: Use React Router's `useSearchParams` to read/write `?model={id}` param
  - [ ] 3.3: Implement `openModal(id)` function that updates URL with model ID
  - [ ] 3.4: Implement `closeModal()` function that removes model param from URL
  - [ ] 3.5: Export `{ modelId, isOpen, openModal, closeModal }` from hook
  - [ ] 3.6: Test browser back button closes modal (URL change triggers modal close)

- [ ] **Task 4: Create ModelDetailModal component** (AC: #1, #2)
  - [ ] 4.1: Create `ModelDetailModal.tsx` in `/apps/web/src/components/models`
  - [ ] 4.2: Use `useModalState` hook to get modal state
  - [ ] 4.3: Create `useModelDetail(modelId)` hook stub (returns mock data for now)
  - [ ] 4.4: Render Modal component with model name as title
  - [ ] 4.5: Add loading spinner while fetching model data
  - [ ] 4.6: Display placeholder content in modal body ("Details coming in next story")
  - [ ] 4.7: Handle error state if model not found

- [ ] **Task 5: Integrate modal trigger in ModelsTable** (AC: #1)
  - [ ] 5.1: Import `useModalState` hook in ModelsTable component
  - [ ] 5.2: Make model name cell clickable (add onClick handler)
  - [ ] 5.3: Call `openModal(model.id)` when model name clicked
  - [ ] 5.4: Add cursor-pointer and hover:underline styles to model name
  - [ ] 5.5: Test clicking model opens modal with correct model ID in URL

- [ ] **Task 6: Add ModelDetailModal to HomePage** (AC: #1)
  - [ ] 6.1: Import ModelDetailModal in HomePage component
  - [ ] 6.2: Render ModelDetailModal (always rendered, visibility controlled by isOpen)
  - [ ] 6.3: Test modal appears when URL has ?model={id} param
  - [ ] 6.4: Test modal is hidden when URL has no model param

- [ ] **Task 7: Testing and polish**
  - [ ] 7.1: Write unit tests for useModalState hook (Vitest)
  - [ ] 7.2: Test URL param updates correctly on open/close
  - [ ] 7.3: Test browser back/forward navigation works
  - [ ] 7.4: Verify modal is accessible (focus trap, ARIA labels)
  - [ ] 7.5: Test on different screen sizes (responsive)

## Dev Notes

### Architecture Context

**URL-Based Modal State Pattern:**
- Modal state lives in URL search params, NOT component state
- Benefits:
  - Shareable URLs: Users can copy `/?model=abc-123` and share
  - Browser navigation: Back button naturally closes modal
  - Deep linking: External links can open specific models
  - Bookmarkable: Users can bookmark model detail "pages"

**Modal Component Design:**
- Generic `Modal` UI component (reusable)
- Specific `ModelDetailModal` component (business logic)
- Separation of concerns: UI vs data fetching

**Why Not Separate Routes?**
- `/models/{id}` would require full page navigation
- Loses table context (user has to navigate back)
- Modal overlay preserves table state and filters

### Project Structure Notes

**Frontend Files to Create:**
```
/apps/web/src/
├── components/
│   ├── ui/
│   │   └── Modal.tsx                    # Generic modal component
│   └── models/
│       └── ModelDetailModal.tsx         # Model-specific modal
├── hooks/
│   ├── useModalState.ts                 # URL-based modal state
│   └── useModelDetail.ts                # Fetch model detail (stub for now)
└── pages/
    └── HomePage.tsx                     # (update) Add ModelDetailModal
```

### Implementation Details

**useModalState Hook Pattern:**
```typescript
// hooks/useModalState.ts
import { useSearchParams, useNavigate } from 'react-router-dom';

export const useModalState = () => {
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();

  const modelId = searchParams.get('model');
  const isOpen = modelId !== null;

  const openModal = (id: string) => {
    const params = new URLSearchParams(searchParams);
    params.set('model', id);
    navigate(`?${params.toString()}`, { replace: false });
  };

  const closeModal = () => {
    const params = new URLSearchParams(searchParams);
    params.delete('model');
    navigate(`?${params.toString()}`, { replace: false });
  };

  return { modelId, isOpen, openModal, closeModal };
};
```

**Modal Component Structure:**
```typescript
// components/ui/Modal.tsx
interface ModalProps {
  isOpen: boolean;
  onClose: () => void;
  title?: string;
  children: React.ReactNode;
}

export const Modal = ({ isOpen, onClose, title, children }: ModalProps) => {
  // Escape key handler
  useEffect(() => {
    const handleEsc = (e: KeyboardEvent) => {
      if (e.key === 'Escape') onClose();
    };
    if (isOpen) {
      document.addEventListener('keydown', handleEsc);
      document.body.style.overflow = 'hidden';
    }
    return () => {
      document.removeEventListener('keydown', handleEsc);
      document.body.style.overflow = 'unset';
    };
  }, [isOpen, onClose]);

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center">
      {/* Backdrop */}
      <div className="absolute inset-0 bg-black/50" onClick={onClose} />

      {/* Modal Content */}
      <div className="relative bg-white rounded-lg shadow-xl max-w-4xl w-full max-h-[90vh] overflow-hidden">
        {/* Header */}
        <div className="flex items-center justify-between p-6 border-b">
          <h2 className="text-2xl font-bold">{title}</h2>
          <button onClick={onClose}>
            <XIcon className="w-6 h-6" />
          </button>
        </div>

        {/* Body */}
        <div className="overflow-y-auto max-h-[calc(90vh-80px)] p-6">
          {children}
        </div>
      </div>
    </div>
  );
};
```

**Integration in ModelsTable:**
```typescript
// components/models/ModelsTable.tsx
const { openModal } = useModalState();

// In table cell:
<td
  onClick={() => openModal(model.id)}
  className="cursor-pointer hover:underline text-blue-600"
>
  {model.name}
</td>
```

### References

- [Epic 4 Analysis: docs/epic-4-analysis-and-plan.md#Story 4.1]
- [ADR-015: URL-Based Modal State Management]
- [Epics Document: docs/epics.md#Story 4.1]
- [React Router useSearchParams Docs](https://reactrouter.com/en/main/hooks/use-search-params)

### Testing Strategy

**Unit Tests:**
- useModalState hook: URL param updates correctly on open/close
- Modal component: Renders when isOpen=true, hidden when false
- Escape key closes modal
- Backdrop click closes modal

**Integration Tests:**
- Clicking model name in table opens modal
- URL updates with ?model={id}
- Browser back button closes modal
- Modal shows correct model name in title

**Accessibility Tests:**
- Focus trap prevents tabbing outside modal
- Escape key works
- Screen reader announces modal open/close

## Dev Agent Record

### Context Reference

<!-- Path(s) to story context XML will be added here by context workflow -->

### Agent Model Used

claude-sonnet-4-5-20250929

### Debug Log References

### Completion Notes List

### File List
