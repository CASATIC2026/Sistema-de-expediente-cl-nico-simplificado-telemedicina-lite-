<script setup>
import { ref, onMounted } from 'vue'
import { Bar } from 'vue-chartjs'

import {
  Chart as ChartJS,
  Title,
  Tooltip,
  Legend,
  BarElement,
  CategoryScale,
  LinearScale
} from 'chart.js'

import {
  ChartBarIcon,
  BeakerIcon,
  DocumentTextIcon,
  UserGroupIcon,
  HeartIcon,
  XMarkIcon
} from '@heroicons/vue/24/outline'

import api from '@/services/api'

ChartJS.register(
  Title,
  Tooltip,
  Legend,
  BarElement,
  CategoryScale,
  LinearScale
)

defineEmits(['cerrar'])

const stats = ref(null)

const filtros = ref({
  ordenes: '',
  incapacidades: '',
  citas: '',
  doctores: '',
  pacientes: ''
})

const cargarStats = async () => {
  try {
    const res = await api.get('/stats')
    stats.value = res.data
  } catch (e) {
    console.error(e)
  }
}

onMounted(() => {
  cargarStats()
})

// Configuración para los gráficos con modo oscuro disponible
const isDark = document.documentElement.classList.contains('dark')
const chartOptions = {
  responsive: true,
  maintainAspectRatio: false,

  plugins: {
    legend: {
      display: false
    }
  },

  scales: {
    x: {
      ticks: {
        color: isDark ? '#e2e8f0' : '#334155'
      },
      grid: {
        color: isDark
          ? 'rgba(148,163,184,0.1)'
          : 'rgba(51,65,85,0.08)'
      }
    },

    y: {
      beginAtZero: true,

      ticks: {
        precision: 0,
        color: isDark ? '#e2e8f0' : '#334155'
      },

      grid: {
        color: isDark
          ? 'rgba(148,163,184,0.1)'
          : 'rgba(51,65,85,0.08)'
      }
    }
  }
}

const crearChartData = (array, color) => ({
  labels: array.map(x => x.mes),
  datasets: [
    {
      label: 'Total',
      data: array.map(x => x.total),
      backgroundColor: color,
      borderRadius: 10,
      borderSkipped: false,
      maxBarThickness: 45
    }
  ]
})

// Helper para filtrar por mes
const filtrarPorMes = (array, filtro) => {
  if (!filtro) return array

  return array.filter(x => x.mes === filtro)
}

const obtenerMeses = (array) => {
  return [...new Set(array.map(x => x.mes))]
}

</script>

