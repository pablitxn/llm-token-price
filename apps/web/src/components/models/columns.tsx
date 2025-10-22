/**
 * TanStack Table column definitions for model table
 * Story 3.3: Defines columns for name, provider, input price, output price
 */

import type { ColumnDef } from '@tanstack/react-table'
import type { ModelTableData } from '@/types/table'
import { formatPrice } from '@/utils/formatters'

/**
 * Column definitions for model comparison table
 * Uses TanStack Table v8 column definition format with type-safe accessors
 * Maintains visual appearance from Story 3.2 with custom cell renderers
 */
export const modelColumns: ColumnDef<ModelTableData>[] = [
  {
    accessorKey: 'name',
    header: 'Model Name',
    cell: (info) => {
      const model = info.row.original
      return (
        <>
          <div className="font-medium">{info.getValue() as string}</div>
          {model.version && <div className="text-xs text-gray-500">v{model.version}</div>}
        </>
      )
    }
  },
  {
    accessorKey: 'provider',
    header: 'Provider',
    cell: (info) => info.getValue()
  },
  {
    accessorKey: 'inputPricePer1M',
    header: () => (
      <>
        Input Price
        <span className="block text-[10px] font-normal text-gray-500 normal-case">
          per 1M tokens
        </span>
      </>
    ),
    cell: (info) => {
      const model = info.row.original
      const price = info.getValue() as number
      return <div className="font-semibold text-right">{formatPrice(price, model.currency)}</div>
    }
  },
  {
    accessorKey: 'outputPricePer1M',
    header: () => (
      <>
        Output Price
        <span className="block text-[10px] font-normal text-gray-500 normal-case">
          per 1M tokens
        </span>
      </>
    ),
    cell: (info) => {
      const model = info.row.original
      const price = info.getValue() as number
      return <div className="font-semibold text-right">{formatPrice(price, model.currency)}</div>
    }
  }
]
