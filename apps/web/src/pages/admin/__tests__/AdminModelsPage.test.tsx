/**
 * Tests for AdminModelsPage component
 */

import { describe, it, expect } from 'vitest'
import { screen } from '@testing-library/react'
import { renderWithProviders } from '@/test/test-utils'
import AdminModelsPage from '../AdminModelsPage'

describe('AdminModelsPage', () => {
  it('renders models management title', () => {
    renderWithProviders(<AdminModelsPage />)
    expect(screen.getByText('Models Management')).toBeInTheDocument()
  })

  it('displays placeholder message', () => {
    renderWithProviders(<AdminModelsPage />)
    expect(
      screen.getByText(/Model CRUD functionality will be implemented in Stories 2.3-2.8/i)
    ).toBeInTheDocument()
  })

  it('lists future story implementations', () => {
    renderWithProviders(<AdminModelsPage />)
    expect(screen.getByText(/Story 2.3: Build Models List View/i)).toBeInTheDocument()
    expect(screen.getByText(/Story 2.4: Create Add New Model Form/i)).toBeInTheDocument()
    expect(screen.getByText(/Story 2.8: Delete Model Functionality/i)).toBeInTheDocument()
  })
})
