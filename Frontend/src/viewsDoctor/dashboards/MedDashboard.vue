<script setup>
import { computed, onMounted, onUnmounted, ref } from 'vue'
import StatCard from '@/componentsDoctor/ui/StatsCard.vue'
import TablaCitasHoy from '@/componentsDoctor/citas/TablaCitasHoy.vue'
import { useCitasStore } from '@/stores/citas'
import BannerClinico from '@/components/BannerClinico.vue'
import { getClinicProfile } from '@/services/api'
import {
  CalendarDaysIcon,
  ClockIcon,
  UserPlusIcon,
  CheckCircleIcon
} from '@heroicons/vue/24/outline'


const citasStore = useCitasStore()
const citas = computed(() => citasStore.citas)

const clinicInfo = ref({
  clinicName: '',
  slogan: '',
  horario: '',
  telefono: ''
})


const cargarClinicProfile = async () => {
  try {
    const data = await getClinicProfile()
    clinicInfo.value = data
  } catch (e) {
    console.error('Error cargando perfil clínico:', e)
  }
}

let intervalId = null

onMounted(async () => {
  try {
    if (typeof citasStore.$reset === 'function') {
      citasStore.$reset()
    }

    await Promise.all([
      citasStore.cargarCitas(),
      citasStore.cargarResumen(),
      cargarClinicProfile()
    ])
  } catch (error) {
    console.error('Error al inicializar los datos del doctor:', error)
  }

  intervalId = setInterval(() => {
    citasStore.cargarCitas()
    citasStore.cargarResumen()
  }, 30000)
})

onUnmounted(() => {
  clearInterval(intervalId)
})

// SALUDO
const saludo = computed(() => {
  const hora = new Date().getHours()
  if (hora < 12) return 'Buenos días'
  if (hora < 19) return 'Buenas tardes'
  return 'Buenas noches'
})

// FECHA HOY FORMATEADA
const fechaHoy = new Date().toLocaleDateString('es-SV', {
  weekday: 'long',
  year: 'numeric',
  month: 'long',
  day: 'numeric'
})

// CITAS DE HOY
const citasHoyLista = computed(() => {
  if (!Array.isArray(citas.value)) return []

  const hoy = new Date()
  hoy.setHours(0, 0, 0, 0)


  return citas.value.filter(c => {
    const fechaCita = new Date(c.start)
    fechaCita.setHours(0, 0, 0, 0)
    return fechaCita.getTime() === hoy.getTime()
  })
})

// CONTADORES
const citasHoy = computed(() => citasHoyLista.value.length)

const enEspera = computed(() =>
  citasHoyLista.value.filter(c => c.estado === 'Pendiente').length
)

const enConsulta = computed(() =>
  citasHoyLista.value.filter(c => c.estado === 'EnConsulta').length
)

const finalizadas = computed(() =>
  citasHoyLista.value.filter(c => c.estado === 'Finalizada').length
)
</script>

<template>

  <main class="space-y-6 p-4 md:p-6 text-slate-800 dark:text-slate-100">

    <!-- Banner clínico -->
    <div class="w-full flex justify-center">
      <div class="w-full max-w-6xl">
        <BannerClinico
        compact
          :clinicName="clinicInfo.clinicName"
          :slogan="clinicInfo.slogan"
          :horario="clinicInfo.horario"
          :telefono="clinicInfo.telefono"
        />
      </div>
    </div>

    <!-- SALUDO -->
    <div class="border-b border-slate-200 dark:border-slate-800 pb-4">
      <h1 class="text-2xl font-bold text-slate-800 dark:text-white">
        {{ saludo }}, Dr(a)
      </h1>
      <p class="text-slate-500 dark:text-slate-400 first-letter:uppercase">
        {{ fechaHoy }}
      </p>
    </div>

    <!-- STATS -->
<div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">

  <StatCard
    title="Citas hoy"
    :value="citasHoy"
    color="text-sky-500"
    :icon="CalendarDaysIcon"
  />

  <StatCard
    title="En espera"
    :value="enEspera"
    color="text-amber-500"
    :icon="ClockIcon"
  />

  <StatCard
    title="En consulta"
    :value="enConsulta"
    color="text-teal-500"
    :icon="UserPlusIcon"
  />

  <StatCard
    title="Finalizadas"
    :value="finalizadas"
    color="text-emerald-500"
    :icon="CheckCircleIcon"
  />

</div>

    <!-- TABLA -->
    <div class="bg-white dark:bg-[#071120]
      rounded-2xl
      shadow-sm
      border border-slate-200 dark:border-slate-800
      overflow-hidden">
      <div class="px-5 py-4 border-b border-slate-100 dark:border-slate-800 bg-slate-50 dark:bg-slate-900/40">
        <h1 class="font-bold text-gray-700">Resumen para este día</h1>
      </div>
      <TablaCitasHoy :citas="citasHoyLista" />
    </div>
  </main>
</template>
