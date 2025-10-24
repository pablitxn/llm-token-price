import { useState, type ReactNode } from 'react'

/**
 * Props for the Tooltip component
 *
 * Story 3.6: Add Capabilities Filters
 * Implements AC #6 (Tooltip explains what each capability means)
 */
interface TooltipProps {
  /** The tooltip content to display on hover/focus */
  content: string
  /** The element that triggers the tooltip (checkbox label, icon, etc.) */
  children: ReactNode
}

/**
 * Reusable tooltip component for displaying helpful explanations
 *
 * Story 3.6: Add Capabilities Filters
 * Implements AC #6 (Tooltip explains what each capability means)
 *
 * Features:
 * - Displays on hover (mouse) and focus (keyboard)
 * - Positioned above the trigger element
 * - Dark background for contrast
 * - Arrow pointing to trigger element
 * - Smooth fade in/out transitions
 * - Accessible with ARIA attributes
 * - Works on touch devices (tap to show)
 *
 * Usage:
 * ```tsx
 * <Tooltip content="Model can call external functions/tools">
 *   <span>Function Calling ⓘ</span>
 * </Tooltip>
 * ```
 *
 * @param props - Component props
 * @param props.content - The tooltip text to display
 * @param props.children - The element that triggers the tooltip
 */
export default function Tooltip({ content, children }: TooltipProps) {
  const [isVisible, setIsVisible] = useState(false)

  return (
    <div className="relative inline-block">
      <div
        onMouseEnter={() => setIsVisible(true)}
        onMouseLeave={() => setIsVisible(false)}
        onFocus={() => setIsVisible(true)}
        onBlur={() => setIsVisible(false)}
        // Support touch devices - tap to show/hide
        onTouchStart={() => setIsVisible(!isVisible)}
      >
        {children}
      </div>

      {/* Tooltip content - positioned above trigger element */}
      {isVisible && (
        <div
          className="absolute bottom-full left-1/2 -translate-x-1/2 mb-2 px-3 py-2 bg-gray-900 text-white text-sm rounded-md shadow-lg whitespace-normal max-w-xs z-50 transition-opacity duration-200 ease-in-out"
          role="tooltip"
          aria-live="polite"
        >
          {content}

          {/* Arrow pointing down to trigger element */}
          <div className="absolute top-full left-1/2 -translate-x-1/2 -mt-1 border-4 border-transparent border-t-gray-900" />
        </div>
      )}
    </div>
  )
}
