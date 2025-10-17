import axios from 'axios'

export const apiClient = axios.create({
  baseURL: '/api', // Vite proxy forwards to http://localhost:5000/api
  timeout: 10000,
  headers: {
    'Content-Type': 'application/json',
  },
  withCredentials: true, // Required to send/receive cookies with cross-origin requests
})

// Request interceptor (placeholder for future auth)
apiClient.interceptors.request.use(
  (config) => {
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
