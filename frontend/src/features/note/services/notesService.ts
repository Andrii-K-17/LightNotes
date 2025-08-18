import { apiClient } from '../../../shared/services/apiClient'
import type { 
  NoteResponseDto,
  NoteRequestDto,
  NoteCollaboratorDto,
  AddCollaboratorRequestDto,
  UpdateCollaboratorRoleRequestDto
} from '../../../entities/note/model/types'

/**
 * Service for interacting with the notes API.
 */
export const notesService = {
  /**
   * Gets all notes of the current user.
   */
  async fetchAllNotes(): Promise<NoteResponseDto[]> {
    return apiClient<NoteResponseDto[]>('/Notes')
  },

  /**
   * Gets a note by its id.
   */
  async fetchNoteById(id: string): Promise<NoteResponseDto> {
    return apiClient<NoteResponseDto>(`/Notes/${id}`)
  },

  /**
   * Creates a new note.
   */
  async createNote(payload: NoteRequestDto): Promise<NoteResponseDto> {
    return apiClient<NoteResponseDto>('/Notes', {
      method: 'POST',
      body: JSON.stringify(payload),
    })
  },

  /**
   * Updates an existing note.
   */
  async updateNote(id: string, payload: NoteRequestDto): Promise<NoteResponseDto> {
    return apiClient<NoteResponseDto>(`/Notes/${id}`, {
      method: 'PUT',
      body: JSON.stringify(payload),
    })
  },

  /**
   * Archives a note (soft delete).
   */
  async archiveNote(id: string) {
    await apiClient(`/Notes/${id}`, {
      method: 'DELETE',
    })
  },

  /**
   * Restores an archived note.
   */
  async restoreNote(id: string) {
    await apiClient(`/Notes/${id}/restore`, {
      method: 'POST',
    })
  },

  /**
   * Permanently deletes a note (hard delete).
   */
  async deleteNotePermanently(id: string) {
    await apiClient(`/Notes/${id}/permanent`, {
      method: 'DELETE',
    })
  },

  /**
   * Adds a new collaborator to a specific note.
   */
  async addCollaborator(noteId: string, payload: AddCollaboratorRequestDto): Promise<NoteCollaboratorDto> {
    return apiClient<NoteCollaboratorDto>(`/Notes/${noteId}/collaborators`, {
      method: 'POST',
      body: JSON.stringify(payload),
    })
  },

  /**
   * Updates the role of an existing collaborator.
   */
  async updateCollaboratorRole(noteId: string, collaboratorUserId: string, payload: UpdateCollaboratorRoleRequestDto): Promise<NoteCollaboratorDto> {
    return apiClient<NoteCollaboratorDto>(`/Notes/${noteId}/collaborators/${collaboratorUserId}`, {
      method: 'PUT',
      body: JSON.stringify(payload),
    })
  },

  /**
   * Removes a collaborator from a note.
   */
  async removeCollaborator(noteId: string, collaboratorUserId: string) {
    await apiClient(`/Notes/${noteId}/collaborators/${collaboratorUserId}`, {
      method: 'DELETE',
    })
  },

  /**
   * Gets the list of collaborators for a specific note.
   */
  async fetchCollaborators(noteId: string): Promise<NoteCollaboratorDto[]> {
    return apiClient<NoteCollaboratorDto[]>(`/Notes/${noteId}/collaborators`)
  },
}
