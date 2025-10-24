import { useMemo } from 'react'
import { useModels } from '@/hooks/useModels'
import { useFilterStore } from '@/store/filterStore'

/**
 * ProviderFilter component - Provider checkbox filter section
 *
 * Story 3.5: Add Provider Filter
 * Implements AC #2 (Provider filter section displays list of all providers with checkboxes)
 * and AC #4 (Multiple providers selectable - OR logic)
 *
 * Features:
 * - Extracts unique providers from model data (useModels hook)
 * - Renders checkboxes alphabetically sorted
 * - Real-time filtering when checkboxes toggled (AC #3)
 * - Accessible with ARIA labels and keyboard navigation
 * - Checkbox state managed by Zustand filterStore
 *
 * OR Logic: Multiple selected providers show models from ANY selected provider
 */
export default function ProviderFilter() {
  const { data } = useModels()
  const selectedProviders = useFilterStore((state) => state.selectedProviders)
  const toggleProvider = useFilterStore((state) => state.toggleProvider)

  // Extract unique providers from model data and sort alphabetically
  const providers = useMemo(() => {
    if (!data?.data) return []
    const uniqueProviders = [...new Set(data.data.map((model) => model.provider))]
    return uniqueProviders.sort() // Alphabetical order (AC #2)
  }, [data])

  // Handle case where no providers available (error/empty data)
  if (providers.length === 0) {
    return (
      <div className="space-y-2">
        <h3 className="text-sm font-medium text-gray-700">Provider</h3>
        <p className="text-xs text-gray-500">No providers available</p>
      </div>
    )
  }

  return (
    <div className="space-y-2">
      <h3 className="text-sm font-medium text-gray-700">Provider</h3>

      {/* Provider checkbox list */}
      <div className="space-y-1.5" role="group" aria-label="Filter by provider">
        {providers.map((provider) => {
          const isChecked = selectedProviders.includes(provider)

          return (
            <label
              key={provider}
              className="flex items-center gap-2 cursor-pointer group"
              htmlFor={`provider-${provider}`}
            >
              <input
                id={`provider-${provider}`}
                type="checkbox"
                checked={isChecked}
                onChange={() => toggleProvider(provider)}
                className="h-4 w-4 text-blue-600 border-gray-300 rounded focus:ring-blue-500 focus:ring-2 cursor-pointer"
                aria-label={`Filter by ${provider}`}
              />
              <span className="text-sm text-gray-700 group-hover:text-gray-900 transition-colors">
                {provider}
              </span>
            </label>
          )
        })}
      </div>
    </div>
  )
}
