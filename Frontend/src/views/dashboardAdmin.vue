<script setup>
import { ref, onMounted, onUnmounted, nextTick } from 'vue'
import FullCalendar from '@fullcalendar/vue3'
import dayGridPlugin from '@fullcalendar/daygrid'
import interactionPlugin from '@fullcalendar/interaction'
import esLocale from '@fullcalendar/core/locales/es'
import api, { logoutPro } from '@/services/api'
import { getProfileIcon } from '@/utils/getProfileIcon'
import {
  getCalendario,
  getResumen,
  getEstadisticas,
  getClinicProfile } from '@/services/api'
  import {
  MoonIcon,
  SunIcon
} from '@heroicons/vue/24/outline'

// Componentes
import configuracionCuenta from '../components/configuracionCuenta.vue'
import tarjetaAgendarCitas from '../components/tarjetaAgendarCitas.vue'
import tarjetaVerMisCitas from '../components/tarjetaVerMisCitas.vue'
import soporteAyuda from '../components/soporteAyuda.vue'
import doctores from '../components/doctores.vue'
import gestionUsuarioAdmin from '../components/gestionUsuarioAdmin.vue'
import BannerClinico from '@/components/BannerClinico.vue'
import AdminStats from '@/componentsDoctor/admin/AdminStats.vue'
import modalDetalleCita from '@/components/modalDetalleCita.vue'


const statsResumen = ref({ hoy: 0 })
const statsUsuarios = ref({ totalPacientes: 0, totalDoctores: 0 })


// Información de Clínica/Farmacia/Negocio
const clinicInfo = ref({
  clinicName: '',
  slogan: '',
  horario: '',
  telefono: ''
})

// Cargar calendario
const calendarRef = ref(null)
const calendarioListo = ref(false) //controla cuándo montar FullCalendar

// ===============================
// PERFIL
const usuario = ref(null)
const nombreUsuario = ref('Cargando...')
const fotoPerfil = ref('')
const iconoPerfil = ref('')

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

const cargarClinicProfile = async () => {
  try {
    const data = await getClinicProfile()
    clinicInfo.value = data
  } catch (e) {
    console.error('Error cargando perfil clínico:', e)
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
    console.error('Error al cargar perfil admin:', error)
    nombreUsuario.value = localStorage.getItem('user_name') || 'Admin'
  }
}

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
  editable: true,
  selectable: true,
  events: [],

  windowResize: () => {
    actualizarAspectRatio()
  },
  eventClick: (info) => {
     const { doctor, estado, doctorNombre, fechaInicio } = info.event.extendedProps
  const fecha = new Date(fechaInicio)

  citaSeleccionada.value = {
    paciente: info.event.title,
    doctor:   doctorNombre,
    estado,
    motivo:   doctor,
    fecha:    fecha.toLocaleDateString('es-SV', {
                weekday: 'long', year: 'numeric', month: 'long', day: 'numeric'
              }),
    hora:     fecha.toLocaleTimeString('es-SV', {
                hour: '2-digit', minute: '2-digit'
              })
  }
  }
})

const getColorEstado = (estado) => {
  const colores = {
    'Pendiente':   '#EAB308', // amarillo
    'Confirmada':  '#22C55E', // verde
    'EnConsulta':  '#3B82F6', // azul
    'Finalizada':  '#10B981', // esmeralda
    'Cancelada':   '#EF4444', // rojo
    'NoAsistida':  '#94A3B8', // gris
  }
  return colores[estado] ?? '#3B82F6'
}


