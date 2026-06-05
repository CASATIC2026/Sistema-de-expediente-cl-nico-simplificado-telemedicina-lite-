<script setup>
import { ref, onMounted } from 'vue'
import { getResumenPaciente } from '@/services/api'
import {
  XMarkIcon,
  CalendarDaysIcon,
  CheckCircleIcon,
  XCircleIcon,
  ExclamationTriangleIcon
} from '@heroicons/vue/24/outline'

const props = defineProps({
  paciente: { type: Object, required: true }
})

const emit    = defineEmits(['cerrar'])
const resumen = ref(null)
const cargando = ref(true)

onMounted(async () => {
  try {
    resumen.value = await getResumenPaciente(props.paciente.id)
  } catch {
  } finally {
    cargando.value = false
  }
})
</script>

<template>
  <div
    class="fixed inset-0 z-[9999] flex items-center justify-center bg-slate-950/80 backdrop-blur-sm p-4"
    @click.self="emit('cerrar')"
  >
      <div class="
        bg-[#f8f6f2] dark:bg-slate-900
        text-slate-800 dark:text-white
        rounded-[2rem] shadow-2xl w-full max-w-md
        border border-[#e8e2d8] dark:border-slate-800/60
        overflow-hidden
      ">

      <!-- HEADER -->
      <div class="px-6 py-5 bg-gradient-to-r from-slate-800 to-slate-900 dark:from-[#0f2040] dark:to-[#10284f] border-b border-slate-200 dark:border-slate-800/50 flex justify-between items-center">
        <div>
          <h2 class="text-base font-black text-sky-300 uppercase tracking-tight">
            Resumen del Paciente
          </h2>
          <p class="text-xs text-slate-300 font-bold mt-0.5">
            {{ props.paciente.nombre }} {{ props.paciente.apellido }}
          </p>
        </div>
        <button
          @click="emit('cerrar')"
          class="text-red-400 bg-red-500/10 hover:bg-red-900 border border-red-500/20 p-2 rounded-full transition-all cursor-pointer"
        >
          <XMarkIcon class="h-4 w-4" />
        </button>
      </div>

      <!-- CARGANDO -->
      <div v-if="cargando" class="p-12 text-center text-cyan-400 font-bold animate-pulse text-xs uppercase tracking-widest">
        Cargando resumen...
      </div>

      <!-- CONTENIDO -->
      <div v-else-if="resumen" class="p-6 space-y-4 bg-[#f8f6f2] dark:bg-slate-900">

        <!-- ÚLTIMA CONSULTA -->
      <div class="bg-white dark:bg-[#071120] rounded-2xl p-4 border border-slate-200 dark:border-slate-800/60">
        <p class="text-[10px] font-black text-slate-700 dark:text-slate-400 uppercase tracking-widest mb-3 flex items-center gap-2">
          <CalendarDaysIcon class="w-3.5 h-3.5" />
          Última Consulta Finalizada
        </p>

        <div v-if="resumen.fechaUltimaConsulta" class="space-y-1">
          <p class="text-teal-700/75 dark:text-blue-300 font-black text-lg">
            {{ resumen.fechaUltimaConsulta }}
          </p>

          <p class="text-slate-500 dark:text-slate-300 text-xs font-bold">
            Atendido por:

            <span class="text-blue-600 dark:text-white">
              {{ resumen.doctorUltimaConsulta }}
            </span>
          </p>
        </div>

        <p v-else class="text-slate-500 dark:text-slate-500 text-sm font-bold italic">
          Sin consultas finalizadas
        </p>
      </div>

        <!-- ESTADÍSTICAS -->
        <div class="grid grid-cols-2 gap-3">

          <div class="bg-white dark:bg-[#071120]
            rounded-2xl p-4 border border-slate-200 dark:border-slate-800/60
            text-center transition-all duration-300
            hover:-translate-y-0.5
            hover:shadow-[0_0_25px_rgba(255,255,255,0.08)]
            hover:border-slate-400/40">
            <p class="text-[9px] font-black text-slate-400 uppercase tracking-widest mb-1">
              Total Citas
            </p>
            <p class="text-3xl font-black text-slate-800 dark:text-white">
              {{ resumen.total }}
            </p>
          </div>

          <div class="bg-white dark:bg-[#071120]
            rounded-2xl p-4 border border-emerald-500/20
            text-center transition-all duration-300
            hover:-translate-y-0.5
            hover:shadow-[0_0_25px_rgba(16,185,129,0.18)]
            hover:border-emerald-500/40">
            <p class="text-[9px] font-black text-emerald-700 uppercase tracking-widest mb-1 flex items-center justify-center gap-1">
              <CheckCircleIcon class="w-3 h-3" />
              Finalizadas
            </p>
            <p class="text-3xl font-black text-emerald-400">
              {{ resumen.finalizadas }}
            </p>
          </div>

          <div class="bg-white dark:bg-[#071120]
            rounded-2xl p-4 border border-red-500/20
            text-center transition-all duration-300
            hover:-translate-y-0.5
            hover:shadow-[0_0_25px_rgba(239,68,68,0.18)]
            hover:border-red-500/40">
            <p class="text-[9px] font-black text-red-400 uppercase tracking-widest mb-1 flex items-center justify-center gap-1">
              <XCircleIcon class="w-3 h-3" />
              Canceladas
            </p>
            <p class="text-3xl font-black text-red-400">
              {{ resumen.canceladas }}
            </p>
          </div>

          <div class="bg-white dark:bg-[#071120]
            rounded-2xl p-4 border border-orange-500/20
            text-center transition-all duration-300
            hover:-translate-y-0.5
            hover:shadow-[0_0_25px_rgba(249,115,22,0.18)]
            hover:border-orange-500/40">
            <p class="text-[9px] font-black text-orange-400 uppercase tracking-widest mb-1 flex items-center justify-center gap-1">
              <ExclamationTriangleIcon class="w-3 h-3" />
              No Asistidas
            </p>
            <p class="text-3xl font-black text-orange-400">
              {{ resumen.noAsistidas }}
            </p>
          </div>

        </div>

      </div>

      <!-- FOOTER -->
      <div class="px-6 py-4 border-t border-[#e6dfd5] dark:border-slate-800/60 dark:bg-gradient-to-r dark:from-[#0f2040] dark:to-[#10284f]">
        <button
          @click="emit('cerrar')"
          class="w-full py-3 bg-slate-800 dark:bg-blue-500
            hover:bg-sky-600 dark:hover:bg-slate-400
            text-white dark:text-[#0b1a30]
            shadow-lg hover:shadow-sky-500/10 font-black text-xs uppercase tracking-widest rounded-xl
            transition-all cursor-pointer"
          >
          Cerrar
        </button>
      </div>

    </div>
  </div>
</template>
