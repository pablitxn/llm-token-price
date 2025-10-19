/**
 * Tests for AdminModelsPage component
 * Story 2.3: Build Models List View in Admin Panel
 */

import { describe, it, expect, vi, beforeEach } from 'vitest'
import { screen, waitFor } from '@testing-library/react'
import { userEvent } from '@testing-library/user-event'
import { renderWithProviders } from '@/test/test-utils'
import AdminModelsPage from '../AdminModelsPage'
import type { AdminModelDto } from '@/types/admin'

// Mock data
const mockModels: AdminModelDto[] = [
  {
    id: '1',
    name: 'GPT-4',
    provider: 'OpenAI',
    version: '0613',
    status: 'active',
    inputPricePer1M: 30.0,
    outputPricePer1M: 60.0,
    currency: 'USD',
    isActive: true,
    createdAt: '2024-01-10T08:00:00Z',
    updatedAt: '2024-01-15T10:30:00Z',
    capabilities: null,
    topBenchmarks: [],
  },
]

// Mock functions - must be declared with vi.hoisted() for use in vi.mock()
const { mockRefetch, mockMutateAsync, mockUseAdminModels, mockUseDeleteModel } = vi.hoisted(() => ({
  mockRefetch: vi.fn(),
  mockMutateAsync: vi.fn(),
  mockUseAdminModels: vi.fn(),
  mockUseDeleteModel: vi.fn(),
}))

// Mock the useAdminModels and useDeleteModel hooks
vi.mock('@/hooks/useAdminModels', () => ({
  useAdminModels: mockUseAdminModels,
  useDeleteModel: mockUseDeleteModel,
}))

beforeEach(() => {
  vi.clearAllMocks()
  // Default mock setup for most tests (empty data)
  mockUseAdminModels.mockReturnValue({
    data: [],
    isLoading: false,
    error: null,
    refetch: mockRefetch,
    isFetching: false,
  })
  mockUseDeleteModel.mockReturnValue({
    mutateAsync: mockMutateAsync,
    isLoading: false,
    error: null,
  })
})

describe('AdminModelsPage', () => {
  it('renders models page title', () => {
    renderWithProviders(<AdminModelsPage />)
    expect(screen.getByText('Models')).toBeInTheDocument()
  })

  it('displays description text', () => {
    renderWithProviders(<AdminModelsPage />)
    expect(
      screen.getByText(/View and manage all LLM models in the system/i)
    ).toBeInTheDocument()
  })

  it('renders Add New Model button', () => {
    renderWithProviders(<AdminModelsPage />)
    expect(screen.getByRole('button', { name: /Add New Model/i })).toBeInTheDocument()
  })

  it('renders search input', () => {
    renderWithProviders(<AdminModelsPage />)
    const searchInput = screen.getByPlaceholderText(/Search by model name or provider/i)
    expect(searchInput).toBeInTheDocument()
  })

  it('displays empty state when no models', async () => {
    renderWithProviders(<AdminModelsPage />)

    await waitFor(() => {
      expect(screen.getByText(/No models available/i)).toBeInTheDocument()
    })
  })
})

describe('AdminModelsPage - Delete Functionality', () => {
  beforeEach(() => {
    // Reconfigure the mock with models data for delete tests
    mockUseAdminModels.mockReturnValue({
      data: mockModels,
      isLoading: false,
      error: null,
      refetch: mockRefetch,
      isFetching: false,
    })
    mockUseDeleteModel.mockReturnValue({
      mutateAsync: mockMutateAsync,
      isLoading: false,
      error: null,
    })
  })

  it('shows delete confirmation modal when delete button is clicked', async () => {
    const user = userEvent.setup()
    renderWithProviders(<AdminModelsPage />)

    // Wait for the delete button to appear
    await waitFor(() => {
      expect(screen.getByRole('button', { name: /Delete GPT-4/i })).toBeInTheDocument()
    })

    // Click the delete button
    const deleteButton = screen.getByRole('button', { name: /Delete GPT-4/i })
    await user.click(deleteButton)

    // Verify modal is shown with heading and buttons
    await waitFor(() => {
      expect(screen.getByRole('heading', { name: 'Delete Model' })).toBeInTheDocument()
      // Verify delete and cancel buttons are present
      expect(screen.getByRole('button', { name: /^Delete$/i })).toBeInTheDocument()
      expect(screen.getByRole('button', { name: /Cancel/i })).toBeInTheDocument()
    })
  })

  it('calls delete mutation when delete is confirmed', async () => {
    const user = userEvent.setup()
    mockMutateAsync.mockResolvedValue(undefined)

    renderWithProviders(<AdminModelsPage />)

    // Wait for and click delete button
    await waitFor(() => {
      expect(screen.getByRole('button', { name: /Delete GPT-4/i })).toBeInTheDocument()
    })

    const deleteButton = screen.getByRole('button', { name: /Delete GPT-4/i })
    await user.click(deleteButton)

    // Wait for modal
    await waitFor(() => {
      expect(screen.getByRole('heading', { name: 'Delete Model' })).toBeInTheDocument()
    })

    const confirmButton = screen.getByRole('button', { name: /^Delete$/i })
    await user.click(confirmButton)

    // Verify mutation was called with correct ID
    await waitFor(() => {
      expect(mockMutateAsync).toHaveBeenCalledWith('1')
    })
  })

  it('closes modal and does not call mutation when cancel is clicked', async () => {
    const user = userEvent.setup()
    renderWithProviders(<AdminModelsPage />)

    // Wait for and click delete button
    await waitFor(() => {
      expect(screen.getByRole('button', { name: /Delete GPT-4/i })).toBeInTheDocument()
    })

    const deleteButton = screen.getByRole('button', { name: /Delete GPT-4/i })
    await user.click(deleteButton)

    // Wait for modal
    await waitFor(() => {
      expect(screen.getByText('Delete Model')).toBeInTheDocument()
    })

    // Click cancel
    const cancelButton = screen.getByRole('button', { name: /Cancel/i })
    await user.click(cancelButton)

    // Verify modal is closed
    await waitFor(() => {
      expect(screen.queryByText('Delete Model')).not.toBeInTheDocument()
    })

    // Verify mutation was not called
    expect(mockMutateAsync).not.toHaveBeenCalled()
  })

  it('closes modal on successful delete', async () => {
    const user = userEvent.setup()
    mockMutateAsync.mockResolvedValue(undefined)

    renderWithProviders(<AdminModelsPage />)

    // Wait for and click delete button
    await waitFor(() => {
      expect(screen.getByRole('button', { name: /Delete GPT-4/i })).toBeInTheDocument()
    })

    const deleteButton = screen.getByRole('button', { name: /Delete GPT-4/i })
    await user.click(deleteButton)

    // Confirm delete
    await waitFor(() => {
      expect(screen.getByText('Delete Model')).toBeInTheDocument()
    })

    const confirmButton = screen.getByRole('button', { name: /^Delete$/i })
    await user.click(confirmButton)

    // Verify modal is closed after successful delete
    await waitFor(() => {
      expect(screen.queryByText('Delete Model')).not.toBeInTheDocument()
    })
  })

  it('keeps modal open and logs error when delete fails', async () => {
    const user = userEvent.setup()
    const consoleErrorSpy = vi.spyOn(console, 'error').mockImplementation(() => {})
    const deleteError = new Error('Failed to delete model')
    mockMutateAsync.mockRejectedValue(deleteError)

    renderWithProviders(<AdminModelsPage />)

    // Wait for and click delete button
    await waitFor(() => {
      expect(screen.getByRole('button', { name: /Delete GPT-4/i })).toBeInTheDocument()
    })

    const deleteButton = screen.getByRole('button', { name: /Delete GPT-4/i })
    await user.click(deleteButton)

    // Confirm delete
    await waitFor(() => {
      expect(screen.getByText('Delete Model')).toBeInTheDocument()
    })

    const confirmButton = screen.getByRole('button', { name: /^Delete$/i })
    await user.click(confirmButton)

    // Verify modal stays open on error
    await waitFor(() => {
      expect(screen.getByText('Delete Model')).toBeInTheDocument()
    })

    // Verify error was logged
    expect(consoleErrorSpy).toHaveBeenCalledWith('Failed to delete model:', deleteError)

    consoleErrorSpy.mockRestore()
  })
})

