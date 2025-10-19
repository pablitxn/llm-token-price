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
