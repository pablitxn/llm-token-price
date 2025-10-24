import { useModels } from '../hooks/useModels'
import ModelTable from '@/components/models/ModelTable'
import FilterSidebar from '@/components/filters/FilterSidebar'
import { LoadingSpinner } from '@/components/ui/LoadingSpinner'
import { EmptyState } from '@/components/ui/EmptyState'
import { ErrorAlert } from '@/components/ui/ErrorAlert'
import { mapErrorToUserMessage } from '@/utils/errorMessages'
import { Database } from 'lucide-react'

/**
 * HomePage component - Public homepage with model table listing
 * Story 3.1: Create Public Homepage with Basic Layout
 * Story 3.2: Fetch and Display Models in Basic Table
 * Story 3.5: Add Provider Filter
 *
 * Acceptance Criteria implemented:
 * Story 3.1:
 * - AC #1: Public route at `/` accessible without authentication
 * - AC #4: Loading state with spinner
 * - AC #5: Empty state when no models available
 * - AC #6: Error state with retry button
 * - AC #13, #14: Accessibility (semantic HTML, ARIA labels)
 *
 * Story 3.2:
 * - AC #1: Frontend fetches models from GET /api/models endpoint
 * - AC #2: Basic HTML table displays models with columns: name, provider, input price, output price
 * - AC #3: Data loads automatically on page mount
 * - AC #4: Loading spinner shown while fetching data
 * - AC #5: Error message displayed if API fails
 * - AC #6: Table displays 10+ models with sample data
 *
 * Story 3.5:
 * - AC #1: Filter sidebar created on left side of table
 */
export default function HomePage() {
  const { data, isLoading, error, refetch } = useModels()

  return (
    <>
      {/* Skip to main content link for screen readers (AC #13 - Accessibility) */}
      <a
        href="#main-content"
        className="sr-only focus:not-sr-only focus:absolute focus:top-4 focus:left-4 focus:z-50 focus:px-4 focus:py-2 focus:bg-blue-600 focus:text-white focus:rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
      >
        Skip to main content
      </a>

      <div className="flex flex-col min-h-[60vh]" id="main-content">
        {/* Hero Section */}
        <div className="text-center mb-8">
          <h1 className="text-3xl sm:text-4xl lg:text-5xl font-bold text-gray-900 mb-4">
            LLM Token Price Comparison
          </h1>
          <p className="text-lg sm:text-xl text-gray-600 max-w-3xl mx-auto">
            Compare pricing, capabilities, and benchmarks across Large Language Model providers
          </p>
        </div>

        {/* Content Area with State Management */}
        <div className="w-full max-w-6xl mx-auto">
          {/* Loading State (AC #4) */}
          {isLoading && (
            <div className="py-12">
              <LoadingSpinner size="lg" text="Loading models..." />
            </div>
          )}

          {/* Error State (AC #6) */}
          {error && (
            <div className="max-w-2xl mx-auto">
              <ErrorAlert
                error={mapErrorToUserMessage(error)}
                onRetry={() => refetch()}
              />
            </div>
          )}

          {/* Empty State (AC #5) */}
          {!isLoading && !error && data && data.data.length === 0 && (
            <EmptyState
              icon={<Database className="h-8 w-8 text-gray-400" aria-hidden="true" />}
              title="No models available"
              message="There are currently no models in the database. Please check back later or contact support if this issue persists."
            />
          )}

          {/* Success State - Models Table with Filter Sidebar */}
          {!isLoading && !error && data && data.data.length > 0 && (
            <div>
              {/* Results Count */}
              <div className="mb-4 text-sm text-gray-600" aria-live="polite">
                Found {data.meta.count} model{data.meta.count !== 1 ? 's' : ''} • Last updated:{' '}
                {new Date(data.meta.timestamp).toLocaleString()}
              </div>

              {/* Filter Sidebar + Models Table Layout (Story 3.5: AC #1) */}
              <div className="flex gap-6">
                {/* Filter Sidebar - Left side */}
                <FilterSidebar />

                {/* Models Table - Right side */}
                <div className="flex-1">
                  <ModelTable models={data.data} />
                </div>
              </div>
            </div>
          )}
        </div>
      </div>
    </>
  )
}
