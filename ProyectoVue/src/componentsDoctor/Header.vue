
//// ESTE COMPONENTE ES  EL ENCABEZADO DEL DASHBOARD DEL DOCTOR
///CONTIENE EL BOTÓN PARA VER SU PERFIL Y CONFIGURAR SU CUENTA, ASÍ COMO EL BOTÓN DE CERRAR SESIÓN





<script setup>
import { ref, onMounted } from 'vue'
import configuracionCuenta from '@/components/configuracionCuenta.vue'
import api, { logoutPro } from '@/services/api'
import fondoHeader from '@/assets/fondoHeader.jpg'

const mostrarConfig  = ref(false)
const usuario        = ref(null)
const nombreUsuario  = ref('Cargando...')

const cargarPerfil = async () => {
  try {
    const res  = await api.get('/Users/me')
    const data = res.data
    usuario.value = data

    const nombre   = data.nombre   ?? data.Nombre   ?? ''
    const apellido = data.apellido ?? data.Apellido  ?? ''
    nombreUsuario.value = `${nombre} ${apellido}`.trim() || 'Doctor'

    localStorage.setItem('user_name', nombreUsuario.value)
  } catch (error) {
    console.error('Error al cargar perfil doctor:', error)
    nombreUsuario.value = localStorage.getItem('user_name') || 'Doctor'
  }
}

onMounted(() => {
  cargarPerfil();
})
</script>

<template>
  <header
    :style="{ backgroundImage: `url(${fondoHeader})` }"
    class="min-h-[80px] md:min-h-[150px] bg-cover bg-center flex items-center justify-between px-4 md:px-8 text-white shadow-lg"
  >
    <!-- IZQUIERDA -->
    <div class="flex items-center gap-3 md:gap-6">
      <img
        src="@/assets/Logo icon.png"
        alt="nuvomed lite"
        class="w-12 h-12 md:w-24 md:h-24 object-contain drop-shadow-[0_0_12px_rgba(34,211,238,0.4)]"
      />
      <div class="text-xs md:text-sm font-bold tracking-wider">
        N O V O M E D<span class="text-cyan-400"> Lite™</span>
      </div>
    </div>

    <!-- DERECHA -->
    <div class="flex items-center gap-3 md:gap-10">

      <!-- Usuario -->
      <div class="flex items-center gap-2 md:gap-4 border-r border-slate-700 pr-3 md:pr-6">
        <!-- Nombre e ID solo visible desde sm -->
        <div class="text-right hidden sm:block">
          <div class="font-semibold text-sm md:text-base">Dr. {{ nombreUsuario }}</div>
          <div class="text-xs opacity-60">ID: {{ usuario?.id ?? '—' }}</div>
        </div>

        <button
          @click="mostrarConfig = true"
          title="Mi Perfil"
          class="p-2 md:p-2.5 bg-slate-800 border border-slate-700 text-[#87CEFA] rounded-xl hover:bg-[#87CEFA] hover:text-slate-900 transition-all shadow-sm"
        >
          <svg xmlns="http://www.w3.org/2000/svg" class="h-6 w-6 md:h-8 md:w-8" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
              d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
          </svg>
        </button>
      </div>

      <!-- Logout -->
      <button
        @click="logoutPro"
        class="bg-red-500 hover:bg-red-600 px-3 py-2 md:px-4 md:py-3 rounded-xl text-[10px] md:text-xs font-bold uppercase tracking-widest transition-colors whitespace-nowrap"
      >
        <!-- Texto corto en móvil, completo en desktop -->
        <span class="sm:hidden">Salir</span>
        <span class="hidden sm:inline">Cerrar sesión</span>
      </button>

    </div>
  </header>

  <configuracionCuenta
    v-if="mostrarConfig"
    :usuarioData="usuario"
    :rolSesion="'doctor'"
    @cerrar="mostrarConfig = false"
  />
</template>