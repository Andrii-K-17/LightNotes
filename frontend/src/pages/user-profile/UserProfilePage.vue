<script setup lang="ts">
import DeleteAccountButton from '../../features/user-profile/ui/DeleteAccountButton.vue'
import { useAuthStore } from '../../entities/session/model/store/auth'
import { useUiStore } from '../../app/store/uiStore'
import Sidebar from '../../shared/ui/Sidebar.vue'
import BaseButton from '../../shared/ui/BaseButton.vue'
import router from '../../app/router'

const uiStore = useUiStore()

const authStore = useAuthStore()

const logoutAndCloseSidebar = () => {
  authStore.logout()
  router.push('/login')
}
</script>

<template>
  <div class="flex">
    <Sidebar />

    <div
      class="flex-1 flex-col py-3 px-4"
      :class="{'hidden': uiStore.isSidebarOpen}"
    >
      <div class="w-full">
        <div class="pb-3 flex flex-col justify-center items-center">
          <div class="text-3xl px-3 font-bold text-neutral-900 dark:text-neutral-100 border-b-2 border-gray-300 pb-2 mb-4">
            Account
          </div>

          <div class="bg-white dark:bg-neutral-800 p-6 rounded-lg shadow-lg w-full">
            <h2 class="text-xl font-semibold text-gray-700 dark:text-gray-200 mb-4">Personal information</h2>
            <div
              v-if="authStore.user"
              class="text-neutral-900 dark:text-neutral-100"
            >
              <p class="mb-2"><strong>Name:</strong> {{ authStore.user.name }}</p>
              <p class="mb-4"><strong>Email:</strong> {{ authStore.user.email }}</p>
            </div>
            <p v-else class="text-gray-500 italic">User data not loaded.</p>

            <hr class="mt-1 mb-3 text-neutral-300"/>
            <div class="flex flex-col justify-center items-center">
              <BaseButton
                @click="logoutAndCloseSidebar"
                :loading="authStore.loading"
                class="px-5 py-2 w-50"
              >
                Log out
              </BaseButton>

              <DeleteAccountButton class="mt-5" />
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
