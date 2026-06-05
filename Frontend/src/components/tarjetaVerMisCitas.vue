<script setup>
import { ref, computed, onMounted } from 'vue'
import { getCitas, cancelarCita as cancelarCitaAPI, descargarReceta, descargarOrdenLaboratorio } from '@/services/api'

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
const loading = ref(true)
const citaSeleccionada = ref(null)


const toast = ref({ visible: false, mensaje: '', tipo: 'success' })
let toastTimer = null
const mostrarToast = (mensaje, tipo = 'success') => {
  if (toastTimer) clearTimeout(toastTimer)
  toast.value = { visible: true, mensaje, tipo }
  toastTimer = setTimeout(() => { toast.value.visible = false }, 3500)
}

// Modal de confirmación
const confirmModal = ref({ visible: false, titulo: '', mensaje: '', resolveFn: null })
const pedirConfirmacion = (titulo, mensaje) => {
  return new Promise((resolve) => {
    confirmModal.value = { visible: true, titulo, mensaje, resolveFn: resolve }
  })
}
const responderConfirm = (respuesta) => {
  confirmModal.value.resolveFn(respuesta)
  confirmModal.value.visible = false
}

// ===============================
// CALCULAR TIEMPO RESTANTE PARA CANCELAR
const calcularTiempoRestante = (fechaCita) => {
  const ahora = new Date()
  const cita = new Date(fechaCita)
  const diferencia = cita - ahora
  const horasRestantes = Math.floor(diferencia / (1000 * 60 * 60))
  const minutosRestantes = Math.floor((diferencia % (1000 * 60 * 60)) / (1000 * 60))

  return {
    totalHoras: horasRestantes,
    minutos: minutosRestantes,
    puedeCancelar: horasRestantes >= 24,
    texto: horasRestantes >= 24
      ? '✓ Puedes cancelar'
      : `⏱ ${horasRestantes}h ${minutosRestantes}m para cancelar`
  }
}

// ===============================
// CARGAR DESDE BACKEND
onMounted(async () => {

  loading.value = true

  try {

    const data = await getCitas()

    citas.value = data.map(c => {

      const fechaObj = new Date(c.start)

      return {
        id: c.idCita,
        nombre: c.pacienteNombreCompleto,
        doctor: c.doctorNombreCompleto || "Doctor asignado",
        fecha: fechaObj.toLocaleDateString(),
        fechaCompleta: c.start,

        hora: fechaObj.toLocaleTimeString('es-SV', {
          hour: '2-digit',
          minute: '2-digit',
          hour12: true,
          timeZone: 'America/El_Salvador'
        }),

        estado: c.estado,
        linkReunion: c.linkReunion,
        tipoConsulta: c.tipoConsulta,
        dui: c.duiPaciente || '',
        telefono: c.telefonoPaciente || '',

        consultaId: c.consulta?.idConsulta || null,
        tieneOrdenLaboratorio: c.consulta?.tieneOrdenLaboratorio || false,
        estadoOrden: c.consulta?.estadoOrden || ''
      }

    })

  } catch (e) {

    console.error("Error cargando citas:", e)

  } finally {

    loading.value = false

  }

})



const copiarLink = async (link) => {
  try {
    await navigator.clipboard.writeText(link)
    mostrarToast('Link copiado al portapapeles', 'success')
  } catch (e) {
    mostrarToast('No se pudo copiar el link', 'error')
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
      cita.telefono?.toLowerCase().includes(b) ||
      cita.fecha?.includes(b)
    )
  })
})


//PARA MOSTRAR FECHA DE LA ULTIMA CONSULTA
const fechaUltimaConsultaFinalizada = computed(() => {
  // Filtramos solo las que ya terminaron
  const terminadas = citas.value.filter(c => c.estado === 'Finalizada')

  if (terminadas.length === 0) return null

  // Ordenamos por fecha de forma descendente (la más reciente primero)
  const masReciente = terminadas.sort((a, b) => {
    return new Date(b.fecha) - new Date(a.fecha)
  })[0]

  return masReciente.fecha
})




