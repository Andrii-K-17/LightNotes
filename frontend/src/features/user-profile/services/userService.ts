import { apiClient } from "../../../shared/services/apiClient"

/**
 * Service for interacting with the user API.
 */
export const userService = {
  /**
   * Deletes a user's account.
   */
  async deleteUserAccount(userId: string) {
    await apiClient(`/User/${userId}`, {
      method: 'DELETE',
    })
  },
}
