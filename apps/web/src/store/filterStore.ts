import { create } from 'zustand'

/**
 * Filter state interface for managing client-side filters
 *
 * Story 3.5: Add Provider Filter
 * CRITICAL: First global Zustand store in Epic 3 - pattern will be reused in Stories 3.6-3.7, 3.11
 */
interface FilterState {
  /** Array of selected provider names for filtering */
  selectedProviders: string[]

  /**
   * Toggles a provider in the selectedProviders array
   * - Adds provider if not present
   * - Removes provider if already present
   *
   * @param provider - Provider name to toggle
   */
  toggleProvider: (provider: string) => void

  /**
   * Clears all filters, resetting to initial empty state
   */
  clearFilters: () => void

  /**
   * Returns the count of active filters
   * Used for displaying badge count in FilterSidebar
   *
   * @returns Number of active filters (selectedProviders.length)
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
 * Features:
 * - Client-side filter state (persists across component remounts)
 * - TypeScript strict mode (zero any types)
 * - Singleton pattern for global access
 * - Will be extended in Stories 3.6-3.7 for capabilities and price range filters
 *
 * Usage:
 * ```tsx
 * const { selectedProviders, toggleProvider, clearFilters } = useFilterStore()
 * ```
 */
export const useFilterStore = create<FilterState>((set, get) => ({
  // Initial state: no filters active
  selectedProviders: [],

  // Toggle provider: add if not present, remove if already present
  toggleProvider: (provider) =>
    set((state) => ({
      selectedProviders: state.selectedProviders.includes(provider)
        ? state.selectedProviders.filter((p) => p !== provider)
        : [...state.selectedProviders, provider],
    })),

  // Clear all filters: reset to empty array
  clearFilters: () => set({ selectedProviders: [] }),

  // Get active filter count: returns length of selectedProviders
  getActiveFilterCount: () => get().selectedProviders.length,
}))
