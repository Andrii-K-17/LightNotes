import { defineStore } from 'pinia'
import { computed, ref } from 'vue'
import { notesService } from '../../../../features/note/services/notesService'
import type { 
  NoteResponseDto, 
  NoteRequestDto, 
  AddCollaboratorRequestDto, 
  UpdateCollaboratorRoleRequestDto 
} from '../types'

/**
 * Pinia store for managing notes state and API interactions.
 */
export const useNotesStore = defineStore('notes', () => {
  const notes = ref<NoteResponseDto[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)

  const hasNotes = computed(() => notes.value.length > 0)
  const sortedNotes = computed(() => {
    return [...notes.value].sort((a, b) => new Date(b.updatedAt).getTime() - new Date(a.updatedAt).getTime())
  })
  
  /**
   * Manages API request state for all async actions.
   */ 
  async function handleApiCall<T>(apiCall: () => Promise<T>): Promise<T | null> {
    loading.value = true
    error.value = null
    try {
      return await apiCall()
    } catch (err: unknown) {
      error.value = err instanceof Error ? err.message : 'An unknown error occurred.'
      return null
    } finally {
      loading.value = false
    }
  }

  /**
   * Fetches all notes and populates the state.
   */
  async function fetchNotes() {
    const data = await handleApiCall(() => notesService.fetchAllNotes())
    if (data) {
      notes.value = data
    }
  }

  /**
   * Fetches a single note by ID.
   */
  async function fetchNoteById(id: string) {
    return await handleApiCall(() => notesService.fetchNoteById(id))
  }

  /**
   * Creates a new note and adds it to the state.
   */
  async function createNote(payload: NoteRequestDto) {
    const created = await handleApiCall(() => notesService.createNote(payload))
    if (created) {
      notes.value.unshift(created)
      return created
    }
    return null
  }

  /**
   * Updates a note in the state.
   */
  async function updateNote(id: string, payload: NoteRequestDto) {
    const updated = await handleApiCall(() => notesService.updateNote(id, payload))
    if (updated) {
      const index = notes.value.findIndex(note => note.id === updated.id)
      if (index !== -1) {
        notes.value[index] = updated
      }
    }
  }

  /**
   * Archives a note and removes it from the state.
   */
  async function archiveNote(id: string) {
    const result = await handleApiCall(() => notesService.archiveNote(id))
    if (result !== null) {
      notes.value = notes.value.filter(note => note.id !== id)
    }
  }

  /**
   * Restores an archived note and adds it to the state.
   */
  async function restoreNote(id: string) {
    const restoredNote = await handleApiCall(() => notesService.restoreNote(id))
    if (restoredNote) {
      notes.value.unshift(restoredNote)
    }
  }

  /**
   * Permanently deletes a note and removes it from the state.
   */
  async function deleteNotePermanently(id: string) {
    const result = await handleApiCall(() => notesService.deleteNotePermanently(id))
    if (result !== null) {
      notes.value = notes.value.filter(note => note.id !== id)
    }
  }

  /**
   * Adds a collaborator to a note.
   */
  async function addCollaborator(noteId: string, payload: AddCollaboratorRequestDto) {
    const newCollaborator = await handleApiCall(() => notesService.addCollaborator(noteId, payload))
    if (newCollaborator) {
      const note = notes.value.find(n => n.id === noteId)
      if (note) {
        note.collaborators.push(newCollaborator) 
      }
    }
  }

  /**
   * Updates a collaborator's role.
   */
  async function updateCollaboratorRole(noteId: string, collaboratorUserId: string, payload: UpdateCollaboratorRoleRequestDto) {
    const updatedCollaborator = await handleApiCall(() => notesService.updateCollaboratorRole(noteId, collaboratorUserId, payload))
    if (updatedCollaborator) {
      const note = notes.value.find(n => n.id === noteId)
      if (note) {
        const collaboratorIndex = note.collaborators.findIndex(c => c.id === updatedCollaborator.id)
        if (collaboratorIndex !== -1) {
          note.collaborators[collaboratorIndex] = updatedCollaborator
        }
      }
    }
  }

  /**
   * Removes a collaborator.
   */
  async function removeCollaborator(noteId: string, collaboratorUserId: string) {
    const result = await handleApiCall(() => notesService.removeCollaborator(noteId, collaboratorUserId))
    if (result !== null) {
      const note = notes.value.find(n => n.id === noteId)
      if (note) {
        note.collaborators = note.collaborators.filter(c => c.id !== collaboratorUserId)
      }
    }
  }
  
  /**
   * Fetches all collaborators for a note.
   */
  async function fetchCollaborators(noteId: string) {
    return await handleApiCall(() => notesService.fetchCollaborators(noteId))
  }

  return {
    notes,
    loading,
    error,
    hasNotes,
    sortedNotes,
    fetchNotes,
    fetchNoteById,
    createNote,
    updateNote,
    archiveNote,
    restoreNote,
    deleteNotePermanently,
    addCollaborator,
    updateCollaboratorRole,
    removeCollaborator,
    fetchCollaborators
  }
})
