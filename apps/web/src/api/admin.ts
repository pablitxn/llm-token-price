import { apiClient } from './client'
import type {
  AdminModelsResponse,
  AdminModelResponse,
  CreateModelRequest,
} from '@/types/admin'

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

/**
 * Fetches all models for admin panel (including inactive)
 * Supports search by name/provider and filtering by status.
 * Admin endpoint returns ALL models and is NEVER cached.
 *
 * @param searchTerm - Optional search term to filter by model name or provider (case-insensitive)
 * @param provider - Optional provider filter (exact match, case-insensitive)
 * @param status - Optional status filter (exact match, case-insensitive)
 * @returns Promise resolving to admin models response (all models including inactive, ordered by updatedAt DESC)
 * @throws Error if request fails (401 Unauthorized if not authenticated, 500 Internal Server Error)
 */
export const getAdminModels = async (
  searchTerm?: string,
  provider?: string,
  status?: string
): Promise<AdminModelsResponse> => {
  const params = new URLSearchParams()
  if (searchTerm) params.append('searchTerm', searchTerm)
  if (provider) params.append('provider', provider)
  if (status) params.append('status', status)

  const queryString = params.toString()
  const url = queryString ? `/admin/models?${queryString}` : '/admin/models'

  const response = await apiClient.get<AdminModelsResponse>(url)
  return response.data
}

/**
 * Fetches a single model by ID for admin panel (including inactive)
 *
 * @param id - Model unique identifier (GUID)
 * @returns Promise resolving to admin model response
 * @throws Error if model not found (404) or request fails (401, 500)
 */
export const getAdminModelById = async (id: string): Promise<AdminModelResponse> => {
  const response = await apiClient.get<AdminModelResponse>(`/admin/models/${id}`)
  return response.data
}

/**
 * Creates a new model in the admin panel
 * Posts model data to backend API which validates and stores in database.
 * On success, cache:models:* patterns are invalidated server-side.
 *
 * @param model - Create model request payload with all required fields
 * @returns Promise resolving to created model with ID and audit timestamps
 * @throws Error if validation fails (400 Bad Request with field details), unauthorized (401), or server error (500)
 */
export const createModel = async (
  model: CreateModelRequest
): Promise<AdminModelResponse> => {
  const response = await apiClient.post<AdminModelResponse>('/admin/models', model)
  return response.data
}

/**
 * Updates an existing model in the admin panel
 * Sends model data to backend API which validates and updates in database.
 * On success, cache:models:* patterns are invalidated server-side.
 *
 * @param id - Model unique identifier (GUID)
 * @param model - Update model request payload with fields to update
 * @returns Promise resolving to updated model with refreshed UpdatedAt timestamp
 * @throws Error if model not found (404), validation fails (400 Bad Request with field details), unauthorized (401), or server error (500)
 */
export const updateModel = async (
  id: string,
  model: CreateModelRequest
): Promise<AdminModelResponse> => {
  const response = await apiClient.put<AdminModelResponse>(`/admin/models/${id}`, model)
  return response.data
}

/**
 * Deletes a model from the system (soft delete - sets isActive = false)
 *
 * @param id - Model unique identifier (GUID)
 * @returns Promise resolving when deletion is successful
 * @throws Error if model not found (404), unauthorized (401), or request fails (500)
 */
export const deleteAdminModel = async (id: string): Promise<void> => {
  await apiClient.delete(`/admin/models/${id}`)
}
