/**
 * AddBenchmarkPage Component
 * Page for creating a new benchmark definition in the admin panel
 * Story 2.9: Create Benchmark Definitions Management
 */

import { BenchmarkForm } from '@/components/admin/BenchmarkForm'

export function AddBenchmarkPage() {
  return (
    <div className="px-4 sm:px-6 lg:px-8">
      {/* Page Header */}
      <div className="mb-8">
        <h1 className="text-2xl font-semibold text-gray-900">Add New Benchmark</h1>
        <p className="mt-2 text-sm text-gray-700">
          Create a new benchmark definition for scoring LLM models.
        </p>
      </div>

      {/* Form */}
      <BenchmarkForm mode="create" />
    </div>
  )
}
