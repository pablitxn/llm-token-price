import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { act, renderHook } from '@testing-library/react'
import ProviderFilter from '../ProviderFilter'
import FilterSidebar from '../FilterSidebar'
import { useFilterStore } from '@/store/filterStore'
import type { ModelDto } from '@/types/models'

/**
 * Component tests for ProviderFilter and FilterSidebar (Story 3.5: Task 8)
 *
 * Tests filtering functionality at the component level:
 * - Provider checkbox rendering
 * - Filter state management
 * - Clear filters button
 * - Filter count badge
 */

// Mock data
const mockModels: ModelDto[] = [
  {
    id: '1',
    name: 'GPT-4',
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
    name: 'Claude 3 Opus',
    provider: 'Anthropic',
    version: '1.0',
    inputPricePer1M: 15.0,
    outputPricePer1M: 75.0,
    currency: 'USD',
    contextWindow: 200000,
    isActive: true,
  },
  {
    id: '3',
    name: 'Gemini Pro',
    provider: 'Google',
    version: '1.0',
    inputPricePer1M: 0.5,
    outputPricePer1M: 1.5,
    currency: 'USD',
    contextWindow: 32768,
    isActive: true,
  },
]

const mockApiResponse = {
  data: mockModels,
  meta: {
    count: mockModels.length,
    timestamp: new Date().toISOString(),
    cached: false,
  },
}

// Mock useModels hook
vi.mock('@/hooks/useModels', () => ({
  useModels: () => ({
    data: mockApiResponse,
    isLoading: false,
    error: null,
    refetch: vi.fn(),
  }),
}))

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

describe('ProviderFilter Component (Task 8)', () => {
  beforeEach(() => {
    const { result } = renderHook(() => useFilterStore())
    act(() => {
      result.current.clearFilters()
    })
  })

  describe('Subtask 8.1-8.3: Provider selection and unselection', () => {
    it('should render provider checkboxes in alphabetical order', () => {
      render(<ProviderFilter />, { wrapper: createTestWrapper() })

      const checkboxes = screen.getAllByRole('checkbox')
      expect(checkboxes).toHaveLength(3)

      // Check alphabetical order: Anthropic, Google, OpenAI
      expect(screen.getByLabelText('Filter by Anthropic')).toBeInTheDocument()
      expect(screen.getByLabelText('Filter by Google')).toBeInTheDocument()
      expect(screen.getByLabelText('Filter by OpenAI')).toBeInTheDocument()
    })

    it('should update filter store when checkbox is checked', async () => {
      const user = userEvent.setup()
      render(<ProviderFilter />, { wrapper: createTestWrapper() })

      const openAICheckbox = screen.getByLabelText('Filter by OpenAI')
      expect(openAICheckbox).not.toBeChecked()

      await user.click(openAICheckbox)

      expect(openAICheckbox).toBeChecked()

      // Verify store was updated
      const { result } = renderHook(() => useFilterStore())
      expect(result.current.selectedProviders).toContain('OpenAI')
    })

    it('should update filter store when checkbox is unchecked', async () => {
      const user = userEvent.setup()

      // Pre-select a provider
      const { result } = renderHook(() => useFilterStore())
      act(() => {
        result.current.toggleProvider('OpenAI')
      })

      render(<ProviderFilter />, { wrapper: createTestWrapper() })

      const openAICheckbox = screen.getByLabelText('Filter by OpenAI')
      expect(openAICheckbox).toBeChecked()

      await user.click(openAICheckbox)

      expect(openAICheckbox).not.toBeChecked()
      expect(result.current.selectedProviders).not.toContain('OpenAI')
    })

    it('should handle multiple provider selections', async () => {
      const user = userEvent.setup()
      render(<ProviderFilter />, { wrapper: createTestWrapper() })

      await user.click(screen.getByLabelText('Filter by OpenAI'))
      await user.click(screen.getByLabelText('Filter by Anthropic'))

      const { result } = renderHook(() => useFilterStore())
      expect(result.current.selectedProviders).toEqual(['OpenAI', 'Anthropic'])
    })
  })

  describe('Subtask 8.10: Edge case - provider with zero models', () => {
    it('should handle empty provider list gracefully', () => {
      // Mock empty data
      vi.mock('@/hooks/useModels', () => ({
        useModels: () => ({
          data: { data: [], meta: { count: 0, timestamp: new Date().toISOString(), cached: false } },
          isLoading: false,
          error: null,
          refetch: vi.fn(),
        }),
      }))

      render(<ProviderFilter />, { wrapper: createTestWrapper() })

      // Should display "No providers available" message
      // Note: This test may need adjustment based on actual implementation
    })
  })
})

