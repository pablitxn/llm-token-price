import { useQuery } from '@tanstack/react-query'
import { fetchModels } from '../api/models'

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
                <div
                  key={model.id}
                  className="bg-white rounded-lg shadow-md p-6 hover:shadow-lg transition-shadow"
                >
                  <div className="flex justify-between items-start mb-3">
                    <div>
                      <h3 className="text-lg font-semibold text-gray-900">{model.name}</h3>
                      <p className="text-sm text-gray-600">{model.provider}</p>
                    </div>
                    <span className="px-2 py-1 text-xs font-medium bg-green-100 text-green-800 rounded">
                      {model.status}
                    </span>
                  </div>

                  <div className="space-y-2 mb-4">
                    <div className="flex justify-between text-sm">
                      <span className="text-gray-600">Input:</span>
                      <span className="font-medium">${model.inputPricePer1M}/1M tokens</span>
                    </div>
                    <div className="flex justify-between text-sm">
                      <span className="text-gray-600">Output:</span>
                      <span className="font-medium">${model.outputPricePer1M}/1M tokens</span>
                    </div>
                  </div>

                  {model.capabilities && (
                    <div className="mb-4 pt-4 border-t border-gray-200">
                      <p className="text-xs text-gray-600 mb-2">Capabilities:</p>
                      <div className="flex flex-wrap gap-1">
                        {model.capabilities.supportsVision && (
                          <span className="px-2 py-0.5 text-xs bg-blue-100 text-blue-800 rounded">Vision</span>
                        )}
                        {model.capabilities.supportsFunctionCalling && (
                          <span className="px-2 py-0.5 text-xs bg-purple-100 text-purple-800 rounded">Functions</span>
                        )}
                        {model.capabilities.supportsJsonMode && (
                          <span className="px-2 py-0.5 text-xs bg-green-100 text-green-800 rounded">JSON Mode</span>
                        )}
                      </div>
                      <p className="text-xs text-gray-500 mt-2">
                        Context: {model.capabilities.contextWindow.toLocaleString()} tokens
                      </p>
                    </div>
                  )}

                  {model.topBenchmarks.length > 0 && (
                    <div className="pt-4 border-t border-gray-200">
                      <p className="text-xs text-gray-600 mb-2">Top Benchmarks:</p>
                      <div className="space-y-1">
                        {model.topBenchmarks.map((benchmark) => (
                          <div key={benchmark.benchmarkName} className="flex justify-between text-xs">
                            <span className="text-gray-600">{benchmark.benchmarkName}:</span>
                            <span className="font-medium">{benchmark.score}/{benchmark.maxScore}</span>
                          </div>
                        ))}
                      </div>
                    </div>
                  )}
                </div>
              ))}
            </div>
          </div>
        )}
      </div>
    </div>
  )
}
