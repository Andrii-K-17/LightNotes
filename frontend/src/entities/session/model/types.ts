export interface RegisterRequestDto {
  email: string
  password: string
  name: string
}

export interface LoginRequestDto {
  email: string
  password: string
}

export interface AuthResponseDto {
  userId: string
  name: string
  email: string
  token: string
}

export interface User {
  id: string
  name: string
  email: string
}
