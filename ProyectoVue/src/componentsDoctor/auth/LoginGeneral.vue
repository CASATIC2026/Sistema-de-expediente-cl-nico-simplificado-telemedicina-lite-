

/// ESTE ES EL VERADERO LOGIN PARA TODOS LOS USUARIOS, INCLUYENDO DOCTORES, PACIENTES Y ADMINISTRADORES


<script setup>

import { useRouter } from 'vue-router'
import { ref, onMounted, onUnmounted } from 'vue'
import { jwtDecode } from 'jwt-decode'
import api, { login, perfil } from '@/services/api'
import ForgotPassword from '@/views/ForgotPassword.vue'
import { EyeIcon, EyeSlashIcon } from '@heroicons/vue/24/outline'


const router = useRouter()
defineEmits(['abrirRegistro'])

const cargando          = ref(false)
const mensajeError      = ref('')
const contraseñaVisible = ref(false)
const mostrarForgotPassword = ref(false)

const toggleContraseña = () => {
  contraseñaVisible.value = !contraseñaVisible.value
}

const loginData = ref({
  email: '',
  password: ''
})

// ===============================
// HELPER — Redirección por rol
// ===============================
const redireccionPorRol = (rol) => {
  if (rol === 'paciente') return router.replace('/app/dashboardPaciente')
  if (rol === 'doctor')   return router.replace('/app/MedDashboard')
  if (rol === 'admin')    return router.replace('/app/dashboardAdmin')
  return router.replace('/')
}

// ===============================
// LOGIN NORMAL
// ===============================
const handleLogin = async () => {
  cargando.value     = true
  mensajeError.value = ''

  try {
    const res = await login(loginData.value)
    const rol = res.rol?.toLowerCase()

    localStorage.setItem('token',                  res.token)
    localStorage.setItem('rol',                    rol)
    localStorage.setItem('requiereCambioPassword', String(res.requiereCambioPassword))
    localStorage.setItem('perfilCompleto',         String(res.perfilCompleto))
    localStorage.setItem('user_id',                res.id)
    localStorage.setItem('user_name',              res.nombre ?? '')

    if (rol === 'admin') {
      return router.replace('/app/dashboardAdmin')
    }

    if (res.perfilCompleto === false && rol !== 'admin') {
      return router.replace('/completar-perfil')
    }

    redireccionPorRol(rol)

  } catch (error) {
    console.error(error)
    mensajeError.value = error.response?.data?.message || 'Datos de usuario incorrectos'
    setTimeout(() => { mensajeError.value = '' }, 5000)
  } finally {
    cargando.value = false
  }
}

// ===============================
// LOGIN CON GOOGLE
// ===============================
const handleGoogleLogin = async (response) => {
  cargando.value     = true
  mensajeError.value = ''

  try {
    const googleToken = response.credential
    const res         = await api.post('/auth/google', { idToken: googleToken })
    const rol         = res.data.rol?.toLowerCase()

    if (res.data.token) {
      localStorage.setItem('token', res.data.token)
    }
    localStorage.setItem('rol',                    rol)
    localStorage.setItem('requiereCambioPassword', String(res.data.requiereCambioPassword))
    localStorage.setItem('perfilCompleto',         String(res.data.perfilCompleto))

    // Guardar datos de Google para completar perfil si hace falta
    const payload = jwtDecode(googleToken)
    localStorage.setItem('user', JSON.stringify({
      nombre:  payload.name,
      email:   payload.email,
      picture: payload.picture
    }))

    const perfilData = await perfil()
    const idUsuario  = perfilData?.id ?? perfilData?.idUsuario ?? perfilData?.userId
    if (idUsuario) {
      localStorage.setItem('user_id', idUsuario)
    }
    if (perfilData?.nombreCompleto) {
      localStorage.setItem('user_name', perfilData.nombreCompleto)
    }

    if (res.data.perfilCompleto === false) {
      return router.replace('/completar-perfil')
    }

    redireccionPorRol(rol)

  } catch (error) {
    console.error(error)
    mensajeError.value = 'Error al iniciar con Google'
    setTimeout(() => { mensajeError.value = '' }, 3000)
  } finally {
    cargando.value = false
  }
}

// ===============================
// GOOGLE INIT
// ===============================
const initGoogle = () => {
  if (typeof google === 'undefined') return

  google.accounts.id.initialize({
    client_id: import.meta.env.VITE_GOOGLE_CLIENT_ID,
    callback: handleGoogleLogin
  })

  const btn = document.getElementById('googleBtn')
  if (btn) {
    google.accounts.id.renderButton(btn, {
      theme: 'filled_blue',
      size:  'large'
    })
  }
}

// ===============================
// Helper - un solo onMounted unificado
// ===============================
onMounted(() => {
  // Si ya hay sesión activa, redirigir directo — no inicializar Google
  const token = localStorage.getItem('token')
  const rol   = localStorage.getItem('rol')
  if (token && rol) {
    redireccionPorRol(rol)
    return
  }

  // Sin sesión → inicializar botón de Google
  if (typeof google !== 'undefined') {
    initGoogle()
  } else {
    window.addEventListener('load', initGoogle)
  }
})

onUnmounted(() => {
  window.removeEventListener('load', initGoogle)
})

