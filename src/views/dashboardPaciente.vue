<script setup>
import { ref } from 'vue'
import FullCalendar from '@fullcalendar/vue3'
import dayGridPlugin from '@fullcalendar/daygrid'
import interactionPlugin from '@fullcalendar/interaction'
import esLocale from '@fullcalendar/core/locales/es' // Para asegurar el idioma

import configuracionCuenta from '../components/configuracionCuenta.vue'
import tarjetaAgendarCitas from '../components/tarjetaAgendarCitas.vue'
import verHistorial from '../components/verHistorial.vue'
import tarjetaVerMisCitas from '../components/tarjetaVerMisCitas.vue'


const calendarOptions = ref({
  plugins: [dayGridPlugin, interactionPlugin],
  initialView: 'dayGridMonth',
  locale: esLocale,
  aspectRatio: 2.25,
  headerToolbar: {
    left: 'prev,next today',
    center: 'title',
    right: 'dayGridMonth',
  },

  titleFormat: { // Formato del título del mes
    month: 'long',
    year: 'numeric'
  },
  
  // RESTRECCIONES PARA EL PACIENTE
  editable: false,      // El paciente no puede arrastrar las citas
  selectable: false,    // No puede seleccionar rangos (:(----))
  
  // LISTA DE CITAS (Aquí cargaremos lo que viene de nuestra base de datos)
  events: [
    { 
      id: '1', 
      title: 'Cons. Gral- Dr. xyzxyzxyz', 
      start: '2026-03-10T14:30:00',
      backgroundColor: '#3498db', // Azul: Pendiente
      extendedProps: { estado: 'Pendiente', doctor: 'Dr. xyzxyzxyz' }
    },
    { 
      id: '2', 
      title: 'Dermatología (Cancelada)', 
      start: '2026-03-05T09:00:00',
      backgroundColor: '#e74c3c', // Rojo: Cancelada
      display: 'background',      // Opcional: sombrear el fondo
      extendedProps: { estado: 'Cancelada' }
    },
    {
      id: '3',
      title: 'Psicología - Finalizada',
      start: '2026-03-01T16:00:00',
      backgroundColor: '#2ecc71', // Verde: Completada
      extendedProps: { estado: 'Completada' }
    }
  ],

  // Al hacer clic, mostramos detalles en lugar de borrar directamente
  eventClick: (info) => {
    const { doctor, estado } = info.event.extendedProps;
    alert(`Detalles de tu Cita:
    Evento: ${info.event.title}
    Estado: ${estado}
    ${doctor ? 'Médico: ' + doctor : ''}`);
  }
})

let mostrarInformacion = ref(false);
let informacionActiva  = () => {
   mostrarInformacion.value = !mostrarInformacion.value;
} 

let accederCita = ref(false);
let accederCitaActiva  = () => {
   accederCita.value = !accederCita.value;
}

let accederHistorial = ref(false);
let accederHistoarialActivo  = () => {
   accederHistorial.value = !accederHistorial.value;
}

let accederVerMisCitas = ref (false);
let accderVerMisCitasActivo = () =>{
  accederVerMisCitas.value = !accederVerMisCitas.value
}



</script>

