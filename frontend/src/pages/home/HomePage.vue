<script setup lang="ts">
import NoteCard from '../../entities/note/ui/NoteCard.vue'
import Sidebar from '../../shared/ui/Sidebar.vue'
import { onMounted } from 'vue'
import { useNotesStore } from '../../entities/note/model/store/notes'
import { useUiStore } from '../../app/store/uiStore'
import BaseButton from '../../shared/ui/BaseButton.vue'
import router from '../../app/router'

const notesStore = useNotesStore()
const uiStore = useUiStore()

onMounted(() => {
  notesStore.fetchNotes()
})

const createNoteAndRedirect = async () => {
  const newNote = await notesStore.createNote({
    title: 'New note',
    content: 'empty',
    isPinned: false,
    isArchived: false,
    tags: []
  })
  
  if (newNote) {
    router.push({ name: 'note-details', params: { id: newNote.id } })
  }
}
</script>

<template>
  <div class="flex">
    <Sidebar />
    
    <div class="flex-1 flex-col p-2" :class="{'hidden': uiStore.isSidebarOpen}">
      <BaseButton
        @click="createNoteAndRedirect"
        :primary="true"
        :loading="notesStore.loading"
        class="h-10 w-full"
      >
        Create a note
      </BaseButton>

      <main
        class="pb-2 pt-2"
      >
        <div v-if="notesStore.loading">
          <p>Loading notes...</p>
        </div>
      
        <div v-else-if="!notesStore.hasNotes">
          <p>You don't have any notes yet.</p>
          <p>Create your first note.</p>
        </div>
      
        <div v-else class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-2">
          <NoteCard 
            v-for="note in notesStore.sortedNotes" 
            :key="note.id" 
            :note="note" 
          />
        </div>
      
        <div v-if="notesStore.error">
          <p class="text-red-500">Error: {{ notesStore.error }}</p>
        </div>
      </main>
    </div>
  </div>
</template>
