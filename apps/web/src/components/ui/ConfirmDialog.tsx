import { Fragment, useState, useEffect } from 'react'
import { X, AlertTriangle } from 'lucide-react'

export interface ConfirmDialogProps {
  open: boolean
  onClose: () => void
  onConfirm: () => void
  title: string
  message: string
  confirmText?: string
  loading?: boolean
  /** Story 2.13 Task 11.3: Require user to type confirmation keyword */
  requireTypedConfirmation?: boolean
  /** The keyword that must be typed (default: 'DELETE') */
  confirmationKeyword?: string
  /** The name of the item being deleted (shown in message) */
  itemName?: string
}

/**
 * ConfirmDialog component with optional two-step confirmation
 * Story 2.13 Task 11: Two-step delete confirmation for destructive actions
 *
 * @example Simple confirmation
 * ```tsx
 * <ConfirmDialog
 *   open={true}
 *   onClose={() => {}}
 *   onConfirm={() => {}}
 *   title="Delete Model"
 *   message="Are you sure?"
 * />
 * ```
 *
 * @example Two-step typed confirmation
 * ```tsx
 * <ConfirmDialog
 *   open={true}
 *   onClose={() => {}}
 *   onConfirm={() => {}}
 *   title="Delete Model"
 *   message="This action cannot be undone."
 *   requireTypedConfirmation={true}
 *   confirmationKeyword="DELETE"
 *   itemName="GPT-4"
 * />
 * ```
 */
export function ConfirmDialog({
  open,
  onClose,
  onConfirm,
  title,
  message,
  confirmText = 'Confirm',
  loading = false,
  requireTypedConfirmation = false,
  confirmationKeyword = 'DELETE',
  itemName,
}: ConfirmDialogProps) {
  const [typedValue, setTypedValue] = useState('')
  const [showTypedConfirmation, setShowTypedConfirmation] = useState(false)

  // Reset state when dialog is closed
  useEffect(() => {
    if (!open) {
      setTypedValue('')
      setShowTypedConfirmation(false)
    }
  }, [open])

  // Reset state when dialog opens/closes
  const handleClose = () => {
    setTypedValue('')
    setShowTypedConfirmation(false)
    onClose()
  }

  // Handle first confirmation step (Yes/No)
  const handleFirstConfirm = () => {
    if (requireTypedConfirmation) {
      // Move to second step (typed confirmation)
      setShowTypedConfirmation(true)
    } else {
      // No typed confirmation required, proceed immediately
      onConfirm()
    }
  }

  // Handle second confirmation step (typed keyword)
  const handleTypedConfirm = () => {
    if (typedValue === confirmationKeyword) {
      onConfirm()
      setTypedValue('')
      setShowTypedConfirmation(false)
    }
  }

  const isTypedConfirmationValid = typedValue === confirmationKeyword

  if (!open) return null

  return (
    <Fragment>
      {/* Backdrop */}
      <div
        className="fixed inset-0 bg-black/50 backdrop-blur-sm z-40"
        onClick={handleClose}
        aria-hidden="true"
      />

      {/* Dialog */}
      <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
        <div
          className="bg-white rounded-lg shadow-xl max-w-md w-full"
          role="dialog"
          aria-modal="true"
          aria-labelledby="dialog-title"
        >
          {/* Header */}
          <div className="flex items-center justify-between px-6 py-4 border-b border-gray-200">
            <h2
              id="dialog-title"
              className="text-lg font-semibold text-gray-900 flex items-center gap-2"
            >
              <AlertTriangle className="w-5 h-5 text-red-600" aria-hidden="true" />
              {title}
            </h2>
            <button
              onClick={handleClose}
              disabled={loading}
              className="text-gray-400 hover:text-gray-600 transition-colors disabled:opacity-50"
              aria-label="Close dialog"
            >
              <X className="w-5 h-5" />
            </button>
          </div>

          {/* Body */}
          <div className="px-6 py-4">
            {!showTypedConfirmation ? (
              /* Step 1: Initial confirmation message */
              <div className="space-y-2">
                <p className="text-gray-700">{message}</p>
                {itemName && (
                  <p className="text-sm text-gray-600">
                    Item: <span className="font-semibold text-gray-900">{itemName}</span>
                  </p>
                )}
              </div>
            ) : (
              /* Step 2: Typed confirmation (Story 2.13 Task 11.3) */
              <div className="space-y-3">
                <p className="text-sm text-gray-700">
                  To confirm deletion, please type{' '}
                  <code className="px-2 py-1 bg-gray-100 text-red-600 font-mono text-sm rounded">
                    {confirmationKeyword}
                  </code>
                </p>
                <input
                  type="text"
                  value={typedValue}
                  onChange={(e) => setTypedValue(e.target.value)}
                  disabled={loading}
                  placeholder={`Type ${confirmationKeyword} to confirm`}
                  className="block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-red-500 focus:border-red-500 disabled:bg-gray-100 disabled:cursor-not-allowed"
                  autoFocus
                  aria-label="Type confirmation keyword"
                />
                {typedValue && !isTypedConfirmationValid && (
                  <p className="text-sm text-red-600" role="alert">
                    Keyword does not match. Please type exactly "{confirmationKeyword}"
                  </p>
                )}
              </div>
            )}
          </div>

          {/* Footer */}
          <div className="flex items-center justify-end gap-3 px-6 py-4 border-t border-gray-200 bg-gray-50 rounded-b-lg">
            <button
              onClick={handleClose}
              disabled={loading}
              className="px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-md hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
            >
              Cancel
            </button>
            {!showTypedConfirmation ? (
              /* Step 1 button */
              <button
                onClick={handleFirstConfirm}
                disabled={loading}
                className="px-4 py-2 text-sm font-medium text-white bg-red-600 border border-transparent rounded-md hover:bg-red-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-red-500 disabled:opacity-50 disabled:cursor-not-allowed transition-colors inline-flex items-center gap-2"
              >
                {loading && (
                  <svg
                    className="animate-spin h-4 w-4"
                    xmlns="http://www.w3.org/2000/svg"
                    fill="none"
                    viewBox="0 0 24 24"
                  >
                    <circle
                      className="opacity-25"
                      cx="12"
                      cy="12"
                      r="10"
                      stroke="currentColor"
                      strokeWidth="4"
                    />
                    <path
                      className="opacity-75"
                      fill="currentColor"
                      d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"
                    />
                  </svg>
                )}
                {confirmText}
              </button>
            ) : (
              /* Step 2 button */
              <button
                onClick={handleTypedConfirm}
                disabled={loading || !isTypedConfirmationValid}
                className="px-4 py-2 text-sm font-medium text-white bg-red-600 border border-transparent rounded-md hover:bg-red-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-red-500 disabled:opacity-50 disabled:cursor-not-allowed transition-colors inline-flex items-center gap-2"
              >
                {loading && (
                  <svg
                    className="animate-spin h-4 w-4"
                    xmlns="http://www.w3.org/2000/svg"
                    fill="none"
                    viewBox="0 0 24 24"
                  >
                    <circle
                      className="opacity-25"
                      cx="12"
                      cy="12"
                      r="10"
                      stroke="currentColor"
                      strokeWidth="4"
                    />
                    <path
                      className="opacity-75"
                      fill="currentColor"
                      d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"
                    />
                  </svg>
                )}
                Confirm Delete
              </button>
            )}
          </div>
        </div>
      </div>
    </Fragment>
  )
}
