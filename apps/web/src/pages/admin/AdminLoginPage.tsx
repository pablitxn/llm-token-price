import { useState, type FormEvent } from 'react'
import { z } from 'zod'
import { useAuth } from '../../hooks/useAuth'

/**
 * Zod schema for login form validation
 * Username: minimum 3 characters
 * Password: minimum 6 characters
 */
const loginSchema = z.object({
  username: z
    .string()
    .min(3, 'Username must be at least 3 characters')
    .max(50, 'Username must not exceed 50 characters'),
  password: z
    .string()
    .min(6, 'Password must be at least 6 characters')
    .max(100, 'Password must not exceed 100 characters'),
})

type LoginFormData = z.infer<typeof loginSchema>

/**
 * AdminLoginPage component
 * Provides secure login interface for admin panel access
 *
 * Features:
 * - Zod validation for username and password
 * - Loading state during authentication
 * - Error message display for failed attempts
 * - Professional TailwindCSS styling
 * - Form accessibility with proper labels and ARIA attributes
 *
 * @component
 */
export default function AdminLoginPage() {
  const { login, isLoading, error: authError } = useAuth()
  const [formData, setFormData] = useState<LoginFormData>({
    username: '',
    password: '',
  })
  const [errors, setErrors] = useState<Partial<Record<keyof LoginFormData, string>>>({})
  const [errorMessage, setErrorMessage] = useState<string | null>(authError)

  /**
   * Validates form data using Zod schema
   * @returns true if validation passes, false otherwise
   */
  const validateForm = (): boolean => {
    try {
      loginSchema.parse(formData)
      setErrors({})
      return true
    } catch (error) {
      if (error instanceof z.ZodError) {
        const fieldErrors: Partial<Record<keyof LoginFormData, string>> = {}
        error.issues.forEach((err: z.ZodIssue) => {
          if (err.path[0]) {
            fieldErrors[err.path[0] as keyof LoginFormData] = err.message
          }
        })
        setErrors(fieldErrors)
      }
      return false
    }
  }

  /**
   * Handles form submission
   * Validates input and initiates authentication process
   */
  const handleSubmit = async (e: FormEvent<HTMLFormElement>) => {
    e.preventDefault()
    setErrorMessage(null)

    if (!validateForm()) {
      return
    }

    try {
      await login(formData.username, formData.password)
      // Login hook handles redirection on success
    } catch (error) {
      const message = error instanceof Error ? error.message : 'Login failed. Please try again.'
      setErrorMessage(message)
    }
  }

  /**
   * Handles input changes and clears field-specific errors
   */
  const handleChange = (field: keyof LoginFormData) => (
    e: React.ChangeEvent<HTMLInputElement>
  ) => {
    setFormData((prev) => ({ ...prev, [field]: e.target.value }))
    setErrors((prev) => ({ ...prev, [field]: undefined }))
    setErrorMessage(null)
  }

  return (
    <div className="min-h-screen flex items-center justify-center bg-gradient-to-br from-slate-50 to-slate-100 px-4 sm:px-6 lg:px-8">
      <div className="max-w-md w-full space-y-8">
        {/* Header */}
        <div className="text-center">
          <h1 className="text-3xl font-bold tracking-tight text-slate-900">
            Admin Panel
          </h1>
          <p className="mt-2 text-sm text-slate-600">
            Sign in to manage model data and benchmarks
          </p>
        </div>

        {/* Login Form */}
        <div className="bg-white shadow-lg rounded-lg border border-slate-200 p-8">
          <form onSubmit={handleSubmit} className="space-y-6" noValidate>
            {/* Global Error Message */}
            {errorMessage && (
              <div
                className="bg-red-50 border border-red-200 text-red-800 rounded-md p-4 text-sm"
                role="alert"
                aria-live="assertive"
              >
                <div className="flex items-start">
                  <svg
                    className="w-5 h-5 text-red-400 mr-2 flex-shrink-0 mt-0.5"
                    fill="currentColor"
                    viewBox="0 0 20 20"
                  >
                    <path
                      fillRule="evenodd"
                      d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z"
                      clipRule="evenodd"
                    />
                  </svg>
                  <span>{errorMessage}</span>
                </div>
              </div>
            )}

            {/* Username Field */}
            <div>
              <label
                htmlFor="username"
                className="block text-sm font-medium text-slate-700 mb-1"
              >
                Username
              </label>
              <input
                id="username"
                name="username"
                type="text"
                autoComplete="username"
                required
                value={formData.username}
                onChange={handleChange('username')}
                disabled={isLoading}
                className={`
                  w-full px-3 py-2 border rounded-md shadow-sm
                  focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent
                  disabled:bg-slate-50 disabled:text-slate-500 disabled:cursor-not-allowed
                  ${
                    errors.username
                      ? 'border-red-300 text-red-900 placeholder-red-300 focus:ring-red-500'
                      : 'border-slate-300 text-slate-900 placeholder-slate-400'
                  }
                `}
                placeholder="Enter your username"
                aria-invalid={!!errors.username}
                aria-describedby={errors.username ? 'username-error' : undefined}
              />
              {errors.username && (
                <p
                  id="username-error"
                  className="mt-1 text-sm text-red-600"
                  role="alert"
                >
                  {errors.username}
                </p>
              )}
            </div>

            {/* Password Field */}
            <div>
              <label
                htmlFor="password"
                className="block text-sm font-medium text-slate-700 mb-1"
              >
                Password
              </label>
              <input
                id="password"
                name="password"
                type="password"
                autoComplete="current-password"
                required
                value={formData.password}
                onChange={handleChange('password')}
                disabled={isLoading}
                className={`
                  w-full px-3 py-2 border rounded-md shadow-sm
                  focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent
                  disabled:bg-slate-50 disabled:text-slate-500 disabled:cursor-not-allowed
                  ${
                    errors.password
                      ? 'border-red-300 text-red-900 placeholder-red-300 focus:ring-red-500'
                      : 'border-slate-300 text-slate-900 placeholder-slate-400'
                  }
                `}
                placeholder="Enter your password"
                aria-invalid={!!errors.password}
                aria-describedby={errors.password ? 'password-error' : undefined}
              />
              {errors.password && (
                <p
                  id="password-error"
                  className="mt-1 text-sm text-red-600"
                  role="alert"
                >
                  {errors.password}
                </p>
              )}
            </div>

            {/* Submit Button */}
            <div>
              <button
                type="submit"
                disabled={isLoading || !formData.username || !formData.password}
                className={`
                  w-full flex justify-center items-center gap-2 py-2.5 px-4
                  border border-transparent rounded-md shadow-sm
                  text-sm font-medium text-white
                  focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500
                  transition-colors duration-200
                  ${
                    isLoading || !formData.username || !formData.password
                      ? 'bg-blue-400 cursor-not-allowed'
                      : 'bg-blue-600 hover:bg-blue-700 active:bg-blue-800'
                  }
                `}
              >
                {isLoading ? (
                  <>
                    <svg
                      className="animate-spin h-5 w-5 text-white"
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
                    <span>Signing in...</span>
                  </>
                ) : (
                  'Sign in'
                )}
              </button>
            </div>
          </form>
        </div>

        {/* Footer Notice */}
        <p className="text-center text-xs text-slate-500">
          This is a secure admin area. Unauthorized access is prohibited.
        </p>
      </div>
    </div>
  )
}
