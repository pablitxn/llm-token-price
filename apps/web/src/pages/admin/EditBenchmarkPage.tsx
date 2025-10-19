/**
 * EditBenchmarkPage Component
 * Page for editing an existing benchmark definition in the admin panel
 * Story 2.9: Create Benchmark Definitions Management
 */

import { useParams, useNavigate } from 'react-router-dom'
import { BenchmarkForm } from '@/components/admin/BenchmarkForm'
import { useBenchmark } from '@/hooks/useBenchmarks'

export function EditBenchmarkPage() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()

  const { data: benchmark, isLoading, error } = useBenchmark(id!)

  if (isLoading) {
    return (
      <div className="px-4 sm:px-6 lg:px-8">
        <div className="flex justify-center items-center min-h-[400px]">
          <div className="inline-block h-8 w-8 animate-spin rounded-full border-4 border-solid border-blue-600 border-r-transparent align-[-0.125em]" role="status">
            <span className="!absolute !-m-px !h-px !w-px !overflow-hidden !whitespace-nowrap !border-0 !p-0 ![clip:rect(0,0,0,0)]">
              Loading...
            </span>
          </div>
        </div>
      </div>
    )
  }

  if (error || !benchmark) {
    return (
      <div className="px-4 sm:px-6 lg:px-8">
        <div className="rounded-md bg-red-50 p-4 mt-8">
          <div className="flex">
            <div className="flex-shrink-0">
              <svg
                className="h-5 w-5 text-red-400"
                xmlns="http://www.w3.org/2000/svg"
                viewBox="0 0 20 20"
                fill="currentColor"
              >
                <path
                  fillRule="evenodd"
                  d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z"
                  clipRule="evenodd"
                />
              </svg>
            </div>
            <div className="ml-3">
              <h3 className="text-sm font-medium text-red-800">Error loading benchmark</h3>
              <div className="mt-2 text-sm text-red-700">
                <p>{error?.message || 'Benchmark not found'}</p>
              </div>
              <div className="mt-4">
                <button
                  type="button"
                  onClick={() => navigate('/admin/benchmarks')}
                  className="rounded-md bg-red-50 px-2 py-1.5 text-sm font-medium text-red-800 hover:bg-red-100"
                >
                  Back to Benchmarks
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>
    )
  }

  return (
    <div className="px-4 sm:px-6 lg:px-8">
      {/* Page Header */}
      <div className="mb-8">
        <h1 className="text-2xl font-semibold text-gray-900">Edit Benchmark</h1>
        <p className="mt-2 text-sm text-gray-700">
          Update the benchmark definition "{benchmark.fullName}".
        </p>
      </div>

      {/* Form */}
      <BenchmarkForm mode="edit" benchmarkId={id} benchmark={benchmark} />
    </div>
  )
}
