<script setup>
defineOptions({ name: 'DoctoresView' })

import { ref, computed, onMounted } from 'vue'
import diasDisponibles from '../components/diasDisponibles.vue'
import configuracionCuenta from './configuracionCuenta.vue'
import { getDoctoresAdmin, toggleEstadoDoctor, createDoctor } from '@/services/api'

const emit = defineEmits(['cerrar'])

// ===============================
// ESTADO
const doctores = ref([])
const filtroQuery = ref('')
const cargando = ref(false)
const mostrarPassword = ref(false)

// Modales
const abrirDias = ref(false)
const informacionDoctor = ref(false)
const mostrarFormularioNuevo = ref(false)
const doctorSeleccionado = ref(null)

// Formulario nuevo doctor ← FALTABA ESTO
const nuevoDoctor = ref({
  nombre: '',
  apellido: '',
  email: '',
  password: '',
  telefono: '',
  dui: '',
  genero: '',
  direccion: '',
  fechaNacimiento: ''
})
const guardandoDoctor = ref(false)
const errorFormulario = ref('')

// ===============================
// CARGAR DOCTORES DEL BACKEND
const cargarDoctores = async () => {
  cargando.value = true
  try {
    const data = await getDoctoresAdmin()
    doctores.value = data.map(d => ({
      id: d.id,
      nombre: d.nombre,
      apellido: d.apellido,
      dui: d.dui || 'Sin DUI',
      email: d.email,
      telefono: d.telefono,
      fotoUrl: d.fotoUrl,
      activo: d.activo ?? true,
      raw: d
    }))
  } catch (e) {
    console.error('Error cargando doctores:', e)
  } finally {
    cargando.value = false
  }
}

onMounted(() => cargarDoctores())

// ===============================
// FILTRO
const doctoresFiltrados = computed(() => {
  const query = filtroQuery.value.toLowerCase().trim()
  if (!query) return doctores.value
  return doctores.value.filter(d =>
    d.dui?.toLowerCase().includes(query) ||
    d.nombre?.toLowerCase().includes(query) ||
    d.apellido?.toLowerCase().includes(query) ||
    d.email?.toLowerCase().includes(query)
  )
})

// ===============================
// TOGGLE ESTADO
const toggleEstado = async (doctor) => {
  const nuevoEstado = !doctor.activo
  try {
    await toggleEstadoDoctor(doctor.id, nuevoEstado)
    doctor.activo = nuevoEstado
  } catch (e) {
    console.error('Error cambiando estado:', e)
    alert('No se pudo cambiar el estado del doctor.')
  }
}

// ===============================
// VER INFORMACIÓN
const verInformacion = (doctor) => {
  doctorSeleccionado.value = doctor.raw
  informacionDoctor.value = true
}

// ===============================
// VER DÍAS DISPONIBLES
const verDiasDisponibles = (doctor) => {
  doctorSeleccionado.value = doctor
  abrirDias.value = true
}

// ===============================
// CREAR NUEVO DOCTOR — solo una vez, con payload correcto
const guardarNuevoDoctor = async () => {
  errorFormulario.value = ''
  guardandoDoctor.value = true
  try {
    await createDoctor({
      Nombre:          nuevoDoctor.value.nombre,
      Apellido:        nuevoDoctor.value.apellido,
      Email:           nuevoDoctor.value.email,
      Password:        nuevoDoctor.value.password,
      Telefono:        nuevoDoctor.value.telefono,
      DUI:             nuevoDoctor.value.dui,
      Genero:          nuevoDoctor.value.genero,
      Direccion:       nuevoDoctor.value.direccion,
      FechaNacimiento: nuevoDoctor.value.fechaNacimiento
    })
    alert('Doctor registrado correctamente ✅')
    mostrarFormularioNuevo.value = false
    nuevoDoctor.value = {
      nombre: '', apellido: '', email: '', password: '',
      telefono: '', dui: '', genero: '', direccion: '', fechaNacimiento: ''
    }
    await cargarDoctores()
  } catch (e) {
    const data = e.response?.data
    if(data?.errors) {
      const mensaje = Object.values(data.errors).flat().join(' | ')
      errorFormulario.value = mensaje
    } else {
    errorFormulario.value = data?.message || 'Error al registrar el doctor.'
  } 
  
}

}
</script>



