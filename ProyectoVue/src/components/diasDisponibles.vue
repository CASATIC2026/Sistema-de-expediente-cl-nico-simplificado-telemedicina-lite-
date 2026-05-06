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
  <div class="fixed inset-0 z-[1000] flex items-start md:items-center justify-center bg-slate-900/60 backdrop-blur-sm p-2 md:p-4 pt-6 md:pt-4">
    
    <div class="bg-white rounded-[1.5rem] md:rounded-[2.5rem] shadow-2xl w-full max-w-2xl overflow-hidden border border-slate-100 flex flex-col max-h-[90vh] md:max-h-[95vh]">

      <div class="px-5 py-4 md:px-8 md:py-6 bg-slate-50 border-b border-slate-100 flex justify-between items-center shrink-0">
        <div class="min-w-0"> <h2 class="text-base md:text-xl font-black text-slate-800 uppercase tracking-tight truncate">Horario del Doctor</h2>
          <p class="text-[9px] md:text-xs text-slate-500 font-bold uppercase truncate">
            {{ props.doctor?.nombre }} {{ props.doctor?.apellido }}
          </p>
        </div>
        <button @click="emit('cerrar')" class="text-slate-400 hover:text-red-500 transition-colors p-1 shrink-0">
          <svg xmlns="http://www.w3.org/2000/svg" class="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"/>
          </svg>
        </button>
      </div>

      <div v-if="cargando" class="p-12 text-center text-slate-400 font-bold animate-pulse">
        Cargando horario...
      </div>

      <div v-else class="p-3 md:p-6 overflow-y-auto overscroll-contain custom-scrollbar">
        <div class="space-y-3">

          <div class="hidden md:grid grid-cols-4 gap-4 px-6 py-2 text-[10px] font-black text-slate-400 uppercase tracking-[0.2em]">
            <div class="col-span-1">Estado / Día</div>
            <div class="col-span-3 text-center">Rango de Horas (Inicio — Fin)</div>
          </div>

          <div
            v-for="dia in disponibilidad"
            :key="dia.diaSemana"
            :class="dia.activo
              ? 'bg-white border-slate-200 shadow-sm'
              : 'bg-slate-50 border-transparent opacity-60'"
            class="flex flex-col md:grid md:grid-cols-4 items-center gap-3 md:gap-4 p-4 md:px-6 md:py-3 rounded-2xl border-2 transition-all"
          >
            <div class="flex items-center justify-between w-full md:w-auto md:col-span-1 border-b border-slate-100 md:border-none pb-2 md:pb-0">
              <div class="flex items-center gap-3">
                <label class="relative inline-flex items-center cursor-pointer scale-90 md:scale-100">
                  <input type="checkbox" v-model="dia.activo" class="sr-only peer">
                  <div class="w-11 h-6 bg-slate-200 peer-focus:ring-4 peer-focus:ring-blue-100 rounded-full peer peer-checked:after:translate-x-full peer-checked:after:border-white after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:border after:rounded-full after:h-5 after:w-5 after:transition-all peer-checked:bg-blue-600"></div>
                </label>
                <span class="font-bold text-slate-700 text-sm md:text-base">{{ dia.nombre }}</span>
              </div>
              <span v-if="!dia.activo" class="md:hidden text-[8px] font-black uppercase text-slate-400 bg-slate-100 px-2 py-1 rounded-md">Inactivo</span>
            </div>

            <div class="w-full md:col-span-3 flex items-center justify-between gap-3">
              <div class="flex-1 group">
                <label class="md:hidden text-[8px] font-black text-slate-400 uppercase mb-1 block ml-1">Inicio</label>
                <div class="relative">
                   <input
                    type="time"
                    v-model="dia.inicio"
                    :disabled="!dia.activo"
                    class="w-full p-2 md:p-2.5 rounded-xl border-2 border-slate-100 focus:border-blue-500 outline-none text-xs md:text-sm font-bold text-slate-600 disabled:bg-slate-50 disabled:text-slate-300 transition-all text-center"
                  />
                </div>
              </div>

              <span class="text-slate-400 font-bold text-xs pt-4 md:pt-0">a</span>

              <div class="flex-1 group">
                <label class="md:hidden text-[8px] font-black text-slate-400 uppercase mb-1 block ml-1">Fin</label>
                <div class="relative">
                  <input
                    type="time"
                    v-model="dia.fin"
                    :disabled="!dia.activo"
                    class="w-full p-2 md:p-2.5 rounded-xl border-2 border-slate-100 focus:border-blue-500 outline-none text-xs md:text-sm font-bold text-slate-600 disabled:bg-slate-50 disabled:text-slate-300 transition-all text-center"
                  />
                </div>
              </div>
            </div>
          </div>
        </div>

        <p v-if="errorMsg" class="mt-4 text-[10px] md:text-sm font-bold text-red-500 text-center bg-red-50 p-3 rounded-xl border border-red-100">
          ⚠️ {{ errorMsg }}
        </p>
      </div>

      <div class="p-4 md:p-8 bg-slate-50 border-t border-slate-100 flex flex-col-reverse md:flex-row gap-2 md:gap-3 shrink-0">
        <button
          @click="emit('cerrar')"
          class="w-full md:flex-1 py-3 md:py-4 bg-white border-2 border-slate-200 text-slate-500 rounded-xl md:rounded-2xl text-[10px] md:text-xs font-black uppercase tracking-widest hover:bg-slate-100 transition-all"
        >
          Cancelar
        </button>
        <button
          @click="guardarCambios"
          :disabled="guardando"
          class="w-full md:flex-[2] py-3 md:py-4 bg-blue-600 disabled:opacity-50 text-white rounded-xl md:rounded-2xl text-[10px] md:text-xs font-black uppercase tracking-widest hover:bg-blue-700 transition-all flex justify-center items-center shadow-md shadow-blue-100"
        >
          <span v-if="!guardando">Actualizar Horario</span>
          <span v-else class="flex items-center gap-2">
            <svg class="animate-spin h-4 w-4 text-white" viewBox="0 0 24 24">
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
  width: 4px;
}
.custom-scrollbar::-webkit-scrollbar-track {
  background: transparent;
}
.custom-scrollbar::-webkit-scrollbar-thumb {
  background: #e2e8f0;
  border-radius: 10px;
}
.custom-scrollbar::-webkit-scrollbar-thumb:hover {
  background: #cbd5e1;
}
</style>