/**
 * Tests for AdminLayout component
 */

import { describe, it, expect, vi } from 'vitest'
import { screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { Route, Routes } from 'react-router-dom'
import { renderWithProviders } from '@/test/test-utils'
import AdminLayout from '../AdminLayout'

// Mock child components to simplify testing
vi.mock('../AdminSidebar', () => ({
  default: ({ isOpen, onClose }: { isOpen: boolean; onClose: () => void }) => (
    <div data-testid="admin-sidebar" data-open={isOpen}>
      <button onClick={onClose}>Close Sidebar</button>
    </div>
  ),
}))

vi.mock('../AdminHeader', () => ({
  default: ({ onMenuClick }: { onMenuClick: () => void }) => (
    <div data-testid="admin-header">
      <button onClick={onMenuClick} data-testid="menu-button">
        Toggle Menu
      </button>
    </div>
  ),
}))

describe('AdminLayout', () => {
  it('renders sidebar and header components', () => {
    renderWithProviders(
      <Routes>
        <Route path="/admin" element={<AdminLayout />}>
          <Route index element={<div>Test Content</div>} />
        </Route>
      </Routes>,
      { initialRoute: '/admin' }
    )

    expect(screen.getByTestId('admin-sidebar')).toBeInTheDocument()
    expect(screen.getByTestId('admin-header')).toBeInTheDocument()
  })

  it('renders child routes via Outlet', () => {
    renderWithProviders(
      <Routes>
        <Route path="/admin" element={<AdminLayout />}>
          <Route index element={<div>Dashboard Content</div>} />
        </Route>
      </Routes>,
      { initialRoute: '/admin' }
    )

    expect(screen.getByText('Dashboard Content')).toBeInTheDocument()
  })

  it('sidebar is closed by default', () => {
    renderWithProviders(
      <Routes>
        <Route path="/admin" element={<AdminLayout />}>
          <Route index element={<div>Test</div>} />
        </Route>
      </Routes>,
      { initialRoute: '/admin' }
    )

    const sidebar = screen.getByTestId('admin-sidebar')
    expect(sidebar).toHaveAttribute('data-open', 'false')
  })

  it('opens sidebar when menu button is clicked', async () => {
    const user = userEvent.setup()
    renderWithProviders(
      <Routes>
        <Route path="/admin" element={<AdminLayout />}>
          <Route index element={<div>Test</div>} />
        </Route>
      </Routes>,
      { initialRoute: '/admin' }
    )

    const menuButton = screen.getByTestId('menu-button')
    await user.click(menuButton)

    const sidebar = screen.getByTestId('admin-sidebar')
    expect(sidebar).toHaveAttribute('data-open', 'true')
  })

  it('closes sidebar when close button is clicked', async () => {
    const user = userEvent.setup()
    renderWithProviders(
      <Routes>
        <Route path="/admin" element={<AdminLayout />}>
          <Route index element={<div>Test</div>} />
        </Route>
      </Routes>,
      { initialRoute: '/admin' }
    )

    // Open sidebar
    const menuButton = screen.getByTestId('menu-button')
    await user.click(menuButton)

    // Close sidebar
    const closeButton = screen.getByText('Close Sidebar')
    await user.click(closeButton)

    const sidebar = screen.getByTestId('admin-sidebar')
    expect(sidebar).toHaveAttribute('data-open', 'false')
  })

  it('renders overlay when sidebar is open', async () => {
    const user = userEvent.setup()
    const { container } = renderWithProviders(
      <Routes>
        <Route path="/admin" element={<AdminLayout />}>
          <Route index element={<div>Test</div>} />
        </Route>
      </Routes>,
      { initialRoute: '/admin' }
    )

    // Initially no overlay
    expect(container.querySelector('.bg-black\\/50')).not.toBeInTheDocument()

    // Open sidebar
    const menuButton = screen.getByTestId('menu-button')
    await user.click(menuButton)

    // Overlay should appear
    expect(container.querySelector('.bg-black\\/50')).toBeInTheDocument()
  })

  it('closes sidebar when clicking overlay', async () => {
    const user = userEvent.setup()
    const { container } = renderWithProviders(
      <Routes>
        <Route path="/admin" element={<AdminLayout />}>
          <Route index element={<div>Test</div>} />
        </Route>
      </Routes>,
      { initialRoute: '/admin' }
    )

    // Open sidebar
    const menuButton = screen.getByTestId('menu-button')
    await user.click(menuButton)

    const sidebar = screen.getByTestId('admin-sidebar')
    expect(sidebar).toHaveAttribute('data-open', 'true')

    // Click overlay
    const overlay = container.querySelector('.bg-black\\/50')
    if (overlay) {
      await user.click(overlay)
    }

    expect(sidebar).toHaveAttribute('data-open', 'false')
  })

  it('has correct layout structure', () => {
    const { container } = renderWithProviders(
      <Routes>
        <Route path="/admin" element={<AdminLayout />}>
          <Route index element={<div>Test</div>} />
        </Route>
      </Routes>,
      { initialRoute: '/admin' }
    )

    const layout = container.querySelector('.min-h-screen.bg-slate-50.flex')
    expect(layout).toBeInTheDocument()

    const mainContent = container.querySelector('main.flex-1.p-6')
    expect(mainContent).toBeInTheDocument()
  })
})
