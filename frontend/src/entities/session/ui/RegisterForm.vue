<script setup lang="ts">
import { ref } from 'vue'
import BaseButton from '../../../shared/ui/BaseButton.vue'
import { useAuthStore } from '../model/store/auth'
import type { RegisterRequestDto } from '../model/types'
import { useRouter } from 'vue-router'

const auth = useAuthStore()
const router = useRouter()

const registrationData = ref<RegisterRequestDto>({
  email: '',
  password: '',
  name: '',
})

const confirmPassword = ref('')
const passwordMismatch = ref<string | null>(null)
const agreedToPolicy = ref(false)

const onSubmit = async () => {
  passwordMismatch.value = null
  clearErrors()

  if (registrationData.value.password !== confirmPassword.value) {
    passwordMismatch.value = 'Passwords do not match.'
    return
  }

  if (!agreedToPolicy.value) {
    return
  }

  await auth.register(registrationData.value)

  if (!auth.error) {
    router.push('/login')
  }
}

const clearErrors = () => {
  setTimeout(() => {
    auth.error = null
    passwordMismatch.value = null
  }, 3000)
}
</script>

<template>
  <form @submit.prevent="onSubmit" class="space-y-4 max-w-md mx-auto">
    <div>
      <label class="block text-sm font-medium dark:text-neutral-300">Name</label>
      <input
        v-model="registrationData.name"
        type="text" required
        class="mt-1 w-full rounded border border-neutral-900 px-3 py-2 dark:border-neutral-400 dark:caret-neutral-400 dark:text-neutral-300 hover:border-sky-400"
      />
    </div>

    <div>
      <label class="block text-sm font-medium dark:text-neutral-300">Email</label>
      <input
        v-model="registrationData.email"
        type="email" required
        class="mt-1 w-full rounded border border-neutral-900 px-3 py-2 dark:border-neutral-400 dark:caret-neutral-400 dark:text-neutral-300 hover:border-sky-400"
      />
    </div>
    
    <div>
      <label class="block text-sm font-medium dark:text-neutral-300">Password</label>
      <input
        v-model="registrationData.password"
        type="password" required minlength="8"
        class="mt-1 w-full rounded border border-neutral-900 px-3 py-2 dark:border-neutral-400 dark:caret-neutral-400 dark:text-neutral-300 hover:border-sky-400"
      />
    </div>
    
    <div>
      <label class="block text-sm font-medium dark:text-neutral-300">Confirm Password</label>
      <input
        v-model="confirmPassword"
        type="password" required minlength="8"
        class="mt-1 w-full rounded border border-neutral-900 px-3 py-2 dark:border-neutral-400 dark:caret-neutral-400 dark:text-neutral-300 hover:border-sky-400"
      />
    </div>

    <div class="flex items-center space-x-2">
      <input
        v-model="agreedToPolicy"
        type="checkbox"
        id="privacy-policy-checkbox"
        required
        class="form-checkbox w-4 h-4 text-sky-400 focus:ring-sky-400 accent-sky-400"
      />
      <label for="privacy-policy-checkbox" class="text-sm dark:text-neutral-300">
        I agree to the
        <router-link to="/privacy-policy" class="text-sky-500 hover:text-sky-600">Privacy Policy</router-link>
        and
        <router-link to="/terms-of-use" class="text-sky-500 hover:text-sky-600">Terms of Use</router-link>
      </label>
    </div>
    
    <div>
      <BaseButton
        :primary="true"
        type="submit"
        :loading="auth.loading"
        class="w-full h-10"
      >
        Sign up
      </BaseButton>
    </div>
    
    <p v-if="auth.error || passwordMismatch" class="text-red-500 text-base text-center animate-pulse">
      {{ auth.error || passwordMismatch }}
    </p>
  </form>
</template>
