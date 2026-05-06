<script setup>
import { ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { resetPassword } from '@/services/api'

const route  = useRoute()
const router = useRouter()

const token             = ref('')
const newPassword       = ref('')
const confirmPassword   = ref('')
const cargando          = ref(false)
const exito             = ref(false)
const error             = ref('')
const verNueva          = ref(false)
const verConfirmar      = ref(false)

onMounted(() => {
  token.value = route.query.token || ''
  if (!token.value) {
    error.value = 'Token inválido. Por favor solicita un nuevo enlace.'
  }
})

const handleSubmit = async () => {
  error.value = ''

  if (newPassword.value.length < 8) {
    error.value = 'La contraseña debe tener al menos 8 caracteres.'
    return
  }
  if (newPassword.value !== confirmPassword.value) {
    error.value = 'Las contraseñas no coinciden.'
    return
  }

  cargando.value = true
  try {
    await resetPassword(token.value, newPassword.value)
    exito.value = true
  } catch (e) {
    error.value = e.response?.data?.message || 'Error al restablecer la contraseña.'
  } finally {
    cargando.value = false
  }
}
</script>

<template>
  <div class="min-h-screen bg-gradient-to-br from-[#0a1628] via-[#0f2040] to-[#0a1628] flex items-center justify-center px-4">
    <div class="w-full max-w-md bg-[#0f1e38] border border-white/10 rounded-2xl p-8 shadow-2xl text-white">

      <!-- Logo / Marca -->
      <div class="text-center mb-8">
        <div class="text-4xl mb-3">🏥</div>
        <h1 class="text-2xl font-bold uppercase tracking-widest text-cyan-400">TelMed</h1>
        <p class="text-white/40 text-xs uppercase tracking-widest mt-1">Plataforma de Telemedicina</p>
      </div>

      <!-- Estado: formulario -->
      <template v-if="!exito">
        <div class="mb-6">
          <h2 class="text-xl font-bold uppercase tracking-wide">Nueva contraseña</h2>
          <p class="text-sm text-white/50 mt-1">Ingresa y confirma tu nueva contraseña.</p>
        </div>

        <form @submit.prevent="handleSubmit" class="space-y-5" :class="{'opacity-50 pointer-events-none': cargando}">

          <!-- Nueva contraseña -->
          <div>
            <div class="flex justify-between items-center mb-2">
              <label class="text-sm font-medium opacity-80 uppercase tracking-tighter">Nueva contraseña</label>
              <button type="button" @click="verNueva = !verNueva"
                class="text-[10px] uppercase tracking-widest font-bold text-cyan-400 hover:text-cyan-300 transition-colors">
                {{ verNueva ? '🙈 Ocultar' : '👁️ Ver' }}
              </button>
            </div>
            <input
              v-model="newPassword"
              :type="verNueva ? 'text' : 'password'"
              required
              placeholder="Mínimo 8 caracteres"
              class="w-full bg-white/5 border border-white/10 rounded-xl px-4 py-3 text-white text-base focus:outline-none focus:ring-2 focus:ring-cyan-500/50 transition-all placeholder:text-white/20"
            />
          </div>

          <!-- Confirmar contraseña -->
          <div>
            <div class="flex justify-between items-center mb-2">
              <label class="text-sm font-medium opacity-80 uppercase tracking-tighter">Confirmar contraseña</label>
              <button type="button" @click="verConfirmar = !verConfirmar"
                class="text-[10px] uppercase tracking-widest font-bold text-cyan-400 hover:text-cyan-300 transition-colors">
                {{ verConfirmar ? '🙈 Ocultar' : '👁️ Ver' }}
              </button>
            </div>
            <input
              v-model="confirmPassword"
              :type="verConfirmar ? 'text' : 'password'"
              required
              placeholder="Repite tu contraseña"
              class="w-full bg-white/5 border border-white/10 rounded-xl px-4 py-3 text-white text-base focus:outline-none focus:ring-2 focus:ring-cyan-500/50 transition-all placeholder:text-white/20"
            />
          </div>

          <!-- Indicador de coincidencia -->
          <div v-if="confirmPassword.length > 0" class="flex items-center gap-2 text-xs">
            <span v-if="newPassword === confirmPassword" class="text-emerald-400">✅ Las contraseñas coinciden</span>
            <span v-else class="text-red-400">❌ Las contraseñas no coinciden</span>
          </div>

          <!-- Error -->
          <div v-if="error" class="bg-red-50 border-l-4 border-red-500 p-3 rounded">
            <p class="text-sm text-red-700 font-medium">⚠️ {{ error }}</p>
          </div>

          <button
            type="submit"
            :disabled="!token || cargando"
            class="w-full py-3.5 bg-cyan-600 hover:bg-cyan-500 disabled:bg-cyan-900 disabled:cursor-not-allowed text-white font-bold rounded-xl transition-all uppercase tracking-[0.2em] text-sm shadow-lg"
          >
            <span v-if="!cargando">RESTABLECER CONTRASEÑA</span>
            <span v-else class="flex items-center justify-center gap-2">
              <span class="w-4 h-4 border-2 border-white/30 border-t-white rounded-full animate-spin inline-block"></span>
              Actualizando...
            </span>
          </button>
        </form>
      </template>

      <!-- Estado: éxito -->
      <template v-else>
        <div class="text-center py-4">
          <div class="text-5xl mb-4">✅</div>
          <h2 class="text-xl font-bold uppercase tracking-wide mb-2">¡Contraseña actualizada!</h2>
          <p class="text-white/60 text-sm leading-relaxed">
            Tu contraseña ha sido restablecida correctamente. Ya puedes iniciar sesión con tu nueva contraseña.
          </p>
          <button
            @click="router.replace('/')"
            class="mt-6 w-full py-3.5 bg-cyan-600 hover:bg-cyan-500 rounded-xl font-bold uppercase tracking-[0.2em] text-sm transition-all shadow-lg"
          >
            IR A INICIAR SESIÓN
          </button>
        </div>
      </template>

    </div>
  </div>
</template>

<style scoped>
input:-webkit-autofill,
input:-webkit-autofill:focus {
  -webkit-text-fill-color: white;
  -webkit-box-shadow: 0 0 0px 1000px #0f1e38 inset;
}
input { font-size: 16px; }
</style>