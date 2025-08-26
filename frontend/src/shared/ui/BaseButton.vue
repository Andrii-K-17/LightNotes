<script setup lang="ts">
import { computed } from 'vue'
import Spinner from './Spinner.vue'

const props = defineProps<{
  primary?: boolean
  disabled?: boolean
  loading?: boolean
}>()

const buttonClasses = computed(() => {
  const baseStyles = 'px-5 py-2 rounded-lg cursor-pointer font-medium transition duration-500 flex items-center justify-center'
  const stateStyles = {
    'pointer-events-none': props.disabled || props.loading,
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
    :disabled="disabled || loading"
    v-bind="$attrs"
  >
    <template v-if="loading">
      <Spinner class="w-5 h-5"/>
    </template>
    <template v-else>
      <slot />
    </template>
  </button>
</template>
