import { API_CONFIG } from '../config/api'

/**
 * A basic HTTP client for executing requests to the API.
 */
export const apiClient = async <T>(endpoint: string, options?: RequestInit): Promise<T> => {
  const token = localStorage.getItem('authToken')

  const headers: HeadersInit = {
    'Content-Type': 'application/json',
    ...(token ? { 'Authorization': `Bearer ${token}` } : {}),
    ...options?.headers,
  }

  const response = await fetch(`${API_CONFIG.BASE_URL}${endpoint}`, {
    ...options,
    headers,
  })

  if (!response.ok) {
    const errorData = await response.json()
    const errorMessage = errorData.detail || errorData.title || 'An error occurred during the request.'
    throw new Error(errorMessage)
  }

  return response.json()
}
