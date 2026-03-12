<script setup>
import { ref } from 'vue'
const emit = defineEmits(['cerrar']);

const usuario = {
  nombre: "Juan Pérez",
  id: "USR-98765"
}

const consultaSeleccionada = ref(null)

const historial = ref([
  { 
    id: 101, 
    fecha: "22-10-2025", 
    doctor: "Dr. Roberto García", 
    especialidad: "Dermatólogo",
    diagnostico: "Paciente presenta dermatitis por contacto. Se recomienda evitar jabones fuertes y usar cremas hidratantes neutras.",
    urlReceta: "#" 
  },
  { 
    id: 98, 
    fecha: "24-11-2025", 
    doctor: "Dra. Elena Torres", 
    especialidad: "General",
    diagnostico: "Gripe común. Reposo y abundante hidratación por 3 días.",
    urlReceta: "#" 
  }
])

const verDetalle = (consulta) => {
  consultaSeleccionada.value = consulta
}
</script>

<template>
  <div class="fixed inset-0 z-[99999] flex items-center justify-center bg-slate-900/80 backdrop-blur-sm p-4" @click.self="emit('cerrar')">
    
    <div class="relative w-full max-w-4xl max-h-[90vh] md:h-auto overflow-hidden rounded-2xl bg-gradient-to-br from-[#023c43] to-[#047a85] shadow-2xl flex flex-col text-white animate-in fade-in zoom-in duration-300">
      
      <button 
        @click="emit('cerrar')" 
        class="absolute top-4 right-4 z-20 flex h-10 w-10 items-center justify-center rounded-full bg-white/10 hover:bg-red-500 transition-colors text-2xl"
      >
        &times;
      </button>

      <header class="p-6 md:p-8 pb-2">
        <h2 v-if="!consultaSeleccionada" class="text-2xl md:text-3xl font-bold">Historial Médico</h2>
        <div v-else class="flex flex-col gap-2">
           <button @click="consultaSeleccionada = null" class="w-fit text-sm font-semibold bg-white/20 hover:bg-white/30 px-4 py-2 rounded-lg transition-all">
            ← Regresar al listado
          </button>
          <h3 class="text-xl md:text-2xl font-bold">Detalle de Consulta #{{ consultaSeleccionada.id }}</h3>
        </div>
      </header>

      <div class="flex-1 overflow-y-auto p-6 md:p-8 pt-4 custom-scrollbar">
        
        <div v-if="!consultaSeleccionada" class="space-y-4">
          <div v-for="consulta in historial" :key="consulta.id" 
            class="flex flex-col md:flex-row md:items-center justify-between p-5 rounded-xl bg-white/5 border border-white/10 hover:bg-white/10 transition-all gap-4"
          >
            <div class="space-y-1">
              <span class="inline-block px-3 py-1 text-xs font-bold bg-sky-500 rounded-full mb-1 uppercase tracking-wider">
                {{ consulta.fecha }}
              </span>
              <p class="text-lg font-semibold">Dr/a: {{ consulta.doctor }}</p>
              <p class="text-white/70 text-sm font-medium">{{ consulta.especialidad }}</p>
            </div>
            <button @click="verDetalle(consulta)" 
              class="w-full md:w-auto px-6 py-3 bg-cyan-500 hover:bg-cyan-400 font-bold rounded-lg transition-colors shadow-lg shadow-cyan-900/20"
            >
              Ver Detalles
            </button>
          </div>
        </div>

        <div v-else class="space-y-6">
          <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
            <section class="p-5 rounded-xl bg-black/20 border border-white/5">
              <h4 class="text-cyan-300 font-bold mb-3 uppercase text-xs tracking-widest">Información del Paciente</h4>
              <p class="text-sm"><span class="text-white/60">Nombre:</span> {{ usuario.nombre }}</p>
              <p class="text-sm"><span class="text-white/60">ID Usuario:</span> {{ usuario.id }}</p>
            </section>

            <section class="p-5 rounded-xl bg-black/20 border border-white/5">
              <h4 class="text-cyan-300 font-bold mb-3 uppercase text-xs tracking-widest">Atendido por:</h4>
              <p class="font-bold text-lg leading-tight">{{ consultaSeleccionada.doctor }}</p>
              <p class="text-sm text-white/70">{{ consultaSeleccionada.especialidad }}</p>
            </section>
          </div>

          <section>
            <h4 class="text-cyan-300 font-bold mb-3 uppercase text-xs tracking-widest">Diagnóstico / Resumen:</h4>
            <div class="p-6 rounded-xl bg-white text-slate-800 leading-relaxed shadow-inner">
              {{ consultaSeleccionada.diagnostico }}
            </div>
          </section>

          <footer class="pt-4">
            <a :href="consultaSeleccionada.urlReceta" download
              class="flex items-center justify-center gap-3 w-full p-4 bg-emerald-500 hover:bg-emerald-400 font-bold rounded-xl transition-all transform hover:scale-[1.01] active:scale-95 shadow-lg shadow-emerald-900/30"
            >
              <span class="text-xl">📄</span> Descargar Receta Médica (PDF)
            </a>
          </footer>
        </div>
      </div>

    </div>
  </div>
</template>

<style scoped>
/* Estilo para una scrollbar más fina y estética en Chrome/Safari */
.custom-scrollbar::-webkit-scrollbar {
  width: 6px;
}
.custom-scrollbar::-webkit-scrollbar-track {
  background: rgba(255, 255, 255, 0.05);
}
.custom-scrollbar::-webkit-scrollbar-thumb {
  background: rgba(255, 255, 255, 0.2);
  border-radius: 10px;
}
.custom-scrollbar::-webkit-scrollbar-thumb:hover {
  background: rgba(255, 255, 255, 0.3);
}
</style>