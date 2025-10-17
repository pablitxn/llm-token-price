# Story 2.12: Add Timestamp Tracking and Display

Status: Ready

## Story

As an administrator,
I want to see when models were last updated,
so that I know data freshness.

## Acceptance Criteria

1. Models list table displays "Last Updated" column with formatted timestamp
2. Models with updates >7 days ago highlighted or flagged
3. Admin dashboard shows count of models needing updates
4. Public API includes pricing_updated_at timestamp
5. Frontend displays "Updated X days ago" on model cards

## Tasks / Subtasks

- [ ] **Task 1: Ensure timestamp fields in database** (AC: #4)
  - [ ] 1.1: Verify `created_at` and `updated_at` columns exist on models table (from Story 1.4)
  - [ ] 1.2: Add `pricing_updated_at` column to models table for tracking pricing changes specifically
  - [ ] 1.3: Create migration to add pricing_updated_at if needed
  - [ ] 1.4: Set default: pricing_updated_at = updated_at for existing models
  - [ ] 1.5: Update model entity to include PricingUpdatedAt property

- [ ] **Task 2: Update timestamps on model modifications** (AC: #4)
  - [ ] 2.1: Set updated_at = DateTime.UtcNow on every model update (already in Story 2.7)
  - [ ] 2.2: Set pricing_updated_at = DateTime.UtcNow when pricing fields change
  - [ ] 2.3: Detect pricing changes: compare old vs new inputPricePer1M and outputPricePer1M
  - [ ] 2.4: Keep updated_at for general updates, pricing_updated_at for pricing-specific
  - [ ] 2.5: Set created_at on model creation (already in Story 2.5)

- [ ] **Task 3: Add "Last Updated" column to admin models list** (AC: #1)
  - [ ] 3.1: Add "Last Updated" column to ModelList table (from Story 2.3)
  - [ ] 3.2: Display updated_at timestamp with relative formatting ("2 days ago")
  - [ ] 3.3: Use date-fns library for relative time formatting
  - [ ] 3.4: Show exact timestamp on hover (tooltip)
  - [ ] 3.5: Make column sortable (most recent first by default)
  - [ ] 3.6: Add column for pricing_updated_at (optional, or combine with general updated_at)

- [ ] **Task 4: Highlight stale models** (AC: #2)
  - [ ] 4.1: Calculate days since last update: `(now - updated_at).days`
  - [ ] 4.2: Apply visual indicator for models >7 days old:
    - [ ] 4.2a: Yellow/orange background or icon
    - [ ] 4.2b: Warning badge with "Stale" label
  - [ ] 4.3: Apply stronger indicator for models >30 days old (critical)
  - [ ] 4.4: Add filter: "Show stale models only" (>7 days)
  - [ ] 4.5: Sort stale models to top of list (optional)

- [ ] **Task 5: Create admin dashboard with data freshness metrics** (AC: #3)
  - [ ] 5.1: Update AdminDashboard component (placeholder from Story 2.2)
  - [ ] 5.2: Add metric card: "Models Needing Updates" (count where updated_at >7 days ago)
  - [ ] 5.3: Add metric card: "Critical Updates" (count where updated_at >30 days ago)
  - [ ] 5.4: Add metric card: "Recently Updated" (count where updated_at <7 days)
  - [ ] 5.5: Add link to "View Stale Models" filtered list
  - [ ] 5.6: Fetch metrics from GET `/api/admin/dashboard/metrics` endpoint

- [ ] **Task 6: Create dashboard metrics endpoint** (AC: #3)
  - [ ] 6.1: Create GET `/api/admin/dashboard/metrics` endpoint
  - [ ] 6.2: Calculate counts:
    - [ ] 6.2a: Total active models
    - [ ] 6.2b: Models >7 days since update
    - [ ] 6.2c: Models >30 days since update
    - [ ] 6.2d: Models updated in last 7 days
  - [ ] 6.3: Add pricing-specific metrics:
    - [ ] 6.3a: Models with pricing >30 days old
    - [ ] 6.3b: Models with no pricing_updated_at (never updated)
  - [ ] 6.4: Cache metrics (5 min TTL) for performance
  - [ ] 6.5: Add [Authorize] attribute

- [ ] **Task 7: Include timestamps in public API** (AC: #4)
  - [ ] 7.1: Add updated_at to GET `/api/models` response (list endpoint)
  - [ ] 7.2: Add pricing_updated_at to model detail response
  - [ ] 7.3: Add created_at to model detail response (optional)
  - [ ] 7.4: Format timestamps as ISO 8601 strings in JSON
  - [ ] 7.5: Update ModelDto and ModelDetailDto to include timestamp fields

- [ ] **Task 8: Display relative timestamps in public frontend** (AC: #5)
  - [ ] 8.1: Show "Updated X days ago" on model cards in comparison table
  - [ ] 8.2: Use date-fns `formatDistanceToNow()` function
  - [ ] 8.3: Show exact timestamp on hover (tooltip)
  - [ ] 8.4: Add freshness indicator icon:
    - [ ] 8.4a: Green checkmark for <7 days
    - [ ] 8.4b: Yellow clock for 7-30 days
    - [ ] 8.4c: Red warning for >30 days
  - [ ] 8.5: Display in model detail modal (pricing tab)

- [ ] **Task 9: Add timestamp utilities and formatting**
  - [ ] 9.1: Create `formatters.ts` utility file (if not exists)
  - [ ] 9.2: Add `formatRelativeTime(date)` function using date-fns
  - [ ] 9.3: Add `formatTimestamp(date)` for absolute formatting
  - [ ] 9.4: Add `getDaysSince(date)` helper for age calculation
  - [ ] 9.5: Add `getFreshnessStatus(date)` returning 'fresh' | 'stale' | 'critical'
  - [ ] 9.6: Export reusable components: `<RelativeTime date={...} />`, `<FreshnessIndicator date={...} />`

- [ ] **Task 10: Add testing**
  - [ ] 10.1: Write unit tests for timestamp formatting utilities
  - [ ] 10.2: Test formatRelativeTime shows correct values ("2 days ago", "1 month ago")
  - [ ] 10.3: Test getFreshnessStatus categorizes correctly
  - [ ] 10.4: Write component tests for RelativeTime component
  - [ ] 10.5: Write integration tests for dashboard metrics endpoint
  - [ ] 10.6: Test metrics return correct counts
  - [ ] 10.7: Test updated_at included in public API responses
  - [ ] 10.8: Test pricing_updated_at only changes when pricing changes
  - [ ] 10.9: Test stale model highlighting in admin UI

## Dev Notes

### Architecture Context

**Timestamp Strategy:**
- `created_at`: Set once on model creation, never changes
- `updated_at`: Updated on ANY model field change (general freshness)
- `pricing_updated_at`: Updated only when input/output prices change (pricing freshness)

**Why Separate Pricing Timestamp?**
- Pricing is critical metric for comparison platform
- Model metadata (version, description) less critical than pricing
- Users care specifically about pricing freshness
- Enables pricing-specific alerts: "Pricing last updated 45 days ago"

**Freshness Categories:**
```
Fresh: < 7 days (green) - Recently verified
Stale: 7-30 days (yellow) - Needs review soon
Critical: > 30 days (red) - Urgent update needed
```

**Use Cases:**
- Admin sees stale models → prioritizes updates
- Public users see stale pricing → know to verify
- Automated alerts: Email admin for models >30 days (future)

### Project Structure Notes

**Database Migration:**
```sql
ALTER TABLE models ADD COLUMN pricing_updated_at TIMESTAMP DEFAULT NOW();

-- Backfill existing models
UPDATE models SET pricing_updated_at = updated_at WHERE pricing_updated_at IS NULL;
```

**Backend Files to Modify:**
```
/backend/src/Backend.Domain/Entities/
  └── Model.cs (add PricingUpdatedAt property)

/backend/src/Backend.Infrastructure/Data/Migrations/
  └── [timestamp]_AddPricingUpdatedAt.cs (new migration)

/backend/src/Backend.Application/Services/
  └── AdminModelService.cs (update pricing_updated_at on price changes)

/backend/src/Backend.API/Controllers/Admin/
  ├── AdminModelsController.cs (return timestamps)
  └── AdminDashboardController.cs (new, add metrics endpoint)

/backend/src/Backend.Application/DTOs/
  ├── ModelDto.cs (add updated_at)
  ├── ModelDetailDto.cs (add pricing_updated_at)
  └── DashboardMetricsDto.cs (new)
```

**Frontend Files to Create/Modify:**
```
/frontend/src/components/admin/
  ├── ModelList.tsx (add Last Updated column)
  └── AdminDashboard.tsx (add metrics cards)

/frontend/src/components/ui/
  ├── RelativeTime.tsx (reusable component)
  └── FreshnessIndicator.tsx (icon + label)

/frontend/src/utils/
  └── formatters.ts (timestamp utilities)

/frontend/src/pages/admin/
  └── AdminDashboardPage.tsx (implement dashboard)

/frontend/src/hooks/
  └── useDashboardMetrics.ts (fetch metrics)
```

### Implementation Details

**Model Entity Update:**
```csharp
public class Model
{
    // ... existing properties

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? PricingUpdatedAt { get; set; }  // NEW
}
```

**Update Service Logic:**
```csharp
public async Task<ModelResponseDto> UpdateModelAsync(Guid id, UpdateModelDto dto)
{
    var model = await _modelRepo.GetByIdAsync(id);
    if (model == null) return null;

    // Check if pricing changed
    bool pricingChanged = model.InputPricePer1M != dto.InputPricePer1M ||
                          model.OutputPricePer1M != dto.OutputPricePer1M;

    // Update fields
    model.InputPricePer1M = dto.InputPricePer1M;
    model.OutputPricePer1M = dto.OutputPricePer1M;
    // ... other fields

    model.UpdatedAt = DateTime.UtcNow;

    if (pricingChanged)
    {
        model.PricingUpdatedAt = DateTime.UtcNow;
    }

    await _modelRepo.SaveChangesAsync();
    // ... invalidate cache, audit log
}
```

**Dashboard Metrics DTO:**
```csharp
public class DashboardMetricsDto
{
    public int TotalActiveModels { get; set; }
    public int ModelsNeedingUpdates { get; set; }      // >7 days
    public int CriticalUpdates { get; set; }           // >30 days
    public int RecentlyUpdated { get; set; }           // <7 days
    public int PricingNeedingUpdates { get; set; }     // pricing >30 days
    public DateTime CalculatedAt { get; set; }
}
```

**Dashboard Metrics Endpoint:**
```csharp
[HttpGet("metrics")]
[Authorize]
[ResponseCache(Duration = 300)] // 5 min cache
public async Task<IActionResult> GetDashboardMetrics()
{
    var now = DateTime.UtcNow;
    var sevenDaysAgo = now.AddDays(-7);
    var thirtyDaysAgo = now.AddDays(-30);

    var models = await _modelRepo.GetAllAsync(includeInactive: false);

    var metrics = new DashboardMetricsDto
    {
        TotalActiveModels = models.Count,
        ModelsNeedingUpdates = models.Count(m => m.UpdatedAt < sevenDaysAgo),
        CriticalUpdates = models.Count(m => m.UpdatedAt < thirtyDaysAgo),
        RecentlyUpdated = models.Count(m => m.UpdatedAt >= sevenDaysAgo),
        PricingNeedingUpdates = models.Count(m =>
            m.PricingUpdatedAt.HasValue && m.PricingUpdatedAt < thirtyDaysAgo),
        CalculatedAt = now
    };

    return Ok(new { data = metrics });
}
```

**Timestamp Formatting Utilities:**
```typescript
import { formatDistanceToNow, formatDistance } from 'date-fns'

export function formatRelativeTime(date: string | Date): string {
  return formatDistanceToNow(new Date(date), { addSuffix: true })
  // Returns: "2 days ago", "3 months ago"
}

export function formatTimestamp(date: string | Date): string {
  return new Date(date).toLocaleDateString('en-US', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  })
  // Returns: "Oct 16, 2025, 2:30 PM"
}

export function getDaysSince(date: string | Date): number {
  const now = new Date()
  const past = new Date(date)
  const diffMs = now.getTime() - past.getTime()
  return Math.floor(diffMs / (1000 * 60 * 60 * 24))
}

export function getFreshnessStatus(date: string | Date): 'fresh' | 'stale' | 'critical' {
  const days = getDaysSince(date)
  if (days <= 7) return 'fresh'
  if (days <= 30) return 'stale'
  return 'critical'
}
```

**RelativeTime Component:**
```typescript
interface RelativeTimeProps {
  date: string | Date
  showIcon?: boolean
}

export function RelativeTime({ date, showIcon = false }: RelativeTimeProps) {
  const relativeTime = formatRelativeTime(date)
  const absoluteTime = formatTimestamp(date)
  const status = getFreshnessStatus(date)

  const icon = {
    fresh: <CheckCircle className="text-green-500" size={16} />,
    stale: <Clock className="text-yellow-500" size={16} />,
    critical: <AlertTriangle className="text-red-500" size={16} />
  }[status]

  return (
    <Tooltip content={absoluteTime}>
      <span className="flex items-center gap-1">
        {showIcon && icon}
        <span className={cn(
          status === 'fresh' && 'text-green-700',
          status === 'stale' && 'text-yellow-700',
          status === 'critical' && 'text-red-700'
        )}>
          {relativeTime}
        </span>
      </span>
    </Tooltip>
  )
}
```

**Admin Dashboard Component:**
```typescript
export function AdminDashboard() {
  const { data: metrics, isLoading } = useDashboardMetrics()

  if (isLoading) return <LoadingSpinner />

  return (
    <div className="grid grid-cols-4 gap-6">
      <MetricCard
        title="Total Models"
        value={metrics.totalActiveModels}
        icon={Database}
      />
      <MetricCard
        title="Needs Update"
        value={metrics.modelsNeedingUpdates}
        icon={Clock}
        status="warning"
        link="/admin/models?filter=stale"
      />
      <MetricCard
        title="Critical"
        value={metrics.criticalUpdates}
        icon={AlertTriangle}
        status="error"
        link="/admin/models?filter=critical"
      />
      <MetricCard
        title="Recently Updated"
        value={metrics.recentlyUpdated}
        icon={CheckCircle}
        status="success"
      />
    </div>
  )
}
```

**ModelList with Last Updated Column:**
```typescript
const columns = [
  { key: 'name', label: 'Model Name', sortable: true },
  { key: 'provider', label: 'Provider', sortable: true },
  { key: 'inputPricePer1M', label: 'Input Price', sortable: true },
  { key: 'outputPricePer1M', label: 'Output Price', sortable: true },
  { key: 'status', label: 'Status', sortable: true },
  { key: 'updatedAt', label: 'Last Updated', sortable: true, render: (model) => (
    <RelativeTime date={model.updatedAt} showIcon />
  ) },
  { key: 'actions', label: 'Actions', sortable: false }
]
```

### References

- [Tech Spec Epic 2: docs/tech-spec-epic-2-8-summary.md#Epic 2]
- [Solution Architecture: docs/solution-architecture.md#3.1 Database Schema - timestamp columns]
- [Epics Document: docs/epics.md#Story 2.12]
- [Data Freshness: docs/solution-architecture.md - Epic 7 references]

### Testing Strategy

**Unit Tests:**
- formatRelativeTime returns correct values for various dates
- getDaysSince calculates correct day count
- getFreshnessStatus categorizes correctly (fresh/stale/critical)
- Dashboard metrics calculation correct

**Component Tests:**
- RelativeTime component displays relative time
- RelativeTime tooltip shows absolute time
- FreshnessIndicator shows correct icon based on date
- AdminDashboard displays metrics correctly

**Integration Tests:**
- GET /api/admin/dashboard/metrics returns correct counts
- Metrics cached for 5 minutes
- UpdateModelAsync sets updated_at timestamp
- UpdateModelAsync sets pricing_updated_at only on price change
- Public API includes updated_at in response
- Models list shows Last Updated column

**Manual Testing:**
- Stale models highlighted in admin list
- Dashboard metrics link to filtered list
- Public model cards show relative time
- Tooltips display absolute timestamps

## Dev Agent Record

### Context Reference

<!-- Path(s) to story context XML will be added here by context workflow -->

### Agent Model Used

claude-sonnet-4-5-20250929

### Debug Log References

### Completion Notes List

### File List
