<script setup>
import { ref, onMounted, onUnmounted, nextTick } from 'vue'

import { getCalendario } from '@/services/api'
import { logoutPro } from '@/services/api'
import api from '@/services/api'
import { getProfileIcon } from '@/utils/getProfileIcon'

import FullCalendar from '@fullcalendar/vue3'
import dayGridPlugin from '@fullcalendar/daygrid'
import interactionPlugin from '@fullcalendar/interaction'
import esLocale from '@fullcalendar/core/locales/es'

// Componentes
import configuracionCuenta from '../components/configuracionCuenta.vue'
import tarjetaAgendarCitas from '../components/tarjetaAgendarCitas.vue'
import tarjetaVerMisCitas from '../components/tarjetaVerMisCitas.vue'
import MisExamenes from '@/components/MisExamenes.vue'
import soporteAyuda from '@/components/soporteAyuda.vue'
import modalDetalleCita from '@/components/modalDetalleCita.vue'

const calendarRef = ref(null)
const calendarioListo = ref(false) //controla cuándo montar FullCalendar

// ===============================
// ASPECT RATIO DINÁMICO
const getAspectRatio = () => window.innerWidth < 768 ? 1.0 : 2.25

const actualizarAspectRatio = () => {
  calendarOptions.value = {
    ...calendarOptions.value,
    aspectRatio: getAspectRatio()
  }
  nextTick(() => {
    calendarRef.value?.getApi()?.updateSize()
  })
}

// ===============================
// CALENDARIO CONFIG
const calendarOptions = ref({
  plugins: [dayGridPlugin, interactionPlugin],
  initialView: 'dayGridMonth',
  locale: esLocale,
  aspectRatio: getAspectRatio(),
  headerToolbar: {
    left: 'prev,next today',
    center: 'title',
    right: 'dayGridMonth',
  },
  titleFormat: { month: 'long', year: 'numeric' },
  editable: false,
  selectable: false,
  events: [],
  windowResize: () => {
    actualizarAspectRatio()
  },
  eventClick: (info) => {
    const { estado, doctorNombre, motivo, fechaInicio, paciente } = info.event.extendedProps
    const fecha = new Date(fechaInicio)

    citaSeleccionada.value = {
      paciente,
      doctor:   doctorNombre,
      estado,
      motivo,
      fecha: fecha.toLocaleDateString('es-SV', {
              weekday: 'long', year: 'numeric', month: 'long', day: 'numeric'
            }),
      hora:  fecha.toLocaleTimeString('es-SV', {
              hour: '2-digit', minute: '2-digit'
            })
    }
  }
})

// ===============================
// CARGAR CALENDARIO
const cargarCalendario = async () => {
  try {
    const data = await getCalendario()

    const eventos = data.map(cita => ({
      id: cita.idCita,
      title: `${cita.pacienteNombreCompleto}`,
      motivo: `${cita.motivo}`,
      start: cita.start,
      end: cita.end,
      extendedProps: {
        estado: cita.estado,
        paciente: cita.pacienteNombreCompleto,
        doctorNombre: cita.doctorNombreCompleto,
        motivo:       cita.titulo,
        fechaInicio:  cita.start
      },
      color:
        cita.estado === 'Pendiente'  ? '#facc15' :
        cita.estado === 'Confirmada' ? '#22c55e' :
        cita.estado === 'NoAsistida'  ? '#f97316' :
        cita.estado === 'Cancelada'  ? '#ef4444' : '#3b82f6'

    }))

    calendarOptions.value = {
      ...calendarOptions.value,
      events: eventos
    }

    await nextTick()
    const calApi = calendarRef.value?.getApi()
    if (calApi) {
      calApi.removeAllEvents()
      calApi.addEventSource(eventos)
    }

  } catch (_) {}
}


// ===============================
// PERFIL
const usuario = ref(null)
const nombreUsuario = ref('Cargando...')
const fotoPerfil = ref('')
const iconoPerfil = ref('')

const cargarPerfil = async () => {
  try {
    const res = await api.get('/Users/me')
    const data = res.data

    usuario.value = data

    const nombre = data.nombre || data.Nombre || ''
    const apellido = data.apellido || data.Apellido || ''
    const nombreCompleto = `${nombre} ${apellido}`.trim() || 'Usuario'

    nombreUsuario.value = nombreCompleto

    const iconoGuardado =
      data.avatarIcono ||
      data.AvatarIcono ||
      ''

    iconoPerfil.value = getProfileIcon(iconoGuardado)

    fotoPerfil.value =
      !iconoGuardado
        ? (data.fotoUrl || data.FotoUrl || '')
        : ''

localStorage.setItem('user_name', nombreCompleto)

  } catch (error) {
    nombreUsuario.value = localStorage.getItem('user_name') || 'Paciente'
  }
}

