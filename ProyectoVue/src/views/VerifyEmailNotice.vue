<template>
  <div class="min-h-screen flex items-center justify-center bg-slate-950 text-white p-6">
    <div class="bg-[#1f2e4e] p-8 rounded-2xl shadow-xl text-center max-w-md">

      <h1 class="text-2xl font-bold mb-4">📩 Revisa tu correo</h1>

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
