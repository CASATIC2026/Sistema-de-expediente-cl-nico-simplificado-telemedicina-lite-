///ESTE FORMULARIO ESTÁ CONECTADO CON LA SALA DE CONSULTA,
//ES EL FORMULARIO QUE SE LLENA DURANTE LA CONSULTA Y AL GUARDARLO SE GENERA UN PDF
// CON LA INFORMACIÓN DE LA CONSULTA, EL DIAGNÓSTICO Y LOS MEDICAMENTOS RECETADOS


<script setup>
import { ref, watch, computed } from 'vue'
import api from '@/services/api'
import { useRouter } from 'vue-router'
import { useVideoStore } from '@/stores/videoStore'
import { finalizarConsulta as finalizarConsultaAPI } from '@/services/api'

const router = useRouter()
const videoStore = useVideoStore()

const props = defineProps({
  cita: Object
})

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
const guardado      = ref(false)
const consultaId    = ref(null)
const cargando      = ref(false)

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

  return camposTextoValidos && hayMedicamentoValido && incapacidadValida
}

// ─── GUARDAR CONSULTA ─────────────────────────────────────────────
const guardarConsulta = async () => {
  if (!props.cita?.idCita) {
    alert("No se encontró el ID de la cita")
    return
  }

  // VALIDACIÓN
  if (!formularioValido()) {
    if (errorIncapacidad.value) {
      alert(`⚠️ ${errorIncapacidad.value}`)
    } else {
      alert("⚠️ Debes completar TODOS los campos y agregar al menos un medicamento válido")
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
      observacionesIncapacidad: tieneIncapacidad.value ? observacionesIncapacidad.value : ''
    }

    const res = await api.post('/consultas', payload)

    consultaId.value = res.data.idConsulta
    guardado.value   = true

    // FINALIZAR CONSULTA AQUÍ
    try {
      await finalizarConsultaAPI(props.cita.idCita)
    } catch {
      console.warn('No se pudo finalizar en backend')
    }

    // CERRAR VIDEOLLAMADA
    videoStore.finalizarLlamada()

    alert("Consulta guardada y finalizada correctamente")

  } catch (error) {
    const msg = error.response?.data?.message || error.response?.data || "Error al guardar"
    alert(`❌ ${msg}`)
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
    alert("❌ Error al descargar el PDF")
    console.error(error)
  } finally {
    cargando.value = false
  }
}
</script>