//CARGAR CITAS
const cargarCitas = async () => {
  try {
    const citas = await getCalendario()

    const eventos = citas.map(c => ({
      id: c.idCita,
      title: c.pacienteNombreCompleto,
      start: c.start,
      end: c.end,
      color: getColorEstado(c.estado),
      extendedProps: {
        estado: c.estado,
        doctor: c.titulo,
        doctorId: c.doctorId,
        doctorNombre: c.doctorNombreCompleto,
        fechaInicio: c.start
      }
    }))

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

//LIFECYCLE
onMounted(async () => {
  await Promise.all([
    cargarPerfil(),
    cargarCitas(),
    cargarEstadisticas(),
    cargarClinicProfile()
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
const mostrarStats = ref(false)
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
<div class="flex flex-col min-h-screen font-sans transition-colors duration-300 md:flex-row bg-slate-100 text-slate-700 dark:bg-slate-950 dark:text-slate-100">
    <!-- ── Sidebar ────────────────────────────────────── -->
   <aside class="flex flex-col w-full p-5 text-white transition-colors duration-300 shadow-lg md:w-72 bg-slate-900 dark:bg-[#0b1120] shrink-0">
      <div class="mb-8 text-center md:mb-10">


          <img
            src="@/assets/Logo icon.png"
            alt="TelMed Lite™"
            class="w-16 h-16 md:w-32 md:h-32 object-contain mx-auto mb-2 drop-shadow-[0_0_12px_rgba(34,211,238,0.4)]"
          />
          <div class="text-sm font-bold tracking-wider">
            T E L M E D<span class="text-cyan-400 dark:text-sky-300"> Lite™</span>
          </div>


        <div class="pt-4 text-sm border-t border-white/10">
          <p class="font-semibold text-emerald-400">Panel Administrador</p>

          <div v-if="usuario" class="flex justify-center my-3">

              <!-- FOTO GOOGLE -->
              <img
                v-if="fotoPerfil"
                :src="fotoPerfil"
                @error="fotoPerfil = ''"
                class="object-cover w-16 h-16 border-2 rounded-full shadow-md border-emerald-400"
              />

              <!-- ICONO PERSONALIZADO -->
              <img
                v-else-if="iconoPerfil"
                :src="iconoPerfil"
                class="object-cover w-16 h-16 rounded-2xl shadow-lg bg-slate-800/80 p-1 border border-cyan-400/30"
              />

              <!-- FALLBACK -->
              <div
                v-else
                class="flex items-center justify-center w-16 h-16 text-xl font-bold text-white border-2 rounded-full bg-emerald-600 border-emerald-400/50"
              >
                {{
                  (usuario.nombre || usuario.Nombre || 'A')[0]
                  }}
                  {{
                    (usuario.apellido || usuario.Apellido || '')[0] || ''
                  }}
              </div>

            </div>
          <p class="text-xs tracking-widest uppercase text-slate-400 dark:text-slate-500">¡Bienvenido!</p>
          <h2 class="px-2 text-lg font-bold text-white truncate">{{ nombreUsuario }}</h2>
        </div>
      </div>

      <nav class="grid flex-grow grid-cols-3 gap-3 md:flex md:flex-col">

        <button @click="accederCita = true" class="flex flex-col items-center w-full gap-1 px-2 py-2
         pt-3 text-center transition-all border-t rounded-lg border-white/10 md:pt-4 md:flex-row md:gap-3
         md:px-4 md:py-3 md:text-left hover:bg-cyan-500/60 hover:md:translate-x-1 cursor-pointer">
          <i class="text-xl fa-solid fa-calendar-plus text-cyan-400"></i>
          <span class="mt-1 leading-tight">Agendar cita</span>
        </button>
        <button @click="accederVerMisCitas = true" class="flex flex-col items-center w-full gap-1 px-2 py-2
        pt-3 text-center transition-all border-t rounded-lg border-white/10 md:pt-4 md:flex-row md:gap-3
        md:px-4 md:py-3 md:text-left hover:bg-cyan-500/60 hover:md:translate-x-1 cursor-pointer">
          <i class="text-xl fa-solid fa-clipboard-list text-cyan-400"></i>
          <span class="leading-tight">Control de citas</span>
        </button>

        <button @click="doctorModal = true" class="flex flex-col items-center w-full gap-1 px-2 py-2 pt-3
        text-center transition-all border-t rounded-lg border-white/10 md:pt-4 md:flex-row md:gap-3
        md:px-4 md:py-3 md:text-left hover:bg-cyan-500/60 hover:md:translate-x-1 cursor-pointer">
          <i class="text-lg fa-solid fa-user-doctor text-cyan-400 md:justify-start"></i>
          <span class="mt-1 leading-tight md:justify-right">Gestion Doctores</span>
        </button>

        <button @click="accederGestionUsuario = true" class="flex flex-col items-center w-full gap-1
        px-2 py-2 pt-3 text-center transition-all border-t rounded-lg border-white/10 md:pt-4 md:flex-row md:gap-3
         md:px-4 md:py-3 md:text-left hover:bg-cyan-500/60 hover:md:translate-x-1 cursor-pointer">
          <i class="text-lg fa-solid fa-hospital-user text-cyan-400"></i>
          <span class="mt-1 leading-tight">Gestion de pacientes</span>
        </button>

        <button @click="mostrarStats= true" class="flex flex-col items-center w-full gap-1 px-2 py-2 pt-3
        text-center transition-all border-t rounded-lg border-white/10 md:pt-4 md:flex-row md:gap-3
        md:px-4 md:py-3 md:text-left hover:bg-cyan-500/60 hover:md:translate-x-1 cursor-pointer">
          <i class="text-lg fa-solid fa-chart-line text-cyan-400"></i>
          <span class="mt-1 leading-tight">Estadísticas</span>
        </button>

        <button @click="accederSoporteAyuda = !accederSoporteAyuda" class="flex flex-col items-center
        w-full gap-1 px-2 py-2 pt-3 text-center transition-all border-t rounded-lg border-white/10 md:pt-4
        md:flex-row md:gap-3 md:px-4 md:py-3 md:text-left hover:bg-cyan-500/60 hover:md:translate-x-1 cursor-pointer">
          <i class="text-lg fa-solid fa-circle-question text-cyan-400"></i>
          <span class="mt-1 leading-tight">Soporte/ayuda</span>
        </button>

        <button @click="mostrarInformacion = true" class="flex flex-col items-center w-full gap-1
        px-2 py-2 pt-3 text-center transition-all border-t rounded-lg border-white/10 md:pt-4 md:flex-row
        md:gap-3 md:px-4 md:py-3 md:text-left hover:bg-cyan-500/60 hover:md:translate-x-1 cursor-pointer">
          <i class="fa-solid fa-user-gear text-cyan-400"></i>
          <span class="mt-1 leading-tight">Mi cuenta</span>
        </button>

        <button
        @click="toggleDark"
        class="flex items-center w-auto gap-3 px-4 py-3 mt-2 text-left transition-all rounded-lg md:w-full hover:bg-sky-500/20 cursor-pointer"
        >
        <MoonIcon
          v-if="!isDark"
          class="w-5 h-5 text-sky-300"
        />
        <SunIcon
          v-else
          class="w-5 h-5 text-yellow-300"
        />
        <span>
          {{ isDark ? 'Modo claro' : 'Modo oscuro' }}
        </span>
      </button>



        <button @click="cerrarSesion" class="flex items-center w-auto gap-3 px-4 py-3 mt-0 text-left text-red-300
        transition-all rounded-lg md:w-full md:mt-auto hover:bg-red-500/50 cursor-pointer">
          <i class="fa-solid fa-power-off"></i>
          <span class="mt-1 leading-tight">Cerrar Sesión</span>
        </button>
      </nav>
    </aside>

    <!-- ── Contenido principal ────────────────────────── -->
    <main class="flex-grow px-0 py-4 overflow-y-auto md:p-6 lg:p-8">
    <div class="mb-6">
            <BannerClinico
              :clinicName="clinicInfo.clinicName"
              :slogan="clinicInfo.slogan"
              :horario="clinicInfo.horario"
              :telefono="clinicInfo.telefono"
            />
          </div>

      <div class="grid grid-cols-1 gap-4 mb-6 md:grid-cols-3">

        <div class="p-4 transition-colors duration-300 border shadow-lg bg-white/80 dark:bg-slate-900/90 backdrop-blur-sm rounded-xl border-slate-200 dark:border-slate-700">
          <p class="text-md text-muted">Citas este día</p>
          <h2 class="text-2xl font-bold">{{ statsResumen.hoy ?? 0 }}</h2>
        </div>

        <div class="p-4 transition-colors duration-300 border shadow-lg bg-white/80 dark:bg-slate-900/90 backdrop-blur-sm rounded-xl border-slate-200 dark:border-slate-700">
          <p class="text-md text-muted">Pacientes registrados</p>
          <h2 class="text-2xl font-bold">{{ statsUsuarios.totalPacientes ?? 0 }}</h2>
        </div>

        <div class="p-4 transition-colors duration-300 border shadow-lg bg-white/80 dark:bg-slate-900/90 backdrop-blur-sm rounded-xl border-slate-200 dark:border-slate-700">
          <p class="text-md text-muted">Doctores registrados</p>
          <h2 class="text-2xl font-bold">{{ statsUsuarios.totalDoctores ?? 0 }}</h2>
        </div>
      </div>
      <div class="mt-8 mb-4">

      <!-- Título y descripción del calendario -->
        <h2 class="text-2xl font-bold tracking-tight text-slate-800 dark:text-slate-100">
          Agenda General
        </h2>
        <p class="mt-1 text-sm text-slate-500 dark:text-slate-400">
          Visualiza y administra las citas programadas.
        </p>
      </div>

      <!-- Calendario-->
      <section class="p-4 transition-colors duration-300 bg-white border shadow-lg dark:bg-slate-900 rounded-2xl md:p-6 border-slate-200 dark:border-slate-700">
        <div class="overflow-hidden calendar-container">
          <FullCalendar
          v-if="calendarioListo"
          ref="calendarRef"
          :options="calendarOptions" />
        </div>
      </section>
    </main>

    <!-- Agendar cita -->
    <div v-if="accederCita"
         class="fixed inset-0 flex items-center justify-center p-4 z-1000 bg-black/50 backdrop-blur-sm"
         @click.self="accederCita = false">
      <tarjetaAgendarCitas :esSecretaria="true" @cerrar="accederCita = false" />
    </div>

    <!-- Ver/control de citas -->
    <div v-if="accederVerMisCitas"
         class="fixed inset-0 overflow-y-auto z-1000"
         @click.self="accederVerMisCitas = false">
      <tarjetaVerMisCitas :esAdmin="true" @cerrar="accederVerMisCitas = false" />
    </div>

    <!-- Gestión de pacientes -->
    <div v-if="accederGestionUsuario"
         class="fixed inset-0 overflow-y-auto z-1000">
      <gestionUsuarioAdmin @cerrar="accederGestionUsuario = false" />
    </div>

    <!-- Doctores — -->
    <div v-if="doctorModal"
         class="fixed inset-0 overflow-y-auto z-1000">
      <doctores :esAdmin="true" @cerrar="doctorModal = false" />
    </div>

    <!-- Estadísticas -->
  <div v-if="mostrarStats"
      class="fixed inset-0 overflow-y-auto z-1000 bg-black/50 backdrop-blur-sm">
    <AdminStats @cerrar="mostrarStats = false" />
  </div>

    <!-- Soporte y ayuda -->
     <Transition
      enter-active-class="transition-all duration-300 ease-out"
      enter-from-class="translate-x-full opacity-0"
      enter-to-class="translate-x-0 opacity-100"
      leave-active-class="transition-all duration-200 ease-in"
      leave-from-class="translate-x-0 opacity-100"
      leave-to-class="translate-x-full opacity-0"
    >
    <soporteAyuda v-if="accederSoporteAyuda"
    @cerrar="accederSoporteAyuda = false" />
    </Transition>

    <!-- Mi cuenta (perfil del admin) -->
    <div v-if="mostrarInformacion"
         class="fixed inset-0 flex items-center justify-center p-4 z-1000 bg-black/50 backdrop-blur-sm"
         @click.self="mostrarInformacion = false">
      <configuracionCuenta
        :usuarioData="usuario"
        :rolSesion="'admin'"
        @cerrar="mostrarInformacion = false"
        @actualizar="cargarPerfil"
        @actualizarClinic="cargarClinicProfile"
      />
    </div>

    <!-- Modal detalle cita -->
    <modalDetalleCita
      :cita="citaSeleccionada"
      @cerrar="citaSeleccionada = null"
    />

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
                <svg xmlns="http://www.w3.org/2000/svg" class="w-7 h-7 text-red-400" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="1.5">
                  <path stroke-linecap="round" stroke-linejoin="round" d="M15.75 9V5.25A2.25 2.25 0 0013.5 3h-6a2.25 2.25 0 00-2.25 2.25v13.5A2.25 2.25 0 007.5 21h6a2.25 2.25 0 002.25-2.25V15M12 9l-3 3m0 0l3 3m-3-3h12.75" />
                </svg>
              </div>

              <div class="text-center">
                <h3 class="text-white font-black text-lg leading-tight">Cerrar sesión</h3>
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

  </div>
</template>

<style>
.fc-toolbar-title {
  font-size: clamp(0.85rem, 2.5vw, 1.25rem) !important;
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
