<script setup>
import { ref, onMounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useVideoStore } from '@/stores/videoStore'
import {
  MoonIcon,
  SunIcon
} from '@heroicons/vue/24/outline'

const menuAbierto = ref(false)
const darkMode = ref(false)

const router = useRouter()
const route = useRoute()
const videoStore = useVideoStore()

const mostrarAvisoConsulta = ref(false)

const links = [
  { name: 'MedDashboard', label: 'Dashboard',        icon: '🏠' },
  { name: 'Agenda',       label: 'Agenda',            icon: '📅' },
  { name: 'Consultas',    label: 'Sala de consulta',  icon: '🩺' },
  { name: 'Pacientes',    label: 'Pacientes',         icon: '👥' },
]

// Modo oscuro
onMounted(() => {
  darkMode.value =
    localStorage.theme === 'dark' ||
    (!('theme' in localStorage) &&
      window.matchMedia('(prefers-color-scheme: dark)').matches)

  actualizarTema()
})

const toggleDarkMode = () => {
  darkMode.value = !darkMode.value
  actualizarTema()
}

const actualizarTema = () => {
  if (darkMode.value) {
    document.documentElement.classList.add('dark')
    localStorage.theme = 'dark'
  } else {
    document.documentElement.classList.remove('dark')
    localStorage.theme = 'light'
  }
}

const navegar = (linkName) => {

  // Permitir entrar a la sala de consulta siempre
  if (linkName === 'Consultas') {
    router.push({ name: linkName })
    menuAbierto.value = false
    return
  }

  // Si hay consulta activa → mostrar aviso
  if (videoStore.isActive) {
     menuAbierto.value = false
     mostrarAvisoConsulta.value = true
     return
  }

  router.push({ name: linkName })
  menuAbierto.value = false
}
</script>

<template>
  <nav
  class="bg-[#4682B4]
         dark:bg-slate-950
         text-white
         shadow-lg
         border-b border-white/10 dark:border-slate-800
         transition-colors duration-300"
>

    <!-- DESKTOP: links horizontales -->
    <div class="hidden md:flex items-center justify-center gap-2 py-3 px-6 relative">

      <button
          v-for="link in links"
          :key="link.name"
          @click="navegar(link.name)"
          class="px-4 py-2 rounded-xl transition-colors whitespace-nowrap text-white hover:bg-white/15 text-md font-medium"
          :class="{
            'bg-white/30 font-bold border-b-2 border-white rounded-none':
              route.name === link.name,

            'animate-pulse text-teal-200':
              link.name === 'Consultas' && videoStore.isActive
          }"
        >
          {{ link.label }}
      </button>

      <!-- TOGGLE -->
      <button
      @click="toggleDarkMode"
      class="absolute right-6
            w-11 h-11 rounded-xl
            bg-white/10 hover:bg-white/20
            border border-white/10
            flex items-center justify-center
            transition-all"
>
        <SunIcon
        v-if="darkMode"
        class="w-5 h-5 text-yellow-300"
      />
      <MoonIcon
        v-else
        class="w-5 h-5 text-cyan-200"
      />
      </button>
    </div>

    <!-- MÓVIL: barra con hamburguesa -->
    <div class="md:hidden flex items-center justify-between px-4 py-3">
      <span class="font-bold text-sm tracking-wider">T e l M e d lite</span>
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
        <button
            v-for="link in links"
            :key="link.name"
            @click="navegar(link.name)"
            class="w-full flex items-center gap-3 px-6 py-3.5 text-sm font-medium hover:bg-white/20 transition-colors"
            :class="{
              'bg-white/30 font-bold border-l-4 border-white':
                route.name === link.name
            }"
          >
            <span>{{ link.icon }}</span>
            {{ link.label }}
          </button>

        <button
            @click="toggleDarkMode"
            class="w-full flex items-center justify-between
            px-6 py-3.5
            text-sm font-medium
            hover:bg-white/10
            transition-colors"
          >
            <div class="flex items-center gap-3">
              <MoonIcon
                v-if="!darkMode"
                class="w-5 h-5"
              />

              <SunIcon
                v-else
                class="w-5 h-5"
              />

              <span>
                {{ darkMode ? 'Modo claro' : 'Modo oscuro' }}
              </span>
            </div>

            <div
              class="w-10 h-5 rounded-full
              transition-all relative"
              :class="darkMode ? 'bg-cyan-500' : 'bg-white/20'"
            >
              <div
                class="absolute top-0.5 w-4 h-4 rounded-full bg-white transition-all"
                :class="darkMode ? 'left-5' : 'left-0.5'"
              ></div>
            </div>
          </button>

      </div>
    </transition>
  </nav>
  <!-- AVISO CONSULTA ACTIVA -->
<transition name="slide">
  <div
    v-if="mostrarAvisoConsulta"
    class="fixed inset-x-4 bottom-5 md:left-1/2 md:-translate-x-1/2 md:max-w-lg
    bg-slate-950 text-white rounded-2xl border border-teal-500/20
    shadow-2xl shadow-black/40 p-4 z-50"
  >

    <div class="flex items-start gap-3">

      <div class="mt-1 h-3 w-3 rounded-full bg-teal-400 animate-pulse"></div>

      <div class="flex-1">
        <h3 class="font-black text-teal-300">
          Consulta en curso
        </h3>

        <p class="text-sm text-slate-300 mt-1">
          Debes finalizar la consulta activa antes de acceder a otras secciones.
        </p>

        <div class="flex gap-2 mt-4">

          <button
            @click="
              mostrarAvisoConsulta = false;
              router.push({ name: 'Consultas' })
            "
            class="bg-teal-500 hover:bg-teal-400 text-white px-4 py-2 rounded-xl text-sm font-bold transition-all"
          >
            Volver a consulta
          </button>

          <button
            @click="mostrarAvisoConsulta = false"
            class="bg-white/10 hover:bg-white/20 px-4 py-2 rounded-xl text-sm font-bold transition-all"
          >
            Entendido
          </button>

        </div>
      </div>
    </div>
  </div>
</transition>
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
