import { useQuery } from '@tanstack/react-query'
import { fetchModels } from '../api/models'
import ModelCard from '@/components/models/ModelCard'
import { LoadingSpinner } from '@/components/ui/LoadingSpinner'
import { EmptyState } from '@/components/ui/EmptyState'
import { ErrorAlert } from '@/components/ui/ErrorAlert'
import { mapErrorToUserMessage } from '@/utils/errorMessages'
import { Database } from 'lucide-react'

/**
 * HomePage component - Public homepage with model listing
 * Story 3.1: Create Public Homepage with Basic Layout
 *
 * Acceptance Criteria implemented:
 * - AC #1: Public route at `/` accessible without authentication
 * - AC #4: Loading state with spinner
 * - AC #5: Empty state when no models available
 * - AC #6: Error state with retry button
 * - AC #13, #14: Accessibility (semantic HTML, ARIA labels)
 */
export default function HomePage() {
  const { data, isLoading, error, refetch } = useQuery({
    queryKey: ['models'],
    queryFn: fetchModels,
    staleTime: 5 * 60 * 1000, // 5 minutes (as per architecture)
    retry: 2, // Retry failed requests up to 2 times
  })

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

          {/* Success State - Models Grid */}
          {!isLoading && !error && data && data.data.length > 0 && (
            <div>
              {/* Results Count */}
              <div className="mb-4 text-sm text-gray-600" aria-live="polite">
                Found {data.meta.count} model{data.meta.count !== 1 ? 's' : ''} • Last updated:{' '}
                {new Date(data.meta.timestamp).toLocaleString()}
              </div>

              {/* Models Grid (Responsive: AC #3) */}
              <div
                className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4"
                role="list"
                aria-label="LLM models"
              >
                {data.data.map((model) => (
                  <div key={model.id} role="listitem">
                    <ModelCard model={model} />
                  </div>
                ))}
              </div>
            </div>
          )}
        </div>
      </div>
    </>
  )
}
