<script setup>
import { ref, computed } from 'vue'

const pestañaActiva = ref('pendientes')
const emit = defineEmits(['close'])

const citas = ref([
  { 
    id: 1, 
    doctor: "Dr. García", 
    especialidad: "Cardiología", 
    fecha: "15 Mar", 
    hora: "10:00 AM", 
    estado: "pendientes",
    link: "https://meet.google.com/abc-defg-hij" 
  },
  { 
    id: 2, 
    doctor: "Dra. López", 
    especialidad: "Pediatría", 
    fecha: "18 Mar", 
    hora: "02:30 PM", 
    estado: "pendientes",
    link: "https://zoom.us/j/123456789"
  },
  { 
    id: 3, 
    doctor: "Dra. López", 
    especialidad: "Pediatría", 
    fecha: "17 Mar", 
    hora: "02:30 PM", 
    estado: "pendientes",
    link: "https://zoom.us/j/123456789"
  },
  { 
    id: 3, 
    doctor: "Dra. López", 
    especialidad: "Pediatría", 
    fecha: "19 Mar", 
    hora: "02:50 PM", 
    estado: "pendientes",
    link: "https://zoom.us/j/123456789"
  },
  { 
    id: 4, 
    doctor: "Dr. Martínez", 
    especialidad: "Médico General", 
    fecha: "05 Mar", 
    hora: "09:00 AM", 
    estado: "finalizadas",
    link: null 
  }
])

const citasFiltradas = computed(() => {
  return citas.value.filter(cita => cita.estado === pestañaActiva.value)
})

const cancelarCita = (id) => {
  if(confirm("¿Estás seguro de que deseas cancelar esta cita?")) {
    const cita = citas.value.find(c => c.id === id)
    if (cita) cita.estado = 'canceladas'
  }
}
</script>

<template>
  <div class="sombraContenedor" @click.self="$emit('close')">
    <div class="tarjetaPrincipal">
      <header class="header-modal">
        <button class="btn-regresar" @click="$emit('close')">
          <span class="icon">←</span> Regresar
        </button>
        <h3>Mis Citas Médicas</h3>
      </header>
      
      <nav class="tabs-navegacion">
        <button 
          v-for="tab in ['pendientes', 'canceladas', 'finalizadas']" 
          :key="tab"
          class="tab-btn"
          :class="{ activa: pestañaActiva === tab }"
          @click="pestañaActiva = tab"
        >
          {{ tab.charAt(0).toUpperCase() + tab.slice(1) }}
        </button>
      </nav>

      <main class="listado-citas">
        <div v-if="citasFiltradas.length > 0">
          <div v-for="cita in citasFiltradas" :key="cita.id" class="cita-card">
            <div class="cita-header">
              <div class="avatar">{{ cita.doctor.charAt(4) }}</div>
              <div class="info">
                <strong>{{ cita.doctor }}</strong>
                <span class="especialidad">{{ cita.especialidad }}</span>
              </div>
            </div>
            
            <div class="cita-detalles">
              <div class="detalle-item">
                <span class="label">Fecha:</span> {{ cita.fecha }}
              </div>
              <div class="detalle-item">
                <span class="label">Hora:</span> {{ cita.hora }}
              </div>
            </div>

            <div v-if="pestañaActiva === 'pendientes' && cita.link" class="video-container">
              <a :href="cita.link" target="_blank" class="btn-video">
                <span class="video-icon">📹</span>
                <div class="video-text">
                  <strong>Unirse a la videollamada</strong>
                  <span>Link de la reunión virtual</span>
                </div>
                <span class="arrow-icon">→</span>
              </a>
            </div>

            <footer class="cita-footer">
              <button 
                v-if="pestañaActiva === 'pendientes'" 
                @click="cancelarCita(cita.id)"
                class="btn-cancelar"
              >
                Cancelar Cita
              </button>
              <span v-else class="status-badge" :class="cita.estado">
                {{ cita.estado }}
              </span>
            </footer>
          </div>
        </div>

        <div v-else class="mensaje-vacio">
          <div class="icon-vacio">📅</div>
          <p>No tienes citas {{ pestañaActiva }}</p>
        </div>
      </main>
    </div>
  </div>
</template>

<style scoped>
/* 1. CONTENEDOR DE FONDO */
.sombraContenedor {
  position: fixed;
  inset: 0;
  background-color: rgba(15, 23, 42, 0.6);
  backdrop-filter: blur(6px);
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 1000;
  padding: 1rem;
}

