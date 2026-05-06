<script setup>
import { ref } from 'vue'

const menuAbierto = ref(false)

const links = [
  { name: 'MedDashboard', label: 'Dashboard',        icon: '🏠' },
  { name: 'Agenda',       label: 'Agenda',            icon: '📅' },
  { name: 'Consultas',    label: 'Sala de consulta',  icon: '🩺' },
  { name: 'Pacientes',    label: 'Pacientes',         icon: '👥' },
]
</script>

<template>
  <nav class="bg-[#4682B4] text-white shadow-md">

    <!-- DESKTOP: links horizontales -->
    <div class="hidden md:flex justify-center gap-2 py-3 px-4">
      <RouterLink
        v-for="link in links"
        :key="link.name"
        :to="{ name: link.name }"
        class="px-4 py-2 rounded transition-colors whitespace-nowrap hover:bg-white/20 text-sm font-medium"
        active-class="bg-white/30 font-bold border-b-2 border-white rounded-none"
      >
        {{ link.label }}
      </RouterLink>
    </div>

    <!-- MÓVIL: barra con hamburguesa -->
    <div class="md:hidden flex items-center justify-between px-4 py-3">
      <span class="font-bold text-sm tracking-wider">NOVOMED</span>
      <button
        @click="menuAbierto = !menuAbierto"
        class="p-2 rounded-lg hover:bg-white/20 transition-colors"
        :aria-expanded="menuAbierto"
        aria-label="Menú de navegación"
      >
        <!-- Icono hamburguesa / X -->
        <svg v-if="!menuAbierto" xmlns="http://www.w3.org/2000/svg" class="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 6h16M4 12h16M4 18h16" />
        </svg>
        <svg v-else xmlns="http://www.w3.org/2000/svg" class="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
        </svg>
      </button>
    </div>

    <!-- MÓVIL: menú desplegable -->
    <transition name="slide">
      <div v-if="menuAbierto" class="md:hidden border-t border-white/20">
        <RouterLink
          v-for="link in links"
          :key="link.name"
          :to="{ name: link.name }"
          class="flex items-center gap-3 px-6 py-3.5 text-sm font-medium hover:bg-white/20 transition-colors"
          active-class="bg-white/30 font-bold border-l-4 border-white"
          @click="menuAbierto = false"
        >
          <span>{{ link.icon }}</span>
          {{ link.label }}
        </RouterLink>
      </div>
    </transition>

  </nav>
</template>

<style scoped>
.slide-enter-active, .slide-leave-active {
  transition: all 0.25s ease;
}
.slide-enter-from, .slide-leave-to {
  opacity: 0;
  transform: translateY(-8px);
}
</style>