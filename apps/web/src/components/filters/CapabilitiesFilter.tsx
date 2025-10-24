import { useFilterStore, type CapabilityType } from '@/store/filterStore'
import Tooltip from '@/components/ui/Tooltip'

/**
 * Capability definition with display information
 *
 * Story 3.6: Add Capabilities Filters
 * Maps backend CapabilityDto fields to user-friendly labels and descriptions
 */
interface Capability {
  /** The capability key matching CapabilityDto field name */
  key: CapabilityType
  /** User-friendly display label */
  label: string
  /** Explanation text shown in tooltip */
  description: string
  /** Optional icon/emoji for visual identification */
  icon: string
}

/**
 * List of all filterable capabilities with display metadata
 *
 * Story 3.6: Add Capabilities Filters
 * Implements AC #2 (Checkboxes for each capability) and AC #6 (Tooltip explains what each capability means)
 *
 * Sorted alphabetically for consistent UI presentation
 */
const CAPABILITIES: Capability[] = [
  {
    key: 'supportsAudioInput',
    label: 'Audio Support',
    description: 'Model supports audio input or output',
    icon: '🎤',
  },
  {
    key: 'supportsFunctionCalling',
    label: 'Function Calling',
    description: 'Model can call external functions/tools during generation',
    icon: '🔧',
  },
  {
    key: 'supportsJsonMode',
    label: 'JSON Mode',
    description: 'Model can output structured JSON responses',
    icon: '{ }',
  },
  {
    key: 'supportsStreaming',
    label: 'Streaming',
    description: 'Model supports streaming responses for real-time output',
    icon: '⚡',
  },
  {
    key: 'supportsVision',
    label: 'Vision Support',
    description: 'Model can process and analyze images',
    icon: '👁️',
  },
]

/**
 * CapabilitiesFilter component - Checkbox list for filtering models by capabilities
 *
 * Story 3.6: Add Capabilities Filters
 * Implements AC #2 (Checkboxes for each capability), AC #3 (Checking capability filters to only models with that capability),
 * AC #4 (Multiple capabilities use AND logic), and AC #6 (Tooltip explains what each capability means)
 *
 * Features:
 * - Checkbox for each capability with label and icon
 * - Info icon (ⓘ) with tooltip showing capability description
 * - Connected to Zustand filterStore for state management
 * - AND logic: selecting multiple capabilities shows only models with ALL selected capabilities
 * - Accessible keyboard navigation and screen reader support
 * - Consistent styling with ProviderFilter (Story 3.5)
 *
 * Logic:
 * - AND logic (different from provider filter OR logic)
 * - Selecting "Function Calling" + "Vision" shows only models with BOTH features
 * - Unselecting updates table immediately
 * - "Clear Filters" button clears all capability selections
 */
export default function CapabilitiesFilter() {
  const selectedCapabilities = useFilterStore((state) => state.selectedCapabilities)
  const toggleCapability = useFilterStore((state) => state.toggleCapability)

  return (
    <div>
      {/* Section heading */}
      <h3 className="text-sm font-semibold text-gray-900 mb-3">Capabilities</h3>

      {/* Capabilities checkbox list */}
      <div className="space-y-2" role="group" aria-label="Capability filters">
        {CAPABILITIES.map((capability) => {
          const isChecked = selectedCapabilities.includes(capability.key)

          return (
            <label
              key={capability.key}
              className="flex items-center gap-2 cursor-pointer group hover:bg-gray-50 -mx-2 px-2 py-1 rounded transition-colors"
            >
              {/* Checkbox input */}
              <input
                type="checkbox"
                checked={isChecked}
                onChange={() => toggleCapability(capability.key)}
                className="w-4 h-4 text-blue-600 border-gray-300 rounded focus:ring-2 focus:ring-blue-500 focus:ring-offset-1 cursor-pointer"
                aria-describedby={`tooltip-${capability.key}`}
              />

              {/* Icon */}
              <span className="text-base" aria-hidden="true">
                {capability.icon}
              </span>

              {/* Label text */}
              <span className="text-sm text-gray-700 flex-1">{capability.label}</span>

              {/* Info icon with tooltip */}
              <Tooltip content={capability.description}>
                <span
                  className="text-gray-400 text-xs cursor-help hover:text-gray-600 transition-colors"
                  aria-label={`Information about ${capability.label}`}
                  tabIndex={0}
                >
                  ⓘ
                </span>
              </Tooltip>
            </label>
          )
        })}
      </div>

      {/* Helper text explaining AND logic */}
      {selectedCapabilities.length > 1 && (
        <p className="mt-3 text-xs text-gray-500 italic">
          Showing models with <strong>all</strong> selected capabilities
        </p>
      )}
    </div>
  )
}
