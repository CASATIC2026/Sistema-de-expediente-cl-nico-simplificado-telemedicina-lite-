/////ESTE ES LA SECCIÓN PARA LAS CONSULTAS "SALA DE CONSULTAS" ( SECCION DONDE MOSTRARÁ LA VIDEOLLAMADA)
//ESTE MISMO CARGA LA INFORMACIÓN DEL PACIENTE
//ESTÁ IMPORTANDO EL FORMULARIO QUE LLENARÁ EL DOCTOR Y GENERARÁ LA RECE (VIENE DE componentsDoctor/consultas)

<script setup>
import { ref, computed, onMounted, nextTick, onUnmounted, watch } from 'vue'

import { getCitaById, iniciarConsulta } from '@/services/api'
import { useCitasStore } from '@/stores/citas'
import { useVideoStore } from '@/stores/videoStore'
import { useRouter } from 'vue-router'
import FormConsulta from '@/componentsDoctor/consultas/FormConsulta.vue'
import verHistorial from '@/components/verHistorial.vue'

defineOptions({ name: 'ConsultaView' })

const isMobile = /Android|iPhone|iPad/i.test(navigator.userAgent)
const llamadaGuardada = ref(null)

const props = defineProps({ id: String })
const citasStore = useCitasStore()
const router = useRouter()
const videoStore = useVideoStore()

const id = computed(() => props.id)
const cita = ref(null)
const loading = ref(true)
const mostrarHistorial = ref(false)

const jitsiContainer = ref(null)
let jitsiApi = null
let yaRedirigio = false

watch(() => videoStore.isActive, (activo) => {
  if (!activo && !yaRedirigio) {
    yaRedirigio = true

    if (jitsiApi) {
      jitsiApi.dispose()
      jitsiApi = null
    }

    cita.value = null
    router.push('/app/Agenda')
  }
})

// REINGRESAR A LLAMADA
function reingresarLlamada() {
  const saved = localStorage.getItem('activeCall')
  if (!saved) return

  const data = JSON.parse(saved)
  const url = `https://meet.jit.si/${data.roomName}`

  window.open(url, '_blank')
}
function syncLlamadaGuardada() {
  const saved = localStorage.getItem('activeCall')
  llamadaGuardada.value = saved ? JSON.parse(saved) : null
}

// Edad
const obtenerEdad = (fechaString) => {
  if (!fechaString) return 'N/A'
  const partes = fechaString.split('/')
  if (partes.length !== 3) return 'N/A'
  const nacimiento = new Date(partes[2], partes[1] - 1, partes[0])
  const hoy = new Date()
  if (isNaN(nacimiento.getTime())) return 'Fecha inválida'
  let edad = hoy.getFullYear() - nacimiento.getFullYear()
  const mes = hoy.getMonth() - nacimiento.getMonth()
  if (mes < 0 || (mes === 0 && hoy.getDate() < nacimiento.getDate())) edad--
  return edad
}

onMounted(async () => {
  if (!id.value) {
    loading.value = false
    return
  }

  try {
    loading.value = true
    cita.value = await getCitaById(id.value)

    try {
      await iniciarConsulta(id.value)
      await citasStore.cargarCitas()
    } catch {
      console.warn('No se pudo iniciar la consulta en backend')
    }

  } catch (e) {
    console.error('Error crítico:', e)
    cita.value = null
  } finally {
    loading.value = false
  }

  // RECUPERAR LLAMADA
  const saved = localStorage.getItem('activeCall')
  if (saved) {
    llamadaGuardada.value = JSON.parse(saved)
  }

  window.addEventListener('focus', syncLlamadaGuardada)

  // MOBILE → abrir Jitsi externo
  if (isMobile && cita.value?.linkReunion) {
    window.open(cita.value.linkReunion, '_blank')
    return
  }

  await nextTick()

  // DESKTOP → embebido
  if (videoStore.isActive && cita.value?.linkReunion && jitsiContainer.value) {
    const room = cita.value.linkReunion
      .split('#')[0]
      .split('?')[0]
      .split('/')
      .pop()

    jitsiApi = new window.JitsiMeetExternalAPI("meet.jit.si", {
      roomName: room,
      parentNode: jitsiContainer.value,
      width: '100%',
      height: '100%',
      configOverwrite: {
        prejoinPageEnabled: false,
        disableDeepLinking: true
      }
    })
  }
})

onUnmounted(() => {
  if (jitsiApi) {
    jitsiApi.dispose()
    jitsiApi = null
  }
})
</script>

