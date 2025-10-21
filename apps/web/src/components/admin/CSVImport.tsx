/**
 * CSVImport Component
 * Bulk import benchmark scores via CSV file upload
 * Supports drag-and-drop, file validation, and displays import results
 * Story 2.11 AC#1
 */

import { useState, useRef, DragEvent, ChangeEvent } from 'react'
import { Upload, FileText, AlertCircle, Download } from 'lucide-react'
import { useImportBenchmarkCSV } from '@/hooks/useBenchmarkScores'
import { ImportResults } from './ImportResults'
import { ImportProgress } from './ImportProgress'

/**
 * CSV bulk import component for benchmark scores
 * Features:
 * - File input with drag-and-drop zone
 * - CSV file validation
 * - Upload progress indicator
 * - Download CSV template
 * - Import results display
 */
export function CSVImport() {
  const [selectedFile, setSelectedFile] = useState<File | null>(null)
  const [isDragging, setIsDragging] = useState(false)
  const fileInputRef = useRef<HTMLInputElement>(null)

  const importMutation = useImportBenchmarkCSV()

  /**
   * Handle file selection from input or drop
   */
  const handleFileSelect = (file: File | null) => {
    if (!file) {
      setSelectedFile(null)
      return
    }

    // Validate CSV file
    if (!file.name.endsWith('.csv')) {
      alert('Please select a CSV file')
      return
    }

    // Validate file size (10MB max per Story 2.11 AC#3)
    const maxSizeBytes = 10 * 1024 * 1024 // 10MB
    if (file.size > maxSizeBytes) {
      alert('File size exceeds 10MB limit')
      return
    }

    setSelectedFile(file)
    // Clear previous results
    importMutation.reset()
  }

  /**
   * Handle file input change event
   */
  const handleFileChange = (e: ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0] || null
    handleFileSelect(file)
  }

  /**
   * Handle drag enter event
   */
  const handleDragEnter = (e: DragEvent<HTMLDivElement>) => {
    e.preventDefault()
    e.stopPropagation()
    setIsDragging(true)
  }

  /**
   * Handle drag leave event
   */
  const handleDragLeave = (e: DragEvent<HTMLDivElement>) => {
    e.preventDefault()
    e.stopPropagation()
    setIsDragging(false)
  }

  /**
   * Handle drag over event
   */
  const handleDragOver = (e: DragEvent<HTMLDivElement>) => {
    e.preventDefault()
    e.stopPropagation()
  }

  /**
   * Handle file drop event
   */
  const handleDrop = (e: DragEvent<HTMLDivElement>) => {
    e.preventDefault()
    e.stopPropagation()
    setIsDragging(false)

    const file = e.dataTransfer.files?.[0] || null
    handleFileSelect(file)
  }

  /**
   * Trigger file input click
   */
  const handleBrowseClick = () => {
    fileInputRef.current?.click()
  }

  /**
   * Upload CSV file
   */
  const handleUpload = () => {
    if (!selectedFile) return

    const formData = new FormData()
    formData.append('file', selectedFile)

    importMutation.mutate(formData)
  }

  /**
   * Download CSV template
   */
  const handleDownloadTemplate = () => {
    // CSV template with headers and example rows (Story 2.11 AC#2)
    const template = `model_id,benchmark_name,score,max_score,test_date,source_url,verified,notes
550e8400-e29b-41d4-a716-446655440000,MMLU,85.2,100,2025-10-01,https://example.com/results,true,Official benchmark
550e8400-e29b-41d4-a716-446655440000,HumanEval,0.72,1,2025-10-02,https://example.com/eval,false,
550e8400-e29b-41d4-a716-446655440001,GSM8K,78.5,100,2025-10-01,,true,Internal test`

    const blob = new Blob([template], { type: 'text/csv' })
    const url = URL.createObjectURL(blob)
    const link = document.createElement('a')
    link.href = url
    link.download = 'benchmark-scores-template.csv'
    document.body.appendChild(link)
    link.click()
    document.body.removeChild(link)
    URL.revokeObjectURL(url)
  }

  /**
   * Reset form for another import
   */
  const handleImportAnother = () => {
    setSelectedFile(null)
    importMutation.reset()
    if (fileInputRef.current) {
      fileInputRef.current.value = ''
    }
  }

  /**
   * Format file size in human-readable format
   */
  const formatFileSize = (bytes: number): string => {
    if (bytes === 0) return '0 Bytes'
    const k = 1024
    const sizes = ['Bytes', 'KB', 'MB']
    const i = Math.floor(Math.log(bytes) / Math.log(k))
    return Math.round(bytes / Math.pow(k, i) * 100) / 100 + ' ' + sizes[i]
  }

  return (
    <div className="space-y-6">
      {/* Header with template download */}
      <div className="flex items-center justify-between">
        <div>
          <h2 className="text-lg font-semibold text-gray-900">Bulk Import Benchmark Scores</h2>
          <p className="mt-1 text-sm text-gray-500">
            Import multiple benchmark scores at once using a CSV file
          </p>
        </div>
        <button
          type="button"
          onClick={handleDownloadTemplate}
          className="inline-flex items-center gap-2 px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-md shadow-sm hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500"
        >
          <Download className="w-4 h-4" />
          Download Template
        </button>
      </div>

      {/* CSV Format Documentation */}
      <div className="bg-blue-50 border border-blue-200 rounded-lg p-4">
        <h3 className="text-sm font-medium text-blue-900 mb-2">CSV Format Requirements</h3>
        <ul className="text-sm text-blue-800 space-y-1 list-disc list-inside">
          <li><strong>model_id</strong>: UUID of the model (required)</li>
          <li><strong>benchmark_name</strong>: Benchmark identifier, e.g., "MMLU" (required)</li>
          <li><strong>score</strong>: Benchmark score as decimal number (required)</li>
          <li><strong>max_score</strong>: Maximum possible score (optional)</li>
          <li><strong>test_date</strong>: Date in YYYY-MM-DD format (optional)</li>
          <li><strong>source_url</strong>: URL to benchmark results (optional)</li>
          <li><strong>verified</strong>: true/false (optional, defaults to false)</li>
          <li><strong>notes</strong>: Additional notes (optional)</li>
        </ul>
      </div>

      {/* All-or-Nothing Import Notice (Story 2.13 Task 6.6) */}
      <div className="bg-amber-50 border border-amber-200 rounded-lg p-4">
        <div className="flex items-start gap-3">
          <AlertCircle className="w-5 h-5 text-amber-600 flex-shrink-0 mt-0.5" />
          <div>
            <h3 className="text-sm font-medium text-amber-900 mb-1">All-or-Nothing Import Policy</h3>
            <p className="text-sm text-amber-800">
              <strong>All rows must be valid for the import to succeed.</strong> If any single row
              contains errors (invalid model ID, unknown benchmark, etc.), the entire import will be
              rejected and NO rows will be added to the database. Please review your CSV file
              carefully before uploading.
            </p>
          </div>
        </div>
      </div>

      {/* Progress Indicator - Story 2.13 Task 12: Enhanced progress indication */}
      {importMutation.isPending && selectedFile && (
        <ImportProgress
          fileName={selectedFile.name}
          fileSize={selectedFile.size}
        />
      )}

      {/* Upload Section - Only show if not processing and no results yet */}
      {!importMutation.isPending && !importMutation.isSuccess && (
        <div className="bg-white shadow rounded-lg p-6">
          {/* Drag and Drop Zone */}
          <div
            onDragEnter={handleDragEnter}
            onDragLeave={handleDragLeave}
            onDragOver={handleDragOver}
            onDrop={handleDrop}
            className={`
              relative border-2 border-dashed rounded-lg p-8 text-center transition-colors
              ${isDragging
                ? 'border-blue-500 bg-blue-50'
                : 'border-gray-300 hover:border-gray-400'
              }
            `}
          >
            <input
              ref={fileInputRef}
              type="file"
              accept=".csv"
              onChange={handleFileChange}
              className="hidden"
              aria-label="CSV file input"
            />

            {!selectedFile ? (
              <div className="space-y-4">
                <Upload className="mx-auto h-12 w-12 text-gray-400" />
                <div>
                  <p className="text-base font-medium text-gray-900">
                    Drop your CSV file here, or{' '}
                    <button
                      type="button"
                      onClick={handleBrowseClick}
                      className="text-blue-600 hover:text-blue-500 underline"
                    >
                      browse
                    </button>
                  </p>
                  <p className="mt-1 text-sm text-gray-500">
                    CSV files up to 10MB
                  </p>
                </div>
              </div>
            ) : (
              <div className="space-y-4">
                <FileText className="mx-auto h-12 w-12 text-blue-600" />
                <div>
                  <p className="text-base font-medium text-gray-900">
                    {selectedFile.name}
                  </p>
                  <p className="mt-1 text-sm text-gray-500">
                    {formatFileSize(selectedFile.size)}
                  </p>
                </div>
                <button
                  type="button"
                  onClick={() => handleFileSelect(null)}
                  className="text-sm text-gray-600 hover:text-gray-500 underline"
                >
                  Remove file
                </button>
              </div>
            )}
          </div>

          {/* Upload Button */}
          <div className="mt-6 flex items-center justify-end gap-4">
            <button
              type="button"
              onClick={handleUpload}
              disabled={!selectedFile}
              className="px-6 py-2 text-sm font-medium text-white bg-blue-600 border border-transparent rounded-md shadow-sm hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 disabled:opacity-50 disabled:cursor-not-allowed"
            >
              Upload and Import
            </button>
          </div>

          {/* Error Display */}
          {importMutation.isError && (
            <div className="mt-4 bg-red-50 border border-red-200 rounded-lg p-4 flex items-start gap-3">
              <AlertCircle className="h-5 w-5 text-red-600 flex-shrink-0 mt-0.5" />
              <div>
                <h4 className="text-sm font-medium text-red-900">Upload failed</h4>
                <p className="mt-1 text-sm text-red-800">
                  {importMutation.error?.message || 'An error occurred while processing the CSV file. Please try again.'}
                </p>
              </div>
            </div>
          )}
        </div>
      )}

      {/* Import Results Display - Story 2.11 AC#6 */}
      {importMutation.isSuccess && importMutation.data && (
        <ImportResults
          result={importMutation.data}
          onImportAnother={handleImportAnother}
        />
      )}
    </div>
  )
}
