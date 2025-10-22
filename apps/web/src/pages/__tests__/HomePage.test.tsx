import { describe, it, expect, vi } from 'vitest'
import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import HomePage from '../HomePage'
import { fetchModels } from '../../api/models'

// Mock the API
vi.mock('../../api/models', () => ({
  fetchModels: vi.fn(),
}))

// Mock ModelCard component to simplify tests
vi.mock('@/components/models/ModelCard', () => ({
  default: ({ model }: { model: { id: string; name: string } }) => (
    <div data-testid={`model-card-${model.id}`}>{model.name}</div>
  ),
}))

const createTestQueryClient = () =>
  new QueryClient({
    defaultOptions: {
      queries: {
        retry: false,
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
                meta: { count: 0, timestamp: new Date().toISOString() },
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
        meta: { count: 0, timestamp: new Date().toISOString() },
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
    it('should display error state when API fails', async () => {
      vi.mocked(fetchModels).mockRejectedValue(new Error('Network error'))

      renderWithQueryClient(<HomePage />)

      // Wait for loading to finish and error state to appear
      await waitFor(
        () => {
          expect(screen.queryByText(/loading models/i)).not.toBeInTheDocument()
        },
        { timeout: 3000 }
      )

      // Now check for error message - can be any user-friendly error
      await waitFor(() => {
        expect(
          screen.getByRole('alert')
        ).toBeInTheDocument()
      })
    })

    it('should allow user to retry after error', async () => {
      const user = userEvent.setup()

      // First call fails, second succeeds
      vi.mocked(fetchModels)
        .mockRejectedValueOnce(new Error('Network error'))
        .mockResolvedValueOnce({
          data: [
            {
              id: '1',
              name: 'GPT-4',
              provider: 'OpenAI',
              inputPricePerMillionTokens: 30,
              outputPricePerMillionTokens: 60,
              contextWindow: 8192,
              isActive: true,
            },
          ],
          meta: { count: 1, timestamp: new Date().toISOString() },
        })

      renderWithQueryClient(<HomePage />)

      // Wait for loading to finish
      await waitFor(
        () => {
          expect(screen.queryByText(/loading models/i)).not.toBeInTheDocument()
        },
        { timeout: 3000 }
      )

      // Wait for error alert to appear
      await waitFor(() => {
        expect(screen.getByRole('alert')).toBeInTheDocument()
      })

      // Click retry button
      const retryButton = screen.getByRole('button', { name: /try again/i })
      await user.click(retryButton)

      // Wait for success state
      await waitFor(
        () => {
          expect(screen.getByTestId('model-card-1')).toBeInTheDocument()
        },
        { timeout: 3000 }
      )
    })
  })

  describe('AC #1, #2, #15, #16: Page Structure and Layout', () => {
    it('should render page with header, main content, and models grid', async () => {
      vi.mocked(fetchModels).mockResolvedValue({
        data: [
          {
            id: '1',
            name: 'GPT-4',
            provider: 'OpenAI',
            inputPricePerMillionTokens: 30,
            outputPricePerMillionTokens: 60,
            contextWindow: 8192,
            isActive: true,
          },
          {
            id: '2',
            name: 'Claude 3',
            provider: 'Anthropic',
            inputPricePerMillionTokens: 15,
            outputPricePerMillionTokens: 75,
            contextWindow: 100000,
            isActive: true,
          },
        ],
        meta: { count: 2, timestamp: new Date().toISOString() },
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

      // Wait for both model cards to appear
      await waitFor(() => {
        expect(screen.getByTestId('model-card-1')).toBeInTheDocument()
        expect(screen.getByTestId('model-card-2')).toBeInTheDocument()
      })

      // Verify results count
      expect(screen.getByText(/found 2 models/i)).toBeInTheDocument()
    })
  })

  describe('AC #13, #14: Accessibility', () => {
    it('should have skip-to-content link for screen readers', async () => {
      vi.mocked(fetchModels).mockResolvedValue({
        data: [],
        meta: { count: 0, timestamp: new Date().toISOString() },
      })

      renderWithQueryClient(<HomePage />)

      // Skip link should exist
      const skipLink = screen.getByText(/skip to main content/i)
      expect(skipLink).toBeInTheDocument()
      expect(skipLink).toHaveAttribute('href', '#main-content')
    })

    it('should have proper ARIA labels for lists', async () => {
      vi.mocked(fetchModels).mockResolvedValue({
        data: [
          {
            id: '1',
            name: 'GPT-4',
            provider: 'OpenAI',
            inputPricePerMillionTokens: 30,
            outputPricePerMillionTokens: 60,
            contextWindow: 8192,
            isActive: true,
          },
        ],
        meta: { count: 1, timestamp: new Date().toISOString() },
      })

      renderWithQueryClient(<HomePage />)

      await waitFor(() => {
        expect(screen.getByRole('list', { name: /llm models/i })).toBeInTheDocument()
      })
    })

    it('should have aria-live regions for dynamic content', async () => {
      vi.mocked(fetchModels).mockResolvedValue({
        data: [
          {
            id: '1',
            name: 'GPT-4',
            provider: 'OpenAI',
            inputPricePerMillionTokens: 30,
            outputPricePerMillionTokens: 60,
            contextWindow: 8192,
            isActive: true,
          },
        ],
        meta: { count: 1, timestamp: new Date().toISOString() },
      })

      renderWithQueryClient(<HomePage />)

      await waitFor(() => {
        const resultsCount = screen.getByText(/found 1 model/i)
        expect(resultsCount).toHaveAttribute('aria-live', 'polite')
      })
    })
  })

  describe('AC #3: Responsive Layout', () => {
    it('should render responsive grid classes', async () => {
      vi.mocked(fetchModels).mockResolvedValue({
        data: [
          {
            id: '1',
            name: 'GPT-4',
            provider: 'OpenAI',
            inputPricePerMillionTokens: 30,
            outputPricePerMillionTokens: 60,
            contextWindow: 8192,
            isActive: true,
          },
        ],
        meta: { count: 1, timestamp: new Date().toISOString() },
      })

      renderWithQueryClient(<HomePage />)

      await waitFor(() => {
        const grid = screen.getByRole('list', { name: /llm models/i })
        expect(grid).toHaveClass('grid', 'grid-cols-1', 'md:grid-cols-2', 'lg:grid-cols-3')
      })
    })
  })
})
