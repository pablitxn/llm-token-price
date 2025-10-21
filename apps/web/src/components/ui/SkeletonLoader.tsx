export interface SkeletonLoaderProps {
  rows?: number
  columns?: number
  className?: string
}

/**
 * Skeleton loader for table data while loading
 * Story 2.13 Task 9.2: Skeleton placeholders for better perceived performance
 */
export function SkeletonLoader({
  rows = 5,
  columns = 4,
  className = '',
}: SkeletonLoaderProps) {
  return (
    <div className={`space-y-3 ${className}`} role="status" aria-label="Loading data">
      {Array.from({ length: rows }).map((_, rowIndex) => (
        <div
          key={rowIndex}
          className="grid gap-4 animate-pulse"
          style={{ gridTemplateColumns: `repeat(${columns}, 1fr)` }}
        >
          {Array.from({ length: columns }).map((_, colIndex) => (
            <div
              key={colIndex}
              className="h-6 bg-gray-200 rounded"
              style={{
                width: colIndex === 0 ? '80%' : '100%', // First column slightly narrower
              }}
            />
          ))}
        </div>
      ))}
      <span className="sr-only">Loading...</span>
    </div>
  )
}

/**
 * Skeleton loader for single table row
 * Useful for inline loading states when adding/updating individual rows
 */
export function SkeletonRow({ columns = 4 }: { columns?: number }) {
  return (
    <tr className="animate-pulse">
      {Array.from({ length: columns }).map((_, index) => (
        <td key={index} className="px-6 py-4">
          <div className="h-4 bg-gray-200 rounded w-3/4" />
        </td>
      ))}
    </tr>
  )
}

/**
 * Skeleton loader for card-based layouts
 * Alternative to table skeleton for grid/card views
 */
export function SkeletonCard({ className = '' }: { className?: string }) {
  return (
    <div className={`border border-gray-200 rounded-lg p-6 space-y-3 animate-pulse ${className}`}>
      <div className="h-6 bg-gray-200 rounded w-3/4" />
      <div className="h-4 bg-gray-200 rounded w-1/2" />
      <div className="h-4 bg-gray-200 rounded w-full" />
      <div className="h-4 bg-gray-200 rounded w-full" />
      <div className="flex gap-2 mt-4">
        <div className="h-8 bg-gray-200 rounded w-20" />
        <div className="h-8 bg-gray-200 rounded w-20" />
      </div>
    </div>
  )
}