<template>
  <div class="space-y-4 md:space-y-6 w-full p-3 md:p-4">
    <h1 class="text-xl md:text-2xl font-bold text-gray-800">Consulta Médica</h1>

    <!-- BARRA DE INFORMACIÓN DEL PACIENTE -->
    <div class="bg-white rounded-xl shadow-sm border border-gray-200 overflow-hidden">

      <div v-if="loading" class="p-8 text-center text-gray-500">
        <span class="animate-pulse">Cargando datos de la cita...</span>
      </div>

      <!-- FIX 1: este div ahora envuelve tanto la info del paciente como la cuadrícula -->
      <div v-else-if="cita">

        <!-- Info del paciente -->
        <div class="flex flex-col md:flex-row items-start md:items-center justify-between p-4 md:p-5 gap-4">

          <!-- Izquierda: Avatar e Info -->
          <div class="flex items-start gap-3 w-full md:w-auto">

            <div class="h-12 w-12 md:h-14 md:w-14 shrink-0 rounded-full border border-blue-200 overflow-hidden flex items-center justify-center bg-blue-100">
              <img v-if="cita.fotoPaciente" :src="cita.fotoPaciente" class="w-full h-full object-cover" />
              <span v-else class="text-blue-600 text-lg md:text-xl font-bold">
                {{ cita.pacienteNombreCompleto?.charAt(0) }}
              </span>
            </div>

            <div class="min-w-0 flex-1">
              <h2 class="text-lg md:text-xl font-bold text-gray-800 truncate">
                {{ cita.pacienteNombreCompleto }}
              </h2>

              <div class="flex flex-wrap gap-2 mt-2">

                <div class="flex items-center bg-blue-50 px-2 py-1 md:px-3 md:py-1.5 rounded-lg border border-blue-100">
                  <span class="text-base mr-1.5">🎂</span>
                  <span v-if="cita.fechaNacimientoPaciente" class="text-blue-900 font-bold text-sm md:text-base">
                    {{ obtenerEdad(cita.fechaNacimientoPaciente) }} años
                  </span>
                </div>

                <div class="flex items-center bg-slate-50 px-2 py-1 md:px-3 md:py-1.5 rounded-lg border border-slate-200 min-w-0">
                  <span class="text-base mr-1.5">📞</span>
                  <span class="text-slate-700 font-bold text-sm md:text-base truncate max-w-[120px] md:max-w-none">
                    {{ cita.telefonoPaciente || 'No disponible' }}
                  </span>
                </div>

                <div class="hidden sm:flex items-center bg-slate-50 px-2 py-1 md:px-3 md:py-1.5 rounded-lg border border-slate-200 min-w-0">
                  <span class="text-base mr-1.5">📧</span>
                  <span class="text-slate-700 font-bold text-sm md:text-base truncate max-w-[160px] md:max-w-xs">
                    {{ cita.correoPaciente || 'No disponible' }}
                  </span>
                </div>

              </div>
            </div>
          </div>

          <!-- Derecha: Acciones -->
          <div class="flex items-center gap-2 md:gap-3 w-full md:w-auto justify-between md:justify-end border-t md:border-t-0 pt-3 md:pt-0">

            <div class="text-left md:text-right md:mr-4">
              <p class="text-xs text-gray-400 font-bold uppercase">Hora de Cita</p>
              <p class="text-base md:text-lg font-semibold text-blue-700">
                {{ new Date(cita.start).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }) }}
              </p>
            </div>

            <button
              @click="mostrarHistorial = true"
              class="flex items-center gap-2 bg-blue-50 text-blue-700 hover:bg-blue-100 px-3 py-2 md:px-4 rounded-lg font-medium transition-colors border border-blue-200 text-sm md:text-base"
            >
              <span>📋</span>
              <span>Ver Historial</span>
            </button>

          </div>
        </div>

        <!-- CUADRÍCULA ahora DENTRO del v-else-if="cita" -->
        <div class="grid grid-cols-1 md:grid-cols-2 gap-4 md:gap-6 items-start p-4 md:p-6">

          <!-- Izquierda: Videollamada -->
          <div class="bg-white rounded-xl shadow-sm border border-gray-200 p-3 md:p-4 sticky top-4">
            <h2 class="text-base md:text-lg font-semibold mb-3 md:mb-4 flex items-center gap-2">
              <span class="h-2 w-2 bg-red-500 rounded-full animate-ping shrink-0"></span>
              Videollamada
            </h2>
            <div class="w-full h-[420px] rounded-lg overflow-hidden flex items-center justify-center bg-slate-900">

                    <!-- DESKTOP -->
                    <div
                      v-if="!isMobile"
                      ref="jitsiContainer"
                      class="w-full h-full"
                    ></div>

                    <!-- MOBILE -->
                    <div
                      v-else
                      class="flex flex-col items-center justify-center text-white text-center px-4"
                    >
                      <p class="mb-4 text-sm opacity-80">
                        La videollamada se abrió en otra pestaña
                      </p>

                      <button
                        v-if="llamadaGuardada"
                        @click="reingresarLlamada"
                        class="bg-green-600 px-4 py-2 rounded-lg font-semibold"
                      >
                        Volver a la llamada
                      </button>
                       <p
                        v-else
                        class="text-xs opacity-60 mt-2"
                      >
                        No hay una llamada activa disponible
                      </p>
                    </div>

                  </div>
            <!--<div ref="jitsiContainer" class="w-full h-[420px] rounded-lg overflow-hidden"></div> -->
          </div>

          <!-- Derecha: Ficha -->
          <div class="bg-white rounded-xl shadow-sm border border-gray-200 p-4 md:p-6">
            <h2 class="text-base md:text-lg font-semibold mb-3 md:mb-4">Ficha de la Consulta</h2>
            <FormConsulta :cita="cita" />
          </div>

        </div>

      </div>
      <!-- cierre correcto del v-else-if="cita" -->

    </div>
    <!-- cierre del contenedor blanco principal -->

    <!-- MODAL HISTORIAL -->
    <!-- rolVisor con comillas simples internas para pasar string literal -->
    <verHistorial
      v-if="mostrarHistorial && cita"
      :esDoctor="true"
      :rolVisor="'doctor'"
      :usuarioSeleccionado="{
        nombre: cita.pacienteNombreCompleto,
        id: cita.pacienteId
      }"
      @cerrar="mostrarHistorial = false"
    />

  </div>
</template>
