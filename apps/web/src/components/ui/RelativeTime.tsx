import { CheckCircle, Clock, AlertTriangle } from 'lucide-react'
import { formatRelativeTime, formatTimestamp, getFreshnessStatus } from '@/utils/formatters'
import type { FreshnessStatus } from '@/types/timestamp'

/**
 * RelativeTime Component
 * Story 2.12: Displays relative timestamps with freshness indicators and tooltips.
 *
 * Shows "X days ago" format with optional freshness icon (green/yellow/red) based on age.
 * Displays absolute timestamp on hover via native HTML title tooltip.
 *
 * Freshness thresholds:
 * - Fresh (< 7 days): Green checkmark
 * - Stale (7-30 days): Yellow clock
 * - Critical (> 30 days): Red warning triangle
 */

export interface RelativeTimeProps {
  /** The date to display (string or Date object) */
  date: string | Date
  /** Whether to show freshness indicator icon (default: false) */
  showIcon?: boolean
  /** Optional CSS class for styling */
  className?: string
}

/**
 * Maps freshness status to Tailwind text color classes
 */
const freshnessColors: Record<FreshnessStatus, string> = {
  fresh: 'text-green-700',
  stale: 'text-yellow-700',
  critical: 'text-red-700'
}

/**
 * RelativeTime component displaying relative timestamps with freshness indicators.
 *
 * @example
 * // Simple usage (no icon)
 * <RelativeTime date="2025-10-19" />
 * // Output: "2 days ago" (if today is 2025-10-21)
 *
 * @example
 * // With freshness icon
 * <RelativeTime date="2025-09-15" showIcon />
 * // Output: 🔺 "1 month ago" (red warning for > 30 days)
 */
export function RelativeTime({ date, showIcon = false, className = '' }: RelativeTimeProps) {
  const relativeTime = formatRelativeTime(date)
  const absoluteTime = formatTimestamp(date)
  const status = getFreshnessStatus(date)

  // Freshness indicator icons
  const freshnessIcons: Record<FreshnessStatus, JSX.Element> = {
    fresh: <CheckCircle className="text-green-500" size={16} />,
    stale: <Clock className="text-yellow-500" size={16} />,
    critical: <AlertTriangle className="text-red-500" size={16} />
  }

  return (
    <span
      className={`flex items-center gap-1 ${className}`}
      title={absoluteTime}
    >
      {showIcon && freshnessIcons[status]}
      <span className={freshnessColors[status]}>
        {relativeTime}
      </span>
    </span>
  )
}
