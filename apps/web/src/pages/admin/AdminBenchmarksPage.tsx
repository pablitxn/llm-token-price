/**
 * AdminBenchmarksPage - Main page for viewing and managing benchmarks in admin panel
 * Displays all benchmark definitions (including inactive) in a table with search and action buttons
 *
 * Story 2.9: Create Benchmark Definitions Management
 */

import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { ConfirmDialog } from '@/components/ui/ConfirmDialog'
import { SkeletonLoader } from '@/components/ui/SkeletonLoader'
import { ErrorAlert } from '@/components/ui/ErrorAlert'
import { useBenchmarks, useDeleteBenchmark } from '@/hooks/useBenchmarks'
import type { BenchmarkResponseDto } from '@/schemas/benchmarkSchema'
import { mapErrorToUserMessage } from '@/utils/errorMessages'

/**
 * AdminBenchmarksPage component
 * Main admin panel page for viewing and managing all benchmark definitions
 */
export default function AdminBenchmarksPage() {
  const navigate = useNavigate()
  const [deleteModalOpen, setDeleteModalOpen] = useState(false)
  const [benchmarkToDelete, setBenchmarkToDelete] = useState<BenchmarkResponseDto | null>(null)
  const [categoryFilter, setCategoryFilter] = useState<string>('')

  // Fetch benchmarks using TanStack Query hook (includes inactive)
  const { data: benchmarks, isLoading, error, refetch } = useBenchmarks(true, categoryFilter || undefined)

  // Delete mutation hook
  const deleteMutation = useDeleteBenchmark()

  /**
   * Handles "Add New Benchmark" button click
   */
  const handleAddNewBenchmark = () => {
    navigate('/admin/benchmarks/new')
  }

  /**
   * Handles edit button click
   */
  const handleEditClick = (benchmark: BenchmarkResponseDto) => {
    navigate(`/admin/benchmarks/${benchmark.id}/edit`)
  }

  /**
   * Handles delete button click
   * Opens confirmation dialog
   */
  const handleDeleteClick = (benchmark: BenchmarkResponseDto) => {
    setBenchmarkToDelete(benchmark)
    setDeleteModalOpen(true)
  }

  /**
   * Handles delete confirmation
   * Performs soft delete (sets isActive = false) and refetches the benchmark list
   */
  const handleConfirmDelete = async () => {
    if (!benchmarkToDelete) return

    try {
      // Execute delete mutation
      await deleteMutation.mutateAsync(benchmarkToDelete.id)

      // Close modal and clear state on success
      setDeleteModalOpen(false)
      setBenchmarkToDelete(null)

      // Note: Query invalidation happens automatically in the mutation's onSuccess
    } catch (error) {
      // Error is already logged in the mutation's onError
      // Keep modal open so user can try again or cancel
      console.error('Failed to delete benchmark:', error)
    }
  }

  /**
   * Handles delete cancellation
   */
  const handleCancelDelete = () => {
    setDeleteModalOpen(false)
    setBenchmarkToDelete(null)
  }

  /**
   * Handle category filter change
   */
  const handleCategoryFilterChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
    setCategoryFilter(e.target.value)
  }

  // Compute benchmarks count for display
  const benchmarksCount = benchmarks?.length ?? 0
  const activeBenchmarksCount = benchmarks?.filter(b => b.isActive).length ?? 0

  return (
    <div className="px-4 sm:px-6 lg:px-8">
      {/* Page Header */}
      <div className="sm:flex sm:items-center sm:justify-between">
        <div>
          <h1 className="text-2xl font-semibold text-gray-900">Benchmarks</h1>
          <p className="mt-2 text-sm text-gray-700">
            View and manage all benchmark definitions for model scoring.
          </p>
        </div>
        <div className="mt-4 sm:mt-0">
          <button
            type="button"
            onClick={handleAddNewBenchmark}
            className="inline-flex items-center justify-center rounded-md border border-transparent bg-blue-600 px-4 py-2 text-sm font-medium text-white shadow-sm hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2"
          >
            <svg
              className="-ml-1 mr-2 h-5 w-5"
              xmlns="http://www.w3.org/2000/svg"
              viewBox="0 0 20 20"
              fill="currentColor"
              aria-hidden="true"
            >
              <path
                fillRule="evenodd"
                d="M10 3a1 1 0 011 1v5h5a1 1 0 110 2h-5v5a1 1 0 11-2 0v-5H4a1 1 0 110-2h5V4a1 1 0 011-1z"
                clipRule="evenodd"
              />
            </svg>
            Add New Benchmark
          </button>
        </div>
      </div>

      {/* Filter Bar */}
      <div className="mt-6 flex items-center gap-4">
        <div className="flex-1 max-w-xs">
          <label htmlFor="category-filter" className="block text-sm font-medium text-gray-700 mb-1">
            Filter by Category
          </label>
          <select
            id="category-filter"
            value={categoryFilter}
            onChange={handleCategoryFilterChange}
            className="block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500 sm:text-sm"
          >
            <option value="">All Categories</option>
            <option value="Reasoning">Reasoning</option>
            <option value="Code">Code</option>
            <option value="Math">Math</option>
            <option value="Language">Language</option>
            <option value="Multimodal">Multimodal</option>
          </select>
        </div>
      </div>

      {/* Benchmarks Count */}
      <div className="mt-4">
        <p className="text-sm text-gray-700">
          Showing <span className="font-medium">{benchmarksCount}</span> benchmark{benchmarksCount !== 1 ? 's' : ''}
          {' '}(<span className="font-medium">{activeBenchmarksCount}</span> active)
        </p>
      </div>

      {/* Loading State - Story 2.13 Task 9: Skeleton loader for better UX */}
      {isLoading && (
        <div className="mt-8">
          <SkeletonLoader rows={10} columns={8} />
        </div>
      )}

      {/* Error State - Story 2.13 Task 10: User-friendly error messages */}
      {error && (
        <div className="mt-8">
          <ErrorAlert
            error={mapErrorToUserMessage(error)}
            onRetry={() => refetch()}
            onReport={() => window.open('mailto:support@example.com?subject=Error%20Loading%20Benchmarks')}
          />
        </div>
      )}

      {/* Benchmarks Table */}
      {!isLoading && !error && benchmarks && (
        <div className="mt-8 flow-root">
          <div className="-mx-4 -my-2 overflow-x-auto sm:-mx-6 lg:-mx-8">
            <div className="inline-block min-w-full py-2 align-middle sm:px-6 lg:px-8">
              <div className="overflow-hidden shadow ring-1 ring-black ring-opacity-5 sm:rounded-lg">
                <table className="min-w-full divide-y divide-gray-300">
                  <thead className="bg-gray-50">
                    <tr>
                      <th scope="col" className="py-3.5 pl-4 pr-3 text-left text-sm font-semibold text-gray-900 sm:pl-6">
                        Name
                      </th>
                      <th scope="col" className="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">
                        Full Name
                      </th>
                      <th scope="col" className="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">
                        Category
                      </th>
                      <th scope="col" className="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">
                        Interpretation
                      </th>
                      <th scope="col" className="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">
                        Typical Range
                      </th>
                      <th scope="col" className="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">
                        QAPS Weight
                      </th>
                      <th scope="col" className="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">
                        Status
                      </th>
                      <th scope="col" className="relative py-3.5 pl-3 pr-4 sm:pr-6">
                        <span className="sr-only">Actions</span>
                      </th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-gray-200 bg-white">
                    {benchmarks.length === 0 ? (
                      <tr>
                        <td colSpan={8} className="px-3 py-8 text-center text-sm text-gray-500">
                          No benchmarks found. Create your first benchmark definition.
                        </td>
                      </tr>
                    ) : (
                      benchmarks.map((benchmark) => (
                        <tr key={benchmark.id} className={!benchmark.isActive ? 'bg-gray-50 opacity-60' : ''}>
                          <td className="whitespace-nowrap py-4 pl-4 pr-3 text-sm font-medium text-gray-900 sm:pl-6">
                            {benchmark.benchmarkName}
                          </td>
                          <td className="whitespace-nowrap px-3 py-4 text-sm text-gray-700">
                            {benchmark.fullName}
                          </td>
                          <td className="whitespace-nowrap px-3 py-4 text-sm text-gray-700">
                            <span className="inline-flex rounded-full bg-blue-100 px-2 py-1 text-xs font-semibold text-blue-800">
                              {benchmark.category}
                            </span>
                          </td>
                          <td className="whitespace-nowrap px-3 py-4 text-sm text-gray-700">
                            {benchmark.interpretation === 'HigherBetter' ? 'Higher is Better' : 'Lower is Better'}
                          </td>
                          <td className="whitespace-nowrap px-3 py-4 text-sm text-gray-700">
                            {benchmark.typicalRangeMin} - {benchmark.typicalRangeMax}
                          </td>
                          <td className="whitespace-nowrap px-3 py-4 text-sm text-gray-700">
                            {(benchmark.weightInQaps * 100).toFixed(0)}%
                          </td>
                          <td className="whitespace-nowrap px-3 py-4 text-sm">
                            {benchmark.isActive ? (
                              <span className="inline-flex rounded-full bg-green-100 px-2 py-1 text-xs font-semibold text-green-800">
                                Active
                              </span>
                            ) : (
                              <span className="inline-flex rounded-full bg-gray-100 px-2 py-1 text-xs font-semibold text-gray-800">
                                Inactive
                              </span>
                            )}
                          </td>
                          <td className="relative whitespace-nowrap py-4 pl-3 pr-4 text-right text-sm font-medium sm:pr-6">
                            <button
                              onClick={() => handleEditClick(benchmark)}
                              className="text-blue-600 hover:text-blue-900 mr-4"
                            >
                              Edit
                            </button>
                            <button
                              onClick={() => handleDeleteClick(benchmark)}
                              className="text-red-600 hover:text-red-900"
                              disabled={!benchmark.isActive}
                            >
                              Delete
                            </button>
                          </td>
                        </tr>
                      ))
                    )}
                  </tbody>
                </table>
              </div>
            </div>
          </div>
        </div>
      )}

      {/* Delete Confirmation Dialog - Story 2.13 Task 11.5: Two-step confirmation */}
      <ConfirmDialog
        open={deleteModalOpen}
        onClose={handleCancelDelete}
        onConfirm={handleConfirmDelete}
        title="Delete Benchmark"
        message="This action cannot be undone. All benchmark scores using this definition will be affected."
        confirmText="Yes, Delete"
        loading={deleteMutation.isPending}
        requireTypedConfirmation={true}
        confirmationKeyword="DELETE"
        itemName={benchmarkToDelete?.benchmarkName}
      />
    </div>
  )
}
