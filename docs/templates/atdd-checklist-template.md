# ATDD Checklist Template

**Story:** [Story ID] - [Story Title]
**Date Started:** [YYYY-MM-DD]
**Developer:** [Name]
**Status:** 🔴 RED | 🟢 GREEN | 🔵 REFACTOR

---

## Overview

**Purpose:** This ATDD (Acceptance Test-Driven Development) checklist enforces test-first discipline for story implementation. Follow the RED → GREEN → REFACTOR cycle for each acceptance criterion.

**Workflow:**
1. **RED Phase:** Write failing tests for acceptance criteria BEFORE implementation
2. **GREEN Phase:** Write minimal code to make tests pass
3. **REFACTOR Phase:** Improve code quality while keeping tests green

---

## Story Context Reference

**Story File:** `[path/to/story.md]`
**Context XML:** `[path/to/story-context.xml]`
**Epic:** [Epic Number] - [Epic Title]
**Story Points:** [Number]
**Estimated Effort:** [Hours/Days]

**Key Documentation:**
- [Doc 1 Title] (`path/to/doc1.md`)
- [Doc 2 Title] (`path/to/doc2.md`)

**Key Code Artifacts:**
- [Component/Service 1] (`path/to/file1.ts`)
- [Component/Service 2] (`path/to/file2.cs`)

**Dependencies:**
- [Package 1] (version X.Y.Z)
- [Package 2] (version X.Y.Z)

---

## Acceptance Criteria Mapping

| AC # | Description | Test Type | Test File | Status |
|------|-------------|-----------|-----------|--------|
| AC#1 | [Description] | Unit/Integration/E2E | `path/to/test.spec.ts` | 🔴 RED |
| AC#2 | [Description] | Unit/Integration/E2E | `path/to/test.spec.ts` | 🔴 RED |
| AC#3 | [Description] | Unit/Integration/E2E | `path/to/test.spec.ts` | 🔴 RED |

**Legend:**
- 🔴 RED: Test written but failing (implementation pending)
- 🟢 GREEN: Test passing (implementation complete)
- 🔵 REFACTOR: Test passing, code refactored for quality

---

## RED Phase: Failing Tests

**Goal:** Write comprehensive failing tests for ALL acceptance criteria BEFORE writing any implementation code.

### Test Group 1: [Feature Area - e.g., "Component Rendering"]

**Acceptance Criteria Covered:** AC#1, AC#2

#### Test 1.1: [Test Description]
- **AC Mapping:** AC#1
- **Test Type:** Unit | Integration | E2E
- **Test File:** `path/to/test.spec.ts`
- **Status:** 🔴 RED
- **Expected Failure Reason:** [e.g., "Component not yet created"]

**Test Code Snippet:**
```typescript
// Example test structure
describe('[Component/Service Name]', () => {
  it('should [expected behavior] (AC#1)', () => {
    // Arrange
    const input = ...;

    // Act
    const result = ...;

    // Assert
    expect(result).toBe(...);
  });
});
```

**Validation:**
- [ ] Test runs and fails with expected error
- [ ] Test clearly validates acceptance criterion
- [ ] Test is isolated (no external dependencies)
- [ ] Test is deterministic (same input → same output)

---

#### Test 1.2: [Test Description]
- **AC Mapping:** AC#2
- **Test Type:** Unit | Integration | E2E
- **Test File:** `path/to/test.spec.ts`
- **Status:** 🔴 RED
- **Expected Failure Reason:** [e.g., "Service method not implemented"]

**Test Code Snippet:**
```typescript
// Example test structure
```

**Validation:**
- [ ] Test runs and fails with expected error
- [ ] Test clearly validates acceptance criterion
- [ ] Test is isolated (no external dependencies)
- [ ] Test is deterministic (same input → same output)

---

### Test Group 2: [Feature Area - e.g., "API Integration"]

**Acceptance Criteria Covered:** AC#3, AC#4

#### Test 2.1: [Test Description]
- **AC Mapping:** AC#3
- **Test Type:** Integration
- **Test File:** `path/to/integration-test.spec.ts`
- **Status:** 🔴 RED
- **Expected Failure Reason:** [e.g., "API endpoint not created"]

**Test Code Snippet:**
```typescript
// Example integration test
```

**Validation:**
- [ ] Test runs and fails with expected error
- [ ] Test validates end-to-end flow
- [ ] Test uses realistic test data
- [ ] Test cleanup is implemented (teardown)

