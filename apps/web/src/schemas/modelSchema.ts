/**
 * Zod validation schema for model creation form
 * Validates CreateModelRequest structure with client-side rules
 * Server-side validation with FluentValidation happens in Story 2.5
 */

import { z } from 'zod'

/**
 * Validation schema for creating a new model
 * Enforces:
 * - Required fields: name, provider, status, prices, currency
 * - Price validation: positive numbers with max 6 decimal places
 * - String length limits: name (255), provider (100), version (50)
 * - Date validation: valid_from < valid_to if both provided
 */
export const createModelSchema = z
  .object({
    name: z
      .string()
      .min(1, 'Name is required')
      .max(255, 'Name must be 255 characters or less'),
    provider: z
      .string()
      .min(1, 'Provider is required')
      .max(100, 'Provider must be 100 characters or less'),
    version: z
      .string()
      .max(50, 'Version must be 50 characters or less')
      .optional()
      .or(z.literal('')),
    releaseDate: z
      .string()
      .optional()
      .or(z.literal('')),
    status: z.enum(['active', 'deprecated', 'beta'], {
      message: 'Status must be active, deprecated, or beta',
    }),
    inputPricePer1M: z
      .number({
        message: 'Input price must be a number',
      })
      .positive('Input price must be greater than 0')
      .multipleOf(0.000001, 'Input price can have at most 6 decimal places'),
    outputPricePer1M: z
      .number({
        message: 'Output price must be a number',
      })
      .positive('Output price must be greater than 0')
      .multipleOf(0.000001, 'Output price can have at most 6 decimal places'),
    currency: z.enum(['USD', 'EUR', 'GBP'], {
      message: 'Currency must be USD, EUR, or GBP',
    }),
    pricingValidFrom: z
      .string()
      .optional()
      .or(z.literal('')),
    pricingValidTo: z
      .string()
      .optional()
      .or(z.literal('')),
    capabilities: z
      .object({
        contextWindow: z
          .number({
            message: 'Context window must be a number',
          })
          .int('Context window must be an integer')
          .min(1000, 'Context window must be at least 1,000 tokens')
          .max(2000000, 'Context window cannot exceed 2,000,000 tokens'),
        maxOutputTokens: z
          .number({
            message: 'Max output tokens must be a number',
          })
          .int('Max output tokens must be an integer')
          .positive('Max output tokens must be positive')
          .optional()
          .nullable(),
        supportsFunctionCalling: z.boolean().default(false),
        supportsVision: z.boolean().default(false),
        supportsAudioInput: z.boolean().default(false),
        supportsAudioOutput: z.boolean().default(false),
        supportsStreaming: z.boolean().default(true),
        supportsJsonMode: z.boolean().default(false),
      })
      .refine(
        (data) => {
          // Cross-field validation: maxOutputTokens <= contextWindow
          if (data.maxOutputTokens && data.contextWindow) {
            return data.maxOutputTokens <= data.contextWindow
          }
          return true
        },
        {
          message: 'Max output tokens cannot exceed context window',
          path: ['maxOutputTokens'],
        }
      ),
  })
  .refine(
    (data) => {
      // Date validation: valid_from < valid_to if both provided
      if (data.pricingValidFrom && data.pricingValidTo) {
        return new Date(data.pricingValidFrom) < new Date(data.pricingValidTo)
      }
      return true
    },
    {
      message: 'Valid From date must be before Valid To date',
      path: ['pricingValidTo'],
    }
  )

/**
 * TypeScript type inferred from Zod schema
 * Use this type for form values
 */
export type CreateModelFormValues = z.infer<typeof createModelSchema>