// ===============================
// LIFECYCLE
onMounted(async () => {
  await Promise.all([
    cargarPerfil(),
    cargarCalendario()
  ])

  await nextTick()


  setTimeout(async () => {
    calendarioListo.value = true
    await nextTick()
    calendarRef.value?.getApi()?.updateSize()
  }, 100)

  window.addEventListener('resize', actualizarAspectRatio)
})

onUnmounted(() => {
  window.removeEventListener('resize', actualizarAspectRatio)
})

// ===============================
// MODALES
const accederCita = ref(false)
const accederVerMisCitas = ref(false)
const accederSoporteAyuda = ref(false)
const MisExamenesVisible = ref(false)
const mostrarInformacion = ref(false)
const citaSeleccionada = ref(null)
const mostrarConfirmLogout = ref(false)

// Modo Oscuro
const isDark = ref(
  document.documentElement.classList.contains('dark')
)

const toggleDark = () => {
  isDark.value = !isDark.value

  document.documentElement.classList.toggle('dark')

  localStorage.setItem('darkMode', isDark.value)
}

// ===============================
// LOGOUT
const cerrarSesion = () => {
 mostrarConfirmLogout.value = true
}

const confirmarLogout = async () => {
  mostrarConfirmLogout.value = false
  await logoutPro()
}

</script>