---

### RED Phase Checklist

- [ ] All acceptance criteria have corresponding tests
- [ ] All tests are written BEFORE implementation
- [ ] All tests fail for the right reasons (not syntax errors)
- [ ] Test coverage meets story requirements (70%+ for critical paths)
- [ ] Tests are independent (can run in any order)
- [ ] Tests use descriptive names (should [behavior] when [condition])
- [ ] Tests follow AAA pattern (Arrange, Act, Assert)

**RED Phase Completion:**
- **Total Tests Written:** [Number]
- **All Tests Failing:** ✅ | ❌
- **Ready for GREEN Phase:** ✅ | ❌

---

## GREEN Phase: Passing Tests

**Goal:** Write MINIMAL code to make tests pass. Focus on functionality, not perfection.

### Implementation 1: [Component/Service Name]

**Tests Addressed:** Test 1.1, Test 1.2

#### Implementation Steps:
1. **Create file:** `path/to/component.tsx`
2. **Implement minimal functionality:**
   - [Step 1: e.g., "Create basic component structure"]
   - [Step 2: e.g., "Add props interface"]
   - [Step 3: e.g., "Implement render logic"]
3. **Run tests:** Verify Test 1.1 and Test 1.2 now pass

**Implementation Code Snippet:**
```typescript
// Example minimal implementation
export const ComponentName = (props: Props) => {
  // Minimal code to make tests pass
  return <div>{props.value}</div>;
};
```

**Validation:**
- [ ] Test 1.1 now passes (🟢 GREEN)
- [ ] Test 1.2 now passes (🟢 GREEN)
- [ ] No new failing tests introduced
- [ ] Implementation is minimal (no premature optimization)

---

### Implementation 2: [Component/Service Name]

**Tests Addressed:** Test 2.1

#### Implementation Steps:
1. **Create file:** `path/to/service.ts`
2. **Implement minimal functionality:**
   - [Step 1]
   - [Step 2]
3. **Run tests:** Verify Test 2.1 now passes

**Implementation Code Snippet:**
```typescript
// Example service implementation
```

**Validation:**
- [ ] Test 2.1 now passes (🟢 GREEN)
- [ ] Integration with existing code works
- [ ] No new failing tests introduced

---

### GREEN Phase Checklist

- [ ] All tests are now passing (100% pass rate)
- [ ] Implementation covers all acceptance criteria
- [ ] No tests were modified to "force" passing (except for legitimate refinements)
- [ ] Code compiles without errors or warnings
- [ ] Integration tests validate end-to-end flows
- [ ] Manual testing confirms expected behavior (for critical paths)

**GREEN Phase Completion:**
- **Total Tests Passing:** [Number] / [Total]
- **Pass Rate:** [Percentage]%
- **All Acceptance Criteria Met:** ✅ | ❌
- **Ready for REFACTOR Phase:** ✅ | ❌

---

## REFACTOR Phase: Code Quality Improvements

**Goal:** Improve code quality while KEEPING tests green. Focus on readability, maintainability, performance.

### Refactoring 1: [Improvement Area - e.g., "Extract Reusable Utility"]

**Files Affected:** `path/to/file1.ts`, `path/to/file2.ts`

**Improvement Description:**
[Describe the refactoring - e.g., "Extract duplicate validation logic into shared utility function"]

**Before Refactoring:**
```typescript
// Example: Duplicate code
const validateEmail1 = (email: string) => /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email);
const validateEmail2 = (email: string) => /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email);
```

**After Refactoring:**
```typescript
// Example: Extracted utility
const validateEmail = (email: string) => /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email);
```

**Validation:**
- [ ] All tests still pass after refactoring (🟢 GREEN maintained)
- [ ] Code is more readable/maintainable
- [ ] No new bugs introduced
- [ ] Performance improved (if applicable)

---

### Refactoring 2: [Improvement Area - e.g., "Optimize Component Rendering"]

**Files Affected:** `path/to/component.tsx`

**Improvement Description:**
[Describe the refactoring - e.g., "Add React.memo to prevent unnecessary re-renders"]

**Before Refactoring:**
```typescript
// Example: Component without memoization
export const ExpensiveComponent = (props: Props) => { /* ... */ };
```

