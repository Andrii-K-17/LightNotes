import { defineStore } from 'pinia'
import { computed, ref } from 'vue'
import { notesService } from '../../../../features/note/services/notesService'
import { 
  type NoteResponseDto, 
  type NoteRequestDto, 
  type AddCollaboratorRequestDto, 
  type UpdateCollaboratorRoleRequestDto, 
  Role
} from '../types'
import { useAuthStore } from '../../../session/model/store/auth'

/**
 * Pinia store for managing notes state and API interactions.
 */
export const useNotesStore = defineStore('notes', () => {
  const notes = ref<NoteResponseDto[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)
  const searchQuery = ref('')

  const authStore = useAuthStore()

  const hasNotes = computed(() => notes.value.length > 0)

  const nonArchivedNotes = computed(() => {
    return notes.value.filter(note => !note.isArchived)
  })

  const sortedNotes = computed(() => {
    const sortedNonArchivedNotes = [...nonArchivedNotes.value].sort((a, b) => {
      if (a.isPinned && !b.isPinned) {
        return -1
      }
      if (!a.isPinned && b.isPinned) {
        return 1
      }
      
      return new Date(b.updatedAt).getTime() - new Date(a.updatedAt).getTime()
    })
    return sortedNonArchivedNotes.filter(note => note.collaborators.length === 0)
  })

  const applySearchFilter = (notesToFilter: NoteResponseDto[]) => {
    if (!searchQuery.value) {
      return notesToFilter
    }
    
    const query = searchQuery.value.toLowerCase()
    
    return notesToFilter.filter(note => 
      note.title.toLowerCase().includes(query) || 
      note.content.toLowerCase().includes(query) ||
      note.tags.some(tag => tag.tag.toLowerCase().includes(query))
    )
  }
  
  const filteredSortedNotes = computed(() => applySearchFilter(sortedNotes.value))
  const filteredArchivedNotes = computed(() => applySearchFilter(archivedNotes.value))
  const filteredReminderNotes = computed(() => applySearchFilter(reminderNotes.value))
  const filteredSharedNotes = computed(() => applySearchFilter(sharedNotes.value))

  const archivedNotes = computed(() => {
    return notes.value.filter(note => note.isArchived)
  })

  const reminderNotes = computed(() => {
    return notes.value.filter(note => note.reminderAt && new Date(note.reminderAt) > new Date())
  })

  const hasArchivedNotes = computed(() => archivedNotes.value.length > 0)
  const hasReminderNotes = computed(() => reminderNotes.value.length > 0)

  const sharedNotes = computed(() => {
    const currentUserId = authStore.user?.id
    return nonArchivedNotes.value.filter(note => {
      const isCollaborator = note.collaborators.some(collaborator => 
        collaborator.userId === currentUserId
      )
      const isSharedNoteOwner = note.ownerId === currentUserId && note.collaborators.length > 0
      return isCollaborator || isSharedNoteOwner
    }).sort((a, b) => {
      if (a.isPinned && !b.isPinned) {
        return -1
      }
      if (!a.isPinned && b.isPinned) {
        return 1
      }
      
      return new Date(b.updatedAt).getTime() - new Date(a.updatedAt).getTime()
    })
  })

  function hasEditPermissions(note: NoteResponseDto): boolean {
    if (!authStore.user || !note) {
      return false
    }

    const isOwner = authStore.user.id === note.ownerId
    if (isOwner) {
      return true
    }

    const isCollaboratorWithEditRights = note.collaborators.some(
      (collaborator) =>
        collaborator.userId === authStore.user?.id && (collaborator.role === Role.Admin || collaborator.role === Role.Editor)
    )

    return isCollaboratorWithEditRights
  }

  /**
   * Handles note updates received via SignalR.
   */
  function handleNoteUpdated(updatedNote: NoteResponseDto) {
    const index = notes.value.findIndex(n => n.id === updatedNote.id)
    if (index !== -1) {
      notes.value[index] = updatedNote
    } else {
      notes.value.unshift(updatedNote)
    }
  }

  /**
   * Handles archiving of a note received via SignalR.
   */
  function handleNoteArchived(noteId: string) {
    const index = notes.value.findIndex(n => n.id === noteId)
    if (index !== -1) {
      notes.value.splice(index, 1)
    }
  }

  /**
   * Handles the restoration of a note received via SignalR.
   */
  function handleNoteRestored(restoredNote: NoteResponseDto) {
    const index = notes.value.findIndex(n => n.id === restoredNote.id)
    if (index !== -1) {
      notes.value[index] = restoredNote
    } else {
      notes.value.unshift(restoredNote)
    }
  }

  /**
   * Handles the deletion of a note received via SignalR.
   */
  function handleNoteDeleted(noteId: string) {
    const index = notes.value.findIndex(n => n.id === noteId)
    if (index !== -1) {
      notes.value.splice(index, 1)
    }
  }

  /**
   * Fetches all notes and populates the state.
   */
  async function fetchNotes() {
    loading.value = true
    error.value = null
    try {
      const data = await notesService.fetchAllNotes()
      notes.value = data
    } catch (err: unknown) {
      error.value = err instanceof Error ? err.message : 'An unknown error occurred.'
    } finally {
      loading.value = false
    }
  }

  /**
   * Fetches a single note by ID.
   */
  async function fetchNoteById(id: string) {
    loading.value = true
    error.value = null
    try {
      return await notesService.fetchNoteById(id)
    } catch (err: unknown) {
      error.value = err instanceof Error ? err.message : 'An unknown error occurred.'
      return null
    } finally {
      loading.value = false
    }
  }

  /**
   * Creates a new note and adds it to the state.
   */
  async function createNote(payload: NoteRequestDto) {
    loading.value = true
    error.value = null
    try {
      const created = await notesService.createNote(payload)
      if (created) {
        notes.value.unshift(created)
        return created
      } else {
        throw new Error('Failed to create note: no response received.')
      }
    } catch (err: unknown) {
      error.value = err instanceof Error ? err.message : 'An unknown error occurred.'
      throw err
    } finally {
      loading.value = false
    }
  }

  /**
   * Updates a note in the state.
   */
  async function updateNote(id: string, payload: NoteRequestDto) {
    loading.value = true
    error.value = null
    try {
      const updated = await notesService.updateNote(id, payload)
      if (!updated) {
        throw new Error('Failed to update note: no response received.')
      }
      const index = notes.value.findIndex(note => note.id === updated.id)
      if (index !== -1) {
        notes.value[index] = updated
      }
    } catch (err: unknown) {
      error.value = err instanceof Error ? err.message : 'An unknown error occurred.'
    } finally {
      loading.value = false
    }
  }

  /**
   * Archives a note and removes it from the state.
   */
  async function archiveNote(id: string) {
    loading.value = true
    error.value = null
    try {
      await notesService.archiveNote(id)
      notes.value = notes.value.filter(note => note.id !== id)
    } catch (err: unknown) {
      error.value = err instanceof Error ? err.message : 'An unknown error occurred.'
    } finally {
      loading.value = false
    }
  }

  /**
   * Restores an archived note and adds it to the state.
   */
  async function restoreNote(id: string) {
    loading.value = true
    error.value = null
    try {
      const restoredNote = await notesService.restoreNote(id)
      if (!restoredNote) {
        throw new Error('Failed to restore note: no response received.')
      }
      const index = notes.value.findIndex(note => note.id === restoredNote.id)
      if (index !== -1) {
        notes.value[index] = restoredNote
      } else {
        notes.value.push(restoredNote)
      }
    } catch (err: unknown) {
      error.value = err instanceof Error ? err.message : 'An unknown error occurred.'
    } finally {
      loading.value = false
    }
  }

  /**
   * Permanently deletes a note and removes it from the state.
   */
  async function deleteNotePermanently(id: string) {
    loading.value = true
    error.value = null
    try {
      await notesService.deleteNotePermanently(id)
      notes.value = notes.value.filter(note => note.id !== id)
    } catch (err: unknown) {
      error.value = err instanceof Error ? err.message : 'An unknown error occurred.'
    } finally {
      loading.value = false
    }
  }

  /**
   * Adds a collaborator to a note.
   */
  async function addCollaborator(noteId: string, payload: AddCollaboratorRequestDto) {
    loading.value = true
    error.value = null
    try {
      const newCollaborator = await notesService.addCollaborator(noteId, payload)
      if (!newCollaborator) {
        throw new Error('Failed to add collaborator: no response received.')
      }
      const note = notes.value.find(n => n.id === noteId)
      if (note) {
        note.collaborators.push(newCollaborator) 
      }
    } catch (err: unknown) {
      error.value = err instanceof Error ? err.message : 'An unknown error occurred.'
    } finally {
      loading.value = false
    }
  }

  /**
   * Updates a collaborator's role.
   */
  async function updateCollaboratorRole(noteId: string, collaboratorUserId: string, payload: UpdateCollaboratorRoleRequestDto) {
    loading.value = true
    error.value = null
    try {
      const updatedCollaborator = await notesService.updateCollaboratorRole(noteId, collaboratorUserId, payload)
      if (!updatedCollaborator) {
        throw new Error('Failed to update collaborator: no response received.')
      }
      const note = notes.value.find(n => n.id === noteId)
      if (note) {
        const collaboratorIndex = note.collaborators.findIndex(c => c.userId === updatedCollaborator.userId)
        if (collaboratorIndex !== -1) {
          note.collaborators[collaboratorIndex] = updatedCollaborator
        }
      }
    } catch (err: unknown) {
      error.value = err instanceof Error ? err.message : 'An unknown error occurred.'
    } finally {
      loading.value = false
    }
  }

  /**
   * Removes a collaborator.
   */
  async function removeCollaborator(noteId: string, collaboratorUserId: string) {
    loading.value = true
    error.value = null
    try {
      await notesService.removeCollaborator(noteId, collaboratorUserId)
      const note = notes.value.find(n => n.id === noteId)
      if (note) {
        note.collaborators = note.collaborators.filter(c => c.userId !== collaboratorUserId)
      }
    } catch (err: unknown) {
      error.value = err instanceof Error ? err.message : 'An unknown error occurred.'
    } finally {
      loading.value = false
    }
  }

  /**
   * Fetches all collaborators for a note.
   */
  async function fetchCollaborators(noteId: string) {
    loading.value = true
    error.value = null
    try {
      return await notesService.fetchCollaborators(noteId)
    } catch (err: unknown) {
      error.value = err instanceof Error ? err.message : 'An unknown error occurred.'
      return null
    } finally {
      loading.value = false
    }
  }

  return {
    notes,
    loading,
    error,
    hasNotes,
    hasArchivedNotes,
    hasReminderNotes,
    sharedNotes,
    sortedNotes,
    archivedNotes,
    nonArchivedNotes,
    reminderNotes,
    searchQuery,
    filteredSortedNotes,
    filteredArchivedNotes,
    filteredReminderNotes,
    filteredSharedNotes,
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
    fetchCollaborators,
    hasEditPermissions,
    handleNoteUpdated,
    handleNoteArchived,
    handleNoteRestored,
    handleNoteDeleted,
  }
})
