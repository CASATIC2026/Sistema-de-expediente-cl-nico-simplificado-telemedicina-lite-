<script setup>
import { ref, onMounted, onUnmounted, nextTick } from 'vue'
import FullCalendar from '@fullcalendar/vue3'
import dayGridPlugin from '@fullcalendar/daygrid'
import interactionPlugin from '@fullcalendar/interaction'
import esLocale from '@fullcalendar/core/locales/es'
import api, { logoutPro } from '@/services/api'
import { getCalendario } from '@/services/api'
import { getResumen, getEstadisticas } from '@/services/api'

// Componentes
import configuracionCuenta from '../components/configuracionCuenta.vue'
import tarjetaAgendarCitas from '../components/tarjetaAgendarCitas.vue'
import tarjetaVerMisCitas from '../components/tarjetaVerMisCitas.vue'
import soporteAyuda from '../components/soporteAyuda.vue'
import doctores from '../components/doctores.vue'
import gestionUsuarioAdmin from '../components/gestionUsuarioAdmin.vue'


const statsResumen = ref({ hoy: 0 })
const statsUsuarios = ref({ totalPacientes: 0, totalDoctores: 0 })

const calendarRef = ref(null)
const calendarioListo = ref(false) //controla cuándo montar FullCalendar

// ===============================
// PERFIL
const usuario = ref(null)
const nombreUsuario = ref('Cargando...')
const fotoPerfil = ref('')

const cargarEstadisticas = async () => {
  try {
    const [resumen, usuarios] = await Promise.all([
      getResumen(),
      getEstadisticas()
    ])
    statsResumen.value = resumen
    statsUsuarios.value = usuarios
  } catch (e) {
    console.error('Error cargando estadísticas:', e)
  }
}

const cargarPerfil = async () => {
  try {
    const res = await api.get('/Users/me')
    const data = res.data
    usuario.value = data

    const nombre = data.nombre || data.Nombre || ''
    const apellido = data.apellido || data.Apellido || ''
    const nombreCompleto = `${nombre} ${apellido}`.trim() || 'Administrador'

    nombreUsuario.value = nombreCompleto
    fotoPerfil.value = data.fotoUrl || data.FotoUrl || ''

    localStorage.setItem('user_name', nombreCompleto)
  } catch (error) {
    console.error('Error al cargar perfil admin:', error)
    nombreUsuario.value = localStorage.getItem('user_name') || 'Admin'
  }
}

// ASPECT RATIO DINÁMICO===
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

// CALENDARIO CONFIG    ===============================
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
  editable: true,
  selectable: true,
  events: [],
  windowResize: () => {
    actualizarAspectRatio()
  },
  eventClick: (info) => {
    const { doctor, estado } = info.event.extendedProps
    alert(`Gestión Admin: ${info.event.title}\nEstado: ${estado}\nDoctor: ${doctor}`)
  }
})


//CARGAR CITAS
const cargarCitas = async () => {
  try {
    const citas = await getCalendario()

    const eventos = citas.map(c => ({
      id: c.idCita,
      title: c.pacienteNombreCompleto,
      start: c.start,
      end: c.end,
      extendedProps: {
        estado: c.estado,
        doctor: c.titulo
      }
    }))

    //Reemplaza objeto entero para que Vue detecte el cambio
    calendarOptions.value = {
      ...calendarOptions.value,
      events: eventos
    }

    //Fuerza recarga via API interna de FullCalendar
    await nextTick()
    const calApi = calendarRef.value?.getApi()
    if (calApi) {
      calApi.removeAllEvents()
      calApi.addEventSource(eventos)
    }

  } catch (e) {
    console.error('Error cargando citas:', e)
  }
}

