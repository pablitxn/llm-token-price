/**
 * Test utilities for component testing
 * Provides custom render function with all necessary providers
 */

import { type ReactElement, type ReactNode } from 'react'
import { render, type RenderOptions } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'

/**
 * Creates a new QueryClient for each test to ensure isolation
 */
function createTestQueryClient() {
  return new QueryClient({
    defaultOptions: {
      queries: {
        retry: false, // Disable retries in tests
        gcTime: Infinity, // Keep data in cache
      },
      mutations: {
        retry: false,
      },
    },
  })
}

/**
 * Custom render options that extend Testing Library's RenderOptions
 */
interface CustomRenderOptions extends Omit<RenderOptions, 'wrapper'> {
  /**
   * Initial route for react-router (default: ['/'])
   */
  initialEntries?: string[]

  /**
   * Custom QueryClient instance (creates new one if not provided)
   */
  queryClient?: QueryClient
}

/**
 * Wrapper component that provides all necessary context providers
 */
function AllTheProviders({
  children,
  queryClient,
  initialEntries = ['/'],
}: {
  children: ReactNode
  queryClient: QueryClient
  initialEntries?: string[]
}) {
  return (
    <QueryClientProvider client={queryClient}>
      <MemoryRouter initialEntries={initialEntries}>
        {children}
      </MemoryRouter>
    </QueryClientProvider>
  )
}

/**
 * Custom render function that wraps Testing Library's render
 * with all necessary providers (Router, QueryClient)
 *
 * @example
 * ```tsx
 * import { renderWithProviders } from '@/test/test-utils'
 *
 * test('renders login page', () => {
 *   const { getByRole } = renderWithProviders(<AdminLoginPage />, {
 *     initialEntries: ['/admin/login']
 *   })
 *   expect(getByRole('button', { name: /sign in/i })).toBeInTheDocument()
 * })
 * ```
 */
export function renderWithProviders(
  ui: ReactElement,
  {
    initialEntries = ['/'],
    queryClient = createTestQueryClient(),
    ...renderOptions
  }: CustomRenderOptions = {}
) {
  function Wrapper({ children }: { children: ReactNode }) {
    return (
      <AllTheProviders queryClient={queryClient} initialEntries={initialEntries}>
        {children}
      </AllTheProviders>
    )
  }

  return {
    ...render(ui, { wrapper: Wrapper, ...renderOptions }),
    queryClient, // Return queryClient for advanced test cases
  }
}

/**
 * Re-export everything from Testing Library
 */
export * from '@testing-library/react'
export { userEvent } from '@testing-library/user-event'
