import { defineStore } from 'pinia'
import { ref } from 'vue'

/**
 * Pinia store for managing UI state.
 */
export const useUiStore = defineStore('ui', () => {
  const isSidebarOpen = ref(false)
  const isDark = ref(false)
  const isSearchPanelOpen = ref(false)

  /**
   * Toggles the sidebar's open/closed state.
   */
  function toggleSidebar() {
    isSidebarOpen.value = !isSidebarOpen.value
    if (isSearchPanelOpen.value) toggleSearchPanel() 
  }

  /**
   * Closes the sidebar, regardless of its current state.
   */
  function closeSidebar() {
    isSidebarOpen.value = false
  }

  /**
   * Applies the selected theme by adding or removing the 'dark' class
   * from the root HTML element and updating localStorage.
   */
  function applyTheme(isDarkTheme: boolean) {
    if (isDarkTheme) {
      document.documentElement.classList.add('dark')
      localStorage.setItem('theme', 'dark')
    } else {
      document.documentElement.classList.remove('dark')
      localStorage.removeItem('theme')
    }
  }

  /**
   * Toggles the current theme: dark/light.
   */
  function toggleTheme() {
    isDark.value = !isDark.value
    applyTheme(isDark.value)
  }

  /**
   * Initializes the theme by loading the user's preferred theme from localStorage.
   */
  function initTheme() {
    const savedTheme = localStorage.getItem('theme')
    isDark.value = savedTheme === 'dark'
    applyTheme(isDark.value)
  }

  /**
   * Toggles the search panel.
   */
  function toggleSearchPanel() {
    isSearchPanelOpen.value = !isSearchPanelOpen.value
  }


  return {
    isSidebarOpen,
    isDark,
    isSearchPanelOpen,
    toggleSidebar,
    closeSidebar,
    toggleTheme,
    initTheme,
    toggleSearchPanel,
  }
})
