/**
 * AdminModelsPage - Main page for viewing and managing models in admin panel
 * Displays all models (including inactive) in a sortable table with search and action buttons
 *
 * Story 2.3: Build Models List View in Admin Panel
 */

import { useState, useEffect } from 'react'
import { useNavigate, useSearchParams } from 'react-router-dom'
import { ModelList } from '@/components/admin/ModelList'
import { ConfirmDialog } from '@/components/ui/ConfirmDialog'
import { LoadingSpinner } from '@/components/ui/LoadingSpinner'
import { SkeletonLoader } from '@/components/ui/SkeletonLoader'
import { ErrorAlert } from '@/components/ui/ErrorAlert'
import { useAdminModels, useDeleteModel } from '@/hooks/useAdminModels'
import type { AdminModelDto } from '@/types/admin'
import { getFreshnessStatus } from '@/utils/formatters'
import type { FreshnessStatus } from '@/types/timestamp'
import { mapErrorToUserMessage } from '@/utils/errorMessages'
import * as React from 'react'

/**
 * Debounce hook - delays execution of a function until after a delay
 * Prevents excessive API calls while user is typing in search input
 * @param value - Value to debounce
 * @param delay - Delay in milliseconds (default: 300ms)
 * @returns Debounced value
 */
function useDebounce<T>(value: T, delay: number = 300): T {
  const [debouncedValue, setDebouncedValue] = useState<T>(value)

  React.useEffect(() => {
    const timer = setTimeout(() => {
      setDebouncedValue(value)
    }, delay)

    return () => {
      clearTimeout(timer)
    }
  }, [value, delay])

  return debouncedValue
}

/**
 * AdminModelsPage component
 * Main admin panel page for viewing and managing all models
 */
