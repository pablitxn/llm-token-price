import type { ModelDto } from '@/types/models'

/**
 * Props for the ModelTable component
 */
interface ModelTableProps {
  models: ModelDto[]
}

/**
 * Formats a price value with currency symbol and proper decimal places
 * @param price - The price value in dollars per 1M tokens
 * @param currency - The currency code (default: USD)
 * @returns Formatted price string (e.g., "$2.50")
 */
const formatPrice = (price: number, currency: string = 'USD'): string => {
  if (currency === 'USD') {
    return `$${price.toFixed(2)}`
  }
  // For other currencies, use Intl.NumberFormat
  return new Intl.NumberFormat('en-US', {
    style: 'currency',
    currency: currency,
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  }).format(price)
}

/**
 * Basic HTML table component for displaying models
 *
 * Story 3.2: Fetch and Display Models in Basic Table
 * Implements AC #2 (Basic HTML table displays models with columns: name, provider, input price, output price)
 * and AC #6 (Table displays 10+ models with sample data)
 *
 * Features:
 * - Semantic HTML markup (table, thead, tbody, tr, th, td)
 * - Currency formatting with $ symbol and 2 decimal places
 * - TailwindCSS styling for readability (borders, padding, alternating rows)
 * - Responsive design
 * - Accessibility attributes
 *
 * @param props - Component props
 * @param props.models - Array of model data to display
 */
export default function ModelTable({ models }: ModelTableProps) {
  return (
    <div className="w-full overflow-x-auto">
      <table className="min-w-full bg-white border border-gray-200 shadow-sm rounded-lg">
        <thead className="bg-gray-50 border-b border-gray-200">
          <tr>
            <th
              scope="col"
              className="px-6 py-3 text-left text-xs font-medium text-gray-700 uppercase tracking-wider"
            >
              Name
            </th>
            <th
              scope="col"
              className="px-6 py-3 text-left text-xs font-medium text-gray-700 uppercase tracking-wider"
            >
              Provider
            </th>
            <th
              scope="col"
              className="px-6 py-3 text-right text-xs font-medium text-gray-700 uppercase tracking-wider"
            >
              Input Price
              <span className="block text-[10px] font-normal text-gray-500 normal-case">
                per 1M tokens
              </span>
            </th>
            <th
              scope="col"
              className="px-6 py-3 text-right text-xs font-medium text-gray-700 uppercase tracking-wider"
            >
              Output Price
              <span className="block text-[10px] font-normal text-gray-500 normal-case">
                per 1M tokens
              </span>
            </th>
          </tr>
        </thead>
        <tbody className="divide-y divide-gray-200">
          {models.map((model, index) => (
            <tr
              key={model.id}
              className={`hover:bg-gray-50 transition-colors ${
                index % 2 === 0 ? 'bg-white' : 'bg-gray-50/50'
              }`}
            >
              <td className="px-6 py-4 whitespace-nowrap">
                <div className="text-sm font-medium text-gray-900">{model.name}</div>
                {model.version && (
                  <div className="text-xs text-gray-500">v{model.version}</div>
                )}
              </td>
              <td className="px-6 py-4 whitespace-nowrap">
                <div className="text-sm text-gray-900">{model.provider}</div>
              </td>
              <td className="px-6 py-4 whitespace-nowrap text-right">
                <div className="text-sm font-semibold text-gray-900">
                  {formatPrice(model.inputPricePer1M, model.currency)}
                </div>
              </td>
              <td className="px-6 py-4 whitespace-nowrap text-right">
                <div className="text-sm font-semibold text-gray-900">
                  {formatPrice(model.outputPricePer1M, model.currency)}
                </div>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}
