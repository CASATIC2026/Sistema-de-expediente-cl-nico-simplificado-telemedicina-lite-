<script setup>

const { citas } = defineProps({
  citas: {
    type: Array,
    default: () => []
  }
})

// Diccionario de estilos para los Badges
const estilosEstado = {
  'Pendiente':
    'bg-amber-100 dark:bg-amber-500/10 text-amber-700 dark:text-amber-300 border-amber-200 dark:border-amber-500/20',

  'EnConsulta':
    'bg-blue-100 dark:bg-cyan-500/10 text-blue-700 dark:text-cyan-300 border-blue-200 dark:border-cyan-500/20',

  'Finalizada':
    'bg-emerald-100 dark:bg-emerald-500/10 text-emerald-700 dark:text-emerald-300 border-emerald-200 dark:border-emerald-500/20',

  'Cancelada':
    'bg-rose-100 dark:bg-rose-500/10 text-rose-700 dark:text-rose-300 border-rose-200 dark:border-rose-500/20'
}

const citaActual = (cita) => {
  const ahora  = new Date()
  const inicio = new Date(cita.start)   // ← camelCase
  const fin    = new Date(cita.end)     // ← camelCase
  return ahora >= inicio && ahora <= fin
}
</script>

<template>
<div
  class="bg-white dark:bg-slate-900
         border border-slate-200 dark:border-slate-800
         rounded-xl shadow-sm
         dark:shadow-[0_0_25px_rgba(255,255,255,0.03)]
         overflow-hidden
         hover:shadow-md
          dark:hover:shadow-[0_0_30px_rgba(14,165,233,0.08)]
         transition-colors duration-300"
>
 <div
  class="px-5 py-4
         border-b border-slate-200 dark:border-slate-800
         bg-slate-50 dark:bg-slate-950/60
         transition-colors duration-300"
>
    <h2 class="text-lg font-bold text-slate-700 dark:text-slate-100">Consultas agendadas para Hoy</h2>
  </div>

  <div class="overflow-x-auto">
    <table class="w-full text-left border-collapse">
      <thead>
        <tr
  class="text-slate-400 dark:text-slate-500
         text-xs uppercase tracking-wider
         border-b border-slate-200 dark:border-slate-800
         transition-colors duration-300"
>
          <th class="px-6 py-3 font-semibold">Hora</th>
          <th class="px-6 py-3 font-semibold">Paciente</th>
          <th class="px-6 py-3 font-semibold">Motivo</th>
          <th class="px-6 py-3 font-semibold text-center">Estado</th>
          <!-- Columna Acciones eliminada -->
        </tr>
      </thead>

      <tbody class="divide-y divide-slate-100 dark:divide-slate-800">
        <tr
          v-for="cita in citas"
          :key="cita.idCita"
          :class="[
            'transition-all duration-200',
            citaActual(cita)
  ? 'bg-teal-50 dark:bg-teal-500/10 border-l-4 border-l-teal-500'
  : 'hover:bg-slate-50 dark:hover:bg-slate-800/40 border-l-4 border-l-transparent'
          ]"
        >
          <!-- HORA -->
          <td class="px-6 py-4">
            <div class="text-sm font-bold text-slate-700 dark:text-slate-100">
              {{ new Date(cita.start).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }) }}
            </div>
          </td>

          <!-- PACIENTE -->
          <td class="px-6 py-4">
            <div class="text-sm font-semibold text-sky-600 dark:text-cyan-400">
              {{ cita.pacienteNombreCompleto }}
            </div>
            <div class="text-xs text-slate-400 dark:text-slate-500">ID: #{{ cita.idCita }}</div>
          </td>

          <!-- MOTIVO -->
          <td class="px-6 py-4 text-sm text-slate-500 dark:text-slate-300">
            {{ cita.titulo }}
            <span class="block text-xs italic text-slate-400 dark:text-slate-500">{{ cita.tipoConsulta }}</span>
          </td>

          <!-- ESTADO -->
          <td class="px-6 py-4 text-center">
            <span
              :class="[
                'inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-bold border',
                estilosEstado[cita.estado] || 'bg-gray-100 text-gray-600 border-gray-200'
              ]"
            >
              <span
                class="w-1.5 h-1.5 rounded-full mr-1.5"
                :class="cita.estado === 'EnConsulta' ? 'animate-pulse bg-blue-500' : ''"
              ></span>
              {{ cita.estado }}
            </span>
          </td>

        </tr>

        <!-- EMPTY STATE -->
        <tr v-if="citas.length === 0">
          <td colspan="4" class="py-12 text-center">
            <div class="flex flex-col items-center">
              <span class="text-slate-300 dark:text-slate-700 mb-2">
                <svg class="w-12 h-12" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
                </svg>
              </span>
              <p class="text-slate-400 dark:text-slate-500 font-medium">>No hay citas programadas para hoy</p>
            </div>
          </td>
        </tr>
      </tbody>
    </table>
  </div>
</div>
</template>
