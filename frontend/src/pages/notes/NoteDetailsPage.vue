<script setup lang="ts">
import { ref, onMounted, watch, computed } from 'vue'
import { useRoute } from 'vue-router'
import { useNotesStore } from '../../entities/note/model/store/notes'
import type { NoteResponseDto } from '../../entities/note/model/types'
import TextEditor from '../../features/note/ui/TextEditor.vue'
import NoteChat from '../../features/note/ui/NoteChat.vue'
import Sidebar from '../../shared/ui/Sidebar.vue'
import Spinner from '../../shared/ui/Spinner.vue'
import { useUiStore } from '../../app/store/uiStore'
import { debounce } from 'lodash'
import NoteTags from '../../features/note/ui/NoteTags.vue'
import NoteCollaborators from '../../features/note/ui/NoteCollaborators.vue'

const route = useRoute()
const notesStore = useNotesStore()
const uiStore = useUiStore()

const note = ref<NoteResponseDto | null>(null)
const loading = ref(true)
const error = ref<string | null>(null)
const noteIsUpdating = ref(false)

const isColorPickerOpen = ref(false)
const isReminderPickerOpen = ref(false)

const colors = ref(['#e3f2fd', '#f3e5f5', '#fce4ec', '#e8f5e9', '#fffde7'])

onMounted(async () => {
  const noteId = route.params.id as string
  note.value = await notesStore.fetchNoteById(noteId)
  loading.value = false
})

/**
 * A debounced function that updates a note on the server.
 */
const debouncedUpdateNote = debounce(async (updatedNote: NoteResponseDto) => {
  if (updatedNote && updatedNote.id) {
    noteIsUpdating.value = true
    try {
      await notesStore.updateNote(updatedNote.id, updatedNote)
    } finally {
      noteIsUpdating.value = false
    }
  }
}, 500)

watch(note, (newNote) => {
  if (newNote) {
    debouncedUpdateNote(newNote)
  }
}, { deep: true })

const toggleColorPicker = () => {
  isColorPickerOpen.value = !isColorPickerOpen.value
  isReminderPickerOpen.value = false
}

const toggleReminderPicker = () => {
  isReminderPickerOpen.value = !isReminderPickerOpen.value
  isColorPickerOpen.value = false
}

const selectColor = (color: string) => {
  if (note.value) {
    note.value.color = color
  }
  isColorPickerOpen.value = false
}

const resetBgColor = () => {
  if (note.value) {
    note.value.color = null
  }
  isColorPickerOpen.value = false
}

const resetReminder = () => {
  if (note.value) {
    note.value.reminderAt = null
  }
  isReminderPickerOpen.value = false
}

const canEdit = computed(() => {
  if (!note.value) {
    return false
  }
  return notesStore.hasEditPermissions(note.value)
})
</script>

<template>
  <div class="flex justify-stretch">
    <Sidebar />
    
    <div
      class="flex-1 flex w-full justify-center p-4"
      :class="{'hidden': uiStore.isSidebarOpen}"
    >
      <div class="w-full">
        <div
          v-if="loading"
          class="text-center text-gray-500 dark:text-gray-400"
        >
          <p>Loading the note...</p>
          <Spinner />
        </div>
        
        <div
          v-else-if="error"
          class="text-center text-red-500"
        >
          <p>Error: {{ error }}</p>
        </div>

        <div v-else-if="note">
          <div 
            class="rounded-lg transition-colors duration-200"
            :class="{ backgroundColor: note.color }"
          >
            <div class="flex items-center justify-between mb-2">
              <input 
                v-model="note.title"
                placeholder="Note title" 
                :disabled="!canEdit"
                class="w-full text-2xl font-bold bg-transparent border-none outline-none focus:ring-0 dark:text-white"
              />
              
              <div
                v-if="canEdit"
                class="flex space-x-2">
                <div class="relative">
                  <button 
                    class="p-2 rounded-full hover:bg-gray-200 dark:hover:bg-gray-700 transition-colors"
                    @click="toggleColorPicker"
                  >
                    🎨
                  </button>
                  <div 
                    class="absolute flex flex-col items-center justify-center right-0 mt-2 w-26 bg-white dark:bg-gray-800 rounded-md shadow-lg p-2 z-10"
                    v-if="isColorPickerOpen"
                  >
                    <div class="flex flex-wrap gap-2">
                      <div 
                        v-for="color in colors" 
                        :key="color" 
                        @click="selectColor(color)" 
                        :style="{ backgroundColor: color }" 
                        class="w-6 h-6 rounded-full cursor-pointer border border-gray-300 dark:border-gray-600 hover:scale-110 transition-transform"
                      ></div>
                      <div
                        @click="resetBgColor"
                        class="w-6 h-6 flex items-center justify-center rounded-full text-sm cursor-pointer text-neutral-900 dark:text-neutral-100 border border-gray-300 dark:border-gray-600 hover:scale-110 transition-transform"
                        >✕</div>
                    </div>
                  </div>
                </div>
                <div class="relative">
                  <button 
                    class="p-2 rounded-full hover:bg-gray-200 dark:hover:bg-gray-700 transition-colors"
                    @click="toggleReminderPicker"
                  >
                    ⏰
                  </button>
                  <div
                    class="absolute flex flex-col items-center justify-center right-0 mt-2 w-58 bg-white dark:bg-gray-800 rounded-md shadow-lg p-4 z-10"
                    v-if="isReminderPickerOpen"
                  >
                    <div
                      for="reminder-datetime"
                      class="block text-sm font-medium text-gray-700 dark:text-gray-200 mb-2">
                      Set a reminder:
                    </div>
                    <input
                      type="datetime-local"
                      id="reminder-datetime"
                      v-model="note.reminderAt"
                      class="w-full rounded-md flex items-center justify-center border-gray-300 dark:border-gray-600 bg-gray-50 dark:bg-gray-700 text-gray-900 dark:text-gray-100 focus:border-indigo-500 focus:ring-indigo-500"
                    />
                    <div
                      @click="resetReminder"
                      class="w-full h-6 mt-2 flex items-center justify-center rounded-lg text-sm cursor-pointer text-neutral-900 dark:text-neutral-100 border border-gray-300 dark:border-gray-600 hover:scale-110 transition-transform"
                      title="Reset the reminder"
                      >Reset a reminder</div>
                  </div>
                </div>
              </div>
            </div>
            
            <TextEditor v-model="note.content" />
          </div>
          
          <div class="mt-2 flex flex-col">
            <NoteTags v-model:note="note" />
          </div>

          <div class="mt-6 flex flex-col lg:flex-row gap-y-5">
            <div class="lg:w-4/5 lg:mr-2">
              <NoteChat :noteId="note.id" />
            </div>
            <div class="h-full">
              <NoteCollaborators :noteId="note.id" />
            </div>
          </div>
          
          <div
            v-if="noteIsUpdating"
            class="mt-2 text-right text-gray-400 dark:text-gray-500"
          >
            Saving...
          </div>
        </div>
        <div
          v-else
          class="text-center text-gray-500 dark:text-gray-400"
        >
          <p>Note not found</p>
        </div>
      </div>
    </div>
  </div>
</template>
