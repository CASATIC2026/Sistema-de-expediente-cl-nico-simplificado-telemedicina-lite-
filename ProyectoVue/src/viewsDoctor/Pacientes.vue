//ESTA ES LA SECCION PACIENTES (LE PERMITE VER BREVE INFORMACION DE SUS PACIENTES E HISTORIAL)

<script setup>
import { ref, computed, onMounted } from 'vue'
import configuracionCuenta from '@/components/configuracionCuenta.vue'
import verHistorial from '@/components/verHistorial.vue'
import {getPacientesAdmin, getPacientesDoctor} from '@/services/api'

// ── Utilidad: leer rol desde el JWT (igual que en el router) ──────────────
const getRolFromToken = () => {
  const token = localStorage.getItem('token')
  if (!token) return ''
  try {
    const payload = JSON.parse(atob(token.split('.')[1]))
    return (
      payload.role ||
      payload.rol ||
      payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] ||
      ''
    ).toLowerCase()
  } catch { return '' }
}

// ── Estado ────────────────────────────────────────────────────────────────
const usuarios    = ref([])
const cargando    = ref(false)
const error       = ref(null)
const filtroQuery = ref('')

// ── Cargar pacientes según rol ────────────────────────────────────────────
const cargarPacientes = async () => {
  cargando.value = true
  error.value    = null
  try {
    const rol  = getRolFromToken()
    let data

    if (rol === 'admin') {
      data = await getPacientesAdmin()   // SI ES ADMIN, TRAE TODOS LOS PACIENTES
    } else {
      data = await getPacientesDoctor()  // SI ES DOCTOR, TRAE SOLO SUS PACIENTES ASIGNADOS    
    }

    usuarios.value = data.map(p => ({
      id:              p.id        || p.Id,
      nombre:          p.nombre    || p.Nombre    || '',
      apellido:        p.apellido  || p.Apellido  || '',
      dui:             p.dui       || p.DUI       || 'Sin DUI',
      edad:            calcularEdad(p.fechaNacimiento || p.FechaNacimiento),
      // --- CAMBIO AQUÍ ---
      // Intentamos leer la fecha desde el backend. Si no existe, ponemos un texto amigable.
      ultimaConsulta:  (p.fechaUltimaConsulta || p.FechaUltimaConsulta) 
        ? new Date(p.fechaUltimaConsulta || p.FechaUltimaConsulta).toLocaleDateString('es-SV') 
        : 'Sin consultas',
      // -------------------

      estado:          (p.activo ?? p.Activo) ? 'Activo' : 'Inactivo',
      email:           p.email     || p.Email     || '',
      telefono:        p.telefono  || p.Telefono  || '',
      fotoUrl:         p.fotoUrl   || p.FotoUrl   || null,
      fechaNacimiento: p.fechaNacimiento || p.FechaNacimiento
    }))
  } catch (e) {
    error.value = 'No se pudieron cargar los pacientes. Intenta de nuevo.'
    console.error(e)
  } finally {
    cargando.value = false
  }
}
// ── Helper: calcular edad desde fecha de nacimiento ──────────────────────
const calcularEdad = (fechaNac) => {
  if (!fechaNac) return '—'
  const hoy  = new Date()
  const nac  = new Date(fechaNac)
  let edad   = hoy.getFullYear() - nac.getFullYear()
  const diff = hoy.getMonth() - nac.getMonth()
  if (diff < 0 || (diff === 0 && hoy.getDate() < nac.getDate())) edad--
  return edad
}

// ── Filtro de búsqueda (sin cambios) ─────────────────────────────────────
const usuariosFiltrados = computed(() => {
  const query = filtroQuery.value.toLowerCase().trim()
  if (!query) return usuarios.value
  return usuarios.value.filter(u =>
    u.dui.includes(query) ||
    u.nombre.toLowerCase().includes(query) ||
    u.apellido.toLowerCase().includes(query)
  )
})

// ── Modales (sin cambios) ─────────────────────────────────────────────────
const accederInformacionPaciente = ref(false)
const accederHistorial           = ref(false)
const pacienteSeleccionado       = ref(null)

