# Audit Logging Integration Guide

This guide shows how to integrate audit logging into admin controllers for Story 2.13 Task 14.

## Overview

All admin CRUD operations (Create, Update, Delete, Import) should be logged to the `audit_logs` table for compliance and traceability.

## Steps to Integrate Audit Logging

### 1. Inject IAuditLogService in Controller

Add the `IAuditLogService` dependency to the controller constructor:

```csharp
using LlmTokenPrice.Application.Interfaces;
using LlmTokenPrice.API.Extensions; // For User.GetUserId()

public class AdminModelsController : ControllerBase
{
    private readonly IAdminModelService _adminModelService;
    private readonly IAuditLogService _auditLogService; // ADD THIS
    private readonly ILogger<AdminModelsController> _logger;

    public AdminModelsController(
        IAdminModelService adminModelService,
        IAuditLogService auditLogService, // ADD THIS
        ILogger<AdminModelsController> logger)
    {
        _adminModelService = adminModelService ?? throw new ArgumentNullException(nameof(adminModelService));
        _auditLogService = auditLogService ?? throw new ArgumentNullException(nameof(auditLogService)); // ADD THIS
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
}
```

### 2. Log Create Operations

Add audit logging **after** the entity is successfully created:

```csharp
[HttpPost]
public async Task<ActionResult<ModelResponseDto>> CreateModel(
    [FromBody] CreateModelRequest request,
    CancellationToken cancellationToken = default)
{
    // ... validation logic ...

    // Create the model
    var modelId = await _adminModelService.CreateModelAsync(request, cancellationToken);

    // Fetch created model to return in response
    var createdModel = await _adminModelService.GetModelByIdAsync(modelId, cancellationToken);

    // ✅ LOG CREATE OPERATION (after successful creation)
    await _auditLogService.LogCreateAsync(
        userId: User.GetUserId(), // Extension method from ClaimsPrincipalExtensions
        entityType: "Model",
        entityId: modelId,
        newValues: createdModel, // The created entity (will be serialized to JSON)
        cancellationToken);

    _logger.LogInformation("Admin: Model {ModelId} created successfully", modelId);

    return CreatedAtAction(nameof(GetModelById), new { id = modelId }, createdModel);
}
```

### 3. Log Update Operations

Add audit logging **before and after** the update to capture old/new values:

```csharp
[HttpPut("{id:guid}")]
public async Task<ActionResult<ModelResponseDto>> UpdateModel(
    Guid id,
    [FromBody] UpdateModelRequest request,
    CancellationToken cancellationToken = default)
{
    // ... validation logic ...

    // ✅ FETCH OLD VALUES (before update)
    var oldModel = await _adminModelService.GetModelByIdAsync(id, cancellationToken);

    if (oldModel == null)
    {
        return NotFound(new { error = "Model not found" });
    }

    // Perform the update
    var updatedModel = await _adminModelService.UpdateModelAsync(id, request, cancellationToken);

    // ✅ LOG UPDATE OPERATION (after successful update)
    await _auditLogService.LogUpdateAsync(
        userId: User.GetUserId(),
        entityType: "Model",
        entityId: id,
        oldValues: oldModel, // Entity before update
        newValues: updatedModel, // Entity after update
        cancellationToken);

    _logger.LogInformation("Admin: Model {ModelId} updated successfully", id);

    return Ok(updatedModel);
}
```

### 4. Log Delete Operations

Add audit logging **before** the delete to capture the old values:

```csharp
[HttpDelete("{id:guid}")]
public async Task<IActionResult> DeleteModel(
    Guid id,
    CancellationToken cancellationToken = default)
{
    // ✅ FETCH OLD VALUES (before delete)
    var modelToDelete = await _adminModelService.GetModelByIdAsync(id, cancellationToken);

    if (modelToDelete == null)
    {
        return NotFound(new { error = "Model not found" });
    }

    // Perform the delete (soft delete: sets IsActive = false)
    var deleted = await _adminModelService.DeleteModelAsync(id, cancellationToken);

    if (!deleted)
    {
        return NotFound(new { error = "Model not found or already inactive" });
    }

    // ✅ LOG DELETE OPERATION (after successful delete)
    await _auditLogService.LogDeleteAsync(
        userId: User.GetUserId(),
        entityType: "Model",
        entityId: id,
        oldValues: modelToDelete, // Entity before deletion
        cancellationToken);

    _logger.LogInformation("Admin: Model {ModelId} deleted successfully", id);

    return NoContent();
}
```

