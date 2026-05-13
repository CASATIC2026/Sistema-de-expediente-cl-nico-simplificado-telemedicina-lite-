<script setup>
import { ref, computed, onMounted } from 'vue'
import configuracionCuenta from '@/components/configuracionCuenta.vue'
import verHistorial from '@/components/verHistorial.vue'
import { getPacientesAdmin, toggleEstadoPaciente } from '@/services/api'

const emit = defineEmits(['cerrar'])

const pacientes        = ref([])
const cargando         = ref(true)
const error            = ref(null)
const filtroQuery      = ref('')

const accederInformacionPaciente = ref(false)
const pacienteSeleccionado       = ref(null)
const accederHistorial           = ref(false)
const pacienteHistorialId        = ref(null)

onMounted(async () => {
  await cargarPacientes()
})

const cargarPacientes = async () => {
  try {
    cargando.value  = true
    error.value     = null
    pacientes.value = await getPacientesAdmin()
  } catch (e) {
    error.value = 'No se pudo cargar la lista de pacientes.'
    console.error(e)
  } finally {
    cargando.value = false
  }
}

const pacientesFiltrados = computed(() => {
  const q = filtroQuery.value.toLowerCase().trim()
  if (!q) return pacientes.value
  return pacientes.value.filter(u =>
    u.dui?.toLowerCase().includes(q) ||
    u.nombre?.toLowerCase().includes(q) ||
    u.apellido?.toLowerCase().includes(q)
  )
})

const toggleEstado = async (paciente) => {
  const nuevoEstado = !paciente.activo
  const accion      = nuevoEstado ? 'activar' : 'inactivar'
  if (!confirm(`¿Deseas ${accion} a ${paciente.nombre} ${paciente.apellido}?`)) return
  try {
    await toggleEstadoPaciente(paciente.id, nuevoEstado)
    paciente.activo = nuevoEstado
  } catch (e) {
    alert('Error al cambiar el estado del paciente.')
    console.error(e)
  }
}

const abrirInformacion = (paciente) => {
  pacienteSeleccionado.value          = paciente
  accederInformacionPaciente.value    = true
}

const abrirHistorial = (paciente) => {
  pacienteHistorialId.value  = paciente
  accederHistorial.value     = true
}

const eliminarPaciente = (id) => {
  if (confirm('¿Eliminar este paciente?')) {
    pacientes.value = pacientes.value.filter(u => u.id !== id)
  }
}
</script>


