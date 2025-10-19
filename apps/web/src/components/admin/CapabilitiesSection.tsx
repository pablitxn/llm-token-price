/**
 * CapabilitiesSection Component
 * Section of ModelForm for capturing model capabilities
 * Includes context window, max output tokens, and feature flags
 */

import { useFormContext } from 'react-hook-form'
import type { CreateModelFormValues } from '@/schemas/modelSchema'

/**
 * Capabilities form section with numeric inputs and feature checkboxes
 * Uses React Hook Form context to integrate with parent ModelForm
 */
export function CapabilitiesSection() {
  const {
    register,
    formState: { errors },
  } = useFormContext<CreateModelFormValues>()

  return (
    <div className="bg-white shadow rounded-lg p-6">
      <h2 className="text-lg font-semibold text-gray-900 mb-2">Model Capabilities</h2>
      <p className="text-sm text-gray-500 mb-4">
        Define the model's technical capabilities and supported features
      </p>

      {/* Context Window and Max Output Tokens */}
      <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 mb-6">
        {/* Context Window */}
        <div>
          <label htmlFor="contextWindow" className="block text-sm font-medium text-gray-700">
            Context Window <span className="text-red-600">*</span>
          </label>
          <p className="text-xs text-gray-500 mt-1 mb-2">
            Maximum number of tokens the model can process in a single request
          </p>
          <input
            type="number"
            id="contextWindow"
            {...register('capabilities.contextWindow', { valueAsNumber: true })}
            className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 shadow-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
            placeholder="128000"
          />
          {errors.capabilities?.contextWindow && (
            <p className="mt-1 text-sm text-red-600">
              {errors.capabilities.contextWindow.message}
            </p>
          )}
        </div>

        {/* Max Output Tokens */}
        <div>
          <label htmlFor="maxOutputTokens" className="block text-sm font-medium text-gray-700">
            Max Output Tokens
          </label>
          <p className="text-xs text-gray-500 mt-1 mb-2">
            Maximum tokens in model's response (optional, must not exceed context window)
          </p>
          <input
            type="number"
            id="maxOutputTokens"
            {...register('capabilities.maxOutputTokens', {
              valueAsNumber: true,
              setValueAs: (v) => (v === '' || v === null || isNaN(v) ? null : Number(v)),
            })}
            className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 shadow-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
            placeholder="4096"
          />
          {errors.capabilities?.maxOutputTokens && (
            <p className="mt-1 text-sm text-red-600">
              {errors.capabilities.maxOutputTokens.message}
            </p>
          )}
        </div>
      </div>

      {/* Feature Capabilities */}
      <div className="border-t pt-4">
        <h3 className="text-sm font-semibold text-gray-900 mb-3">Supported Features</h3>
        <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
          {/* Function Calling */}
          <div className="flex items-start">
            <div className="flex items-center h-5">
              <input
                type="checkbox"
                id="supportsFunctionCalling"
                {...register('capabilities.supportsFunctionCalling')}
                className="h-4 w-4 rounded border-gray-300 text-blue-600 focus:ring-blue-500"
              />
            </div>
            <div className="ml-3">
              <label htmlFor="supportsFunctionCalling" className="text-sm font-medium text-gray-700">
                Supports Function Calling
              </label>
              <p className="text-xs text-gray-500">
                Model can call external functions/tools based on user prompts
              </p>
            </div>
          </div>

          {/* Vision */}
          <div className="flex items-start">
            <div className="flex items-center h-5">
              <input
                type="checkbox"
                id="supportsVision"
                {...register('capabilities.supportsVision')}
                className="h-4 w-4 rounded border-gray-300 text-blue-600 focus:ring-blue-500"
              />
            </div>
            <div className="ml-3">
              <label htmlFor="supportsVision" className="text-sm font-medium text-gray-700">
                Supports Vision
              </label>
              <p className="text-xs text-gray-500">Model can process and understand images</p>
            </div>
          </div>

          {/* Audio Input */}
          <div className="flex items-start">
            <div className="flex items-center h-5">
              <input
                type="checkbox"
                id="supportsAudioInput"
                {...register('capabilities.supportsAudioInput')}
                className="h-4 w-4 rounded border-gray-300 text-blue-600 focus:ring-blue-500"
              />
            </div>
            <div className="ml-3">
              <label htmlFor="supportsAudioInput" className="text-sm font-medium text-gray-700">
                Supports Audio Input
              </label>
              <p className="text-xs text-gray-500">Model can process speech (speech-to-text)</p>
            </div>
          </div>

          {/* Audio Output */}
          <div className="flex items-start">
            <div className="flex items-center h-5">
              <input
                type="checkbox"
                id="supportsAudioOutput"
                {...register('capabilities.supportsAudioOutput')}
                className="h-4 w-4 rounded border-gray-300 text-blue-600 focus:ring-blue-500"
              />
            </div>
            <div className="ml-3">
              <label htmlFor="supportsAudioOutput" className="text-sm font-medium text-gray-700">
                Supports Audio Output
              </label>
              <p className="text-xs text-gray-500">Model can generate speech (text-to-speech)</p>
            </div>
          </div>

          {/* Streaming */}
          <div className="flex items-start">
            <div className="flex items-center h-5">
              <input
                type="checkbox"
                id="supportsStreaming"
                {...register('capabilities.supportsStreaming')}
                defaultChecked
                className="h-4 w-4 rounded border-gray-300 text-blue-600 focus:ring-blue-500"
              />
            </div>
            <div className="ml-3">
              <label htmlFor="supportsStreaming" className="text-sm font-medium text-gray-700">
                Supports Streaming
              </label>
              <p className="text-xs text-gray-500">Model can stream responses token-by-token</p>
            </div>
          </div>

          {/* JSON Mode */}
          <div className="flex items-start">
            <div className="flex items-center h-5">
              <input
                type="checkbox"
                id="supportsJsonMode"
                {...register('capabilities.supportsJsonMode')}
                className="h-4 w-4 rounded border-gray-300 text-blue-600 focus:ring-blue-500"
              />
            </div>
            <div className="ml-3">
              <label htmlFor="supportsJsonMode" className="text-sm font-medium text-gray-700">
                Supports JSON Mode
              </label>
              <p className="text-xs text-gray-500">Model can output structured JSON responses</p>
            </div>
          </div>
        </div>
      </div>
    </div>
  )
}
