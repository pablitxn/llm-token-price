import { useCallback } from 'react'
import { useNavigate, useLocation } from 'react-router-dom'
import { useAuthStore } from '../store/authStore'
import * as adminApi from '../api/admin'

/**
 * Custom hook for authentication operations
 *
 * Provides methods for login, logout, and access to auth state
 * Integrates Zustand store with API calls and navigation
 *
 * @returns Authentication state and methods
 *
 * @example
 * ```tsx
 * function LoginPage() {
 *   const { login, isLoading, error } = useAuth()
 *
 *   const handleSubmit = async () => {
 *     await login('admin', 'password123')
 *   }
 * }
 * ```
 */
export function useAuth() {
  const navigate = useNavigate()
  const location = useLocation()
  const { isAuthenticated, user, isLoading, error, setUser, clearAuth, setLoading, setError } =
    useAuthStore()

  /**
   * Authenticates user with provided credentials
   * On success, stores user info and redirects to intended destination or admin dashboard
   *
   * @param username - Admin username
   * @param password - Admin password
   * @param redirectTo - Path to redirect after successful login (optional, uses location state if available)
   */
  const login = useCallback(
    async (username: string, password: string, redirectTo?: string) => {
      setLoading(true)
      setError(null)

      try {
        const response = await adminApi.login(username, password)

        if (response.success) {
          // Store user information in Zustand store
          // JWT token is already stored in HttpOnly cookie by the server
          setUser({
            username,
            role: 'admin',
          })

          // Redirect to intended destination (from location state) or provided redirectTo or default /admin
          const destination = redirectTo || (location.state as { from?: string })?.from || '/admin'
          navigate(destination, { replace: true })
        } else {
          setError(response.message)
        }
      } catch (err) {
        const errorMessage =
          err instanceof Error ? err.message : 'Login failed. Please try again.'
        setError(errorMessage)
        throw err // Re-throw for component-level error handling if needed
      } finally {
        setLoading(false)
      }
    },
    [location, navigate, setUser, setLoading, setError]
  )

  /**
   * Logs out the current user
   * Clears JWT cookie on server and auth state in client
   *
   * @param redirectTo - Path to redirect after logout (default: /admin/login)
   */
  const logout = useCallback(
    async (redirectTo: string = '/admin/login') => {
      setLoading(true)

      try {
        await adminApi.logout()
        clearAuth()
        navigate(redirectTo)
      } catch (err) {
        console.error('Logout failed:', err)
        // Clear local state even if API call fails (best effort)
        clearAuth()
        navigate(redirectTo)
      } finally {
        setLoading(false)
      }
    },
    [navigate, clearAuth, setLoading]
  )

  return {
    isAuthenticated,
    user,
    login,
    logout,
    isLoading,
    error,
  }
}
