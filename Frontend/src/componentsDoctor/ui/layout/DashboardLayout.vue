<script setup>
import { computed } from 'vue'

// Importación de componentes exclusivos del doctor
import Header from '@/componentsDoctor/Header.vue'
import Navbar from '@/componentsDoctor/Navbar.vue'

// Obtenemos el rol del localStorage de forma limpia
const rol = computed(() => {
  const userRole = localStorage.getItem('userRole') || localStorage.getItem('rol')
  return userRole ? userRole.toLowerCase() : null
})
</script>

<template>
  <!-- Contenedor base  -->
 <div
  v-if="rol" class="flex flex-col min-h-screen overflow-hidden bg-slate-100 text-slate-800
  dark:bg-[#020817] dark:text-slate-100 transition-colors duration-300">

    <!-- 1. CABECERA: Solo se renderiza si el rol es  'doctor' -->
    <div
        v-if="rol === 'doctor'" class="shrink-0 shadow-sm z-10 bg-white/90 dark:bg-slate-950/90
        backdrop-blur border-b border-slate-200 dark:border-slate-800 transition-colors duration-300">
      <Header />
      <Navbar />
    </div>

    <main class="flex-1 overflow-y-auto bg-slate-50 dark:bg-[#020817] transition-colors duration-300">
      <div class="w-full h-full">
        <router-view />
      </div>
    </main>
  </div>

  
  <div v-else class="h-screen w-full flex flex-col items-center justify-center bg-white">
    <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-sky-600 mb-4"></div>
    <p class="text-slate-500 font-medium">Cargando panel...</p>
  </div>
</template>

<style scoped>
.fade-enter-active, .fade-leave-active { transition: opacity 0.2s ease; }
.fade-enter-from, .fade-leave-to { opacity: 0; }
</style>
