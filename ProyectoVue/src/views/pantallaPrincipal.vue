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

      <!-- ── PANEL IZQUIERDO-->
      <div class="relative z-10 flex flex-col items-center justify-center w-full gap-4 px-6 pb-6 md:w-1/2 pt-14 md:p-12 md:gap-6">

        <!-- Nav-->
        <nav class="md:absolute md:top-8 flex gap-4 text-[10px] font-bold text-white/60 uppercase tracking-widest self-center">
          <router-link to="/preguntas-frecuentes" class="transition-colors hover:text-cyan-400">
            Preguntas Frecuentes
          </router-link>
        </nav>


        <img
          src="@/assets/Logo icon.png"
          alt="Logo_icon"
          class="w-36 h-36 sm:w-52 sm:h-52 md:w-80 md:h-80 lg:w-96 lg:h-96 object-contain drop-shadow-[0_0_24px_rgba(34,211,238,0.35)]"
        />

        <!-- Nombre de marca -->
        <div class="text-center">
          <h1 class="text-2xl font-extrabold tracking-tight text-white sm:text-3xl md:text-4xl lg:text-5xl">
            TelMed <span class="text-cyan-400">Lite™</span>
          </h1>
          <p class="text-xs text-white/50 tracking-[0.2em] md:tracking-[0.3em] uppercase mt-2">
            CONECTA CON TU SALUD<br/>
            HASTA DONDE ESTÉS
          </p>
        </div>


        <div class="md:absolute md:bottom-8 text-[10px] text-white/30 flex gap-4 mt-2 md:mt-0">
          <router-link to="/privacidad" class="transition-colors hover:text-cyan-400">Política De Privacidad</router-link>
          <router-link to="/terminos" class="transition-colors hover:text-cyan-400">Términos De Uso</router-link>
          <router-link to="/acerca-de-nosotros" class="transition-colors hover:text-cyan-400">Acerca De Nosotros</router-link>
        </div>

      </div>

      <!-- -->
      <div class="relative flex flex-col items-center justify-center w-full px-6 pt-4 pb-10 border-t md:w-1/2 bg-white/5 backdrop-blur-sm md:p-8 md:border-t-0 md:border-l border-white/5">

        <LoginGeneral @abrirRegistro="abrirRegistro" />

        <div class="mt-6 md:absolute md:bottom-8 md:mt-0 text-[10px] text-white/30 text-center">
          ©2026 Telmed & Health Lite™ System - Todos los derechos reservados.
        <br />
          Desarrollado por J3G Team©
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
