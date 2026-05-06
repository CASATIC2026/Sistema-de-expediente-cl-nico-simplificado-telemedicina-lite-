<script setup>
import { ref, onMounted, onUnmounted, nextTick } from 'vue'
import { useRouter } from 'vue-router'
import { getCalendario } from '@/services/api'
import { logoutPro } from '@/services/api'
import api from '@/services/api'

import FullCalendar from '@fullcalendar/vue3'
import dayGridPlugin from '@fullcalendar/daygrid'
import interactionPlugin from '@fullcalendar/interaction'
import esLocale from '@fullcalendar/core/locales/es'

// Componentes
import configuracionCuenta from '../components/configuracionCuenta.vue'
import tarjetaAgendarCitas from '../components/tarjetaAgendarCitas.vue'
import tarjetaVerMisCitas from '../components/tarjetaVerMisCitas.vue'
import soporteAyuda from '@/components/soporteAyuda.vue'

const calendarRef = ref(null)
const calendarioListo = ref(false) //controla cuándo montar FullCalendar
const router = useRouter()

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
    const { estado, paciente } = info.event.extendedProps
    alert(`Paciente: ${paciente}\nEstado: ${estado}\nFecha: ${info.event.start.toLocaleString()}\nTipo consulta: ${info.event.title}`)
  }
})

