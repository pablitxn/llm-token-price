import { InboxIcon } from 'lucide-react'

export interface EmptyStateProps {
  /** Title for the empty state */
  title?: string
  /** Description message */
  message?: string
  /** Optional icon component */
  icon?: React.ReactNode
  /** Optional action button */
  action?: {
    label: string
    onClick: () => void
  }
  /** Additional CSS classes */
  className?: string
}

/**
 * EmptyState component for displaying when no data is available
 * Story 3.1 AC #5: Empty state for homepage when no models available
 *
 * @example
 * ```tsx
 * <EmptyState
 *   title="No models available"
 *   message="There are currently no models in the database. Please check back later."
 * />
 * ```
 */
export function EmptyState({
  title = 'No data available',
  message = 'There is no data to display at this time.',
  icon,
  action,
  className = '',
}: EmptyStateProps) {
  return (
    <div
      className={`flex flex-col items-center justify-center py-12 px-4 text-center ${className}`}
      role="status"
      aria-live="polite"
    >
      <div className="flex items-center justify-center w-16 h-16 rounded-full bg-gray-100 mb-4">
        {icon || <InboxIcon className="h-8 w-8 text-gray-400" aria-hidden="true" />}
      </div>

      <h3 className="text-lg font-medium text-gray-900 mb-2">{title}</h3>

      <p className="text-sm text-gray-600 max-w-md mb-6">{message}</p>

      {action && (
        <button
          type="button"
          onClick={action.onClick}
          className="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md text-white bg-blue-600 hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 transition-colors"
        >
          {action.label}
        </button>
      )}
    </div>
  )
}
