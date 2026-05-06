<script setup>
import { ref } from 'vue'

import LoginGeneral from '@/componentsDoctor/auth/LoginGeneral.vue'
import formularioRegistro from '@/components/formularioRegistro.vue'

const llamarRegistro = ref(false)

const abrirRegistro = () => {
  llamarRegistro.value = true
}

const cerrarRegistro = () => {
  llamarRegistro.value = false
}
</script>

<template>
  <div class="min-h-screen bg-[#0a0f16] flex items-center justify-center p-4 md:p-8 font-sans">

    <div class="relative w-full max-w-7xl bg-[#111827] rounded-[24px] md:rounded-[40px] shadow-[0_0_50px_rgba(34,211,238,0.15)] border border-white/10 flex flex-col md:flex-row overflow-hidden">

      <!-- ── PANEL IZQUIERDO: Logo y marca ── -->
      <div class="w-full md:w-1/2 px-6 pt-14 pb-6 md:p-12 flex flex-col items-center justify-center relative z-10 gap-4 md:gap-6">

        <!-- Nav: ocupa espacio real en móvil, absoluto en desktop -->
        <nav class="md:absolute md:top-8 flex gap-4 text-[10px] font-bold text-white/60 uppercase tracking-widest self-center">
          <router-link to="/preguntas-frecuentes" class="hover:text-cyan-400 transition-colors">
            Preguntas Frecuentes
          </router-link>
        </nav>

        <!-- Logo: responsive sizing -->
        <img
          src="@/assets/Logo icon.png"
          alt="Logo_icon"
          class="w-36 h-36 sm:w-52 sm:h-52 md:w-80 md:h-80 lg:w-96 lg:h-96 object-contain drop-shadow-[0_0_24px_rgba(34,211,238,0.35)]"
        />

        <!-- Nombre de marca -->
        <div class="text-center">
          <h1 class="text-2xl sm:text-3xl md:text-4xl lg:text-5xl font-extrabold text-white tracking-tight">
            N O V O M E D <span class="text-cyan-400">Lite™</span>
          </h1>
          <p class="text-xs text-white/50 tracking-[0.2em] md:tracking-[0.3em] uppercase mt-2">
            NUESTRA PRIORIDAD ES TU SALUD
          </p>
        </div>

        <!-- Links legales: flujo normal en móvil, absoluto en desktop -->
        <div class="md:absolute md:bottom-8 text-[10px] text-white/30 flex gap-4 mt-2 md:mt-0">
          <router-link to="/privacidad" class="hover:text-cyan-400 transition-colors">Política de Privacidad</router-link>
          <router-link to="/terminos" class="hover:text-cyan-400 transition-colors">Términos de Uso</router-link>
        </div>

      </div>

      <!-- ── PANEL DERECHO: Login ── -->
      <div class="w-full md:w-1/2 bg-white/5 backdrop-blur-sm flex flex-col items-center justify-center px-6 pb-10 pt-4 md:p-8 border-t md:border-t-0 md:border-l border-white/5 relative">

        <LoginGeneral @abrirRegistro="abrirRegistro" />

        <div class="mt-6 md:absolute md:bottom-8 md:mt-0 text-[10px] text-white/30 text-center">
          ©2026-2027 Telmed&Health lite - Todos los derechos reservados.
        </div>

      </div>

    </div>

    <!-- Modal de registro -->
    <formularioRegistro
      v-if="llamarRegistro"
      :esGoogle="false"
      @cerrar="cerrarRegistro"
    />

  </div>
</template>