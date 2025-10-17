/**
 * Tests for AdminDashboardPage component
 */

import { describe, it, expect, vi } from 'vitest'
import { screen } from '@testing-library/react'
import { renderWithProviders } from '@/test/test-utils'
import AdminDashboardPage from '../AdminDashboardPage'

// Mock the useAuth hook
vi.mock('@/hooks/useAuth', () => ({
  useAuth: () => ({
    user: { username: 'testadmin', role: 'admin' },
    isAuthenticated: true,
    login: vi.fn(),
    logout: vi.fn(),
    isLoading: false,
    error: null,
  }),
}))

describe('AdminDashboardPage', () => {
  it('renders dashboard title', () => {
    renderWithProviders(<AdminDashboardPage />)
    expect(screen.getByText('Dashboard')).toBeInTheDocument()
  })

  it('displays welcome message with username', () => {
    renderWithProviders(<AdminDashboardPage />)
    expect(screen.getByText(/Welcome back,/i)).toBeInTheDocument()
    expect(screen.getByText('testadmin')).toBeInTheDocument()
  })

  it('renders quick stats section', () => {
    renderWithProviders(<AdminDashboardPage />)
    expect(screen.getByText('Quick Stats')).toBeInTheDocument()
    expect(screen.getByText('Total Models')).toBeInTheDocument()
    expect(screen.getByText('Active Benchmarks')).toBeInTheDocument()
    expect(screen.getByText('Last Updated')).toBeInTheDocument()
  })

  it('displays placeholder stat values', () => {
    renderWithProviders(<AdminDashboardPage />)
    const placeholders = screen.getAllByText('--')
    expect(placeholders).toHaveLength(3)
  })

  it('shows future implementation message', () => {
    renderWithProviders(<AdminDashboardPage />)
    expect(
      screen.getByText(/Admin panel features will be implemented in future stories/i)
    ).toBeInTheDocument()
  })
})
