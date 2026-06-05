<script setup>
import FullCalendar from '@fullcalendar/vue3'
import dayGridPlugin from '@fullcalendar/daygrid'
import timeGridPlugin from '@fullcalendar/timegrid'
import interactionPlugin from '@fullcalendar/interaction'
import { updateCita } from '@/services/api'
import { useCitasStore } from '@/stores/citas'
import { ref, watch } from 'vue'

const citasStore = useCitasStore()

// Props
const props = defineProps({
  citas: {
    type: Array,
    default: () => []
  },
  editable: {
    type: Boolean,
    default: false
  },
  selectable: {
    type: Boolean,
    default: false
  }
})

const emit = defineEmits(['cita-click','cita-movida'])

// Estado del modal
const showModal = ref(false)
const selectedEvent = ref(null)

// Colores según estado
const getEstadoColor = (estado) => {
  switch (estado) {
    case 'Confirmada': return '#10b981'
    case 'Pendiente': return '#f59e0b'
    case 'Cancelada': return '#ef4444'
    case 'EnConsulta': return '#3b82f6'
    case 'Finalizada': return '#6b7280'
    default: return '#3b82f6'
  }
}

// Click en evento
const handleEventClick = (info) => {

  const cita = {
    idCita: info.event.id,
    title: info.event.title,
    start: info.event.start,
    end: info.event.end,
    ...info.event.extendedProps
  }

  selectedEvent.value = cita
  showModal.value = true

  emit('cita-click', cita)
}

// Drag & Drop citas
const handleEventDrop = async (info) => {

  if (info.event.extendedProps.estado !== "Pendiente") {
    info.revert()
    return
  }

  const id = info.event.id

  try {

    await updateCita(id, {
      FechaInicio: info.event.start,
      FechaFin: info.event.end,
      Motivo: info.event.extendedProps.motivo,
      TipoConsulta: info.event.extendedProps.tipo
    })

    console.log("Cita movida correctamente")

    await citasStore.cargarCitas()

  } catch (error) {

    console.error("Error al mover cita:", error.response?.data)

    info.revert()

  }

}

// Opciones calendario
const calendarOptions = ref({
  plugins: [dayGridPlugin, timeGridPlugin, interactionPlugin],

  initialView: 'dayGridMonth',

  height: 'auto',
  expandRows: true,
  fixedWeekCount: false,

  slotDuration: '00:30:00',
  slotLabelInterval: '00:30',
  slotMinTime: '08:00:00',
  slotMaxTime: '17:00:00',

  headerToolbar: {
    left: 'prev,next today',
    center: 'title',
    right: 'dayGridMonth,timeGridWeek,timeGridDay'
  },

  locale: 'es',
  events: [],          // ← empieza vacío

  editable: false,
  selectable: false,

  dateClick: (info) => {
    if (info.view.type === 'dayGridMonth') {
      info.view.calendar.changeView('timeGridDay', info.date)
    }
  },

  eventClick: handleEventClick,
  eventDrop: handleEventDrop
})

// Watcher: cuando cambien las props.citas, actualizamos eventos
watch(
  () => props.citas,
  (nuevasCitas) => {
    calendarOptions.value.events = nuevasCitas.map(cita => ({
      id: cita.idCita,
      title: `${cita.pacienteNombreCompleto} - ${cita.titulo}`,
      start: cita.start,
      end: cita.end,
      backgroundColor: getEstadoColor(cita.estado),
      borderColor: getEstadoColor(cita.estado),
      extendedProps: {
        idCita:   cita.idCita,
        estado:   cita.estado,
        tipo:     cita.tipoConsulta,
        motivo:   cita.titulo,
        paciente: cita.pacienteNombreCompleto
      }
    }))
  },
  { immediate: true, deep: true }  // ← immediate para que corra al montar
)

</script>

<template>
  <div
  class="bg-white dark:bg-slate-950
         p-4 rounded-2xl shadow-sm
         border border-gray-100 dark:border-slate-800
         w-full overflow-hidden transition-colors"
>
    <FullCalendar :options="calendarOptions" />
  </div>
</template>

<style>

/* ========================= */
/* BASE */
/* ========================= */

.fc {
  font-family: inherit;
}

.fc-toolbar.fc-header-toolbar {
  margin-bottom: 1.5rem !important;
  padding-top: 0.5rem;
}

/* Título */
.fc-toolbar-title {
  font-size: clamp(1rem, 2vw, 1.5rem) !important;
  font-weight: 800;
}

/* Botones */
.fc-button {
  border-radius: 0.8rem !important;
  padding: 0.55rem 0.9rem !important;
  font-weight: 700 !important;
  border: none !important;
  transition: all 0.2s ease;
}

/* Día actual */
.fc-day-today {
  background: rgba(59, 130, 246, 0.08) !important;
}

/* Eventos */
.fc-event {
  border: none !important;
  border-radius: 0.55rem !important;
  padding: 2px 4px !important;
  font-size: 0.75rem;
  font-weight: 600;
}

/* Hover */
.fc-event:hover {
  transform: scale(1.01);
}

/* ========================= */
/* LIGHT MODE */
/* ========================= */

.fc-theme-standard td,
.fc-theme-standard th {
  border-color: rgb(226 232 240);
}

.fc-col-header-cell {
  background: #f8fafc;
}

.fc-col-header-cell-cushion {
  color: #334155;
  font-weight: 700;
}

.fc-daygrid-day {
  background: white;
}

.fc-daygrid-day-number {
  color: #334155;
  font-weight: 600;
}

.fc-button {
  background: #334155 !important;
}

.fc-button:hover {
  background: #4682B4 !important;
}

.fc-button-active {
  background: #2563eb !important;
}

/* ========================= */
/* DARK MODE */
/* ========================= */

.dark .fc {
  color: #e2e8f0;
}

.dark .fc-theme-standard td,
.dark .fc-theme-standard th {
  border-color: rgb(51 65 85);
}

.dark .fc-col-header-cell {
  background: rgb(15 23 42);
}

.dark .fc-col-header-cell-cushion {
  color: white;
}

.dark .fc-daygrid-day {
  background: rgb(2 6 23);
}

.dark .fc-daygrid-day:hover {
  background: rgb(15 23 42);
}

.dark .fc-daygrid-day-number {
  color: #f8fafc;
}

.dark .fc-day-other .fc-daygrid-day-number {
  color: rgb(100 116 139);
}

/* Día actual dark */
.dark .fc-day-today {
  background: rgba(59, 130, 246, 0.14) !important;
}

/* Override interno FullCalendar */
.dark .fc {
  --fc-today-bg-color: rgba(59, 130, 246, 0.14);
}

/* Botones dark */
.dark .fc-button {
  background: rgb(30 41 59) !important;
}

.dark .fc-button:hover {
  background: rgb(70 130 180) !important;
}

.dark .fc-button-active {
  background: rgb(37 99 235) !important;
}

/* TEXTO EVENTOS LIGHT */
.fc-event-title,
.fc-event-time {
  color: #000000 !important;
}

/* TEXTO EVENTOS DARK */
.dark .fc-event-title,
.dark .fc-event-time {
  color: white !important;
}

</style>
