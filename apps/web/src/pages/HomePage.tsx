import { useQuery } from '@tanstack/react-query'
import { fetchModels } from '../api/models'
import ModelCard from '@/components/models/ModelCard'

export default function HomePage() {
  const { data, isLoading, error } = useQuery({
    queryKey: ['models'],
    queryFn: fetchModels,
  })

  return (
    <div className="flex flex-col min-h-[60vh]">
      <div className="text-center mb-8">
        <h1 className="text-4xl font-bold text-gray-900 mb-4">
          LLM Token Price Comparison
        </h1>
        <p className="text-xl text-gray-600">
          Compare pricing, capabilities, and benchmarks across Large Language Model providers
        </p>
      </div>

      <div className="w-full max-w-6xl mx-auto">
        {isLoading && (
          <div className="text-center py-8">
            <div className="inline-block animate-spin rounded-full h-8 w-8 border-b-2 border-gray-900"></div>
            <p className="mt-4 text-gray-600">Loading models...</p>
          </div>
        )}

        {error && (
          <div className="bg-red-50 border border-red-200 rounded-lg p-4 text-center">
            <p className="text-red-800">Error loading models: {(error as Error).message}</p>
          </div>
        )}

        {data && (
          <div>
            <div className="mb-4 text-sm text-gray-600">
              Found {data.meta.count} models • Last updated: {new Date(data.meta.timestamp).toLocaleString()}
            </div>

            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
              {data.data.map((model) => (
                <ModelCard key={model.id} model={model} />
              ))}
            </div>
          </div>
        )}
      </div>
    </div>
  )
}
