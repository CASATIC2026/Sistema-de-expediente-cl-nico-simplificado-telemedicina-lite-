///ESTE FORMULARIO ESTÁ CONECTADO CON LA SALA DE CONSULTA,
//ES EL FORMULARIO QUE SE LLENA DURANTE LA CONSULTA Y AL GUARDARLO SE GENERA UN PDF
// CON LA INFORMACIÓN DE LA CONSULTA, EL DIAGNÓSTICO Y LOS MEDICAMENTOS RECETADOS


<script setup>
import { ref, watch, computed } from 'vue'
import api from '@/services/api'
import { useRouter } from 'vue-router'

const router = useRouter()

const props = defineProps({
  cita: Object
})

const emit = defineEmits(['consulta-guardada'])

// Campos del formulario
const sintomas      = ref('')
const evolucion     = ref('')
const diagnostico   = ref('')
const tratamiento   = ref('')
const observaciones = ref('')
const tieneIncapacidad = ref(false)
const fechaInicioIncapacidad = ref('')
const fechaFinIncapacidad = ref('')
const diasIncapacidad = ref(null)
const motivoIncapacidad = ref('')
const observacionesIncapacidad = ref('')
// orden de laboratorio
const tieneOrdenLaboratorio = ref(false)
const examenes = ref([
  { nombre: '', indicaciones: '' }
])

const agregarExamen = () => {
  examenes.value.push({ nombre: '', indicaciones: '' })
}

const eliminarExamen = (index) => {
  examenes.value.splice(index, 1)
}

const errorOrden = computed(() => {
  if (!tieneOrdenLaboratorio.value) return ''
  const hayExamenValido = examenes.value.some(e => e.nombre.trim())
  if (!hayExamenValido) return 'Debes agregar al menos un examen.'
  return ''
})
const guardado      = ref(false)
const consultaId    = ref(null)
const cargando      = ref(false)

const notificacion = ref({
  visible: false,
  mensaje: '',
  tipo: 'success'
})

const mostrarNotificacion = (
  mensaje,
  tipo = 'success'
) => {
  notificacion.value = {
    visible: true,
    mensaje,
    tipo
  }

  setTimeout(() => {
    notificacion.value.visible = false
  }, 3500)
}

const medicamentos = ref([
  { nombre: '', dosis: '', frecuencia: '', duracion: '' }
])

const agregarMedicamento = () => {
  medicamentos.value.push({ nombre: '', dosis: '', frecuencia: '', duracion: '' })
}

const eliminarMedicamento = (index) => {
  medicamentos.value.splice(index, 1)
}

const buildUtcDateString = (dateValue) => {
  if (!dateValue) return null
  const date = new Date(`${dateValue}T00:00:00Z`)
  return date.toISOString()
}

const actualizarDiasIncapacidad = () => {
  if (!fechaInicioIncapacidad.value || !fechaFinIncapacidad.value) {
    diasIncapacidad.value = null
    return
  }

  const inicio = new Date(fechaInicioIncapacidad.value)
  const fin = new Date(fechaFinIncapacidad.value)
  if (isNaN(inicio.getTime()) || isNaN(fin.getTime()) || fin < inicio) {
    diasIncapacidad.value = null
    return
  }

  diasIncapacidad.value = Math.floor((fin - inicio) / (1000 * 60 * 60 * 24)) + 1
}

watch([fechaInicioIncapacidad, fechaFinIncapacidad], actualizarDiasIncapacidad)

const diasIncapacidadInvalido = computed(() => {
  if (!tieneIncapacidad.value) return false
  if (!fechaInicioIncapacidad.value || !fechaFinIncapacidad.value) return false
  if (!diasIncapacidad.value) return true
  return diasIncapacidad.value <= 0
})

const fechaFinIncapacidadInvalida = computed(() => {
  if (!tieneIncapacidad.value) return false
  if (!fechaInicioIncapacidad.value || !fechaFinIncapacidad.value) return false
  return new Date(fechaFinIncapacidad.value) < new Date(fechaInicioIncapacidad.value)
})

