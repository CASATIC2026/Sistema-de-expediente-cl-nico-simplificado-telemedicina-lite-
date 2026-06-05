<script setup>
defineProps({
  title: String,
  value: Number,
  color: String,
  icon: [Object, Function]
})
</script>

<template>
  <div
    class="group relative overflow-hidden
           rounded-3xl border
           border-slate-200 dark:border-slate-800
           bg-white dark:bg-slate-900/80
           p-5 shadow-sm
           hover:-translate-y-1
           hover:shadow-xl
           dark:hover:shadow-teal-500/30
           transition-all duration-300"
  >

    <!-- Glow decorativo -->
    <div
      class="absolute -top-10 -right-10 w-24 h-24 rounded-full blur-3xl opacity-10 group-hover:opacity-20 transition"
      :class="color.replace('text-', 'bg-')"
    ></div>

    <div class="relative flex items-start justify-between">

      <!-- TEXTO -->
      <div>
        <p class="text-xs font-black uppercase tracking-widest text-slate-400 dark:text-slate-500 mb-2">
          {{ title }}
        </p>

        <h2
          class="text-4xl font-black"
          :class="color"
        >
          {{ value }}
        </h2>
      </div>

      <!-- ICONO -->
      <div
        class="w-12 h-12 rounded-2xl
               flex items-center justify-center
               bg-slate-100 dark:bg-slate-800/80
               border border-slate-200 dark:border-slate-700"
      >
        <component
          :is="icon"
          class="w-6 h-6"
          :class="color"
        />
      </div>

    </div>
  </div>
</template>
