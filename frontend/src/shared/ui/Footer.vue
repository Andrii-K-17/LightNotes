<script setup lang="ts">
</script>

<template>
  <footer class="bg-neutral-100 text-neutral-700 p-4 border-t-1 dark:bg-neutral-800 dark:text-neutral-300 transition duration-500 border-gray-200 dark:border-black flex justify-center items-center">
    <div class="text-sm flex flex-col items-center">
      <div class="mb-2">
        <span>&copy; {{ new Date().getFullYear() }} LightNotes. All rights reserved.</span>
      </div>
      <div class="flex flex-wrap justify-center gap-x-2">
        <span>
          <router-link to="/privacy-policy" class="hover:text-sky-500 dark:hover:text-sky-400 transition-colors">Privacy Policy</router-link>
        </span>
        <span>
          <router-link to="/terms-of-use" class="hover:text-sky-500 dark:hover:text-sky-400 transition-colors">Terms of Use</router-link>
        </span>
      </div>
    </div>
  </footer>
</template>
