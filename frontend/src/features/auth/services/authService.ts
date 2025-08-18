import { apiClient } from '../../../shared/services/apiClient'
import type {
  AuthResponseDto,
  LoginRequestDto,
  RegisterRequestDto
} from '../../../entities/session/model/types'

/**
 * Service for interacting with the auth API.
 */
export const authService = {
  /**
   * Sends a login request to the API.
   */
  async login(credentials: LoginRequestDto): Promise<AuthResponseDto> {
    return apiClient<AuthResponseDto>('/Auth/login', {
      method: 'POST',
      body: JSON.stringify(credentials),
    })
  },

  /**
   * Sends a registration request to the API.
   */
  async register(registrationData: RegisterRequestDto) {
    await apiClient('/Auth/register', {
      method: 'POST',
      body: JSON.stringify(registrationData),
    })
  },
}
