<script setup lang="ts">
import RegisterForm from '../../entities/session/ui/RegisterForm.vue'
</script>

<template>
  <div>
    <h1 class="text-3xl font-bold mb-2 text-center dark:text-white">Sign up</h1>
    <p class="text-gray-600 dark:text-gray-300 mb-6 text-center">Create your account to get started.</p>
    
    <RegisterForm />

    <p class="text-center text-sm dark:text-neutral-400 pt-4">
      Already have an account? 
      <router-link to="/login" class="text-sky-500 hover:text-sky-600 hover:underline font-medium">
        Sign in
      </router-link>
    </p>
  </div>
</template>
