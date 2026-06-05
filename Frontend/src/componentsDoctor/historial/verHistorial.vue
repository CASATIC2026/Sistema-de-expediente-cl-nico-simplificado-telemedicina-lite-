<script setup>
import { ref, onMounted } from 'vue'
import api from '@/services/api'

const props = defineProps({
  usuarioSeleccionado: Object
})

const emit = defineEmits(['cerrar'])

const historial = ref([])
const consultaSeleccionada = ref(null)

onMounted(async () => {
  try {
    const res = await api.get(`/consultas/paciente/${props.usuarioSeleccionado.id}`)

    historial.value = res.data.map(c => ({
      id: c.idConsulta,
      fecha: new Date(c.fecha).toLocaleDateString(),
      doctor: c.cita?.doctor
        ? `${c.cita.doctor.nombre} ${c.cita.doctor.apellido}`
        : "Sin asignar",
      especialidad: "General",
      diagnostico: c.diagnostico,
      urlReceta: `http://localhost:5050/api/consultas/${c.idConsulta}/pdf`
    }))

  } catch (error) {
    console.error("Error cargando historial:", error)
  }
})

const verDetalle = (consulta) => {
  consultaSeleccionada.value = consulta
}
</script>

<template>
  <div
    class="fixed inset-0 z-50 flex items-center justify-center
           bg-black/60 backdrop-blur-sm p-4"
    @click.self="emit('cerrar')"
  >

    <div
      class="bg-white dark:bg-slate-900
             w-full max-w-2xl max-h-[90vh]
             overflow-y-auto
             rounded-3xl p-6
             border border-slate-200 dark:border-slate-800
             shadow-2xl dark:shadow-black/40
             transition-colors duration-300"
    >

      <!-- HEADER -->
      <div class="flex items-center justify-between mb-6">
        <h2
          v-if="!consultaSeleccionada"
          class="text-2xl font-black text-slate-800 dark:text-white"
        >
          Historial del paciente
        </h2>

        <button
          @click="emit('cerrar')"
          class="w-10 h-10 rounded-xl
                 bg-slate-100 dark:bg-slate-800
                 text-slate-500 dark:text-slate-300
                 hover:bg-red-500 hover:text-white
                 transition-all font-bold"
        >
          ✖
        </button>
      </div>

      <!-- LISTA -->
      <div v-if="!consultaSeleccionada">

        <!-- EMPTY -->
        <div
          v-if="historial.length === 0"
          class="py-16 text-center"
        >
          <div
            class="w-20 h-20 mx-auto mb-4 rounded-full
                   bg-slate-100 dark:bg-slate-800
                   flex items-center justify-center"
          >
            📁
          </div>

          <p class="text-slate-500 dark:text-slate-400 font-bold">
            Este paciente aún no tiene consultas registradas
          </p>
        </div>

        <!-- ITEMS -->
        <div
          v-for="c in historial"
          :key="c.id"
          class="border border-slate-200 dark:border-slate-800
                 bg-slate-50 dark:bg-slate-950/60
                 p-5 rounded-2xl mb-4
                 transition-all hover:border-blue-400/40"
        >

          <div class="space-y-2">
            <p class="text-slate-700 dark:text-slate-200">
              <strong>Fecha:</strong> {{ c.fecha }}
            </p>

            <p class="text-slate-700 dark:text-slate-200">
              <strong>Doctor:</strong> {{ c.doctor }}
            </p>
          </div>

          <button
            @click="verDetalle(c)"
            class="mt-4 text-sm font-bold
                   text-blue-600 dark:text-cyan-400
                   hover:underline"
          >
            Ver detalles
          </button>

        </div>

      </div>

      <!-- DETALLE -->
      <div v-else>

        <button
          @click="consultaSeleccionada = null"
          class="mb-5 text-sm font-bold
                 text-slate-500 dark:text-slate-300
                 hover:text-blue-600 dark:hover:text-cyan-400
                 transition-colors"
        >
          ← Volver
        </button>

        <div class="space-y-4">

          <p class="text-slate-700 dark:text-slate-200">
            <strong>Fecha:</strong>
            {{ consultaSeleccionada.fecha }}
          </p>

          <p class="text-slate-700 dark:text-slate-200">
            <strong>Doctor:</strong>
            {{ consultaSeleccionada.doctor }}
          </p>

          <div
            class="mt-4 p-4 rounded-2xl
                   bg-slate-50 dark:bg-slate-950/60
                   border border-slate-200 dark:border-slate-800"
          >
            <p class="font-bold text-slate-800 dark:text-white mb-2">
              Diagnóstico
            </p>

            <p class="text-slate-600 dark:text-slate-300 whitespace-pre-line">
              {{ consultaSeleccionada.diagnostico }}
            </p>
          </div>

          <a
            :href="consultaSeleccionada.urlReceta"
            target="_blank"
            class="block mt-6
                   bg-green-600 hover:bg-green-700
                   text-white
                   px-4 py-3 rounded-2xl
                   text-center font-bold
                   transition-all shadow-lg shadow-green-500/20"
          >
            Descargar PDF
          </a>

        </div>
      </div>
    </div>
  </div>
</template>