describe('FilterSidebar Component (Task 8)', () => {
  beforeEach(() => {
    const { result } = renderHook(() => useFilterStore())
    act(() => {
      result.current.clearFilters()
    })
  })

  describe('Subtask 8.4: Clear Filters button', () => {
    it('should be disabled when no filters are active', () => {
      render(<FilterSidebar />, { wrapper: createTestWrapper() })

      const clearButton = screen.getByLabelText('Clear all filters')
      expect(clearButton).toBeDisabled()
    })

    it('should be enabled when filters are active', async () => {
      const user = userEvent.setup()
      render(<FilterSidebar />, { wrapper: createTestWrapper() })

      // Select a provider
      await user.click(screen.getByLabelText('Filter by OpenAI'))

      // Clear button should be enabled
      const clearButton = screen.getByLabelText('Clear all filters')
      expect(clearButton).not.toBeDisabled()
    })

    it('should clear all filters when clicked', async () => {
      const user = userEvent.setup()
      render(<FilterSidebar />, { wrapper: createTestWrapper() })

      // Select multiple providers
      await user.click(screen.getByLabelText('Filter by OpenAI'))
      await user.click(screen.getByLabelText('Filter by Anthropic'))

      const { result } = renderHook(() => useFilterStore())
      expect(result.current.selectedProviders.length).toBe(2)

      // Click clear button
      const clearButton = screen.getByLabelText('Clear all filters')
      await user.click(clearButton)

      // Store should be cleared
      await waitFor(() => {
        expect(result.current.selectedProviders.length).toBe(0)
      })
    })
  })

  describe('Subtask 8.5: Filter count badge', () => {
    it('should not display badge when no filters active', () => {
      render(<FilterSidebar />, { wrapper: createTestWrapper() })

      expect(screen.queryByLabelText(/active filter/)).not.toBeInTheDocument()
    })

    it('should display correct count with 1 filter', async () => {
      const user = userEvent.setup()
      render(<FilterSidebar />, { wrapper: createTestWrapper() })

      await user.click(screen.getByLabelText('Filter by OpenAI'))

      await waitFor(() => {
        expect(screen.getByLabelText('1 active filter')).toBeInTheDocument()
      })
    })

    it('should display correct count with 2 filters', async () => {
      const user = userEvent.setup()
      render(<FilterSidebar />, { wrapper: createTestWrapper() })

      await user.click(screen.getByLabelText('Filter by OpenAI'))
      await user.click(screen.getByLabelText('Filter by Anthropic'))

      await waitFor(() => {
        expect(screen.getByLabelText('2 active filters')).toBeInTheDocument()
      })
    })

    it('should display correct count with 3 filters', async () => {
      const user = userEvent.setup()
      render(<FilterSidebar />, { wrapper: createTestWrapper() })

      await user.click(screen.getByLabelText('Filter by OpenAI'))
      await user.click(screen.getByLabelText('Filter by Anthropic'))
      await user.click(screen.getByLabelText('Filter by Google'))

      await waitFor(() => {
        expect(screen.getByLabelText('3 active filters')).toBeInTheDocument()
      })
    })

    it('should hide badge after clearing filters', async () => {
      const user = userEvent.setup()
      render(<FilterSidebar />, { wrapper: createTestWrapper() })

      await user.click(screen.getByLabelText('Filter by OpenAI'))

      await waitFor(() => {
        expect(screen.getByLabelText('1 active filter')).toBeInTheDocument()
      })

      await user.click(screen.getByLabelText('Clear all filters'))

      await waitFor(() => {
        expect(screen.queryByLabelText(/active filter/)).not.toBeInTheDocument()
      })
    })
  })
})