### 5. Log Import Operations (CSV, Bulk Operations)

Add audit logging **after** a bulk import operation:

```csharp
[HttpPost("import-csv")]
public async Task<IActionResult> ImportModelsCsv(
    IFormFile file,
    CancellationToken cancellationToken = default)
{
    // ... file validation ...

    // Process CSV file
    using var stream = file.OpenReadStream();
    var result = await _csvImportService.ImportModelsAsync(stream, cancellationToken);

    // ✅ LOG IMPORT OPERATION (after processing)
    await _auditLogService.LogImportAsync(
        userId: User.GetUserId(),
        entityType: "Model",
        entityId: Guid.Empty, // Use zero GUID for summary (or first imported entity ID)
        newValues: new
        {
            FileName = file.FileName,
            TotalRows = result.TotalRows,
            SuccessfulImports = result.SuccessfulImports,
            FailedImports = result.FailedImports,
            SkippedDuplicates = result.SkippedDuplicates
        }, // Summary of import operation
        cancellationToken);

    _logger.LogInformation(
        "Admin: CSV import completed - File: {FileName}, Success: {Success}, Failed: {Failed}",
        file.FileName,
        result.SuccessfulImports,
        result.FailedImports);

    return Ok(new { data = result });
}
```

## Best Practices

### ✅ DO

1. **Always use `User.GetUserId()`** extension method to get the authenticated user identifier
2. **Log AFTER successful operations** (except Update/Delete where old values are needed first)
3. **Pass the complete entity/DTO** to the audit service (it handles JSON serialization)
4. **Use meaningful entity types**: "Model", "Benchmark", "BenchmarkScore" (consistent naming)
5. **Handle exceptions gracefully**: Don't let audit logging failures break the main operation
6. **Use CancellationToken** for all async audit logging calls

### ❌ DON'T

1. **Don't log before operations** (the operation might fail, creating false audit entries)
2. **Don't serialize entities manually** (AuditLogService handles JSON serialization)
3. **Don't log sensitive data** (passwords, API keys) - audit logs are queryable
4. **Don't use hardcoded user IDs** - always get from `User.GetUserId()`
5. **Don't block operations on audit logging failures** - log the error but continue

## Entity Types to Use

| Controller | Entity Type |
|------------|-------------|
| AdminModelsController | `"Model"` |
| AdminBenchmarksController | `"Benchmark"` |
| AdminBenchmarkScoresController | `"BenchmarkScore"` |

## Actions

| Operation | Action String |
|-----------|---------------|
| Create (POST) | `"Create"` |
| Update (PUT) | `"Update"` |
| Delete (DELETE) | `"Delete"` |
| Bulk Import (CSV) | `"Import"` |

## Example: Complete Integration

See `/docs/audit-logging-examples.md` for complete controller examples with audit logging integrated.

## Testing Audit Logging

After integration, verify audit logging works:

```bash
# 1. Create a model via API
curl -X POST http://localhost:5000/api/admin/models \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"name":"Test Model","provider":"TestCo","inputPricePer1M":1.0,"outputPricePer1M":2.0}'

# 2. Query audit logs
curl http://localhost:5000/api/admin/audit-log?entityType=Model&action=Create \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"

# 3. Verify audit log entry exists with:
#    - Correct userId (your email/username)
#    - Action = "Create"
#    - EntityType = "Model"
#    - NewValues contains the created model JSON
#    - OldValues is null (Create has no previous state)
```

## Controllers to Update

The following controllers need audit logging integration:

- [x] ✅ `AuditLogController.cs` - Already has GET endpoints (no CRUD operations to log)
- [ ] ⏳ `AdminModelsController.cs` - **HIGH PRIORITY** (Create, Update, Delete)
- [ ] ⏳ `AdminBenchmarksController.cs` - **HIGH PRIORITY** (Create, Update, Delete, Import CSV)
- [ ] ⏳ `AdminBenchmarkScoresController.cs` - **MEDIUM** (if exists)
- [ ] ⏳ `AdminAuthController.cs` - **LOW** (login/logout don't need audit logging)

## Notes

- Audit logs are **append-only** - never updated or deleted
- All audit logs are queryable via `GET /api/admin/audit-log` with filters
- CSV export available at `GET /api/admin/audit-log/export`
- Frontend can display audit trail for each entity by querying with `entityType` + `entityId` filters

---

**Story**: 2.13 Task 14: Implement Audit Log
**Last Updated**: 2025-10-21
