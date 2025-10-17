import type { ModelDto } from '@/types/models'

interface ModelCardProps {
  model: ModelDto
}

export default function ModelCard({ model }: ModelCardProps) {
  return (
    <div className="bg-white rounded-lg shadow-md p-6 hover:shadow-lg transition-shadow">
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
  )
}
