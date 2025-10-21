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

  // Story 2.13 Task 11: Two-step confirmation tests
  describe('Two-step typed confirmation', () => {
    const twoStepProps = {
      ...defaultProps,
      requireTypedConfirmation: true,
      confirmationKeyword: 'DELETE',
      itemName: 'GPT-4',
      message: 'This action cannot be undone.',
      confirmText: 'Delete',
    }

    it('shows item name in first step', () => {
      render(<ConfirmDialog {...twoStepProps} />)

      expect(screen.getByText(/This action cannot be undone/)).toBeInTheDocument()
      expect(screen.getByText('GPT-4')).toBeInTheDocument()
    })

    it('moves to second step when confirm button is clicked', async () => {
      const user = userEvent.setup()
      render(<ConfirmDialog {...twoStepProps} />)

      // Initially on step 1
      expect(
        screen.queryByText(/To confirm deletion, please type/)
      ).not.toBeInTheDocument()

      // Click confirm button
      const confirmButton = screen.getByRole('button', { name: /delete/i })
      await user.click(confirmButton)

      // Now on step 2
      expect(screen.getByText(/To confirm deletion, please type/)).toBeInTheDocument()
      expect(screen.getByText('DELETE')).toBeInTheDocument()
      expect(screen.getByLabelText(/type confirmation keyword/i)).toBeInTheDocument()
    })

    it('shows text input in second step', async () => {
      const user = userEvent.setup()
      render(<ConfirmDialog {...twoStepProps} />)

      // Move to step 2
      const confirmButton = screen.getByRole('button', { name: /delete/i })
      await user.click(confirmButton)

      // Check for input field
      const input = screen.getByLabelText(/type confirmation keyword/i)
      expect(input).toBeInTheDocument()
      expect(input).toHaveAttribute('placeholder', 'Type DELETE to confirm')
    })

    it('disables confirm button until keyword matches', async () => {
      const user = userEvent.setup()
      render(<ConfirmDialog {...twoStepProps} />)

      // Move to step 2
      await user.click(screen.getByRole('button', { name: /delete/i }))

      // Confirm button should be disabled initially
      const confirmDeleteButton = screen.getByRole('button', {
        name: /confirm delete/i,
      })
      expect(confirmDeleteButton).toBeDisabled()

      // Type incorrect keyword
      const input = screen.getByLabelText(/type confirmation keyword/i)
      await user.type(input, 'delete')
      expect(confirmDeleteButton).toBeDisabled()

      // Clear and type correct keyword
      await user.clear(input)
      await user.type(input, 'DELETE')
      expect(confirmDeleteButton).toBeEnabled()
    })

    it('shows error message when typed keyword does not match', async () => {
      const user = userEvent.setup()
      render(<ConfirmDialog {...twoStepProps} />)

      // Move to step 2
      await user.click(screen.getByRole('button', { name: /delete/i }))

      // Type incorrect keyword
      const input = screen.getByLabelText(/type confirmation keyword/i)
      await user.type(input, 'wrong')

      expect(
        screen.getByText(/Keyword does not match.*DELETE/)
      ).toBeInTheDocument()
    })

    it('calls onConfirm only when correct keyword is typed', async () => {
      const user = userEvent.setup()
      const onConfirm = vi.fn()
      render(<ConfirmDialog {...twoStepProps} onConfirm={onConfirm} />)

      // Move to step 2
      await user.click(screen.getByRole('button', { name: /delete/i }))

      // Type correct keyword
      const input = screen.getByLabelText(/type confirmation keyword/i)
      await user.type(input, 'DELETE')

      // Click confirm
      const confirmButton = screen.getByRole('button', { name: /confirm delete/i })
      await user.click(confirmButton)

      expect(onConfirm).toHaveBeenCalledTimes(1)
    })

    it('resets state when dialog is closed and reopened', async () => {
      const user = userEvent.setup()
      const { rerender } = render(<ConfirmDialog {...twoStepProps} />)

      // Move to step 2 and type keyword
      await user.click(screen.getByRole('button', { name: /delete/i }))
      const input = screen.getByLabelText(/type confirmation keyword/i)
      await user.type(input, 'DELETE')

      // Close dialog
      rerender(<ConfirmDialog {...twoStepProps} open={false} />)

      // Reopen dialog
      rerender(<ConfirmDialog {...twoStepProps} open={true} />)

      // Should be back to step 1
      expect(
        screen.queryByLabelText(/type confirmation keyword/i)
      ).not.toBeInTheDocument()
      expect(screen.getByText('GPT-4')).toBeInTheDocument()
    })

    it('works with custom confirmation keyword', async () => {
      const user = userEvent.setup()
      const customKeywordProps = {
        ...twoStepProps,
        confirmationKeyword: 'REMOVE',
      }
      render(<ConfirmDialog {...customKeywordProps} />)

      // Move to step 2
      await user.click(screen.getByRole('button', { name: /delete/i }))

      // Should show custom keyword
      expect(screen.getByText('REMOVE')).toBeInTheDocument()
      expect(screen.getByPlaceholderText('Type REMOVE to confirm')).toBeInTheDocument()

      // Type custom keyword
      const input = screen.getByLabelText(/type confirmation keyword/i)
      await user.type(input, 'REMOVE')

      // Confirm button should be enabled
      const confirmButton = screen.getByRole('button', { name: /confirm delete/i })
      expect(confirmButton).toBeEnabled()
    })

    it('does not require typed confirmation when flag is false', async () => {
      const user = userEvent.setup()
      const onConfirm = vi.fn()
      const singleStepProps = {
        ...twoStepProps,
        requireTypedConfirmation: false,
        onConfirm,
      }
      render(<ConfirmDialog {...singleStepProps} />)

      // Click confirm button
      const confirmButton = screen.getByRole('button', { name: /delete/i })
      await user.click(confirmButton)

      // Should call onConfirm immediately without showing step 2
      expect(onConfirm).toHaveBeenCalledTimes(1)
      expect(
        screen.queryByLabelText(/type confirmation keyword/i)
      ).not.toBeInTheDocument()
    })
  })
})
