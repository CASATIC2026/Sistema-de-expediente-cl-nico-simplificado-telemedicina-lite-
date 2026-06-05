<script setup>
import { ref } from 'vue'

const emit = defineEmits(['cerrar', 'actualizar'])

// Inicializamos los 7 días de la semana
const disponibilidad = ref([
  { nombre: 'Lunes', activo: true, inicio: '08:00', fin: '16:00' },
  { nombre: 'Martes', activo: true, inicio: '08:00', fin: '16:00' },
  { nombre: 'Miércoles', activo: true, inicio: '08:00', fin: '16:00' },
  { nombre: 'Jueves', activo: true, inicio: '08:00', fin: '16:00' },
  { nombre: 'Viernes', activo: true, inicio: '08:00', fin: '16:00' },
  { nombre: 'Sábado', activo: false, inicio: '09:00', fin: '12:00' },
  { nombre: 'Domingo', activo: false, inicio: '00:00', fin: '00:00' },
])

const guardarCambios = () => {
  // Filtramos solo los días que el doctor activó para enviar al backend después
  const datosAEnviar = disponibilidad.value.filter(d => d.activo)
  console.log("Enviando configuración:", datosAEnviar)
  emit('actualizar', disponibilidad.value)
}
</script>

<template>
  <div class="fixed inset-0 z-[1000] flex items-center justify-center bg-slate-900/60 backdrop-blur-sm p-4">
    <div class="bg-white rounded-[2.5rem] shadow-2xl w-full max-w-2xl overflow-hidden border border-slate-100">
      
      <div class="px-8 py-6 bg-slate-50 border-b border-slate-100 flex justify-between items-center">
        <div>
          <h2 class="text-xl font-black text-slate-800 uppercase tracking-tight">Horario del Doctor</h2>
          <p class="text-xs text-slate-500 font-bold uppercase">Configuración de disponibilidad semanal</p>
        </div>
        <button @click="emit('cerrar')" class="text-slate-400 hover:text-red-500 transition-colors">
          <svg xmlns="http://www.w3.org/2000/svg" class="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
          </svg>
        </button>
      </div>

      <div class="p-6 max-h-[70vh] overflow-y-auto">
        <div class="space-y-3">
          <div class="hidden md:grid grid-cols-4 gap-4 px-6 py-2 text-[10px] font-black text-slate-400 uppercase tracking-[0.2em]">
            <div class="col-span-1">Estado / Día</div>
            <div class="col-span-3 text-center">Rango de Horas (Inicio - Fin)</div>
          </div>

          <div v-for="dia in disponibilidad" :key="dia.nombre" 
               :class="dia.activo ? 'bg-white border-slate-200' : 'bg-slate-50 border-transparent opacity-60'"
               class="grid grid-cols-1 md:grid-cols-4 items-center gap-4 p-4 md:px-6 md:py-3 rounded-2xl border-2 transition-all">
            
            <div class="flex items-center gap-3">
              <label class="relative inline-flex items-center cursor-pointer">
                <input type="checkbox" v-model="dia.activo" class="sr-only peer">
                <div class="w-11 h-6 bg-slate-200 peer-focus:outline-none peer-focus:ring-4 peer-focus:ring-blue-100 rounded-full peer peer-checked:after:translate-x-full peer-checked:after:border-white after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:border-gray-300 after:border after:rounded-full after:h-5 after:w-5 after:transition-all peer-checked:bg-blue-600"></div>
              </label>
              <span class="font-bold text-slate-700">{{ dia.nombre }}</span>
            </div>

            <div class="col-span-3 flex items-center justify-center gap-2">
              <input 
                type="time" 
                v-model="dia.inicio" 
                :disabled="!dia.activo"
                class="w-full md:w-32 p-2 rounded-xl border-2 border-slate-100 focus:border-blue-500 outline-none text-sm font-semibold text-slate-600 disabled:bg-slate-100 disabled:cursor-not-allowed transition-all"
              />
              <span class="text-slate-400 font-bold">a</span>
              <input 
                type="time" 
                v-model="dia.fin" 
                :disabled="!dia.activo"
                class="w-full md:w-32 p-2 rounded-xl border-2 border-slate-100 focus:border-blue-500 outline-none text-sm font-semibold text-slate-600 disabled:bg-slate-100 disabled:cursor-not-allowed transition-all"
              />
            </div>
          </div>
        </div>
      </div>

      <div class="p-8 bg-slate-50 border-t border-slate-100 flex flex-col md:flex-row gap-3">
        <button 
          @click="emit('cerrar')" 
          class="flex-1 py-4 bg-white border-2 border-slate-200 text-slate-500 rounded-2xl text-xs font-black uppercase tracking-widest hover:bg-slate-100 transition-all"
        >
          Cancelar
        </button>
        <button 
          @click="guardarCambios" 
          class="flex-[2] py-4 bg-blue-600 text-white rounded-2xl text-xs font-black uppercase tracking-widest hover:bg-blue-700 hover:shadow-lg hover:shadow-blue-200 transition-all"
        >
          Actualizar Horario
        </button>
      </div>
    </div>
  </div>
</template>