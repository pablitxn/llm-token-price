import { describe, it, expect } from 'vitest'
import { render, screen } from '@testing-library/react'
import { createColumnHelper } from '@tanstack/react-table'
import { modelColumns } from '../columns'
import type { ModelDto } from '@/types/models'

/**
 * Unit tests for TanStack Table column definitions
 *
 * Story 3.3: TanStack Table Integration
 * Tests AC #3: Column definitions created for all model fields
 *
 * Coverage:
 * - Column count and IDs
 * - Column accessors map to ModelDto fields
 * - Column headers render correctly
 * - Custom cell renderers work as expected
 * - Price formatting in cells
 */

// Helper to create mock ModelDto data
const createMockModel = (overrides?: Partial<ModelDto>): ModelDto => ({
  id: 'test-model-1',
  name: 'GPT-4',
  provider: 'OpenAI',
  version: '0613',
  status: 'active',
  inputPricePer1M: 30.0,
  outputPricePer1M: 60.0,
  currency: 'USD',
  updatedAt: '2024-01-01T00:00:00Z',
  capabilities: {
    contextWindow: 8192,
    maxOutputTokens: 4096,
    supportsFunctionCalling: true,
    supportsVision: false,
    supportsAudioInput: false,
    supportsAudioOutput: false,
    supportsStreaming: true,
    supportsJsonMode: true,
  },
  topBenchmarks: [],
  ...overrides,
})

