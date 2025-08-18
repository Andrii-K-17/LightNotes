<script setup lang="ts">
import NoteCard from '../../entities/note/ui/NoteCard.vue'
import Sidebar from '../../shared/ui/Sidebar.vue'
import { onMounted, ref } from 'vue'
import { useNotesStore } from '../../entities/note/model/store/notes'
import { useUiStore } from '../../app/store/uiStore'

const menuIcon = '/src/assets/images/sidebar/menuIcon.svg'

const notesStore = useNotesStore()
const uiStore = useUiStore()

onMounted(() => {
  notesStore.fetchNotes()
})

const isSidebarOpen = ref(false)
</script>

<template>
  <div class="md:flex">
    <Sidebar 
      class="h-full w-64 transition-transform duration-300 z-30 fixed left-0 md:static md:translate-x-0 pr-4 pl-4" 
      :class="{
        'translate-x-0 w-full': uiStore.isSidebarOpen,
        '-translate-x-full': !uiStore.isSidebarOpen,
      }"
    />
    
    <main
      class="flex-1 p-2"
      :class="{'hidden': isSidebarOpen}"
    >
      <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-2">
        <NoteCard 
          v-for="note in notesStore.notes" 
          :key="note.id" 
          :note="note" 
        />
      </div>
    </main>
  </div>
</template>
