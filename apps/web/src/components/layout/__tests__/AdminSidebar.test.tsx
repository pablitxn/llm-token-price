/**
 * Tests for AdminSidebar component
 */

import { describe, it, expect, vi, beforeEach } from 'vitest'
import { screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { renderWithProviders } from '@/test/test-utils'
import AdminSidebar from '../AdminSidebar'

describe('AdminSidebar', () => {
  const onClose = vi.fn()

  beforeEach(() => {
    onClose.mockClear()
  })

  it('renders sidebar title', () => {
    renderWithProviders(<AdminSidebar isOpen={true} onClose={onClose} />)
    expect(screen.getByText('LLM Admin')).toBeInTheDocument()
  })

  it('renders all navigation items', () => {
    renderWithProviders(<AdminSidebar isOpen={true} onClose={onClose} />)

    expect(screen.getByText('Dashboard')).toBeInTheDocument()
    expect(screen.getByText('Models')).toBeInTheDocument()
    expect(screen.getByText('Benchmarks')).toBeInTheDocument()
  })

  it('renders version info in footer', () => {
    renderWithProviders(<AdminSidebar isOpen={true} onClose={onClose} />)
    expect(screen.getByText('Admin Panel v1.0')).toBeInTheDocument()
  })

  it('highlights active navigation item', () => {
    renderWithProviders(<AdminSidebar isOpen={true} onClose={onClose} />, {
      initialEntries: ['/admin/dashboard'],
    })

    const dashboardLink = screen.getByText('Dashboard').closest('a')
    expect(dashboardLink).toHaveClass('bg-slate-700')
    expect(dashboardLink).toHaveClass('font-medium')
  })

  it('does not highlight inactive navigation items', () => {
    renderWithProviders(<AdminSidebar isOpen={true} onClose={onClose} />, {
      initialEntries: ['/admin/dashboard'],
    })

    const modelsLink = screen.getByText('Models').closest('a')
    expect(modelsLink).toHaveClass('text-slate-300')
    expect(modelsLink).not.toHaveClass('bg-slate-700')
  })

  it('applies correct translation when sidebar is open', () => {
    const { container } = renderWithProviders(
      <AdminSidebar isOpen={true} onClose={onClose} />
    )
    const sidebar = container.querySelector('aside')
    expect(sidebar).toHaveClass('translate-x-0')
  })

  it('applies correct translation when sidebar is closed on mobile', () => {
    const { container } = renderWithProviders(
      <AdminSidebar isOpen={false} onClose={onClose} />
    )
    const sidebar = container.querySelector('aside')
    expect(sidebar).toHaveClass('-translate-x-full')
  })

  it('calls onClose when navigation link is clicked', async () => {
    const user = userEvent.setup()
    renderWithProviders(<AdminSidebar isOpen={true} onClose={onClose} />)

    const dashboardLink = screen.getByText('Dashboard')
    await user.click(dashboardLink)

    expect(onClose).toHaveBeenCalledTimes(1)
  })

  it('all navigation items have correct href attributes', () => {
    renderWithProviders(<AdminSidebar isOpen={true} onClose={onClose} />)

    const dashboardLink = screen.getByText('Dashboard').closest('a')
    const modelsLink = screen.getByText('Models').closest('a')
    const benchmarksLink = screen.getByText('Benchmarks').closest('a')

    expect(dashboardLink).toHaveAttribute('href', '/admin/dashboard')
    expect(modelsLink).toHaveAttribute('href', '/admin/models')
    expect(benchmarksLink).toHaveAttribute('href', '/admin/benchmarks')
  })
})
