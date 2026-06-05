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
  } catch {}
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
  } catch {

    horariosDisponibles.value = []
  } finally {
    cargandoHorarios.value = false
  }
}

// HORARIOS DISPONIBLES (Computed)
const horariosDisponibles = ref([])

const toast = ref({ visible: false, mensaje: '', tipo: 'success' })
let toastTimer = null
const mostrarToast = (mensaje, tipo = 'success') => {
  if (toastTimer) clearTimeout(toastTimer)
  toast.value = { visible: true, mensaje, tipo }
  toastTimer = setTimeout(() => { toast.value.visible = false }, 3500)
}

// ===============================
// BUSCAR PACIENTE POR DUI (Solo secretaria/admin)
const buscarPaciente = async () => {
  if (!duiBusqueda.value.trim()) {
    mostrarToast('Ingrese un número de DUI para buscar.', 'error')
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
    mostrarToast(msg, 'error')
    pacienteEncontrado.value = null
  } finally {
    buscandoPaciente.value = false
  }
}

// ===============================
// CALCULAR FECHA FIN (+30 minutos)
const calcularFechaFin = (fecha, hora) => {
  const [hh, mm] = hora.split(':').map(Number)
  const totalMin = hh * 60 + mm + 30
  const finHH = String(Math.floor(totalMin / 60)).padStart(2, '0')
  const finMM = String(totalMin % 60).padStart(2, '0')
  return `${fecha}T${finHH}:${finMM}:00-06:00`
}

