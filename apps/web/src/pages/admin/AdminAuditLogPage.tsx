/**
 * AdminAuditLogPage - Audit trail viewer for admin panel
 * Displays all admin CRUD operations with filtering and CSV export
 *
 * Story 2.13 Task 14: Implement Audit Log
 */

import { useState } from 'react'
import { useSearchParams } from 'react-router-dom'
import { getAuditLogs, exportAuditLogsToCSV } from '@/api/admin'
import { LoadingSpinner } from '@/components/ui/LoadingSpinner'
import { ErrorAlert } from '@/components/ui/ErrorAlert'
import type { AuditLogDto, AuditLogFilters } from '@/types/auditLog'
import { useQuery } from '@tanstack/react-query'
import { formatDistanceToNow } from 'date-fns'

/**
 * AdminAuditLogPage component
 * Main admin panel page for viewing audit trail records
 */
export default function AdminAuditLogPage() {
  const [searchParams, setSearchParams] = useSearchParams()
  const [isExporting, setIsExporting] = useState(false)

  // Filter state from URL params
  const [filters, setFilters] = useState<AuditLogFilters>({
    userId: searchParams.get('userId') || undefined,
    entityType: searchParams.get('entityType') || undefined,
    action: searchParams.get('action') || undefined,
    startDate: searchParams.get('startDate') || undefined,
    endDate: searchParams.get('endDate') || undefined,
    page: parseInt(searchParams.get('page') || '1', 10),
    pageSize: parseInt(searchParams.get('pageSize') || '20', 10),
  })

  // Fetch audit logs using TanStack Query
  const { data, isLoading, error, refetch } = useQuery({
    queryKey: ['auditLogs', filters],
    queryFn: () => getAuditLogs(filters),
    staleTime: 30000, // 30 seconds (audit logs are less frequently updated)
  })

  /**
   * Updates filters and syncs with URL params
   */
  const updateFilters = (newFilters: Partial<AuditLogFilters>) => {
    const updated = { ...filters, ...newFilters, page: 1 } // Reset to page 1 on filter change
    setFilters(updated)

    // Sync with URL
    const params: Record<string, string> = {}
    if (updated.userId) params.userId = updated.userId
    if (updated.entityType) params.entityType = updated.entityType
    if (updated.action) params.action = updated.action
    if (updated.startDate) params.startDate = updated.startDate
    if (updated.endDate) params.endDate = updated.endDate
    params.page = updated.page?.toString() || '1'
    params.pageSize = updated.pageSize?.toString() || '20'
    setSearchParams(params)
  }

  /**
   * Handles CSV export button click
   */
  const handleExport = async () => {
    setIsExporting(true)
    try {
      const blob = await exportAuditLogsToCSV({
        userId: filters.userId,
        entityType: filters.entityType,
        action: filters.action,
        startDate: filters.startDate,
        endDate: filters.endDate,
      })

      // Create download link
      const url = URL.createObjectURL(blob)
      const a = document.createElement('a')
      a.href = url
      a.download = `audit-log-${new Date().toISOString().split('T')[0]}.csv`
      document.body.appendChild(a)
      a.click()
      document.body.removeChild(a)
      URL.revokeObjectURL(url)
    } catch (err) {
      console.error('CSV export failed:', err)
      alert('Failed to export CSV. Please try again.')
    } finally {
      setIsExporting(false)
    }
  }

  /**
   * Renders a single audit log row
   */
  const renderAuditLogRow = (log: AuditLogDto) => {
    const timestamp = new Date(log.timestamp)
    const relativeTime = formatDistanceToNow(timestamp, { addSuffix: true })

    // Get action color
    const actionColor = {
      Create: 'text-green-600',
      Update: 'text-blue-600',
      Delete: 'text-red-600',
      Import: 'text-purple-600',
    }[log.action] || 'text-gray-600'

    return (
      <tr key={log.id} className="border-b hover:bg-gray-50">
        <td className="px-4 py-3 text-sm text-gray-900" title={timestamp.toLocaleString()}>
          {relativeTime}
        </td>
        <td className="px-4 py-3 text-sm text-gray-900">{log.userId}</td>
        <td className={`px-4 py-3 text-sm font-semibold ${actionColor}`}>{log.action}</td>
        <td className="px-4 py-3 text-sm text-gray-900">{log.entityType}</td>
        <td className="px-4 py-3 text-sm font-mono text-gray-600 truncate max-w-xs" title={log.entityId}>
          {log.entityId.slice(0, 8)}...
        </td>
        <td className="px-4 py-3 text-sm text-gray-500">
          {log.oldValues ? '✓' : '-'}
        </td>
        <td className="px-4 py-3 text-sm text-gray-500">
          {log.newValues ? '✓' : '-'}
        </td>
      </tr>
    )
  }

  const auditLogs = data?.data.items || []
  const pagination = data?.data.pagination

  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      {/* Page Header */}
      <div className="mb-6">
        <h1 className="text-3xl font-bold text-gray-900">Audit Log</h1>
        <p className="mt-2 text-sm text-gray-600">
          Complete audit trail of all admin CRUD operations for compliance and traceability
        </p>
      </div>

      {/* Filters */}
      <div className="bg-white shadow rounded-lg p-4 mb-6">
        <h2 className="text-lg font-semibold mb-4">Filters</h2>
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
          {/* User ID Filter */}
          <div>
            <label htmlFor="userId" className="block text-sm font-medium text-gray-700 mb-1">
              User
            </label>
            <input
              id="userId"
              type="text"
              placeholder="admin@example.com"
              value={filters.userId || ''}
              onChange={(e) => updateFilters({ userId: e.target.value || undefined })}
              className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
          </div>

          {/* Entity Type Filter */}
          <div>
            <label htmlFor="entityType" className="block text-sm font-medium text-gray-700 mb-1">
              Entity Type
            </label>
            <select
              id="entityType"
              value={filters.entityType || ''}
              onChange={(e) => updateFilters({ entityType: e.target.value || undefined })}
              className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
            >
              <option value="">All</option>
              <option value="Model">Model</option>
              <option value="Benchmark">Benchmark</option>
              <option value="BenchmarkScore">BenchmarkScore</option>
            </select>
          </div>

          {/* Action Filter */}
          <div>
            <label htmlFor="action" className="block text-sm font-medium text-gray-700 mb-1">
              Action
            </label>
            <select
              id="action"
              value={filters.action || ''}
              onChange={(e) => updateFilters({ action: e.target.value || undefined })}
              className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
            >
              <option value="">All</option>
              <option value="Create">Create</option>
              <option value="Update">Update</option>
              <option value="Delete">Delete</option>
              <option value="Import">Import</option>
            </select>
          </div>

          {/* Start Date Filter */}
          <div>
            <label htmlFor="startDate" className="block text-sm font-medium text-gray-700 mb-1">
              Start Date
            </label>
            <input
              id="startDate"
              type="date"
              value={filters.startDate || ''}
              onChange={(e) => updateFilters({ startDate: e.target.value || undefined })}
              className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
          </div>

          {/* End Date Filter */}
          <div>
            <label htmlFor="endDate" className="block text-sm font-medium text-gray-700 mb-1">
              End Date
            </label>
            <input
              id="endDate"
              type="date"
              value={filters.endDate || ''}
              onChange={(e) => updateFilters({ endDate: e.target.value || undefined })}
              className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
          </div>
        </div>

        {/* Action Buttons */}
        <div className="mt-4 flex gap-2">
          <button
            onClick={() => updateFilters({ userId: undefined, entityType: undefined, action: undefined, startDate: undefined, endDate: undefined })}
            className="px-4 py-2 text-sm text-gray-700 bg-gray-100 rounded hover:bg-gray-200"
          >
            Clear Filters
          </button>
          <button
            onClick={handleExport}
            disabled={isExporting}
            className="px-4 py-2 text-sm text-white bg-blue-600 rounded hover:bg-blue-700 disabled:opacity-50"
          >
            {isExporting ? 'Exporting...' : 'Export to CSV'}
          </button>
        </div>
      </div>

      {/* Error State */}
      {error && (
        <ErrorAlert
          message="Failed to load audit logs"
          details={(error as Error).message}
          onRetry={() => refetch()}
        />
      )}

      {/* Loading State */}
      {isLoading && (
        <div className="flex justify-center py-12">
          <LoadingSpinner />
        </div>
      )}

      {/* Audit Log Table */}
      {!isLoading && !error && (
        <div className="bg-white shadow rounded-lg overflow-hidden">
          <table className="min-w-full divide-y divide-gray-200">
            <thead className="bg-gray-50">
              <tr>
                <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Timestamp
                </th>
                <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  User
                </th>
                <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Action
                </th>
                <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Entity Type
                </th>
                <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Entity ID
                </th>
                <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Old
                </th>
                <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  New
                </th>
              </tr>
            </thead>
            <tbody className="bg-white divide-y divide-gray-200">
              {auditLogs.length === 0 ? (
                <tr>
                  <td colSpan={7} className="px-4 py-8 text-center text-gray-500">
                    No audit logs found. Try adjusting your filters.
                  </td>
                </tr>
              ) : (
                auditLogs.map(renderAuditLogRow)
              )}
            </tbody>
          </table>

          {/* Pagination */}
          {pagination && pagination.totalPages > 1 && (
            <div className="bg-gray-50 px-4 py-3 flex items-center justify-between border-t border-gray-200">
              <div className="flex-1 flex justify-between sm:hidden">
                <button
                  onClick={() => updateFilters({ page: (filters.page || 1) - 1 })}
                  disabled={!pagination.hasPreviousPage}
                  className="relative inline-flex items-center px-4 py-2 border border-gray-300 text-sm font-medium rounded-md text-gray-700 bg-white hover:bg-gray-50 disabled:opacity-50"
                >
                  Previous
                </button>
                <button
                  onClick={() => updateFilters({ page: (filters.page || 1) + 1 })}
                  disabled={!pagination.hasNextPage}
                  className="ml-3 relative inline-flex items-center px-4 py-2 border border-gray-300 text-sm font-medium rounded-md text-gray-700 bg-white hover:bg-gray-50 disabled:opacity-50"
                >
                  Next
                </button>
              </div>
              <div className="hidden sm:flex-1 sm:flex sm:items-center sm:justify-between">
                <div>
                  <p className="text-sm text-gray-700">
                    Showing page <span className="font-medium">{pagination.currentPage}</span> of{' '}
                    <span className="font-medium">{pagination.totalPages}</span> ({pagination.totalItems} total records)
                  </p>
                </div>
                <div>
                  <nav className="relative z-0 inline-flex rounded-md shadow-sm -space-x-px" aria-label="Pagination">
                    <button
                      onClick={() => updateFilters({ page: (filters.page || 1) - 1 })}
                      disabled={!pagination.hasPreviousPage}
                      className="relative inline-flex items-center px-2 py-2 rounded-l-md border border-gray-300 bg-white text-sm font-medium text-gray-500 hover:bg-gray-50 disabled:opacity-50"
                    >
                      Previous
                    </button>
                    <button
                      onClick={() => updateFilters({ page: (filters.page || 1) + 1 })}
                      disabled={!pagination.hasNextPage}
                      className="relative inline-flex items-center px-2 py-2 rounded-r-md border border-gray-300 bg-white text-sm font-medium text-gray-500 hover:bg-gray-50 disabled:opacity-50"
                    >
                      Next
                    </button>
                  </nav>
                </div>
              </div>
            </div>
          )}
        </div>
      )}
    </div>
  )
}
