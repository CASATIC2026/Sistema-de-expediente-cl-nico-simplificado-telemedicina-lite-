/////ESTE ES LA SECCIÓN PARA LAS CONSULTAS "SALA DE CONSULTAS" ( SECCION DONDE MOSTRARÁ LA VIDEOLLAMADA)
//ESTE MISMO CARGA LA INFORMACIÓN DEL PACIENTE
//ESTÁ IMPORTANDO EL FORMULARIO QUE LLENARÁ EL DOCTOR Y GENERARÁ LA RECETA (VIENE DE componentsDoctor/consultas)

<script setup>
import { ref, computed, onMounted, nextTick, onUnmounted, watch } from 'vue'

import { getCitaById, iniciarConsulta, finalizarConsulta } from '@/services/api'
import { useCitasStore } from '@/stores/citas'
import { useVideoStore } from '@/stores/videoStore'
import { useRouter } from 'vue-router'
import { profileIcons } from '@/utils/profileIcons'

import FormConsulta from '@/componentsDoctor/consultas/FormConsulta.vue'
import verHistorial from '@/components/verHistorial.vue'
import { marcarNoAsistida } from '@/services/api'
import { logger } from '@/utils/logger'
defineOptions({ name: 'ConsultaView' })
import { VideoCameraSlashIcon,
          CakeIcon,
          PhoneIcon,
          EnvelopeIcon,
          CheckCircleIcon,
          ClipboardDocumentListIcon
        } from '@heroicons/vue/24/solid'

const isMobile = /Android|iPhone|iPad/i.test(navigator.userAgent)
const llamadaGuardada = ref(null)

const props = defineProps({ id: String })
const citasStore = useCitasStore()
const router = useRouter()
const videoStore = useVideoStore()

const id = computed(() => props.id)

const cita = ref(null)

const iconoPaciente = computed(() => {
  if (!cita.value?.avatarIconoPaciente) return null

  return profileIcons.paciente.find(
    icon => icon.id === cita.value.avatarIconoPaciente
  )?.src || null
})

const loading = ref(true)
const mostrarHistorial = ref(false)
const cargandoNoAsistida = ref(false)
const consultaListaParaCerrar = ref(false)

const mostrarModalNoAsistio = ref(false)
const errorNoAsistida = ref('')

// Toast
const toast = ref({ visible: false, mensaje: '', tipo: 'success' })
let toastTimer = null
const mostrarToast = (mensaje, tipo = 'success') => {
  if (toastTimer) clearTimeout(toastTimer)
  toast.value = { visible: true, mensaje, tipo }
  toastTimer = setTimeout(() => { toast.value.visible = false }, 3500)
}

const jitsiContainer = ref(null)
let jitsiApi = null
let yaRedirigio = false

watch(() => videoStore.isActive, (activo) => {

  if (activo) return

  if (!videoStore.consultaFinalizada) return

  if (yaRedirigio) return

  yaRedirigio = true

    if (jitsiApi) {
      jitsiApi.dispose()
      jitsiApi = null
    }

    cita.value = null
    router.push('/app/Agenda')
  }
, { immediate: false})

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
  videoStore.consultaFinalizada = false

  if (!id.value) {
    loading.value = false
    return
  }
  if(!videoStore.isActive)
  {
    videoStore.recuperarLlamada()
  }

  try {
    loading.value = true
    cita.value = await getCitaById(id.value)


    if (
      cita.value.estado === 'NoAsistida' ||
      cita.value.estado === 'Finalizada' ||
      cita.value.estado === 'Cancelada'
    ) {
      cita.value = null
      loading.value = false
      return
    }

    try {
      await iniciarConsulta(id.value)
      await citasStore.cargarCitas()
    } catch {
       logger.warn('No se pudo iniciar la consulta en backend')
    }

  } catch (e) {
    logger.error('Error crítico:', e)
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
  if (!isMobile && cita.value?.linkReunion && jitsiContainer.value) {
    if (!window.JitsiMeetExternalAPI) {
      console.error('Jitsi SDK no está disponible. Verifica que el script esté cargado.')
      mostrarToast('No se pudo cargar la videollamada. Recarga la página e intenta de nuevo.', 'error')
      return
    }

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
  window.removeEventListener('focus', syncLlamadaGuardada)
  if (jitsiApi) {
    jitsiApi.dispose()
    jitsiApi = null
  }
})

