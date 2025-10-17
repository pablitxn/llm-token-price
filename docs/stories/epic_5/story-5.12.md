# Story 5.12: Add Export Comparison Feature

Status: Draft

## Story

As a user,
I want to export comparison data in various formats,
so that I can share findings with my team or save for future reference.

## Acceptance Criteria

1. "Export" button displayed on comparison page (top-right or bottom of page)
2. Export to CSV: All model data, pricing, capabilities, and benchmark scores in tabular format
3. Export chart as PNG: Capture current chart visualization as image (optional for MVP)
4. File downloads with descriptive filename: `comparison-YYYY-MM-DD-HHMMSS.{ext}`
5. Success toast notification confirms export completion

## Tasks / Subtasks

### Task Group 1: Create ExportButton Component (AC: #1)
- [ ] Create component file: `apps/web/src/components/comparison/ExportButton.tsx`
  - [ ] Define `ExportButtonProps` interface:
    ```typescript
    interface ExportButtonProps {
      comparisonData: ComparisonExportData;
      className?: string;
    }

    interface ComparisonExportData {
      models: ModelDto[];
      exportDate: string;
      selectedBenchmarks?: string[];
    }
    ```
  - [ ] Create functional component with TypeScript
  - [ ] Export as named export
- [ ] Component layout structure
  - [ ] Dropdown button with export options
  - [ ] Primary button: "Export" with Download icon
  - [ ] Dropdown menu: "Export as CSV", "Export Chart as PNG"
  - [ ] Example:
    ```typescript
    <div className="relative">
      <button
        onClick={() => setIsOpen(!isOpen)}
        className="px-4 py-2 bg-blue-500 text-white rounded-lg hover:bg-blue-600 flex items-center gap-2"
      >
        <Download className="w-4 h-4" />
        Export
        <ChevronDown className="w-4 h-4" />
      </button>

      {isOpen && (
        <div className="absolute right-0 mt-2 w-48 bg-white border border-gray-200 rounded-lg shadow-lg z-20">
          <button onClick={handleExportCSV} className="w-full px-4 py-2 text-left hover:bg-gray-50">
            <FileText className="w-4 h-4 inline mr-2" />
            Export as CSV
          </button>
          <button onClick={handleExportPNG} className="w-full px-4 py-2 text-left hover:bg-gray-50">
            <Image className="w-4 h-4 inline mr-2" />
            Export Chart as PNG
          </button>
        </div>
      )}
    </div>
    ```
  - [ ] Icons from lucide-react: `Download`, `ChevronDown`, `FileText`, `Image`

### Task Group 2: CSV Export Functionality (AC: #2, #4)
- [ ] Create CSV export utility: `apps/web/src/utils/exportCSV.ts`
  - [ ] Function: `exportComparisonToCSV(data: ComparisonExportData): void`
  - [ ] Build CSV content from comparison data
  - [ ] Trigger browser download
- [ ] CSV Structure:
  ```csv
  Model,Provider,Input Price,Output Price,Context Window,Max Output,Function Calling,Vision,Audio,Streaming,JSON Mode,MMLU,HumanEval,GSM8K,...
  GPT-4 Turbo,OpenAI,10.00,30.00,128000,4096,Yes,Yes,No,Yes,Yes,86.4,67.0,92.0,...
  Claude 3 Opus,Anthropic,15.00,75.00,200000,4096,Yes,Yes,No,Yes,Yes,86.8,84.9,95.0,...
  Gemini Pro,Google,7.00,21.00,32000,2048,No,Yes,Yes,Yes,No,79.1,74.4,86.5,...
  ```
