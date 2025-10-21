/**
 * E2E Tests for Story 2.12: Timestamp Tracking and Display
 * Test ID: 2.12-E2E
 * Priority: P1 (High - Core admin functionality)
 *
 * Prerequisites:
 * - Backend API running on localhost:5000
 * - Database seeded with test models (varying update timestamps)
 * - Admin authentication configured
 *
 * Test Framework: Playwright
 * Knowledge Base: network-first.md, fixture-architecture.md, data-factories.md
 */

import { test, expect, Page } from '@playwright/test';
import { createTestModel, cleanupTestModels } from './support/factories/model.factory';
import { authenticateAsAdmin } from './support/fixtures/auth.fixture';

// =============================================================================
// Test Suite 1: Admin Dashboard Metrics Flow (AC #3)
// =============================================================================

test.describe('2.12-E2E-001: Admin Dashboard Freshness Metrics', () => {
  let page: Page;
  let testModels: any[];

  test.beforeEach(async ({ browser }) => {
    page = await browser.newPage();

    // GIVEN: Test data setup with varying freshness
    testModels = [
      await createTestModel({
        name: 'Fresh Model 1',
        updatedAt: new Date(Date.now() - 2 * 24 * 60 * 60 * 1000) // 2 days ago
      }),
      await createTestModel({
        name: 'Fresh Model 2',
        updatedAt: new Date(Date.now() - 5 * 24 * 60 * 60 * 1000) // 5 days ago
      }),
      await createTestModel({
        name: 'Stale Model 1',
        updatedAt: new Date(Date.now() - 15 * 24 * 60 * 60 * 1000) // 15 days ago
      }),
      await createTestModel({
        name: 'Stale Model 2',
        updatedAt: new Date(Date.now() - 20 * 24 * 60 * 60 * 1000) // 20 days ago
      }),
      await createTestModel({
        name: 'Critical Model 1',
        updatedAt: new Date(Date.now() - 45 * 24 * 60 * 60 * 1000) // 45 days ago
      }),
      await createTestModel({
        name: 'Critical Model 2',
        updatedAt: new Date(Date.now() - 60 * 24 * 60 * 60 * 1000) // 60 days ago
      }),
    ];

    // GIVEN: Admin authenticated
    await authenticateAsAdmin(page);
  });

  test.afterEach(async () => {
    // Cleanup: Delete test models
    await cleanupTestModels(testModels);
    await page.close();
  });

  test('2.12-E2E-001a: Dashboard displays freshness metric cards', async () => {
    // GIVEN: Admin on dashboard page
    await page.goto('/admin/dashboard');

    // THEN: Dashboard shows metric cards
    await expect(page.locator('[data-testid="metric-card-total"]')).toBeVisible();
    await expect(page.locator('[data-testid="metric-card-fresh"]')).toBeVisible();
    await expect(page.locator('[data-testid="metric-card-stale"]')).toBeVisible();
    await expect(page.locator('[data-testid="metric-card-critical"]')).toBeVisible();
  });

  test('2.12-E2E-001b: Dashboard shows correct metric counts', async () => {
    // GIVEN: Admin on dashboard page
    await page.goto('/admin/dashboard');

    // Wait for metrics to load
    await page.waitForSelector('[data-testid="metric-card-total"]');

    // THEN: Total models count is correct (6 test models + existing)
    const totalCount = await page.locator('[data-testid="metric-total-count"]').textContent();
    expect(parseInt(totalCount!)).toBeGreaterThanOrEqual(6);

    // THEN: Fresh count shows models < 7 days old (2 fresh models)
    const freshCount = await page.locator('[data-testid="metric-fresh-count"]').textContent();
    expect(parseInt(freshCount!)).toBeGreaterThanOrEqual(2);

    // THEN: Stale count shows models 7-30 days old (2 stale models)
    const staleCount = await page.locator('[data-testid="metric-stale-count"]').textContent();
    expect(parseInt(staleCount!)).toBeGreaterThanOrEqual(2);

    // THEN: Critical count shows models > 30 days old (2 critical models)
    const criticalCount = await page.locator('[data-testid="metric-critical-count"]').textContent();
    expect(parseInt(criticalCount!)).toBeGreaterThanOrEqual(2);
  });

  test('2.12-E2E-001c: Clicking "Critical Updates" navigates to filtered model list', async () => {
    // GIVEN: Admin on dashboard page
    await page.goto('/admin/dashboard');

    // Wait for metrics to load
    await page.waitForSelector('[data-testid="metric-card-critical"]');

    // WHEN: Admin clicks "Critical Updates" metric card
    await page.click('[data-testid="metric-card-critical"]');

    // THEN: Navigates to models page with freshness=critical filter
    await expect(page).toHaveURL(/\/admin\/models\?freshness=critical/);

    // THEN: Model list shows only critical models
    await expect(page.locator('[data-testid="model-row"]')).toHaveCount(2);

    // THEN: All displayed models have red warning indicators
    const rows = await page.locator('[data-testid="model-row"]').all();
    for (const row of rows) {
      await expect(row.locator('[data-testid="freshness-icon-critical"]')).toBeVisible();
    }
  });

  test('2.12-E2E-001d: Clicking "Stale" metric navigates to filtered list', async () => {
    // GIVEN: Admin on dashboard page
    await page.goto('/admin/dashboard');

    // WHEN: Admin clicks "Needs Update" (stale) metric card
    await page.click('[data-testid="metric-card-stale"]');

    // THEN: Navigates to models page with freshness=stale filter
    await expect(page).toHaveURL(/\/admin\/models\?freshness=stale/);

    // THEN: Model list shows only stale models
    await expect(page.locator('[data-testid="model-row"]')).toHaveCount(2);

    // THEN: All displayed models have yellow warning indicators
    const rows = await page.locator('[data-testid="model-row"]').all();
    for (const row of rows) {
      await expect(row.locator('[data-testid="freshness-icon-stale"]')).toBeVisible();
    }
  });
});