<template>
  <div
    :class="[
      'flex flex-col md:flex-row min-h-screen font-sans transition-colors duration-300',
      isDark
        ? 'bg-[#020617] text-slate-200'
        : 'bg-slate-100 text-slate-700'
    ]"
  >

    <!-- SIDEBAR -->
    <aside
      :class="[
        'w-full md:w-64 flex flex-col p-3 md:p-5 shadow-lg shrink-0 transition-colors duration-300',
        isDark
          ? 'bg-gradient-to-b from-[#020617] via-[#081225] to-[#0f172a] text-white border-r border-cyan-500/10'
          : 'bg-slate-950 text-white'
      ]"
    >

      <div class="mb-4 md:mb-10 text-center">

        <img
          src="@/assets/Logo icon.png"
          alt="TelMed Lite™"
          class="w-16 h-16 md:w-32 md:h-32 object-contain mx-auto mb-2 drop-shadow-[0_0_12px_rgba(34,211,238,0.4)]"
        />

        <div class="text-xs md:text-sm font-bold tracking-wider">
          TelMed <span class="text-cyan-400"> Lite™</span>
        </div>

        <div class="text-sm border-t border-white/10 pt-3 md:pt-4">

          <p class="font-semibold text-cyan-400">
            Portal Paciente
          </p>

          <div v-if="usuario" class="flex justify-center my-2 md:my-3">

              <!-- FOTO -->
              <img
                v-if="fotoPerfil"
                :src="fotoPerfil"
                @error="fotoPerfil = ''"
                class="w-12 h-12 md:w-16 md:h-16 rounded-full border-2 border-cyan-400 object-cover"
              />

              <!-- ICONO -->
              <img
                v-else-if="iconoPerfil"
                :src="iconoPerfil"
                class="w-12 h-12 md:w-16 md:h-16 rounded-2xl object-cover bg-slate-800/70 p-1 border border-cyan-400/30 shadow-lg"
              />

              <!-- FALLBACK -->
              <div
                v-else
                class="w-12 h-12 md:w-16 md:h-16 rounded-full bg-cyan-600 flex items-center justify-center text-lg md:text-xl font-bold text-white border-2 border-cyan-400/50"
              >
                {{ usuario.nombre?.[0] || 'U' }}{{ usuario.apellido?.[0] || '' }}
              </div>

            </div>

          <p class="text-xs text-slate-400 uppercase tracking-widest">
            ¡Bienvenido!
          </p>

          <h2 class="text-base md:text-lg font-bold text-white truncate px-2">
            {{ nombreUsuario }}
          </h2>

        </div>

      </div>

      <!-- NAV -->
      <nav class="grid grid-cols-3 md:grid-cols-1 md:flex md:flex-col gap-1 md:gap-2">

        <button
          @click="accederCita = true"
          class="border-t border-white/10 pt-3 md:pt-4 flex flex-col md:flex-row items-center gap-1 md:gap-3 px-2 py-2 md:px-4 md:py-3 rounded-lg w-full
           text-center md:text-left transition-all hover:bg-cyan-500/60 hover:md:translate-x-1 cursor-pointer"
        >
          <i class="fa-solid fa-calendar-plus text-base md:text-lg text-emerald-400"></i>

          <span class="text-[11px] md:text-base font-medium leading-tight">
            Nueva cita
          </span>
        </button>

        <button
          @click="accederVerMisCitas = true"
          class="border-t border-white/10 pt-3 md:pt-4 flex flex-col md:flex-row items-center gap-1 md:gap-3 px-2 py-2 md:px-4 md:py-3 rounded-lg w-full
           text-center md:text-left transition-all hover:bg-cyan-500/60 hover:md:translate-x-1 cursor-pointer"
        >
          <i class="fa-solid fa-clipboard-list text-base md:text-lg text-emerald-400"></i>

          <span class="text-[11px] md:text-base font-medium leading-tight">
            Ver mis citas
          </span>
        </button>

        <button
          @click="MisExamenesVisible = true"
          class="border-t border-white/10 pt-3 md:pt-4 flex flex-col md:flex-row items-center gap-1 md:gap-3 px-2 py-2
           md:px-4 md:py-3 rounded-lg w-full text-center md:text-left transition-all hover:bg-cyan-500/60 hover:md:translate-x-1 cursor-pointer"
        >
          <i class="fa-solid fa-flask text-base md:text-lg text-emerald-400"></i>

          <span class="text-[11px] md:text-base font-medium leading-tight">
            Mis exámenes
          </span>
        </button>

        <button
          @click="accederSoporteAyuda = !accederSoporteAyuda"
          class="border-t border-white/10 pt-3 md:pt-4 flex flex-col md:flex-row items-center gap-1 md:gap-3 px-2 py-2 md:px-4 md:py-3
           rounded-lg w-full text-center md:text-left transition-all hover:bg-cyan-500/60 hover:md:translate-x-1 cursor-pointer"
        >
          <i class="fa-solid fa-circle-question text-base md:text-lg text-emerald-400"></i>

          <span class="text-[11px] md:text-base font-medium leading-tight">
            Soporte/ayuda
          </span>
        </button>

        <button
          @click="mostrarInformacion = true"
          class="border-t border-white/10 pt-3 md:pt-4 flex flex-col md:flex-row items-center gap-1 md:gap-3 px-2 py-2 md:px-4 md:py-3
          rounded-lg w-full text-center md:text-left transition-all hover:bg-cyan-500/60 hover:md:translate-x-1 cursor-pointer"
        >
          <i class="fa-solid fa-user-gear text-base md:text-lg text-emerald-400"></i>

          <span class="text-[11px] md:text-base font-medium leading-tight">
            Mi cuenta
          </span>
        </button>

        <!-- TOGGLE DARK MODE -->
        <button
          @click="toggleDark"
          class="border-t border-white/10 pt-3 md:pt-4 flex flex-col md:flex-row items-center gap-1 md:gap-3 px-2 py-2 md:px-4 md:py-3
          rounded-lg w-full text-center md:text-left transition-all hover:bg-indigo-500/40 hover:md:translate-x-1 cursor-pointer"
        >
          <i
            :class="[
              'text-base md:text-lg',
              isDark
                ? 'fa-solid fa-sun text-amber-300'
                : 'fa-solid fa-moon text-indigo-300'
            ]"
          ></i>

          <span class="text-[11px] md:text-base font-medium leading-tight">
            {{ isDark ? 'Modo claro' : 'Modo oscuro' }}
          </span>
        </button>

        <button
          @click="cerrarSesion"
          class="border-t border-white/10 pt-3 md:pt-4 flex flex-col md:flex-row items-center gap-1 md:gap-3 px-2 py-2 md:px-4 md:py-3
          rounded-lg w-full text-center md:text-left transition-all hover:bg-red-500/40 hover:md:translate-x-1 col-span-3 md:col-span-1 cursor-pointer"
        >
          <i class="fa-solid fa-power-off text-base md:text-lg text-red-400"></i>

          <span class="text-[11px] md:text-base font-medium leading-tight text-red-400">
            Cerrar Sesión
          </span>
        </button>
      </nav>
    </aside>

    <!-- MAIN -->
    <main class="flex-grow p-3 md:p-8">

      <header class="flex justify-between items-end mb-10">

        <div>

          <h1
            :class="[
              'text-3xl font-extrabold tracking-tight transition-colors',
              isDark ? 'text-white' : 'text-slate-900'
            ]"
          >
            Mi calendario
          </h1>

          <p
            :class="[
              'mt-1 font-medium transition-colors',
              isDark ? 'text-slate-400' : 'text-slate-500'
            ]"
          >
            Revisa el estado de tus citas en el siguiente calendario
          </p>

        </div>
      </header>

      <!-- CALENDARIO -->
      <section
        :class="[
          'rounded-2xl p-3 md:p-6 overflow-x-auto transition-colors duration-300',
          isDark
            ? 'bg-[#0f172a] border border-cyan-500/10 shadow-xl'
            : 'bg-white border shadow-sm'
        ]"
      >

        <FullCalendar
          v-if="calendarioListo"
          ref="calendarRef"
          :options="calendarOptions"
        />

      </section>

      <!-- MODALES -->
      <div
        v-if="accederCita"
        class="modal"
        @click.self="accederCita = false"
      >
        <tarjetaAgendarCitas
          :esSecretaria="false"
          @cerrar="() => { accederCita = false; cargarCalendario(); }"
        />
      </div>

      <div
        v-if="accederVerMisCitas"
        class="modal"
        @click.self="accederVerMisCitas = false"
      >
        <tarjetaVerMisCitas
          @cerrar="accederVerMisCitas = false"
        />
      </div>

      <div
        v-if="MisExamenesVisible"
        class="modal"
        @click.self="MisExamenesVisible = false"
      >
        <MisExamenes
          @cerrar="MisExamenesVisible = false"
        />
      </div>

      <Transition
        enter-active-class="transition-all duration-300 ease-out"
        enter-from-class="translate-x-full opacity-0"
        enter-to-class="translate-x-0 opacity-100"
        leave-active-class="transition-all duration-200 ease-in"
        leave-from-class="translate-x-0 opacity-100"
        leave-to-class="translate-x-full opacity-0"
      >

        <soporteAyuda
          v-if="accederSoporteAyuda"
          @cerrar="accederSoporteAyuda = false"
        />

      </Transition>

      <div
        v-if="mostrarInformacion"
        class="modal"
        @click.self="mostrarInformacion = false"
      >
        <configuracionCuenta
          :usuarioData="usuario"
          :rolSesion="usuario?.rol?.toLowerCase() || 'paciente'"
          @cerrar="mostrarInformacion = false"
          @actualizar="cargarPerfil"
        />
      </div>

      <!-- Modal detalle cita -->
      <modalDetalleCita
        :cita="citaSeleccionada"
        @cerrar="citaSeleccionada = null"
      />

    </main>

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
              <svg xmlns="http://www.w3.org/2000/svg" class="w-7 h-7 text-red-400" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="1.5">
                <path stroke-linecap="round" stroke-linejoin="round" d="M15.75 9V5.25A2.25 2.25 0 0013.5 3h-6a2.25 2.25 0 00-2.25 2.25v13.5A2.25 2.25 0 007.5 21h6a2.25 2.25 0 002.25-2.25V15M12 9l-3 3m0 0l3 3m-3-3h12.75" />
              </svg>
            </div>


            <div class="text-center">
              <h3 class="text-white font-black text-lg leading-tight">Cerrar sesión</h3>
              <p class="text-slate-400 text-sm font-medium mt-1.5">¿Deseas cerrar tu sesión de forma segura?</p>
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

  </div>
