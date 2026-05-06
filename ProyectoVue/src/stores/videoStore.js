import { defineStore } from 'pinia'
import { ref } from 'vue'

export const useVideoStore = defineStore('video', () => {
  const isActive    = ref(false)
  const isMinimized = ref(false)
  const roomName    = ref('')
  const citaId      = ref(null)

  function iniciarLlamada(room, id) {
    roomName.value    = room
    citaId.value      = id
    isActive.value    = true
    isMinimized.value = false

    localStorage.setItem('activeCall', JSON.stringify({
      roomName: room,
      citaId: id
    }))
  }

  function minimizar()  { isMinimized.value = true  }
  function expandir()   { isMinimized.value = false }

  function finalizarLlamada() {
    isActive.value    = false
    isMinimized.value = false
    roomName.value    = ''
    citaId.value      = null
    localStorage.removeItem('activeCall')
  }

  function recuperarLlamada() {
  const saved = localStorage.getItem('activeCall')
  if (saved) {
    const data = JSON.parse(saved)
    roomName.value = data.roomName
    citaId.value   = data.citaId
    isActive.value = true
  }
}

  return { isActive, isMinimized, roomName, citaId,
           iniciarLlamada, minimizar, expandir, finalizarLlamada, recuperarLlamada }
})
