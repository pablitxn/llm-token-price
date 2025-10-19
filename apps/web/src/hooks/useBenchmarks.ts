/**
 * TanStack Query hooks for benchmark management
 * Handles server state for benchmark CRUD operations with automatic cache invalidation
 */

import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import {
  getAdminBenchmarks,
  getAdminBenchmarkById,
  createBenchmark,
  updateBenchmark,
  deleteBenchmark,
} from '@/api/admin'
import type {
  CreateBenchmarkFormData,
  UpdateBenchmarkFormData,
} from '@/schemas/benchmarkSchema'

/**
 * Query key factory for benchmark queries
 * Provides consistent query key structure for caching and invalidation
 */
export const benchmarkKeys = {
  all: ['benchmarks'] as const,
  lists: () => [...benchmarkKeys.all, 'list'] as const,
  list: (filters?: { includeInactive?: boolean; category?: string }) =>
    [...benchmarkKeys.lists(), filters] as const,
  details: () => [...benchmarkKeys.all, 'detail'] as const,
  detail: (id: string) => [...benchmarkKeys.details(), id] as const,
}

/**
 * Fetches all benchmarks for admin panel
 * TanStack Query automatically caches result and manages loading/error states
 *
 * @param includeInactive - If true, includes inactive benchmarks (default: true)
 * @param category - Optional category filter
 * @returns Query result with benchmarks array, loading state, and error
 */
export const useBenchmarks = (includeInactive = true, category?: string) => {
  return useQuery({
    queryKey: benchmarkKeys.list({ includeInactive, category }),
    queryFn: () => getAdminBenchmarks(includeInactive, category),
    staleTime: 5 * 60 * 1000, // 5 minutes (admin data changes less frequently)
  })
}

/**
 * Fetches a single benchmark by ID
 *
 * @param id - Benchmark unique identifier (GUID)
 * @returns Query result with benchmark details, loading state, and error
 */
export const useBenchmark = (id: string) => {
  return useQuery({
    queryKey: benchmarkKeys.detail(id),
    queryFn: () => getAdminBenchmarkById(id),
    enabled: !!id, // Only fetch if ID is provided
    staleTime: 5 * 60 * 1000,
  })
}

/**
 * Creates a new benchmark definition
 * Automatically invalidates benchmark list cache on success
 *
 * @returns Mutation object with mutate function, loading state, and error handling
 */
export const useCreateBenchmark = () => {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (benchmark: CreateBenchmarkFormData) => createBenchmark(benchmark),
    onSuccess: () => {
      // Invalidate all benchmark list queries to refetch with new benchmark
      queryClient.invalidateQueries({ queryKey: benchmarkKeys.lists() })
    },
  })
}

/**
 * Updates an existing benchmark definition
 * Automatically invalidates related caches on success
 *
 * @returns Mutation object with mutate function, loading state, and error handling
 */
export const useUpdateBenchmark = () => {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateBenchmarkFormData }) =>
      updateBenchmark(id, data),
    onSuccess: (_data, variables) => {
      // Invalidate the specific benchmark detail query
      queryClient.invalidateQueries({ queryKey: benchmarkKeys.detail(variables.id) })
      // Invalidate all benchmark list queries
      queryClient.invalidateQueries({ queryKey: benchmarkKeys.lists() })
    },
  })
}

/**
 * Deletes a benchmark (soft delete)
 * Automatically invalidates benchmark list cache on success
 *
 * @returns Mutation object with mutate function, loading state, and error handling
 */
export const useDeleteBenchmark = () => {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (id: string) => deleteBenchmark(id),
    onSuccess: () => {
      // Invalidate all benchmark queries to refetch updated list
      queryClient.invalidateQueries({ queryKey: benchmarkKeys.all })
    },
  })
}
