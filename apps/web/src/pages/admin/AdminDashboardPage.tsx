import { useAuth } from '../../hooks/useAuth'

/**
 * AdminDashboardPage component
 * Main admin panel dashboard (placeholder for now)
 *
 * @component
 */
export default function AdminDashboardPage() {
  const { user, logout } = useAuth()

  return (
    <div className="min-h-screen bg-slate-50 p-8">
      <div className="max-w-7xl mx-auto">
        {/* Header */}
        <div className="bg-white shadow-md rounded-lg border border-slate-200 p-6 mb-6">
          <div className="flex items-center justify-between">
            <div>
              <h1 className="text-2xl font-bold text-slate-900">Admin Dashboard</h1>
              <p className="text-sm text-slate-600 mt-1">
                Welcome back, <span className="font-medium">{user?.username}</span>
              </p>
            </div>
            <button
              onClick={() => logout()}
              className="px-4 py-2 bg-red-600 hover:bg-red-700 text-white rounded-md shadow-sm font-medium transition-colors duration-200"
            >
              Logout
            </button>
          </div>
        </div>

        {/* Content */}
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
    </div>
  )
}
