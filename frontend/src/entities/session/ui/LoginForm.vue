<script setup lang="ts">
import { ref } from 'vue'
import BaseButton from '../../../shared/ui/BaseButton.vue'
import { useAuthStore } from '../model/store/auth'
import type { LoginRequestDto } from '../model/types'
import { useRouter } from 'vue-router'

const auth = useAuthStore()
const router = useRouter()

const credentials = ref<LoginRequestDto>({
  email: '',
  password: '',
})

const onSubmit = async () => {
  clearErrors()

  await auth.login(credentials.value)
  
  if (auth.isAuthenticated) {
    router.push('/home')
  }
}

const clearErrors = () => {
  setTimeout(() => {
    auth.error = null
  }, 3000)
}
</script>

<template>
  <form @submit.prevent="onSubmit" class="space-y-4 max-w-md mx-auto">
    <div>
      <label class="block text-sm font-medium dark:text-neutral-300">Email</label>
      <input
        v-model="credentials.email"
        type="email" required
        class="mt-1 w-full rounded border border-neutral-900 px-3 py-2 dark:border-neutral-400 dark:caret-neutral-400 dark:text-neutral-300 hover:border-sky-400"
      />
    </div>
    
    <div>
      <label class="block text-sm font-medium dark:text-neutral-300">Password</label>
      <input
        v-model="credentials.password"
        type="password" required minlength="8"
        class="mt-1 w-full rounded border border-neutral-900 px-3 py-2 dark:border-neutral-400 dark:caret-neutral-400 dark:text-neutral-300 hover:border-sky-400"
      />
    </div>
    
    <div>
      <BaseButton
        :primary="true"
        type="submit"
        :loading="auth.loading"
        class="w-full h-10"
      >
        Log in
      </BaseButton>
    </div>

    <p v-if="auth.error" class="text-red-500 text-base text-center animate-pulse">
      {{ auth.error }}
    </p>
  </form>
</template>
