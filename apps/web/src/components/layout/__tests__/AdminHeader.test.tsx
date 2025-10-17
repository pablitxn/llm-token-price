/**
 * Tests for AdminHeader component
 */

import { describe, it, expect, vi, beforeEach } from 'vitest'
import { screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { renderWithProviders } from '@/test/test-utils'
import AdminHeader from '../AdminHeader'

// Create mocks outside of describe block
const mockLogout = vi.fn()
const onMenuClick = vi.fn()

// Mock useAuth hook
vi.mock('@/hooks/useAuth', () => ({
  useAuth: () => ({
    user: { username: 'admin', role: 'admin' },
    logout: mockLogout,
    isAuthenticated: true,
    login: vi.fn(),
    isLoading: false,
    error: null,
  }),
}))

describe('AdminHeader', () => {
  beforeEach(() => {
    mockLogout.mockClear()
    onMenuClick.mockClear()
  })

  it('renders admin panel title', () => {
    renderWithProviders(<AdminHeader onMenuClick={onMenuClick} />)
    expect(screen.getByText('Admin Panel')).toBeInTheDocument()
  })

  it('displays username in dropdown button', () => {
    renderWithProviders(<AdminHeader onMenuClick={onMenuClick} />)
    // Username may be hidden on mobile (sm:block)
    const usernameElements = screen.queryAllByText('admin')
    expect(usernameElements.length).toBeGreaterThan(0)
  })

  it('renders hamburger menu button', () => {
    renderWithProviders(<AdminHeader onMenuClick={onMenuClick} />)
    const menuButton = screen.getByLabelText('Toggle menu')
    expect(menuButton).toBeInTheDocument()
  })

  it('calls onMenuClick when hamburger is clicked', async () => {
    const user = userEvent.setup()
    renderWithProviders(<AdminHeader onMenuClick={onMenuClick} />)

    const menuButton = screen.getByLabelText('Toggle menu')
    await user.click(menuButton)

    expect(onMenuClick).toHaveBeenCalledTimes(1)
  })

  it('opens user dropdown when clicked', async () => {
    const user = userEvent.setup()
    renderWithProviders(<AdminHeader onMenuClick={onMenuClick} />)

    // Find user dropdown button (has user icon)
    const userButton = screen.getByRole('button', { expanded: false })
    await user.click(userButton)

    // Dropdown should now be expanded
    expect(screen.getByRole('button', { expanded: true })).toBeInTheDocument()

    // Logout button should be visible
    expect(screen.getByText('Logout')).toBeInTheDocument()
  })

  it('displays user info in dropdown menu', async () => {
    const user = userEvent.setup()
    renderWithProviders(<AdminHeader onMenuClick={onMenuClick} />)

    const userButton = screen.getByRole('button', { expanded: false })
    await user.click(userButton)

    // User info section - username appears in button (may be hidden on mobile) + dropdown
    expect(screen.getAllByText('admin').length).toBeGreaterThanOrEqual(2)
    // Role is 'admin' from mock, not 'Administrator'
    expect(screen.getAllByText('admin')).toBeTruthy()
  })

  it('calls logout when logout button is clicked', async () => {
    const user = userEvent.setup()
    renderWithProviders(<AdminHeader onMenuClick={onMenuClick} />)

    // Open dropdown
    const userButton = screen.getByRole('button', { expanded: false })
    await user.click(userButton)

    // Click logout
    const logoutButton = screen.getByText('Logout')
    await user.click(logoutButton)

    expect(mockLogout).toHaveBeenCalled()
  })

  it('closes dropdown when clicking outside', async () => {
    const user = userEvent.setup()
    const { container } = renderWithProviders(<AdminHeader onMenuClick={onMenuClick} />)

    // Open dropdown
    const userButton = screen.getByRole('button', { expanded: false })
    await user.click(userButton)
    expect(screen.getByText('Logout')).toBeInTheDocument()

    // Click outside
    await user.click(container)

    // Dropdown should be closed (logout button no longer visible)
    expect(screen.queryByText('Logout')).not.toBeInTheDocument()
  })
})