// =============================================================================
// Test Suite 2: Admin Model List Freshness Filtering (AC #1, #2)
// =============================================================================

test.describe('2.12-E2E-002: Admin Model List Freshness Filtering', () => {
  let page: Page;
  let testModels: any[];

  test.beforeEach(async ({ browser }) => {
    page = await browser.newPage();

    // GIVEN: Test models with varying freshness
    testModels = [
      await createTestModel({
        name: 'Alpha Fresh',
        provider: 'TestProvider',
        updatedAt: new Date(Date.now() - 1 * 24 * 60 * 60 * 1000) // 1 day ago
      }),
      await createTestModel({
        name: 'Beta Stale',
        provider: 'TestProvider',
        updatedAt: new Date(Date.now() - 10 * 24 * 60 * 60 * 1000) // 10 days ago
      }),
      await createTestModel({
        name: 'Gamma Critical',
        provider: 'TestProvider',
        updatedAt: new Date(Date.now() - 40 * 24 * 60 * 60 * 1000) // 40 days ago
      }),
    ];

    // GIVEN: Admin authenticated
    await authenticateAsAdmin(page);
  });

  test.afterEach(async () => {
    await cleanupTestModels(testModels);
    await page.close();
  });

  test('2.12-E2E-002a: Model list displays "Last Updated" column', async () => {
    // GIVEN: Admin on models page
    await page.goto('/admin/models');

    // THEN: Table header shows "Last Updated" column
    await expect(page.locator('[data-testid="column-header-updated"]')).toBeVisible();
    await expect(page.locator('[data-testid="column-header-updated"]')).toHaveText(/Last Updated/i);
  });

  test('2.12-E2E-002b: Timestamps display with relative formatting', async () => {
    // GIVEN: Admin on models page
    await page.goto('/admin/models');

    // Wait for models to load
    await page.waitForSelector('[data-testid="model-row"]');

    // THEN: Fresh model shows "1 day ago" or similar
    const freshRow = page.locator('[data-testid="model-row"]', { hasText: 'Alpha Fresh' });
    await expect(freshRow.locator('[data-testid="last-updated"]')).toContainText(/day ago/i);

    // THEN: Stale model shows "10 days ago" or similar
    const staleRow = page.locator('[data-testid="model-row"]', { hasText: 'Beta Stale' });
    await expect(staleRow.locator('[data-testid="last-updated"]')).toContainText(/days ago/i);

    // THEN: Critical model shows "about 1 month ago" or similar
    const criticalRow = page.locator('[data-testid="model-row"]', { hasText: 'Gamma Critical' });
    await expect(criticalRow.locator('[data-testid="last-updated"]')).toContainText(/(month|days) ago/i);
  });

  test('2.12-E2E-002c: Hovering timestamp shows exact datetime tooltip', async () => {
    // GIVEN: Admin on models page
    await page.goto('/admin/models');
    await page.waitForSelector('[data-testid="model-row"]');

    // WHEN: Hovering over timestamp
    const firstTimestamp = page.locator('[data-testid="last-updated"]').first();
    await firstTimestamp.hover();

    // THEN: Tooltip shows absolute timestamp (e.g., "Oct 21, 2025, 2:30 PM")
    const titleAttr = await firstTimestamp.getAttribute('title');
    expect(titleAttr).toMatch(/\w{3}\s+\d{1,2},\s+\d{4}/); // Matches "Oct 21, 2025" format
  });

  test('2.12-E2E-002d: Fresh models show green checkmark icon', async () => {
    // GIVEN: Admin on models page
    await page.goto('/admin/models');

    // THEN: Fresh model row shows green checkmark
    const freshRow = page.locator('[data-testid="model-row"]', { hasText: 'Alpha Fresh' });
    await expect(freshRow.locator('[data-testid="freshness-icon-fresh"]')).toBeVisible();

    // THEN: Icon has green color class
    const icon = freshRow.locator('[data-testid="freshness-icon-fresh"]');
    await expect(icon).toHaveClass(/text-green/);
  });

  test('2.12-E2E-002e: Stale models show yellow clock icon', async () => {
    // GIVEN: Admin on models page
    await page.goto('/admin/models');

    // THEN: Stale model row shows yellow clock
    const staleRow = page.locator('[data-testid="model-row"]', { hasText: 'Beta Stale' });
    await expect(staleRow.locator('[data-testid="freshness-icon-stale"]')).toBeVisible();

    // THEN: Icon has yellow color class
    const icon = staleRow.locator('[data-testid="freshness-icon-stale"]');
    await expect(icon).toHaveClass(/text-yellow/);
  });

  test('2.12-E2E-002f: Critical models show red warning icon', async () => {
    // GIVEN: Admin on models page
    await page.goto('/admin/models');

    // THEN: Critical model row shows red warning
    const criticalRow = page.locator('[data-testid="model-row"]', { hasText: 'Gamma Critical' });
    await expect(criticalRow.locator('[data-testid="freshness-icon-critical"]')).toBeVisible();

    // THEN: Icon has red color class
    const icon = criticalRow.locator('[data-testid="freshness-icon-critical"]');
    await expect(icon).toHaveClass(/text-red/);
  });

  test('2.12-E2E-002g: Freshness filter buttons are displayed', async () => {
    // GIVEN: Admin on models page
    await page.goto('/admin/models');

    // THEN: All filter buttons are visible
    await expect(page.locator('[data-testid="filter-all"]')).toBeVisible();
    await expect(page.locator('[data-testid="filter-fresh"]')).toBeVisible();
    await expect(page.locator('[data-testid="filter-stale"]')).toBeVisible();
    await expect(page.locator('[data-testid="filter-critical"]')).toBeVisible();
  });

  test('2.12-E2E-002h: Clicking "Fresh" filter shows only fresh models', async () => {
    // GIVEN: Admin on models page
    await page.goto('/admin/models');
    await page.waitForSelector('[data-testid="model-row"]');

    // WHEN: Clicking "Fresh" filter button
    await page.click('[data-testid="filter-fresh"]');

    // THEN: URL updates with freshness=fresh parameter
    await expect(page).toHaveURL(/freshness=fresh/);

    // THEN: Only fresh models are displayed
    await expect(page.locator('[data-testid="model-row"]', { hasText: 'Alpha Fresh' })).toBeVisible();
    await expect(page.locator('[data-testid="model-row"]', { hasText: 'Beta Stale' })).not.toBeVisible();
    await expect(page.locator('[data-testid="model-row"]', { hasText: 'Gamma Critical' })).not.toBeVisible();
  });

  test('2.12-E2E-002i: Clicking "Stale" filter shows only stale models', async () => {
    // GIVEN: Admin on models page
    await page.goto('/admin/models');
    await page.waitForSelector('[data-testid="model-row"]');

    // WHEN: Clicking "Stale" filter button
    await page.click('[data-testid="filter-stale"]');

    // THEN: URL updates with freshness=stale parameter
    await expect(page).toHaveURL(/freshness=stale/);

    // THEN: Only stale models are displayed
    await expect(page.locator('[data-testid="model-row"]', { hasText: 'Beta Stale' })).toBeVisible();
    await expect(page.locator('[data-testid="model-row"]', { hasText: 'Alpha Fresh' })).not.toBeVisible();
    await expect(page.locator('[data-testid="model-row"]', { hasText: 'Gamma Critical' })).not.toBeVisible();
  });

  test('2.12-E2E-002j: Clicking "Critical" filter shows only critical models', async () => {
    // GIVEN: Admin on models page
    await page.goto('/admin/models');
    await page.waitForSelector('[data-testid="model-row"]');

    // WHEN: Clicking "Critical" filter button
    await page.click('[data-testid="filter-critical"]');

    // THEN: URL updates with freshness=critical parameter
    await expect(page).toHaveURL(/freshness=critical/);

    // THEN: Only critical models are displayed
    await expect(page.locator('[data-testid="model-row"]', { hasText: 'Gamma Critical' })).toBeVisible();
    await expect(page.locator('[data-testid="model-row"]', { hasText: 'Alpha Fresh' })).not.toBeVisible();
    await expect(page.locator('[data-testid="model-row"]', { hasText: 'Beta Stale' })).not.toBeVisible();
  });

  test('2.12-E2E-002k: Filter state persists in URL (bookmarkable)', async () => {
    // GIVEN: Admin applies stale filter
    await page.goto('/admin/models');
    await page.click('[data-testid="filter-stale"]');
    await expect(page).toHaveURL(/freshness=stale/);

    // WHEN: Refreshing the page
    await page.reload();

    // THEN: Filter state persists after reload
    await expect(page).toHaveURL(/freshness=stale/);

    // THEN: Only stale models still displayed
    await expect(page.locator('[data-testid="model-row"]', { hasText: 'Beta Stale' })).toBeVisible();
  });

  test('2.12-E2E-002l: Clicking "All Models" clears filter', async () => {
    // GIVEN: Admin has stale filter applied
    await page.goto('/admin/models?freshness=stale');

    // WHEN: Clicking "All Models" button
    await page.click('[data-testid="filter-all"]');

    // THEN: URL parameter is cleared
    await expect(page).toHaveURL('/admin/models');

    // THEN: All models are displayed
    await expect(page.locator('[data-testid="model-row"]', { hasText: 'Alpha Fresh' })).toBeVisible();
    await expect(page.locator('[data-testid="model-row"]', { hasText: 'Beta Stale' })).toBeVisible();
    await expect(page.locator('[data-testid="model-row"]', { hasText: 'Gamma Critical' })).toBeVisible();
  });
});

