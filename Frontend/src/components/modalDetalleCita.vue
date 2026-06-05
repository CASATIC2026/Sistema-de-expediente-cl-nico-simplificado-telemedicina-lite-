<script setup>
import {
  XMarkIcon,
  UserIcon,
  HeartIcon,
  ClipboardDocumentListIcon,
  CalendarDaysIcon,
  ClockIcon
} from '@heroicons/vue/24/outline'


defineProps({
  cita: {
    type: Object,
    default: null
  }
})

defineEmits(['cerrar'])
</script>

<template>
  <Transition
    enter-active-class="transition duration-300 ease-out"
    enter-from-class="opacity-0 scale-90 translate-y-4"
    enter-to-class="opacity-100 scale-100 translate-y-0"
    leave-active-class="transition duration-200 ease-in"
    leave-from-class="opacity-100 scale-100 translate-y-0"
    leave-to-class="opacity-0 scale-90 translate-y-4"
  >
    <div
      v-if="cita"
      class="fixed inset-0 z-1200 flex items-center justify-center px-4 py-6 bg-slate-900/70 backdrop-blur-md"
      @click.self="$emit('cerrar')"
    >
      <!-- Modal -->
      <div
  class="relative w-full max-w-md overflow-hidden rounded-3xl shadow-2xl
         bg-white/5 dark:bg-slate-900/80
         backdrop-blur-2xl
         border border-white/20 dark:border-slate-700"
>

        <!-- Header -->
        <div
          class="relative px-6 py-5 overflow-hidden
                 bg-linear-to-r from-slate-700 via-sky-800 to-slate-800"
        >
          <!-- Glow decor -->
          <div
            class="absolute w-40 h-40 rounded-full -top-10 -right-10 bg-white/10 blur-2xl"
          ></div>

          <div class="relative flex items-center justify-between">
            <div>
              <h3 class="text-xl font-bold tracking-wide text-white">
                Detalle de cita
              </h3>
              <p class="mt-1 text-sm text-cyan-100">
                Información  la consulta médica
              </p>
            </div>

            <button
              @click="$emit('cerrar')"
              class="flex items-center justify-center w-10 h-10 transition rounded-full
                     bg-white/10 hover:bg-white/20 text-white"
            >
             <XMarkIcon class="w-5 h-5" />
            </button>
          </div>
        </div>

        <!-- Content -->
        <div class="px-6 py-5 space-y-4 bg-white/5">

          <!-- Paciente -->
          <div
            class="flex items-center justify-between p-4 rounded-2xl
                   bg-slate-400 dark:bg-slate-800/70"
          >
            <div>
              <p class="text-xs font-medium tracking-wide text-sky-800 dark:text-sky-300 uppercase">
                Paciente
              </p>
              <p class="text-sm font-semibold text-slate-900 dark:text-slate-100">
                {{ cita.paciente }}
              </p>
            </div>

            <div
              class="flex items-center justify-center w-11 h-11 rounded-xl
                     bg-cyan-100 dark:bg-cyan-900/40"
            >
              <UserIcon class="w-6 h-6 text-cyan-600 dark:text-cyan-400" />
            </div>
          </div>

          <!-- Doctor -->
          <div
            class="flex items-center justify-between p-4 rounded-2xl
                   bg-slate-400 dark:bg-slate-800/70"
          >
            <div>
              <p class="text-xs font-medium tracking-wide text-sky-800 dark:text-sky-300 uppercase">
                Doctor
              </p>
              <p class="text-sm font-semibold text-slate-900 dark:text-slate-100">
                {{ cita.doctor }}
              </p>
            </div>

            <div
              class="flex items-center justify-center w-11 h-11 rounded-xl
                     bg-emerald-100 dark:bg-emerald-900/40"
            >
              <HeartIcon class="w-6 h-6 text-emerald-600 dark:text-emerald-400" />
            </div>
          </div>

          <!-- Estado -->
          <div
            class="flex items-center justify-between p-4 rounded-2xl
                   bg-slate-400 dark:bg-slate-800/70"
          >
            <div>
              <p class="text-xs font-medium tracking-wide text-sky-800 dark:text-sky-300 uppercase">
                Estado
              </p>

              <span
                :class="{
                  'bg-yellow-100 text-yellow-700 dark:bg-yellow-900/30 dark:text-yellow-300':
                    cita.estado === 'Pendiente',

                  'bg-green-100 text-green-700 dark:bg-green-900/30 dark:text-green-300':
                    cita.estado === 'Confirmada',

                  'bg-blue-100 text-blue-700 dark:bg-blue-900/30 dark:text-blue-300':
                    cita.estado === 'EnConsulta',

                  'bg-emerald-100 text-emerald-700 dark:bg-emerald-900/30 dark:text-emerald-300':
                    cita.estado === 'Finalizada',

                  'bg-red-100 text-red-700 dark:bg-red-900/30 dark:text-red-300':
                    cita.estado === 'Cancelada',

                  'bg-slate-200 text-slate-600 dark:bg-slate-700 dark:text-slate-300':
                    cita.estado === 'NoAsistida',
                }"
                class="inline-flex px-3 py-1 mt-1 text-xs font-semibold rounded-full"
              >
                {{ cita.estado }}
              </span>
            </div>

            <div
              class="flex items-center justify-center w-11 h-11 rounded-xl
                     bg-sky-100 dark:bg-sky-900/40"
            >
              <ClipboardDocumentListIcon class="w-6 h-6 text-sky-600 dark:text-sky-400"/>
            </div>
          </div>

          <!-- Grid -->
          <div class="grid grid-cols-1 gap-4 sm:grid-cols-2">

           <div
            class="p-4 rounded-2xl
                  bg-slate-400 dark:bg-slate-800/70"
          >
            <div class="flex items-center gap-2 mb-2">
              <CalendarDaysIcon class="w-5 h-5 text-teal-900 dark:text-teal-200" />
              <p class="text-xs font-medium tracking-wide text-sky-900 dark:text-sky-300 uppercase">
                Fecha
              </p>
            </div>

            <p class="text-sm font-semibold text-slate-900 dark:text-white">
              {{ cita.fecha }}
            </p>
          </div>

           <div
              class="p-4 rounded-2xl
                    bg-slate-400 dark:bg-slate-800/70"
            >
              <div class="flex items-center gap-2 mb-2">
                <ClockIcon class="w-5 h-5 text-teal-900 dark:text-teal-200" />
                <p class="text-xs font-medium tracking-wide text-sky-900 dark:text-sky-300 uppercase">
                  Hora
                </p>
              </div>

              <p class="text-sm font-semibold text-slate-900 dark:text-white">
                {{ cita.hora }}
              </p>
            </div>
          </div>

        </div>

        <!-- Footer -->
        <div
          class="flex items-center justify-end gap-3 px-6 py-4 border-t
                 border-slate-200 dark:border-slate-700
               bg-linear-to-r from-slate-700 via-sky-800 to-slate-800"
        >
        </div>

      </div>
    </div>
  </Transition>
</template>
