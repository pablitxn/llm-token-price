/**
 * Zod validation schema for benchmark creation and update forms
 * Validates CreateBenchmarkRequest and UpdateBenchmarkRequest structures with client-side rules
 * Server-side validation with FluentValidation ensures data integrity
 */

import { z } from 'zod'

/**
 * Benchmark category enum values
 * Must match BenchmarkCategory enum in backend (Domain.Enums)
 */
export const benchmarkCategories = [
  'Reasoning',
  'Code',
  'Math',
  'Language',
  'Multimodal',
] as const

/**
 * Benchmark interpretation enum values
 * Must match BenchmarkInterpretation enum in backend (Domain.Enums)
 */
export const benchmarkInterpretations = [
  'HigherBetter',
  'LowerBetter',
] as const

/**
 * Validation schema for creating a new benchmark
 * Enforces:
 * - Required fields: benchmarkName, fullName, category, interpretation, typicalRange, weightInQaps
 * - BenchmarkName: 1-50 chars, alphanumeric + underscore only
 * - FullName: 1-255 chars
 * - Description: optional, max 1000 chars
 * - Category: one of 5 enum values
 * - Interpretation: one of 2 enum values
 * - TypicalRangeMin < TypicalRangeMax
 * - WeightInQaps: 0.00-1.00, max 2 decimal places
 */
export const createBenchmarkSchema = z
  .object({
    benchmarkName: z
      .string()
      .min(1, 'Benchmark name is required')
      .max(50, 'Benchmark name must be 50 characters or less')
      .regex(
        /^[a-zA-Z0-9_]+$/,
        'Benchmark name can only contain letters, numbers, and underscores'
      ),
    fullName: z
      .string()
      .min(1, 'Full name is required')
      .max(255, 'Full name must be 255 characters or less'),
    description: z
      .string()
      .max(1000, 'Description must be 1000 characters or less')
      .optional()
      .or(z.literal('')),
    category: z.enum(benchmarkCategories, {
      message: 'Please select a valid category',
    }),
    interpretation: z.enum(benchmarkInterpretations, {
      message: 'Please select a valid interpretation',
    }),
    typicalRangeMin: z
      .number({
        message: 'Typical range minimum must be a number',
      })
      .finite('Typical range minimum must be a valid number'),
    typicalRangeMax: z
      .number({
        message: 'Typical range maximum must be a number',
      })
      .finite('Typical range maximum must be a valid number'),
    weightInQaps: z
      .number({
        message: 'QAPS weight must be a number',
      })
      .min(0, 'QAPS weight must be between 0.00 and 1.00')
      .max(1, 'QAPS weight must be between 0.00 and 1.00')
      .multipleOf(0.01, 'QAPS weight can have at most 2 decimal places')
      .default(0),
  })
  .refine((data) => data.typicalRangeMin < data.typicalRangeMax, {
    message: 'Typical range minimum must be less than maximum',
    path: ['typicalRangeMax'], // Error associated with max field
  })

/**
 * Validation schema for updating an existing benchmark
 * Same as create schema except benchmarkName is excluded (immutable)
 */
export const updateBenchmarkSchema = z
  .object({
    fullName: z
      .string()
      .min(1, 'Full name is required')
      .max(255, 'Full name must be 255 characters or less'),
    description: z
      .string()
      .max(1000, 'Description must be 1000 characters or less')
      .optional()
      .or(z.literal('')),
    category: z.enum(benchmarkCategories, {
      message: 'Please select a valid category',
    }),
    interpretation: z.enum(benchmarkInterpretations, {
      message: 'Please select a valid interpretation',
    }),
    typicalRangeMin: z
      .number({
        message: 'Typical range minimum must be a number',
      })
      .finite('Typical range minimum must be a valid number'),
    typicalRangeMax: z
      .number({
        message: 'Typical range maximum must be a number',
      })
      .finite('Typical range maximum must be a valid number'),
    weightInQaps: z
      .number({
        message: 'QAPS weight must be a number',
      })
      .min(0, 'QAPS weight must be between 0.00 and 1.00')
      .max(1, 'QAPS weight must be between 0.00 and 1.00')
      .multipleOf(0.01, 'QAPS weight can have at most 2 decimal places'),
  })
  .refine((data) => data.typicalRangeMin < data.typicalRangeMax, {
    message: 'Typical range minimum must be less than maximum',
    path: ['typicalRangeMax'],
  })

/**
 * TypeScript types inferred from Zod schemas
 */
export type CreateBenchmarkFormData = z.infer<typeof createBenchmarkSchema>
export type UpdateBenchmarkFormData = z.infer<typeof updateBenchmarkSchema>

/**
 * Benchmark response DTO type (matches backend BenchmarkResponseDto)
 */
export interface BenchmarkResponseDto {
  id: string
  benchmarkName: string
  fullName: string
  description?: string | null
  category: (typeof benchmarkCategories)[number]
  interpretation: (typeof benchmarkInterpretations)[number]
  typicalRangeMin: number
  typicalRangeMax: number
  weightInQaps: number
  createdAt: string
  isActive: boolean
}
