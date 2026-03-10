<script setup>
import { ref, computed } from 'vue'

const pestañaActiva = ref('pendientes')
const emit = defineEmits(['close']) // Para cerrar el componente desde el padre

const citas = ref([
  { id: 1, doctor: "Dr. García", especialidad: "Cardiología", fecha: "15 Mar", hora: "10:00 AM", estado: "pendientes" },
  { id: 2, doctor: "Dra. López", especialidad: "Pediatría", fecha: "18 Mar", hora: "02:30 PM", estado: "pendientes" },
  { id: 2, doctor: "Dra. López", especialidad: "Pediatría", fecha: "18 Mar", hora: "02:30 PM", estado: "pendientes" },
  { id: 2, doctor: "Dra. López", especialidad: "Pediatría", fecha: "18 Mar", hora: "02:30 PM", estado: "pendientes" },
  { id: 3, doctor: "Dr. Martínez", especialidad: "Médico General", fecha: "05 Mar", hora: "09:00 AM", estado: "finalizadas" }
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
          <span class="icon">←</span>Regresar
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
/* 1. CONTENEDOR DE FONDO (OVERLAY) */
.sombraContenedor {
  position: fixed;
  inset: 0;
  background-color: rgba(15, 23, 42, 0.6); /* Azul oscuro traslúcido */
  backdrop-filter: blur(6px); /* Desenfoque del fondo */
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 1000;
  padding: 1rem;
  transition: all 0.3s ease;
}

/* 2. TARJETA PRINCIPAL (MODAL) */
.tarjetaPrincipal {
  background: #f8fafc;
  width: 100%;
  max-width: 480px; /* Tamaño ideal para escritorio */
  max-height: 85vh; /* Evita que se salga de la pantalla verticalmente */
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

h3 { 
  margin: 10px 0 0; 
  color: #1e293b; 
  font-size: 1.25rem; 
  font-weight: 700;
}

.btn-regresar {
  border: none;
  background: #f1f5f9;
  color: #64748b;
  padding: 8px 14px;
  border-radius: 10px;
  font-weight: 600;
  cursor: pointer;
  font-size: 0.85rem;
  transition: 0.2s;
}

.btn-regresar:hover {
  background: #e2e8f0;
  color: #1e293b;
}

/* 4. NAVEGACIÓN (TABS) */
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
  font-size: 0.9rem;
}

.tab-btn.activa {
  background: white;
  color: #2563eb;
  box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1);
}

/* 5. LISTADO DE CITAS */
.listado-citas {
  padding: 0 24px 24px;
  overflow-y: auto; /* Scroll interno si hay muchas citas */
  scrollbar-width: thin;
  scrollbar-color: #cbd5e1 transparent;
}

.cita-card {
  background: white;
  border-radius: 18px;
  padding: 18px;
  margin-bottom: 16px;
  border: 1px solid #e2e8f0;
  transition: transform 0.2s;
}

.cita-card:hover {
  border-color: #bfdbfe;
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
  font-size: 1.1rem;
}

.info strong { display: block; color: #1e293b; font-size: 1rem; }
.especialidad { font-size: 0.85rem; color: #64748b; }

.cita-detalles {
  display: grid;
  grid-template-columns: 1fr 1fr;
  background: #f8fafc;
  padding: 12px;
  border-radius: 12px;
  font-size: 0.85rem;
  gap: 8px;
}

.label { color: #94a3b8; font-weight: 500; margin-right: 4px; }

.cita-footer {
  margin-top: 14px;
  display: flex;
  gap: 10px;
}

/* 6. BOTONES Y ESTADOS */
.btn-cancelar {
  width: 100%;
  padding: 12px;
  border-radius: 12px;
  border: 1.5px solid #fee2e2;
  background: #fff;
  color: #ef4444;
  font-weight: 700;
  cursor: pointer;
  transition: 0.2s;
}

.btn-cancelar:hover {
  background: #ef4444;
  color: white;
  border-color: #ef4444;
}

.status-badge {
  width: 100%;
  text-align: center;
  padding: 8px;
  border-radius: 10px;
  font-size: 0.8rem;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.status-badge.finalizadas { background: #dcfce7; color: #16a34a; }
.status-badge.canceladas { background: #f1f5f9; color: #64748b; }

.mensaje-vacio {
  text-align: center;
  padding: 50px 20px;
  color: #94a3b8;
}

.icon-vacio { font-size: 3.5rem; margin-bottom: 10px; display: block; }

/* 7. AJUSTES RESPONSIVOS (MÓVILES) */

@media (max-width: 480px) {
  .tarjetaPrincipal {
    max-height: 92vh;
    border-radius: 20px 20px 0 0; /* Estilo bottom-sheet */
    align-self: flex-end; /* Se pega abajo para fácil acceso con el pulgar */
    margin: 0;
  }

  .header-modal { padding: 16px 20px; }
  
  .tabs-navegacion { margin: 12px 20px; }

  .listado-citas { padding: 0 20px 20px; }

  .cita-detalles {
    grid-template-columns: 1fr; /* Una sola columna para que no se amontone */
    gap: 4px;
  }
}

/* Para teléfonos muy pequeños */
@media (max-width: 360px) {
  h3 { font-size: 1.1rem; }
  .tab-btn { font-size: 0.8rem; padding: 8px 4px; }
  .avatar { width: 36px; height: 36px; }
}
</style>