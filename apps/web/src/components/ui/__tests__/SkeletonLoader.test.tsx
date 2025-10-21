import { describe, it, expect } from 'vitest'
import { render, screen } from '@testing-library/react'
import { SkeletonLoader, SkeletonRow, SkeletonCard } from '../SkeletonLoader'

describe('SkeletonLoader', () => {
  it('should render default 5 rows with 4 columns', () => {
    const { container } = render(<SkeletonLoader />)
    const rows = container.querySelectorAll('.grid')
    expect(rows).toHaveLength(5)

    // Each row should have 4 columns
    const firstRow = rows[0]
    const columns = firstRow.querySelectorAll('.bg-gray-200')
    expect(columns).toHaveLength(4)
  })

  it('should render custom number of rows', () => {
    const { container } = render(<SkeletonLoader rows={10} />)
    const rows = container.querySelectorAll('.grid')
    expect(rows).toHaveLength(10)
  })

  it('should render custom number of columns', () => {
    const { container } = render(<SkeletonLoader rows={1} columns={6} />)
    const columns = container.querySelectorAll('.bg-gray-200')
    expect(columns).toHaveLength(6)
  })

  it('should have role="status" for screen readers', () => {
    render(<SkeletonLoader />)
    expect(screen.getByRole('status')).toBeInTheDocument()
  })

  it('should have aria-label for accessibility', () => {
    render(<SkeletonLoader />)
    expect(screen.getByRole('status')).toHaveAttribute('aria-label', 'Loading data')
  })

  it('should have sr-only loading text', () => {
    render(<SkeletonLoader />)
    expect(screen.getByText('Loading...')).toHaveClass('sr-only')
  })

  it('should apply custom className', () => {
    const { container } = render(<SkeletonLoader className="custom-skeleton" />)
    expect(container.firstChild).toHaveClass('custom-skeleton')
  })

  it('should have animate-pulse animation', () => {
    const { container } = render(<SkeletonLoader />)
    const rows = container.querySelectorAll('.animate-pulse')
    expect(rows.length).toBeGreaterThan(0)
  })
})

describe('SkeletonRow', () => {
  it('should render table row with default 4 columns', () => {
    const { container } = render(
      <table>
        <tbody>
          <SkeletonRow />
        </tbody>
      </table>
    )
    const cells = container.querySelectorAll('td')
    expect(cells).toHaveLength(4)
  })

  it('should render custom number of columns', () => {
    const { container } = render(
      <table>
        <tbody>
          <SkeletonRow columns={6} />
        </tbody>
      </table>
    )
    const cells = container.querySelectorAll('td')
    expect(cells).toHaveLength(6)
  })

  it('should have animate-pulse class', () => {
    const { container } = render(
      <table>
        <tbody>
          <SkeletonRow />
        </tbody>
      </table>
    )
    const row = container.querySelector('tr')
    expect(row).toHaveClass('animate-pulse')
  })
})

describe('SkeletonCard', () => {
  it('should render card skeleton', () => {
    const { container } = render(<SkeletonCard />)
    const card = container.firstChild as HTMLElement
    expect(card).toHaveClass('border', 'rounded-lg', 'p-6')
  })

  it('should have animate-pulse animation', () => {
    const { container } = render(<SkeletonCard />)
    const card = container.firstChild as HTMLElement
    expect(card).toHaveClass('animate-pulse')
  })

  it('should apply custom className', () => {
    const { container } = render(<SkeletonCard className="custom-card" />)
    const card = container.firstChild as HTMLElement
    expect(card).toHaveClass('custom-card')
  })

  it('should render multiple skeleton elements', () => {
    const { container } = render(<SkeletonCard />)
    const skeletonElements = container.querySelectorAll('.bg-gray-200')
    expect(skeletonElements.length).toBeGreaterThan(3)
  })
})
