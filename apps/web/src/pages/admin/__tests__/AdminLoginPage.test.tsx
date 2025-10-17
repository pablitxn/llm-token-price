/**
 * Test Suite: AdminLoginPage Component
 * Story: 2.1 - Admin Panel Authentication
 * Test Level: P1 (High Priority)
 *
 * Coverage:
 * - Client-side validation (Zod schema)
 * - Form error display
 * - Button loading states
 * - User interaction flows
 */

import { describe, test, expect, vi } from 'vitest'
import { screen, waitFor } from '@testing-library/react'
import { userEvent } from '@testing-library/user-event'
import { renderWithProviders } from '@/test/test-utils'
import AdminLoginPage from '../AdminLoginPage'

// Mock the useAuth hook
vi.mock('@/hooks/useAuth', () => ({
  useAuth: vi.fn(() => ({
    login: vi.fn(),
    isLoading: false,
    error: null,
  })),
}))

describe('AdminLoginPage - Client-Side Validation (P1)', () => {
  test('P1-001: shows validation error for empty username', async () => {
    const user = userEvent.setup()
    renderWithProviders(<AdminLoginPage />, {
      initialRoute: '/admin/login',
    })

    // Find form elements
    const usernameInput = screen.getByLabelText(/username/i)
    const passwordInput = screen.getByLabelText(/password/i)
    const submitButton = screen.getByRole('button', { name: /sign in/i })

    // Type in password to enable button, but leave username with invalid value
    await user.type(usernameInput, 'ab') // Only 2 characters (invalid)
    await user.type(passwordInput, 'password123')

    // Try to submit with invalid username
    await user.click(submitButton)

    // Assert: validation error should appear
    await waitFor(() => {
      expect(screen.getByText(/username must be at least 3 characters/i)).toBeInTheDocument()
    })

    // Assert: username field should have error styling
    expect(usernameInput).toHaveAttribute('aria-invalid', 'true')
  })

  test('P1-002: shows validation error for password < 6 characters', async () => {
    const user = userEvent.setup()
    renderWithProviders(<AdminLoginPage />, {
      initialRoute: '/admin/login',
    })

    const usernameInput = screen.getByLabelText(/username/i)
    const passwordInput = screen.getByLabelText(/password/i)
    const submitButton = screen.getByRole('button', { name: /sign in/i })

    // Fill username and short password (only 5 chars)
    await user.type(usernameInput, 'admin')
    await user.type(passwordInput, '12345')

    // Try to submit
    await user.click(submitButton)

    // Assert: validation error should appear
    await waitFor(() => {
      expect(screen.getByText(/password must be at least 6 characters/i)).toBeInTheDocument()
    })

    // Assert: password field should have error styling
    expect(passwordInput).toHaveAttribute('aria-invalid', 'true')
  })

  test('P1-003: displays Zod validation errors inline with proper ARIA attributes', async () => {
    const user = userEvent.setup()
    renderWithProviders(<AdminLoginPage />, {
      initialRoute: '/admin/login',
    })

    const usernameInput = screen.getByLabelText(/username/i)
    const passwordInput = screen.getByLabelText(/password/i)
    const submitButton = screen.getByRole('button', { name: /sign in/i })

    // Fill with invalid values (too short) to enable button
    await user.type(usernameInput, 'ab') // 2 chars (invalid)
    await user.type(passwordInput, '12345') // 5 chars (invalid)

    // Try to submit with invalid values
    await user.click(submitButton)

    // Assert: both validation errors should appear
    await waitFor(() => {
      expect(screen.getByText(/username must be at least 3 characters/i)).toBeInTheDocument()
      expect(screen.getByText(/password must be at least 6 characters/i)).toBeInTheDocument()
    })

    // Assert: error messages have proper ARIA roles
    const usernameError = screen.getByText(/username must be at least 3 characters/i)
    const passwordError = screen.getByText(/password must be at least 6 characters/i)

    expect(usernameError).toHaveAttribute('role', 'alert')
    expect(passwordError).toHaveAttribute('role', 'alert')

    // Assert: inputs are linked to their error messages via aria-describedby
    expect(usernameInput).toHaveAttribute('aria-describedby', 'username-error')
    expect(passwordInput).toHaveAttribute('aria-describedby', 'password-error')
  })

  test('P1-004: clears validation error when user corrects input', async () => {
    const user = userEvent.setup()
    renderWithProviders(<AdminLoginPage />, {
      initialRoute: '/admin/login',
    })

    const usernameInput = screen.getByLabelText(/username/i)
    const passwordInput = screen.getByLabelText(/password/i)
    const submitButton = screen.getByRole('button', { name: /sign in/i })

    // Fill password first to enable button
    await user.type(passwordInput, 'password123')

    // Trigger validation error with short username
    await user.type(usernameInput, 'ab') // Only 2 chars
    await user.click(submitButton)

    // Wait for error to appear
    await waitFor(() => {
      expect(screen.getByText(/username must be at least 3 characters/i)).toBeInTheDocument()
    })

    // Correct the input by typing more characters (don't clear, just append)
    await user.type(usernameInput, 'cde') // Now "abcde" = 5 chars

    // Assert: error should disappear immediately when user starts typing
    await waitFor(() => {
      expect(screen.queryByText(/username must be at least 3 characters/i)).not.toBeInTheDocument()
    })

    // Assert: aria-invalid should be false
    expect(usernameInput).toHaveAttribute('aria-invalid', 'false')
  })
})