</template>

<style>
.fc-toolbar-title {
  font-size: clamp(0.85rem, 2.5vw, 1.25rem) !important;
}
.fc-event{
  cursor:pointer;
}

.fc-toolbar.fc-header-toolbar {
  flex-wrap: wrap;
  gap: 0.5rem;
}

.fc-daygrid-day-number,
.fc-col-header-cell-cushion {
  font-size: clamp(0.7rem, 2vw, 0.9rem);
}

/* ========================= */
/* DARK MODE */
/* ========================= */

.dark .fc {
  color: #e2e8f0;
}

/* Bordes */
.dark .fc-theme-standard td,
.dark .fc-theme-standard th {
  border-color: rgb(51 65 85);
}

/* Header */
.dark .fc-col-header-cell {
  background: rgb(15 23 42);
  color: white;
  padding: 0.5rem 0;
}

/* Fondo días */
.dark .fc-daygrid-day {
  background: rgb(15 23 42);
  transition: background 0.2s ease;
}

/* Hover */
.dark .fc-daygrid-day:hover {
  background: rgb(30 41 59);
}

/* Números */
.dark .fc-daygrid-day-number {
  color: #f8fafc;
  padding: 0.4rem;
  font-weight: 500;
}

/* Otros meses */
.dark .fc-day-other .fc-daygrid-day-number {
  color: rgb(100 116 139);
}

/* Título */
.dark .fc-toolbar-title {
  color: white;
}

/* Botones */
.dark .fc-button {
  background: rgb(51 65 85) !important;
  border: none !important;
  box-shadow: none !important;
}

/* Hover botones */
.dark .fc-button:hover {
  background: rgb(70 130 180) !important;
}

/* Activo */
.dark .fc-button-active {
  background: rgb(59 130 246) !important;
}

/* Eventos */
.dark .fc-event {
  background: linear-gradient(
    135deg,
    rgb(14 165 233),
    rgb(37 99 235)
  ) !important;

  border: none !important;
  border-radius: 0.5rem;
  cursor: pointer;
}

/* Hoy */
.dark .fc-day-today {
  background: rgba(59, 130, 246, 0.15) !important;
}

/* Override variable interna */
.dark .fc {
  --fc-today-bg-color: rgba(59, 130, 246, 0.15);
}
</style>
