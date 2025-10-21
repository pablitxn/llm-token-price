import { describe, it, expect, vi } from 'vitest'
import { render, screen, fireEvent } from '@testing-library/react'
import { ErrorAlert, FieldError } from '../ErrorAlert'
import type { UserError } from '@/utils/errorMessages'

describe('ErrorAlert', () => {
  const mockError: UserError = {
    message: 'Something went wrong',
    code: 'SERVER_ERROR',
    action: 'Please try again',
    canReport: true,
  }

  it('should render error message', () => {
    render(<ErrorAlert error={mockError} />)
    expect(screen.getByText('Something went wrong')).toBeInTheDocument()
  })

  it('should render action message when provided', () => {
    render(<ErrorAlert error={mockError} />)
    expect(screen.getByText('Please try again')).toBeInTheDocument()
  })

  it('should render Try Again button when onRetry is provided', () => {
    const onRetry = vi.fn()
    render(<ErrorAlert error={mockError} onRetry={onRetry} />)

    const retryButton = screen.getByRole('button', { name: /try again/i })
    expect(retryButton).toBeInTheDocument()
  })

  it('should call onRetry when Try Again button is clicked', () => {
    const onRetry = vi.fn()
    render(<ErrorAlert error={mockError} onRetry={onRetry} />)

    const retryButton = screen.getByRole('button', { name: /try again/i })
    fireEvent.click(retryButton)

    expect(onRetry).toHaveBeenCalledTimes(1)
  })

  it('should render Report Issue button when error.canReport is true and onReport is provided', () => {
    const onReport = vi.fn()
    render(<ErrorAlert error={mockError} onReport={onReport} />)

    const reportButton = screen.getByRole('button', { name: /report issue/i })
    expect(reportButton).toBeInTheDocument()
  })

  it('should NOT render Report Issue button when error.canReport is false', () => {
    const onReport = vi.fn()
    const errorWithoutReport: UserError = { ...mockError, canReport: false }

    render(<ErrorAlert error={errorWithoutReport} onReport={onReport} />)

    const reportButton = screen.queryByRole('button', { name: /report issue/i })
    expect(reportButton).not.toBeInTheDocument()
  })

  it('should NOT render Report Issue button when onReport is not provided', () => {
    render(<ErrorAlert error={mockError} />)

    const reportButton = screen.queryByRole('button', { name: /report issue/i })
    expect(reportButton).not.toBeInTheDocument()
  })

  it('should call onReport when Report Issue button is clicked', () => {
    const onReport = vi.fn()
    render(<ErrorAlert error={mockError} onReport={onReport} />)

    const reportButton = screen.getByRole('button', { name: /report issue/i })
    fireEvent.click(reportButton)

    expect(onReport).toHaveBeenCalledTimes(1)
  })

  it('should have role="alert" and aria-live="assertive"', () => {
    const { container } = render(<ErrorAlert error={mockError} />)
    const alert = container.firstChild as HTMLElement

    expect(alert).toHaveAttribute('role', 'alert')
    expect(alert).toHaveAttribute('aria-live', 'assertive')
  })

  it('should apply custom className', () => {
    const { container } = render(<ErrorAlert error={mockError} className="custom-class" />)
    const alert = container.firstChild as HTMLElement

    expect(alert).toHaveClass('custom-class')
  })

  it('should render without action message', () => {
    const errorWithoutAction: UserError = {
      message: 'Error occurred',
      code: 'ERROR',
    }
    render(<ErrorAlert error={errorWithoutAction} />)

    expect(screen.getByText('Error occurred')).toBeInTheDocument()
    expect(screen.queryByText('Please try again')).not.toBeInTheDocument()
  })

  it('should render both Try Again and Report Issue buttons when both callbacks are provided', () => {
    const onRetry = vi.fn()
    const onReport = vi.fn()
    render(<ErrorAlert error={mockError} onRetry={onRetry} onReport={onReport} />)

    expect(screen.getByRole('button', { name: /try again/i })).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /report issue/i })).toBeInTheDocument()
  })
})

describe('FieldError', () => {
  it('should render field error message', () => {
    render(<FieldError message="This field is required" />)
    expect(screen.getByText('This field is required')).toBeInTheDocument()
  })

  it('should have role="alert"', () => {
    render(<FieldError message="Error message" />)
    const error = screen.getByRole('alert')
    expect(error).toBeInTheDocument()
  })

  it('should have error styling', () => {
    render(<FieldError message="Error message" />)
    const error = screen.getByRole('alert')
    expect(error).toHaveClass('text-red-600')
  })
})