describe('AdminLoginPage - Form Behavior (P1)', () => {
  test('P1-005: submit button is disabled when fields are empty', () => {
    renderWithProviders(<AdminLoginPage />, {
      initialRoute: '/admin/login',
    })

    const submitButton = screen.getByRole('button', { name: /sign in/i })

    // Assert: button should be disabled initially
    expect(submitButton).toBeDisabled()
    expect(submitButton).toHaveClass('cursor-not-allowed')
  })

  test('P1-006: submit button is enabled when both fields have values', async () => {
    const user = userEvent.setup()
    renderWithProviders(<AdminLoginPage />, {
      initialRoute: '/admin/login',
    })

    const usernameInput = screen.getByLabelText(/username/i)
    const passwordInput = screen.getByLabelText(/password/i)
    const submitButton = screen.getByRole('button', { name: /sign in/i })

    // Initially disabled
    expect(submitButton).toBeDisabled()

    // Fill both fields
    await user.type(usernameInput, 'admin')
    await user.type(passwordInput, 'password123')

    // Assert: button should now be enabled
    await waitFor(() => {
      expect(submitButton).not.toBeDisabled()
    })
  })

  test('P1-007: form inputs are disabled during loading state', async () => {
    // Mock useAuth hook with isLoading = true
    const { useAuth } = await import('@/hooks/useAuth')
    vi.mocked(useAuth).mockReturnValue({
      login: vi.fn(),
      logout: vi.fn(),
      isLoading: true,
      error: null,
      isAuthenticated: false,
      user: null,
    })

    renderWithProviders(<AdminLoginPage />, {
      initialRoute: '/admin/login',
    })

    const usernameInput = screen.getByLabelText(/username/i)
    const passwordInput = screen.getByLabelText(/password/i)
    const submitButton = screen.getByRole('button')

    // Assert: inputs should be disabled during loading
    expect(usernameInput).toBeDisabled()
    expect(passwordInput).toBeDisabled()
    expect(submitButton).toBeDisabled()

    // Assert: loading indicator should be visible
    expect(screen.getByText(/signing in\.\.\./i)).toBeInTheDocument()

    // Assert: loading spinner should be present
    const spinner = screen.getByRole('button').querySelector('svg.animate-spin')
    expect(spinner).toBeInTheDocument()

    // Restore mock
    vi.mocked(useAuth).mockRestore()
  })

  test('P1-008: displays authentication error message from API', async () => {
    const errorMessage = 'Invalid username or password'

    // Mock useAuth hook with error state
    const { useAuth } = await import('@/hooks/useAuth')
    vi.mocked(useAuth).mockReturnValue({
      login: vi.fn(),
      logout: vi.fn(),
      isLoading: false,
      error: errorMessage,
      isAuthenticated: false,
      user: null,
    })

    renderWithProviders(<AdminLoginPage />, {
      initialRoute: '/admin/login',
    })

    // Assert: error message should be displayed
    expect(screen.getByText(errorMessage)).toBeInTheDocument()

    // Assert: error alert has proper ARIA attributes
    const errorAlert = screen.getByRole('alert')
    expect(errorAlert).toHaveAttribute('aria-live', 'assertive')

    // Restore mock
    vi.mocked(useAuth).mockRestore()
  })
})

