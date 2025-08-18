<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import lightThemeIcon from '../../assets/images/theme/lightTheme.svg'
import darkThemeIcon from '../../assets/images/theme/darkTheme.svg'
import { useAuthStore } from '../../entities/session/model/store/auth'
import { useUiStore } from '../../app/store/uiStore'

const menuIcon = '/src/assets/images/sidebar/menuIcon.svg'

const isDark = ref(false)
const authStore = useAuthStore()
const isLoggedIn = computed(() => authStore.isAuthenticated)
const userName = computed(() => authStore.user?.name)
const uiStore = useUiStore()

const applyTheme = (isDarkTheme: boolean) => {
  if (isDarkTheme) {
    document.documentElement.classList.add('dark')
    localStorage.setItem('theme', 'dark')
  } else {
    document.documentElement.classList.remove('dark')
    localStorage.removeItem('theme')
  }
}

onMounted(() => {
  const savedTheme = localStorage.getItem('theme')
  isDark.value = savedTheme === 'dark'
  applyTheme(isDark.value)
})

const toggleTheme = () => {
  isDark.value = !isDark.value
  applyTheme(isDark.value)
}

const themeIconSrc = computed(() => {
  return isDark.value ? darkThemeIcon : lightThemeIcon
})
</script>

<template>
  <header class="bg-neutral-100 text-black p-4 border-b-1 dark:bg-neutral-800 dark:text-white transition duration-500 border-gray-200 dark:border-black flex justify-between items-center">
    <button
      @click="uiStore.toggleSidebar"
      class="md:hidden p-2 absolute top-3 left-3 z-50"
    >
      <svg class="w-6 h-6 text-black dark:text-white">
        <use :href="`${menuIcon}#menu-icon`"></use>
      </svg>
    </button>
    
    <router-link
      to="/home"
      class="flex items-center gap-2"
    >
      <img src="../../assets/images/LightNotes.png" alt="Logo" class="h-8 ml-11 md:ml-0 transition-transform duration-300" />
      <span class="font-semibold text-lg">LightNotes</span>
    </router-link>
    
    <div class="flex justify-end gap-7">
      <button
        @click="toggleTheme"
        class="flex items-center justify-center rounded-full"
      >
        <img
          :src="themeIconSrc"
          alt="Theme toggle"
          class="w-5 h-5"
        />
      </button>

      <div v-if="isLoggedIn">
        <span class="font-medium text-gray-800 dark:text-gray-200 transition-colors duration-500">
          Hello, {{ userName }}
        </span>
      </div>
      <div v-else>
        <router-link
          to="/login"
          class="px-5 py-2 rounded-lg font-medium transition duration-500  bg-sky-500 text-white hover:bg-sky-600"
        >
          Sing in
        </router-link>
      </div>
    </div>
  </header>
</template>
