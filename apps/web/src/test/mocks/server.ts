/**
 * MSW server setup for Node.js (Vitest) tests
 */

import { setupServer } from 'msw/node'
import { beforeAll, afterEach, afterAll } from 'vitest'
import { handlers } from './handlers'

/**
 * Create MSW server with default handlers
 * This server will intercept HTTP requests during tests
 */
export const server = setupServer(...handlers)

/**
 * Start/stop server lifecycle hooks
 * Import and call these in your test setup file
 */
export function setupMswServer() {
  // Start server before all tests
  beforeAll(() => {
    server.listen({ onUnhandledRequest: 'warn' })
  })

  // Reset handlers after each test to ensure test isolation
  afterEach(() => {
    server.resetHandlers()
  })

  // Clean up after all tests are done
  afterAll(() => {
    server.close()
  })
}
