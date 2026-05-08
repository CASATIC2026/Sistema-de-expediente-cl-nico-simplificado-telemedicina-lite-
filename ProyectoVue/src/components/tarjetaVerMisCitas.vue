<script setup>
import { ref, computed, onMounted } from 'vue'
import verHistorial from '@/components/verHistorial.vue'
import { getCitas, cancelarCita as cancelarCitaAPI, descargarReceta } from '@/services/api'

const props = defineProps({
  esAdmin: {
    type: Boolean,
    default: false
  }
})

const emit = defineEmits(['cerrar'])
const pestañaActiva = ref('Pendiente')
const filtroBusqueda = ref('')
const accederHistorial = ref(false)
const citas = ref([])
const citaSeleccionada = ref(null) 

// ===============================
// CARGAR DESDE BACKEND
onMounted(async () => {
  try {
    const data = await getCitas()
    citas.value = data.map(c => ({
      id: c.idCita,
      nombre: c.pacienteNombreCompleto,
      doctor: c.doctorNombreCompleto || "Doctor asignado",
      fecha: new Date(c.start).toLocaleDateString(),
      hora: new Date(c.start).toLocaleTimeString('es-SV', {hour: '2-digit', minute: '2-digit', hour12: true, timeZone: 'America/El_Salvador'}),
      estado: c.estado, // Pendiente, Cancelada, Finalizada
      linkReunion: c.linkReunion,
      tipoConsulta: c.tipoConsulta,
      dui: c.duiPaciente || '',
      telefono: c.telefonoPaciente || ''
    }))
  } catch (e) {
    console.error("Error cargando citas:", e)
  }
})



const copiarLink = async (link) => {
  try {
    await navigator.clipboard.writeText(link)
    alert('Link copiado al portapapeles ✅')
  } catch (e) {
    alert('No se pudo copiar el link')
  }
}

// ===============================
// FILTRO
const citasFiltradas = computed(() => {
  return citas.value.filter(cita => {
    const coincideEstado = cita.estado === pestañaActiva.value
    if (!props.esAdmin) return coincideEstado
    const b = filtroBusqueda.value.toLowerCase()
    return coincideEstado && (
      cita.nombre.toLowerCase().includes(b) ||
      cita.doctor.toLowerCase().includes(b) ||
      cita.dui?.toLowerCase().includes(b) ||
      cita.telefono?.toLowerCase().includes(b)
    )
  })
})


//PARA MOSTRAR FECHA DE LA ULTIMA CONSULTA
const fechaUltimaConsultaFinalizada = computed(() => {
  // Filtramos solo las que ya terminaron
  const terminadas = citas.value.filter(c => c.estado === 'Finalizada')
  
  if (terminadas.length === 0) return null

  // Ordenamos por fecha de forma descendente (la más reciente primero)
  // Nota: Usamos el ID o creamos un objeto Date desde el string para comparar
  const masReciente = terminadas.sort((a, b) => {
    return new Date(b.fecha) - new Date(a.fecha)
  })[0]

  return masReciente.fecha
})




// ===============================
// ACCIONES
const ejecutarCancelar = async (id) => {
  if (!confirm("¿Seguro que desea anular esta cita?")) return
  try {
    await cancelarCitaAPI(id)
    const cita = citas.value.find(c => c.id === id)
    if (cita) cita.estado = 'Cancelada'
  } catch (e) {
    console.error(e)
    alert("Error al cancelar")
  }
}

const descargarPDF = async (id) => {
  try {
    const blob = await descargarReceta(id)
    const url = window.URL.createObjectURL(blob)
    const a = document.createElement('a')
    a.href = url
    a.download = `receta_${id}.pdf`
    a.click()
    window.URL.revokeObjectURL(url)
  } catch (e) {
    console.error(e)
    alert("No se pudo descargar la receta")
  }
}

const accederHistorialActivo = (id = null) => {
  citaSeleccionada.value = id
  accederHistorial.value = !accederHistorial.value
}
</script>

