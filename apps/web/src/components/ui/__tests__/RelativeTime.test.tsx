/**
 * Component tests for RelativeTime component
 * Story 2.12: Task 10.4
 */

import { describe, it, expect, vi, beforeAll, afterAll } from 'vitest'
import { render, screen } from '@testing-library/react'
import { RelativeTime } from '../RelativeTime'

describe('RelativeTime component', () => {
  // Mock the current date to avoid time-dependent test flakiness
  const MOCK_NOW = new Date('2025-10-21T12:00:00Z')

  beforeAll(() => {
    vi.useFakeTimers()
    vi.setSystemTime(MOCK_NOW)
  })

  afterAll(() => {
    vi.useRealTimers()
  })

  it('should render relative time for recent date', () => {
    render(<RelativeTime date="2025-10-19T12:00:00Z" />)
    expect(screen.getByText('2 days ago')).toBeInTheDocument()
  })

  it('should show absolute timestamp in title attribute (tooltip)', () => {
    const { container } = render(<RelativeTime date="2025-10-19T14:30:00Z" />)
    const element = container.querySelector('span[title]')
    expect(element).toHaveAttribute('title')
    const title = element?.getAttribute('title')
    expect(title).toContain('Oct')
    expect(title).toContain('19')
    expect(title).toContain('2025')
  })

  it('should display green checkmark icon for fresh date (< 7 days)', () => {
    const { container } = render(<RelativeTime date="2025-10-19T12:00:00Z" showIcon />)
    // Fresh dates should have green color class
    const textElement = screen.getByText('2 days ago')
    expect(textElement).toHaveClass('text-green-700')
  })

  it('should display yellow clock icon for stale date (7-30 days)', () => {
    const { container } = render(<RelativeTime date="2025-10-06T12:00:00Z" showIcon />)
    // Stale dates should have yellow color class
    const textElement = screen.getByText('15 days ago')
    expect(textElement).toHaveClass('text-yellow-700')
  })

  it('should display red warning icon for critical date (> 30 days)', () => {
    const { container } = render(<RelativeTime date="2025-09-06T12:00:00Z" showIcon />)
    // Critical dates should have red color class
    const textElement = screen.getByText('about 2 months ago')
    expect(textElement).toHaveClass('text-red-700')
  })

  it('should not show icon when showIcon is false', () => {
    const { container } = render(<RelativeTime date="2025-10-19T12:00:00Z" showIcon={false} />)
    // Should only have text, no icon SVG elements
    const svgElements = container.querySelectorAll('svg')
    expect(svgElements.length).toBe(0)
  })

  it('should not show icon by default when showIcon is not specified', () => {
    const { container } = render(<RelativeTime date="2025-10-19T12:00:00Z" />)
    // Should only have text, no icon SVG elements
    const svgElements = container.querySelectorAll('svg')
    expect(svgElements.length).toBe(0)
  })

  it('should accept Date object', () => {
    const date = new Date('2025-10-19T12:00:00Z')
    render(<RelativeTime date={date} />)
    expect(screen.getByText('2 days ago')).toBeInTheDocument()
  })

  it('should apply custom className', () => {
    const { container } = render(<RelativeTime date="2025-10-19T12:00:00Z" className="custom-class" />)
    const element = container.querySelector('.custom-class')
    expect(element).toBeInTheDocument()
  })
})
