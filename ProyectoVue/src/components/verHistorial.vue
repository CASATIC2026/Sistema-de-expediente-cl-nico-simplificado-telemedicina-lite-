<script setup>
import { ref, onMounted } from 'vue'
import { getMisCitasDetalle, getHistorialPaciente, descargarRecetaAdmin } from '@/services/api'

const props = defineProps({
  esAdmin:             { type: Boolean, default: false },
  esDoctor:             { type: Boolean, default: false },
  rolVisor:            { type: String,  default: '' },
  usuarioSeleccionado: { type: Object,  default: () => null }
})

const emit = defineEmits(['cerrar'])

const consultaSeleccionada = ref(null)
const historial            = ref([])
const loading              = ref(false)
const error                = ref(null)


onMounted(async () => {
  loading.value = true
  error.value   = null
  try {
    let data = []

    if ((props.esAdmin || props.esDoctor) && props.usuarioSeleccionado?.id) {
      // Doctor o admin viendo historial de un paciente
      data = await getHistorialPaciente(props.usuarioSeleccionado.id)
    } else {
      // Paciente viendo su propio historial
      data = await getMisCitasDetalle()
    }

    historial.value = data.map(c => ({
      id:           c.idCita,
      fecha:        new Date(c.fechaInicio).toLocaleDateString('es-SV'),
      doctor:       c.doctor       || 'Sin asignar',
      tipoConsulta: c.tipoConsulta || '—',
      motivo:       c.motivo       || '—',
      diagnostico:  c.consulta?.Diagnostico || c.consulta?.diagnostico || 'Sin diagnóstico registrado',
      sintomas:     c.consulta?.Sintomas    || c.consulta?.sintomas    || '',
      tratamiento:  c.consulta?.Tratamiento || c.consulta?.tratamiento || '',
      estado:       c.estado       || '—',
      observaciones: c.consulta?.Observaciones || c.consulta?.observaciones || ''
    }))
  } catch (e) {
    console.error(e)
    error.value = 'No se pudo cargar el historial'
  } finally {
    loading.value = false
  }
})

const verDetalle = (consulta) => {
  consultaSeleccionada.value = consulta
}

const descargando = ref(false)

const descargarRecetaPaciente = async (citaId) => {
  descargando.value = true
  try {
    const blob = await descargarRecetaAdmin(citaId)
    const url  = window.URL.createObjectURL(blob)
    const a    = document.createElement('a')
    a.href     = url
    a.download = `receta_${citaId}.pdf`
    a.click()
    window.URL.revokeObjectURL(url)
  } catch (e) {
    console.error(e)
    alert('No se pudo descargar la receta.')
  } finally {
    descargando.value = false
  }
}




</script>