**After Refactoring:**
```typescript
// Example: Component with memoization
export const ExpensiveComponent = React.memo((props: Props) => { /* ... */ });
```

**Validation:**
- [ ] All tests still pass (🟢 GREEN maintained)
- [ ] Performance benchmarked (e.g., 50% fewer renders)
- [ ] No regressions in functionality

---

### REFACTOR Phase Checklist

- [ ] Code follows project style guidelines (ESLint/Prettier passing)
- [ ] Duplicate code extracted into reusable functions/components
- [ ] Variable/function names are descriptive
- [ ] Comments added for complex logic (why, not what)
- [ ] Performance optimizations applied (where applicable)
- [ ] Security best practices followed (input sanitization, etc.)
- [ ] All tests still pass (100% pass rate maintained)
- [ ] Code reviewed (self-review or peer review)

**REFACTOR Phase Completion:**
- **Code Quality Score:** [Subjective assessment: High/Medium/Low]
- **All Tests Still Passing:** ✅ | ❌
- **Ready for Story Approval:** ✅ | ❌

---

## Final Validation

### Acceptance Criteria Verification

| AC # | Description | Test Status | Manual Verification |
|------|-------------|-------------|---------------------|
| AC#1 | [Description] | 🟢 GREEN | ✅ Verified |
| AC#2 | [Description] | 🟢 GREEN | ✅ Verified |
| AC#3 | [Description] | 🟢 GREEN | ✅ Verified |

### Definition of Done Checklist

- [ ] All acceptance criteria met (100%)
- [ ] All tests passing (100% pass rate)
- [ ] Test coverage meets minimum threshold (70%+)
- [ ] Code quality checks pass (ESLint, TypeScript strict mode, etc.)
- [ ] No compiler warnings or errors
- [ ] Documentation updated (README, inline comments, etc.)
- [ ] Manual testing performed for critical paths
- [ ] Story context XML referenced throughout implementation
- [ ] Code reviewed (self-review minimum, peer review preferred)
- [ ] Ready for story-approved workflow

**Story Completion Status:**
- **Tests Written:** [Number]
- **Tests Passing:** [Number] ([Percentage]%)
- **Acceptance Criteria Met:** [Number] / [Total]
- **Definition of Done:** ✅ COMPLETE | ❌ INCOMPLETE

---

## Usage Instructions

### How to Use This Template

1. **Copy this template** to your story directory: `docs/stories/atdd-checklist-[story-id].md`
2. **Fill in the Overview section** with story details from the story markdown and context XML
3. **Create Acceptance Criteria Mapping table** using acceptance criteria from the story
4. **RED Phase:**
   - Write failing tests for each acceptance criterion
   - Document each test with AC mapping, file path, and expected failure reason
   - Run tests to confirm they fail for the right reasons
   - Check off RED Phase Checklist items
5. **GREEN Phase:**
   - Implement minimal code to make tests pass
   - Document implementation steps and code snippets
   - Run tests after each implementation to verify they pass
   - Check off GREEN Phase Checklist items
6. **REFACTOR Phase:**
   - Identify code quality improvements
   - Refactor while keeping tests green
   - Document before/after code for each refactoring
   - Check off REFACTOR Phase Checklist items
7. **Final Validation:**
   - Verify all acceptance criteria met
   - Complete Definition of Done checklist
   - Mark story ready for approval

### Best Practices

- **Test-First Discipline:** NEVER write implementation code before writing tests
- **Red-Green-Refactor Cycle:** Follow the cycle strictly for each feature
- **Small Iterations:** Work on 1-2 acceptance criteria at a time
- **Frequent Test Runs:** Run tests after every code change
- **Commit Often:** Commit after each RED, GREEN, and REFACTOR phase
- **Reference Context:** Use Story Context XML as single source of truth
- **Document Deviations:** If you deviate from the plan, document why

### Example Reference

**See Story 1.11 (Establish Test Infrastructure)** as a successful example of ATDD discipline:
- File: `docs/stories/story-1.11.md`
- Context: `docs/stories/story-context-1.1.11.xml`
- Result: 42 tests passing, 100% success rate, all 14 acceptance criteria met

---

## Notes

**Template Version:** 1.0
**Created:** 2025-10-21 (Story 3.1b AC #15)
**Last Updated:** 2025-10-21
**Maintained By:** Development Team

**Feedback:** If you have suggestions for improving this template, update it and document changes in the story that made the improvement.
