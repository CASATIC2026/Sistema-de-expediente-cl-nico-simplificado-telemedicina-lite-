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

    <div class="max-w-4xl mx-auto">

      <!-- Encabezado -->
      <div class="text-center mb-12 mt-10">
        <h1 class="text-4xl font-extrabold tracking-tight mb-4">
          <span class="text-white">Preguntas</span>
          <span class="text-cyan-400 ml-3">Frecuentes</span>
        </h1>

        <p class="text-slate-400">
          Todo lo que necesitas saber sobre tu plataforma de telemedicina TelMed Lite™
        </p>
      </div>

      <!-- FAQ -->
      <div class="space-y-4">

        <!-- Pregunta 1 -->
        <details class="group bg-slate-900/50 border border-white/10 rounded-2xl p-6 transition-all hover:bg-slate-800/50 cursor-pointer">
          <summary class="flex justify-between items-center font-bold text-lg text-cyan-50 list-none">
            ¿Cómo funciona la plataforma?

            <span class="text-cyan-400 transition group-open:rotate-180">
              <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24"
                stroke-width="2" stroke="currentColor" class="w-6 h-6">
                <path stroke-linecap="round" stroke-linejoin="round"
                  d="m19.5 8.25-7.5 7.5-7.5-7.5" />
              </svg>
            </span>
          </summary>

          <p class="mt-4 text-slate-400 leading-relaxed">
            Es sencillo: te registras, validas tu cuenta, inicias sesión y agendas tu cita.<br />
            Al agendar la cita, se creará un enlace único para recibir tu consulta mediante Jitsi Meet.<br />
            Serás atendido por un médico del personal y, al finalizar la consulta,
            tendrás acceso a tu receta médica en PDF.
          </p>
        </details>

        <!-- Pregunta 2 -->
        <details class="group bg-slate-900/50 border border-white/10 rounded-2xl p-6 transition-all hover:bg-slate-800/50 cursor-pointer">
          <summary class="flex justify-between items-center font-bold text-lg text-cyan-50 list-none">
            ¿Cómo me registro?

            <span class="text-cyan-400 transition group-open:rotate-180">
              <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24"
                stroke-width="2" stroke="currentColor" class="w-6 h-6">
                <path stroke-linecap="round" stroke-linejoin="round"
                  d="m19.5 8.25-7.5 7.5-7.5-7.5" />
              </svg>
            </span>
          </summary>

          <p class="mt-4 text-slate-400 leading-relaxed">
            • Haz click en "Registrarse" en la página principal.<br />
            • Completa el formulario con tus datos.<br />
            • Recibirás un correo de verificación.<br />
            • Una vez validada tu cuenta, podrás iniciar sesión y usar la plataforma.
          </p>
        </details>

        <!-- Pregunta 3 -->
        <details class="group bg-slate-900/50 border border-white/10 rounded-2xl p-6 transition-all hover:bg-slate-800/50 cursor-pointer">
          <summary class="flex justify-between items-center font-bold text-lg text-cyan-50 list-none">
            ¿Tiene un costo el uso de la plataforma?

            <span class="text-cyan-400 transition group-open:rotate-180">
              <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24"
                stroke-width="2" stroke="currentColor" class="w-6 h-6">
                <path stroke-linecap="round" stroke-linejoin="round"
                  d="m19.5 8.25-7.5 7.5-7.5-7.5" />
              </svg>
            </span>
          </summary>

          <p class="mt-4 text-slate-400 leading-relaxed">
            No, el uso de la plataforma es completamente gratuito.
          </p>
        </details>

        <!-- Pregunta 4 -->
        <details class="group bg-slate-900/50 border border-white/10 rounded-2xl p-6 transition-all hover:bg-slate-800/50 cursor-pointer">
          <summary class="flex justify-between items-center font-bold text-lg text-cyan-50 list-none">
            ¿Mis datos y recetas están seguros?

            <span class="text-cyan-400 transition group-open:rotate-180">
              <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24"
                stroke-width="2" stroke="currentColor" class="w-6 h-6">
                <path stroke-linecap="round" stroke-linejoin="round"
                  d="m19.5 8.25-7.5 7.5-7.5-7.5" />
              </svg>
            </span>
          </summary>

          <p class="mt-4 text-slate-400 leading-relaxed">
            Sí. Toda la información se encuentra protegida y cifrada.
          </p>
        </details>

        <!-- Pregunta 5 -->
        <details class="group bg-slate-900/50 border border-white/10 rounded-2xl p-6 transition-all hover:bg-slate-800/50 cursor-pointer">
          <summary class="flex justify-between items-center font-bold text-lg text-cyan-50 list-none">
            ¿Cómo puedo agendar una cita médica?

            <span class="text-cyan-400 transition group-open:rotate-180">
              <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24"
                stroke-width="2" stroke="currentColor" class="w-6 h-6">
                <path stroke-linecap="round" stroke-linejoin="round"
                  d="m19.5 8.25-7.5 7.5-7.5-7.5" />
              </svg>
            </span>
          </summary>

          <p class="mt-4 text-slate-400 leading-relaxed">
            Después de iniciar sesión, podrás acceder al panel de citas y seleccionar
            médico, fecha y horario disponible.
          </p>
        </details>

        <!-- Pregunta 6 -->
        <details class="group bg-slate-900/50 border border-white/10 rounded-2xl p-6 transition-all hover:bg-slate-800/50 cursor-pointer">
          <summary class="flex justify-between items-center font-bold text-lg text-cyan-50 list-none">
            ¿Qué hago si tengo problemas técnicos?

            <span class="text-cyan-400 transition group-open:rotate-180">
              <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24"
                stroke-width="2" stroke="currentColor" class="w-6 h-6">
                <path stroke-linecap="round" stroke-linejoin="round"
                  d="m19.5 8.25-7.5 7.5-7.5-7.5" />
              </svg>
            </span>
          </summary>

          <p class="mt-4 text-slate-400 leading-relaxed">
            Puedes contactar al equipo de soporte mediante el formulario de contacto
            disponible en la plataforma cuando inicies sesión.
          </p>
        </details>

        <!-- Pregunta 7 -->
        <details class="group bg-slate-900/50 border border-white/10 rounded-2xl p-6 transition-all hover:bg-slate-800/50 cursor-pointer">
          <summary class="flex justify-between items-center font-bold text-lg text-cyan-50 list-none">
            ¿Qué dispositivo necesito para la consulta?

            <span class="text-cyan-400 transition group-open:rotate-180">
              <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24"
                stroke-width="2" stroke="currentColor" class="w-6 h-6">
                <path stroke-linecap="round" stroke-linejoin="round"
                  d="m19.5 8.25-7.5 7.5-7.5-7.5" />
              </svg>
            </span>
          </summary>

          <p class="mt-4 text-slate-400 leading-relaxed">
            Puedes usar una computadora, tablet o móvil con acceso a internet, micrófono y opcionalmente cámara.<br />
          </p>
        </details>

         <details class="group bg-slate-900/50 border border-white/10 rounded-2xl p-6 transition-all hover:bg-slate-800/50 cursor-pointer">
          <summary class="flex justify-between items-center font-bold text-lg text-cyan-50 list-none">
            ¿Necesito instalar alguna aplicación adicional?

            <span class="text-cyan-400 transition group-open:rotate-180">
              <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24"
                stroke-width="2" stroke="currentColor" class="w-6 h-6">
                <path stroke-linecap="round" stroke-linejoin="round"
                  d="m19.5 8.25-7.5 7.5-7.5-7.5" />
              </svg>
            </span>
          </summary>

          <p class="mt-4 text-slate-400 leading-relaxed">
            No es necesario ya que la consulta se realiza mediante Jitsi Meet,<br />
            sin embargo, puedes instalar la app oficial de Jitsi en tu dispositivo para una mejor experiencia.
          </p>
        </details>

      </div>

      <!-- Footer -->
      <div class="mt-14 text-center text-sm text-slate-500">
        Desarrollado por J3G Team©
      </div>

    </div>
  </div>
</template>