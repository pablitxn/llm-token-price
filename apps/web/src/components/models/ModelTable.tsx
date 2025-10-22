import { useReactTable, getCoreRowModel, flexRender } from '@tanstack/react-table'
import type { ModelDto } from '@/types/models'
import { modelColumns } from './columns'

/**
 * Props for the ModelTable component
 */
interface ModelTableProps {
  models: ModelDto[]
}

/**
 * TanStack Table component for displaying models
 *
 * Story 3.3: Integrate TanStack Table for Advanced Features
 * Refactored from basic HTML table (Story 3.2) to use TanStack Table library
 *
 * Features:
 * - TanStack Table v8 with headless UI pattern
 * - Maintains same visual appearance as Story 3.2 (alternating rows, hover effects)
 * - Type-safe column definitions with TypeScript
 * - Prepares foundation for sorting (Story 3.4) and filtering (Stories 3.5-3.7)
 * - Semantic HTML markup with accessibility attributes
 *
 * @param props - Component props
 * @param props.models - Array of model data to display
 */
export default function ModelTable({ models }: ModelTableProps) {
  const table = useReactTable({
    data: models,
    columns: modelColumns,
    getCoreRowModel: getCoreRowModel()
  })

  return (
    <div className="w-full overflow-x-auto">
      <table className="min-w-full bg-white border border-gray-200 shadow-sm rounded-lg">
        <thead className="bg-gray-50 border-b border-gray-200">
          {table.getHeaderGroups().map((headerGroup) => (
            <tr key={headerGroup.id}>
              {headerGroup.headers.map((header) => {
                const isPriceColumn =
                  header.column.id === 'inputPricePer1M' ||
                  header.column.id === 'outputPricePer1M'

                return (
                  <th
                    key={header.id}
                    scope="col"
                    className={`px-6 py-3 text-xs font-medium text-gray-700 uppercase tracking-wider ${
                      isPriceColumn ? 'text-right' : 'text-left'
                    }`}
                  >
                    {header.isPlaceholder
                      ? null
                      : flexRender(header.column.columnDef.header, header.getContext())}
                  </th>
                )
              })}
            </tr>
          ))}
        </thead>
        <tbody className="divide-y divide-gray-200">
          {table.getRowModel().rows.map((row, index) => (
            <tr
              key={row.id}
              className={`hover:bg-gray-50 transition-colors ${
                index % 2 === 0 ? 'bg-white' : 'bg-gray-50/50'
              }`}
            >
              {row.getVisibleCells().map((cell) => (
                <td key={cell.id} className="px-6 py-4 whitespace-nowrap">
                  <div className="text-sm text-gray-900">
                    {flexRender(cell.column.columnDef.cell, cell.getContext())}
                  </div>
                </td>
              ))}
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}
