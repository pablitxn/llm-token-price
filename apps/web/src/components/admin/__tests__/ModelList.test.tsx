/**
 * ModelList Component Tests
 * Tests Story 2.3 acceptance criteria for the models table component
 */

import { describe, it, expect, vi } from 'vitest'
import { render, screen } from '@testing-library/react'
import { userEvent } from '@testing-library/user-event'
import { ModelList } from '../ModelList'
import type { AdminModelDto } from '@/types/admin'

// Mock useNavigate from react-router-dom
const mockNavigate = vi.fn()
vi.mock('react-router-dom', () => ({
  useNavigate: () => mockNavigate,
}))

// Sample test data
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
  {
    id: '2',
    name: 'Claude 3 Opus',
    provider: 'Anthropic',
    version: null,
    status: 'active',
    inputPricePer1M: 15.0,
    outputPricePer1M: 75.0,
    currency: 'USD',
    isActive: true,
    createdAt: '2024-01-12T09:00:00Z',
    updatedAt: '2024-01-16T11:00:00Z',
    capabilities: null,
    topBenchmarks: [],
  },
  {
    id: '3',
    name: 'Gemini Pro',
    provider: 'Google',
    version: null,
    status: 'deprecated',
    inputPricePer1M: 0.5,
    outputPricePer1M: 1.5,
    currency: 'USD',
    isActive: false,
    createdAt: '2024-01-05T07:00:00Z',
    updatedAt: '2024-01-14T09:00:00Z',
    capabilities: null,
    topBenchmarks: [],
  },
]

