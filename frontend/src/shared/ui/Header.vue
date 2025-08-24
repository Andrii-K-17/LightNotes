<script setup lang="ts">
import { onMounted, computed } from 'vue'
import lightThemeIcon from '../../assets/images/theme/lightTheme.svg'
import darkThemeIcon from '../../assets/images/theme/darkTheme.svg'
import { useAuthStore } from '../../entities/session/model/store/auth'
import { useUiStore } from '../../app/store/uiStore'
import { useRoute } from 'vue-router'

const menuIcon = '/src/assets/images/sidebar/menuIcon.svg'
const searchIcon = '/src/assets/images/searchPanel/searchIcon.svg#search'

const authStore = useAuthStore()
const uiStore = useUiStore()

const isLoggedIn = computed(() => authStore.isAuthenticated)
const userName = computed(() => authStore.user?.name)

const themeIconSrc = computed(() => {
  return uiStore.isDark ? darkThemeIcon : lightThemeIcon
})

const route = useRoute()
const isActive = (path: string) => route.path === path

onMounted(() => {
  uiStore.initTheme()
})
</script>

<template>
  <header class="sticky top-0 h-17 w-full flex justify-between items-center z-50 bg-neutral-100 text-black p-4 border-b-1 dark:bg-neutral-800 dark:text-white transition duration-500 border-gray-200 dark:border-black">
    <button
      @click="uiStore.toggleSidebar"
      class="md:hidden p-2 absolute top-3 left-3 z-50 cursor-pointer"
      :class="{'hidden': !authStore.isAuthenticated}"
    >
      <svg class="w-6 h-6 text-black dark:text-white">
        <use :href="`${menuIcon}#menu-icon`"></use>
      </svg>
    </button>
    
    <div class="md:flex hidden items-center gap-2 md:ml-0">
      <router-link
        class="flex flex-row items-center gap-2 md:ml-0"
        to="/home"
        :class="{
          'ml-0': !authStore.isAuthenticated,
          'ml-11': authStore.isAuthenticated,
        }"
      >
        <img src="../../assets/images/LightNotes.png" alt="Logo" class="h-8 transition-transform duration-300" />
        <span class="font-semibold text-lg">LightNotes</span>
      </router-link>
    </div>

    <div class="flex justify-end items-end gap-7 w-full">
      <button
        v-if="isActive('/home') || isActive('/shared-notes') || isActive('/trash') || isActive('/reminders')"
        @click="uiStore.toggleSearchPanel"
        class="flex items-center justify-center rounded-full cursor-pointer"
      >
        <svg class="w-5 h-5 text-black dark:text-white">
          <use :href="searchIcon"></use>
        </svg>
      </button>

      <button
        @click="uiStore.toggleTheme"
        class="flex items-center justify-center rounded-full cursor-pointer"
      >
        <img
          :src="themeIconSrc"
          alt="Theme toggle"
          class="w-5 h-5" 
        />
      </button>

      <div v-if="isLoggedIn">
        <router-link to="/profile">
          <span class="font-medium text-gray-800 dark:text-gray-200 transition-colors duration-500">
            Hello, {{ userName }}
          </span>
        </router-link>
      </div>
      <div v-else>
        <router-link
          to="/login"
          class="px-5 py-2 rounded-lg font-medium transition duration-500 bg-sky-500 text-white hover:bg-sky-600"
        >
          Sing in
        </router-link>
      </div>
    </div>
  </header>
</template>
