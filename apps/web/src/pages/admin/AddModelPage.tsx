/**
 * AddModelPage Component
 * Admin page for creating a new LLM model
 * Protected route requiring authentication via AdminLayout
 */

import { ModelForm } from '@/components/admin/ModelForm'

/**
 * Add New Model page
 * Displays form for creating a new model with breadcrumb navigation
 */
export function AddModelPage() {
  return (
    <div className="space-y-6">
      {/* Breadcrumb */}
      <nav className="flex text-sm text-gray-500" aria-label="Breadcrumb">
        <ol className="flex items-center space-x-2">
          <li>
            <a href="/admin" className="hover:text-gray-700">
              Dashboard
            </a>
          </li>
          <li>
            <span className="mx-2">/</span>
          </li>
          <li>
            <a href="/admin/models" className="hover:text-gray-700">
              Models
            </a>
          </li>
          <li>
            <span className="mx-2">/</span>
          </li>
          <li className="text-gray-900 font-medium">Add New Model</li>
        </ol>
      </nav>

      {/* Page Header */}
      <div>
        <h1 className="text-2xl font-bold text-gray-900">Add New Model</h1>
        <p className="mt-2 text-sm text-gray-600">
          Add a new LLM model to the database with pricing and configuration details.
        </p>
      </div>

      {/* Model Form */}
      <ModelForm mode="create" />
    </div>
  )
}
