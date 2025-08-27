<script setup lang="ts">
import { ref } from 'vue'

const featureList = ref([
  {
    title: 'Clean, intuitive interface',
    description: 'A minimalist design that eliminates visual clutter and helps the user focus completely on creativity. 🎨'
  },
  {
    title: 'Collaborative notes',
    description: 'A powerful tool for teamwork. Share notes with friends or colleagues to work on ideas together. 🤝'
  },
  {
    title: 'In-note chat',
    description: 'Discuss details in real-time right inside the note. This makes teamwork more efficient by eliminating the need to switch between different apps. 💬'
  },
])
</script>

<template>
  <div class="zoom-in-and-focus flex flex-col min-h-screen text-gray-900 dark:text-gray-100 transition-colors duration-500">
    <main class="flex flex-col items-center text-center px-4 md:px-8 pt-16 pb-12">
      <section class="w-full flex flex-col justify-center items-center space-y-8">
        <div class="flex flex-col items-center">
          <h1 class="text-5xl md:text-7xl mb-7 font-extrabold tracking-tighter leading-tight bg-clip-text text-transparent bg-gradient-to-r from-blue-500 to-sky-400">
            Clarity, made simple.
          </h1>
          <p class="mt-4 text-xl md:text-2xl font-medium text-gray-600 dark:text-gray-400">
            Create, share, and manage your notes effortlessly.
          </p>
        </div>
        <div class="flex flex-col justify-center items-center mt-3 mb-10">
          <router-link
            to="/register"
            class="w-full sm:w-auto px-15 py-4 text-lg font-bold text-white bg-sky-500 dark:bg-sky-600 rounded-full shadow-lg hover:bg-sky-600 dark:hover:bg-sky-700 transition-all duration-300 transform hover:scale-105"
          >
            Get started
          </router-link>
          <router-link
            to="/login"
            class="px-15 py-4 text-md underline text-neutral-700 dark:text-neutral-300 hover:text-sky-500 dark:hover:text-sky-400 transition-all duration-300 transform hover:scale-105"
          >
            Already have an account? Log in
          </router-link>
        </div>
        <hr class="w-full text-neutral-300"/>
      </section>

      <section class="w-full mt-10 max-w-6xl mx-auto">
        <h2 class="text-3xl md:text-4xl font-extrabold text-center mb-12">Core features</h2>
        <div class="grid grid-cols-1 md:grid-cols-3 gap-8">
          <div
            v-for="(feature, index) in featureList"
            :key="index"
            class="p-8 rounded-2xl shadow-xl transition-all duration-300 transform hover:scale-105 hover:shadow-2xl bg-white dark:bg-zinc-900 border border-gray-100 dark:border-zinc-800"
          >
            <h3 class="text-xl md:text-2xl font-bold mb-2">
              {{ feature.title }}
            </h3>
            <p class="text-gray-600 dark:text-gray-400">
              {{ feature.description }}
            </p>
          </div>
        </div>
      </section>
    </main>
  </div>
</template>

<style scoped>
  @keyframes zoomInAndFocus {
    0% {
      opacity: 0;
      filter: blur(10px);
    }
    100% {
      opacity: 1;
      filter: blur(0);
    }
  }
  .zoom-in-and-focus {
    animation: zoomInAndFocus 1s linear forwards;
  }
</style>
