<script setup>
defineOptions({ name: 'DoctoresView' })

import { ref, computed, onMounted } from 'vue'
import { getProfileIcon } from '@/utils/getProfileIcon'
import diasDisponibles from '../components/diasDisponibles.vue'
import configuracionCuenta from './configuracionCuenta.vue'
import { getDoctoresAdmin, toggleEstadoDoctor, createDoctor } from '@/services/api'
import {
  UserPlusIcon,
  UserGroupIcon,
  ExclamationTriangleIcon,
  XMarkIcon,
  MagnifyingGlassIcon,
  EyeIcon,
  CalendarDaysIcon
} from '@heroicons/vue/24/outline'

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

// Formulario nuevo doctor
const nuevoDoctor = ref({
  nombre: '',
  apellido: '',
  email: '',
  password: '',
  telefono: '',
  dui: '',
  genero: '',
  direccion: '',
  fechaNacimiento: '',
  jvpm: '',
  especialidad: ''
})
const guardandoDoctor = ref(false)

const errorFormulario = ref('')

const toast = ref({ visible: false, mensaje: '', tipo: 'success' })
let toastTimer = null

const mostrarToast = (mensaje, tipo = 'success') => {
  if (toastTimer) clearTimeout(toastTimer)
  toast.value = { visible: true, mensaje, tipo }
  toastTimer = setTimeout(() => { toast.value.visible = false }, 3500)
}

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
      avatarIcono: d.avatarIcono || d.AvatarIcono || null,
      activo: d.activo ?? true,
      raw: d,
      especialidad: d.especialidad || 'No asignada',
      jvpm: d.jvpm || 'No asignado'

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
    mostrarToast('No se pudo cambiar el estado del doctor.', 'error')
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
// CREAR NUEVO DOCTOR —
const guardarNuevoDoctor = async () => {
  errorFormulario.value = ''

  // Validación local antes de llamar al API
  const { nombre, apellido, email, password, telefono, dui, genero, direccion, fechaNacimiento } = nuevoDoctor.value

  if (!nombre.trim() || !apellido.trim()) {
    errorFormulario.value = 'El nombre y apellido son obligatorios.'
    return
  }
  if (!email.trim() || !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) {
    errorFormulario.value = 'Ingresa un correo electrónico válido.'
    return
  }
  if (!password || password.length < 8) {
    errorFormulario.value = 'La contraseña debe tener al menos 8 caracteres.'
    return
  }
  if (!dui.trim()) {
    errorFormulario.value = 'El DUI es obligatorio.'
    return
  }
  if (!telefono.trim() || !/^\d{8}$/.test(telefono)) {
    errorFormulario.value = 'El teléfono debe tener exactamente 8 dígitos.'
    return
  }
  if (!genero) {
    errorFormulario.value = 'Selecciona un género.'
    return
  }
  if (!direccion.trim()) {
    errorFormulario.value = 'La dirección es obligatoria.'
    return
  }
  if (!fechaNacimiento) {
    errorFormulario.value = 'La fecha de nacimiento es obligatoria.'
    return
  }

  // Validar edad mínima (18 años)
  const hoy = new Date()
  const nacimiento = new Date(fechaNacimiento)
  const edad = hoy.getFullYear() - nacimiento.getFullYear()
  const cumplioEsteAnio =
    hoy.getMonth() > nacimiento.getMonth() ||
    (hoy.getMonth() === nacimiento.getMonth() && hoy.getDate() >= nacimiento.getDate())
  if (edad < 18 || (edad === 18 && !cumplioEsteAnio)) {
    errorFormulario.value = 'El doctor debe ser mayor de 18 años.'
    return
  }

  guardandoDoctor.value = true

  try {
    await createDoctor({
      Nombre:          nombre,
      Apellido:        apellido,
      Email:           email,
      Password:        password,
      Telefono:        telefono,
      DUI:             dui,
      Genero:          genero,
      Direccion:       direccion,
      FechaNacimiento: fechaNacimiento,
      JVPM:            nuevoDoctor.value.jvpm,
      Especialidad:    nuevoDoctor.value.especialidad
    })

    mostrarToast('Doctor registrado exitosamente.', 'success')
    mostrarFormularioNuevo.value = false
    nuevoDoctor.value = {
      nombre: '', apellido: '', email: '', password: '',
      telefono: '', dui: '', genero: '', direccion: '',
      fechaNacimiento: '', jvpm: '', especialidad: ''
    }
    await cargarDoctores()

  } catch (e) {
    const data = e.response?.data
    if (data?.errors) {
      errorFormulario.value = Object.values(data.errors).flat().join(' | ')
    } else {
      errorFormulario.value = data?.message || 'Error al registrar el doctor.'
    }
  } finally {
    // CRÍTICO: siempre libera el botón
    guardandoDoctor.value = false
  }
}
</script>