// ===============================
// ACCIONES
const ejecutarCancelar = async (cita) => {
  const tiempoRestante = calcularTiempoRestante(cita.fechaCompleta)

  // Validar 24 horas
  if (!tiempoRestante.puedeCancelar) {
    mostrarToast(`No puedes cancelar con menos de 24 horas. Te faltan: ${tiempoRestante.totalHoras}h ${tiempoRestante.minutos}m`, 'error')
    return
  }

  const ok = await pedirConfirmacion('Cancelar cita', '¿Seguro que deseas anular esta cita? Esta acción no se puede deshacer.')
  if (!ok) return
  try {
    await cancelarCitaAPI(cita.id)
    const citaEnLista = citas.value.find(c => c.id === cita.id)
    if (citaEnLista) citaEnLista.estado = 'Cancelada'
    mostrarToast('Cita cancelada correctamente', 'success')
  } catch (e) {
    console.error(e)
    mostrarToast(e.response?.data?.message || 'No se pudo cancelar la cita', 'error')
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
    mostrarToast('No se pudo descargar la receta', 'error')
  }
}

const descargarOrdenPDF = async (consultaId) => {
  if (!consultaId) return
  try {
    const blob = await descargarOrdenLaboratorio(consultaId)
    const url = window.URL.createObjectURL(blob)
    const a = document.createElement('a')
    a.href = url
    a.download = `orden_laboratorio_${consultaId}.pdf`
    a.click()
    window.URL.revokeObjectURL(url)
  } catch (e) {
    console.error(e)
    mostrarToast('No se pudo descargar la orden de laboratorio', 'error')
  }
}

const accederHistorialActivo = (id = null) => {
  citaSeleccionada.value = id
  accederHistorial.value = !accederHistorial.value
}
</script>

