<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue'
import { useNotesStore } from '../../../entities/note/model/store/notes'
import { useRoute } from 'vue-router'
import type { NoteResponseDto } from '../../../entities/note/model/types'

const props = defineProps<{ modelValue: string }>()
const emit = defineEmits(['update:modelValue'])

const editorRef = ref<HTMLElement | null>(null)

const notesStore = useNotesStore()
const route = useRoute()

const note = ref<NoteResponseDto | null>(null)

const noteId = route.path.split('note/')[1]

const canEdit = computed(() => {
  if (!note.value) {
    return false
  }
  return notesStore.hasEditPermissions(note.value)
})

/**
 * Emits the updated HTML content to the parent component.
 */
const handleInput = () => {
  if (editorRef.value) {
    emit('update:modelValue', editorRef.value.innerHTML)
  }
}

/**
 * Sets the initial content.
 */
onMounted(async () => {
  note.value = await notesStore.fetchNoteById(noteId)
  if (editorRef.value && props.modelValue) {
    editorRef.value.innerHTML = props.modelValue
  }
})

/**
 * Watches for changes in the modelValue prop to synchronize with the editor's content.
 */
watch(() => props.modelValue, (newValue) => {
  if (editorRef.value && newValue !== editorRef.value.innerHTML) {
    editorRef.value.innerHTML = newValue
  }
})

/**
 * Toggles an inline HTML tag on the selected text.
 */
const handleButtonClick = (tag: string) => {
  const selection = window.getSelection()
  if (!selection || selection.rangeCount === 0 || selection.isCollapsed) return

  const range = selection.getRangeAt(0)

  // Check if the selected text is already wrapped in the target tag
  const element = getParentTag(selection.anchorNode, tag)
  if (element) {
    const parent = element.parentNode
    if (!parent) return

    while (element.firstChild){
      parent.insertBefore(element.firstChild, element)
    }
    parent.removeChild(element)
  } else {
    const wrapper = document.createElement(tag)
    wrapper.appendChild(range.extractContents())
    range.insertNode(wrapper)
    range.selectNode(wrapper)
    selection.removeAllRanges()
    selection.addRange(range)
  }

  handleInput()
}

/**
 * Searches for the nearest parent HTML element with a specific tag name.
 */
const getParentTag = (node: Node | null, tag: string): HTMLElement | null => {
  while (node && node !== editorRef.value) {
    if (node instanceof HTMLElement && node.tagName.toLowerCase() === tag) {
      return node
    }
    node = node.parentNode
  }
  return null
}

const buttons = [
  { text: 'B', tag: 'strong', title: 'Bold' },
  { text: 'I', tag: 'em', title: 'Italic' },
  { text: 'U', tag: 'u', title: 'Underline' },
  { text: 'M', tag: 'mark', title: 'Mark text' },
]
</script>

<template>
  <div class="border rounded-lg overflow-hidden border-neutral-300 dark:border-neutral-600">
    <div v-if="canEdit" class="flex flex-wrap justify-center gap-2 p-2 border-b border-neutral-300 dark:border-neutral-600 bg-neutral-100 dark:bg-neutral-800">
      <button
        v-for="button in buttons"
        :key="button.tag"
        :title="button.title"
        @click="handleButtonClick(button.tag)"
        class="px-5 py-1 text-sm border font-bold font-mono rounded-md bg-white border-neutral-300 hover:bg-neutral-200 cursor-pointer dark:bg-neutral-900 dark:text-neutral-100 dark:border-neutral-700 dark:hover:bg-neutral-800"
        :class="{
          'italic': button.text === 'I',
          'underline': button.text === 'U',
        }"
        >
        {{ button.text }}
      </button>
    </div>

    <div
      ref="editorRef"
      class="min-h-56 p-3 outline-none dark:text-neutral-100"
      :contenteditable="canEdit"
      @input="handleInput"
    ></div>
  </div>
</template>
