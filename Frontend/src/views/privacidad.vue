<script setup>
import { useRouter } from 'vue-router'

const router = useRouter()
</script>

<template>
  <div class="min-h-screen bg-[#0a1628] text-white p-8 relative">

    <!-- Botón volver -->
    <button
      @click="router.push('/')"
      class="absolute top-6 left-6 flex items-center gap-2 px-4 py-2 rounded-xl bg-slate-900/70 border border-white/10 hover:bg-slate-800 transition-all text-sm font-medium text-cyan-300 hover:text-cyan-200 shadow-lg"
    >
      ← Volver al inicio
    </button>

    <div class="max-w-4xl mx-auto mt-10">

      <!-- Encabezado -->
      <div class="text-center mb-12">
        <h1 class="text-4xl font-extrabold tracking-tight mb-4">
          <span class="text-white">Política de</span>
          <span class="text-cyan-400 ml-3">Privacidad</span>
        </h1>

        <p class="text-slate-400">
          Cómo protegemos y utilizamos la información dentro de TelMed Lite™
        </p>
      </div>

      <!-- Contenido -->
      <div class="space-y-6">

        <!-- Sección -->
        <section class="p-2">
        <p class="text-slate-300 leading-relaxed text-center">
          En TelMed Lite™ respetamos y protegemos la privacidad de nuestros usuarios.<br />
          Esta política describe cómo recopilamos, utilizamos y protegemos su información personal dentro de la plataforma.
        </p>
      </section>

        <!-- 1 -->
        <section class="bg-slate-900/50 border border-white/10 rounded-2xl p-6">
          <h2 class="text-xl font-bold text-cyan-300 mb-3">
            1. Información que recopilamos
          </h2>

          <p class="text-slate-400 leading-relaxed">
            Podemos recopilar información como nombre, correo electrónico,
            teléfono y datos necesarios para la atención médica en línea.
          </p>
        </section>

        <!-- 2 -->
        <section class="bg-slate-900/50 border border-white/10 rounded-2xl p-6">
          <h2 class="text-xl font-bold text-cyan-300 mb-3">
            2. Uso de la información
          </h2>

          <p class="text-slate-400 leading-relaxed">
            La información recopilada se utiliza para ofrecer servicios de
            telemedicina, gestionar consultas médicas y mejorar la experiencia
            del usuario dentro de la plataforma.
          </p>
        </section>

        <!-- 3 -->
        <section class="bg-slate-900/50 border border-white/10 rounded-2xl p-6">
          <h2 class="text-xl font-bold text-cyan-300 mb-3">
            3. Protección de datos
          </h2>

          <p class="text-slate-400 leading-relaxed">
            Implementamos medidas de seguridad para proteger la información
            contra accesos no autorizados, pérdidas o alteraciones indebidas.
          </p>
        </section>

        <!-- 4 -->
        <section class="bg-slate-900/50 border border-white/10 rounded-2xl p-6">
          <h2 class="text-xl font-bold text-cyan-300 mb-3">
            4. Compartir información
          </h2>

          <p class="text-slate-400 leading-relaxed">
            No compartimos información personal con terceros, excepto cuando sea
            necesario para la prestación del servicio o requerido legalmente.
          </p>
        </section>

      </div>

      <!-- Footer -->
      <div class="mt-14 text-center text-sm text-slate-500">
        Última actualización: 2026 <br />
        Desarrollado por J3G Team©
      </div>

    </div>
  </div>
</template>