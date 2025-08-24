import { defineStore } from 'pinia'
import { ref } from 'vue'
import { useAuthStore } from '../../../session/model/store/auth'
import { userService } from '../../../../features/user-profile/services/userService'
import { useRouter } from 'vue-router'

/**
 * Pinia store for managing user profile data and actions.
 */
export const useUserProfileStore = defineStore('user-profile', () => {  
  const loading = ref(false)
  const error = ref<string | null>(null)
  const authStore = useAuthStore()
  const router = useRouter()

  /**
   * Deletes the current user's account.
   */
  async function deleteAccount() {
    loading.value = true
    error.value = null

    const userId = authStore.user!.id

    try {
      await userService.deleteUserAccount(userId)
      authStore.logout()
      await router.push('/register')
    } catch (err: unknown) {
      error.value = err instanceof Error ? err.message : 'An unknown error occurred.'
      console.error('Error deleting account:', err)
      throw err 
    } finally {
      loading.value = false
    }
  }

  return {
    loading,
    error,
    deleteAccount,
  }
})
