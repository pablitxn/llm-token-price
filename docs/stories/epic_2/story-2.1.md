# Story 2.1: Admin Panel Authentication

Status: In Progress

## Story

As an administrator,
I want secure login to admin panel,
so that only authorized users can manage model data.

## Acceptance Criteria

1. Admin login page created with username/password fields
2. Backend authentication endpoint `/api/admin/auth/login` created
3. Simple authentication mechanism (hardcoded credentials for MVP, or basic JWT)
4. Session/token stored in browser for authenticated requests
5. Protected routes redirect to login if not authenticated
6. Logout functionality clears session

## Tasks / Subtasks

- [ ] **Task 1: Create admin login page component** (AC: #1)
  - [ ] 1.1: Create `AdminLoginPage.tsx` in `/frontend/src/pages/admin`
  - [ ] 1.2: Add username and password input fields with form validation (Zod schema)
  - [ ] 1.3: Add login button with loading state
  - [ ] 1.4: Style with TailwindCSS for clean, professional appearance
  - [ ] 1.5: Add error message display for failed authentication

- [ ] **Task 2: Implement backend authentication endpoint** (AC: #2, #3)
  - [ ] 2.1: Create `AdminAuthController.cs` in `/backend/src/Backend.API/Controllers/Admin`
  - [ ] 2.2: Implement `POST /api/admin/auth/login` endpoint
  - [ ] 2.3: Add JWT token generation service using `System.IdentityModel.Tokens.Jwt`
  - [ ] 2.4: Configure hardcoded admin credentials in appsettings (for MVP)
  - [ ] 2.5: Set JWT secret key in appsettings (generate secure random key)
  - [ ] 2.6: Return JWT token in HttpOnly cookie with 24-hour expiration
  - [ ] 2.7: Add validation for username/password format

- [ ] **Task 3: Implement authentication state management** (AC: #4)
  - [ ] 3.1: Create `useAuth` hook in `/frontend/src/hooks`
  - [ ] 3.2: Add auth API functions in `/frontend/src/api/admin.ts`
  - [ ] 3.3: Store authentication state in Zustand store (`authStore.ts`)
  - [ ] 3.4: Implement token storage in HttpOnly cookie (set by backend)
  - [ ] 3.5: Add axios interceptor to include credentials in admin API requests

- [ ] **Task 4: Implement protected routes** (AC: #5)
  - [ ] 4.1: Create `ProtectedRoute` wrapper component
  - [ ] 4.2: Check authentication status before rendering admin routes
  - [ ] 4.3: Redirect to `/admin/login` if not authenticated
  - [ ] 4.4: Wrap all admin panel routes with `ProtectedRoute`
  - [ ] 4.5: Preserve intended destination URL for post-login redirect

- [ ] **Task 5: Implement logout functionality** (AC: #6)
  - [ ] 5.1: Add logout button to admin layout header
  - [ ] 5.2: Create `POST /api/admin/auth/logout` endpoint
  - [ ] 5.3: Clear JWT cookie on backend logout
  - [ ] 5.4: Clear auth state in frontend Zustand store
  - [ ] 5.5: Redirect to login page after logout

- [ ] **Task 6: Add authentication testing**
  - [ ] 6.1: Write unit tests for JWT generation service (xUnit)
  - [ ] 6.2: Write integration tests for login endpoint (WebApplicationFactory)
  - [ ] 6.3: Write frontend tests for login form (Vitest + Testing Library)
  - [ ] 6.4: Test protected route redirection behavior
  - [ ] 6.5: Test logout clears session correctly

## Dev Notes

### Architecture Context

**Authentication Pattern:**
- JWT-based authentication with HttpOnly cookies (more secure than localStorage)
- Backend signs JWT with secret key, includes admin user identifier
- Frontend receives token in cookie, automatically sent with subsequent requests
- No OAuth/OIDC needed for MVP (1-3 admin users only)

**Security Considerations:**
- HttpOnly cookies prevent XSS attacks (JavaScript can't access token)
- SameSite=Strict cookie attribute prevents CSRF
- HTTPS required in production (enforce via middleware)
- Token expiration: 24 hours (configurable)
- Credentials hardcoded for MVP, move to environment variables or user management system post-MVP

**Tech Stack:**
- Backend: `System.IdentityModel.Tokens.Jwt` NuGet package
- Frontend: Axios for API calls, Zustand for auth state
- Validation: Zod (client), FluentValidation (server)

### Project Structure Notes

**Backend Files to Create:**
```
/backend/src/Backend.API/Controllers/Admin/
  └── AdminAuthController.cs

/backend/src/Backend.Application/Services/
  └── AuthService.cs

/backend/src/Backend.Application/DTOs/
  └── LoginDto.cs
  └── AuthResponseDto.cs
```

**Frontend Files to Create:**
```
/frontend/src/pages/admin/
  └── AdminLoginPage.tsx

/frontend/src/hooks/
  └── useAuth.ts

/frontend/src/api/
  └── admin.ts (add auth functions)

/frontend/src/store/
  └── authStore.ts

/frontend/src/components/
  └── ProtectedRoute.tsx
```

### Implementation Details

**JWT Token Structure:**
```json
{
  "sub": "admin-username",
  "role": "admin",
  "exp": 1729641600,
  "iat": 1729555200
}
```

**Login Endpoint Contract:**
```typescript
// POST /api/admin/auth/login
Request: {
  username: string
  password: string
}

Response: {
  success: boolean
  message: string
  // Token sent via HttpOnly cookie, not in response body
}

Cookie: {
  name: "admin_token"
  httpOnly: true
  secure: true (production only)
  sameSite: "Strict"
  maxAge: 86400 (24 hours)
}
```

**Protected Route Pattern:**
```typescript
<Route path="/admin/*" element={
  <ProtectedRoute>
    <AdminLayout />
  </ProtectedRoute>
} />
```

### References

- [Tech Spec Epic 2: docs/tech-spec-epic-2-8-summary.md#Epic 2]
- [Solution Architecture: docs/solution-architecture.md#ADR-008: JWT for Admin Authentication]
- [Epics Document: docs/epics.md#Story 2.1]
- [Security Section: docs/solution-architecture.md#12.1 Authentication and Authorization]

### Testing Strategy

**Unit Tests:**
- JWT generation with correct claims and expiration
- Password validation logic
- Auth state management in Zustand store

**Integration Tests:**
- Login endpoint returns 200 OK with valid credentials
- Login endpoint returns 401 Unauthorized with invalid credentials
- Token correctly set in HttpOnly cookie
- Protected endpoints reject requests without valid token

**E2E Tests (deferred to Story 2.3):**
- Full flow: Navigate to /admin → Redirected to login → Enter credentials → Login successful → Redirected to admin dashboard

## Dev Agent Record

### Context Reference

- `docs/stories/epic_2/story-context-2.1.xml` (Generated: 2025-10-17)

### Agent Model Used

claude-sonnet-4-5-20250929

### Debug Log References

### Completion Notes List

### File List
