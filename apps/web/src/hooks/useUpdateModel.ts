import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useNavigate } from 'react-router-dom'
import { updateModel } from '@/api/admin'
import type { CreateModelRequest } from '@/types/admin'

/**
 * TanStack Query mutation hook for updating an existing model
 * Handles cache invalidation and navigation on success
 *
 * @returns Mutation object with mutate, isPending, error, and data states
 *
 * @example
 * ```tsx
 * const { mutate: updateModel, isPending, error } = useUpdateModel()
 *
 * const handleSubmit = (data: CreateModelRequest) => {
 *   updateModel({ id: modelId, data })
 * }
 * ```
 */
export function useUpdateModel() {
  const queryClient = useQueryClient()
  const navigate = useNavigate()

  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: CreateModelRequest }) =>
      updateModel(id, data),

    onSuccess: (response) => {
      // Invalidate admin models list cache to reflect changes
      queryClient.invalidateQueries({ queryKey: ['admin', 'models'] })

      // Invalidate specific model cache
      queryClient.invalidateQueries({
        queryKey: ['admin', 'models', response.id],
      })

      // Navigate back to models list
      navigate('/admin/models')

      // Optional: Show success notification
      // This could be integrated with a toast/notification system in the future
      console.log(`Model '${response.name}' updated successfully`)
    },

    onError: (error) => {
      // Error handling - errors will be displayed in the form
      console.error('Failed to update model:', error)
    },
  })
}
