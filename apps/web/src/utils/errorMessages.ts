/**
 * Error message mapping utilities
 * Story 2.13 Task 10: User-friendly error messages with technical details in console
 */

export interface UserError {
  /** User-friendly error message */
  message: string
  /** Error code for categorization */
  code: string
  /** Optional action suggestion */
  action?: string
  /** Whether to show "Report Issue" button */
  canReport?: boolean
}

/**
 * Maps HTTP status codes and error types to user-friendly messages
 * Story 2.13 Task 10.2: Common error mappings
 */
const ERROR_MESSAGES: Record<number | string, UserError> = {
  // Client errors (4xx)
  400: {
    message: 'Invalid data. Please check your inputs and try again.',
    code: 'VALIDATION_ERROR',
    action: 'Review the highlighted fields for errors',
  },
  401: {
    message: 'Your session has expired. Please log in again.',
    code: 'AUTHENTICATION_ERROR',
    action: 'Redirecting to login page...',
  },
  403: {
    message: 'You do not have permission to perform this action.',
    code: 'PERMISSION_ERROR',
    action: 'Contact your administrator if you believe this is an error',
  },
  404: {
    message: 'The requested item was not found.',
    code: 'NOT_FOUND',
    action: 'The item may have been deleted or moved',
  },
  409: {
    message: 'This action conflicts with existing data.',
    code: 'CONFLICT_ERROR',
    action: 'Please refresh the page and try again',
  },
  422: {
    message: 'The data you submitted cannot be processed.',
    code: 'UNPROCESSABLE_ENTITY',
    action: 'Check your inputs for invalid values',
  },
  429: {
    message: 'Too many requests. Please slow down and try again later.',
    code: 'RATE_LIMIT_EXCEEDED',
    action: 'Wait a moment before trying again',
  },

  // Server errors (5xx)
  500: {
    message: 'Something went wrong on our end. Please try again or contact support.',
    code: 'SERVER_ERROR',
    action: 'Our team has been notified',
    canReport: true,
  },
  502: {
    message: 'The server is temporarily unavailable. Please try again in a moment.',
    code: 'BAD_GATEWAY',
    action: 'Retry in a few seconds',
  },
  503: {
    message: 'The service is temporarily unavailable. Please try again later.',
    code: 'SERVICE_UNAVAILABLE',
    action: 'We are performing maintenance',
  },
  504: {
    message: 'The request took too long to complete. Please try again.',
    code: 'TIMEOUT',
    action: 'Try simplifying your request',
  },

  // Network errors
  NETWORK_ERROR: {
    message: 'Unable to connect to the server. Please check your internet connection.',
    code: 'NETWORK_ERROR',
    action: 'Verify your network connection and try again',
  },
  TIMEOUT_ERROR: {
    message: 'The request took too long. Please try again.',
    code: 'TIMEOUT_ERROR',
    action: 'Your connection may be slow',
  },

  // Application-specific errors
  CSV_IMPORT_ERROR: {
    message: 'Failed to import CSV file. Please check the file format.',
    code: 'CSV_IMPORT_ERROR',
    action: 'Download the template and verify your file matches the format',
  },
  VALIDATION_ERROR: {
    message: 'Please correct the errors in the form before submitting.',
    code: 'VALIDATION_ERROR',
    action: 'Check the highlighted fields',
  },
}

/**
 * Default fallback error message for unknown errors
 */
const DEFAULT_ERROR: UserError = {
  message: 'An unexpected error occurred. Please try again.',
  code: 'UNKNOWN_ERROR',
  action: 'If the problem persists, contact support',
  canReport: true,
}

/**
 * Extracts error details from various error types
 */
