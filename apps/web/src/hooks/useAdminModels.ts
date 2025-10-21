/**
 * TanStack Query hook for fetching admin models list
 * Provides server state management with automatic caching and refetch logic
 */

import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { getAdminModels, deleteAdminModel } from '@/api/admin'
import type { AdminModelDto, PaginationMetadata } from '@/types/admin'

/**
 * Hook return type (Story 2.13 Task 5.6: Added pagination support)
 */
export interface UseAdminModelsResult {
  /** Array of admin models (including inactive), undefined while loading */
  data: AdminModelDto[] | undefined
  /** Pagination metadata (only present when using pagination) */
  pagination: PaginationMetadata | undefined
  /** Loading state (initial fetch) */
  isLoading: boolean
  /** Error object if request failed, null otherwise */
  error: Error | null
  /** Refetch function to manually trigger data refresh */
  refetch: () => void
  /** Background refetching state (for stale data updates) */
  isFetching: boolean
}

/**
 * Fetches all models for admin panel using TanStack Query
 * Supports optional search, filter, and pagination parameters.
 *
 * Story 2.13 Task 5.6: Added pagination support (page, pageSize)
 *
 * Key features:
 * - Automatic caching with 5min stale time (matches public API pattern)
 * - Background refetch on window focus
 * - Automatic retry on failure (3 attempts)
 * - Returns all models (active, inactive, deprecated, beta)
 * - Pagination support with metadata (totalItems, totalPages, hasNext/hasPrev)
 *
 * @param searchTerm - Optional search term to filter by model name or provider (case-insensitive)
 * @param provider - Optional provider filter (exact match, case-insensitive)
 * @param status - Optional status filter (exact match, case-insensitive)
 * @param page - Optional page number (1-indexed)
 * @param pageSize - Optional page size (default: 20, max: 100)
 * @returns Query result with data, pagination metadata, loading state, error, and refetch function
 *
 * @example Without pagination
 * ```tsx
 * const { data: models, isLoading } = useAdminModels()
 * ```
 *
 * @example With pagination
 * ```tsx
 * const { data: models, pagination, isLoading } = useAdminModels(undefined, undefined, undefined, 1, 20)
 * // pagination = { currentPage: 1, pageSize: 20, totalItems: 150, totalPages: 8, hasNextPage: true, hasPreviousPage: false }
 * ```
 */
export function useAdminModels(
  searchTerm?: string,
  provider?: string,
  status?: string,
  page?: number,
  pageSize?: number
): UseAdminModelsResult {
  const queryResult = useQuery({
    // Query key includes all params to trigger refetch when they change
    queryKey: ['admin', 'models', searchTerm, provider, status, page, pageSize],

    // Query function calls the admin API client
    queryFn: async () => {
      const response = await getAdminModels(searchTerm, provider, status, page, pageSize)
      return response
    },

    // Caching strategy (matches public API pattern)
    staleTime: 5 * 60 * 1000, // 5 minutes - data considered fresh
    gcTime: 10 * 60 * 1000, // 10 minutes - cache garbage collection time (formerly cacheTime)

    // Refetch configuration
    refetchOnWindowFocus: true, // Refetch when user returns to tab
    refetchOnReconnect: true, // Refetch when network reconnects

    // Retry configuration
    retry: 3, // Retry failed requests up to 3 times
    retryDelay: (attemptIndex) => Math.min(1000 * 2 ** attemptIndex, 30000), // Exponential backoff
  })

  // Extract data and pagination from response based on response type
  // If pagination params were provided, response.data is PagedResult<AdminModelDto>
  // Otherwise, response.data is AdminModelDto[]
  const isPaginated = page !== undefined || pageSize !== undefined
  const data = isPaginated
    ? (queryResult.data?.data as { items: AdminModelDto[] })?.items
    : (queryResult.data?.data as AdminModelDto[])

  const pagination = isPaginated
    ? (queryResult.data?.data as { pagination: PaginationMetadata })?.pagination
    : undefined

  return {
    data,
    pagination,
    isLoading: queryResult.isLoading,
    error: queryResult.error,
    refetch: queryResult.refetch,
    isFetching: queryResult.isFetching,
  }
}

/**
 * Hook for fetching a single admin model by ID
 *
 * @param id - Model unique identifier (GUID)
 * @returns Query result with model data, loading state, error, and refetch function
 *
 * @example
 * ```tsx
 * function EditModelPage({ modelId }: { modelId: string }) {
 *   const { data: model, isLoading, error } = useAdminModel(modelId)
 *
 *   if (isLoading) return <div>Loading...</div>
 *   if (error) return <div>Error: {error.message}</div>
 *   if (!model) return <div>Model not found</div>
 *
 *   return <ModelEditForm model={model} />
 * }
 * ```
 */
export function useAdminModel(id: string) {
  return useQuery({
    queryKey: ['admin', 'models', id],
    queryFn: async () => {
      const { getAdminModelById } = await import('@/api/admin')
      const response = await getAdminModelById(id)
      return response.data
    },
    staleTime: 5 * 60 * 1000,
    gcTime: 10 * 60 * 1000,
    enabled: !!id, // Only run query if id is provided
  })
}

/**
 * Hook for deleting an admin model
 * Uses TanStack Query mutation with automatic cache invalidation
 *
 * @returns Mutation object with mutate, mutateAsync, isLoading, error, etc.
 *
 * @example
 * ```tsx
 * function ModelList() {
 *   const deleteMutation = useDeleteModel()
 *
 *   const handleDelete = async (id: string) => {
 *     try {
 *       await deleteMutation.mutateAsync(id)
 *       toast.success('Model deleted successfully')
 *     } catch (error) {
 *       toast.error('Failed to delete model')
 *     }
 *   }
 *
 *   return <button onClick={() => handleDelete(model.id)}>Delete</button>
 * }
 * ```
 */
export function useDeleteModel() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (id: string) => {
      const response = await deleteAdminModel(id)
      return response
    },
    onSuccess: () => {
      // Invalidate all admin models queries to trigger refetch
      queryClient.invalidateQueries({ queryKey: ['admin', 'models'] })
    },
    onError: (error: Error) => {
      console.error('Failed to delete model:', error)
    },
  })
}
