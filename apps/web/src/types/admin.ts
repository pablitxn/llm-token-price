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
