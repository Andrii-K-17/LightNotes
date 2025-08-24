<script setup lang="ts">
import { useUserProfileStore } from '../../../entities/user-profile/model/store/user-profile'

const userProfileStore = useUserProfileStore()

const confirmDeletion = async () => {
  if (window.confirm('Are you sure you want to delete your account? This action is irreversible.')) {
    await userProfileStore.deleteAccount()
  }
}
</script>

<template>
  <div>
    <button
      class="px-5 py-2 rounded-lg font-medium transition duration-500 flex items-center justify-center bg-red-600 text-neutral-100 hover:bg-red-500"
      @click="confirmDeletion"
      :disabled="userProfileStore.loading"
    >
      <span v-if="userProfileStore.loading">Deleting...</span>
      <span v-else>Delete my account</span>
    </button>

    <p v-if="userProfileStore.error" class="text-red-500">{{ userProfileStore.error }}</p>
  </div>
</template>
