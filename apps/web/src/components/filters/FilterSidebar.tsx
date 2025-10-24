import { useFilterStore } from '@/store/filterStore'
import ProviderFilter from './ProviderFilter'

/**
 * FilterSidebar component - Main filter container positioned left of ModelTable
 *
 * Story 3.5: Add Provider Filter
 * Implements AC #1 (Filter sidebar created on left side of table)
 * and AC #6 (Filter state shows count of active filters)
 *
 * Features:
 * - Displays filter count badge when filters are active
 * - "Clear Filters" button to reset all selections
 * - Contains ProviderFilter section (AC #2)
 * - Responsive design (visible on desktop, collapsible on mobile)
 * - Accessible with semantic HTML and ARIA attributes
 *
 * Future enhancements (Stories 3.6-3.7):
 * - CapabilityFilter component
 * - PriceRangeFilter component
 */
export default function FilterSidebar() {
  const clearFilters = useFilterStore((state) => state.clearFilters)
  const getActiveFilterCount = useFilterStore((state) => state.getActiveFilterCount)
  const filterCount = getActiveFilterCount()

  return (
    <aside
      className="w-64 bg-white border-r border-gray-200 p-4 rounded-lg shadow-sm"
      aria-label="Filters sidebar"
    >
      {/* Header with filter count badge and clear button */}
      <div className="flex items-center justify-between mb-6">
        <h2 className="text-lg font-semibold text-gray-900">
          Filters
          {/* Filter count badge - only shown when filters are active (AC #6) */}
          {filterCount > 0 && (
            <span
              className="ml-2 inline-flex items-center justify-center px-2 py-0.5 text-xs font-medium text-blue-700 bg-blue-100 rounded-full"
              aria-label={`${filterCount} active filter${filterCount !== 1 ? 's' : ''}`}
            >
              {filterCount}
            </span>
          )}
        </h2>

        {/* Clear Filters button (AC #5) - Task 5 will enhance this */}
        <button
          onClick={clearFilters}
          disabled={filterCount === 0}
          className={`text-sm font-medium transition-colors ${
            filterCount === 0
              ? 'text-gray-400 cursor-not-allowed'
              : 'text-blue-600 hover:text-blue-800 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-1 rounded px-2 py-1'
          }`}
          aria-label="Clear all filters"
        >
          Clear
        </button>
      </div>

      {/* Filter Sections */}
      <div className="space-y-6">
        {/* Provider Filter Section (AC #2) */}
        <ProviderFilter />

        {/* Future filter sections will be added here:
         * Story 3.6: <CapabilityFilter />
         * Story 3.7: <PriceRangeFilter />
         */}
      </div>
    </aside>
  )
}
