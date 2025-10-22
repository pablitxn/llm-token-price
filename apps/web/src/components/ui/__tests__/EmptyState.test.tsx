import { describe, it, expect, vi } from 'vitest'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { EmptyState } from '../EmptyState'
import { Database } from 'lucide-react'

describe('EmptyState Component - Story 3.1 AC #5', () => {
  it('should render with default props', () => {
    render(<EmptyState />)

    expect(screen.getByText('No data available')).toBeInTheDocument()
    expect(
      screen.getByText('There is no data to display at this time.')
    ).toBeInTheDocument()
  })

  it('should render with custom title and message', () => {
    render(
      <EmptyState
        title="No models found"
        message="Try adjusting your search criteria"
      />
    )

    expect(screen.getByText('No models found')).toBeInTheDocument()
    expect(
      screen.getByText('Try adjusting your search criteria')
    ).toBeInTheDocument()
  })

  it('should render custom icon', () => {
    const { container } = render(
      <EmptyState
        icon={<Database className="custom-icon" aria-hidden="true" />}
      />
    )

    const icon = container.querySelector('.custom-icon')
    expect(icon).toBeInTheDocument()
  })

  it('should render action button when provided', () => {
    const handleClick = vi.fn()

    render(
      <EmptyState
        title="No data"
        action={{
          label: 'Refresh',
          onClick: handleClick,
        }}
      />
    )

    const button = screen.getByRole('button', { name: /refresh/i })
    expect(button).toBeInTheDocument()
  })

  it('should call action onClick when button is clicked', async () => {
    const user = userEvent.setup()
    const handleClick = vi.fn()

    render(
      <EmptyState
        action={{
          label: 'Load Data',
          onClick: handleClick,
        }}
      />
    )

    const button = screen.getByRole('button', { name: /load data/i })
    await user.click(button)

    expect(handleClick).toHaveBeenCalledTimes(1)
  })

  it('should have proper accessibility attributes', () => {
    render(<EmptyState title="Empty" message="No items" />)

    // Should have role="status" for screen readers
    const container = screen.getByRole('status')
    expect(container).toBeInTheDocument()
    expect(container).toHaveAttribute('aria-live', 'polite')
  })

  it('should apply custom className', () => {
    render(<EmptyState className="custom-class" />)

    const container = screen.getByRole('status')
    expect(container).toHaveClass('custom-class')
  })

  it('should hide default icon when custom icon is provided', () => {
    const { container } = render(
      <EmptyState icon={<Database data-testid="custom-icon" />} />
    )

    // Custom icon should be present
    expect(screen.getByTestId('custom-icon')).toBeInTheDocument()

    // Default InboxIcon should not be present (check by looking for the icon that's not the custom one)
    const icons = container.querySelectorAll('svg')
    expect(icons.length).toBe(1)
  })

  it('should render without action button when not provided', () => {
    render(<EmptyState />)

    expect(screen.queryByRole('button')).not.toBeInTheDocument()
  })

  it('should render complete example from homepage use case', () => {
    render(
      <EmptyState
        icon={<Database className="h-8 w-8 text-gray-400" aria-hidden="true" />}
        title="No models available"
        message="There are currently no models in the database. Please check back later or contact support if this issue persists."
      />
    )

    expect(screen.getByText('No models available')).toBeInTheDocument()
    expect(
      screen.getByText(
        /there are currently no models in the database/i
      )
    ).toBeInTheDocument()
  })
})