const errorIncapacidad = computed(() => {
  if (!tieneIncapacidad.value) return ''
  if (!fechaInicioIncapacidad.value || !fechaFinIncapacidad.value) return 'Debes indicar fecha inicio y fecha fin de la incapacidad.'
  if (new Date(fechaFinIncapacidad.value) < new Date(fechaInicioIncapacidad.value)) return 'La fecha fin no puede ser anterior a la fecha inicio.'
  if (!diasIncapacidad.value || diasIncapacidad.value <= 0) return 'Debes ingresar un número válido de días.'
  if (!motivoIncapacidad.value.trim()) return 'Debes indicar el motivo de la incapacidad.'
  return ''
})

// Validación de campos
const formularioValido = () => {
  const camposTextoValidos =
    sintomas.value.trim() &&
    evolucion.value.trim() &&
    diagnostico.value.trim() &&
    tratamiento.value.trim() &&
    observaciones.value.trim()
// Al menos un medicamento con todos sus campos llenos
  const hayMedicamentoValido = medicamentos.value.some(m =>
    m.nombre.trim() &&
    m.dosis.trim() &&
    m.frecuencia.trim() &&
    m.duracion.trim()
  )

  const incapacidadValida = !tieneIncapacidad.value || (
    fechaInicioIncapacidad.value &&
    fechaFinIncapacidad.value &&
    motivoIncapacidad.value.trim() &&
    new Date(fechaFinIncapacidad.value) >= new Date(fechaInicioIncapacidad.value) &&
    diasIncapacidad.value > 0
  )
  const ordenValida = !tieneOrdenLaboratorio.value ||
    examenes.value.some(e => e.nombre.trim())

  return camposTextoValidos && hayMedicamentoValido && incapacidadValida && ordenValida
}

// ─── GUARDAR CONSULTA ─────────────────────────────────────────────
const guardarConsulta = async () => {
  if (!props.cita?.idCita) {
    mostrarNotificacion(
  'No se encontró el ID de la cita',
  'error'
)
    return
  }


  // VALIDACIÓN
  if (!formularioValido()) {
    if (errorIncapacidad.value) {
      mostrarNotificacion(
      errorIncapacidad.value,
      'error'
    )
    } else {
      mostrarNotificacion(
  "⚠️ Debes completar TODOS los campos y agregar al menos un medicamento válido",
  'error'
)
    }
    return
  }

  try {
    cargando.value = true

    const payload = {
      citaId:           props.cita.idCita,
      sintomas:         sintomas.value,
      evolucion:        evolucion.value,
      diagnostico:      diagnostico.value,
      tratamiento:      tratamiento.value,
      observaciones:    observaciones.value,
      medicamentosJson: JSON.stringify(medicamentos.value),
      tieneIncapacidad: tieneIncapacidad.value,
      fechaInicioIncapacidad: tieneIncapacidad.value ? buildUtcDateString(fechaInicioIncapacidad.value) : null,
      fechaFinIncapacidad:   tieneIncapacidad.value ? buildUtcDateString(fechaFinIncapacidad.value) : null,
      diasIncapacidad:       tieneIncapacidad.value ? diasIncapacidad.value : null,
      motivoIncapacidad:     tieneIncapacidad.value ? motivoIncapacidad.value : '',
      observacionesIncapacidad: tieneIncapacidad.value ? observacionesIncapacidad.value : '',

      tieneOrdenLaboratorio: tieneOrdenLaboratorio.value,
      examenesJson: tieneOrdenLaboratorio.value ? JSON.stringify(examenes.value) : null
    }

    const res = await api.post('/consultas', payload)

    consultaId.value = res.data.idConsulta
    guardado.value   = true

  emit('consulta-guardada')
  mostrarNotificacion("Consulta guardada correctamente", "success")

  } catch (error) {
    const msg = error.response?.data?.message || error.response?.data || "Error al guardar"
   mostrarNotificacion(msg, 'error')
    console.error(error)
  } finally {
    cargando.value = false
  }
}

// ─── DESCARGAR PDF ────────────────────────────────────────────────
const descargarPdf = async () => {
  try {
    cargando.value = true

    const res = await api.get(`/consultas/${consultaId.value}/pdf`, {
      responseType: 'blob'
    })

    const url  = window.URL.createObjectURL(new Blob([res.data]))
    const link = document.createElement('a')
    link.href  = url
    link.setAttribute('download', `consulta_${consultaId.value}.pdf`)
    document.body.appendChild(link)
    link.click()
    link.remove()

    // Redirección
    router.push('/app/Agenda')

  } catch (error) {
    mostrarNotificacion(
  'Error al descargar el PDF.',
  'error'
)
    console.error(error)
  } finally {
    cargando.value = false
  }
}

