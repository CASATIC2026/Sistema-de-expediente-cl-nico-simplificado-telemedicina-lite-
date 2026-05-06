import { defineStore } from 'pinia'
import { getCitas, getResumen, crearCita as apiCrearCita } from '@/services/api'

export const useCitasStore = defineStore('citas', {
  state: () => ({
    citas: [],
    resumen: {
      pendientes: 0,
      confirmadas: 0,
      canceladas: 0,
      enConsulta: 0,
      finalizadas: 0,
      hoy: 0
    },
    cargando: false,
    usarMocks: false  // Cambiado a false para conectar el backend
  }),

  actions: {
    // ─── CARGAR CITAS DESDE BACKEND ───────────────────────────────
    async cargarCitas() {
      this.cargando = true
      try {
        const data = await getCitas()
        this.citas = data || []
      } catch (error) {
        console.error('Error cargando citas:', error)
        this.citas = []
      } finally {
        this.cargando = false
      }
    },

    // ─── CARGAR RESUMEN (StatCards) ───────────────────────────────
    async cargarResumen() {
      try {
        const data = await getResumen()
        this.resumen = {
          pendientes: data.pendientes  ?? 0,
          confirmadas: data.confirmadas ?? 0,
          canceladas: data.canceladas  ?? 0,
          enConsulta: data.enConsulta  ?? 0,
          finalizadas: data.finalizadas  ?? 0,
          hoy: data.hoy        ?? 0
        }
      } catch (error) {
        console.error('Error cargando resumen:', error)
      }
    },

    // ─── CREAR CITA ───────────────────────────────────────────────
    async crearCita(nuevaCita) {
      try {
        await apiCrearCita(nuevaCita)
        await this.cargarCitas()
      } catch (error) {
        console.error('Error creando cita:', error)
        throw error
      }
    }
  }
})