<script setup>
import { reactive, ref, onMounted, computed } from 'vue'
import { useRouter } from 'vue-router'
import { register } from '@/services/api'
import api from '@/services/api'
import { EyeIcon, EyeSlashIcon } from '@heroicons/vue/24/outline'

const router = useRouter()

// VARIABLES DE ESTADO
const verContraseña = ref(false)
const cargando = ref(false)
const mensajeError = ref('')

const form = reactive({
  nombre: '',
  apellido: '',
  genero: '',
  fechaNacimiento: '',
  dui: '',
  direccion: '',
  telefono: '',
  email: '',
  password: '',
})

const emit = defineEmits(['cerrar'])

const props = defineProps({
  esGoogle: {
    type: Boolean,
    default: false,
  },
})

// ===============================
// FUNCIÓN CERRAR MODAL
const cerrar = () => {
  emit('cerrar')
}

// ===============================
// AUTOCOMPLETAR SI VIENE DE GOOGLE
onMounted(() => {
  if (props.esGoogle) {
    let user = {}
    try {
      user = JSON.parse(localStorage.getItem('user')) || {}
    } catch {
      user = {}
    }

    const nombreCompleto = user.nombre || ''
    const partes = nombreCompleto.trim().split(/\s+/)

    if (partes.length > 0) {
      form.nombre = partes[0] || ''
      form.apellido = partes.slice(1).join(' ') || ''
    }

    form.email = user.email || ''
  }
})

// ===============================
// CONTRASEÑA SEGURA
const passwordValido = computed(() => {
 const regex = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{8,}$/
  return regex.test(form.password)
})

const contraseñaEsSegura = passwordValido

// ===============================
// ENVÍO FORMULARIO
const enviarFormulario = async () => {
  if (cargando.value) return
  mensajeError.value = ''

  // VALIDACIONES
  if (!form.nombre.trim() || !form.apellido.trim() || !form.dui.trim()) {
    mensajeError.value = 'Nombre, Apellido y DUI son obligatorios'
    return
  }

  if (!props.esGoogle && !passwordValido.value) {
    mensajeError.value =
      'La contraseña debe tener al menos 8 caracteres, incluyendo mayúsculas, minúsculas, números y caracteres especiales.'
    return
  }

  cargando.value = true

  try {
    let res

    // ===============================
    // REGISTRO NORMAL
    if (!props.esGoogle) {
      const response = await register(form)
      res = response.data

      // IMPORTANTE: NO GUARDAR TOKEN NI LOGUEAR
      // SOLO REDIRIGIR A AVISO
      await router.push('/verify-email-notice')
      cerrar()
      return
    }

    // ===============================
    // GOOGLE (flujo normal con login)
    else {
      const response = await api.post('/auth/completar-perfil', form)
      res = response.data

      if (res?.token) {
        localStorage.setItem('token', res.token)
      }

      localStorage.setItem('perfilCompleto', 'true')

      const rolServidor = res.rol || res.role || 'paciente'
      const rolFinal = rolServidor.toLowerCase()
      localStorage.setItem('rol', rolFinal)

      const rutas = {
        admin: '/app/dashboardAdmin',
        paciente: '/app/dashboardPaciente',
        doctor: '/app/MedDashboard',
      }

      const destino = rutas[rolFinal] || '/app/dashboardPaciente'

      await router.push(destino)
      cerrar()
    }

  } catch (error) {
    console.error('Error detallado en el registro:', error)

    const responseData = error.response?.data

    if (responseData?.errors) {
      const allErrors = Object.values(responseData.errors)
        .flat()
        .join(' - ')
      mensajeError.value = allErrors || 'Error de validación en el backend.'
    } else {
      mensajeError.value =
        responseData?.message ||
        responseData ||
        'Error en los datos. Revisa el DUI o el formato de fecha.'
    }
  } finally {
    cargando.value = false
  }
}
</script>

