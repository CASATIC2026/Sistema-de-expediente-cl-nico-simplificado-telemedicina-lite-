<script setup>
import { useRouter } from 'vue-router'

const router = useRouter()
</script>

<template>
  <div class="min-h-screen bg-[#0a1628] text-white p-8 relative">

    <!-- Botón volver -->
    <button
      @click="router.push('/')"
      class="absolute flex items-center gap-2 px-4 py-2 text-sm font-medium transition-all border shadow-lg top-6 left-6 rounded-xl bg-slate-900/70 border-white/10 hover:bg-slate-800 text-cyan-300 hover:text-cyan-200"
    >
      ← Volver al inicio
    </button>

    <div class="max-w-4xl mx-auto mt-10">

      <!-- Encabezado -->
      <div class="mb-12 text-center">
        <h1 class="mb-4 text-4xl font-extrabold tracking-tight">
          <span class="text-white">Acerca de</span>
          <span class="ml-3 text-cyan-400">Nosotros</span>
        </h1>

        <p class="text-slate-400">
          Conoce la visión, misión y propósito detrás de TelMed Lite™
        </p>
      </div>

      <!-- Contenido -->
      <div class="space-y-6">

        <!-- Intro -->
        <section class="p-2">
          <p class="leading-relaxed text-center text-slate-300">
            TelMed Lite™ es una plataforma de telemedicina creada para facilitar
            la conexión entre pacientes y profesionales de la salud mediante
            consultas médicas en línea, ofreciendo accesibilidad, seguridad y comodidad.
          </p>
        </section>

        <!-- 1 -->
        <section class="p-6 border bg-slate-900/50 border-white/10 rounded-2xl">
          <h2 class="mb-3 text-xl font-bold text-cyan-300">
            Nuestra misión
          </h2>

          <p class="leading-relaxed text-slate-400">
            Brindar acceso a atención médica de calidad mediante herramientas digitales
            que permitan a pacientes y doctores conectarse de manera segura,
            eficiente y accesible desde cualquier lugar.
          </p>
        </section>

        <!-- 2 -->
        <section class="p-6 border bg-slate-900/50 border-white/10 rounded-2xl">
          <h2 class="mb-3 text-xl font-bold text-cyan-300">
            Nuestra visión
          </h2>

          <p class="leading-relaxed text-slate-400">
            Convertirnos en una plataforma referente en telemedicina,
            contribuyendo a mejorar la calidad de vida de las personas
            mediante servicios médicos digitales modernos y confiables.
          </p>
        </section>

        <!-- 3 -->
        <section class="p-6 border bg-slate-900/50 border-white/10 rounded-2xl">
          <h2 class="mb-3 text-xl font-bold text-cyan-300">
            Nuestros valores
          </h2>

          <ul class="ml-5 space-y-2 leading-relaxed list-disc text-slate-400">
            <li>Confidencialidad en la información médica.</li>
            <li>Accesibilidad para todos los usuarios.</li>
            <li>Profesionalismo en cada consulta.</li>
            <li>Innovación tecnológica aplicada a la salud.</li>
          </ul>
        </section>

        <!-- 4 -->
        <section class="p-6 border bg-slate-900/50 border-white/10 rounded-2xl">
          <h2 class="mb-3 text-xl font-bold text-cyan-300">
            Nuestro propósito
          </h2>

          <p class="leading-relaxed text-slate-400">
            Creemos que la tecnología puede transformar la forma en que las personas
            reciben atención médica, eliminando barreras de distancia y facilitando
            consultas rápidas, seguras y eficientes desde cualquier dispositivo.
          </p>
        </section>

      </div>

      <!-- Footer -->
      <div class="text-sm text-center mt-14 text-slate-500">
        TelMed & Health Lite™ System 2026© <br />
        Desarrollado por J3G Team©
      </div>

    </div>
  </div>
</template>
