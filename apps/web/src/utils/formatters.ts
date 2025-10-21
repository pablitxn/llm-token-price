import { formatDistanceToNow } from 'date-fns'

/**
 * Formats a date as relative time from now (e.g., "2 days ago", "1 month ago").
 * Story 2.12: Used for displaying timestamp freshness in admin and public UI.
 *
 * @param date - The date to format (string or Date object)
 * @returns Relative time string with "ago" suffix
 *
 * @example
 * formatRelativeTime(new Date('2025-10-19')) // "2 days ago"
 * formatRelativeTime('2025-09-21') // "1 month ago"
 */
export function formatRelativeTime(date: string | Date): string {
  return formatDistanceToNow(new Date(date), { addSuffix: true })
}

/**
 * Formats a date as an absolute timestamp for tooltips and detailed views.
 * Uses US locale format: "Oct 16, 2025, 2:30 PM"
 *
 * @param date - The date to format (string or Date object)
 * @returns Formatted absolute timestamp string
 *
 * @example
 * formatTimestamp(new Date('2025-10-21T14:30:00')) // "Oct 21, 2025, 2:30 PM"
 */
export function formatTimestamp(date: string | Date): string {
  return new Date(date).toLocaleDateString('en-US', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  })
}

/**
 * Calculates the number of days since a given date.
 * Used for determining freshness categories and stale model detection.
 *
 * @param date - The date to calculate from (string or Date object)
 * @returns Number of days elapsed (always positive, rounded down)
 *
 * @example
 * getDaysSince('2025-10-19') // 2 (if today is 2025-10-21)
 * getDaysSince(new Date('2025-09-21')) // 30
 */
export function getDaysSince(date: string | Date): number {
  const now = new Date()
  const past = new Date(date)
  const diffMs = now.getTime() - past.getTime()
  return Math.floor(diffMs / (1000 * 60 * 60 * 24))
}

/**
 * Categorizes a date into freshness status based on age thresholds.
 * Story 2.12 freshness categories:
 * - Fresh: < 7 days (data is recent and reliable)
 * - Stale: 7-30 days (needs review soon)
 * - Critical: > 30 days (urgent update required)
 *
 * @param date - The date to categorize (string or Date object)
 * @returns Freshness status: 'fresh' | 'stale' | 'critical'
 *
 * @example
 * getFreshnessStatus('2025-10-20') // 'fresh' (1 day old)
 * getFreshnessStatus('2025-10-10') // 'stale' (11 days old)
 * getFreshnessStatus('2025-09-15') // 'critical' (36 days old)
 */
export function getFreshnessStatus(date: string | Date): 'fresh' | 'stale' | 'critical' {
  const days = getDaysSince(date)
  if (days < 7) return 'fresh'
  if (days < 30) return 'stale'
  return 'critical'
}
