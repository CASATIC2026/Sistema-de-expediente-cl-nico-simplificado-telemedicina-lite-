<script setup>
import { ref, computed, watchEffect } from 'vue'
import { perfil } from '@/services/api'
import api from '@/services/api'

// ── 1. PROPS Y EMITS primero siempre ──────────────────
const props = defineProps({
  usuarioData: { type: Object, default: () => ({}) },
  rolSesion:   { type: String, default: 'paciente' },
  modoAdmin:   { type: Boolean, default: false },
  pacienteId:  { type: Number, default: null }
})

const emit = defineEmits(['cerrar', 'actualizar'])

// ── 2. COMPUTED que dependen de props ─────────────────
const tituloModal = computed(() => {
  const rol = props.rolSesion?.toLowerCase()
  if (props.modoAdmin)  return 'Información del Paciente'
  if (rol === 'admin')  return 'Panel de Administrador'
  if (rol === 'doctor') return 'Mi Perfil Médico'
  return 'Mi Perfil'
})

const subtituloModal = computed(() => {
  if (props.modoAdmin)  return 'Vista de solo lectura'
  const rol = props.rolSesion?.toLowerCase()
  if (rol === 'admin')  return 'Gestión y Configuración'
  if (rol === 'doctor') return 'Información del Profesional'
  return 'Identidad Digital'
})

const puedeEditar = computed(() => {
  if (props.modoAdmin) return false
  return ['paciente', 'admin', 'doctor'].includes(props.rolSesion?.toLowerCase())
})

//const esVistaPacientePropia = computed(() => props.rolSesion === 'paciente')

const esUsuarioGoogle = computed(
  () => formulario.value.esGoogle === true)
// ── 3. Estado local ───────────────────────────────────
const editarPerfil = ref(false)
const cargando     = ref(false)
const formulario   = ref({})

// ── Notificación inline ───────────────────────────────
const notificacion = ref({ mensaje: '', tipo: '' })
const mostrarNotificacion = (mensaje, tipo = 'exito') => {
  notificacion.value = { mensaje, tipo }
  setTimeout(() => notificacion.value = { mensaje: '', tipo: '' }, 4000)
}

// ── 4. watchEffect: reacciona cuando llegan datos ─────
watchEffect(() => {
  const d = props.usuarioData
  if (!d || Object.keys(d).length === 0) return
  formulario.value = {
    nombre:    d.nombre    ?? d.Nombre    ?? '',
    apellido:  d.apellido  ?? d.Apellido  ?? '',
    email:     d.email     ?? d.Email     ?? '',
    dui:       d.dui       ?? d.DUI       ?? 'No disponible',
    telefono:  d.telefono  ?? d.Telefono  ?? '',
    direccion: d.direccion ?? d.Direccion ?? '',
    genero:    d.genero    ?? d.Genero    ?? 'No especificado',
    rol:       d.rol       ?? d.Rol       ?? '',
    esGoogle:  d.esGoogle  ?? false
  }
})

// ── 5. Guardar cambios (solo modo propio) ─────────────
const guardarEdicionCambios = async () => {
  if (props.modoAdmin) return
  try {
    cargando.value = true
    await api.put('/Users/perfil', {
      nombre:    formulario.value.nombre,
      apellido:  formulario.value.apellido,
      direccion: formulario.value.direccion,
      telefono:  formulario.value.telefono
    })
    editarPerfil.value = false
    try {
      const actualizado = await perfil()
      formulario.value = {
        nombre:    actualizado.nombre    ?? actualizado.Nombre    ?? '',
        apellido:  actualizado.apellido  ?? actualizado.Apellido  ?? '',
        email:     actualizado.email     ?? actualizado.Email     ?? '',
        dui:       actualizado.dui       ?? actualizado.DUI       ?? 'No disponible',
        telefono:  actualizado.telefono  ?? actualizado.Telefono  ?? '',
        direccion: actualizado.direccion ?? actualizado.Direccion ?? '',
        genero:    actualizado.genero    ?? actualizado.Genero    ?? 'No especificado',
        rol:       actualizado.rol       ?? actualizado.Rol       ?? ''
      }
    } catch (e) {
      console.error('Error recargando perfil:', e)
    }
    emit('actualizar')
    mostrarNotificacion('Perfil actualizado correctamente.')
  } catch (e) {
    console.error('Error al actualizar:', e)
    mostrarNotificacion(e.response?.data?.message || 'No se pudo actualizar la información.', 'error')
  } finally {
    cargando.value = false
  }
}

// ── Cambio de contraseña ──────────────────────────────
const mostrarCambioPassword = ref(false)
const cargandoPassword      = ref(false)
const passwordForm = ref({ actual: '', nueva: '', confirmar: '' })

