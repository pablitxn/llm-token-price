/**
 * BenchmarkScoreForm Component
 * Form for adding and editing benchmark scores to models in the admin panel
 * Uses React Hook Form for performance and Zod for validation
 * Story 2.10 - Benchmark Score Entry Form
 */

import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { useState, useEffect } from 'react'
import {
  createBenchmarkScoreSchema,
  type CreateBenchmarkScoreFormData,
} from '@/schemas/benchmarkScoreSchema'
import { useAddBenchmarkScore, useUpdateBenchmarkScore } from '@/hooks/useBenchmarkScores'
import { useBenchmarks } from '@/hooks/useBenchmarks'
import type { BenchmarkScoreResponseDto } from '@/types/admin'

interface BenchmarkScoreFormProps {
  /** Model ID this score belongs to */
  modelId: string
  /** Form mode: 'create' for new scores, 'edit' for existing scores */
  mode: 'create' | 'edit'
  /** Benchmark score data for edit mode */
  score?: BenchmarkScoreResponseDto
  /** Callback when form is successfully submitted */
  onSuccess?: () => void
  /** Callback when form is cancelled */
  onCancel?: () => void
}

/**
 * Benchmark score creation/edit form with validation
 * Implements double-layer validation: Zod (client) + FluentValidation (server)
 * Displays warning when score is outside benchmark's typical range
 */
