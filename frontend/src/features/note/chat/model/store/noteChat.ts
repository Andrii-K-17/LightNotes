import { defineStore } from 'pinia'
import * as signalR from '@microsoft/signalr'
import { API_CONFIG } from '../../../../../shared/config/api'
import { useAuthStore } from '../../../../../entities/session/model/store/auth'
import { ref } from 'vue'
import type { Message, SendMessageRequest } from '../types'

/**
 * Pinia store for managing real-time chat for notes using SignalR.
 */
export const useNoteChatStore = defineStore('noteChat', () => {
  const connection = ref<signalR.HubConnection | null>(null)
  const messages = ref<Message[]>([])
  const currentNoteId = ref<string | null>(null)
  const isConnecting = ref(false)
  const chatError = ref<string | null>(null)

  const authStore = useAuthStore()

  /**
   * Establishes a SignalR connection to a specific note's chat.
   */
  const startConnection = async (noteId: string) => {
    if (connection.value?.state === signalR.HubConnectionState.Connected && currentNoteId.value === noteId) {
      return
    }
    if (connection.value) {
      await stopConnection()
    }
    
    const token = authStore.token
    if (!token) {
      chatError.value = "Authorization token not found."
      return
    }

    connection.value = new signalR.HubConnectionBuilder()
      .withUrl(`${API_CONFIG.BASE_URL}/notechathub`, {
        accessTokenFactory: () => token
      })
      .withAutomaticReconnect()
      .build()

    connection.value.on('ReceiveChatHistory', (history: Message[]) => {
      messages.value = history
    })
    connection.value.on('ReceiveMessage', (message: Message) => {
      if (!messages.value.some(msg => msg.id === message.id)) {
        messages.value.push(message)
      }
    })
    connection.value.on('MessageDeleted', (messageId: string) => {
      messages.value = messages.value.filter(msg => msg.id !== messageId)
    })

    isConnecting.value = true
    chatError.value = null

    try {
      await connection.value.start()
      currentNoteId.value = noteId
      await connection.value.invoke('JoinNoteChat', noteId)
      await connection.value.invoke('GetChatHistory', noteId)
    } catch (err) {
      chatError.value = "Failed to establish connection."
      console.error("Connection error: ", err)
    } finally {
      isConnecting.value = false
    }
  }

  /**
   * Stops the SignalR connection and cleans up state.
   */
  const stopConnection = async () => {
    if (connection.value?.state === signalR.HubConnectionState.Connected) {
      if (currentNoteId.value) {
        await connection.value.invoke('LeaveNoteChat', currentNoteId.value)
      }
      await connection.value.stop()
      messages.value = []
      currentNoteId.value = null
    }
  }

  /**
   * Sends a new message to the connected chat.
   */
  const sendMessage = async (text: string) => {
    if (!text.trim() || !connection.value || !currentNoteId.value) return
    chatError.value = null
    try {
      const request: SendMessageRequest = { text: text.trim() }
      await connection.value.invoke('SendMessage', currentNoteId.value, request)
    } catch (err) {
      chatError.value = "Failed to send message."
      console.error("Error sending message: ", err)
    }
  }

  /**
   * Deletes a message from the chat.
   */
  const deleteMessage = async (messageId: string) => {
    if (!connection.value || !currentNoteId.value) return
    try {
      await connection.value.invoke('DeleteMessage', currentNoteId.value, messageId)
    } catch (err) {
      console.error("Error deleting message: ", err)
    }
  } 

  return {
    messages,
    isConnecting,
    chatError,
    startConnection,
    stopConnection,
    sendMessage,
    deleteMessage,
  }
})