const passwordValido = computed(() =>
  passwordForm.value.actual &&
  passwordForm.value.nueva &&
  passwordForm.value.nueva === passwordForm.value.confirmar &&
  passwordForm.value.nueva.length >= 6
)

const cancelarCambioPassword = () => {
  mostrarCambioPassword.value = false
  Object.assign(passwordForm.value, { actual: '', nueva: '', confirmar: '' })
}

const cambiarPassword = async () => {
  if (!passwordValido.value) return
  try {
    cargandoPassword.value = true
    await api.post('/Auth/cambiar-password', {
      passwordActual: passwordForm.value.actual,
      passwordNueva:  passwordForm.value.nueva
    })
    mostrarNotificacion('¡Contraseña actualizada correctamente!')
    cancelarCambioPassword()
  } catch (e) {
    mostrarNotificacion(e.response?.data?.message || 'No se pudo cambiar la contraseña.', 'error')
  } finally {
    cargandoPassword.value = false
  }
}

const verPassword = ref({ actual: false, nueva: false, confirmar: false })
</script>

<template>
  <div
    class="fixed inset-0 flex items-center justify-center bg-slate-900/50 backdrop-blur-sm z-[9999] p-2 md:p-4"
    @click.self="emit('cerrar')"
  >
    <div class="bg-white text-slate-800 w-full max-w-xl max-h-[92vh] md:max-h-[95vh] overflow-hidden rounded-[1.5rem] md:rounded-[2rem] shadow-2xl flex flex-col animate-in border border-slate-100">

      <header class="relative px-4 py-5 md:px-6 md:pt-6 md:pb-4 text-center border-b border-slate-100 shrink-0">
        <button
          @click="emit('cerrar')"
          class="absolute top-3 right-3 md:top-5 md:right-5 text-slate-300 hover:text-red-400 transition-all p-1.5 rounded-xl hover:bg-red-50"
          title="Cerrar"
        >
          <svg xmlns="http://www.w3.org/2000/svg" class="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
          </svg>
        </button>

        <div class="mx-auto w-12 h-12 md:w-16 md:h-16 bg-linear-to-br from-cyan-400 to-blue-600 rounded-xl md:rounded-2xl flex items-center justify-center shadow-lg shadow-blue-200 mb-2 md:mb-3">
          <span class="text-xl md:text-2xl text-white font-black uppercase">
            {{ formulario.nombre?.[0] || '?' }}{{ formulario.apellido?.[0] || '' }}
          </span>
        </div>

        <h2 class="text-base md:text-lg font-black text-slate-800 tracking-tight leading-tight">{{ tituloModal }}</h2>
        <p class="text-slate-400 text-[10px] md:text-xs font-bold mt-0.5 uppercase tracking-widest">{{ subtituloModal }}</p>
      </header>

      <transition name="notif">
        <div
          v-if="notificacion.mensaje"
          class="mx-4 md:mx-6 mt-4 px-4 py-3 rounded-xl text-xs md:text-sm font-bold flex items-center gap-3 shrink-0"
          :class="notificacion.tipo === 'exito'
            ? 'bg-emerald-50 text-emerald-700 border border-emerald-200'
            : 'bg-red-50 text-red-600 border border-red-200'"
        >
          <span class="text-base">{{ notificacion.tipo === 'exito' ? '✅' : '⚠️' }}</span>
          {{ notificacion.mensaje }}
        </div>
      </transition>

      <div class="flex-1 overflow-y-auto px-4 pb-4 pt-4 md:px-6 md:pb-6 md:pt-4 custom-scrollbar space-y-4">

        <div class="rounded-2xl border border-slate-100 overflow-hidden">
          <div class="px-4 py-3 bg-slate-50 border-b border-slate-100 flex items-center gap-2">
            <div class="h-1.5 w-6 bg-cyan-500 rounded-full"></div>
            <h3 class="text-[9px] md:text-[10px] font-black uppercase tracking-[0.2em] text-slate-500">Datos generales</h3>
          </div>
          <div class="grid grid-cols-1 xs:grid-cols-2 gap-px bg-slate-100">
            <div class="bg-white p-3 md:p-4 space-y-0.5">
              <p class="text-[9px] md:text-[10px] font-black text-slate-400 uppercase">Nombre Completo</p>
              <p class="text-xs md:text-sm font-bold text-slate-700">{{ formulario.nombre }} {{ formulario.apellido }}</p>
            </div>
            <div class="bg-white p-3 md:p-4 space-y-0.5">
              <p class="text-[9px] md:text-[10px] font-black text-slate-400 uppercase">DUI</p>
              <p class="text-xs md:text-sm font-bold text-slate-700">{{ formulario.dui || 'No disponible' }}</p>
            </div>
            <div class="bg-white p-3 md:p-4 space-y-0.5">
              <p class="text-[9px] md:text-[10px] font-black text-slate-400 uppercase">Género</p>
              <p class="text-xs md:text-sm font-bold text-slate-700">{{ formulario.genero || 'No especificado' }}</p>
            </div>
            <div class="bg-white p-3 md:p-4 space-y-0.5">
              <p class="text-[9px] md:text-[10px] font-black text-slate-400 uppercase">Tipo de Cuenta</p>
              <p class="text-xs md:text-sm font-bold text-cyan-600 capitalize">{{ formulario.rol }}</p>
            </div>
          </div>
        </div>

        <div class="rounded-2xl border border-slate-100 overflow-hidden">
          <div class="px-4 py-3 bg-slate-50 border-b border-slate-100 flex items-center justify-between">
            <div class="flex items-center gap-2">
              <div class="h-1.5 w-6 bg-blue-500 rounded-full"></div>
              <h3 class="text-[9px] md:text-[10px] font-black uppercase tracking-[0.2em] text-slate-500">Contacto</h3>
            </div>
            <button
              v-if="!editarPerfil && puedeEditar && !props.modoAdmin"
              @click="editarPerfil = true"
              class="text-[9px] md:text-[10px] font-black uppercase bg-blue-50 text-blue-600 px-3 py-1.5 rounded-lg hover:bg-blue-600 hover:text-white transition-all"
            >
              Editar
            </button>
          </div>

          <div class="p-4 md:p-6 space-y-4">
            <div class="flex flex-col gap-1">
              <label class="text-[9px] md:text-[10px] font-black text-slate-400 uppercase">Teléfono</label>
              <div v-if="!editarPerfil" class="text-xs md:text-sm font-bold text-slate-700">
                {{ formulario.telefono || 'No asignado' }}
              </div>
              <input
                v-else
                v-model="formulario.telefono"
                type="text"
                class="w-full px-3 py-2.5 md:px-4 md:py-3 bg-slate-50 border-2 border-transparent focus:border-cyan-500 rounded-xl outline-none transition-all font-bold text-slate-700 text-sm"
              />
            </div>

            <div class="flex flex-col gap-1">
              <label class="text-[9px] md:text-[10px] font-black text-slate-400 uppercase">Correo Electrónico</label>
              <div class="text-xs md:text-sm font-bold text-slate-700 break-all">{{ formulario.email }}</div>
            </div>

            <div class="flex flex-col gap-1">
              <label class="text-[9px] md:text-[10px] font-black text-slate-400 uppercase">Dirección</label>
              <div v-if="!editarPerfil" class="text-xs md:text-sm font-bold text-slate-600 italic break-words">
                "{{ formulario.direccion || 'No asignada' }}"
              </div>
              <textarea
                v-else
                v-model="formulario.direccion"
                rows="2"
                class="w-full px-3 py-2.5 md:px-4 md:py-3 bg-slate-50 border-2 border-transparent focus:border-cyan-500 rounded-xl outline-none transition-all font-bold text-slate-700 text-sm resize-none"
              ></textarea>
            </div>
          </div>
        </div>

        <div v-if="!props.modoAdmin && !esUsuarioGoogle" class="rounded-2xl border border-slate-100 overflow-hidden">
          <div class="px-4 py-3 bg-slate-50 border-b border-slate-100 flex items-center justify-between">
            <div class="flex items-center gap-2">
              <div class="h-1.5 w-6 bg-violet-500 rounded-full"></div>
              <h3 class="text-[9px] md:text-[10px] font-black uppercase tracking-[0.2em] text-slate-500">Seguridad</h3>
            </div>
            <button
              v-if="!mostrarCambioPassword"
              @click="mostrarCambioPassword = true"
              class="text-[9px] md:text-[10px] font-black uppercase bg-violet-50 text-violet-600 px-3 py-1.5 rounded-lg hover:bg-violet-600 hover:text-white transition-all"
            >
              Cambiar
            </button>
          </div>

          <div class="p-4 md:p-5">
            <div v-if="!mostrarCambioPassword" class="flex items-center gap-2 text-xs md:text-sm text-slate-400 font-bold">
              <span>🔒</span>Contraseña protegida
            </div>

            <transition name="expand">
              <div v-if="mostrarCambioPassword" class="space-y-3">
                <div class="flex flex-col gap-1">
                  <label class="text-[9px] md:text-[10px] font-black text-slate-400 uppercase">Contraseña Actual</label>
                  <div class="relative">
                    <input
                      v-model="passwordForm.actual"
                      :type="verPassword.actual ? 'text' : 'password'"
                      placeholder="••••••••"
                      class="w-full px-4 py-2.5 md:py-3 pr-12 bg-slate-50 border-2 border-transparent focus:border-violet-400 rounded-xl outline-none transition-all font-bold text-slate-700 text-sm"
                    />
                    <button type="button" @click="verPassword.actual = !verPassword.actual" class="absolute right-3 top-1/2 -translate-y-1/2 text-slate-400 p-1">
                       <svg v-if="!verPassword.actual" xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" /><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z" /></svg>
                       <svg v-else xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13.875 18.825A10.05 10.05 0 0112 19c-4.478 0-8.268-2.943-9.543-7a9.97 9.97 0 011.563-3.029m5.858.908a3 3 0 114.243 4.243M9.878 9.878l4.242 4.242M9.88 9.88l-3.29-3.29m7.532 7.532l3.29 3.29M3 3l3.59 3.59m0 0A9.953 9.953 0 0112 5c4.478 0 8.268 2.943 9.543 7a10.025 10.025 0 01-4.132 4.411m0 0L21 21" /></svg>
                    </button>
                  </div>
                </div>

                <div class="flex flex-col gap-1">
                  <label class="text-[9px] md:text-[10px] font-black text-slate-400 uppercase">Nueva</label>

                  <div class="relative">
                    <input
                      v-model="passwordForm.nueva"
                      :type="verPassword.nueva ? 'text' : 'password'"
                      placeholder="••••••••"
                      class="w-full px-4 py-2.5 md:py-3 pr-12 bg-slate-50 border-2 border-transparent focus:border-violet-400 rounded-xl outline-none transition-all font-bold text-slate-700 text-sm"
                    />
                    <button type="button" @click="verPassword.nueva = !verPassword.nueva" class="absolute right-3 top-1/2 -translate-y-1/2 text-slate-400 p-1">
                       <svg v-if="!verPassword.nueva" xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" /><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z" /></svg>
                       <svg v-else xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13.875 18.825A10.05 10.05 0 0112 19c-4.478 0-8.268-2.943-9.543-7a9.97 9.97 0 011.563-3.029m5.858.908a3 3 0 114.243 4.243M9.878 9.878l4.242 4.242M9.88 9.88l-3.29-3.29m7.532 7.532l3.29 3.29M3 3l3.59 3.59m0 0A9.953 9.953 0 0112 5c4.478 0 8.268 2.943 9.543 7a10.025 10.025 0 01-4.132 4.411m0 0L21 21" /></svg>
                    </button>
                  </div>
                </div>

                <div class="flex gap-2 pt-1">
                  <button @click="cancelarCambioPassword" class="flex-1 py-2.5 text-[10px] font-black uppercase text-slate-400 rounded-xl hover:bg-red-50">Cancelar</button>
                  <button @click="cambiarPassword" :disabled="cargandoPassword || !passwordValido" class="flex-[2] bg-slate-900 text-white py-2.5 rounded-xl font-black text-[10px] uppercase tracking-widest disabled:opacity-40">Guardar</button>
                </div>
              </div>
            </transition>
          </div>
        </div>
      </div>

      <footer class="px-4 py-4 md:px-6 md:py-4 border-t border-slate-100 bg-white shrink-0">
        <div v-if="editarPerfil" class="flex flex-col xs:flex-row items-center gap-2 md:gap-3">
          <button
            @click="editarPerfil = false"
            :disabled="cargando"
            class="w-full xs:flex-1 text-[10px] font-black uppercase tracking-widest text-slate-400 py-3 rounded-xl hover:bg-red-50"
          >
            Cancelar
          </button>
          <button
            @click="guardarEdicionCambios"
            :disabled="cargando"
            class="w-full xs:flex-[2] bg-slate-900 text-white py-3.5 rounded-xl font-black text-[10px] uppercase tracking-[0.15em] transition-all shadow-md flex justify-center items-center disabled:opacity-50"
          >
            <span v-if="!cargando">Guardar Cambios</span>
            <span v-else class="animate-spin border-2 border-white/30 border-t-white rounded-full h-4 w-4"></span>
          </button>
        </div>

        <button
          v-else
          @click="emit('cerrar')"
          class="w-full bg-gradient-to-r from-cyan-500 to-blue-600 text-white py-3.5 rounded-xl font-black text-[10px] md:text-xs uppercase tracking-[0.15em] transition-all shadow-md"
        >
          {{ esUsuarioGoogle ? 'Google Account' : 'Cerrar' }}
        </button>
      </footer>

    </div>
  </div>
</template>