<template>

  <div class="min-h-screen bg-slate-50 p-4 md:p-10 flex flex-col items-center">

    <!-- HEADER -->
    <div class="w-full max-w-[95%] lg:w-[90%] mb-4">
      <button @click="emit('cerrar')"
        class="text-sm font-bold uppercase tracking-widest bg-white border border-slate-200 hover:bg-slate-50 cursor-pointer text-cyan-600 px-5 py-3 rounded-2xl transition-all flex items-center gap-2 shadow-sm">
        ← Regresar
      </button>
    </div>

    <div class="w-full max-w-[95%] lg:w-[90%]">

      <!-- TÍTULO + BUSCADOR + BOTÓN -->
      <div class="mb-8 flex flex-col md:flex-row md:items-end justify-between gap-6">
        <div>
          <h1 class="text-2xl md:text-3xl font-black text-slate-800 tracking-tight">Gestión de Doctores</h1>
          <p class="text-slate-500 font-medium text-base">Panel de control administrativo</p>
        </div>

        <div class="relative w-full md:w-96 group">
          <div class="absolute inset-y-0 left-0 pl-4 flex items-center pointer-events-none">
            <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5 text-slate-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
            </svg>
          </div>
          <input v-model="filtroQuery" type="text"
            placeholder="Buscar por DUI, nombre o email..."
            class="w-full pl-11 pr-4 py-3 md:py-4 bg-white border-2 border-slate-200 rounded-2xl shadow-sm outline-none focus:border-blue-500 focus:ring-4 focus:ring-blue-50 transition-all text-sm font-semibold text-slate-700" />
        </div>

        <button @click="mostrarFormularioNuevo = true"
          class="bg-cyan-900 hover:bg-cyan-500 text-white font-black text-xs tracking-widest uppercase px-8 py-4 rounded-2xl shadow-lg shadow-blue-200 transition-all active:scale-95 flex items-center gap-2">
          <span class="text-lg">+</span> Nuevo Doctor
        </button>
      </div>

      <!-- LOADING -->
      <div v-if="cargando" class="py-24 text-center text-slate-400 font-bold">Cargando doctores...</div>

      <!-- TABLA DESKTOP -->
      <div v-else class="hidden md:block bg-white rounded-[2.5rem] shadow-xl shadow-slate-200/50 border border-slate-100 overflow-hidden">
        <div class="overflow-x-auto">
          <table class="w-full text-left border-collapse">
            <thead>
              <tr class="bg-slate-50/50 border-b border-slate-200 text-slate-500">
                <th class="px-8 py-6 text-[11px] font-black uppercase tracking-[0.2em]">Doctor</th>
                <th class="px-8 py-6 text-[11px] font-black uppercase tracking-[0.2em]">DUI</th>
                <th class="px-8 py-6 text-[11px] font-black uppercase tracking-[0.2em]">Estado</th>
                <th class="px-8 py-6 text-[11px] font-black uppercase tracking-[0.2em] text-center">Info</th>
                <th class="px-8 py-6 text-[11px] font-black uppercase tracking-[0.2em] text-center">Disponibilidad</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-slate-50">
              <tr v-for="doctor in doctoresFiltrados" :key="doctor.id" class="hover:bg-slate-100 transition-all group">
                
                <td class="px-8 py-6">
                  <div class="flex items-center gap-4">
                    <div class="w-12 h-12 shrink-0 rounded-2xl bg-gradient-to-br from-cyan-700 to-cyan-800 flex items-center justify-center font-bold text-white group-hover:from-slate-800 group-hover:to-blue-700 group-hover:text-white transition-all shadow-sm">
                      {{ doctor.nombre?.[0] }}{{ doctor.apellido?.[0] }}
                    </div>
                    <div>
                      <p class="font-bold text-slate-800 text-lg leading-tight">{{ doctor.nombre }} {{ doctor.apellido }}</p>
                      <p class="text-xs text-slate-400 font-bold uppercase tracking-tighter mt-1">{{ doctor.email }}</p>
                    </div>
                  </div>
                </td>

                <td class="px-8 py-6">
                  <span class="font-mono text-base font-bold text-slate-600 bg-slate-100/80 px-4 py-2 rounded-xl">
                    {{ doctor.dui }}
                  </span>
                </td>

                <td class="px-8 py-6">
                  <button @click="toggleEstado(doctor)"
                    :class="doctor.activo ? 'bg-emerald-200 text-emerald-900 border-emerald-200' : 'bg-amber-100 text-amber-700 border-amber-200'"
                    class="px-4 py-1.5 rounded-full text-[10px] font-black uppercase tracking-widest shadow-sm border hover:scale-105 transition-transform">
                    {{ doctor.activo ? 'Activo' : 'Inactivo' }}
                  </button>
                </td>

                <td class="px-8 py-6 text-center">
                  <button @click="verInformacion(doctor)"
                    class="h-11 px-5 bg-cyan-800 hover:bg-cyan-600 border-2 border-slate-100 text-white rounded-2xl text-xs font-black hover:border-blue-500 hover:text-blue-600 transition-all">
                    INFO
                  </button>
                </td>

                <td class="px-8 py-6 text-center">
                  <button @click="verDiasDisponibles(doctor)"
                    class="h-11 px-5 bg-cyan-800 text-white rounded-2xl text-xs font-black hover:bg-cyan-600 transition-all shadow-md">
                    DÍAS DISPONIBLES
                  </button>
                </td>

              </tr>
            </tbody>
          </table>
        </div>
      </div>

      <!-- CARDS MOBILE -->
      <div v-if="!cargando" class="md:hidden space-y-4">
        <div v-for="doctor in doctoresFiltrados" :key="doctor.id"
          class="bg-white p-6 rounded-[2rem] shadow-md border border-slate-100 flex flex-col gap-4">
          <div class="flex items-center gap-4">
            <div class="w-14 h-14 rounded-2xl bg-blue-600 flex items-center justify-center font-bold text-white shadow-lg">
              {{ doctor.nombre?.[0] }}{{ doctor.apellido?.[0] }}
            </div>
            <div class="flex-1">
              <div class="flex justify-between items-start">
                <p class="font-black text-slate-800 text-xl leading-tight">{{ doctor.nombre }}<br>{{ doctor.apellido }}</p>
                <span :class="doctor.activo ? 'bg-emerald-100 text-emerald-700' : 'bg-amber-100 text-amber-700'"
                  class="px-3 py-1 rounded-full text-[9px] font-black uppercase tracking-widest border border-white">
                  {{ doctor.activo ? 'Activo' : 'Inactivo' }}
                </span>
              </div>
              <p class="font-mono text-sm font-bold text-slate-500 mt-2">{{ doctor.dui }}</p>
            </div>
          </div>
          <div class="grid grid-cols-2 gap-2 mt-2">
            <button @click="verInformacion(doctor)" class="py-4 bg-slate-50 text-slate-600 rounded-xl text-[10px] font-black uppercase tracking-widest border border-slate-100">Info</button>
            <button @click="verDiasDisponibles(doctor)" class="py-4 bg-blue-50 text-blue-600 rounded-xl text-[10px] font-black uppercase tracking-widest border border-blue-100">Días</button>
            <button @click="toggleEstado(doctor)"
              :class="doctor.activo ? 'bg-amber-50 text-amber-600 border-amber-100' : 'bg-emerald-50 text-emerald-600 border-emerald-100'"
              class="col-span-2 py-4 rounded-xl text-[10px] font-black uppercase tracking-widest border transition-colors">
              {{ doctor.activo ? 'Inactivar Doctor' : 'Activar Doctor' }}
            </button>
          </div>
        </div>
      </div>

      <!-- VACÍO -->
      <div v-if="!cargando && doctoresFiltrados.length === 0" class="p-12 md:p-24 text-center">
        <div class="text-6xl mb-4 opacity-10">👨‍⚕️</div>
        <p class="text-slate-400 font-bold">No se encontraron doctores con ese criterio.</p>
      </div>

    </div>

    <!-- ======================== -->
    <!-- MODAL: NUEVO DOCTOR -->
    <!-- ======================== -->
    <div v-if="mostrarFormularioNuevo"
      class="fixed inset-0 z-[9999] flex items-center justify-center bg-black/50 backdrop-blur-sm p-4"
      @click.self="mostrarFormularioNuevo = false">
      <div class="bg-white rounded-[2.5rem] shadow-2xl w-full max-w-lg p-8 overflow-y-auto max-h-[90vh]">
        
        <h2 class="text-2xl font-black text-slate-800 mb-6">Registrar Nuevo Doctor</h2>

        <div class="space-y-4">
          <div class="grid grid-cols-2 gap-4">
            <div>
              <label class="text-xs font-black text-slate-400 uppercase mb-1 block">Nombre</label>
              <input v-model="nuevoDoctor.nombre" type="text" class="w-full px-4 py-3 bg-slate-50 border-2 border-transparent focus:border-blue-500 rounded-2xl outline-none font-bold text-slate-700" />
            </div>
            <div>
              <label class="text-xs font-black text-slate-400 uppercase mb-1 block">Apellido</label>
              <input v-model="nuevoDoctor.apellido" type="text" class="w-full px-4 py-3 bg-slate-50 border-2 border-transparent focus:border-blue-500 rounded-2xl outline-none font-bold text-slate-700" />
            </div>
          </div>

          <div>
            <label class="text-xs font-black text-slate-400 uppercase mb-1 block">Correo Electrónico</label>
            <input v-model="nuevoDoctor.email" type="email" class="w-full px-4 py-3 bg-slate-50 border-2 border-transparent focus:border-blue-500 rounded-2xl outline-none font-bold text-slate-700" />
          </div>



        <!-- Contraseña Temporal -->
          <div>
            <label class="text-xs font-black text-slate-400 uppercase mb-1 block">Contraseña Temporal</label>
            
            <!-- ✅ Wrapper relativo para posicionar el botón dentro del input -->
            <div class="relative">
              <input
                v-model="nuevoDoctor.password"
                :type="mostrarPassword ? 'text' : 'password'"
                class="w-full px-4 py-3 pr-12 bg-slate-50 border-2 border-transparent focus:border-blue-500 rounded-2xl outline-none font-bold text-slate-700"
              />
              <button
                type="button"
                @click="mostrarPassword = !mostrarPassword"
                class="absolute right-3 top-1/2 -translate-y-1/2 text-slate-400 hover:text-slate-600 transition-colors p-1"
              >
                <!-- Ojo abierto — contraseña visible -->
                <svg v-if="mostrarPassword" xmlns="http://www.w3.org/2000/svg" class="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
                  <path stroke-linecap="round" stroke-linejoin="round" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z"/>
                  <path stroke-linecap="round" stroke-linejoin="round" d="M2.458 12C3.732 7.943 7.523 5 12 5c4.477 0 8.268 2.943 9.542 7-1.274 4.057-5.065 7-9.542 7-4.477 0-8.268-2.943-9.542-7z"/>
                </svg>
                <!-- Ojo tachado — contraseña oculta -->
                <svg v-else xmlns="http://www.w3.org/2000/svg" class="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
                  <path stroke-linecap="round" stroke-linejoin="round" d="M13.875 18.825A10.05 10.05 0 0112 19c-4.477 0-8.268-2.943-9.542-7a9.97 9.97 0 012.425-4.083M9.88 9.88a3 3 0 104.243 4.243M3 3l18 18"/>
                </svg>
              </button>
            </div>

            <p class="text-[11px] text-slate-400 mt-1 font-semibold">
              Mínimo 8 caracteres, un número y un símbolo (ej: Admin@123)
            </p>
          </div>



          <div class="grid grid-cols-2 gap-4">
            <div>
              <label class="text-xs font-black text-slate-400 uppercase mb-1 block">DUI</label>
              <input v-model="nuevoDoctor.dui" type="text" placeholder="00000000-0" class="w-full px-4 py-3 bg-slate-50 border-2 border-transparent focus:border-blue-500 rounded-2xl outline-none font-bold text-slate-700" />
            </div>


            <div>
              <label class="text-xs font-black text-slate-400 uppercase mb-1 block">Fecha de Nacimiento</label>
              <input v-model="nuevoDoctor.fechaNacimiento" type="date"
                class="w-full px-4 py-3 bg-slate-50 border-2 border-transparent focus:border-blue-500 rounded-2xl outline-none font-bold text-slate-700" />
            </div>




          <div>
            <label class="text-xs font-black text-slate-400 uppercase mb-1 block">Teléfono</label>
            <input v-model="nuevoDoctor.telefono" type="text" placeholder="78781234"
              class="w-full px-4 py-3 bg-slate-50 border-2 border-transparent focus:border-blue-500 rounded-2xl outline-none font-bold text-slate-700" />
            <p class="text-[11px] text-slate-400 mt-1 font-semibold">
              8 dígitos, sin guiones (ej: 78781234)
            </p>
          </div>

    




          </div>
          <div>
            <label class="text-xs font-black text-slate-400 uppercase mb-1 block">Género</label>
            <select v-model="nuevoDoctor.genero" class="w-full px-4 py-3 bg-slate-50 border-2 border-transparent focus:border-blue-500 rounded-2xl outline-none font-bold text-slate-700">
              <option value="">-- Seleccionar --</option>
              <option value="Masculino">Masculino</option>
              <option value="Femenino">Femenino</option>
            </select>
          </div>

          <div>
            <label class="text-xs font-black text-slate-400 uppercase mb-1 block">Dirección</label>
            <input v-model="nuevoDoctor.direccion" type="text" class="w-full px-4 py-3 bg-slate-50 border-2 border-transparent focus:border-blue-500 rounded-2xl outline-none font-bold text-slate-700" />
          </div>

          <!-- El rol es fijo: Doctor — no se muestra al usuario -->
          <p v-if="errorFormulario" class="text-red-500 text-sm font-bold">⚠️ {{ errorFormulario }}</p>
        </div>

        <div class="flex gap-4 mt-8">
          <button @click="mostrarFormularioNuevo = false"
            class="flex-1 py-4 bg-slate-100 text-slate-500 rounded-2xl font-black text-xs uppercase tracking-widest hover:bg-slate-200 transition-all">
            Cancelar
          </button>
          <button @click="guardarNuevoDoctor" :disabled="guardandoDoctor"
            class="flex-[2] py-4 bg-blue-600 hover:bg-blue-700 disabled:opacity-60 text-white rounded-2xl font-black text-xs uppercase tracking-widest transition-all">
            {{ guardandoDoctor ? 'Registrando...' : 'Registrar Doctor' }}
          </button>
        </div>
      </div>
    </div>

        <!-- MODALES -->
        <diasDisponibles
        v-if="abrirDias"
        :doctor="doctorSeleccionado"
        @cerrar="abrirDias = false"
      />
    <configuracionCuenta 
      v-if="informacionDoctor" 
      :usuarioData="doctorSeleccionado" 
      :rolSesion="'doctor'"
      :modoAdmin = "true"
      @cerrar="informacionDoctor = false" 
    />

  </div>
</template>