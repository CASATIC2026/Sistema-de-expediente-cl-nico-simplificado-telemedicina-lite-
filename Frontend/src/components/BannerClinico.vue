<script setup>
import {
  ClockIcon,
  PhoneIcon,
  BuildingOffice2Icon
} from '@heroicons/vue/24/outline'

defineProps({
  clinicName: String,
  slogan: String,
  horario: String,
  telefono: String,
  // Nueva prop para controlar el diseño sin romper el del admin
  compact: {
    type: Boolean,
    default: false
  }
})
</script>

<template>
  <section
    :class="[
      'relative overflow-hidden transition-all duration-300',

      // Modo Admin / Original
      {
        'bg-gradient-to-r from-cyan-700 via-sky-800 to-blue-900 border-y border-cyan-400/20 sm:border sm:border-cyan-400/30 shadow-none sm:shadow-xl px-4 py-3 sm:px-6 sm:py-5 lg:px-8 lg:py-6 rounded-none sm:rounded-3xl':
          !compact
      },

      // Modo Doctor / Compacto
      {
        'bg-white dark:bg-[#071120] border border-slate-200/80 dark:border-slate-800 shadow-sm px-4 py-3 sm:px-6 sm:py-4 lg:px-6 lg:py-4 rounded-2xl mb-2':
          compact
      }
    ]"
  >
    <!-- Glow decorativo solo admin -->
    <div
      v-if="!compact"
      class="
        absolute -right-10 -top-10
        h-32 w-32
        rounded-full
        bg-cyan-300/10
        blur-3xl
      "
    ></div>

    <div
      class="
        relative z-10
        flex flex-col
        gap-2
        lg:flex-row
        lg:items-center
        lg:justify-between
      "
    >
      <!-- Info izquierda -->
      <div class="flex flex-col gap-0.5">

        <!-- Label -->
        <div
          :class="[
            'flex items-center gap-1.5 mb-0.5',
            compact
              ? 'text-slate-400 dark:text-slate-500'
              : 'text-cyan-200'
          ]"
        >
          <BuildingOffice2Icon class="h-4 w-4 shrink-0" />

          <p
            class="
              text-[10px]
              uppercase
              tracking-[0.3em]
              font-semibold
            "
          >
            {{ compact ? 'Información de la Clínica' : 'Nuestros datos' }}
          </p>
        </div>

        <!-- Nombre clínica -->
        <h1
          :class="[
            'leading-tight font-black sm:text-2xl',
            compact
              ? 'text-slate-800 dark:text-slate-100 text-lg'
              : 'text-white text-xl sm:text-3xl'
          ]"
        >
          {{ clinicName }}
        </h1>

        <!-- Datos -->
        <div
          :class="[
            'flex flex-col gap-1 sm:flex-row sm:flex-wrap sm:items-center sm:gap-5 text-xs',
            compact
              ? 'text-slate-500 dark:text-slate-400'
              : 'text-cyan-100 sm:text-sm'
          ]"
        >
          <!-- Horario -->
          <div class="flex items-center gap-1.5">
            <ClockIcon
              :class="[
                'h-4 w-4 shrink-0',
                compact
                  ? 'text-slate-400 dark:text-slate-500'
                  : 'text-cyan-300'
              ]"
            />

            <span>{{ horario }}</span>
          </div>

          <!-- Teléfono -->
          <div class="flex items-center gap-1.5">
            <PhoneIcon
              :class="[
                'h-4 w-4 shrink-0',
                compact
                  ? 'text-slate-400 dark:text-slate-500'
                  : 'text-cyan-300'
              ]"
            />

            <span>{{ telefono }}</span>
          </div>
        </div>
      </div>

      <!-- Slogan -->
      <div class="max-w-md">
        <p
          :class="[
            'text-xs italic leading-relaxed sm:text-right',
            compact
              ? 'text-slate-400 dark:text-slate-500 lg:text-sm'
              : 'text-cyan-100/90 sm:text-sm lg:text-lg'
          ]"
        >
          "{{ slogan }}"
        </p>
      </div>
    </div>
  </section>
</template>
