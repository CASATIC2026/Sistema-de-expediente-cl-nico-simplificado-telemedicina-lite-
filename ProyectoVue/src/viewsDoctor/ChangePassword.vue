<script setup>
import { ref, computed } from 'vue'
import { useRouter } from 'vue-router'
import { changePassword } from '@/services/api'
import logoIcon from '@/assets/Logo icon.png'
import { EyeIcon, EyeSlashIcon } from '@heroicons/vue/24/outline'

const router = useRouter()

const passwordActual = ref('')
const passwordNueva = ref('')
const confirmPassword = ref('')

const mensaje = ref('')
const error = ref('')
const cargando = ref(false)

const verActual = ref(false)
const verNueva = ref(false)
const verConfirmar = ref(false)

// Validación contraseña segura
const contraseñaEsSegura = computed(() => {
  const regex =
    /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z\d]).{8,}$/

  return regex.test(passwordNueva.value)
})

// Cambiar contraseña
const submit = async () => {

  mensaje.value = ''
  error.value = ''

  if (!contraseñaEsSegura.value) {
    error.value =
      'La contraseña debe tener mayúscula, minúscula, número y símbolo.'
    return
  }

  if (passwordNueva.value !== confirmPassword.value) {
    error.value = 'Las contraseñas no coinciden.'
    return
  }

  cargando.value = true

  try {

    const response = await changePassword({
      PasswordActual: passwordActual.value,
      PasswordNueva: passwordNueva.value
    })

    // Guardar nuevo token
    if (response.data?.token) {
      localStorage.setItem('token', response.data.token)
    }

    mensaje.value = 'Contraseña actualizada correctamente'

    // Actualizar flags
    localStorage.setItem('requiereCambioPassword', 'false')
    localStorage.setItem('perfilCompleto', 'true')

    const rol = localStorage.getItem('rol')?.toLowerCase()

    const rutas = {
      paciente: '/app/dashboardPaciente',
      doctor: '/app/MedDashboard',
      admin: '/app/dashboardAdmin'
    }

    setTimeout(() => {
      router.replace(rutas[rol] || '/')
    }, 1500)

  } catch (err) {

    console.error(err)

    error.value =
      err.response?.data?.message ||
      err.response?.data ||
      'Error al cambiar contraseña'

  } finally {
    cargando.value = false
  }
}
</script>

