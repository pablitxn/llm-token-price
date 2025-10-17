# Story 2.7: Create Edit Model Functionality

Status: Ready

## Story

As an administrator,
I want to edit existing models,
so that I can update pricing or correct information.

## Acceptance Criteria

1. Edit model form loads existing model data
2. Form pre-populated with all current values (basic info, pricing, capabilities)
3. PUT `/api/admin/models/{id}` endpoint created
4. Endpoint updates model and capabilities in database
5. Updated_at timestamp refreshed on save
6. Success redirects to models list with confirmation

## Tasks / Subtasks

- [ ] **Task 1: Create edit model page** (AC: #1, #2)
  - [ ] 1.1: Create `EditModelPage.tsx` in `/frontend/src/pages/admin`
  - [ ] 1.2: Extract model ID from route params (`/admin/models/:id/edit`)
  - [ ] 1.3: Fetch model data using `useQuery` with model ID
  - [ ] 1.4: Show loading spinner while fetching
  - [ ] 1.5: Show error message if model not found (404)
  - [ ] 1.6: Reuse `ModelForm.tsx` component from Story 2.4
  - [ ] 1.7: Pass mode prop: 'create' vs 'edit' to ModelForm

- [ ] **Task 2: Pre-populate form with existing data** (AC: #2)
  - [ ] 2.1: Pass fetched model data to ModelForm as defaultValues
  - [ ] 2.2: Use React Hook Form's reset() or defaultValues prop
  - [ ] 2.3: Pre-populate basic info fields (name, provider, version, etc.)
  - [ ] 2.4: Pre-populate pricing fields
  - [ ] 2.5: Pre-populate capabilities fields from Story 2.6
  - [ ] 2.6: Ensure date fields converted from ISO strings to date picker format
  - [ ] 2.7: Verify all fields display existing values correctly

- [ ] **Task 3: Update ModelForm to support edit mode** (AC: #2)
  - [ ] 3.1: Add mode prop to ModelForm ('create' | 'edit')
  - [ ] 3.2: Change page title: "Add New Model" vs "Edit Model: {name}"
  - [ ] 3.3: Change submit button text: "Create Model" vs "Save Changes"
  - [ ] 3.4: Use different mutation: createModel vs updateModel
  - [ ] 3.5: Pass model ID to update mutation
  - [ ] 3.6: Disable name and provider fields in edit mode (prevent changing unique key)

- [ ] **Task 4: Create PUT endpoint** (AC: #3)
  - [ ] 4.1: Add PUT action to AdminModelsController.cs
  - [ ] 4.2: Accept `UpdateModelDto` from request body and model ID from route
  - [ ] 4.3: Add [Authorize] attribute for JWT authentication
  - [ ] 4.4: Call AdminModelService.UpdateModelAsync()
  - [ ] 4.5: Return 200 OK with updated model data
  - [ ] 4.6: Return 404 Not Found if model doesn't exist
  - [ ] 4.7: Add Swagger/OpenAPI documentation

- [ ] **Task 5: Implement update service method** (AC: #4, #5)
  - [ ] 5.1: Create UpdateModelAsync(id, UpdateModelDto) in AdminModelService
  - [ ] 5.2: Fetch existing model by ID
  - [ ] 5.3: Return 404 if model not found
  - [ ] 5.4: Update model properties from DTO
  - [ ] 5.5: Set updated_at = DateTime.UtcNow
  - [ ] 5.6: Update associated ModelCapabilities entity
  - [ ] 5.7: Save changes in single transaction
  - [ ] 5.8: Invalidate cache (models list, model detail, best value)
  - [ ] 5.9: Create audit log entry with before/after values

- [ ] **Task 6: Create UpdateModelDto and validator** (AC: #3)
  - [ ] 6.1: Create UpdateModelDto.cs (similar to CreateModelDto)
  - [ ] 6.2: Optionally exclude name/provider (immutable in edit)
  - [ ] 6.3: Create UpdateModelValidator with same rules as CreateModelValidator
  - [ ] 6.4: Add validation for ID existence
  - [ ] 6.5: Register validator in DI

- [ ] **Task 7: Implement frontend update mutation** (AC: #6)
  - [ ] 7.1: Create useUpdateModel hook (TanStack Query mutation)
  - [ ] 7.2: Call PUT /api/admin/models/{id} with form data
  - [ ] 7.3: On success: invalidate models query cache
  - [ ] 7.4: On success: show toast "Model '{name}' updated successfully"
  - [ ] 7.5: On success: navigate to /admin/models
  - [ ] 7.6: On error: display validation messages in form
  - [ ] 7.7: Show loading state on submit button during request

- [ ] **Task 8: Add optimistic updates (optional enhancement)**
  - [ ] 8.1: Update TanStack Query cache optimistically before server response
  - [ ] 8.2: Rollback cache if update fails
  - [ ] 8.3: Show immediate feedback to admin
  - [ ] 8.4: Note: Optional for MVP, can defer to post-launch

- [ ] **Task 9: Add audit logging for updates** (AC: #4)
  - [ ] 9.1: Capture model state before update
  - [ ] 9.2: Capture model state after update
  - [ ] 9.3: Log changes_json with before/after diff
  - [ ] 9.4: Include only changed fields in diff
  - [ ] 9.5: Log action = "UPDATE", entity_type = "MODEL"
  - [ ] 9.6: Save to admin_audit_log table

- [ ] **Task 10: Add testing**
  - [ ] 10.1: Write component tests for EditModelPage (Vitest)
  - [ ] 10.2: Test form pre-populates with fetched data
  - [ ] 10.3: Test form submission calls update mutation
  - [ ] 10.4: Write unit tests for UpdateModelValidator
  - [ ] 10.5: Write unit tests for UpdateModelAsync service method
  - [ ] 10.6: Write integration tests for PUT endpoint
  - [ ] 10.7: Test endpoint returns 200 with valid data
  - [ ] 10.8: Test endpoint returns 404 for non-existent model
  - [ ] 10.9: Test endpoint returns 400 with invalid data
  - [ ] 10.10: Verify updated_at timestamp changes
  - [ ] 10.11: Verify capabilities updated correctly
  - [ ] 10.12: Verify audit log entry created with diff

## Dev Notes

### Architecture Context

**Edit vs. Create:**
- Reuse ModelForm component with mode prop
- Different API endpoints: POST /models (create) vs PUT /models/{id} (update)
- Different mutations: createModel vs updateModel
- Edit mode disables immutable fields (name, provider)

**Update Transaction:**
```
1. Fetch existing model + capabilities
2. Update model fields
3. Update capabilities fields
4. Set updated_at timestamp
5. Save in single transaction
6. Invalidate cache
7. Audit log
```

**Immutable Fields:**
- Model name and provider should not change (unique identifier)
- If name/provider change needed → delete old, create new
- Consider disabling or hiding these fields in edit mode
- Alternative: Allow change but check for duplicates

### Project Structure Notes

**Frontend Files to Create/Modify:**
```
/frontend/src/pages/admin/
  ├── EditModelPage.tsx (new)
  └── AddModelPage.tsx (modify to pass mode='create')

/frontend/src/components/admin/
  └── ModelForm.tsx (modify to support edit mode)

/frontend/src/hooks/
  └── useUpdateModel.ts (new mutation)

/frontend/src/api/
  └── admin.ts (add updateModel function)
```

**Backend Files to Create/Modify:**
```
/backend/src/Backend.API/Controllers/Admin/
  └── AdminModelsController.cs (add PUT action)

/backend/src/Backend.Application/Services/
  └── AdminModelService.cs (add UpdateModelAsync)

/backend/src/Backend.Application/DTOs/
  └── UpdateModelDto.cs (new, or reuse CreateModelDto)

/backend/src/Backend.Application/Validators/
  └── UpdateModelValidator.cs (new, or reuse CreateModelValidator)

/backend/tests/Backend.API.Tests/
  └── AdminModelsControllerTests.cs (add PUT tests)
```

### Implementation Details

**Edit Route Configuration:**
```typescript
// In App.tsx
<Route path="/admin/models">
  <Route index element={<AdminModelsPage />} />
  <Route path="new" element={<AddModelPage />} />
  <Route path=":id/edit" element={<EditModelPage />} />
</Route>
```

**EditModelPage Component:**
```typescript
export function EditModelPage() {
  const { id } = useParams()
  const { data: model, isLoading, error } = useQuery({
    queryKey: ['admin-model', id],
    queryFn: () => getAdminModelById(id)
  })

  if (isLoading) return <LoadingSpinner />
  if (error || !model) return <ErrorMessage message="Model not found" />

  return (
    <div>
      <h1>Edit Model: {model.name}</h1>
      <ModelForm
        mode="edit"
        modelId={id}
        defaultValues={model}
      />
    </div>
  )
}
```

**ModelForm with Mode Support:**
```typescript
interface ModelFormProps {
  mode: 'create' | 'edit'
  modelId?: string
  defaultValues?: Model
}

export function ModelForm({ mode, modelId, defaultValues }: ModelFormProps) {
  const createMutation = useCreateModel()
  const updateMutation = useUpdateModel()

  const form = useForm({
    resolver: zodResolver(modelSchema),
    defaultValues
  })

  const onSubmit = (data) => {
    if (mode === 'create') {
      createMutation.mutate(data)
    } else {
      updateMutation.mutate({ id: modelId, data })
    }
  }

  return (
    <form onSubmit={form.handleSubmit(onSubmit)}>
      {/* Name field - disabled in edit mode */}
      <Input
        label="Model Name"
        {...form.register('name')}
        disabled={mode === 'edit'}
      />

      {/* ... other fields */}

      <Button type="submit" loading={createMutation.isPending || updateMutation.isPending}>
        {mode === 'create' ? 'Create Model' : 'Save Changes'}
      </Button>
    </form>
  )
}
```

**PUT Endpoint:**
```csharp
[HttpPut("{id}")]
[Authorize]
[ProducesResponseType(typeof(ModelResponseDto), StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
[ProducesResponseType(typeof(ValidationErrorResponse), StatusCodes.Status400BadRequest)]
public async Task<IActionResult> UpdateModel(Guid id, [FromBody] UpdateModelDto dto)
{
    var model = await _adminModelService.UpdateModelAsync(id, dto);

    if (model == null)
        return NotFound(new { error = new { message = "Model not found" } });

    return Ok(new
    {
        data = model,
        meta = new { message = "Model updated successfully", cacheInvalidated = true }
    });
}
```

**UpdateModelAsync Service:**
```csharp
public async Task<ModelResponseDto> UpdateModelAsync(Guid id, UpdateModelDto dto)
{
    // 1. Fetch existing model
    var model = await _modelRepo.GetByIdWithCapabilitiesAsync(id);
    if (model == null) return null;

    // 2. Capture before state for audit
    var beforeState = _mapper.Map<ModelResponseDto>(model);

    // 3. Update model fields
    model.Version = dto.Version;
    model.ReleaseDate = dto.ReleaseDate;
    model.Status = dto.Status;
    model.InputPricePer1M = dto.InputPricePer1M;
    model.OutputPricePer1M = dto.OutputPricePer1M;
    model.Currency = dto.Currency;
    model.PricingValidFrom = dto.PricingValidFrom;
    model.PricingValidTo = dto.PricingValidTo;
    model.UpdatedAt = DateTime.UtcNow;

    // 4. Update capabilities
    model.Capabilities.ContextWindow = dto.Capabilities.ContextWindow;
    model.Capabilities.MaxOutputTokens = dto.Capabilities.MaxOutputTokens;
    model.Capabilities.SupportsFunctionCalling = dto.Capabilities.SupportsFunctionCalling;
    // ... other capability fields

    // 5. Save changes
    await _modelRepo.SaveChangesAsync();

    // 6. Invalidate cache
    await InvalidateCacheForModel(id);

    // 7. Audit log
    var afterState = _mapper.Map<ModelResponseDto>(model);
    await _auditLog.LogAsync(new AuditLogEntry
    {
        AdminUser = GetCurrentUsername(),
        Action = "UPDATE",
        EntityType = "MODEL",
        EntityId = id,
        ChangesJson = JsonSerializer.Serialize(new { before = beforeState, after = afterState })
    });

    return afterState;
}

private async Task InvalidateCacheForModel(Guid id)
{
    await _cache.RemoveAsync($"cache:model:{id}:v1");
    await _cache.RemovePatternAsync("cache:models:*");
    await _cache.RemovePatternAsync("cache:bestvalue:*");
}
```

**Audit Log Diff Example:**
```json
{
  "before": {
    "inputPricePer1M": 0.003,
    "outputPricePer1M": 0.015,
    "status": "active"
  },
  "after": {
    "inputPricePer1M": 0.0025,
    "outputPricePer1M": 0.012,
    "status": "active"
  }
}
```

### References

- [Tech Spec Epic 2: docs/tech-spec-epic-2-8-summary.md#Epic 2]
- [Solution Architecture: docs/solution-architecture.md#7.2 Admin API Contracts]
- [Epics Document: docs/epics.md#Story 2.7]
- [Cache Invalidation: docs/solution-architecture.md#4.3 Cache Invalidation Strategy]

### Testing Strategy

**Component Tests:**
- EditModelPage fetches model by ID
- Form pre-populates with fetched model data
- Name/provider fields disabled in edit mode
- Submit calls updateModel mutation with correct ID
- Success shows toast and redirects to models list

**Unit Tests:**
- UpdateModelValidator validates all fields
- UpdateModelAsync updates model and capabilities
- UpdatedAt timestamp refreshed
- Audit log captures before/after state

**Integration Tests:**
- PUT /api/admin/models/{id} with valid data returns 200
- Response includes updated model
- Database updated with new values
- Updated_at timestamp changed
- Capabilities updated correctly
- PUT with non-existent ID returns 404
- PUT with invalid data returns 400
- Cache invalidated after update

**E2E Test:**
- Admin logs in → navigates to models → clicks edit → changes price → saves → sees updated model in list

## Dev Agent Record

### Context Reference

<!-- Path(s) to story context XML will be added here by context workflow -->

### Agent Model Used

claude-sonnet-4-5-20250929

### Debug Log References

### Completion Notes List

### File List
