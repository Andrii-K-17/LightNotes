<script setup lang="ts">
import { computed } from 'vue'
import { useRoute } from 'vue-router'
import BaseButton from './BaseButton.vue'
import { useAuthStore } from '../../entities/session/model/store/auth'

const notesIcon = 'src/assets/images/sidebar/notesIcon.svg'
const archiveIcon = 'src/assets/images/sidebar/archiveIcon.svg'
const trashIcon = 'src/assets/images/sidebar/trashIcon.svg'
const remindersIcon = 'src/assets/images/sidebar/remindersIcon.svg'

const route = useRoute()
const authStore = useAuthStore()

const menuItems = [
  { name: 'Notes', icon: notesIcon, path: '/home' },
  { name: 'Archive', icon: archiveIcon, path: '/archive' },
  { name: 'Trash', icon: trashIcon, path: '/trash' },
  { name: 'Reminders', icon: remindersIcon, path: '/reminders' },
]

const isActive = (path: string) => computed(() => route.path === path)
</script>

<template>
  <aside class="bg-[#f9f6f6] dark:bg-[#1f1f1f] p-2 transition-colors duration-500 rounded-lg">
    <nav class="flex flex-col space-y-2">
      <router-link
        v-for="item in menuItems"
        :key="item.name"
        :to="item.path"
        class="flex items-center gap-2 p-2 rounded-lg transition-colors duration-300"
        :class="{
          'bg-sky-200 dark:bg-sky-950 dark:text-neutral-100 border border-neutral-400 dark:border-neutral-300': isActive(item.path).value,
          'bg-gray-200 dark:bg-[#2b2b2b] dark:text-neutral-100 border border-neutral-100 dark:border-neutral-800 hover:border-sky-600': !isActive(item.path).value,
        }"
      >
        <svg class="w-7 h-7 text-black dark:text-white">
          <use :href="`${item.icon}#${item.name}`"></use>
        </svg>
        <span>{{ item.name }}</span>
      </router-link>
      <hr class="mt-1 mb-3 text-neutral-300"/>
      <BaseButton
          @click="authStore.logout"
          :loading="authStore.loading"
          class="w-full h-10"
        >
          Log out
      </BaseButton>
    </nav>
  </aside>
</template>
