///ESTE FORMULARIO ESTÁ CONECTADO CON LA SALA DE CONSULTA,
//ES EL FORMULARIO QUE SE LLENA DURANTE LA CONSULTA Y AL GUARDARLO SE GENERA UN PDF
// CON LA INFORMACIÓN DE LA CONSULTA, EL DIAGNÓSTICO Y LOS MEDICAMENTOS RECETADOS


<script setup>
import { ref } from 'vue'
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

  return camposTextoValidos && hayMedicamentoValido
}

// ─── GUARDAR CONSULTA ─────────────────────────────────────────────
const guardarConsulta = async () => {
  if (!props.cita?.idCita) {
    alert("No se encontró el ID de la cita")
    return
  }

  // VALIDACIÓN
  if (!formularioValido()) {
    alert("⚠️ Debes completar TODOS los campos y agregar al menos un medicamento válido")
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
      medicamentosJson: JSON.stringify(medicamentos.value)
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

    // Redireción
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

      <button
        @click="guardarConsulta"
        :disabled="cargando"
        class="bg-blue-600 hover:bg-blue-700 text-white px-4 py-2 rounded-lg w-full disabled:opacity-50"
      >
        {{ cargando ? 'Guardando...' : 'Guardar diagnóstico' }}
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
