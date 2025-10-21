/**
 * Type definitions for Audit Log feature
 * Story 2.13 Task 14: Implement Audit Log
 */

/**
 * Audit log entry DTO from backend
 * Represents a single audit trail record for admin CRUD operations
 */
export interface AuditLogDto {
  /**
   * Unique identifier for the audit log entry
   */
  id: string

  /**
   * Timestamp when the action occurred (UTC, ISO 8601 format)
   */
  timestamp: string

  /**
   * User identifier who performed the action (email or username)
   */
  userId: string

  /**
   * Action performed: "Create", "Update", "Delete", "Import"
   */
  action: 'Create' | 'Update' | 'Delete' | 'Import'

  /**
   * Entity type affected: "Model", "Benchmark", "BenchmarkScore"
   */
  entityType: 'Model' | 'Benchmark' | 'BenchmarkScore'

  /**
   * Unique identifier of the affected entity
   */
  entityId: string

  /**
   * JSON string representing the entity BEFORE the operation
   * NULL for Create actions (no previous state)
   */
  oldValues: string | null

  /**
   * JSON string representing the entity AFTER the operation
   * NULL for Delete actions (entity no longer exists)
   */
  newValues: string | null
}

/**
 * Audit log filter parameters for API queries
 */
export interface AuditLogFilters {
  /**
   * Filter by user identifier (email or username)
   */
  userId?: string

  /**
   * Filter by entity type
   */
  entityType?: string

  /**
   * Filter by action
   */
  action?: string

  /**
   * Filter for audit logs after this date (inclusive, ISO 8601 format)
   */
  startDate?: string

  /**
   * Filter for audit logs before this date (inclusive, ISO 8601 format)
   */
  endDate?: string

  /**
   * Page number (1-indexed)
   */
  page?: number

  /**
   * Page size (default: 20, max: 100)
   */
  pageSize?: number
}

/**
 * Pagination metadata from backend
 */
export interface PaginationMetadata {
  currentPage: number
  pageSize: number
  totalItems: number
  totalPages: number
  hasNextPage: boolean
  hasPreviousPage: boolean
}

/**
 * Paginated audit log response
 */
export interface PagedAuditLogResult {
  items: AuditLogDto[]
  pagination: PaginationMetadata
}

/**
 * API response wrapper for audit logs
 */
export interface AuditLogResponse {
  data: PagedAuditLogResult
  meta: {
    timestamp: string
  }
}
