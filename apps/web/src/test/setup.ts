/**
 * Vitest test setup file
 * Runs before all tests to configure the testing environment
 */

import { expect, afterEach, vi } from 'vitest'
import { cleanup } from '@testing-library/react'
import '@testing-library/jest-dom'
import { setupMswServer } from './mocks/server'

// Setup MSW (Mock Service Worker) for API mocking
setupMswServer()

// Cleanup after each test case (e.g., clear jsdom)
afterEach(() => {
  cleanup()
})

// Mock window.matchMedia (not implemented in jsdom)
Object.defineProperty(window, 'matchMedia', {
  writable: true,
  value: vi.fn().mockImplementation((query) => ({
    matches: false,
    media: query,
    onchange: null,
    addListener: vi.fn(), // deprecated
    removeListener: vi.fn(), // deprecated
    addEventListener: vi.fn(),
    removeEventListener: vi.fn(),
    dispatchEvent: vi.fn(),
  })),
})

// Mock IntersectionObserver (not implemented in jsdom)
global.IntersectionObserver = class IntersectionObserver {
  constructor() {}
  disconnect() {}
  observe() {}
  takeRecords() {
    return []
  }
  unobserve() {}
} as unknown as typeof IntersectionObserver

// Mock ResizeObserver (not implemented in jsdom)
global.ResizeObserver = class ResizeObserver {
  constructor() {}
  disconnect() {}
  observe() {}
  unobserve() {}
} as unknown as typeof ResizeObserver

// Extend expect with custom matchers
expect.extend({})

// Suppress console errors during tests (optional, uncomment if needed)
// vi.spyOn(console, 'error').mockImplementation(() => {})