<template>
  <div class="fixed inset-0 z-[99999] flex items-center justify-center bg-slate-900/80 backdrop-blur-sm p-4"
       @click.self="emit('cerrar')">

    <div class="relative w-full max-w-4xl max-h-[90vh] rounded-2xl bg-gradient-to-br from-[#023c43] to-[#047a85] shadow-2xl flex flex-col text-white border border-white/10">

      <!-- CERRAR -->
      <button
        @click="emit('cerrar')"
        class="absolute top-4 right-4 z-20 flex h-10 w-10 items-center justify-center rounded-full bg-white/10 hover:bg-red-500 text-2xl"
      >
        &times;
      </button>

      <!-- HEADER -->
      <header class="p-6 md:p-8 pb-2 shrink-0">
        <h2 v-if="!consultaSeleccionada" class="text-2xl md:text-3xl font-bold">
          {{ esAdmin && usuarioSeleccionado
              ? `Historial de ${usuarioSeleccionado.nombre}`
              : 'Mi Historial Médico' }}
        </h2>
        <div v-else>
          <button @click="consultaSeleccionada = null"
            class="mb-2 text-sm bg-white/10 px-4 py-2 rounded-lg">
            ← Regresar
          </button>
          <h3 class="text-xl font-bold">
            Consulta #{{ consultaSeleccionada.id }}
          </h3>
        </div>
      </header>

      <!-- BODY -->
      <div class="flex-1 overflow-y-auto p-6 space-y-6 min-h-0">

        <!-- LOADING -->
        <div v-if="loading" class="text-center py-20 text-white/60">
          Cargando historial...
        </div>

        <!-- ERROR -->
        <div v-else-if="error" class="text-center py-20 text-red-300">
          {{ error }}
        </div>

        <!-- LISTADO -->
        <div v-else-if="!consultaSeleccionada && historial.length > 0" class="space-y-4">
          <div v-for="consulta in historial" :key="consulta.id"
               class="p-5 rounded-xl bg-white/5 hover:bg-white/10 flex justify-between items-center">
            <div>
              <p class="text-sm text-cyan-300">Fecha: {{ consulta.fecha }}   ||   Tipo consulta : {{ consulta.tipoConsulta }}  ||   Estado: {{ consulta.estado }} </p>
              <p class="text-sm text-cyan-300">Motivo consulta: {{consulta.motivo}}</p>
            </div>
            <button @click="verDetalle(consulta)"
                    class="bg-cyan-500 px-4 py-2 rounded-lg">
              Ver
            </button>
          </div>
        </div>

        <!-- SIN DATOS -->
        <div v-else-if="!consultaSeleccionada && historial.length === 0"
             class="text-center text-white/40 py-20">
          No hay historial disponible
        </div>

        <!-- DETALLE -->
        <div v-else-if="consultaSeleccionada" class="space-y-6">

          <div class="grid md:grid-cols-2 gap-4">
            <div class="bg-black/20 p-4 rounded-xl">
              <p><strong>Paciente:</strong>
                {{ (esAdmin || esDoctor ) && usuarioSeleccionado ? usuarioSeleccionado.nombre : 'Tú' }}
              </p>
            </div>
            <div class="bg-black/20 p-4 rounded-xl">
              <p><strong>Doctor:</strong> {{ consultaSeleccionada.doctor }}</p>
            </div>
          </div>

          <!-- Síntomas -->
          <div v-if="consultaSeleccionada.sintomas">
            <h4 class="text-cyan-300 font-bold mb-2">Síntomas</h4>
            <div class="bg-black/20 p-4 rounded-xl text-white/80">
              {{ consultaSeleccionada.sintomas }}
            </div>
          </div>

          <!-- Diagnóstico (doctor y paciente sí, admin no) -->
          <div>
            <h4 class="text-cyan-300 font-bold mb-2">Diagnóstico</h4>
            <div v-if="rolVisor !== 'admin'" class="bg-white text-black p-4 rounded-xl">
              {{ consultaSeleccionada.diagnostico || 'Sin diagnóstico registrado' }}
            </div>
            <div v-else class="text-white/40 italic">
              🔒 Información restringida
            </div>
          </div>

          <!-- Tratamiento (doctor y paciente sí, admin no) -->
          <div v-if="consultaSeleccionada.tratamiento && rolVisor !== 'admin'">
            <h4 class="text-cyan-300 font-bold mb-2">Tratamiento</h4>
            <div class="bg-black/20 p-4 rounded-xl text-white/80">
              {{ consultaSeleccionada.tratamiento }}
            </div>
          </div>

          <!-- Observaciones -->
          <div v-if="consultaSeleccionada.observaciones && rolVisor !== 'admin'">
            <h4 class="text-cyan-300 font-bold mb-2">Observaciones</h4>
            <div class="bg-black/20 p-4 rounded-xl text-white/80">
              {{ consultaSeleccionada.observaciones }}
            </div>
          </div>

          <!-- Botón receta -->
          <button
            v-if="consultaSeleccionada.id"
            @click="descargarRecetaPaciente(consultaSeleccionada.id)"
            :disabled="descargando"
            class="w-full text-center bg-emerald-500 hover:bg-emerald-600 p-4 rounded-xl font-bold transition-all disabled:opacity-50">
            {{ descargando ? 'Descargando...' : 'Descargar receta' }}
          </button>

        </div>

      </div>
    </div>
  </div>
</template>