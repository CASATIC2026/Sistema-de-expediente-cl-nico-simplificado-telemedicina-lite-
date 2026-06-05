<script setup>
import { ref, onMounted } from 'vue'
import {
  getMisOrdenesLaboratorio,
  descargarOrdenLaboratorio,
  subirResultadoLaboratorio
} from '@/services/api'

const emit = defineEmits(['cerrar'])

const ordenes   = ref([])
const loading   = ref(false)
const error     = ref(null)

// Por cada orden guardamos su estado de subida local
const subiendo  = ref({})
const archivos  = ref({})



const toast = ref({ visible: false, mensaje: '', tipo: 'success' })
let toastTimer = null
const mostrarToast = (mensaje, tipo = 'success') => {
  if (toastTimer) clearTimeout(toastTimer)
  toast.value = { visible: true, mensaje, tipo }
  toastTimer = setTimeout(() => { toast.value.visible = false }, 3500)
}

onMounted(async () => {
  loading.value = true
  try {
    const data = await getMisOrdenesLaboratorio()
    ordenes.value = data.map(o => ({
      citaId:           o.citaId,
      consultaId:       o.consultaId,
      fecha:            new Date(o.fecha).toLocaleDateString('es-SV'),
      tipoConsulta:     o.tipoConsulta,
      doctor:           o.doctor,
      estadoOrden:      o.estadoOrden,
      observaciones:    o.observacionesLaboratorio
    }))
  } catch (e) {
    error.value = 'No se pudieron cargar tus órdenes de laboratorio.'
    console.error(e)
  } finally {
    loading.value = false
  }
})

const onArchivoSeleccionado = (consultaId, event) => {
  const file = event.target.files[0]
  if (!file) return
  if (file.type !== 'application/pdf') {
    mostrarToast('Solo se aceptan archivos PDF', 'error')
    event.target.value = ''
    return
  }
  if (file.size > 5 * 1024 * 1024) {
    mostrarToast('El archivo no puede superar los 5MB', 'error')
    event.target.value = ''
    return
  }
  archivos.value[consultaId] = file
}

const subirResultado = async (orden) => {
  const archivo = archivos.value[orden.consultaId]
  if (!archivo) return

  subiendo.value[orden.consultaId] = true
  try {
    await subirResultadoLaboratorio(orden.consultaId, archivo)
    // Actualizar estado local sin recargar
    const found = ordenes.value.find(o => o.consultaId === orden.consultaId)
    if (found) {
      found.estadoOrden   = 'ResultadosSubidos'
      found.nombreArchivo = archivo.name
    }
    delete archivos.value[orden.consultaId]
  } catch (e) {
    mostrarToast('Error al subir los resultados. Intenta de nuevo.', 'error')
    console.error(e)
  } finally {
    subiendo.value[orden.consultaId] = false
  }
}

const descargarOrden = async (consultaId) => {
  try {
    const blob = await descargarOrdenLaboratorio(consultaId)
    const url  = window.URL.createObjectURL(blob)
    const a    = document.createElement('a')
    a.href     = url
    a.download = `orden_laboratorio_${consultaId}.pdf`
    a.click()
    window.URL.revokeObjectURL(url)
  } catch (e) {
    mostrarToast('Error al descargar la orden', 'error')
    console.error(e)
  }
}

const etiquetaEstado = (estado) => {
  switch (estado) {
    case 'Pendiente':         return { texto: '⏳ Pendiente de subir resultados', clase: 'bg-yellow-50 text-yellow-700 border-yellow-200' }
    case 'ResultadosSubidos': return { texto: '📤 Resultados enviados — En revisión médica', clase: 'bg-blue-50 text-blue-700 border-blue-200' }
    case 'Revisado':          return { texto: '✅ Revisado por el doctor', clase: 'bg-emerald-50 text-emerald-700 border-emerald-200' }
    default:                  return { texto: estado, clase: 'bg-slate-50 text-slate-500 border-slate-200' }
  }
}
</script>

