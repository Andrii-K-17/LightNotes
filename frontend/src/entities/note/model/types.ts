export type Role = 'Owner' | 'Editor' | 'Viewer'

export interface AddCollaboratorRequestDto {
  userEmail: string
  role: Role
}

export interface NoteCollaboratorDto {
  id: string
  name: string
  email: string
  role: Role
}

export interface NoteRequestDto {
  title: string
  content: string
  color?: string | null
  isPinned: boolean
  isArchived: boolean
  reminderAt?: string | null
  tags: NoteTagDto[]
}

export interface NoteResponseDto {
  id: string
  title: string
  content: string
  color?: string | null
  ownerId: string
  ownerName: string
  isPinned: boolean
  isArchived: boolean
  createdAt: string
  updatedAt: string
  reminderAt?: string | null
  tags: NoteTagDto[]
  collaborators: NoteCollaboratorDto[]
}

export interface NoteTagDto {
  tag: string
}

export interface UpdateCollaboratorRoleRequestDto {
  newRole: Role
}
