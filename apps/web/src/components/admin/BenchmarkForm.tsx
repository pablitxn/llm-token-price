/**
 * BenchmarkForm Component
 * Form for creating and editing benchmark definitions in the admin panel
 * Uses React Hook Form for performance and Zod for validation
 */

import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { useNavigate } from 'react-router-dom'
import { useEffect } from 'react'
import {
  createBenchmarkSchema,
  updateBenchmarkSchema,
  benchmarkCategories,
  benchmarkInterpretations,
  type CreateBenchmarkFormData,
  type UpdateBenchmarkFormData,
  type BenchmarkResponseDto,
} from '@/schemas/benchmarkSchema'
import { useCreateBenchmark, useUpdateBenchmark } from '@/hooks/useBenchmarks'

interface BenchmarkFormProps {
  /** Form mode: 'create' for new benchmarks, 'edit' for existing benchmarks */
  mode: 'create' | 'edit'
  /** Benchmark ID (required for edit mode) */
  benchmarkId?: string
  /** Benchmark data for edit mode (null for create mode) */
  benchmark?: BenchmarkResponseDto | null
}

/**
 * Benchmark creation/edit form with validation
 * Implements double-layer validation: Zod (client) + FluentValidation (server, Story 2.9)
 *
 * @param props - Component props
 * @param props.mode - Form mode ('create' | 'edit')
 * @param props.benchmarkId - Benchmark ID for edit mode
 * @param props.benchmark - Benchmark data for edit mode
 */
