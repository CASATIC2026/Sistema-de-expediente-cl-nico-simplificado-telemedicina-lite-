<script setup>
import { ref } from 'vue'
import { forgotPassword } from '@/services/api'

const emit = defineEmits(['cerrar'])

const email      = ref('')
const cargando   = ref(false)
const enviado    = ref(false)
const error      = ref('')

const handleSubmit = async () => {
  error.value    = ''
  cargando.value = true
  try {
    await forgotPassword(email.value.trim().toLowerCase())
    enviado.value = true
  } catch (e) {
    error.value = e.response?.data?.message || 'Error al enviar el correo.'
  } finally {
    cargando.value = false
  }
}
</script>

<template>
  <!-- Overlay -->
  <div
    class="fixed inset-0 z-50 flex items-center justify-center bg-black/60 backdrop-blur-sm px-4"
    @click.self="emit('cerrar')"
  >
    <div class="relative w-full max-w-md bg-[#0f1e38] border border-white/10 rounded-2xl p-8 shadow-2xl text-white">

      <!-- Botón cerrar -->
      <button
        @click="emit('cerrar')"
        class="absolute top-4 right-4 text-white/40 hover:text-white transition-colors text-xl"
      >✕</button>

      <!-- Estado: formulario -->
      <template v-if="!enviado">
        <div class="mb-6">
          <h2 class="text-xl font-bold uppercase tracking-wide">Recuperar contraseña</h2>
          <p class="text-sm text-white/50 mt-1">
            Ingresa tu correo y te enviaremos un enlace para restablecer tu contraseña.
          </p>
        </div>

        <form @submit.prevent="handleSubmit" class="space-y-5" :class="{'opacity-50 pointer-events-none': cargando}">
          <div>
            <label class="block text-sm font-medium mb-2 opacity-80 uppercase tracking-tighter">
              Correo electrónico
            </label>
            <input
              v-model="email"
              type="email"
              required
              placeholder="ejemplo@salud.com"
              class="w-full bg-white/5 border border-white/10 rounded-xl px-4 py-3 text-white text-base focus:outline-none focus:ring-2 focus:ring-cyan-500/50 focus:bg-white/10 transition-all placeholder:text-white/20"
            />
          </div>

          <!-- Error -->
          <div v-if="error" class="bg-red-50 border-l-4 border-red-500 p-3 rounded">
            <p class="text-sm text-red-700 font-medium">⚠️ {{ error }}</p>
          </div>

          <button
            type="submit"
            class="w-full py-3.5 bg-cyan-600 hover:bg-cyan-500 active:bg-cyan-700 text-white font-bold rounded-xl transition-all uppercase tracking-[0.2em] text-sm shadow-lg"
          >
            <span v-if="!cargando">ENVIAR ENLACE</span>
            <span v-else class="flex items-center justify-center gap-2">
              <span class="w-4 h-4 border-2 border-white/30 border-t-white rounded-full animate-spin inline-block"></span>
              Enviando...
            </span>
          </button>
        </form>
      </template>

      <!-- Estado: correo enviado -->
      <template v-else>
        <div class="text-center py-4">
          <div class="text-5xl mb-4">📧</div>
          <h2 class="text-xl font-bold uppercase tracking-wide mb-2">¡Correo enviado!</h2>
          <p class="text-white/60 text-sm leading-relaxed">
            Si el correo existe en nuestro sistema, recibirás un enlace para restablecer tu contraseña en los próximos minutos.
          </p>
          <p class="text-white/40 text-xs mt-3">Revisa también tu carpeta de spam.</p>
          <button
            @click="emit('cerrar')"
            class="mt-6 w-full py-3 bg-white/10 hover:bg-white/20 rounded-xl font-bold uppercase tracking-wider text-sm transition-all"
          >
            Cerrar
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