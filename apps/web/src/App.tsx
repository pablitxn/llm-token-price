import { Routes, Route, Navigate } from 'react-router-dom'
import Layout from './components/layout/Layout'
import HomePage from './pages/HomePage'
import CalculatorPage from './pages/CalculatorPage'
import ComparisonPage from './pages/ComparisonPage'
import NotFoundPage from './pages/NotFoundPage'
import AdminLoginPage from './pages/admin/AdminLoginPage'
import AdminDashboardPage from './pages/admin/AdminDashboardPage'
import AdminModelsPage from './pages/admin/AdminModelsPage'
import { AddModelPage } from './pages/admin/AddModelPage'
import { EditModelPage } from './pages/admin/EditModelPage'
import AdminBenchmarksPage from './pages/admin/AdminBenchmarksPage'
import { AddBenchmarkPage } from './pages/admin/AddBenchmarkPage'
import { EditBenchmarkPage } from './pages/admin/EditBenchmarkPage'
import AdminAuditLogPage from './pages/admin/AdminAuditLogPage'
import AdminLayout from './components/layout/AdminLayout'
import ProtectedRoute from './components/auth/ProtectedRoute'

export default function App() {
  return (
    <Routes>
      {/* Public Routes (with Layout) */}
      <Route path="/" element={<Layout><HomePage /></Layout>} />
      <Route path="/calculator" element={<Layout><CalculatorPage /></Layout>} />
      <Route path="/compare" element={<Layout><ComparisonPage /></Layout>} />

      {/* Admin Login (no layout) */}
      <Route path="/admin/login" element={<AdminLoginPage />} />

      {/* Admin Routes (nested with AdminLayout) */}
      <Route
        path="/admin"
        element={
          <ProtectedRoute>
            <AdminLayout />
          </ProtectedRoute>
        }
      >
        {/* Default /admin redirects to /admin/dashboard */}
        <Route index element={<Navigate to="dashboard" replace />} />
        <Route path="dashboard" element={<AdminDashboardPage />} />
        <Route path="models" element={<AdminModelsPage />} />
        <Route path="models/new" element={<AddModelPage />} />
        <Route path="models/:id/edit" element={<EditModelPage />} />
        <Route path="benchmarks" element={<AdminBenchmarksPage />} />
        <Route path="benchmarks/new" element={<AddBenchmarkPage />} />
        <Route path="benchmarks/:id/edit" element={<EditBenchmarkPage />} />
        <Route path="audit-log" element={<AdminAuditLogPage />} />
      </Route>

      {/* 404 Not Found */}
      <Route path="*" element={<Layout><NotFoundPage /></Layout>} />
    </Routes>
  )
}
