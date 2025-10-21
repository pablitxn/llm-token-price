/**
 * Zod validation schema for benchmark score creation and update forms
 * Validates CreateBenchmarkScoreDto structure with client-side rules
 * Server-side validation with FluentValidation ensures data integrity
 */

import { z } from 'zod'

/**
 * Validation schema for creating/updating a benchmark score
 * Enforces:
 * - Required fields: benchmarkId, score
 * - BenchmarkId: must be a valid UUID
 * - Score: must be a finite number
 * - MaxScore: optional, must be >= Score when provided
 * - TestDate: optional ISO 8601 date string
 * - SourceUrl: optional, must be valid HTTP/HTTPS URL
 * - Verified: optional boolean (defaults to false)
 * - Notes: optional, max 500 chars
 */
export const createBenchmarkScoreSchema = z
  .object({
    benchmarkId: z
      .string()
      .min(1, 'Benchmark is required')
      .uuid('Invalid benchmark ID format'),
    score: z
      .number({
        message: 'Score must be a number',
      })
      .finite('Score must be a valid number'),
    maxScore: z
      .number({
        message: 'Max score must be a number',
      })
      .finite('Max score must be a valid number')
      .nullable()
      .optional(),
    testDate: z
      .string()
      .datetime({ message: 'Test date must be a valid ISO 8601 datetime' })
      .nullable()
      .optional()
      .or(z.literal('')),
    sourceUrl: z
      .string()
      .url('Source URL must be a valid HTTP/HTTPS URL')
      .nullable()
      .optional()
      .or(z.literal('')),
    verified: z.boolean().optional().default(false),
    notes: z
      .string()
      .max(500, 'Notes must be 500 characters or less')
      .nullable()
      .optional()
      .or(z.literal('')),
  })
  .strict()
  // Cross-field validation: Score <= MaxScore
  .refine(
    (data) => {
      if (data.maxScore !== null && data.maxScore !== undefined) {
        return data.score <= data.maxScore
      }
      return true
    },
    {
      message: 'Score cannot exceed max score',
      path: ['score'], // Error attached to score field
    }
  )

/**
 * TypeScript type inferred from createBenchmarkScoreSchema
 * Use this type for form state in React components
 */
export type CreateBenchmarkScoreFormData = z.infer<typeof createBenchmarkScoreSchema>

/**
 * Validation schema for updating a benchmark score
 * Same rules as create (all fields can be changed)
 */
export const updateBenchmarkScoreSchema = createBenchmarkScoreSchema

/**
 * TypeScript type for update form data
 */
export type UpdateBenchmarkScoreFormData = z.infer<typeof updateBenchmarkScoreSchema>
