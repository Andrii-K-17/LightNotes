<script setup lang="ts">
import { computed } from 'vue'
import { formatDate } from '../../../shared/lib/helpers'
import type { NoteResponseDto } from '../../note/model/types'
import { truncateText } from '../../../shared/lib/helpers'
import router from '../../../app/router'
import { useRoute } from 'vue-router'
import { useUiStore } from '../../../app/store/uiStore'

const pinIcon = 'src/assets/images/noteCard/pinIcon.svg'
const archiveIcon = 'src/assets/images/noteCard/archiveIcon.svg'
const trashIcon = 'src/assets/images/noteCard/trashIcon.svg'
const restoreIcon = 'src/assets/images/noteCard/restoreIcon.svg'

const props = defineProps<{
  note: NoteResponseDto
}>()

const emit = defineEmits(['pin', 'archive', 'delete', 'restore'])

const route = useRoute()

const isActive = (path: string) => route.path === path

const truncatedContent = computed(() => {
  return truncateText(props.note.content, 200)
})

const handleNoteAction = (action: 'pin' | 'archive' | 'delete' | 'restore', event: Event) => {
  event.stopPropagation()
  emit(action, props.note.id)
}

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
  <div
    class="border border-neutral-400 dark:border-neutral-100 hover:border-sky-600 rounded-lg p-4 space-y-2 transition-all duration-300 cursor-pointer flex flex-col justify-between"
    :class="{ 
      'bg-neutral-100 dark:bg-neutral-800': !props.note.color
    }"
    :style="props.note.color ? cardStyle : {}"
    @click="router.push(`/note/${props.note.id}`)"
  >
  <div>
      <h2 class="text-lg font-semibold mb-2 line-clamp-2 dark:text-neutral-50">
        {{ note.title }}
      </h2>
      <p
        class="text-gray-700 text-sm mb-4 line-clamp-4 dark:text-gray-300"
        v-html="truncatedContent"
      >
      </p>

      <div v-if="note.tags && note.tags.length" class="flex flex-wrap gap-2 mt-2">
        <span
          v-for="(tag, index) in note.tags"
          :key="index"
          class="bg-gray-200 text-gray-700 text-xs px-2 py-1 rounded-full"
        >
          #{{ tag }}
        </span>
      </div>
    </div>

    <div class="flex justify-between items-center text-xs text-gray-600 dark:text-gray-400 mt-auto">
      <span>{{ formatDate(note.createdAt) }}</span>
      
      <div class="flex gap-2">
        <div v-if="isActive('/home') || isActive('/reminders')">
          <button
            @click="(event) => handleNoteAction('pin', event)"
            class="hover:text-sky-600"
            :title="`${props.note.isPinned ? 'Unpin the note' : 'Pin the note'}`"
          >
            <svg class="w-5 h-5 text-black dark:text-white fill-none hover:fill-blue-500">
              <use :href="`${pinIcon}#${props.note.isPinned ? 'unpin' : 'pin'}`"></use>
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
            <svg class="w-5 h-5 text-black dark:text-white fill-none hover:fill-blue-500">
              <use :href="`${trashIcon}#trash`"></use>
            </svg>
          </button>
        </div>
      </div>
    </div>
  </div>
</template>
