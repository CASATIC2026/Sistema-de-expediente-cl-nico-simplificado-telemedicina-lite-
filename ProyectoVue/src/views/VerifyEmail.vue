<script setup>
import { onMounted, ref } from 'vue'
import { useRoute } from 'vue-router'
import api from '@/services/api'

const route = useRoute()

const estado = ref('verificando') // verificando | ok | error
const mensaje = ref('')

onMounted(async () => {
  const token = route.query.token

  if (!token) {
    estado.value = 'error'
    mensaje.value = 'Token inválido'
    return
  }

  try {
    const res = await api.get(`/auth/verify-email?token=${token}`)
    estado.value = 'ok'
    mensaje.value = res.data.message
  } catch (err) {
    estado.value = 'error'
    mensaje.value = err.response?.data?.message || 'Error al verificar'
  }
})
</script>

<template>
  <div class="min-h-screen flex items-center justify-center bg-slate-950 text-white p-6">
    <div class="bg-[#1f2e4e] p-8 rounded-2xl shadow-xl text-center max-w-md">

      <h1 class="text-2xl font-bold mb-4">
        <span v-if="estado === 'verificando'">⏳ Verificando...</span>
        <span v-else-if="estado === 'ok'">✅ Cuenta verificada</span>
        <span v-else>❌ Error</span>
      </h1>

      <p class="text-slate-300 mb-6">{{ mensaje }}</p>

      <button
        @click="$router.push('/')"
        class="bg-cyan-600 hover:bg-cyan-500 px-4 py-2 rounded-lg font-semibold"
      >
        Ir al login
      </button>

    </div>
  </div>
</template>
