import { Navigate, useLocation } from 'react-router-dom'
import { useAuthStore } from '../../store/authStore'

/**
 * Props for the ProtectedRoute component
 */
interface ProtectedRouteProps {
  /** Child components to render if authenticated */
  children: React.ReactNode

  /** Path to redirect to if not authenticated (default: /admin/login) */
  redirectTo?: string
}

/**
 * ProtectedRoute Component
 *
 * Wrapper component that restricts access to authenticated users only.
 * Redirects to login page if user is not authenticated, preserving the
 * intended destination for post-login redirect.
 *
 * @param props - Component props
 * @returns Protected content if authenticated, otherwise redirects to login
 *
 * @example
 * ```tsx
 * <Route
 *   path="/admin/*"
 *   element={
 *     <ProtectedRoute>
 *       <AdminDashboard />
 *     </ProtectedRoute>
 *   }
 * />
 * ```
 */
export default function ProtectedRoute({
  children,
  redirectTo = '/admin/login',
}: ProtectedRouteProps) {
  const { isAuthenticated } = useAuthStore()
  const location = useLocation()

  if (!isAuthenticated) {
    // Redirect to login, preserving the intended destination
    // The login page can use this state to redirect back after successful auth
    return <Navigate to={redirectTo} state={{ from: location.pathname }} replace />
  }

  return <>{children}</>
}