export function BenchmarkScoreForm({
  modelId,
  mode,
  score,
  onSuccess,
  onCancel,
}: BenchmarkScoreFormProps) {
  const [showOutOfRangeWarning, setShowOutOfRangeWarning] = useState(false)

  const { data: benchmarksData, isLoading: benchmarksLoading } = useBenchmarks(true)
  const benchmarks = benchmarksData || []

  const {
    mutate: addScore,
    isPending: isAdding,
    error: addError,
  } = useAddBenchmarkScore(modelId)
  const {
    mutate: updateScore,
    isPending: isUpdating,
    error: updateError,
  } = useUpdateBenchmarkScore(modelId)

  const isPending = mode === 'create' ? isAdding : isUpdating
  const error = mode === 'create' ? addError : updateError

  const {
    register,
    handleSubmit,
    formState: { errors, isDirty },
    watch,
    reset,
  } = useForm<CreateBenchmarkScoreFormData>({
    resolver: zodResolver(createBenchmarkScoreSchema),
    defaultValues: score
      ? {
          benchmarkId: score.benchmarkId,
          score: score.score,
          maxScore: score.maxScore,
          testDate: score.testDate || undefined,
          sourceUrl: score.sourceUrl || undefined,
          verified: score.verified,
          notes: score.notes || undefined,
        }
      : {
          benchmarkId: '',
          score: 0,
          maxScore: undefined,
          testDate: undefined,
          sourceUrl: undefined,
          verified: false,
          notes: undefined,
        },
  })

  // Watch benchmarkId and score to check if out of range
  const selectedBenchmarkId = watch('benchmarkId')
  const currentScore = watch('score')

  useEffect(() => {
    if (selectedBenchmarkId && currentScore !== undefined) {
      const selectedBenchmark = benchmarks.find((b) => b.id === selectedBenchmarkId)
      if (selectedBenchmark) {
        const isOutOfRange =
          currentScore < selectedBenchmark.typicalRangeMin ||
          currentScore > selectedBenchmark.typicalRangeMax
        setShowOutOfRangeWarning(isOutOfRange)
      }
    }
  }, [selectedBenchmarkId, currentScore, benchmarks])

  const onSubmit = (data: CreateBenchmarkScoreFormData) => {
    // Convert empty strings to null
    const payload = {
      benchmarkId: data.benchmarkId,
      score: data.score,
      maxScore: data.maxScore || null,
      testDate: data.testDate || null,
      sourceUrl: data.sourceUrl || null,
      verified: data.verified || false,
      notes: data.notes || null,
    }

    if (mode === 'create') {
      addScore(payload, {
        onSuccess: () => {
          reset()
          onSuccess?.()
        },
      })
    } else if (score) {
      updateScore(
        { scoreId: score.id, data: payload },
        {
          onSuccess: () => {
            onSuccess?.()
          },
        }
      )
    }
  }

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
      {/* Benchmark Selection */}
      <div>
        <label htmlFor="benchmarkId" className="block text-sm font-medium text-gray-700 mb-2">
          Benchmark <span className="text-red-500">*</span>
        </label>
        <select
          {...register('benchmarkId')}
          id="benchmarkId"
          disabled={benchmarksLoading || mode === 'edit'}
          className="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500 disabled:bg-gray-100 disabled:cursor-not-allowed"
        >
          <option value="">Select a benchmark...</option>
          {benchmarks.map((benchmark) => (
            <option key={benchmark.id} value={benchmark.id}>
              {benchmark.benchmarkName} - {benchmark.fullName} ({benchmark.category})
            </option>
          ))}
        </select>
        {errors.benchmarkId && (
          <p className="mt-1 text-sm text-red-600">{errors.benchmarkId.message}</p>
        )}
      </div>

      {/* Score Input */}
      <div>
        <label htmlFor="score" className="block text-sm font-medium text-gray-700 mb-2">
          Score <span className="text-red-500">*</span>
        </label>
        <input
          {...register('score', { valueAsNumber: true })}
          type="number"
          step="0.01"
          id="score"
          className="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
          placeholder="e.g., 87.5"
        />
        {errors.score && <p className="mt-1 text-sm text-red-600">{errors.score.message}</p>}

        {/* Out of Range Warning */}
        {showOutOfRangeWarning && (
          <div className="mt-2 p-3 bg-yellow-50 border border-yellow-200 rounded-md">
            <div className="flex items-start">
              <svg
                className="h-5 w-5 text-yellow-400 mr-2 flex-shrink-0"
                fill="currentColor"
                viewBox="0 0 20 20"
              >
                <path
                  fillRule="evenodd"
                  d="M8.257 3.099c.765-1.36 2.722-1.36 3.486 0l5.58 9.92c.75 1.334-.213 2.98-1.742 2.98H4.42c-1.53 0-2.493-1.646-1.743-2.98l5.58-9.92zM11 13a1 1 0 11-2 0 1 1 0 012 0zm-1-8a1 1 0 00-1 1v3a1 1 0 002 0V6a1 1 0 00-1-1z"
                  clipRule="evenodd"
                />
              </svg>
              <p className="text-sm text-yellow-800">
                <strong>Warning:</strong> Score is outside the typical range for this benchmark.
                You can still submit, but please verify the score is correct.
              </p>
            </div>
          </div>
        )}
      </div>

      {/* Max Score (Optional) */}
      <div>
        <label htmlFor="maxScore" className="block text-sm font-medium text-gray-700 mb-2">
          Max Score (optional)
        </label>
        <input
          {...register('maxScore', { valueAsNumber: true })}
          type="number"
          step="0.01"
          id="maxScore"
          className="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
          placeholder="e.g., 100"
        />
        {errors.maxScore && <p className="mt-1 text-sm text-red-600">{errors.maxScore.message}</p>}
        <p className="mt-1 text-sm text-gray-500">
          Maximum possible score (e.g., 100 for percentage-based benchmarks)
        </p>
      </div>

      {/* Test Date (Optional) */}
      <div>
        <label htmlFor="testDate" className="block text-sm font-medium text-gray-700 mb-2">
          Test Date (optional)
        </label>
        <input
          {...register('testDate')}
          type="datetime-local"
          id="testDate"
          className="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
        />
        {errors.testDate && <p className="mt-1 text-sm text-red-600">{errors.testDate.message}</p>}
      </div>

      {/* Source URL (Optional) */}
      <div>
        <label htmlFor="sourceUrl" className="block text-sm font-medium text-gray-700 mb-2">
          Source URL (optional)
        </label>
        <input
          {...register('sourceUrl')}
          type="url"
          id="sourceUrl"
          className="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
          placeholder="https://..."
        />
        {errors.sourceUrl && (
          <p className="mt-1 text-sm text-red-600">{errors.sourceUrl.message}</p>
        )}
        <p className="mt-1 text-sm text-gray-500">URL to the benchmark result source</p>
      </div>

      {/* Verified Checkbox */}
      <div className="flex items-start">
        <input
          {...register('verified')}
          type="checkbox"
          id="verified"
          className="h-4 w-4 mt-1 text-blue-600 border-gray-300 rounded focus:ring-blue-500"
        />
        <label htmlFor="verified" className="ml-2 block text-sm text-gray-700">
          Verified Score
          <p className="text-xs text-gray-500 mt-1">
            Check if this is an official/verified score from the model creator or paper
          </p>
        </label>
      </div>

      {/* Notes (Optional) */}
      <div>
        <label htmlFor="notes" className="block text-sm font-medium text-gray-700 mb-2">
          Notes (optional)
        </label>
        <textarea
          {...register('notes')}
          id="notes"
          rows={3}
          maxLength={500}
          className="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
          placeholder="Additional context about this score..."
        />
        {errors.notes && <p className="mt-1 text-sm text-red-600">{errors.notes.message}</p>}
        <p className="mt-1 text-sm text-gray-500">Max 500 characters</p>
      </div>

      {/* Error Display */}
      {error && (
        <div className="p-4 bg-red-50 border border-red-200 rounded-md">
          <p className="text-sm text-red-800">
            <strong>Error:</strong> {error.message || 'Failed to save benchmark score'}
          </p>
        </div>
      )}

      {/* Form Actions */}
      <div className="flex items-center justify-end gap-3 pt-4 border-t border-gray-200">
        {onCancel && (
          <button
            type="button"
            onClick={onCancel}
            className="px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-md hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2"
            disabled={isPending}
          >
            Cancel
          </button>
        )}
        <button
          type="submit"
          disabled={isPending || !isDirty}
          className="px-4 py-2 text-sm font-medium text-white bg-blue-600 border border-transparent rounded-md hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2 disabled:opacity-50 disabled:cursor-not-allowed"
        >
          {isPending ? (mode === 'create' ? 'Adding...' : 'Updating...') : mode === 'create' ? 'Add Score' : 'Update Score'}
        </button>
      </div>
    </form>
  )
}
