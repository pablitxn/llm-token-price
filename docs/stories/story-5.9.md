# Story 5.9: Add Capabilities Comparison Matrix

Status: Draft

## Story

As a user,
I want a visual comparison of model capabilities,
so that I can see feature support at a glance and identify gaps in functionality.

## Acceptance Criteria

1. Capabilities matrix created as grid/table layout
2. Rows: capabilities (function calling, vision, audio, streaming, JSON mode, etc.)
3. Columns: selected models
4. Checkmark icon (✓) if model supports capability, X icon or empty if not
5. Color coding: green for supported, gray for not supported
6. Visual design makes it easy to spot capability gaps across models

## Tasks / Subtasks

### Task Group 1: Create CapabilitiesMatrix Component (AC: #1, #2, #3)
- [ ] Create component file: `apps/web/src/components/comparison/CapabilitiesMatrix.tsx`
  - [ ] Define `CapabilitiesMatrixProps` interface:
    ```typescript
    interface CapabilitiesMatrixProps {
      models: ModelDto[];
      className?: string;
    }
    ```
  - [ ] Create functional component with TypeScript
  - [ ] Export as named export
- [ ] Component layout structure
  - [ ] Section container: `<section className="mt-8 p-6 bg-white border border-gray-200 rounded-lg">`
  - [ ] Section heading: "Capabilities Comparison" (`text-2xl font-bold mb-6`)
  - [ ] Description: "Feature support across selected models" (`text-gray-600 mb-4`)
  - [ ] Table container with horizontal scroll: `<div className="overflow-x-auto">`
- [ ] Use semantic HTML table
  - [ ] `<table className="min-w-full border-collapse">`
  - [ ] `<thead>` for header row (model names)
  - [ ] `<tbody>` for capability rows
  - [ ] Table caption for accessibility: `<caption className="sr-only">Capabilities comparison matrix</caption>`

### Task Group 2: Define Capability List (AC: #2)
- [ ] Create capabilities configuration file: `apps/web/src/config/capabilities.ts`
  - [ ] Define capability metadata:
    ```typescript
    export interface CapabilityDefinition {
      id: string;
      label: string;
      description: string;
      category: 'core' | 'advanced' | 'integration';
      modelField: keyof ModelDto;  // Maps to ModelDto boolean field
    }

    export const CAPABILITIES: CapabilityDefinition[] = [
      {
        id: 'function-calling',
        label: 'Function Calling',
        description: 'Ability to call external functions and tools',
        category: 'core',
        modelField: 'supportsFunctionCalling',
      },
      {
        id: 'vision',
        label: 'Vision',
        description: 'Image understanding and analysis',
        category: 'core',
        modelField: 'supportsVision',
      },
      {
        id: 'audio',
        label: 'Audio',
        description: 'Audio input processing',
        category: 'advanced',
        modelField: 'supportsAudio',
      },
      {
        id: 'streaming',
        label: 'Streaming',
        description: 'Streaming response generation',
        category: 'core',
        modelField: 'supportsStreaming',
      },
      {
        id: 'json-mode',
        label: 'JSON Mode',
        description: 'Guaranteed JSON output format',
        category: 'integration',
        modelField: 'supportsJsonMode',
      },
    ];
    ```
  - [ ] Export capability list
  - [ ] Export helper: `getCapabilityValue(model: ModelDto, capability: CapabilityDefinition): boolean`
- [ ] Optional: Group capabilities by category
  - [ ] Core features: Function calling, Vision, Streaming
  - [ ] Advanced features: Audio, Video (future)
  - [ ] Integration features: JSON mode, API compatibility

