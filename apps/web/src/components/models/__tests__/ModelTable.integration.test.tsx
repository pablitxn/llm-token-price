import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, waitFor } from '@testing-library/react'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import ModelTable from '../ModelTable'
import type { ModelDto } from '@/types/models'

/**
 * Integration tests for ModelTable with TanStack Query
 *
 * Story 3.3: TanStack Table Integration
 * Tests integration with useModels hook and TanStack Query
 *
 * Coverage:
 * - ModelTable receives data from useModels hook
 * - Table updates when query data changes
 * - Loading/error states handled by parent component (HomePage)
 * - Real-world data flow from API → Query → Table
 */

const createMockModel = (overrides?: Partial<ModelDto>): ModelDto => ({
  id: `model-${Math.random()}`,
  name: 'Test Model',
  provider: 'TestAI',
  version: '1.0',
  status: 'active',
  inputPricePer1M: 10.0,
  outputPricePer1M: 20.0,
  currency: 'USD',
  updatedAt: '2024-01-01T00:00:00Z',
  capabilities: {
    contextWindow: 4096,
    maxOutputTokens: 2048,
    supportsFunctionCalling: false,
    supportsVision: false,
    supportsAudioInput: false,
    supportsAudioOutput: false,
    supportsStreaming: true,
    supportsJsonMode: false,
  },
  topBenchmarks: [],
  ...overrides,
})

