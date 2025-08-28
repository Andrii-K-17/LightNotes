<script setup lang="ts">
import { useRoute } from 'vue-router'
import { useUiStore } from '../../app/store/uiStore'

const notesIcon = '/images/sidebar/notesIcon.svg'
const sharedIcon = '/images/sidebar/sharedIcon.svg'
const trashIcon = '/images/sidebar/trashIcon.svg'
const remindersIcon = '/images/sidebar/remindersIcon.svg'

const route = useRoute()
const uiStore = useUiStore()

const menuItems = [
  { name: 'Notes', icon: notesIcon, id: 'Notes', path: '/home' },
  { name: 'Shared notes', icon: sharedIcon, id: 'SharedNotes', path: '/shared-notes' },
  { name: 'Reminders', icon: remindersIcon, id: 'Reminders', path: '/reminders' },
  { name: 'Trash', icon: trashIcon, id: 'Trash', path: '/trash' },
]

const isActive = (path: string) => route.path === path
</script>

<template>
  <aside
    class="bg-[#f9f6f6] dark:bg-[#1f1f1f] p-2 transition-all duration-300 rounded-lg h-full w-64 z-30 md:sticky fixed md:top-17 left-0 md:translate-x-0 pr-4 pl-4"
    :class="{
      'translate-x-0 w-full': uiStore.isSidebarOpen,
      '-translate-x-full': !uiStore.isSidebarOpen,
    }"
  >
    <nav class="flex flex-col space-y-2">
      <router-link
        v-for="item in menuItems"
        :key="item.name"
        :to="item.path"
        @click="uiStore.closeSidebar"
        class="flex items-center gap-2 p-2 rounded-lg transition-colors duration-300"
        :class="{
          'bg-sky-200 dark:bg-sky-950 dark:text-neutral-100 border border-neutral-400 dark:border-neutral-300': isActive(item.path),
          'bg-gray-200 dark:bg-[#2b2b2b] dark:text-neutral-100 border border-neutral-100 dark:border-neutral-800 hover:border-sky-600': !isActive(item.path),
        }"
      >
        <svg class="w-7 h-7 text-black dark:text-white">
          <use :href="`${item.icon}#${item.id}`"></use>
        </svg>
        <span>{{ item.name }}</span>
      </router-link>
    </nav>
  </aside>
</template>
