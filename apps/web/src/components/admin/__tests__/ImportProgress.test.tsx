/**
 * Tests for ImportProgress Component
 * Story 2.13 Task 12: CSV import progress indicator
 */

import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest'
import { render, screen, waitFor, act } from '@testing-library/react'
import { ImportProgress } from '../ImportProgress'

describe('ImportProgress', () => {
  beforeEach(() => {
    vi.useFakeTimers()
  })

  afterEach(() => {
    vi.restoreAllMocks()
  })

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

  it('starts at uploading stage', () => {
    const { container } = render(<ImportProgress {...defaultProps} />)

    // Check the main stage label (not the stage indicator text)
    const stageLabel = container.querySelector('.text-sm.font-medium.text-gray-700')
    expect(stageLabel).toHaveTextContent('Uploading file')
  })

  it('shows progress bar with initial 0%', () => {
    render(<ImportProgress {...defaultProps} />)

    const progressBar = screen.getByRole('progressbar')
    expect(progressBar).toBeInTheDocument()
    expect(progressBar).toHaveAttribute('aria-valuenow', '0')
    expect(progressBar).toHaveAttribute('aria-valuemin', '0')
    expect(progressBar).toHaveAttribute('aria-valuemax', '100')
  })

  it('progresses through stages over time', async () => {
    const { container } = render(<ImportProgress {...defaultProps} />)

    // Initial stage: Uploading
    const stageLabel = container.querySelector('.text-sm.font-medium.text-gray-700')
    expect(stageLabel).toHaveTextContent('Uploading file')

    // After 1 second: Validating
    await act(async () => {
      vi.advanceTimersByTime(1100) // Advance past 1 second mark plus buffer
      vi.runOnlyPendingTimers()
    })
    await waitFor(() => {
      expect(stageLabel).toHaveTextContent('Validating data')
    }, { timeout: 1000 })

    // After 2.5 seconds total: Importing
    await act(async () => {
      vi.advanceTimersByTime(1600) // Advance past 1.5 seconds plus buffer
      vi.runOnlyPendingTimers()
    })
    await waitFor(() => {
      expect(stageLabel).toHaveTextContent('Importing records')
    }, { timeout: 1000 })
  })

  it('shows all three stage indicators', () => {
    const { container } = render(<ImportProgress {...defaultProps} />)

    // Check that all three stages are visible
    expect(screen.getByText('Uploading file')).toBeInTheDocument()
    expect(screen.getByText('Validating data')).toBeInTheDocument()
    expect(screen.getByText('Importing records')).toBeInTheDocument()

    // Check for icons (Upload, CheckCircle2, Database from lucide-react)
    const stageIcons = container.querySelectorAll('svg')
    expect(stageIcons.length).toBeGreaterThanOrEqual(3)
  })

  it('animates progress bar smoothly', async () => {
    render(<ImportProgress {...defaultProps} />)

    const progressBar = screen.getByRole('progressbar')

    // Initial progress
    expect(progressBar).toHaveAttribute('aria-valuenow', '0')

    // Progress should increase over time
    await act(async () => {
      vi.advanceTimersByTime(500)
      vi.runOnlyPendingTimers()
    })
    await waitFor(() => {
      const currentProgress = parseInt(progressBar.getAttribute('aria-valuenow') || '0')
      expect(currentProgress).toBeGreaterThan(0)
      expect(currentProgress).toBeLessThanOrEqual(33)
    }, { timeout: 1000 })
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
  })

  it('highlights current stage visually', async () => {
    const { container } = render(<ImportProgress {...defaultProps} />)

    // Get all stage containers
    const stageContainers = container.querySelectorAll('.grid > div')
    expect(stageContainers.length).toBe(3)

    // First stage should be active (blue background)
    expect(stageContainers[0]).toHaveClass('bg-blue-50')

    // Advance to second stage
    await act(async () => {
      vi.advanceTimersByTime(1100)
      vi.runOnlyPendingTimers()
    })

    // First should be completed (green), second should be active (blue)
    await waitFor(() => {
      expect(stageContainers[0]).toHaveClass('bg-green-50')
      expect(stageContainers[1]).toHaveClass('bg-blue-50')
    }, { timeout: 1000 })
  })

  it('renders with 0 byte file size', () => {
    render(<ImportProgress {...defaultProps} fileSize={0} />)

    expect(screen.getByText(/Size: 0 Bytes/i)).toBeInTheDocument()
  })

  it('updates progress percentage display', async () => {
    render(<ImportProgress {...defaultProps} />)

    // Initial 0%
    expect(screen.getByText('0%')).toBeInTheDocument()

    // Progress increases
    await act(async () => {
      vi.advanceTimersByTime(500)
      vi.runOnlyPendingTimers()
    })
    await waitFor(() => {
      // Should show some progress > 0%
      const percentageTexts = screen.queryAllByText(/\d+%/)
      const hasNonZeroPercentage = percentageTexts.some(el => el.textContent !== '0%')
      expect(hasNonZeroPercentage).toBe(true)
    }, { timeout: 1000 })
  })
})