describe('AdminModelsPage - URL Query Params Persistence', () => {
  beforeEach(() => {
    // Reset mocks to default for URL params tests
    mockUseAdminModels.mockReturnValue({
      data: [],
      isLoading: false,
      error: null,
      refetch: mockRefetch,
      isFetching: false,
    })
    mockUseDeleteModel.mockReturnValue({
      mutateAsync: mockMutateAsync,
      isLoading: false,
      error: null,
    })
  })

  it('reads search term from URL params on mount', () => {
    // Render with search param in URL
    renderWithProviders(<AdminModelsPage />, {
      initialEntries: ['/admin/models?search=GPT'],
    })

    // Verify search input has the value from URL
    const searchInput = screen.getByPlaceholderText(/Search by model name or provider/i) as HTMLInputElement
    expect(searchInput.value).toBe('GPT')

    // Verify the "Searching for" text is displayed
    expect(screen.getByText(/Searching for:/i)).toBeInTheDocument()
    expect(screen.getByText('GPT')).toBeInTheDocument()
  })

  it('updates search input value when user types', async () => {
    const user = userEvent.setup()
    renderWithProviders(<AdminModelsPage />)

    // Type in search input
    const searchInput = screen.getByPlaceholderText(/Search by model name or provider/i)
    await user.type(searchInput, 'Claude')

    // Verify input value is updated
    await waitFor(() => {
      expect((searchInput as HTMLInputElement).value).toBe('Claude')
    })

    // Verify the "Searching for" text is displayed
    await waitFor(() => {
      expect(screen.getByText(/Searching for:/i)).toBeInTheDocument()
    })
  })

  it('clears search input when clear button is clicked', async () => {
    const user = userEvent.setup()
    renderWithProviders(<AdminModelsPage />, {
      initialEntries: ['/admin/models?search=GPT'],
    })

    // Verify search input has initial value
    const searchInput = screen.getByPlaceholderText(/Search by model name or provider/i) as HTMLInputElement
    expect(searchInput.value).toBe('GPT')

    // Click clear button
    const clearButton = screen.getByLabelText(/Clear search/i)
    await user.click(clearButton)

    // Verify search input is cleared
    await waitFor(() => {
      expect(searchInput.value).toBe('')
    })

    // Verify the "Searching for" text is no longer displayed
    expect(screen.queryByText(/Searching for:/i)).not.toBeInTheDocument()
  })

  it('preserves search term on component remount', () => {
    // First render with search term
    const { unmount } = renderWithProviders(<AdminModelsPage />, {
      initialEntries: ['/admin/models?search=Anthropic'],
    })

    // Verify search input has the value
    let searchInput = screen.getByPlaceholderText(/Search by model name or provider/i) as HTMLInputElement
    expect(searchInput.value).toBe('Anthropic')

    // Unmount and remount with same URL
    unmount()
    renderWithProviders(<AdminModelsPage />, {
      initialEntries: ['/admin/models?search=Anthropic'],
    })

    // Verify search term is still present after remount
    searchInput = screen.getByPlaceholderText(/Search by model name or provider/i) as HTMLInputElement
    expect(searchInput.value).toBe('Anthropic')
  })
})