### Task Group 3: Build Matrix Table Structure (AC: #2, #3)
- [ ] Create table header row (model names)
  - [ ] First cell: "Capability" label (column header)
  - [ ] Model cells: Model name + provider badge
    ```typescript
    <thead>
      <tr className="border-b border-gray-200">
        <th className="sticky left-0 bg-white px-6 py-4 text-left text-sm font-semibold text-gray-900 z-10">
          Capability
        </th>
        {models.map(model => (
          <th key={model.id} className="px-6 py-4 text-center text-sm font-semibold text-gray-900">
            <div>{model.name}</div>
            <div className="text-xs text-gray-500 font-normal">{model.provider}</div>
          </th>
        ))}
      </tr>
    </thead>
    ```
  - [ ] Sticky first column: `sticky left-0 bg-white z-10` (capability names stay visible on scroll)
  - [ ] Center-align model columns for icons
- [ ] Create capability rows
  - [ ] Iterate over CAPABILITIES array
  - [ ] For each capability, create row:
    ```typescript
    <tbody>
      {CAPABILITIES.map(capability => (
        <tr key={capability.id} className="border-b border-gray-200 even:bg-gray-50">
          <td className="sticky left-0 bg-white px-6 py-4 text-sm font-medium text-gray-900 z-10">
            <div className="flex items-center gap-2">
              <span>{capability.label}</span>
              <InfoTooltip text={capability.description} />
            </div>
          </td>
          {models.map(model => (
            <td key={model.id} className="px-6 py-4 text-center">
              <CapabilityCell
                supported={getCapabilityValue(model, capability)}
              />
            </td>
          ))}
        </tr>
      ))}
    </tbody>
    ```
  - [ ] Zebra striping: `even:bg-gray-50` for readability
  - [ ] Cell padding: `px-6 py-4`

