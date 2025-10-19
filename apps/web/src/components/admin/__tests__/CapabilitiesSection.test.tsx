/**
 * Unit tests for CapabilitiesSection component
 * Story 2.6: Add Capabilities Section to Model Form
 *
 * Test Coverage:
 * - AC #1: Capabilities section renders with all fields
 * - AC #2: Number inputs for context_window and max_output_tokens
 * - AC #3: Checkboxes for binary capabilities (6 flags)
 * - Cross-field validation: maxOutputTokens <= contextWindow
 */

import { describe, it, expect, beforeEach } from 'vitest'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { FormProvider, useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { CapabilitiesSection } from '../CapabilitiesSection'
import { createModelSchema, type CreateModelFormValues } from '@/schemas/modelSchema'

// Test wrapper component that provides React Hook Form context
function TestWrapper({ children, defaultValues }: { children: React.ReactNode; defaultValues?: Partial<CreateModelFormValues> }) {
  const methods = useForm<CreateModelFormValues>({
    resolver: zodResolver(createModelSchema),
    defaultValues: {
      name: 'Test Model',
      provider: 'Test Provider',
      status: 'active',
      inputPricePer1M: 10,
      outputPricePer1M: 30,
      currency: 'USD',
      capabilities: {
        contextWindow: 128000,
        maxOutputTokens: 4096,
        supportsFunctionCalling: false,
        supportsVision: false,
        supportsAudioInput: false,
        supportsAudioOutput: false,
        supportsStreaming: true,
        supportsJsonMode: false,
      },
      ...defaultValues,
    },
  })

  return <FormProvider {...methods}>{children}</FormProvider>
}

describe('CapabilitiesSection', () => {
  describe('AC #1: Section renders with proper structure', () => {
    it('should render the section header', () => {
      render(
        <TestWrapper>
          <CapabilitiesSection />
        </TestWrapper>
      )

      expect(screen.getByText('Model Capabilities')).toBeInTheDocument()
      expect(screen.getByText(/Define the model's technical capabilities/i)).toBeInTheDocument()
    })

    it('should render within a styled container', () => {
      const { container } = render(
        <TestWrapper>
          <CapabilitiesSection />
        </TestWrapper>
      )

      const section = container.querySelector('.bg-white.shadow.rounded-lg')
      expect(section).toBeInTheDocument()
    })
  })

  describe('AC #2: Number inputs for context window and max output tokens', () => {
    it('should render context window input field', () => {
      render(
        <TestWrapper>
          <CapabilitiesSection />
        </TestWrapper>
      )

      const contextInput = screen.getByLabelText(/Context Window/i)
      expect(contextInput).toBeInTheDocument()
      expect(contextInput).toHaveAttribute('type', 'number')
      expect(contextInput).toHaveAttribute('placeholder', '128000')
    })

    it('should render max output tokens input field', () => {
      render(
        <TestWrapper>
          <CapabilitiesSection />
        </TestWrapper>
      )

      const maxOutputInput = screen.getByLabelText(/Max Output Tokens/i)
      expect(maxOutputInput).toBeInTheDocument()
      expect(maxOutputInput).toHaveAttribute('type', 'number')
      expect(maxOutputInput).toHaveAttribute('placeholder', '4096')
    })

    it('should display helper text for context window', () => {
      render(
        <TestWrapper>
          <CapabilitiesSection />
        </TestWrapper>
      )

      expect(screen.getByText(/Maximum number of tokens the model can process/i)).toBeInTheDocument()
    })

    it('should display helper text for max output tokens', () => {
      render(
        <TestWrapper>
          <CapabilitiesSection />
        </TestWrapper>
      )

      expect(screen.getByText(/Maximum tokens in model's response/i)).toBeInTheDocument()
    })

    it('should mark context window as required', () => {
      render(
        <TestWrapper>
          <CapabilitiesSection />
        </TestWrapper>
      )

      // Check for required asterisk in the label
      const labels = screen.getAllByText(/Context Window/i)
      const labelElement = labels.find(el => el.tagName === 'LABEL')
      expect(labelElement?.textContent).toContain('*')
    })

    it('should accept numeric input for context window', async () => {
      const user = userEvent.setup()
      render(
        <TestWrapper>
          <CapabilitiesSection />
        </TestWrapper>
      )

      const contextInput = screen.getByLabelText(/Context Window/i) as HTMLInputElement
      await user.clear(contextInput)
      await user.type(contextInput, '256000')

      expect(contextInput.value).toBe('256000')
    })

    it('should accept numeric input for max output tokens', async () => {
      const user = userEvent.setup()
      render(
        <TestWrapper>
          <CapabilitiesSection />
        </TestWrapper>
      )

      const maxOutputInput = screen.getByLabelText(/Max Output Tokens/i) as HTMLInputElement
      await user.clear(maxOutputInput)
      await user.type(maxOutputInput, '8192')

      expect(maxOutputInput.value).toBe('8192')
    })
  })

  describe('AC #3: Checkboxes for binary capabilities (6 flags)', () => {
    it('should render all 6 capability checkboxes', () => {
      render(
        <TestWrapper>
          <CapabilitiesSection />
        </TestWrapper>
      )

      expect(screen.getByLabelText(/Supports Function Calling/i)).toBeInTheDocument()
      expect(screen.getByLabelText(/Supports Vision/i)).toBeInTheDocument()
      expect(screen.getByLabelText(/Supports Audio Input/i)).toBeInTheDocument()
      expect(screen.getByLabelText(/Supports Audio Output/i)).toBeInTheDocument()
      expect(screen.getByLabelText(/Supports Streaming/i)).toBeInTheDocument()
      expect(screen.getByLabelText(/Supports JSON Mode/i)).toBeInTheDocument()
    })

    it('should display helper text for function calling', () => {
      render(
        <TestWrapper>
          <CapabilitiesSection />
        </TestWrapper>
      )

      expect(screen.getByText(/Model can call external functions\/tools/i)).toBeInTheDocument()
    })

    it('should display helper text for vision', () => {
      render(
        <TestWrapper>
          <CapabilitiesSection />
        </TestWrapper>
      )

      expect(screen.getByText(/Model can process and understand images/i)).toBeInTheDocument()
    })

    it('should display helper text for audio input', () => {
      render(
        <TestWrapper>
          <CapabilitiesSection />
        </TestWrapper>
      )

      expect(screen.getByText(/Model can process speech/i)).toBeInTheDocument()
    })

    it('should display helper text for audio output', () => {
      render(
        <TestWrapper>
          <CapabilitiesSection />
        </TestWrapper>
      )

      expect(screen.getByText(/Model can generate speech/i)).toBeInTheDocument()
    })

    it('should display helper text for streaming', () => {
      render(
        <TestWrapper>
          <CapabilitiesSection />
        </TestWrapper>
      )

      expect(screen.getByText(/Model can stream responses token-by-token/i)).toBeInTheDocument()
    })

    it('should display helper text for JSON mode', () => {
      render(
        <TestWrapper>
          <CapabilitiesSection />
        </TestWrapper>
      )

      expect(screen.getByText(/Model can output structured JSON responses/i)).toBeInTheDocument()
    })

    it('should have streaming checkbox checked by default', () => {
      render(
        <TestWrapper>
          <CapabilitiesSection />
        </TestWrapper>
      )

      const streamingCheckbox = screen.getByLabelText(/Supports Streaming/i) as HTMLInputElement
      expect(streamingCheckbox).toBeChecked()
    })

    it('should have other checkboxes unchecked by default', () => {
      render(
        <TestWrapper>
          <CapabilitiesSection />
        </TestWrapper>
      )

      expect(screen.getByLabelText(/Supports Function Calling/i)).not.toBeChecked()
      expect(screen.getByLabelText(/Supports Vision/i)).not.toBeChecked()
      expect(screen.getByLabelText(/Supports Audio Input/i)).not.toBeChecked()
      expect(screen.getByLabelText(/Supports Audio Output/i)).not.toBeChecked()
      expect(screen.getByLabelText(/Supports JSON Mode/i)).not.toBeChecked()
    })

    it('should toggle function calling checkbox', async () => {
      const user = userEvent.setup()
      render(
        <TestWrapper>
          <CapabilitiesSection />
        </TestWrapper>
      )

      const checkbox = screen.getByLabelText(/Supports Function Calling/i) as HTMLInputElement
      expect(checkbox).not.toBeChecked()

      await user.click(checkbox)
      expect(checkbox).toBeChecked()

      await user.click(checkbox)
      expect(checkbox).not.toBeChecked()
    })

    it('should toggle vision checkbox', async () => {
      const user = userEvent.setup()
      render(
        <TestWrapper>
          <CapabilitiesSection />
        </TestWrapper>
      )

      const checkbox = screen.getByLabelText(/Supports Vision/i) as HTMLInputElement
      expect(checkbox).not.toBeChecked()

      await user.click(checkbox)
      expect(checkbox).toBeChecked()
    })

    it('should toggle JSON mode checkbox', async () => {
      const user = userEvent.setup()
      render(
        <TestWrapper>
          <CapabilitiesSection />
        </TestWrapper>
      )

      const checkbox = screen.getByLabelText(/Supports JSON Mode/i) as HTMLInputElement
      expect(checkbox).not.toBeChecked()

      await user.click(checkbox)
      expect(checkbox).toBeChecked()
    })

    it('should allow multiple checkboxes to be checked simultaneously', async () => {
      const user = userEvent.setup()
      render(
        <TestWrapper>
          <CapabilitiesSection />
        </TestWrapper>
      )

      const functionCalling = screen.getByLabelText(/Supports Function Calling/i)
      const vision = screen.getByLabelText(/Supports Vision/i)
      const jsonMode = screen.getByLabelText(/Supports JSON Mode/i)

      await user.click(functionCalling)
      await user.click(vision)
      await user.click(jsonMode)

      expect(functionCalling).toBeChecked()
      expect(vision).toBeChecked()
      expect(jsonMode).toBeChecked()
    })
  })

  /**
   * Note: Detailed validation tests (min/max ranges, cross-field validation)
   * are covered in ModelForm.test.tsx where the full form context is available.
   * CapabilitiesSection focuses on rendering and basic interaction.
   */

  describe('Integration with React Hook Form', () => {
    it('should register all fields with React Hook Form', () => {
      const { container } = render(
        <TestWrapper>
          <CapabilitiesSection />
        </TestWrapper>
      )

      // Check all inputs have id attributes (registered with RHF)
      expect(container.querySelector('input[id="contextWindow"]')).toBeInTheDocument()
      expect(container.querySelector('input[id="maxOutputTokens"]')).toBeInTheDocument()
      expect(container.querySelector('input[id="supportsFunctionCalling"]')).toBeInTheDocument()
      expect(container.querySelector('input[id="supportsVision"]')).toBeInTheDocument()
      expect(container.querySelector('input[id="supportsAudioInput"]')).toBeInTheDocument()
      expect(container.querySelector('input[id="supportsAudioOutput"]')).toBeInTheDocument()
      expect(container.querySelector('input[id="supportsStreaming"]')).toBeInTheDocument()
      expect(container.querySelector('input[id="supportsJsonMode"]')).toBeInTheDocument()
    })
  })

  describe('Accessibility', () => {
    it('should have proper label associations for inputs', () => {
      render(
        <TestWrapper>
          <CapabilitiesSection />
        </TestWrapper>
      )

      const contextInput = screen.getByLabelText(/Context Window/i)
      const maxOutputInput = screen.getByLabelText(/Max Output Tokens/i)

      expect(contextInput).toHaveAccessibleName()
      expect(maxOutputInput).toHaveAccessibleName()
    })

    it('should have proper label associations for checkboxes', () => {
      render(
        <TestWrapper>
          <CapabilitiesSection />
        </TestWrapper>
      )

      expect(screen.getByLabelText(/Supports Function Calling/i)).toHaveAccessibleName()
      expect(screen.getByLabelText(/Supports Vision/i)).toHaveAccessibleName()
      expect(screen.getByLabelText(/Supports Streaming/i)).toHaveAccessibleName()
    })

    it('should mark required field with asterisk', () => {
      render(
        <TestWrapper>
          <CapabilitiesSection />
        </TestWrapper>
      )

      // Find the label element specifically
      const labels = screen.getAllByText(/Context Window/i)
      const labelElement = labels.find(el => el.tagName === 'LABEL')
      expect(labelElement?.textContent).toContain('*')
    })
  })
})
