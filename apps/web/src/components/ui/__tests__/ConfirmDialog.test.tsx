import { describe, it, expect, vi } from 'vitest'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { ConfirmDialog } from '../ConfirmDialog'

describe('ConfirmDialog', () => {
  const defaultProps = {
    open: true,
    onClose: vi.fn(),
    onConfirm: vi.fn(),
    title: 'Delete Model',
    message: "Are you sure you want to delete 'GPT-4'? This action cannot be undone.",
  }

  it('renders dialog with title and message when open', () => {
    render(<ConfirmDialog {...defaultProps} />)

    expect(screen.getByRole('dialog')).toBeInTheDocument()
    expect(screen.getByText('Delete Model')).toBeInTheDocument()
    expect(
      screen.getByText(/Are you sure you want to delete 'GPT-4'/i)
    ).toBeInTheDocument()
  })

  it('does not render when open is false', () => {
    render(<ConfirmDialog {...defaultProps} open={false} />)

    expect(screen.queryByRole('dialog')).not.toBeInTheDocument()
  })

  it('displays model name in message text', () => {
    const messageWithModelName =
      "Are you sure you want to delete 'Claude-3 Opus'? This action cannot be undone."
    render(<ConfirmDialog {...defaultProps} message={messageWithModelName} />)

    expect(screen.getByText(/Claude-3 Opus/)).toBeInTheDocument()
  })

  it('calls onClose when Cancel button is clicked', async () => {
    const user = userEvent.setup()
    const onClose = vi.fn()
    render(<ConfirmDialog {...defaultProps} onClose={onClose} />)

    const cancelButton = screen.getByRole('button', { name: /cancel/i })
    await user.click(cancelButton)

    expect(onClose).toHaveBeenCalledTimes(1)
  })

  it('calls onClose when backdrop is clicked', async () => {
    const user = userEvent.setup()
    const onClose = vi.fn()
    render(<ConfirmDialog {...defaultProps} onClose={onClose} />)

    // Click on backdrop (the div with bg-black/50 class)
    const backdrop = screen.getByRole('dialog').parentElement?.querySelector(
      '[aria-hidden="true"]'
    )
    expect(backdrop).toBeInTheDocument()
    await user.click(backdrop!)

    expect(onClose).toHaveBeenCalledTimes(1)
  })

  it('calls onClose when X button is clicked', async () => {
    const user = userEvent.setup()
    const onClose = vi.fn()
    render(<ConfirmDialog {...defaultProps} onClose={onClose} />)

    const closeButton = screen.getByRole('button', { name: /close dialog/i })
    await user.click(closeButton)

    expect(onClose).toHaveBeenCalledTimes(1)
  })

  it('calls onConfirm when Confirm button is clicked', async () => {
    const user = userEvent.setup()
    const onConfirm = vi.fn()
    render(<ConfirmDialog {...defaultProps} onConfirm={onConfirm} />)

    const confirmButton = screen.getByRole('button', { name: /confirm/i })
    await user.click(confirmButton)

    expect(onConfirm).toHaveBeenCalledTimes(1)
  })

  it('renders custom confirmText when provided', () => {
    render(<ConfirmDialog {...defaultProps} confirmText="Delete" />)

    expect(screen.getByRole('button', { name: /delete/i })).toBeInTheDocument()
    expect(
      screen.queryByRole('button', { name: /^confirm$/i })
    ).not.toBeInTheDocument()
  })

  it('disables buttons and shows spinner when loading is true', () => {
    render(<ConfirmDialog {...defaultProps} loading={true} />)

    const cancelButton = screen.getByRole('button', { name: /cancel/i })
    const confirmButton = screen.getByRole('button', { name: /confirm/i })
    const closeButton = screen.getByRole('button', { name: /close dialog/i })

    expect(cancelButton).toBeDisabled()
    expect(confirmButton).toBeDisabled()
    expect(closeButton).toBeDisabled()

    // Check for spinner (SVG with animation)
    const spinner = confirmButton.querySelector('svg')
    expect(spinner).toBeInTheDocument()
    expect(spinner).toHaveClass('animate-spin')
  })

  it('applies destructive styling to confirm button', () => {
    render(<ConfirmDialog {...defaultProps} />)

    const confirmButton = screen.getByRole('button', { name: /confirm/i })

    // Check for red background class
    expect(confirmButton).toHaveClass('bg-red-600')
    expect(confirmButton).toHaveClass('hover:bg-red-700')
  })

  it('does not call onConfirm or onClose during loading state', async () => {
    const user = userEvent.setup()
    const onClose = vi.fn()
    const onConfirm = vi.fn()
    render(
      <ConfirmDialog
        {...defaultProps}
        onClose={onClose}
        onConfirm={onConfirm}
        loading={true}
      />
    )

    const cancelButton = screen.getByRole('button', { name: /cancel/i })
    const confirmButton = screen.getByRole('button', { name: /confirm/i })

    // Try to click disabled buttons (should not trigger callbacks)
    await user.click(cancelButton)
    await user.click(confirmButton)

    expect(onClose).not.toHaveBeenCalled()
    expect(onConfirm).not.toHaveBeenCalled()
  })

  it('renders with custom title and message', () => {
    const customTitle = 'Warning'
    const customMessage = 'This is a custom warning message.'
    render(
      <ConfirmDialog
        {...defaultProps}
        title={customTitle}
        message={customMessage}
      />
    )

    expect(screen.getByText(customTitle)).toBeInTheDocument()
    expect(screen.getByText(customMessage)).toBeInTheDocument()
  })
})
