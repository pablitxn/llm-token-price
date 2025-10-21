/**
 * BenchmarkScoresSection Component
 * Displays and manages benchmark scores for a model in the admin panel
 * Includes list view, add/edit forms, and delete functionality
 * Story 2.10 - Benchmark Score Entry Form
 */

import { useState } from 'react'
import { useBenchmarkScores, useDeleteBenchmarkScore } from '@/hooks/useBenchmarkScores'
import { BenchmarkScoreForm } from './BenchmarkScoreForm'
import type { BenchmarkScoreResponseDto } from '@/types/admin'

interface BenchmarkScoresSectionProps {
  /** Model ID to display scores for */
  modelId: string
}

/**
 * Complete benchmark scores management section
 * Shows:
 * - List of existing scores with metadata
 * - Add new score form
 * - Edit/delete actions for each score
 * - Empty state when no scores exist
 */
export function BenchmarkScoresSection({ modelId }: BenchmarkScoresSectionProps) {
  const [showAddForm, setShowAddForm] = useState(false)
  const [editingScore, setEditingScore] = useState<BenchmarkScoreResponseDto | null>(null)
  const [deleteConfirmId, setDeleteConfirmId] = useState<string | null>(null)

  const {
    data: scoresData,
    isLoading,
    error,
  } = useBenchmarkScores(modelId)
  const scores = scoresData?.data || []

  const { mutate: deleteScore, isPending: isDeleting } = useDeleteBenchmarkScore(modelId)

  const handleDelete = (scoreId: string) => {
    deleteScore(scoreId, {
      onSuccess: () => {
        setDeleteConfirmId(null)
      },
    })
  }

  if (isLoading) {
    return (
      <div className="py-8 text-center">
        <div className="inline-block animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
        <p className="mt-2 text-sm text-gray-600">Loading benchmark scores...</p>
      </div>
    )
  }

  if (error) {
    return (
      <div className="p-4 bg-red-50 border border-red-200 rounded-md">
        <p className="text-sm text-red-800">
          <strong>Error loading scores:</strong> {error.message}
        </p>
      </div>
    )
  }

  return (
    <div className="space-y-6">
      {/* Section Header */}
      <div className="flex items-center justify-between">
        <div>
          <h3 className="text-lg font-medium text-gray-900">Benchmark Scores</h3>
          <p className="mt-1 text-sm text-gray-500">
            Add performance benchmarks to help users compare this model
          </p>
        </div>
        {!showAddForm && !editingScore && (
          <button
            onClick={() => setShowAddForm(true)}
            className="px-4 py-2 text-sm font-medium text-white bg-blue-600 border border-transparent rounded-md hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2"
          >
            Add Score
          </button>
        )}
      </div>

      {/* Add Score Form */}
      {showAddForm && (
        <div className="p-6 bg-gray-50 border border-gray-200 rounded-lg">
          <h4 className="text-md font-medium text-gray-900 mb-4">Add New Benchmark Score</h4>
          <BenchmarkScoreForm
            modelId={modelId}
            mode="create"
            onSuccess={() => setShowAddForm(false)}
            onCancel={() => setShowAddForm(false)}
          />
        </div>
      )}

      {/* Edit Score Form */}
      {editingScore && (
        <div className="p-6 bg-gray-50 border border-gray-200 rounded-lg">
          <h4 className="text-md font-medium text-gray-900 mb-4">Edit Benchmark Score</h4>
          <BenchmarkScoreForm
            modelId={modelId}
            mode="edit"
            score={editingScore}
            onSuccess={() => setEditingScore(null)}
            onCancel={() => setEditingScore(null)}
          />
        </div>
      )}

      {/* Scores List */}
      {scores.length === 0 ? (
        <div className="text-center py-12 bg-gray-50 border-2 border-dashed border-gray-300 rounded-lg">
          <svg
            className="mx-auto h-12 w-12 text-gray-400"
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
          >
            <path
              strokeLinecap="round"
              strokeLinejoin="round"
              strokeWidth={2}
              d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z"
            />
          </svg>
          <h3 className="mt-2 text-sm font-medium text-gray-900">No benchmark scores</h3>
          <p className="mt-1 text-sm text-gray-500">
            Get started by adding a benchmark score for this model.
          </p>
          {!showAddForm && (
            <button
              onClick={() => setShowAddForm(true)}
              className="mt-4 px-4 py-2 text-sm font-medium text-white bg-blue-600 border border-transparent rounded-md hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2"
            >
              Add First Score
            </button>
          )}
        </div>
      ) : (
        <div className="bg-white shadow-sm border border-gray-200 rounded-lg overflow-hidden">
          <table className="min-w-full divide-y divide-gray-200">
            <thead className="bg-gray-50">
              <tr>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Benchmark
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Category
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Score
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Normalized
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Status
                </th>
                <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Actions
                </th>
              </tr>
            </thead>
            <tbody className="bg-white divide-y divide-gray-200">
              {scores.map((score) => (
                <tr key={score.id} className="hover:bg-gray-50">
                  <td className="px-6 py-4 whitespace-nowrap">
                    <div className="flex flex-col">
                      <div className="text-sm font-medium text-gray-900">{score.benchmarkName}</div>
                      {score.notes && (
                        <div className="text-xs text-gray-500 mt-1">{score.notes}</div>
                      )}
                    </div>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    <span className="px-2 inline-flex text-xs leading-5 font-semibold rounded-full bg-blue-100 text-blue-800">
                      {score.category}
                    </span>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    <div className="text-sm text-gray-900">
                      {score.score}
                      {score.maxScore && ` / ${score.maxScore}`}
                    </div>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    <div className="text-sm text-gray-900">
                      {(score.normalizedScore * 100).toFixed(1)}%
                    </div>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    <div className="flex flex-col gap-1">
                      {score.verified && (
                        <span className="px-2 inline-flex text-xs leading-5 font-semibold rounded-full bg-green-100 text-green-800">
                          Verified
                        </span>
                      )}
                      {score.isOutOfRange && (
                        <span className="px-2 inline-flex text-xs leading-5 font-semibold rounded-full bg-yellow-100 text-yellow-800">
                          Out of Range
                        </span>
                      )}
                    </div>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                    <button
                      onClick={() => setEditingScore(score)}
                      className="text-blue-600 hover:text-blue-900 mr-4"
                      disabled={!!editingScore || showAddForm}
                    >
                      Edit
                    </button>
                    {deleteConfirmId === score.id ? (
                      <div className="inline-flex gap-2">
                        <button
                          onClick={() => handleDelete(score.id)}
                          disabled={isDeleting}
                          className="text-red-600 hover:text-red-900 disabled:opacity-50"
                        >
                          Confirm
                        </button>
                        <button
                          onClick={() => setDeleteConfirmId(null)}
                          disabled={isDeleting}
                          className="text-gray-600 hover:text-gray-900 disabled:opacity-50"
                        >
                          Cancel
                        </button>
                      </div>
                    ) : (
                      <button
                        onClick={() => setDeleteConfirmId(score.id)}
                        className="text-red-600 hover:text-red-900"
                        disabled={!!editingScore || showAddForm}
                      >
                        Delete
                      </button>
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {/* Summary Stats */}
      {scores.length > 0 && (
        <div className="bg-blue-50 border border-blue-200 rounded-lg p-4">
          <div className="flex items-start">
            <svg
              className="h-5 w-5 text-blue-400 mr-2 flex-shrink-0"
              fill="currentColor"
              viewBox="0 0 20 20"
            >
              <path
                fillRule="evenodd"
                d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7-4a1 1 0 11-2 0 1 1 0 012 0zM9 9a1 1 0 000 2v3a1 1 0 001 1h1a1 1 0 100-2v-3a1 1 0 00-1-1H9z"
                clipRule="evenodd"
              />
            </svg>
            <div className="flex-1">
              <p className="text-sm text-blue-800">
                <strong>Total Scores:</strong> {scores.length} |{' '}
                <strong>Verified:</strong> {scores.filter((s) => s.verified).length} |{' '}
                <strong>Out of Range:</strong> {scores.filter((s) => s.isOutOfRange).length}
              </p>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}
