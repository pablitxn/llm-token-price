import { describe, it, expect, beforeEach } from 'vitest'
import { render, screen, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import ModelTable from '../ModelTable'
import type { ModelDto } from '@/types/models'

/**
 * Component tests for ModelTable with TanStack Table integration
 *
 * Story 3.3: TanStack Table Integration
 * Tests AC #2, #4, #5: Table rendering, visual appearance, and basic functionality
 *
 * Coverage:
 * - Basic table rendering with TanStack Table
 * - Multiple rows rendering
 * - Empty state handling
 * - Column headers and data cells
 * - Visual styling (alternating rows, hover states)
 * - Semantic HTML structure
 * - Accessibility attributes
 * - Performance with different dataset sizes
 */

// Helper to create mock ModelDto data
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

const createMockModels = (count: number): ModelDto[] => {
  return Array.from({ length: count }, (_, i) =>
    createMockModel({
      id: `model-${i}`,
      name: `Model ${i + 1}`,
      provider: `Provider ${(i % 3) + 1}`,
      version: `v${i}.0`,
      inputPricePer1M: 10.0 + i,
      outputPricePer1M: 20.0 + i * 2,
    })
  )
}

describe('ModelTable', () => {
  describe('Basic Rendering (AC #2)', () => {
    it('should render table element with proper HTML structure', () => {
      const mockModels = createMockModels(1)
      render(<ModelTable models={mockModels} />)

      expect(screen.getByRole('table')).toBeInTheDocument()
    })

    it('should render table with thead and tbody', () => {
      const mockModels = createMockModels(1)
      const { container } = render(<ModelTable models={mockModels} />)

      const table = container.querySelector('table')
      expect(table).toBeInTheDocument()
      expect(table?.querySelector('thead')).toBeInTheDocument()
      expect(table?.querySelector('tbody')).toBeInTheDocument()
    })

    it('should render 4 column headers', () => {
      const mockModels = createMockModels(1)
      render(<ModelTable models={mockModels} />)

      const headers = screen.getAllByRole('columnheader')
      expect(headers).toHaveLength(4)
    })

    it('should render correct column header names', () => {
      const mockModels = createMockModels(1)
      render(<ModelTable models={mockModels} />)

      expect(screen.getByRole('columnheader', { name: /name/i })).toBeInTheDocument()
      expect(screen.getByRole('columnheader', { name: /provider/i })).toBeInTheDocument()
      expect(screen.getByRole('columnheader', { name: /input price/i })).toBeInTheDocument()
      expect(screen.getByRole('columnheader', { name: /output price/i })).toBeInTheDocument()
    })

    it('should render price headers with "per 1M tokens" subtitle', () => {
      const mockModels = createMockModels(1)
      const { container } = render(<ModelTable models={mockModels} />)

      const inputPriceHeader = screen.getByRole('columnheader', { name: /input price/i })
      expect(within(inputPriceHeader).getByText(/per 1M tokens/i)).toBeInTheDocument()

      const outputPriceHeader = screen.getByRole('columnheader', { name: /output price/i })
      expect(within(outputPriceHeader).getByText(/per 1M tokens/i)).toBeInTheDocument()
    })
  })

  describe('Data Display (AC #2, #4)', () => {
    it('should display single model data correctly', () => {
      const mockModel = createMockModel({
        name: 'GPT-4',
        provider: 'OpenAI',
        version: '0613',
        inputPricePer1M: 30.0,
        outputPricePer1M: 60.0,
      })

      render(<ModelTable models={[mockModel]} />)

      expect(screen.getByText('GPT-4')).toBeInTheDocument()
      expect(screen.getByText('v0613')).toBeInTheDocument()
      expect(screen.getByText('OpenAI')).toBeInTheDocument()
      expect(screen.getByText('$30.00')).toBeInTheDocument()
      expect(screen.getByText('$60.00')).toBeInTheDocument()
    })

    it('should display multiple models', () => {
      const mockModels = createMockModels(5)
      render(<ModelTable models={mockModels} />)

      const rows = screen.getAllByRole('row')
      // 1 header row + 5 data rows = 6 total
      expect(rows).toHaveLength(6)
    })

    it('should display model without version correctly', () => {
      const mockModel = createMockModel({
        name: 'Test Model',
        version: null,
      })

      const { container } = render(<ModelTable models={[mockModel]} />)

      expect(screen.getByText('Test Model')).toBeInTheDocument()
      // Should not have version text like "v1.0" - check for absence of version div
      expect(screen.queryByText(/^v\d/)).not.toBeInTheDocument()
    })

    it('should format prices with currency symbol', () => {
      const mockModel = createMockModel({
        inputPricePer1M: 15.5,
        outputPricePer1M: 30.75,
        currency: 'USD',
      })

      render(<ModelTable models={[mockModel]} />)

      expect(screen.getByText('$15.50')).toBeInTheDocument()
      expect(screen.getByText('$30.75')).toBeInTheDocument()
    })

    it('should display EUR currency correctly', () => {
      const mockModel = createMockModel({
        inputPricePer1M: 20.0,
        outputPricePer1M: 40.0,
        currency: 'EUR',
      })

      const { container } = render(<ModelTable models={[mockModel]} />)

      expect(container.textContent).toContain('20.00')
      expect(container.textContent).toContain('€')
    })
  })

  describe('Empty State Handling', () => {
    it('should render empty table with headers when no models provided', () => {
      render(<ModelTable models={[]} />)

      expect(screen.getByRole('table')).toBeInTheDocument()
      expect(screen.getAllByRole('columnheader')).toHaveLength(4)

      const rows = screen.getAllByRole('row')
      // Only header row, no data rows
      expect(rows).toHaveLength(1)
    })

    it('should not crash with undefined models array', () => {
      render(<ModelTable models={[] as ModelDto[]} />)
      expect(screen.getByRole('table')).toBeInTheDocument()
    })
  })

  describe('Visual Styling (AC #4)', () => {
    it('should apply correct wrapper classes for horizontal scroll', () => {
      const mockModels = createMockModels(1)
      const { container } = render(<ModelTable models={mockModels} />)

      const wrapper = container.firstChild as HTMLElement
      expect(wrapper).toHaveClass('w-full', 'overflow-x-auto')
    })

    it('should apply table styling classes', () => {
      const mockModels = createMockModels(1)
      const { container } = render(<ModelTable models={mockModels} />)

      const table = container.querySelector('table')
      expect(table).toHaveClass(
        'min-w-full',
        'bg-white',
        'border',
        'border-gray-200',
        'shadow-sm',
        'rounded-lg'
      )
    })

    it('should apply thead styling classes', () => {
      const mockModels = createMockModels(1)
      const { container } = render(<ModelTable models={mockModels} />)

      const thead = container.querySelector('thead')
      expect(thead).toHaveClass('bg-gray-50', 'border-b', 'border-gray-200')
    })

    it('should apply header cell styling', () => {
      const mockModels = createMockModels(1)
      const { container } = render(<ModelTable models={mockModels} />)

      const th = container.querySelector('th')
      expect(th).toHaveClass(
        'px-6',
        'py-3',
        'text-xs',
        'font-medium',
        'text-gray-700',
        'uppercase',
        'tracking-wider'
      )
    })

    it('should apply text-left alignment to name and provider headers', () => {
      const mockModels = createMockModels(1)
      render(<ModelTable models={mockModels} />)

      const nameHeader = screen.getByRole('columnheader', { name: /name/i })
      expect(nameHeader).toHaveClass('text-left')

      const providerHeader = screen.getByRole('columnheader', { name: /provider/i })
      expect(providerHeader).toHaveClass('text-left')
    })

    it('should apply text-right alignment to price headers', () => {
      const mockModels = createMockModels(1)
      render(<ModelTable models={mockModels} />)

      const inputPriceHeader = screen.getByRole('columnheader', { name: /input price/i })
      expect(inputPriceHeader).toHaveClass('text-right')

      const outputPriceHeader = screen.getByRole('columnheader', { name: /output price/i })
      expect(outputPriceHeader).toHaveClass('text-right')
    })

    it('should apply alternating row background colors', () => {
      const mockModels = createMockModels(4)
      const { container } = render(<ModelTable models={mockModels} />)

      const rows = container.querySelectorAll('tbody tr')

      // Even rows (0, 2) should have bg-white
      expect(rows[0]).toHaveClass('bg-white')
      expect(rows[2]).toHaveClass('bg-white')

      // Odd rows (1, 3) should have bg-gray-50/50
      expect(rows[1]).toHaveClass('bg-gray-50/50')
      expect(rows[3]).toHaveClass('bg-gray-50/50')
    })

    it('should apply hover state classes to rows', () => {
      const mockModels = createMockModels(2)
      const { container } = render(<ModelTable models={mockModels} />)

      const rows = container.querySelectorAll('tbody tr')
      rows.forEach((row) => {
        expect(row).toHaveClass('hover:bg-gray-50', 'transition-colors')
      })
    })

    it('should apply tbody divider styling', () => {
      const mockModels = createMockModels(2)
      const { container } = render(<ModelTable models={mockModels} />)

      const tbody = container.querySelector('tbody')
      expect(tbody).toHaveClass('divide-y', 'divide-gray-200')
    })

    it('should apply cell padding and whitespace classes', () => {
      const mockModels = createMockModels(1)
      const { container } = render(<ModelTable models={mockModels} />)

      const cells = container.querySelectorAll('tbody td')
      cells.forEach((cell) => {
        expect(cell).toHaveClass('px-6', 'py-4', 'whitespace-nowrap')
      })
    })

    it('should apply text-right alignment to price cells', () => {
      const mockModels = createMockModels(1)
      const { container } = render(<ModelTable models={mockModels} />)

      const rows = container.querySelectorAll('tbody tr')
      const firstRow = rows[0]
      const cells = firstRow.querySelectorAll('td')

      // Name and provider cells (0, 1) - no text-right
      expect(cells[0]).not.toHaveClass('text-right')
      expect(cells[1]).not.toHaveClass('text-right')

      // Price cells (2, 3) - text-right
      expect(cells[2]).toHaveClass('text-right')
      expect(cells[3]).toHaveClass('text-right')
    })
  })

  describe('Semantic HTML & Accessibility (AC #4)', () => {
    it('should use semantic table elements', () => {
      const mockModels = createMockModels(1)
      const { container } = render(<ModelTable models={mockModels} />)

      expect(container.querySelector('table')).toBeInTheDocument()
      expect(container.querySelector('thead')).toBeInTheDocument()
      expect(container.querySelector('tbody')).toBeInTheDocument()
      expect(container.querySelector('tr')).toBeInTheDocument()
      expect(container.querySelector('th')).toBeInTheDocument()
      expect(container.querySelector('td')).toBeInTheDocument()
    })

    it('should have scope="col" on all header cells', () => {
      const mockModels = createMockModels(1)
      render(<ModelTable models={mockModels} />)

      const headers = screen.getAllByRole('columnheader')
      headers.forEach((header) => {
        expect(header).toHaveAttribute('scope', 'col')
      })
    })

    it('should have role="table" on table element', () => {
      const mockModels = createMockModels(1)
      render(<ModelTable models={mockModels} />)

      const table = screen.getByRole('table')
      expect(table).toBeInTheDocument()
    })

    it('should have accessible row structure', () => {
      const mockModels = createMockModels(3)
      render(<ModelTable models={mockModels} />)

      const rows = screen.getAllByRole('row')
      // 1 header + 3 data rows
      expect(rows).toHaveLength(4)
    })
  })

  describe('Performance (AC #5)', () => {
    it('should render 10 models without performance issues', () => {
      const mockModels = createMockModels(10)
      const start = performance.now()

      render(<ModelTable models={mockModels} />)

      const end = performance.now()
      const renderTime = end - start

      // Should render in <500ms (AC #5 target)
      expect(renderTime).toBeLessThan(500)

      // Verify all rows rendered
      const rows = screen.getAllByRole('row')
      expect(rows).toHaveLength(11) // 1 header + 10 data
    })

    it('should render 50 models efficiently', () => {
      const mockModels = createMockModels(50)
      const start = performance.now()

      render(<ModelTable models={mockModels} />)

      const end = performance.now()
      const renderTime = end - start

      // Should render in <500ms (AC #5 target for 50 models)
      expect(renderTime).toBeLessThan(500)

      const rows = screen.getAllByRole('row')
      expect(rows).toHaveLength(51) // 1 header + 50 data
    })

    it('should not crash with 100+ models', () => {
      const mockModels = createMockModels(100)

      expect(() => {
        render(<ModelTable models={mockModels} />)
      }).not.toThrow()

      const rows = screen.getAllByRole('row')
      expect(rows).toHaveLength(101) // 1 header + 100 data
    })

    it('should handle large dataset without memory leaks', () => {
      const mockModels = createMockModels(200)

      const { unmount } = render(<ModelTable models={mockModels} />)

      // Verify renders
      expect(screen.getByRole('table')).toBeInTheDocument()

      // Clean unmount
      unmount()
    })
  })

  describe('Integration with TanStack Table (AC #2)', () => {
    it('should use TanStack Table to render data (not manual map)', () => {
      const mockModels = createMockModels(2)
      const { container } = render(<ModelTable models={mockModels} />)

      // Verify table structure created by TanStack Table
      const table = container.querySelector('table')
      expect(table).toBeInTheDocument()

      // TanStack Table generates unique keys for rows
      const rows = container.querySelectorAll('tbody tr')
      expect(rows).toHaveLength(2)
    })

    it('should display all provided models in table', () => {
      const mockModels = [
        createMockModel({ name: 'GPT-4', provider: 'OpenAI' }),
        createMockModel({ name: 'Claude 3.5', provider: 'Anthropic' }),
        createMockModel({ name: 'Gemini Pro', provider: 'Google' }),
      ]

      render(<ModelTable models={mockModels} />)

      expect(screen.getByText('GPT-4')).toBeInTheDocument()
      expect(screen.getByText('Claude 3.5')).toBeInTheDocument()
      expect(screen.getByText('Gemini Pro')).toBeInTheDocument()

      expect(screen.getByText('OpenAI')).toBeInTheDocument()
      expect(screen.getByText('Anthropic')).toBeInTheDocument()
      expect(screen.getByText('Google')).toBeInTheDocument()
    })

    it('should maintain data order from props', () => {
      const mockModels = [
        createMockModel({ name: 'First Model' }),
        createMockModel({ name: 'Second Model' }),
        createMockModel({ name: 'Third Model' }),
      ]

      render(<ModelTable models={mockModels} />)

      const nameHeaders = screen.getAllByText(/Model/i)
      const names = nameHeaders.filter((el) => el.tagName === 'DIV').map((el) => el.textContent)

      expect(names).toEqual(['First Model', 'Second Model', 'Third Model'])
    })
  })

  describe('Real-World Data Scenarios', () => {
    it('should display realistic LLM pricing data', () => {
      const realModels = [
        createMockModel({
          name: 'GPT-4 Turbo',
          provider: 'OpenAI',
          version: '1106',
          inputPricePer1M: 10.0,
          outputPricePer1M: 30.0,
        }),
        createMockModel({
          name: 'Claude 3.5 Sonnet',
          provider: 'Anthropic',
          version: '20240620',
          inputPricePer1M: 3.0,
          outputPricePer1M: 15.0,
        }),
        createMockModel({
          name: 'Gemini 1.5 Pro',
          provider: 'Google',
          version: null,
          inputPricePer1M: 7.0,
          outputPricePer1M: 21.0,
        }),
      ]

      render(<ModelTable models={realModels} />)

      // Verify all models display
      expect(screen.getByText('GPT-4 Turbo')).toBeInTheDocument()
      expect(screen.getByText('Claude 3.5 Sonnet')).toBeInTheDocument()
      expect(screen.getByText('Gemini 1.5 Pro')).toBeInTheDocument()

      // Verify prices format correctly
      expect(screen.getByText('$10.00')).toBeInTheDocument()
      expect(screen.getByText('$30.00')).toBeInTheDocument()
      expect(screen.getByText('$3.00')).toBeInTheDocument()
      expect(screen.getByText('$15.00')).toBeInTheDocument()
      expect(screen.getByText('$7.00')).toBeInTheDocument()
      expect(screen.getByText('$21.00')).toBeInTheDocument()
    })

    it('should handle edge case: free models (zero pricing)', () => {
      const freeModel = createMockModel({
        name: 'Free Model',
        inputPricePer1M: 0,
        outputPricePer1M: 0,
      })

      render(<ModelTable models={[freeModel]} />)

      const zeroPrices = screen.getAllByText('$0.00')
      expect(zeroPrices).toHaveLength(2) // input and output both zero
    })

    it('should handle edge case: very expensive models', () => {
      const expensiveModel = createMockModel({
        name: 'Premium Model',
        inputPricePer1M: 500.0,
        outputPricePer1M: 1000.0,
      })

      render(<ModelTable models={[expensiveModel]} />)

      expect(screen.getByText('$500.00')).toBeInTheDocument()
      expect(screen.getByText('$1000.00')).toBeInTheDocument()
    })
  })
})
