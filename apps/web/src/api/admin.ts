import { apiClient } from './client'
import type {
  AdminModelsResponse,
  AdminModelsPagedResponse,
  AdminModelResponse,
  CreateModelRequest,
  BenchmarkScoresResponse,
  BenchmarkScoreResponse,
  CreateBenchmarkScoreDto,
  CSVImportResultDto,
  DashboardMetricsResponse,
} from '@/types/admin'
import type {
  BenchmarkResponseDto,
  CreateBenchmarkFormData,
  UpdateBenchmarkFormData,
} from '@/schemas/benchmarkSchema'
import type {
  AuditLogResponse,
  AuditLogFilters,
} from '@/types/auditLog'

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
 * Supports search by name/provider, filtering by status, and pagination.
 * Admin endpoint returns ALL models and is NEVER cached.
 *
 * Story 2.13 Task 5.6: Added pagination support (page, pageSize)
 *
 * @param searchTerm - Optional search term to filter by model name or provider (case-insensitive)
 * @param provider - Optional provider filter (exact match, case-insensitive)
 * @param status - Optional status filter (exact match, case-insensitive)
 * @param page - Optional page number (1-indexed, requires pageSize)
 * @param pageSize - Optional page size (default: 20, max: 100)
 * @returns Promise resolving to paginated response if page/pageSize provided, otherwise full list
 * @throws Error if request fails (401 Unauthorized if not authenticated, 500 Internal Server Error)
 */
