// ESTE COMPONENTE ES  EL ENCABEZADO DEL DASHBOARD DEL DOCTOR
//CONTIENE EL BOTÓN PARA VER SU PERFIL Y CONFIGURAR SU CUENTA, ASÍ COMO EL BOTÓN DE CERRAR SESIÓN

<script setup>
import { ref, onMounted, onUnmounted } from 'vue'
import configuracionCuenta from '@/components/configuracionCuenta.vue'
import api, { logoutPro } from '@/services/api'
import { getProfileIcon } from '@/utils/getProfileIcon'

import fondoHeader from '@/assets/fondoHeader.jpg'
import { useRouter } from 'vue-router'
import { useVideoStore } from '@/stores/videoStore'
import {
  ArrowRightStartOnRectangleIcon
} from '@heroicons/vue/24/outline'

const mostrarConfig  = ref(false)
const usuario        = ref(null)

const nombreUsuario  = ref('Cargando...')
const fotoPerfil = ref('')
const iconoPerfil = ref('')

const router = useRouter()
const videoStore = useVideoStore()

const mostrarAvisoLogout = ref(false)
const mostrarConfirmLogout = ref(false)

const cargarPerfil = async () => {
  try {
    const res  = await api.get('/Users/me')
    const data = res.data
    usuario.value = data

    const nombre   = data.nombre   ?? data.Nombre   ?? ''
    const apellido = data.apellido ?? data.Apellido  ?? ''

   nombreUsuario.value = `${nombre} ${apellido}`.trim() || 'Doctor'

    fotoPerfil.value =
      data.fotoUrl ||
      data.FotoUrl ||
      ''

    const iconoGuardado =
      data.avatarIcono ||
      data.AvatarIcono ||
      ''

iconoPerfil.value = getProfileIcon(iconoGuardado)

localStorage.setItem('user_name', nombreUsuario.value)
  } catch (error) {
    console.error('Error al cargar perfil doctor:', error)
    nombreUsuario.value = localStorage.getItem('user_name') || 'Doctor'
  }
}

const actualizarPerfil = () => {
  cargarPerfil()
}

onMounted(() => {
  cargarPerfil()

  window.addEventListener('perfil-actualizado', actualizarPerfil)
})

onUnmounted(() => {
  window.removeEventListener('perfil-actualizado', actualizarPerfil)
})

const intentarLogout = () => {

  // Si hay consulta activa → bloquear
  if (videoStore.isActive) {
    mostrarAvisoLogout.value = true
    return
  }

  mostrarConfirmLogout.value = true
}

const confirmarLogout = async () => {
  mostrarConfirmLogout.value = false
  await logoutPro()
}

onMounted(() => {
  cargarPerfil();
})



</script>

