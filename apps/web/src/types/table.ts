/**
 * Table-specific types and helpers for TanStack Table
 */

import { createColumnHelper } from '@tanstack/react-table'
import type { ModelDto } from '@/types/models'

/**
 * Type alias for model table data
 * Using ModelDto directly to maintain consistency with backend contract
 */
export type ModelTableData = ModelDto

/**
 * Column helper for type-safe column definitions
 * Provides accessor functions with full TypeScript inference
 */
export const modelColumnHelper = createColumnHelper<ModelTableData>()
