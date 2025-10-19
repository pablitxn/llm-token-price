/**
 * Component tests for AdminBenchmarksPage
 * Story 2.9 - Task 10.1: Test benchmarks list page
 * Priority: P1-P2 (List view and filtering testing)
 */

import { describe, test, expect, vi, beforeEach } from 'vitest'
import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { BrowserRouter } from 'react-router-dom'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import AdminBenchmarksPage from './AdminBenchmarksPage'
import type { BenchmarkResponseDto } from '@/schemas/benchmarkSchema'

// Mock benchmarks data
const mockBenchmarks: BenchmarkResponseDto[] = [
  {
    id: '1',
    benchmarkName: 'MMLU',
    fullName: 'Massive Multitask Language Understanding',
    description: 'Tests reasoning ability',
    category: 'Reasoning',
    interpretation: 'HigherBetter',
    typicalRangeMin: 0,
    typicalRangeMax: 100,
    weightInQaps: 0.30,
    createdAt: '2024-01-01T00:00:00Z',
    isActive: true,
  },
  {
    id: '2',
    benchmarkName: 'HumanEval',
    fullName: 'Human Eval Coding',
    description: 'Tests coding ability',
    category: 'Code',
    interpretation: 'HigherBetter',
    typicalRangeMin: 0,
    typicalRangeMax: 100,
    weightInQaps: 0.25,
    createdAt: '2024-01-02T00:00:00Z',
    isActive: true,
  },
  {
    id: '3',
    benchmarkName: 'OldBenchmark',
    fullName: 'Deprecated Benchmark',
    description: 'Old benchmark',
    category: 'Math',
    interpretation: 'LowerBetter',
    typicalRangeMin: 0,
    typicalRangeMax: 100,
    weightInQaps: 0.20,
    createdAt: '2024-01-03T00:00:00Z',
    isActive: false,
  },
]

// Mock TanStack Query hooks
vi.mock('@/hooks/useBenchmarks', () => ({
  useBenchmarks: vi.fn(() => ({
    data: mockBenchmarks,
    isLoading: false,
    error: null,
    refetch: vi.fn(),
  })),
  useDeleteBenchmark: vi.fn(() => ({
    mutateAsync: vi.fn().mockResolvedValue(undefined),
    isPending: false,
  })),
}))

// Mock react-router-dom navigation
const mockNavigate = vi.fn()
vi.mock('react-router-dom', async () => {
  const actual = await vi.importActual('react-router-dom')
  return {
    ...actual,
    useNavigate: () => mockNavigate,
  }
})

// Mock ConfirmDialog
vi.mock('@/components/ui/ConfirmDialog', () => ({
  ConfirmDialog: ({ open, onConfirm, onClose, title, message }: any) => {
    if (!open) return null
    return (
      <div data-testid="confirm-dialog">
        <h2>{title}</h2>
        <p>{message}</p>
        <button onClick={onConfirm}>Confirm</button>
        <button onClick={onClose}>Cancel</button>
      </div>
    )
  },
}))

