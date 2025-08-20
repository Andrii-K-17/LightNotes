import router from '../../app/router'
import { useAuthStore } from '../../entities/session/model/store/auth'
import { API_CONFIG } from '../config/api'

/**
 * A basic HTTP client for executing requests to the API.
 */
export const apiClient = async <T>(endpoint: string, options?: RequestInit): Promise<T | null> => {
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

  // Handle 401 (unauthorized) error to automatically log out the user.
  if (response.status === 401) {
    const authStore = useAuthStore()
    authStore.logout()
    
    router.push('/login')
    
    throw new Error('Unauthorized: User session has expired.')
  }

  if (!response.ok) {
    const errorData = await response.json()
    const errorMessage = errorData.detail || errorData.title || 'An error occurred during the request.'
    throw new Error(errorMessage)
  }

  // Handle successful requests with no content
  if (response.status === 204) {
    return null
  }

  return response.json()
}