<template>
  <div class="min-h-screen p-4 transition-colors duration-300 md:p-6 bg-slate-100 dark:bg-slate-950">

    <!-- HEADER -->
    <div
      class="flex flex-col gap-4 mb-6 sm:flex-row sm:items-center sm:justify-between"
    >
      <div>
        <h1 class="text-3xl font-black tracking-tight text-slate-800 dark:text-slate-100">
          Estadísticas
        </h1>

        <p class="mt-1 text-sm text-slate-500 dark:text-slate-400">
          Panel analítico de TelMed Lite™
        </p>
      </div>

      <button
        @click="$emit('cerrar')"
       class="flex items-center gap-2 px-4 py-2 font-medium text-white transition rounded-2xl bg-slate-800 dark:bg-slate-700 hover:bg-slate-700 dark:hover:bg-slate-600"
      >
        <XMarkIcon class="w-5 h-5" />
        Cerrar
      </button>
    </div>

    <!-- LOADING -->
    <div
      v-if="!stats"
      class="flex items-center justify-center h-[60vh]"
    >
      <div class="text-center">
        <div
          class="w-16 h-16 mx-auto mb-4 border-4 rounded-full border-slate-300 dark:border-slate-700 border-t-slate-700 dark:border-t-cyan-400 animate-spin"
        />

        <p class="font-medium text-slate-600 dark:text-slate-300">
          Cargando estadísticas...
        </p>
      </div>
    </div>

    <!-- CONTENT -->
    <div
      v-else
      class="grid grid-cols-1 gap-6 2xl:grid-cols-12"
    >

      <!-- COLUMNA IZQUIERDA -->
      <div class="grid gap-6 2xl:col-span-8">

        <!-- GRID GRÁFICOS -->
        <div class="grid grid-cols-1 gap-6 xl:grid-cols-2">

          <!-- ÓRDENES -->
          <div class="p-5 transition-colors duration-300 bg-white shadow-sm dark:bg-slate-900 rounded-3xl border border-slate-200 dark:border-slate-700">
            <div class="flex items-center gap-3 mb-4">
              <div class="p-3 text-blue-700 bg-blue-100 rounded-2xl">
                <BeakerIcon class="w-6 h-6" />
              </div>

              <div>
                <h2 class="text-lg font-bold text-slate-800 dark:text-slate-100">
                  Órdenes de laboratorio
                </h2>

                <p class="text-sm text-slate-500">
                  Órdenes emitidas por mes
                </p>
              </div>
            </div>
            <div class="flex justify-end mb-4">

          <select
            v-model="filtros.ordenes"
            class="px-3 py-2 text-sm transition border rounded-xl border-slate-200 dark:border-slate-700 bg-white dark:bg-slate-800 text-slate-700 dark:text-slate-100"
          >
            <option value="">
              Todos los meses
            </option>

            <option
              v-for="mes in obtenerMeses(stats.ordenesPorMes)"
              :key="mes"
              :value="mes"
            >
              {{ mes }}
            </option>
          </select>
        </div>

            <div class="h-72">

              <Bar
              :data="crearChartData(
                filtrarPorMes(stats.ordenesPorMes, filtros.ordenes),
                '#3B82F6'
              )"
              :options="chartOptions"
            />
            </div>
          </div>

          <!-- INCAPACIDADES -->
          <div class="p-5 transition-colors duration-300 bg-white shadow-sm dark:bg-slate-900 rounded-3xl border border-slate-200 dark:border-slate-700">
            <div class="flex items-center gap-3 mb-4">
              <div class="p-3 text-amber-700 bg-amber-100 rounded-2xl">
                <DocumentTextIcon class="w-6 h-6" />
              </div>

              <div>
                <h2 class="text-lg font-bold text-slate-800 dark:text-slate-100">
                  Incapacidades emitidas
                </h2>

                <p class="text-sm text-slate-500">
                  Incapacidades generadas por mes
                </p>
              </div>
            </div>

            <div class="flex justify-end mb-4">
            <select
              v-model="filtros.incapacidades"
              class="px-3 py-2 text-sm transition border rounded-xl border-slate-200 dark:border-slate-700 bg-white dark:bg-slate-800 text-slate-700 dark:text-slate-100"
            >
              <option value="">
                Todos los meses
              </option>

              <option
                v-for="mes in obtenerMeses(stats.incapacidadesPorMes)"
                :key="mes"
                :value="mes"
              >
                {{ mes }}
              </option>
            </select>
          </div>

            <div class="h-72">
              <Bar
              :data="crearChartData(
                filtrarPorMes(stats.incapacidadesPorMes, filtros.incapacidades),
                '#F59E0B'
              )"
              :options="chartOptions"
            />
            </div>
          </div>

        </div>

        <!-- CITAS -->
        <div class="p-5 transition-colors duration-300 bg-white shadow-sm dark:bg-slate-900 rounded-3xl border border-slate-200 dark:border-slate-700">
          <div class="flex items-center gap-3 mb-4">
            <div class="p-3 text-emerald-700 bg-emerald-100 rounded-2xl">
              <ChartBarIcon class="w-6 h-6" />
            </div>

            <div>
              <h2 class="text-lg font-bold text-slate-800 dark:text-slate-100">
                Citas totales
              </h2>

              <p class="text-sm text-slate-500">
                Flujo mensual de citas médicas
              </p>
            </div>
          </div>

          <div class="flex justify-end mb-4">
          <select
            v-model="filtros.citas"
            class="px-3 py-2 text-sm transition border rounded-xl border-slate-200 dark:border-slate-700 bg-white dark:bg-slate-800 text-slate-700 dark:text-slate-100"
          >
            <option value="">
              Todos los meses
            </option>

            <option
              v-for="mes in obtenerMeses(stats.citasPorMes)"
              :key="mes"
              :value="mes"
            >
              {{ mes }}
            </option>
          </select>
        </div>

          <div class="h-80">
            <Bar
            :data="crearChartData(
              filtrarPorMes(stats.citasPorMes, filtros.citas),
              '#10B981'
            )"
            :options="chartOptions"
          />
          </div>
        </div>

      </div>

     <!-- COLUMNA DERECHA -->
