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
import { useAdminModels, useDeleteModel } from '@/hooks/useAdminModels'
import type { AdminModelDto } from '@/types/admin'
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

  // Debounce search term to avoid excessive re-renders and API calls
  const debouncedSearchTerm = useDebounce(searchTerm, 300)

  // Sync search term with URL params when it changes
  useEffect(() => {
    if (searchTerm) {
      setSearchParams({ search: searchTerm })
    } else {
      // Remove search param if empty
      setSearchParams({})
    }
  }, [searchTerm, setSearchParams])

  // Fetch models using TanStack Query hook
  const { data: models, isLoading, error, refetch } = useAdminModels(debouncedSearchTerm)

  // Delete mutation hook
  const deleteMutation = useDeleteModel()

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

  // Compute models count for display
  const modelsCount = models?.length ?? 0

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

      {/* Models Count */}
      <div className="mt-4">
        <p className="text-sm text-gray-700">
          Showing <span className="font-medium">{modelsCount}</span> model{modelsCount !== 1 ? 's' : ''}
        </p>
      </div>

      {/* Loading State */}
      {isLoading && (
        <div className="mt-8 flex justify-center">
          <div className="inline-block h-8 w-8 animate-spin rounded-full border-4 border-solid border-blue-600 border-r-transparent align-[-0.125em] motion-reduce:animate-[spin_1.5s_linear_infinite]" role="status">
            <span className="!absolute !-m-px !h-px !w-px !overflow-hidden !whitespace-nowrap !border-0 !p-0 ![clip:rect(0,0,0,0)]">
              Loading...
            </span>
          </div>
        </div>
      )}

      {/* Error State */}
      {error && (
        <div className="mt-8 rounded-md bg-red-50 p-4">
          <div className="flex">
            <div className="flex-shrink-0">
              <svg
                className="h-5 w-5 text-red-400"
                xmlns="http://www.w3.org/2000/svg"
                viewBox="0 0 20 20"
                fill="currentColor"
                aria-hidden="true"
              >
                <path
                  fillRule="evenodd"
                  d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z"
                  clipRule="evenodd"
                />
              </svg>
            </div>
            <div className="ml-3">
              <h3 className="text-sm font-medium text-red-800">Error loading models</h3>
              <div className="mt-2 text-sm text-red-700">
                <p>{error.message}</p>
              </div>
              <div className="mt-4">
                <button
                  type="button"
                  onClick={() => refetch()}
                  className="rounded-md bg-red-50 px-2 py-1.5 text-sm font-medium text-red-800 hover:bg-red-100 focus:outline-none focus:ring-2 focus:ring-red-600 focus:ring-offset-2 focus:ring-offset-red-50"
                >
                  Try Again
                </button>
              </div>
            </div>
          </div>
        </div>
      )}

      {/* Models Table */}
      {!isLoading && !error && models && (
        <div className="mt-8">
          <ModelList models={models} searchTerm={debouncedSearchTerm} onDeleteClick={handleDeleteClick} />
        </div>
      )}

      {/* Delete Confirmation Dialog */}
      <ConfirmDialog
        open={deleteModalOpen}
        onClose={handleCancelDelete}
        onConfirm={handleConfirmDelete}
        title="Delete Model"
        message={`Are you sure you want to delete '${modelToDelete?.name}'? This action cannot be undone.`}
        confirmText="Delete"
        loading={deleteMutation.isPending}
      />
    </div>
  )
}