<template>
  <div class="min-h-screen bg-gradient-to-br from-[#0a1628] via-[#0f2040] to-[#0a1628] flex items-center justify-center px-4">

    <div class="w-full max-w-md bg-slate-900 border border-white/10 rounded-2xl p-8 shadow-2xl text-white">

      <!-- Logo -->
      <div class="text-center mb-2 -mt-4">

        <div class="w-32 h-32 mx-auto mb-2 flex items-center justify-center">
          <img
            :src="logoIcon"
            alt="TelMed Logo"
            class="w-full h-full object-contain drop-shadow-[0_0_20px_rgba(34,211,238,0.3)]"
          />
        </div>

        <h1 class="text-2xl font-bold tracking-widest mt-[-10px]">
          <span class="text-white">TelMed</span>
          <span class="text-cyan-400 ml-2">Lite™</span>
        </h1>

      </div>

      <!-- Header -->
      <div class="mb-8 mt-10 text-center">
        <h2 class="text-xl font-bold tracking-wide">
          Cambia tu contraseña
        </h2>

        <p class="text-sm text-white/50 mt-1">
          Por seguridad, debes actualizar tu contraseña temporal.
        </p>
      </div>

      <!-- Form -->
      <form
        @submit.prevent="submit"
        class="space-y-5"
        :class="{ 'opacity-50 pointer-events-none': cargando }"
      >

      <!-- CONTRASEÑA ACTUAL -->
      <div>
        <label class="block text-sm font-medium mb-2 text-white/70 tracking-wide">
          Contraseña actual :
        </label>

        <div class="relative">

          <input
            v-model="passwordActual"
            :type="verActual ? 'text' : 'password'"
            required
            placeholder="Escribe tu contraseña actual"
            class="w-full bg-white/5 border border-white/10 rounded-xl px-4 py-3 pr-12 text-white text-base focus:outline-none focus:ring-2 focus:ring-cyan-500/50 transition-all placeholder:text-white/20"
          />

          <button
            type="button"
            @click="verActual = !verActual"
            class="absolute right-3 top-1/2 -translate-y-1/2 text-white/70 hover:text-cyan-300 transition-colors p-1"
          >
            <EyeIcon
              v-if="!verActual"
              class="w-5 h-5"
            />

            <EyeSlashIcon
              v-else
              class="w-5 h-5"
            />
          </button>

        </div>
      </div>

        <!-- NUEVA CONTRASEÑA -->
        <div>

          <label class="block text-sm font-medium mb-2 text-white/70 tracking-wide">
            Nueva contraseña :
          </label>

          <div class="relative">

            <input
              v-model="passwordNueva"
              :type="verNueva ? 'text' : 'password'"
              required
              placeholder="Escribe una nueva contraseña"
              :class="[
                'w-full bg-white/5 border rounded-xl px-4 py-3 pr-12 text-white text-base focus:outline-none focus:ring-2 transition-all placeholder:text-white/20',
                passwordNueva.length > 0
                  ? (contraseñaEsSegura.value
                      ? 'border-green-500/50 focus:ring-green-500/50'
                      : 'border-orange-500/50 focus:ring-orange-500/50')
                  : 'border-white/10 focus:ring-cyan-500/50'
              ]"
            />

            <button
              type="button"
              @click="verNueva = !verNueva"
              class="absolute right-3 top-1/2 -translate-y-1/2 text-white/70 hover:text-cyan-300 transition-colors p-1"
            >
              <EyeIcon
                v-if="!verNueva"
                class="w-5 h-5"
              />

              <EyeSlashIcon
                v-else
                class="w-5 h-5"
              />
            </button>

          </div>

          <!-- Requisitos -->
          <div
            v-if="passwordNueva.length > 0"
            class="grid grid-cols-2 gap-1 mt-2 text-[10px] bg-white/5 p-2 rounded-lg border border-white/5"
          >
            <p :class="/[A-Z]/.test(passwordNueva) ? 'text-green-400' : 'text-slate-500'">
              ● Una mayúscula
            </p>

            <p :class="/[a-z]/.test(passwordNueva) ? 'text-green-400' : 'text-slate-500'">
              ● Una minúscula
            </p>

            <p :class="/\d/.test(passwordNueva) ? 'text-green-400' : 'text-slate-500'">
              ● Un número
            </p>

            <p :class="/[@$!%*?&]/.test(passwordNueva) ? 'text-green-400' : 'text-slate-500'">
              ● Un símbolo
            </p>

            <p :class="passwordNueva.length >= 8 ? 'text-green-400' : 'text-slate-500'">
              ● Mínimo 8 caracteres
            </p>
          </div>
        </div>

        <!-- CONFIRMAR CONTRASEÑA -->
        <div>

          <label class="block text-sm font-medium mb-2 text-white/70 tracking-wide">
            Confirmar contraseña :
          </label>

          <div class="relative">

            <input
              v-model="confirmPassword"
              :type="verConfirmar ? 'text' : 'password'"
              required
              placeholder="Repite tu contraseña"
              class="w-full bg-white/5 border border-white/10 rounded-xl px-4 py-3 pr-12 text-white text-base focus:outline-none focus:ring-2 focus:ring-cyan-500/50 transition-all placeholder:text-white/20"
            />

            <button
              type="button"
              @click="verConfirmar = !verConfirmar"
              class="absolute right-3 top-1/2 -translate-y-1/2 text-white/70 hover:text-cyan-300 transition-colors p-1"
            >
              <EyeIcon
                v-if="!verConfirmar"
                class="w-5 h-5"
              />

              <EyeSlashIcon
                v-else
                class="w-5 h-5"
              />
            </button>

          </div>
        </div>

        <!-- Coincidencia -->
        <div
          v-if="confirmPassword.length > 0"
          class="flex items-center gap-2 text-xs"
        >
          <span
            v-if="passwordNueva === confirmPassword"
            class="text-emerald-400"
          >
            ✅ Las contraseñas coinciden
          </span>

          <span
            v-else
            class="text-red-400"
          >
            ❌ Las contraseñas no coinciden
          </span>
        </div>

        <!-- Error -->
        <div
          v-if="error"
          class="bg-red-500/10 border border-red-500/20 p-3 rounded-xl"
        >
          <p class="text-sm text-red-300 font-medium flex items-center gap-2">
            <span>⚠️</span>
            {{ error }}
          </p>
        </div>

        <!-- Success -->
        <div
          v-if="mensaje"
          class="bg-emerald-500/10 border border-emerald-500/20 p-3 rounded-xl"
        >
          <p class="text-sm text-emerald-300 font-medium flex items-center gap-2">
            <span>✅</span>
            {{ mensaje }}
          </p>
        </div>

        <!-- Submit -->
        <button
          type="submit"
          :disabled="cargando"
          class="w-full py-3.5 bg-gradient-to-r from-cyan-600 to-blue-600 hover:from-cyan-500 hover:to-blue-500 disabled:bg-cyan-900 disabled:cursor-not-allowed text-white font-bold !rounded-xl transition-all uppercase tracking-[0.2em] text-sm shadow-lg"
        >
          <span v-if="!cargando">
            CAMBIAR CONTRASEÑA
          </span>

          <span
            v-else
            class="flex items-center justify-center gap-2"
          >
            <span class="w-4 h-4 border-2 border-white/30 border-t-white rounded-full animate-spin inline-block"></span>
            Actualizando...
          </span>
        </button>

      </form>

    </div>
  </div>
</template>

<style scoped>
input:-webkit-autofill,
input:-webkit-autofill:focus {
  -webkit-text-fill-color: white;
  -webkit-box-shadow: 0 0 0px 1000px #0f1e38 inset;
}

input {
  font-size: 16px;
}
</style>