- [ ] CSV generation algorithm:
  ```typescript
  export const exportComparisonToCSV = (data: ComparisonExportData) => {
    // 1. Define columns
    const columns = [
      'Model',
      'Provider',
      'Input Price ($/1M)',
      'Output Price ($/1M)',
      'Context Window',
      'Max Output',
      'Function Calling',
      'Vision',
      'Audio',
      'Streaming',
      'JSON Mode',
      ...data.models[0].benchmarkScores.map(b => b.benchmarkName),  // Dynamic benchmarks
    ];

    // 2. Build header row
    const header = columns.join(',');

    // 3. Build data rows
    const rows = data.models.map(model => {
      return [
        model.name,
        model.provider,
        model.inputPricePer1M.toFixed(2),
        model.outputPricePer1M.toFixed(2),
        model.contextWindow,
        model.maxOutputTokens,
        model.supportsFunctionCalling ? 'Yes' : 'No',
        model.supportsVision ? 'Yes' : 'No',
        model.supportsAudio ? 'Yes' : 'No',
        model.supportsStreaming ? 'Yes' : 'No',
        model.supportsJsonMode ? 'Yes' : 'No',
        ...model.benchmarkScores.map(b => b.score?.toFixed(1) || 'N/A'),
      ].join(',');
    });

    // 4. Combine header + rows
    const csvContent = [header, ...rows].join('\n');

    // 5. Trigger download
    downloadCSV(csvContent, generateFilename('csv'));
  };
  ```

### Task Group 3: CSV Download Helper (AC: #4)
- [ ] Create download utility: `apps/web/src/utils/downloadFile.ts`
  - [ ] Function: `downloadCSV(content: string, filename: string): void`
  - [ ] Implementation:
    ```typescript
    export const downloadCSV = (content: string, filename: string) => {
      // Create Blob with CSV content
      const blob = new Blob([content], { type: 'text/csv;charset=utf-8;' });

      // Create download link
      const link = document.createElement('a');
      const url = URL.createObjectURL(blob);

      link.setAttribute('href', url);
      link.setAttribute('download', filename);
      link.style.visibility = 'hidden';

      // Trigger download
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);

      // Clean up
      URL.revokeObjectURL(url);
    };
    ```
  - [ ] Works in all modern browsers (Chrome, Firefox, Safari, Edge)
- [ ] Generate filename with timestamp
  - [ ] Function: `generateFilename(extension: string): string`
  - [ ] Format: `comparison-YYYY-MM-DD-HHMMSS.{ext}`
  - [ ] Example: `comparison-2025-01-17-143022.csv`
  - [ ] Implementation:
    ```typescript
    export const generateFilename = (extension: string): string => {
      const now = new Date();
      const timestamp = now.toISOString().replace(/[:.]/g, '-').slice(0, -5);  // Remove milliseconds + Z
      return `comparison-${timestamp}.${extension}`;
    };
    ```

### Task Group 4: PNG Chart Export (Optional for MVP) (AC: #3, #4)
- [ ] Export chart as PNG image
  - [ ] Chart.js provides `chart.toBase64Image()` method
  - [ ] Returns PNG data URL
  - [ ] Trigger download with generated filename
- [ ] Create PNG export utility: `apps/web/src/utils/exportChart.ts`
  - [ ] Function: `exportChartAsPNG(chartRef: ChartJS, filename: string): void`
  - [ ] Implementation:
    ```typescript
    import type { ChartJS } from 'chart.js';

    export const exportChartAsPNG = (chartRef: ChartJS, filename: string) => {
      // Get chart as base64 PNG
      const base64Image = chartRef.toBase64Image();

      // Convert to downloadable link
      const link = document.createElement('a');
      link.href = base64Image;
      link.download = filename;

      // Trigger download
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
    };
    ```
  - [ ] Access chart instance: Use `ref` on BaseChart component
- [ ] Add ref to BaseChart component
  - [ ] Update BaseChart: Forward ref to Chart.js instance
  - [ ] Use `useRef` in parent: `const chartRef = useRef<ChartJS>(null);`
  - [ ] Pass ref to BaseChart: `<BaseChart ref={chartRef} ... />`
  - [ ] Export on button click: `exportChartAsPNG(chartRef.current, 'benchmark-chart-2025-01-17.png')`

### Task Group 5: Handle Missing Data in CSV (AC: #2)
- [ ] Handle null/undefined values
  - [ ] Benchmark scores: `score?.toFixed(1) || 'N/A'`
  - [ ] Prices: `price?.toFixed(2) || '0.00'`
  - [ ] Booleans: `value ? 'Yes' : 'No'` (treat undefined as No)