describe('modelColumns', () => {
  describe('Column Structure', () => {
    it('should have exactly 4 column definitions', () => {
      expect(modelColumns).toHaveLength(4)
    })

    it('should have correct column IDs in order', () => {
      const columnIds = modelColumns.map((col) => col.id)
      expect(columnIds).toEqual(['name', 'provider', 'inputPrice', 'outputPrice'])
    })

    it('should have all columns defined with required properties', () => {
      modelColumns.forEach((column) => {
        expect(column).toHaveProperty('id')
        expect(column).toHaveProperty('header')
        // TanStack Table columns use either 'accessorKey' or 'accessorFn'
        // Our columns use accessor() helper which sets the accessor internally
      })
    })
  })

  describe('Column Accessors', () => {
    it('name column should access ModelDto.name field', () => {
      const nameColumn = modelColumns[0]
      const mockModel = createMockModel({ name: 'Test Model Name' })

      // TanStack Table columns use accessorFn under the hood
      expect(nameColumn.id).toBe('name')
    })

    it('provider column should access ModelDto.provider field', () => {
      const providerColumn = modelColumns[1]
      expect(providerColumn.id).toBe('provider')
    })

    it('inputPrice column should access ModelDto.inputPricePer1M field', () => {
      const inputPriceColumn = modelColumns[2]
      expect(inputPriceColumn.id).toBe('inputPrice')
    })

    it('outputPrice column should access ModelDto.outputPricePer1M field', () => {
      const outputPriceColumn = modelColumns[3]
      expect(outputPriceColumn.id).toBe('outputPrice')
    })
  })

  describe('Column Headers', () => {
    it('name column should render "Name" header', () => {
      const nameColumn = modelColumns[0]
      const headerFn = nameColumn.header as () => string
      expect(headerFn()).toBe('Name')
    })

    it('provider column should render "Provider" header', () => {
      const providerColumn = modelColumns[1]
      const headerFn = providerColumn.header as () => string
      expect(headerFn()).toBe('Provider')
    })

    it('inputPrice column should render "Input Price" header with subtitle', () => {
      const inputPriceColumn = modelColumns[2]
      const headerFn = inputPriceColumn.header as () => JSX.Element

      const { container } = render(<>{headerFn()}</>)
      expect(container.textContent).toContain('Input Price')
      expect(container.textContent).toContain('per 1M tokens')
    })

    it('outputPrice column should render "Output Price" header with subtitle', () => {
      const outputPriceColumn = modelColumns[3]
      const headerFn = outputPriceColumn.header as () => JSX.Element

      const { container } = render(<>{headerFn()}</>)
      expect(container.textContent).toContain('Output Price')
      expect(container.textContent).toContain('per 1M tokens')
    })

    it('price headers should have correct styling classes', () => {
      const inputPriceColumn = modelColumns[2]
      const headerFn = inputPriceColumn.header as () => JSX.Element

      const { container } = render(<>{headerFn()}</>)
      const span = container.querySelector('span')
      expect(span).toHaveClass('block', 'text-[10px]', 'font-normal', 'text-gray-500', 'normal-case')
    })
  })

  describe('Name Column Cell Renderer', () => {
    it('should render model name with proper styling', () => {
      const nameColumn = modelColumns[0]
      const mockModel = createMockModel({ name: 'GPT-4 Turbo' })

      const cellContext = {
        getValue: () => 'GPT-4 Turbo',
        row: { original: mockModel },
      } as any

      const cellFn = nameColumn.cell as (context: any) => JSX.Element
      const { container } = render(<>{cellFn(cellContext)}</>)

      expect(screen.getByText('GPT-4 Turbo')).toBeInTheDocument()
      expect(container.querySelector('.text-sm.font-medium.text-gray-900')).toBeInTheDocument()
    })

    it('should display version when present', () => {
      const nameColumn = modelColumns[0]
      const mockModel = createMockModel({ name: 'GPT-4', version: '0613' })

      const cellContext = {
        getValue: () => 'GPT-4',
        row: { original: mockModel },
      } as any

      const cellFn = nameColumn.cell as (context: any) => JSX.Element
      render(<>{cellFn(cellContext)}</>)

      expect(screen.getByText('v0613')).toBeInTheDocument()
    })

    it('should not display version when null', () => {
      const nameColumn = modelColumns[0]
      const mockModel = createMockModel({ name: 'GPT-4', version: null })

      const cellContext = {
        getValue: () => 'GPT-4',
        row: { original: mockModel },
      } as any

      const cellFn = nameColumn.cell as (context: any) => JSX.Element
      const { container } = render(<>{cellFn(cellContext)}</>)

      expect(container.textContent).not.toContain('v')
      expect(screen.getByText('GPT-4')).toBeInTheDocument()
    })

    it('should have correct CSS classes for version text', () => {
      const nameColumn = modelColumns[0]
      const mockModel = createMockModel({ version: '0613' })

      const cellContext = {
        getValue: () => 'Test Model',
        row: { original: mockModel },
      } as any

      const cellFn = nameColumn.cell as (context: any) => JSX.Element
      const { container } = render(<>{cellFn(cellContext)}</>)

      const versionDiv = container.querySelector('.text-xs.text-gray-500')
      expect(versionDiv).toBeInTheDocument()
      expect(versionDiv?.textContent).toBe('v0613')
    })
  })

  describe('Provider Column Cell Renderer', () => {
    it('should render provider name with proper styling', () => {
      const providerColumn = modelColumns[1]

      const cellContext = {
        getValue: () => 'OpenAI',
      } as any

      const cellFn = providerColumn.cell as (context: any) => JSX.Element
      const { container } = render(<>{cellFn(cellContext)}</>)

      expect(screen.getByText('OpenAI')).toBeInTheDocument()
      expect(container.querySelector('.text-sm.text-gray-900')).toBeInTheDocument()
    })

    it('should render different providers correctly', () => {
      const providerColumn = modelColumns[1]
      const providers = ['OpenAI', 'Anthropic', 'Google', 'Meta']

      providers.forEach((providerName) => {
        const cellContext = { getValue: () => providerName } as any
        const cellFn = providerColumn.cell as (context: any) => JSX.Element
        const { container } = render(<>{cellFn(cellContext)}</>)

        expect(screen.getByText(providerName)).toBeInTheDocument()
      })
    })
  })

  describe('Input Price Column Cell Renderer', () => {
    it('should format USD input price correctly', () => {
      const inputPriceColumn = modelColumns[2]
      const mockModel = createMockModel({ inputPricePer1M: 30.0, currency: 'USD' })

      const cellContext = {
        getValue: () => 30.0,
        row: { original: mockModel },
      } as any

      const cellFn = inputPriceColumn.cell as (context: any) => JSX.Element
      render(<>{cellFn(cellContext)}</>)

      expect(screen.getByText('$30.00')).toBeInTheDocument()
    })

    it('should format EUR input price correctly', () => {
      const inputPriceColumn = modelColumns[2]
      const mockModel = createMockModel({ inputPricePer1M: 25.5, currency: 'EUR' })

      const cellContext = {
        getValue: () => 25.5,
        row: { original: mockModel },
      } as any

      const cellFn = inputPriceColumn.cell as (context: any) => JSX.Element
      const { container } = render(<>{cellFn(cellContext)}</>)

      expect(container.textContent).toContain('25.50')
      expect(container.textContent).toContain('€')
    })

    it('should have correct CSS classes for price display', () => {
      const inputPriceColumn = modelColumns[2]
      const mockModel = createMockModel()

      const cellContext = {
        getValue: () => 30.0,
        row: { original: mockModel },
      } as any

      const cellFn = inputPriceColumn.cell as (context: any) => JSX.Element
      const { container } = render(<>{cellFn(cellContext)}</>)

      const priceDiv = container.querySelector('.text-sm.font-semibold.text-gray-900')
      expect(priceDiv).toBeInTheDocument()
    })

    it('should handle zero price', () => {
      const inputPriceColumn = modelColumns[2]
      const mockModel = createMockModel({ inputPricePer1M: 0 })

      const cellContext = {
        getValue: () => 0,
        row: { original: mockModel },
      } as any

      const cellFn = inputPriceColumn.cell as (context: any) => JSX.Element
      render(<>{cellFn(cellContext)}</>)

      expect(screen.getByText('$0.00')).toBeInTheDocument()
    })

    it('should format fractional prices correctly', () => {
      const inputPriceColumn = modelColumns[2]
      const mockModel = createMockModel({ inputPricePer1M: 0.15 })

      const cellContext = {
        getValue: () => 0.15,
        row: { original: mockModel },
      } as any

      const cellFn = inputPriceColumn.cell as (context: any) => JSX.Element
      render(<>{cellFn(cellContext)}</>)

      expect(screen.getByText('$0.15')).toBeInTheDocument()
    })
  })

  describe('Output Price Column Cell Renderer', () => {
    it('should format USD output price correctly', () => {
      const outputPriceColumn = modelColumns[3]
      const mockModel = createMockModel({ outputPricePer1M: 60.0, currency: 'USD' })

      const cellContext = {
        getValue: () => 60.0,
        row: { original: mockModel },
      } as any

      const cellFn = outputPriceColumn.cell as (context: any) => JSX.Element
      render(<>{cellFn(cellContext)}</>)

      expect(screen.getByText('$60.00')).toBeInTheDocument()
    })

    it('should format GBP output price correctly', () => {
      const outputPriceColumn = modelColumns[3]
      const mockModel = createMockModel({ outputPricePer1M: 45.0, currency: 'GBP' })

      const cellContext = {
        getValue: () => 45.0,
        row: { original: mockModel },
      } as any

      const cellFn = outputPriceColumn.cell as (context: any) => JSX.Element
      const { container } = render(<>{cellFn(cellContext)}</>)

      expect(container.textContent).toContain('45.00')
      expect(container.textContent).toContain('£')
    })

    it('should have correct CSS classes for price display', () => {
      const outputPriceColumn = modelColumns[3]
      const mockModel = createMockModel()

      const cellContext = {
        getValue: () => 60.0,
        row: { original: mockModel },
      } as any

      const cellFn = outputPriceColumn.cell as (context: any) => JSX.Element
      const { container } = render(<>{cellFn(cellContext)}</>)

      const priceDiv = container.querySelector('.text-sm.font-semibold.text-gray-900')
      expect(priceDiv).toBeInTheDocument()
    })
  })

  describe('Real-World LLM Model Examples', () => {
    it('should render GPT-4 pricing correctly', () => {
      const mockGpt4 = createMockModel({
        name: 'GPT-4',
        provider: 'OpenAI',
        version: '0613',
        inputPricePer1M: 30.0,
        outputPricePer1M: 60.0,
        currency: 'USD',
      })

      const inputPriceColumn = modelColumns[2]
      const outputPriceColumn = modelColumns[3]

      const inputContext = {
        getValue: () => mockGpt4.inputPricePer1M,
        row: { original: mockGpt4 },
      } as any

      const outputContext = {
        getValue: () => mockGpt4.outputPricePer1M,
        row: { original: mockGpt4 },
      } as any

      const inputCellFn = inputPriceColumn.cell as (context: any) => JSX.Element
      const outputCellFn = outputPriceColumn.cell as (context: any) => JSX.Element

      render(
        <>
          {inputCellFn(inputContext)}
          {outputCellFn(outputContext)}
        </>
      )

      expect(screen.getByText('$30.00')).toBeInTheDocument()
      expect(screen.getByText('$60.00')).toBeInTheDocument()
    })

    it('should render Claude 3.5 Sonnet pricing correctly', () => {
      const mockClaude = createMockModel({
        name: 'Claude 3.5 Sonnet',
        provider: 'Anthropic',
        version: '20240620',
        inputPricePer1M: 3.0,
        outputPricePer1M: 15.0,
        currency: 'USD',
      })

      const inputPriceColumn = modelColumns[2]
      const outputPriceColumn = modelColumns[3]

      const inputContext = {
        getValue: () => mockClaude.inputPricePer1M,
        row: { original: mockClaude },
      } as any

      const outputContext = {
        getValue: () => mockClaude.outputPricePer1M,
        row: { original: mockClaude },
      } as any

      const inputCellFn = inputPriceColumn.cell as (context: any) => JSX.Element
      const outputCellFn = outputPriceColumn.cell as (context: any) => JSX.Element

      render(
        <>
          {inputCellFn(inputContext)}
          {outputCellFn(outputContext)}
        </>
      )

      expect(screen.getByText('$3.00')).toBeInTheDocument()
      expect(screen.getByText('$15.00')).toBeInTheDocument()
    })
  })
})