// ===============================
// CARGAR CALENDARIO
const cargarCalendario = async () => {
  try {
    const data = await getCalendario()

    const eventos = data.map(cita => ({
      id: cita.idCita,
      title: `${cita.tipoConsulta}`,
      motivo: '${cita.motivo}',
      start: cita.start,
      end: cita.end,
      extendedProps: {
        estado: cita.estado,
        paciente: cita.pacienteNombreCompleto
      },
      color:
        cita.estado === 'Pendiente'  ? '#facc15' :
        cita.estado === 'Confirmada' ? '#22c55e' :
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

  } catch (e) {
    console.error(e)
  }
}

// ===============================
// PERFIL
const usuario = ref(null)
const nombreUsuario = ref('Cargando...')
const fotoPerfil = ref('')

const cargarPerfil = async () => {
  try {
    const res = await api.get('/Users/me')
    const data = res.data

    usuario.value = data

    const nombre = data.nombre || data.Nombre || ''
    const apellido = data.apellido || data.Apellido || ''
    const nombreCompleto = `${nombre} ${apellido}`.trim() || 'Usuario'

    nombreUsuario.value = nombreCompleto
    fotoPerfil.value = data.fotoUrl || data.FotoUrl || ''

    localStorage.setItem('user_name', nombreCompleto)

  } catch (error) {
    console.error('Error:', error.response?.data || error.message)
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

  //Esperar que el contenedor tenga dimensiones reales antes de montar
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
const mostrarInformacion = ref(false)

// ===============================
// LOGOUT
const cerrarSesion = async () => {
  if (confirm('¿Desea cerrar su sesión de forma segura?')) {
    await logoutPro()
  }
}
</script>

<template>
  <div class="flex flex-col md:flex-row min-h-screen bg-slate-100 font-sans text-slate-700">

    <!-- SIDEBAR -->
    <aside class="w-full md:w-64 bg-slate-900 text-white flex flex-col p-3 md:p-5 shadow-lg shrink-0">

      <div class="mb-4 md:mb-10 text-center">
        <img
          src="@/assets/Logo icon.png"
          alt="nuvomed lite"
          class="w-16 h-16 md:w-32 md:h-32 object-contain mx-auto mb-2 drop-shadow-[0_0_12px_rgba(34,211,238,0.4)]"
        />
        <div class="text-xs md:text-sm font-bold tracking-wider">
          N O V O M E D<span class="text-cyan-400"> Lite™</span>
        </div>

        <div class="text-sm border-t border-white/10 pt-3 md:pt-4">
          <p class="font-semibold text-cyan-400">Portal Paciente</p>

          <div v-if="usuario" class="flex justify-center my-2 md:my-3">
            <img
              v-if="fotoPerfil"
              :src="fotoPerfil"
              @error="fotoPerfil = ''"
              class="w-12 h-12 md:w-16 md:h-16 rounded-full border-2 border-cyan-400 object-cover"
            />
            <div
              v-else
              class="w-12 h-12 md:w-16 md:h-16 rounded-full bg-cyan-600 flex items-center justify-center text-lg md:text-xl font-bold text-white border-2 border-cyan-400/50"
            >
              {{ usuario.nombre?.[0] || 'U' }}{{ usuario.apellido?.[0] || '' }}
            </div>
          </div>

          <p class="text-xs text-slate-400 uppercase tracking-widest">¡Bienvenido!</p>
          <h2 class="text-base md:text-lg font-bold text-white truncate px-2">{{ nombreUsuario }}</h2>
        </div>
      </div>

      <!-- NAV -->
      <nav class="grid grid-cols-3 md:grid-cols-1 md:flex md:flex-col gap-1 md:gap-2">

        <button @click="accederCita = true"
          class="border-t border-white/10 pt-3 md:pt-4 flex flex-col md:flex-row items-center gap-1 md:gap-3 px-2 py-2 md:px-4 md:py-3 rounded-lg w-full text-center md:text-left transition-all hover:bg-cyan-500/60 hover:md:translate-x-1">
          <i class="fa-solid fa-calendar-plus text-base md:text-lg text-emerald-400"></i>
          <span class="text-[11px] md:text-base font-medium leading-tight">Nueva cita</span>
        </button>

        <button @click="accederVerMisCitas = true"
          class="border-t border-white/10 pt-3 md:pt-4 flex flex-col md:flex-row items-center gap-1 md:gap-3 px-2 py-2 md:px-4 md:py-3 rounded-lg w-full text-center md:text-left transition-all hover:bg-cyan-500/60 hover:md:translate-x-1">
          <i class="fa-solid fa-clipboard-list text-base md:text-lg text-emerald-400"></i>
          <span class="text-[11px] md:text-base font-medium leading-tight">Ver mis citas</span>
        </button>

        <button @click="accederSoporteAyuda = true"
          class="border-t border-white/10 pt-3 md:pt-4 flex flex-col md:flex-row items-center gap-1 md:gap-3 px-2 py-2 md:px-4 md:py-3 rounded-lg w-full text-center md:text-left transition-all hover:bg-cyan-500/60 hover:md:translate-x-1">
          <i class="fa-solid fa-circle-question text-base md:text-lg text-emerald-400"></i>
          <span class="text-[11px] md:text-base font-medium leading-tight">Soporte/ayuda</span>
        </button>

        <button @click="mostrarInformacion = true"
          class="border-t border-white/10 pt-3 md:pt-4 flex flex-col md:flex-row items-center gap-1 md:gap-3 px-2 py-2 md:px-4 md:py-3 rounded-lg w-full text-center md:text-left transition-all hover:bg-cyan-500/60 hover:md:translate-x-1">
          <i class="fa-solid fa-user-gear text-base md:text-lg text-emerald-400"></i>
          <span class="text-[11px] md:text-base font-medium leading-tight">Mi cuenta</span>
        </button>

        <button @click="cerrarSesion"
          class="border-t border-white/10 pt-3 md:pt-4 flex flex-col md:flex-row items-center gap-1 md:gap-3 px-2 py-2 md:px-4 md:py-3 rounded-lg w-full text-center md:text-left transition-all hover:bg-red-500/40 hover:md:translate-x-1 col-span-3 md:col-span-1">
          <i class="fa-solid fa-power-off text-base md:text-lg text-red-400"></i>
          <span class="text-[11px] md:text-base font-medium leading-tight text-red-400">Cerrar Sesión</span>
        </button>

      </nav>
    </aside>

    <!-- MAIN -->
    <main class="flex-grow p-3 md:p-8">


      <header class="flex justify-between items-end mb-10">
        <div>
          <h1 class="text-3xl font-extrabold text-slate-900 tracking-tight">Mi calendario</h1>
          <p class="text-slate-500 mt-1 font-medium">Revisa el estado de tus citas en el siguiente calendario</p>
        </div>
        <!-- Indicador de fecha actual (Opcional, añade seriedad) -->
        <div class="hidden lg:block text-right">
          <p class="text-xs font-bold text-slate-400 uppercase tracking-widest">Sistema actual</p>
          <div class="flex items-center gap-2 text-emerald-600 font-bold text-sm">
            <span class="w-2 h-2 bg-emerald-500 rounded-full animate-pulse"></span>
            Sincronizado
          </div>
        </div>
      </header>




      <section class="bg-white rounded-xl p-3 md:p-6 shadow-sm border overflow-x-auto">
        <FullCalendar 
        v-if="calendarioListo"
        ref="calendarRef"
        :options="calendarOptions" />
      </section>

      <!-- MODALES -->
      <div v-if="accederCita" class="modal" @click.self="accederCita = false">
        <tarjetaAgendarCitas
          :esSecretaria="false"
          @cerrar="() => { accederCita = false; cargarCalendario(); }"
        />
      </div>

      <div v-if="accederVerMisCitas" class="modal" @click.self="accederVerMisCitas = false">
        <tarjetaVerMisCitas @cerrar="accederVerMisCitas = false" />
      </div>

      <soporteAyuda v-if="accederSoporteAyuda" @cerrar="accederSoporteAyuda = false" />

      <div v-if="mostrarInformacion" class="modal" @click.self="mostrarInformacion = false">
        <configuracionCuenta
          :usuarioData="usuario"
          :rolSesion="usuario?.rol?.toLowerCase() || 'paciente'"
          @cerrar="mostrarInformacion = false"
          @actualizar="cargarPerfil"
        />
      </div>

    </main>
  </div>
</template>

<style scoped>
.modal {
  position: fixed;
  inset: 0;
  display: flex;
  align-items: flex-start;
  justify-content: center;
  background: rgba(0, 0, 0, 0.5);
  overflow-y: auto;
  padding: 1rem;
  z-index: 50;
}

/* En desktop centramos verticalmente si hay espacio */
@media (min-width: 768px) {
  .modal {
    align-items: center;
    padding: 2rem;
  }
}

:deep(.fc-button-primary) {
  background-color: #0891b2 !important;
  border-color: #0891b2 !important;
  border-radius: 8px;
}

/* Calendario legible en móvil */
:deep(.fc-toolbar-title) {
  font-size: clamp(0.85rem, 2.5vw, 1.25rem) !important;
}

:deep(.fc-toolbar.fc-header-toolbar) {
  flex-wrap: wrap;
  gap: 0.5rem;
}

:deep(.fc-daygrid-day-number),
:deep(.fc-col-header-cell-cushion) {
  font-size: clamp(0.7rem, 2vw, 0.9rem);
}
</style>