const ejecutarNoAsistida = async () => {
  try {
    cargandoNoAsistida.value = true
    errorNoAsistida.value = ''

    await marcarNoAsistida(id.value)

    consultaListaParaCerrar.value = true
    mostrarModalNoAsistio.value = false

  } catch (e) {
    errorNoAsistida.value =
      e.response?.data?.message || 'No se pudo marcar la cita'
  } finally {
    cargandoNoAsistida.value = false
  }
}
const finalizandoConsulta = ref(false)

const finalizarSalaConsulta = async () => {
  try {
    finalizandoConsulta.value = true

    // CAMBIAR ESTADO REAL EN BACKEND
    await finalizarConsulta(id.value)

    // ACTUALIZAR ESTADO LOCAL
    if (cita.value) {
      cita.value.estado = 'Finalizada'
    }

    // FINALIZAR VIDEO
    videoStore.marcarConsultaFinalizada()
    videoStore.finalizarLlamada()

    // LIMPIAR STORAGE MOBILE
    sessionStorage.removeItem('jitsi_opened')
    localStorage.removeItem('activeCall')

    consultaListaParaCerrar.value = false

    await citasStore.cargarCitas()

  } catch (e) {
    logger.error('Error finalizando consulta:', e)

    mostrarToast(
      'No se pudo finalizar la consulta correctamente',
      'error'
    )
  } finally {
    finalizandoConsulta.value = false
  }
}

</script>

<template>
  <!-- Contenedor principal-->
  <div class="space-y-4 md:space-y-6 w-full p-3 md:p-4 text-slate-800 dark:text-slate-100 transition-colors duration-300">
    <h1 class="text-xl md:text-2xl font-black text-slate-800 dark:text-white tracking-tight">Consulta Médica</h1>

    <!-- Banner Consulta activa -->
    <div
  v-if="cita"
  class="flex items-center justify-between bg-teal-500/10 dark:bg-teal-500/15 border border-teal-500/20 rounded-2xl px-4 py-3"
>
  <div>
    <p class="text-xs uppercase tracking-widest font-black text-teal-600 dark:text-teal-300">
      Consulta activa
    </p>

    <p class="text-sm font-bold text-slate-700 dark:text-slate-200">
      Atención en curso con {{ cita.pacienteNombreCompleto }}
    </p>
  </div>

  <div class="flex items-center gap-2">
    <span class="h-2.5 w-2.5 rounded-full bg-teal-400 animate-pulse"></span>

    <span class="text-xs font-black uppercase tracking-wider text-teal-600 dark:text-teal-300">
      En consulta
    </span>
  </div>