<template>
  <div
    class="fixed inset-0 z-50 flex items-center justify-center bg-slate-900/60 backdrop-blur-sm p-4"
    @click.self="emit('cerrar')"
  >
    <div class="bg-white w-full max-w-3xl max-h-[90vh] rounded-3xl shadow-2xl flex flex-col overflow-hidden">

      <!-- HEADER -->
      <header class="bg-slate-800 px-6 py-5 flex items-center justify-between shrink-0">
        <div>
          <h2 class="text-white text-2xl font-black">🔬 Mis Exámenes de Laboratorio</h2>
          <p class="text-cyan-400 text-sm mt-1">Gestiona tus órdenes y sube tus resultados</p>
        </div>
        <button
          @click="emit('cerrar')"
          class="text-white bg-white/10 hover:bg-red-500 w-9 h-9 rounded-full text-xl flex items-center justify-center transition-colors"
        >
          &times;
        </button>
      </header>

      <!-- BODY -->
      <div class="flex-1 overflow-y-auto p-6 space-y-5">

        <!-- LOADING -->
        <div v-if="loading" class="flex flex-col items-center py-20 text-slate-400">
          <div class="w-10 h-10 border-4 border-teal-500 border-t-transparent rounded-full animate-spin mb-4"></div>
          <p class="font-bold">Cargando tus órdenes...</p>
        </div>

        <!-- ERROR -->
        <div v-else-if="error" class="text-center py-20 text-red-500 font-bold">
          {{ error }}
        </div>

        <!-- SIN ÓRDENES -->
        <div v-else-if="ordenes.length === 0" class="text-center py-20 text-slate-400">
          <div class="text-5xl mb-4">🔬</div>
          <p class="font-bold">No tienes órdenes de laboratorio</p>
          <p class="text-sm mt-1">Cuando tu médico genere una orden aparecerá aquí</p>
        </div>

        <!-- LISTA DE ÓRDENES -->
        <div v-else class="space-y-5">
          <div
            v-for="orden in ordenes"
            :key="orden.consultaId"
            class="border border-slate-200 rounded-2xl overflow-hidden"
          >
            <!-- Cabecera de la orden -->
            <div class="bg-slate-50 px-5 py-4 flex flex-col sm:flex-row sm:items-center justify-between gap-3">
              <div>
                <p class="font-bold text-slate-800">Consulta #{{ orden.consultaId }}</p>
                <p class="text-sm text-slate-500">{{ orden.fecha }} · {{ orden.tipoConsulta }}</p>
                <p class="text-sm text-slate-500">Dr(a). {{ orden.doctor }}</p>
              </div>
              <button
                @click="descargarOrden(orden.consultaId)"
                class="flex items-center gap-2 px-4 py-2 bg-teal-600 hover:bg-teal-700 text-white rounded-xl text-sm font-bold transition-colors shrink-0"
              >
                🔬 Descargar orden PDF
              </button>
            </div>

            <!-- Estado -->
            <div class="px-5 py-4 space-y-4">
              <div
                :class="['border rounded-xl px-4 py-3 text-sm font-semibold', etiquetaEstado(orden.estadoOrden).clase]"
              >
                {{ etiquetaEstado(orden.estadoOrden).texto }}
              </div>

              <!-- PENDIENTE — subir resultados -->
              <div v-if="orden.estadoOrden === 'Pendiente'" class="space-y-3">
                <p class="text-sm text-slate-600">
                  Cuando el laboratorio te entregue tus resultados en PDF, súbelos aquí para que tu médico los revise:
                </p>

                <div class="border-2 border-dashed border-slate-300 rounded-xl p-4 text-center">
                  <input
                    :id="`archivo-${orden.consultaId}`"
                    type="file"
                    accept="application/pdf"
                    class="hidden"
                    @change="onArchivoSeleccionado(orden.consultaId, $event)"
                  />
                  <label
                    :for="`archivo-${orden.consultaId}`"
                    class="cursor-pointer flex flex-col items-center gap-2 text-slate-500 hover:text-teal-600 transition-colors"
                  >
                    <span class="text-3xl">📁</span>
                    <span class="text-sm font-medium">
                      {{
                        archivos[orden.consultaId]
                          ? archivos[orden.consultaId].name
                          : 'Seleccionar archivo PDF (Máx. 5MB)'
                      }}
                    </span>
                  </label>
                </div>

                <button
                  @click="subirResultado(orden)"
                  :disabled="!archivos[orden.consultaId] || subiendo[orden.consultaId]"
                  class="w-full py-3 bg-teal-600 hover:bg-teal-700 text-white rounded-xl font-bold text-sm disabled:opacity-40 disabled:cursor-not-allowed transition-colors"
                >
                  {{ subiendo[orden.consultaId] ? 'Enviando...' : '📤 Enviar resultados al médico' }}
                </button>
              </div>

              <!-- RESULTADOS SUBIDOS — esperando revisión -->
              <div
                v-else-if="orden.estadoOrden === 'ResultadosSubidos'"
                class="bg-blue-50 border border-blue-200 rounded-xl p-4 text-sm text-blue-700"
              >
                <p class="font-bold mb-1">📤 Resultados recibidos</p>
                <p>Tu médico revisará tus resultados próximamente. Te notificaremos por correo cuando estén listos.</p>
              </div>

              <!-- REVISADO — observaciones del doctor -->
              <div
                v-else-if="orden.estadoOrden === 'Revisado'"
                class="bg-emerald-50 border border-emerald-200 rounded-xl p-4 space-y-2"
              >
                <p class="text-emerald-700 font-bold text-sm">✅ Tu médico revisó tus resultados</p>
                <p class="text-slate-700 text-sm font-medium">Observaciones del Dr(a). {{ orden.doctor }}:</p>
                <div class="bg-white border border-emerald-100 rounded-lg p-3 text-slate-700 text-sm">
                  {{ orden.observaciones || 'Sin observaciones adicionales.' }}
                </div>
              </div>

            </div>
          </div>
        </div>

      </div>
    </div>
    <!-- TOAST -->
    <Transition
      enter-active-class="transition-all duration-300 ease-out"
      enter-from-class="opacity-0 translate-y-4 scale-95"
      enter-to-class="opacity-100 translate-y-0 scale-100"
      leave-active-class="transition-all duration-200 ease-in"
      leave-from-class="opacity-100 translate-y-0 scale-100"
      leave-to-class="opacity-0 translate-y-4 scale-95"
    >
      <div
        v-if="toast.visible"
        class="fixed bottom-6 right-6 z-[99999] flex items-center gap-3 px-5 py-4 rounded-2xl shadow-2xl border backdrop-blur-sm min-w-[280px] max-w-sm"
        :class="toast.tipo === 'success'
          ? 'bg-emerald-950/90 border-emerald-500/30 text-emerald-300'
          : 'bg-red-950/90 border-red-500/30 text-red-300'"
      >
        <div
          class="shrink-0 w-8 h-8 rounded-xl flex items-center justify-center"
          :class="toast.tipo === 'success' ? 'bg-emerald-500/20' : 'bg-red-500/20'"
        >
          <svg v-if="toast.tipo === 'success'" xmlns="http://www.w3.org/2000/svg" class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2.5">
            <path stroke-linecap="round" stroke-linejoin="round" d="M5 13l4 4L19 7" />
          </svg>
          <svg v-else xmlns="http://www.w3.org/2000/svg" class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2.5">
            <path stroke-linecap="round" stroke-linejoin="round" d="M6 18L18 6M6 6l12 12" />
          </svg>
        </div>

        <div class="flex-1">
          <p class="text-[11px] font-black uppercase tracking-widest mb-0.5"
            :class="toast.tipo === 'success' ? 'text-emerald-400' : 'text-red-400'">
            {{ toast.tipo === 'success' ? 'Éxito' : 'Error' }}
          </p>
          <p class="text-xs font-semibold leading-snug">{{ toast.mensaje }}</p>
        </div>

        <button
          @click="toast.visible = false"
          class="shrink-0 w-6 h-6 rounded-lg flex items-center justify-center opacity-50 hover:opacity-100 transition-opacity"
          :class="toast.tipo === 'success' ? 'hover:bg-emerald-500/20' : 'hover:bg-red-500/20'"
        >
          &times;
        </button>

        <div class="absolute bottom-0 left-0 h-[2px] rounded-full w-full overflow-hidden">
          <div
            class="h-full rounded-full"
            :class="toast.tipo === 'success' ? 'bg-emerald-400' : 'bg-red-400'"
            style="animation: shrink 3.5s linear forwards"
          />
        </div>
      </div>
    </Transition>
  </div>
</template>
<style scoped>
@keyframes shrink {
  from { width: 100%; }
  to   { width: 0%; }
}
</style>