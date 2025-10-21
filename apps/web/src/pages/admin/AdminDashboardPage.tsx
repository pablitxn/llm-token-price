import { useAuth } from '../../hooks/useAuth'
import { useDashboardMetrics } from '../../hooks/useDashboardMetrics'
import { useNavigate } from 'react-router-dom'

/**
 * AdminDashboardPage Component
 *
 * Main admin panel dashboard showing quick stats and overview.
 * Rendered within AdminLayout component (Story 2.2).
 *
 * Story 2.12: Now displays real-time data freshness metrics:
 * - Total active models
 * - Models needing updates (>7 days)
 * - Critical updates (>30 days)
 * - Recently updated models (<7 days)
 *
 * @component
 */
export default function AdminDashboardPage() {
  const { user } = useAuth()
  const { data: metrics, isLoading, error } = useDashboardMetrics()
  const navigate = useNavigate()

  return (
    <div>
      {/* Welcome Message */}
      <div className="mb-6">
        <h1 className="text-2xl font-bold text-slate-900">Dashboard</h1>
        <p className="text-sm text-slate-600 mt-1">
          Welcome back, <span className="font-medium">{user?.username}</span>
        </p>
      </div>

      {/* Loading State */}
      {isLoading && (
        <div className="bg-white shadow-md rounded-lg border border-slate-200 p-12 text-center">
          <div className="inline-block h-8 w-8 animate-spin rounded-full border-4 border-solid border-blue-600 border-r-transparent" role="status">
            <span className="!absolute !-m-px !h-px !w-px !overflow-hidden !whitespace-nowrap !border-0 !p-0 ![clip:rect(0,0,0,0)]">
              Loading metrics...
            </span>
          </div>
          <p className="mt-4 text-sm text-slate-600">Loading dashboard metrics...</p>
        </div>
      )}

      {/* Error State */}
      {error && (
        <div className="bg-red-50 border border-red-200 rounded-lg p-6">
          <h3 className="text-sm font-medium text-red-800">Error loading dashboard metrics</h3>
          <p className="mt-2 text-sm text-red-700">{error.message}</p>
        </div>
      )}

      {/* Data Freshness Metrics */}
      {!isLoading && !error && metrics && (
        <div className="bg-white shadow-md rounded-lg border border-slate-200 p-6">
          <h2 className="text-lg font-semibold text-slate-900 mb-4">Data Freshness Metrics</h2>
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
            {/* Total Models */}
            <div className="bg-blue-50 border border-blue-200 rounded-lg p-4">
              <p className="text-sm text-blue-600 font-medium">Total Models</p>
              <p className="text-2xl font-bold text-blue-900 mt-1">{metrics.totalActiveModels}</p>
            </div>

            {/* Recently Updated */}
            <div
              className="bg-green-50 border border-green-200 rounded-lg p-4 cursor-pointer hover:bg-green-100 transition-colors"
              onClick={() => navigate('/admin/models?freshness=fresh')}
            >
              <p className="text-sm text-green-600 font-medium">Recently Updated</p>
              <p className="text-2xl font-bold text-green-900 mt-1">{metrics.recentlyUpdated}</p>
              <p className="text-xs text-green-600 mt-2">Fresh (&lt; 7 days)</p>
            </div>

            {/* Needs Update */}
            <div
              className="bg-yellow-50 border border-yellow-200 rounded-lg p-4 cursor-pointer hover:bg-yellow-100 transition-colors"
              onClick={() => navigate('/admin/models?freshness=stale')}
            >
              <p className="text-sm text-yellow-600 font-medium">Needs Update</p>
              <p className="text-2xl font-bold text-yellow-900 mt-1">{metrics.modelsNeedingUpdates}</p>
              <p className="text-xs text-yellow-600 mt-2">Stale (7-30 days)</p>
            </div>

            {/* Critical Updates */}
            <div
              className="bg-red-50 border border-red-200 rounded-lg p-4 cursor-pointer hover:bg-red-100 transition-colors"
              onClick={() => navigate('/admin/models?freshness=critical')}
            >
              <p className="text-sm text-red-600 font-medium">Critical Updates</p>
              <p className="text-2xl font-bold text-red-900 mt-1">{metrics.criticalUpdates}</p>
              <p className="text-xs text-red-600 mt-2">Critical (&gt; 30 days)</p>
            </div>
          </div>

          {/* Pricing Freshness */}
          {metrics.pricingNeedingUpdates > 0 && (
            <div className="mt-6 p-4 bg-orange-50 border border-orange-200 rounded-lg">
              <p className="text-sm text-orange-700">
                <span className="font-medium">{metrics.pricingNeedingUpdates} models</span> have pricing data older than 30 days and may need verification.
              </p>
            </div>
          )}

          <p className="text-xs text-slate-500 mt-6">
            Click on a metric card to view filtered model list
          </p>
        </div>
      )}

      {/* Data Quality Metrics (Story 2.13 Task 15) */}
      {!isLoading && !error && metrics && (
        <div className="mt-6 bg-white shadow-md rounded-lg border border-slate-200 p-6">
          <h2 className="text-lg font-semibold text-slate-900 mb-4">Data Quality Metrics</h2>
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
            {/* Incomplete Models */}
            <div className="bg-purple-50 border border-purple-200 rounded-lg p-4">
              <p className="text-sm text-purple-600 font-medium">Incomplete Models</p>
              <p className="text-2xl font-bold text-purple-900 mt-1">{metrics.incompleteModels}</p>
              <p className="text-xs text-purple-600 mt-2">&lt; 3 benchmark scores</p>
            </div>

            {/* Recent Additions */}
            <div className="bg-cyan-50 border border-cyan-200 rounded-lg p-4">
              <p className="text-sm text-cyan-600 font-medium">Recent Additions</p>
              <p className="text-2xl font-bold text-cyan-900 mt-1">{metrics.recentAdditions}</p>
              <p className="text-xs text-cyan-600 mt-2">Added in last 7 days</p>
            </div>

            {/* Average Benchmarks */}
            <div className="bg-indigo-50 border border-indigo-200 rounded-lg p-4">
              <p className="text-sm text-indigo-600 font-medium">Avg Benchmarks/Model</p>
              <p className="text-2xl font-bold text-indigo-900 mt-1">{metrics.averageBenchmarksPerModel.toFixed(1)}</p>
              <p className="text-xs text-indigo-600 mt-2">Overall coverage quality</p>
            </div>
          </div>

          {/* Models by Provider - Simple Bar Chart */}
          {Object.keys(metrics.modelsByProvider).length > 0 && (
            <div className="mt-6">
              <h3 className="text-md font-semibold text-slate-900 mb-3">Models by Provider</h3>
              <div className="space-y-2">
                {Object.entries(metrics.modelsByProvider)
                  .sort(([, a], [, b]) => b - a)
                  .map(([provider, count]) => {
                    const maxCount = Math.max(...Object.values(metrics.modelsByProvider))
                    const percentage = (count / maxCount) * 100

                    return (
                      <div key={provider} className="flex items-center gap-3">
                        <span className="text-sm font-medium text-slate-700 w-24 text-right">{provider}</span>
                        <div className="flex-1 bg-slate-100 rounded-full h-6 relative">
                          <div
                            className="bg-blue-600 h-6 rounded-full flex items-center justify-end pr-3 transition-all duration-500"
                            style={{ width: `${percentage}%` }}
                          >
                            <span className="text-xs font-semibold text-white">{count}</span>
                          </div>
                        </div>
                      </div>
                    )
                  })}
              </div>
              <p className="text-xs text-slate-500 mt-4">
                Total providers: {Object.keys(metrics.modelsByProvider).length}
              </p>
            </div>
          )}
        </div>
      )}
    </div>
  )
}
