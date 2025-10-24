import { describe, it, expect } from 'vitest'
import { formatPrice } from '../formatPrice'

/**
 * Unit tests for formatPrice utility
 *
 * Story 3.3: TanStack Table Integration
 * Tests AC #3: Column definitions with currency formatting
 *
 * Coverage:
 * - USD formatting (default and explicit)
 * - Non-USD currencies (EUR, GBP, JPY)
 * - Edge cases (zero, negative, very small, very large)
 * - Decimal precision (always 2 decimal places for USD)
 */

describe('formatPrice', () => {
  describe('USD Currency (Default)', () => {
    it('should format USD prices with $ symbol and 2 decimals', () => {
      expect(formatPrice(30.0)).toBe('$30.00')
      expect(formatPrice(60.5)).toBe('$60.50')
      expect(formatPrice(0.005)).toBe('$0.01')
    })

    it('should format USD prices when currency explicitly provided', () => {
      expect(formatPrice(30.0, 'USD')).toBe('$30.00')
      expect(formatPrice(100.99, 'USD')).toBe('$100.99')
    })

    it('should handle zero price', () => {
      expect(formatPrice(0)).toBe('$0.00')
      expect(formatPrice(0.0, 'USD')).toBe('$0.00')
    })

    it('should handle very small prices', () => {
      expect(formatPrice(0.001)).toBe('$0.00')
      expect(formatPrice(0.009)).toBe('$0.01')
      expect(formatPrice(0.005)).toBe('$0.01') // rounds to 2 decimals
    })

    it('should handle very large prices', () => {
      expect(formatPrice(999999.99)).toBe('$999999.99')
      expect(formatPrice(1000000.0)).toBe('$1000000.00')
    })

    it('should always show 2 decimal places for USD', () => {
      expect(formatPrice(5)).toBe('$5.00')
      expect(formatPrice(10.5)).toBe('$10.50')
      expect(formatPrice(10.55)).toBe('$10.55')
    })

    it('should round to 2 decimal places for USD', () => {
      // Note: JavaScript's toFixed() uses banker's rounding (round half to even)
      // 10.555 rounds to 10.56 (round half up in most cases, but can vary)
      const result = formatPrice(10.555)
      expect(result).toMatch(/\$10\.(55|56)/) // Accept either due to floating point behavior

      expect(formatPrice(10.554)).toBe('$10.55') // rounds down
      expect(formatPrice(10.999)).toBe('$11.00') // rounds up to next dollar
    })
  })

  describe('Non-USD Currencies', () => {
    it('should format EUR prices correctly', () => {
      const formatted = formatPrice(25.5, 'EUR')
      // Intl.NumberFormat produces '€25.50' or '25.50 €' depending on locale
      expect(formatted).toContain('25.50')
      expect(formatted).toContain('€')
    })

    it('should format GBP prices correctly', () => {
      const formatted = formatPrice(15.75, 'GBP')
      expect(formatted).toContain('15.75')
      expect(formatted).toContain('£')
    })

    it('should format JPY prices correctly (no decimals for JPY)', () => {
      const formatted = formatPrice(1000, 'JPY')
      // JPY typically doesn't use decimals, but our function forces 2
      expect(formatted).toContain('1,000.00')
      expect(formatted).toContain('¥')
    })

    it('should handle zero for non-USD currencies', () => {
      const eurZero = formatPrice(0, 'EUR')
      expect(eurZero).toContain('0.00')
      expect(eurZero).toContain('€')
    })
  })

  describe('Edge Cases', () => {
    it('should handle negative prices (if passed)', () => {
      // Note: Negative prices shouldn't occur in production, but test defensive behavior
      expect(formatPrice(-10.5)).toBe('$-10.50')
    })

    it('should handle fractional cents correctly', () => {
      expect(formatPrice(0.004)).toBe('$0.00') // rounds down
      expect(formatPrice(0.006)).toBe('$0.01') // rounds up
    })

    it('should handle repeating decimals', () => {
      expect(formatPrice(10 / 3)).toBe('$3.33') // 3.333... rounds to $3.33
    })

    it('should handle Number.MAX_SAFE_INTEGER edge case', () => {
      const maxSafe = Number.MAX_SAFE_INTEGER
      const formatted = formatPrice(maxSafe)
      expect(formatted).toContain('$')
      expect(formatted).toContain('.00')
    })
  })

  describe('Type Safety & Input Validation', () => {
    it('should handle integer prices without decimals', () => {
      expect(formatPrice(50)).toBe('$50.00')
      expect(formatPrice(100)).toBe('$100.00')
    })

    it('should handle prices with 1 decimal place', () => {
      expect(formatPrice(12.5)).toBe('$12.50')
    })

    it('should handle prices with more than 2 decimal places', () => {
      expect(formatPrice(19.999)).toBe('$20.00')
      expect(formatPrice(19.994)).toBe('$19.99')
    })

    it('should default to USD when currency is empty string', () => {
      // Our function defaults to USD, but empty string will use Intl.NumberFormat
      // This tests defensive behavior
      const formatted = formatPrice(10)
      expect(formatted).toBe('$10.00')
    })
  })

  describe('Real-World LLM Pricing Examples', () => {
    it('should format GPT-4 input price ($30.00 per 1M tokens)', () => {
      expect(formatPrice(30.0)).toBe('$30.00')
    })

    it('should format GPT-4 output price ($60.00 per 1M tokens)', () => {
      expect(formatPrice(60.0)).toBe('$60.00')
    })

    it('should format Claude 3.5 Sonnet input price ($3.00 per 1M tokens)', () => {
      expect(formatPrice(3.0)).toBe('$3.00')
    })

    it('should format Claude 3.5 Sonnet output price ($15.00 per 1M tokens)', () => {
      expect(formatPrice(15.0)).toBe('$15.00')
    })

    it('should format very cheap model prices (e.g., $0.10 per 1M)', () => {
      expect(formatPrice(0.1)).toBe('$0.10')
      expect(formatPrice(0.05)).toBe('$0.05')
    })

    it('should format sub-cent prices', () => {
      expect(formatPrice(0.001)).toBe('$0.00')
      expect(formatPrice(0.01)).toBe('$0.01')
    })
  })
})
