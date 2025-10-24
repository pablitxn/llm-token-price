import { useState, useEffect } from 'react'
import {
  useReactTable,
  getCoreRowModel,
  getSortedRowModel,
  getFilteredRowModel,
  flexRender,
  type ColumnDef,
  type SortingState,
  type ColumnFiltersState,
} from '@tanstack/react-table'
import type { ModelDto } from '@/types/models'
import { modelColumns } from './columns'
import { useFilterStore } from '@/store/filterStore'

/**
 * Props for the ModelTable component
 */
interface ModelTableProps {
  models: ModelDto[]
}

/**
 * TanStack Table component for displaying models with advanced features
 *
 * Story 3.3: Integrate TanStack Table for Advanced Features
 * Implements AC #2 (Models data rendered using TanStack Table component, replacing the basic HTML table from Story 3.2)
 * and AC #4 (Table maintains the same visual appearance and functionality as Story 3.2)
 *
 * Story 3.4: Add Column Sorting
 * Implements sorting functionality with getSortedRowModel
 *
 * Story 3.5: Add Provider Filter
 * Implements AC #3 (Checking/unchecking provider filters table in real-time)
 * with getFilteredRowModel and columnFilters state
 *
 * Features:
 * - TanStack Table headless UI pattern (logic without imposed UI structure)
 * - Semantic HTML markup (table, thead, tbody, tr, th, td)
 * - Currency formatting with $ symbol and 2 decimal places (via columns.tsx)
 * - TailwindCSS styling for readability (borders, padding, alternating rows)
 * - Responsive design
 * - Accessibility attributes
 * - Composable row models: getCoreRowModel → getSortedRowModel → getFilteredRowModel
 *
 * @param props - Component props
 * @param props.models - Array of model data to display
 */
export default function ModelTable({ models }: ModelTableProps) {
  // Table state: sorting and filtering
  const [sorting, setSorting] = useState<SortingState>([])
  const [columnFilters, setColumnFilters] = useState<ColumnFiltersState>([])

  // Read selected providers from Zustand filter store
  const selectedProviders = useFilterStore((state) => state.selectedProviders)

  // Update columnFilters when selectedProviders changes
  // This triggers TanStack Table to re-filter the rows in real-time (AC #3)
  useEffect(() => {
    setColumnFilters([
      {
        id: 'provider',
        value: selectedProviders,
      },
    ])
  }, [selectedProviders])

  // Initialize TanStack Table with data and column definitions
  // Composable row models: Core → Sorted (Story 3.4) → Filtered (Story 3.5)
  const table = useReactTable({
    data: models,
    columns: modelColumns as ColumnDef<ModelDto>[],
    getCoreRowModel: getCoreRowModel(),
    getSortedRowModel: getSortedRowModel(), // Story 3.4
    getFilteredRowModel: getFilteredRowModel(), // Story 3.5
    state: {
      sorting,
      columnFilters,
    },
    onSortingChange: setSorting,
    onColumnFiltersChange: setColumnFilters,
  })

  return (
    <div className="w-full overflow-x-auto">
      <table className="min-w-full bg-white border border-gray-200 shadow-sm rounded-lg">
        <thead className="bg-gray-50 border-b border-gray-200">
          {table.getHeaderGroups().map((headerGroup) => (
            <tr key={headerGroup.id}>
              {headerGroup.headers.map((header) => (
                <th
                  key={header.id}
                  scope="col"
                  className={`px-6 py-3 text-xs font-medium text-gray-700 uppercase tracking-wider ${
                    header.column.id === 'inputPrice' || header.column.id === 'outputPrice'
                      ? 'text-right'
                      : 'text-left'
                  }`}
                >
                  {header.isPlaceholder
                    ? null
                    : flexRender(header.column.columnDef.header, header.getContext())}
                </th>
              ))}
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
                <td
                  key={cell.id}
                  className={`px-6 py-4 whitespace-nowrap ${
                    cell.column.id === 'inputPrice' || cell.column.id === 'outputPrice'
                      ? 'text-right'
                      : ''
                  }`}
                >
                  {flexRender(cell.column.columnDef.cell, cell.getContext())}
                </td>
              ))}
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}
