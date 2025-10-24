import { create } from 'zustand'

/**
 * Capability types matching backend ModelCapabilities fields
 *
 * Story 3.6: Add Capabilities Filters
 * These types correspond to the CapabilityDto interface boolean fields
 */
export type CapabilityType =
  | 'supportsFunctionCalling'
  | 'supportsVision'
  | 'supportsAudioInput'
  | 'supportsStreaming'
  | 'supportsJsonMode'

/**
 * Filter state interface for managing client-side filters
 *
 * Story 3.5: Add Provider Filter
 * Story 3.6: Add Capabilities Filters (extended)
 * CRITICAL: First global Zustand store in Epic 3 - pattern will be reused in Stories 3.6-3.7, 3.11
 */
interface FilterState {
  /** Array of selected provider names for filtering (Story 3.5) */
  selectedProviders: string[]

  /** Array of selected capability types for filtering (Story 3.6) */
  selectedCapabilities: CapabilityType[]

  /**
   * Toggles a provider in the selectedProviders array
   * - Adds provider if not present
   * - Removes provider if already present
   *
   * @param provider - Provider name to toggle
   */
  toggleProvider: (provider: string) => void

  /**
   * Toggles a capability in the selectedCapabilities array
   * - Adds capability if not present
   * - Removes capability if already present
   *
   * Story 3.6: Add Capabilities Filters
   * Implements AC #3 (Checking capability filters to only models with that capability)
   *
   * @param capability - Capability type to toggle
   */
  toggleCapability: (capability: CapabilityType) => void

  /**
   * Clears all filters, resetting to initial empty state
   *
   * Story 3.6: Updated to clear both providers and capabilities
   */
  clearFilters: () => void

  /**
   * Returns the count of active filters
   * Used for displaying badge count in FilterSidebar
   *
   * Story 3.6: Updated to include capabilities count
   *
   * @returns Number of active filters (selectedProviders.length + selectedCapabilities.length)
   */
  getActiveFilterCount: () => number
}

/**
 * Zustand store for filter state management
 *
 * Story 3.5: Add Provider Filter
 * Implements AC #3 (Checking/unchecking provider filters table in real-time)
 * and AC #4 (Multiple providers selectable - OR logic)
 *
 * Story 3.6: Add Capabilities Filters (extended)
 * Implements AC #3 (Checking capability filters to only models with that capability)
 * and AC #4 (Multiple capabilities use AND logic - must have all selected)
 *
 * Features:
 * - Client-side filter state (persists across component remounts)
 * - TypeScript strict mode (zero any types)
 * - Singleton pattern for global access
 * - Provider filters use OR logic (any selected provider)
 * - Capability filters use AND logic (all selected capabilities)
 * - Will be extended in Story 3.7 for price range filters
 *
 * Usage:
 * ```tsx
 * const { selectedProviders, selectedCapabilities, toggleProvider, toggleCapability, clearFilters } = useFilterStore()
 * ```
 */
export const useFilterStore = create<FilterState>((set, get) => ({
  // Initial state: no filters active
  selectedProviders: [],
  selectedCapabilities: [],

  // Toggle provider: add if not present, remove if already present
  toggleProvider: (provider) =>
    set((state) => ({
      selectedProviders: state.selectedProviders.includes(provider)
        ? state.selectedProviders.filter((p) => p !== provider)
        : [...state.selectedProviders, provider],
    })),

  // Toggle capability: add if not present, remove if already present (Story 3.6)
  toggleCapability: (capability) =>
    set((state) => ({
      selectedCapabilities: state.selectedCapabilities.includes(capability)
        ? state.selectedCapabilities.filter((c) => c !== capability)
        : [...state.selectedCapabilities, capability],
    })),

  // Clear all filters: reset to empty arrays (Story 3.6 - updated to clear capabilities too)
  clearFilters: () =>
    set({
      selectedProviders: [],
      selectedCapabilities: [],
    }),

  // Get active filter count: returns total of providers + capabilities (Story 3.6)
  getActiveFilterCount: () => {
    const { selectedProviders, selectedCapabilities } = get()
    return selectedProviders.length + selectedCapabilities.length
  },
}))
