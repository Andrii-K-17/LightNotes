<script setup lang="ts">
import { ref, watch } from 'vue'
import { useNotesStore } from '../../entities/note/model/store/notes'
import { debounce } from 'lodash'
import { useUiStore } from '../../app/store/uiStore'

const searchIcon = '/src/assets/images/searchPanel/searchIcon.svg#search'

const notesStore = useNotesStore()
const uiStore = useUiStore()

const searchQuery = ref(notesStore.searchQuery)

const debouncedUpdateSearchQuery = debounce((value: string) => {
  notesStore.searchQuery = value
}, 300)

watch(searchQuery, (newValue) => {
  debouncedUpdateSearchQuery(newValue)
})
</script>

<template>
  <div
    v-if="uiStore.isSearchPanelOpen"
    class="flex items-center px-3 py-0 w-full rounded-lg border border-neutral-900 dark:border-neutral-400 dark:caret-neutral-400 dark:text-neutral-300 hover:border-sky-400"
  >
    <svg class="m-0 p-0 w-5 h-5 text-black dark:text-white">
      <use :href="searchIcon"></use>
    </svg>
    <input
      v-model="searchQuery"
      type="text"
      placeholder="Search notes..."
      class="w-full m-0 pl-2 rounded-lg border-none outline-none border-neutral-900 px-3 py-2 dark:border-neutral-400 dark:caret-neutral-400 dark:text-neutral-300 hover:border-sky-400"
    />
  </div>
</template>