// ===============================
// GUARDAR CITA
const guardarCita = async () => {
  // Validar que la secretaria haya buscado un paciente
  if (props.esSecretaria && !form.value.pacienteId) {
    mostrarToast('Debe buscar y seleccionar un paciente primero.', 'error')
    return

  }

  guardando.value = true

  try {
    const storedId = localStorage.getItem('user_id')
    const idParaConvertir = props.esSecretaria ? form.value.pacienteId : storedId
    const pacienteIdFinal = parseInt(idParaConvertir)

    if (!pacienteIdFinal || isNaN(pacienteIdFinal)) {
      mostrarToast('Error: No hay un ID de paciente válido.', 'error')
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

    await cargarHorarios()
    mostrarToast('¡Cita agendada con éxito!', 'success')
    setTimeout(() => emit('cerrar'), 2500)

  } catch (err) {
    const mensajeError = err.response?.data?.message || 'Error al guardar la cita, los campos no están completos o el horario no está disponible.'
    mostrarToast(mensajeError, 'error')
  } finally {
    guardando.value = false
  }
}
</script>

<template>
  <div
    class="fixed inset-0 z-[99998] flex items-center justify-center p-2 sm:p-4 bg-slate-900/80 backdrop-blur-md"
    @click.self="emit('cerrar')"
  >
    <div
      class="relative w-full max-w-lg
      bg-white dark:bg-slate-900
      rounded-[2rem] sm:rounded-[2.5rem]
      shadow-2xl overflow-hidden flex flex-col
      max-h-[98vh] sm:max-h-[95vh]
      border border-slate-200 dark:border-slate-700"
    >

      <!-- HEADER -->
      <div
        class="relative
        bg-slate-800 dark:bg-slate-950
        px-5 sm:px-8 pt-6 sm:pt-8 pb-5 sm:pb-7
        shrink-0 overflow-hidden"
      >

        <div class="absolute -top-10 -right-10 w-40 h-40 bg-cyan-500/10 rounded-full pointer-events-none"></div>
        <div class="absolute -bottom-6 left-8 w-24 h-24 bg-white/5 rounded-full pointer-events-none"></div>

        <button
          type="button"
          @click="emit('cerrar')"
          class="absolute top-4 right-4 sm:top-5 sm:right-5
          w-9 h-9 flex items-center justify-center rounded-full
          bg-white/10 hover:bg-white/20
          text-white/70 hover:text-white
          transition-all border border-white/15"
        >
          <svg xmlns="http://www.w3.org/2000/svg" class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2.5">
            <path stroke-linecap="round" stroke-linejoin="round" d="M6 18L18 6M6 6l12 12"/>
          </svg>
        </button>

        <div class="flex items-center gap-3 sm:gap-4 pr-10">
          <div
            class="w-10 h-10 sm:w-12 sm:h-12
            rounded-2xl bg-white/10 border border-white/20
            flex items-center justify-center shrink-0"
          >
            <svg xmlns="http://www.w3.org/2000/svg" class="w-5 h-5 sm:w-6 sm:h-6 text-cyan-300" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="1.8">
              <path stroke-linecap="round" stroke-linejoin="round" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z"/>
            </svg>
          </div>

          <div class="min-w-0">
            <p class="text-cyan-400 text-[10px] font-black uppercase tracking-[0.25em] mb-0.5 truncate">
              {{ props.esSecretaria ? 'Panel Administrativo' : 'Telemedicina' }}
            </p>

            <h2 class="text-white text-lg sm:text-xl font-black leading-tight truncate">
              {{ props.esSecretaria ? 'Agendar cita para paciente' : 'Nueva cita médica' }}
            </h2>
          </div>
        </div>
      </div>

      <!-- CUERPO -->
      <div
        class="flex-1 overflow-y-auto
        bg-slate-50 dark:bg-slate-950
        px-4 sm:px-8 py-5 sm:py-7
        space-y-5 sm:space-y-6 custom-scrollbar"
      >

        <!-- BUSCAR PACIENTE -->
        <div
          v-if="props.esSecretaria"
          class="bg-blue-50 dark:bg-slate-900
          border-2 border-blue-100 dark:border-slate-700
          rounded-3xl p-4 sm:p-5 space-y-3"
        >
          <p class="text-[11px] font-black uppercase tracking-[0.2em] text-slate-500 dark:text-slate-400">
            Buscar paciente por DUI
          </p>

          <div class="flex flex-wrap gap-2">
            <input
              v-model="duiBusqueda"
              type="text"
              placeholder="00000000-0"
              class="flex-1 min-w-0 px-4 py-3
              bg-white dark:bg-slate-800
              border-2 border-slate-200 dark:border-slate-700
              focus:border-cyan-500 focus:ring-4 focus:ring-cyan-500/20
              rounded-xl text-sm font-semibold
              text-slate-700 dark:text-slate-100
              outline-none transition-all"
            />

            <button
              type="button"
              :disabled="buscandoPaciente"
              @click="buscarPaciente"
              class="px-4 sm:px-5 py-3
              bg-slate-900 dark:bg-cyan-700
              hover:bg-cyan-700 dark:hover:bg-cyan-600
              disabled:opacity-50
              text-white text-sm font-black
              rounded-xl transition-all
              flex items-center gap-2 whitespace-nowrap shrink-0"
            >
              <span
                v-if="buscandoPaciente"
                class="w-4 h-4 border-2 border-white/30 border-t-white rounded-full animate-spin"
              ></span>

              <svg
                v-else
                xmlns="http://www.w3.org/2000/svg"
                class="w-4 h-4"
                fill="none"
                viewBox="0 0 24 24"
                stroke="currentColor"
                stroke-width="2.5"
              >
                <path stroke-linecap="round" stroke-linejoin="round" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"/>
              </svg>

              {{ buscandoPaciente ? '' : 'Buscar' }}
            </button>
          </div>

          <div
            v-if="pacienteEncontrado"
            class="flex items-center gap-3
            bg-emerald-50 dark:bg-emerald-950/30
            border-2 border-emerald-200 dark:border-emerald-800
            px-4 py-3 rounded-xl"
          >
            <div
              class="w-9 h-9 rounded-xl
              bg-emerald-100 dark:bg-emerald-900
              flex items-center justify-center
              text-xs font-black
              text-emerald-700 dark:text-emerald-300
              shrink-0"
            >
              {{ pacienteEncontrado.nombre?.[0] }}{{ pacienteEncontrado.apellido?.[0] }}
            </div>

            <div class="flex-1 min-w-0">
              <p class="text-sm font-black text-emerald-800 dark:text-emerald-200 leading-none truncate">
                {{ pacienteEncontrado.nombre }} {{ pacienteEncontrado.apellido }}
              </p>

              <p class="text-[11px] font-semibold text-emerald-600 dark:text-emerald-400 mt-0.5">
                Paciente verificado
              </p>
            </div>
          </div>

          <p
            v-if="!pacienteEncontrado"
            class="text-[11px] font-bold text-amber-600 dark:text-amber-400 flex items-center gap-1.5"
          >
            Busque un paciente antes de continuar
          </p>
        </div>

        <!-- DOCTOR -->
        <div class="space-y-2">
          <label class="text-[11px] font-black uppercase tracking-[0.2em] text-slate-500 dark:text-slate-400">
            Doctor disponible
          </label>

          <div class="relative">
            <select
              v-model="form.doctorID"
              class="w-full px-4 py-3.5
              bg-white dark:bg-slate-800
              border-2 border-slate-200 dark:border-slate-700
              focus:border-cyan-500 focus:ring-4 focus:ring-cyan-500/20
              rounded-xl text-sm font-semibold
              text-slate-700 dark:text-slate-100
              outline-none appearance-none transition-all cursor-pointer"
              required
            >
              <option value="" disabled>
                Seleccione un doctor
              </option>

              <option v-for="d in doctores" :key="d.id" :value="d.id">
                {{ d.nombreCompleto || d.nombre }}
              </option>
            </select>
          </div>
        </div>

        <!-- FECHA + HORA -->
        <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">

          <!-- FECHA -->
          <div class="space-y-2">
            <label class="text-[11px] font-black uppercase tracking-[0.2em] text-slate-500 dark:text-slate-400">
              Fecha
            </label>

            <input
              type="date"
              v-model="form.fecha"
              :min="hoy"
              class="w-full px-4 py-3.5
              bg-white dark:bg-slate-800
              border-2 border-slate-200 dark:border-slate-700
              focus:border-cyan-500 focus:ring-4 focus:ring-cyan-500/20
              rounded-xl text-sm font-semibold
              text-slate-700 dark:text-slate-100
              outline-none transition-all"
              required
            />
          </div>

          <!-- HORA -->
          <div class="space-y-2">
            <label class="text-[11px] font-black uppercase tracking-[0.2em] text-slate-500 dark:text-slate-400">
              Horario
            </label>

            <div
              v-if="cargandoHorarios"
              class="w-full px-4 py-3.5
              bg-white dark:bg-slate-800
              border-2 border-slate-200 dark:border-slate-700
              rounded-xl flex items-center gap-2
              text-sm text-slate-400 dark:text-slate-300 font-semibold"
            >
              <span class="w-4 h-4 border-2 border-slate-300 border-t-cyan-500 rounded-full animate-spin"></span>
              Cargando horarios...
            </div>

            <div v-else class="relative">
              <select
                v-model="form.hora"
                :disabled="!form.fecha || !form.doctorID"
                class="w-full px-4 py-3.5
                bg-white dark:bg-slate-800
                border-2 border-slate-200 dark:border-slate-700
                focus:border-cyan-500 focus:ring-4 focus:ring-cyan-500/20
                rounded-xl text-sm font-semibold
                text-slate-700 dark:text-slate-100
                outline-none appearance-none transition-all
                disabled:opacity-40 disabled:cursor-not-allowed
                cursor-pointer"
                required
              >
                <option value="" disabled>
                  {{
                    (!form.fecha || !form.doctorID)
                      ? 'Elija doctor y fecha'
                      : horariosDisponibles.length === 0
                        ? 'Sin horarios disponibles'
                        : 'Seleccionar horario'
                  }}
                </option>

                <option
                  v-for="h in horariosDisponibles"
                  :key="h.valor"
                  :value="h.valor"
                >
                  {{ h.etiqueta }}
                </option>
              </select>
            </div>
          </div>
        </div>

        <!-- TIPO -->
        <div class="space-y-2">
          <label class="text-[11px] font-black uppercase tracking-[0.2em] text-slate-500 dark:text-slate-400">
            Tipo de consulta
          </label>

          <div class="grid grid-cols-2 gap-3">

            <!-- Primera vez -->
            <button
              type="button"
              @click="form.tipo = 'Primera vez'"
              :class="form.tipo === 'Primera vez'
                ? 'bg-slate-900 dark:bg-cyan-700 text-white border-slate-900 dark:border-cyan-700 shadow-lg'
                : 'bg-white dark:bg-slate-800 text-slate-500 dark:text-slate-300 border-slate-200 dark:border-slate-700 hover:border-cyan-400 hover:text-cyan-600 dark:hover:text-cyan-400'"
              class="py-3 sm:py-4 px-3 sm:px-4 rounded-2xl text-xs font-black border-2 transition-all text-left"
            >
              Primera vez
            </button>

            <!-- Seguimiento -->
            <button
              type="button"
              @click="form.tipo = 'Seguimiento'"
              :class="form.tipo === 'Seguimiento'
                ? 'bg-slate-900 dark:bg-cyan-700 text-white border-slate-900 dark:border-cyan-700 shadow-lg'
                : 'bg-white dark:bg-slate-800 text-slate-500 dark:text-slate-300 border-slate-200 dark:border-slate-700 hover:border-cyan-400 hover:text-cyan-600 dark:hover:text-cyan-400'"
              class="py-3 sm:py-4 px-3 sm:px-4 rounded-2xl text-xs font-black border-2 transition-all text-left"
            >
              Seguimiento
            </button>

          </div>
        </div>

        <!-- MOTIVO -->
        <div class="space-y-2">
          <label class="text-[11px] font-black uppercase tracking-[0.2em] text-slate-500 dark:text-slate-400">
            Motivo de consulta
          </label>

          <textarea
            v-model="form.motivo"
            rows="3"
            placeholder="Describa brevemente el motivo..."
            class="w-full px-4 py-3.5
            bg-white dark:bg-slate-800
            border-2 border-slate-200 dark:border-slate-700
            focus:border-cyan-500 focus:ring-4 focus:ring-cyan-500/20
            rounded-xl text-sm font-semibold
            text-slate-700 dark:text-slate-100
            outline-none transition-all resize-none
            placeholder:text-slate-300 dark:placeholder:text-slate-500"
            required
          ></textarea>
        </div>

        <!-- SUBMIT -->
        <button
          type="button"
          @click="guardarCita"
          :disabled="guardando || (props.esSecretaria && !pacienteEncontrado)"
          class="w-full py-4
          bg-gradient-to-r from-slate-900 to-cyan-700
          dark:from-cyan-700 dark:to-blue-700
          hover:from-slate-800 hover:to-cyan-600
          disabled:opacity-40 disabled:cursor-not-allowed
          text-white text-sm font-black uppercase tracking-widest
          rounded-2xl transition-all shadow-lg
          hover:shadow-cyan-500/20 hover:-translate-y-0.5
          active:translate-y-0
          flex items-center justify-center gap-3"
        >
          <span
            v-if="guardando"
            class="w-5 h-5 border-2 border-white/30 border-t-white rounded-full animate-spin"
          ></span>

          <svg
            v-else
            xmlns="http://www.w3.org/2000/svg"
            class="w-5 h-5"
            fill="none"
            viewBox="0 0 24 24"
            stroke="currentColor"
            stroke-width="2.5"
          >
            <path stroke-linecap="round" stroke-linejoin="round" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z"/>
          </svg>

          {{
            guardando
              ? 'Registrando cita...'
              : (props.esSecretaria
                  ? 'Registrar cita en el sistema'
                  : 'Confirmar mi cita médica')
          }}
        </button>

      </div>
    </div>
    <!-- TOAST -->
    <Transition
      enter-active-class="transition-all duration-300 ease-out"
      enter-from-class="opacity-0 translate-y-4 scale-95"
      enter-to-class="opacity-100 translate-y-0 scale-100"
      leave-active-class="transition-all duration-200 ease-in"
      leave-from-class="opacity-100 translate-y-0 scale-100"
      leave-to-class="opacity-0 translate-y-4 scale-95"
    >
      <div
        v-if="toast.visible"
        class="fixed bottom-6 right-6 z-[99999] flex items-center gap-3 px-5 py-4 rounded-2xl shadow-2xl border backdrop-blur-sm min-w-[280px] max-w-sm"
        :class="toast.tipo === 'success'
          ? 'bg-emerald-950/90 border-emerald-500/30 text-emerald-300'
          : 'bg-red-950/90 border-red-500/30 text-red-300'"
      >
        <div
          class="shrink-0 w-8 h-8 rounded-xl flex items-center justify-center"
          :class="toast.tipo === 'success' ? 'bg-emerald-500/20' : 'bg-red-500/20'"
        >
          <svg v-if="toast.tipo === 'success'" xmlns="http://www.w3.org/2000/svg" class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2.5">
            <path stroke-linecap="round" stroke-linejoin="round" d="M5 13l4 4L19 7" />
          </svg>
          <svg v-else xmlns="http://www.w3.org/2000/svg" class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2.5">
            <path stroke-linecap="round"
            stroke-linejoin="round"
            d="M12 9v3m0 4h.01M10.29 3.86l-7.5 13A1 1 0 003.67 18h16.66a1 1 0 00.87-1.5l-7.5-13a1 1 0 00-1.74 0z" />
          </svg>
        </div>

        <div class="flex-1">
          <p class="text-[11px] font-black uppercase tracking-widest mb-0.5"
            :class="toast.tipo === 'success' ? 'text-emerald-400' : 'text-red-400'">
            {{ toast.tipo === 'success' ? 'Éxito' : 'Error' }}
          </p>
          <p class="text-xs font-semibold leading-snug">{{ toast.mensaje }}</p>
        </div>

        <button
          @click="toast.visible = false"
          class="shrink-0 w-12 h-12  text-3xl rounded-lg flex items-center justify-center opacity-50 hover:opacity-100 transition-opacity"
          :class="toast.tipo === 'success' ? 'hover:bg-emerald-500/20' : 'hover:bg-red-500/20'"
        >
          &times;
        </button>

        <div class="absolute bottom-0 left-0 h-[2px] rounded-full w-full overflow-hidden">
          <div
            class="h-full rounded-full"
            :class="toast.tipo === 'success' ? 'bg-emerald-400' : 'bg-red-400'"
            style="animation: shrink 3.5s linear forwards"
          />
        </div>
      </div>
    </Transition>
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
  background: #cbd5e1;
  border-radius: 10px;
}

.dark .custom-scrollbar::-webkit-scrollbar-thumb {
  background: #334155;
}

.custom-scrollbar::-webkit-scrollbar-thumb:hover {
  background: #94a3b8;
}

@keyframes shrink {
  from { width: 100%; }
  to   { width: 0%; }
}
</style>
