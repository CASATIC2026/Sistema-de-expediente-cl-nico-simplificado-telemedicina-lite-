<script setup>
import { ref, onMounted } from 'vue'
import { getHorarioDoctor, actualizarHorarioDoctor } from '@/services/api'

const props = defineProps({
  doctor: { type: Object, required: true }
})
const emit = defineEmits(['cerrar', 'actualizar'])

const guardando = ref(false)
const cargando  = ref(true)
const errorMsg  = ref('')

// Estructura base — se sobreescribe con lo que venga del backend
const disponibilidad = ref([
  { diaSemana: 1, nombre: 'Lunes',     activo: false, inicio: '08:00', fin: '16:00' },
  { diaSemana: 2, nombre: 'Martes',    activo: false, inicio: '08:00', fin: '16:00' },
  { diaSemana: 3, nombre: 'Miércoles', activo: false, inicio: '08:00', fin: '16:00' },
  { diaSemana: 4, nombre: 'Jueves',    activo: false, inicio: '08:00', fin: '16:00' },
  { diaSemana: 5, nombre: 'Viernes',   activo: false, inicio: '08:00', fin: '16:00' },
  { diaSemana: 6, nombre: 'Sábado',    activo: false, inicio: '09:00', fin: '12:00' },
  { diaSemana: 0, nombre: 'Domingo',   activo: false, inicio: '00:00', fin: '00:00' },
])

onMounted(async () => {
  try {
    const data = await getHorarioDoctor(props.doctor.id)
    // Sobrescribir solo los días que el backend devuelva
    data.forEach(h => {
      const dia = disponibilidad.value.find(d => d.diaSemana === h.diaSemana)
      if (dia) {
        dia.activo = h.activo
        dia.inicio = h.horaInicio.substring(0, 5) // "08:00:00" → "08:00"
        dia.fin    = h.horaFin.substring(0, 5)
      }
    })
  } catch (e) {
    console.error('Error cargando horario:', e)
  } finally {
    cargando.value = false
  }
})
const guardarCambios = async () => {
  guardando.value = true
  errorMsg.value  = ''
  try {
    const payload = disponibilidad.value.map(d => ({
      DiaSemana:  d.diaSemana,
      HoraInicio: d.inicio,
      HoraFin:    d.fin,
      Activo:     d.activo
    }))
    await actualizarHorarioDoctor(props.doctor.id, payload)
    emit('actualizar', disponibilidad.value)
    emit('cerrar')
  } catch (e) {
    errorMsg.value = e.response?.data?.message || 'Error al guardar el horario.'
  } finally {
    guardando.value = false
  }
}
</script>

