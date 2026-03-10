<script setup>
import { ref } from 'vue'
import FullCalendar from '@fullcalendar/vue3'
import dayGridPlugin from '@fullcalendar/daygrid'
import interactionPlugin from '@fullcalendar/interaction'
import esLocale from '@fullcalendar/core/locales/es' // Para asegurar el idioma
import cuentaUsuario from './cuentaUsuario.vue'

const calendarOptions = ref({
  plugins: [dayGridPlugin, interactionPlugin],
  initialView: 'dayGridMonth',
  locale: esLocale,
  headerToolbar: {
    left: 'prev,next today',
    center: 'title',
    right: 'dayGridMonth'
  },
  
  // RESTRECCIONES PARA EL PACIENTE
  editable: false,      // El paciente no puede arrastrar las citas
  selectable: false,    // No puede seleccionar rangos (a menos que quieras que pida cita así)
  
  // LISTA DE CITAS (Aquí cargarías lo que viene de tu base de datos)
  events: [
    { 
      id: '1', 
      title: 'Consulta General - Dr. Arrieta', 
      start: '2026-03-10T14:30:00',
      backgroundColor: '#3498db', // Azul: Pendiente
      extendedProps: { estado: 'Pendiente', doctor: 'Dr. Arrieta' }
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


</script>

<template>
  <div class="patient-calendar">
    <div class="botonesControl">
      <div class="miLogo">miLOGO</div>
      <button>Agendar cita</button>
      <button>Ver citas</button>
      <button>Mi historial</button>
      <button @click="informacionActiva">Mi usuario</button>
      <button>Salir</button>
    </div>
    <div class="legend">
      <span class="badge pending">● Pendiente</span>
      <span class="badge completed">● Completada</span>
      <span class="badge cancelled">● Cancelada</span>
    </div>
    <FullCalendar :options="calendarOptions" />
    <div v-if="mostrarInformacion" @click.self="informacionActiva">  <cuentaUsuario /></div>
  </div>


</template>

<style>
.patient-calendar {
  background: #f1f5f9; 
  color: #0f172a;
   margin: 0;
  padding: 0;
  min-height: 100vh;
}

.legend {
  margin-top: 10px;
  margin-bottom: 15px;
  display: flex;
  gap: 15px;
  font-size: 20px;
}

.badge {
  
  font-size: 0.9em;
  font-weight: bold;
}

.pending { color: #005b97; }
.completed { color: #165503; }
.cancelled { color: #e61903; }

/* Estilo personalizado para el calendario del paciente */
:deep(.fc-event) {
  cursor: pointer;
  border: none;
  padding: 2px 5px;
}

.botonesControl {
  display: flex;
  justify-content: space-between; /* Logo a la izquierda, botones a la derecha */
  align-items: center;
  flex-wrap: wrap; /* Si no caben, se pasan a la siguiente línea */
  gap: 15px;
  background-color: #1e3a8a;
  padding: 15px 40px;
  box-shadow: 0 4px 10px rgba(0,0,0,0.1);
}

@media (max-width: 600px) {
  .botonesControl {
    flex-direction:column; /* En móvil: uno arriba del otro */
    text-align: center;
  }
}

.miLogo{
  font-size: 1.5rem;
  font-weight: bold;
}
@media (max-width: 480px) {
  .miLogo {
    font-size: 1.2rem; 
  }
}
button {
  background-color: white;
  color: #1e3a8a;
  font-weight: 600;
  padding: 10px 20px;
  border-radius: 8px;
  border: 1px solid #e2e8f0;
  transition: all 0.3s ease;
}

button:hover {
  background-color: #1e3a8a;
  color: white;
  transform: translateY(-2px);
}

/* Ajuste para móviles */
@media (max-width: 768px) {
  button {
    width: 50%;       /* En móviles, los botones suelen ser de ancho completo */
    padding: 15px;     /* Reducimos un poco para ganar espacio vertical */
    font-size: 16px;   /* Tamaño ideal para que sea fácil de tocar con el dedo */
  }
}


</style>