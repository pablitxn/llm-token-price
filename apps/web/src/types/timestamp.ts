/**
 * Timestamp-related type definitions for Story 2.12
 */

/**
 * Freshness status categories based on data age.
 * Used for UI styling and stale data highlighting.
 *
 * Thresholds:
 * - fresh: < 7 days old (data is recent and reliable)
 * - stale: 7-30 days old (needs review soon)
 * - critical: > 30 days old (urgent update required)
 */
export type FreshnessStatus = 'fresh' | 'stale' | 'critical'