<template>
  <div class="min-h-screen bg-slate-50 p-4 md:p-10 flex flex-col items-center">

    <!-- Botón regresar -->
    <div class="w-full max-w-[95%] lg:w-[90%] mb-4">
      <button
        class="text-sm font-bold uppercase tracking-widest bg-white border border-slate-200 hover:bg-slate-50 cursor-pointer text-cyan-600 px-5 py-3 rounded-2xl transition-all flex items-center gap-2 shadow-sm"
        type="button"
        @click="emit('cerrar')"
      >
        ← Regresar
      </button>
    </div>

    <div class="w-full max-w-[95%] lg:w-[90%]">

      <!-- Header + Buscador -->
      <div class="mb-8 flex flex-col md:flex-row md:items-end justify-between gap-6">
        <div>
          <h1 class="text-2xl md:text-3xl font-black text-slate-800 tracking-tight text-center md:text-left">Gestión de Usuarios</h1>
          <p class="text-slate-500 font-medium text-base md:text-lg text-center md:text-left">Panel de control administrativo</p>
        </div>

        <div class="relative w-full md:w-96 group">
          <div class="absolute inset-y-0 left-0 pl-4 flex items-center pointer-events-none">
            <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5 text-slate-400 group-focus-within:text-blue-500 transition-colors" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
            </svg>
          </div>
          <input
            v-model="filtroQuery"
            type="text"
            placeholder="Buscar por DUI o Nombre..."
            class="w-full pl-11 pr-4 py-3 md:py-4 bg-white border-2 border-slate-200 rounded-2xl md:rounded-[1.5rem] shadow-sm outline-none focus:border-blue-500 focus:ring-4 focus:ring-blue-50 transition-all text-sm font-semibold text-slate-700"
          />
        </div>
      </div>

      <!-- Estado: cargando -->
      <div v-if="cargando" class="flex justify-center items-center py-24">
        <div class="w-10 h-10 border-4 border-blue-200 border-t-blue-600 rounded-full animate-spin"></div>
      </div>

      <!-- Estado: error -->
      <div v-else-if="error" class="p-10 text-center text-red-500 font-bold bg-red-50 rounded-3xl border border-red-100">
        {{ error }}
        <button @click="cargarPacientes" class="mt-4 block mx-auto text-sm text-blue-600 underline">Reintentar</button>
      </div>

      <!-- Tabla escritorio -->
      <div v-else class="hidden md:block bg-slate-200 rounded-[2.5rem] shadow-xl shadow-slate-200/50 border border-slate-100 overflow-hidden">
        <div class="overflow-x-auto">
          <table class="w-full text-left border-collapse">
            <thead>
              <tr class="bg-slate-50/50 border-b border-slate-100 text-slate-400">
                <th class="px-8 py-6 text-[11px] font-black uppercase tracking-[0.2em]">Paciente</th>
                <th class="px-8 py-6 text-[11px] font-black uppercase tracking-[0.2em]">Identificador (DUI)</th>
                <th class="px-8 py-6 text-[11px] font-black uppercase tracking-[0.2em]">Estado</th>
                <th class="px-8 py-6 text-[11px] font-black uppercase tracking-[0.2em] text-center">Acciones</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-slate-50">
              <tr v-for="paciente in pacientesFiltrados" :key="paciente.id" class="hover:bg-blue-50/20 transition-all group">

                <!-- Nombre -->
                <td class="px-8 py-6">
                  <div class="flex items-center gap-4">
                    <div class="w-12 h-12 shrink-0 rounded-2xl bg-cyan-600 from-cyan-700 flex items-center justify-center font-bold text-slate-600 group-hover:from-blue-500 group-hover:to-blue-700 group-hover:text-white transition-all shadow-sm">
                      {{ paciente.nombre?.[0] }}{{ paciente.apellido?.[0] }}
                    </div>
                    <div>
                      <p class="font-bold text-slate-800 text-lg leading-tight">{{ paciente.nombre }} {{ paciente.apellido }}</p>
                      <p class="text-xs text-slate-400 font-bold uppercase tracking-tighter mt-1">Paciente</p>
                    </div>
                  </div>
                </td>

                <!-- DUI -->
                <td class="px-8 py-6">
                  <span class="font-mono text-base font-bold text-slate-600 bg-slate-100/80 px-4 py-2 rounded-xl">
                    {{ paciente.dui ?? 'Sin DUI' }}
                  </span>
                </td>

                <!-- Estado -->
                <td class="px-8 py-6">
                  <span
                    :class="paciente.activo ? 'bg-emerald-300 text-emerald-700' : 'bg-red-300 text-red-600'"
                    class="px-4 py-1.5 rounded-full text-[10px] font-black uppercase tracking-widest shadow-sm border border-white"
                  >
                    {{ paciente.activo ? 'Activo' : 'Inactivo' }}
                  </span>
                </td>

                <!-- Acciones -->
                <td class="px-8 py-6 text-center">
                  <div class="flex items-center justify-center gap-3">
                    <!-- INFO -->
                    <button
                      @click="abrirInformacion(paciente)"
                      class="h-11 px-5 bg-cyan-700 border-2 border-slate-100 text-white rounded-2xl text-xs font-black hover:bg-500 transition-all shadow-md"
                    >INFORMACIÓN</button>

                    <!-- HISTORIAL -->
                    <button
                      @click="abrirHistorial(paciente)"
                      class="h-11 px-5 bg-cyan-700 text-white rounded-2xl text-xs font-black hover:bg-cyan-500 transition-all shadow-md"
                    >HISTORIAL</button>

                    <!-- ACTIVAR / INACTIVAR -->
                    <button
                      @click="toggleEstado(paciente)"
                      :class="paciente.activo
                        ? 'bg-amber-50 text-amber-500 hover:bg-amber-100 border-amber-100'
                        : 'bg-emerald-50 text-emerald-600 hover:bg-emerald-100 border-emerald-100'"
                      class="h-11 px-4 rounded-2xl text-xs font-black border-2 transition-all"
                    >
                      {{ paciente.activo ? 'INACTIVAR' : 'ACTIVAR' }}
                    </button>

                    <!-- ELIMINAR -->
                    <button
                      @click="eliminarPaciente(paciente.id)"
                      class="h-11 w-11 flex items-center justify-center bg-slate-50 text-slate-400 rounded-2xl hover:bg-red-50 hover:text-red-500 transition-all"
                    >
                      <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
                      </svg>
                    </button>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

      <!-- Cards móvil -->
      <div class="md:hidden space-y-4">
        <div v-for="paciente in pacientesFiltrados" :key="paciente.id" class="bg-white p-6 rounded-[2rem] shadow-md border border-slate-100 flex flex-col gap-4">
          <div class="flex items-center gap-4">
            <div class="w-14 h-14 rounded-2xl bg-blue-600 flex items-center justify-center font-bold text-white shadow-lg">
              {{ paciente.nombre?.[0] }}{{ paciente.apellido?.[0] }}
            </div>
            <div class="flex-1">
              <div class="flex justify-between items-start">
                <p class="font-black text-slate-800 text-xl leading-tight">{{ paciente.nombre }}<br>{{ paciente.apellido }}</p>
                <span
                  :class="paciente.activo ? 'bg-emerald-100 text-emerald-700' : 'bg-red-100 text-red-600'"
                  class="px-3 py-1 rounded-full text-[9px] font-black uppercase tracking-widest border border-white"
                >{{ paciente.activo ? 'Activo' : 'Inactivo' }}</span>
              </div>
              <p class="font-mono text-sm font-bold text-slate-500 mt-2">{{ paciente.dui ?? 'Sin DUI' }}</p>
            </div>
          </div>

          <div class="grid grid-cols-2 gap-2 mt-2">
            <button @click="abrirInformacion(paciente)" class="py-4 bg-slate-50 text-slate-600 rounded-xl text-[10px] font-black uppercase tracking-widest border border-slate-100">Información</button>
            <button @click="abrirHistorial(paciente)" class="py-4 bg-blue-50 text-blue-600 rounded-xl text-[10px] font-black uppercase tracking-widest border border-blue-100">Historial</button>
            <button @click="toggleEstado(paciente)" :class="paciente.activo ? 'bg-amber-50 text-amber-500 border-amber-100' : 'bg-emerald-50 text-emerald-600 border-emerald-100'" class="py-4 rounded-xl text-[10px] font-black uppercase tracking-widest border">
              {{ paciente.activo ? 'Inactivar' : 'Activar' }}
            </button>
            <button @click="eliminarPaciente(paciente.id)" class="py-4 bg-red-50 text-red-500 rounded-xl text-[10px] font-black uppercase tracking-widest border border-red-100">Eliminar</button>
          </div>
        </div>
      </div>

      <!-- Sin resultados -->
      <div v-if="!cargando && !error && pacientesFiltrados.length === 0" class="p-12 md:p-24 text-center">
        <p class="text-slate-400 font-bold text-lg">No se encontraron pacientes.</p>
      </div>

    </div>
  </div>

  <!-- Modales -->
  <configuracionCuenta
    v-if="accederInformacionPaciente"
    :usuarioData="pacienteSeleccionado"
    :rolSesion = "pacienteSeleccionado?.rol??'paciente'"
    :modoAdmin="true"
    :pacienteId="pacienteSeleccionado?.id"
    @cerrar="accederInformacionPaciente = false"
  />
  <verHistorial
    v-if="accederHistorial"
    :usuarioSeleccionado ="pacienteHistorialId"
    :esAdmin = "true"
    rolVisor = "admin"
    @cerrar="accederHistorial = false"
  />
</template>