const abrirInfo = (paciente) => {
  pacienteSeleccionado.value       = paciente
  accederInformacionPaciente.value = true
}
const abrirHistorial = (paciente) => {
  pacienteSeleccionado.value = paciente
  accederHistorial.value     = true
}

// ── Montar ────────────────────────────────────────────────────────────────
onMounted(cargarPacientes)
</script>

<template>
  <!-- El margen superior (mt) se ajustó ya que no está el botón superior -->
  <div class="min-h-screen bg-slate-50 p-4 md:p-10 flex flex-col items-center font-sans">
    
    <div class="w-full max-w-[95%] lg:w-[90%] mt-4">
      
      <!-- HEADER PROFESIONAL -->
      <div class="mb-10 flex flex-col md:flex-row md:items-end justify-between gap-6">
        <div>
          <div class="flex items-center gap-3 mb-2">
            <span class="bg-blue-600 w-2 h-8 rounded-full"></span>
            <h1 class="text-3xl md:text-4xl font-black text-slate-800 tracking-tight">Gestión de Pacientes</h1>
          </div>
          <p class="text-slate-500 font-bold text-lg">Panel para gestion de pacientes</p>
        </div>


        <!-- Cargando -->
        <div v-if="cargando" class="mt-10 p-20 text-center">
          <div class="w-10 h-10 border-4 border-blue-600 border-t-transparent rounded-full animate-spin mx-auto"></div>
          <p class="text-slate-400 font-bold mt-4">Cargando pacientes...</p>
        </div>

        <!-- Error -->
        <div v-else-if="error" class="mt-10 p-10 text-center bg-red-50 rounded-[2.5rem] border border-red-100">
          <p class="text-red-500 font-bold">{{ error }}</p>
          <button @click="cargarPacientes" class="mt-4 px-6 py-2 bg-red-500 text-white rounded-xl font-bold">
            Reintentar
          </button>
        </div>

        <!-- BUSCADOR ESTILIZADO -->
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
            class="w-full pl-12 pr-4 py-4 bg-gray-100 border-2 border-slate-100 rounded-[1.25rem] shadow-lg outline-none focus:border-blue-500 focus:ring-4 focus:ring-blue-500/10 transition-all text-sm font-bold text-slate-700 placeholder:text-slate-300"
          >
        </div>
      </div>

      <!-- TABLA (ESCRITORIO) -->
      <div class="hidden md:block bg-white rounded-[2.5rem] shadow-2xl shadow-slate-200/60 border border-slate-100 overflow-hidden">
        <table class="w-full text-left border-collapse">
          <thead>
            <tr class="bg-slate-50/80 border-b border-slate-100 text-slate-400">
              <th class="px-8 py-6 text-[10px] font-black uppercase tracking-[0.2em]">Información del Paciente</th>
              <th class="px-8 py-6 text-[10px] font-black uppercase tracking-[0.2em] text-center">Edad</th>
              <th class="px-8 py-6 text-[10px] font-black uppercase tracking-[0.2em]">Última Consulta</th>
              <th class="px-8 py-6 text-[10px] font-black uppercase tracking-[0.2em] text-center">Acciones Médicas</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-slate-50">
            <tr v-for="user in usuariosFiltrados" :key="user.id" class="hover:bg-blue-50/30 transition-all group">
              <td class="px-8 py-6">
                <div class="flex items-center gap-5">
                  <div class="w-14 h-14 shrink-0 rounded-[1.25rem] bg-slate-100 flex items-center justify-center font-black text-slate-500 group-hover:bg-blue-600 group-hover:text-white transition-all shadow-inner uppercase">
                    {{ user.nombre[0] }}{{ user.apellido[0] }}
                  </div>
                  <div>
                    <p class="font-black text-slate-800 text-lg leading-tight">{{ user.nombre }} {{ user.apellido }}</p>
                    <p class="text-[11px] text-slate-400 font-black uppercase mt-1 tracking-widest">DUI: {{ user.dui }}</p>
                  </div>
                </div>
              </td>
              <td class="px-8 py-6 text-center">
                <span class="text-slate-700 font-bold text-base">{{ user.edad }} años</span>
              </td>
              <td class="px-8 py-6">
                <span class="text-slate-800 font-bold text-sm">{{ user.ultimaConsulta }}</span>
              </td>
              <td class="px-8 py-6">
                <div class="flex items-center justify-center gap-3">
                  <button @click="abrirInfo(user)" title="Ver Perfil" class="p-3 bg-white border border-slate-200 text-slate-400 rounded-2xl hover:border-blue-500 hover:text-blue-600 transition-all shadow-sm">
                    <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
                    </svg>
                  </button>
                  <button @click="abrirHistorial(user)" class="flex-1 max-w-[160px] py-3.5 bg-blue-600 text-white rounded-2xl text-[10px] font-black uppercase tracking-widest hover:bg-blue-700 hover:shadow-lg hover:shadow-blue-200 transition-all active:scale-95">
                    Ver Historial
                  </button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- CARDS (MÓVIL) -->
      <div class="md:hidden space-y-4">
        <div v-for="user in usuariosFiltrados" :key="user.id" class="bg-white p-6 rounded-[2.5rem] shadow-lg border border-slate-100 flex flex-col gap-5">
          <div class="flex items-center gap-4">
            <div class="w-14 h-14 rounded-2xl bg-blue-600 flex items-center justify-center font-black text-white shadow-lg text-xl">
              {{ user.nombre[0] }}{{ user.apellido[0] }}
            </div>
            <div>
              <p class="font-black text-slate-800 text-xl leading-tight">{{ user.nombre }} {{ user.apellido }}</p>
              <p class="text-xs font-bold text-slate-400 mt-1">DUI: {{ user.dui }}</p>
            </div>
          </div>
          
          <div class="grid grid-cols-2 gap-4 py-2 border-y border-slate-50">
            <div class="text-center border-r border-slate-50">
              <p class="text-[10px] font-black text-slate-400 uppercase">Edad</p>
              <p class="font-bold text-slate-700">{{ user.edad }} años</p>
            </div>
            <div class="text-center">
              <p class="text-[10px] font-black text-slate-400 uppercase">Última Cita</p>
              <p class="font-bold text-slate-700">{{ user.ultimaConsulta }}</p>
            </div>
          </div>

          <div class="flex gap-3">
            <button @click="abrirInfo(user)" class="flex-1 py-4 bg-slate-50 text-slate-500 rounded-2xl text-[10px] font-black uppercase tracking-widest">Ficha</button>
            <button @click="abrirHistorial(user)" class="flex-[2] py-4 bg-blue-600 text-white rounded-2xl text-[10px] font-black uppercase tracking-widest shadow-md">Ver Historial</button>
          </div>
        </div>
      </div>

      <!-- ESTADO VACÍO -->
      <div v-if="usuariosFiltrados.length === 0" class="mt-10 p-20 text-center bg-white rounded-[2.5rem] border-2 border-dashed border-slate-200">
        <div class="bg-slate-50 w-20 h-20 rounded-full flex items-center justify-center mx-auto mb-4">
          <svg xmlns="http://www.w3.org/2000/svg" class="h-10 w-10 text-slate-300" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9.172 9.172a4 4 0 015.656 0M9 10h.01M15 10h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
          </svg>
        </div>
        <p class="text-slate-400 font-bold text-lg">No se encontraron resultados para "{{ filtroQuery }}"</p>
      </div>

    </div>
  </div>

  <!-- MODALES -->
  <configuracionCuenta 
    v-if="accederInformacionPaciente" 
    :usuarioData="pacienteSeleccionado" 
    :rolSesion ="getRolFromToken()"
    :modoAdmin="true"
    @cerrar="accederInformacionPaciente = false" 
  />
  <verHistorial 
    v-if="accederHistorial" 
    :esAdmin ="true"
    :rolVisor = "getRolFromToken() "
    :usuarioSeleccionado="pacienteSeleccionado" 
    @cerrar="accederHistorial = false" 
  />
</template>

<style scoped>
.group:hover {
  transform: translateY(-1px);
}
</style>