export const getAdminModels = async (
  searchTerm?: string,
  provider?: string,
  status?: string,
  page?: number,
  pageSize?: number
): Promise<AdminModelsResponse | AdminModelsPagedResponse> => {
  const params = new URLSearchParams()
  if (searchTerm) params.append('searchTerm', searchTerm)
  if (provider) params.append('provider', provider)
  if (status) params.append('status', status)
  if (page !== undefined) params.append('page', page.toString())
  if (pageSize !== undefined) params.append('pageSize', pageSize.toString())

  const queryString = params.toString()
  const url = queryString ? `/admin/models?${queryString}` : '/admin/models'

  // Response type differs based on whether pagination is used
  if (page !== undefined || pageSize !== undefined) {
    const response = await apiClient.get<AdminModelsPagedResponse>(url)
    return response.data
  } else {
    const response = await apiClient.get<AdminModelsResponse>(url)
    return response.data
  }
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

// ============================================================================
// Benchmark Management API Functions
// ============================================================================

/**
 * Fetches all benchmarks for admin panel (including inactive)
 * Supports filtering by category.
 * Admin endpoint returns ALL benchmarks and is NEVER cached.
 *
 * @param includeInactive - If true, includes inactive benchmarks (default: true)
 * @param category - Optional category filter (Reasoning, Code, Math, Language, Multimodal)
 * @returns Promise resolving to array of benchmark response DTOs (ordered alphabetically by name)
 * @throws Error if request fails (401 Unauthorized if not authenticated, 500 Internal Server Error)
 */
export const getAdminBenchmarks = async (
  includeInactive = true,
  category?: string
): Promise<BenchmarkResponseDto[]> => {
  const params = new URLSearchParams()
  params.append('includeInactive', includeInactive.toString())
  if (category) params.append('category', category)

  const queryString = params.toString()
  const url = queryString ? `/admin/benchmarks?${queryString}` : '/admin/benchmarks'

  const response = await apiClient.get<BenchmarkResponseDto[]>(url)
  return response.data
}

/**
 * Fetches a single benchmark by ID for admin panel (including inactive)
 *
 * @param id - Benchmark unique identifier (GUID)
 * @returns Promise resolving to benchmark response DTO
 * @throws Error if benchmark not found (404) or request fails (401, 500)
 */
export const getAdminBenchmarkById = async (id: string): Promise<BenchmarkResponseDto> => {
  const response = await apiClient.get<BenchmarkResponseDto>(`/admin/benchmarks/${id}`)
  return response.data
}

/**
 * Creates a new benchmark definition in the admin panel
 * Posts benchmark data to backend API which validates (including unique name check) and stores in database.
 * On success, cache:benchmarks:*, cache:qaps:*, cache:bestvalue:* patterns are invalidated server-side.
 *
 * @param benchmark - Create benchmark request payload with all required fields
 * @returns Promise resolving to created benchmark with ID and timestamps
 * @throws Error if validation fails (400 Bad Request), duplicate name (409 Conflict), unauthorized (401), or server error (500)
 */
export const createBenchmark = async (
  benchmark: CreateBenchmarkFormData
): Promise<BenchmarkResponseDto> => {
  const response = await apiClient.post<BenchmarkResponseDto>('/admin/benchmarks', benchmark)
  return response.data
}

/**
 * Updates an existing benchmark definition in the admin panel
 * BenchmarkName is immutable and cannot be changed via UPDATE.
 * Sends benchmark data to backend API which validates and updates in database.
 * On success, cache:benchmarks:*, cache:qaps:*, cache:bestvalue:* patterns are invalidated server-side.
 *
 * @param id - Benchmark unique identifier (GUID)
 * @param benchmark - Update benchmark request payload with fields to update (excludes benchmarkName)
 * @returns Promise resolving to updated benchmark
 * @throws Error if benchmark not found (404), validation fails (400 Bad Request), unauthorized (401), or server error (500)
 */
export const updateBenchmark = async (
  id: string,
  benchmark: UpdateBenchmarkFormData
): Promise<BenchmarkResponseDto> => {
  const response = await apiClient.put<BenchmarkResponseDto>(`/admin/benchmarks/${id}`, benchmark)
  return response.data
}

/**
 * Deletes a benchmark from the system (soft delete - sets isActive = false)
 * Backend checks for dependent BenchmarkScores before deletion.
 *
 * @param id - Benchmark unique identifier (GUID)
 * @returns Promise resolving when deletion is successful
 * @throws Error if benchmark not found (404), has dependent scores (400 Bad Request), unauthorized (401), or request fails (500)
 */
export const deleteBenchmark = async (id: string): Promise<void> => {
  await apiClient.delete(`/admin/benchmarks/${id}`)
}

// ============================================================================
// Benchmark Score Management API Functions
// ============================================================================

/**
 * Fetches all benchmark scores for a specific model
 * Returns denormalized data including benchmark name and category for display.
 *
 * @param modelId - Model unique identifier (GUID)
 * @returns Promise resolving to array of benchmark score response DTOs (ordered alphabetically by benchmark name)
 * @throws Error if request fails (401 Unauthorized if not authenticated, 500 Internal Server Error)
 */
export const getModelBenchmarkScores = async (
  modelId: string
): Promise<BenchmarkScoresResponse> => {
  const response = await apiClient.get<BenchmarkScoresResponse>(
    `/admin/models/${modelId}/benchmarks`
  )
  return response.data
}

/**
 * Adds a new benchmark score to a model
 * Validates model and benchmark existence, prevents duplicates, calculates normalized score server-side.
 * Displays warning in UI if score falls outside benchmark's typical range (but allows submission).
 * On success, cache:model:{modelId}:*, cache:qaps:*, cache:bestvalue:* patterns are invalidated server-side.
 *
 * @param modelId - Model unique identifier (GUID)
 * @param score - Create benchmark score request payload with all required fields
 * @returns Promise resolving to created benchmark score with normalized score and isOutOfRange flag
 * @throws Error if model/benchmark not found (400 Bad Request), duplicate score exists (400 Bad Request), unauthorized (401), or server error (500)
 */
export const addBenchmarkScore = async (
  modelId: string,
  score: CreateBenchmarkScoreDto
): Promise<BenchmarkScoreResponse> => {
  const response = await apiClient.post<BenchmarkScoreResponse>(
    `/admin/models/${modelId}/benchmarks`,
    score
  )
  return response.data
}

/**
 * Updates an existing benchmark score for a model
 * Recalculates normalized score server-side and invalidates cache.
 * If benchmarkId changes, enforces unique constraint (one score per model+benchmark).
 *
 * @param modelId - Model unique identifier (GUID)
 * @param scoreId - Benchmark score unique identifier (GUID)
 * @param score - Update benchmark score request payload with fields to update
 * @returns Promise resolving to updated benchmark score with refreshed normalized score
 * @throws Error if score not found or doesn't belong to model (404), benchmark not found (400), duplicate exists (400), unauthorized (401), or server error (500)
 */
export const updateBenchmarkScore = async (
  modelId: string,
  scoreId: string,
  score: CreateBenchmarkScoreDto
): Promise<BenchmarkScoreResponse> => {
  const response = await apiClient.put<BenchmarkScoreResponse>(
    `/admin/models/${modelId}/benchmarks/${scoreId}`,
    score
  )
  return response.data
}

/**
 * Deletes a benchmark score from a model (hard delete - physical removal)
 * Unlike models/benchmarks, scores don't use soft delete.
 * On success, cache:model:{modelId}:*, cache:qaps:*, cache:bestvalue:* patterns are invalidated server-side.
 *
 * @param modelId - Model unique identifier (GUID)
 * @param scoreId - Benchmark score unique identifier (GUID)
 * @returns Promise resolving when deletion is successful
 * @throws Error if score not found or doesn't belong to model (404), unauthorized (401), or request fails (500)
 */
export const deleteBenchmarkScore = async (
  modelId: string,
  scoreId: string
): Promise<void> => {
  await apiClient.delete(`/admin/models/${modelId}/benchmarks/${scoreId}`)
}

/**
 * Imports multiple benchmark scores via CSV file upload (Story 2.11)
 * Accepts multipart/form-data with CSV file, validates each row, imports valid scores
 * Returns detailed results with success/failure counts and error details
 *
 * CSV Format:
 * - model_id: UUID (required)
 * - benchmark_name: string (required)
 * - score: decimal (required)
 * - max_score: decimal (optional)
 * - test_date: YYYY-MM-DD (optional)
 * - source_url: URL (optional)
 * - verified: true/false (optional)
 * - notes: string (optional)
 *
 * Features:
 * - Partial success: valid rows imported even if some fail
 * - Row-by-row validation with detailed error messages
 * - Duplicate detection (skips by default)
 * - File size limit: 10MB max
 *
 * @param formData - FormData containing CSV file (key: 'file')
 * @returns Promise resolving to import result with success/failure counts and errors
 * @throws Error if file is invalid format, exceeds size limit, or server error occurs
 */
export const importBenchmarkCSV = async (
  formData: FormData
): Promise<CSVImportResultDto> => {
  const response = await apiClient.post<CSVImportResultDto>(
    '/admin/benchmarks/import-csv',
    formData,
    {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    }
  )
  return response.data
}

// ============================================================================
// Dashboard Metrics API Functions (Story 2.12)
// ============================================================================

/**
 * Fetches dashboard metrics showing data freshness statistics
 * Returns counts of models by freshness category (fresh/stale/critical)
 * Cached server-side for 5 minutes for performance
 *
 * @returns Promise resolving to dashboard metrics response
 * @throws Error if request fails (401 Unauthorized if not authenticated, 500 Internal Server Error)
 */
export const getDashboardMetrics = async (): Promise<DashboardMetricsResponse> => {
  const response = await apiClient.get<DashboardMetricsResponse>('/admin/dashboard/metrics')
  return response.data
}

// ============================================================================
// Audit Log API Functions (Story 2.13 Task 14)
// ============================================================================

/**
 * Fetches paginated audit logs with optional filtering
 * Returns audit trail records of all admin CRUD operations
 * Supports filtering by user, entity type, action, and date range
 *
 * Story 2.13 Task 14: Comprehensive audit logging for compliance
 *
 * @param filters - Optional filters (userId, entityType, action, startDate, endDate, page, pageSize)
 * @returns Promise resolving to paginated audit log response
 * @throws Error if request fails (401 Unauthorized if not authenticated, 400 Bad Request if pagination invalid)
 *
 * @example
 * // Fetch first page of audit logs
 * const logs = await getAuditLogs({ page: 1, pageSize: 20 })
 *
 * // Filter by user
 * const userLogs = await getAuditLogs({ userId: 'admin@example.com', page: 1, pageSize: 20 })
 *
 * // Filter by entity type and action
 * const modelUpdates = await getAuditLogs({ entityType: 'Model', action: 'Update', page: 1, pageSize: 20 })
 *
 * // Filter by date range
 * const recentLogs = await getAuditLogs({
 *   startDate: '2024-01-01',
 *   endDate: '2024-01-31',
 *   page: 1,
 *   pageSize: 20
 * })
 */
export const getAuditLogs = async (filters?: AuditLogFilters): Promise<AuditLogResponse> => {
  const params = new URLSearchParams()

  if (filters?.userId) params.append('userId', filters.userId)
  if (filters?.entityType) params.append('entityType', filters.entityType)
  if (filters?.action) params.append('action', filters.action)
  if (filters?.startDate) params.append('startDate', filters.startDate)
  if (filters?.endDate) params.append('endDate', filters.endDate)
  if (filters?.page) params.append('page', filters.page.toString())
  if (filters?.pageSize) params.append('pageSize', filters.pageSize.toString())

  const url = `/admin/audit-log${params.toString() ? `?${params.toString()}` : ''}`
  const response = await apiClient.get<AuditLogResponse>(url)
  return response.data
}

/**
 * Exports audit logs to CSV format with optional filtering
 * Returns a downloadable CSV file containing all audit logs matching the filters
 * No pagination - exports ALL matching records
 *
 * CSV Format:
 * - Id: UUID
 * - Timestamp: ISO 8601 datetime
 * - UserId: User identifier (email or username)
 * - Action: Create, Update, Delete, Import
 * - EntityType: Model, Benchmark, BenchmarkScore
 * - EntityId: UUID of affected entity
 * - OldValues: JSON string (before state)
 * - NewValues: JSON string (after state)
 *
 * Story 2.13 Task 14.9: CSV export for audit compliance and analysis
 *
 * @param filters - Optional filters (userId, entityType, action, startDate, endDate)
 * @returns Promise resolving to Blob containing CSV file
 * @throws Error if request fails (401 Unauthorized if not authenticated)
 *
 * @example
 * // Export all audit logs
 * const blob = await exportAuditLogsToCSV()
 * const url = URL.createObjectURL(blob)
 * const a = document.createElement('a')
 * a.href = url
 * a.download = 'audit-log-2024-01-15.csv'
 * a.click()
 *
 * // Export filtered audit logs
 * const blob = await exportAuditLogsToCSV({
 *   entityType: 'Model',
 *   startDate: '2024-01-01',
 *   endDate: '2024-01-31'
 * })
 */
export const exportAuditLogsToCSV = async (
  filters?: Omit<AuditLogFilters, 'page' | 'pageSize'>
): Promise<Blob> => {
  const params = new URLSearchParams()

  if (filters?.userId) params.append('userId', filters.userId)
  if (filters?.entityType) params.append('entityType', filters.entityType)
  if (filters?.action) params.append('action', filters.action)
  if (filters?.startDate) params.append('startDate', filters.startDate)
  if (filters?.endDate) params.append('endDate', filters.endDate)

  const url = `/admin/audit-log/export${params.toString() ? `?${params.toString()}` : ''}`
  const response = await apiClient.get(url, {
    responseType: 'blob', // Important: Tell axios to handle binary data
  })
  return response.data as Blob
}
