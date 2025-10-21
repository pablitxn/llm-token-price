import { describe, it, expect } from 'vitest'
import { render, screen } from '@testing-library/react'
import { LoadingSpinner } from '../LoadingSpinner'

describe('LoadingSpinner', () => {
  it('should render with default size (md)', () => {
    const { container } = render(<LoadingSpinner />)
    const spinner = container.querySelector('svg')
    expect(spinner).toBeInTheDocument()
    expect(spinner).toHaveClass('h-8', 'w-8')
  })

  it('should render with small size', () => {
    const { container } = render(<LoadingSpinner size="sm" />)
    const spinner = container.querySelector('svg')
    expect(spinner).toHaveClass('h-4', 'w-4')
  })

  it('should render with large size', () => {
    const { container } = render(<LoadingSpinner size="lg" />)
    const spinner = container.querySelector('svg')
    expect(spinner).toHaveClass('h-12', 'w-12')
  })

  it('should display optional text', () => {
    render(<LoadingSpinner text="Loading models..." />)
    expect(screen.getByText('Loading models...')).toBeInTheDocument()
  })

  it('should not display text when not provided', () => {
    const { container } = render(<LoadingSpinner />)
    const text = container.querySelector('p')
    expect(text).not.toBeInTheDocument()
  })

  it('should apply custom className', () => {
    const { container } = render(<LoadingSpinner className="custom-class" />)
    const wrapper = container.firstChild as HTMLElement
    expect(wrapper).toHaveClass('custom-class')
  })

  it('should have spinning animation', () => {
    const { container } = render(<LoadingSpinner />)
    const spinner = container.querySelector('svg')
    expect(spinner).toHaveClass('animate-spin')
  })

  it('should have aria-live="polite" on text for screen readers', () => {
    render(<LoadingSpinner text="Loading..." />)
    const text = screen.getByText('Loading...')
    expect(text).toHaveAttribute('aria-live', 'polite')
  })
})
