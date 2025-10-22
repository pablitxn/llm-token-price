import { useQuery } from '@tanstack/react-query'
import { fetchModels } from '../api/models'
import type { ModelsResponse } from '../types/models'

/**
 * Custom hook for fetching all active models using TanStack Query
 *
 * Story 3.2: Fetch and Display Models in Basic Table
 * Implements AC #1 (Frontend fetches models from GET /api/models endpoint)
 * and AC #3 (Data loads automatically on page mount)
 *
 * Canonical TanStack Query pattern for Epic 3+ with:
 * - 5min staleTime (architecture requirement)
 * - 30min cacheTime (architecture requirement)
 * - Retry up to 2 times for failed requests
 *
 * @returns TanStack Query result with data, isLoading, error, refetch
 */
export const useModels = () => {
  return useQuery<ModelsResponse>({
    queryKey: ['models'],
    queryFn: fetchModels,
    staleTime: 5 * 60 * 1000,  // 5 min (from Architecture)
    gcTime: 30 * 60 * 1000,    // 30 min (formerly cacheTime in v4, now gcTime in v5)
    retry: 2, // Retry failed requests up to 2 times
  })
}