function extractErrorDetails(error: unknown): {
  status?: number
  statusText?: string
  message?: string
  originalError: unknown
} {
  // Axios error structure
  if (error && typeof error === 'object' && 'response' in error) {
    const axiosError = error as {
      response?: { status?: number; statusText?: string; data?: { message?: string } }
      message?: string
    }
    return {
      status: axiosError.response?.status,
      statusText: axiosError.response?.statusText,
      message: axiosError.response?.data?.message || axiosError.message,
      originalError: error,
    }
  }

  // Fetch error structure
  if (error && typeof error === 'object' && 'status' in error) {
    const fetchError = error as {
      status?: number
      statusText?: string
      message?: string
    }
    return {
      status: fetchError.status,
      statusText: fetchError.statusText,
      message: fetchError.message,
      originalError: error,
    }
  }

  // Plain Error object
  if (error instanceof Error) {
    return {
      message: error.message,
      originalError: error,
    }
  }

  // Unknown error type
  return {
    message: String(error),
    originalError: error,
  }
}

/**
 * Maps an error to a user-friendly message
 * Story 2.13 Task 10.1: Error message mapping utility
 *
 * @param error - The error to map (can be Error, HTTP error, or unknown)
 * @param logToConsole - Whether to log technical details to console (default: true)
 * @returns User-friendly error object with message, code, and optional action
 *
 * @example
 * ```typescript
 * try {
 *   await createModel(data)
 * } catch (error) {
 *   const userError = mapErrorToUserMessage(error)
 *   toast.error(userError.message)
 *   if (userError.action) {
 *     toast.info(userError.action)
 *   }
 * }
 * ```
 */
export function mapErrorToUserMessage(error: unknown, logToConsole = true): UserError {
  const details = extractErrorDetails(error)

  // Log technical details to console for debugging (Task 10.4)
  if (logToConsole) {
    console.error('[Error Handler]', {
      userFacing: 'Mapping error to user message',
      status: details.status,
      statusText: details.statusText,
      technicalMessage: details.message,
      originalError: details.originalError,
      timestamp: new Date().toISOString(),
    })
  }

  // Map by HTTP status code if available
  if (details.status && ERROR_MESSAGES[details.status]) {
    return ERROR_MESSAGES[details.status]
  }

  // Map by error message patterns
  const message = details.message?.toLowerCase() || ''

  if (message.includes('network') || message.includes('fetch')) {
    return ERROR_MESSAGES.NETWORK_ERROR
  }

  if (message.includes('timeout')) {
    return ERROR_MESSAGES.TIMEOUT_ERROR
  }

  if (message.includes('validation') || message.includes('invalid')) {
    return ERROR_MESSAGES.VALIDATION_ERROR
  }

  if (message.includes('csv') || message.includes('import')) {
    return ERROR_MESSAGES.CSV_IMPORT_ERROR
  }

  // Fallback to default error
  return DEFAULT_ERROR
}

/**
 * Formats validation errors from backend into user-friendly messages
 *
 * @param validationErrors - Object with field names as keys and error messages as values
 * @returns Formatted error message
 *
 * @example
 * ```typescript
 * const errors = {
 *   name: 'Name is required',
 *   email: 'Email is invalid'
 * }
 * const message = formatValidationErrors(errors)
 * // Returns: "Name is required. Email is invalid."
 * ```
 */
export function formatValidationErrors(
  validationErrors: Record<string, string | string[]>
): string {
  const messages: string[] = []

  for (const [, error] of Object.entries(validationErrors)) {
    if (Array.isArray(error)) {
      messages.push(...error)
    } else {
      messages.push(error)
    }
  }

  return messages.join('. ') + '.'
}

/**
 * Checks if an error should show a "Report Issue" button
 */
export function shouldShowReportButton(error: unknown): boolean {
  const userError = mapErrorToUserMessage(error, false)
  return userError.canReport ?? false
}

/**
 * Gets a short error title for display in toasts/alerts
 */
export function getErrorTitle(error: unknown): string {
  const details = extractErrorDetails(error)

  if (details.status) {
    if (details.status >= 500) return 'Server Error'
    if (details.status === 404) return 'Not Found'
    if (details.status === 403) return 'Access Denied'
    if (details.status === 401) return 'Session Expired'
    if (details.status === 400) return 'Invalid Input'
  }

  return 'Error'
}