<template>
  <div 
    class="fixed inset-0 z-50 flex items-center justify-center bg-slate-900/60 backdrop-blur-sm p-0 md:p-8"
    @click.self="emit('cerrar')"
  >
    
    <div class="bg-white w-full max-w-[95%] h-full md:h-[90vh] rounded-none md:rounded-[32px] shadow-2xl flex flex-col overflow-hidden border-none md:border md:border-white/20">
      
      <!-- HEADER -->
      <header class="p-3 md:p-10 bg-slate-800 border-b border-slate-100 flex flex-col md:flex-row md:items-center justify-between gap-4 md:gap-6">
        <div class="flex items-center gap-3 md:gap-6">
          <button 
            @click="emit('cerrar')"
            class="flex items-center gap-2 px-3 py-2 md:px-4 md:py-2 bg-white border border-slate-200 text-slate-600 rounded-full hover:bg-slate-50 hover:border-slate-300 transition-all shadow-lg shrink-0"
          >
            ←
            <span class="text-sm font-bold">Regresar</span>
          </button>
          <div class="min-w-0">
            <h3 class="text-white text-xl md:text-3xl font-black tracking-tight truncate">
              {{ props.esAdmin ? 'Gestión de Consultas' : 'Mis Citas Médicas' }}
            </h3>
            <p class="text-white font-medium text-xs md:text-base">
              {{ props.esAdmin ? 'Administra y supervisa el estado de las videollamadas' : 'Historial de tus citas médicas' }}
            </p>
          </div>
        </div>

        <div v-if="props.esAdmin" class="relative w-full md:w-96">
          <input 
            v-model="filtroBusqueda"
            type="text" 
            placeholder="Buscar por paciente o ID..."
            class="w-full pl-12 pr-6 py-3 md:py-4 bg-slate-50 border border-slate-200 focus:border-blue-500 focus:ring-4 focus:ring-blue-500/10 rounded-2xl outline-none transition-all font-medium text-sm"
          />
          <span class="absolute left-4 top-1/2 -translate-y-1/2 text-slate-400">🔍</span>
        </div>
      </header>
      
      <!-- TABS -->
      <nav class="flex bg-slate-800 px-2 md:px-8 mx-2 md:mx-10 mt-4 md:mt-10 md:p-3 rounded-2xl md:rounded-3xl gap-1 md:gap-2 border-2 border-cyan-400">
        <button 
          v-for="tab in ['Pendiente', 'EnConsulta', 'Cancelada', 'Finalizada']" 
          :key="tab"
          @click="pestañaActiva = tab"
          class="flex-1 py-2 md:py-3 rounded-full font-bold text-[9px] md:text-sm transition-all uppercase tracking-wider"
          :class="pestañaActiva === tab 
            ? 'bg-slate-300 text-slate-900 ring-1 ring-slate-200' 
            : 'text-white hover:bg-slate-200/50'"
        >
          {{ tab === 'EnConsulta' ? 'En Curso' : tab }}
        </button>
      </nav>

      <!-- MAIN -->
      <main class="flex-1 px-3 md:px-10 py-4 md:py-6 overflow-y-auto custom-scrollbar">
        
        <div v-if="citasFiltradas.length > 0" class="grid gap-4">
          <div 
            v-for="cita in citasFiltradas" :key="cita.id" 
            class="group bg-white border border-slate-100 rounded-full p-4 md:p-6 flex flex-col hover:shadow-xl hover:shadow-blue-500/5 hover:border-blue-200 transition-all duration-300 lg:grid lg:grid-cols-12 lg:items-center"
          >
            <!-- Nombre / Avatar -->
            <div class="lg:col-span-4 flex items-center gap-4">
              <div class="w-12 h-12 md:w-14 md:h-14 bg-blue-600 text-white rounded-full flex shrink-0 items-center justify-center text-xl font-bold shadow-lg shadow-blue-200">
                {{ cita.nombre.charAt(0) }}
              </div>
              <div class="min-w-0">
                <h4 class="text-slate-900 font-bold text-base md:text-lg truncate">{{ cita.nombre }}</h4>
                <div class="flex items-center gap-2 flex-wrap">
                  <span class="px-2 py-0.5 bg-slate-100 text-slate-500 text-[10px] font-black rounded-md uppercase">#Tel: {{ cita.telefono }}</span>
                  <span v-if="cita.estado === 'EnConsulta'" class="flex items-center gap-1 text-green-600 text-[10px] font-bold animate-pulse">
                    ● En vivo
                  </span>
                </div>
              </div>
            </div>

            <!-- Detalles -->
            <div class="lg:col-span-3 mt-3 lg:mt-0 flex flex-col pt-3 lg:pt-0 border-t lg:border-t-0 lg:border-l lg:pl-8 border-slate-100">
              <span class="text-slate-400 font-bold text-[10px] uppercase tracking-tighter">Detalles de la consulta</span>
              <span class="text-slate-700 font-bold text-sm">Consulta: {{ cita.tipoConsulta }}</span>
              <span class="text-blue-600 font-medium text-xs">DR: {{ cita.doctor }}</span>
              <span class="text-blue-600 font-medium text-xs">Estado: {{ cita.estado }}</span>
              <span class="text-blue-600 font-medium text-xs">ID cita: {{ cita.id }}</span>
            </div>

            <!-- Fecha -->
            <div class="lg:col-span-2 mt-3 lg:mt-0 flex flex-col pt-3 lg:pt-0 border-t lg:border-t-0 lg:border-l lg:pl-8 border-slate-100">
              <span class="text-slate-400 font-bold text-[10px] uppercase tracking-tighter">Fecha y Hora</span>
              <span class="text-slate-700 font-bold text-sm">{{ cita.fecha }}</span>
              <span class="text-slate-500 font-medium text-xs">{{ cita.hora }}</span>
            </div>

            <!-- Acciones -->
            <div class="lg:col-span-3 flex items-center justify-start lg:justify-end gap-2 mt-4 lg:mt-0 pt-3 lg:pt-0 border-t lg:border-t-0 border-slate-100 flex-wrap">
              
              <template v-if="cita.estado === 'Pendiente' || cita.estado === 'EnConsulta'">
                
                <button 
                  v-if="props.esAdmin"
                  @click="copiarLink(cita.linkReunion)"
                  class="px-4 py-2.5 bg-cyan-50 text-cyan-700 rounded-full text-xs font-black hover:bg-cyan-100 transition-colors"
                >
                  COPIAR ENLACE
                </button>

                <span
                  v-if="!props.esAdmin && cita.estado === 'Pendiente'"
                  class="px-4 py-2.5 rounded-full text-xs font-black text-center bg-blue-100 text-blue-400 cursor-not-allowed select-none"
                  title="El doctor debe iniciar la consulta primero"
                >
                  🕐 PRÓXIMO A INICIAR
                </span>

                <a 
                  v-if="!props.esAdmin && cita.estado === 'EnConsulta' && cita.linkReunion"
                  :href="cita.linkReunion"
                  target="_blank"
                  rel="noopener noreferrer"
                  class="px-6 py-2.5 rounded-full text-xs font-black text-center bg-green-600 text-white hover:bg-green-700 transition-all shadow-md shadow-green-200 animate-pulse"
                >
                  🟢 UNIRSE AHORA
                </a>

                <button 
                  v-if="cita.estado === 'Pendiente'"
                  @click="ejecutarCancelar(cita.id)"
                  class="p-2.5 text-red-400 hover:text-red-600 hover:bg-red-50 rounded-full transition-all"
                  title="Cancelar Cita"
                >
                  ✕
                </button>
              </template>

              <template v-if="cita.estado === 'Finalizada'">
                <button @click="descargarPDF(cita.id)"
                 class="flex items-center gap-2 px-4 py-2.5 bg-green-600 text-white rounded-full font-bold text-xs hover:bg-green-700 transition-all shadow-md shadow-green-200">
                  📄 Receta PDF
                </button>
              </template>

              <div v-if="cita.estado === 'Cancelada'" class="px-4 py-2 bg-red-50 text-red-400 rounded-full font-bold text-[10px] uppercase">
                Anulada
              </div>
            </div>

          </div>
        </div>

        <div v-else class="h-full flex flex-col items-center justify-center py-20">
          <div class="w-24 h-24 bg-slate-50 rounded-full flex items-center justify-center text-4xl mb-6 grayscale opacity-50">📂</div>
          <h4 class="text-slate-400 font-bold uppercase tracking-[0.2em] text-sm">No se encontraron registros</h4>
        </div>
      </main>
    </div>
  </div>
</template>