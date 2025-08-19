<script setup lang="ts">
import LoginForm from '../../entities/session/ui/LoginForm.vue'
</script>

<template>
  <div class="pt-4 pb-4">
    <h1 class="text-3xl font-bold mb-2 text-center dark:text-white">Log in</h1>
    <p class="text-gray-600 dark:text-gray-300 mb-6 text-center">Welcome back! Please sign in to your account.</p>
    
    <LoginForm />

    <p class="text-center text-sm dark:text-neutral-300 pt-4">
      Don’t have an account? 
      <router-link to="/register" class="text-sky-500 hover:text-sky-600 hover:underline font-medium">
        Sign up
      </router-link>
    </p>
  </div>
</template>
