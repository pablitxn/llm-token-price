/**
 * TanStack Query hook for fetching admin models list
 * Provides server state management with automatic caching and refetch logic
 */

import { useQuery } from '@tanstack/react-query'
import { getAdminModels } from '@/api/admin'
import type { AdminModelDto } from '@/types/admin'

/**
 * Hook return type
 */
export interface UseAdminModelsResult {
  /** Array of admin models (including inactive), undefined while loading */
  data: AdminModelDto[] | undefined
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
 * Supports optional search and filter parameters.
 *
 * Key features:
 * - Automatic caching with 5min stale time (matches public API pattern)
 * - Background refetch on window focus
 * - Automatic retry on failure (3 attempts)
 * - Returns all models (active, inactive, deprecated, beta)
 *
 * @param searchTerm - Optional search term to filter by model name or provider (case-insensitive)
 * @param provider - Optional provider filter (exact match, case-insensitive)
 * @param status - Optional status filter (exact match, case-insensitive)
 * @returns Query result with data, loading state, error, and refetch function
 *
 * @example
 * ```tsx
 * function AdminModelsPage() {
 *   const { data: models, isLoading, error } = useAdminModels()
 *
 *   if (isLoading) return <div>Loading...</div>
 *   if (error) return <div>Error: {error.message}</div>
 *
 *   return <ModelList models={models} />
 * }
 * ```
 *
 * @example With search
 * ```tsx
 * const [searchTerm, setSearchTerm] = useState('')
 * const { data: models } = useAdminModels(searchTerm)
 * ```
 */
export function useAdminModels(
  searchTerm?: string,
  provider?: string,
  status?: string
): UseAdminModelsResult {
  const queryResult = useQuery({
    // Query key includes search params to trigger refetch when they change
    queryKey: ['admin', 'models', searchTerm, provider, status],

    // Query function calls the admin API client
    queryFn: async () => {
      const response = await getAdminModels(searchTerm, provider, status)
      return response.data // Extract data array from response
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

  return {
    data: queryResult.data,
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
