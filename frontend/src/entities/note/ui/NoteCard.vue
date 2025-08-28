<script setup lang="ts">
import { computed, ref } from 'vue'
import { formatDate } from '../../../shared/lib/helpers'
import type { NoteResponseDto } from '../../note/model/types'
import router from '../../../app/router'
import { useRoute } from 'vue-router'
import { useUiStore } from '../../../app/store/uiStore'
import Message from '../../../shared/ui/Message.vue'
import { useAuthStore } from '../../session/model/store/auth'

const pinIcon = '/images/noteCard/pinIcon.svg'
const archiveIcon = '/images/noteCard/archiveIcon.svg'
const trashIcon = '/images/noteCard/trashIcon.svg'
const restoreIcon = '/images/noteCard/restoreIcon.svg'

const props = defineProps<{
  note: NoteResponseDto
}>()

const emit = defineEmits(['pin', 'archive', 'delete', 'restore'])

const route = useRoute()

const isActive = (path: string) => route.path === path

const messageText = ref('')

const showMessage = (message: string, time: number = 3000) => {
   messageText.value = message
  setTimeout(() => {
    messageText.value = '' 
  }, time)
} 

const handleNoteAction = (action: 'pin' | 'archive' | 'delete' | 'restore', event: Event) => {
  event.stopPropagation()
  emit(action, props.note.id)
}

const authStore = useAuthStore()

const uiStore = useUiStore()

const colors: { [key: string]: string } = {
  '#e3f2fd': '#13527e',
  '#f3e5f5': '#57225f',
  '#fce4ec': '#6b2a40',
  '#e8f5e9': '#1a5b1f',
  '#fffde7': '#69631b',
}

const cardStyle = computed(() => {
  if (props.note.color) {
    if (uiStore.isDark && props.note.color in colors) {
      return { backgroundColor: colors[props.note.color] } 
    } else {
      return { backgroundColor: props.note.color } 
    }
  }
  return undefined
})
</script>

<template>
  <Message v-if="messageText" :message="messageText" />
  <div
    class="border border-neutral-400 dark:border-neutral-100 hover:border-sky-600 rounded-lg p-4 space-y-2 transition-all duration-300 cursor-pointer flex flex-col justify-between"
    :class="{ 
      'bg-neutral-100 dark:bg-neutral-800': !note.color
    }"
    :style="note.color ? cardStyle : {}"
    @click="isActive('/trash') ? showMessage('Notes in the trash cannot be edited.') : router.push(`/note/${note.id}`)"
  >
    <div class="flex flex-col">
      <div
        v-if="note.collaborators.length > 0 && authStore.user?.id === note.ownerId"
        class="h-5 w-fit mr-1 mb-1 pl-2 pr-2 flex items-center justify-center rounded text-sm text-neutral-900 dark:text-neutral-100 border border-gray-400 dark:border-gray-500"
      >
        Owner
      </div>

      <h2 class="text-lg font-semibold mb-2 line-clamp-1 dark:text-neutral-50">
        {{ props.note.title }}
      </h2>

      <p
        class="text-gray-700 text-sm mb-4 line-clamp-4 dark:text-gray-300"
        v-html="props.note.content"
      >
      </p>

      <div
        v-if="note.tags && note.tags.length"
        class="flex flex-wrap gap-2 mt-2"
      >
        <span
          v-for="(tag, index) in note.tags"
          :key="index"
          class="bg-gray-200 font-mono font-semibold text-gray-700 text-xs px-2 py-1 rounded-lg"
        >
          #{{ tag.tag }}
        </span>
      </div>
    </div>

    <div class="flex justify-between items-center text-xs text-gray-600 dark:text-gray-400 mt-auto">
      <div class="flex flex-col items-start">
        <div
          v-if="isActive('/reminders')"
          class="font-bold text-base"
        >
          {{ 
            `Reminder at: ${note.reminderAt ? new Date(note.reminderAt).toLocaleDateString() : null} ${note.reminderAt ? new Date(note.reminderAt).toLocaleTimeString() : null}`
          }}
        </div>

        <div>{{ formatDate(note.createdAt) }}</div>
      </div>
      
      <div class="flex gap-2">
        <div v-if="!isActive('/trash')">
          <button
            @click="(event) => handleNoteAction('pin', event)"
            class="hover:text-sky-600"
            :title="`${note.isPinned ? 'Unpin the note' : 'Pin the note'}`"
          >
            <svg class="w-5 h-5 text-black dark:text-white fill-none hover:fill-blue-500">
              <use :href="`${pinIcon}#${note.isPinned ? 'unpin' : 'pin'}`"></use>
            </svg>
          </button>
          <button
            @click="(event) => handleNoteAction('archive', event)"
            class="hover:text-sky-600"
            title="Archive the note"
          >
            <svg class="w-5 h-5 text-black dark:text-white fill-none hover:fill-blue-500">
              <use :href="`${archiveIcon}#archive`"></use>
            </svg>
          </button>
        </div>

        <div v-else-if="isActive('/trash')">
          <button
            @click="(event) => handleNoteAction('restore', event)"
            class="hover:text-sky-600"
            title="Restore the note"
          >
            <svg class="w-4 h-4 text-black dark:text-white fill-none hover:fill-blue-500">
              <use :href="`${restoreIcon}#restore`"></use>
            </svg>
          </button>
          <button
            @click="(event) => handleNoteAction('delete', event)"
            class="hover:text-red-600"
            title="Delete the note"
          >
            <svg class="w-5 h-5 text-black dark:text-white fill-none hover:fill-red-500">
              <use :href="`${trashIcon}#trash`"></use>
            </svg>
          </button>
        </div>
      </div>
    </div>
  </div>
</template>
