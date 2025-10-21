import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest'
import {
  mapErrorToUserMessage,
  formatValidationErrors,
  shouldShowReportButton,
  getErrorTitle,
} from '../errorMessages'

describe('errorMessages utilities', () => {
  let consoleErrorSpy: ReturnType<typeof vi.spyOn>

  beforeEach(() => {
    consoleErrorSpy = vi.spyOn(console, 'error').mockImplementation(() => {})
  })

  afterEach(() => {
    consoleErrorSpy.mockRestore()
  })

  describe('mapErrorToUserMessage', () => {
    it('should map 400 Bad Request to user-friendly message', () => {
      const error = { response: { status: 400 } }
      const result = mapErrorToUserMessage(error)

      expect(result.message).toBe('Invalid data. Please check your inputs and try again.')
      expect(result.code).toBe('VALIDATION_ERROR')
      expect(result.action).toBe('Review the highlighted fields for errors')
    })

    it('should map 401 Unauthorized to session expired message', () => {
      const error = { response: { status: 401 } }
      const result = mapErrorToUserMessage(error)

      expect(result.message).toBe('Your session has expired. Please log in again.')
      expect(result.code).toBe('AUTHENTICATION_ERROR')
    })

    it('should map 404 Not Found to user-friendly message', () => {
      const error = { response: { status: 404 } }
      const result = mapErrorToUserMessage(error)

      expect(result.message).toBe('The requested item was not found.')
      expect(result.code).toBe('NOT_FOUND')
    })

    it('should map 500 Internal Server Error to user-friendly message', () => {
      const error = { response: { status: 500 } }
      const result = mapErrorToUserMessage(error)

      expect(result.message).toBe(
        'Something went wrong on our end. Please try again or contact support.'
      )
      expect(result.code).toBe('SERVER_ERROR')
      expect(result.canReport).toBe(true)
    })

    it('should map 429 Rate Limit to user-friendly message', () => {
      const error = { response: { status: 429 } }
      const result = mapErrorToUserMessage(error)

      expect(result.message).toBe('Too many requests. Please slow down and try again later.')
      expect(result.code).toBe('RATE_LIMIT_EXCEEDED')
    })

    it('should handle network errors by message pattern', () => {
      const error = new Error('Network request failed')
      const result = mapErrorToUserMessage(error)

      expect(result.message).toBe(
        'Unable to connect to the server. Please check your internet connection.'
      )
      expect(result.code).toBe('NETWORK_ERROR')
    })

    it('should handle timeout errors by message pattern', () => {
      const error = new Error('Request timeout exceeded')
      const result = mapErrorToUserMessage(error)

      expect(result.message).toBe('The request took too long. Please try again.')
      expect(result.code).toBe('TIMEOUT_ERROR')
    })

    it('should return default error for unknown error types', () => {
      const error = new Error('Some unknown error')
      const result = mapErrorToUserMessage(error)

      expect(result.message).toBe('An unexpected error occurred. Please try again.')
      expect(result.code).toBe('UNKNOWN_ERROR')
      expect(result.canReport).toBe(true)
    })

    it('should log technical details to console by default', () => {
      const error = { response: { status: 500, data: { message: 'Internal error' } } }
      mapErrorToUserMessage(error)

      expect(consoleErrorSpy).toHaveBeenCalledWith(
        '[Error Handler]',
        expect.objectContaining({
          status: 500,
          technicalMessage: 'Internal error',
        })
      )
    })

    it('should not log to console when logToConsole is false', () => {
      const error = { response: { status: 500 } }
      mapErrorToUserMessage(error, false)

      expect(consoleErrorSpy).not.toHaveBeenCalled()
    })

    it('should handle Fetch API error structure', () => {
      const error = {
        status: 403,
        statusText: 'Forbidden',
        message: 'Access denied',
      }
      const result = mapErrorToUserMessage(error)

      expect(result.message).toBe('You do not have permission to perform this action.')
      expect(result.code).toBe('PERMISSION_ERROR')
    })

    it('should handle plain Error objects', () => {
      const error = new Error('Validation failed: invalid email')
      const result = mapErrorToUserMessage(error)

      expect(result.code).toBe('VALIDATION_ERROR')
    })

    it('should handle string errors', () => {
      const error = 'Something went wrong'
      const result = mapErrorToUserMessage(error)

      expect(result.code).toBe('UNKNOWN_ERROR')
    })
  })

  describe('formatValidationErrors', () => {
    it('should format single validation error', () => {
      const errors = { name: 'Name is required' }
      const result = formatValidationErrors(errors)

      expect(result).toBe('Name is required.')
    })

    it('should format multiple validation errors', () => {
      const errors = {
        name: 'Name is required',
        email: 'Email is invalid',
      }
      const result = formatValidationErrors(errors)

      expect(result).toBe('Name is required. Email is invalid.')
    })

    it('should handle array of errors for a single field', () => {
      const errors = {
        password: ['Password is too short', 'Password must contain a number'],
      }
      const result = formatValidationErrors(errors)

      expect(result).toBe('Password is too short. Password must contain a number.')
    })

    it('should handle mixed string and array errors', () => {
      const errors = {
        name: 'Name is required',
        password: ['Password is too short', 'Password must contain a number'],
        email: 'Email is invalid',
      }
      const result = formatValidationErrors(errors)

      expect(result).toContain('Name is required')
      expect(result).toContain('Password is too short')
      expect(result).toContain('Password must contain a number')
      expect(result).toContain('Email is invalid')
    })

    it('should handle empty errors object', () => {
      const errors = {}
      const result = formatValidationErrors(errors)

      expect(result).toBe('.')
    })
  })

  describe('shouldShowReportButton', () => {
    it('should return true for 500 errors', () => {
      const error = { response: { status: 500 } }
      expect(shouldShowReportButton(error)).toBe(true)
    })

    it('should return true for unknown errors', () => {
      const error = new Error('Random error')
      expect(shouldShowReportButton(error)).toBe(true)
    })

    it('should return false for 400 errors', () => {
      const error = { response: { status: 400 } }
      expect(shouldShowReportButton(error)).toBe(false)
    })

    it('should return false for 404 errors', () => {
      const error = { response: { status: 404 } }
      expect(shouldShowReportButton(error)).toBe(false)
    })

    it('should not log to console', () => {
      const error = { response: { status: 500 } }
      shouldShowReportButton(error)

      expect(consoleErrorSpy).not.toHaveBeenCalled()
    })
  })

  describe('getErrorTitle', () => {
    it('should return "Server Error" for 500 status', () => {
      const error = { response: { status: 500 } }
      expect(getErrorTitle(error)).toBe('Server Error')
    })

    it('should return "Server Error" for 502 status', () => {
      const error = { response: { status: 502 } }
      expect(getErrorTitle(error)).toBe('Server Error')
    })

    it('should return "Not Found" for 404 status', () => {
      const error = { response: { status: 404 } }
      expect(getErrorTitle(error)).toBe('Not Found')
    })

    it('should return "Access Denied" for 403 status', () => {
      const error = { response: { status: 403 } }
      expect(getErrorTitle(error)).toBe('Access Denied')
    })

    it('should return "Session Expired" for 401 status', () => {
      const error = { response: { status: 401 } }
      expect(getErrorTitle(error)).toBe('Session Expired')
    })

    it('should return "Invalid Input" for 400 status', () => {
      const error = { response: { status: 400 } }
      expect(getErrorTitle(error)).toBe('Invalid Input')
    })

    it('should return "Error" for unknown errors', () => {
      const error = new Error('Unknown')
      expect(getErrorTitle(error)).toBe('Error')
    })
  })
})
