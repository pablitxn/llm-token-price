# Story 2.11: Add Bulk Benchmark Import via CSV

Status: Ready

## Story

As an administrator,
I want to import multiple benchmark scores via CSV,
so that I can efficiently add data for new models.

## Acceptance Criteria

1. CSV upload form created on benchmarks page
2. CSV template documented (model_id, benchmark_name, score, test_date, source_url)
3. File upload processed in backend
4. CSV parsed and validated (check model/benchmark exist, scores valid)
5. Valid rows imported to database
6. Import results shown (X successful, Y failed with reasons)

## Tasks / Subtasks

- [ ] **Task 1: Create CSV upload UI** (AC: #1)
  - [ ] 1.1: Create `CSVImport.tsx` component in `/frontend/src/components/admin`
  - [ ] 1.2: Add file input for CSV upload (accept=".csv")
  - [ ] 1.3: Add drag-and-drop zone for file selection
  - [ ] 1.4: Show selected file name and size
  - [ ] 1.5: Add "Upload" button (disabled until file selected)
  - [ ] 1.6: Add loading spinner during upload/processing
  - [ ] 1.7: Integrate into AdminBenchmarksPage or dedicated import page

- [ ] **Task 2: Create CSV template and documentation** (AC: #2)
  - [ ] 2.1: Define CSV format: `model_id,benchmark_name,score,max_score,test_date,source_url,verified,notes`
  - [ ] 2.2: Create downloadable template CSV file with headers and example rows
  - [ ] 2.3: Add "Download Template" button in CSV import component
  - [ ] 2.4: Document each column: format, required/optional, examples
  - [ ] 2.5: Add validation rules documentation (e.g., date format: YYYY-MM-DD)
  - [ ] 2.6: Show example CSV in UI help text

- [ ] **Task 3: Implement file upload endpoint** (AC: #3)
  - [ ] 3.1: Create POST `/api/admin/benchmarks/import-csv` endpoint
  - [ ] 3.2: Accept multipart/form-data with file upload
  - [ ] 3.3: Add [Authorize] attribute for JWT authentication
  - [ ] 3.4: Limit file size (e.g., 10MB max)
  - [ ] 3.5: Validate file is CSV (check extension and content-type)
  - [ ] 3.6: Read file stream into memory or temp file
  - [ ] 3.7: Pass file to CSV processing service

- [ ] **Task 4: Create CSV parsing service** (AC: #4)
  - [ ] 4.1: Create `CSVImportService.cs` in `/Backend.Application/Services`
  - [ ] 4.2: Use CSV parsing library (CsvHelper or built-in)
  - [ ] 4.3: Parse CSV rows into `BenchmarkScoreImportRow` DTOs
  - [ ] 4.4: Skip header row
  - [ ] 4.5: Handle malformed rows gracefully (collect errors, don't crash)
  - [ ] 4.6: Trim whitespace from all fields
  - [ ] 4.7: Return list of parsed rows with row numbers for error reporting

- [ ] **Task 5: Implement row validation** (AC: #4)
  - [ ] 5.1: For each row, validate model_id is valid UUID and exists in database
  - [ ] 5.2: Validate benchmark_name exists (case-insensitive lookup)
  - [ ] 5.3: Validate score is valid decimal number
  - [ ] 5.4: Validate max_score is valid decimal if provided
  - [ ] 5.5: Validate test_date is valid date format (YYYY-MM-DD) if provided
  - [ ] 5.6: Validate source_url is valid URL if provided
  - [ ] 5.7: Collect validation errors per row with field and message
  - [ ] 5.8: Check for duplicate model+benchmark in same CSV (prevent redundant imports)

- [ ] **Task 6: Implement bulk import logic** (AC: #5)
  - [ ] 6.1: Separate valid rows from invalid rows
  - [ ] 6.2: For each valid row, create ModelBenchmarkScore entity
  - [ ] 6.3: Calculate normalized_score using BenchmarkNormalizer
  - [ ] 6.4: Check if score already exists (skip or update based on strategy)
  - [ ] 6.5: Batch insert all valid scores in single transaction
  - [ ] 6.6: Handle transaction failure (rollback all if any fails, or partial success)
  - [ ] 6.7: Invalidate cache after successful import
  - [ ] 6.8: Create audit log entry with import summary

- [ ] **Task 7: Implement import results response** (AC: #6)
  - [ ] 7.1: Create `CSVImportResultDto` with summary: totalRows, successCount, failureCount
  - [ ] 7.2: Include list of failed rows with row number, error message, and original data
  - [ ] 7.3: Include list of skipped rows (if duplicates)
  - [ ] 7.4: Return 200 OK even with partial failures (not 400)
  - [ ] 7.5: Return 400 only if file parsing completely fails

- [ ] **Task 8: Display import results in UI** (AC: #6)
  - [ ] 8.1: Create `ImportResults.tsx` component to display results
  - [ ] 8.2: Show success message: "X of Y rows imported successfully"
  - [ ] 8.3: Display failed rows in table: row number, error, data
  - [ ] 8.4: Add "Download Failed Rows" button to export failures as CSV
  - [ ] 8.5: Show skipped rows (duplicates) separately
  - [ ] 8.6: Add "Import Another File" button to reset form
  - [ ] 8.7: Highlight successful import count in green, failures in red

- [ ] **Task 9: Add import options**
  - [ ] 9.1: Add "Skip duplicates" vs "Update duplicates" radio button option
  - [ ] 9.2: Implement update logic if "Update duplicates" selected
  - [ ] 9.3: Add "Abort on first error" vs "Continue on error" option
  - [ ] 9.4: Default: Skip duplicates, Continue on error

- [ ] **Task 10: Add testing**
  - [ ] 10.1: Write unit tests for CSV parsing service
  - [ ] 10.2: Test valid CSV parses correctly
  - [ ] 10.3: Test malformed CSV handled gracefully
  - [ ] 10.4: Write unit tests for row validation
  - [ ] 10.5: Test validation catches invalid model_id, benchmark_name, scores
  - [ ] 10.6: Write integration tests for import endpoint
  - [ ] 10.7: Test successful import persists scores to database
  - [ ] 10.8: Test partial success (some valid, some invalid rows)
  - [ ] 10.9: Test duplicate handling (skip or update)
  - [ ] 10.10: Test file size limit enforcement
  - [ ] 10.11: Write component tests for CSVImport UI

## Dev Notes

### Architecture Context

**CSV Import Flow:**
```
1. Admin uploads CSV file
2. Backend receives multipart file
3. Parse CSV into rows
4. Validate each row (model exists, benchmark exists, score valid)
5. Separate valid vs invalid rows
6. Batch insert valid rows
7. Return results: success count, failed rows with errors
8. Admin reviews results, downloads failed rows to fix
9. Admin re-imports fixed rows
```

**Error Handling Strategy:**
- **Partial success:** Import valid rows even if some fail
- **Transaction:** All-or-nothing per valid subset (or allow partial with option)
- **Error reporting:** Detailed per-row errors for admin to fix
- **Failed rows export:** Download CSV of failures for correction

**Performance Considerations:**
- Batch insert for efficiency (avoid N individual inserts)
- Stream large CSV files (don't load entire file into memory)
- Limit file size (10MB = ~200K rows, more than enough)
- Async processing for very large imports (optional enhancement)

### Project Structure Notes

**Frontend Files to Create:**
```
/frontend/src/components/admin/
  ├── CSVImport.tsx
  ├── ImportResults.tsx
  └── CSVTemplateDownload.tsx

/frontend/src/hooks/
  └── useImportBenchmarkCSV.ts

/frontend/src/api/
  └── admin.ts (add importBenchmarkCSV function)
```

**Backend Files to Create:**
```
/backend/src/Backend.API/Controllers/Admin/
  └── AdminBenchmarksController.cs (add POST import endpoint)

/backend/src/Backend.Application/Services/
  └── CSVImportService.cs

/backend/src/Backend.Application/DTOs/
  ├── BenchmarkScoreImportRow.cs
  └── CSVImportResultDto.cs

/backend/src/Backend.Infrastructure/CSV/
  └── CSVParser.cs (optional abstraction)

/backend/tests/Backend.Application.Tests/
  └── CSVImportServiceTests.cs
```

**NuGet Package:**
```xml
<PackageReference Include="CsvHelper" Version="30.0.1" />
```

### Implementation Details

**CSV Template Format:**
```csv
model_id,benchmark_name,score,max_score,test_date,source_url,verified,notes
550e8400-e29b-41d4-a716-446655440000,MMLU,85.2,100,2025-10-01,https://example.com/results,true,Official benchmark
550e8400-e29b-41d4-a716-446655440000,HumanEval,0.72,1,2025-10-02,https://example.com/eval,false,
550e8400-e29b-41d4-a716-446655440001,GSM8K,78.5,100,2025-10-01,,true,Internal test
```

**BenchmarkScoreImportRow DTO:**
```csharp
public class BenchmarkScoreImportRow
{
    public int RowNumber { get; set; }
    public string ModelId { get; set; }
    public string BenchmarkName { get; set; }
    public string Score { get; set; }
    public string MaxScore { get; set; }
    public string TestDate { get; set; }
    public string SourceUrl { get; set; }
    public string Verified { get; set; }
    public string Notes { get; set; }
}
```

**CSVImportResultDto:**
```csharp
public class CSVImportResultDto
{
    public int TotalRows { get; set; }
    public int SuccessfulImports { get; set; }
    public int FailedImports { get; set; }
    public int SkippedDuplicates { get; set; }
    public List<FailedRow> Errors { get; set; } = new();
}

public class FailedRow
{
    public int RowNumber { get; set; }
    public string Error { get; set; }
    public Dictionary<string, string> Data { get; set; }
}
```

**CSVImportService:**
```csharp
public class CSVImportService
{
    private readonly IModelRepository _modelRepo;
    private readonly IBenchmarkRepository _benchmarkRepo;
    private readonly BenchmarkNormalizer _normalizer;

    public async Task<CSVImportResultDto> ImportBenchmarkScoresAsync(
        Stream fileStream,
        bool skipDuplicates = true)
    {
        var result = new CSVImportResultDto();
        var validRows = new List<ModelBenchmarkScore>();
        var errors = new List<FailedRow>();

        // 1. Parse CSV
        using var reader = new StreamReader(fileStream);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

        var rows = csv.GetRecords<BenchmarkScoreImportRow>().ToList();
        result.TotalRows = rows.Count;

        // 2. Validate and transform each row
        foreach (var row in rows.Select((r, i) => new { Row = r, Index = i + 2 })) // +2 for header + 0-index
        {
            var validationResult = await ValidateRow(row.Row, row.Index);

            if (!validationResult.IsValid)
            {
                errors.Add(new FailedRow
                {
                    RowNumber = row.Index,
                    Error = validationResult.Error,
                    Data = RowToDictionary(row.Row)
                });
                continue;
            }

            // Check for duplicate
            var isDuplicate = await _benchmarkRepo.ScoreExistsAsync(
                validationResult.ModelId,
                validationResult.BenchmarkId
            );

            if (isDuplicate)
            {
                if (skipDuplicates)
                {
                    result.SkippedDuplicates++;
                    continue;
                }
                // else: update logic here
            }

            // Calculate normalized score
            var normalizedScore = _normalizer.Normalize(
                validationResult.Score,
                validationResult.Benchmark.TypicalRangeMin,
                validationResult.Benchmark.TypicalRangeMax
            );

            // Create entity
            validRows.Add(new ModelBenchmarkScore
            {
                Id = Guid.NewGuid(),
                ModelId = validationResult.ModelId,
                BenchmarkId = validationResult.BenchmarkId,
                Score = validationResult.Score,
                MaxScore = validationResult.MaxScore,
                NormalizedScore = normalizedScore,
                TestDate = validationResult.TestDate ?? DateTime.UtcNow,
                SourceUrl = validationResult.SourceUrl,
                Verified = validationResult.Verified,
                Notes = validationResult.Notes,
                CreatedAt = DateTime.UtcNow
            });
        }

        // 3. Batch insert valid rows
        if (validRows.Any())
        {
            await _benchmarkRepo.BulkAddScoresAsync(validRows);
            await _benchmarkRepo.SaveChangesAsync();
            result.SuccessfulImports = validRows.Count;

            // Invalidate cache
            await InvalidateCache();
        }

        result.FailedImports = errors.Count;
        result.Errors = errors;

        return result;
    }

    private async Task<RowValidationResult> ValidateRow(BenchmarkScoreImportRow row, int rowNumber)
    {
        var result = new RowValidationResult { RowNumber = rowNumber };

        // Validate model_id
        if (!Guid.TryParse(row.ModelId, out var modelId))
        {
            result.Error = "Invalid model_id format (must be UUID)";
            return result;
        }

        var model = await _modelRepo.GetByIdAsync(modelId);
        if (model == null)
        {
            result.Error = $"Model not found: {row.ModelId}";
            return result;
        }
        result.ModelId = modelId;

        // Validate benchmark_name
        var benchmark = await _benchmarkRepo.GetByNameAsync(row.BenchmarkName);
        if (benchmark == null)
        {
            result.Error = $"Benchmark not found: {row.BenchmarkName}";
            return result;
        }
        result.Benchmark = benchmark;
        result.BenchmarkId = benchmark.Id;

        // Validate score
        if (!decimal.TryParse(row.Score, out var score))
        {
            result.Error = "Invalid score (must be a number)";
            return result;
        }
        result.Score = score;

        // Validate max_score (optional)
        if (!string.IsNullOrEmpty(row.MaxScore))
        {
            if (!decimal.TryParse(row.MaxScore, out var maxScore))
            {
                result.Error = "Invalid max_score (must be a number)";
                return result;
            }
            if (score > maxScore)
            {
                result.Error = "Score cannot exceed max_score";
                return result;
            }
            result.MaxScore = maxScore;
        }

        // Validate test_date (optional)
        if (!string.IsNullOrEmpty(row.TestDate))
        {
            if (!DateTime.TryParse(row.TestDate, out var testDate))
            {
                result.Error = "Invalid test_date format (use YYYY-MM-DD)";
                return result;
            }
            result.TestDate = testDate;
        }

        // Validate source_url (optional)
        if (!string.IsNullOrEmpty(row.SourceUrl))
        {
            if (!Uri.TryCreate(row.SourceUrl, UriKind.Absolute, out _))
            {
                result.Error = "Invalid source_url format";
                return result;
            }
            result.SourceUrl = row.SourceUrl;
        }

        // Parse verified (default false)
        result.Verified = bool.TryParse(row.Verified, out var verified) && verified;

        result.Notes = row.Notes;
        result.IsValid = true;
        return result;
    }
}

internal class RowValidationResult
{
    public int RowNumber { get; set; }
    public bool IsValid { get; set; }
    public string Error { get; set; }
    public Guid ModelId { get; set; }
    public Guid BenchmarkId { get; set; }
    public Benchmark Benchmark { get; set; }
    public decimal Score { get; set; }
    public decimal? MaxScore { get; set; }
    public DateTime? TestDate { get; set; }
    public string SourceUrl { get; set; }
    public bool Verified { get; set; }
    public string Notes { get; set; }
}
```

**POST Import Endpoint:**
```csharp
[HttpPost("import-csv")]
[Authorize]
[RequestSizeLimit(10_485_760)] // 10MB limit
[ProducesResponseType(typeof(CSVImportResultDto), StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
public async Task<IActionResult> ImportBenchmarkScoresCSV(IFormFile file)
{
    if (file == null || file.Length == 0)
        return BadRequest(new { error = "No file uploaded" });

    if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
        return BadRequest(new { error = "File must be CSV format" });

    using var stream = file.OpenReadStream();
    var result = await _csvImportService.ImportBenchmarkScoresAsync(stream);

    return Ok(new
    {
        data = result,
        meta = new
        {
            message = $"Import completed: {result.SuccessfulImports} successful, {result.FailedImports} failed"
        }
    });
}
```

**CSVImport Component:**
```typescript
export function CSVImport() {
  const [file, setFile] = useState<File | null>(null)
  const importMutation = useImportBenchmarkCSV()

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setFile(e.target.files?.[0] || null)
  }

  const handleUpload = () => {
    if (!file) return

    const formData = new FormData()
    formData.append('file', file)

    importMutation.mutate(formData, {
      onSuccess: (result) => {
        toast.success(`Imported ${result.successfulImports} scores`)
      }
    })
  }

  return (
    <div className="space-y-4">
      <div className="flex items-center gap-4">
        <input
          type="file"
          accept=".csv"
          onChange={handleFileChange}
          className="file-input"
        />
        <Button
          onClick={handleUpload}
          disabled={!file || importMutation.isPending}
          loading={importMutation.isPending}
        >
          Upload CSV
        </Button>
        <Button variant="secondary" onClick={() => downloadTemplate()}>
          Download Template
        </Button>
      </div>

      {file && <p className="text-sm">Selected: {file.name} ({formatBytes(file.size)})</p>}

      {importMutation.data && (
        <ImportResults result={importMutation.data} />
      )}
    </div>
  )
}
```

### References

- [Tech Spec Epic 2: docs/tech-spec-epic-2-8-summary.md#Epic 2]
- [Solution Architecture: docs/solution-architecture.md#7.2 Admin API Contracts]
- [Epics Document: docs/epics.md#Story 2.11]
- [CSV Format: Standard CSV with headers]

### Testing Strategy

**Unit Tests:**
- CSV parsing handles valid CSV correctly
- Parsing handles malformed CSV gracefully
- Row validation catches invalid model_id, benchmark_name, score
- Validation enforces score <= max_score
- Duplicate detection works correctly

**Integration Tests:**
- POST /api/admin/benchmarks/import-csv with valid CSV imports successfully
- Valid rows persisted to database
- Response includes success count and failed rows
- Partial success: some valid, some invalid rows
- Duplicate handling (skip or update)
- File size limit enforced
- Invalid file format returns 400

**Performance Tests:**
- Import 1000 rows completes in <10 seconds
- Import 10K rows completes in <60 seconds
- Memory usage stays under 500MB

**E2E Test:**
- Admin navigates to benchmarks → uploads CSV → sees import results → failed rows downloadable

## Dev Agent Record

### Context Reference

<!-- Path(s) to story context XML will be added here by context workflow -->

### Agent Model Used

claude-sonnet-4-5-20250929

### Debug Log References

### Completion Notes List

### File List
