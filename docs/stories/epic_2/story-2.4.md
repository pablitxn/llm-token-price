# Story 2.4: Create Add New Model Form

Status: Ready

## Story

As an administrator,
I want form to add new LLM model,
so that I can expand the model database.

## Acceptance Criteria

1. Add model form page created with all required fields:
   - Basic info: name, provider, version, release date, status
   - Pricing: input price/1M, output price/1M, currency, validity dates
2. Form validation ensures required fields completed
3. Form validation ensures prices are positive numbers
4. "Save" button posts data to backend API
5. Success redirects to models list with confirmation message
6. Error displays validation messages

## Tasks / Subtasks

- [ ] **Task 1: Create add model form page** (AC: #1)
  - [ ] 1.1: Create `AddModelPage.tsx` in `/frontend/src/pages/admin`
  - [ ] 1.2: Create `ModelForm.tsx` component in `/frontend/src/components/admin`
  - [ ] 1.3: Set up React Hook Form for form state management
  - [ ] 1.4: Create Zod schema for form validation
  - [ ] 1.5: Add breadcrumb: Dashboard > Models > Add New Model
  - [ ] 1.6: Style form with TailwindCSS for clean layout

- [ ] **Task 2: Add basic info fields** (AC: #1 - basic info)
  - [ ] 2.1: Add "Model Name" text input (required)
  - [ ] 2.2: Add "Provider" text input or dropdown (required)
  - [ ] 2.3: Add "Version" text input (optional)
  - [ ] 2.4: Add "Release Date" date picker (optional)
  - [ ] 2.5: Add "Status" dropdown with options: active, deprecated, beta (required, default: active)
  - [ ] 2.6: Add field labels and placeholder text
  - [ ] 2.7: Display validation errors inline below each field

- [ ] **Task 3: Add pricing fields** (AC: #1 - pricing)
  - [ ] 3.1: Add "Input Price per 1M Tokens" number input (required)
  - [ ] 3.2: Add "Output Price per 1M Tokens" number input (required)
  - [ ] 3.3: Add "Currency" dropdown with USD, EUR, GBP options (required, default: USD)
  - [ ] 3.4: Add "Pricing Valid From" date picker (optional)
  - [ ] 3.5: Add "Pricing Valid To" date picker (optional)
  - [ ] 3.6: Add helper text explaining price format (e.g., "$0.003000 per 1M tokens")
  - [ ] 3.7: Format price inputs to show 6 decimal places

- [ ] **Task 4: Implement form validation** (AC: #2, #3, #6)
  - [ ] 4.1: Validate required fields (name, provider, status, input price, output price, currency)
  - [ ] 4.2: Validate prices are positive numbers (>0)
  - [ ] 4.3: Validate prices have max 6 decimal places
  - [ ] 4.4: Validate model name max length (255 characters)
  - [ ] 4.5: Validate provider max length (100 characters)
  - [ ] 4.6: Validate pricing valid_from < valid_to if both provided
  - [ ] 4.7: Display validation errors in red text below fields
  - [ ] 4.8: Disable submit button if form has errors

- [ ] **Task 5: Implement form submission** (AC: #4, #5)
  - [ ] 5.1: Create submit handler function
  - [ ] 5.2: Use TanStack Query mutation for POST request
  - [ ] 5.3: Show loading spinner on submit button during request
  - [ ] 5.4: Call `POST /api/admin/models` API endpoint
  - [ ] 5.5: On success: redirect to `/admin/models` with success toast
  - [ ] 5.6: Success toast: "Model '{name}' created successfully"
  - [ ] 5.7: On error: display error message in form (AC: #6)
  - [ ] 5.8: Parse server validation errors and show field-specific messages

- [ ] **Task 6: Add form actions**
  - [ ] 6.1: Add "Save" button (primary, submits form)
  - [ ] 6.2: Add "Cancel" button (secondary, navigates back to models list)
  - [ ] 6.3: Confirm navigation away if form has unsaved changes
  - [ ] 6.4: Add "Reset Form" button to clear all fields

- [ ] **Task 7: Create admin models API endpoint** (referenced in Story 2.5, placeholder here)
  - [ ] 7.1: Note: Full implementation in Story 2.5
  - [ ] 7.2: Endpoint contract defined for frontend integration
  - [ ] 7.3: Frontend can call endpoint (will return 501 Not Implemented until Story 2.5)

- [ ] **Task 8: Add testing**
  - [ ] 8.1: Write component tests for ModelForm (Vitest)
  - [ ] 8.2: Test required field validation
  - [ ] 8.3: Test price validation (positive numbers, decimal places)
  - [ ] 8.4: Test form submission calls mutation
  - [ ] 8.5: Test success shows toast and redirects
  - [ ] 8.6: Test error displays validation messages
  - [ ] 8.7: Test cancel button navigates back

## Dev Notes

### Architecture Context

**Form Architecture:**
- React Hook Form for uncontrolled forms (better performance)
- Zod for schema validation (client-side)
- TanStack Query mutation for API call
- FluentValidation on backend (server-side, implemented in Story 2.5)

**Form Sections:**
```
ModelForm
├── BasicInfoSection
│   ├── Name (text, required)
│   ├── Provider (text/dropdown, required)
│   ├── Version (text, optional)
│   ├── Release Date (date, optional)
│   └── Status (dropdown, required)
├── PricingSection
│   ├── Input Price (number, required)
│   ├── Output Price (number, required)
│   ├── Currency (dropdown, required)
│   ├── Valid From (date, optional)
│   └── Valid To (date, optional)
└── FormActions
    ├── Save Button
    ├── Cancel Button
    └── Reset Button
```

**Validation Strategy:**
- Client-side: Immediate UX feedback with Zod
- Server-side: Final validation with FluentValidation (Story 2.5)
- Double validation ensures security (never trust client)

### Project Structure Notes

**Frontend Files to Create:**
```
/frontend/src/pages/admin/
  └── AddModelPage.tsx

/frontend/src/components/admin/
  ├── ModelForm.tsx
  ├── BasicInfoSection.tsx
  ├── PricingSection.tsx
  └── FormActions.tsx

/frontend/src/schemas/
  └── modelSchema.ts (Zod validation)

/frontend/src/hooks/
  └── useCreateModel.ts (TanStack Query mutation)

/frontend/src/api/
  └── admin.ts (add createModel function)
```

**Backend Files (Story 2.5):**
```
/backend/src/Backend.API/Controllers/Admin/
  └── AdminModelsController.cs (POST endpoint - Story 2.5)
```

### Implementation Details

**Zod Validation Schema:**
```typescript
import { z } from 'zod'

export const createModelSchema = z.object({
  name: z.string().min(1, 'Name is required').max(255),
  provider: z.string().min(1, 'Provider is required').max(100),
  version: z.string().max(50).optional(),
  releaseDate: z.string().optional(), // ISO date string
  status: z.enum(['active', 'deprecated', 'beta']),
  inputPricePer1M: z.number().positive('Must be greater than 0').multipleOf(0.000001),
  outputPricePer1M: z.number().positive('Must be greater than 0').multipleOf(0.000001),
  currency: z.enum(['USD', 'EUR', 'GBP']),
  pricingValidFrom: z.string().optional(),
  pricingValidTo: z.string().optional()
}).refine(data => {
  if (data.pricingValidFrom && data.pricingValidTo) {
    return new Date(data.pricingValidFrom) < new Date(data.pricingValidTo)
  }
  return true
}, {
  message: 'Valid From date must be before Valid To date',
  path: ['pricingValidTo']
})
```

**React Hook Form Usage:**
```typescript
const { register, handleSubmit, formState: { errors, isDirty } } = useForm({
  resolver: zodResolver(createModelSchema),
  defaultValues: {
    status: 'active',
    currency: 'USD'
  }
})
```

**API Call Contract (Frontend):**
```typescript
// POST /api/admin/models
Request: {
  name: string
  provider: string
  version?: string
  releaseDate?: string
  status: 'active' | 'deprecated' | 'beta'
  inputPricePer1M: number
  outputPricePer1M: number
  currency: string
  pricingValidFrom?: string
  pricingValidTo?: string
}

Response: {
  data: {
    id: string
    // ... created model fields
  }
  meta: {
    message: "Model created successfully"
  }
}

Error Response: {
  error: {
    code: "VALIDATION_ERROR"
    message: "Validation failed"
    details: {
      field: "inputPricePer1M"
      message: "Must be a positive number"
    }[]
  }
}
```

**Form Field Styling:**
- Labels: font-medium text-gray-700
- Inputs: border border-gray-300 rounded-md px-3 py-2
- Error messages: text-sm text-red-600 mt-1
- Required fields: Red asterisk (*)
- Helper text: text-sm text-gray-500

### References

- [Tech Spec Epic 2: docs/tech-spec-epic-2-8-summary.md#Epic 2]
- [Solution Architecture: docs/solution-architecture.md#7.2 Admin API Contracts]
- [Epics Document: docs/epics.md#Story 2.4]
- [Form Libraries: docs/solution-architecture.md#1.1 Technology Decision Table - React Hook Form, Zod]

### Testing Strategy

**Component Tests:**
- Form renders all fields correctly
- Required validation shows errors on blur
- Price validation rejects negative numbers
- Date validation ensures valid_from < valid_to
- Submit button disabled when form invalid
- Form submission calls mutation with correct data
- Success redirects to models list
- Error displays server validation messages

**Integration Tests (deferred to Story 2.5):**
- POST /api/admin/models creates model in database
- Server validation catches invalid data
- Duplicate model name returns validation error

**Manual Testing Checklist:**
- Fill out form and submit successfully
- Try to submit with missing required fields (blocked)
- Try to submit with negative price (validation error)
- Try to submit with invalid date range (validation error)
- Cancel button returns to models list
- Unsaved changes confirmation works

## Dev Agent Record

### Context Reference

<!-- Path(s) to story context XML will be added here by context workflow -->

### Agent Model Used

claude-sonnet-4-5-20250929

### Debug Log References

### Completion Notes List

### File List
