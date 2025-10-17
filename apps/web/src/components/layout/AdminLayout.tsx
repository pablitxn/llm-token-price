import { useState } from 'react'
import { Outlet } from 'react-router-dom'
import AdminSidebar from './AdminSidebar'
import AdminHeader from './AdminHeader'

/**
 * AdminLayout Component
 *
 * Master-detail layout for admin panel with responsive behavior:
 * - Desktop (≥1024px): Permanent sidebar navigation
 * - Tablet/Mobile (<1024px): Collapsible drawer sidebar
 *
 * Uses React Router's Outlet pattern for nested routing.
 * All admin routes (/admin/dashboard, /admin/models, /admin/benchmarks)
 * render through this layout.
 *
 * @component
 * @example
 * ```tsx
 * <Route path="/admin" element={<ProtectedRoute><AdminLayout /></ProtectedRoute>}>
 *   <Route path="dashboard" element={<AdminDashboardPage />} />
 *   <Route path="models" element={<AdminModelsPage />} />
 * </Route>
 * ```
 */
export default function AdminLayout() {
  // State for mobile/tablet sidebar collapse
  const [isSidebarOpen, setIsSidebarOpen] = useState(false)

  return (
    <div className="min-h-screen bg-slate-50 flex">
      {/* Sidebar - Always visible on desktop (lg:), drawer on mobile */}
      <AdminSidebar isOpen={isSidebarOpen} onClose={() => setIsSidebarOpen(false)} />

      {/* Main Content Area */}
      <div className="flex-1 flex flex-col lg:ml-64">
        {/* Header with breadcrumb, user dropdown, hamburger menu */}
        <AdminHeader onMenuClick={() => setIsSidebarOpen(!isSidebarOpen)} />

        {/* Dynamic Content from nested routes */}
        <main className="flex-1 p-6">
          <Outlet />
        </main>
      </div>

      {/* Mobile overlay - closes sidebar when clicking outside */}
      {isSidebarOpen && (
        <div
          className="fixed inset-0 bg-black/50 z-30 lg:hidden"
          onClick={() => setIsSidebarOpen(false)}
          aria-hidden="true"
        />
      )}
    </div>
  )
}
