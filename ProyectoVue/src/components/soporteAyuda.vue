<script setup>
import { computed, ref } from 'vue';


const emit = defineEmits(['cerrar']);

const faqs = ref([
  { id: 1, 
    categoria: '💻 Problemas Técnicos',
    pregunta: 'Mi cámara o micrófono no funcionan, ¿Qué hago?', 
    respuesta: `1. Revisa que el navegador tenga permisos: haz clic en el icono del candado (🔒) junto a la dirección web y activa "Cámara" y "Micrófono". 
                2. Asegúrate de que ninguna otra aplicación (como Zoom, Teams o WhatsApp) esté usando la cámara en ese momento.
                3. Si persiste, recarga la página (F5) e intenta ingresar nuevamente.`
  },
    
  { id: 2,
    categoria: '📅 Sobre mi Cita',
    pregunta: '¿Cómo puedo cancelar o reprogramar una cita?',
    respuesta: `Ve a la sección "Ver citas" y selecciona la opción "Cancelar" en la cita correspondiente. 
                Ten en cuenta que las cancelaciones deben hacerse con al menos 24 horas de 
                anticipación para evitar cargos adicionales.`
  },

  { id: 3,
    categoria: '📅 Sobre mi consulta',
    pregunta: '¿Cómo me uno a mi videollamada?',
    respuesta: `Dirígete a la sección "Ver citas" en tu panel lateral. Allí encontrarás tu próxima consulta programada.
                Diez minutos antes de la hora acordada, se habilitará el botón "Entrar a la consulta".
                Haz clic en él y asegúrate de permitir el acceso a tu cámara y micrófono cuando el navegador lo solicite.`
  },

  { id: 3,
    categoria: '📅 Sobre mi consulta',
    pregunta: '¿Puedo entrar a la consulta desde mi celular?',
    respuesta: `Sí, nuestra plataforma es compatible con dispositivos móviles.
                Solo asegúrate de usar un navegador actualizado (Chrome o Safari) y tener una conexión a internet estable.`
  },

  { id: 4, 
    categoria: '📄 Recetas y Médica',
    pregunta: '¿Dónde descargo mi receta?', 
    respuesta: `Una vez finalizada la consulta, ve a la sección  "Historial". Busca la fecha de tu atención y haz clic en el icono de "Descargar Receta".
                El documento se abrirá en formato PDF para que puedas guardarlo o imprimirlo.` 
  },
  { id: 4, 
    categoria: '📄 Sobre mi historial',
    pregunta:  'No encuentro mi historial de consultas anteriores.', 
    respuesta: `Asegúrate de estar en la cuenta correcta. Si la consulta fue muy reciente,
                puede tardar hasta 5 minutos en aparecer en el historial mientras el sistema procesa el cierre de la sesión médica` 
  }
]);

// Lógica para agrupar las preguntas por categoría automáticamente
const faqsAgrupadas = computed(() => {
  return faqs.value.reduce((acc, obj) => {
    const clave = obj.categoria;
    if (!acc[clave]) acc[clave] = [];
    acc[clave].push(obj);
    return acc;
  }, {});
});
</script>


<template>
    <Transition 
      enter-active-class="transition transform duration-300 ease-in-out"
      enter-from-class="translate-x-full"
      enter-to-class="translate-x-0"
      leave-active-class="transition transform duration-200 ease-in-out"
      leave-from-class="translate-x-0"
      leave-to-class="translate-x-full"
    >
      <div 
           :style="{backgroundColor: 'white', position: 'fixed', top: '0', right: '0', height: '100%', zIndex: '9999' }"
           class="shadow-2xl flex flex-col w-full sm:w-[450px]">
           
        <div class="p-6 border-b border-gray-100 flex justify-between items-start bg-white">
          <div>
            <h3 class="text-xl font-bold text-slate-800">Centro de Ayuda</h3>
            <p class="text-sm text-slate-500 mt-1">Encuentra soluciones rápidas</p>
          </div>
          <button @click="$emit('cerrar')" class="text-red-400 hover:text-slate-600 text-4xl md:text-5xl lg:text-6xl transition-all p-1 leading-none">
            &times;
          </button>
        </div>

        <div class="flex-1 overflow-y-auto p-6 space-y-8 bg-white">
          <div v-for="(grupo, categoria) in faqsAgrupadas" :key="categoria" class="space-y-3">
            <h4 class="text-xs font-bold uppercase tracking-wider text-slate-400 ml-1">
              {{ categoria }}
            </h4>
            
            <div v-for="faq in grupo" :key="faq.id" class="border border-slate-100 rounded-xl overflow-hidden shadow-sm bg-white">
              <details class="group">
                <summary class="flex justify-between items-center p-4 cursor-pointer hover:bg-slate-50 list-none font-medium text-slate-700">
                  {{ faq.pregunta }}
                  <span class="transition group-open:rotate-180">
                    <i class="fa-solid fa-chevron-down text-xs text-slate-400"></i>
                  </span>
                </summary>
                <div class="px-4 pb-4 text-sm text-slate-600 leading-relaxed border-t border-slate-50 pt-3">
                  {{ faq.respuesta }}
                </div>
              </details>
            </div>
          </div>
        </div>

        <div class="p-6 border-t border-gray-100 bg-slate-50 space-y-4">
          <p class="text-sm font-semibold text-slate-700 text-center">¿Aún necesitas ayuda?</p>
          <div class="grid grid-cols-2 gap-3">
            <a href="tel:+57300000000" 
               class="flex items-center justify-center gap-2 px-4 py-3 bg-white border border-slate-200 rounded-xl text-sm font-medium text-slate-700 hover:bg-slate-50 transition-colors">
              <i class="fa-solid fa-phone text-emerald-500"></i> Llamar
            </a>
            <a href="#" 
               class="flex items-center justify-center gap-2 px-4 py-3 bg-[#25D366] text-white rounded-xl text-sm font-medium hover:bg-opacity-90 transition-colors text-center">
              <i class="fa-brands fa-whatsapp"></i> WhatsApp
            </a>
          </div>
        </div>
      </div>
    </Transition>
</template>
