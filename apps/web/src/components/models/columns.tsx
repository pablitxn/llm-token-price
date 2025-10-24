import { createColumnHelper } from '@tanstack/react-table'
import type { ModelDto } from '@/types/models'
import { formatPrice } from '@/utils/formatPrice'

/**
 * Column definitions for the Models table using TanStack Table
 *
 * Story 3.3: Integrate TanStack Table for Advanced Features
 * Implements AC #3 (Column definitions created for all model fields displayed in Story 3.2)
 *
 * Uses createColumnHelper for type-safe column definitions.
 * Columns mirror the basic HTML table from Story 3.2: name, provider, input price, output price
 */

const columnHelper = createColumnHelper<ModelDto>()

/**
 * Column definitions array for TanStack Table
 * Each column definition includes:
 * - accessor: The field name from ModelDto
 * - header: Display name for the column header
 * - cell: Optional custom cell renderer (used for price formatting)
 */
export const modelColumns = [
  columnHelper.accessor('name', {
    id: 'name',
    header: () => 'Name',
    cell: (info) => {
      const model = info.row.original
      return (
        <div>
          <div className="text-sm font-medium text-gray-900">{info.getValue()}</div>
          {model.version && <div className="text-xs text-gray-500">v{model.version}</div>}
        </div>
      )
    },
  }),

  columnHelper.accessor('provider', {
    id: 'provider',
    header: () => 'Provider',
    cell: (info) => <div className="text-sm text-gray-900">{info.getValue()}</div>,
    // Filter function for provider column (Story 3.5: OR logic)
    // Empty filterValue array = show all models
    // Non-empty filterValue = show only models from selected providers
    filterFn: (row, columnId, filterValue: string[]) => {
      const provider = row.getValue(columnId) as string
      return filterValue.length === 0 || filterValue.includes(provider)
    },
  }),

  columnHelper.accessor('inputPricePer1M', {
    id: 'inputPrice',
    header: () => (
      <div>
        Input Price
        <span className="block text-[10px] font-normal text-gray-500 normal-case">
          per 1M tokens
        </span>
      </div>
    ),
    cell: (info) => {
      const model = info.row.original
      return (
        <div className="text-sm font-semibold text-gray-900">
          {formatPrice(info.getValue(), model.currency)}
        </div>
      )
    },
  }),

  columnHelper.accessor('outputPricePer1M', {
    id: 'outputPrice',
    header: () => (
      <div>
        Output Price
        <span className="block text-[10px] font-normal text-gray-500 normal-case">
          per 1M tokens
        </span>
      </div>
    ),
    cell: (info) => {
      const model = info.row.original
      return (
        <div className="text-sm font-semibold text-gray-900">
          {formatPrice(info.getValue(), model.currency)}
        </div>
      )
    },
  }),
]
