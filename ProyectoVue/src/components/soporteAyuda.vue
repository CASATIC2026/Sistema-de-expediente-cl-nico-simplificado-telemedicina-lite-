<script setup>
import { computed, ref } from 'vue'

import {
  ComputerDesktopIcon,
  CalendarDaysIcon,
  VideoCameraIcon,
  DocumentTextIcon,
  ClipboardDocumentListIcon
} from '@heroicons/vue/24/outline'

defineEmits(['cerrar'])

const faqs = ref([
  {
    id: 1,
    categoria: 'Problemas Técnicos',
    icono: ComputerDesktopIcon,
    pregunta: 'Mi cámara o micrófono no funcionan, ¿Qué hago?',
    respuesta: `1. Revisa que el navegador tenga permisos: haz clic en el icono del candado (🔒) junto a la dirección web y activa "Cámara" y "Micrófono".
                2. Asegúrate de que ninguna otra aplicación (como Zoom, Teams o WhatsApp) esté usando la cámara en ese momento.
                3. Si persiste, recarga la página (F5) e intenta ingresar nuevamente.`
  },

  {
    id: 2,
    categoria: 'Sobre mi Cita',
    icono: CalendarDaysIcon,
    pregunta: '¿Cómo puedo cancelar o reprogramar una cita?',
    respuesta: `Ve a la sección "Ver citas" y selecciona la opción "Cancelar" en la cita correspondiente.
                Ten en cuenta que las cancelaciones deben hacerse con al menos 24 horas de
                anticipación para evitar cargos adicionales.`
  },

  {
    id: 3,
    categoria: 'Sobre mi Consulta',
    icono: VideoCameraIcon,
    pregunta: '¿Cómo me uno a mi videollamada?',
    respuesta: `Dirígete a la sección "Ver citas" en tu panel lateral. Allí encontrarás tu próxima consulta programada.
                Diez minutos antes de la hora acordada, se habilitará el botón "Entrar a la consulta".
                Haz clic en él y asegúrate de permitir el acceso a tu cámara y micrófono cuando el navegador lo solicite.`
  },

  {
    id: 4,
    categoria: 'Sobre mi Consulta',
    icono: VideoCameraIcon,
    pregunta: '¿Puedo entrar a la consulta desde mi celular?',
    respuesta: `Sí, nuestra plataforma es compatible con dispositivos móviles.
                Solo asegúrate de usar un navegador actualizado (Chrome o Safari) y tener una conexión a internet estable.`
  },

  {
    id: 5,
    categoria: 'Recetas Médicas',
    icono: DocumentTextIcon,
    pregunta: '¿Dónde descargo mi receta?',
    respuesta: `Una vez finalizada la consulta, ve a la sección "Historial". Busca la fecha de tu atención y haz clic en el icono de "Descargar Receta".
                El documento se abrirá en formato PDF para que puedas guardarlo o imprimirlo.`
  },

  {
    id: 6,
    categoria: 'Sobre mi Historial',
    icono: ClipboardDocumentListIcon,
    pregunta: 'No encuentro mi historial de consultas anteriores.',
    respuesta: `Asegúrate de estar en la cuenta correcta. Si la consulta fue muy reciente,
                puede tardar hasta 5 minutos en aparecer en el historial mientras el sistema procesa el cierre de la sesión médica.`
  }
])

const faqsAgrupadas = computed(() => {
  return faqs.value.reduce((acc, obj) => {

    const clave = obj.categoria

    if (!acc[clave]) {
      acc[clave] = {
        icono: obj.icono,
        items: []
      }
    }

    acc[clave].items.push(obj)

    return acc

  }, {})
})
</script>

<template>

  <div
    class="fixed top-0 right-0 h-full z-9999 bg-[#0a1628] shadow-2xl flex flex-col w-full sm:w-112.5 border-l border-cyan-500/20"
  >

      <!-- HEADER -->
      <div
        class="p-6 border-b border-white/10 flex justify-between items-start bg-linear-to-r from-[#0f2040] to-[#10284f]"
      >

        <div>
          <h3 class="text-2xl font-bold text-white tracking-wide">
            Centro de Ayuda
          </h3>

          <p class="text-sm text-cyan-200/70 mt-1">
            Encuentra soluciones rápidas
          </p>
        </div>

        <button
          @click="$emit('cerrar')"
          class="w-10 h-10 md:w-12 md:h-12 flex items-center justify-center rounded-full bg-red-500/10 hover:bg-red-500/20 border border-red-400/20 text-red-300 hover:text-red-200 text-2xl md:text-3xl transition-all leading-none shrink-0"
        >
          &times;
        </button>

      </div>

      <!-- CUERPO -->
      <div class="flex-1 overflow-y-auto p-6 space-y-8 bg-[#0a1628]">

        <div
          v-for="(grupo, categoria) in faqsAgrupadas"
          :key="categoria"
          class="space-y-3"
        >

          <!-- TITULO CATEGORIA -->
          <h4
            class="flex items-center gap-2 text-xs font-bold uppercase tracking-[0.2em] text-cyan-400 ml-1"
          >

            <component
              :is="grupo.icono"
              class="w-4 h-4"
            />

            {{ categoria }}

          </h4>

          <!-- FAQS -->
          <div
            v-for="faq in grupo.items"
            :key="faq.id"
            class="border border-white/10 rounded-2xl overflow-hidden shadow-lg bg-slate-900/70 backdrop-blur-sm"
          >

            <details class="group">

              <summary
                class="flex justify-between items-center p-4 cursor-pointer hover:bg-white/5 list-none font-medium text-slate-100 transition-colors"
              >

                {{ faq.pregunta }}

                <span class="transition group-open:rotate-180 shrink-0 ml-2">
                  <i class="fa-solid fa-chevron-down text-xs text-cyan-400"></i>
                </span>

              </summary>

              <div
                class="px-4 pb-4 text-sm text-slate-300 leading-relaxed border-t border-white/5 pt-3 whitespace-pre-line"
              >
                {{ faq.respuesta }}
              </div>

            </details>

          </div>

        </div>

      </div>

      <!-- FOOTER -->
      <div class="p-6 border-t border-white/10 bg-[#0f2040] space-y-4">

        <p class="text-sm font-semibold text-white text-center">
          ¿Aún necesitas ayuda?
        </p>

        <div class="grid grid-cols-2 gap-3">

          <!-- LLAMAR -->
          <a
            href="tel:+50300000000"
            class="flex items-center justify-center gap-2 px-4 py-3 bg-white/5 border border-white/10 rounded-xl text-sm font-medium text-slate-200 hover:bg-white/10 transition-all"
          >
            <i class="fa-solid fa-phone text-cyan-400"></i>
            Llamar
          </a>

          <!-- WHATSAPP -->
          <a
            href="https://wa.me/50300000000"
            target="_blank"
            rel="noopener noreferrer"
            class="flex items-center justify-center gap-2 px-4 py-3 bg-green-600 hover:bg-green-500 rounded-xl text-sm font-bold text-white transition-all shadow-lg"
          >
            <i class="fa-brands fa-whatsapp text-lg"></i>
            WhatsApp
          </a>

        </div>

      </div>

    </div>
</template>
