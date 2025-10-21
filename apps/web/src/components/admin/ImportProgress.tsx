/**
 * ImportProgress Component
 * Real-time progress indicator for CSV import operations via SSE
 * Story 2.13 Task 12: Add CSV import progress indicator
 *
 * Displays live progress updates from backend:
 * - Parsing → Validating → Importing → Complete
 * - Real row counts (processed/total)
 * - Success/failure/skipped counters
 * - Cancellation support
 */

import { Upload, CheckCircle2, Database, XCircle } from 'lucide-react'

export interface CSVImportProgressData {
  phase: string // "Parsing" | "Validating" | "Importing" | "Complete" | "Cancelled" | "Failed"
  totalRows: number
  processedRows: number
  successCount: number
  failureCount: number
  skippedCount: number
  percentComplete: number
  message: string
}

export interface ImportProgressProps {
  /** File being imported */
  fileName: string
  /** File size in bytes */
  fileSize: number
  /** Real-time progress data from SSE stream */
  progressData?: CSVImportProgressData
  /** Cancel import callback */
  onCancel?: () => void
}

type Stage = 'parsing' | 'validating' | 'importing' | 'complete'

interface ProgressStage {
  id: Stage
  label: string
  icon: typeof Upload
}

const STAGES: ProgressStage[] = [
  { id: 'parsing', label: 'Parsing CSV', icon: Upload },
  { id: 'validating', label: 'Validating data', icon: CheckCircle2 },
  { id: 'importing', label: 'Importing records', icon: Database },
]

/**
 * Real-time progress indicator for CSV imports using SSE updates from backend
 * Displays actual progress instead of simulated stages
 */
export function ImportProgress({ fileName, fileSize, progressData, onCancel }: ImportProgressProps) {
  const formatFileSize = (bytes: number): string => {
    if (bytes === 0) return '0 Bytes'
    const k = 1024
    const sizes = ['Bytes', 'KB', 'MB']
    const i = Math.floor(Math.log(bytes) / Math.log(k))
    return Math.round((bytes / Math.pow(k, i)) * 100) / 100 + ' ' + sizes[i]
  }

  // Determine current stage from progress data
  const getCurrentStage = (): Stage => {
    if (!progressData) return 'parsing'
    const phase = progressData.phase.toLowerCase()
    if (phase === 'parsing') return 'parsing'
    if (phase === 'validating') return 'validating'
    if (phase === 'importing') return 'importing'
    return 'complete'
  }

  const currentStage = getCurrentStage()
  const progress = progressData?.percentComplete ?? 0
  const message = progressData?.message ?? 'Starting import...'

  return (
    <div className="bg-white border border-gray-200 rounded-lg p-6 space-y-6" role="status" aria-live="polite">
      {/* File Information */}
      <div className="flex items-start justify-between">
        <div className="flex-1">
          <h3 className="text-sm font-medium text-gray-900">Processing Import</h3>
          <p className="mt-1 text-sm text-gray-600">{fileName}</p>
          <div className="mt-2 flex items-center gap-4 text-xs text-gray-500">
            <span>Size: {formatFileSize(fileSize)}</span>
            {progressData && progressData.totalRows > 0 && (
              <span>{progressData.totalRows} rows</span>
            )}
          </div>
        </div>
        {/* Cancel Button (Task 12.6) */}
        {onCancel && currentStage !== 'complete' && (
          <button
            type="button"
            onClick={onCancel}
            className="inline-flex items-center gap-2 px-3 py-1.5 text-sm font-medium text-red-700 bg-red-50 border border-red-200 rounded-md hover:bg-red-100 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-red-500"
            aria-label="Cancel import"
          >
            <XCircle className="w-4 h-4" />
            Cancel
          </button>
        )}
      </div>

      {/* Progress Bar (Task 12.3, 12.4) */}
      <div>
        <div className="flex items-center justify-between mb-2">
          <span className="text-sm font-medium text-gray-700">{message}</span>
          <span className="text-sm font-medium text-gray-900">{Math.round(progress)}%</span>
        </div>
        <div className="w-full bg-gray-200 rounded-full h-2 overflow-hidden">
          <div
            className="bg-blue-600 h-2 rounded-full transition-all duration-200 ease-out"
            style={{ width: `${progress}%` }}
            role="progressbar"
            aria-valuenow={Math.round(progress)}
            aria-valuemin={0}
            aria-valuemax={100}
            aria-label={`Import progress: ${Math.round(progress)}%`}
          />
        </div>
        {/* Real-time counters (Task 12.4) */}
        {progressData && progressData.totalRows > 0 && (
          <div className="mt-3 flex items-center justify-between text-xs">
            <span className="text-gray-600">
              Processing row {progressData.processedRows} of {progressData.totalRows}
            </span>
            <div className="flex items-center gap-4">
              {progressData.successCount > 0 && (
                <span className="text-green-600 font-medium">
                  ✓ {progressData.successCount} valid
                </span>
              )}
              {progressData.failureCount > 0 && (
                <span className="text-red-600 font-medium">
                  ✗ {progressData.failureCount} failed
                </span>
              )}
              {progressData.skippedCount > 0 && (
                <span className="text-yellow-600 font-medium">
                  ⊘ {progressData.skippedCount} skipped
                </span>
              )}
            </div>
          </div>
        )}
      </div>

      {/* Stage Indicators */}
      <div className="grid grid-cols-3 gap-4">
        {STAGES.map((stage) => {
          const Icon = stage.icon
          const isActive = stage.id === currentStage
          const currentIndex = STAGES.findIndex(s => s.id === currentStage)
          const stageIndex = STAGES.findIndex(s => s.id === stage.id)
          const isCompleted = currentIndex > stageIndex

          return (
            <div
              key={stage.id}
              className={`
                flex items-center gap-2 p-3 rounded-lg border transition-all
                ${isActive ? 'bg-blue-50 border-blue-200' : ''}
                ${isCompleted ? 'bg-green-50 border-green-200' : ''}
                ${!isActive && !isCompleted ? 'bg-gray-50 border-gray-200' : ''}
              `}
            >
              <Icon
                className={`
                  w-5 h-5 flex-shrink-0
                  ${isActive ? 'text-blue-600 animate-pulse' : ''}
                  ${isCompleted ? 'text-green-600' : ''}
                  ${!isActive && !isCompleted ? 'text-gray-400' : ''}
                `}
                aria-hidden="true"
              />
              <div className="flex-1 min-w-0">
                <p
                  className={`
                    text-xs font-medium truncate
                    ${isActive ? 'text-blue-900' : ''}
                    ${isCompleted ? 'text-green-900' : ''}
                    ${!isActive && !isCompleted ? 'text-gray-500' : ''}
                  `}
                >
                  {stage.label}
                </p>
              </div>
            </div>
          )
        })}
      </div>

      {/* Helper Text */}
      <div className="pt-2 border-t border-gray-200">
        <p className="text-xs text-gray-500">
          {progressData?.phase === 'Cancelled'
            ? 'Import cancelled. No rows were imported.'
            : progressData?.phase === 'Failed'
              ? 'Import failed. Please check the error message and try again.'
              : 'Please wait while we process your import. You can cancel anytime by clicking the Cancel button.'}
        </p>
      </div>
    </div>
  )
}
