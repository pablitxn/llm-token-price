/**
 * Utility for formatting price values with currency symbols
 *
 * Story 3.3: TanStack Table Integration
 * Extracted from columns.tsx for testability
 */

/**
 * Formats a price value with currency symbol and proper decimal places
 *
 * @param price - The price value in dollars per 1M tokens
 * @param currency - The currency code (default: USD)
 * @returns Formatted price string (e.g., "$2.50")
 *
 * @example
 * formatPrice(30.00, 'USD') // "$30.00"
 * formatPrice(0.005, 'USD') // "$0.01"
 * formatPrice(25.50, 'EUR') // "€25.50"
 */
export const formatPrice = (price: number, currency: string = 'USD'): string => {
  if (currency === 'USD') {
    return `$${price.toFixed(2)}`
  }

  // For other currencies, use Intl.NumberFormat
  return new Intl.NumberFormat('en-US', {
    style: 'currency',
    currency: currency,
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  }).format(price)
}
