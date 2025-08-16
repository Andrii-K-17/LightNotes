<script setup lang="ts">
import { computed } from 'vue'

const props = defineProps<{
  primary?: boolean
  disabled?: boolean
}>()

const buttonClasses = computed(() => {
  const baseStyles = 'px-5 py-2 rounded-lg font-medium transition duration-500'
  const stateStyles = {
    'opacity-50': props.disabled,
  }
  const colorStyles = {
    'bg-sky-500 text-white hover:bg-sky-600': props.primary,
    'bg-gray-300 text-gray-900 hover:bg-gray-400 dark:bg-gray-700 dark:text-gray-100 dark:hover:bg-gray-600': !props.primary,
  }

  return [baseStyles, stateStyles, colorStyles]
})
</script>

<template>
  <button
    :class="buttonClasses"
    :disabled="disabled"
    v-bind="$attrs"
  >
    <slot />
  </button>
</template>
