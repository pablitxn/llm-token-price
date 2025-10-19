/**
 * ModelForm Component
 * Form for creating and editing LLM models in the admin panel
 * Uses React Hook Form for performance and Zod for validation
 */

import { useForm, FormProvider } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { useNavigate } from 'react-router-dom'
import { useEffect } from 'react'
import { createModelSchema, type CreateModelFormValues } from '@/schemas/modelSchema'
import { useCreateModel } from '@/hooks/useCreateModel'
import { useUpdateModel } from '@/hooks/useUpdateModel'
import { CapabilitiesSection } from './CapabilitiesSection'
import type { AdminModelDto } from '@/types/admin'
import { format } from 'date-fns'

interface ModelFormProps {
  /** Form mode: 'create' for new models, 'edit' for existing models */
  mode: 'create' | 'edit'
  /** Model ID (required for edit mode) */
  modelId?: string
  /** Model data for edit mode (null for create mode) */
  model?: AdminModelDto | null
}

/**
 * Model creation/edit form with validation
 * Implements double-layer validation: Zod (client) + FluentValidation (server, Story 2.5)
 *
 * @param props - Component props
 * @param props.mode - Form mode ('create' | 'edit')
 * @param props.modelId - Model ID for edit mode
 * @param props.model - Model data for edit mode
 */
