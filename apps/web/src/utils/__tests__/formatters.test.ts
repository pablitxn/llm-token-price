/**
 * Unit tests for timestamp formatting utilities
 * Story 2.12: Task 10.1, 10.2, 10.3
 */

import { describe, it, expect, vi, beforeAll, afterAll } from 'vitest'
import {
  formatRelativeTime,
  formatTimestamp,
  getDaysSince,
  getFreshnessStatus,
} from '../formatters'

describe('formatters - timestamp utilities', () => {
  // Mock the current date to avoid time-dependent test flakiness
  const MOCK_NOW = new Date('2025-10-21T12:00:00Z')

  beforeAll(() => {
    vi.useFakeTimers()
    vi.setSystemTime(MOCK_NOW)
  })

  afterAll(() => {
    vi.useRealTimers()
  })

  describe('formatRelativeTime', () => {
    it('should format date from 2 days ago', () => {
      const twoDaysAgo = new Date('2025-10-19T12:00:00Z')
      expect(formatRelativeTime(twoDaysAgo)).toBe('2 days ago')
    })

    it('should format date from 1 month ago', () => {
      const oneMonthAgo = new Date('2025-09-21T12:00:00Z')
      expect(formatRelativeTime(oneMonthAgo)).toBe('about 1 month ago')
    })

    it('should format date from today as "less than a minute ago"', () => {
      const now = new Date('2025-10-21T12:00:00Z')
      expect(formatRelativeTime(now)).toBe('less than a minute ago')
    })

    it('should format date from 45 days ago', () => {
      const fortyFiveDaysAgo = new Date('2025-09-06T12:00:00Z')
      expect(formatRelativeTime(fortyFiveDaysAgo)).toBe('about 2 months ago')
    })

    it('should accept ISO 8601 string', () => {
      const dateString = '2025-10-19T12:00:00Z'
      expect(formatRelativeTime(dateString)).toBe('2 days ago')
    })
  })

  describe('formatTimestamp', () => {
    it('should format date as absolute timestamp', () => {
      const date = new Date('2025-10-19T14:30:00Z')
      const result = formatTimestamp(date)
      // Format varies by locale, just check it contains key components
      expect(result).toContain('Oct')
      expect(result).toContain('19')
      expect(result).toContain('2025')
    })

    it('should accept ISO 8601 string', () => {
      const dateString = '2025-10-19T14:30:00Z'
      const result = formatTimestamp(dateString)
      expect(result).toContain('Oct')
      expect(result).toContain('19')
    })
  })

  describe('getDaysSince', () => {
    it('should return 0 for today', () => {
      const today = new Date('2025-10-21T12:00:00Z')
      expect(getDaysSince(today)).toBe(0)
    })

    it('should return 2 for date 2 days ago', () => {
      const twoDaysAgo = new Date('2025-10-19T12:00:00Z')
      expect(getDaysSince(twoDaysAgo)).toBe(2)
    })

    it('should return 7 for date 7 days ago', () => {
      const sevenDaysAgo = new Date('2025-10-14T12:00:00Z')
      expect(getDaysSince(sevenDaysAgo)).toBe(7)
    })

    it('should return 30 for date 30 days ago', () => {
      const thirtyDaysAgo = new Date('2025-09-21T12:00:00Z')
      expect(getDaysSince(thirtyDaysAgo)).toBe(30)
    })

    it('should return 45 for date 45 days ago', () => {
      const fortyFiveDaysAgo = new Date('2025-09-06T12:00:00Z')
      expect(getDaysSince(fortyFiveDaysAgo)).toBe(45)
    })

    it('should accept ISO 8601 string', () => {
      const dateString = '2025-10-19T12:00:00Z'
      expect(getDaysSince(dateString)).toBe(2)
    })
  })

  describe('getFreshnessStatus', () => {
    it('should return "fresh" for date today', () => {
      const today = new Date('2025-10-21T12:00:00Z')
      expect(getFreshnessStatus(today)).toBe('fresh')
    })

    it('should return "fresh" for date 3 days ago', () => {
      const threeDaysAgo = new Date('2025-10-18T12:00:00Z')
      expect(getFreshnessStatus(threeDaysAgo)).toBe('fresh')
    })

    it('should return "fresh" for date 6 days ago (boundary < 7)', () => {
      const sixDaysAgo = new Date('2025-10-15T12:00:00Z')
      expect(getFreshnessStatus(sixDaysAgo)).toBe('fresh')
    })

    it('should return "stale" for date exactly 7 days ago (boundary)', () => {
      const sevenDaysAgo = new Date('2025-10-14T12:00:00Z')
      expect(getFreshnessStatus(sevenDaysAgo)).toBe('stale')
    })

    it('should return "stale" for date 15 days ago', () => {
      const fifteenDaysAgo = new Date('2025-10-06T12:00:00Z')
      expect(getFreshnessStatus(fifteenDaysAgo)).toBe('stale')
    })

    it('should return "stale" for date 29 days ago (boundary < 30)', () => {
      const twentyNineDaysAgo = new Date('2025-09-22T12:00:00Z')
      expect(getFreshnessStatus(twentyNineDaysAgo)).toBe('stale')
    })

    it('should return "critical" for date exactly 30 days ago (boundary)', () => {
      const thirtyDaysAgo = new Date('2025-09-21T12:00:00Z')
      expect(getFreshnessStatus(thirtyDaysAgo)).toBe('critical')
    })

    it('should return "critical" for date 45 days ago', () => {
      const fortyFiveDaysAgo = new Date('2025-09-06T12:00:00Z')
      expect(getFreshnessStatus(fortyFiveDaysAgo)).toBe('critical')
    })

    it('should return "critical" for date 90 days ago', () => {
      const ninetyDaysAgo = new Date('2025-07-23T12:00:00Z')
      expect(getFreshnessStatus(ninetyDaysAgo)).toBe('critical')
    })

    it('should accept ISO 8601 string', () => {
      const dateString = '2025-10-19T12:00:00Z'
      expect(getFreshnessStatus(dateString)).toBe('fresh')
    })
  })
})