export function BenchmarkForm({ mode, benchmarkId, benchmark = null }: BenchmarkFormProps) {
  const navigate = useNavigate()
  const { mutate: createBenchmark, isPending: isCreating, error: createError } = useCreateBenchmark()
  const { mutate: updateBenchmark, isPending: isUpdating, error: updateError } = useUpdateBenchmark()

  const isPending = mode === 'create' ? isCreating : isUpdating
  const error = mode === 'create' ? createError : updateError

  type FormData = typeof mode extends 'create' ? CreateBenchmarkFormData : UpdateBenchmarkFormData

  const {
    register,
    handleSubmit,
    formState: { errors, isDirty },
    reset,
  } = useForm<FormData>({
    resolver: zodResolver(mode === 'create' ? createBenchmarkSchema : updateBenchmarkSchema) as any,
    defaultValues: benchmark
      ? {
          // Pre-populate from benchmark data in edit mode
          ...(mode === 'create' && { benchmarkName: benchmark.benchmarkName }),
          fullName: benchmark.fullName,
          description: benchmark.description || '',
          category: benchmark.category,
          interpretation: benchmark.interpretation,
          typicalRangeMin: benchmark.typicalRangeMin,
          typicalRangeMax: benchmark.typicalRangeMax,
          weightInQaps: benchmark.weightInQaps,
        }
      : {
          // Default values for create mode
          benchmarkName: '',
          fullName: '',
          description: '',
          category: 'Reasoning' as const,
          interpretation: 'HigherBetter' as const,
          typicalRangeMin: 0,
          typicalRangeMax: 100,
          weightInQaps: 0,
        },
  })

  // Warn user about unsaved changes when navigating away
  useEffect(() => {
    const handleBeforeUnload = (e: BeforeUnloadEvent) => {
      if (isDirty) {
        e.preventDefault()
        e.returnValue = ''
      }
    }

    window.addEventListener('beforeunload', handleBeforeUnload)
    return () => window.removeEventListener('beforeunload', handleBeforeUnload)
  }, [isDirty])

  const onSubmit = (data: any) => {
    // Convert empty description to undefined
    const payload = {
      ...data,
      description: data.description || undefined,
    }

    if (mode === 'create') {
      createBenchmark(payload as CreateBenchmarkFormData, {
        onSuccess: () => {
          navigate('/admin/benchmarks')
        },
      })
    } else if (mode === 'edit' && benchmarkId) {
      updateBenchmark(
        { id: benchmarkId, data: payload as UpdateBenchmarkFormData },
        {
          onSuccess: () => {
            navigate('/admin/benchmarks')
          },
        }
      )
    }
  }

  const handleCancel = () => {
    if (isDirty) {
      const confirmed = window.confirm(
        'You have unsaved changes. Are you sure you want to leave?'
      )
      if (!confirmed) return
    }
    navigate('/admin/benchmarks')
  }

  const handleReset = () => {
    reset()
  }

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-8">
      {/* Basic Info Section */}
      <div className="bg-white shadow rounded-lg p-6">
        <h2 className="text-lg font-semibold text-gray-900 mb-4">Basic Information</h2>
        <div className="grid grid-cols-1 gap-6 sm:grid-cols-2">
          {/* Benchmark Name - Only shown in create mode */}
          {mode === 'create' && (
            <div className="sm:col-span-2">
              <label htmlFor="benchmarkName" className="block text-sm font-medium text-gray-700">
                Benchmark Name <span className="text-red-600">*</span>
              </label>
              <p className="mt-1 text-xs text-gray-500">
                Short identifier (e.g., "MMLU"). Cannot be changed after creation.
              </p>
              <input
                type="text"
                id="benchmarkName"
                {...register('benchmarkName' as any)}
                className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 shadow-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                placeholder="e.g., MMLU"
              />
              {(errors as any).benchmarkName && (
                <p className="mt-1 text-sm text-red-600">{(errors as any).benchmarkName.message}</p>
              )}
            </div>
          )}

          {/* Edit Mode: Display benchmark name as read-only */}
          {mode === 'edit' && benchmark && (
            <div className="sm:col-span-2">
              <label htmlFor="benchmarkName" className="block text-sm font-medium text-gray-700">
                Benchmark Name
                <span className="ml-2 text-xs text-gray-500">(Cannot be changed)</span>
              </label>
              <input
                type="text"
                id="benchmarkName"
                value={benchmark.benchmarkName}
                disabled
                className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 shadow-sm bg-gray-100 cursor-not-allowed opacity-60"
              />
            </div>
          )}

          {/* Full Name */}
          <div className="sm:col-span-2">
            <label htmlFor="fullName" className="block text-sm font-medium text-gray-700">
              Full Name <span className="text-red-600">*</span>
            </label>
            <input
              type="text"
              id="fullName"
              {...register('fullName')}
              className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 shadow-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
              placeholder="e.g., Massive Multitask Language Understanding"
            />
            {errors.fullName && (
              <p className="mt-1 text-sm text-red-600">{errors.fullName.message}</p>
            )}
          </div>

          {/* Description */}
          <div className="sm:col-span-2">
            <label htmlFor="description" className="block text-sm font-medium text-gray-700">
              Description
            </label>
            <textarea
              id="description"
              rows={3}
              {...register('description')}
              className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 shadow-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
              placeholder="Explain what this benchmark measures..."
            />
            {errors.description && (
              <p className="mt-1 text-sm text-red-600">{errors.description.message}</p>
            )}
          </div>

          {/* Category */}
          <div>
            <label htmlFor="category" className="block text-sm font-medium text-gray-700">
              Category <span className="text-red-600">*</span>
            </label>
            <select
              id="category"
              {...register('category')}
              className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 shadow-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
            >
              {benchmarkCategories.map((cat) => (
                <option key={cat} value={cat}>
                  {cat}
                </option>
              ))}
            </select>
            {errors.category && (
              <p className="mt-1 text-sm text-red-600">{errors.category.message}</p>
            )}
          </div>

          {/* Interpretation */}
          <div>
            <label htmlFor="interpretation" className="block text-sm font-medium text-gray-700">
              Interpretation <span className="text-red-600">*</span>
            </label>
            <select
              id="interpretation"
              {...register('interpretation')}
              className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 shadow-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
            >
              {benchmarkInterpretations.map((interp) => (
                <option key={interp} value={interp}>
                  {interp === 'HigherBetter' ? 'Higher is Better' : 'Lower is Better'}
                </option>
              ))}
            </select>
            {errors.interpretation && (
              <p className="mt-1 text-sm text-red-600">{errors.interpretation.message}</p>
            )}
          </div>
        </div>
      </div>

      {/* Range and Weight Section */}
      <div className="bg-white shadow rounded-lg p-6">
        <h2 className="text-lg font-semibold text-gray-900 mb-4">Typical Range & QAPS Weight</h2>
        <p className="text-sm text-gray-500 mb-4">
          Define the expected score range for this benchmark and its weight in the QAPS (Quality-Adjusted Price per Score) calculation.
        </p>
        <div className="grid grid-cols-1 gap-6 sm:grid-cols-3">
          {/* Typical Range Min */}
          <div>
            <label
              htmlFor="typicalRangeMin"
              className="block text-sm font-medium text-gray-700"
            >
              Typical Range Min <span className="text-red-600">*</span>
            </label>
            <input
              type="number"
              id="typicalRangeMin"
              step="0.01"
              {...register('typicalRangeMin', { valueAsNumber: true })}
              className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 shadow-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
              placeholder="0"
            />
            {errors.typicalRangeMin && (
              <p className="mt-1 text-sm text-red-600">{errors.typicalRangeMin.message}</p>
            )}
          </div>

          {/* Typical Range Max */}
          <div>
            <label
              htmlFor="typicalRangeMax"
              className="block text-sm font-medium text-gray-700"
            >
              Typical Range Max <span className="text-red-600">*</span>
            </label>
            <input
              type="number"
              id="typicalRangeMax"
              step="0.01"
              {...register('typicalRangeMax', { valueAsNumber: true })}
              className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 shadow-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
              placeholder="100"
            />
            {errors.typicalRangeMax && (
              <p className="mt-1 text-sm text-red-600">{errors.typicalRangeMax.message}</p>
            )}
          </div>

          {/* Weight in QAPS */}
          <div>
            <label htmlFor="weightInQaps" className="block text-sm font-medium text-gray-700">
              Weight in QAPS <span className="text-red-600">*</span>
            </label>
            <p className="mt-1 text-xs text-gray-500">0.00 to 1.00 (e.g., 0.30 for 30%)</p>
            <input
              type="number"
              id="weightInQaps"
              step="0.01"
              min="0"
              max="1"
              {...register('weightInQaps', { valueAsNumber: true })}
              className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 shadow-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
              placeholder="0.30"
            />
            {errors.weightInQaps && (
              <p className="mt-1 text-sm text-red-600">{errors.weightInQaps.message}</p>
            )}
          </div>
        </div>
      </div>

      {/* Server Error Display */}
      {error && (
        <div className="bg-red-50 border border-red-200 rounded-lg p-4">
          <p className="text-sm text-red-800">
            {error.message || 'An error occurred while saving the benchmark. Please try again.'}
          </p>
        </div>
      )}

      {/* Form Actions */}
      <div className="flex items-center justify-end gap-4">
        <button
          type="button"
          onClick={handleReset}
          className="px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-md shadow-sm hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500"
        >
          Reset Form
        </button>
        <button
          type="button"
          onClick={handleCancel}
          className="px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-md shadow-sm hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500"
        >
          Cancel
        </button>
        <button
          type="submit"
          disabled={isPending}
          className="px-4 py-2 text-sm font-medium text-white bg-blue-600 border border-transparent rounded-md shadow-sm hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 disabled:opacity-50 disabled:cursor-not-allowed"
        >
          {isPending ? (
            <span className="flex items-center">
              <svg
                className="animate-spin -ml-1 mr-2 h-4 w-4 text-white"
                xmlns="http://www.w3.org/2000/svg"
                fill="none"
                viewBox="0 0 24 24"
              >
                <circle
                  className="opacity-25"
                  cx="12"
                  cy="12"
                  r="10"
                  stroke="currentColor"
                  strokeWidth="4"
                />
                <path
                  className="opacity-75"
                  fill="currentColor"
                  d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"
                />
              </svg>
              Saving...
            </span>
          ) : mode === 'create' ? (
            'Create Benchmark'
          ) : (
            'Save Changes'
          )}
        </button>
      </div>
    </form>
  )
}
