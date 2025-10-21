import { AlertCircle } from 'lucide-react'
import type { UserError } from '@/utils/errorMessages'

export interface ErrorAlertProps {
  /** User-friendly error object from mapErrorToUserMessage */
  error: UserError
  /** Optional callback when "Try Again" is clicked */
  onRetry?: () => void
  /** Optional callback when "Report Issue" is clicked */
  onReport?: () => void
  /** Additional CSS classes */
  className?: string
}

/**
 * ErrorAlert component for displaying user-friendly error messages
 * Story 2.13 Task 10.3 & 10.5: User-friendly errors with optional "Report Issue" button
 *
 * @example
 * ```tsx
 * const userError = mapErrorToUserMessage(error)
 * <ErrorAlert
 *   error={userError}
 *   onRetry={() => refetch()}
 *   onReport={() => window.open('mailto:support@example.com')}
 * />
 * ```
 */
export function ErrorAlert({ error, onRetry, onReport, className = '' }: ErrorAlertProps) {
  return (
    <div
      className={`rounded-md bg-red-50 border border-red-200 p-4 ${className}`}
      role="alert"
      aria-live="assertive"
    >
      <div className="flex">
        <div className="flex-shrink-0">
          <AlertCircle className="h-5 w-5 text-red-400" aria-hidden="true" />
        </div>
        <div className="ml-3 flex-1">
          <h3 className="text-sm font-medium text-red-800">{error.message}</h3>
          {error.action && (
            <div className="mt-2 text-sm text-red-700">
              <p>{error.action}</p>
            </div>
          )}
          <div className="mt-4 flex items-center gap-3">
            {onRetry && (
              <button
                type="button"
                onClick={onRetry}
                className="rounded-md bg-red-50 px-3 py-2 text-sm font-medium text-red-800 hover:bg-red-100 focus:outline-none focus:ring-2 focus:ring-red-600 focus:ring-offset-2 focus:ring-offset-red-50 transition-colors"
              >
                Try Again
              </button>
            )}
            {error.canReport && onReport && (
              <button
                type="button"
                onClick={onReport}
                className="rounded-md bg-red-50 px-3 py-2 text-sm font-medium text-red-800 hover:bg-red-100 focus:outline-none focus:ring-2 focus:ring-red-600 focus:ring-offset-2 focus:ring-offset-red-50 transition-colors inline-flex items-center gap-2"
              >
                <svg
                  className="h-4 w-4"
                  fill="none"
                  viewBox="0 0 24 24"
                  stroke="currentColor"
                  aria-hidden="true"
                >
                  <path
                    strokeLinecap="round"
                    strokeLinejoin="round"
                    strokeWidth={2}
                    d="M3 8l7.89 5.26a2 2 0 002.22 0L21 8M5 19h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v10a2 2 0 002 2z"
                  />
                </svg>
                Report Issue
              </button>
            )}
          </div>
        </div>
      </div>
    </div>
  )
}

/**
 * Inline error message for form fields
 * Simpler version of ErrorAlert for individual field errors
 */
export function FieldError({ message }: { message: string }) {
  return (
    <p className="mt-1 text-sm text-red-600" role="alert">
      {message}
    </p>
  )
}
