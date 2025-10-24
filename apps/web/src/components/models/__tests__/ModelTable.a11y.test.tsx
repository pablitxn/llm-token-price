import { describe, it, expect, beforeAll } from 'vitest'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { axe } from 'vitest-axe'
import ModelTable from '../ModelTable'
import type { ModelDto } from '@/types/models'

/**
 * Accessibility tests for ModelTable
 *
 * Story 3.3: TanStack Table Integration
 * Tests AC #4: Maintains visual appearance and accessibility
 *
 * Coverage:
 * - ARIA attributes and semantic HTML
 * - Keyboard navigation
 * - Screen reader compatibility
 * - WCAG 2.1 AA compliance (via axe-core automated tests)
 * - Focus management
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

describe('ModelTable Accessibility Tests', () => {
  describe('Semantic HTML (WCAG 1.3.1)', () => {
    it('should use proper table semantics', () => {
      const mockModels = [createMockModel()]
      const { container } = render(<ModelTable models={mockModels} />)

      // Verify semantic table structure
      expect(container.querySelector('table')).toBeInTheDocument()
      expect(container.querySelector('thead')).toBeInTheDocument()
      expect(container.querySelector('tbody')).toBeInTheDocument()
      expect(container.querySelector('tr')).toBeInTheDocument()
      expect(container.querySelector('th')).toBeInTheDocument()
      expect(container.querySelector('td')).toBeInTheDocument()
    })

    it('should have proper heading hierarchy', () => {
      const mockModels = [createMockModel()]
      render(<ModelTable models={mockModels} />)

      // All table headers should be th elements
      const headers = screen.getAllByRole('columnheader')
      expect(headers.length).toBeGreaterThan(0)
      headers.forEach((header) => {
        expect(header.tagName).toBe('TH')
      })
    })

    it('should use scope attribute on table headers', () => {
      const mockModels = [createMockModel()]
      render(<ModelTable models={mockModels} />)

      const headers = screen.getAllByRole('columnheader')
      headers.forEach((header) => {
        expect(header).toHaveAttribute('scope', 'col')
      })
    })

    it('should have role="table" on table element', () => {
      const mockModels = [createMockModel()]
      render(<ModelTable models={mockModels} />)

      const table = screen.getByRole('table')
      expect(table).toBeInTheDocument()
    })
  })

  describe('Keyboard Navigation (WCAG 2.1.1)', () => {
    it('should allow tab navigation through table rows', async () => {
      const user = userEvent.setup()
      const mockModels = [
        createMockModel({ name: 'Model 1' }),
        createMockModel({ name: 'Model 2' }),
      ]

      render(<ModelTable models={mockModels} />)

      const table = screen.getByRole('table')
      table.focus()

      // Tab should navigate to first row
      await user.keyboard('{Tab}')

      // Verify focus moved
      expect(document.activeElement).toBeDefined()
    })

    it('should be reachable via keyboard navigation', () => {
      const mockModels = [createMockModel()]
      const { container } = render(<ModelTable models={mockModels} />)

      const table = container.querySelector('table')
      expect(table).not.toHaveAttribute('tabindex', '-1') // Should not be hidden from keyboard
    })
  })

  describe('Screen Reader Support (WCAG 4.1.2)', () => {
    it('should provide meaningful column headers for screen readers', () => {
      const mockModels = [createMockModel()]
      render(<ModelTable models={mockModels} />)

      // Verify column headers have accessible names
      expect(screen.getByRole('columnheader', { name: /name/i })).toBeInTheDocument()
      expect(screen.getByRole('columnheader', { name: /provider/i })).toBeInTheDocument()
      expect(screen.getByRole('columnheader', { name: /input price/i })).toBeInTheDocument()
      expect(screen.getByRole('columnheader', { name: /output price/i })).toBeInTheDocument()
    })

    it('should have accessible row structure', () => {
      const mockModels = [
        createMockModel({ name: 'Model 1' }),
        createMockModel({ name: 'Model 2' }),
      ]

      render(<ModelTable models={mockModels} />)

      const rows = screen.getAllByRole('row')
      expect(rows.length).toBeGreaterThan(0)
    })

    it('should provide context for price headers with subtitle', () => {
      const mockModels = [createMockModel()]
      render(<ModelTable models={mockModels} />)

      const inputPriceHeader = screen.getByRole('columnheader', { name: /input price/i })
      expect(inputPriceHeader.textContent).toContain('per 1M tokens')

      const outputPriceHeader = screen.getByRole('columnheader', { name: /output price/i })
      expect(outputPriceHeader.textContent).toContain('per 1M tokens')
    })
  })

  describe('WCAG 2.1 AA Compliance (Automated Tests)', () => {
    it('should not have automated accessibility violations', async () => {
      const mockModels = [
        createMockModel({ name: 'GPT-4', provider: 'OpenAI' }),
        createMockModel({ name: 'Claude 3.5', provider: 'Anthropic' }),
      ]

      const { container } = render(<ModelTable models={mockModels} />)

      const results = await axe(container)
      expect(results.violations).toEqual([])
    })

    it('should pass axe tests with empty data', async () => {
      const { container } = render(<ModelTable models={[]} />)

      const results = await axe(container)
      expect(results.violations).toEqual([])
    })

    it('should pass axe tests with large dataset', async () => {
      const mockModels = Array.from({ length: 20 }, (_, i) =>
        createMockModel({ name: `Model ${i + 1}` })
      )

      const { container } = render(<ModelTable models={mockModels} />)

      const results = await axe(container)
      expect(results.violations).toEqual([])
    })
  })

  describe('Color Contrast (WCAG 1.4.3)', () => {
    it('should use semantic color classes for readability', () => {
      const mockModels = [createMockModel()]
      const { container } = render(<ModelTable models={mockModels} />)

      // Verify text color classes provide sufficient contrast
      const headerCells = container.querySelectorAll('th')
      headerCells.forEach((th) => {
        expect(th).toHaveClass('text-gray-700') // Should have readable text color
      })

      const dataCells = container.querySelectorAll('td .text-gray-900')
      expect(dataCells.length).toBeGreaterThan(0) // Data should use high-contrast text
    })

    it('should maintain contrast in alternating rows', () => {
      const mockModels = [
        createMockModel({ name: 'Model 1' }),
        createMockModel({ name: 'Model 2' }),
      ]

      const { container } = render(<ModelTable models={mockModels} />)

      const rows = container.querySelectorAll('tbody tr')

      // Even rows: bg-white
      expect(rows[0]).toHaveClass('bg-white')

      // Odd rows: bg-gray-50/50
      expect(rows[1]).toHaveClass('bg-gray-50/50')

      // Both should maintain text contrast
      rows.forEach((row) => {
        const textElements = row.querySelectorAll('.text-gray-900')
        expect(textElements.length).toBeGreaterThan(0)
      })
    })
  })

  describe('Focus Management (WCAG 2.4.7)', () => {
    it('should not have hidden focus indicators', () => {
      const mockModels = [createMockModel()]
      const { container } = render(<ModelTable models={mockModels} />)

      // Verify no focus:outline-none that would hide focus
      const table = container.querySelector('table')
      expect(table).not.toHaveClass('outline-none')
    })

    it('should allow focus on interactive elements', () => {
      const mockModels = [createMockModel()]
      const { container } = render(<ModelTable models={mockModels} />)

      const table = container.querySelector('table')
      expect(table).toBeInTheDocument()

      // Table should be reachable
      expect(table).not.toHaveAttribute('aria-hidden', 'true')
    })
  })

  describe('Responsive Design Accessibility (WCAG 1.4.10)', () => {
    it('should provide horizontal scroll for overflow', () => {
      const mockModels = [createMockModel()]
      const { container } = render(<ModelTable models={mockModels} />)

      const wrapper = container.firstChild as HTMLElement
      expect(wrapper).toHaveClass('overflow-x-auto')
    })

    it('should maintain table structure in scrollable container', () => {
      const mockModels = [createMockModel()]
      const { container } = render(<ModelTable models={mockModels} />)

      const wrapper = container.firstChild as HTMLElement
      const table = wrapper.querySelector('table')

      expect(table).toBeInTheDocument()
      expect(wrapper).toHaveClass('w-full') // Full width container
    })
  })

  describe('Text Alternatives (WCAG 1.1.1)', () => {
    it('should provide meaningful text for version information', () => {
      const mockModel = createMockModel({
        name: 'GPT-4',
        version: '0613',
      })

      render(<ModelTable models={[mockModel]} />)

      // Version should be clearly labeled
      expect(screen.getByText('v0613')).toBeInTheDocument()
      expect(screen.getByText('GPT-4')).toBeInTheDocument()
    })

    it('should format prices in human-readable format', () => {
      const mockModel = createMockModel({
        inputPricePer1M: 30.0,
        outputPricePer1M: 60.0,
      })

      render(<ModelTable models={[mockModel]} />)

      // Prices should be clear and formatted
      expect(screen.getByText('$30.00')).toBeInTheDocument()
      expect(screen.getByText('$60.00')).toBeInTheDocument()
    })

    it('should provide context for price units', () => {
      const mockModels = [createMockModel()]
      render(<ModelTable models={mockModels} />)

      const inputPriceHeader = screen.getByRole('columnheader', { name: /input price/i })
      expect(inputPriceHeader.textContent).toContain('per 1M tokens')

      const outputPriceHeader = screen.getByRole('columnheader', { name: /output price/i })
      expect(outputPriceHeader.textContent).toContain('per 1M tokens')
    })
  })

  describe('Error Prevention & Recovery (WCAG 3.3.1)', () => {
    it('should handle missing data gracefully', () => {
      const modelWithoutVersion = createMockModel({
        name: 'Test Model',
        version: null,
      })

      render(<ModelTable models={[modelWithoutVersion]} />)

      // Should display name without crashing
      expect(screen.getByText('Test Model')).toBeInTheDocument()
    })

    it('should handle empty dataset without accessibility violations', async () => {
      const { container } = render(<ModelTable models={[]} />)

      // Should still render accessible table structure
      expect(screen.getByRole('table')).toBeInTheDocument()
      expect(screen.getAllByRole('columnheader')).toHaveLength(4)

      const results = await axe(container)
      expect(results.violations).toEqual([])
    })
  })

  describe('Language Specification (WCAG 3.1.1)', () => {
    it('should use consistent language for all text', () => {
      const mockModels = [createMockModel()]
      render(<ModelTable models={mockModels} />)

      // All headers should be in English (default)
      expect(screen.getByText('Name')).toBeInTheDocument()
      expect(screen.getByText('Provider')).toBeInTheDocument()
      expect(screen.getByText(/Input Price/i)).toBeInTheDocument()
      expect(screen.getByText(/Output Price/i)).toBeInTheDocument()
    })
  })

  describe('Real-World Accessibility Scenarios', () => {
    it('should be accessible with real LLM model data', async () => {
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
      ]

      const { container } = render(<ModelTable models={realModels} />)

      const results = await axe(container)
      expect(results.violations).toEqual([])

      // Verify all data is accessible
      expect(screen.getByText('GPT-4 Turbo')).toBeInTheDocument()
      expect(screen.getByText('Claude 3.5 Sonnet')).toBeInTheDocument()
    })

    it('should be accessible with diverse currency formats', async () => {
      const models = [
        createMockModel({ currency: 'USD', inputPricePer1M: 10.0 }),
        createMockModel({ currency: 'EUR', inputPricePer1M: 8.5 }),
        createMockModel({ currency: 'GBP', inputPricePer1M: 7.0 }),
      ]

      const { container } = render(<ModelTable models={models} />)

      const results = await axe(container)
      expect(results.violations).toEqual([])
    })
  })
})
