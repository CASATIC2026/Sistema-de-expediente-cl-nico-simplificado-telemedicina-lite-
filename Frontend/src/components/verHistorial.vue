<script setup>
import { ref, onMounted, computed } from 'vue'
import { getMisCitasDetalle, getHistorialPaciente, descargarRecetaAdmin, revisarLaboratorio, descargarResultadoPdf,
  descargarOrdenLaboratorio } from '@/services/api'
import {
  ClipboardDocumentListIcon,
  PaperAirplaneIcon,
  PencilSquareIcon,
  ArrowDownTrayIcon,
  XMarkIcon
} from '@heroicons/vue/24/outline'


const props = defineProps({
  esDoctor:            { type: Boolean, default: false },
  usuarioSeleccionado: { type: Object,  default: () => null }
})

const emit = defineEmits(['cerrar'])

const consultaSeleccionada = ref(null)
const pestañaActiva        = ref('resumen')
const historial            = ref([])
const loading              = ref(false)
const error                = ref(null)
const descargando          = ref(false)
const observacionesDoctor  = ref('')
const enviandoRevision     = ref(false)

// Toast
const toast = ref({ visible: false, mensaje: '', tipo: 'success' })
let toastTimer = null
const mostrarToast = (mensaje, tipo = 'success') => {
  if (toastTimer) clearTimeout(toastTimer)
  toast.value = { visible: true, mensaje, tipo }
  toastTimer = setTimeout(() => { toast.value.visible = false }, 3500)
}

// Obtener avatar de usuario
const obtenerAvatar = (usuario) => {
  if (!usuario?.avatarIcono) return null

  try {
    return new URL(`/src/assets/Profile-Icons/${usuario.avatarIcono}`, import.meta.url).href
  } catch {
    return null
  }
}