</div>

    <!-- BARRA DE INFORMACIÓN DEL PACIENTE -->
    <div class="bg-white dark:bg-slate-900 rounded-2xl shadow-sm border border-slate-200 dark:border-slate-800 overflow-hidden transition-colors">

      <div v-if="loading" class="p-8 text-center text-slate-500 dark:text-slate-400">
        <span class="animate-pulse">Cargando datos de la cita...</span>
      </div>

      <div v-else-if="cita">

        <!-- Info del paciente -->
        <div class="flex flex-col md:flex-row items-start md:items-center justify-between p-4 md:p-5 gap-4">

          <!-- Izquierda: Avatar e Info -->
          <div class="flex items-start gap-3 w-full md:w-auto">

            <div class="h-12 w-12 md:h-14 md:w-14 shrink-0 rounded-2xl border border-sky-200 dark:border-sky-500/20 overflow-hidden flex items-center justify-center bg-sky-100 dark:bg-sky-500/10">

            <!-- Foto del paciente -->
            <img
              v-if="cita.fotoPaciente"
              :src="cita.fotoPaciente"
              class="w-full h-full object-cover"
            />

            <!-- Icono personalizado -->
            <img
              v-else-if="iconoPaciente"
              :src="iconoPaciente"
              class="w-full h-full object-cover"
            />

            <!-- Fallback inicial -->
            <span
              v-else
              class="text-sky-700 dark:text-sky-300 text-lg md:text-xl font-black"
            >
              {{ cita.pacienteNombreCompleto?.charAt(0) }}
            </span>

          </div>

            <div class="min-w-0 flex-1">
              <h2 class="text-lg md:text-xl font-black text-slate-800 dark:text-white truncate tracking-tight">
                {{ cita.pacienteNombreCompleto }}
              </h2>

              <div class="flex flex-wrap gap-2 mt-2">

                <div class="flex items-center bg-sky-50 dark:bg-sky-500/10 px-2 py-1 md:px-3 md:py-1.5 rounded-xl border border-sky-200 dark:border-sky-500/20">
                  <CakeIcon class="h-4 w-4 mr-1.5 text-sky-600 dark:text-sky-300 shrink-0" />
                  <span v-if="cita.fechaNacimientoPaciente" class="text-sky-800 dark:text-sky-200 font-bold text-sm md:text-base">
                    {{ obtenerEdad(cita.fechaNacimientoPaciente) }} años
                  </span>
                </div>

                <div class="flex items-center bg-slate-50 dark:bg-slate-800/70 px-2 py-1 md:px-3 md:py-1.5 rounded-lg border border-slate-200 dark:border-slate-700 min-w-0">
                  <PhoneIcon class="h-4 w-4 mr-1.5 text-slate-500 dark:text-slate-300 shrink-0" />
                  <span class="text-slate-700 dark:text-slate-200 font-bold text-sm md:text-base truncate max-w-[120px] md:max-w-none">
                    {{ cita.telefonoPaciente || 'No disponible' }}
                  </span>
                </div>

                <div class="hidden sm:flex items-center bg-slate-50 dark:bg-slate-800/70 px-2 py-1 md:px-3 md:py-1.5 rounded-lg border border-slate-200 dark:border-slate-700 min-w-0">
                 <EnvelopeIcon class="h-4 w-4 mr-1.5 text-slate-500 dark:text-slate-300 shrink-0" />
                  <span class="text-slate-700 dark:text-slate-200 font-bold text-sm md:text-base truncate max-w-[160px] md:max-w-xs">
                    {{ cita.correoPaciente || 'No disponible' }}
                  </span>
                </div>

              </div>
            </div>
          </div>

          <!-- Derecha: Acciones -->
          <div class="flex items-center gap-2 md:gap-3 w-full md:w-auto justify-between md:justify-end border-t md:border-t-0 pt-3 md:pt-0">

            <div class="text-left md:text-right md:mr-4">
              <p class="text-xs text-slate-400 dark:text-slate-500 font-black uppercase tracking-wider">Hora de Cita</p>
              <p class="text-base md:text-lg font-black text-sky-600 dark:text-sky-300">
                {{ new Date(cita.start).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }) }}
              </p>
            </div>

            <button
              @click="mostrarHistorial = true"
              class="flex items-center gap-2 bg-sky-50 dark:bg-sky-500/10 text-sky-700 dark:text-sky-300 hover:bg-sky-100 dark:hover:bg-sky-500/20 px-3 py-2 md:px-4
              rounded-xl font-bold transition-all border border-sky-200 dark:border-sky-500/20 text-sm md:text-base"
            >
              <ClipboardDocumentListIcon class="h-5 w-5" />
              <span>Ver Historial</span>
            </button>

            <button
                v-if="!consultaListaParaCerrar"
                @click="mostrarModalNoAsistio = true"
              :disabled="cargandoNoAsistida"
             class="flex items-center gap-2 bg-red-500/10 dark:bg-red-500/15 text-red-700 dark:text-red-300 hover:bg-red-500/20 px-3 py-2 md:px-4
             rounded-xl font-bold transition-all border border-red-300 dark:border-red-500/20 text-sm md:text-base disabled:opacity-50 cursor-pointer disabled:cursor-not-allowed"
            >
              <span>No asistió</span>
            </button>

            <button
              v-if="consultaListaParaCerrar"
              @click="finalizarSalaConsulta"
              :disabled="finalizandoConsulta"
              class="flex items-center gap-2 bg-teal-500 hover:bg-teal-400
              text-white px-4 py-2 rounded-xl font-black transition-all
              shadow-lg shadow-teal-500/20"
            >
              <CheckCircleIcon class="h-5 w-5" />
              <span>
                {{ finalizandoConsulta
                  ? 'Finalizando...'
                  : 'Consulta Finalizada'
                }}
              </span>
            </button>
          </div>
        </div>

        <div class="grid grid-cols-1 md:grid-cols-2 gap-4 md:gap-6 items-start p-4 md:p-6">

          <!-- Izquierda: Videollamada -->
          <div class="bg-white dark:bg-slate-900 rounded-2xl shadow-sm border border-slate-200 dark:border-slate-800 p-3 md:p-4 xl:sticky xl:top-4 transition-colors">
            <h2 class="flex items-center gap-2 text-base md:text-lg font-black text-slate-800 dark:text-white">
              <span class="h-2 w-2 bg-red-500 rounded-full animate-ping shrink-0"></span>
              Videollamada
             </h2>

             <!-- RECUPERAR LLAMADA DESKTOP -->
                <div
                  v-if="!isMobile && llamadaGuardada && !consultaListaParaCerrar"
                  class="mt-3 mb-3 flex justify-end"
                >
                  <button
                    @click="reingresarLlamada"
                    class="flex items-center gap-2 px-3 py-2 rounded-xl
                    bg-sky-500 hover:bg-sky-400 text-white text-sm font-bold
                    transition-all shadow-sm"
                  >
                    Reabrir videollamada
                  </button>
                </div>

           <!-- Contenedor videollamada (Jitsi)-->
            <div class="w-full h-[420px] rounded-lg overflow-hidden flex items-center justify-center bg-slate-950 border border-slate-800">

                    <div
                      v-if="!isMobile"
                      ref="jitsiContainer"
                      class="w-full h-full"
                    ></div>

                    <div
                      v-else
                      class="flex flex-col items-center justify-center text-white text-center px-4"
                    >
                      <p
                          v-if="!consultaListaParaCerrar"
                          class="mb-4 text-sm opacity-80"
                        >
                          La videollamada se abrió en otra pestaña
                        </p>

                      <p
                          v-if="consultaListaParaCerrar"
                          class="text-emerald-300 text-sm font-bold mt-3"
                        >
                          La videollamada terminó correctamente.
                        </p>

                  <!--Mobile Call UI-->
                      <button
                        v-if="llamadaGuardada && !consultaListaParaCerrar"
                        @click="reingresarLlamada"
                        class="bg-teal-500 hover:bg-teal-600 text-white px-4 py-2 rounded-xl font-bold transition-all shadow-sm"
                      >
                        Volver a la llamada
                      </button>

                      <button
                        v-else-if="consultaListaParaCerrar"
                        @click="finalizarSalaConsulta"
                        :disabled="finalizandoConsulta"
                        class="mt-3 bg-teal-500 hover:bg-teal-400 text-white px-4 py-2 rounded-xl font-bold w-full"
                      >
                        {{
                            finalizandoConsulta
                              ? 'Finalizando...'
                              : 'Consulta Finalizada'
                          }}
                      </button>

                      <p
                        v-else
                        class="text-xs opacity-60 mt-2"
                      >
                        No hay una llamada activa disponible
                      </p>
                    </div>
                  </div>
          </div>

          <!-- Derecha: Ficha Consulta -->
          <div class="bg-white dark:bg-slate-900 rounded-2xl shadow-sm border border-slate-200 dark:border-slate-800 p-4 md:p-6 transition-colors">
            <h2 class="text-base md:text-lg font-black text-slate-800 dark:text-white">Ficha de la Consulta</h2>
            <FormConsulta
              :cita="cita"
              @consulta-guardada="consultaListaParaCerrar = true"
            />
          </div>
        </div>
      </div>

      <!-- Sin consulta activa (Estado Vacío) -->
      <div v-else class ="flex flex-col items-center justify-center py-24 text-center px-6">
       <VideoCameraSlashIcon class="w-16 h-16 text-slate-400 dark:text-slate-600 mb-4" />
        <h2 class="text-xl font-black text-slate-700 dark:text-slate-200 mb-2">Sin consulta activa</h2>
        <p class="text-slate-400 dark:text-slate-500 text-sm max-w-xs">
          Para iniciar una consulta, ve a tu <strong>Agenda</strong>, selecciona una cita y haz clic en "Entrar a consulta".
        </p>
        <!-- Botón Ir a mi Agenda -->
        <button
        @click="$router.push('/app/Agenda')"
        class="mt-6 px-5 py-2.5 bg-sky-500 hover:bg-sky-600 dark:bg-sky-500 dark:hover:bg-sky-400 text-white text-sm font-bold rounded-xl transition-all shadow-sm"
        >
      Ir a mi Agenda</button>
      </div>
    </div>
    <!-- cierre del contenedor blanco principal -->

    <!-- MODAL NO ASISTIÓ -->
    <div
      v-if="mostrarModalNoAsistio"
      class="fixed inset-0 z-50 flex items-center justify-center bg-slate-950/60 backdrop-blur-sm p-4"
    >
      <div
        class="w-full max-w-md rounded-3xl bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 shadow-2xl p-6 animate-in fade-in zoom-in-95 duration-200"
      >
        <div class="flex items-start gap-4">

          <div class="h-12 w-12 rounded-2xl bg-red-500/10 flex items-center justify-center shrink-0">
            <span class="text-2xl">⚠️</span>
          </div>

          <div class="flex-1">
            <h2 class="text-lg font-black text-slate-800 dark:text-white">
              Marcar como no asistió
            </h2>

            <p class="mt-2 text-sm text-slate-600 dark:text-slate-300 leading-relaxed">
              Esta acción marcará la cita como
              <strong>"No asistida"</strong>
              y finalizará la consulta actual.
            </p>

            <p
              v-if="errorNoAsistida"
              class="mt-3 text-sm text-red-600 dark:text-red-400 font-medium"
            >
              {{ errorNoAsistida }}
            </p>

            <div class="flex items-center justify-end gap-3 mt-6">

              <button
                @click="mostrarModalNoAsistio = false"
                class="px-4 py-2 rounded-xl border border-slate-300 dark:border-slate-700 text-slate-700 dark:text-slate-200 font-bold hover:bg-slate-100 dark:hover:bg-slate-800 transition-all"
              >
                Cancelar
              </button>

              <button
                @click="ejecutarNoAsistida"
                :disabled="cargandoNoAsistida"
                class="px-4 py-2 rounded-xl bg-red-600 hover:bg-red-500 text-white font-black transition-all disabled:opacity-50"
              >
                {{
                  cargandoNoAsistida
                    ? 'Marcando...'
                    : 'Confirmar'
                }}
              </button>

            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- MODAL HISTORIAL -->
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
