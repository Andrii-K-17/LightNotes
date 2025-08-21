<script setup lang="ts">

const props = defineProps<{
  message: string 
}>() 

</script>

<template>
  <div v-if="message" class="fixed top-5 left-1/2 -translate-x-1/2 p-4 px-8 bg-neutral-800 text-neutral-100 dark:bg-neutral-200 dark:text-neutral-900 rounded-md shadow-lg z-50">
    {{ message }}
  </div>
</template>