// =============================================================================
// Test Suite 3: Public Model Card Timestamp Display (AC #5)
// =============================================================================

test.describe('2.12-E2E-003: Public Model Card Timestamp Display', () => {
  let page: Page;
  let testModels: any[];

  test.beforeEach(async ({ browser }) => {
    page = await browser.newPage();

    // GIVEN: Test models visible to public
    testModels = [
      await createTestModel({
        name: 'Public Fresh Model',
        provider: 'OpenAI',
        status: 'active',
        updatedAt: new Date(Date.now() - 3 * 24 * 60 * 60 * 1000) // 3 days ago
      }),
      await createTestModel({
        name: 'Public Stale Model',
        provider: 'Anthropic',
        status: 'active',
        updatedAt: new Date(Date.now() - 12 * 24 * 60 * 60 * 1000) // 12 days ago
      }),
    ];
  });

  test.afterEach(async () => {
    await cleanupTestModels(testModels);
    await page.close();
  });

  test('2.12-E2E-003a: Public model cards display relative timestamp', async () => {
    // GIVEN: User on public models page
    await page.goto('/');

    // Wait for models to load
    await page.waitForSelector('[data-testid="model-card"]');

    // THEN: Fresh model card shows relative time
    const freshCard = page.locator('[data-testid="model-card"]', { hasText: 'Public Fresh Model' });
    await expect(freshCard.locator('[data-testid="updated-timestamp"]')).toContainText(/days ago/i);

    // THEN: Stale model card shows relative time
    const staleCard = page.locator('[data-testid="model-card"]', { hasText: 'Public Stale Model' });
    await expect(staleCard.locator('[data-testid="updated-timestamp"]')).toContainText(/days ago/i);
  });

  test('2.12-E2E-003b: Public cards show freshness icons', async () => {
    // GIVEN: User on public models page
    await page.goto('/');
    await page.waitForSelector('[data-testid="model-card"]');

    // THEN: Fresh model shows green icon
    const freshCard = page.locator('[data-testid="model-card"]', { hasText: 'Public Fresh Model' });
    await expect(freshCard.locator('[data-testid="freshness-icon-fresh"]')).toBeVisible();

    // THEN: Stale model shows yellow icon
    const staleCard = page.locator('[data-testid="model-card"]', { hasText: 'Public Stale Model' });
    await expect(staleCard.locator('[data-testid="freshness-icon-stale"]')).toBeVisible();
  });

  test('2.12-E2E-003c: Hovering public timestamp shows tooltip', async () => {
    // GIVEN: User on public models page
    await page.goto('/');
    await page.waitForSelector('[data-testid="model-card"]');

    // WHEN: Hovering over timestamp
    const timestamp = page.locator('[data-testid="updated-timestamp"]').first();
    await timestamp.hover();

    // THEN: Tooltip shows absolute datetime
    const titleAttr = await timestamp.getAttribute('title');
    expect(titleAttr).toMatch(/\w{3}\s+\d{1,2},\s+\d{4}/);
  });

  test('2.12-E2E-003d: Public API includes updatedAt in model response', async ({ request }) => {
    // WHEN: Fetching public models API
    const response = await request.get('http://localhost:5000/api/models');

    // THEN: Response is successful
    expect(response.status()).toBe(200);

    // THEN: Response includes updatedAt timestamp
    const body = await response.json();
    expect(body.data).toBeInstanceOf(Array);
    expect(body.data.length).toBeGreaterThan(0);

    // THEN: Each model has updatedAt field
    const firstModel = body.data[0];
    expect(firstModel).toHaveProperty('updatedAt');
    expect(firstModel.updatedAt).toMatch(/^\d{4}-\d{2}-\d{2}T/); // ISO 8601 format
  });
});
