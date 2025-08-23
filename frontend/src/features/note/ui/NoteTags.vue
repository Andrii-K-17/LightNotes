<script setup lang="ts">
import { computed, ref } from 'vue'
import type { NoteResponseDto } from '../../../entities/note/model/types'
import BaseButton from '../../../shared/ui/BaseButton.vue'
import { useNotesStore } from '../../../entities/note/model/store/notes'

const props = defineProps<{ note: NoteResponseDto }>()
const emit = defineEmits(['update:note'])

const notesStore = useNotesStore()

const newTagInput = ref('')

const addTag = () => {
  const tagName = newTagInput.value.trim()
  if (tagName && !props.note.tags.some(t => t.tag === tagName)) {
    const updatedNote = {
      ...props.note,
      tags: [...props.note.tags, { tag: tagName }]
    }

    emit('update:note', updatedNote)
    newTagInput.value = ''
  }
}

const removeTag = (tagName: string) => {
  const updatedNote = {
    ...props.note,
    tags: [...props.note.tags].filter(t => t.tag !== tagName)
  }

  emit('update:note', updatedNote)
}

const showTagsInput = ref(false)

const toggleTagsInput = () => {
  showTagsInput.value = !showTagsInput.value
}

const canEdit = computed(() => {
  return notesStore.hasEditPermissions(props.note)
})
</script>

<template>
  <div class="flex flex-col gap-2">
    <div class="flex flex-wrap gap-2">
      <span
        v-for="tag in note.tags"
        :key="tag.tag"
        class="px-2 py-1 bg-gray-200 dark:bg-neutral-700 dark:text-neutral-100 rounded-lg text-sm flex items-center group cursor-pointer"
      >
        #{{ tag.tag }}
        <button
          v-if="canEdit"
          @click="removeTag(tag.tag)"
          class="w-4 h-4 ml-1 flex items-center justify-center rounded-full text-sm cursor-pointer text-neutral-900 dark:text-neutral-100 border border-gray-300 dark:border-gray-600 hover:text-red-800 dark:hover:text-red-400 hover:scale-110 transition-transform"
        >
          &times
        </button>
      </span>
      <button
        v-if="!showTagsInput && canEdit"
        @click="toggleTagsInput"
        class="px-2 py-1 bg-sky-200 dark:bg-sky-800 dark:text-neutral-100 rounded-lg text-sm flex items-center group cursor-pointer hover:scale-105 transition-transform"
      >
        Add a tag
      </button>
    </div>

    <div
      v-if="showTagsInput"
      class="flex flex-row gap-2 items-center"
    >
      <input
        v-model="newTagInput"
        @keyup.enter="addTag"
        class="w-full border rounded-lg px-2 py-1 border-neutral-300 dark:border-neutral-600 dark:text-neutral-100"
        placeholder="Add a tag..."
      />
      <BaseButton
        @click="addTag"
        :primary="true"
        class="h-8"
      >
        Add
      </BaseButton>
    </div>
  </div>
</template>
