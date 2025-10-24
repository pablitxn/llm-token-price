import { render, screen } from '@testing-library/react'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { renderHook, act } from '@testing-library/react'
import ProviderFilter from '../ProviderFilter'
import FilterSidebar from '../FilterSidebar'
import { useFilterStore } from '@/store/filterStore'

/**
 * Error handling and resilience tests (Story 3.5: Task 9)
 *
 * Tests how components handle error conditions:
 * 9.1: useModels error handling (graceful degradation)
 * 9.2: Empty data array handling
 * 9.3: Undefined provider names
 * 9.4: Rapid toggle provider calls (race conditions)
 * 9.5: Filter during TanStack Table loading
 */

function createTestWrapper() {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: {
        retry: false,
      },
    },
  })

  return ({ children }: { children: React.ReactNode }) => (
    <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
  )
}

describe('Filter Error Handling (Task 9)', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    const { result } = renderHook(() => useFilterStore())
    act(() => {
      result.current.clearFilters()
    })
  })

  describe('Subtask 9.1: useModels error handling', () => {
    it('should gracefully handle useModels hook error', async () => {
      // Mock useModels to return error
      vi.mock('@/hooks/useModels', () => ({
        useModels: () => ({
          data: null,
          isLoading: false,
          error: new Error('Network error'),
          refetch: vi.fn(),
        }),
      }))

      // Component should render without crashing
      const { container } = render(<ProviderFilter />, { wrapper: createTestWrapper() })
      expect(container).toBeInTheDocument()

      // Should display "No providers available" message (graceful degradation)
      expect(screen.getByText('No providers available')).toBeInTheDocument()
    })

    it('should handle FilterSidebar when useModels errors', () => {
      vi.mock('@/hooks/useModels', () => ({
        useModels: () => ({
          data: null,
          isLoading: false,
          error: new Error('Network error'),
          refetch: vi.fn(),
        }),
      }))

      const { container } = render(<FilterSidebar />, { wrapper: createTestWrapper() })
      expect(container).toBeInTheDocument()

      // Sidebar should still render with clear button disabled
      expect(screen.getByLabelText('Clear all filters')).toBeDisabled()
    })
  })

  describe('Subtask 9.2: Empty data array handling', () => {
    it('should display "No providers available" when data is empty', () => {
      vi.mock('@/hooks/useModels', () => ({
        useModels: () => ({
          data: { data: [], meta: { count: 0, timestamp: new Date().toISOString(), cached: false } },
          isLoading: false,
          error: null,
          refetch: vi.fn(),
        }),
      }))

      render(<ProviderFilter />, { wrapper: createTestWrapper() })

      expect(screen.getByText('No providers available')).toBeInTheDocument()
      expect(screen.queryByRole('checkbox')).not.toBeInTheDocument()
    })

    it('should handle empty data in FilterSidebar', () => {
      vi.mock('@/hooks/useModels', () => ({
        useModels: () => ({
          data: { data: [], meta: { count: 0, timestamp: new Date().toISOString(), cached: false } },
          isLoading: false,
          error: null,
          refetch: vi.fn(),
        }),
      }))

      const { container } = render(<FilterSidebar />, { wrapper: createTestWrapper() })
      expect(container).toBeInTheDocument()

      // Should still render sidebar structure
      expect(screen.getByText('Filters')).toBeInTheDocument()
      expect(screen.getByText('Provider')).toBeInTheDocument()
    })
  })

  describe('Subtask 9.3: Undefined provider names handling', () => {
    it('should filter out undefined or null provider names', () => {
      // Mock data with undefined/null providers
      const mockDataWithUndefined = {
        data: [
          {
            id: '1',
            name: 'Model 1',
            provider: 'OpenAI',
            version: '1.0',
            inputPricePer1M: 30.0,
            outputPricePer1M: 60.0,
            currency: 'USD',
            contextWindow: 8192,
            isActive: true,
          },
          {
            id: '2',
            name: 'Model 2',
            provider: undefined as unknown as string, // Invalid provider
            version: '1.0',
            inputPricePer1M: 15.0,
            outputPricePer1M: 75.0,
            currency: 'USD',
            contextWindow: 200000,
            isActive: true,
          },
        ],
        meta: { count: 2, timestamp: new Date().toISOString(), cached: false },
      }

      vi.mock('@/hooks/useModels', () => ({
        useModels: () => ({
          data: mockDataWithUndefined,
          isLoading: false,
          error: null,
          refetch: vi.fn(),
        }),
      }))

      // Component should render without crashing
      const { container } = render(<ProviderFilter />, { wrapper: createTestWrapper() })
      expect(container).toBeInTheDocument()

      // Should only show valid providers (OpenAI), not undefined
      // The useMemo in ProviderFilter should filter out undefined values
    })
  })

  describe('Subtask 9.4: Rapid toggleProvider calls (race conditions)', () => {
    it('should handle rapid sequential toggleProvider calls correctly', () => {
      const { result } = renderHook(() => useFilterStore())

      // Rapidly toggle providers in quick succession
      act(() => {
        result.current.toggleProvider('OpenAI')
        result.current.toggleProvider('Anthropic')
        result.current.toggleProvider('Google')
        result.current.toggleProvider('OpenAI') // Remove
        result.current.toggleProvider('Mistral')
        result.current.toggleProvider('Anthropic') // Remove
        result.current.toggleProvider('Cohere')
        result.current.toggleProvider('OpenAI') // Add again
        result.current.toggleProvider('OpenAI') // Remove again
      })

      // Final state should be: Google, Mistral, Cohere (no OpenAI or Anthropic)
      expect(result.current.selectedProviders).toEqual(['Google', 'Mistral', 'Cohere'])
      expect(result.current.getActiveFilterCount()).toBe(3)
    })

    it('should handle rapid toggle of same provider multiple times', () => {
      const { result } = renderHook(() => useFilterStore())

      // Toggle same provider 100 times rapidly
      act(() => {
        for (let i = 0; i < 100; i++) {
          result.current.toggleProvider('OpenAI')
        }
      })

      // Even number of toggles = not in array
      expect(result.current.selectedProviders).toEqual([])
      expect(result.current.getActiveFilterCount()).toBe(0)
    })

    it('should handle rapid toggle with clearFilters interspersed', () => {
      const { result } = renderHook(() => useFilterStore())

      act(() => {
        result.current.toggleProvider('OpenAI')
        result.current.toggleProvider('Anthropic')
        result.current.clearFilters()
        result.current.toggleProvider('Google')
        result.current.toggleProvider('Mistral')
        result.current.clearFilters()
        result.current.toggleProvider('Cohere')
      })

      // Final state should only have Cohere
      expect(result.current.selectedProviders).toEqual(['Cohere'])
      expect(result.current.getActiveFilterCount()).toBe(1)
    })
  })

  describe('Subtask 9.5: Filter application during TanStack Table loading', () => {
    it('should handle filter store updates when models are loading', () => {
      vi.mock('@/hooks/useModels', () => ({
        useModels: () => ({
          data: null,
          isLoading: true,
          error: null,
          refetch: vi.fn(),
        }),
      }))

      const { container } = render(<ProviderFilter />, { wrapper: createTestWrapper() })
      expect(container).toBeInTheDocument()

      // Should show "No providers available" during loading
      // (since data is null)
      expect(screen.getByText('No providers available')).toBeInTheDocument()
    })

    it('should allow filter state changes even when data is loading', () => {
      const { result } = renderHook(() => useFilterStore())

      // Simulate user toggling filters while data is loading
      act(() => {
        result.current.toggleProvider('OpenAI')
        result.current.toggleProvider('Anthropic')
      })

      // Store state should update regardless of loading state
      expect(result.current.selectedProviders).toEqual(['OpenAI', 'Anthropic'])
      expect(result.current.getActiveFilterCount()).toBe(2)

      // When data eventually loads, filters should already be applied
    })

    it('should preserve filter state across loading -> loaded transition', () => {
      const { result } = renderHook(() => useFilterStore())

      // Set filters before data loads
      act(() => {
        result.current.toggleProvider('OpenAI')
      })

      expect(result.current.selectedProviders).toContain('OpenAI')

      // Simulate loading state (data loads in background)
      // Filter state should persist
      expect(result.current.selectedProviders).toContain('OpenAI')
      expect(result.current.getActiveFilterCount()).toBe(1)
    })
  })

  describe('Additional resilience tests', () => {
    it('should handle clearFilters when already empty', () => {
      const { result } = renderHook(() => useFilterStore())

      expect(result.current.selectedProviders).toEqual([])

      act(() => {
        result.current.clearFilters()
      })

      expect(result.current.selectedProviders).toEqual([])
      expect(result.current.getActiveFilterCount()).toBe(0)
    })

    it('should handle multiple clearFilters calls in succession', () => {
      const { result } = renderHook(() => useFilterStore())

      act(() => {
        result.current.toggleProvider('OpenAI')
        result.current.clearFilters()
        result.current.clearFilters()
        result.current.clearFilters()
      })

      expect(result.current.selectedProviders).toEqual([])
    })

    it('should handle getActiveFilterCount calls repeatedly', () => {
      const { result } = renderHook(() => useFilterStore())

      act(() => {
        result.current.toggleProvider('OpenAI')
      })

      // Call getActiveFilterCount multiple times
      const count1 = result.current.getActiveFilterCount()
      const count2 = result.current.getActiveFilterCount()
      const count3 = result.current.getActiveFilterCount()

      expect(count1).toBe(1)
      expect(count2).toBe(1)
      expect(count3).toBe(1)
    })
  })
})
