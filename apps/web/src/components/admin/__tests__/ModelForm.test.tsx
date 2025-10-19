/**
 * ModelForm Component Tests
 * Tests Story 2.4 acceptance criteria for model creation form
 */

import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { BrowserRouter } from 'react-router-dom'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { ModelForm } from '../ModelForm'

// Mock the admin API
vi.mock('@/api/admin', () => ({
  createModel: vi.fn(),
}))

// Mock useNavigate
const mockNavigate = vi.fn()
vi.mock('react-router-dom', async () => {
  const actual = await vi.importActual('react-router-dom')
  return {
    ...actual,
    useNavigate: () => mockNavigate,
  }
})

describe('ModelForm', () => {
  let queryClient: QueryClient

  beforeEach(() => {
    queryClient = new QueryClient({
      defaultOptions: {
        queries: { retry: false },
        mutations: { retry: false },
      },
    })
    vi.clearAllMocks()
  })

  const renderForm = () => {
    return render(
      <BrowserRouter>
        <QueryClientProvider client={queryClient}>
          <ModelForm />
        </QueryClientProvider>
      </BrowserRouter>
    )
  }

  /**
   * AC 2.4.1: Form renders with all required fields
   */
  it('renders all required form fields', () => {
    renderForm()

    // Basic Info Section
    expect(screen.getByLabelText(/model name/i)).toBeInTheDocument()
    expect(screen.getByLabelText(/provider/i)).toBeInTheDocument()
    expect(screen.getByLabelText(/version/i)).toBeInTheDocument()
    expect(screen.getByLabelText(/release date/i)).toBeInTheDocument()
    expect(screen.getByLabelText(/status/i)).toBeInTheDocument()

    // Pricing Section
    expect(screen.getByLabelText(/input price per 1m tokens/i)).toBeInTheDocument()
    expect(screen.getByLabelText(/output price per 1m tokens/i)).toBeInTheDocument()
    expect(screen.getByLabelText(/currency/i)).toBeInTheDocument()
    expect(screen.getByLabelText(/pricing valid from/i)).toBeInTheDocument()
    expect(screen.getByLabelText(/pricing valid to/i)).toBeInTheDocument()

    // Form Actions
    expect(screen.getByRole('button', { name: /save/i })).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /cancel/i })).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /reset form/i })).toBeInTheDocument()
  })

  /**
   * AC 2.4.2: Form validation ensures required fields completed
   */
  it('shows validation errors for required fields when submitting empty form', async () => {
    const user = userEvent.setup()
    renderForm()

    const saveButton = screen.getByRole('button', { name: /save/i })
    await user.click(saveButton)

    // Wait for validation errors to appear
    await waitFor(() => {
      expect(screen.getByText(/name is required/i)).toBeInTheDocument()
    })

    expect(screen.getByText(/provider is required/i)).toBeInTheDocument()
  })

  /**
   * AC 2.4.3: Form validation ensures prices are positive numbers
   */
  it('validates that prices must be positive numbers', async () => {
    const user = userEvent.setup()
    renderForm()

    const nameInput = screen.getByLabelText(/model name/i)
    const providerInput = screen.getByLabelText(/provider/i)
    const inputPrice = screen.getByLabelText(/input price per 1m tokens/i)

    // Fill required string fields first
    await user.type(nameInput, 'Test Model')
    await user.type(providerInput, 'Test Provider')

    // Enter a negative price (should trigger validation error)
    await user.clear(inputPrice)
    await user.type(inputPrice, '0.001')
    await user.clear(inputPrice)
    await user.type(inputPrice, '-1')

    // Submit form
    await user.click(screen.getByRole('button', { name: /save/i }))

    // Price validation should trigger (React Hook Form validates on submit)
    await waitFor(() => {
      expect(screen.getByText(/must be greater than 0/i)).toBeInTheDocument()
    })
  })

  /**
   * AC 2.4.6: Error displays validation messages
   */
  it('displays inline validation errors below fields', async () => {
    const user = userEvent.setup()
    renderForm()

    const nameInput = screen.getByLabelText(/model name/i)

    // Focus and blur without entering value
    await user.click(nameInput)
    await user.tab()

    // Click save to trigger validation
    await user.click(screen.getByRole('button', { name: /save/i }))

    await waitFor(() => {
      const nameError = screen.getByText(/name is required/i)
      expect(nameError).toBeInTheDocument()
      expect(nameError).toHaveClass('text-red-600') // Inline error styling
    })
  })

  /**
   * Test form reset functionality
   */
  it('resets form when Reset button is clicked', async () => {
    const user = userEvent.setup()
    renderForm()

    const nameInput = screen.getByLabelText(/model name/i) as HTMLInputElement
    const providerInput = screen.getByLabelText(/provider/i) as HTMLInputElement

    // Fill in some values
    await user.type(nameInput, 'GPT-4')
    await user.type(providerInput, 'OpenAI')

    expect(nameInput.value).toBe('GPT-4')
    expect(providerInput.value).toBe('OpenAI')

    // Click reset
    await user.click(screen.getByRole('button', { name: /reset form/i }))

    // Values should be cleared
    expect(nameInput.value).toBe('')
    expect(providerInput.value).toBe('')
  })

  /**
   * Test cancel navigation
   */
  it('navigates back to models list when Cancel button is clicked', async () => {
    const user = userEvent.setup()
    renderForm()

    await user.click(screen.getByRole('button', { name: /cancel/i }))

    expect(mockNavigate).toHaveBeenCalledWith('/admin/models')
  })

  /**
   * Test helper text for price format
   */
  it('displays helper text explaining price format', () => {
    renderForm()

    expect(
      screen.getByText(/enter prices per 1 million tokens with up to 6 decimal places/i)
    ).toBeInTheDocument()
  })

  /**
   * Test default values
   */
  it('sets default values for status and currency', () => {
    renderForm()

    const statusSelect = screen.getByLabelText(/status/i) as HTMLSelectElement
    const currencySelect = screen.getByLabelText(/currency/i) as HTMLSelectElement

    expect(statusSelect.value).toBe('active')
    expect(currencySelect.value).toBe('USD')
  })
})
