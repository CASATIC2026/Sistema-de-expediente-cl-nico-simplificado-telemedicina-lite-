<script setup>
import { ref } from 'vue';
const emit = defineEmits(['cerrar']);

const props = defineProps({
  esSecretaria: {
    type: Boolean,
    default: true
  }
});

const textoBusqueda = ref('');
const pacienteSeleccionado = ref(null);

const form = ref({
  pacienteID: '',
  doctorID: '',
  fecha: '',
  hora: '',
  tipo: '',
  motivo: '',
  estado: 'Pendiente'
});

const listaPacientes = ref([
  { id: 10, nombre: "Paciente x , apellido y", dui: "100000001" },
  { id: 25, nombre: "Paciente y, Apellido y", dui: "200000002" },
  { id: 42, nombre: "Paciente z, Apellido z", dui: "300000003" }
]);

const vincularPaciente = () => {
  const encontrado = listaPacientes.value.find(p => p.dui === textoBusqueda.value);
  if (encontrado) {
    form.value.pacienteID = encontrado.id;
    pacienteSeleccionado.value = encontrado;
  } else {
    form.value.pacienteID = '';
    pacienteSeleccionado.value = null;
  }
};

const guardarCita = () => {
  if (props.esSecretaria && !form.value.pacienteID) {
    alert("Por favor, seleccione un paciente válido de la lista.");
    return;
  }
  console.log("Enviando a la DB:", form.value);
  alert("¡Cita agendada con éxito!");
};
</script>

<template>
  <div class="fixed inset-0 z-[99998] flex items-center justify-center bg-cyan-900/60 backdrop-blur-sm p-4" @click.self="emit('cerrar')">
    
    <div class="relative w-full max-w-lg bg-gradient-to-br from-[#024047] to-[#04c4c4] rounded-2xl shadow-2xl p-6 md:p-8 overflow-y-auto max-h-[95vh] animate-in fade-in zoom-in duration-200">
      
      <button 
        type="button" 
        class="absolute top-4 right-4 text-white/70 hover:text-white text-3xl leading-none transition-colors"
        @click="emit('cerrar')"
      >
        &times;
      </button>
      
      <header class="mb-6 pr-6">
        <h2 class="text-white text-xl md:text-2xl font-bold leading-tight">
          {{ esSecretaria ? 'Panel Administrativo: Agendar Cita' : 'Complete los campos para agendar su cita' }}
        </h2>
      </header>

      <form @submit.prevent="guardarCita" class="space-y-5">
        
        <div v-if="esSecretaria" class="flex flex-col space-y-2">
          <label for="pacienteBusqueda" class="text-sm font-semibold text-white/90">Buscar paciente por su DUI</label>
          <input 
            list="pacientesSugeridos" 
            id="pacienteBusqueda" 
            v-model="textoBusqueda"
            @change="vincularPaciente"
            placeholder="Ingrese un N° identificación (DUI)..."
            class="w-full p-3 rounded-lg border-none bg-white text-slate-800 focus:ring-2 focus:ring-cyan-300 outline-none transition-all text-base"
            required
          >
          <datalist id="pacientesSugeridos">
            <option v-for="p in listaPacientes" :key="p.id" :value="p.dui">
              {{ p.nombre }}
            </option>
          </datalist>
          
          <div class="min-h-[20px]">
            <span v-if="pacienteSeleccionado" class="text-emerald-300 text-xs font-bold flex items-center gap-1">
              ✅ Seleccionado: {{ pacienteSeleccionado.nombre }}
            </span>
            <span v-else-if="textoBusqueda" class="text-rose-300 text-xs font-bold flex items-center gap-1">
              ❌ Paciente no encontrado
            </span>
          </div>
          <input type="hidden" v-model="form.pacienteID">
        </div>

        <div class="flex flex-col space-y-2">
          <label for="doctorID" class="text-sm font-semibold text-white/90">Doctor disponible</label>
          <select v-model="form.doctorID" id="doctorID" class="w-full p-3 rounded-lg border-none bg-white text-slate-800 focus:ring-2 focus:ring-cyan-300 outline-none appearance-none cursor-pointer" required>
            <option value="" disabled>-- Seleccione un doctor --</option>
            <option value="1">Doctor abcde efghihk</option>
          </select>
        </div>

        <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div class="flex flex-col space-y-2">
            <label for="fecha" class="text-sm font-semibold text-white/90">Seleccionar fecha</label>
            <input type="date" v-model="form.fecha" id="fecha" class="w-full p-3 rounded-lg border-none bg-white text-slate-800 focus:ring-2 focus:ring-cyan-300 outline-none" required>
          </div>

          <div class="flex flex-col space-y-2">
            <label for="hora" class="text-sm font-semibold text-white/90">Seleccionar horario</label>
            <select v-model="form.hora" id="hora" class="w-full p-3 rounded-lg border-none bg-white text-slate-800 focus:ring-2 focus:ring-cyan-300 outline-none appearance-none cursor-pointer" required>
              <option value="" disabled>--- Horarios ---</option>
              <option value="08:30">08:30-9:00 AM</option>
              <option value="09:00">09:00-9:30 AM</option>
              <option value="09:30">9:30-10:00 AM</option>
              <option value="10:00">10:00-10:30 AM</option>
            </select>
          </div>
        </div>

        <div class="flex flex-col space-y-2">
          <label for="tipo" class="text-sm font-semibold text-white/90">Tipo de consulta</label>
          <select v-model="form.tipo" id="tipo" class="w-full p-3 rounded-lg border-none bg-white text-slate-800 focus:ring-2 focus:ring-cyan-300 outline-none appearance-none cursor-pointer" required>
            <option value="" disabled>--- Seleccionar ---</option>
            <option value="primera">Primera vez</option>
            <option value="seguimiento">Consulta de seguimiento</option>
          </select>
        </div>

        <div class="flex flex-col space-y-2">
          <label for="motivo" class="text-sm font-semibold text-white/90">Motivo de la consulta</label>
          <textarea 
            v-model="form.motivo" 
            id="motivo" 
            rows="2"
            placeholder="Breve descripción..."
            class="w-full p-3 rounded-lg border-none bg-white text-slate-800 focus:ring-2 focus:ring-cyan-300 outline-none resize-none"
            required
          ></textarea>
        </div>

        <button 
          type="submit" 
          class="w-full mt-4 p-4 bg-cyan-500 hover:bg-cyan-400 text-white font-bold rounded-xl shadow-lg shadow-cyan-950/20 transform active:scale-95 transition-all"
        >
          {{ esSecretaria ? 'Registrar Cita en el Sistema' : 'Generar mi cita médica' }}
        </button>

      </form>
    </div>
  </div>
</template>

<style scoped>
/* Estilizamos el calendario y select para que se vean mejor en móviles */
input[type="date"]::-webkit-calendar-picker-indicator {
  cursor: pointer;
  filter: invert(0.5);
}
</style>