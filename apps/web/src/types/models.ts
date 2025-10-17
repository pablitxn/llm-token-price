/**
 * TypeScript types for Model DTOs matching the backend API response structure.
 * These types are derived from the backend DTOs in LlmTokenPrice.Application/DTOs
 */

/**
 * Benchmark score information
 */
export interface BenchmarkScoreDto {
  benchmarkName: string
  score: number
  maxScore: number | null
  normalizedScore: number | null
}

/**
 * Model capability information
 */
export interface CapabilityDto {
  contextWindow: number
  maxOutputTokens: number | null
  supportsFunctionCalling: boolean
  supportsVision: boolean
  supportsAudioInput: boolean
  supportsAudioOutput: boolean
  supportsStreaming: boolean
  supportsJsonMode: boolean
}

/**
 * Complete model information with pricing, capabilities, and benchmarks
 */
export interface ModelDto {
  id: string
  name: string
  provider: string
  version: string | null
  status: string
  inputPricePer1M: number
  outputPricePer1M: number
  currency: string
  updatedAt: string
  capabilities: CapabilityDto | null
  topBenchmarks: BenchmarkScoreDto[]
}

/**
 * API response metadata
 */
export interface ApiResponseMeta {
  count?: number
  cached: boolean
  timestamp: string
}

/**
 * Standard API response wrapper
 */
export interface ApiResponse<T> {
  data: T
  meta: ApiResponseMeta
}

/**
 * Models list API response type
 */
export type ModelsResponse = ApiResponse<ModelDto[]>

/**
 * Single model API response type
 */
export type ModelResponse = ApiResponse<ModelDto>
