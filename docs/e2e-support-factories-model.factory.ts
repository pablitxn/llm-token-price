/**
 * Model Data Factory
 * Story 2.12 E2E Test Support
 * Knowledge Base: data-factories.md
 *
 * Factory functions for creating test model data with faker.
 * Supports overrides for specific test scenarios.
 * Provides cleanup helpers for test isolation.
 */

import { faker } from '@faker-js/faker';
import axios from 'axios';

const API_BASE_URL = process.env.API_BASE_URL || 'http://localhost:5000/api';
const ADMIN_API_URL = `${API_BASE_URL}/admin`;

/**
 * Model data structure matching backend AdminModelDto
 */
export interface TestModel {
  id?: string;
  name: string;
  provider: string;
  version?: string | null;
  status: 'active' | 'deprecated' | 'beta';
  inputPricePer1M: number;
  outputPricePer1M: number;
  currency: string;
  updatedAt?: Date;
  pricingUpdatedAt?: Date;
  createdAt?: Date;
  isActive?: boolean;
}

/**
 * Create a test model via backend API
 * Uses faker for randomized data to avoid collisions
 * Supports overrides for specific test scenarios
 *
 * @param overrides - Optional fields to override defaults
 * @returns Created model with ID from backend
 *
 * @example
 * ```typescript
 * // Fresh model (updated recently)
 * const freshModel = await createTestModel({
 *   name: 'GPT-4 Test',
 *   updatedAt: new Date(Date.now() - 2 * 24 * 60 * 60 * 1000) // 2 days ago
 * });
 *
 * // Critical model (outdated)
 * const criticalModel = await createTestModel({
 *   name: 'Legacy Model',
 *   updatedAt: new Date(Date.now() - 45 * 24 * 60 * 60 * 1000) // 45 days ago
 * });
 * ```
 */
export async function createTestModel(overrides: Partial<TestModel> = {}): Promise<TestModel> {
  // Generate default model data with faker
  const defaultModel: TestModel = {
    name: overrides.name || `${faker.company.name()} Model ${faker.string.alphanumeric(6)}`,
    provider: overrides.provider || faker.helpers.arrayElement(['OpenAI', 'Anthropic', 'Google', 'Meta']),
    version: overrides.version ?? faker.system.semver(),
    status: overrides.status || 'active',
    inputPricePer1M: overrides.inputPricePer1M ?? faker.number.float({ min: 0.5, max: 30, multipleOf: 0.01 }),
    outputPricePer1M: overrides.outputPricePer1M ?? faker.number.float({ min: 1, max: 60, multipleOf: 0.01 }),
    currency: overrides.currency || 'USD',
  };

  // Merge with overrides
  const modelData = { ...defaultModel, ...overrides };

  try {
    // POST to admin API to create model
    const response = await axios.post(
      `${ADMIN_API_URL}/models`,
      modelData,
      {
        headers: {
          'Content-Type': 'application/json',
          // Note: In real implementation, include admin JWT token
          // 'Authorization': `Bearer ${adminToken}`
        }
      }
    );

    // If updatedAt override provided, update the model timestamp via PATCH
    if (overrides.updatedAt) {
      await axios.patch(
        `${ADMIN_API_URL}/models/${response.data.data.id}/timestamps`,
        {
          updatedAt: overrides.updatedAt.toISOString()
        }
      );
    }

    return response.data.data;
  } catch (error) {
    console.error('Failed to create test model:', error);
    throw error;
  }
}

/**
 * Create multiple test models at once
 * Useful for setting up test data with varying freshness
 *
 * @param count - Number of models to create
 * @param overrides - Optional overrides applied to all models
 * @returns Array of created models
 *
 * @example
 * ```typescript
 * // Create 10 random models
 * const models = await createTestModels(10);
 *
 * // Create 5 stale models (10-20 days old)
 * const staleModels = await createTestModels(5, {
 *   status: 'active',
 *   updatedAt: new Date(Date.now() - 15 * 24 * 60 * 60 * 1000)
 * });
 * ```
 */
