<script setup>
import FullCalendar from '@fullcalendar/vue3'
import dayGridPlugin from '@fullcalendar/daygrid'
import timeGridPlugin from '@fullcalendar/timegrid'
import interactionPlugin from '@fullcalendar/interaction'
import { updateCita } from '@/services/api'
import { useCitasStore } from '@/stores/citas'
import { ref, computed, watch } from 'vue'

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

// Convertimos citas en eventos
const eventos = computed(() =>
  props.citas.map(cita => ({
    id: cita.idCita,
    idCita: cita.idCita,
    title: `${cita.pacienteNombreCompleto} - ${cita.titulo}`,
    start: cita.start,
    end: cita.end,

    backgroundColor: getEstadoColor(cita.estado),
    borderColor: getEstadoColor(cita.estado),

    extendedProps: {
      idCita: cita.idCita,
      estado: cita.estado,
      tipo: cita.tipoConsulta,
      motivo: cita.titulo,
      paciente: cita.pacienteNombreCompleto
    }
  }))
)

// Click en evento
const handleEventClick = (info) => {

  const cita = {
    idCita: info.event.id,
    title: info.event.title,
    start: info.event.start,
    end: info.event.end,
    ...info.event.extendedProps
  }

  console.log("Click Cita", cita)

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

<div class="bg-white p-4 rounded-2xl shadow-sm border border-gray-100 w-full overflow-hidden">

  <FullCalendar :options="calendarOptions" />

</div>



</template>