// =
//LIFECYCLE
onMounted(async () => {
  await Promise.all([
    cargarPerfil(),
    cargarCitas(),
    cargarEstadisticas()
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


// MODALES    // ===============================
const accederCita = ref(false)
const accederVerMisCitas = ref(false)
const accederSoporteAyuda = ref(false)
const mostrarInformacion = ref(false)
const accederGestionUsuario = ref(false)
const doctorModal = ref(false)
</script>

<template>
  <div class="flex flex-col md:flex-row min-h-screen bg-slate-100 font-sans text-slate-700">
    
    <!-- ── Sidebar ────────────────────────────────────── -->
    <aside class="w-full md:w-64 bg-slate-900 text-white flex flex-col p-5 shadow-lg shrink-0">
      <div class="mb-8 md:mb-10 text-center">


          <img
            src="@/assets/Logo icon.png"
            alt="nuvomed lite"
            class="w-16 h-16 md:w-32 md:h-32 object-contain mx-auto mb-2 drop-shadow-[0_0_12px_rgba(34,211,238,0.4)]"
          />
          <div class="text-sm font-bold tracking-wider">
            T E L M E D<span class="text-cyan-400"> Lite™</span>
          </div>
        

        <div class="text-sm border-t border-white/10 pt-4">
          <p class="font-semibold text-emerald-400">Panel Administrador</p>

          <div v-if="usuario" class="flex justify-center my-3">
            <img v-if="fotoPerfil" :src="fotoPerfil" @error="fotoPerfil = ''" 
                class="w-16 h-16 rounded-full border-2 border-emerald-400 object-cover shadow-md" />
            
            <div v-else class="w-16 h-16 rounded-full bg-emerald-600 flex items-center justify-center text-xl font-bold text-white border-2 border-emerald-400/50">
              {{ usuario.nombre?.[0] || 'A' }}{{ usuario.apellido?.[0] || '' }}
            </div>
          </div>

          <p class="text-xs text-slate-400 uppercase tracking-widest">¡Bienvenido!</p>
          <h2 class="text-lg font-bold text-white truncate px-2">{{ nombreUsuario }}</h2>
        </div>
      </div>

      <nav class="grid grid-cols-3 md:flex md:flex-col gap-3 flex-grow">
        
        <button @click="accederCita = true" class="border-t border-white/10 pt-3 md:pt-4 flex flex-col md:flex-row items-center gap-1 md:gap-3 px-2 py-2 md:px-4 md:py-3 rounded-lg w-full text-center md:text-left transition-all hover:bg-cyan-500/60 hover:md:translate-x-1">
          <i class="fa-solid fa-calendar-plus text-xl text-cyan-400"></i>
          <span class="mt-1 leading-tight">Agendar cita</span>
        </button>
        <button @click="accederVerMisCitas = true" class="border-t border-white/10 pt-3 md:pt-4 flex flex-col md:flex-row items-center gap-1 md:gap-3 px-2 py-2 md:px-4 md:py-3 rounded-lg w-full text-center md:text-left transition-all hover:bg-cyan-500/60 hover:md:translate-x-1">
          <i class="fa-solid fa-clipboard-list text-xl text-cyan-400"></i> 
          <span class="leading-tight">Control de citas</span>
        </button>

        <button @click="doctorModal = true" class="border-t border-white/10 pt-3 md:pt-4 flex flex-col md:flex-row items-center gap-1 md:gap-3 px-2 py-2 md:px-4 md:py-3 rounded-lg w-full text-center md:text-left transition-all hover:bg-cyan-500/60 hover:md:translate-x-1">
          <i class="fa-solid fa-user-doctor text-lg text-cyan-400 md:justify-start"></i> 
          <span class=" mt-1 leading-tight md:justify-right">Gestion Doctores</span>
        </button>

        <button @click="accederGestionUsuario = true" class="border-t border-white/10 pt-3 md:pt-4 flex flex-col md:flex-row items-center gap-1 md:gap-3 px-2 py-2 md:px-4 md:py-3 rounded-lg w-full text-center md:text-left transition-all hover:bg-cyan-500/60 hover:md:translate-x-1">
          <i class="fa-solid fa-hospital-user text-lg text-cyan-400"></i> 
          <span class="mt-1 leading-tight">Gestion de pacientes</span>
        </button>

        <button @click="accederSoporteAyuda = true" class="border-t border-white/10 pt-3 md:pt-4 flex flex-col md:flex-row items-center gap-1 md:gap-3 px-2 py-2 md:px-4 md:py-3 rounded-lg w-full text-center md:text-left transition-all hover:bg-cyan-500/60 hover:md:translate-x-1">
          <i class="fa-solid fa-circle-question text-lg text-cyan-400"></i> 
          <span class="mt-1 leading-tight">Soporte/ayuda</span>
        </button>

        <button @click="mostrarInformacion = true" class="border-t border-white/10 pt-3 md:pt-4 flex flex-col md:flex-row items-center gap-1 md:gap-3 px-2 py-2 md:px-4 md:py-3 rounded-lg w-full text-center md:text-left transition-all hover:bg-cyan-500/60 hover:md:translate-x-1">
          <i class="fa-solid fa-user-gear text-cyan-400"></i>
          <span class="mt-1 leading-tight">Mi cuenta</span>
        </button>
        
        <button @click="logoutPro" class="flex items-center gap-3 px-4 py-3 rounded-lg w-auto md:w-full text-left transition-all mt-0 md:mt-auto text-red-300 hover:bg-red-500/50">
          <i class="fa-solid fa-power-off"></i>
          <span class="mt-1 leading-tight">Cerrar Sesión</span>
        </button>
      </nav>
    </aside>

    <!-- ── Contenido principal ────────────────────────── -->
    <main class="flex-grow p-4 md:p-8 overflow-y-auto">
      <header class="flex justify-between items-end mb-10">
        <div>
          <h1 class="text-3xl font-extrabold text-slate-900 tracking-tight">Calendario administrativo</h1>
          <p class="text-slate-500 mt-1 font-medium">Vista general del panel administrativo</p>
        </div>
     
        <div class="hidden lg:block text-right">
          <p class="text-xs font-bold text-slate-400 uppercase tracking-widest">Estado del Sistema</p>
          <div class="flex items-center gap-2 text-emerald-600 font-bold text-sm">
            <span class="w-2 h-2 bg-emerald-500 rounded-full animate-pulse"></span>
            Sincronizado
          </div>
        </div>
      </header>




      <div class="grid grid-cols-1 md:grid-cols-3 gap-4 mb-6">

        <div class="bg-slate-200 p-4 rounded-xl shadow-lg border border-border">
          <p class="text-md text-muted">Citas este día</p>
          <h2 class="text-2xl font-bold">{{ statsResumen.hoy ?? 0 }}</h2>
        </div>

        <div class="bg-emerald-100 p-4 rounded-xl shadow-lg border border-border">
          <p class="text-md text-muted">Pacientes registrados</p>
          <h2 class="text-2xl font-bold">{{ statsUsuarios.totalPacientes ?? 0 }}</h2>
        </div>

        <div class="bg-cyan-100 p-4 rounded-xl shadow-lg border border-border">
          <p class="text-md text-muted">Doctores registrados</p>
          <h2 class="text-2xl font-bold">{{ statsUsuarios.totalDoctores ?? 0 }}</h2>
        </div>

      </div>









      <section class="bg-white rounded-xl p-4 md:p-6 shadow-sm border border-slate-200">
        <div class="calendar-container overflow-hidden">
          <FullCalendar 
          v-if="calendarioListo"
          ref="calendarRef"
          :options="calendarOptions" />
        </div>
      </section>
    </main>

  

    <!-- Agendar cita -->
    <div v-if="accederCita" 
         class="fixed inset-0 z-[1000] flex items-center justify-center bg-black/50 backdrop-blur-sm p-4" 
         @click.self="accederCita = false">
      <tarjetaAgendarCitas :esSecretaria="true" @cerrar="accederCita = false" />
    </div>

    <!-- Ver/control de citas -->
    <div v-if="accederVerMisCitas" 
         class="fixed inset-0 z-[1000] overflow-y-auto" 
         @click.self="accederVerMisCitas = false">
      <tarjetaVerMisCitas :esAdmin="true" @cerrar="accederVerMisCitas = false" />
    </div>

    <!-- Gestión de pacientes -->
    <div v-if="accederGestionUsuario" 
         class="fixed inset-0 z-[1000] overflow-y-auto">
      <gestionUsuarioAdmin @cerrar="accederGestionUsuario = false" />
    </div>

    <!-- Doctores — -->
    <div v-if="doctorModal" 
         class="fixed inset-0 z-[1000] overflow-y-auto">
      <doctores :esAdmin="true" @cerrar="doctorModal = false" />
    </div>

    <!-- Soporte y ayuda -->
    <soporteAyuda v-if="accederSoporteAyuda" @cerrar="accederSoporteAyuda = false" />

    <!-- Mi cuenta (perfil del admin) -->
    <div v-if="mostrarInformacion" 
         class="fixed inset-0 z-[1000] flex items-center justify-center bg-black/50 backdrop-blur-sm p-4" 
         @click.self="mostrarInformacion = false">
      <configuracionCuenta 
        :usuarioData="usuario" 
        :rolSesion="'admin'" 
        @cerrar="mostrarInformacion = false" 
      />
    </div>

  </div>
</template>

<style scoped>
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