- [ ] Handle CSV special characters
  - [ ] Commas in values: Wrap in quotes `"Claude 3, Opus"`
  - [ ] Quotes in values: Escape with double quotes `"Claude ""Pro"""`
  - [ ] Newlines: Replace with space or escaped `\n`
  - [ ] Helper function:
    ```typescript
    const escapeCSVValue = (value: string | number | boolean): string => {
      const str = String(value);
      if (str.includes(',') || str.includes('"') || str.includes('\n')) {
        return `"${str.replace(/"/g, '""')}"`;  // Wrap in quotes, escape existing quotes
      }
      return str;
    };
    ```

### Task Group 6: Success Toast Notification (AC: #5)
- [ ] Create toast notification system (or use existing library)
  - [ ] Option A: Use react-hot-toast library
    - Install: `pnpm add react-hot-toast`
    - Simple API: `toast.success('Exported successfully!')`
  - [ ] Option B: Custom toast component
    - Create `Toast.tsx` component
    - State management with Zustand or React Context
  - [ ] Recommendation: Use react-hot-toast (simple, battle-tested)
- [ ] Show success toast on export
  - [ ] After CSV download: `toast.success('Comparison exported as CSV')`
  - [ ] After PNG download: `toast.success('Chart exported as PNG')`
  - [ ] Toast duration: 3 seconds
  - [ ] Toast position: Top-right corner
- [ ] Show error toast on failure
  - [ ] If export fails: `toast.error('Export failed. Please try again.')`
  - [ ] Log error to console for debugging

### Task Group 7: Integrate ExportButton into ComparisonPage (AC: #1)
- [ ] Update `ComparisonPage.tsx` to include ExportButton
  - [ ] Import: `import { ExportButton } from '@/components/comparison/ExportButton';`
  - [ ] Position: Top-right of page (header area) or bottom of comparison section
  - [ ] Placement options:
    - **Option A**: Page header next to "Back to Table" link
    - **Option B**: Floating bottom-right corner (sticky)
    - **Option C**: Below CapabilitiesMatrix (last section)
  - [ ] Recommendation: Option A (top-right, easy to find)
- [ ] Pass comparison data to ExportButton
  - [ ] Data: `{ models: data.models, exportDate: new Date().toISOString() }`
  - [ ] Selected benchmarks: From MetricSelector state (optional filter)
- [ ] Layout structure:
  ```typescript
  <div className="flex items-center justify-between mb-6">
    <button onClick={handleBack} className="text-blue-500 hover:underline">
      ← Back to Models
    </button>
    <ExportButton comparisonData={{ models: data.models, exportDate: new Date().toISOString() }} />
  </div>
  ```

### Task Group 8: Export Button States (AC: #1, #5)
- [ ] Loading state during export
  - [ ] Button disabled: `disabled={isExporting}`
  - [ ] Loading spinner: `{isExporting ? <Loader2 className="animate-spin" /> : <Download />}`
  - [ ] Text: "Exporting..." vs "Export"
  - [ ] Prevent multiple clicks during export
- [ ] Disabled state if no data
  - [ ] If `models.length === 0`: Disable button
  - [ ] Tooltip: "Select models to export"
  - [ ] Gray out button: `opacity-50 cursor-not-allowed`
- [ ] Error state
  - [ ] If export fails: Show error toast
  - [ ] Re-enable button
  - [ ] Allow retry

### Task Group 9: CSV Column Selection (Optional Enhancement) (AC: #2)
- [ ] Allow users to choose which columns to export
  - [ ] Checkbox list: "Include Pricing", "Include Capabilities", "Include Benchmarks"
  - [ ] Default: All selected
  - [ ] Dynamically build CSV columns based on selection
- [ ] Advanced export dialog (out of MVP scope)
  - [ ] Modal with export options
  - [ ] Column selection checkboxes
  - [ ] Format selection: CSV, JSON, Excel
  - [ ] Not implemented in MVP (keep simple)

### Task Group 10: Export All vs Export Visible (AC: #2)
- [ ] Export all comparison data
  - [ ] Includes all models on page (2-5 models)
  - [ ] Includes all benchmarks (not just selected in chart)
  - [ ] Complete data export
- [ ] Alternative: Export only visible chart data
  - [ ] Only selected benchmarks (from MetricSelector)
  - [ ] Only visible datasets (if legends hidden)
  - [ ] Smaller file, less complete
  - [ ] Not recommended (users expect full data)
- [ ] Recommendation: Export all data (comprehensive)

### Task Group 11: Type Definitions (AC: #2)
- [ ] Update `apps/web/src/types/export.ts`
  - [ ] Define `ExportButtonProps` interface
  - [ ] Define `ComparisonExportData` interface:
    ```typescript
    export interface ComparisonExportData {
      models: ModelDto[];
      exportDate: string;
      selectedBenchmarks?: string[];
      exportedBy?: string;  // Future: User name
    }
    ```
  - [ ] Define `ExportFormat` type: `'csv' | 'png' | 'json'` (future)
- [ ] Export utility function types
  - [ ] `exportComparisonToCSV: (data: ComparisonExportData) => void`
  - [ ] `exportChartAsPNG: (chartRef: ChartJS, filename: string) => void`
  - [ ] `downloadCSV: (content: string, filename: string) => void`

### Task Group 12: Accessibility (AC: #1, #5)
- [ ] Export button accessibility
  - [ ] ARIA: `aria-label="Export comparison data"`, `aria-haspopup="menu"`, `aria-expanded={isOpen}`
  - [ ] Keyboard: Enter/Space to open dropdown, Arrow keys to navigate options
  - [ ] Focus management: Focus first menu item on open
- [ ] Dropdown menu accessibility
  - [ ] ARIA: `role="menu"`, menu items `role="menuitem"`
  - [ ] Keyboard: Arrow up/down to navigate, Enter to select, Escape to close
  - [ ] Focus trap: Keep focus within menu when open
- [ ] Toast accessibility
  - [ ] ARIA: `role="status"`, `aria-live="polite"` (screen reader announces)
  - [ ] Visual indicator: Icon + text + color
  - [ ] Dismissible: Close button or auto-dismiss after 3s

### Task Group 13: Performance Optimization (AC: #2)
- [ ] CSV generation performance
  - [ ] Synchronous CSV building: <50ms for 5 models × 20 columns
  - [ ] No performance concern (small dataset)
- [ ] Large dataset handling (future)
  - [ ] If >100 models: Use Web Worker for CSV generation
  - [ ] If >1MB file: Stream to download (chunked)
  - [ ] Not needed for MVP (max 5 models)
- [ ] PNG export performance
  - [ ] Chart.js `toBase64Image()`: ~100-200ms (synchronous)
  - [ ] High-resolution export: Increase chart resolution before export
  - [ ] Default resolution sufficient for MVP

### Task Group 14: Testing and Verification (AC: #1-5)
- [ ] Write unit test for `exportComparisonToCSV`
  - [ ] Test with 3 models, full data
  - [ ] Verify CSV structure (header + rows)
  - [ ] Verify special character escaping (commas, quotes)
  - [ ] Verify N/A for missing scores
  - [ ] Use Vitest
- [ ] Write unit test for `generateFilename`
  - [ ] Test timestamp format: `comparison-YYYY-MM-DD-HHMMSS.csv`
  - [ ] Verify unique filename (timestamp changes)
  - [ ] Use Vitest
- [ ] Write integration test for ExportButton component
  - [ ] Render with comparison data
  - [ ] Click "Export" button: Dropdown opens
  - [ ] Click "Export as CSV": Verify `downloadCSV` called
  - [ ] Verify success toast appears
  - [ ] Use Vitest + React Testing Library
- [ ] Manual E2E testing
  - [ ] Navigate to `/compare?models=1,2,3`
  - [ ] Click "Export" button: Dropdown opens with 2 options
  - [ ] Click "Export as CSV":
    - File downloads: `comparison-2025-01-17-143022.csv`
    - Toast appears: "Comparison exported as CSV"
    - Open CSV: Verify 3 rows (models) + header
    - Verify columns: Model, Provider, Prices, Capabilities, Benchmarks
    - Verify data accuracy (matches comparison page)
  - [ ] Click "Export Chart as PNG":
    - File downloads: `benchmark-chart-2025-01-17-143022.png`
    - Toast appears: "Chart exported as PNG"
    - Open PNG: Verify chart image (grouped bar chart)
  - [ ] Test with 0 models selected: Button disabled
  - [ ] Test with missing data: CSV shows "N/A" correctly

## Dev Notes

### Architecture Alignment
- **Utility Functions**: CSV generation in `utils/exportCSV.ts` (pure function, testable)
- **Download Helper**: Browser download logic in `utils/downloadFile.ts` (reusable)
- **Component**: ExportButton manages UI only (delegates to utilities)
- **No Backend Changes**: Client-side export only (no server processing)
- **Future Enhancement**: Server-side export for PDF, Excel (out of MVP scope)

### CSV vs JSON vs Excel

**CSV (Implemented):**
- ✅ Universal format (opens in Excel, Google Sheets, text editors)
- ✅ Lightweight (small file size)
- ✅ Easy to generate (string manipulation)
- ❌ No formatting (plain text)
- ❌ No formulas or charts

**JSON (Future):**
- ✅ Structured data (easy to parse)
- ✅ Preserves data types
- ❌ Not human-readable in plain text editor
- ❌ Requires parser to view

**Excel (Future):**
- ✅ Rich formatting (colors, bold, borders)
- ✅ Supports formulas and charts
- ❌ Complex to generate (requires library)
- ❌ Larger file size

MVP: CSV only (simple, universal). Future: Add Excel with library (e.g., xlsx.js).

### CSV Special Character Escaping

CSV format requires escaping special characters:

**Commas:**
- Problem: Commas separate columns
- Solution: Wrap value in quotes: `"Claude 3, Opus"`

**Quotes:**
- Problem: Quotes denote string boundaries
- Solution: Escape with double quotes: `"He said ""Hello"""`

**Newlines:**
- Problem: Newlines separate rows
- Solution: Replace with space or `\n` within quotes: `"Line 1\nLine 2"`

Implementation:
```typescript
const escapeCSV = (value: any): string => {
  const str = String(value);
  if (str.includes(',') || str.includes('"') || str.includes('\n')) {
    return `"${str.replace(/"/g, '""')}"`;
  }
  return str;
};
```

### PNG Export Quality

Chart.js `toBase64Image()` exports at canvas resolution:
- Default: 1x device pixel ratio (e.g., 800x400px)
- High-DPI: 2x device pixel ratio (Retina displays)
- Custom: Increase chart size before export

For better quality:
```typescript
// Temporarily increase chart size
chartRef.current.resize(1600, 800);  // Double resolution
const png = chartRef.current.toBase64Image();
chartRef.current.resize(800, 400);  // Restore original size
```

Not needed for MVP (default resolution sufficient).

### Filename Format Rationale

Format: `comparison-YYYY-MM-DD-HHMMSS.{ext}`

Examples:
- `comparison-2025-01-17-143022.csv`
- `benchmark-chart-2025-01-17-143530.png`

Benefits:
- **Sortable**: Alphabetical = chronological
- **Unique**: Timestamp to second precision (unlikely collision)
- **Descriptive**: "comparison" prefix identifies file type
- **ISO-like**: YYYY-MM-DD is internationally recognized

Alternative formats considered:
- Timestamp only: `1705501822.csv` (not human-readable)
- Model names: `gpt4-claude-gemini.csv` (too long, special characters)
- Short date: `comparison-2025-01-17.csv` (collisions if multiple exports per day)

### Toast Notification Library Comparison

**react-hot-toast (Recommended):**
- ✅ Simple API: `toast.success('Message')`
- ✅ Small bundle size (~4KB)
- ✅ Customizable (position, duration, styling)
- ✅ Accessible (ARIA attributes built-in)
- Install: `pnpm add react-hot-toast`

**react-toastify:**
- ✅ Feature-rich (progress bars, icons)
- ❌ Larger bundle size (~10KB)
- ❌ More complex API

**Custom toast:**
- ✅ Full control over styling
- ❌ More code to maintain
- ❌ Need to implement accessibility manually

Recommendation: react-hot-toast (simple, lightweight).

### Export Button Positioning Options

**Option A: Page Header (Top-Right) - Recommended**
```
┌─────────────────────────────────────────────┐
│ ← Back to Models              [Export ▼]   │
│                                             │
│ Model Comparison                            │
└─────────────────────────────────────────────┘
```
Pros: Easy to find, consistent with common UI patterns
Cons: Requires scrolling up if at bottom of page

**Option B: Floating Bottom-Right**
```
┌─────────────────────────────────────────────┐
│                                             │
│ Comparison Content                          │
│                                             │
│                                   [Export]  │ ← Sticky
└─────────────────────────────────────────────┘
```
Pros: Always visible (sticky)
Cons: May block content on small screens

**Option C: Bottom of Page**
```
┌─────────────────────────────────────────────┐
│ Capabilities Matrix                         │
│ ───────────────────────────────────────────│
│                               [Export ▼]    │
└─────────────────────────────────────────────┘
```
Pros: Logically after viewing all data
Cons: Requires scrolling down (not discoverable)

Recommendation: Option A (top-right, standard pattern).

### Prerequisites
- **Story 5.10**: ComparisonPage layout finalized (integration point)
- **Story 5.3**: ComparisonTable provides data structure reference
- **Story 5.6**: BenchmarkBarChart provides chart ref for PNG export
- **ModelDto**: Complete data structure with all export fields
- No new dependencies required for CSV (optional: react-hot-toast)

### Quality Gates
- TypeScript strict mode: ✅ Zero `any` types
- CSV export: ✅ Downloads file with complete data
- Filename: ✅ Timestamp format correct
- Special characters: ✅ Commas, quotes escaped properly
- PNG export: ✅ Chart image downloads (if implemented)
- Toast notification: ✅ Success message appears
- Button disabled: ✅ When no models selected
- Accessibility: ✅ ARIA labels, keyboard navigation
- Performance: ✅ Export completes in <100ms

### Project Structure Notes
```
apps/web/src/
├── components/
│   └── comparison/
│       ├── ExportButton.tsx               # New component (this story)
│       └── ComparisonPage.tsx             # Updated: Add ExportButton
├── utils/
│   ├── exportCSV.ts                       # New utility (this story)
│   ├── exportChart.ts                     # New utility (this story)
│   └── downloadFile.ts                    # New utility (this story)
│       ├── downloadCSV()
│       └── generateFilename()
└── types/
    └── export.ts                          # New types (this story)