### Task Group 4: Create CapabilityCell Component (AC: #4, #5)
- [ ] Create sub-component: `CapabilityCell.tsx` (or inline in CapabilitiesMatrix)
  - [ ] Props: `{ supported: boolean }`
  - [ ] Render checkmark or X icon based on support
    ```typescript
    export const CapabilityCell = ({ supported }: { supported: boolean }) => {
      if (supported) {
        return (
          <div className="flex items-center justify-center">
            <Check className="w-5 h-5 text-green-600" strokeWidth={2.5} />
            <span className="sr-only">Supported</span>
          </div>
        );
      }

      return (
        <div className="flex items-center justify-center">
          <X className="w-5 h-5 text-gray-400" strokeWidth={2} />
          <span className="sr-only">Not supported</span>
        </div>
      );
    };
    ```
  - [ ] Icons from lucide-react: `Check`, `X`
  - [ ] Checkmark: Green (#10b981 - green-600), bold stroke
  - [ ] X icon: Gray (#9ca3af - gray-400), thinner stroke
- [ ] Alternative: Use colored background cells
  - [ ] Supported: `bg-green-100 border border-green-500`
  - [ ] Not supported: `bg-gray-100 border border-gray-300`
  - [ ] Less visual clutter (no icons)
  - [ ] Recommendation: Use icons for clarity

### Task Group 5: Color Coding and Visual Design (AC: #5, #6)
- [ ] Define color scheme
  - [ ] Supported (yes): Green theme
    - Icon: `text-green-600` (#059669)
    - Background (optional): `bg-green-50`
    - Border (optional): `border-green-200`
  - [ ] Not supported (no): Gray theme
    - Icon: `text-gray-400` (#9ca3af)
    - Background (optional): `bg-gray-50`
    - Border (optional): `border-gray-200`
- [ ] Visual hierarchy
  - [ ] Capability labels: Bold, left-aligned
  - [ ] Icons: Centered in cells
  - [ ] Model names: Bold, centered
  - [ ] Provider names: Small, gray, centered below model name
- [ ] Hover effects
  - [ ] Row hover: `hover:bg-blue-50` (highlight entire row on hover)
  - [ ] Cell hover: Tooltip shows capability description
  - [ ] Transition: `transition-colors duration-150`

### Task Group 6: Tooltip for Capability Descriptions (AC: #6)
- [ ] Create InfoTooltip component
  - [ ] Small info icon next to capability label
  - [ ] Icon: Info from lucide-react (`w-4 h-4 text-gray-400`)
  - [ ] Hover shows tooltip with description
  - [ ] Example: "Function Calling - Ability to call external functions and tools"
- [ ] Implement tooltip using Headless UI or native title
  - [ ] Option A: Headless UI `<Popover>` for rich tooltip
  - [ ] Option B: Native `title` attribute (simpler, less control)
  - [ ] Recommendation: Native title for simplicity
    ```typescript
    <Info className="w-4 h-4 text-gray-400" title={capability.description} />
    ```
- [ ] Accessibility
  - [ ] Tooltip: `role="tooltip"`, `aria-describedby`
  - [ ] Info icon: `aria-label="More information"`

### Task Group 7: Sticky First Column for Scrolling (AC: #6)
- [ ] Implement sticky first column
  - [ ] CSS: `position: sticky; left: 0;` on first `<th>` and `<td>`
  - [ ] Background: `bg-white` to cover scrolling content
  - [ ] Z-index: `z-10` to stay above other cells
  - [ ] Box shadow (optional): `shadow-sm` to create depth when scrolling
- [ ] Horizontal scroll behavior
  - [ ] Container: `overflow-x-auto`
  - [ ] Table: `min-w-full` (content-driven width)
  - [ ] Scroll smoothly: `scroll-smooth`
- [ ] Test with 5+ models
  - [ ] 2-3 models: No scroll needed
  - [ ] 5+ models: Horizontal scroll, capability names stay visible

### Task Group 8: Category Grouping (Optional Enhancement) (AC: #2, #6)
- [ ] Group capabilities by category
  - [ ] Category headers: "Core Features", "Advanced Features", "Integration"
  - [ ] Section rows with different background:
    ```typescript
    <tr className="bg-gray-100">
      <td colSpan={models.length + 1} className="px-6 py-2 text-sm font-semibold text-gray-700">
        Core Features
      </td>
    </tr>
    ```
  - [ ] Capabilities under each category
- [ ] Collapsible categories (optional, Story 5.10 feature)
  - [ ] Similar to BenchmarkComparisonSection (Story 5.4)
  - [ ] Click category header to expand/collapse
  - [ ] Default: All expanded
- [ ] Skip if not needed (keep simple for MVP)

### Task Group 9: Handle Missing Capability Data (AC: #4)
- [ ] Check for undefined/null capability fields
  - [ ] If `model.supportsFunctionCalling === undefined`: Treat as false (not supported)
  - [ ] Display gray X icon (same as explicitly false)
- [ ] Display warning if many models missing data
  - [ ] Count models with undefined capabilities
  - [ ] If >50% missing: Show note "Some capability data unavailable"
  - [ ] Small gray text below table
- [ ] Partial data handling
  - [ ] Model has 3/5 capabilities defined: Show known values, treat undefined as false

### Task Group 10: Integrate into ComparisonPage (AC: #1)
- [ ] Update `ComparisonPage.tsx` to include CapabilitiesMatrix
  - [ ] Import component: `import { CapabilitiesMatrix } from '@/components/comparison/CapabilitiesMatrix';`
  - [ ] Add below PricingComparisonChart (Story 5.8)
  - [ ] Render: `<CapabilitiesMatrix models={data.models} />`
  - [ ] Placement: After pricing chart, before export/navigation section
- [ ] Section divider
  - [ ] Add horizontal rule: `<hr className="my-8 border-gray-200" />`
  - [ ] Or use spacing: `<div className="h-8" />`
- [ ] Loading state
  - [ ] Show skeleton while `isLoading`
  - [ ] Skeleton: Gray table with animate-pulse
  - [ ] Match table dimensions (5 rows × N columns)

### Task Group 11: Responsive Behavior (AC: #1, #6)
- [ ] Desktop layout (≥1024px)
  - [ ] Full table width
  - [ ] All columns visible (if ≤3 models)
  - [ ] Horizontal scroll if >3 models
- [ ] Tablet layout (768-1023px)
  - [ ] Horizontal scroll likely (multiple models)
  - [ ] Sticky first column works well
  - [ ] Same as desktop
- [ ] Mobile layout (<768px)
  - [ ] Horizontal scroll required
  - [ ] Consider card layout alternative:
    - Each model as card
    - Capabilities listed vertically
    - Checkmarks inline with labels
  - [ ] Or keep table with small font sizes
  - [ ] Recommendation: Keep table with horizontal scroll (simpler)
- [ ] Test scrolling behavior
  - [ ] Smooth scroll on touch devices
  - [ ] Scroll indicators visible (browser default)

### Task Group 12: Type Definitions (AC: #2, #4)
- [ ] Create types file: `apps/web/src/types/capabilities.ts`
  - [ ] Define `CapabilitiesMatrixProps` interface
  - [ ] Define `CapabilityDefinition` interface (already in config)
  - [ ] Define `CapabilityCellProps` interface:
    ```typescript
    export interface CapabilityCellProps {
      supported: boolean;
      capabilityName?: string;  // For tooltip
    }
    ```
- [ ] Update `ModelDto` type (verify fields exist)
  - [ ] Ensure boolean fields: `supportsFunctionCalling`, `supportsVision`, `supportsAudio`, `supportsStreaming`, `supportsJsonMode`
  - [ ] Add missing fields if needed (coordinate with backend)

### Task Group 13: Accessibility (AC: #1, #4, #6)
- [ ] Table accessibility
  - [ ] Semantic HTML: `<table>`, `<thead>`, `<tbody>`, `<th>`, `<td>`
  - [ ] Table caption: `<caption className="sr-only">Capabilities comparison matrix</caption>`
  - [ ] Scope attributes: `<th scope="col">` for model headers, `<th scope="row">` for capability labels
- [ ] Icon accessibility
  - [ ] Screen reader text: `<span className="sr-only">Supported</span>` or "Not supported"
  - [ ] ARIA label on cells: `aria-label="Function calling supported"` or "not supported"
- [ ] Color contrast
  - [ ] Green checkmark: WCAG AA contrast (4.5:1) against white
  - [ ] Gray X: Sufficient contrast against white or gray-50
  - [ ] Test with color blindness simulator
- [ ] Keyboard navigation
  - [ ] Table: Navigable with Tab key (browser default)
  - [ ] Info tooltips: Focusable with keyboard, show on focus

### Task Group 14: Performance Optimization (AC: #1)
- [ ] Memoize capability checks
  - [ ] Use `useMemo` for capability matrix data:
    ```typescript
    const capabilityMatrix = useMemo(
      () => CAPABILITIES.map(cap => ({
        ...cap,
        values: models.map(m => getCapabilityValue(m, cap)),
      })),
      [models]
    );
    ```
  - [ ] Prevents recalculating on every render
- [ ] Optimize component re-renders
  - [ ] Use `React.memo()` on CapabilitiesMatrix
  - [ ] Use `React.memo()` on CapabilityCell
  - [ ] Prevent re-render when unrelated state changes
- [ ] Test performance
  - [ ] Matrix with 5 models × 8 capabilities = 40 cells
  - [ ] Renders in <100ms (very fast)

### Task Group 15: Testing and Verification (AC: #1-6)
- [ ] Write unit test for `getCapabilityValue` utility
  - [ ] Test: Model with `supportsFunctionCalling: true` returns true
  - [ ] Test: Model with `supportsFunctionCalling: false` returns false
  - [ ] Test: Model with `supportsFunctionCalling: undefined` returns false
  - [ ] Use Vitest
- [ ] Write integration test for CapabilitiesMatrix component
  - [ ] Render with 3 models, 5 capabilities
  - [ ] Verify table has 5 rows (capabilities) + 1 header row
  - [ ] Verify table has 3 model columns + 1 capability column
  - [ ] Verify checkmark icons for supported capabilities
  - [ ] Verify X icons for unsupported capabilities
  - [ ] Use Vitest + React Testing Library
- [ ] Manual E2E testing
  - [ ] Navigate to `/compare?models=1,2,3`
  - [ ] Scroll to "Capabilities Comparison" section
  - [ ] Verify table with 5 capabilities (rows) × 3 models (columns)
  - [ ] Verify first column (capability names) sticky on horizontal scroll
  - [ ] Verify checkmarks (✓) for supported features (green)
  - [ ] Verify X icons for unsupported features (gray)
  - [ ] Hover over capability label: Tooltip shows description
  - [ ] Hover over row: Background changes to blue-50
  - [ ] Test responsive: Resize window, verify horizontal scroll works
  - [ ] Test mobile (<768px): Table scrolls horizontally, first column stays visible

## Dev Notes

### Architecture Alignment
- **Component Location**: `apps/web/src/components/comparison/CapabilitiesMatrix.tsx` (comparison-specific)
- **Configuration**: Capability definitions in `config/capabilities.ts` (centralized, easy to extend)
- **Data Access**: Direct read from ModelDto boolean fields (no transformation needed)
- **No Backend Changes**: Uses existing ModelDto fields
- **Reusability**: CapabilityCell can be reused in ModelCard (Story 5.2) or other components

### Matrix vs. Cards Layout
**Matrix (Table) Approach** (selected):
- ✅ Scannable: Easy to compare capabilities across all models at once
- ✅ Compact: Fits more information in less space
- ✅ Standard pattern: Familiar to users
- ❌ Horizontal scroll needed for many models

**Card Approach** (alternative):
- ✅ Mobile-friendly: No horizontal scroll
- ✅ Vertical layout: Natural for small screens
- ❌ Less scannable: Hard to compare across models
- ❌ More scrolling: Longer page

Recommendation: Matrix for desktop/tablet, consider card fallback for mobile (future enhancement).

### Sticky First Column Pattern
CSS for sticky capability names:
```css
th:first-child,
td:first-child {
  position: sticky;
  left: 0;
  background-color: white;
  z-index: 10;
  box-shadow: 2px 0 4px rgba(0,0,0,0.05);  /* Depth effect */
}
```

Benefits:
- Capability names always visible during horizontal scroll
- User always knows which row they're looking at
- Better UX for wide tables (5+ models)

### Capability Field Mapping
Maps UI labels to ModelDto fields:
```typescript
'Function Calling' → model.supportsFunctionCalling
'Vision'           → model.supportsVision
'Audio'            → model.supportsAudio
'Streaming'        → model.supportsStreaming
'JSON Mode'        → model.supportsJsonMode
```

This mapping defined in `CAPABILITIES` config, easy to extend with new capabilities.

### Icon Choice Rationale
**Checkmark (✓) for Yes:**
- Universal symbol for affirmative/supported
- Green color reinforces positive meaning
- Bold stroke (2.5px) for visibility

**X for No:**
- Clear negative indicator
- Gray color de-emphasizes (less important than supported features)
- Thinner stroke (2px) to avoid visual clutter

**Alternative considered: Empty cell for No**
- Pros: Less clutter, cleaner look
- Cons: Ambiguous (missing data vs not supported?)
- Rejected: X icon clearer

### Color Scheme
**Green for Supported:**
- Green universally means "go", "yes", "active"
- Consistent with "cheapest" indicator (Story 5.8)
- Accessible: Sufficient contrast against white

**Gray for Not Supported:**
- Gray means "inactive", "unavailable"
- De-emphasizes missing features
- Avoids negative connotation (red too harsh)

**Avoiding Red:**
- Red implies error or problem
- Missing capability is not an error, just unavailable
- Gray more neutral

### Tooltip Pattern
Native HTML title attribute:
```html
<Info className="w-4 h-4 text-gray-400" title="Ability to call external functions and tools" />
```

Pros: Simple, no dependencies, built-in accessibility
Cons: Limited styling, browser-dependent appearance

Alternative: Headless UI Popover (richer, custom-styled tooltips)

### Category Grouping (Optional)
Example with categories:
```
┌─────────────────────┬────────┬────────┬────────┐
│ Capability          │ GPT-4  │ Claude │ Gemini │
├─────────────────────┼────────┼────────┼────────┤
│ CORE FEATURES                                  │ ← Category header
├─────────────────────┼────────┼────────┼────────┤
│ Function Calling    │   ✓    │   ✓    │   ✗    │
│ Vision              │   ✓    │   ✓    │   ✓    │
│ Streaming           │   ✓    │   ✓    │   ✓    │
├─────────────────────┼────────┼────────┼────────┤
│ ADVANCED FEATURES                              │
├─────────────────────┼────────┼────────┼────────┤
│ Audio               │   ✗    │   ✗    │   ✓    │
└─────────────────────┴────────┴────────┴────────┘
```

Benefits: Logical grouping, easier to scan
Skip for MVP (keep simple).

### Responsive Strategy
**Desktop (≥1024px):**
- Full table width
- All columns visible (if 2-3 models)
- Horizontal scroll (if 4+ models)
- Sticky first column

**Tablet (768-1023px):**
- Same as desktop
- Horizontal scroll likely

**Mobile (<768px):**
- Horizontal scroll required
- Sticky first column critical
- Small font sizes (10-11px)
- Consider alternative layout in future (Story 5.14)

### Prerequisites
- **Story 5.4**: Establishes table pattern for comparison (BenchmarkComparisonSection)
- **Story 5.2**: ModelCard already displays some capabilities (can reuse CapabilityCell)
- **ModelDto**: Must include capability boolean fields (verify with backend)
- No new dependencies required

### Quality Gates
- TypeScript strict mode: ✅ Zero `any` types
- Matrix renders: ✅ Table with capabilities × models grid
- Icons: ✅ Green checkmarks, gray X icons
- Color coding: ✅ Visual distinction supported vs not supported
- Sticky column: ✅ First column stays visible on horizontal scroll
- Responsive: ✅ Horizontal scroll works on all screen sizes
- Accessibility: ✅ Semantic table, ARIA labels, screen reader support
- Performance: ✅ Renders in <100ms with 5 models × 8 capabilities

### Project Structure Notes
```
apps/web/src/
├── components/
│   └── comparison/
│       ├── CapabilitiesMatrix.tsx         # New component (this story)
│       ├── CapabilityCell.tsx             # New sub-component (this story)
│       └── ComparisonTable.tsx            # From Story 5.3
├── config/
│   └── capabilities.ts                    # New config (this story)
│       ├── CAPABILITIES array
│       └── getCapabilityValue()
└── types/
    └── capabilities.ts                    # New types (this story)
```

### Performance Considerations
- Matrix with 5 models × 8 capabilities = 40 cells
- Each cell renders icon (Check or X): Very fast (<1ms per cell)
- Total render time: <100ms
- No chart rendering (faster than Chart.js components)
- Sticky positioning: No performance impact (CSS-only)

### Data Flow
```
ComparisonPage (useComparisonData)
  → data.models (ModelDto[])
    → CapabilitiesMatrix component
      → CAPABILITIES config (capability definitions)
        → For each capability:
          → For each model:
            → getCapabilityValue(model, capability)
              → model.supportsFunctionCalling (boolean)
                → CapabilityCell component
                  → Check icon (green) or X icon (gray)
```

### References
- [Source: docs/tech-spec-epic-5.md#Services and Modules] - CapabilitiesMatrix component spec
- [Source: docs/tech-spec-epic-5.md#Acceptance Criteria] - AC-5.9: Capabilities matrix requirements
- [Source: docs/epics.md#Story 5.9] - Original story with 6 acceptance criteria
- [Source: docs/stories/story-5.4.md] - Table pattern reference (BenchmarkComparisonSection)
- [Source: docs/stories/story-5.3.md] - ComparisonTable sticky column pattern
- [Source: docs/solution-architecture.md#Frontend Components] - Component organization

## Dev Agent Record

### Context Reference

<!-- Path(s) to story context XML will be added here by context workflow -->

### Agent Model Used

claude-sonnet-4-5-20250929

### Debug Log References

### Completion Notes List

### File List
