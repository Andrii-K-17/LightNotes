<script setup lang="ts">
import { onMounted, computed} from 'vue'
import lightThemeIcon from '../../assets/images/theme/lightTheme.svg'
import darkThemeIcon from '../../assets/images/theme/darkTheme.svg'
import { useAuthStore } from '../../entities/session/model/store/auth'
import { useUiStore } from '../../app/store/uiStore'
import { useRoute } from 'vue-router'
import MobileMenu from './MobileMenu.vue'

const menuIcon = '/src/assets/images/sidebar/menuIcon.svg'
const searchIcon = '/src/assets/images/searchPanel/searchIcon.svg#search'
const userProfileIcon = '/src/assets/images/profile/userProfileIcon.svg#userProfile'

const authStore = useAuthStore()
const uiStore = useUiStore()

const isLoggedIn = computed(() => authStore.isAuthenticated)

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
  <MobileMenu />

  <header class="sticky top-0 h-17 w-full flex justify-between items-center z-50 bg-neutral-100 text-black p-4 border-b-1 dark:bg-neutral-800 dark:text-white transition duration-500 border-gray-200 dark:border-black">
    <button
      @click="uiStore.toggleSidebar"
      class="md:hidden p-2 absolute top-auto left-3 z-50 cursor-pointer"
      :class="{'hidden': !authStore.isAuthenticated}"
    >
      <svg v-if="!uiStore.isSidebarOpen" class="w-6 h-6 text-black dark:text-white">
        <use :href="`${menuIcon}#menu-icon`"></use>
      </svg>
      <p v-else class="text-black dark:text-white font-semibold text-2xl">✕</p>
    </button>

    <button
      v-if="!authStore.isAuthenticated"
      @click="uiStore.toggleMobileMenu"
      class="md:hidden p-2 absolute top-auto right-3 z-50 cursor-pointer"
    >
      <svg v-if="!uiStore.isMobileMenuOpen" class="w-6 h-6 text-black dark:text-white">
        <use :href="`${menuIcon}#menu-icon`"></use>
      </svg>
      <p v-else class="text-black dark:text-white font-semibold text-2xl">✕</p>
    </button>
    
    <div class="flex justify-center items-center gap-2 md:ml-0">
      <router-link
        class="flex flex-row justify-center items-center gap-2 md:ml-1"
        to="/"
        :class="{
          'ml-1': !authStore.isAuthenticated,
          'ml-15': authStore.isAuthenticated,
        }"
      >
        <img src="../../assets/images/LightNotes.png" alt="Logo" class="h-8 transition-transform duration-300" />
        <span class="font-bold font-sans text-lg">LightNotes</span>
      </router-link>
    </div>

    <div
      class="flex justify-end items-center gap-7 w-full"
      :class="{'md:flex hidden': !authStore.isAuthenticated}"
    >
      <button
        v-if="isActive('/home') || isActive('/shared-notes') || isActive('/trash') || isActive('/reminders')"
        @click="uiStore.toggleSearchPanel"
        class="flex items-center justify-center rounded-full cursor-pointer mr-1"
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
        <router-link
          to="/profile"
          class="flex justify-center items-center mr-1"
        >
          <svg class="w-8 h-8 text-black dark:text-white">
            <use :href="userProfileIcon"></use>
          </svg>
        </router-link>
      </div>
      <div v-else class="flex flex-row justify-center items-center gap-x-2">
        <router-link
          to="/register"
          class="px-0 py-1 rounded-lg font-medium text-sky-500 hover:text-sky-600"
        >
          Sign up
        </router-link>
        <p>or</p>
        <router-link
          to="/login"
          class="px-5 py-1 rounded-lg font-medium transition duration-500 bg-sky-500 text-white hover:bg-sky-600"
        >
          Log in
        </router-link>
      </div>
    </div>
  </header>
</template>