describe('AdminLoginPage - Accessibility (P2)', () => {
  test('P2-001: form has proper ARIA labels for screen readers', () => {
    renderWithProviders(<AdminLoginPage />, {
      initialRoute: '/admin/login',
    })

    const usernameInput = screen.getByLabelText(/username/i)
    const passwordInput = screen.getByLabelText(/password/i)

    // Assert: inputs have proper labels
    expect(usernameInput).toHaveAttribute('id', 'username')
    expect(passwordInput).toHaveAttribute('id', 'password')

    // Assert: inputs have autocomplete attributes
    expect(usernameInput).toHaveAttribute('autocomplete', 'username')
    expect(passwordInput).toHaveAttribute('autocomplete', 'current-password')
  })

  test('P2-002: form can be submitted via Enter key', async () => {
    const mockLogin = vi.fn()

    // Mock useAuth hook
    const { useAuth } = await import('@/hooks/useAuth')
    vi.mocked(useAuth).mockReturnValue({
      login: mockLogin,
      logout: vi.fn(),
      isLoading: false,
      error: null,
      isAuthenticated: false,
      user: null,
    })

    const user = userEvent.setup()
    renderWithProviders(<AdminLoginPage />, {
      initialRoute: '/admin/login',
    })

    const usernameInput = screen.getByLabelText(/username/i)
    const passwordInput = screen.getByLabelText(/password/i)

    // Fill form and press Enter
    await user.type(usernameInput, 'admin')
    await user.type(passwordInput, 'password123')
    await user.keyboard('{Enter}')

    // Assert: login should be called
    await waitFor(() => {
      expect(mockLogin).toHaveBeenCalledWith('admin', 'password123')
    })

    // Restore mock
    vi.mocked(useAuth).mockRestore()
  })

  test('P2-003: form has noValidate attribute to disable browser validation', () => {
    renderWithProviders(<AdminLoginPage />, {
      initialRoute: '/admin/login',
    })

    const form = screen.getByRole('button', { name: /sign in/i }).closest('form')

    // Assert: form should have noValidate to rely on Zod validation
    expect(form).toHaveAttribute('novalidate')
  })
})

describe('AdminLoginPage - Edge Cases (P2)', () => {
  test('P2-004: handles extremely long username input', async () => {
    const user = userEvent.setup()
    renderWithProviders(<AdminLoginPage />, {
      initialRoute: '/admin/login',
    })

    const usernameInput = screen.getByLabelText(/username/i)
    const submitButton = screen.getByRole('button', { name: /sign in/i })

    // Type username with 51 characters (exceeds 50 char limit)
    const longUsername = 'a'.repeat(51)
    await user.type(usernameInput, longUsername)
    await user.type(screen.getByLabelText(/password/i), 'password123')
    await user.click(submitButton)

    // Assert: validation error should appear
    await waitFor(() => {
      expect(screen.getByText(/username must not exceed 50 characters/i)).toBeInTheDocument()
    })
  })

  test('P2-005: handles extremely long password input', async () => {
    const user = userEvent.setup()
    renderWithProviders(<AdminLoginPage />, {
      initialRoute: '/admin/login',
    })

    const usernameInput = screen.getByLabelText(/username/i)
    const passwordInput = screen.getByLabelText(/password/i)
    const submitButton = screen.getByRole('button', { name: /sign in/i })

    // Type password with 101 characters (exceeds 100 char limit)
    const longPassword = 'a'.repeat(101)
    await user.type(usernameInput, 'admin')
    await user.type(passwordInput, longPassword)
    await user.click(submitButton)

    // Assert: validation error should appear
    await waitFor(() => {
      expect(screen.getByText(/password must not exceed 100 characters/i)).toBeInTheDocument()
    })
  })
})