<template>
  <div
    class="fixed inset-0 z-50 flex items-center justify-center
    bg-slate-900/60 backdrop-blur-sm p-0"
    @click.self="emit('cerrar')"
  >

    <div
      class="w-full h-full flex flex-col overflow-hidden border-none shadow-2xl
      bg-white dark:bg-slate-950"
    >

      <!-- HEADER -->
      <header
        class="p-3 md:p-10
        bg-slate-900
        border-b border-slate-800
        flex flex-col md:flex-row md:items-center justify-between
        gap-4 md:gap-6"
      >

        <div class="flex items-center gap-3 md:gap-6">

          <button
            @click="emit('cerrar')"
            class="flex items-center gap-2
            px-3 py-2 md:px-4 md:py-2
            bg-white dark:bg-slate-900
            border border-slate-200 dark:border-slate-700
            text-slate-600 dark:text-slate-200
            rounded-xl
            hover:bg-slate-50 dark:hover:bg-slate-800
            hover:border-slate-300 dark:hover:border-slate-600
            transition-all shadow-lg shrink-0"
          >
            ←
            <span class="text-sm font-bold">Regresar</span>
          </button>

          <div class="min-w-0">
            <h3
              class="text-white
              text-xl md:text-3xl
              font-black tracking-tight truncate"
            >
              {{ props.esAdmin ? 'Gestión de Consultas' : 'Mis Citas Médicas' }}
            </h3>

            <p
              class="text-teal-300
              font-medium text-xs md:text-base"
            >
              {{
                props.esAdmin
                  ? 'Administra y supervisa el estado de las videollamadas'
                  : 'Historial de tus citas médicas'
              }}
            </p>
          </div>
        </div>

        <!-- BUSCADOR -->
        <div
          v-if="props.esAdmin"
          class="relative w-full md:w-96"
        >
          <input
            v-model="filtroBusqueda"
            type="text"
            placeholder="Buscar por paciente o ID..."
            class="w-full pl-12 pr-6 py-3 md:py-4
            bg-white dark:bg-slate-900
            border border-slate-200 dark:border-slate-700
            focus:border-blue-500 focus:ring-4 focus:ring-blue-500/10
            rounded-2xl outline-none transition-all
            font-medium text-sm
            text-slate-700 dark:text-slate-100"
          />

          <span
            class="absolute left-4 top-1/2 -translate-y-1/2
            text-slate-400 dark:text-slate-500"
          >
            🔍
          </span>
        </div>

      </header>

      <!-- TABS -->
      <nav
        class="flex
        bg-slate-900
        px-2 md:px-8
        mx-2 md:mx-10 mt-4 md:mt-8
        md:p-3
        rounded-2xl md:rounded-3xl
        gap-1 md:gap-2
        border border-slate-700"
      >
        <button
          v-for="tab in ['Pendiente', 'EnConsulta', 'Cancelada', 'Finalizada', 'NoAsistida']"
          :key="tab"
          @click="pestañaActiva = tab"
          class="flex-1 py-2 md:py-3
          rounded-xl font-bold
          text-[9px] md:text-sm
          transition-all uppercase tracking-wider cursor-pointer"
          :class="
            pestañaActiva === tab
              ? 'bg-teal-600 text-white ring-1 ring-teal-400 shadow-lg shadow-teal-900/20'
              : 'text-slate-300 hover:bg-slate-800/70'"
          "
        >
          {{ tab === 'EnConsulta' ? 'En Curso' : tab === 'NoAsistida' ? 'No Asistidas' : tab }}
        </button>
      </nav>

      <!-- MAIN -->
      <main
        class="flex-1 px-3 md:px-10 py-4 md:py-6
        overflow-y-auto custom-scrollbar"
      >







      <!-- SKELETON LOADER -->
          <div
            v-if="loading"
            class="grid gap-4"
          >

            <div
              v-for="n in 5"
              :key="n"
              class="animate-pulse
              bg-white dark:bg-slate-900
              border border-slate-200 dark:border-slate-800
              rounded-3xl p-6"
            >

              <!-- Header -->
              <div class="flex items-center gap-4">

                <!-- Avatar -->
                <div
                  class="w-14 h-14
                  rounded-xl
                  bg-slate-300 dark:bg-slate-700"
                ></div>

                <!-- Nombre -->
                <div class="flex-1">

                  <div
                    class="h-4 w-48 rounded
                    bg-slate-300 dark:bg-slate-700
                    mb-2"
                  ></div>

                  <div
                    class="h-3 w-32 rounded
                    bg-slate-200 dark:bg-slate-800"
                  ></div>

                </div>

              </div>

              <!-- Contenido -->
              <div
                class="grid grid-cols-1 md:grid-cols-3
                gap-4 mt-6"
              >

                <div
                  class="h-16 rounded-2xl
                  bg-slate-200 dark:bg-slate-800"
                ></div>

                <div
                  class="h-16 rounded-2xl
                  bg-slate-200 dark:bg-slate-800"
                ></div>

                <div
                  class="h-16 rounded-2xl
                  bg-slate-200 dark:bg-slate-800"
                ></div>

              </div>

              <!-- Botones -->
              <div class="flex justify-end gap-3 mt-6">

                <div
                  class="h-10 w-28 rounded-full
                  bg-slate-300 dark:bg-slate-700"
                ></div>

                <div
                  class="h-10 w-36 rounded-full
                  bg-slate-300 dark:bg-slate-700"
                ></div>

              </div>

            </div>

          </div>

          <!-- LISTA REAL -->
          <div
            v-else-if="citasFiltradas.length > 0"
            class="grid gap-4"
          >
        




















          <div
            v-for="cita in citasFiltradas"
            :key="cita.id"
            class="group
            bg-white dark:bg-gradient-to-r dark:from-[#0f2040] dark:to-[#10284f]
            border border-slate-200 dark:border-slate-700
            rounded-3xl p-4 md:p-6
            flex flex-col
            hover:shadow-xl hover:border-blue-200 dark:hover:border-cyan-400
            transition-all duration-300
            lg:grid lg:grid-cols-12 lg:items-center"
          >

            <!-- Nombre -->
            <div class="lg:col-span-4 flex items-center gap-4">

              <div
                class="w-12 h-12 md:w-14 md:h-14
                bg-cyan-500
                text-white rounded-xl
                flex shrink-0 items-center justify-center
                text-xl font-bold shadow-lg"
              >
                {{ cita.nombre.charAt(0) }}
              </div>

              <div class="min-w-0">

                <h4
                  class="text-slate-900 dark:text-white
                  font-bold text-base md:text-lg truncate"
                >
                  {{ cita.nombre }}
                </h4>

                <div class="flex items-center gap-2 flex-wrap">

                  <span
                    class="px-2 py-0.5
                    bg-slate-100 dark:bg-slate-800
                    text-slate-500 dark:text-slate-300
                    text-[10px] font-black rounded-md uppercase"
                  >
                    #Tel: {{ cita.telefono }}
                  </span>

                  <span
                    v-if="cita.estado === 'EnConsulta'"
                    class="flex items-center gap-1
                    text-green-600 dark:text-green-400
                    text-[10px] font-bold animate-pulse"
                  >
                    ● En vivo
                  </span>

                </div>
              </div>
            </div>

            <!-- Detalles -->
            <div
              class="lg:col-span-3 mt-3 lg:mt-0
              flex flex-col
              pt-3 lg:pt-0
              border-t lg:border-t-0 lg:border-l
              lg:pl-8
              border-slate-200 dark:border-cyan-500/30"
            >
              <span
                class="text-slate-800 dark:text-white
                font-bold text-[14px] uppercase tracking-tighter"
              >
                Detalles de la consulta
              </span>

              <span class="text-cyan-600 dark:text-cyan-400 font-bold text-xs">
                Consulta: {{ cita.tipoConsulta }}
              </span>

              <span class="text-slate-500 dark:text-cyan-400 font-medium text-xs">
                DR: {{ cita.doctor }}
              </span>

              <span class="text-slate-500 dark:text-cyan-400 font-medium text-xs">
                Estado: {{ cita.estado === 'NoAsistida' ? 'No Asistida' : 
                cita.estado === 'EnConsulta' ? 'En Consulta' : 
                cita.estado }}
              </span>

              <span class="text-slate-500 dark:text-cyan-400 font-medium text-xs">
                ID cita: {{ cita.id }}
              </span>
            </div>

            <!-- Fecha -->
            <div
              class="lg:col-span-2 mt-3 lg:mt-0
              flex flex-col
              pt-3 lg:pt-0
              border-t lg:border-t-0 lg:border-l
              lg:pl-8
              border-slate-200 dark:border-slate-700"
            >
              <span
                class="text-slate-800 dark:text-white
                font-bold text-[14px] uppercase tracking-tighter"
              >
                Fecha y Hora
              </span>

              <span class="text-cyan-600 dark:text-cyan-400 font-bold text-sm">
                {{ cita.fecha }}
              </span>

              <span class="text-slate-500 dark:text-cyan-400 font-medium text-xs">
                {{ cita.hora }}
              </span>
            </div>

            <!-- Acciones -->
            <div
              class="lg:col-span-3
              flex items-center justify-start lg:justify-end
              gap-2 mt-4 lg:mt-0
              pt-3 lg:pt-0
              border-t lg:border-t-0
              border-slate-200 dark:border-slate-700
              flex-wrap"
            >

              <template v-if="cita.estado === 'Pendiente' || cita.estado === 'EnConsulta'">

                <button
                  v-if="props.esAdmin"
                  @click="copiarLink(cita.linkReunion)"
                  class="px-4 py-2.5
                  bg-cyan-50 dark:bg-cyan-900/30
                  text-cyan-700 dark:text-cyan-300
                  rounded-full text-xs font-black
                  hover:bg-cyan-100 dark:hover:bg-cyan-800/40
                  transition-colors"
                >
                  COPIAR ENLACE
                </button>

                <span
                  v-if="!props.esAdmin && cita.estado === 'Pendiente'"
                  class="px-4 py-2.5 rounded-full text-xs font-black text-center
                  bg-blue-100 dark:bg-blue-900/30
                  text-blue-700 dark:text-blue-300
                  cursor-not-allowed select-none"
                >
                  🕐 PRÓXIMO A INICIAR
                </span>

                <a
                  v-if="!props.esAdmin && cita.estado === 'EnConsulta' && cita.linkReunion"
                  :href="cita.linkReunion"
                  target="_blank"
                  rel="noopener noreferrer"
                  class="px-6 py-2.5 rounded-full text-xs font-black text-center
                  bg-green-600 text-white
                  hover:bg-green-700
                  transition-all shadow-md animate-pulse"
                >
                  🟢 UNIRSE AHORA
                </a>

                <button
                  v-if="cita.estado === 'Pendiente' && (props.esAdmin || calcularTiempoRestante(cita.fechaCompleta).puedeCancelar)"
                  @click="ejecutarCancelar(cita)"
                  class="px-4 py-2.5 rounded-full text-xs font-black text-center
                  bg-red-600 text-white
                  hover:bg-red-700 transition-all shadow-md cursor-pointer"
                >
                  ✕ CANCELAR
                </button>

              </template>

              <!-- FINALIZADA -->
              <template v-if="cita.estado === 'Finalizada'">

                <div class="flex flex-col gap-2">

                  <button
                    @click="descargarPDF(cita.id)"
                    class="flex items-center gap-2
                    px-4 py-2.5
                    bg-green-600 text-white
                    rounded-xl font-bold text-sm
                    hover:bg-green-700 transition-all cursor-pointer"
                  >
                    📄 Receta PDF
                  </button>

                  <button
                    v-if="cita.tieneOrdenLaboratorio"
                    @click="descargarOrdenPDF(cita.consultaId)"
                    class="flex items-center gap-2
                    px-4 py-2.5
                    bg-teal-600 text-white
                    rounded-full font-bold text-xs
                    hover:bg-teal-700 transition-all"
                  >
                    🔬 Orden Lab PDF
                  </button>

                </div>

              </template>

              <!-- CANCELADA -->
              <div
                v-if="cita.estado === 'Cancelada'"
                class="px-4 py-2
                bg-red-50 dark:bg-red-900/30
                text-red-600 dark:text-red-300
                rounded-full font-bold text-[10px] uppercase"
              >
                Anulada
              </div>
              <div
                v-if="cita.estado === 'NoAsistida'"
                class="px-4 py-2
                bg-orange-50 dark:bg-orange-900/30
                text-orange-600 dark:text-orange-300
                rounded-full font-bold text-[10px] uppercase"
              >
                No asistida
              </div>

            </div>

          </div>

        </div>

        <!-- VACÍO -->
        <div
          v-else
          class="h-full flex flex-col items-center justify-center py-20"
        >
          <div
            class="w-24 h-24
            bg-slate-100 dark:bg-slate-800
            rounded-full flex items-center justify-center
            text-4xl mb-6 grayscale opacity-50"
          >
            📂
            
          </div>

          <h4
            class="text-slate-400 dark:text-slate-500
            font-bold uppercase tracking-[0.2em] text-sm"
          >
            No se encontraron registros
          </h4>
        </div>

      </main>

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

    <!-- MODAL CONFIRMACIÓN -->
    <Transition
      enter-active-class="transition-all duration-200 ease-out"
      enter-from-class="opacity-0"
      enter-to-class="opacity-100"
      leave-active-class="transition-all duration-150 ease-in"
      leave-from-class="opacity-100"
      leave-to-class="opacity-0"
    >
      <div
        v-if="confirmModal.visible"
        class="fixed inset-0 z-[99998] flex items-center justify-center bg-slate-950/80 backdrop-blur-sm p-4"
        @click.self="responderConfirm(false)"
      >
        <Transition
          enter-active-class="transition-all duration-200 ease-out"
          enter-from-class="opacity-0 scale-95 -translate-y-2"
          enter-to-class="opacity-100 scale-100 translate-y-0"
        >
          <div
            v-if="confirmModal.visible"
            class="bg-gradient-to-br from-[#0f2040] to-[#10284f] border border-cyan-500/20 rounded-2xl shadow-2xl w-full max-w-sm p-6 flex flex-col gap-5"
          >
            <div class="flex items-center gap-3">
              <div class="w-10 h-10 rounded-xl bg-amber-500/15 flex items-center justify-center shrink-0">
                <svg xmlns="http://www.w3.org/2000/svg" class="w-5 h-5 text-amber-400" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
                  <path stroke-linecap="round" stroke-linejoin="round" d="M12 9v4m0 4h.01M10.29 3.86L1.82 18a2 2 0 001.71 3h16.94a2 2 0 001.71-3L13.71 3.86a2 2 0 00-3.42 0z" />
                </svg>
              </div>
              <div>
                <p class="text-[10px] font-black text-amber-400 uppercase tracking-widest">Confirmación</p>
                <p class="text-white font-black text-base leading-tight">{{ confirmModal.titulo }}</p>
              </div>
            </div>
            <p class="text-slate-300 text-sm font-semibold leading-relaxed px-1">
              {{ confirmModal.mensaje }}
            </p>
            <div class="flex gap-3">
              <button
                @click="responderConfirm(false)"
                class="flex-1 py-3 rounded-xl bg-slate-700 hover:bg-slate-600 text-slate-300 font-black text-[10px] uppercase tracking-widest transition-colors"
              >
                Cancelar
              </button>
              <button
                @click="responderConfirm(true)"
                class="flex-[2] py-3 rounded-xl bg-amber-500 hover:bg-amber-400 text-slate-900 font-black text-[10px] uppercase tracking-widest transition-colors"
              >
                Confirmar
              </button>
            </div>
          </div>
        </Transition>
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
