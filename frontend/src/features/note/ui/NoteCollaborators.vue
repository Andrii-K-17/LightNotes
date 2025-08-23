<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useNotesStore } from '../../../entities/note/model/store/notes'
import { Role } from '../../../entities/note/model/types'
import type { NoteResponseDto } from '../../../entities/note/model/types'
import BaseButton from '../../../shared/ui/BaseButton.vue'
import Message from '../../../shared/ui/Message.vue'
import { useAuthStore } from '../../../entities/session/model/store/auth'

const props = defineProps<{ noteId: string }>()

const notesStore = useNotesStore()
const authStore = useAuthStore()

const note = ref<NoteResponseDto | null>(null)
const newCollaboratorEmail = ref('')
const isRolePickerOpen = ref<boolean>(false)

const collaborators = computed(() => note.value?.collaborators || [])

const isOwner = computed(() => {
  if (!note.value) {
    return false
  }
  return authStore.user?.id === note.value.ownerId
})

const roles = computed(() =>
  Object.keys(Role)
    .filter(key => key !== 'Admin')
)

const messageText = ref('')

const showMessage = (message: string, time: number = 3000) => {
   messageText.value = message
  setTimeout(() => {
    messageText.value = '' 
  }, time)
}

const toggleRolePicker = () => {
  isRolePickerOpen.value = !isRolePickerOpen.value
}

const selectRole = (collaboratorId: string, newRole: string) => {
  isRolePickerOpen.value = false
  updateCollaboratorRole(collaboratorId, newRole)
}

const addCollaborator = async () => {
  const email = newCollaboratorEmail.value.trim()
  const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/

  if (!email) {
    showMessage('Email cannot be empty.')
    return
  }
  if (!emailRegex.test(email)) {
    showMessage('Please enter a valid email address.')
    return
  }

  await notesStore.addCollaborator(props.noteId, { userEmail: email, role: Role.Viewer })
  if (notesStore.error) {
    showMessage(notesStore.error)
  }
  newCollaboratorEmail.value = ''
  note.value = await notesStore.fetchNoteById(props.noteId)
}

const updateCollaboratorRole = async (collaboratorId: string, newRole: string) => {
  const roleValue = Role[newRole as keyof typeof Role]

  await notesStore.updateCollaboratorRole(props.noteId, collaboratorId, { newRole: roleValue })
  if (notesStore.error) {
    showMessage(notesStore.error)
  }
  note.value = await notesStore.fetchNoteById(props.noteId)
}

const removeCollaborator = async (collaboratorId: string) => {
  await notesStore.removeCollaborator(props.noteId, collaboratorId)
  if (notesStore.error) {
    showMessage(notesStore.error)
  }
  note.value = await notesStore.fetchNoteById(props.noteId)
}

onMounted(async () => {
  note.value = await notesStore.fetchNoteById(props.noteId)
})
</script>

<template>
  <Message v-if="messageText" :message="messageText" />

  <div class="p-4 border rounded-lg border-neutral-300 dark:border-neutral-600">
    <h3 class="text-lg font-semibold mb-2 dark:text-neutral-100">Collaborators</h3>

    <div v-if="note" class="overflow-y-auto custom-scrollbar">
      <div class="space-y-2 mb-4">
        <div
          v-for="collaborator in collaborators"
          :key="collaborator.userId"
          class="flex items-center justify-between p-2 border border-neutral-300 dark:border-neutral-600 bg-neutral-200 dark:bg-neutral-800 rounded-md"
        >
          <div class="flex-1 flex items-center flex-wrap gap-y-1">
            <div
              v-if="isOwner"
              class="relative ml-2"
            >
              <button
                class="p-1 text-xs rounded border border-gray-400 dark:border-gray-600 dark:text-neutral-100 hover:scale-110 transition-transform"
                @click="toggleRolePicker"
              >
                {{ Object.keys(Role).find(key => (Role as any)[key] === collaborator.role) }}
              </button>
              
              <div
                v-if="isRolePickerOpen"
                class="absolute left-0 mt-2 w-20 bg-white dark:bg-gray-800 rounded-md shadow-lg p-1 z-10"
              >
                <button
                  v-for="roleName in roles"
                  :key="roleName"
                  @click="selectRole(collaborator.userId, roleName)"
                  class="w-full text-left px-2 py-1 text-sm rounded-md hover:bg-gray-200 dark:hover:bg-gray-700"
                >
                  {{ roleName }}
                </button>
              </div>
            </div>
            <span class="font-medium dark:text-white ml-2">
              {{ collaborator.userName }}
            </span>
            <span v-if="isOwner" class="pl-1 pr-1 font-medium text-sm dark:text-white ml-2 rounded border border-gray-400 dark:border-gray-600">
              {{ collaborator.userEmail }}
            </span>

            <span v-if="!isOwner" class="text-gray-500 text-sm ml-2 mr-2">
              ({{ roles[collaborator.role - 1] }})
            </span>
          </div>
          <button
            v-if="isOwner"
            @click="removeCollaborator(collaborator.userId)"
            class="w-5 h-5 ml-1 flex items-center justify-center rounded-full cursor-pointer text-neutral-900 dark:text-neutral-100 border border-gray-400 dark:border-gray-600 hover:text-red-800 dark:hover:text-red-400 hover:scale-110 transition-transform"
            title="Remove collaborator">
            &times
          </button>
        </div>
      </div>

      <div v-if="isOwner" class="flex flex-col gap-2">
        <h4 class="text-md font-semibold dark:text-white">Add Collaborator</h4>
        <div class="flex gap-2">
          <input
            v-model="newCollaboratorEmail"
            type="email"
            placeholder="Enter collaborator's email"
            @keyup.enter="addCollaborator"
            class="w-full border rounded px-2 py-1 border-neutral-300 dark:border-neutral-600 dark:text-neutral-100"
          />
          <BaseButton
            :primary="true"
            @click="addCollaborator"
          >
            Add
          </BaseButton>
        </div>
      </div>
    </div>
  </div>
</template>