describe('ModelList Component', () => {
  /**
   * AC 2.3.1, AC 2.3.2: Table renders with correct columns
   */
  it('should render table with correct columns', () => {
    render(<ModelList models={mockModels} />)

    // Check table headers
    expect(screen.getByText(/Name/i)).toBeInTheDocument()
    expect(screen.getByText(/Provider/i)).toBeInTheDocument()
    expect(screen.getByText(/Input Price/i)).toBeInTheDocument()
    expect(screen.getByText(/Output Price/i)).toBeInTheDocument()
    expect(screen.getByText(/Status/i)).toBeInTheDocument()
    expect(screen.getByText(/Last Updated/i)).toBeInTheDocument()
    expect(screen.getByText(/Actions/i)).toBeInTheDocument()
  })

  /**
   * AC 2.3.2: Table shows model data correctly
   */
  it('should display model data correctly', () => {
    render(<ModelList models={mockModels} />)

    // Check first model data
    expect(screen.getByText('GPT-4')).toBeInTheDocument()
    expect(screen.getByText('OpenAI')).toBeInTheDocument()
    expect(screen.getByText(/\$30\.000000/)).toBeInTheDocument()
    expect(screen.getByText(/\$60\.000000/)).toBeInTheDocument()

    // Check second model
    expect(screen.getByText('Claude 3 Opus')).toBeInTheDocument()
    expect(screen.getByText('Anthropic')).toBeInTheDocument()
  })

  /**
   * AC 2.3.2: Prices formatted with 6 decimal places and currency symbol
   */
  it('should format prices with 6 decimal places', () => {
    render(<ModelList models={mockModels} />)

    // Input price for GPT-4
    expect(screen.getByText('$30.000000')).toBeInTheDocument()
    // Output price for GPT-4
    expect(screen.getByText('$60.000000')).toBeInTheDocument()
    // Input price for Claude 3 Opus
    expect(screen.getByText('$15.000000')).toBeInTheDocument()
  })

  /**
   * AC 2.3.2: Status badges display with correct colors
   */
  it('should display status badges with correct styling', () => {
    render(<ModelList models={mockModels} />)

    const statusBadges = screen.getAllByText(/active|deprecated/i)
    expect(statusBadges.length).toBeGreaterThan(0)

    // Active status should have green badge
    const activeBadges = statusBadges.filter((badge) => badge.textContent === 'active')
    activeBadges.forEach((badge) => {
      expect(badge).toHaveClass('bg-green-100')
      expect(badge).toHaveClass('text-green-800')
    })

    // Deprecated status should have red badge
    const deprecatedBadges = statusBadges.filter((badge) => badge.textContent === 'deprecated')
    deprecatedBadges.forEach((badge) => {
      expect(badge).toHaveClass('bg-red-100')
      expect(badge).toHaveClass('text-red-800')
    })
  })

  /**
   * AC 2.3.1: Inactive models should display "Inactive" badge
   */
  it('should display inactive badge for inactive models', () => {
    render(<ModelList models={mockModels} />)

    const inactiveBadges = screen.getAllByText(/Inactive/i)
    expect(inactiveBadges.length).toBe(1) // Only Gemini Pro is inactive

    inactiveBadges.forEach((badge) => {
      expect(badge).toHaveClass('bg-gray-100')
      expect(badge).toHaveClass('text-gray-800')
    })
  })

  /**
   * AC 2.3.5: Edit button navigates to edit page
   */
  it('should navigate to edit page when edit button clicked', async () => {
    const user = userEvent.setup()
    render(<ModelList models={mockModels} />)

    const editButtons = screen.getAllByRole('button', { name: /Edit/i })
    expect(editButtons.length).toBe(3)

    // Click first edit button (table is sorted by updatedAt DESC, so Claude 3 Opus is first)
    await user.click(editButtons[0])

    // Should navigate to edit page for the first visible model (Claude 3 Opus, id: 2)
    expect(mockNavigate).toHaveBeenCalledWith('/admin/models/2/edit')
  })

  /**
   * AC 2.3.6: Delete button triggers callback
   */
  it('should trigger delete callback when delete button clicked', async () => {
    const user = userEvent.setup()
    const mockOnDelete = vi.fn()
    render(<ModelList models={mockModels} onDeleteClick={mockOnDelete} />)

    const deleteButtons = screen.getAllByRole('button', { name: /Delete/i })
    expect(deleteButtons.length).toBe(3)

    // Click first delete button (table is sorted by updatedAt DESC, so Claude 3 Opus is first)
    await user.click(deleteButtons[0])

    // Should trigger callback with the first visible model (Claude 3 Opus, mockModels[1])
    expect(mockOnDelete).toHaveBeenCalledWith(mockModels[1])
  })

  /**
   * AC 2.3.2: Table sorting - shows sort indicator when header clicked
   */
  it('should display sort indicators when column headers clicked', async () => {
    const user = userEvent.setup()
    render(<ModelList models={mockModels} />)

    // Default sort is by updatedAt DESC, so check for down arrow
    const updatedAtHeader = screen.getByText(/Last Updated/i)
    expect(updatedAtHeader.textContent).toContain('↓')

    // Click name header to sort ascending
    const nameHeader = screen.getByText(/Name/i).closest('th') as HTMLElement
    await user.click(nameHeader)

    // Should now show up arrow for ascending sort
    expect(nameHeader.textContent).toContain('↑')

    // Click again to sort descending
    await user.click(nameHeader)

    // Should now show down arrow for descending sort
    expect(nameHeader.textContent).toContain('↓')
  })

  /**
   * AC 2.3.1: Empty state when no models
   */
  it('should display empty state when no models', () => {
    render(<ModelList models={[]} />)

    expect(screen.getByText(/No models available/i)).toBeInTheDocument()
  })

  /**
   * AC 2.3.3: Empty state with search term
   */
  it('should display search-specific empty state when no results', () => {
    render(<ModelList models={[]} searchTerm="nonexistent" />)

    expect(screen.getByText(/No models found matching your search/i)).toBeInTheDocument()
  })

  /**
   * AC 2.3.2: Relative timestamps display correctly
   */
  it('should display relative timestamps', () => {
    render(<ModelList models={mockModels} />)

    // Should show relative time (e.g., "X days ago")
    const relativeTimeCells = screen.getAllByText(/ago/i)
    expect(relativeTimeCells.length).toBeGreaterThan(0)
  })
})