<template>
  <div class="space-y-4">

    <h2 class="text-lg font-semibold">Ficha clínica</h2>

    <!-- FORMULARIO — se oculta tras guardar -->
    <template v-if="!guardado">

      <textarea v-model="sintomas"      class="w-full border p-2 rounded" placeholder="Síntomas"></textarea>
      <textarea v-model="evolucion"     class="w-full border p-2 rounded" placeholder="Evolución"></textarea>
      <textarea v-model="diagnostico"   class="w-full border p-2 rounded" placeholder="Diagnóstico"></textarea>
      <textarea v-model="tratamiento"   class="w-full border p-2 rounded" placeholder="Tratamiento"></textarea>
      <textarea v-model="observaciones" class="w-full border p-2 rounded" placeholder="Observaciones"></textarea>

      <div class="bg-slate-50 border border-slate-200 rounded-xl p-4 mb-4">
        <label class="flex items-center gap-3 cursor-pointer">
          <input
            type="checkbox"
            v-model="tieneIncapacidad"
            class="h-4 w-4 rounded border-slate-300 text-blue-600 focus:ring-blue-500"
          />
          <span class="font-semibold">Generar incapacidad médica</span>
        </label>

        <div v-if="tieneIncapacidad" class="mt-4 space-y-4">
          <div class="grid grid-cols-1 sm:grid-cols-2 gap-3">
            <label class="block text-sm font-medium text-slate-700">
              Fecha inicio
              <input
                type="date"
                v-model="fechaInicioIncapacidad"
                class="mt-1 w-full border rounded p-2"
              />
            </label>
            <label class="block text-sm font-medium text-slate-700">
              Fecha fin
              <input
                type="date"
                v-model="fechaFinIncapacidad"
                :class="[
                  'mt-1 w-full border rounded p-2',
                  fechaFinIncapacidadInvalida ? 'border-red-500 ring-1 ring-red-200' : 'border-slate-300'
                ]"
              />
            </label>
          </div>

          <div class="grid grid-cols-1 sm:grid-cols-2 gap-3">
            <label class="block text-sm font-medium text-slate-700">
              Días de incapacidad
              <input
                type="number"
                v-model.number="diasIncapacidad"
                min="1"
                :class="[
                  'mt-1 w-full border rounded p-2',
                  diasIncapacidadInvalido ? 'border-red-500 ring-1 ring-red-200' : 'border-slate-300'
                ]"
                placeholder="Automático o ajustable"
              />
              <p class="text-xs text-slate-500 mt-1">
                Se calcula automáticamente desde las fechas, pero puedes ajustarlo si es necesario.
              </p>
              <p v-if="diasIncapacidadInvalido" class="text-xs text-red-600 mt-1">
                Ingresa un número válido de días (mayor a 0).
              </p>
            </label>
            <label class="block text-sm font-medium text-slate-700">
              Motivo
              <input
                type="text"
                v-model="motivoIncapacidad"
                class="mt-1 w-full border rounded p-2"
                placeholder="Motivo de la incapacidad"
              />
            </label>
          </div>

          <label class="block text-sm font-medium text-slate-700">
            Observaciones de incapacidad
            <textarea
              v-model="observacionesIncapacidad"
              class="mt-1 w-full border rounded p-2"
              rows="3"
              placeholder="Opcional"
            ></textarea>
          </label>

          <p class="text-sm text-slate-500">
            La incapacidad se adjuntará al mismo PDF y, si el paciente tiene correo, también se le enviará una copia.
          </p>
        </div>
      </div>

      <div>
        <h3 class="font-semibold mb-2">Medicamentos</h3>

        <div
          v-for="(med, index) in medicamentos"
          :key="index"
          class="border p-2 rounded mb-2 space-y-2"
        >
          <input v-model="med.nombre"    class="w-full border p-1 rounded" placeholder="Nombre" />
          <input v-model="med.dosis"     class="w-full border p-1 rounded" placeholder="Dosis" />
          <input v-model="med.frecuencia" class="w-full border p-1 rounded" placeholder="Frecuencia" />
          <input v-model="med.duracion"  class="w-full border p-1 rounded" placeholder="Duración" />
          <button @click="eliminarMedicamento(index)" class="text-red-500 text-sm">
            Eliminar
          </button>
        </div>

        <button @click="agregarMedicamento" class="bg-gray-800 text-white px-2 py-1 rounded">
          + Agregar medicamento
        </button>
      </div>

      <div v-if="errorIncapacidad" class="rounded-lg border border-red-200 bg-red-50 p-3 mb-3 text-sm text-red-700">
        {{ errorIncapacidad }}
      </div>

      <button
        @click="guardarConsulta"
        :disabled="cargando"
        :class="[
          'text-white px-4 py-2 rounded-lg w-full disabled:opacity-50 transition-colors',
          errorIncapacidad ? 'bg-red-600 hover:bg-red-700' : 'bg-blue-600 hover:bg-blue-700'
        ]"
      >
        {{ cargando ? 'Guardando...' : (errorIncapacidad ? 'Revisa los errores' : 'Guardar diagnóstico') }}
      </button>

    </template>

    <!-- TRAS GUARDAR — muestra botón de PDF -->
    <template v-else>
      <div class="bg-emerald-50 border border-emerald-200 rounded-xl p-6 text-center space-y-4">

        <div class="text-emerald-600 text-4xl">✅</div>

        <p class="text-emerald-700 font-semibold text-lg">
          Consulta guardada correctamente
        </p>

        <p class="text-slate-500 text-sm">
          Puedes descargar la receta médica en PDF para el paciente.
        </p>

        <button
          @click="descargarPdf"
          :disabled="cargando"
          class="bg-blue-600 hover:bg-blue-700 text-white px-6 py-2 rounded-lg font-medium disabled:opacity-50"
        >
          {{ cargando ? 'Generando PDF...' : '📄 Descargar receta PDF' }}
        </button>

      </div>
    </template>

  </div>
</template>
