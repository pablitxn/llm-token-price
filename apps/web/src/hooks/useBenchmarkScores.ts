/**
 * TanStack Query hooks for benchmark score management
 * Handles server state for benchmark score CRUD operations with automatic cache invalidation
 * Story 2.10 - Benchmark Score Entry Form
 * Story 2.11 - Bulk CSV Import
 */

import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import {
  getModelBenchmarkScores,
  addBenchmarkScore,
  updateBenchmarkScore,
  deleteBenchmarkScore,
  importBenchmarkCSV,
} from '@/api/admin'
import type { CreateBenchmarkScoreDto } from '@/types/admin'

/**
 * Query key factory for benchmark score queries
 * Provides consistent query key structure for caching and invalidation
 */
export const benchmarkScoreKeys = {
  all: ['benchmarkScores'] as const,
  byModel: (modelId: string) => [...benchmarkScoreKeys.all, 'model', modelId] as const,
}

/**
 * Fetches all benchmark scores for a specific model
 * TanStack Query automatically caches result and manages loading/error states
 *
 * @param modelId - Model unique identifier (GUID)
 * @returns Query result with benchmark scores array, loading state, and error
 */
export const useBenchmarkScores = (modelId: string) => {
  return useQuery({
    queryKey: benchmarkScoreKeys.byModel(modelId),
    queryFn: () => getModelBenchmarkScores(modelId),
    enabled: !!modelId, // Only fetch if modelId is provided
    staleTime: 5 * 60 * 1000, // 5 minutes (scores change less frequently)
  })
}

/**
 * Adds a new benchmark score to a model
 * Automatically invalidates model's score cache and model detail cache on success
 * Server calculates normalized score and validates uniqueness
 *
 * @returns Mutation object with mutate function, loading state, and error handling
 */
export const useAddBenchmarkScore = (modelId: string) => {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (score: CreateBenchmarkScoreDto) => addBenchmarkScore(modelId, score),
    onSuccess: () => {
      // Invalidate benchmark scores query for this model
      queryClient.invalidateQueries({ queryKey: benchmarkScoreKeys.byModel(modelId) })
      // Invalidate model detail query (scores affect model display)
      queryClient.invalidateQueries({ queryKey: ['adminModels', 'detail', modelId] })
    },
  })
}

/**
 * Updates an existing benchmark score for a model
 * Recalculates normalized score server-side and invalidates related caches
 *
 * @returns Mutation object with mutate function, loading state, and error handling
 */
export const useUpdateBenchmarkScore = (modelId: string) => {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ scoreId, data }: { scoreId: string; data: CreateBenchmarkScoreDto }) =>
      updateBenchmarkScore(modelId, scoreId, data),
    onSuccess: () => {
      // Invalidate benchmark scores query for this model
      queryClient.invalidateQueries({ queryKey: benchmarkScoreKeys.byModel(modelId) })
      // Invalidate model detail query
      queryClient.invalidateQueries({ queryKey: ['adminModels', 'detail', modelId] })
    },
  })
}

/**
 * Deletes a benchmark score from a model (hard delete)
 * Automatically invalidates model's score cache on success
 *
 * @returns Mutation object with mutate function, loading state, and error handling
 */
export const useDeleteBenchmarkScore = (modelId: string) => {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (scoreId: string) => deleteBenchmarkScore(modelId, scoreId),
    onSuccess: () => {
      // Invalidate benchmark scores query for this model
      queryClient.invalidateQueries({ queryKey: benchmarkScoreKeys.byModel(modelId) })
      // Invalidate model detail query
      queryClient.invalidateQueries({ queryKey: ['adminModels', 'detail', modelId] })
    },
  })
}

/**
 * Imports multiple benchmark scores via CSV file upload (Story 2.11)
 * Processes CSV, validates each row, imports valid scores, returns detailed results
 * Automatically invalidates all benchmark score caches on success
 *
 * Features:
 * - Partial success handling (valid rows imported even if some fail)
 * - Row-by-row validation with error reporting
 * - Duplicate detection (skip or update based on import options)
 * - Cache invalidation for model details + QAPS + best value
 *
 * @returns Mutation object with mutate function, loading state, and error handling
 */
export const useImportBenchmarkCSV = () => {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (formData: FormData) => importBenchmarkCSV(formData),
    onSuccess: () => {
      // Invalidate all benchmark score queries (import affects multiple models)
      queryClient.invalidateQueries({ queryKey: benchmarkScoreKeys.all })
      // Invalidate all model detail queries
      queryClient.invalidateQueries({ queryKey: ['adminModels', 'detail'] })
      // Invalidate QAPS and best value caches (normalized scores changed)
      queryClient.invalidateQueries({ queryKey: ['qaps'] })
      queryClient.invalidateQueries({ queryKey: ['bestValue'] })
    },
  })
}
