/**
 * TanStack Query hook for fetching dashboard metrics
 * Provides server state management with automatic caching
 * Story 2.12: Dashboard freshness metrics
 */

import { useQuery } from '@tanstack/react-query'
import { getDashboardMetrics } from '@/api/admin'
import type { DashboardMetricsDto } from '@/types/admin'

/**
 * Hook return type
 */
export interface UseDashboardMetricsResult {
  /** Dashboard metrics data (counts by freshness category), undefined while loading */
  data: DashboardMetricsDto | undefined
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
 * Fetches dashboard metrics using TanStack Query
 * Metrics are cached server-side for 5 minutes
 *
 * Key features:
 * - Automatic caching with 2min stale time (metrics change infrequently)
 * - Background refetch on window focus
 * - Automatic retry on failure (3 attempts)
 *
 * @returns Query result with data, loading state, error, and refetch function
 *
 * @example
 * ```tsx
 * function AdminDashboard() {
 *   const { data: metrics, isLoading, error } = useDashboardMetrics()
 *
 *   if (isLoading) return <div>Loading...</div>
 *   if (error) return <div>Error: {error.message}</div>
 *
 *   return <MetricCard value={metrics.totalActiveModels} title="Total Models" />
 * }
 * ```
 */
export function useDashboardMetrics(): UseDashboardMetricsResult {
  const queryResult = useQuery({
    // Query key for dashboard metrics
    queryKey: ['admin', 'dashboard', 'metrics'],

    // Query function calls the admin API client
    queryFn: async () => {
      const response = await getDashboardMetrics()
      return response.data // Extract data object from response
    },

    // Caching strategy (metrics change less frequently than models)
    staleTime: 2 * 60 * 1000, // 2 minutes - data considered fresh
    gcTime: 5 * 60 * 1000, // 5 minutes - cache garbage collection time

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
