/**
 * Tests for AdminModelsPage component
 * Story 2.3: Build Models List View in Admin Panel
 */

import { describe, it, expect, vi } from 'vitest'
import { screen, waitFor } from '@testing-library/react'
import { renderWithProviders } from '@/test/test-utils'
import AdminModelsPage from '../AdminModelsPage'

// Mock the useAdminModels hook
vi.mock('@/hooks/useAdminModels', () => ({
  useAdminModels: vi.fn(() => ({
    data: [],
    isLoading: false,
    error: null,
    refetch: vi.fn(),
    isFetching: false,
  })),
}))

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
