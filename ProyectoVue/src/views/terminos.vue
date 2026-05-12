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
          <span class="text-white">Términos y</span>
          <span class="ml-3 text-cyan-400">Condiciones</span>
        </h1>

        <p class="text-slate-400">
          Normas y condiciones para el uso de la plataforma TelMed Lite™
        </p>
      </div>

      <!-- Contenido -->
      <div class="space-y-6">

        <!-- Intro -->
       <section class="p-2">
        <p class="leading-relaxed text-center text-slate-300">
          Al utilizar la plataforma TelMed Lite™, usted acepta los siguientes
          términos y condiciones<br />
          relacionados con el uso del servicio:
        </p>
      </section>

        <!-- 1 -->
        <section class="p-6 border bg-slate-900/50 border-white/10 rounded-2xl">
          <h2 class="mb-3 text-xl font-bold text-cyan-300">
            1. Uso del servicio
          </h2>

          <p class="leading-relaxed text-slate-400">
            La plataforma está destinada a facilitar consultas médicas en línea
            entre pacientes y profesionales de salud autorizados.
          </p>
        </section>

        <!-- 2 -->
        <section class="p-6 border bg-slate-900/50 border-white/10 rounded-2xl">
          <h2 class="mb-3 text-xl font-bold text-cyan-300">
            2. Responsabilidad médica
          </h2>

          <p class="leading-relaxed text-slate-400">
            Los profesionales de salud son responsables de la información,
            diagnósticos y recomendaciones proporcionadas durante las consultas.
          </p>
        </section>

        <!-- 3 -->
        <section class="p-6 border bg-slate-900/50 border-white/10 rounded-2xl">
          <h2 class="mb-3 text-xl font-bold text-cyan-300">
            3. Uso adecuado
          </h2>

          <p class="leading-relaxed text-slate-400">
            El usuario se compromete a utilizar la plataforma de forma
            responsable y a proporcionar información veraz y actualizada.
          </p>
        </section>

        <!-- 4 -->
        <section class="p-6 border bg-slate-900/50 border-white/10 rounded-2xl">
          <h2 class="mb-3 text-xl font-bold text-cyan-300">
            4. Modificaciones
          </h2>

          <p class="leading-relaxed text-slate-400">
            TelMed Lite™ se reserva el derecho de modificar estos términos y
            condiciones en cualquier momento para mejorar el servicio o adaptarse
            a nuevos requerimientos legales.
          </p>
        </section>

      </div>

      <!-- Footer -->
      <div class="text-sm text-center mt-14 text-slate-500">
        Última actualización: 2026 <br />
        Desarrollado por J3G Team©
      </div>

    </div>
  </div>
</template>
