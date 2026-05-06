<script setup>
import { ref, watch, nextTick, onUnmounted } from 'vue'
import { useVideoStore } from '@/stores/videoStore'

const videoStore = useVideoStore()
const jitsiContainer = ref(null)
let jitsiApi = null

// FINALIZAR CONSULTA
function finalizarConsulta() {
  if (jitsiApi) {
    jitsiApi.dispose()
    jitsiApi = null
  }
  videoStore.finalizarLlamada()
}

// INICIALIZAR / DESTRUIR JITSI
watch(() => videoStore.isActive, async (val) => {
  if (val) {
    await nextTick()
    if (jitsiContainer.value && !jitsiApi) {
      jitsiApi = new window.JitsiMeetExternalAPI("meet.jit.si", {
        roomName: videoStore.roomName,
        parentNode: jitsiContainer.value,
        width: '100%',
        height: '100%',
        configOverwrite: {
          prejoinPageEnabled: false,
          disableDeepLinking: true
        }
      })

      jitsiApi.addEventListener('readyToClose', () => finalizarConsulta())
    }
  } else {
    finalizarConsulta()
  }
}, { immediate: true })

onUnmounted(() => {
  if (jitsiApi) jitsiApi.dispose()
})
</script>

<template>
  <Teleport to="body">
    <div
      v-if="videoStore.isActive"
      class="fixed left-4 top-1/2 -translate-y-1/2 z-[9999] w-[520px] h-[400px] shadow-2xl rounded-xl border border-white/20 flex flex-col bg-[#1a2e4a] overflow-hidden"
    >
      <!-- HEADER -->
      <div class="h-10 bg-black/50 flex items-center justify-between px-3 shrink-0">
        <span class="text-white text-[10px] font-bold uppercase tracking-wider flex items-center gap-2">
          <span class="w-2 h-2 bg-red-500 rounded-full animate-ping"></span>
          Consulta en vivo
        </span>

        <button
          @click.stop="finalizarConsulta"
          class="bg-red-600 text-white text-[10px] px-2 py-1 rounded font-bold hover:bg-red-700"
        >
          FINALIZAR
        </button>
      </div>

      <!-- VIDEO -->
      <div
        ref="jitsiContainer"
        class="flex-grow bg-slate-900"
      ></div>
    </div>
  </Teleport>
</template>
