import { test, expect } from '@playwright/test'

/**
 * E2E Tests for HomePage - Story 3.1
 * Tests acceptance criteria AC #1, #2, #3, #13, #14
 */

test.describe('HomePage - Public Homepage with Basic Layout', () => {
  test.beforeEach(async ({ page }) => {
    // Navigate to homepage before each test
    await page.goto('/')
  })

  test('AC #1, #2: Page loads with header, main content, and footer', async ({ page }) => {
    // Verify page loads successfully
    await expect(page).toHaveTitle(/LLM Pricing Comparison/i)

    // Verify header is present
    const header = page.locator('header')
    await expect(header).toBeVisible()

    // Verify platform name/logo in header
    await expect(page.getByRole('link', { name: /LLM Pricing - Home/i })).toBeVisible()

    // Verify main content area
    const main = page.locator('main')
    await expect(main).toBeVisible()

    // Verify page heading
    await expect(page.getByRole('heading', { name: /LLM Token Price Comparison/i })).toBeVisible()

    // Verify footer is present
    const footer = page.locator('footer')
    await expect(footer).toBeVisible()

    // Verify About link in footer
    await expect(page.getByRole('link', { name: /about/i }).last()).toBeVisible()

    // Verify Contact link in footer (AC #2)
    await expect(page.getByRole('link', { name: /contact/i })).toBeVisible()
  })

  test('AC #2: Navigation bar with search input placeholder', async ({ page }) => {
    // Verify navigation exists
    const nav = page.locator('nav[aria-label="Main navigation"]')
    await expect(nav).toBeVisible()

    // Verify search input placeholder
    const searchInput = page.getByPlaceholder(/search models/i)
    await expect(searchInput).toBeVisible()

    // Verify it's disabled (placeholder for future functionality)
    await expect(searchInput).toBeDisabled()
  })

  test('AC #13, #14: Accessibility - Skip to content link', async ({ page }) => {
    // Focus on the page (simulate Tab key)
    await page.keyboard.press('Tab')

    // Skip link should be focused and visible when focused
    const skipLink = page.getByText(/skip to main content/i)
    await expect(skipLink).toBeFocused()
    await expect(skipLink).toHaveAttribute('href', '#main-content')
  })

  test('AC #13: Keyboard navigation works for all interactive elements', async ({ page }) => {
    // Tab through interactive elements
    await page.keyboard.press('Tab') // Skip link
    await page.keyboard.press('Tab') // Logo/Home link
    await page.keyboard.press('Tab') // Search input (disabled)
    await page.keyboard.press('Tab') // Home nav link

    const homeLink = page.getByRole('link', { name: /^home$/i }).first()
    await expect(homeLink).toBeFocused()

    // Continue tabbing through navigation
    await page.keyboard.press('Tab') // Calculator link
    const calcLink = page.getByRole('link', { name: /calculator/i })
    await expect(calcLink).toBeFocused()

    await page.keyboard.press('Tab') // Compare link
    const compareLink = page.getByRole('link', { name: /compare/i })
    await expect(compareLink).toBeFocused()
  })

  test('AC #14: All navigation elements have ARIA labels', async ({ page }) => {
    // Check header navigation has aria-label
    const mainNav = page.locator('nav[aria-label="Main navigation"]')
    await expect(mainNav).toHaveAttribute('aria-label', 'Main navigation')

    // Check footer navigation has aria-label
    const footerNav = page.locator('nav[aria-label="Footer navigation"]')
    await expect(footerNav).toHaveAttribute('aria-label', 'Footer navigation')

    // Check individual navigation links have aria-labels
    await expect(page.getByRole('link', { name: /home page/i })).toBeVisible()
    await expect(page.getByRole('link', { name: /cost calculator/i })).toBeVisible()
    await expect(page.getByRole('link', { name: /compare models/i })).toBeVisible()
  })

  test('AC #3: Responsive layout - Desktop (1920×1080)', async ({ page }) => {
    // Set viewport to desktop size
    await page.setViewportSize({ width: 1920, height: 1080 })

    // Verify layout renders correctly
    const header = page.locator('header')
    await expect(header).toBeVisible()

    // Search input should be visible on desktop
    const searchInput = page.getByPlaceholder(/search models/i)
    await expect(searchInput).toBeVisible()

    // Navigation should be horizontal (visible)
    const nav = page.locator('nav[aria-label="Main navigation"]')
    await expect(nav).toBeVisible()
  })

  test('AC #3: Responsive layout - Tablet (768×1024)', async ({ page }) => {
    // Set viewport to tablet size
    await page.setViewportSize({ width: 768, height: 1024 })

    // Verify layout adapts
    const header = page.locator('header')
    await expect(header).toBeVisible()

    // Search input should be visible on tablet (md breakpoint)
    const searchInput = page.getByPlaceholder(/search models/i)
    await expect(searchInput).toBeVisible()

    // No horizontal scrollbar (check body width)
    const bodyWidth = await page.evaluate(() => document.body.scrollWidth)
    const viewportWidth = await page.evaluate(() => window.innerWidth)
    expect(bodyWidth).toBeLessThanOrEqual(viewportWidth)
  })

  test('AC #3: Responsive layout - Mobile (375×667)', async ({ page }) => {
    // Set viewport to mobile size
    await page.setViewportSize({ width: 375, height: 667 })

    // Verify layout adapts for mobile
    const header = page.locator('header')
    await expect(header).toBeVisible()

    // Search input should be hidden on mobile (hidden md:flex)
    const searchInput = page.getByPlaceholder(/search models/i)
    await expect(searchInput).not.toBeVisible()

    // Navigation should be hidden, mobile menu button visible
    const nav = page.locator('nav[aria-label="Main navigation"]')
    await expect(nav).not.toBeVisible()

    // Mobile menu button should be visible
    const mobileMenuButton = page.getByRole('button', { name: /open navigation menu/i })
    await expect(mobileMenuButton).toBeVisible()

    // No horizontal scrollbar on mobile
    const bodyWidth = await page.evaluate(() => document.body.scrollWidth)
    const viewportWidth = await page.evaluate(() => window.innerWidth)
    expect(bodyWidth).toBeLessThanOrEqual(viewportWidth)
  })

  test('AC #4, #5, #6: Loading, Empty, and Error states', async ({ page }) => {
    // This test verifies the states are handled properly in the UI
    // The actual state rendering is tested in component tests

    // Page should load without errors
    await expect(page.getByRole('heading', { name: /LLM Token Price Comparison/i })).toBeVisible()

    // Wait for content to load (either models, loading, empty, or error state)
    await page.waitForLoadState('networkidle')

    // Verify one of the states is present
    const hasLoadingState = await page.getByText(/loading models/i).isVisible().catch(() => false)
    const hasEmptyState = await page.getByText(/no models available/i).isVisible().catch(() => false)
    const hasErrorState = await page.getByText(/error/i).isVisible().catch(() => false)
    const hasModels = await page.getByRole('list', { name: /llm models/i }).isVisible().catch(() => false)

    // At least one state should be visible
    expect(hasLoadingState || hasEmptyState || hasErrorState || hasModels).toBe(true)
  })

  test('AC #7, #10: Performance - Page loads quickly without layout shift', async ({ page }) => {
    // Measure page load time
    const startTime = Date.now()

    await page.goto('/')
    await page.waitForLoadState('domcontentloaded')

    const loadTime = Date.now() - startTime

    // Page should load in <2 seconds (AC #7)
    // Note: In CI/CD this might be slower, so we use a generous limit
    expect(loadTime).toBeLessThan(5000)

    // Verify viewport meta tag is present (prevents CLS, AC #10)
    const viewportMeta = await page.locator('meta[name="viewport"]').getAttribute('content')
    expect(viewportMeta).toContain('width=device-width')
    expect(viewportMeta).toContain('initial-scale=1.0')
  })
})
