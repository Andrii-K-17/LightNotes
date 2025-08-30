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

const currentNote = computed<NoteResponseDto | null>(() => {
  const noteId = route.path.split('note/')[1]
  return notesStore.notes.find(n => n.id === noteId) || null
})

const isExpandedEditor = ref<boolean>(false)

const canEdit = computed(() => {
  if (!currentNote.value) {
    return false
  }
  return notesStore.hasEditPermissions(currentNote.value)
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
 * Sets the initial content from the computed note or the prop.
 */
onMounted(() => {
  if (editorRef.value) {
    editorRef.value.innerHTML = props.modelValue || currentNote.value?.content || ''
  }
})

/**
 * Watches for changes in the note's content from the Pinia store
 * and synchronizes it with the editor's content.
 */
watch(() => currentNote.value?.content, (newValue) => {
  if (editorRef.value && newValue !== editorRef.value.innerHTML) {
    editorRef.value.innerHTML = newValue || ''
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

const expandEditor = () => {
  isExpandedEditor.value = !isExpandedEditor.value
  isExpandedEditor.value ? document.body.classList.add('overflow-hidden') : document.body.classList.remove('overflow-hidden')
}
</script>

<template>
  <div
    class="border rounded-lg overflow-hidden transition-all duration-500 border-neutral-300 dark:border-neutral-600"
    :class="{'fixed md:w-[90%] w-full md:h-[85%] shadow-[0px_0px_50px_0px_rgb(0,_65,_110)] h-full top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 z-50 bg-neutral-100 dark:bg-neutral-800 flex flex-col': isExpandedEditor}"
  >
    <div class="flex flex-row items-center gap-2 p-2 border-b border-neutral-300 dark:border-neutral-600 bg-neutral-100 dark:bg-neutral-800">
      <div class="flex-grow"></div>
      <div v-if="canEdit" class="flex flex-wrap items-center gap-2">
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
      <div class="flex-grow"></div>
      <button
        @click="expandEditor"
        title="Expand"
        class="h-5 w-5 pl-1 pr-1 flex items-center justify-center rounded-lg text-sm cursor-pointer text-neutral-900 dark:text-neutral-100 border border-gray-400 dark:border-gray-500 hover:scale-110 transition-transform">
        ⛶
      </button>
    </div>

    <div
      ref="editorRef"
      class="min-h-55 p-3 overflow-y-scroll outline-none dark:text-neutral-100"
      :class="{'h-55': !isExpandedEditor}"
      :contenteditable="canEdit"
      @input="handleInput"
    ></div>
  </div>
</template>