<template>
  <div class="fixed inset-0 z-9999 flex items-center justify-center bg-slate-950/80 backdrop-blur-md p-4 overflow-hidden animacion-suave">
    <form
      class="bg-[#1f2e4e]/95 w-full max-w-115 p-6 md:p-10 rounded-[2.5rem] shadow-2xl flex flex-col gap-5 max-h-[90vh] overflow-y-auto border border-white/10 text-white custom-scrollbar transition-all"
      @submit.prevent="enviarFormulario"
    >
      <div v-if="!esGoogle">
        <button
          class="self-start text-xs font-bold uppercase tracking-widest bg-white/10 hover:bg-white/20 text-cyan-400 px-5 py-2.5 rounded-2xl transition-all flex items-center gap-2 mb-2 group cursor-pointer"
          type="button"
          @click="cerrar"
        >
          <span class="group-hover:-translate-x-1 transition-transform">←</span> Regresar
        </button>
      </div>

      <div class="text-center md:text-left">
        <h3 class="text-3xl font-extrabold text-white">
          {{ esGoogle ? 'Completa tu perfil' : 'Crear Cuenta' }}
        </h3>
        <p class="text-sm text-slate-400 mt-2">Únete a nuestra plataforma de salud</p>
      </div>

      <div v-if="mensajeError" class="bg-red-500/20 border border-red-500/50 text-red-200 p-3 rounded-xl text-xs text-center animate-pulse">
        {{ mensajeError }}
      </div>

      <div class="flex flex-col gap-5">
        <div class="grid grid-cols-2 gap-4">
          <div class="flex flex-col gap-2">
            <label class="text-[11px] font-bold uppercase text-slate-400">Nombres</label>
            <input v-model="form.nombre" type="text" required class="bg-white/5 border border-white/10 p-3 rounded-xl outline-none text-white focus:ring-2 focus:ring-cyan-500/50" />
          </div>
          <div class="flex flex-col gap-2">
            <label class="text-[11px] font-bold uppercase text-slate-400">Apellidos</label>
            <input v-model="form.apellido" type="text" required class="bg-white/5 border border-white/10 p-3 rounded-xl outline-none text-white focus:ring-2 focus:ring-cyan-500/50" />
          </div>
        </div>

        <div class="grid grid-cols-2 gap-4">
          <div class="flex flex-col gap-2">
            <label class="text-[11px] font-bold uppercase text-slate-400">Nacimiento</label>
            <input v-model="form.fechaNacimiento" type="date" required class="bg-white/5 border border-white/10 p-3 rounded-xl text-white outline-none" />
          </div>
          <div class="flex flex-col gap-2">
            <label class="text-[11px] font-bold uppercase text-slate-400">DUI</label>
            <input v-model="form.dui" type="text" placeholder="00000000-0" required class="bg-white/5 border border-white/10 p-3 rounded-xl outline-none text-white focus:ring-2 focus:ring-cyan-500/50" />
          </div>
        </div>

        <div class="grid grid-cols-2 gap-4">
          <div class="flex flex-col gap-2">
            <label class="text-[11px] font-bold uppercase text-slate-400">Teléfono (sin guión)</label>
            <input v-model="form.telefono" type="text" placeholder="77777777" required class="bg-white/5 border border-white/10 p-3 rounded-xl outline-none text-white focus:ring-2 focus:ring-cyan-500/50" />
          </div>
          <div class="flex flex-col gap-2">
            <label class="text-[11px] font-bold uppercase text-slate-400">Dirección</label>
            <input v-model="form.direccion" type="text" placeholder="San Salvador..." required class="bg-white/5 border border-white/10 p-3 rounded-xl outline-none text-white focus:ring-2 focus:ring-cyan-500/50" />
          </div>
        </div>

        <div class="flex flex-col gap-2">
          <label class="text-[11px] font-bold uppercase text-slate-400">Género</label>
          <select v-model="form.genero" required class="bg-white/5 border border-white/10 p-3 rounded-xl text-white outline-none cursor-pointer">
            <option value="" disabled class="bg-slate-900">Seleccionar</option>
            <option value="Masculino" class="bg-slate-900">Masculino</option>
            <option value="Femenino" class="bg-slate-900">Femenino</option>
          </select>
        </div>

        <div v-if="!esGoogle" class="flex flex-col gap-2">
          <label class="text-[11px] font-bold uppercase text-slate-400">Correo</label>
          <input v-model="form.email" type="email" required class="bg-white/5 border border-white/10 p-3 rounded-xl outline-none text-white focus:ring-2 focus:ring-cyan-500/50" />
        </div>

        <div v-if="esGoogle" class="flex flex-col gap-2">
          <label class="text-[11px] font-bold uppercase text-slate-400">Correo</label>
          <input v-model="form.email" type="email" readonly class="bg-white/10 border border-white/10 p-3 rounded-xl outline-none text-white opacity-70 cursor-not-allowed" />
          <p class="text-xs text-slate-400">Este correo viene de Google y no puede cambiarse aquí.</p>
          <p class="text-xs text-slate-400">Tu cuenta utilizará inicio de sesión con google.</p>
        </div>

        <div v-if="!esGoogle" class="flex flex-col gap-2">
          <label class="text-[11px] font-bold uppercase text-slate-400">Escribe una contraseña
            <span v-if="!esGoogle">*</span>
          </label>
          <div class="relative w-full">
            <input
              v-model="form.password"
              :type="verContraseña ? 'text' : 'password'"
              :required="!esGoogle"
              :class="['w-full bg-white/5 border p-3 pr-12 rounded-xl outline-none text-white focus:ring-2 transition-all',
              form.password.length > 0 ? (contraseñaEsSegura ? 'border-green-500/50 focus:ring-green-500/50' : 'border-orange-500/50 focus:ring-orange-500/50') : 'border-white/10 focus:ring-cyan-500/50']"
            />

            <button
              type="button"
              @click="verContraseña = !verContraseña"
              class="absolute right-3 top-1/2 -translate-y-1/2 text-slate-400 hover:text-cyan-400 p-1 transition-colors"
            >
              <span class="text-lg"><EyeIcon v-if="verContraseña" class="w-5 h-5" /><EyeSlashIcon v-else class="w-5 h-5"/></span>
            </button>
          </div>
          <div v-if="form.password.length > 0" class="grid grid-cols-2 gap-1 mt-1 text-[10px]">
            <p :class="/[A-Z]/.test(form.password) ? 'text-green-400' : 'text-slate-500'">● Una mayúscula</p>
            <p :class="/[a-z]/.test(form.password) ? 'text-green-400' : 'text-slate-500'">● Una minúscula</p>
            <p :class="/\d/.test(form.password) ? 'text-green-400' : 'text-slate-500'">● Un número</p>
            <p :class="/[@$!%*?&]/.test(form.password) ? 'text-green-400' : 'text-slate-500'">● Un símbolo (@$!%*?&)</p>
            <p :class="form.password.length >= 8 ? 'text-green-400' : 'text-slate-500'">● Mínimo 8 caracteres</p>
          </div>

        </div>
      </div>

      <button
        :disabled="cargando"
        type="submit"
        class="mt-4 p-4 bg-cyan-600 hover:bg-cyan-500 text-white font-bold rounded-2xl shadow-lg transition-all active:scale-95 uppercase text-xs tracking-widest disabled:opacity-50"
      >
        {{ cargando ? 'Registrando...' : 'Registrarme ahora' }}
      </button>
    </form>
  </div>
</template>

<style scoped>
.animacion-suave { animation: fadeIn 0.3s ease-out; }
@keyframes fadeIn { from { opacity: 0; transform: scale(0.95); } to { opacity: 1; transform: scale(1); } }
.custom-scrollbar::-webkit-scrollbar { width: 4px; }
.custom-scrollbar::-webkit-scrollbar-thumb { background: rgba(34, 211, 238, 0.2); border-radius: 20px; }
</style>
