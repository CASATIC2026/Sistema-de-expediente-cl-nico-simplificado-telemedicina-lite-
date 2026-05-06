<script setup>
import { ref, onMounted, watch } from 'vue'
import { crearCita, getDoctores, buscarPacientePorDui, getSlotsDisponibles } from '@/services/api'

const emit = defineEmits(['cerrar'])

const props = defineProps({
  esSecretaria: { type: Boolean, default: false },
})
// ===============================
// ESTADO (REFS)
// ===============================
const duiBusqueda = ref('')
const pacienteEncontrado = ref(null)
const buscandoPaciente = ref(false)
const guardando = ref(false)
const cargandoHorarios = ref(false)
// FIX: Calcular fecha de hoy en hora local de El Salvador, no en UTC
const hoy = new Date().toLocaleDateString('es-CA', { timeZone: 'America/El_Salvador' })
const doctores = ref([])


const form = ref({
  pacienteId: '',
  doctorID: '',
  fecha: '',
  hora: '',
  tipo: '',
  motivo: '',
})

// ===============================
// WATCH — solo el watch maneja la recarga de horarios
// FIX: Se quitaron los @change duplicados del template
// ===============================
watch(
  () => [form.value.fecha, form.value.doctorID],
  () => {
    form.value.hora = ''
    cargarHorarios()
  }
)


// ===============================
// CARGAR DOCTORES
// ===============================
onMounted(async () => {
  try {
    const res = await getDoctores()
    doctores.value = res
  } catch (e) {
    console.error('Error cargando doctores', e)
  }
})

// ===============================
// CARGAR HORARIOS OCUPADOS
// ===============================
const cargarHorarios = async () => {
  if (!form.value.fecha || !form.value.doctorID) return

  cargandoHorarios.value    = true
  horariosDisponibles.value = []
  form.value.hora           = ''

  try {
    const slots = await getSlotsDisponibles(form.value.doctorID, form.value.fecha)
    // El backend ya filtra ocupados, pero por seguridad filtramos de nuevo
    horariosDisponibles.value = slots.filter(s => !s.ocupado)
  } catch (e) {
    console.error('Error cargando slots:', e)
    horariosDisponibles.value = []
  } finally {
    cargandoHorarios.value = false
  }
}

// ===============================
// HORARIOS DISPONIBLES (Computed)
// Solo muestra los que NO están ocupados
// ===============================
const horariosDisponibles = ref([])

// ===============================
// BUSCAR PACIENTE POR DUI (Solo secretaria/admin)
// FIX: Función que faltaba completamente
// ===============================
const buscarPaciente = async () => {
  if (!duiBusqueda.value.trim()) {
    alert('Ingrese un número de DUI para buscar.')
    return
  }

  buscandoPaciente.value = true
  pacienteEncontrado.value = null
  form.value.pacienteId = ''

  try {
    const res = await buscarPacientePorDui(duiBusqueda.value.trim())
    pacienteEncontrado.value = res
    form.value.pacienteId = res.id
  } catch (e) {
    const msg = e.response?.data?.message || 'Paciente no encontrado con ese DUI.'
    alert(msg)
    pacienteEncontrado.value = null
  } finally {
    buscandoPaciente.value = false
  }
}

// ===============================
// CALCULAR FECHA FIN (+30 minutos)
// FIX: Antes generaba "08:30:30:00" — ahora calcula bien
// ===============================
const calcularFechaFin = (fecha, hora) => {
  const [hh, mm] = hora.split(':').map(Number)
  const totalMin = hh * 60 + mm + 30
  const finHH = String(Math.floor(totalMin / 60)).padStart(2, '0')
  const finMM = String(totalMin % 60).padStart(2, '0')
  return `${fecha}T${finHH}:${finMM}:00-06:00`
}

// ===============================
// GUARDAR CITA
// ===============================
const guardarCita = async () => {
  // Validar que la secretaria haya buscado un paciente
  if (props.esSecretaria && !form.value.pacienteId) {
    alert('Debe buscar y seleccionar un paciente primero.')
    return

  }

  guardando.value = true

  try {
    const storedId = localStorage.getItem('user_id')
    const idParaConvertir = props.esSecretaria ? form.value.pacienteId : storedId
    const pacienteIdFinal = parseInt(idParaConvertir)

    if (!pacienteIdFinal || isNaN(pacienteIdFinal)) {
      alert('Error: No hay un ID de paciente válido.')
      return
    }

    //FIX: Timezone explícito de El Salvador (UTC-6)
    const fechaInicioStr = `${form.value.fecha}T${form.value.hora}:00-06:00`
    const fechaFinStr = calcularFechaFin(form.value.fecha, form.value.hora)

    const payload = {
      PacienteId: pacienteIdFinal,
      DoctorId: parseInt(form.value.doctorID),
      Motivo: form.value.motivo || 'Consulta General',
      FechaInicio: fechaInicioStr,
      FechaFin: fechaFinStr,
      TipoConsulta: form.value.tipo,
      LinkReunion: '',
    }

    await crearCita(payload)
    alert('¡Cita agendada con éxito!')

    // Refrescamos para que esa hora ya no aparezca disponible
    await cargarHorarios()
    emit('cerrar')

  } catch (err) {
    const mensajeError = err.response?.data?.message || 'Error al guardar la cita.'
    alert(mensajeError)
  } finally {
    guardando.value = false
  }
}
</script>