describe('AdminBenchmarksPage Component', () => {
  let queryClient: QueryClient

  beforeEach(() => {
    queryClient = new QueryClient({
      defaultOptions: {
        queries: { retry: false },
        mutations: { retry: false },
      },
    })
    mockNavigate.mockClear()
  })

  const renderPage = () => {
    return render(
      <QueryClientProvider client={queryClient}>
        <BrowserRouter>
          <AdminBenchmarksPage />
        </BrowserRouter>
      </QueryClientProvider>
    )
  }

  /**
   * [P1] Should render page header and "Add New Benchmark" button
   * Story 2.9 AC#1: Benchmarks management page created
   */
  test('[P1] should render page header and add new button', () => {
    // GIVEN: AdminBenchmarksPage component
    // WHEN: Rendering the page
    renderPage()

    // THEN: Page header and add button should be visible
    expect(screen.getByRole('heading', { name: /benchmarks/i })).toBeInTheDocument()
    expect(screen.getByText(/view and manage all benchmark definitions/i)).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /add new benchmark/i })).toBeInTheDocument()
  })

  /**
   * [P1] Should display all benchmarks in table
   * Story 2.9 AC#2: List view shows all benchmark definitions
   */
  test('[P1] should display all benchmarks in table including inactive', () => {
    // GIVEN: AdminBenchmarksPage with mock benchmarks
    // WHEN: Rendering the page
    renderPage()

    // THEN: All benchmarks should be displayed
    expect(screen.getByText('MMLU')).toBeInTheDocument()
    expect(screen.getByText('Massive Multitask Language Understanding')).toBeInTheDocument()
    expect(screen.getByText('HumanEval')).toBeInTheDocument()
    expect(screen.getByText('OldBenchmark')).toBeInTheDocument()

    // AND: Should show count
    expect(screen.getByText(/showing.*3.*benchmark/i)).toBeInTheDocument()
    expect(screen.getByText(/2.*active/i)).toBeInTheDocument()
  })

  /**
   * [P1] Should display table with all required columns
   * Story 2.9 - Task 1.4: Table columns
   */
  test('[P1] should display table with all required columns', () => {
    // GIVEN: AdminBenchmarksPage
    // WHEN: Rendering the page
    renderPage()

    // THEN: All column headers should be present
    expect(screen.getByText('Name')).toBeInTheDocument()
    expect(screen.getByText('Full Name')).toBeInTheDocument()
    expect(screen.getByText('Category')).toBeInTheDocument()
    expect(screen.getByText('Interpretation')).toBeInTheDocument()
    expect(screen.getByText('Typical Range')).toBeInTheDocument()
    expect(screen.getByText('QAPS Weight')).toBeInTheDocument()
    expect(screen.getByText('Status')).toBeInTheDocument()
  })

  /**
   * [P1] Should display category badges
   */
  test('[P1] should display category badges for each benchmark', () => {
    // GIVEN: AdminBenchmarksPage
    // WHEN: Rendering the page
    renderPage()

    // THEN: Category badges should be visible
    expect(screen.getByText('Reasoning')).toBeInTheDocument()
    expect(screen.getByText('Code')).toBeInTheDocument()
    expect(screen.getByText('Math')).toBeInTheDocument()
  })

  /**
   * [P1] Should display interpretation as readable text
   */
  test('[P1] should display interpretation as readable text', () => {
    // GIVEN: AdminBenchmarksPage
    // WHEN: Rendering the page
    renderPage()

    // THEN: Interpretations should be readable
    const higherBetterCells = screen.getAllByText(/higher is better/i)
    expect(higherBetterCells.length).toBeGreaterThan(0)

    expect(screen.getByText(/lower is better/i)).toBeInTheDocument()
  })

  /**
   * [P1] Should display typical range as "min - max"
   */
  test('[P1] should display typical range in correct format', () => {
    // GIVEN: AdminBenchmarksPage
    // WHEN: Rendering the page
    renderPage()

    // THEN: Range should be displayed as "0 - 100"
    const rangeCells = screen.getAllByText('0 - 100')
    expect(rangeCells.length).toBe(3) // All 3 benchmarks have 0-100 range
  })

  /**
   * [P1] Should display QAPS weight as percentage
   */
  test('[P1] should display QAPS weight as percentage', () => {
    // GIVEN: AdminBenchmarksPage
    // WHEN: Rendering the page
    renderPage()

    // THEN: Weights should be displayed as percentages
    expect(screen.getByText('30%')).toBeInTheDocument() // 0.30
    expect(screen.getByText('25%')).toBeInTheDocument() // 0.25
    expect(screen.getByText('20%')).toBeInTheDocument() // 0.20
  })

  /**
   * [P1] Should display Active/Inactive status badges
   */
  test('[P1] should display status badges for active and inactive benchmarks', () => {
    // GIVEN: AdminBenchmarksPage with active and inactive benchmarks
    // WHEN: Rendering the page
    renderPage()

    // THEN: Status badges should be visible
    const activeBadges = screen.getAllByText('Active')
    expect(activeBadges).toHaveLength(2)

    expect(screen.getByText('Inactive')).toBeInTheDocument()
  })

  /**
   * [P2] Should render category filter dropdown
   * Story 2.9 - Task 1.6: Filter by category
   */
  test('[P2] should render category filter dropdown with all categories', () => {
    // GIVEN: AdminBenchmarksPage
    // WHEN: Rendering the page
    renderPage()

    // THEN: Category filter should be present
    expect(screen.getByLabelText(/filter by category/i)).toBeInTheDocument()

    const filterSelect = screen.getByLabelText(/filter by category/i)
    expect(filterSelect).toBeInTheDocument()

    // Should have "All Categories" option
    expect(screen.getByRole('option', { name: /all categories/i })).toBeInTheDocument()
  })

  /**
   * [P1] Should have Edit and Delete buttons for each benchmark
   * Story 2.9 AC#5: Edit and delete functionality
   */
  test('[P1] should display Edit and Delete buttons for each active benchmark', () => {
    // GIVEN: AdminBenchmarksPage
    // WHEN: Rendering the page
    renderPage()

    // THEN: Edit buttons should be present (one per benchmark)
    const editButtons = screen.getAllByRole('button', { name: /edit/i })
    expect(editButtons).toHaveLength(3)

    // AND: Delete buttons should be present
    const deleteButtons = screen.getAllByRole('button', { name: /delete/i })
    expect(deleteButtons).toHaveLength(3)
  })

  /**
   * [P1] Should navigate to /admin/benchmarks/new when clicking "Add New Benchmark"
   * Story 2.9 - Task 1.5: Add "Add New Benchmark" button
   */
  test('[P1] should navigate to create page when clicking Add New Benchmark button', async () => {
    // GIVEN: AdminBenchmarksPage
    const user = userEvent.setup()
    renderPage()

    // WHEN: Clicking "Add New Benchmark" button
    const addButton = screen.getByRole('button', { name: /add new benchmark/i })
    await user.click(addButton)

    // THEN: Should navigate to /admin/benchmarks/new
    expect(mockNavigate).toHaveBeenCalledWith('/admin/benchmarks/new')
  })

  /**
   * [P1] Should navigate to edit page when clicking Edit button
   */
  test('[P1] should navigate to edit page when clicking Edit button', async () => {
    // GIVEN: AdminBenchmarksPage
    const user = userEvent.setup()
    renderPage()

    // WHEN: Clicking first Edit button (MMLU benchmark)
    const editButtons = screen.getAllByRole('button', { name: /edit/i })
    await user.click(editButtons[0])

    // THEN: Should navigate to /admin/benchmarks/:id/edit
    expect(mockNavigate).toHaveBeenCalledWith('/admin/benchmarks/1/edit')
  })

  /**
   * [P1] Should open confirmation dialog when clicking Delete button
   * Story 2.9 - Task 7.1: Delete button with confirmation dialog
   */
  test('[P1] should open confirmation dialog when clicking Delete button', async () => {
    // GIVEN: AdminBenchmarksPage
    const user = userEvent.setup()
    renderPage()

    // WHEN: Clicking first Delete button
    const deleteButtons = screen.getAllByRole('button', { name: /delete/i })
    await user.click(deleteButtons[0])

    // THEN: Confirmation dialog should be visible
    await waitFor(() => {
      expect(screen.getByTestId('confirm-dialog')).toBeInTheDocument()
    })
    expect(screen.getByText(/delete benchmark/i)).toBeInTheDocument()
    expect(screen.getByText(/MMLU/)).toBeInTheDocument()
  })

  /**
   * [P1] Should close confirmation dialog when clicking Cancel
   */
  test('[P1] should close dialog when clicking Cancel in confirmation', async () => {
    // GIVEN: AdminBenchmarksPage with confirmation dialog open
    const user = userEvent.setup()
    renderPage()

    const deleteButtons = screen.getAllByRole('button', { name: /delete/i })
    await user.click(deleteButtons[0])

    await waitFor(() => {
      expect(screen.getByTestId('confirm-dialog')).toBeInTheDocument()
    })

    // WHEN: Clicking Cancel button
    const cancelButton = screen.getByRole('button', { name: /cancel/i })
    await user.click(cancelButton)

    // THEN: Dialog should close
    await waitFor(() => {
      expect(screen.queryByTestId('confirm-dialog')).not.toBeInTheDocument()
    })
  })

  /**
   * [P1] Should visually distinguish inactive benchmarks
   */
  test('[P1] should apply visual styling to inactive benchmarks', () => {
    // GIVEN: AdminBenchmarksPage with inactive benchmark
    renderPage()

    // WHEN: Checking the inactive benchmark row
    const inactiveBenchmarkRow = screen.getByText('OldBenchmark').closest('tr')

    // THEN: Row should have opacity styling
    expect(inactiveBenchmarkRow).toHaveClass('opacity-60')
    expect(inactiveBenchmarkRow).toHaveClass('bg-gray-50')
  })

  /**
   * [P2] Should display "No benchmarks found" when list is empty
   */
  test('[P2] should display empty state when no benchmarks exist', async () => {
    // GIVEN: AdminBenchmarksPage with no benchmarks
    const { useBenchmarks } = await import('@/hooks/useBenchmarks')
    vi.mocked(useBenchmarks).mockReturnValue({
      data: [],
      isLoading: false,
      error: null,
      refetch: vi.fn(),
    } as any)

    renderPage()

    // THEN: Empty state message should be displayed
    expect(screen.getByText(/no benchmarks found/i)).toBeInTheDocument()
    expect(screen.getByText(/create your first benchmark/i)).toBeInTheDocument()
  })

  /**
   * [P2] Should display loading state
   */
  test('[P2] should display loading spinner while fetching benchmarks', async () => {
    // GIVEN: AdminBenchmarksPage in loading state
    const { useBenchmarks } = await import('@/hooks/useBenchmarks')
    vi.mocked(useBenchmarks).mockReturnValue({
      data: undefined,
      isLoading: true,
      error: null,
      refetch: vi.fn(),
    } as any)

    renderPage()

    // THEN: Loading spinner should be visible
    expect(screen.getByRole('status')).toBeInTheDocument()
    expect(screen.getByText(/loading/i)).toBeInTheDocument()
  })

  /**
   * [P2] Should display error state with retry button
   */
  test('[P2] should display error message with retry button on fetch error', async () => {
    // GIVEN: AdminBenchmarksPage with error
    const { useBenchmarks } = await import('@/hooks/useBenchmarks')
    const mockRefetch = vi.fn()
    vi.mocked(useBenchmarks).mockReturnValue({
      data: undefined,
      isLoading: false,
      error: new Error('Failed to fetch benchmarks'),
      refetch: mockRefetch,
    } as any)

    renderPage()

    // THEN: Error message and retry button should be visible
    expect(screen.getByText(/error loading benchmarks/i)).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /try again/i })).toBeInTheDocument()
  })
})
