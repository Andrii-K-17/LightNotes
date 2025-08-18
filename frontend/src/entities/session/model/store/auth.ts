import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { authService } from '../../../../features/auth/services/authService'
import type { LoginRequestDto, RegisterRequestDto, User } from '../types'

/**
 * Retrieves the initial authentication state from localStorage.
 * This ensures the user remains authenticated on page refresh.
 */
const getInitialState = () => {
  const token = localStorage.getItem('authToken')

  const userJson = localStorage.getItem('user')
  let user: User | null = null

  if (userJson) {
    try {
      user = JSON.parse(userJson)
    } catch (e) {
      console.error('Failed to parse user data from localStorage', e)
    }
  }

  return { token, user }
}

/**
 * Pinia store for managing authentication state.
 */
export const useAuthStore = defineStore('auth', () => {
  const { token: initialToken, user: initialUser } = getInitialState()
  const token = ref<string | null>(initialToken)
  const user = ref<User | null>(initialUser)
  const loading = ref<boolean>(false)
  const error = ref<string | null>(null)

  const isAuthenticated = computed<boolean>(() => !!token.value)

  /**
   * Authenticates the user with the provided credentials.
   */
  async function login(credentials: LoginRequestDto) {
    loading.value = true
    error.value = null
    try {
      const result = await authService.login(credentials)

      token.value = result.token
      
      const loggedInUser: User = {
        id: result.userId,
        name: result.name,
        email: result.email,
      }
      user.value = loggedInUser

      localStorage.setItem('authToken', result.token)
      localStorage.setItem('user', JSON.stringify(loggedInUser))
    } catch (err: unknown) {
      error.value = err instanceof Error ? err.message : 'An unknown error occurred.'
    } finally {
      loading.value = false
    }
  }

  /**
   * Registers a new user.
   */
  async function register(registrationData: RegisterRequestDto) {
    loading.value = true
    error.value = null
    try {
      await authService.register(registrationData)
    } catch (err: unknown) {
      error.value = err instanceof Error ? err.message : 'An unknown error occurred.'
    } finally {
      loading.value = false
    }
  }

  /**
   * Clears the user's session and removes data from localStorage.
   */
  function logout() {
    token.value = null
    user.value = null
    localStorage.removeItem('authToken')
    localStorage.removeItem('user')
  }

  return {
    token,
    user,
    loading,
    error,
    isAuthenticated,
    login,
    register,
    logout,
  }
})