<template>
  <header
    :style="{ backgroundImage: `url(${fondoHeader})` }"
    class="min-h-20 md:min-h-37.5 bg-cover bg-center flex items-center justify-between px-4 md:px-8 text-white shadow-lg"
  >
    <!-- IZQUIERDA -->
    <div class="flex items-center gap-3 md:gap-6">
      <img
        src="@/assets/Logo icon.png"
        alt="TelMed Lite™"
        class="w-12 h-12 md:w-24 md:h-24 object-contain drop-shadow-[0_0_12px_rgba(34,211,238,0.4)]"
      />
      <div class="hidden md:block text-2xl md:text-3xl font-bold tracking-wider">
       <strong>TelMed</strong><span class="text-cyan-400"> Lite™</span>
      </div>
    </div>

    <!-- DERECHA -->
    <div class="flex items-center gap-3 md:gap-10">

      <!-- Usuario -->
      <div class="flex items-center gap-2 md:gap-4 border-r border-slate-700 pr-3 md:pr-6">
        <div class="text-right hidden sm:block">
          <div class="font-semibold text-sm md:text-base">Dr. {{ nombreUsuario }}</div>

        </div>

        <button
          @click="mostrarConfig = true"
          title="Mi Perfil"
          class="overflow-hidden p-1 bg-slate-800 border border-slate-700 rounded-2xl hover:border-cyan-400 hover:scale-105 cursor-pointer transition-all shadow-sm"
        >

          <!-- FOTO -->
          <img
            v-if="fotoPerfil"
            :src="fotoPerfil"
            @error="fotoPerfil = ''"
            class="w-12 h-12 md:w-14 md:h-14 rounded-xl object-cover"
          />

          <!-- ICONO -->
          <img
            v-else-if="iconoPerfil"
            :src="iconoPerfil"
            class="w-12 h-12 md:w-14 md:h-14 rounded-xl object-cover bg-slate-900"
          />

          <!-- FALLBACK -->
          <div
            v-else
            class="w-12 h-12 md:w-14 md:h-14 rounded-xl bg-cyan-500 flex items-center justify-center font-bold text-slate-900"
          >
            {{ nombreUsuario?.[0] || 'D' }}
          </div>

        </button>
      </div>

      <!-- Logout -->

        <button
        @click="intentarLogout"
        title="Cerrar sesión"
        class="group relative p-2.5 md:p-3 rounded-xl
        bg-slate-900/70 backdrop-blur-md
        border border-slate-700/70

        text-slate-300
        hover:text-red-300
        hover:border-red-500/50
        hover:bg-red-700

        transition-all duration-200
        shadow-md shadow-black/20
        cursor-pointer"
      >
        <ArrowRightStartOnRectangleIcon
          class="w-5 h-5 md:w-6 md:h-6 transition-transform duration-200 group-hover:translate-x-0.5"
        />
      </button>
    </div>

  </header>

      <Teleport to="body">
      <configuracionCuenta
        v-if="mostrarConfig"
        :usuarioData="usuario"
        :rolSesion="'doctor'"
        @cerrar="mostrarConfig = false"
         @actualizar="cargarPerfil"
      />
    </Teleport>

    <!-- Aviso logout con consulta activa-->
    <transition name="slide">
  <div
    v-if="mostrarAvisoLogout"
    class="fixed inset-x-4 bottom-5 md:left-1/2 md:-translate-x-1/2 md:max-w-lg
    bg-slate-950 text-white rounded-2xl border border-red-500/20
    shadow-2xl shadow-black/40 p-4 z-50"
  >

    <div class="flex items-start gap-3">

      <div class="mt-1 h-3 w-3 rounded-full bg-red-400 animate-pulse"></div>

      <div class="flex-1">

        <h3 class="font-black text-red-300">
          Consulta activa detectada
        </h3>

        <p class="text-sm text-slate-300 mt-1">
          Debes finalizar la consulta médica antes de cerrar sesión.
        </p>

        <div class="flex gap-2 mt-4">

          <button
            @click="
              mostrarAvisoLogout = false;
              router.push({ name: 'Consultas' })
            "
            class="bg-red-500 hover:bg-red-400 text-white px-4 py-2 rounded-xl text-sm font-bold transition-all"
          >
            Volver a consulta
          </button>

          <button
            @click="mostrarAvisoLogout = false"
            class="bg-white/10 hover:bg-white/20 px-4 py-2 rounded-xl text-sm font-bold transition-all"
          >
            Entendido
          </button>

        </div>

      </div>
    </div>
  </div>
</transition>

<!-- Modal Cerrar Sesión -->
<Transition
  enter-active-class="transition-all duration-200 ease-out"
  enter-from-class="opacity-0"
  enter-to-class="opacity-100"
  leave-active-class="transition-all duration-150 ease-in"
  leave-from-class="opacity-100"
  leave-to-class="opacity-0"
>
  <div
    v-if="mostrarConfirmLogout"
    class="fixed inset-0 z-[9999] flex items-center justify-center bg-slate-950/80 backdrop-blur-sm p-4"
    @click.self="mostrarConfirmLogout = false"
  >
    <Transition
      enter-active-class="transition-all duration-200 ease-out"
      enter-from-class="opacity-0 scale-95 translate-y-2"
      enter-to-class="opacity-100 scale-100 translate-y-0"
    >
      <div
        v-if="mostrarConfirmLogout"
        class="bg-gradient-to-br from-[#0f2040] to-[#10284f] border border-cyan-500/20 rounded-2xl shadow-2xl w-full max-w-sm p-6 flex flex-col items-center gap-5"
      >
        <div class="w-14 h-14 rounded-2xl bg-red-500/10 border border-red-500/20 flex items-center justify-center">
          <ArrowRightStartOnRectangleIcon
            class="w-7 h-7 text-red-400"
          />
        </div>

        <div class="text-center">
          <h3 class="text-white font-black text-lg leading-tight">
            Cerrar sesión
          </h3>

          <p class="text-slate-400 text-sm font-medium mt-1.5">
            ¿Deseas cerrar tu sesión de forma segura?
          </p>
        </div>

        <div class="flex gap-3 w-full">
          <button
            @click="mostrarConfirmLogout = false"
            class="flex-1 py-3 rounded-xl bg-slate-700 hover:bg-slate-600 text-slate-300 font-black text-[10px] uppercase tracking-widest transition-colors"
          >
            Cancelar
          </button>

          <button
            @click="confirmarLogout"
            class="flex-1 py-3 rounded-xl bg-red-500 hover:bg-red-400 text-white font-black text-[10px] uppercase tracking-widest transition-colors"
          >
            Sí, salir
          </button>
        </div>

      </div>
    </Transition>
  </div>
</Transition>

</template>
