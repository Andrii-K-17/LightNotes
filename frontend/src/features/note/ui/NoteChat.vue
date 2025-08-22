<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue'
import BaseButton from '../../../shared/ui/BaseButton.vue'
import { useNoteChatStore } from '../chat/model/store/noteChat'
import { useAuthStore } from '../../../entities/session/model/store/auth'

const props = defineProps<{ noteId: string }>()
const noteChatStore = useNoteChatStore()
const authStore = useAuthStore()

const newMessage = ref('')

onMounted(() => {
  noteChatStore.startConnection(props.noteId)
})

onUnmounted(() => {
  noteChatStore.stopConnection()
})

/**
 * Handles sending a new message to the chat.
 */
const handleSendMessage = async() => {
  await noteChatStore.sendMessage(newMessage.value)
  newMessage.value = ''
}

/**
 * Handles the deletion of a message.
 */
const handleDeleteMessage = async(id: string) => {
  await noteChatStore.deleteMessage(id)
  newMessage.value = ''
}
</script>

<template>
  <div class="border rounded-lg p-3 space-y-3 border-neutral-300 dark:border-neutral-600">
    <div class="h-40 overflow-y-auto border-b pb-2 border-neutral-300 dark:border-neutral-600 custom-scrollbar">
      <div
        v-for="message in noteChatStore.messages"
        :key="message.id"
        class="flex mb-2" 
        :class="{ 'justify-end': message.senderId === authStore.user?.id }"
      >
        <div
          class="text-sm dark:text-neutral-100 p-2 rounded-lg max-w-[90%] break-words"
          :class="{
            'bg-sky-300 text-neutral-900 dark:bg-sky-800 dark:text-neutral-100': message.senderId === authStore.user?.id,
            'bg-neutral-200 dark:bg-neutral-700': message.senderId !== authStore.user?.id
          }"
        >
          <div class="flex flex-row items-center justify-between">
            <div class="font-semibold">{{ message.senderName }}</div>
            <button
              v-if="message.senderId === authStore.user?.id"
              @click="handleDeleteMessage(message.id)"
              class="h-5 ml-1 pl-1 pr-1 flex items-center justify-center rounded-lg text-sm cursor-pointer text-neutral-900 dark:text-neutral-100 border border-gray-400 dark:border-gray-500 hover:scale-110 transition-transform"
              title="Delete the message"
              >
              Delete
            </button>
          </div>
          <div>{{ message.text }}</div>
        </div>
      </div>
    </div>
    <div class="flex gap-2 flex-row">
      <input
        v-model="newMessage"
        @keyup.enter="handleSendMessage"
        class="w-full border rounded px-2 py-1 border-neutral-300 dark:border-neutral-600 dark:text-neutral-100"
        placeholder="Type a message..."
      />
      <BaseButton :primary="true" @click="handleSendMessage">
        Send
      </BaseButton>
    </div>
  </div>
</template>
