/**
 * useCSVImportSSE Hook
 * Custom hook for CSV import with real-time progress updates via Server-Sent Events (SSE)
 * Story 2.13 Task 12: Add CSV import progress indicator
 *
 * Features:
 * - EventSource connection to /api/admin/benchmarks/import-csv-stream
 * - Real-time progress updates (parsing, validating, importing phases)
 * - Cancellation support (closes EventSource connection)
 * - Error handling and automatic reconnection
 */

import { useState, useCallback, useRef } from 'react'
import type { CSVImportProgressData } from '@/components/admin/ImportProgress'
import type { CSVImportResultDto } from '@/types/admin'

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000'

export interface UseCSVImportSSEState {
  isImporting: boolean
  progressData: CSVImportProgressData | null
  result: CSVImportResultDto | null
  error: string | null
}

export interface UseCSVImportSSEReturn extends UseCSVImportSSEState {
  startImport: (formData: FormData) => Promise<void>
  cancelImport: () => void
  reset: () => void
}

/**
 * Hook for CSV import with SSE progress tracking
 * Connects to backend SSE endpoint and provides real-time progress updates
 */
export function useCSVImportSSE(): UseCSVImportSSEReturn {
  const [state, setState] = useState<UseCSVImportSSEState>({
    isImporting: false,
    progressData: null,
    result: null,
    error: null,
  })

  const eventSourceRef = useRef<EventSource | null>(null)
  const abortControllerRef = useRef<AbortController | null>(null)

  /**
   * Cancel ongoing import by closing EventSource connection
   * Backend detects disconnection via CancellationToken
   */
  const cancelImport = useCallback(() => {
    if (eventSourceRef.current) {
      eventSourceRef.current.close()
      eventSourceRef.current = null
    }

    if (abortControllerRef.current) {
      abortControllerRef.current.abort()
      abortControllerRef.current = null
    }

    setState((prev) => ({
      ...prev,
      isImporting: false,
      progressData: {
        phase: 'Cancelled',
        totalRows: prev.progressData?.totalRows || 0,
        processedRows: 0,
        successCount: 0,
        failureCount: 0,
        skippedCount: 0,
        percentComplete: 0,
        message: 'Import cancelled by user',
      },
    }))
  }, [])

  /**
   * Reset state for a new import
   */
  const reset = useCallback(() => {
    cancelImport()
    setState({
      isImporting: false,
      progressData: null,
      result: null,
      error: null,
    })
  }, [cancelImport])

  /**
   * Start CSV import with SSE progress updates
   * Uploads file to /api/admin/benchmarks/import-csv-stream endpoint
   */
  const startImport = useCallback(
    async (formData: FormData): Promise<void> => {
      try {
        setState({
          isImporting: true,
          progressData: null,
          result: null,
          error: null,
        })

        // Get auth token from localStorage (or wherever your auth is stored)
        const token = localStorage.getItem('auth_token') // Adjust based on your auth implementation

        // Upload file and establish SSE connection
        abortControllerRef.current = new AbortController()

        const response = await fetch(`${API_BASE_URL}/api/admin/benchmarks/import-csv-stream`, {
          method: 'POST',
          body: formData,
          headers: {
            Authorization: token ? `Bearer ${token}` : '',
          },
          signal: abortControllerRef.current.signal,
        })

        if (!response.ok) {
          const errorText = await response.text()
          let errorMessage = 'Failed to start import'
          try {
            const errorJson = JSON.parse(errorText)
            errorMessage = errorJson.error || errorMessage
          } catch {
            errorMessage = errorText || errorMessage
          }
          throw new Error(errorMessage)
        }

        // Process SSE stream
        const reader = response.body?.getReader()
        const decoder = new TextDecoder()

        if (!reader) {
          throw new Error('Failed to get response stream')
        }

        let buffer = ''

        while (true) {
          const { done, value } = await reader.read()

          if (done) {
            break
          }

          buffer += decoder.decode(value, { stream: true })

          // Split on double newlines (SSE message separator)
          const messages = buffer.split('\n\n')
          buffer = messages.pop() || '' // Keep incomplete message in buffer

          for (const message of messages) {
            if (!message.trim()) continue

            // Parse SSE data: format is "data: {json}\n"
            const dataMatch = message.match(/^data: (.*)$/m)
            if (dataMatch && dataMatch[1]) {
              try {
                const progressData: CSVImportProgressData = JSON.parse(dataMatch[1])

                setState((prev) => ({
                  ...prev,
                  progressData,
                  // Store final result if phase is Complete
                  result:
                    progressData.phase === 'Complete' && 'finalResult' in progressData
                      ? (progressData as any).finalResult
                      : prev.result,
                }))

                // Check if import is complete
                if (progressData.phase === 'Complete' || progressData.phase === 'Failed' || progressData.phase === 'Cancelled') {
                  setState((prev) => ({
                    ...prev,
                    isImporting: false,
                    error: progressData.phase === 'Failed' ? (progressData as any).errorMessage || 'Import failed' : null,
                  }))
                  reader.cancel()
                  break
                }
              } catch (parseError) {
                console.error('Failed to parse SSE message:', dataMatch[1], parseError)
              }
            }
          }
        }
      } catch (error) {
        if (error instanceof Error && error.name === 'AbortError') {
          // User cancelled - already handled in cancelImport
          return
        }

        console.error('CSV import error:', error)
        setState((prev) => ({
          ...prev,
          isImporting: false,
          error: error instanceof Error ? error.message : 'Unknown error occurred',
        }))
      } finally {
        abortControllerRef.current = null
      }
    },
    []
  )

  return {
    ...state,
    startImport,
    cancelImport,
    reset,
  }
}
