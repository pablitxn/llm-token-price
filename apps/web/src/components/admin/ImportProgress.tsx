/**
 * ImportProgress Component
 * Multi-stage progress indicator for CSV import operations
 * Story 2.13 Task 12: Add CSV import progress indicator
 *
 * Shows staged progress for better UX:
 * - Stage 1: Uploading file (0-33%)
 * - Stage 2: Validating data (33-66%)
 * - Stage 3: Importing records (66-100%)
 */

import { useEffect, useState } from 'react'
import { Upload, CheckCircle2, Database } from 'lucide-react'

export interface ImportProgressProps {
  /** File being imported */
  fileName: string
  /** File size in bytes */
  fileSize: number
  /** Estimated number of rows (optional) */
  estimatedRows?: number
}

type Stage = 'uploading' | 'validating' | 'importing'

interface ProgressStage {
  id: Stage
  label: string
  icon: typeof Upload
  minProgress: number
  maxProgress: number
}

const STAGES: ProgressStage[] = [
  { id: 'uploading', label: 'Uploading file', icon: Upload, minProgress: 0, maxProgress: 33 },
  { id: 'validating', label: 'Validating data', icon: CheckCircle2, minProgress: 33, maxProgress: 66 },
  { id: 'importing', label: 'Importing records', icon: Database, minProgress: 66, maxProgress: 100 },
]

/**
 * Multi-stage progress indicator for CSV imports
 * Provides better perceived performance by showing distinct processing stages
 */
export function ImportProgress({ fileName, fileSize, estimatedRows }: ImportProgressProps) {
  const [currentStage, setCurrentStage] = useState<Stage>('uploading')
  const [progress, setProgress] = useState(0)

  // Simulate stage progression (since backend is synchronous)
  useEffect(() => {
    // Stage 1: Uploading (0-33%) - 1 second
    const uploadTimer = setTimeout(() => {
      setCurrentStage('validating')
    }, 1000)

    // Stage 2: Validating (33-66%) - 1.5 seconds
    const validateTimer = setTimeout(() => {
      setCurrentStage('importing')
    }, 2500)

    return () => {
      clearTimeout(uploadTimer)
      clearTimeout(validateTimer)
    }
  }, [])

  // Animate progress bar smoothly within current stage
  useEffect(() => {
    const stage = STAGES.find(s => s.id === currentStage)
    if (!stage) return

    const duration = 1000 // 1 second per stage
    const interval = 16 // ~60fps
    const steps = duration / interval
    const progressPerStep = (stage.maxProgress - stage.minProgress) / steps

    let currentProgress = stage.minProgress

    const timer = setInterval(() => {
      currentProgress += progressPerStep
      if (currentProgress >= stage.maxProgress) {
        currentProgress = stage.maxProgress
        clearInterval(timer)
      }
      setProgress(Math.min(currentProgress, stage.maxProgress))
    }, interval)

    return () => clearInterval(timer)
  }, [currentStage])

  const formatFileSize = (bytes: number): string => {
    if (bytes === 0) return '0 Bytes'
    const k = 1024
    const sizes = ['Bytes', 'KB', 'MB']
    const i = Math.floor(Math.log(bytes) / Math.log(k))
    return Math.round((bytes / Math.pow(k, i)) * 100) / 100 + ' ' + sizes[i]
  }

  return (
    <div className="bg-white border border-gray-200 rounded-lg p-6 space-y-6" role="status" aria-live="polite">
      {/* File Information */}
      <div className="flex items-start justify-between">
        <div className="flex-1">
          <h3 className="text-sm font-medium text-gray-900">Processing Import</h3>
          <p className="mt-1 text-sm text-gray-600">{fileName}</p>
          <div className="mt-2 flex items-center gap-4 text-xs text-gray-500">
            <span>Size: {formatFileSize(fileSize)}</span>
            {estimatedRows && <span>Est. {estimatedRows} rows</span>}
          </div>
        </div>
      </div>

      {/* Progress Bar */}
      <div>
        <div className="flex items-center justify-between mb-2">
          <span className="text-sm font-medium text-gray-700">
            {STAGES.find(s => s.id === currentStage)?.label}
          </span>
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
      </div>

      {/* Stage Indicators */}
      <div className="grid grid-cols-3 gap-4">
        {STAGES.map((stage) => {
          const Icon = stage.icon
          const isActive = stage.id === currentStage
          const isCompleted = STAGES.findIndex(s => s.id === currentStage) > STAGES.findIndex(s => s.id === stage.id)

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
          Please wait while we process your import. This may take a few moments depending on the file size.
        </p>
      </div>
    </div>
  )
}