<template>
  <div
    class="fixed inset-0 z-[99998] flex items-center justify-center p-4 bg-slate-900/80 backdrop-blur-md"
    @click.self="emit('cerrar')"
  >
    <div class="relative w-full max-w-lg bg-white rounded-[2.5rem] shadow-2xl overflow-hidden flex flex-col max-h-[95vh]">

      <!-- HEADER -->
      <div class="relative bg-slate-800 px-8 pt-8 pb-7 shrink-0 overflow-hidden">
        
        <!-- Círculos decorativos -->
        <div class="absolute -top-10 -right-10 w-40 h-40 bg-cyan-500/10 rounded-full pointer-events-none"></div>
        <div class="absolute -bottom-6 left-8 w-24 h-24 bg-white/5 rounded-full pointer-events-none"></div>

        <!-- Botón cerrar -->
        <button
          type="button"
          @click="emit('cerrar')"
          class="absolute top-5 right-5 w-9 h-9 flex items-center justify-center rounded-full bg-white/10 hover:bg-white/20 text-white/70 hover:text-white transition-all border border-white/15"
        >
          <svg xmlns="http://www.w3.org/2000/svg" class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2.5">
            <path stroke-linecap="round" stroke-linejoin="round" d="M6 18L18 6M6 6l12 12"/>
          </svg>
        </button>

        <!-- Ícono + título -->
        <div class="flex items-center gap-4">
          <div class="w-12 h-12 rounded-2xl bg-white/10 border border-white/20 flex items-center justify-center shrink-0">
            <svg xmlns="http://www.w3.org/2000/svg" class="w-6 h-6 text-cyan-300" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="1.8">
              <path stroke-linecap="round" stroke-linejoin="round" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z"/>
            </svg>
          </div>
          <div>
            <p class="text-cyan-300/90 text-[10px] font-black uppercase tracking-[0.25em] mb-0.5">
              {{ props.esSecretaria ? 'Panel Administrativo' : 'Telemedicina' }}
            </p>
            <h2 class="text-cyan-300/30 text-xl font-black leading-tight">
              {{ props.esSecretaria ? 'Agendar cita para paciente' : 'Nueva cita médica' }}
            </h2>
          </div>
        </div>
      </div>

      <!-- CUERPO -->
      <div class="flex-1 overflow-y-auto bg-slate-50 px-8 py-7 space-y-6 custom-scrollbar">

        <!-- BUSCAR PACIENTE (solo secretaria) -->
        <div v-if="props.esSecretaria" class="bg-blue-50 border-2 border-blue-100 rounded-2xl p-5 space-y-3">
          <p class="text-[11px] font-black uppercase tracking-[0.2em] text-slate-500">Buscar paciente por DUI</p>
          
          <div class="flex gap-2">
            <input
              v-model="duiBusqueda"
              type="text"
              placeholder="00000000-0"
              class="flex-1 px-4 py-3 bg-white border-2 border-slate-200 focus:border-cyan-500 focus:ring-4 focus:ring-cyan-50 rounded-xl text-sm font-semibold text-slate-700 outline-none transition-all"
            />
            <button
              type="button"
              :disabled="buscandoPaciente"
              @click="buscarPaciente"
              class="px-5 py-3 bg-slate-900 hover:bg-cyan-700 disabled:opacity-50 text-white text-sm font-black rounded-xl transition-all flex items-center gap-2 whitespace-nowrap"
            >
              <span v-if="buscandoPaciente" class="w-4 h-4 border-2 border-white/30 border-t-white rounded-full animate-spin"></span>
              <svg v-else xmlns="http://www.w3.org/2000/svg" class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2.5">
                <path stroke-linecap="round" stroke-linejoin="round" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"/>
              </svg>
              {{ buscandoPaciente ? '' : 'Buscar' }}
            </button>
          </div>

          <!-- Paciente encontrado -->
          <div v-if="pacienteEncontrado" class="flex items-center gap-3 bg-emerald-50 border-2 border-emerald-200 px-4 py-3 rounded-xl">
            <div class="w-9 h-9 rounded-xl bg-emerald-100 flex items-center justify-center text-xs font-black text-emerald-700 shrink-0">
              {{ pacienteEncontrado.nombre?.[0] }}{{ pacienteEncontrado.apellido?.[0] }}
            </div>
            <div class="flex-1 min-w-0">
              <p class="text-sm font-black text-emerald-800 leading-none truncate">
                {{ pacienteEncontrado.nombre }} {{ pacienteEncontrado.apellido }}
              </p>
              <p class="text-[11px] font-semibold text-emerald-600 mt-0.5">Paciente verificado</p>
            </div>
            <svg xmlns="http://www.w3.org/2000/svg" class="w-5 h-5 text-emerald-500 shrink-0" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2.5">
              <path stroke-linecap="round" stroke-linejoin="round" d="M5 13l4 4L19 7"/>
            </svg>
          </div>

          <!-- Advertencia sin paciente -->
          <p v-if="!pacienteEncontrado" class="text-[11px] font-bold text-amber-600 flex items-center gap-1.5">
            <svg xmlns="http://www.w3.org/2000/svg" class="w-3.5 h-3.5 shrink-0" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2.5">
              <path stroke-linecap="round" stroke-linejoin="round" d="M12 9v2m0 4h.01M10.29 3.86L1.82 18a2 2 0 001.71 3h16.94a2 2 0 001.71-3L13.71 3.86a2 2 0 00-3.42 0z"/>
            </svg>
            Busque un paciente antes de continuar
          </p>
        </div>

        <!-- DOCTOR -->
        <div class="space-y-2">
          <label class="text-[11px] font-black uppercase tracking-[0.2em] text-slate-500">Doctor disponible</label>
          <div class="relative">
            <select
              v-model="form.doctorID"
              class="w-full px-4 py-3.5 bg-white border-2 border-slate-200 focus:border-cyan-500 focus:ring-4 focus:ring-cyan-50 rounded-xl text-sm font-semibold text-slate-700 outline-none appearance-none transition-all cursor-pointer"
              required
            >
              <option value="" disabled>Seleccione un doctor</option>
              <option v-for="d in doctores" :key="d.id" :value="d.id">
                {{ d.nombreCompleto || d.nombre }}
              </option>
            </select>
            <svg xmlns="http://www.w3.org/2000/svg" class="w-4 h-4 text-slate-400 absolute right-4 top-1/2 -translate-y-1/2 pointer-events-none" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2.5">
              <path stroke-linecap="round" stroke-linejoin="round" d="M19 9l-7 7-7-7"/>
            </svg>
          </div>
        </div>

        <!-- FECHA + HORA -->
        <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
          <div class="space-y-2">
            <label class="text-[11px] font-black uppercase tracking-[0.2em] text-slate-500">Fecha</label>
            <input
              type="date"
              v-model="form.fecha"
              :min="hoy"
              class="w-full px-4 py-3.5 bg-white border-2 border-slate-200 focus:border-cyan-500 focus:ring-4 focus:ring-cyan-50 rounded-xl text-sm font-semibold text-slate-700 outline-none transition-all"
              required
            />
          </div>

          <div class="space-y-2">
            <label class="text-[11px] font-black uppercase tracking-[0.2em] text-slate-500">Horario</label>

            <div v-if="cargandoHorarios" class="w-full px-4 py-3.5 bg-white border-2 border-slate-200 rounded-xl flex items-center gap-2 text-sm text-slate-400 font-semibold">
              <span class="w-4 h-4 border-2 border-slate-300 border-t-cyan-500 rounded-full animate-spin"></span>
              Cargando horarios...
            </div>

            <div v-else class="relative">
              <select
                v-model="form.hora"
                :disabled="!form.fecha || !form.doctorID"
                class="w-full px-4 py-3.5 bg-white border-2 border-slate-200 focus:border-cyan-500 focus:ring-4 focus:ring-cyan-50 rounded-xl text-sm font-semibold text-slate-700 outline-none appearance-none transition-all disabled:opacity-40 disabled:cursor-not-allowed cursor-pointer"
                required
              >
                <option value="" disabled>
                  {{ (!form.fecha || !form.doctorID) ? 'Elija doctor y fecha' : horariosDisponibles.length === 0 ? 'Sin horarios disponibles' : 'Seleccionar horario' }}
                </option>
                <option v-for="h in horariosDisponibles" :key="h.valor" :value="h.valor">
                  {{ h.etiqueta }}
                </option>
              </select>
              <svg xmlns="http://www.w3.org/2000/svg" class="w-4 h-4 text-slate-400 absolute right-4 top-1/2 -translate-y-1/2 pointer-events-none" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2.5">
                <path stroke-linecap="round" stroke-linejoin="round" d="M19 9l-7 7-7-7"/>
              </svg>
            </div>

            <p v-if="!cargandoHorarios && form.fecha && form.doctorID && horariosDisponibles.length === 0"
               class="text-[11px] font-bold text-amber-600 flex items-center gap-1.5">
              <svg xmlns="http://www.w3.org/2000/svg" class="w-3.5 h-3.5 shrink-0" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2.5">
                <path stroke-linecap="round" stroke-linejoin="round" d="M12 9v2m0 4h.01M10.29 3.86L1.82 18a2 2 0 001.71 3h16.94a2 2 0 001.71-3L13.71 3.86a2 2 0 00-3.42 0z"/>
              </svg>
              Sin horarios disponibles para este día
            </p>
          </div>
        </div>

        <!-- TIPO DE CONSULTA -->
        <div class="space-y-2">
          <label class="text-[11px] font-black uppercase tracking-[0.2em] text-slate-500">Tipo de consulta</label>
          <div class="grid grid-cols-2 gap-3">
            <button
              type="button"
              @click="form.tipo = 'Primera vez'"
              :class="form.tipo === 'Primera vez'
                ? 'bg-slate-900 text-white border-slate-900 shadow-lg'
                : 'bg-white text-slate-500 border-slate-200 hover:border-cyan-400 hover:text-cyan-600'"
              class="py-4 px-4 rounded-2xl text-xs font-black border-2 transition-all text-left"
            >
              <svg xmlns="http://www.w3.org/2000/svg" class="w-5 h-5 mb-2" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="1.8">
                <path stroke-linecap="round" stroke-linejoin="round" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z"/>
              </svg>
              Primera vez
            </button>
            <button
              type="button"
              @click="form.tipo = 'Seguimiento'"
              :class="form.tipo === 'Seguimiento'
                ? 'bg-slate-900 text-white border-slate-900 shadow-lg'
                : 'bg-white text-slate-500 border-slate-200 hover:border-cyan-400 hover:text-cyan-600'"
              class="py-4 px-4 rounded-2xl text-xs font-black border-2 transition-all text-left"
            >
              <svg xmlns="http://www.w3.org/2000/svg" class="w-5 h-5 mb-2" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="1.8">
                <path stroke-linecap="round" stroke-linejoin="round" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15"/>
              </svg>
              Seguimiento
            </button>
          </div>
        </div>

        <!-- MOTIVO -->
        <div class="space-y-2">
          <label class="text-[11px] font-black uppercase tracking-[0.2em] text-slate-500">Motivo de consulta</label>
          <textarea
            v-model="form.motivo"
            rows="3"
            placeholder="Describa brevemente el motivo..."
            class="w-full px-4 py-3.5 bg-white border-2 border-slate-200 focus:border-cyan-500 focus:ring-4 focus:ring-cyan-50 rounded-xl text-sm font-semibold text-slate-700 outline-none transition-all resize-none placeholder:text-slate-300"
            required
          ></textarea>
        </div>

        <!-- SUBMIT -->
        <button
          type="button"
          @click="guardarCita"
          :disabled="guardando || (props.esSecretaria && !pacienteEncontrado)"
          class="w-full py-4 bg-gradient-to-r from-slate-900 to-cyan-700 hover:from-slate-800 hover:to-cyan-600 disabled:opacity-40 disabled:cursor-not-allowed text-white text-sm font-black uppercase tracking-widest rounded-2xl transition-all shadow-lg hover:shadow-cyan-200 hover:-translate-y-0.5 active:translate-y-0 flex items-center justify-center gap-3"
        >
          <span v-if="guardando" class="w-5 h-5 border-2 border-white/30 border-t-white rounded-full animate-spin"></span>
          <svg v-else xmlns="http://www.w3.org/2000/svg" class="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2.5">
            <path stroke-linecap="round" stroke-linejoin="round" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z"/>
          </svg>
          {{ guardando ? 'Registrando cita...' : (props.esSecretaria ? 'Registrar cita en el sistema' : 'Confirmar mi cita médica') }}
        </button>

      </div>
    </div>
  </div>
</template>

<style scoped>
.custom-scrollbar::-webkit-scrollbar { width: 6px; }
.custom-scrollbar::-webkit-scrollbar-track { background: transparent; }
.custom-scrollbar::-webkit-scrollbar-thumb { background: #e2e8f0; border-radius: 10px; }
.custom-scrollbar::-webkit-scrollbar-thumb:hover { background: #cbd5e1; }
</style>