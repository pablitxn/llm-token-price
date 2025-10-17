import { create } from 'zustand'
import { persist } from 'zustand/middleware'

/**
 * Authenticated user information
 */
export interface User {
  username: string
  role: string
}

/**
 * Authentication state interface
 */
interface AuthState {
  /** Whether the user is currently authenticated */
  isAuthenticated: boolean

  /** Authenticated user information (null if not authenticated) */
  user: User | null

  /** Whether an authentication operation is in progress */
  isLoading: boolean

  /** Error message from last failed authentication attempt */
  error: string | null

  /** Sets the authenticated user and updates isAuthenticated status */
  setUser: (user: User) => void

  /** Clears authentication state (logout) */
  clearAuth: () => void

  /** Sets loading state */
  setLoading: (isLoading: boolean) => void

  /** Sets error state */
  setError: (error: string | null) => void
}

/**
 * Zustand store for authentication state management
 *
 * Features:
 * - Persists authentication state to localStorage
 * - Provides actions for login/logout state updates
 * - Tracks loading and error states
 *
 * Note: The actual JWT token is stored in HttpOnly cookies (server-side),
 * not in this store. This store only tracks authentication status and user info.
 */
export const useAuthStore = create<AuthState>()(
  persist(
    (set) => ({
      isAuthenticated: false,
      user: null,
      isLoading: false,
      error: null,

      setUser: (user) =>
        set({
          user,
          isAuthenticated: true,
          error: null,
        }),

      clearAuth: () =>
        set({
          user: null,
          isAuthenticated: false,
          error: null,
        }),

      setLoading: (isLoading) =>
        set({
          isLoading,
        }),

      setError: (error) =>
        set({
          error,
        }),
    }),
    {
      name: 'auth-storage', // localStorage key
      partialize: (state) => ({
        // Only persist user info and auth status, not loading/error states
        isAuthenticated: state.isAuthenticated,
        user: state.user,
      }),
    }
  )
)
