/**
 * Authentication Fixtures
 * Story 2.12 E2E Test Support
 * Knowledge Base: fixture-architecture.md
 *
 * Provides authenticated page fixtures for admin tests.
 * Auto-cleanup after test completion.
 */

import { test as base, Page } from '@playwright/test';
import axios from 'axios';

const API_BASE_URL = process.env.API_BASE_URL || 'http://localhost:5000/api';
const ADMIN_API_URL = `${API_BASE_URL}/admin`;

/**
 * Admin credentials for test environment
 * Note: These should be configured via environment variables
 */
const ADMIN_USERNAME = process.env.ADMIN_USERNAME || 'admin@test.com';
const ADMIN_PASSWORD = process.env.ADMIN_PASSWORD || 'TestPassword123!';

/**
 * Authenticate as admin user
 * Navigates to login page, fills credentials, submits form
 * Waits for successful authentication and dashboard navigation
 *
 * @param page - Playwright Page object
 * @returns Promise that resolves when authenticated
 *
 * @example
 * ```typescript
 * test('admin dashboard test', async ({ page }) => {
 *   await authenticateAsAdmin(page);
 *   await page.goto('/admin/dashboard');
 *   // Test authenticated admin functionality
 * });
 * ```
 */
export async function authenticateAsAdmin(page: Page): Promise<void> {
  // Navigate to login page
  await page.goto('/admin/login');

  // Fill login form
  await page.fill('[data-testid="email-input"]', ADMIN_USERNAME);
  await page.fill('[data-testid="password-input"]', ADMIN_PASSWORD);

  // Submit form
  await page.click('[data-testid="login-button"]');

  // Wait for successful authentication
  await page.waitForURL('/admin/dashboard', { timeout: 10000 });

  // Verify authentication by checking for user menu or logout button
  await page.waitForSelector('[data-testid="admin-user-menu"]', { timeout: 5000 });
}

/**
 * Extended Playwright test with authenticated admin fixture
 * Auto-authenticates before each test, auto-cleanup after
 *
 * @example
 * ```typescript
 * import { test } from './support/fixtures/auth.fixture';
 *
 * test('dashboard shows metrics', async ({ authenticatedAdminPage }) => {
 *   // Page is already authenticated as admin
 *   await authenticatedAdminPage.goto('/admin/dashboard');
 *   await expect(authenticatedAdminPage.locator('[data-testid="metric-card"]')).toBeVisible();
 * });
 * ```
 */
export const test = base.extend<{ authenticatedAdminPage: Page }>({
  authenticatedAdminPage: async ({ page }, use) => {
    // Setup: Authenticate as admin
    await authenticateAsAdmin(page);

    // Provide authenticated page to test
    await use(page);

    // Cleanup: Logout (optional - browser context will be destroyed anyway)
    try {
      await page.goto('/admin/logout');
    } catch (error) {
      // Ignore errors during cleanup
      console.warn('Logout failed during cleanup:', error);
    }
  },
});

/**
 * Get admin authentication token via API
 * Useful for API tests that need admin credentials
 *
 * @returns Admin JWT token
 *
 * @example
 * ```typescript
 * const token = await getAdminAuthToken();
 * const response = await request.get('/admin/models', {
 *   headers: { 'Authorization': `Bearer ${token}` }
 * });
 * ```
 */
export async function getAdminAuthToken(): Promise<string> {
  try {
    const response = await axios.post(`${ADMIN_API_URL}/auth/login`, {
      username: ADMIN_USERNAME,
      password: ADMIN_PASSWORD,
    });

    // Backend should set HttpOnly cookie, but may also return token
    if (response.data.token) {
      return response.data.token;
    }

    // If token not in response, extract from cookie
    const setCookie = response.headers['set-cookie'];
    if (setCookie) {
      const tokenCookie = setCookie.find((cookie: string) =>
        cookie.startsWith('admin_token=')
      );
      if (tokenCookie) {
        const token = tokenCookie.split(';')[0].split('=')[1];
        return token;
      }
    }

    throw new Error('No admin token found in login response');
  } catch (error) {
    console.error('Failed to get admin auth token:', error);
    throw error;
  }
}

/**
 * Logout admin user
 * Clears authentication state
 *
 * @param page - Playwright Page object
 */
export async function logoutAdmin(page: Page): Promise<void> {
  await page.goto('/admin/logout');
  await page.waitForURL('/admin/login', { timeout: 5000 });
}
