<script setup lang="ts">
import { computed } from 'vue'
import { useUiStore } from '../../app/store/uiStore'

const lightThemeIcon = '/images/theme/lightTheme.svg'
const darkThemeIcon = '/images/theme/darkTheme.svg'

const uiStore = useUiStore()

const menuItems = [
  { name: 'Log in', action: 'Reminders', path: '/login' },
  { name: 'Sign up', action: 'SharedNotes', path: '/register' },
]

const themeIconSrc = computed(() => {
  return uiStore.isDark ? darkThemeIcon : lightThemeIcon
})
</script>

<template>
  <div
    class="bg-[#f9f6f6] dark:bg-[#1f1f1f] p-2 transition-all duration-300 h-full w-full z-30 md:hidden fixed top-17 md:translate-y-0 pr-4 pl-4"
    :class="{
      'translate-y-0': uiStore.isMobileMenuOpen,
      '-translate-y-full': !uiStore.isMobileMenuOpen,
    }"
  >
    <nav class="flex flex-col mt-1 space-y-3">
      <router-link
        v-for="item in menuItems"
        :key="item.name"
        :to="item.path"
        @click="uiStore.toggleMobileMenu"
        class="flex items-center p-2 border-b-1 transition-colors duration-300 dark:text-neutral-100 border-b-neutral-400 dark:border-b-neutral-300"
      >
        <span>{{ item.name }}</span>
      </router-link>
      <button
        @click="uiStore.toggleTheme"
        class="flex items-center justify-center mt-2 rounded-full cursor-pointer"
      >
        <img
          :src="themeIconSrc"
          alt="Theme toggle"
          class="w-5 h-5" 
        />
      </button>
    </nav>
  </div>
</template>
