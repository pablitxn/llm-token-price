/**
 * ImportResults Component
 * Displays results of CSV import with success/failure summary
 * Shows detailed error information for failed rows
 * Story 2.11 AC#6
 */

import { CheckCircle, XCircle, AlertTriangle, Download } from 'lucide-react'
import type { CSVImportResultDto } from '@/types/admin'

interface ImportResultsProps {
  /** Import result data from backend */
  result: CSVImportResultDto
  /** Callback when user wants to import another file */
  onImportAnother: () => void
}

/**
 * Display CSV import results with success/failure breakdown
 * Features:
 * - Summary statistics (total, successful, failed, skipped)
 * - Failed rows table with error details
 * - Download failed rows as CSV for correction
 * - Import another file action
 */
export function ImportResults({ result, onImportAnother }: ImportResultsProps) {
  /**
   * Download failed rows as CSV for correction
   */
  const handleDownloadFailedRows = () => {
    if (!result.errors || result.errors.length === 0) return

    // Build CSV with headers
    const headers = ['Row Number', 'Error', ...Object.keys(result.errors[0].data)]
    const rows = result.errors.map((error) => {
      const dataValues = Object.values(error.data)
      return [error.rowNumber, `"${error.error}"`, ...dataValues.map(v => `"${v}"`)]
    })

    const csv = [
      headers.join(','),
      ...rows.map(row => row.join(','))
    ].join('\n')

    const blob = new Blob([csv], { type: 'text/csv' })
    const url = URL.createObjectURL(blob)
    const link = document.createElement('a')
    link.href = url
    link.download = 'failed-rows.csv'
    document.body.appendChild(link)
    link.click()
    document.body.removeChild(link)
    URL.revokeObjectURL(url)
  }

  const hasErrors = result.failedImports > 0
  const hasSkipped = result.skippedDuplicates > 0
  const allSuccessful = result.failedImports === 0 && result.skippedDuplicates === 0

  return (
    <div className="space-y-6">
      {/* Summary Card */}
      <div className="bg-white shadow rounded-lg p-6">
        <div className="flex items-center justify-between mb-6">
          <h3 className="text-lg font-semibold text-gray-900">Import Results</h3>
          <button
            type="button"
            onClick={onImportAnother}
            className="px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-md shadow-sm hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500"
          >
            Import Another File
          </button>
        </div>

        {/* Status Message */}
        <div className={`
          mb-6 p-4 rounded-lg flex items-start gap-3
          ${allSuccessful ? 'bg-green-50 border border-green-200' : 'bg-yellow-50 border border-yellow-200'}
        `}>
          {allSuccessful ? (
            <CheckCircle className="h-6 w-6 text-green-600 flex-shrink-0" />
          ) : (
            <AlertTriangle className="h-6 w-6 text-yellow-600 flex-shrink-0" />
          )}
          <div>
            <h4 className={`text-sm font-medium ${allSuccessful ? 'text-green-900' : 'text-yellow-900'}`}>
              {allSuccessful
                ? 'All rows imported successfully!'
                : 'Import completed with some issues'
              }
            </h4>
            <p className={`mt-1 text-sm ${allSuccessful ? 'text-green-800' : 'text-yellow-800'}`}>
              {result.successfulImports} of {result.totalRows} rows imported successfully
              {hasErrors && `, ${result.failedImports} failed`}
              {hasSkipped && `, ${result.skippedDuplicates} skipped (duplicates)`}
            </p>
          </div>
        </div>

        {/* Statistics Grid */}
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-4">
          {/* Total Rows */}
          <div className="bg-gray-50 rounded-lg p-4">
            <div className="text-sm font-medium text-gray-500">Total Rows</div>
            <div className="mt-2 text-2xl font-semibold text-gray-900">{result.totalRows}</div>
          </div>

          {/* Successful */}
          <div className="bg-green-50 rounded-lg p-4">
            <div className="flex items-center gap-2">
              <CheckCircle className="h-4 w-4 text-green-600" />
              <div className="text-sm font-medium text-green-700">Successful</div>
            </div>
            <div className="mt-2 text-2xl font-semibold text-green-900">{result.successfulImports}</div>
          </div>

          {/* Failed */}
          <div className="bg-red-50 rounded-lg p-4">
            <div className="flex items-center gap-2">
              <XCircle className="h-4 w-4 text-red-600" />
              <div className="text-sm font-medium text-red-700">Failed</div>
            </div>
            <div className="mt-2 text-2xl font-semibold text-red-900">{result.failedImports}</div>
          </div>

          {/* Skipped */}
          <div className="bg-yellow-50 rounded-lg p-4">
            <div className="flex items-center gap-2">
              <AlertTriangle className="h-4 w-4 text-yellow-600" />
              <div className="text-sm font-medium text-yellow-700">Skipped</div>
            </div>
            <div className="mt-2 text-2xl font-semibold text-yellow-900">{result.skippedDuplicates}</div>
          </div>
        </div>
      </div>

      {/* Failed Rows Table */}
      {hasErrors && (
        <div className="bg-white shadow rounded-lg overflow-hidden">
          <div className="px-6 py-4 border-b border-gray-200 flex items-center justify-between">
            <h4 className="text-base font-semibold text-gray-900">
              Failed Rows ({result.failedImports})
            </h4>
            <button
              type="button"
              onClick={handleDownloadFailedRows}
              className="inline-flex items-center gap-2 px-3 py-1.5 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-md shadow-sm hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500"
            >
              <Download className="w-4 h-4" />
              Download Failed Rows
            </button>
          </div>

          <div className="overflow-x-auto">
            <table className="min-w-full divide-y divide-gray-200">
              <thead className="bg-gray-50">
                <tr>
                  <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Row #
                  </th>
                  <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Error
                  </th>
                  <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Data
                  </th>
                </tr>
              </thead>
              <tbody className="bg-white divide-y divide-gray-200">
                {result.errors.map((error, index) => (
                  <tr key={index} className="hover:bg-gray-50">
                    <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">
                      {error.rowNumber}
                    </td>
                    <td className="px-6 py-4 text-sm text-red-600">
                      {error.error}
                    </td>
                    <td className="px-6 py-4 text-sm text-gray-500">
                      <div className="space-y-1">
                        {Object.entries(error.data).map(([key, value]) => (
                          <div key={key} className="flex gap-2">
                            <span className="font-medium text-gray-700">{key}:</span>
                            <span className="text-gray-900">{value || '(empty)'}</span>
                          </div>
                        ))}
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {/* Help Text */}
      {hasErrors && (
        <div className="bg-blue-50 border border-blue-200 rounded-lg p-4">
          <h4 className="text-sm font-medium text-blue-900 mb-2">What to do next?</h4>
          <ol className="text-sm text-blue-800 space-y-1 list-decimal list-inside">
            <li>Download the failed rows using the button above</li>
            <li>Fix the errors in your CSV file</li>
            <li>Re-import the corrected file</li>
          </ol>
        </div>
      )}
    </div>
  )
}
