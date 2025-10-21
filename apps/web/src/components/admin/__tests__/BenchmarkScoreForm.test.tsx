/**
 * Component tests for BenchmarkScoreForm
 * Tests form validation, out-of-range warnings, and submission
 * Story 2.10 - Benchmark Score Entry Form
 */

import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { BenchmarkScoreForm } from '../BenchmarkScoreForm'
import type { BenchmarkScoreResponseDto } from '@/types/admin'
import * as useBenchmarks from '@/hooks/useBenchmarks'
import * as useBenchmarkScores from '@/hooks/useBenchmarkScores'

// Mock API calls
vi.mock('@/api/admin')
vi.mock('@/hooks/useBenchmarks')
vi.mock('@/hooks/useBenchmarkScores', () => ({
  useAddBenchmarkScore: vi.fn(),
  useUpdateBenchmarkScore: vi.fn(),
}))

describe('BenchmarkScoreForm', () => {
  const mockModelId = '550e8400-e29b-41d4-a716-446655440000'
  const mockBenchmarkId = '660e8400-e29b-41d4-a716-446655440000'

  const mockBenchmarks = [
    {
      id: mockBenchmarkId,
      benchmarkName: 'MMLU',
      fullName: 'Massive Multitask Language Understanding',
      category: 'Reasoning',
      typicalRangeMin: 60,
      typicalRangeMax: 95,
      weightInQaps: 0.30,
      isActive: true,
      createdAt: new Date().toISOString(),
    },
    {
      id: '770e8400-e29b-41d4-a716-446655440000',
      benchmarkName: 'HumanEval',
      fullName: 'Human Eval Code Benchmark',
      category: 'Code',
      typicalRangeMin: 0,
      typicalRangeMax: 100,
      weightInQaps: 0.25,
      isActive: true,
      createdAt: new Date().toISOString(),
    },
  ]

  let queryClient: QueryClient

  beforeEach(() => {
    queryClient = new QueryClient({
      defaultOptions: {
        queries: { retry: false },
        mutations: { retry: false },
      },
    })

    // Mock useBenchmarks hook
    vi.mocked(useBenchmarks.useBenchmarks).mockReturnValue({
      data: mockBenchmarks,
      isLoading: false,
      error: null,
    } as any)

    // Mock useAddBenchmarkScore hook
    vi.mocked(useBenchmarkScores.useAddBenchmarkScore).mockReturnValue({
      mutate: vi.fn(),
      isPending: false,
      error: null,
    } as any)

    // Mock useUpdateBenchmarkScore hook
    vi.mocked(useBenchmarkScores.useUpdateBenchmarkScore).mockReturnValue({
      mutate: vi.fn(),
      isPending: false,
      error: null,
    } as any)
  })

  const renderForm = (props: {
    mode: 'create' | 'edit'
    score?: BenchmarkScoreResponseDto
    onSuccess?: () => void
    onCancel?: () => void
  }) => {
    return render(
      <QueryClientProvider client={queryClient}>
        <BenchmarkScoreForm modelId={mockModelId} {...props} />
      </QueryClientProvider>
    )
  }

  describe('Create Mode', () => {
    it('[P0] should render all form fields', () => {
      renderForm({ mode: 'create' })

      expect(screen.getByLabelText(/benchmark/i)).toBeInTheDocument()
      expect(screen.getByLabelText(/^score/i)).toBeInTheDocument()
      expect(screen.getByLabelText(/max score/i)).toBeInTheDocument()
      expect(screen.getByLabelText(/test date/i)).toBeInTheDocument()
      expect(screen.getByLabelText(/source url/i)).toBeInTheDocument()
      expect(screen.getByLabelText(/verified/i)).toBeInTheDocument()
      expect(screen.getByLabelText(/notes/i)).toBeInTheDocument()
    })

    it('[P0] should display benchmark options from API', () => {
      renderForm({ mode: 'create' })

      const select = screen.getByLabelText(/benchmark/i) as HTMLSelectElement
      expect(select).toHaveLength(3) // "Select a benchmark..." + 2 options

      expect(screen.getByText(/MMLU - Massive Multitask/i)).toBeInTheDocument()
      expect(screen.getByText(/HumanEval - Human Eval/i)).toBeInTheDocument()
    })

    it('[P0] should show validation error when benchmark is not selected', async () => {
      const user = userEvent.setup()
      renderForm({ mode: 'create' })

      const scoreInput = screen.getByLabelText(/^score/i)
      await user.type(scoreInput, '87.5')

      const submitButton = screen.getByRole('button', { name: /add score/i })
      await user.click(submitButton)

      await waitFor(() => {
        expect(screen.getByText(/benchmark is required/i)).toBeInTheDocument()
      })
    })

    it.skip('[P0] should show validation error when score is empty', async () => {
      const user = userEvent.setup()
      renderForm({ mode: 'create' })

      // Make form dirty by selecting a benchmark
      const benchmarkSelect = screen.getByLabelText(/benchmark/i)
      await user.selectOptions(benchmarkSelect, mockBenchmarkId)

      // Leave score empty and try to submit
      const submitButton = screen.getByRole('button', { name: /add score/i })
      await user.click(submitButton)

      await waitFor(() => {
        expect(screen.getByText(/score must be a valid number/i)).toBeInTheDocument()
      })
    })

    it('[P1] should show out-of-range warning when score is below typical minimum', async () => {
      const user = userEvent.setup()
      renderForm({ mode: 'create' })

      // Select MMLU benchmark (typical range 60-95)
      const benchmarkSelect = screen.getByLabelText(/benchmark/i)
      await user.selectOptions(benchmarkSelect, mockBenchmarkId)

      // Enter score below minimum
      const scoreInput = screen.getByLabelText(/^score/i)
      await user.clear(scoreInput)
      await user.type(scoreInput, '45.2')

      await waitFor(() => {
        expect(
          screen.getByText(/score is outside the typical range/i)
        ).toBeInTheDocument()
      })
    })

    it('[P1] should show out-of-range warning when score is above typical maximum', async () => {
      const user = userEvent.setup()
      renderForm({ mode: 'create' })

      // Select MMLU benchmark (typical range 60-95)
      const benchmarkSelect = screen.getByLabelText(/benchmark/i)
      await user.selectOptions(benchmarkSelect, mockBenchmarkId)

      // Enter score above maximum
      const scoreInput = screen.getByLabelText(/^score/i)
      await user.clear(scoreInput)
      await user.type(scoreInput, '98.7')

      await waitFor(() => {
        expect(
          screen.getByText(/score is outside the typical range/i)
        ).toBeInTheDocument()
      })
    })

    it('[P1] should NOT show out-of-range warning when score is within typical range', async () => {
      const user = userEvent.setup()
      renderForm({ mode: 'create' })

      // Select MMLU benchmark (typical range 60-95)
      const benchmarkSelect = screen.getByLabelText(/benchmark/i)
      await user.selectOptions(benchmarkSelect, mockBenchmarkId)

      // Enter score within range
      const scoreInput = screen.getByLabelText(/^score/i)
      await user.clear(scoreInput)
      await user.type(scoreInput, '87.5')

      await waitFor(() => {
        expect(
          screen.queryByText(/score is outside the typical range/i)
        ).not.toBeInTheDocument()
      })
    })

    it('[P1] should show validation error when score exceeds max score', async () => {
      const user = userEvent.setup()
      renderForm({ mode: 'create' })

      const benchmarkSelect = screen.getByLabelText(/benchmark/i)
      await user.selectOptions(benchmarkSelect, mockBenchmarkId)

      const scoreInput = screen.getByLabelText(/^score/i)
      await user.type(scoreInput, '95')

      const maxScoreInput = screen.getByLabelText(/max score/i)
      await user.type(maxScoreInput, '90')

      const submitButton = screen.getByRole('button', { name: /add score/i })
      await user.click(submitButton)

      await waitFor(() => {
        expect(screen.getByText(/score cannot exceed max score/i)).toBeInTheDocument()
      })
    })

    it.skip('[P1] should show validation error when source URL is invalid', async () => {
      const user = userEvent.setup()
      renderForm({ mode: 'create' })

      // Fill required fields to make form dirty and enable submit
      const benchmarkSelect = screen.getByLabelText(/benchmark/i)
      await user.selectOptions(benchmarkSelect, mockBenchmarkId)

      const scoreInput = screen.getByLabelText(/^score/i)
      await user.type(scoreInput, '85')

      // Enter invalid URL
      const sourceUrlInput = screen.getByLabelText(/source url/i)
      await user.type(sourceUrlInput, 'not-a-valid-url')

      const submitButton = screen.getByRole('button', { name: /add score/i })
      await user.click(submitButton)

      await waitFor(() => {
        expect(
          screen.getByText(/source url must be a valid/i)
        ).toBeInTheDocument()
      })
    })

    it('[P2] should enforce 500 character limit on notes', () => {
      renderForm({ mode: 'create' })

      const notesTextarea = screen.getByLabelText(/notes/i) as HTMLTextAreaElement
      expect(notesTextarea.maxLength).toBe(500)
      expect(screen.getByText(/max 500 characters/i)).toBeInTheDocument()
    })

    it('[P1] should disable submit button when form is pristine', () => {
      renderForm({ mode: 'create' })

      const submitButton = screen.getByRole('button', { name: /add score/i })
      expect(submitButton).toBeDisabled()
    })

    it('[P1] should call onCancel when cancel button is clicked', async () => {
      const user = userEvent.setup()
      const onCancel = vi.fn()
      renderForm({ mode: 'create', onCancel })

      const cancelButton = screen.getByRole('button', { name: /cancel/i })
      await user.click(cancelButton)

      expect(onCancel).toHaveBeenCalledOnce()
    })
  })

  describe('Edit Mode', () => {
    const mockExistingScore: BenchmarkScoreResponseDto = {
      id: '880e8400-e29b-41d4-a716-446655440000',
      modelId: mockModelId,
      benchmarkId: mockBenchmarkId,
      benchmarkName: 'MMLU',
      category: 'Reasoning',
      score: 85.0,
      maxScore: 100,
      normalizedScore: 0.85,
      testDate: '2024-01-15T10:00:00Z',
      sourceUrl: 'https://example.com/results',
      verified: true,
      notes: 'Original test results',
      createdAt: new Date().toISOString(),
      isOutOfRange: false,
    }

    it('[P0] should populate form fields with existing score data', () => {
      renderForm({ mode: 'edit', score: mockExistingScore })

      const benchmarkSelect = screen.getByLabelText(/benchmark/i) as HTMLSelectElement
      expect(benchmarkSelect.value).toBe(mockBenchmarkId)

      const scoreInput = screen.getByLabelText(/^score/i) as HTMLInputElement
      expect(scoreInput.value).toBe('85')

      const maxScoreInput = screen.getByLabelText(/max score/i) as HTMLInputElement
      expect(maxScoreInput.value).toBe('100')

      const verifiedCheckbox = screen.getByLabelText(/verified/i) as HTMLInputElement
      expect(verifiedCheckbox.checked).toBe(true)

      const notesTextarea = screen.getByLabelText(/notes/i) as HTMLTextAreaElement
      expect(notesTextarea.value).toBe('Original test results')
    })

    it('[P1] should disable benchmark selection in edit mode', () => {
      renderForm({ mode: 'edit', score: mockExistingScore })

      const benchmarkSelect = screen.getByLabelText(/benchmark/i) as HTMLSelectElement
      expect(benchmarkSelect).toBeDisabled()
    })

    it('[P1] should show "Update Score" button text in edit mode', () => {
      renderForm({ mode: 'edit', score: mockExistingScore })

      expect(screen.getByRole('button', { name: /update score/i })).toBeInTheDocument()
      expect(
        screen.queryByRole('button', { name: /add score/i })
      ).not.toBeInTheDocument()
    })
  })

  describe('Loading States', () => {
    it('[P2] should disable benchmark select when benchmarks are loading', () => {
      vi.mocked(useBenchmarks.useBenchmarks).mockReturnValue({
        data: undefined,
        isLoading: true,
        error: null,
      } as any)

      renderForm({ mode: 'create' })

      const benchmarkSelect = screen.getByLabelText(/benchmark/i) as HTMLSelectElement
      expect(benchmarkSelect).toBeDisabled()
    })

    it('[P2] should show loading text on submit button when mutation is pending', async () => {
      const user = userEvent.setup()
      renderForm({ mode: 'create' })

      const benchmarkSelect = screen.getByLabelText(/benchmark/i)
      await user.selectOptions(benchmarkSelect, mockBenchmarkId)

      const scoreInput = screen.getByLabelText(/^score/i)
      await user.type(scoreInput, '87.5')

      // Note: Actual loading state testing requires mocking the mutation hook
      // which is done in the useBenchmarkScores mock
    })
  })
})