<template>
  <div class="fixed inset-0 z-[1000] flex items-start md:items-center justify-center bg-slate-950/80 backdrop-blur-sm p-2 md:p-4 pt-6 md:pt-4">
    
    <div class="bg-[#0b1a30] text-white rounded-[1.5rem] md:rounded-[2.5rem] shadow-2xl w-full max-w-2xl overflow-hidden border border-slate-800/60 flex flex-col max-h-[90vh] md:max-h-[95vh]">

      <div class="px-5 py-4 md:px-8 md:py-6 bg-linear-to-r from-[#0f2040] to-[#10284f] border-b border-slate-800/50 flex justify-between items-center shrink-0relative">
        <div class="min-w-0"> 
          <h2 class="text-base md:text-xl font-black text-cyan-400 uppercase tracking-tight truncate">Horario del Doctor</h2>
          <p class="text-[9px] md:text-xs text-white font-bold uppercase truncate opacity-90 mt-0.5">
            {{ props.doctor?.nombre }} {{ props.doctor?.apellido }}
          </p>
        </div>
        
        <button 
          @click="emit('cerrar')" 
          class="text-red-400 bg-red-500/10 hover:bg-red-500/20 border border-red-500/20 transition-all p-2 rounded-full shrink-0 cursor-pointer"
          title="Cerrar"
        >
          <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M6 18L18 6M6 6l12 12"/>
          </svg>
        </button>
      </div>

      <div v-if="cargando" class="p-12 text-center text-cyan-400 font-bold animate-pulse uppercase tracking-wider text-xs md:text-sm">
        Cargando horario...
      </div>

      <div v-else class="p-3 md:p-6 overflow-y-auto overscroll-contain custom-scrollbar bg-linear-to-r from-[#0f2040] to-[#10284f]">
        <div class="space-y-3">

          <div class="hidden md:grid grid-cols-4 gap-4 px-6 py-1 text-[12px] font-black text-white uppercase tracking-[0.2em]">
            <div class="col-span-1">Estado / Día</div>
            <div class="col-span-3 text-center">Rango de Horas (Inicio — Fin)</div>
          </div>

          <div
            v-for="dia in disponibilidad"
            :key="dia.diaSemana"
            :class="dia.activo
              ? 'bg-[#071120]/60 border-slate-800 shadow-xs'
              : 'bg-[#071120]/20 border-slate-900/40 opacity-40'"
            class="flex flex-col md:grid md:grid-cols-4 items-center gap-3 md:gap-4 p-4 md:px-6 md:py-3 rounded-2xl border transition-all"
          >
            <div class="flex items-center justify-between w-full md:w-auto md:col-span-1 border-b border-slate-800/40 md:border-none pb-2 md:pb-0">
              <div class="flex items-center gap-3">
                <label class="relative inline-flex items-center cursor-pointer scale-90 md:scale-100">
                  <input type="checkbox" v-model="dia.activo" class="sr-only peer">
                  <div class="w-11 h-6 bg-slate-800 peer-focus:ring-2 peer-focus:ring-cyan-500/10 rounded-full peer peer-checked:after:translate-x-full peer-checked:after:border-transparent after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-slate-400 peer-checked:after:bg-[#0b1a30] after:rounded-full after:h-5 after:w-5 after:transition-all peer-checked:bg-cyan-500"></div>
                </label>
                <span class="font-bold text-white text-sm md:text-base">{{ dia.nombre }}</span>
              </div>
              <span v-if="!dia.activo" class="md:hidden text-[8px] font-black uppercase text-slate-400 bg-[#071120] px-2 py-1 rounded-md border border-slate-800">Inactivo</span>
            </div>

            <div class="w-full md:col-span-3 flex items-center justify-between gap-3">
              <div class="flex-1 group">
                <label class="md:hidden text-[8px] font-black text-cyan-400 uppercase mb-1 block ml-1">Inicio</label>
                <div class="relative">
                  <input
                    type="time"
                    v-model="dia.inicio"
                    :disabled="!dia.activo"
                    class="w-full p-2 md:p-2.5 rounded-xl border border-slate-800 bg-[#071120] focus:border-cyan-500 outline-none text-xs md:text-sm font-bold text-white disabled:bg-[#071120]/30 disabled:text-slate-600 disabled:border-transparent transition-all text-center focus:ring-1 focus:ring-cyan-500/20"
                  />
                </div>
              </div>

              <span class="text-slate-400 font-bold text-xs pt-4 md:pt-0">a</span>

              <div class="flex-1 group">
                <label class="md:hidden text-[8px] font-black text-cyan-400 uppercase mb-1 block ml-1">Fin</label>
                <div class="relative">
                  <input
                    type="time"
                    v-model="dia.fin"
                    :disabled="!dia.activo"
                    class="w-full p-2 md:p-2.5 rounded-xl border border-slate-800 bg-[#071120] focus:border-cyan-500 outline-none text-xs md:text-sm font-bold text-white disabled:bg-[#071120]/30 disabled:text-slate-600 disabled:border-transparent transition-all text-center focus:ring-1 focus:ring-cyan-500/20"
                  />
                </div>
              </div>
            </div>
          </div>
        </div>

        <p v-if="errorMsg" class="mt-4 text-[10px] md:text-sm font-bold text-red-400 text-center bg-red-500/10 p-3 rounded-xl border border-red-500/20">
          ⚠️ {{ errorMsg }}
        </p>
      </div>

      <div class="p-4 md:p-6 bg-linear-to-r from-[#0f2040] to-[#10284f] border-t border-slate-800/60 flex flex-col-reverse md:flex-row gap-2 md:gap-3 shrink-0">
        <button
          @click="emit('cerrar')"
          class="w-full md:flex-1 py-3 md:py-3.5 bg-transparent border border-slate-700 text-slate-300 rounded-xl md:rounded-2xl text-[10px] md:text-xs font-black uppercase tracking-widest hover:bg-red-400 hover:text-white transition-all cursor-pointer"
        >
          Cancelar
        </button>
        <button
          @click="guardarCambios"
          :disabled="guardando"
          class="w-full md:flex-[2] py-3 md:py-3.5 bg-cyan-500 disabled:opacity-40 text-[#0b1a30] rounded-xl md:rounded-2xl text-[10px] md:text-xs font-black uppercase tracking-widest hover:bg-cyan-400 transition-all flex justify-center items-center shadow-md disabled:pointer-events-none cursor-pointer"
        >
          <span v-if="!guardando">Actualizar Horario</span>
          <span v-else class="flex items-center gap-2">
            <svg class="animate-spin h-4 w-4 text-[#0b1a30]" viewBox="0 0 24 24">
              <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4" fill="none"></circle>
              <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
            </svg>
            Cargando...
          </span>
        </button>
      </div>

    </div>
  </div>
</template>

<style scoped>
.custom-scrollbar::-webkit-scrollbar {
  width: 6px;
}
.custom-scrollbar::-webkit-scrollbar-track {
  background: transparent;
}
.custom-scrollbar::-webkit-scrollbar-thumb {
  background: #cad8f0;
  border-radius: 10px;
}
input[type="time"]::-webkit-calendar-picker-indicator {
  filter: invert(100%) sepia(100%) grayscale(100%) hue-rotate(160deg) brightness(150%); cursor: pointer;
}
</style>