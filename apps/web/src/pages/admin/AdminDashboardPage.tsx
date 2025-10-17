import { useAuth } from '../../hooks/useAuth'

/**
 * AdminDashboardPage Component
 *
 * Main admin panel dashboard showing quick stats and overview.
 * Rendered within AdminLayout component (Story 2.2).
 *
 * Future enhancements (Story 2.7+):
 * - Real-time data quality metrics
 * - Recent activity feed
 * - Model/benchmark statistics
 * - Admin action audit log preview
 *
 * @component
 */
export default function AdminDashboardPage() {
  const { user } = useAuth()

  return (
    <div>
      {/* Welcome Message */}
      <div className="mb-6">
        <h1 className="text-2xl font-bold text-slate-900">Dashboard</h1>
        <p className="text-sm text-slate-600 mt-1">
          Welcome back, <span className="font-medium">{user?.username}</span>
        </p>
      </div>

      {/* Quick Stats */}
      <div className="bg-white shadow-md rounded-lg border border-slate-200 p-6">
        <h2 className="text-lg font-semibold text-slate-900 mb-4">Quick Stats</h2>
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
          <div className="bg-blue-50 border border-blue-200 rounded-lg p-4">
            <p className="text-sm text-blue-600 font-medium">Total Models</p>
            <p className="text-2xl font-bold text-blue-900 mt-1">--</p>
          </div>
          <div className="bg-green-50 border border-green-200 rounded-lg p-4">
            <p className="text-sm text-green-600 font-medium">Active Benchmarks</p>
            <p className="text-2xl font-bold text-green-900 mt-1">--</p>
          </div>
          <div className="bg-purple-50 border border-purple-200 rounded-lg p-4">
            <p className="text-sm text-purple-600 font-medium">Last Updated</p>
            <p className="text-2xl font-bold text-purple-900 mt-1">--</p>
          </div>
        </div>
        <p className="text-sm text-slate-500 mt-6">
          Admin panel features will be implemented in future stories (Epic 2).
        </p>
      </div>
    </div>
  )
}
