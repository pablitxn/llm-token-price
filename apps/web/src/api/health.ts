import { apiClient } from './client'

export const checkHealth = () => apiClient.get('/health')