export function ModelForm({ mode, modelId, model = null }: ModelFormProps) {
  const navigate = useNavigate()
  const { mutate: createModel, isPending: isCreating, error: createError } = useCreateModel()
  const { mutate: updateModel, isPending: isUpdating, error: updateError } = useUpdateModel()

  const isPending = mode === 'create' ? isCreating : isUpdating
  const error = mode === 'create' ? createError : updateError

  const methods = useForm<CreateModelFormValues>({
    resolver: zodResolver(createModelSchema),
    defaultValues: model
      ? {
          // Pre-populate from model data in edit mode
          name: model.name,
          provider: model.provider,
          version: model.version || '',
          releaseDate: model.releaseDate ? format(new Date(model.releaseDate), 'yyyy-MM-dd') : '',
          status: model.status,
          inputPricePer1M: model.inputPricePer1M,
          outputPricePer1M: model.outputPricePer1M,
          currency: model.currency,
          pricingValidFrom: model.pricingValidFrom
            ? format(new Date(model.pricingValidFrom), 'yyyy-MM-dd')
            : '',
          pricingValidTo: model.pricingValidTo
            ? format(new Date(model.pricingValidTo), 'yyyy-MM-dd')
            : '',
          capabilities: {
            contextWindow: model.capability?.contextWindow ?? 0,
            maxOutputTokens: model.capability?.maxOutputTokens ?? null,
            supportsFunctionCalling: model.capability?.supportsFunctionCalling ?? false,
            supportsVision: model.capability?.supportsVision ?? false,
            supportsAudioInput: model.capability?.supportsAudioInput ?? false,
            supportsAudioOutput: model.capability?.supportsAudioOutput ?? false,
            supportsStreaming: model.capability?.supportsStreaming ?? true,
            supportsJsonMode: model.capability?.supportsJsonMode ?? false,
          },
        }
      : {
          // Default values for create mode
          name: '',
          provider: '',
          version: '',
          releaseDate: '',
          status: 'active',
          inputPricePer1M: 0,
          outputPricePer1M: 0,
          currency: 'USD',
          pricingValidFrom: '',
          pricingValidTo: '',
          capabilities: {
            contextWindow: 0,
            maxOutputTokens: null,
            supportsFunctionCalling: false,
            supportsVision: false,
            supportsAudioInput: false,
            supportsAudioOutput: false,
            supportsStreaming: true,
            supportsJsonMode: false,
          },
        },
  })

  const {
    register,
    handleSubmit,
    formState: { errors, isDirty },
    reset,
  } = methods

  // Warn user about unsaved changes when navigating away
  useEffect(() => {
    const handleBeforeUnload = (e: BeforeUnloadEvent) => {
      if (isDirty) {
        e.preventDefault()
        e.returnValue = ''
      }
    }

    window.addEventListener('beforeunload', handleBeforeUnload)
    return () => window.removeEventListener('beforeunload', handleBeforeUnload)
  }, [isDirty])

  const onSubmit = (data: CreateModelFormValues) => {
    // Convert empty strings to undefined for optional fields
    const payload = {
      ...data,
      version: data.version || undefined,
      releaseDate: data.releaseDate || undefined,
      pricingValidFrom: data.pricingValidFrom || undefined,
      pricingValidTo: data.pricingValidTo || undefined,
    }

    if (mode === 'create') {
      createModel(payload)
    } else if (mode === 'edit' && modelId) {
      updateModel({ id: modelId, data: payload })
    }
  }

  const handleCancel = () => {
    if (isDirty) {
      const confirmed = window.confirm(
        'You have unsaved changes. Are you sure you want to leave?'
      )
      if (!confirmed) return
    }
    navigate('/admin/models')
  }

  const handleReset = () => {
    reset()
  }

  return (
    <FormProvider {...methods}>
      <form onSubmit={handleSubmit(onSubmit)} className="space-y-8">
      {/* Basic Info Section */}
      <div className="bg-white shadow rounded-lg p-6">
        <h2 className="text-lg font-semibold text-gray-900 mb-4">Basic Information</h2>
        <div className="grid grid-cols-1 gap-6 sm:grid-cols-2">
          {/* Model Name */}
          <div className="sm:col-span-2">
            <label htmlFor="name" className="block text-sm font-medium text-gray-700">
              Model Name <span className="text-red-600">*</span>
              {mode === 'edit' && (
                <span className="ml-2 text-xs text-gray-500">(Cannot be changed)</span>
              )}
            </label>
            <input
              type="text"
              id="name"
              {...register('name')}
              disabled={mode === 'edit'}
              className={`mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 shadow-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500 ${
                mode === 'edit' ? 'bg-gray-100 cursor-not-allowed opacity-60' : ''
              }`}
              placeholder="e.g., GPT-4 Turbo"
            />
            {errors.name && (
              <p className="mt-1 text-sm text-red-600">{errors.name.message}</p>
            )}
          </div>

          {/* Provider */}
          <div>
            <label htmlFor="provider" className="block text-sm font-medium text-gray-700">
              Provider <span className="text-red-600">*</span>
              {mode === 'edit' && (
                <span className="ml-2 text-xs text-gray-500">(Cannot be changed)</span>
              )}
            </label>
            <input
              type="text"
              id="provider"
              {...register('provider')}
              disabled={mode === 'edit'}
              className={`mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 shadow-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500 ${
                mode === 'edit' ? 'bg-gray-100 cursor-not-allowed opacity-60' : ''
              }`}
              placeholder="e.g., OpenAI"
            />
            {errors.provider && (
              <p className="mt-1 text-sm text-red-600">{errors.provider.message}</p>
            )}
          </div>

          {/* Version */}
          <div>
            <label htmlFor="version" className="block text-sm font-medium text-gray-700">
              Version
            </label>
            <input
              type="text"
              id="version"
              {...register('version')}
              className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 shadow-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
              placeholder="e.g., 1.0"
            />
            {errors.version && (
              <p className="mt-1 text-sm text-red-600">{errors.version.message}</p>
            )}
          </div>

          {/* Release Date */}
          <div>
            <label htmlFor="releaseDate" className="block text-sm font-medium text-gray-700">
              Release Date
            </label>
            <input
              type="date"
              id="releaseDate"
              {...register('releaseDate')}
              className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 shadow-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
            />
            {errors.releaseDate && (
              <p className="mt-1 text-sm text-red-600">{errors.releaseDate.message}</p>
            )}
          </div>

          {/* Status */}
          <div>
            <label htmlFor="status" className="block text-sm font-medium text-gray-700">
              Status <span className="text-red-600">*</span>
            </label>
            <select
              id="status"
              {...register('status')}
              className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 shadow-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
            >
              <option value="active">Active</option>
              <option value="deprecated">Deprecated</option>
              <option value="beta">Beta</option>
            </select>
            {errors.status && (
              <p className="mt-1 text-sm text-red-600">{errors.status.message}</p>
            )}
          </div>
        </div>
      </div>

      {/* Pricing Section */}
      <div className="bg-white shadow rounded-lg p-6">
        <h2 className="text-lg font-semibold text-gray-900 mb-4">Pricing</h2>
        <p className="text-sm text-gray-500 mb-4">
          Enter prices per 1 million tokens with up to 6 decimal places (e.g., $0.003000 per 1M
          tokens)
        </p>
        <div className="grid grid-cols-1 gap-6 sm:grid-cols-2">
          {/* Input Price */}
          <div>
            <label
              htmlFor="inputPricePer1M"
              className="block text-sm font-medium text-gray-700"
            >
              Input Price per 1M Tokens <span className="text-red-600">*</span>
            </label>
            <input
              type="number"
              id="inputPricePer1M"
              step="0.000001"
              {...register('inputPricePer1M', { valueAsNumber: true })}
              className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 shadow-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
              placeholder="0.003000"
            />
            {errors.inputPricePer1M && (
              <p className="mt-1 text-sm text-red-600">{errors.inputPricePer1M.message}</p>
            )}
          </div>

          {/* Output Price */}
          <div>
            <label
              htmlFor="outputPricePer1M"
              className="block text-sm font-medium text-gray-700"
            >
              Output Price per 1M Tokens <span className="text-red-600">*</span>
            </label>
            <input
              type="number"
              id="outputPricePer1M"
              step="0.000001"
              {...register('outputPricePer1M', { valueAsNumber: true })}
              className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 shadow-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
              placeholder="0.015000"
            />
            {errors.outputPricePer1M && (
              <p className="mt-1 text-sm text-red-600">{errors.outputPricePer1M.message}</p>
            )}
          </div>

          {/* Currency */}
          <div>
            <label htmlFor="currency" className="block text-sm font-medium text-gray-700">
              Currency <span className="text-red-600">*</span>
            </label>
            <select
              id="currency"
              {...register('currency')}
              className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 shadow-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
            >
              <option value="USD">USD</option>
              <option value="EUR">EUR</option>
              <option value="GBP">GBP</option>
            </select>
            {errors.currency && (
              <p className="mt-1 text-sm text-red-600">{errors.currency.message}</p>
            )}
          </div>

          {/* Pricing Valid From */}
          <div>
            <label
              htmlFor="pricingValidFrom"
              className="block text-sm font-medium text-gray-700"
            >
              Pricing Valid From
            </label>
            <input
              type="date"
              id="pricingValidFrom"
              {...register('pricingValidFrom')}
              className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 shadow-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
            />
            {errors.pricingValidFrom && (
              <p className="mt-1 text-sm text-red-600">{errors.pricingValidFrom.message}</p>
            )}
          </div>

          {/* Pricing Valid To */}
          <div className="sm:col-span-2">
            <label htmlFor="pricingValidTo" className="block text-sm font-medium text-gray-700">
              Pricing Valid To
            </label>
            <input
              type="date"
              id="pricingValidTo"
              {...register('pricingValidTo')}
              className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 shadow-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
            />
            {errors.pricingValidTo && (
              <p className="mt-1 text-sm text-red-600">{errors.pricingValidTo.message}</p>
            )}
          </div>
        </div>
      </div>

      {/* Capabilities Section */}
      <CapabilitiesSection />

      {/* Server Error Display */}
      {error && (
        <div className="bg-red-50 border border-red-200 rounded-lg p-4">
          <p className="text-sm text-red-800">
            {error.message || 'An error occurred while creating the model. Please try again.'}
          </p>
        </div>
      )}

      {/* Form Actions */}
      <div className="flex items-center justify-end gap-4">
        <button
          type="button"
          onClick={handleReset}
          className="px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-md shadow-sm hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500"
        >
          Reset Form
        </button>
        <button
          type="button"
          onClick={handleCancel}
          className="px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-md shadow-sm hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500"
        >
          Cancel
        </button>
        <button
          type="submit"
          disabled={isPending}
          className="px-4 py-2 text-sm font-medium text-white bg-blue-600 border border-transparent rounded-md shadow-sm hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 disabled:opacity-50 disabled:cursor-not-allowed"
        >
          {isPending ? (
            <span className="flex items-center">
              <svg
                className="animate-spin -ml-1 mr-2 h-4 w-4 text-white"
                xmlns="http://www.w3.org/2000/svg"
                fill="none"
                viewBox="0 0 24 24"
              >
                <circle
                  className="opacity-25"
                  cx="12"
                  cy="12"
                  r="10"
                  stroke="currentColor"
                  strokeWidth="4"
                />
                <path
                  className="opacity-75"
                  fill="currentColor"
                  d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"
                />
              </svg>
              Saving...
            </span>
          ) : mode === 'create' ? (
            'Create Model'
          ) : (
            'Save Changes'
          )}
        </button>
      </div>
      </form>
    </FormProvider>
  )
}
