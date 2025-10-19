/**
 * TanStack Query mutation hook for creating a new model
 * Handles API calls, error states, loading states, and cache invalidation
 */

import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useNavigate } from 'react-router-dom'
import { createModel } from '@/api/admin'
import type { CreateModelRequest, AdminModelResponse } from '@/types/admin'

/**
 * Hook for creating a new model with TanStack Query mutation
 *
 * Features:
 * - Automatic cache invalidation for admin models list on success
 * - Navigation to models list page after successful creation
 * - Error handling with field-specific validation messages
 * - Loading state management
 *
 * @returns Mutation object with mutate function, loading/error states
 *
 * @example
 * ```tsx
 * const { mutate, isPending, error } = useCreateModel()
 *
 * const handleSubmit = (data: CreateModelRequest) => {
 *   mutate(data, {
 *     onSuccess: (response) => {
 *       toast.success(`Model '${response.data.name}' created successfully`)
 *     }
 *   })
 * }
 * ```
 */
export function useCreateModel() {
  const queryClient = useQueryClient()
  const navigate = useNavigate()

  return useMutation<AdminModelResponse, Error, CreateModelRequest>({
    mutationFn: createModel,
    onSuccess: () => {
      // Invalidate admin models query to refresh the list
      queryClient.invalidateQueries({ queryKey: ['admin', 'models'] })

      // Navigate back to models list
      navigate('/admin/models')
    },
    onError: (error) => {
      // Error handling is managed by the component
      // Authentication errors (401) should redirect to login
      console.error('Failed to create model:', error)
    },
  })
}
