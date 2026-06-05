<script setup>
import { EnvelopeIcon } from '@heroicons/vue/24/solid'
</script>

<template>
  <div class="min-h-screen flex items-center justify-center bg-slate-950 text-white p-6">
    <div class="bg-[#1f2e4e] p-8 rounded-2xl shadow-xl text-center max-w-md">

      <div class="flex flex-col items-center gap-3 mb-4">

  <EnvelopeIcon
    class="w-14 h-14 text-cyan-400"
  />

  <h1 class="text-2xl font-bold">
    Revisa tu correo
  </h1>

</div>

      <p class="text-slate-300 mb-6">
        Te enviamos un enlace para verificar tu cuenta.
        Debes confirmarlo antes de iniciar sesión.
      </p>

      <button
        @click="$router.push('/')"
        class="bg-cyan-600 hover:bg-cyan-500 px-4 py-2 rounded-lg font-semibold"
      >
        Ir al login
      </button>

    </div>
  </div>
</template>
