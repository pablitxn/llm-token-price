/**
 * ModelList component - Admin panel table for viewing all models
 * Displays models with name, provider, pricing, status, and last updated
 * Supports sorting, search, and action buttons (Edit/Delete)
 */

import { useMemo, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import type { AdminModelDto } from '@/types/admin'
import { RelativeTime } from '@/components/ui/RelativeTime'

/**
 * Props for ModelList component
 */
export interface ModelListProps {
  /** Array of models to display */
  models: AdminModelDto[]
  /** Optional search term for highlighting */
  searchTerm?: string
  /** Optional callback for delete button click */
  onDeleteClick?: (model: AdminModelDto) => void
}

/**
 * Sort configuration
 */
type SortKey = 'name' | 'provider' | 'inputPricePer1M' | 'outputPricePer1M' | 'status' | 'updatedAt'
type SortDirection = 'asc' | 'desc'

interface SortConfig {
  key: SortKey
  direction: SortDirection
}

/**
 * Formats a decimal price with currency symbol and 6 decimal places
 * @param price - Price per 1M tokens
 * @param currency - Currency code (default: USD)
 * @returns Formatted price string (e.g., "$30.000000")
 */
function formatPrice(price: number, currency: string = 'USD'): string {
  const symbol = currency === 'USD' ? '$' : currency
  return `${symbol}${price.toFixed(6)}`
}


/**
 * Returns Tailwind CSS classes for status badge
 * @param status - Model status (active, deprecated, beta)
 * @returns CSS classes for badge styling
 */
function getStatusBadgeClasses(status: string): string {
  const baseClasses = 'inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium'

  switch (status.toLowerCase()) {
    case 'active':
      return `${baseClasses} bg-green-100 text-green-800`
    case 'deprecated':
      return `${baseClasses} bg-red-100 text-red-800`
    case 'beta':
      return `${baseClasses} bg-yellow-100 text-yellow-800`
    default:
      return `${baseClasses} bg-gray-100 text-gray-800`
  }
}

/**
 * ModelList component
 * Renders a table of models with sortable columns and action buttons
 */
export function ModelList({ models, searchTerm, onDeleteClick }: ModelListProps) {
  const navigate = useNavigate()
  const [sortConfig, setSortConfig] = useState<SortConfig>({
    key: 'updatedAt',
    direction: 'desc', // Most recently updated first (matches backend default)
  })

  /**
   * Handles column header click for sorting
   */
  const handleSort = (key: SortKey) => {
    setSortConfig((prev) => ({
      key,
      direction: prev.key === key && prev.direction === 'asc' ? 'desc' : 'asc',
    }))
  }

  /**
   * Sorts models based on current sort configuration
   */
  const sortedModels = useMemo(() => {
    const sorted = [...models].sort((a, b) => {
      const aValue = a[sortConfig.key]
      const bValue = b[sortConfig.key]

      // Handle null/undefined values
      if (aValue == null) return 1
      if (bValue == null) return -1

      // Compare values
      if (aValue < bValue) return sortConfig.direction === 'asc' ? -1 : 1
      if (aValue > bValue) return sortConfig.direction === 'asc' ? 1 : -1
      return 0
    })
    return sorted
  }, [models, sortConfig])

  /**
   * Renders sort indicator (up/down arrow) for active column
   */
  const renderSortIndicator = (key: SortKey) => {
    if (sortConfig.key !== key) return null
    return (
      <span className="ml-1 inline-block">
        {sortConfig.direction === 'asc' ? '↑' : '↓'}
      </span>
    )
  }

  /**
   * Handles edit button click
   */
  const handleEditClick = (modelId: string) => {
    navigate(`/admin/models/${modelId}/edit`)
  }

  /**
   * Handles delete button click
   */
  const handleDeleteClickInternal = (model: AdminModelDto) => {
    if (onDeleteClick) {
      onDeleteClick(model)
    }
  }

  // Show message if no models
  if (models.length === 0) {
    return (
      <div className="text-center py-12">
        <p className="text-gray-500">
          {searchTerm ? 'No models found matching your search.' : 'No models available.'}
        </p>
      </div>
    )
  }

  return (
    <div className="overflow-x-auto">
      <table className="min-w-full divide-y divide-gray-200">
        <thead className="bg-gray-50">
          <tr>
            <th
              scope="col"
              className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider cursor-pointer hover:bg-gray-100"
              onClick={() => handleSort('name')}
            >
              Name {renderSortIndicator('name')}
            </th>
            <th
              scope="col"
              className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider cursor-pointer hover:bg-gray-100"
              onClick={() => handleSort('provider')}
            >
              Provider {renderSortIndicator('provider')}
            </th>
            <th
              scope="col"
              className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider cursor-pointer hover:bg-gray-100"
              onClick={() => handleSort('inputPricePer1M')}
            >
              Input Price {renderSortIndicator('inputPricePer1M')}
            </th>
            <th
              scope="col"
              className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider cursor-pointer hover:bg-gray-100"
              onClick={() => handleSort('outputPricePer1M')}
            >
              Output Price {renderSortIndicator('outputPricePer1M')}
            </th>
            <th
              scope="col"
              className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider cursor-pointer hover:bg-gray-100"
              onClick={() => handleSort('status')}
            >
              Status {renderSortIndicator('status')}
            </th>
            <th
              scope="col"
              className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider cursor-pointer hover:bg-gray-100"
              onClick={() => handleSort('updatedAt')}
            >
              Last Updated {renderSortIndicator('updatedAt')}
            </th>
            <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
              Actions
            </th>
          </tr>
        </thead>
        <tbody className="bg-white divide-y divide-gray-200">
          {sortedModels.map((model) => (
            <tr key={model.id} className="hover:bg-gray-50">
              <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">
                {model.name}
                {model.version && <span className="ml-1 text-gray-500">({model.version})</span>}
              </td>
              <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{model.provider}</td>
              <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                {formatPrice(model.inputPricePer1M, model.currency)}
              </td>
              <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                {formatPrice(model.outputPricePer1M, model.currency)}
              </td>
              <td className="px-6 py-4 whitespace-nowrap text-sm">
                <span className={getStatusBadgeClasses(model.status)}>
                  {model.status}
                </span>
                {!model.isActive && (
                  <span className="ml-2 inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-gray-100 text-gray-800">
                    Inactive
                  </span>
                )}
              </td>
              <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                <RelativeTime date={model.updatedAt} showIcon />
              </td>
              <td className="px-6 py-4 whitespace-nowrap text-sm font-medium">
                <button
                  type="button"
                  onClick={() => handleEditClick(model.id)}
                  className="text-blue-600 hover:text-blue-900 mr-4"
                  aria-label={`Edit ${model.name}`}
                >
                  Edit
                </button>
                <button
                  type="button"
                  onClick={() => handleDeleteClickInternal(model)}
                  className="text-red-600 hover:text-red-900"
                  aria-label={`Delete ${model.name}`}
                >
                  Delete
                </button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}
