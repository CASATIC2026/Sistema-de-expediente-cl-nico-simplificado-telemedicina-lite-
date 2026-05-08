<script setup>
import { computed } from 'vue'
import { useRoute } from 'vue-router'
import { useAuth } from '@/composables/useAuth'

// Importación de componentes exclusivos del doctor
import Header from '@/componentsDoctor/Header.vue'
import Navbar from '@/componentsDoctor/Navbar.vue'

const route = useRoute()
const { logout } = useAuth()

// Obtenemos el rol del localStorage de forma limpia
const rol = computed(() => {
  const userRole = localStorage.getItem('userRole') || localStorage.getItem('rol')
  return userRole ? userRole.toLowerCase() : null
})
</script>

<template>
  <!-- Contenedor base  -->
  <div v-if="rol" class="flex flex-col min-h-screen bg-slate-100 overflow-hidden">

    <!-- 1. CABECERA: Solo se renderiza si el rol es exactamente 'doctor' -->
    <header v-if="rol === 'doctor'" class="flex-shrink-0 shadow-sm z-10 bg-white">
      <Header />
      <Navbar />
    </header>

    <main class="flex-grow overflow-y-auto bg-slate-50">
      <div class="w-full h-full">

        <!--PARA QUE EL DOCTOR PUEDA MANTENER LA VIDEOLLAMADA EN CURSO MIENTRAS NEVAGA EN EL NAVBAR-->
        <router-view v-slot="{ Component }">
            <KeepAlive :include="['ConsultaView']">
              <component :is="Component" />
            </KeepAlive>  
        </router-view>
      </div>
    </main>

  </div>

  <!-- Pantalla de carga profesional mientras se identifica el rol -->
  <div v-else class="h-screen w-full flex flex-col items-center justify-center bg-white">
    <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-sky-600 mb-4"></div>
    <p class="text-slate-500 font-medium">Cargando panel...</p>
  </div>
</template>

<style scoped>
.fade-enter-active, .fade-leave-active { transition: opacity 0.2s ease; }
.fade-enter-from, .fade-leave-to { opacity: 0; }
</style>
