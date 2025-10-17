import { apiClient } from './client'
import type { ModelsResponse, ModelResponse } from '../types/models'

/**
 * Fetches all active models from the API
 * @returns Promise resolving to the models response
 */
export const fetchModels = async (): Promise<ModelsResponse> => {
  const response = await apiClient.get<ModelsResponse>('/models')
  return response.data
}

/**
 * Fetches a single model by ID from the API
 * @param id - The model's unique identifier (GUID)
 * @returns Promise resolving to the model response
 */
export const fetchModelById = async (id: string): Promise<ModelResponse> => {
  const response = await apiClient.get<ModelResponse>(`/models/${id}`)
  return response.data
}
