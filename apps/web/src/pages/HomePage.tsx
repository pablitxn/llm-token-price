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
      {/* Hero Section */}
      <section className="text-center mb-8">
        <h1 className="text-3xl md:text-4xl lg:text-5xl font-bold text-gray-900 mb-4">
          LLM Token Price Comparison
        </h1>
        <p className="text-lg md:text-xl text-gray-600 max-w-3xl mx-auto">
          Compare pricing, capabilities, and benchmarks across Large Language Model providers
        </p>
      </section>

      {/* Main Content Area */}
      <div className="w-full max-w-6xl mx-auto">
        {isLoading && (
          <div className="text-center py-12" role="status" aria-live="polite">
            <div className="inline-block animate-spin rounded-full h-12 w-12 border-4 border-gray-200 border-t-gray-900 rounded-full"></div>
            <p className="mt-4 text-gray-600 text-lg">Loading model data...</p>
          </div>
        )}

        {error && (
          <div className="bg-red-50 border border-red-200 rounded-lg p-4 text-center" role="alert">
            <p className="text-red-800">Error loading models: {(error as Error).message}</p>
          </div>
        )}

        {data && (
          <article>
            <div className="mb-6 text-sm text-gray-600">
              Found {data.meta.count} models • Last updated: {new Date(data.meta.timestamp).toLocaleString()}
            </div>

            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
              {data.data.map((model) => (
                <ModelCard key={model.id} model={model} />
              ))}
            </div>
          </article>
        )}
      </div>
    </div>
  )
}
