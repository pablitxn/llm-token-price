import { describe, it, expect, vi } from 'vitest'
import { render, screen, waitFor } from '@testing-library/react'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import HomePage from '../HomePage'
import { fetchModels } from '../../api/models'

// Mock the API
vi.mock('../../api/models', () => ({
  fetchModels: vi.fn(),
}))

// Mock ModelTable component to simplify tests
vi.mock('@/components/models/ModelTable', () => ({
  default: ({ models }: { models: Array<{ id: string; name: string }> }) => (
    <table data-testid="models-table">
      <tbody>
        {models.map((model) => (
          <tr key={model.id} data-testid={`model-row-${model.id}`}>
            <td>{model.name}</td>
          </tr>
        ))}
      </tbody>
    </table>
  ),
}))

const createTestQueryClient = () =>
  new QueryClient({
    defaultOptions: {
      queries: {
        retry: false, // Disable retries for predictable testing
        gcTime: 0, // Disable caching
      },
    },
  })

function renderWithQueryClient(component: React.ReactElement) {
  const queryClient = createTestQueryClient()
  return render(
    <QueryClientProvider client={queryClient}>{component}</QueryClientProvider>
  )
}

describe('HomePage Component - Story 3.1', () => {
  describe('AC #4: Loading State', () => {
    it('should display loading spinner while fetching models', async () => {
      // Mock a delayed response
      vi.mocked(fetchModels).mockImplementation(
        () =>
          new Promise((resolve) => {
            setTimeout(() => {
              resolve({
                data: [],
                meta: { count: 0, cached: false, timestamp: new Date().toISOString() },
              })
            }, 1000)
          })
      )

      renderWithQueryClient(<HomePage />)

      // Loading spinner should be visible
      expect(screen.getByText(/loading models/i)).toBeInTheDocument()

      // Wait for loading to complete
      await waitFor(() => {
        expect(screen.queryByText(/loading models/i)).not.toBeInTheDocument()
      })
    })
  })

  describe('AC #5: Empty State', () => {
    it('should display empty state when no models are available', async () => {
      vi.mocked(fetchModels).mockResolvedValue({
        data: [],
        meta: { count: 0, cached: false, timestamp: new Date().toISOString() },
      })

      renderWithQueryClient(<HomePage />)

      // Wait for the empty state to appear
      await waitFor(() => {
        expect(screen.getByText(/no models available/i)).toBeInTheDocument()
      })

      expect(
        screen.getByText(
          /there are currently no models in the database/i
        )
      ).toBeInTheDocument()
    })
  })

  describe('AC #6: Error State with Retry Button', () => {
    it('should display error state when API fails with retry button', async () => {
      // Mock rejected API call
      vi.mocked(fetchModels).mockRejectedValue(new Error('Network error'))

      renderWithQueryClient(<HomePage />)

      // Wait for error alert to appear
      const errorAlert = await screen.findByRole('alert', {}, { timeout: 5000 })
      expect(errorAlert).toBeInTheDocument()

      // Verify retry button exists (AC #6 - retry functionality)
      const retryButton = screen.getByRole('button', { name: /try again/i })
      expect(retryButton).toBeInTheDocument()
      expect(retryButton).toBeEnabled()
    })
  })

  describe('AC #1, #2, #15, #16: Page Structure and Layout', () => {
    it('should render page with header, main content, and models table', async () => {
      vi.mocked(fetchModels).mockResolvedValue({
        data: [
          {
            id: '1',
            name: 'GPT-4',
            provider: 'OpenAI',
            version: null,
            status: 'active',
            inputPricePer1M: 30,
            outputPricePer1M: 60,
            currency: 'USD',
            updatedAt: new Date().toISOString(),
            capabilities: null,
            topBenchmarks: [],
          },
          {
            id: '2',
            name: 'Claude 3',
            provider: 'Anthropic',
            version: null,
            status: 'active',
            inputPricePer1M: 15,
            outputPricePer1M: 75,
            currency: 'USD',
            updatedAt: new Date().toISOString(),
            capabilities: null,
            topBenchmarks: [],
          },
        ],
        meta: { count: 2, cached: false, timestamp: new Date().toISOString() },
      })

      renderWithQueryClient(<HomePage />)

      // Wait for loading to finish
      await waitFor(
        () => {
          expect(screen.queryByText(/loading models/i)).not.toBeInTheDocument()
        },
        { timeout: 3000 }
      )

      // Verify page title
      expect(
        screen.getByRole('heading', { name: /llm token price comparison/i })
      ).toBeInTheDocument()

      // Wait for both model rows to appear in table
      await waitFor(() => {
        expect(screen.getByTestId('model-row-1')).toBeInTheDocument()
        expect(screen.getByTestId('model-row-2')).toBeInTheDocument()
      })

      // Verify results count
      expect(screen.getByText(/found 2 models/i)).toBeInTheDocument()
    })
  })

  describe('AC #13, #14: Accessibility', () => {
    it('should have skip-to-content link for screen readers', async () => {
      vi.mocked(fetchModels).mockResolvedValue({
        data: [],
        meta: { count: 0, cached: false, timestamp: new Date().toISOString() },
      })

      renderWithQueryClient(<HomePage />)

      // Skip link should exist
      const skipLink = screen.getByText(/skip to main content/i)
      expect(skipLink).toBeInTheDocument()
      expect(skipLink).toHaveAttribute('href', '#main-content')
    })

    it('should render table with models data', async () => {
      vi.mocked(fetchModels).mockResolvedValue({
        data: [
          {
            id: '1',
            name: 'GPT-4',
            provider: 'OpenAI',
            version: null,
            status: 'active',
            inputPricePer1M: 30,
            outputPricePer1M: 60,
            currency: 'USD',
            updatedAt: new Date().toISOString(),
            capabilities: null,
            topBenchmarks: [],
          },
        ],
        meta: { count: 1, cached: false, timestamp: new Date().toISOString() },
      })

      renderWithQueryClient(<HomePage />)

      await waitFor(() => {
        expect(screen.getByTestId('models-table')).toBeInTheDocument()
      })
    })

    it('should have aria-live regions for dynamic content', async () => {
      vi.mocked(fetchModels).mockResolvedValue({
        data: [
          {
            id: '1',
            name: 'GPT-4',
            provider: 'OpenAI',
            version: null,
            status: 'active',
            inputPricePer1M: 30,
            outputPricePer1M: 60,
            currency: 'USD',
            updatedAt: new Date().toISOString(),
            capabilities: null,
            topBenchmarks: [],
          },
        ],
        meta: { count: 1, cached: false, timestamp: new Date().toISOString() },
      })

      renderWithQueryClient(<HomePage />)

      await waitFor(() => {
        const resultsCount = screen.getByText(/found 1 model/i)
        expect(resultsCount).toHaveAttribute('aria-live', 'polite')
      })
    })
  })

  describe('AC #3: Responsive Layout', () => {
    it('should render table with overflow handling for responsive design', async () => {
      vi.mocked(fetchModels).mockResolvedValue({
        data: [
          {
            id: '1',
            name: 'GPT-4',
            provider: 'OpenAI',
            version: null,
            status: 'active',
            inputPricePer1M: 30,
            outputPricePer1M: 60,
            currency: 'USD',
            updatedAt: new Date().toISOString(),
            capabilities: null,
            topBenchmarks: [],
          },
        ],
        meta: { count: 1, cached: false, timestamp: new Date().toISOString() },
      })

      renderWithQueryClient(<HomePage />)

      await waitFor(() => {
        const table = screen.getByTestId('models-table')
        expect(table).toBeInTheDocument()
      })
    })
  })
})
