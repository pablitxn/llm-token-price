import { AlertCircle, ExternalLink } from 'lucide-react'
import type { UserError } from '@/utils/errorMessages'

export interface ErrorAlertProps {
  /** User-friendly error object from mapErrorToUserMessage */
  error: UserError
  /** Optional callback when "Try Again" is clicked */
  onRetry?: () => void
  /** Optional callback when "Report Issue" is clicked (overrides default GitHub Issues link) */
  onReport?: () => void
  /** Additional CSS classes */
  className?: string
  /** Raw error for technical details in GitHub issue (optional) */
  rawError?: Error
}

/**
 * Generate GitHub issue URL with pre-filled error information
 * Story 3.1b Task 3.2: Automated GitHub issue reporting
 */
function generateGitHubIssueUrl(error: UserError, rawError?: Error): string {
  const repoUrl = 'https://github.com/pablitxn/llm-token-price'
  const timestamp = new Date().toISOString()
  const userAgent = navigator.userAgent
  const currentUrl = window.location.href

  // Build issue title
  const title = `[Bug Report] ${error.message}`

  // Build issue body with error details
  const body = `## Bug Description

**Error Message:**
${error.message}

${error.action ? `**Suggested Action:**\n${error.action}\n\n` : ''}

## Environment

- **Timestamp:** ${timestamp}
- **URL:** ${currentUrl}
- **User Agent:** ${userAgent}

## Technical Details

${rawError ? `**Stack Trace:**
\`\`\`
${rawError.stack || rawError.toString()}
\`\`\`

**Error Type:** ${rawError.name}
` : '**Stack Trace:** Not available'}

## Steps to Reproduce

1. Navigate to: ${currentUrl}
2. (Please describe what you were doing when this error occurred)
3. Error appeared: "${error.message}"

## Expected Behavior

(Please describe what you expected to happen)

## Actual Behavior

(Please describe what actually happened)

---
*This issue was automatically generated from the error alert. Please add any additional context that might help us resolve this issue.*`

  // Encode parameters for GitHub issue URL
  const params = new URLSearchParams({
    title,
    body,
    labels: 'bug,user-reported'
  })

  return `${repoUrl}/issues/new?${params.toString()}`
}

/**
 * ErrorAlert component for displaying user-friendly error messages
 * Story 2.13 Task 10.3 & 10.5: User-friendly errors with optional "Report Issue" button
 * Story 3.1b Task 3.2: GitHub issue link with pre-filled template
 *
 * @example
 * ```tsx
 * const userError = mapErrorToUserMessage(error)
 * <ErrorAlert
 *   error={userError}
 *   onRetry={() => refetch()}
 *   rawError={error} // Include raw error for technical details
 * />
 * ```
 */
export function ErrorAlert({ error, onRetry, onReport, className = '', rawError }: ErrorAlertProps) {
  /**
   * Handle report button click - either use custom callback or open GitHub issue
   */
  const handleReport = () => {
    if (onReport) {
      onReport()
    } else {
      const issueUrl = generateGitHubIssueUrl(error, rawError)
      window.open(issueUrl, '_blank', 'noopener,noreferrer')
    }
  }
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
            {error.canReport && (
              <button
                type="button"
                onClick={handleReport}
                className="rounded-md bg-red-50 px-3 py-2 text-sm font-medium text-red-800 hover:bg-red-100 focus:outline-none focus:ring-2 focus:ring-red-600 focus:ring-offset-2 focus:ring-offset-red-50 transition-colors inline-flex items-center gap-2"
                title="Report this issue on GitHub"
              >
                <ExternalLink className="h-4 w-4" aria-hidden="true" />
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
