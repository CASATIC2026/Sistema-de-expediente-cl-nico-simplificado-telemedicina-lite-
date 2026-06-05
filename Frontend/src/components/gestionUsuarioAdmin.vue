<script setup>
import { ref, computed, onMounted } from 'vue'
import { getProfileIcon } from '@/utils/getProfileIcon'

import configuracionCuenta from '@/components/configuracionCuenta.vue'
import ResumenPaciente from '@/components/ResumenPaciente.vue'

import { getPacientesAdmin, toggleEstadoPaciente, eliminarPacienteSoft } from '@/services/api'
import {
  MagnifyingGlassIcon, EyeIcon, ClockIcon, TrashIcon, UserGroupIcon, XMarkIcon } from '@heroicons/vue/24/outline'

const emit = defineEmits(['cerrar'])

const pacientes        = ref([])
const cargando         = ref(true)
const error            = ref(null)
const filtroQuery      = ref('')

const accederInformacionPaciente = ref(false)
const pacienteSeleccionado       = ref(null)


const accederResumen     = ref(false)
const pacienteResumenId  = ref(null)
// Toast
const toast = ref({ visible: false, mensaje: '', tipo: 'success' })
let toastTimer = null
const mostrarToast = (mensaje, tipo = 'success') => {
  if (toastTimer) clearTimeout(toastTimer)
  toast.value = { visible: true, mensaje, tipo }
  toastTimer = setTimeout(() => { toast.value.visible = false }, 3500)
}


const confirmModal = ref({
  visible: false,
  titulo: '',
  mensaje: '',
  resolveFn: null
})
const pedirConfirmacion = (titulo, mensaje) => {
  return new Promise((resolve) => {
    confirmModal.value = { visible: true, titulo, mensaje, resolveFn: resolve }
  })
}
const responderConfirm = (respuesta) => {
  confirmModal.value.resolveFn(respuesta)
  confirmModal.value.visible = false
}


const abrirResumen = (paciente) => {
  pacienteResumenId.value = paciente
  accederResumen.value    = true
}

onMounted(async () => {
  await cargarPacientes()
})

const cargarPacientes = async () => {
  try {
    cargando.value  = true
    error.value     = null
    pacientes.value = await getPacientesAdmin()
  } catch (e) {
    error.value = 'No se pudo cargar la lista de pacientes.'
  } finally {
    cargando.value = false
  }
}

const pacientesFiltrados = computed(() => {
  const q = filtroQuery.value.toLowerCase().trim()
  if (!q) return pacientes.value
  return pacientes.value.filter(u =>
    u.dui?.toLowerCase().includes(q) ||
    u.nombre?.toLowerCase().includes(q) ||
    u.apellido?.toLowerCase().includes(q)
  )
})

const obtenerIconoPaciente = (paciente) => {
  const iconoGuardado =
    paciente.avatarIcono ||
    paciente.AvatarIcono ||
    ''
  return getProfileIcon(iconoGuardado)
}

const toggleEstado = async (paciente) => {
  const nuevoEstado = !paciente.activo
  const accion      = nuevoEstado ? 'activar' : 'inactivar'
  const ok = await pedirConfirmacion(
    `${accion.charAt(0).toUpperCase() + accion.slice(1)} paciente`,
    `¿Deseas ${accion} a ${paciente.nombre} ${paciente.apellido}?`
  )
  if (!ok) return

  try {
    await toggleEstadoPaciente(paciente.id, nuevoEstado)
    paciente.activo = nuevoEstado
  } catch (e) {
    mostrarToast('Error al cambiar el estado del paciente.', 'error')
  }
}

const abrirInformacion = (paciente) => {
  pacienteSeleccionado.value          = paciente
  accederInformacionPaciente.value    = true
}



const eliminarPaciente =async (paciente) => {
  const ok = await pedirConfirmacion(
    'Eliminar paciente',
    `¿Eliminar a ${paciente.nombre} ${paciente.apellido}? Esta acción no se puede deshacer.`
  )
  if (!ok) return
  try {
    await eliminarPacienteSoft(paciente.id)
    pacientes.value = pacientes.value.filter(u => u.id !== paciente.id)
  } catch (e) {
    mostrarToast('Error al eliminar el paciente.', 'error')
  }
}
</script>