export async function createTestModels(
  count: number,
  overrides: Partial<TestModel> = {}
): Promise<TestModel[]> {
  const models: TestModel[] = [];

  for (let i = 0; i < count; i++) {
    const model = await createTestModel(overrides);
    models.push(model);
  }

  return models;
}

/**
 * Create test models with varying freshness
 * Automatically distributes models across freshness categories
 *
 * @param distribution - Object specifying count per category
 * @returns Array of created models
 *
 * @example
 * ```typescript
 * const models = await createModelsWithFreshness({
 *   fresh: 3,    // 3 models < 7 days old
 *   stale: 2,    // 2 models 7-30 days old
 *   critical: 2  // 2 models > 30 days old
 * });
 * ```
 */
export async function createModelsWithFreshness(distribution: {
  fresh?: number;
  stale?: number;
  critical?: number;
}): Promise<TestModel[]> {
  const models: TestModel[] = [];

  // Fresh models (< 7 days)
  if (distribution.fresh) {
    for (let i = 0; i < distribution.fresh; i++) {
      const daysAgo = faker.number.int({ min: 0, max: 6 });
      const model = await createTestModel({
        name: `Fresh Model ${i + 1}`,
        updatedAt: new Date(Date.now() - daysAgo * 24 * 60 * 60 * 1000)
      });
      models.push(model);
    }
  }

  // Stale models (7-30 days)
  if (distribution.stale) {
    for (let i = 0; i < distribution.stale; i++) {
      const daysAgo = faker.number.int({ min: 7, max: 29 });
      const model = await createTestModel({
        name: `Stale Model ${i + 1}`,
        updatedAt: new Date(Date.now() - daysAgo * 24 * 60 * 60 * 1000)
      });
      models.push(model);
    }
  }

  // Critical models (> 30 days)
  if (distribution.critical) {
    for (let i = 0; i < distribution.critical; i++) {
      const daysAgo = faker.number.int({ min: 30, max: 90 });
      const model = await createTestModel({
        name: `Critical Model ${i + 1}`,
        updatedAt: new Date(Date.now() - daysAgo * 24 * 60 * 60 * 1000)
      });
      models.push(model);
    }
  }

  return models;
}

/**
 * Delete test models (cleanup)
 * Soft delete via admin API (sets isActive = false)
 *
 * @param models - Array of models to delete
 *
 * @example
 * ```typescript
 * test.afterEach(async () => {
 *   await cleanupTestModels(testModels);
 * });
 * ```
 */
export async function cleanupTestModels(models: TestModel[]): Promise<void> {
  const deletePromises = models.map(async (model) => {
    if (!model.id) return;

    try {
      await axios.delete(`${ADMIN_API_URL}/models/${model.id}`);
    } catch (error) {
      console.warn(`Failed to delete test model ${model.id}:`, error);
      // Continue cleanup even if one fails
    }
  });

  await Promise.all(deletePromises);
}

/**
 * Delete all test models matching a pattern
 * Useful for cleaning up leaked test data
 *
 * @param namePattern - Regex pattern to match model names
 *
 * @example
 * ```typescript
 * // Clean up all models with "Test" in name
 * await cleanupTestModelsByPattern(/Test/);
 * ```
 */
export async function cleanupTestModelsByPattern(namePattern: RegExp): Promise<void> {
  try {
    // Fetch all admin models
    const response = await axios.get(`${ADMIN_API_URL}/models`);
    const allModels = response.data.data;

    // Filter models matching pattern
    const testModels = allModels.filter((model: TestModel) =>
      namePattern.test(model.name)
    );

    // Delete matching models
    await cleanupTestModels(testModels);

    console.log(`Cleaned up ${testModels.length} test models matching pattern: ${namePattern}`);
  } catch (error) {
    console.error('Failed to cleanup test models by pattern:', error);
    throw error;
  }
}