<div class="grid gap-6 2xl:col-span-4">

  <!-- TOP DOCTORES -->
  <div class="p-5 transition-colors duration-300 bg-white shadow-sm dark:bg-slate-900 rounded-3xl border border-slate-200 dark:border-slate-700">

    <!-- HEADER -->
    <div class="flex items-start justify-between gap-4 mb-6">

      <div class="flex items-center gap-3">
        <div class="p-3 rounded-2xl bg-violet-100 text-violet-700">
          <UserGroupIcon class="w-6 h-6" />
        </div>

        <div>
          <h2 class="text-lg font-bold text-slate-800 dark:text-slate-100">
            Doctores más solicitados
          </h2>

          <p class="text-sm text-slate-500">
            Ranking de consultas agendadas
          </p>
        </div>
      </div>

      <select
        v-model="filtros.doctores"
        class="px-3 py-2 text-sm transition border rounded-xl border-slate-200 dark:border-slate-700 bg-white dark:bg-slate-800 text-slate-700 dark:text-slate-100 focus:outline-none focus:ring-2 focus:ring-slate-300"
      >
        <option value="">
          Todos los meses
        </option>

        <option
          v-for="mes in obtenerMeses(stats.topDoctores)"
          :key="mes"
          :value="mes"
        >
          {{ mes }}
        </option>
      </select>

    </div>

    <!-- LISTA -->
    <div
      v-if="filtrarPorMes(stats.topDoctores, filtros.doctores).length"
      class="space-y-4"
    >
      <div
        v-for="(doctor, index) in filtrarPorMes(stats.topDoctores, filtros.doctores).slice(0, 5)"
        :key="doctor.doctor"
        class="flex items-center justify-between p-3 transition border border-slate-100 rounded-2xl hover:bg-slate-50 dark:hover:bg-slate-800"
      >
        <div class="flex items-center gap-3">
          <div
            class="flex items-center justify-center w-9 h-9 font-bold rounded-xl bg-slate-100 text-slate-700"
          >
            {{ index + 1 }}
          </div>

          <div>
            <p class="font-semibold text-slate-800 dark:text-slate-100">
              {{ doctor.doctor }}
            </p>

            <p class="text-sm text-slate-500 dark:text-slate-400">
              Consultas registradas
            </p>
          </div>
        </div>

        <div
          class="px-3 py-1 text-sm font-bold rounded-xl bg-violet-100 text-violet-700"
        >
          {{ doctor.total }}
        </div>
      </div>
    </div>

    <p
      v-else
      class="text-slate-500 dark:text-slate-400"
    >
      Sin datos disponibles.
    </p>

  </div>

  <!-- TOP PACIENTES -->
  <div class="p-5 transition-colors duration-300 bg-white shadow-sm dark:bg-slate-900 rounded-3xl border border-slate-200 dark:border-slate-700">

    <!-- HEADER -->
    <div class="flex items-start justify-between gap-4 mb-6">

      <div class="flex items-center gap-3">
        <div class="p-3 text-rose-700 rounded-2xl bg-rose-100">
          <HeartIcon class="w-6 h-6" />
        </div>

        <div>
          <h2 class="text-lg font-bold text-slate-800 dark:text-slate-100">
            Pacientes frecuentes
          </h2>

          <p class="text-sm text-slate-500 dark:text-slate-400">
            Pacientes con más citas registradas
          </p>
        </div>
      </div>

      <select
        v-model="filtros.pacientes"
        class="px-3 py-2 text-sm transition border rounded-xl border-slate-200 dark:border-slate-700 bg-white dark:bg-slate-800 text-slate-700 dark:text-slate-100 focus:outline-none focus:ring-2 focus:ring-slate-300"
      >
        <option value="">
          Todos los meses
        </option>

        <option
          v-for="mes in obtenerMeses(stats.topPacientes)"
          :key="mes"
          :value="mes"
        >
          {{ mes }}
        </option>
      </select>

    </div>

    <!-- LISTA -->
    <div
      v-if="filtrarPorMes(stats.topPacientes, filtros.pacientes).length"
      class="space-y-4"
    >
      <div
        v-for="(paciente, index) in filtrarPorMes(stats.topPacientes, filtros.pacientes).slice(0, 5)"
        :key="paciente.paciente"
        class="flex items-center justify-between p-3 transition border border-slate-100 rounded-2xl hover:bg-slate-50 dark:hover:bg-slate-800"
      >
        <div class="flex items-center gap-3">
          <div
            class="flex items-center justify-center w-9 h-9 font-bold rounded-xl bg-slate-100 text-slate-700"
          >
            {{ index + 1 }}
          </div>

          <div>
            <p class="font-semibold text-slate-800 dark:text-slate-100">
              {{ paciente.paciente }}
            </p>

            <p class="text-sm text-slate-500 dark:text-slate-400">
              Visitas realizadas
            </p>
          </div>
        </div>

        <div
          class="px-3 py-1 text-sm font-bold rounded-xl bg-rose-100 text-rose-700"
        >
          {{ paciente.total }}
        </div>
      </div>
    </div>

    <p
      v-else
      class="text-slate-500 dark:text-slate-400"
    >
      Sin datos disponibles.
    </p>

  </div>

</div>

    </div>

  </div>
</template>