<template>
  <div class="min-h-screen bg-slate-300 dark:bg-slate-700 p-3 md:p-10 flex flex-col items-center transition-colors">

    <div class="w-full max-w-[95%] lg:w-[90%] mb-4">
      <button @click="emit('cerrar')"
        class="text-[10px] md:text-sm font-bold uppercase tracking-widest bg-white border border-slate-200 hover:bg-slate-50 cursor-pointer text-slate-600 px-4 py-2.5 md:px-5 md:py-3 rounded-xl md:rounded-2xl transition-all flex items-center gap-2 shadow-sm">
        ← <span>Regresar</span>
      </button>
    </div>

    <div class="w-full max-w-[95%] lg:w-[90%]">

      <!-- HEADER -->
      <div class="mb-6 md:mb-8 bg-slate-800 rounded-2xl md:rounded-3xl px-5 py-5 md:px-8 md:py-6 flex flex-col md:flex-row md:items-center justify-between gap-4 md:gap-6">
        <div>
          <h1 class="text-xl md:text-3xl font-black text-white tracking-tight leading-tight">Gestión de Doctores</h1>
          <p class="text-slate-400 font-medium text-sm md:text-base">Panel de control administrativo</p>
        </div>

        <!-- Bucador -->
        <div class="flex flex-col sm:flex-row gap-3 w-full md:w-auto">
        <div class="relative w-full md:w-80 lg:w-96 group">

          <div class="absolute inset-y-0 left-0 pl-4 flex items-center pointer-events-none">
            <MagnifyingGlassIcon class="h-5 w-5 text-slate-400" />
          </div>

          <input
            v-model="filtroQuery"
            type="text"
            placeholder="Buscar por DUI..."
            class="w-full pl-11 pr-4 py-3 bg-slate-200 dark:bg-slate-800 border-2 border-transparent focus:border-teal-300 rounded-xl outline-none transition-all text-sm font-semibold text-slate-700 dark:text-slate-100 placeholder:text-slate-400"
          />
        </div>


          <button @click="mostrarFormularioNuevo = true"
            class="bg-teal-300 hover:bg-teal-200 text-slate-900 font-black text-[10px] md:text-xs tracking-widest uppercase px-6 py-3 rounded-xl shadow-lg transition-all active:scale-95 flex items-center justify-center gap-2 cursor-pointer">
            <UserPlusIcon class="w-4 h-4" /> Nuevo Doctor
          </button>
        </div>
      </div>

      <div v-if="cargando" class="py-20 text-center text-slate-400 font-bold">Cargando doctores...</div>

      <!-- TABLA ESCRITORIO -->
      <div v-else class="hidden md:block bg-white dark:bg-slate-900 rounded-[1.5rem] shadow-xl overflow-hidden transition-colors">
        <div class="overflow-x-auto">
          <table class="w-full text-left border-collapse">
            <thead>
              <tr class="bg-slate-950 text-white">
                <th class="px-8 py-5 text-[11px] font-black uppercase tracking-[0.2em]">Doctor</th>
                <th class="px-8 py-5 text-[11px] font-black uppercase tracking-[0.2em]">IDENTIFICACIÓN</th>
                <th class="px-8 py-5 text-[11px] font-black uppercase tracking-[0.2em]">Estado</th>
                <th class="px-8 py-5 text-[11px] font-black uppercase tracking-[0.2em] text-center">Perfil</th>
                <th class="px-8 py-5 text-[11px] font-black uppercase tracking-[0.2em] text-center">Disponibilidad</th>
              </tr>
            </thead>
            <tbody class="divide-y bg-linear-to-r from-[#0f2040] to-[#10284f]">
              <tr v-for="doctor in doctoresFiltrados" :key="doctor.id" class="hover:bg-slate-850 transition-all group">
                <td class="px-8 py-5">
                  <div class="flex items-center gap-4">

                    <div class="w-11 h-11 shrink-0 rounded-xl overflow-hidden bg-slate-800 border border-cyan-400/20 ring-2 ring-cyan-400/10 shadow-sm">
                    <img
                      v-if="doctor.avatarIcono"
                      :src="getProfileIcon(doctor.avatarIcono)"
                      alt="Avatar doctor"
                      class="w-full h-full object-cover"
                    />

                    <div
                      v-else
                      class="w-full h-full bg-cyan-400 flex items-center justify-center font-bold text-white text-sm"
                    >
                      {{ doctor.nombre?.[0] }}{{ doctor.apellido?.[0] }}
                    </div>
                  </div>

                    <div>
                      <p class="font-bold text-white text-base leading-tight">{{ doctor.nombre }} {{ doctor.apellido }}</p>
                      <p class="text-[10px] text-slate-400 font-bold mt-0.5">{{ doctor.email }}</p>
                    </div>
                  </div>
                </td>
                <td class="px-8 py-5">
                  <span class="font-mono text-sm font-bold text-slate-900 bg-sky-200 px-3 py-1.5 rounded-lg">{{ doctor.dui }}</span>
                </td>
                <td class="px-8 py-5">
                  <button @click="toggleEstado(doctor)"
                    :class="doctor.activo
                      ? 'bg-green-400 text-emerald-700 border-emerald-200 cursor-pointer'
                      : 'bg-red-300 text-amber-700 border-amber-200 cursor-pointer'"
                    class="px-3 py-1.5 rounded-full text-[10px] font-black uppercase tracking-widest border hover:scale-105 transition-all">
                    {{ doctor.activo ? 'Activo' : 'Inactivo' }}
                  </button>
                </td>
                <td class="px-8 py-5 text-center">
                  <button
                    @click="verInformacion(doctor)"
                    class="h-9 px-5 bg-sky-200 hover:bg-sky-400 text-slate-950 rounded-xl text-xs font-black transition-all uppercase tracking-widest cursor-pointer inline-flex items-center justify-center gap-2"
                  >
                    <EyeIcon class="w-4 h-4" />
                    Info
                  </button>
                </td>
                <td class="px-8 py-5 text-center">
                  <button
                    @click="verDiasDisponibles(doctor)"
                    class="h-9 px-5 bg-sky-200 hover:bg-sky-400 text-slate-950 rounded-xl text-xs font-black transition-all uppercase tracking-widest cursor-pointer inline-flex items-center justify-center gap-2"
                  >
                    <CalendarDaysIcon class="w-4 h-4" />
                    Horarios
                  </button>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

      <!-- CARDS MÓVIL -->
      <div v-if="!cargando" class="md:hidden space-y-3">
        <div v-for="doctor in doctoresFiltrados" :key="doctor.id"
          class="bg-white dark:bg-[#10284f] p-4 rounded-2xl shadow-sm border border-slate-100 dark:border-cyan-500/10 ">
          <div class="flex items-center gap-3">

            <div class="w-12 h-12 shrink-0 rounded-xl overflow-hidden bg-slate-800 border border-cyan-400/20 ring-2 ring-cyan-400/10">
            <img
              v-if="doctor.avatarIcono"
              :src="getProfileIcon(doctor.avatarIcono)"
              alt="Avatar doctor"
              class="w-full h-full object-cover"
            />

            <div
              v-else
              class="w-full h-full bg-cyan-500 flex items-center justify-center font-bold text-white text-sm"
            >
              {{ doctor.nombre?.[0] }}{{ doctor.apellido?.[0] }}
            </div>
          </div>

            <div class="flex-1 min-w-0">
              <div class="flex justify-between items-start gap-2">
                <p class="font-black text-slate-800 text-base leading-tight truncate">{{ doctor.nombre }} {{ doctor.apellido }}</p>
                <span :class="doctor.activo
                  ? 'bg-emerald-100 text-emerald-700'
                  : 'bg-amber-100 text-amber-700'"
                  class="shrink-0 px-2.5 py-1 rounded-full text-[8px] font-black uppercase tracking-widest">
                  {{ doctor.activo ? 'Activo' : 'Inactivo' }}
                </span>
              </div>
              <p class="text-[10px] text-slate-400 font-bold truncate mt-0.5">{{ doctor.email }}</p>
            </div>
          </div>

          <div class="flex items-center justify-between bg-sky-50 dark:bg-slate-800 px-3 py-2 rounded-xl">
            <span class="text-[9px] font-black text-slate-400 uppercase">DUI</span>
            <span class="font-mono text-xs font-bold text-slate-600 dark:text-slate-200">{{ doctor.dui }}</span>
          </div>

          <div class="grid grid-cols-2 gap-2">
            <button @click="verInformacion(doctor)"
              class="py-3 bg-slate-800 text-white rounded-xl text-[10px] font-black uppercase tracking-widest hover:bg-cyan-700 transition-all text-center">
              <EyeIcon class="w-4 h-4" />
              Info
            </button>
            <button
              @click="verDiasDisponibles(doctor)"
              class="py-3 bg-cyan-500 text-white rounded-xl text-[10px] font-black uppercase tracking-widest hover:bg-cyan-700 transition-all flex items-center justify-center gap-2"
            >
              <CalendarDaysIcon class="w-4 h-4" />
              Horarios
            </button>
          <button @click="toggleEstado(doctor)"
              :class="doctor.activo
                ? 'bg-amber-50 text-amber-600 border border-amber-100'
                : 'bg-emerald-50 text-emerald-600 border border-emerald-100'"
              class="col-span-2 py-3 rounded-xl text-[9px] font-black uppercase tracking-widest transition-colors">
              {{ doctor.activo ? 'Inactivar Doctor' : 'Activar Doctor' }}
            </button>
          </div>
        </div>
      </div>

      <!-- VACÍO -->
      <div v-if="!cargando && doctoresFiltrados.length === 0" class="py-16 text-center">
       <div class="flex justify-center mb-4 opacity-20">
        <UserGroupIcon class="w-14 h-14 text-slate-400" />
      </div>
        <p class="text-slate-500 dark:text-slate-400 font-bold text-sm">No se encontraron doctores con ese criterio.</p>
      </div>

    </div>

    <!-- MODAL NUEVO DOCTOR -->
      <div v-if="mostrarFormularioNuevo"
        class="fixed inset-0 z-[9999] flex items-center justify-center bg-slate-950/80 backdrop-blur-sm p-3 md:p-4"
        @click.self="mostrarFormularioNuevo = false">
        <div class="bg-linear-to-r from-[#0f2040] to-[#10284f] w-full max-w-lg max-h-[95vh] overflow-y-auto rounded-2xl md:rounded-3xl shadow-2xl border border-cyan-500/20 flex flex-col custom-scrollbar">

          <!-- Header modal -->
          <div class="slate-700 px-6 py-5 rounded-t-2xl md:rounded-t-3xl flex justify-between items-center shrink-0 border-b border-cyan-500/20">
            <div>
              <h2 class="text-lg md:text-xl font-black text-white">Nuevo Doctor</h2>
              <p class="text-cyan-400 text-[10px] uppercase tracking-widest font-medium mt-0.5">Registro de médico</p>
            </div>
            <button @click="mostrarFormularioNuevo = false"
              class="text-slate-400 hover:text-white bg-white/10 hover:bg-red-500 w-9 h-9 rounded-full flex items-center justify-center transition-colors text-sm font-bold border border-white/10">
             <XMarkIcon class="w-4 h-4" />
            </button>
          </div>

          <div class="p-5 md:p-8 space-y-5 bg-linear-to-r from-[#0f2040] to-[#10284f]">

            <!-- Sección datos personales -->
            <div class="space-y-3">
              <div class="flex items-center gap-2">
                <svg xmlns="http://www.w3.org/2000/svg" class="h-3.5 w-3.5 text-cyan-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
                </svg>
                <span class="text-[10px] font-black text-cyan-400 uppercase tracking-[0.15em]">Datos personales</span>
              </div>

              <div class="bg-cyan-500/5 border border-cyan-500/15 rounded-xl p-4 space-y-3">
                <div class="grid grid-cols-1 sm:grid-cols-2 gap-3">
                  <div>
                    <label class="text-[9px] font-black text-slate-400 uppercase tracking-wider mb-1.5 block">Nombre</label>
                    <input v-model="nuevoDoctor.nombre" type="text"
                      class="w-full px-3 py-2.5 bg-[#0a0f1e] border border-cyan-500/20 focus:border-cyan-400 rounded-xl outline-none font-bold text-white text-sm transition-colors focus:ring-1 focus:ring-cyan-500/20 placeholder:text-slate-600" />
                  </div>
                  <div>
                    <label class="text-[9px] font-black text-slate-400 uppercase tracking-wider mb-1.5 block">Apellido</label>
                    <input v-model="nuevoDoctor.apellido" type="text"
                      class="w-full px-3 py-2.5 bg-[#0a0f1e] border border-cyan-500/20 focus:border-cyan-400 rounded-xl outline-none font-bold text-white text-sm transition-colors focus:ring-1 focus:ring-cyan-500/20 placeholder:text-slate-600" />
                  </div>
                </div>

                <div class="grid grid-cols-1 sm:grid-cols-2 gap-3">
                  <div>
                    <label class="text-[9px] font-black text-slate-400 uppercase tracking-wider mb-1.5 block">DUI</label>
                    <input v-model="nuevoDoctor.dui" type="text" placeholder="00000000-0"
                      class="w-full px-3 py-2.5 bg-[#0a0f1e] border border-cyan-500/20 focus:border-cyan-400 rounded-xl outline-none font-bold text-white text-sm font-mono transition-colors focus:ring-1 focus:ring-cyan-500/20 placeholder:text-slate-600" />
                  </div>
                  <div>
                    <label class="text-[9px] font-black text-slate-400 uppercase tracking-wider mb-1.5 block">Nacimiento</label>
                    <input v-model="nuevoDoctor.fechaNacimiento" type="date"
                      class="w-full px-3 py-2.5 bg-[#0a0f1e] border border-cyan-500/20 focus:border-cyan-400 rounded-xl outline-none font-bold text-white text-sm transition-colors focus:ring-1 focus:ring-cyan-500/20" />
                  </div>
                </div>

                <div class="grid grid-cols-1 sm:grid-cols-2 gap-3">
                  <div>
                    <label class="text-[9px] font-black text-slate-400 uppercase tracking-wider mb-1.5 block">
                      Teléfono <span class="normal-case text-slate-500">(8 dígitos)</span>
                    </label>
                    <input v-model="nuevoDoctor.telefono" type="text" placeholder="78781234"
                      class="w-full px-3 py-2.5 bg-[#0a0f1e] border border-cyan-500/20 focus:border-cyan-400 rounded-xl outline-none font-bold text-white text-sm transition-colors focus:ring-1 focus:ring-cyan-500/20 placeholder:text-slate-600" />
                  </div>
                  <div>
                    <label class="text-[9px] font-black text-slate-400 uppercase tracking-wider mb-1.5 block">Género</label>
                    <select v-model="nuevoDoctor.genero"
                      class="w-full px-3 py-2.5 bg-[#0a0f1e] border border-cyan-500/20 focus:border-cyan-400 rounded-xl outline-none font-bold text-white text-sm transition-colors focus:ring-1 focus:ring-cyan-500/20">
                      <option value="" class="bg-[#0a0f1e]">-- Seleccionar --</option>
                      <option value="Masculino" class="bg-[#0a0f1e]">Masculino</option>
                      <option value="Femenino" class="bg-[#0a0f1e]">Femenino</option>
                    </select>
                  </div>
                </div>
              </div>
            </div>

            <!-- Sección profesional -->
            <div class="space-y-3">
              <div class="flex items-center gap-2">
                <svg xmlns="http://www.w3.org/2000/svg" class="h-3.5 w-3.5 text-cyan-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
                </svg>
                <span class="text-[10px] font-black text-cyan-400 uppercase tracking-[0.15em]">Información profesional</span>
              </div>

              <div class="bg-cyan-500/5 border border-cyan-500/15 rounded-xl p-4 space-y-3">
                <div class="grid grid-cols-1 sm:grid-cols-2 gap-3">
                  <div>
                    <label class="text-[9px] font-black text-slate-400 uppercase tracking-wider mb-1.5 block">JVPM</label>
                    <input v-model="nuevoDoctor.jvpm" type="text"
                      class="w-full px-3 py-2.5 bg-[#0a0f1e] border border-cyan-500/20 focus:border-cyan-400 rounded-xl outline-none font-bold text-white text-sm transition-colors focus:ring-1 focus:ring-cyan-500/20" />
                  </div>
                  <div>
                    <label class="text-[9px] font-black text-slate-400 uppercase tracking-wider mb-1.5 block">Especialidad</label>
                    <input v-model="nuevoDoctor.especialidad" type="text"
                      class="w-full px-3 py-2.5 bg-[#0a0f1e] border border-cyan-500/20 focus:border-cyan-400 rounded-xl outline-none font-bold text-white text-sm transition-colors focus:ring-1 focus:ring-cyan-500/20" />
                  </div>
                </div>

                <div>
                  <label class="text-[9px] font-black text-slate-400 uppercase tracking-wider mb-1.5 block">Dirección</label>
                  <input v-model="nuevoDoctor.direccion" type="text"
                    class="w-full px-3 py-2.5 bg-[#0a0f1e] border border-cyan-500/20 focus:border-cyan-400 rounded-xl outline-none font-bold text-white text-sm transition-colors focus:ring-1 focus:ring-cyan-500/20" />
                </div>
              </div>
            </div>

            <!-- Sección acceso -->
            <div class="space-y-3">
              <div class="flex items-center gap-2">
                <svg xmlns="http://www.w3.org/2000/svg" class="h-3.5 w-3.5 text-slate-500" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z" />
                </svg>
                <span class="text-[10px] font-black text-slate-500 uppercase tracking-[0.15em]">Acceso al sistema</span>
              </div>

              <div class="bg-slate-500/5 border border-slate-500/15 rounded-xl p-4 space-y-3">
                <div>
                  <label class="text-[9px] font-black text-slate-400 uppercase tracking-wider mb-1.5 block">Correo Electrónico</label>
                  <input v-model="nuevoDoctor.email" type="email"
                    class="w-full px-3 py-2.5 bg-[#0a0f1e] border border-slate-700 focus:border-cyan-400 rounded-xl outline-none font-bold text-white text-sm transition-colors focus:ring-1 focus:ring-cyan-500/20 placeholder:text-slate-600" placeholder="ejemplo@gmail.com" />
                </div>

                <div>
                  <label class="text-[9px] font-black text-slate-400 uppercase tracking-wider mb-1.5 block">Contraseña Temporal</label>
                  <div class="relative">
                    <input
                      v-model="nuevoDoctor.password"
                      :type="mostrarPassword ? 'text' : 'password'"
                      placeholder="••••••••"
                      class="w-full px-3 py-2.5 pr-11 bg-[#0a0f1e] border border-slate-700 focus:border-cyan-400 rounded-xl outline-none font-bold text-white text-sm transition-colors focus:ring-1 focus:ring-cyan-500/20 placeholder:text-slate-600"
                    />
                    <button type="button" @click="mostrarPassword = !mostrarPassword"
                      class="absolute right-3 top-1/2 -translate-y-1/2 text-slate-400 hover:text-cyan-400 p-1 transition-colors">
                      <svg v-if="mostrarPassword" xmlns="http://www.w3.org/2000/svg" class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2"><path stroke-linecap="round" stroke-linejoin="round" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z"/><path stroke-linecap="round" stroke-linejoin="round" d="M2.458 12C3.732 7.943 7.523 5 12 5c4.477 0 8.268 2.943 9.542 7-1.274 4.057-5.065 7-9.542 7-4.477 0-8.268-2.943-9.542-7z"/></svg>
                      <svg v-else xmlns="http://www.w3.org/2000/svg" class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2"><path stroke-linecap="round" stroke-linejoin="round" d="M13.875 18.825A10.05 10.05 0 0112 19c-4.478 0-8.268-2.943-9.542-7a9.97 9.97 0 012.425-4.083M9.88 9.88a3 3 0 104.243 4.243M3 3l18 18"/></svg>
                    </button>
                  </div>
                  <p class="text-[9px] text-slate-500 mt-1.5 font-semibold">Mínimo 8 caracteres, un número y un símbolo.</p>
                </div>
              </div>
            </div>

            <!-- Error -->
            <div
              v-if="errorFormulario"
              class="flex items-start gap-2 text-red-400 text-[10px] md:text-sm font-bold bg-red-500/10 border border-red-500/20 rounded-xl px-4 py-3"
            >
              <ExclamationTriangleIcon class="w-4 h-4 shrink-0 mt-0.5" />
              <span>
                {{ errorFormulario }}
              </span>
            </div>

            <!-- Botones -->
            <div class="flex flex-col-reverse sm:flex-row gap-3 pt-1">
              <button @click="mostrarFormularioNuevo = false"
                class="w-full sm:flex-1 py-3.5 bg-slate-600 hover:bg-red-400 text-slate-400 hover:text-white rounded-xl font-black text-[10px] uppercase tracking-widest transition-colors border border-slate-700">
                Cancelar
              </button>
              <button @click="guardarNuevoDoctor" :disabled="guardandoDoctor"
                class="w-full sm:flex-[2] py-3.5 bg-cyan-500 hover:bg-cyan-400 text-slate-900 rounded-xl font-black text-[10px] uppercase tracking-widest transition-all disabled:opacity-40 disabled:cursor-not-allowed flex items-center justify-center gap-2">
                <span v-if="!guardandoDoctor">Registrar Doctor</span>
                <span v-else class="flex items-center gap-2">
                  <span class="animate-spin border-2 border-white/30 border-t-white rounded-full h-4 w-4"></span>
                  Registrando...
                </span>
              </button>
            </div>

          </div>
        </div>
      </div>
      <!-- TOAST NOTIFICATION -->
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
        <!-- Ícono -->
        <div
          class="shrink-0 w-8 h-8 rounded-xl flex items-center justify-center"
          :class="toast.tipo === 'success' ? 'bg-emerald-500/20' : 'bg-red-500/20'"
        >
          <svg v-if="toast.tipo === 'success'" xmlns="http://www.w3.org/2000/svg" class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2.5">
            <path stroke-linecap="round" stroke-linejoin="round" d="M5 13l4 4L19 7" />
          </svg>
          <svg v-else xmlns="http://www.w3.org/2000/svg" class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2.5">
            <path stroke-linecap="round" stroke-linejoin="round" d="M6 18L18 6M6 6l12 12" />
          </svg>
        </div>

        <!-- Mensaje -->
        <div class="flex-1">
          <p class="text-[11px] font-black uppercase tracking-widest mb-0.5"
            :class="toast.tipo === 'success' ? 'text-emerald-400' : 'text-red-400'">
            {{ toast.tipo === 'success' ? 'Éxito' : 'Error' }}
          </p>
          <p class="text-xs font-semibold leading-snug">{{ toast.mensaje }}</p>
        </div>

        <!-- Cerrar -->
        <button @click="toast.visible = false"
          class="shrink-0 w-6 h-6 rounded-lg flex items-center justify-center opacity-50 hover:opacity-100 transition-opacity"
          :class="toast.tipo === 'success' ? 'hover:bg-emerald-500/20' : 'hover:bg-red-500/20'">
          <XMarkIcon class="w-3.5 h-3.5" />
        </button>

        <!-- Barra de progreso -->
        <div class="absolute bottom-0 left-0 h-[2px] rounded-full w-full overflow-hidden">
          <div
            class="h-full rounded-full"
            :class="toast.tipo === 'success' ? 'bg-emerald-400' : 'bg-red-400'"
            style="animation: shrink 3.5s linear forwards"
          />
        </div>
      </div>
    </Transition>
    <diasDisponibles v-if="abrirDias" :doctor="doctorSeleccionado" @cerrar="abrirDias = false" />
    <configuracionCuenta v-if="informacionDoctor" :usuarioData="doctorSeleccionado" :rolSesion="'doctor'" :modoAdmin="true" @cerrar="informacionDoctor = false" />

  </div>
</template>

<style scoped>
input[type="date"]::webkit-calendar-picker-indicator {
  filter: invert(1) sepia(1);
  cursor: pointer;
}
.custom-scrollbar::-webkit-scrollbar { width: 5px; }
.custom-scrollbar::-webkit-scrollbar-track { background: #f1f5f9; }
.custom-scrollbar::-webkit-scrollbar-thumb { background: #cbd5e1; border-radius: 10px; }
</style>
