///ESTA ES LA SECCION  ( CONTIENE EL CALENDARIO Y LOS STATCARDS)
// Y ABAJO UNA SECCION PARA VER LOS DETALLES DE LA CONSULTA QUE SELECCION DESDE EL CALENDARIO

<script setup>

import { ref, computed, onMounted } from 'vue'
import Calendar from '@/componentsDoctor/calendar/Calendar.vue'
import StatCard from '@/componentsDoctor/ui/StatsCard.vue'
import { useRouter } from 'vue-router'
import { useCitasStore } from '@/stores/citas'
import { useVideoStore } from '@/stores/videoStore'
import {
  CalendarDaysIcon,
  ClockIcon,
  UserPlusIcon,
  CheckCircleIcon,
  XCircleIcon,
   ClipboardDocumentCheckIcon,
   VideoCameraIcon,
} from '@heroicons/vue/24/outline'

const router = useRouter()
const citasStore = useCitasStore()
const videoStore = useVideoStore()

const citas   = computed(() => citasStore.citas)
const resumen = computed(() => citasStore.resumen)
const selectedCita = ref(null)

// Toast
const toast = ref({ visible: false, mensaje: '', tipo: 'success' })
let toastTimer = null
const mostrarToast = (mensaje, tipo = 'success') => {
  if (toastTimer) clearTimeout(toastTimer)
  toast.value = { visible: true, mensaje, tipo }
  toastTimer = setTimeout(() => { toast.value.visible = false }, 3500)
}

// ─── CARGAR TODO AL MONTAR
onMounted(async () => {
  await Promise.all([
    citasStore.cargarCitas(),
    citasStore.cargarResumen()
  ])
})


function prepararConsulta(cita) {
  if (!cita.linkReunion) {
    mostrarToast('Esta cita no tiene enlace de videollamada configurado.', 'error')
    return
  }

  const roomName = cita.linkReunion.split('#')[0].split('?')[0].split('/').pop()

  videoStore.iniciarLlamada(roomName, cita.idCita)

  router.push({ name: 'Consulta', params: { id: cita.idCita } })
}

function seleccionarCita(citaDelCalendario) {

  const citaEncontrada = citasStore.citas.find(c => c.idCita === citaDelCalendario.idCita);

  if (citaEncontrada) {
    selectedCita.value = citaEncontrada;
  } else {
    // Si no la encuentra por idCita, intenta por id (a veces el calendario usa 'id')
    selectedCita.value = citasStore.citas.find(c => c.id === citaDelCalendario.id);
  }
}
</script>

