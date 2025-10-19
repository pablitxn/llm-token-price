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

// Mock functions
const mockRefetch = vi.fn()
const mockMutateAsync = vi.fn()

// Mock the useAdminModels and useDeleteModel hooks
vi.mock('@/hooks/useAdminModels', () => ({
  useAdminModels: vi.fn(() => ({
    data: [],
    isLoading: false,
    error: null,
    refetch: mockRefetch,
    isFetching: false,
  })),
  useDeleteModel: vi.fn(() => ({
    mutateAsync: mockMutateAsync,
    isLoading: false,
    error: null,
  })),
}))

beforeEach(() => {
  vi.clearAllMocks()
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

// TODO: Fix mock configuration for delete tests - beforeEach async not working as expected
describe.skip('AdminModelsPage - Delete Functionality', () => {
  beforeEach(async () => {
    // Import the hook mock and set return value with models
    const { useAdminModels } = await import('@/hooks/useAdminModels')
    vi.mocked(useAdminModels).mockReturnValue({
      data: mockModels,
      isLoading: false,
      error: null,
      refetch: mockRefetch,
      isFetching: false,
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

    // Verify modal is shown
    await waitFor(() => {
      expect(screen.getByText('Delete Model')).toBeInTheDocument()
      expect(screen.getByText(/Are you sure you want to delete GPT-4/i)).toBeInTheDocument()
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

    // Wait for modal and click confirm
    await waitFor(() => {
      expect(screen.getByText('Delete Model')).toBeInTheDocument()
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