<template>
  <div class="min-h-screen bg-slate-300 dark:bg-slate-700 p-3 md:p-10 flex flex-col items-center transition-colors">

    <!-- BOTÓN REGRESAR -->
    <div class="w-full max-w-[95%] lg:w-[90%] mb-4">
      <button
        @click="emit('cerrar')"
        class="text-[10px] md:text-sm font-bold uppercase tracking-widest bg-white border border-slate-200 hover:bg-sky-200 cursor-pointer text-slate-600 px-4 py-2.5 md:px-5 md:py-3 rounded-xl md:rounded-2xl transition-all flex items-center gap-2 shadow-sm"
      >
        ← <span>Regresar</span>
      </button>
    </div>

    <div class="w-full max-w-[95%] lg:w-[90%]">

      <!-- HEADER -->
      <div class="mb-6 md:mb-8 bg-slate-800 rounded-2xl md:rounded-3xl px-5 py-5 md:px-8 md:py-6 flex flex-col md:flex-row md:items-center justify-between gap-4 md:gap-6">

        <div>
          <h1 class="text-xl md:text-3xl font-black text-white tracking-tight leading-tight">
            Gestión de Usuarios
          </h1>

          <p class="text-slate-400 font-medium text-sm md:text-base">
            Panel de control administrativo
          </p>
        </div>

        <!-- BUSCADOR -->
        <div class="relative w-full md:w-80 lg:w-96 group">

          <div class="absolute inset-y-0 left-0 pl-4 flex items-center pointer-events-none">
            <MagnifyingGlassIcon class="h-5 w-5 text-slate-400" />
          </div>

          <input
            v-model="filtroQuery"
            type="text"
            placeholder="Buscar por DUI o nombre..."
            class="w-full pl-11 pr-4 py-3 bg-slate-200 dark:bg-slate-800 border-2 border-transparent focus:border-teal-300 rounded-xl outline-none transition-all text-sm font-semibold text-slate-700 dark:text-slate-100 placeholder:text-slate-400"
          />
        </div>
      </div>

      <!-- CARGANDO -->
      <div v-if="cargando" class="flex justify-center items-center py-24">
        <div class="w-10 h-10 border-4 border-slate-400/20 border-t-teal-300 rounded-full animate-spin"></div>
      </div>

      <!-- ERROR -->
      <div
        v-else-if="error"
        class="p-10 text-center text-red-400 font-bold bg-red-500/10 rounded-3xl border border-red-500/20"
      >
        {{ error }}

        <button
          @click="cargarPacientes"
          class="mt-4 block mx-auto text-sm text-sky-300 hover:text-sky-200 underline font-black uppercase tracking-wider cursor-pointer"
        >
          Reintentar
        </button>
      </div>

      <!-- TABLA -->
      <div
        v-else
        class="hidden md:block bg-white dark:bg-slate-900 rounded-[1.5rem] shadow-xl overflow-hidden transition-colors"
      >
        <div class="overflow-x-auto">

          <table class="w-full text-left border-collapse">

            <thead>
              <tr class="bg-slate-950 text-white">

                <th class="px-8 py-5 text-[11px] font-black uppercase tracking-[0.2em]">
                  Paciente
                </th>

                <th class="px-8 py-5 text-[11px] font-black uppercase tracking-[0.2em]">
                  Identificación
                </th>

                <th class="px-8 py-5 text-[11px] font-black uppercase tracking-[0.2em]">
                  Estado
                </th>

                <th class="px-8 py-5 text-[11px] font-black uppercase tracking-[0.2em] text-center">
                  Acciones
                </th>

              </tr>
            </thead>

            <tbody class="divide-y bg-linear-to-r from-[#0f2040] to-[#10284f]">

              <tr
                v-for="paciente in pacientesFiltrados"
                :key="paciente.id"
                class="hover:bg-slate-800/30 transition-all group"
              >

                <!-- PACIENTE -->
                <td class="px-8 py-5">

                  <div class="flex items-center gap-4">

                    <div class="w-11 h-11 shrink-0 rounded-xl overflow-hidden shadow-sm border border-cyan-400/20 bg-slate-800" >
                        <!-- FOTO -->
                        <img
                          v-if="paciente.fotoUrl || paciente.FotoUrl"
                          :src="paciente.fotoUrl || paciente.FotoUrl"
                           @error="$event.target.style.display='none'"
                          class="w-full h-full object-cover"
                        />

                        <!-- ICONO -->
                        <img
                          v-else-if="obtenerIconoPaciente(paciente)"
                          :src="obtenerIconoPaciente(paciente)"
                          @error="$event.target.style.display='none'"
                          class="w-full h-full object-cover"
                        />

                        <!-- FALLBACK -->
                        <div
                          v-else
                          class="w-full h-full bg-teal-300 flex items-center justify-center font-bold text-slate-900 text-sm"
                        >
                          {{ paciente.nombre?.[0] }}{{ paciente.apellido?.[0] }}
                        </div>

                      </div>
                    <div>
                      <p class="font-bold text-white text-base leading-tight">
                        {{ paciente.nombre }} {{ paciente.apellido }}
                      </p>

                      <p class="text-[10px] text-slate-400 font-bold mt-0.5">
                        Paciente
                      </p>
                    </div>

                  </div>
                </td>

                <!-- DUI -->
                <td class="px-8 py-5">
                  <span class="font-mono text-sm font-bold text-slate-900 bg-sky-200 px-3 py-1.5 rounded-lg">
                    {{ paciente.dui ?? 'Sin DUI' }}
                  </span>
                </td>

                <!-- ESTADO -->
                <td class="px-8 py-5">

                  <button
                    @click="toggleEstado(paciente)"
                    :class="paciente.activo
                      ? 'bg-green-400 text-emerald-700 border-emerald-200'
                      : 'bg-red-300 text-red-700 border-red-200'"
                    class="px-3 py-1.5 rounded-full text-[10px] font-black uppercase tracking-widest border hover:scale-105 transition-all cursor-pointer"
                  >
                    {{ paciente.activo ? 'Activo' : 'Inactivo' }}
                  </button>

                </td>

                <!-- ACCIONES -->
                <td class="px-8 py-5 text-center">

                  <div class="flex items-center justify-center gap-2">

                    <!-- INFO -->
                    <button
                      @click="abrirInformacion(paciente)"
                      class="h-10 px-5 bg-sky-200 hover:bg-sky-400 text-slate-900 rounded-xl text-xs font-black transition-all uppercase tracking-widest cursor-pointer flex items-center justify-center gap-2"
                    >
                      <EyeIcon class="w-4 h-4" />
                      Info
                    </button>
                    <button
                      @click="abrirResumen(paciente)"
                      class="h-10 px-5 bg-teal-500/20 hover:bg-teal-500 text-teal-300 hover:text-white rounded-xl text-xs font-black transition-all uppercase tracking-widest cursor-pointer flex items-center justify-center gap-2"
                    >
                      <ClockIcon class="w-4 h-4" />
                      Resumen
                    </button>
                    <!-- ELIMINAR -->
                    <button
                      @click="eliminarPaciente(paciente)"
                      class="h-10 w-10 flex items-center justify-center bg-red-400/10 text-red-400 rounded-xl hover:bg-red-400 hover:text-white transition-all border border-red-400/20 cursor-pointer"
                    >
                      <TrashIcon class="w-4 h-4" />
                    </button>

                  </div>

                </td>

              </tr>

            </tbody>

          </table>

        </div>
      </div>

      <!-- MOBILE -->
      <div class="md:hidden space-y-3">

        <div
          v-for="paciente in pacientesFiltrados"
          :key="paciente.id"
          class="bg-white dark:bg-[#10284f] p-4 rounded-2xl shadow-sm border border-slate-100 dark:border-cyan-500/10"
        >

          <div class="flex items-center gap-3">

            <div class="w-12 h-12 shrink-0 rounded-xl overflow-hidden border border-cyan-400/20 bg-slate-800">

              <!-- FOTO -->
              <img
                v-if="paciente.fotoUrl || paciente.FotoUrl"
                :src="paciente.fotoUrl || paciente.FotoUrl"
                class="w-full h-full object-cover"
              />

              <!-- ICONO -->
              <img
                v-else-if="obtenerIconoPaciente(paciente)"
                :src="obtenerIconoPaciente(paciente)"
                class="w-full h-full object-cover"
              />

              <!-- FALLBACK -->
              <div
                v-else
                class="w-full h-full bg-teal-300 flex items-center justify-center font-bold text-slate-900 text-sm"
              >
                {{ paciente.nombre?.[0] }}{{ paciente.apellido?.[0] }}
              </div>

            </div>

            <div class="flex-1 min-w-0">

              <div class="flex justify-between items-start gap-2">

                <p class="font-black text-slate-800 dark:text-white text-base leading-tight truncate">
                  {{ paciente.nombre }} {{ paciente.apellido }}
                </p>

                <span
                  :class="paciente.activo
                    ? 'bg-emerald-100 text-emerald-700'
                    : 'bg-red-100 text-red-700'"
                  class="shrink-0 px-2.5 py-1 rounded-full text-[8px] font-black uppercase tracking-widest"
                >
                  {{ paciente.activo ? 'Activo' : 'Inactivo' }}
                </span>

              </div>

              <p class="text-[10px] text-slate-400 font-bold truncate mt-0.5">
                Paciente
              </p>

            </div>

          </div>

          <!-- DUI -->
          <div class="flex items-center justify-between bg-sky-50 dark:bg-slate-800 px-3 py-2 rounded-xl mt-3">

            <span class="text-[9px] font-black text-slate-400 uppercase">
              DUI
            </span>

            <span class="font-mono text-xs font-bold text-slate-600 dark:text-slate-200">
              {{ paciente.dui ?? 'Sin DUI' }}
            </span>

          </div>

          <!-- BOTONES -->
          <div class="grid grid-cols-2 gap-2 mt-3">

            <button
              @click="abrirInformacion(paciente)"
              class="py-3 bg-sky-200 text-slate-900 rounded-xl text-[10px] font-black uppercase tracking-widest hover:bg-sky-300 transition-all flex items-center justify-center gap-2"
            >
              <EyeIcon class="w-4 h-4" />
              Info
            </button>
            <button
              @click="abrirResumen(paciente)"
              class="py-3 bg-teal-500/20 text-teal-300 rounded-xl text-[10px] font-black uppercase tracking-widest hover:bg-teal-500 hover:text-white transition-all flex items-center justify-center gap-2"
            >
              <ClockIcon class="w-4 h-4" />
              Resumen
            </button>


            <button
              @click="toggleEstado(paciente)"
              :class="paciente.activo
                ? 'bg-red-50 text-red-600 border border-red-100'
                : 'bg-emerald-50 text-emerald-600 border border-emerald-100'"
              class="py-3 rounded-xl text-[9px] font-black uppercase tracking-widest transition-colors"
            >
              {{ paciente.activo ? 'Inactivar' : 'Activar' }}
            </button>

            <button
              @click="eliminarPaciente(paciente)"
              class="py-3 bg-red-50 text-red-600 border border-red-100 rounded-xl text-[9px] font-black uppercase tracking-widest transition-colors flex items-center justify-center gap-2"
            >
              <TrashIcon class="w-4 h-4" />
              Eliminar
            </button>

          </div>

        </div>

      </div>

      <!-- VACÍO -->
      <div
        v-if="!cargando && !error && pacientesFiltrados.length === 0"
        class="py-16 text-center"
      >

        <div class="flex justify-center mb-4 opacity-20">
          <UserGroupIcon class="w-14 h-14 text-slate-400" />
        </div>

        <p class="text-slate-500 dark:text-slate-400 font-bold text-sm">
          No se encontraron pacientes.
        </p>

      </div>

    </div>
  </div>

  <!-- MODALES -->

  <configuracionCuenta
    v-if="accederInformacionPaciente"
    :usuarioData="pacienteSeleccionado"
    :rolSesion="pacienteSeleccionado?.rol ?? 'paciente'"
    :modoAdmin="true"
    :pacienteId="pacienteSeleccionado?.id"
    @cerrar="accederInformacionPaciente = false"
  />

      <ResumenPaciente
      v-if="accederResumen"
      :paciente="pacienteResumenId"
      @cerrar="accederResumen = false"
    />
    <!-- TOAST -->
  <Transition
    enter-active-class="transition-all duration-300 ease-out"
    enter-from-class="opacity-0 translate-y-4 scale-95"
    enter-to-class="opacity-100 translate-y-0 scale-100"
    leave-active-class="transition-all duration-200 ease-in"
    leave-from-class="opacity-100 translate-y-0 scale-100"
    leave-to-class="opacity-0 translate-y-4 scale-95"
  >
    <div
      v-if="toast.visible"
      class="fixed bottom-6 right-6 z-[99999] flex items-center gap-3 px-5 py-4 rounded-2xl shadow-2xl border backdrop-blur-sm min-w-[280px] max-w-sm"
      :class="toast.tipo === 'success'
        ? 'bg-emerald-950/90 border-emerald-500/30 text-emerald-300'
        : 'bg-red-950/90 border-red-500/30 text-red-300'"
    >
      <div class="shrink-0 w-8 h-8 rounded-xl flex items-center justify-center"
        :class="toast.tipo === 'success' ? 'bg-emerald-500/20' : 'bg-red-500/20'">
        <svg v-if="toast.tipo === 'success'" xmlns="http://www.w3.org/2000/svg" class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2.5">
          <path stroke-linecap="round" stroke-linejoin="round" d="M5 13l4 4L19 7" />
        </svg>
        <svg v-else xmlns="http://www.w3.org/2000/svg" class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2.5">
          <path stroke-linecap="round" stroke-linejoin="round" d="M6 18L18 6M6 6l12 12" />
        </svg>
      </div>
      <div class="flex-1">
        <p class="text-[11px] font-black uppercase tracking-widest mb-0.5"
          :class="toast.tipo === 'success' ? 'text-emerald-400' : 'text-red-400'">
          {{ toast.tipo === 'success' ? 'Éxito' : 'Error' }}
        </p>
        <p class="text-xs font-semibold leading-snug">{{ toast.mensaje }}</p>
      </div>
      <button @click="toast.visible = false"
        class="shrink-0 w-6 h-6 rounded-lg flex items-center justify-center opacity-50 hover:opacity-100 transition-opacity"
        :class="toast.tipo === 'success' ? 'hover:bg-emerald-500/20' : 'hover:bg-red-500/20'">
        <XMarkIcon class="w-3.5 h-3.5" />
      </button>
      <div class="absolute bottom-0 left-0 h-[2px] rounded-full w-full overflow-hidden">
        <div class="h-full rounded-full"
          :class="toast.tipo === 'success' ? 'bg-emerald-400' : 'bg-red-400'"
          style="animation: shrink 3.5s linear forwards" />
      </div>
    </div>
  </Transition>

  <!-- MODAL CONFIRMACIÓN -->
  <Transition
    enter-active-class="transition-all duration-200 ease-out"
    enter-from-class="opacity-0"
    enter-to-class="opacity-100"
    leave-active-class="transition-all duration-150 ease-in"
    leave-from-class="opacity-100"
    leave-to-class="opacity-0"
  >
    <div
      v-if="confirmModal.visible"
      class="fixed inset-0 z-[99998] flex items-center justify-center bg-slate-950/80 backdrop-blur-sm p-4"
      @click.self="responderConfirm(false)"
    >
      <Transition
        enter-active-class="transition-all duration-200 ease-out"
        enter-from-class="opacity-0 scale-95 -translate-y-2"
        enter-to-class="opacity-100 scale-100 translate-y-0"
      >
        <div class="bg-gradient-to-br from-[#0f2040] to-[#10284f] border border-cyan-500/20 rounded-2xl shadow-2xl w-full max-w-sm p-6 flex flex-col gap-5">

          <!-- Ícono + título -->
          <div class="flex items-center gap-3">
            <div class="w-10 h-10 rounded-xl bg-amber-500/15 flex items-center justify-center shrink-0">
              <svg xmlns="http://www.w3.org/2000/svg" class="w-5 h-5 text-amber-400" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
                <path stroke-linecap="round" stroke-linejoin="round" d="M12 9v4m0 4h.01M10.29 3.86L1.82 18a2 2 0 001.71 3h16.94a2 2 0 001.71-3L13.71 3.86a2 2 0 00-3.42 0z" />
              </svg>
            </div>
            <div>
              <p class="text-[10px] font-black text-amber-400 uppercase tracking-widest">Confirmación</p>
              <p class="text-white font-black text-base leading-tight">{{ confirmModal.titulo }}</p>
            </div>
          </div>

          <!-- Mensaje -->
          <p class="text-slate-300 text-sm font-semibold leading-relaxed px-1">
            {{ confirmModal.mensaje }}
          </p>

          <!-- Botones -->
          <div class="flex gap-3">
            <button
              @click="responderConfirm(false)"
              class="flex-1 py-3 rounded-xl bg-slate-700 hover:bg-slate-600 text-slate-300 font-black text-[10px] uppercase tracking-widest transition-colors"
            >
              Cancelar
            </button>
            <button
              @click="responderConfirm(true)"
              class="flex-[2] py-3 rounded-xl bg-amber-500 hover:bg-amber-400 text-slate-900 font-black text-[10px] uppercase tracking-widest transition-colors"
            >
              Confirmar
            </button>
          </div>

        </div>
      </Transition>
    </div>
  </Transition>
</template>
<style scoped>
@keyframes shrink {
  from { width: 100%; }
  to   { width: 0%; }
}
</style>
