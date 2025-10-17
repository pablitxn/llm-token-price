# Story 2.5: Create Backend API for Adding Models

Status: Ready

## Story

As an administrator,
I want backend API to persist new models,
so that added models are saved to database.

## Acceptance Criteria

1. POST `/api/admin/models` endpoint created
2. Endpoint accepts JSON model data with validation
3. Model entity created and saved to database with timestamps
4. ModelCapabilities entity created with default values
5. API returns 201 Created with new model ID
6. Validation errors return 400 Bad Request with details

## Tasks / Subtasks

- [ ] **Task 1: Create POST models endpoint** (AC: #1)
  - [ ] 1.1: Add POST action to `AdminModelsController.cs`
  - [ ] 1.2: Add `[Authorize]` attribute for JWT authentication
  - [ ] 1.3: Accept `CreateModelDto` from request body
  - [ ] 1.4: Call `AdminModelService.CreateModelAsync()` application service
  - [ ] 1.5: Return 201 Created with Location header pointing to new model
  - [ ] 1.6: Add Swagger/OpenAPI documentation for endpoint

- [ ] **Task 2: Create DTOs for model creation** (AC: #2)
  - [ ] 2.1: Create `CreateModelDto.cs` in `/Backend.Application/DTOs`
  - [ ] 2.2: Add properties: name, provider, version, releaseDate, status, inputPricePer1M, outputPricePer1M, currency, pricingValidFrom, pricingValidTo
  - [ ] 2.3: Create `ModelResponseDto.cs` for response
  - [ ] 2.4: Add AutoMapper mapping profile from Model entity to ModelResponseDto

- [ ] **Task 3: Implement FluentValidation validator** (AC: #2, #6)
  - [ ] 3.1: Create `CreateModelValidator.cs` in `/Backend.Application/Validators`
  - [ ] 3.2: Validate name: required, max 255 characters
  - [ ] 3.3: Validate provider: required, max 100 characters
  - [ ] 3.4: Validate inputPricePer1M: required, greater than 0, max 6 decimal places
  - [ ] 3.5: Validate outputPricePer1M: required, greater than 0, max 6 decimal places
  - [ ] 3.6: Validate currency: required, must be valid currency code (USD, EUR, GBP)
  - [ ] 3.7: Validate status: required, must be 'active', 'deprecated', or 'beta'
  - [ ] 3.8: Validate date range: pricingValidFrom < pricingValidTo if both provided
  - [ ] 3.9: Register validator in dependency injection

- [ ] **Task 4: Create AdminModelService** (AC: #3, #4)
  - [ ] 4.1: Create `AdminModelService.cs` in `/Backend.Application/Services`
  - [ ] 4.2: Inject `IModelRepository` and `IUnitOfWork` (or DbContext)
  - [ ] 4.3: Implement `CreateModelAsync(CreateModelDto dto)` method
  - [ ] 4.4: Create Model entity from DTO
  - [ ] 4.5: Set created_at and updated_at timestamps
  - [ ] 4.6: Set is_active = true by default
  - [ ] 4.7: Save model to database via repository
  - [ ] 4.8: Create ModelCapabilities entity with default values
  - [ ] 4.9: Link capabilities to model (foreign key)
  - [ ] 4.10: Save capabilities to database
  - [ ] 4.11: Commit transaction
  - [ ] 4.12: Return created model ID

- [ ] **Task 5: Implement default capabilities creation** (AC: #4)
  - [ ] 5.1: Create ModelCapabilities entity with model_id foreign key
  - [ ] 5.2: Set default values: contextWindow = 0, all boolean flags = false
  - [ ] 5.3: Note: Admin will update capabilities in Story 2.6
  - [ ] 5.4: Ensure one-to-one relationship constraint (unique model_id)

- [ ] **Task 6: Add error handling and validation** (AC: #6)
  - [ ] 6.1: Catch validation exceptions and return 400 Bad Request
  - [ ] 6.2: Format validation errors as JSON with field names and messages
  - [ ] 6.3: Catch duplicate model name+provider and return 400 with specific message
  - [ ] 6.4: Catch database exceptions and return 500 with generic error message
  - [ ] 6.5: Log all errors with Serilog
  - [ ] 6.6: Use global exception handler middleware for consistency

- [ ] **Task 7: Invalidate cache after model creation**
  - [ ] 7.1: Inject `ICacheService` into AdminModelService
  - [ ] 7.2: After successful save, invalidate public models list cache
  - [ ] 7.3: Remove cache keys: `cache:models:list:*`, `cache:bestvalue:*`
  - [ ] 7.4: Use cache invalidation pattern from architecture
  - [ ] 7.5: Consider domain event (ModelCreatedEvent) for decoupling

- [ ] **Task 8: Add audit logging**
  - [ ] 8.1: Create audit log entry after model creation
  - [ ] 8.2: Log admin username from JWT claims
  - [ ] 8.3: Log action = "CREATE", entity_type = "MODEL"
  - [ ] 8.4: Log entity_id = new model ID
  - [ ] 8.5: Log changes_json with created model data
  - [ ] 8.6: Save to admin_audit_log table

- [ ] **Task 9: Add testing**
  - [ ] 9.1: Write unit tests for CreateModelValidator (xUnit)
  - [ ] 9.2: Test all validation rules (required fields, positive prices, etc.)
  - [ ] 9.3: Write unit tests for AdminModelService
  - [ ] 9.4: Test model and capabilities created correctly
  - [ ] 9.5: Write integration tests for POST endpoint
  - [ ] 9.6: Test endpoint returns 201 with valid data
  - [ ] 9.7: Test endpoint returns 400 with invalid data
  - [ ] 9.8: Test endpoint returns 401 without JWT token
  - [ ] 9.9: Test duplicate model name returns 400
  - [ ] 9.10: Verify model persisted to database
  - [ ] 9.11: Verify audit log entry created

## Dev Notes

### Architecture Context

**Hexagonal Architecture Layers:**
```
AdminModelsController (API/Presentation)
  ↓ calls
AdminModelService (Application)
  ↓ uses
ModelRepository (Infrastructure)
  ↓ persists
PostgreSQL Database
```

**Transaction Boundary:**
- Create model + create capabilities in single transaction
- Use EF Core implicit transaction (single SaveChangesAsync)
- Rollback if any operation fails

**Domain Events (Optional Enhancement):**
- Emit `ModelCreatedEvent` after successful save
- Event handler invalidates cache
- Event handler creates audit log
- Decouples cross-cutting concerns from service

### Project Structure Notes

**Backend Files to Create:**
```
/backend/src/Backend.API/Controllers/Admin/
  └── AdminModelsController.cs (add POST action)

/backend/src/Backend.Application/Services/
  └── AdminModelService.cs

/backend/src/Backend.Application/DTOs/
  ├── CreateModelDto.cs
  └── ModelResponseDto.cs

/backend/src/Backend.Application/Validators/
  └── CreateModelValidator.cs

/backend/src/Backend.Application/Mapping/
  └── ModelMappingProfile.cs (AutoMapper)

/backend/src/Backend.Infrastructure/Repositories/
  └── ModelRepository.cs (add Create method)

/backend/tests/Backend.Application.Tests/
  ├── CreateModelValidatorTests.cs
  └── AdminModelServiceTests.cs

/backend/tests/Backend.API.Tests/
  └── AdminModelsControllerTests.cs
```

### Implementation Details

**Controller Action:**
```csharp
[HttpPost]
[Authorize]
[ProducesResponseType(typeof(ModelResponseDto), StatusCodes.Status201Created)]
[ProducesResponseType(typeof(ValidationErrorResponse), StatusCodes.Status400BadRequest)]
public async Task<IActionResult> CreateModel([FromBody] CreateModelDto dto)
{
    var modelId = await _adminModelService.CreateModelAsync(dto);
    var model = await _adminModelService.GetModelByIdAsync(modelId);

    return CreatedAtAction(
        nameof(GetModel),
        new { id = modelId },
        new { data = model, meta = new { message = "Model created successfully" } }
    );
}
```

**Service Method:**
```csharp
public class AdminModelService
{
    private readonly IModelRepository _modelRepo;
    private readonly ICacheService _cache;
    private readonly IAuditLogRepository _auditLog;
    private readonly IMapper _mapper;

    public async Task<Guid> CreateModelAsync(CreateModelDto dto)
    {
        // 1. Check for duplicates
        var existing = await _modelRepo.GetByNameAndProviderAsync(dto.Name, dto.Provider);
        if (existing != null)
            throw new ValidationException("Model with this name and provider already exists");

        // 2. Create model entity
        var model = new Model
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Provider = dto.Provider,
            Version = dto.Version,
            ReleaseDate = dto.ReleaseDate,
            Status = dto.Status,
            InputPricePer1M = dto.InputPricePer1M,
            OutputPricePer1M = dto.OutputPricePer1M,
            Currency = dto.Currency,
            PricingValidFrom = dto.PricingValidFrom,
            PricingValidTo = dto.PricingValidTo,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // 3. Create default capabilities
        var capabilities = new ModelCapabilities
        {
            Id = Guid.NewGuid(),
            ModelId = model.Id,
            ContextWindow = 0,
            MaxOutputTokens = null,
            SupportsFunctionCalling = false,
            SupportsVision = false,
            SupportsAudioInput = false,
            SupportsAudioOutput = false,
            SupportsStreaming = true,
            SupportsJsonMode = false
        };

        // 4. Save (EF Core transaction)
        await _modelRepo.AddAsync(model);
        await _modelRepo.AddCapabilitiesAsync(capabilities);
        await _modelRepo.SaveChangesAsync();

        // 5. Invalidate cache
        await _cache.RemovePatternAsync("cache:models:*");
        await _cache.RemovePatternAsync("cache:bestvalue:*");

        // 6. Audit log
        await _auditLog.LogAsync(new AuditLogEntry
        {
            AdminUser = GetCurrentUsername(),
            Action = "CREATE",
            EntityType = "MODEL",
            EntityId = model.Id,
            ChangesJson = JsonSerializer.Serialize(new { after = model })
        });

        return model.Id;
    }
}
```

**FluentValidation Example:**
```csharp
public class CreateModelValidator : AbstractValidator<CreateModelDto>
{
    public CreateModelValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Model name is required")
            .MaximumLength(255).WithMessage("Model name cannot exceed 255 characters");

        RuleFor(x => x.Provider)
            .NotEmpty().WithMessage("Provider is required")
            .MaximumLength(100).WithMessage("Provider cannot exceed 100 characters");

        RuleFor(x => x.InputPricePer1M)
            .GreaterThan(0).WithMessage("Input price must be greater than 0")
            .ScalePrecision(6, 10).WithMessage("Input price can have max 6 decimal places");

        RuleFor(x => x.OutputPricePer1M)
            .GreaterThan(0).WithMessage("Output price must be greater than 0")
            .ScalePrecision(6, 10).WithMessage("Output price can have max 6 decimal places");

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required")
            .Must(c => new[] { "USD", "EUR", "GBP" }.Contains(c))
            .WithMessage("Currency must be USD, EUR, or GBP");

        When(x => x.PricingValidFrom.HasValue && x.PricingValidTo.HasValue, () =>
        {
            RuleFor(x => x.PricingValidFrom)
                .LessThan(x => x.PricingValidTo)
                .WithMessage("Valid From must be before Valid To");
        });
    }
}
```

**Error Response Format:**
```json
{
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "Validation failed",
    "details": [
      {
        "field": "inputPricePer1M",
        "message": "Input price must be greater than 0"
      },
      {
        "field": "name",
        "message": "Model name is required"
      }
    ]
  }
}
```

### References

- [Tech Spec Epic 2: docs/tech-spec-epic-2-8-summary.md#Epic 2]
- [Solution Architecture: docs/solution-architecture.md#2.1 Hexagonal Architecture]
- [Epics Document: docs/epics.md#Story 2.5]
- [Database Schema: docs/solution-architecture.md#3.1 Database Schema]
- [Validation: docs/solution-architecture.md#12.2 Input Validation]

### Testing Strategy

**Unit Tests:**
- CreateModelValidator validates all fields correctly
- CreateModelValidator rejects invalid data
- AdminModelService creates model and capabilities
- AdminModelService handles duplicate names
- Service invalidates cache after creation
- Audit log entry created with correct data

**Integration Tests:**
- POST /api/admin/models with valid data returns 201
- Response includes created model ID and data
- Model persisted to database with correct values
- Capabilities created with default values
- POST with invalid data returns 400 with validation errors
- POST without auth token returns 401
- Duplicate model name returns 400
- Cache invalidated after successful creation

**Performance Tests:**
- Model creation completes in <200ms
- Transaction rollback works on failure
- Concurrent model creations handled correctly

## Dev Agent Record

### Context Reference

<!-- Path(s) to story context XML will be added here by context workflow -->

### Agent Model Used

claude-sonnet-4-5-20250929

### Debug Log References

### Completion Notes List

### File List