onMounted(async () => {
  loading.value = true
  error.value   = null
  try {
    let data = []
    if (props.esDoctor && props.usuarioSeleccionado?.id) {
      data = await getHistorialPaciente(props.usuarioSeleccionado.id)
    } else {
      data = await getMisCitasDetalle()
    }

    historial.value = data.map(c => ({
      id:                      c.idCita,
      fecha:                   c.fechaInicio
      ? new Date(c.fechaInicio).toLocaleDateString('es-SV')
      : 'Sin fecha',
      doctor:                  c.doctor       || 'Sin asignar',
      tipoConsulta:            c.tipoConsulta || '—',
      motivo:                  c.motivo       || '—',
      estado:                  c.estado       || '—',

      // datos clínicos
      consultaId:              c.consulta?.idConsulta       || c.consulta?.IdConsulta       || null,
      diagnostico:             c.consulta?.diagnostico      || c.consulta?.Diagnostico      || 'Sin diagnóstico / Consulta pendiente',
      sintomas:                c.consulta?.sintomas         || c.consulta?.Sintomas         || '',
      tratamiento:             c.consulta?.tratamiento      || c.consulta?.Tratamiento      || '',
      observaciones:           c.consulta?.observaciones    || c.consulta?.Observaciones    || '',
      medicamentosJson:        c.consulta?.medicamentosJson || c.consulta?.MedicamentosJson || '',

      // laboratorio
      tieneOrdenLaboratorio:       c.consulta?.tieneOrdenLaboratorio       || c.consulta?.TieneOrdenLaboratorio       || false,
      estadoOrden:                 c.consulta?.estadoOrden                 || c.consulta?.EstadoOrden                 || '',
      observacionesLaboratorio:    c.consulta?.observacionesLaboratorio    || c.consulta?.ObservacionesLaboratorio    || '',
      resultadoPdfPath:            c.consulta?.resultadoPdfPath            || c.consulta?.ResultadoPdfPath            || null
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
  pestañaActiva.value        = 'resumen'
  observacionesDoctor.value  = ''
}

const medicamentosParsed = computed(() => {
  if (!consultaSeleccionada.value?.medicamentosJson) return []
  try {
    return JSON.parse(consultaSeleccionada.value.medicamentosJson)
  } catch { return [] }
})

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
    mostrarToast('No se pudo descargar la receta.', 'error')
    console.error(e)
  } finally {
    descargando.value = false
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
    mostrarToast('No se pudo descargar la orden.', 'error')
    console.error(e)
  }
}

const descargarResultados = async (consultaId) => {
  try {
    const blob = await descargarResultadoPdf(consultaId)
    const url  = window.URL.createObjectURL(blob)
    const a    = document.createElement('a')
    a.href     = url
    a.download = `resultados_${consultaId}.pdf`
    a.click()
    window.URL.revokeObjectURL(url)
  } catch (e) {
    mostrarToast('No se pudo descargar el archivo de resultados.', 'error')
    console.error(e)
  }
}

const enviarRevision = async () => {
  if (!observacionesDoctor.value.trim()) {
    mostrarToast('Debes escribir tus observaciones antes de enviar.', 'error')
    return
  }
  enviandoRevision.value = true
  if (!consultaSeleccionada.value?.consultaId) {
  mostrarToast('Consulta inválida.', 'error')
  return
}
  try {
    await revisarLaboratorio(
      consultaSeleccionada.value.consultaId,
      observacionesDoctor.value
    )
    // Actualizar estado local
    consultaSeleccionada.value.estadoOrden              = 'Revisado'
    consultaSeleccionada.value.observacionesLaboratorio = observacionesDoctor.value

    // Actualizar también en la lista
    const enLista = historial.value.find(h => h.id === consultaSeleccionada.value.id)
    if (enLista) {
      enLista.estadoOrden              = 'Revisado'
      enLista.observacionesLaboratorio = observacionesDoctor.value
    }

    mostrarToast('Revisión enviada. El paciente será notificado por correo.', 'success')
  } catch (e) {
    mostrarToast('Error al enviar la revisión.', 'error')
    console.error(e)
  } finally {
    enviandoRevision.value = false
  }
}

const etiquetaEstadoOrden = (estado) => {
  switch (estado) {
    case 'Pendiente':         return { texto: '⏳ Esperando resultados del paciente', clase: 'bg-yellow-50 dark:bg-yellow-950/40 text-yellow-700 dark:text-yellow-600 border-yellow-200 dark:border-yellow-900/50' }
    case 'ResultadosSubidos': return { texto: '📥 Resultados recibidos — Pendiente de revisión', clase: 'bg-blue-50 dark:bg-blue-950/40 text-blue-700 dark:text-blue-600 border-blue-200 dark:border-blue-900/50' }
    case 'Revisado':          return { texto: '✅ Revisado y enviado al paciente', clase: 'bg-emerald-50 dark:bg-emerald-950/40 text-emerald-700 dark:text-emerald-600 border-emerald-200 dark:border-emerald-900/50' }
    default:                  return { texto: '—', clase: 'bg-slate-50 dark:bg-slate-950/40 text-slate-400 border-slate-200 dark:border-slate-800/50' }
  }
}
</script>

<template>
  <div
   class="fixed inset-0 z-99999 max-h-full flex items-center justify-center bg-white/30 backdrop-blur-md dark:bg-slate-950/80 p-0"
    @click.self="emit('cerrar')"
    >
    <!--Contenedor principal-->
    <div class="relative w-full h-full md:max-h-full md:max-w-full rounded-none md:rounded-2xl bg-white/55 backdrop-blur-xl dark:bg-slate-900 shadow-2xl flex flex-col border-0 md:border md:border-slate-200 dark:border-slate-800/60 overflow-hidden">

      <!-- Botón cerrar -->
      <button
  @click="emit('cerrar')"
  class="absolute top-5 right-5 z-20
  w-11 h-11 rounded-full
  bg-white/70 backdrop-blur-md
  border border-slate-200/70
  dark:bg-slate-900/70 dark:border-slate-700/60

  flex items-center justify-center

  text-slate-600 dark:text-slate-300

  shadow-md shadow-slate-900/10
  hover:bg-red-500 hover:text-white hover:border-red-500
  hover:shadow-[0_0_20px_rgba(239,68,68,0.45)]

  transition-all duration-200
  cursor-pointer"
  title="Cerrar"
>
  <XMarkIcon class="w-5 h-5" />
</button>

      <!-- Layout principal: sidebar + contenido -->
      <div class="flex flex-col md:flex-row flex-1 overflow-hidden min-h-0">

        <!-- Sidebar-->
        <aside class="w-full md:w-56 shrink-0 bg-[#071120] border-b md:border-b-0 md:border-r border-slate-800/60 flex flex-row md:flex-col p-4 md:p-6 gap-4 md:gap-7">

          <!-- Regresar -->
          <button
            v-if="consultaSeleccionada"
            @click="consultaSeleccionada = null"
            class="flex items-center gap-1.5 text-[14px] font-bold text-slate-300 hover:bg-slate-400 uppercase tracking-wider transition-colors cursor-pointer whitespace-nowrap ml-auto md:ml-0 px-3 py-1 rounded-lg border border-slate-700 hover:border-slate-500"
          >
            ← Regresar
          </button>
          <div v-else class="hidden md:block">
            <p class="text-[10px] font-bold uppercase tracking-widest text-slate-600">Historial</p>
          </div>

          <!-- Paciente -->
          <div class="space-y-2">
            <p class="text-[14px] font-bold uppercase tracking-widest text-slate-500">Paciente</p>
            <div class="flex items-center gap-2.5">

              <div class="w-9 h-9 rounded-full overflow-hidden border border-cyan-500/30 bg-cyan-500/10 shrink-0 flex items-center justify-center">
              <img
                v-if="obtenerAvatar(usuarioSeleccionado)"
                :src="obtenerAvatar(usuarioSeleccionado)"
                alt="Avatar"
                class="w-full h-full object-cover"
              />

              <span
                v-else
                class="text-cyan-400 text-xs font-bold"
              >
                {{ usuarioSeleccionado ? usuarioSeleccionado.nombre?.charAt(0).toUpperCase() : 'P' }}
              </span>
            </div>

              <p class="text-sm font-semibold text-white leading-snug">
                {{ esDoctor && usuarioSeleccionado
                  ? usuarioSeleccionado.nombre + ' ' + usuarioSeleccionado.apellido
                  : 'Mi historial' }}
              </p>
            </div>
          </div>

          <!-- Doctor (solo en detalle) -->
          <div v-if="consultaSeleccionada" class="space-y-1.5">
            <p class="text-[10px] font-bold uppercase tracking-widest text-slate-500">Doctor</p>
            <p class="text-sm font-semibold text-white leading-snug">{{ consultaSeleccionada.doctor }}</p>
          </div>

        </aside>

        <!-- ── CONTENIDO PRINCIPAL ── -->
        <div class="flex-1 flex flex-col overflow-hidden min-w-0">

          <!-- Header con título y pestañas (detalle) -->
          <header v-if="consultaSeleccionada" class="px-6 md:px-8 pt-6 pb-0 border-b border-slate-200 dark:border-slate-800/50 shrink-0">
            <h2 class="text-base md:text-lg font-black text-gray-900 dark:text-white uppercase tracking-tight mb-4">
              Consulta #{{ consultaSeleccionada.consultaId }}
              <span class="text-gray-400 dark:text-slate-600 font-normal mx-1.5">—</span>
              <span class="text-gray-600 dark:text-slate-300 font-semibold">{{ consultaSeleccionada.fecha }}</span>
            </h2>

            <!-- Pestañas underline -->
            <div class="flex gap-0 -mb-px overflow-x-auto">
              <button
                @click="pestañaActiva = 'resumen'"
                :class="[
                  'flex items-center gap-1.5 px-4 py-2.5 text-[14px] font-bold uppercase tracking-wider border-b-2 transition-all cursor-pointer whitespace-nowrap',
                  pestañaActiva === 'resumen'
                    ? 'border-cyan-400 text-cyan-600 dark:border-cyan-400 dark:text-cyan-400'
                    : 'border-transparent text-gray-500 dark:text-slate-500 hover:text-gray-700 dark:hover:text-slate-300'
                ]"
              >
                <ClipboardDocumentListIcon class="w-4 h-4" />
                  Resumen clínico
              </button>
              <button
                @click="pestañaActiva = 'medicamentos'"
                :class="[
                  'flex items-center gap-1.5 px-4 py-2.5 text-[14px] font-bold uppercase tracking-wider border-b-2 transition-all cursor-pointer whitespace-nowrap',
                  pestañaActiva === 'medicamentos'
                    ? 'border-cyan-400 text-cyan-600 dark:border-cyan-400 dark:text-cyan-400'
                    : 'border-transparent text-gray-500 dark:text-slate-500 hover:text-gray-700 dark:hover:text-slate-300'
                ]"
              >
                💊 Medicamentos
              </button>
              <button
                v-if="consultaSeleccionada.tieneOrdenLaboratorio"
                @click="pestañaActiva = 'laboratorio'"
                :class="[
                  'relative flex items-center gap-1.5 px-4 py-2.5 text-[14px] font-bold uppercase tracking-wider border-b-2 transition-all cursor-pointer whitespace-nowrap',
                  pestañaActiva === 'laboratorio'
                    ? 'border-cyan-400 text-cyan-600 dark:border-cyan-400 dark:text-cyan-400'
                    : 'border-transparent text-gray-500 dark:text-slate-500 hover:text-gray-700 dark:hover:text-slate-300'
                ]"
              >
                🔬 Laboratorio
                <span
                  v-if="consultaSeleccionada.estadoOrden === 'ResultadosSubidos'"
                  class="absolute top-1.5 right-1 bg-red-500 text-white text-[9px] font-black w-3.5 h-3.5 flex items-center justify-center rounded-full animate-pulse"
                >!</span>
              </button>
            </div>
          </header>

          <!-- Header lista -->
          <header v-else class="px-6 md:px-8 pt-6 pb-5 border-b border-white/30 dark:border-slate-800/50 shrink-0 bg-trasparent backdrop-blur-md">
            <h2 class="text-lg md:text-xl font-black text-slate-700 dark:text-white uppercase tracking-tight">
              {{ esDoctor && usuarioSeleccionado
                ? `Historial de ${usuarioSeleccionado.nombre} ${usuarioSeleccionado.apellido}`
                : 'Mi historial médico' }}
            </h2>
          </header>

          <!-- Cuerpo scrollable -->
          <div class="flex-1 overflow-y-auto p-6 md:p-8 space-y-4 min-h-0 custom-scrollbar bg-transparent dark:bg-slate-950/30">

            <!-- Loading -->
            <div v-if="loading" class="text-center py-24 text-cyan-600 dark:text-cyan-400 text-sm font-bold animate-pulse uppercase tracking-widest">
              Cargando historial...
            </div>

            <!-- Error -->
            <div v-else-if="error" class="text-center py-20 text-red-600 dark:text-red-400 text-sm font-bold bg-red-50 dark:bg-red-500/5 rounded-xl border border-red-200 dark:border-red-500/20">
              ⚠️ {{ error }}
            </div>

            <!-- ── LISTA DE CONSULTAS ── -->
            <div v-else-if="!consultaSeleccionada && historial.length > 0" class="space-y-3">
              <div
                v-for="consulta in historial"
                :key="consulta.id"
                class="rounded-xl bg-[#071120] border border-slate-800/70 hover:border-slate-700/60 transition-all overflow-hidden"
              >
                <!-- Fila superior: fecha, tipo, estado -->
                <div class="grid grid-cols-2 sm:grid-cols-3 border-b border-slate-800/60">
                  <div class="px-5 py-3 border-r border-slate-800/60">
                    <p class="text-[10px] font-bold uppercase tracking-widest text-slate-500 mb-1">Fecha de consulta</p>
                    <p class="text-sm font-semibold text-slate-200">{{ consulta.fecha }}</p>
                  </div>
                  <div class="px-5 py-3 sm:border-r border-slate-800/60">
                    <p class="text-[10px] font-bold uppercase tracking-widest text-slate-500 mb-1">Tipo de consulta</p>
                    <p class="text-sm font-semibold text-slate-200">{{ consulta.tipoConsulta }}</p>
                  </div>
                  <div class="px-5 py-3 col-span-2 sm:col-span-1 border-t sm:border-t-0 border-slate-800/60">
                    <p class="text-[10px] font-bold uppercase tracking-widest text-slate-500 mb-1.5">Estado</p>
                    <span
                      :class="[
                        'inline-block text-[10px] font-black uppercase tracking-wider px-2.5 py-1 rounded-md',
                        consulta.estado === 'Finalizada'
                          ? 'bg-emerald-500/20 text-emerald-400 border border-emerald-500/30'
                          : 'bg-yellow-500/10 text-yellow-400 border border-yellow-500/20'
                      ]"
                    >
                      {{ consulta.estado === 'NoAsistida' ? 'No asistida' : consulta.estado }}
                    </span>
                  </div>
                </div>

                <!-- Fila inferior: motivo + doctor + botón -->
                <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4 px-5 py-4">
                  <div class="space-y-1.5 flex-1">
                    <p class="text-[10px] font-bold uppercase tracking-widest text-slate-500">Motivo principal</p>
                    <p class="text-sm text-slate-200 leading-relaxed">{{ consulta.motivo }}</p>
                    <p class="text-xs text-slate-500 font-medium">Médico tratante: {{ consulta.doctor }}</p>
                    <div v-if="consulta.tieneOrdenLaboratorio && consulta.estadoOrden === 'ResultadosSubidos'" class="pt-0.5">
                      <span class="inline-flex items-center gap-1 text-[10px] font-black uppercase tracking-wider bg-blue-500/10 text-blue-400 border border-blue-500/20 px-2.5 py-1 rounded-md">
                        🔬 Resultados por revisar
                      </span>
                    </div>
                  </div>

                  <button
                  v-if ="consulta.estado == 'Finalizada'"
                    @click="verDetalle(consulta)"
                    class="shrink-0 w-full sm:w-auto border border-slate-700 hover:border-cyan-500/60 hover:text-cyan-400 text-slate-300 px-5 py-2.5 rounded-lg text-[11px] font-black uppercase tracking-wider transition-all cursor-pointer whitespace-nowrap"
                  >
                    Ver consulta →
                  </button>
                  <span
                    v-else
                    class="shrink-0 text-[11px] font-black uppercase tracking-wider text-slate-600"
                  >
                    Sin acciones disponibles
                  </span>

                </div>
              </div>
            </div>
            <!-- Sin historial -->
            <div
              v-else-if="!consultaSeleccionada && historial.length === 0"
              class="text-center text-slate-600 font-bold uppercase tracking-wider py-24 text-sm"
            >
              No hay historial disponible
            </div>

            <!-- ── DETALLE ── -->
            <div v-else-if="consultaSeleccionada">

              <!-- RESUMEN -->
              <div v-if="pestañaActiva === 'resumen'" class="space-y-4">
                <div class="grid md:grid-cols-2 gap-4">
                  <div v-if="consultaSeleccionada.sintomas" class="bg-[#071120] border border-slate-800/40 rounded-xl p-5">
                    <p class="text-[10px] font-black uppercase tracking-widest text-slate-400 mb-2">Síntomas</p>
                    <p class="text-sm text-slate-200 leading-relaxed">{{ consultaSeleccionada.sintomas }}</p>
                  </div>
                  <div class="bg-[#071120] border border-slate-800/40 rounded-xl p-5">
                    <p class="text-[10px] font-black uppercase tracking-widest text-slate-400 mb-2">Diagnóstico</p>
                    <p class="text-sm text-slate-200 leading-relaxed">{{ consultaSeleccionada.diagnostico || 'Sin diagnóstico registrado' }}</p>
                  </div>
                </div>

                <div v-if="consultaSeleccionada.tratamiento" class="bg-[#071120] border border-slate-800/40 rounded-xl p-5">
                  <p class="text-[10px] font-black uppercase tracking-widest text-slate-400 mb-2">Tratamiento</p>
                  <p class="text-sm text-slate-200 leading-relaxed">{{ consultaSeleccionada.tratamiento }}</p>
                </div>

                <div v-if="consultaSeleccionada.observaciones" class="bg-[#071120] border border-slate-800/40 rounded-xl p-5">
                  <p class="text-[10px] font-black uppercase tracking-widest text-slate-400 mb-2">Observaciones</p>
                  <p class="text-sm text-slate-200 leading-relaxed">{{ consultaSeleccionada.observaciones }}</p>
                </div>

                <div class="flex flex-wrap gap-3 pt-1">
                  <button
                    v-if="consultaSeleccionada.id && consultaSeleccionada.estado === 'Finalizada'"
                    @click="descargarRecetaPaciente(consultaSeleccionada.id)"
                    :disabled="descargando"
                    class="flex items-center gap-2 border border-slate-700 hover:border-emerald-500/60 hover:text-emerald-400 text-slate-300 px-5 py-2.5 rounded-lg text-[11px] font-black uppercase tracking-wider transition-all disabled:opacity-40 cursor-pointer"
                  >
                    📄 {{ descargando ? 'Descargando...' : 'Descargar receta PDF' }}
                  </button>
                </div>
              </div>

              <!-- MEDICAMENTOS -->
              <div v-else-if="pestañaActiva === 'medicamentos'" class="space-y-4">
                <div v-if="medicamentosParsed.length === 0" class="text-slate-600 font-bold text-xs p-6 text-center border border-slate-800/40 rounded-xl uppercase tracking-wider bg-slate-900/20">
                  No se registraron medicamentos en esta consulta.
                </div>

                <div v-else class="border border-slate-800/50 rounded-xl overflow-hidden">
                  <table class="w-full text-sm">
                    <thead class="bg-slate-900/50 border-b border-slate-800/60">
                      <tr>
                        <th class="text-left text-[10px] font-black uppercase tracking-widest text-slate-400 px-5 py-3 w-10">#</th>
                        <th class="text-left text-[10px] font-black uppercase tracking-widest text-slate-400 px-5 py-3">Medicamento</th>
                        <th class="text-left text-[10px] font-black uppercase tracking-widest text-slate-400 px-5 py-3">Dosis</th>
                        <th class="text-left text-[10px] font-black uppercase tracking-widest text-slate-400 px-4 py-3">Frecuencia</th>
                        <th class="text-left text-[10px] font-black uppercase tracking-widest text-slate-400 px-4 py-3">Duración</th>
                      </tr>
                    </thead>
                    <tbody>
                      <tr
                        v-for="(med, i) in medicamentosParsed"
                        :key="i"
                        class="border-b border-slate-800/40 last:border-0 hover:bg-slate-900/30 transition-colors"
                      >
                        <td class="px-5 py-3.5 text-slate-600 font-semibold text-xs">{{ i + 1 }}</td>
                        <td class="px-5 py-3.5 font-semibold text-slate-200">{{ med.nombre }}</td>
                        <td class="px-5 py-3.5 text-slate-400">{{ med.dosis }}</td>
                        <td class="px-4 py-3.5 text-slate-400">{{ med.frecuencia }}</td>
                        <td class="px-4 py-3.5 text-slate-400">{{ med.duracion }}</td>
                      </tr>
                    </tbody>
                  </table>
                </div>

                <div class="flex justify-end pt-1">
                  <button
                    v-if="consultaSeleccionada.id && consultaSeleccionada.estado === 'Finalizada'"
                    @click="descargarRecetaPaciente(consultaSeleccionada.id)"
                    :disabled="descargando"
                    class="flex items-center gap-2 bg-emerald-600 hover:bg-emerald-500 text-white px-5 py-2.5 rounded-lg text-[11px] font-black uppercase tracking-wider transition-all disabled:opacity-40 cursor-pointer shadow-md"
                  >
                    📄 {{ descargando ? 'Descargando...' : 'Descargar receta PDF' }}
                  </button>
                </div>
              </div>

              <!-- LABORATORIO -->
              <div v-else-if="pestañaActiva === 'laboratorio'" class="space-y-4">

                <div
                  :class="['border rounded-xl px-5 py-3 text-xs font-black uppercase tracking-wider text-center', etiquetaEstadoOrden(consultaSeleccionada.estadoOrden).clase]"
                >
                  {{ etiquetaEstadoOrden(consultaSeleccionada.estadoOrden).texto }}
                </div>

                <div class="flex flex-wrap gap-3">
                  <button
                    @click="descargarOrden(consultaSeleccionada.consultaId)"
                    class="flex items-center gap-2 border border-slate-700 hover:border-cyan-500/50 hover:text-cyan-400 text-slate-300 px-5 py-2.5 rounded-lg text-[11px] font-black uppercase tracking-wider transition-all cursor-pointer"
                  >
                    🔬 Descargar orden de laboratorio PDF
                  </button>
                  <button
                    v-if="consultaSeleccionada.estadoOrden === 'ResultadosSubidos' || consultaSeleccionada.estadoOrden === 'Revisado'"
                    @click="descargarResultados(consultaSeleccionada.consultaId)"
                    class="flex items-center gap-2 border border-slate-700 hover:border-cyan-500/50 hover:text-cyan-400 text-slate-300 px-5 py-2.5 rounded-lg text-[11px] font-black uppercase tracking-wider transition-all cursor-pointer"
                  >
                  <ArrowDownTrayIcon class="w-4 h-4" />
                    Ver resultados PDF
                  </button>
                </div>

                <!-- Interpretación médica -->
                <div
                  v-if="esDoctor && consultaSeleccionada.estadoOrden === 'ResultadosSubidos'"
                  class="bg-[#071120] border border-slate-800/40 rounded-xl p-5 space-y-3"
                >
                  <p class="text-[10px] font-black uppercase tracking-widest text-slate-400">
                    <PencilSquareIcon class="w-4 h-4 inline mr-1" />
                    Interpretación médica
                  </p>
                  <p class="text-xs text-slate-500">Tus observaciones se enviarán automáticamente al correo del paciente.</p>
                  <textarea
                    v-model="observacionesDoctor"
                    rows="4"
                    placeholder="Escribe aquí tus observaciones profesionales sobre los resultados presentados..."
                    class="w-full bg-slate-900/60 border border-slate-800 rounded-xl p-3 text-sm text-slate-200 placeholder:text-slate-600 resize-none focus:outline-none focus:border-cyan-500 focus:ring-1 focus:ring-cyan-500/20 transition"
                  ></textarea>
                  <div class="flex justify-end">
                    <button
                      @click="enviarRevision"
                      :disabled="!observacionesDoctor.trim() || enviandoRevision"
                      class="flex items-center gap-2 bg-cyan-600 hover:bg-cyan-500 text-white px-6 py-2.5 rounded-lg text-[11px] font-black uppercase tracking-wider transition-all disabled:opacity-40 disabled:pointer-events-none cursor-pointer shadow-md"
                    >
                      <PaperAirplaneIcon class="w-4 h-4" />{{ enviandoRevision ? 'Enviando...' : 'Enviar revisión al paciente' }}
                    </button>
                  </div>
                </div>

                <!-- Observaciones ya revisadas -->
                <div
                  v-if="consultaSeleccionada.estadoOrden === 'Revisado'"
                  class="bg-[#071120] border border-slate-800/40 rounded-xl p-5 space-y-3"
                >
                  <p class="text-[10px] font-black uppercase tracking-widest text-slate-400">Observaciones enviadas al paciente</p>
                  <p class="text-sm text-slate-300 leading-relaxed">
                    {{ consultaSeleccionada.observacionesLaboratorio || 'Sin observaciones.' }}
                  </p>
                </div>

                <!-- Pendiente -->
                <div
                  v-if="consultaSeleccionada.estadoOrden === 'Pendiente'"
                  class="text-center text-slate-600 font-bold uppercase tracking-wider text-xs py-8 border border-slate-800/30 rounded-xl bg-slate-900/20"
                >
                  El paciente aún no ha subido sus resultados.
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
          <XMarkIcon class="w-3.5 h-3.5" />
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
.custom-scrollbar::-webkit-scrollbar { width: 8px; }
.custom-scrollbar::-webkit-scrollbar-track { background: transparent; }
.custom-scrollbar::-webkit-scrollbar-thumb { background: rgb(112, 136, 167); border-radius: 4px; }
.custom-scrollbar::-webkit-scrollbar-thumb:hover { background: rgb(71, 85, 105); }

@media (prefers-color-scheme: light) {
  .custom-scrollbar::-webkit-scrollbar-thumb { background: rgb(203, 213, 225); }
  .custom-scrollbar::-webkit-scrollbar-thumb:hover { background: rgb(148, 163, 184); }
}
@keyframes shrink {
  from { width: 100%; }
  to   { width: 0%; }
}
</style>
