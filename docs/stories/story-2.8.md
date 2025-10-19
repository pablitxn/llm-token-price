# Story 2.8: Create Delete Model Functionality

Status: ContextReadyDraft

## Story

As an administrator,
I want to delete models,
so that I can remove outdated or incorrect entries.

## Acceptance Criteria

1. Delete button triggers confirmation dialog
2. DELETE `/api/admin/models/{id}` endpoint created
3. Endpoint soft-deletes model (sets is_active=false) or hard-deletes
4. Associated capabilities and benchmark scores handled (cascade delete or set inactive)
5. Success refreshes models list with confirmation message

## Tasks / Subtasks

- [ ] **Task 1: Add delete confirmation dialog** (AC: #1)
  - [ ] 1.1: Create `ConfirmDialog.tsx` reusable component in `/frontend/src/components/ui`
  - [ ] 1.2: Add title, message, and action buttons (Cancel, Confirm)
  - [ ] 1.3: Trigger dialog when delete button clicked in ModelList
  - [ ] 1.4: Show warning message: "Are you sure you want to delete '{modelName}'? This action cannot be undone."
  - [ ] 1.5: Style confirm button as destructive (red)
  - [ ] 1.6: Close dialog on cancel or successful delete

- [ ] **Task 2: Implement delete button in models list** (AC: #1)
  - [ ] 2.1: Add onClick handler to delete button in ModelRow
  - [ ] 2.2: Store model ID and name in state for confirmation dialog
  - [ ] 2.3: Open confirmation dialog on click
  - [ ] 2.4: Call delete mutation on confirmation
  - [ ] 2.5: Show loading state on delete button during request

- [ ] **Task 3: Create DELETE endpoint** (AC: #2)
  - [ ] 3.1: Add DELETE action to AdminModelsController.cs
  - [ ] 3.2: Accept model ID from route parameter
  - [ ] 3.3: Add [Authorize] attribute for JWT authentication
  - [ ] 3.4: Call AdminModelService.DeleteModelAsync(id)
  - [ ] 3.5: Return 204 No Content on success
  - [ ] 3.6: Return 404 Not Found if model doesn't exist
  - [ ] 3.7: Add Swagger/OpenAPI documentation

- [ ] **Task 4: Decide soft-delete vs hard-delete strategy** (AC: #3, #4)
  - [ ] 4.1: Evaluate soft-delete (set is_active=false) vs hard-delete (remove from DB)
  - [ ] 4.2: **Recommendation:** Soft-delete for MVP (preserves audit trail)
  - [ ] 4.3: Document decision in architecture notes
  - [ ] 4.4: Add admin UI filter to show/hide inactive models (future enhancement)
  - [ ] 4.5: Hard-delete option reserved for admin superuser (post-MVP)

- [ ] **Task 5: Implement soft-delete service method** (AC: #3, #4)
  - [ ] 5.1: Create DeleteModelAsync(id) in AdminModelService
  - [ ] 5.2: Fetch model by ID
  - [ ] 5.3: Return 404 if model not found or already deleted
  - [ ] 5.4: Set model.IsActive = false
  - [ ] 5.5: Set model.UpdatedAt = DateTime.UtcNow
  - [ ] 5.6: Keep associated capabilities and benchmark scores (soft-delete cascade)
  - [ ] 5.7: Alternative: Set capabilities.IsActive = false if table has is_active column
  - [ ] 5.8: Save changes to database
  - [ ] 5.9: Invalidate cache (models list, model detail, best value)
  - [ ] 5.10: Create audit log entry

- [ ] **Task 6: Handle associated data** (AC: #4)
  - [ ] 6.1: Decide cascade behavior for ModelCapabilities
  - [ ] 6.2: **Option A:** Keep capabilities (soft-delete model only)
  - [ ] 6.3: **Option B:** Cascade soft-delete capabilities (set is_active=false)
  - [ ] 6.4: Decide behavior for BenchmarkScores
  - [ ] 6.5: **Recommendation:** Keep benchmark scores for historical data
  - [ ] 6.6: Update public API to exclude inactive models (WHERE is_active=true)
  - [ ] 6.7: Admin API can still query inactive models for recovery

- [ ] **Task 7: Implement frontend delete mutation** (AC: #5)
  - [ ] 7.1: Create useDeleteModel hook (TanStack Query mutation)
  - [ ] 7.2: Call DELETE /api/admin/models/{id}
  - [ ] 7.3: On success: invalidate models query cache
  - [ ] 7.4: On success: show toast "Model '{name}' deleted successfully"
  - [ ] 7.5: On success: close confirmation dialog
  - [ ] 7.6: On error: show error toast with message
  - [ ] 7.7: Handle 404 error (model not found or already deleted)

- [ ] **Task 8: Update models list to exclude deleted models** (AC: #5)
  - [ ] 8.1: Ensure public API GET /api/models filters WHERE is_active=true
  - [ ] 8.2: Admin API can optionally show inactive models with query param
  - [ ] 8.3: Add "Show Deleted" toggle in admin UI (future enhancement)
  - [ ] 8.4: Verify deleted models don't appear in public comparison table

- [ ] **Task 9: Add audit logging for delete** (AC: #3)
  - [ ] 9.1: Log action = "DELETE" (or "SOFT_DELETE")
  - [ ] 9.2: Log entity_type = "MODEL", entity_id = model ID
  - [ ] 9.3: Log changes_json with model state before deletion
  - [ ] 9.4: Log admin username from JWT claims
  - [ ] 9.5: Save to admin_audit_log table

- [ ] **Task 10: Add model recovery feature (optional enhancement)**
  - [ ] 10.1: Add "Recover" button in admin UI for soft-deleted models
  - [ ] 10.2: Create PUT /api/admin/models/{id}/recover endpoint
  - [ ] 10.3: Set is_active=true to restore model
  - [ ] 10.4: Note: Optional for MVP, can defer to post-launch

- [ ] **Task 11: Add testing**
  - [ ] 11.1: Write component tests for delete confirmation dialog (Vitest)
  - [ ] 11.2: Test dialog opens on delete button click
  - [ ] 11.3: Test cancel closes dialog without calling mutation
  - [ ] 11.4: Test confirm calls delete mutation
  - [ ] 11.5: Write unit tests for DeleteModelAsync service method
  - [ ] 11.6: Write integration tests for DELETE endpoint
  - [ ] 11.7: Test endpoint returns 204 on successful delete
  - [ ] 11.8: Test endpoint returns 404 for non-existent model
  - [ ] 11.9: Verify model.is_active set to false in database
  - [ ] 11.10: Verify deleted model excluded from public API
  - [ ] 11.11: Verify audit log entry created
  - [ ] 11.12: Verify cache invalidated

## Dev Notes

### Architecture Context

**Soft-Delete vs Hard-Delete:**
- **Soft-delete (Recommended for MVP):**
  - Set `is_active = false` instead of removing row
  - Preserves data for audit trail and recovery
  - Public API excludes inactive models (WHERE is_active=true)
  - Admin can view/recover deleted models (optional)
  - Benchmark scores and pricing history preserved
- **Hard-delete (Future consideration):**
  - Permanently removes model from database
  - Cascade deletes capabilities and benchmark scores
  - Used for test data or duplicate entries
  - Requires admin superuser permission

**Delete Flow:**
```
Delete button click
  → Confirmation dialog
  → User confirms
  → DELETE /api/admin/models/{id}
  → Set is_active=false
  → Update updated_at timestamp
  → Invalidate cache
  → Audit log
  → Return 204
  → Refresh models list
  → Show success toast
```

**Associated Data Handling:**
- ModelCapabilities: Keep (or set is_active=false if column exists)
- BenchmarkScores: Keep (historical data valuable)
- PricingHistory: Keep (Phase 2 feature)
- Cache: Invalidate all models-related cache keys

### Project Structure Notes

**Frontend Files to Create/Modify:**
```
/frontend/src/components/ui/
  └── ConfirmDialog.tsx (new)

/frontend/src/components/admin/
  └── ModelList.tsx (modify to add delete handler)

/frontend/src/hooks/
  └── useDeleteModel.ts (new mutation)

/frontend/src/api/
  └── admin.ts (add deleteModel function)
```

**Backend Files to Create/Modify:**
```
/backend/src/Backend.API/Controllers/Admin/
  └── AdminModelsController.cs (add DELETE action)

/backend/src/Backend.Application/Services/
  └── AdminModelService.cs (add DeleteModelAsync)

/backend/tests/Backend.API.Tests/
  └── AdminModelsControllerTests.cs (add DELETE tests)
```

### Implementation Details

**ConfirmDialog Component:**
```typescript
interface ConfirmDialogProps {
  open: boolean
  onClose: () => void
  onConfirm: () => void
  title: string
  message: string
  confirmText?: string
  loading?: boolean
}

export function ConfirmDialog({
  open,
  onClose,
  onConfirm,
  title,
  message,
  confirmText = 'Confirm',
  loading = false
}: ConfirmDialogProps) {
  return (
    <Dialog open={open} onClose={onClose}>
      <DialogHeader>{title}</DialogHeader>
      <DialogBody>{message}</DialogBody>
      <DialogFooter>
        <Button variant="secondary" onClick={onClose} disabled={loading}>
          Cancel
        </Button>
        <Button
          variant="destructive"
          onClick={onConfirm}
          loading={loading}
        >
          {confirmText}
        </Button>
      </DialogFooter>
    </Dialog>
  )
}
```

**Delete Handler in ModelList:**
```typescript
export function ModelList() {
  const [deleteDialog, setDeleteDialog] = useState<{ id: string, name: string } | null>(null)
  const deleteMutation = useDeleteModel()

  const handleDeleteClick = (model: Model) => {
    setDeleteDialog({ id: model.id, name: model.name })
  }

  const handleConfirmDelete = async () => {
    if (!deleteDialog) return

    await deleteMutation.mutateAsync(deleteDialog.id, {
      onSuccess: () => {
        toast.success(`Model '${deleteDialog.name}' deleted successfully`)
        setDeleteDialog(null)
      },
      onError: (error) => {
        toast.error(error.message || 'Failed to delete model')
      }
    })
  }

  return (
    <>
      <table>
        {/* ... table content */}
        <button onClick={() => handleDeleteClick(model)}>Delete</button>
      </table>

      <ConfirmDialog
        open={!!deleteDialog}
        onClose={() => setDeleteDialog(null)}
        onConfirm={handleConfirmDelete}
        title="Delete Model"
        message={`Are you sure you want to delete '${deleteDialog?.name}'? This action cannot be undone.`}
        confirmText="Delete"
        loading={deleteMutation.isPending}
      />
    </>
  )
}
```

**DELETE Endpoint:**
```csharp
[HttpDelete("{id}")]
[Authorize]
[ProducesResponseType(StatusCodes.Status204NoContent)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
public async Task<IActionResult> DeleteModel(Guid id)
{
    var result = await _adminModelService.DeleteModelAsync(id);

    if (!result)
        return NotFound(new { error = new { message = "Model not found" } });

    return NoContent();
}
```

**DeleteModelAsync Service:**
```csharp
public async Task<bool> DeleteModelAsync(Guid id)
{
    // 1. Fetch model
    var model = await _modelRepo.GetByIdAsync(id);
    if (model == null || !model.IsActive)
        return false;

    // 2. Capture before state for audit
    var beforeState = _mapper.Map<ModelResponseDto>(model);

    // 3. Soft-delete: set is_active = false
    model.IsActive = false;
    model.UpdatedAt = DateTime.UtcNow;

    // 4. Optional: soft-delete capabilities (if is_active column exists)
    // model.Capabilities.IsActive = false;

    // 5. Save changes
    await _modelRepo.SaveChangesAsync();

    // 6. Invalidate cache
    await _cache.RemoveAsync($"cache:model:{id}:v1");
    await _cache.RemovePatternAsync("cache:models:*");
    await _cache.RemovePatternAsync("cache:bestvalue:*");

    // 7. Audit log
    await _auditLog.LogAsync(new AuditLogEntry
    {
        AdminUser = GetCurrentUsername(),
        Action = "SOFT_DELETE",
        EntityType = "MODEL",
        EntityId = id,
        ChangesJson = JsonSerializer.Serialize(new { before = beforeState })
    });

    return true;
}
```

**Public API Filter (Existing GET /api/models):**
```csharp
// In ModelQueryService or ModelRepository
public async Task<List<Model>> GetActiveModelsAsync()
{
    return await _dbContext.Models
        .Where(m => m.IsActive == true)  // Exclude soft-deleted
        .Include(m => m.Capabilities)
        .ToListAsync();
}
```

**Recovery Endpoint (Optional):**
```csharp
[HttpPut("{id}/recover")]
[Authorize]
public async Task<IActionResult> RecoverModel(Guid id)
{
    var model = await _modelRepo.GetByIdAsync(id);
    if (model == null)
        return NotFound();

    model.IsActive = true;
    model.UpdatedAt = DateTime.UtcNow;
    await _modelRepo.SaveChangesAsync();

    await InvalidateCache();

    return Ok(new { message = "Model recovered successfully" });
}
```

### References

- [Tech Spec Epic 2: docs/tech-spec-epic-2-8-summary.md#Epic 2]
- [Solution Architecture: docs/solution-architecture.md#3.1 Database Schema - is_active column]
- [Epics Document: docs/epics.md#Story 2.8]
- [Soft Delete Pattern: Common in enterprise apps for audit/compliance]

### Testing Strategy

**Component Tests:**
- Delete button click opens confirmation dialog
- Dialog displays correct model name
- Cancel closes dialog without calling mutation
- Confirm calls delete mutation with correct ID
- Success shows toast and closes dialog
- Error shows error toast

**Unit Tests:**
- DeleteModelAsync sets is_active to false
- DeleteModelAsync updates updated_at timestamp
- DeleteModelAsync returns false for non-existent model
- DeleteModelAsync creates audit log entry

**Integration Tests:**
- DELETE /api/admin/models/{id} returns 204 on success
- Model.is_active set to false in database
- DELETE with non-existent ID returns 404
- Deleted model excluded from public GET /api/models
- Deleted model still accessible via admin GET (with filter)
- Cache invalidated after delete
- Audit log entry created with before state

**E2E Test:**
- Admin logs in → navigates to models → clicks delete → confirms → model removed from list → success toast shown

## Dev Agent Record

### Context Reference

- `docs/stories/story-context-2.8.xml` (Generated: 2025-10-19)

### Agent Model Used

claude-sonnet-4-5-20250929

### Debug Log References

### Completion Notes List

### File List