const descargarOrdenPdf = async () => {
  try {
    cargando.value = true
    const res = await api.get(`/consultas/${consultaId.value}/orden-pdf`, {
      responseType: 'blob'
    })
    const url  = window.URL.createObjectURL(new Blob([res.data]))
    const link = document.createElement('a')
    link.href  = url
    link.setAttribute('download', `orden_laboratorio_${consultaId.value}.pdf`)
    document.body.appendChild(link)
    link.click()
    link.remove()
  } catch (error) {
    mostrarNotificacion(
  'Error al descargar la orden de laboratorio.',
  'error'
)
    console.error(error)
  } finally {
    cargando.value = false
  }
}
</script>

<template>
  <div class="space-y-5 text-slate-800 dark:text-slate-100">

    <!-- TOAST -->
    <transition
      enter-active-class="transition duration-300 ease-out"
      enter-from-class="opacity-0 translate-y-2"
      enter-to-class="opacity-100 translate-y-0"
      leave-active-class="transition duration-200 ease-in"
      leave-from-class="opacity-100"
      leave-to-class="opacity-0 translate-y-2"
    >
      <div
        v-if="notificacion.visible"
        class="fixed bottom-5 right-5 z-[9999] px-5 py-4 rounded-2xl shadow-2xl border backdrop-blur-md max-w-sm"
        :class="notificacion.tipo === 'success'
          ? 'bg-emerald-500/95 border-emerald-300 text-white'
          : 'bg-red-500/95 border-red-300 text-white'"
      >
        <div class="flex items-start gap-3">
          <div class="text-lg">
            {{ notificacion.tipo === 'success' ? '✅' : '⚠️' }}
          </div>

          <div>
            <p class="font-black text-sm tracking-wide">
              {{ notificacion.tipo === 'success'
                ? 'Operación completada'
                : 'Ocurrió un problema'
              }}
            </p>

            <p class="text-sm opacity-90">
              {{ notificacion.mensaje }}
            </p>
          </div>
        </div>
      </div>
    </transition>

    <!-- FORMULARIO — se oculta tras guardar -->
    <template v-if="!guardado">

      <textarea
        v-model="sintomas"
        class="w-full rounded-xl border border-slate-300 dark:border-slate-700 bg-white dark:bg-slate-800/80 px-3 py-2.5 text-slate-800 dark:text-slate-100 placeholder:text-slate-400 dark:placeholder:text-slate-500 focus:outline-none focus:ring-2 focus:ring-sky-500/40 focus:border-sky-500 transition-all"
        placeholder="Síntomas"
        >
      </textarea>

      <textarea
      v-model="evolucion"
      class="w-full rounded-xl border border-slate-300 dark:border-slate-700 bg-white dark:bg-slate-800/80 px-3 py-2.5 text-slate-800 dark:text-slate-100 placeholder:text-slate-400 dark:placeholder:text-slate-500 focus:outline-none focus:ring-2 focus:ring-sky-500/40 focus:border-sky-500 transition-all"
      placeholder="Evolución">
    </textarea>

      <textarea
      v-model="diagnostico"
      class="w-full rounded-xl border border-slate-300 dark:border-slate-700 bg-white dark:bg-slate-800/80 px-3 py-2.5 text-slate-800 dark:text-slate-100 placeholder:text-slate-400 dark:placeholder:text-slate-500 focus:outline-none focus:ring-2 focus:ring-sky-500/40 focus:border-sky-500 transition-all"
      placeholder="Diagnóstico"
        >
      </textarea>

      <textarea
        v-model="tratamiento"
        class="w-full rounded-xl border border-slate-300 dark:border-slate-700 bg-white dark:bg-slate-800/80 px-3 py-2.5 text-slate-800 dark:text-slate-100 placeholder:text-slate-400 dark:placeholder:text-slate-500 focus:outline-none focus:ring-2 focus:ring-sky-500/40 focus:border-sky-500 transition-all"
        placeholder="Tratamiento"
      >
      </textarea>

      <textarea
        v-model="observaciones"
        class="w-full rounded-xl border border-slate-300 dark:border-slate-700 bg-white dark:bg-slate-800/80 px-3 py-2.5 text-slate-800 dark:text-slate-100 placeholder:text-slate-400 dark:placeholder:text-slate-500 focus:outline-none focus:ring-2 focus:ring-sky-500/40 focus:border-sky-500 transition-all"
        placeholder="Observaciones"
        >
      </textarea>

      <!-- INCAPACIDAD -->
      <div class="bg-slate-50 dark:bg-slate-800/60 border border-slate-200 dark:border-slate-700 rounded-2xl p-4 mb-4 transition-colors">
        <label class="flex items-center gap-3 cursor-pointer">
          <input
            type="checkbox"
            v-model="tieneIncapacidad"
            class="h-4 w-4 rounded border-slate-300 text-blue-600 focus:ring-blue-500"
          />
          <span class="font-bold text-slate-800 dark:text-slate-100">
             Generar incapacidad médica
          </span>
        </label>

        <div v-if="tieneIncapacidad" class="mt-4 space-y-4">
          <div class="grid grid-cols-1 sm:grid-cols-2 gap-3">
            <label class="block text-sm font-medium text-slate-700 dark:text-slate-300">
              Fecha inicio
              <input
                type="date"
                v-model="fechaInicioIncapacidad"
                class="mt-1 w-full rounded-xl border border-slate-300 dark:border-slate-700 bg-white dark:bg-slate-900/70 px-3 py-2 text-slate-800 dark:text-slate-100 placeholder:text-slate-400 dark:placeholder:text-slate-500 focus:outline-none focus:ring-2 focus:ring-sky-500/40 transition-all"
              />
            </label>
            <label class="block text-sm font-medium text-slate-700 dark:text-slate-300">
              Fecha fin
              <input
                type="date"
                v-model="fechaFinIncapacidad"
                :class="[
                  'mt-1 w-full rounded-xl border border-slate-300 dark:border-slate-700 bg-white dark:bg-slate-900/70 px-3 py-2 text-slate-800 dark:text-slate-100 placeholder:text-slate-400 dark:placeholder:text-slate-500 focus:outline-none focus:ring-2 focus:ring-sky-500/40 transition-all',
                  fechaFinIncapacidadInvalida ? 'border-red-500 ring-1 ring-red-200' : 'border-slate-300'
                ]"
              />
            </label>
          </div>

          <div class="grid grid-cols-1 sm:grid-cols-2 gap-3">
            <label class="block text-sm font-semibold text-slate-700 dark:text-slate-300">
              Días de incapacidad
              <input
                type="number"
                v-model.number="diasIncapacidad"
                min="1"
                :class="[
                  'mt-1 w-full rounded-xl border border-slate-300 dark:border-slate-700 bg-white dark:bg-slate-900/70 px-3 py-2 text-slate-800 dark:text-slate-100 placeholder:text-slate-400 dark:placeholder:text-slate-500 focus:outline-none focus:ring-2 focus:ring-sky-500/40 transition-all',
                  diasIncapacidadInvalido ? 'border-red-500 ring-1 ring-red-200' : 'border-slate-300'
                ]"
                placeholder="Automático o ajustable"
              />
              <p class="text-xs text-slate-500 dark:text-slate-400 mt-1">
                Se calcula automáticamente desde las fechas, pero puedes ajustarlo si es necesario.
              </p>
              <p v-if="diasIncapacidadInvalido" class="text-xs text-red-600 dark:text-red-400 mt-1">
                Ingresa un número válido de días (mayor a 0).
              </p>
            </label>
            <label class="block text-sm font-medium text-slate-700 dark:text-slate-300">
              Motivo
              <input
                type="text"
                v-model="motivoIncapacidad"
                class="mt-1 w-full rounded-xl border border-slate-300 dark:border-slate-700 bg-white dark:bg-slate-900/70 px-3 py-2 text-slate-800 dark:text-slate-100 placeholder:text-slate-400 dark:placeholder:text-slate-500 focus:outline-none focus:ring-2 focus:ring-sky-500/40 transition-all"
                placeholder="Motivo de la incapacidad"
              />
            </label>
          </div>

          <label class="block text-sm font-medium text-slate-700 dark:text-slate-300">
            Observaciones de incapacidad
            <textarea
              v-model="observacionesIncapacidad"
              class="mt-1 w-full rounded-xl border border-slate-300 dark:border-slate-700 bg-white dark:bg-slate-900/70 px-3 py-2 text-slate-800 dark:text-slate-100 placeholder:text-slate-400 dark:placeholder:text-slate-500 focus:outline-none focus:ring-2 focus:ring-sky-500/40 transition-all"
              rows="3"
              placeholder="Opcional"
            ></textarea>
          </label>

          <p class="text-sm text-slate-500 dark:text-slate-400">
            Se generará un documento de incapacidad en formato PDF y se enviará en automático al correo del paciente.
          </p>
        </div>
      </div>


      <!-- ORDEN DE LABORATORIO -->
      <div class="bg-slate-50 dark:bg-slate-800/60 border border-slate-200 dark:border-slate-700 rounded-2xl p-4 mb-4 transition-colors">
        <label class="flex items-center gap-3 cursor-pointer">
          <input
            type="checkbox"
            v-model="tieneOrdenLaboratorio"
            class="h-4 w-4 rounded border-slate-300 text-blue-600 focus:ring-blue-500"
          />
          <span class="font-semibold">Agregar orden de laboratorio</span>
        </label>

        <div v-if="tieneOrdenLaboratorio" class="mt-4 space-y-3">

          <div
            v-for="(examen, index) in examenes"
            :key="index"
            class="border border-slate-200 dark:border-slate-700 bg-white dark:bg-slate-900/70 rounded-xl p-3 space-y-2 transition-colors"
          >
            <div class="grid grid-cols-1 sm:grid-cols-2 gap-2">
              <label class="block text-sm font-medium text-slate-700 dark:text-slate-300">
                Nombre del examen
                <input
                  type="text"
                  v-model="examen.nombre"
                  class="mt-1 w-full rounded-xl border border-slate-300 dark:border-slate-700 bg-white dark:bg-slate-800/80 px-3 py-2 text-sm text-slate-800 dark:text-slate-100 placeholder:text-slate-400 dark:placeholder:text-slate-500 focus:outline-none focus:ring-2 focus:ring-teal-500/40 transition-all"
                  placeholder="Ej: Hemograma completo"
                />
              </label>
              <label class="block text-sm font-medium text-slate-700 dark:text-slate-300">
                Indicaciones
                <input
                  type="text"
                  v-model="examen.indicaciones"
                  class="mt-1 w-full rounded-xl border border-slate-300 dark:border-slate-700 bg-white dark:bg-slate-800/80 px-3 py-2 text-sm text-slate-800 dark:text-slate-100 placeholder:text-slate-400 dark:placeholder:text-slate-500 focus:outline-none focus:ring-2 focus:ring-teal-500/40 transition-all"
                  placeholder="Ej: En ayunas (opcional)"
                />
              </label>
            </div>
            <button
              v-if="examenes.length > 1"
              @click="eliminarExamen(index)"
              class="text-red-500 text-xs font-medium"
            >
              Eliminar examen
            </button>
          </div>

          <button
            @click="agregarExamen"
            class="bg-slate-800 hover:bg-slate-700 dark:bg-slate-700 dark:hover:bg-slate-600 text-white px-4 py-2 rounded-xl text-sm font-bold transition-all"
          >
            + Agregar examen
          </button>

          <p v-if="errorOrden" class="text-xs text-red-600">
            {{ errorOrden }}
          </p>

          <p class="text-sm text-slate-500 dark:text-slate-400">
            El paciente podrá descargar esta orden desde su portal una vez finalizada la consulta.
          </p>

        </div>
      </div>

      <!-- Medicamentos -->
      <div>
        <h3 class="font-semibold mb-2">Medicamentos</h3>

        <div
          v-for="(med, index) in medicamentos"
          :key="index"
          class="border border-slate-200 dark:border-slate-700 bg-slate-50 dark:bg-slate-800/50 p-3 rounded-2xl mb-3 space-y-2 transition-colors"
        >
          <input v-model="med.nombre"
          class="w-full rounded-xl border border-slate-300 dark:border-slate-700 bg-white dark:bg-slate-900/70 px-3 py-2 text-slate-800 dark:text-slate-100 placeholder:text-slate-400 dark:placeholder:text-slate-500 focus:outline-none focus:ring-2 focus:ring-sky-500/40 transition-all"
          placeholder="Nombre" />

          <input v-model="med.dosis"
          class="w-full rounded-xl border border-slate-300 dark:border-slate-700 bg-white dark:bg-slate-900/70 px-3 py-2 text-slate-800 dark:text-slate-100 placeholder:text-slate-400 dark:placeholder:text-slate-500 focus:outline-none focus:ring-2 focus:ring-sky-500/40 transition-all"
          placeholder="Dosis" />

          <input v-model="med.frecuencia"
          class="w-full rounded-xl border border-slate-300 dark:border-slate-700 bg-white dark:bg-slate-900/70 px-3 py-2 text-slate-800 dark:text-slate-100 placeholder:text-slate-400 dark:placeholder:text-slate-500 focus:outline-none focus:ring-2 focus:ring-sky-500/40 transition-all"
          placeholder="Frecuencia" />

          <input v-model="med.duracion"
          class="w-full rounded-xl border border-slate-300 dark:border-slate-700 bg-white dark:bg-slate-900/70 px-3 py-2 text-slate-800 dark:text-slate-100 placeholder:text-slate-400 dark:placeholder:text-slate-500 focus:outline-none focus:ring-2 focus:ring-sky-500/40 transition-all"
          placeholder="Duración" />

          <button @click="eliminarMedicamento(index)" class="text-red-500 text-sm">
            Eliminar
          </button>
        </div>

        <button @click="agregarMedicamento"
        class="bg-sky-600 hover:bg-sky-500 text-white px-4 py-2 rounded-xl font-bold transition-all shadow-sm">
          + Agregar medicamento
        </button>
      </div>

      <div v-if="errorIncapacidad" class="rounded-lg border border-red-200 bg-red-50 p-3 mb-3 text-sm text-red-700 dark:text-red-400">
        {{ errorIncapacidad }}
      </div>

      <button
        @click="guardarConsulta"
        :disabled="cargando"
        :class="[
          'text-white px-5 py-3 rounded-2xl w-full disabled:opacity-50 transition-all font-black shadow-sm',
          errorIncapacidad ? 'bg-red-600 hover:bg-red-700' : 'bg-blue-600 hover:bg-blue-700'
        ]"
      >
        {{ cargando ? 'Guardando...' : (errorIncapacidad ? 'Revisa los errores' : 'Guardar diagnóstico') }}
      </button>

    </template>

    <!-- TRAS GUARDAR — muestra botón de PDF -->
    <template v-else>
      <div class="bg-emerald-500/10 dark:bg-emerald-500/15 border border-emerald-300 dark:border-emerald-500/20 rounded-3xl p-6 text-center space-y-4 transition-colors">

        <div class="text-emerald-600 text-4xl">✅</div>

        <p class="text-emerald-700 dark:text-emerald-300 font-black text-lg">
          Consulta guardada correctamente
        </p>

        <p class="text-emerald-700 dark:text-emerald-300 font-black text-lg">
          Puedes descargar la receta médica en PDF para el paciente.
        </p>

        <button
          @click="descargarPdf"
          :disabled="cargando"
          class="bg-sky-600 hover:bg-sky-500 text-white px-6 py-2.5 rounded-xl font-bold disabled:opacity-50 transition-all"
        >
          {{ cargando ? 'Generando PDF...' : '📄 Descargar receta PDF' }}
        </button>

        <button
          v-if="tieneOrdenLaboratorio"
          @click="descargarOrdenPdf"
          :disabled="cargando"
          class="w-full bg-teal-600 hover:bg-teal-500 text-white px-6 py-2.5 rounded-xl font-bold disabled:opacity-50 transition-all"
        >
          {{ cargando ? 'Generando PDF...' : '🔬 Descargar orden de laboratorio' }}
        </button>

      </div>
    </template>

  </div>
</template>
