<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue'
import BaseButton from '../../../shared/ui/BaseButton.vue'
import { useNoteChatStore } from '../chat/model/store/noteChat'
import { useAuthStore } from '../../../entities/session/model/store/auth'
import { useNotesStore } from '../../../entities/note/model/store/notes'
import type { NoteResponseDto } from '../../../entities/note/model/types'

const props = defineProps<{ noteId: string }>()
const noteChatStore = useNoteChatStore()
const authStore = useAuthStore()
const notesStore = useNotesStore()

const newMessage = ref('')

const note = ref<NoteResponseDto | null>(null)

const isExpandedChat = ref<boolean>(false)

onMounted(async () => {
  note.value = await notesStore.fetchNoteById(props.noteId)
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

const expandChat = () => {
  isExpandedChat.value = !isExpandedChat.value
  isExpandedChat.value ? document.body.classList.add('overflow-hidden') : document.body.classList.remove('overflow-hidden')
}
</script>

<template>
  
  <div
    class="border rounded-lg pb-3 pr-1 pl-3 pt-1 transition-all duration-500 border-neutral-300 dark:border-neutral-600"
    :class="{'fixed md:w-4/5 w-full shadow-[0px_0px_50px_0px_rgb(0,_65,_110)] md:h-4/5 h-full top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 z-50 bg-neutral-100 dark:bg-neutral-800 flex flex-col': isExpandedChat}"
  >
    <div class="flex flex-row justify-end items-center mb-1 mr-1.5">
      <button
        @click="expandChat"
        title="Expand"
        class="h-5 w-5 ml-1 pl-1 pr-1 flex items-center justify-center rounded-lg text-sm cursor-pointer text-neutral-900 dark:text-neutral-100 border border-gray-400 dark:border-gray-500 hover:scale-110 transition-transform">
        ⛶
      </button>
    </div>

    <div class="overflow-y-auto h-40 mb-3 mr-2 flex-grow border-b pb-2 border-neutral-300 dark:border-neutral-600 custom-scrollbar">
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
            <div
              v-if="message.senderId === note?.ownerId && authStore.user?.id !== note?.ownerId"
              class="h-5 ml-1 pl-1 pr-1 flex items-center justify-center rounded text-sm text-neutral-900 dark:text-neutral-100 border border-gray-400 dark:border-gray-500"
              >
              Owner
            </div>
          </div>
          <div>{{ message.text }}</div>
        </div>
      </div>
    </div>

    <div class="flex gap-2 flex-row mr-2">
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
