/**
 * TypeScript types for Admin Model DTOs matching the backend Admin API response structure.
 * These types extend the public ModelDto with additional admin-specific fields.
 * Derived from backend AdminModelDto in LlmTokenPrice.Application/DTOs/AdminModelDto.cs
 */

import type { BenchmarkScoreDto, CapabilityDto } from './models'

/**
 * Complete model information for admin panel with full audit fields
 * Extends public ModelDto with:
 * - createdAt: Creation timestamp
 * - isActive: Soft delete flag
 * Includes ALL models (active, inactive, deprecated, beta)
 */
export interface AdminModelDto {
  id: string
  name: string
  provider: string
  version: string | null
  status: 'active' | 'deprecated' | 'beta'
  inputPricePer1M: number
  outputPricePer1M: number
  currency: string
  isActive: boolean
  createdAt: string // ISO 8601
  updatedAt: string // ISO 8601
  capabilities: CapabilityDto | null
  topBenchmarks: BenchmarkScoreDto[]
}

/**
 * Admin API response metadata
 * Admin endpoints are NEVER cached (cached always false)
 */
export interface AdminApiResponseMeta {
  totalCount?: number
  cached: boolean // Always false for admin endpoints
  timestamp: string
}

/**
 * Standard Admin API response wrapper
 */
export interface AdminApiResponse<T> {
  data: T
  meta: AdminApiResponseMeta
}

/**
 * Admin models list API response type
 */
export type AdminModelsResponse = AdminApiResponse<AdminModelDto[]>

/**
 * Single admin model API response type
 */
export type AdminModelResponse = AdminApiResponse<AdminModelDto>

/**
 * Create model request payload
 * Matches backend CreateModelDto structure
 * Used for POST /api/admin/models
 */
export interface CreateModelRequest {
  name: string
  provider: string
  version?: string
  releaseDate?: string // ISO 8601 date string
  status: 'active' | 'deprecated' | 'beta'
  inputPricePer1M: number
  outputPricePer1M: number
  currency: string
  pricingValidFrom?: string // ISO 8601 date string
  pricingValidTo?: string // ISO 8601 date string
}

// ========== Benchmark Score Management Types ==========

/**
 * Benchmark score response DTO with normalized score and metadata
 * Matches backend BenchmarkScoreResponseDto structure
 */
export interface BenchmarkScoreResponseDto {
  id: string
  modelId: string
  benchmarkId: string
  benchmarkName: string
  category: string
  score: number
  maxScore: number | null
  normalizedScore: number // 0-1 range for QAPS calculation
  testDate: string | null // ISO 8601
  sourceUrl: string | null
  verified: boolean
  notes: string | null
  createdAt: string // ISO 8601
  isOutOfRange: boolean // True if score outside benchmark's typical range
}

/**
 * Create benchmark score request payload
 * Matches backend CreateBenchmarkScoreDto structure
 * Used for POST /api/admin/models/{modelId}/benchmarks
 */
export interface CreateBenchmarkScoreDto {
  benchmarkId: string
  score: number
  maxScore?: number | null
  testDate?: string | null // ISO 8601 date string
  sourceUrl?: string | null
  verified?: boolean
  notes?: string | null
}

/**
 * Benchmark score list API response type
 */
export type BenchmarkScoresResponse = AdminApiResponse<BenchmarkScoreResponseDto[]>

/**
 * Single benchmark score API response type
 */
export type BenchmarkScoreResponse = AdminApiResponse<BenchmarkScoreResponseDto>

// ========== CSV Import Types (Story 2.11) ==========

/**
 * Failed row details from CSV import
 * Contains row number, error message, and original data for correction
 */
export interface FailedRow {
  rowNumber: number
  error: string
  data: Record<string, string>
}

/**
 * CSV import result response DTO
 * Matches backend CSVImportResultDto structure
 * Provides detailed success/failure statistics and error details
 */
export interface CSVImportResultDto {
  totalRows: number
  successfulImports: number
  failedImports: number
  skippedDuplicates: number
  errors: FailedRow[]
}
