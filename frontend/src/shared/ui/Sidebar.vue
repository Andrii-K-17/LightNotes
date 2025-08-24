<script setup lang="ts">
import { useRoute } from 'vue-router'
import BaseButton from './BaseButton.vue'
import { useAuthStore } from '../../entities/session/model/store/auth'
import router from '../../app/router'
import { useUiStore } from '../../app/store/uiStore'

const notesIcon = '/src/assets/images/sidebar/notesIcon.svg'
const sharedIcon = '/src/assets/images/sidebar/sharedIcon.svg'
const trashIcon = '/src/assets/images/sidebar/trashIcon.svg'
const remindersIcon = '/src/assets/images/sidebar/remindersIcon.svg'

const route = useRoute()
const authStore = useAuthStore()
const uiStore = useUiStore()

const menuItems = [
  { name: 'Notes', icon: notesIcon, id: 'Notes', path: '/home' },
  { name: 'Shared notes', icon: sharedIcon, id: 'SharedNotes', path: '/shared-notes' },
  { name: 'Reminders', icon: remindersIcon, id: 'Reminders', path: '/reminders' },
  { name: 'Trash', icon: trashIcon, id: 'Trash', path: '/trash' },
]

const isActive = (path: string) => route.path === path

const logoutAndCloseSidebar = () => {
  authStore.logout()
  uiStore.closeSidebar()
  router.push('/login')
}
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
      <hr class="mt-1 mb-3 text-neutral-300"/>
      <BaseButton
        @click="logoutAndCloseSidebar"
        :loading="authStore.loading"
        class="w-full h-10"
      >
        Log out
      </BaseButton>
    </nav>
  </aside>
</template>
