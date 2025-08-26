<script setup lang="ts">
import Header from './Header.vue'
import Footer from './Footer.vue'
import { onMounted, onUnmounted, ref } from 'vue'
import { useNotesStore } from '../../entities/note/model/store/notes'
import { truncateText } from '../lib/helpers'
import { useAuthStore } from '../../entities/session/model/store/auth'

const notesStore = useNotesStore()
const authStore = useAuthStore()

const timer = ref<number | null>(null)

const checkReminders = () => {
  const now = new Date()
  notesStore.reminderNotes.forEach(async note => {
    if (note.reminderAt && new Date(note.reminderAt) <= now) {
      alert(`Reminder: ${truncateText(note.title, 50)} - ${truncateText(note.content, 200)}`)
      await notesStore.updateNote(note.id, {
        ...note,
        reminderAt: null
      })
    }
  }) 
}

onMounted(async () => {
  if (authStore.isAuthenticated) {
    await notesStore.fetchNotes()
  }
  timer.value = setInterval(checkReminders, 50000) 
  checkReminders()
})

onUnmounted(() => {
  if (timer.value) {
    clearInterval(timer.value) 
  }
}) 
</script>

<template>
  <div class="min-h-screen flex flex-col">
    <Header />
    <main class="flex-1 justify-center pl-4 pr-4">
      <slot />
    </main>
    <Footer />
  </div>
</template>
