import { useParams, useNavigate } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import { getAdminModelById } from '@/api/admin'
import { ModelForm } from '@/components/admin/ModelForm'
import { BenchmarkScoresSection } from '@/components/admin/BenchmarkScoresSection'

export function EditModelPage() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()

  const {
    data: model,
    isLoading,
    error,
  } = useQuery({
    queryKey: ['admin', 'models', id],
    queryFn: () => {
      if (!id) throw new Error('Model ID is required')
      return getAdminModelById(id)
    },
    enabled: !!id,
  })

  if (!id) {
    return (
      <div className="max-w-4xl mx-auto p-6">
        <div className="bg-red-50 border border-red-200 text-red-800 rounded-lg p-4">
          <h2 className="text-lg font-semibold mb-2">Invalid Model ID</h2>
          <p>No model ID was provided in the URL.</p>
          <button
            onClick={() => navigate('/admin/models')}
            className="mt-4 px-4 py-2 bg-red-600 text-white rounded-lg hover:bg-red-700"
          >
            Back to Models List
          </button>
        </div>
      </div>
    )
  }

  if (isLoading) {
    return (
      <div className="max-w-4xl mx-auto p-6">
        <div className="flex items-center justify-center min-h-[400px]">
          <div className="flex flex-col items-center gap-4">
            <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
            <p className="text-gray-600">Loading model data...</p>
          </div>
        </div>
      </div>
    )
  }

  if (error || !model) {
    return (
      <div className="max-w-4xl mx-auto p-6">
        <div className="bg-red-50 border border-red-200 text-red-800 rounded-lg p-4">
          <h2 className="text-lg font-semibold mb-2">Model Not Found</h2>
          <p>
            {error instanceof Error
              ? error.message
              : 'The model you are trying to edit could not be found.'}
          </p>
          <button
            onClick={() => navigate('/admin/models')}
            className="mt-4 px-4 py-2 bg-red-600 text-white rounded-lg hover:bg-red-700"
          >
            Back to Models List
          </button>
        </div>
      </div>
    )
  }

  return (
    <div className="max-w-4xl mx-auto p-6 space-y-8">
      <div className="mb-6">
        <h1 className="text-2xl font-bold text-gray-900">
          Edit Model: {model.name}
        </h1>
        <p className="text-gray-600 mt-1">
          Update pricing, capabilities, or other model information
        </p>
      </div>

      {/* Model Basic Information Form */}
      <ModelForm mode="edit" modelId={id} model={model} />

      {/* Benchmark Scores Section */}
      <div className="border-t border-gray-200 pt-8">
        <BenchmarkScoresSection modelId={id} />
      </div>
    </div>
  )
}