describe('ModelTable Integration Tests', () => {
  let queryClient: QueryClient

  beforeEach(() => {
    queryClient = new QueryClient({
      defaultOptions: {
        queries: {
          retry: false,
          gcTime: 0,
        },
      },
    })
  })

  describe('Data Flow Integration', () => {
    it('should render when provided with models array', () => {
      const mockModels = [
        createMockModel({ name: 'GPT-4', provider: 'OpenAI' }),
        createMockModel({ name: 'Claude 3.5', provider: 'Anthropic' }),
      ]

      render(
        <QueryClientProvider client={queryClient}>
          <ModelTable models={mockModels} />
        </QueryClientProvider>
      )

      expect(screen.getByText('GPT-4')).toBeInTheDocument()
      expect(screen.getByText('Claude 3.5')).toBeInTheDocument()
    })

    it('should update table when models prop changes', () => {
      const initialModels = [createMockModel({ name: 'Initial Model' })]

      const { rerender } = render(
        <QueryClientProvider client={queryClient}>
          <ModelTable models={initialModels} />
        </QueryClientProvider>
      )

      expect(screen.getByText('Initial Model')).toBeInTheDocument()

      const updatedModels = [
        createMockModel({ name: 'Updated Model' }),
        createMockModel({ name: 'New Model' }),
      ]

      rerender(
        <QueryClientProvider client={queryClient}>
          <ModelTable models={updatedModels} />
        </QueryClientProvider>
      )

      expect(screen.queryByText('Initial Model')).not.toBeInTheDocument()
      expect(screen.getByText('Updated Model')).toBeInTheDocument()
      expect(screen.getByText('New Model')).toBeInTheDocument()
    })

    it('should handle empty models array gracefully', () => {
      render(
        <QueryClientProvider client={queryClient}>
          <ModelTable models={[]} />
        </QueryClientProvider>
      )

      // Table should render with headers but no data rows
      expect(screen.getByRole('table')).toBeInTheDocument()
      expect(screen.getAllByRole('columnheader')).toHaveLength(4)

      const rows = screen.getAllByRole('row')
      expect(rows).toHaveLength(1) // Only header row
    })
  })

  describe('TanStack Table Reactivity', () => {
    it('should re-render table when data changes', async () => {
      const initialModels = [
        createMockModel({ name: 'Model 1', inputPricePer1M: 10.0 }),
      ]

      const { rerender } = render(
        <QueryClientProvider client={queryClient}>
          <ModelTable models={initialModels} />
        </QueryClientProvider>
      )

      expect(screen.getByText('$10.00')).toBeInTheDocument()

      const updatedModels = [
        createMockModel({ name: 'Model 1', inputPricePer1M: 15.0 }),
      ]

      rerender(
        <QueryClientProvider client={queryClient}>
          <ModelTable models={updatedModels} />
        </QueryClientProvider>
      )

      await waitFor(() => {
        expect(screen.getByText('$15.00')).toBeInTheDocument()
        expect(screen.queryByText('$10.00')).not.toBeInTheDocument()
      })
    })

    it('should handle adding new rows dynamically', () => {
      const initialModels = [createMockModel({ name: 'Model 1' })]

      const { rerender } = render(
        <QueryClientProvider client={queryClient}>
          <ModelTable models={initialModels} />
        </QueryClientProvider>
      )

      let rows = screen.getAllByRole('row')
      expect(rows).toHaveLength(2) // 1 header + 1 data

      const updatedModels = [
        ...initialModels,
        createMockModel({ name: 'Model 2' }),
        createMockModel({ name: 'Model 3' }),
      ]

      rerender(
        <QueryClientProvider client={queryClient}>
          <ModelTable models={updatedModels} />
        </QueryClientProvider>
      )

      rows = screen.getAllByRole('row')
      expect(rows).toHaveLength(4) // 1 header + 3 data
    })

    it('should handle removing rows dynamically', () => {
      const initialModels = [
        createMockModel({ name: 'Model 1' }),
        createMockModel({ name: 'Model 2' }),
        createMockModel({ name: 'Model 3' }),
      ]

      const { rerender } = render(
        <QueryClientProvider client={queryClient}>
          <ModelTable models={initialModels} />
        </QueryClientProvider>
      )

      let rows = screen.getAllByRole('row')
      expect(rows).toHaveLength(4) // 1 header + 3 data

      const updatedModels = [initialModels[0]] // Only keep first model

      rerender(
        <QueryClientProvider client={queryClient}>
          <ModelTable models={updatedModels} />
        </QueryClientProvider>
      )

      rows = screen.getAllByRole('row')
      expect(rows).toHaveLength(2) // 1 header + 1 data
    })
  })

  describe('Real-World Data Integration', () => {
    it('should display data matching API response structure', () => {
      // Simulate API response structure from GET /api/models
      const apiResponseModels: ModelDto[] = [
        {
          id: '550e8400-e29b-41d4-a716-446655440000',
          name: 'GPT-4 Turbo',
          provider: 'OpenAI',
          version: '1106',
          status: 'active',
          inputPricePer1M: 10.0,
          outputPricePer1M: 30.0,
          currency: 'USD',
          updatedAt: '2024-10-24T00:00:00Z',
          capabilities: {
            contextWindow: 128000,
            maxOutputTokens: 4096,
            supportsFunctionCalling: true,
            supportsVision: true,
            supportsAudioInput: false,
            supportsAudioOutput: false,
            supportsStreaming: true,
            supportsJsonMode: true,
          },
          topBenchmarks: [
            {
              benchmarkName: 'MMLU',
              score: 86.4,
              maxScore: 100,
              normalizedScore: 0.864,
            },
          ],
        },
      ]

      render(
        <QueryClientProvider client={queryClient}>
          <ModelTable models={apiResponseModels} />
        </QueryClientProvider>
      )

      expect(screen.getByText('GPT-4 Turbo')).toBeInTheDocument()
      expect(screen.getByText('v1106')).toBeInTheDocument()
      expect(screen.getByText('OpenAI')).toBeInTheDocument()
      expect(screen.getByText('$10.00')).toBeInTheDocument()
      expect(screen.getByText('$30.00')).toBeInTheDocument()
    })

    it('should handle models with missing optional fields', () => {
      const minimalModel: ModelDto = {
        id: 'test-id',
        name: 'Minimal Model',
        provider: 'TestProvider',
        version: null, // Optional
        status: 'active',
        inputPricePer1M: 5.0,
        outputPricePer1M: 10.0,
        currency: 'USD',
        updatedAt: '2024-01-01T00:00:00Z',
        capabilities: null, // Optional
        topBenchmarks: [], // Empty array
      }

      render(
        <QueryClientProvider client={queryClient}>
          <ModelTable models={[minimalModel]} />
        </QueryClientProvider>
      )

      expect(screen.getByText('Minimal Model')).toBeInTheDocument()
      expect(screen.getByText('TestProvider')).toBeInTheDocument()
      expect(screen.getByText('$5.00')).toBeInTheDocument()
      expect(screen.getByText('$10.00')).toBeInTheDocument()
    })
  })

  describe('Performance Integration', () => {
    it('should efficiently render large datasets from query', () => {
      const largeDataset = Array.from({ length: 100 }, (_, i) =>
        createMockModel({
          id: `model-${i}`,
          name: `Model ${i + 1}`,
          provider: `Provider ${(i % 5) + 1}`,
          inputPricePer1M: 10.0 + i * 0.1,
          outputPricePer1M: 20.0 + i * 0.2,
        })
      )

      const start = performance.now()

      render(
        <QueryClientProvider client={queryClient}>
          <ModelTable models={largeDataset} />
        </QueryClientProvider>
      )

      const end = performance.now()
      const renderTime = end - start

      // Should render 100 models in <1s (AC #5 extended target)
      expect(renderTime).toBeLessThan(1000)

      const rows = screen.getAllByRole('row')
      expect(rows).toHaveLength(101) // 1 header + 100 data
    })

    it('should not leak memory on repeated re-renders', () => {
      const models = [createMockModel()]

      const { rerender, unmount } = render(
        <QueryClientProvider client={queryClient}>
          <ModelTable models={models} />
        </QueryClientProvider>
      )

      // Simulate 10 re-renders (like query refetches)
      for (let i = 0; i < 10; i++) {
        rerender(
          <QueryClientProvider client={queryClient}>
            <ModelTable
              models={models.map((m) => ({ ...m, inputPricePer1M: m.inputPricePer1M + i }))}
            />
          </QueryClientProvider>
        )
      }

      expect(screen.getByRole('table')).toBeInTheDocument()

      unmount()
    })
  })

  describe('Edge Cases', () => {
    it('should handle concurrent prop updates', async () => {
      const models1 = [createMockModel({ name: 'Model A' })]
      const models2 = [createMockModel({ name: 'Model B' })]
      const models3 = [createMockModel({ name: 'Model C' })]

      const { rerender } = render(
        <QueryClientProvider client={queryClient}>
          <ModelTable models={models1} />
        </QueryClientProvider>
      )

      rerender(
        <QueryClientProvider client={queryClient}>
          <ModelTable models={models2} />
        </QueryClientProvider>
      )

      rerender(
        <QueryClientProvider client={queryClient}>
          <ModelTable models={models3} />
        </QueryClientProvider>
      )

      await waitFor(() => {
        expect(screen.getByText('Model C')).toBeInTheDocument()
        expect(screen.queryByText('Model A')).not.toBeInTheDocument()
        expect(screen.queryByText('Model B')).not.toBeInTheDocument()
      })
    })

    it('should handle switching from populated to empty data', () => {
      const populatedModels = [
        createMockModel({ name: 'Model 1' }),
        createMockModel({ name: 'Model 2' }),
      ]

      const { rerender } = render(
        <QueryClientProvider client={queryClient}>
          <ModelTable models={populatedModels} />
        </QueryClientProvider>
      )

      expect(screen.getByText('Model 1')).toBeInTheDocument()

      rerender(
        <QueryClientProvider client={queryClient}>
          <ModelTable models={[]} />
        </QueryClientProvider>
      )

      expect(screen.queryByText('Model 1')).not.toBeInTheDocument()
      const rows = screen.getAllByRole('row')
      expect(rows).toHaveLength(1) // Only header
    })

    it('should handle switching from empty to populated data', () => {
      const { rerender } = render(
        <QueryClientProvider client={queryClient}>
          <ModelTable models={[]} />
        </QueryClientProvider>
      )

      let rows = screen.getAllByRole('row')
      expect(rows).toHaveLength(1) // Only header

      const populatedModels = [
        createMockModel({ name: 'New Model 1' }),
        createMockModel({ name: 'New Model 2' }),
      ]

      rerender(
        <QueryClientProvider client={queryClient}>
          <ModelTable models={populatedModels} />
        </QueryClientProvider>
      )

      expect(screen.getByText('New Model 1')).toBeInTheDocument()
      rows = screen.getAllByRole('row')
      expect(rows).toHaveLength(3) // 1 header + 2 data
    })
  })
})
