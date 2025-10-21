import axios from 'axios'
import { getCurrentLanguage } from '@/components/admin/LanguageSelector'

export const apiClient = axios.create({
  baseURL: '/api', // Vite proxy forwards to http://localhost:5000/api
  timeout: 10000,
  headers: {
    'Content-Type': 'application/json',
  },
  withCredentials: true, // Required to send/receive cookies with cross-origin requests
})

// Request interceptor - Adds Accept-Language header and auth token
apiClient.interceptors.request.use(
  (config) => {
    // Add Accept-Language header for localized validation messages (Story 2.13 Task 13.6)
    // Backend RequestLocalizationMiddleware reads this header to set CultureInfo
    const language = getCurrentLanguage()
    config.headers['Accept-Language'] = language

    // Add auth token here when implemented
    // const token = localStorage.getItem('token')
    // if (token) {
    //   config.headers.Authorization = `Bearer ${token}`
    // }
    return config
  },
  (error) => Promise.reject(error)
)

// Response interceptor (error handling)
apiClient.interceptors.response.use(
  (response) => response,
  (error) => {
    console.error('API Error:', error.response?.data || error.message)
    return Promise.reject(error)
  }
)