</script>

<template>
  <div class="w-full max-w-md p-4 md:p-8 text-white">

    <div class="mb-6 md:mb-8">
      <h3 class="text-xl md:text-2xl font-bold uppercase tracking-wide">Iniciar sesión</h3>
    </div>

    <form @submit.prevent="handleLogin" class="space-y-5 md:space-y-6" :class="{'opacity-50 pointer-events-none': cargando}">

      <div>
        <label class="block text-sm font-medium mb-2 opacity-80 uppercase tracking-tighter">
          Correo Electrónico
        </label>
        <input
          v-model="loginData.email"
          type="email"
          required
          class="w-full bg-white/5 border border-white/10 rounded-xl px-4 py-3 md:py-3.5 text-white text-base focus:outline-none focus:ring-2 focus:ring-cyan-500/50 focus:bg-white/10 transition-all placeholder:text-white/20"
          placeholder="ejemplo@salud.com"
        >
      </div>

      <div>
        <div>
          <label class="block text-sm font-medium mb-2 opacity-80 uppercase tracking-tighter">Contraseña</label>

          <div class="relative">

          <input
            v-model="loginData.password"
            :type="contraseñaVisible ? 'text' : 'password'"
            required
            class="w-full bg-white/5 border border-white/10 rounded-xl px-4 py-3 md:py-3.5 pr-12 text-white text-base focus:outline-none focus:ring-2 focus:ring-cyan-500/50 focus:bg-white/10 transition-all placeholder:text-white/20"
            placeholder="********"
          >

          <button
            type="button"
            @click="toggleContraseña"
            class="absolute right-3 top-1/2 -translate-y-1/2 text-cyan-400 hover:text-cyan-300 transition-colors"
          >

            <EyeIcon
              v-if="!contraseñaVisible"
              class="w-5 h-5"
            />

            <EyeSlashIcon
              v-else
              class="w-5 h-5"
            />

          </button>
        </div>

        </div>

        <!-- ENLACE PARA PARA RECUPERAR CONTRASEÑA OLVIDADA -->
        <div class="text-right mt-1">
          <button
            type="button"
            @click="mostrarForgotPassword = true"
            class="text-[11px] text-cyan-400/70 hover:text-cyan-300 uppercase tracking-widest transition-colors touch-manipulation"
          >
            ¿Olvidaste tu contraseña?
          </button>
        </div>
      </div>



      <button
        type="submit"
        class="w-full py-3.5 md:py-4 bg-cyan-600 hover:bg-cyan-500 active:bg-cyan-700 text-white font-bold rounded-full transition-all active:scale-[0.98] shadow-lg shadow-cyan-950/20 uppercase tracking-[0.2em] text-sm touch-manipulation"
      >
        INICIAR
      </button>

      <div class="flex items-center my-4 md:my-6">
        <div class="flex-grow border-t border-slate-700"></div>
        <span class="px-3 text-slate-500 text-xs uppercase">O continúa con</span>
        <div class="flex-grow border-t border-slate-700"></div>
      </div>

      <!-- Google button: overflow hidden para evitar desborde en móvil -->
      <div id="googleBtn" class="flex justify-center h-[50px] overflow-hidden" v-if="!cargando"></div>

      <!-- Error: break-words para textos largos -->
      <div v-if="mensajeError" class="bg-red-50 border-l-4 border-red-500 p-3 md:p-4 my-4 rounded shadow-sm">
        <div class="flex items-start gap-2">
          <span class="text-red-500 shrink-0 mt-0.5">⚠️</span>
          <p class="text-sm text-red-700 font-medium break-words">{{ mensajeError }}</p>
        </div>
      </div>

      <div class="flex flex-col items-center justify-center min-h-[50px] w-full py-3 md:py-4">
        <div v-if="cargando" class="flex flex-col items-center space-y-3">
          <div class="w-8 h-8 border-4 border-blue-200 border-t-blue-600 rounded-full animate-spin"></div>
          <p class="text-sm font-medium text-slate-300 animate-pulse text-center">
            Verificando cuenta médica...
          </p>
        </div>
      </div>

      <!-- Registro: flex-wrap para pantallas muy angostas -->
      <p class="text-center text-sm text-white/60 flex flex-wrap items-center justify-center gap-1">
        ¿No tienes cuenta?
        <button
          type="button"
          @click="$emit('abrirRegistro')"
          class="text-cyan-400 font-bold hover:text-cyan-300 hover:underline uppercase transition-all touch-manipulation"
        >
          Registrarme
        </button>
      </p>

    </form>
    <!-- MODAL PARA CUANDO USUARIO OLVIDA SU CONTRASEÑA-->
      <ForgotPassword
        v-if="mostrarForgotPassword"
        @cerrar="mostrarForgotPassword = false"
      />
  </div>
</template>

<style scoped>
input:-webkit-autofill,
input:-webkit-autofill:hover,
input:-webkit-autofill:focus {
  -webkit-text-fill-color: white;
  -webkit-box-shadow: 0 0 0px 1000px #1f2e4e inset;
  transition: background-color 5000s ease-in-out 0s;
}

/* Evita zoom automático en iOS al enfocar inputs */
input {
  font-size: 16px;
}
</style>