export default function AdminModelsPage() {
  const navigate = useNavigate()
  const [searchParams, setSearchParams] = useSearchParams()
  const [deleteModalOpen, setDeleteModalOpen] = useState(false)
  const [modelToDelete, setModelToDelete] = useState<AdminModelDto | null>(null)

  // Initialize search term from URL params or empty string
  const [searchTerm, setSearchTerm] = useState(searchParams.get('search') || '')

  // Initialize freshness filter from URL params or 'all'
  const [freshnessFilter, setFreshnessFilter] = useState<'all' | FreshnessStatus>(
    (searchParams.get('freshness') as 'all' | FreshnessStatus) || 'all'
  )

  // Pagination state (Story 2.13 Task 5.6)
  const [page, setPage] = useState(parseInt(searchParams.get('page') || '1', 10))
  const [pageSize, setPageSize] = useState(parseInt(searchParams.get('pageSize') || '20', 10))

  // Debounce search term to avoid excessive re-renders and API calls
  const debouncedSearchTerm = useDebounce(searchTerm, 300)

  // Sync search term, freshness filter, and pagination with URL params when they change
  useEffect(() => {
    const params: Record<string, string> = {}
    if (searchTerm) {
      params.search = searchTerm
    }
    if (freshnessFilter !== 'all') {
      params.freshness = freshnessFilter
    }
    params.page = page.toString()
    params.pageSize = pageSize.toString()
    setSearchParams(params)
  }, [searchTerm, freshnessFilter, page, pageSize, setSearchParams])

  // Fetch models using TanStack Query hook with pagination (Story 2.13 Task 5.6)
  const { data: models, pagination, isLoading, error, refetch } = useAdminModels(
    debouncedSearchTerm,
    undefined, // provider filter
    undefined, // status filter
    page,
    pageSize
  )

  // Delete mutation hook
  const deleteMutation = useDeleteModel()

  /**
   * Filter models by freshness status
   */
  const filteredModels = models?.filter((model) => {
    if (freshnessFilter === 'all') return true
    const status = getFreshnessStatus(model.updatedAt)
    return status === freshnessFilter
  }) ?? []

  /**
   * Handles clear search button click
   */
  const handleClearSearch = () => {
    setSearchTerm('')
  }

  /**
   * Handles "Add New Model" button click
   */
  const handleAddNewModel = () => {
    navigate('/admin/models/new')
  }

  /**
   * Handles delete button click from ModelList
   * Opens confirmation dialog
   */
  const handleDeleteClick = (model: AdminModelDto) => {
    setModelToDelete(model)
    setDeleteModalOpen(true)
  }

  /**
   * Handles delete confirmation
   * Performs soft delete (sets isActive = false) and refetches the model list
   */
  const handleConfirmDelete = async () => {
    if (!modelToDelete) return

    try {
      // Execute delete mutation
      await deleteMutation.mutateAsync(modelToDelete.id)

      // Close modal and clear state on success
      setDeleteModalOpen(false)
      setModelToDelete(null)

      // Note: Query invalidation happens automatically in the mutation's onSuccess
    } catch (error) {
      // Error is already logged in the mutation's onError
      // Keep modal open so user can try again or cancel
      console.error('Failed to delete model:', error)
    }
  }

  /**
   * Handles delete cancellation
   */
  const handleCancelDelete = () => {
    setDeleteModalOpen(false)
    setModelToDelete(null)
  }

  // Compute models count for display (Story 2.13 Task 5.6: use pagination metadata)
  const modelsCount = pagination?.totalItems ?? filteredModels.length

  // Pagination handlers
  const handlePageChange = (newPage: number) => {
    setPage(newPage)
  }

  const handlePageSizeChange = (newPageSize: number) => {
    setPageSize(newPageSize)
    setPage(1) // Reset to first page when changing page size
  }

  return (
    <div className="px-4 sm:px-6 lg:px-8">
      {/* Page Header */}
      <div className="sm:flex sm:items-center sm:justify-between">
        <div>
          <h1 className="text-2xl font-semibold text-gray-900">Models</h1>
          <p className="mt-2 text-sm text-gray-700">
            View and manage all LLM models in the system (including inactive).
          </p>
        </div>
        <div className="mt-4 sm:mt-0">
          <button
            type="button"
            onClick={handleAddNewModel}
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
            Add New Model
          </button>
        </div>
      </div>

      {/* Search Bar */}
      <div className="mt-6">
        <div className="relative rounded-md shadow-sm">
          <div className="pointer-events-none absolute inset-y-0 left-0 flex items-center pl-3">
            <svg
              className="h-5 w-5 text-gray-400"
              xmlns="http://www.w3.org/2000/svg"
              viewBox="0 0 20 20"
              fill="currentColor"
              aria-hidden="true"
            >
              <path
                fillRule="evenodd"
                d="M8 4a4 4 0 100 8 4 4 0 000-8zM2 8a6 6 0 1110.89 3.476l4.817 4.817a1 1 0 01-1.414 1.414l-4.816-4.816A6 6 0 012 8z"
                clipRule="evenodd"
              />
            </svg>
          </div>
          <input
            type="text"
            name="search"
            id="search"
            className="block w-full rounded-md border-gray-300 pl-10 pr-12 focus:border-blue-500 focus:ring-blue-500 sm:text-sm"
            placeholder="Search by model name or provider..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
          />
          {searchTerm && (
            <div className="absolute inset-y-0 right-0 flex items-center pr-3">
              <button
                type="button"
                onClick={handleClearSearch}
                className="text-gray-400 hover:text-gray-600"
                aria-label="Clear search"
              >
                <svg
                  className="h-5 w-5"
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
              </button>
            </div>
          )}
        </div>
        {searchTerm && (
          <p className="mt-2 text-sm text-gray-500">
            Searching for: <span className="font-medium">{searchTerm}</span>
          </p>
        )}
      </div>

      {/* Freshness Filter */}
      <div className="mt-4">
        <label htmlFor="freshness-filter" className="block text-sm font-medium text-gray-700 mb-2">
          Filter by Data Freshness
          <span className="ml-2 text-xs text-gray-500 font-normal">(filters current page only)</span>
        </label>
        <div className="flex gap-2">
          <button
            type="button"
            onClick={() => setFreshnessFilter('all')}
            className={`px-4 py-2 text-sm font-medium rounded-md ${
              freshnessFilter === 'all'
                ? 'bg-blue-600 text-white'
                : 'bg-white text-gray-700 border border-gray-300 hover:bg-gray-50'
            }`}
          >
            All Models
          </button>
          <button
            type="button"
            onClick={() => setFreshnessFilter('fresh')}
            className={`px-4 py-2 text-sm font-medium rounded-md ${
              freshnessFilter === 'fresh'
                ? 'bg-green-600 text-white'
                : 'bg-white text-gray-700 border border-gray-300 hover:bg-gray-50'
            }`}
          >
            🟢 Fresh (&lt; 7 days)
          </button>
          <button
            type="button"
            onClick={() => setFreshnessFilter('stale')}
            className={`px-4 py-2 text-sm font-medium rounded-md ${
              freshnessFilter === 'stale'
                ? 'bg-yellow-600 text-white'
                : 'bg-white text-gray-700 border border-gray-300 hover:bg-gray-50'
            }`}
          >
            🟡 Stale (7-30 days)
          </button>
          <button
            type="button"
            onClick={() => setFreshnessFilter('critical')}
            className={`px-4 py-2 text-sm font-medium rounded-md ${
              freshnessFilter === 'critical'
                ? 'bg-red-600 text-white'
                : 'bg-white text-gray-700 border border-gray-300 hover:bg-gray-50'
            }`}
          >
            🔴 Critical (&gt; 30 days)
          </button>
        </div>
      </div>

      {/* Page Size Selector & Models Count (Story 2.13 Task 5.6) */}
      <div className="mt-4 flex items-center justify-between">
        <p className="text-sm text-gray-700">
          Showing <span className="font-medium">{filteredModels.length}</span> of{' '}
          <span className="font-medium">{modelsCount}</span> model{modelsCount !== 1 ? 's' : ''}
          {freshnessFilter !== 'all' && (
            <span className="ml-1 text-gray-500">
              (filtered by {freshnessFilter} on current page)
            </span>
          )}
          {pagination && (
            <span className="ml-2 text-gray-500">
              • Page {pagination.currentPage} of {pagination.totalPages}
            </span>
          )}
        </p>

        <div className="flex items-center gap-2">
          <label htmlFor="pageSize" className="text-sm text-gray-700">
            Items per page:
          </label>
          <select
            id="pageSize"
            value={pageSize}
            onChange={(e) => handlePageSizeChange(parseInt(e.target.value, 10))}
            className="rounded-md border-gray-300 py-1 pl-3 pr-10 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
          >
            <option value={10}>10</option>
            <option value={20}>20</option>
            <option value={50}>50</option>
            <option value={100}>100</option>
          </select>
        </div>
      </div>

      {/* Loading State - Story 2.13 Task 9: Skeleton loader for better UX */}
      {isLoading && (
        <div className="mt-8">
          <SkeletonLoader rows={pageSize} columns={6} />
        </div>
      )}

      {/* Error State - Story 2.13 Task 10: User-friendly error messages */}
      {error && (
        <div className="mt-8">
          <ErrorAlert
            error={mapErrorToUserMessage(error)}
            onRetry={() => refetch()}
            onReport={() => window.open('mailto:support@example.com?subject=Error%20Loading%20Models')}
          />
        </div>
      )}

      {/* Models Table */}
      {!isLoading && !error && models && (
        <div className="mt-8">
          <ModelList models={filteredModels} searchTerm={debouncedSearchTerm} onDeleteClick={handleDeleteClick} />
        </div>
      )}

      {/* Pagination Controls (Story 2.13 Task 5.6) */}
      {!isLoading && !error && pagination && pagination.totalPages > 1 && (
        <div className="mt-6 flex items-center justify-between border-t border-gray-200 bg-white px-4 py-3 sm:px-6">
          <div className="flex flex-1 justify-between sm:hidden">
            {/* Mobile pagination */}
            <button
              onClick={() => handlePageChange(page - 1)}
              disabled={!pagination.hasPreviousPage}
              className="relative inline-flex items-center rounded-md border border-gray-300 bg-white px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50 disabled:cursor-not-allowed disabled:opacity-50"
            >
              Previous
            </button>
            <button
              onClick={() => handlePageChange(page + 1)}
              disabled={!pagination.hasNextPage}
              className="relative ml-3 inline-flex items-center rounded-md border border-gray-300 bg-white px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50 disabled:cursor-not-allowed disabled:opacity-50"
            >
              Next
            </button>
          </div>
          <div className="hidden sm:flex sm:flex-1 sm:items-center sm:justify-between">
            <div>
              <p className="text-sm text-gray-700">
                Showing page <span className="font-medium">{pagination.currentPage}</span> of{' '}
                <span className="font-medium">{pagination.totalPages}</span> ({pagination.totalItems} total models)
              </p>
            </div>
            <div>
              <nav className="isolate inline-flex -space-x-px rounded-md shadow-sm" aria-label="Pagination">
                {/* Previous button */}
                <button
                  onClick={() => handlePageChange(page - 1)}
                  disabled={!pagination.hasPreviousPage}
                  className="relative inline-flex items-center rounded-l-md border border-gray-300 bg-white px-2 py-2 text-sm font-medium text-gray-500 hover:bg-gray-50 focus:z-20 disabled:cursor-not-allowed disabled:opacity-50"
                >
                  <span className="sr-only">Previous</span>
                  <svg className="h-5 w-5" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 20 20" fill="currentColor">
                    <path fillRule="evenodd" d="M12.79 5.23a.75.75 0 01-.02 1.06L8.832 10l3.938 3.71a.75.75 0 11-1.04 1.08l-4.5-4.25a.75.75 0 010-1.08l4.5-4.25a.75.75 0 011.06.02z" clipRule="evenodd" />
                  </svg>
                </button>

                {/* Page numbers */}
                {Array.from({ length: Math.min(pagination.totalPages, 7) }, (_, i) => {
                  // Show first 3, current page, and last 3 pages
                  let pageNum: number
                  if (pagination.totalPages <= 7) {
                    pageNum = i + 1
                  } else if (pagination.currentPage <= 4) {
                    pageNum = i + 1
                  } else if (pagination.currentPage >= pagination.totalPages - 3) {
                    pageNum = pagination.totalPages - 6 + i
                  } else {
                    pageNum = pagination.currentPage - 3 + i
                  }

                  return (
                    <button
                      key={pageNum}
                      onClick={() => handlePageChange(pageNum)}
                      className={`relative inline-flex items-center border px-4 py-2 text-sm font-medium focus:z-20 ${
                        pageNum === pagination.currentPage
                          ? 'z-10 border-blue-500 bg-blue-50 text-blue-600'
                          : 'border-gray-300 bg-white text-gray-500 hover:bg-gray-50'
                      }`}
                    >
                      {pageNum}
                    </button>
                  )
                })}

                {/* Next button */}
                <button
                  onClick={() => handlePageChange(page + 1)}
                  disabled={!pagination.hasNextPage}
                  className="relative inline-flex items-center rounded-r-md border border-gray-300 bg-white px-2 py-2 text-sm font-medium text-gray-500 hover:bg-gray-50 focus:z-20 disabled:cursor-not-allowed disabled:opacity-50"
                >
                  <span className="sr-only">Next</span>
                  <svg className="h-5 w-5" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 20 20" fill="currentColor">
                    <path fillRule="evenodd" d="M7.21 14.77a.75.75 0 01.02-1.06L11.168 10 7.23 6.29a.75.75 0 111.04-1.08l4.5 4.25a.75.75 0 010 1.08l-4.5 4.25a.75.75 0 01-1.06-.02z" clipRule="evenodd" />
                  </svg>
                </button>
              </nav>
            </div>
          </div>
        </div>
      )}

      {/* Delete Confirmation Dialog - Story 2.13 Task 11.4: Two-step confirmation */}
      <ConfirmDialog
        open={deleteModalOpen}
        onClose={handleCancelDelete}
        onConfirm={handleConfirmDelete}
        title="Delete Model"
        message="This action cannot be undone. All associated data will be permanently deleted."
        confirmText="Yes, Delete"
        loading={deleteMutation.isPending}
        requireTypedConfirmation={true}
        confirmationKeyword="DELETE"
        itemName={modelToDelete?.name}
      />
    </div>
  )
}