<template>
  <!-- Contenedor Principal -->
  <div
  class="space-y-6 w-full min-h-screen p-6
         bg-slate-100 dark:bg-[#020817]
         transition-colors duration-300"
        >

        <!-- Titulo-->
        <div class="border-b border-slate-200 dark:border-slate-800 pb-4">
          <h1 class="text-2xl font-black tracking-tight text-slate-800 dark:text-white">
            Agenda Médica
          </h1>

          <p class="text-sm text-slate-500 dark:text-slate-400 mt-1">
            Gestión de citas y consultas del día
          </p>
        </div>

    <div class="grid grid-cols-1 lg:grid-cols-4 gap-6 w-full">

      <!-- ── STATCARDS -->
      <div class="space-y-4 lg:col-span-1">
        <StatCard
            title="Citas hoy"
            :value="resumen.hoy"
            color="text-sky-500"
            :icon="CalendarDaysIcon"
          />

          <StatCard
            title="Pendientes"
            :value="resumen.pendientes"
            color="text-amber-500"
            :icon="ClockIcon"
          />

          <StatCard
            title="En consulta"
            :value="resumen.enConsulta"
            color="text-teal-500"
            :icon="UserPlusIcon"
          />

          <StatCard
            title="Finalizadas"
            :value="resumen.finalizadas"
            color="text-emerald-500"
            :icon="CheckCircleIcon"
          />

          <StatCard
            title="Canceladas"
            :value="resumen.canceladas"
            color="text-rose-500"
            :icon="XCircleIcon"
          />
      </div>

      <!-- ── CALENDARIO  -->
     <div class="lg:col-span-3">
        <div
          v-if="citasStore.cargando"
          class="text-sm font-medium text-slate-400 dark:text-slate-500"
        >
          Cargando citas...
        </div>

        <Calendar
          v-else
          :key="citas.length"
          :citas="citas"
          @cita-click="seleccionarCita"
        />
      </div>

    <!--Panel Detalles de cita seleccionada-->
      <div
          class="p-6 rounded-2xl border border-slate-700/30 dark:border-slate-700/40 shadow-2xl mt-3 lg:col-span-4 backdrop-blur-sm overflow-hidden"
          style="background: linear-gradient(135deg, #0f172a 0%, #132238 45%, #0b3b5c 100%);"
        >
          <!-- Sin cita seleccionada -->
          <div
            v-if="!selectedCita"
            class="flex flex-col items-center justify-center py-8 text-center"
          >
           <div
              class="w-14 h-14 rounded-2xl
              bg-slate-100 dark:bg-slate-800/60
              border border-slate-600 dark:border-slate-700/50
              flex items-center justify-center mb-4 shadow-inner"
            >
              <CalendarDaysIcon class="w-7 h-7 text-teal-800 dark:text-cyan-300" />
            </div>


            <p class="text-sm font-semibold tracking-wide text-slate-300">
              Ninguna cita seleccionada
            </p>


            <p class="text-xs text-slate-500 mt-1">
              Seleccioná una cita en el calendario para visualizar los detalles.
            </p>
          </div>


          <!-- Con cita seleccionada -->
          <div v-else>


            <!-- Header -->
            <div
              class="flex items-center justify-between mb-6 flex-wrap gap-3 pb-4 border-b border-slate-700/30"
            >
              <div class="flex items-center gap-3">
                <div
                  class="w-10 h-10 rounded-xl bg-cyan-500/10 border border-cyan-400/20 flex items-center justify-center shadow-md"
                >
                  <ClipboardDocumentCheckIcon
                    class="w-5 h-5 text-cyan-400 group-hover:scale-110 transition-transform duration-300"
                  />
                </div>


                <div>
                  <p class="text-[11px] uppercase tracking-[0.2em] text-slate-500 font-black">
                    Consulta médica
                  </p>


                  <h3 class="text-base md:text-lg font-bold text-white tracking-tight">
                    Detalle de la cita
                  </h3>
                </div>
              </div>


              <!-- Estado -->
              <span
                class="text-[11px] font-black uppercase tracking-widest px-4 py-2 rounded-full border shadow-lg backdrop-blur-md"
                :class="{
                  'bg-yellow-500/10 text-yellow-300 border-yellow-500/20':
                    selectedCita.estado === 'Pendiente',


                  'bg-emerald-500/15 text-emerald-300 border-emerald-500/30':
                    selectedCita.estado === 'EnConsulta',


                  'bg-slate-500/10 text-slate-300 border-slate-500/20':
                    selectedCita.estado === 'Finalizada',


                  'bg-red-500/15 text-red-300 border-red-500/30':
                    selectedCita.estado === 'Cancelada',


                  'bg-orange-500/15 text-orange-300 border-orange-500/30':
                    selectedCita.estado === 'NoAsistida',
                }"
              >
                {{
                  selectedCita.estado === 'EnConsulta'
                    ? 'En consulta'
                    : selectedCita.estado === 'NoAsistida'
                      ? 'No asistió'
                      : selectedCita.estado
                }}
              </span>
            </div>

            <!-- GRID -->
            <div
              class="grid grid-cols-1 sm:grid-cols-2 xl:grid-cols-5 gap-5"
            >

              <!-- Paciente -->
              <div
                class="bg-slate-900/30 border border-slate-700/30 rounded-xl p-4 hover:border-teal-400/30 hover:bg-slate-800/30 transition-all"
              >
                <p
                  class="text-[10px] font-black uppercase tracking-[0.2em] text-slate-500 mb-2"
                >
                  Paciente
                </p>

                <p class="text-sm font-semibold text-white leading-relaxed">
                  {{ selectedCita.pacienteNombreCompleto }}
                </p>
              </div>

              <!-- DUI -->
              <div
                class="bg-slate-900/30 border border-slate-700/30 rounded-xl p-4 hover:border-teal-400/30 hover:bg-slate-800/30 transition-all"
              >
                <p
                  class="text-[10px] font-black uppercase tracking-[0.2em] text-slate-500 mb-2"
                >
                  DUI
                </p>

                <p class="text-sm font-semibold text-slate-100">
                  {{ selectedCita.duiPaciente }}
                </p>
              </div>

              <!-- Teléfono -->
              <div
                class="bg-slate-900/30 border border-slate-700/30 rounded-xl p-4 hover:border-teal-400/30 hover:bg-slate-800/30 transition-all"
              >
                <p
                  class="text-[10px] font-black uppercase tracking-[0.2em] text-slate-500 mb-2"
                >
                  Teléfono
                </p>

                <p class="text-sm font-semibold text-slate-100">
                  {{ selectedCita.telefonoPaciente }}
                </p>
              </div>

              <!-- Hora -->
              <div
                class="bg-slate-900/30 border border-slate-700/30 rounded-xl p-4 hover:border-teal-400/30 hover:bg-slate-800/30 transition-all"
              >
                <p
                  class="text-[10px] font-black uppercase tracking-[0.2em] text-slate-500 mb-2"
                >
                  Hora de la cita
                </p>

                <p class="text-sm font-semibold text-cyan-300">
                  {{
                    new Date(selectedCita.start).toLocaleTimeString([], {
                      hour: '2-digit',
                      minute: '2-digit'
                    })
                  }}
                </p>
              </div>

              <!-- Tipo -->
              <div
                class="bg-slate-900/30 border border-slate-700/30 rounded-xl p-4 hover:border-teal-400/30 hover:bg-slate-800/30 transition-all"
              >
                <p
                  class="text-[10px] font-black uppercase tracking-[0.2em] text-slate-500 mb-2"
                >
                  Tipo de consulta
                </p>

                <p class="text-sm font-semibold text-white">
                  {{ selectedCita.tipoConsulta }}
                </p>
              </div>

              <!-- Motivo -->
              <div
                class="sm:col-span-2 xl:col-span-3 bg-slate-900/30 border border-slate-700/30 rounded-xl p-4 hover:border-teal-400/30 hover:bg-slate-800/30 transition-all"
              >
                <p
                  class="text-[10px] font-black uppercase tracking-[0.2em] text-slate-500 mb-2"
                >
                  Motivo de consulta
                </p>

                <p class="text-sm text-slate-200 leading-relaxed">
                  {{ selectedCita.titulo }}
                </p>
              </div>

              <!-- Acción -->
              <div
                class="sm:col-span-2 xl:col-span-2 flex items-end justify-end"
              >
                <button
                  v-if="selectedCita.estado === 'Pendiente' || selectedCita.estado === 'EnConsulta'"
                  @click="prepararConsulta(selectedCita)"
                  class="w-full xl:w-auto flex items-center justify-center gap-2 bg-cyan-500 hover:bg-cyan-400 hover:shadow-cyan-400/30 hover:-translate-y-0.5 text-slate-900 px-6 py-3 rounded-xl text-sm font-black uppercase tracking-wider transition-all duration-300 cursor-pointer shadow-lg shadow-cyan-500/20 hover:scale-[1.02]"
                >
                 <VideoCameraIcon class="w-5 h-5" />
                  Entrar a consulta
                </button>

                <span
                  v-else
                  class="text-xs font-semibold uppercase tracking-widest text-slate-500"
                >
                  Sin acciones disponibles
                </span>
              </div>
            </div>
          </div>
        </div>
      </div>

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
        <div
          class="shrink-0 w-8 h-8 rounded-xl flex items-center justify-center"
          :class="toast.tipo === 'success' ? 'bg-emerald-500/20' : 'bg-red-500/20'"
        >
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

        <button
          @click="toast.visible = false"
          class="shrink-0 w-6 h-6 rounded-lg flex items-center justify-center opacity-50 hover:opacity-100 transition-opacity"
          :class="toast.tipo === 'success' ? 'hover:bg-emerald-500/20' : 'hover:bg-red-500/20'"
        >
          &times;
        </button>

        <div class="absolute bottom-0 left-0 h-[2px] rounded-full w-full overflow-hidden">
          <div
            class="h-full rounded-full"
            :class="toast.tipo === 'success' ? 'bg-emerald-400' : 'bg-red-400'"
            style="animation: shrink 3.5s linear forwards"
          />
        </div>
        
      </div>
    </Transition>

  </div>
</template>

<style scoped>
@keyframes shrink {
  from { width: 100%; }
  to   { width: 0%; }
}
</style>
