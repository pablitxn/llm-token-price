/**
 * Tests for AdminBenchmarksPage component
 */

import { describe, it, expect } from 'vitest'
import { screen } from '@testing-library/react'
import { renderWithProviders } from '@/test/test-utils'
import AdminBenchmarksPage from '../AdminBenchmarksPage'

describe('AdminBenchmarksPage', () => {
  it('renders benchmarks management title', () => {
    renderWithProviders(<AdminBenchmarksPage />)
    expect(screen.getByText('Benchmarks Management')).toBeInTheDocument()
  })

  it('displays placeholder message', () => {
    renderWithProviders(<AdminBenchmarksPage />)
    expect(
      screen.getByText(/Benchmark CRUD functionality will be implemented in Stories 2.9-2.11/i)
    ).toBeInTheDocument()
  })

  it('lists future story implementations', () => {
    renderWithProviders(<AdminBenchmarksPage />)
    expect(
      screen.getByText(/Story 2.9: Create Benchmark Definitions Management/i)
    ).toBeInTheDocument()
    expect(
      screen.getByText(/Story 2.10: Create Benchmark Score Entry Form/i)
    ).toBeInTheDocument()
    expect(
      screen.getByText(/Story 2.11: Add Bulk Benchmark Import via CSV/i)
    ).toBeInTheDocument()
  })
})
