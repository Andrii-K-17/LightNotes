<script setup lang="ts">
import Header from './Header.vue'
import Footer from './Footer.vue'
import { onMounted, onUnmounted, ref } from 'vue'
import { useNotesStore } from '../../entities/note/model/store/notes'

const notesStore = useNotesStore()

const timer = ref<number | null>(null)

const checkReminders = () => {
  const now = new Date()
  notesStore.reminderNotes.forEach(async note => {
    if (note.reminderAt && new Date(note.reminderAt) <= now) {
      alert(`Reminder: ${note.title} - ${note.content}`)
      await notesStore.updateNote(note.id, {
        ...note,
        reminderAt: null
      })
    }
  }) 
}

onMounted(async () => {
  await notesStore.fetchNotes()
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
    <main class="flex-1 container mx-auto pl-4 pr-4">
      <slot />
    </main>
    <Footer />
  </div>
</template>