/* 2. TARJETA PRINCIPAL */
.tarjetaPrincipal {
  background: #f8fafc;
  width: 100%;
  max-width: 480px;
  max-height: 85vh;
  border-radius: 24px;
  overflow: hidden;
  display: flex;
  flex-direction: column;
  box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.25);
  animation: subir 0.4s cubic-bezier(0.16, 1, 0.3, 1);
}

@keyframes subir {
  from { opacity: 0; transform: translateY(40px); }
  to { opacity: 1; transform: translateY(0); }
}

/* 3. CABECERA */
.header-modal {
  padding: 20px 24px;
  background: white;
  border-bottom: 1px solid #e2e8f0;
}

h3 { margin: 10px 0 0; color: #1e293b; font-size: 1.25rem; font-weight: 700; }

.btn-regresar {
  border: none;
  background: #f1f5f9;
  color: #64748b;
  padding: 8px 14px;
  border-radius: 10px;
  font-weight: 600;
  cursor: pointer;
  font-size: 0.85rem;
}

/* 4. TABS */
.tabs-navegacion {
  display: flex;
  background: #e2e8f0;
  margin: 15px 24px;
  padding: 4px;
  border-radius: 14px;
  gap: 4px;
}

.tab-btn {
  flex: 1;
  border: none;
  padding: 10px;
  border-radius: 10px;
  background: transparent;
  color: #64748b;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.2s ease;
}

.tab-btn.activa {
  background: white;
  color: #2563eb;
  box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1);
}

/* 5. LISTADO Y CARDS */
.listado-citas {
  padding: 0 24px 24px;
  overflow-y: auto;
}

.cita-card {
  background: white;
  border-radius: 18px;
  padding: 18px;
  margin-bottom: 16px;
  border: 1px solid #e2e8f0;
}

.cita-header {
  display: flex;
  gap: 12px;
  align-items: center;
  margin-bottom: 14px;
}

.avatar {
  width: 44px;
  height: 44px;
  background: #eff6ff;
  color: #2563eb;
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 800;
}

.info strong { display: block; color: #1e293b; }
.especialidad { font-size: 0.85rem; color: #64748b; }

.cita-detalles {
  display: grid;
  grid-template-columns: 1fr 1fr;
  background: #f8fafc;
  padding: 12px;
  border-radius: 12px;
  font-size: 0.85rem;
}

.label { color: #94a3b8; font-weight: 500; }

/* 6. SECCIÓN VIDEOLLAMADA */
.video-container {
  margin-top: 14px;
}

.btn-video {
  display: flex;
  align-items: center;
  gap: 12px;
  background: #f0f7ff;
  border: 1.5px solid #dbeafe;
  padding: 12px;
  border-radius: 14px;
  text-decoration: none;
  transition: transform 0.2s;
}

.btn-video:hover {
  background: #e0efff;
  transform: translateY(-2px);
}

.video-icon {
  font-size: 1.3rem;
  background: white;
  width: 38px;
  height: 38px;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 10px;
}

.video-text {
  flex: 1;
  display: flex;
  flex-direction: column;
}

.video-text strong { color: #2563eb; font-size: 0.85rem; }
.video-text span { color: #60a5fa; font-size: 0.7rem; }

.arrow-icon { color: #2563eb; font-weight: bold; }

/* 7. FOOTER Y BOTONES */
.cita-footer { margin-top: 14px; }

.btn-cancelar {
  width: 100%;
  padding: 10px;
  border-radius: 12px;
  border: 1.5px solid #fee2e2;
  background: white;
  color: #ef4444;
  font-weight: 700;
  cursor: pointer;
}

.status-badge {
  display: block;
  width: 100%;
  text-align: center;
  padding: 8px;
  border-radius: 10px;
  font-size: 0.75rem;
  font-weight: 700;
  text-transform: uppercase;
}

.status-badge.finalizadas { background: #dcfce7; color: #16a34a; }
.status-badge.canceladas { background: #f1f5f9; color: #64748b; }

.mensaje-vacio { text-align: center; padding: 40px 0; color: #94a3b8; }

/* 8. RESPONSIVO */
@media (max-width: 480px) {
  .tarjetaPrincipal {
    max-height: 90vh;
    border-radius: 24px 24px 0 0;
    align-self: flex-end;
  }
  .cita-detalles { grid-template-columns: 1fr; gap: 4px; }
  .btn-video { padding: 10px; }
}
</style>