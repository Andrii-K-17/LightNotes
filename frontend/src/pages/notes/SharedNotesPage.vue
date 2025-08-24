<script setup lang="ts">
import { onMounted } from 'vue'
import { useNotesStore } from '../../entities/note/model/store/notes'
import Sidebar from '../../shared/ui/Sidebar.vue'
import NoteCard from '../../entities/note/ui/NoteCard.vue'
import SearchPanel from '../../shared/ui/SearchPanel.vue'

const notesStore = useNotesStore()

onMounted(async () => {
  await notesStore.fetchNotes()
})
</script>

<template>
  <div class="flex">
    <Sidebar />

    <div class="flex-1 p-2">
      <div class="mb-2">
        <SearchPanel />
      </div>

      <div
        v-if="notesStore.loading"
        class="text-center dark:text-gray-400"
      >
        <p>Loading shared notes...</p>
      </div>
      <div
        v-else-if="notesStore.sharedNotes.length === 0"
        class="text-center dark:text-gray-400"
      >
        <p>You don't have any shared notes yet.</p>
      </div>
      <div
        v-else
        class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4"
      >
        <NoteCard
          v-for="note in notesStore.sharedNotes"
          :key="note.id"
          :note="note"
          @pin="notesStore.updateNote(note.id, {
            ...note,
            isPinned: !note.isPinned
          })"
          @archive="notesStore.archiveNote(note.id)"
        />
      </div>
      <div
        v-if="notesStore.error"
        class="text-red-500 mt-4"
      >
        <p>Error: {{ notesStore.error }}</p>
      </div>
    </div>
  </div>
</template>
