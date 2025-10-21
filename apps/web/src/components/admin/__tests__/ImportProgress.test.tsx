/**
 * Tests for ImportProgress Component
 * Story 2.13 Task 12: CSV import progress indicator
 */

import { describe, it, expect } from 'vitest'
import { render, screen } from '@testing-library/react'
import { ImportProgress } from '../ImportProgress'

describe('ImportProgress', () => {
  const defaultProps = {
    fileName: 'benchmark-scores.csv',
    fileSize: 1024 * 100, // 100KB
  }

  it('renders file information correctly', () => {
    render(<ImportProgress {...defaultProps} />)

    expect(screen.getByText('Processing Import')).toBeInTheDocument()
    expect(screen.getByText('benchmark-scores.csv')).toBeInTheDocument()
    expect(screen.getByText(/Size: 100 KB/i)).toBeInTheDocument()
  })

  it('shows estimated rows when provided', () => {
    render(<ImportProgress {...defaultProps} estimatedRows={150} />)

    expect(screen.getByText(/Est\. 150 rows/i)).toBeInTheDocument()
  })

  it('shows current stage label', () => {
    const { container } = render(<ImportProgress {...defaultProps} />)

    // Check that a stage label is displayed
    const stageLabel = container.querySelector('.text-sm.font-medium.text-gray-700')
    expect(stageLabel).toBeInTheDocument()
    expect(stageLabel).toHaveTextContent(/Uploading file|Validating data|Importing records/)
  })

  it('shows progress bar with proper attributes', () => {
    render(<ImportProgress {...defaultProps} />)

    const progressBar = screen.getByRole('progressbar')
    expect(progressBar).toBeInTheDocument()
    expect(progressBar).toHaveAttribute('aria-valuemin', '0')
    expect(progressBar).toHaveAttribute('aria-valuemax', '100')
    expect(progressBar).toHaveAttribute('aria-valuenow')
    expect(progressBar).toHaveAttribute('aria-label')
  })

  it('shows all three stage indicators', () => {
    const { container } = render(<ImportProgress {...defaultProps} />)

    // Check that all three stages are visible in the DOM
    const stageIndicators = container.querySelectorAll('.grid.grid-cols-3 > div')
    expect(stageIndicators.length).toBe(3)

    // Verify stage names are present (they appear multiple times - in main label and stage indicators)
    expect(screen.getAllByText(/Uploading file/i).length).toBeGreaterThan(0)
    expect(screen.getAllByText(/Validating data/i).length).toBeGreaterThan(0)
    expect(screen.getAllByText(/Importing records/i).length).toBeGreaterThan(0)
  })

  it('shows stage icons', () => {
    const { container } = render(<ImportProgress {...defaultProps} />)

    // Check for SVG icons (Upload, CheckCircle2, Database from lucide-react)
    const stageIcons = container.querySelectorAll('svg[aria-hidden="true"]')
    expect(stageIcons.length).toBeGreaterThanOrEqual(3)
  })

  it('formats file size correctly for different units', () => {
    // Test KB
    const { rerender } = render(<ImportProgress {...defaultProps} fileSize={1024 * 50} />)
    expect(screen.getByText(/Size: 50 KB/i)).toBeInTheDocument()

    // Test MB
    rerender(<ImportProgress {...defaultProps} fileSize={1024 * 1024 * 2.5} />)
    expect(screen.getByText(/Size: 2\.5 MB/i)).toBeInTheDocument()

    // Test Bytes
    rerender(<ImportProgress {...defaultProps} fileSize={512} />)
    expect(screen.getByText(/Size: 512 Bytes/i)).toBeInTheDocument()
  })

  it('shows helper text', () => {
    render(<ImportProgress {...defaultProps} />)

    expect(
      screen.getByText(/Please wait while we process your import/i)
    ).toBeInTheDocument()
  })

  it('has proper accessibility attributes', () => {
    const { container } = render(<ImportProgress {...defaultProps} />)

    // Check for role="status" and aria-live="polite"
    const statusContainer = container.querySelector('[role="status"]')
    expect(statusContainer).toBeInTheDocument()
    expect(statusContainer).toHaveAttribute('aria-live', 'polite')

    // Check progress bar accessibility
    const progressBar = screen.getByRole('progressbar')
    expect(progressBar).toHaveAttribute('aria-label')
    expect(progressBar.getAttribute('aria-label')).toMatch(/Import progress/)
  })

  it('renders with 0 byte file size', () => {
    render(<ImportProgress {...defaultProps} fileSize={0} />)

    expect(screen.getByText(/Size: 0 Bytes/i)).toBeInTheDocument()
  })

  it('displays percentage value', () => {
    render(<ImportProgress {...defaultProps} />)

    // Should show a percentage (could be 0% or higher depending on timing)
    expect(screen.getByText(/\d+%/)).toBeInTheDocument()
  })

  it('has visual stage differentiation', () => {
    const { container } = render(<ImportProgress {...defaultProps} />)

    // Get all stage containers
    const stageContainers = container.querySelectorAll('.grid.grid-cols-3 > div')
    expect(stageContainers.length).toBe(3)

    // At least one stage should have active/completed styling (bg-blue-50 or bg-green-50)
    const hasActiveOrCompletedStyle = Array.from(stageContainers).some((container) =>
      container.className.includes('bg-blue-50') || container.className.includes('bg-green-50')
    )
    expect(hasActiveOrCompletedStyle).toBe(true)
  })
})