<template>
  <div class="flex flex-col md:flex-row min-h-screen bg-slate-100 font-sans text-slate-700">
    
    <aside class="w-full md:w-64 bg-[#1a365d] text-white flex flex-col p-5 shadow-lg shrink-0">
      <div class="mb-8 md:mb-10 text-center">
        <div class="text-2xl font-bold tracking-wider mb-4">miLOGO</div>
        <div class="text-sm border-t border-white/10 pt-4">
          <p class="font-semibold">Usuario</p>
          <p class="text-slate-400 text-xs mt-1">● Usuario Activo</p>
        </div>
      </div>

      <nav class="flex flex-row md:flex-col flex-wrap justify-center md:justify-start gap-2 md:gap-3 flex-grow">
        <button @click="accederCitaActiva" class="flex items-center gap-3 px-4 py-3 rounded-lg w-auto md:w-full text-left transition-all hover:bg-white/10 hover:translate-x-1">
          <span class="text-lg">📅+</span> <span class="hidden md:inline">Agendar cita</span>
        </button>
        
        <button @click="accderVerMisCitasActivo" class="flex items-center gap-3 px-4 py-3 rounded-lg w-auto md:w-full text-left transition-all bg-blue-600 text-white font-semibold shadow-md">
          <span class="text-lg">👁️..</span> <span class="hidden md:inline">Ver mis citas</span>
        </button>
        
        <button @click="accederHistoarialActivo" class="flex items-center gap-3 px-4 py-3 rounded-lg w-auto md:w-full text-left transition-all hover:bg-white/10 hover:translate-x-1">
          <span class="text-lg">📜.</span> <span class="hidden md:inline">Mi historial</span>
        </button>
        
        <button @click="informacionActiva" class="flex items-center gap-3 px-4 py-3 rounded-lg w-auto md:w-full text-left transition-all hover:bg-white/10 hover:translate-x-1">
          <span class="text-lg">👤..</span> <span class="hidden md:inline">Mi usuario</span>
        </button>
        
        <button class="flex items-center gap-3 px-4 py-3 rounded-lg w-auto md:w-full text-left transition-all mt-0 md:mt-auto text-red-300 hover:bg-red-500/10 hover:text-red-400">
          <span class="text-lg">🚪...</span> <span class="hidden md:inline">Salir</span>
        </button>
      </nav>
    </aside>

    <main class="flex-grow p-4 md:p-8 overflow-y-auto">
      
      <section class="bg-white rounded-xl p-4 md:p-6 shadow-sm border border-slate-200">
        
        <div class="flex flex-col md:flex-row justify-between items-start md:items-center mb-6 pb-4 border-b border-slate-100 gap-4">
          <div class="flex flex-wrap gap-4 bg-slate-50 px-4 py-2 rounded-lg border border-slate-200 text-xs md:text-sm">
            <span class="flex items-center gap-1.5 font-bold text-blue-600">● Pendiente</span>
            <span class="flex items-center gap-1.5 font-bold text-green-600">● Completada</span>
            <span class="flex items-center gap-1.5 font-bold text-red-600">● Cancelada</span>
          </div>
        </div>

        <div class="calendar-container overflow-hidden">
          <FullCalendar :options="calendarOptions" />
        </div>
      </section>

      <div v-if="mostrarInformacion" class="fixed inset-0 z-[1000] flex items-center justify-center bg-black/50 backdrop-blur-sm p-4" @click.self="informacionActiva">
        <configuracionCuenta @cerrar="mostrarInformacion = false" />
      </div>

      <div v-if="accederCita" class="fixed inset-0 z-[1000] flex items-center justify-center bg-black/50 backdrop-blur-sm p-4" @click.self="accederCitaActiva">
        <tarjetaAgendarCitas @cerrar="accederCita = false" />
      </div>

      <div v-if="accederHistorial" class="fixed inset-0 z-[1000] flex items-center justify-center bg-black/50 backdrop-blur-sm p-4" @click.self="accederHistoarialActivo">
        <verHistorial @cerrar="accederHistorial = false" />
      </div>

    </main>
  </div>
</template>

<style scoped>
/* Mantenemos :deep() para los elementos internos de FullCalendar 
  que Tailwind no puede alcanzar directamente.
*/

:deep(.fc-toolbar-title) {
  @apply text-lg md:text-2xl font-bold text-slate-700 capitalize !important;
}

:deep(.fc-button-primary) {
  @apply !bg-slate-100 !border-slate-200 !text-slate-600 !font-bold !rounded-md hover:!bg-slate-200 transition-colors !shadow-none !capitalize !text-sm !px-4 !py-2 !important;
}

:deep(.fc-button-active) {
  @apply !bg-blue-600 !border-blue-600 !text-white !important;
}

:deep(.fc-event) {
  @apply !cursor-pointer !border-none !px-2 !py-0.5 !rounded !text-[11px] md:!text-xs !font-bold !shadow-sm !important;
}

:deep(.fc-daygrid-day-number) {
  @apply !p-2 !text-slate-500 !no-underline !font-medium !important;
}

/* Ajuste responsivo para el tamaño de la fuente del calendario en móvil */
@media (max-width: 640px) {
  :deep(.fc-header-toolbar) {
    @apply flex-col gap-3 !mb-4 !important;
  }
}
</style>