```

### Performance Considerations
- CSV generation: <50ms for 5 models × 20 columns (synchronous, fast)
- PNG export: ~100-200ms (Chart.js toBase64Image)
- File download: Instant (browser handles)
- No performance concerns for MVP data size

### Data Flow
```
User clicks "Export as CSV"
  → ExportButton.handleExportCSV()
    → exportComparisonToCSV(comparisonData)
      → Build CSV header (column names)
        → Build CSV rows (model data)
          → escapeCSVValue() for special characters
            → Join rows with newlines
              → downloadCSV(csvContent, filename)
                → Create Blob
                  → Create download link
                    → Trigger browser download
                      → File saves: "comparison-2025-01-17-143022.csv"
  → Toast: "Comparison exported as CSV" (3s)
```

### References
- [Source: docs/tech-spec-epic-5.md#Services and Modules] - Export functionality specification
- [Source: docs/tech-spec-epic-5.md#Acceptance Criteria] - AC-5.12: Export feature requirements
- [Source: docs/epics.md#Story 5.12] - Original story with 5 acceptance criteria
- [Source: docs/stories/story-5.3.md] - ComparisonTable (data structure reference)
- [Source: docs/stories/story-5.6.md] - BenchmarkBarChart (chart ref for PNG export)

## Dev Agent Record

### Context Reference

<!-- Path(s) to story context XML will be added here by context workflow -->

### Agent Model Used

claude-sonnet-4-5-20250929

### Debug Log References

### Completion Notes List

### File List
