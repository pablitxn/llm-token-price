import { apiClient } from './client'

/**
 * Authentication API response structure
 */
export interface AuthResponse {
  success: boolean
  message: string
}

/**
 * Login request payload
 */
export interface LoginRequest {
  username: string
  password: string
}

/**
 * Authenticates admin user with provided credentials
 * JWT token is automatically stored in HttpOnly cookie by the server
 *
 * @param username - Admin username
 * @param password - Admin password
 * @returns Promise resolving to authentication response
 * @throws Error if authentication fails (401) or request is malformed (400)
 */
export const login = async (username: string, password: string): Promise<AuthResponse> => {
  const response = await apiClient.post<AuthResponse>('/admin/auth/login', {
    username,
    password,
  })
  return response.data
}

/**
 * Logs out the current admin user
 * Clears the JWT token cookie on the server side
 *
 * @returns Promise resolving to logout confirmation
 */
export const logout = async (): Promise<AuthResponse> => {
  const response = await apiClient.post<AuthResponse>('/admin/auth/logout')
  return response.data
}

/**
 * Verifies if the current user has a valid authentication session
 * This can be used to check authentication status on app load
 *
 * @returns Promise resolving to true if authenticated, false otherwise
 */
export const checkAuthStatus = async (): Promise<boolean> => {
  try {
    // You can implement a dedicated /auth/verify endpoint later
    // For now, we'll rely on axios interceptors and cookie presence
    // This is a placeholder that always returns false until implemented
    return false
  } catch {
    return false
  }
}
