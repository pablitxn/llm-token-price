/**
 * Component tests for BenchmarkForm
 * Story 2.9 - Task 10.1-10.2: Test form validation and rendering
 * Priority: P1 (High - form component testing)
 */

import { describe, test, expect, vi, beforeEach } from 'vitest'
import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { BrowserRouter } from 'react-router-dom'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { BenchmarkForm } from './BenchmarkForm'
import type { BenchmarkResponseDto } from '@/schemas/benchmarkSchema'

// Mock TanStack Query hooks
vi.mock('@/hooks/useBenchmarks', () => ({
  useCreateBenchmark: () => ({
    mutate: vi.fn(),
    isPending: false,
    error: null,
  }),
  useUpdateBenchmark: () => ({
    mutate: vi.fn(),
    isPending: false,
    error: null,
  }),
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

describe('BenchmarkForm Component', () => {
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

  const renderForm = (props: React.ComponentProps<typeof BenchmarkForm>) => {
    return render(
      <QueryClientProvider client={queryClient}>
        <BrowserRouter>
          <BenchmarkForm {...props} />
        </BrowserRouter>
      </QueryClientProvider>
    )
  }

  /**
   * [P1] Should render all required fields in create mode
   * Story 2.9 AC#3: Form includes all required fields
   */
  test('[P1] should render all required fields in create mode', () => {
    // GIVEN: BenchmarkForm in create mode
    // WHEN: Rendering the form
    renderForm({ mode: 'create' })

    // THEN: All required fields should be visible
    expect(screen.getByLabelText(/benchmark name/i)).toBeInTheDocument()
    expect(screen.getByLabelText(/full name/i)).toBeInTheDocument()
    expect(screen.getByLabelText(/description/i)).toBeInTheDocument()
    expect(screen.getByLabelText(/category/i)).toBeInTheDocument()
    expect(screen.getByLabelText(/interpretation/i)).toBeInTheDocument()
    expect(screen.getByLabelText(/typical range min/i)).toBeInTheDocument()
    expect(screen.getByLabelText(/typical range max/i)).toBeInTheDocument()
    expect(screen.getByLabelText(/weight in qaps/i)).toBeInTheDocument()

    // Submit button should be present
    expect(screen.getByRole('button', { name: /create benchmark/i })).toBeInTheDocument()
  })

  /**
   * [P1] Should disable benchmarkName field in edit mode
   * Story 2.9 - Task 6.6: Prevent changing benchmark_name (immutable identifier)
   */
  test('[P1] should disable benchmarkName field in edit mode', () => {
    // GIVEN: BenchmarkForm in edit mode with benchmark data
    const mockBenchmark: BenchmarkResponseDto = {
      id: '123e4567-e89b-12d3-a456-426614174000',
      benchmarkName: 'MMLU',
      fullName: 'Massive Multitask Language Understanding',
      description: 'Test description',
      category: 'Reasoning',
      interpretation: 'HigherBetter',
      typicalRangeMin: 0,
      typicalRangeMax: 100,
      weightInQaps: 0.30,
      createdAt: '2024-01-01T00:00:00Z',
      isActive: true,
    }

    // WHEN: Rendering form in edit mode
    renderForm({
      mode: 'edit',
      benchmarkId: mockBenchmark.id,
      benchmark: mockBenchmark,
    })

    // THEN: BenchmarkName field should be disabled
    const benchmarkNameInput = screen.getByDisplayValue('MMLU')
    expect(benchmarkNameInput).toBeDisabled()
    expect(screen.getByText(/cannot be changed/i)).toBeInTheDocument()
  })

  /**
   * [P1] Should display validation errors for required fields
   * Story 2.9 - Task 10.2: Test form validation (required fields)
   */
  test('[P1] should display validation errors for required fields when submitted empty', async () => {
    // GIVEN: BenchmarkForm in create mode with empty fields
    const user = userEvent.setup()
    renderForm({ mode: 'create' })

    // WHEN: Submitting form without filling required fields
    const submitButton = screen.getByRole('button', { name: /create benchmark/i })

    // Clear default values first
    const benchmarkNameInput = screen.getByLabelText(/benchmark name/i)
    await user.clear(benchmarkNameInput)

    await user.click(submitButton)

    // THEN: Validation errors should be displayed
    await waitFor(() => {
      expect(screen.getByText(/benchmark name is required/i)).toBeInTheDocument()
    })
  })

  /**
   * [P2] Should validate benchmarkName format (alphanumeric + underscore only)
   * Story 2.9 - Task 3.2: Validate benchmark_name format
   */
  test('[P2] should reject invalid characters in benchmarkName', async () => {
    // GIVEN: BenchmarkForm in create mode
    const user = userEvent.setup()
    renderForm({ mode: 'create' })

    // WHEN: Entering benchmarkName with invalid characters
    const benchmarkNameInput = screen.getByLabelText(/benchmark name/i)
    await user.type(benchmarkNameInput, 'Invalid-Name!')

    // Fill other required fields
    await user.type(screen.getByLabelText(/full name/i), 'Full Name')

    const submitButton = screen.getByRole('button', { name: /create benchmark/i })
    await user.click(submitButton)

    // THEN: Should display validation error
    await waitFor(() => {
      expect(screen.getByText(/can only contain letters, numbers, and underscores/i)).toBeInTheDocument()
    })
  })

  /**
   * [P2] Should validate typical range (min < max)
   * Story 2.9 - Task 10.2: Test range validation
   */
  test('[P2] should validate typical range min < max', async () => {
    // GIVEN: BenchmarkForm with min > max
    const user = userEvent.setup()
    renderForm({ mode: 'create' })

    // WHEN: Entering min greater than max
    const minInput = screen.getByLabelText(/typical range min/i)
    const maxInput = screen.getByLabelText(/typical range max/i)

    await user.clear(minInput)
    await user.type(minInput, '100')

    await user.clear(maxInput)
    await user.type(maxInput, '0')

    // Fill other required fields
    await user.type(screen.getByLabelText(/benchmark name/i), 'TestBenchmark')
    await user.type(screen.getByLabelText(/full name/i), 'Full Name')

    const submitButton = screen.getByRole('button', { name: /create benchmark/i })
    await user.click(submitButton)

    // THEN: Should display validation error
    await waitFor(() => {
      expect(screen.getByText(/minimum must be less than maximum/i)).toBeInTheDocument()
    })
  })

  /**
   * [P2] Should validate weight in QAPS (0-1, max 2 decimals)
   * Story 2.9 - Task 10.2: Test QAPS weight validation
   */
  test('[P2] should validate weight in QAPS range (0-1)', async () => {
    // GIVEN: BenchmarkForm with weight > 1
    const user = userEvent.setup()
    renderForm({ mode: 'create' })

    // WHEN: Entering weight > 1.0
    const weightInput = screen.getByLabelText(/weight in qaps/i)
    await user.clear(weightInput)
    await user.type(weightInput, '1.5')

    // Fill other required fields
    await user.type(screen.getByLabelText(/benchmark name/i), 'TestBenchmark')
    await user.type(screen.getByLabelText(/full name/i), 'Full Name')

    const submitButton = screen.getByRole('button', { name: /create benchmark/i })
    await user.click(submitButton)

    // THEN: Should display validation error
    await waitFor(() => {
      expect(screen.getByText(/weight must be between 0.00 and 1.00/i)).toBeInTheDocument()
    })
  })

  /**
   * [P1] Should pre-populate form fields in edit mode
   */
  test('[P1] should pre-populate form fields with benchmark data in edit mode', () => {
    // GIVEN: Benchmark data for editing
    const mockBenchmark: BenchmarkResponseDto = {
      id: '123e4567-e89b-12d3-a456-426614174000',
      benchmarkName: 'HumanEval',
      fullName: 'Human Eval Coding Benchmark',
      description: 'Evaluates coding abilities',
      category: 'Code',
      interpretation: 'HigherBetter',
      typicalRangeMin: 0,
      typicalRangeMax: 100,
      weightInQaps: 0.25,
      createdAt: '2024-01-01T00:00:00Z',
      isActive: true,
    }

    // WHEN: Rendering form in edit mode
    renderForm({
      mode: 'edit',
      benchmarkId: mockBenchmark.id,
      benchmark: mockBenchmark,
    })

    // THEN: All fields should be pre-populated
    expect(screen.getByDisplayValue('HumanEval')).toBeInTheDocument()
    expect(screen.getByDisplayValue('Human Eval Coding Benchmark')).toBeInTheDocument()
    expect(screen.getByDisplayValue('Evaluates coding abilities')).toBeInTheDocument()
    expect(screen.getByDisplayValue('Code')).toBeInTheDocument()
    expect(screen.getByDisplayValue('HigherBetter')).toBeInTheDocument()
    expect(screen.getByDisplayValue('0')).toBeInTheDocument()
    expect(screen.getByDisplayValue('100')).toBeInTheDocument()
    expect(screen.getByDisplayValue('0.25')).toBeInTheDocument()
  })

  /**
   * [P1] Should have all category options available
   */
  test('[P1] should render all 5 category options', () => {
    // GIVEN: BenchmarkForm in create mode
    renderForm({ mode: 'create' })

    // WHEN: Finding category select
    const categorySelect = screen.getByLabelText(/category/i)

    // THEN: All 5 categories should be options
    expect(categorySelect).toBeInTheDocument()
    const options = screen.getAllByRole('option')
    const categoryOptions = options.filter(option =>
      ['Reasoning', 'Code', 'Math', 'Language', 'Multimodal'].includes(option.textContent || '')
    )
    expect(categoryOptions).toHaveLength(5)
  })

  /**
   * [P1] Should have both interpretation options available
   */
  test('[P1] should render both interpretation options', () => {
    // GIVEN: BenchmarkForm in create mode
    renderForm({ mode: 'create' })

    // WHEN: Finding interpretation select
    const interpretationSelect = screen.getByLabelText(/interpretation/i)

    // THEN: Both options should be available
    expect(interpretationSelect).toBeInTheDocument()
    expect(screen.getByText(/higher is better/i)).toBeInTheDocument()
    expect(screen.getByText(/lower is better/i)).toBeInTheDocument()
  })

  /**
   * [P1] Should show "Save Changes" button in edit mode
   */
  test('[P1] should show correct button text for edit mode', () => {
    // GIVEN: BenchmarkForm in edit mode
    const mockBenchmark: BenchmarkResponseDto = {
      id: '123',
      benchmarkName: 'Test',
      fullName: 'Test',
      category: 'Reasoning',
      interpretation: 'HigherBetter',
      typicalRangeMin: 0,
      typicalRangeMax: 100,
      weightInQaps: 0.30,
      createdAt: '2024-01-01T00:00:00Z',
      isActive: true,
    }

    // WHEN: Rendering in edit mode
    renderForm({
      mode: 'edit',
      benchmarkId: mockBenchmark.id,
      benchmark: mockBenchmark,
    })

    // THEN: Button should say "Save Changes"
    expect(screen.getByRole('button', { name: /save changes/i })).toBeInTheDocument()
    expect(screen.queryByRole('button', { name: /create benchmark/i })).not.toBeInTheDocument()
  })

  /**
   * [P2] Should provide helper text for weight in QAPS field
   */
  test('[P2] should show helper text for QAPS weight field', () => {
    // GIVEN: BenchmarkForm in create mode
    renderForm({ mode: 'create' })

    // WHEN: Checking for helper text
    // THEN: Helper text should explain QAPS weight range
    expect(screen.getByText(/0.00 to 1.00/i)).toBeInTheDocument()
  })

  /**
   * [P1] Should have Cancel and Reset Form buttons
   */
  test('[P1] should render Cancel and Reset Form buttons', () => {
    // GIVEN: BenchmarkForm in create mode
    renderForm({ mode: 'create' })

    // WHEN: Checking for action buttons
    // THEN: Cancel and Reset buttons should be present
    expect(screen.getByRole('button', { name: /cancel/i })).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /reset form/i })).toBeInTheDocument()
  })
})
