<script setup>
import { computed, onMounted } from 'vue'
import StatCard from '@/componentsDoctor/ui/StatsCard.vue'
import TablaCitasHoy from '@/componentsDoctor/citas/TablaCitasHoy.vue'
import { useCitasStore } from '@/stores/citas'

const citasStore = useCitasStore()
const citas = computed(() => citasStore.citas)

onMounted(() => {
  citasStore.cargarCitas()
  citasStore.cargarResumen()
  setInterval(() => {
    citasStore.cargarCitas()
    citasStore.cargarResumen()
  }, 30000)
})

/* LÓGICA DE SALUDO Y FECHA (Se mantiene igual) */
const saludo = computed(() => {
  const hora = new Date().getHours()
  if (hora < 12) return "Buenos días"
  if (hora < 19) return "Buenas tardes"
  return "Buenas noches"
})

const fechaHoy = new Date().toLocaleDateString('es-SV', {
  weekday: 'long', year: 'numeric', month: 'long', day: 'numeric'
})

/* LÓGICA DE CITAS (Se mantiene igual) */
const citasHoyLista = computed(() => {
  if (!Array.isArray(citas.value)) return []
  const hoy = new Date().toDateString()
  return citas.value.filter(c => new Date(c.start).toDateString() === hoy)
})

const proximaCita = computed(() => {
  const ahora = new Date()
  const futuras = citasHoyLista.value
    .filter(c => new Date(c.Start) > ahora)
    .sort((a,b) => new Date(a.Start) - new Date(b.Start))
  return futuras[0] || null
})

const citasHoy = computed(() => citasHoyLista.value.length)
const enEspera = computed(() => citasHoyLista.value.filter(c => c.estado === "Pendiente").length)
const enConsulta = computed(() => citasHoyLista.value.filter(c => c.estado === "EnConsulta").length)
const finalizadas = computed(() => citasHoyLista.value.filter(c => c.estado === "Finalizada").length)
</script>

<template>
  <!-- Eliminamos el Header y Navbar de aquí, porque ya están en el Layout -->
  <main class="space-y-6">

    <!-- SALUDO -->
    <div class="border-b border-gray-200 pb-4">
      <h1 class="text-2xl font-bold text-gray-800">
        {{ saludo }}, Doctor
      </h1>
      <p class="text-slate-500 first-letter:uppercase">
        {{ fechaHoy }}
      </p>
    </div>

    <!-- PRÓXIMA CITA -->
    <div v-if="proximaCita" class="bg-white border border-blue-100 rounded-xl shadow-sm p-5">
       <!-- ... tu código de próxima cita ... -->
    </div>

    <!-- STATS -->
    <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
      <StatCard title="Citas hoy" :value="citasHoy" color="text-sky-500"/>
      <StatCard title="En espera" :value="enEspera" color="text-amber-500" />
      <StatCard title="En consulta" :value="enConsulta" color="text-teal-600" />
      <StatCard title="Finalizadas" :value="finalizadas" color="text-emerald-500" />
    </div>

    <!-- TABLA -->
    <div class="bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden">
      <div class="px-5 py-4 border-b border-gray-50 bg-gray-50/50">
        <h1 class="font-bold text-gray-700">Resumen para este día</h1>
      </div>
      <TablaCitasHoy :citas="citasHoyLista" />
    </div>
  </main>
</template>
