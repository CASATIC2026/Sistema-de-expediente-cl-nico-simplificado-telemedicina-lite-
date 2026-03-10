<script setup>
import { ref, computed } from 'vue';

// PROPS: Para controlar si es vista de Paciente o Secretaria
const props = defineProps({
  esSecretaria: {
    type: Boolean,
    default: false
  }
});

// ESTADO DEL FORMULARIO
const textoBusqueda = ref(''); // Lo que escribe la secretaria (DUI)
const pacienteSeleccionado = ref(null); // Objeto del paciente encontrado

const form = ref({
  pacienteID: '', // ID real que irá a la base de datos
  doctorID: '',
  fecha: '',
  hora: '',
  estado: 'Pendiente'
});

// SIMULACIÓN DE BASE DE DATOS (Sustituir por llamada a API)
const listaPacientes = ref([
  { id: 10, nombre: "Paciente x , apellido y", dui: "100000001" },
  { id: 25, nombre: "Paciente y, Apellido y", dui: "200000002" },
  { id: 42, nombre: "Paciente z, Apellido z", dui: "300000003" }
]);

// LÓGICA PARA VINCULAR EL DUI CON EL ID
const vincularPaciente = () => {
  // Buscamos en la lista el paciente que coincida con el DUI escrito
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
  // Validación extra antes de enviar
  if (props.esSecretaria && !form.value.pacienteID) {
    alert("Por favor, seleccione un paciente válido de la lista.");
    return;
  }
  
  console.log("Enviando a la DB:", form.value);
  alert("¡Cita agendada con éxito!");
};
</script>

<template>
  <div class="contenedorCitas">
    <button type="button" class="botonCerrar">&times;</button>
    
    <p class="titulo">
      <strong>{{ esSecretaria ? 'Panel Administrativo: Agendar Cita' : 'Complete los campos para agendar su cita' }}</strong>
    </p>

    <form @submit.prevent="guardarCita">
      
      <!-- SECCIÓN EXCLUSIVA SECRETARIA: Buscador por DUI -->
      <div v-if="esSecretaria" class="campoCitas">
        <label for="pacienteBusqueda">Buscar paciente por su DUI</label>
        <input 
          list="pacientesSugeridos" 
          id="pacienteBusqueda" 
          v-model="textoBusqueda"
          @change="vincularPaciente"
          placeholder="Ingrese un N° identificación (DUI)..."
          class="input-control"
          required
        >
        <datalist id="pacientesSugeridos">
          <option v-for="p in listaPacientes" :key="p.id" :value="p.dui">
            {{ p.nombre }}
          </option>
        </datalist>
        
        <!-- Feedback para la secretaria -->
        <span v-if="pacienteSeleccionado" class="msj-exito">
          ✅ Seleccionado: {{ pacienteSeleccionado.nombre }}
        </span>
        <span v-else-if="textoBusqueda" class="msj-error">
          ❌ Paciente no encontrado
        </span>

        <input type="hidden" v-model="form.pacienteID">
      </div>

      <!-- SECCIÓN COMÚN: Doctor -->
      <div class="campoCitas">
        <label for="doctorID">Doctor disponible</label>
        <select v-model="form.doctorID" id="doctorID" class="input-control" required>
          <option value="" disabled>--Seleccione un doctor--</option>
          <option value="1">Doctor abcde efghihk</option>
          <option value="2">Dra. aeiou aeiou</option>
        </select>
      </div>

      <!-- SECCIÓN COMÚN: Fecha -->
      <div class="campoCitas">
        <label for="fecha">Selecionar una fecha</label>
        <input type="date" v-model="form.fecha" id="fecha" class="input-control" required>
      </div>

      <!-- SECCIÓN COMÚN: Hora -->
      <div class="campoCitas">
        <label for="hora">Seleccionar una horario </label>
        <select v-model="form.hora" id="hora" class="input-control" required>
          <option value="" disabled>---Horario disponible---</option>
          <option value="08:00">08:30-9:00 AM</option>
          <option value="09:00">09:00-9:30 AM</option>
          <option value="10:00">9:30-10:00 AM</option>
          <option value="10:00">10:00-10:30 AM</option>
        </select>
      </div>

       <div class="campoCitas">
        <label for="motivo">Motivo de la consulta</label>
        <input type="text" v-model="form.motivo" id="motivo" class="input-control" required>
      </div>

      <button type="submit" class="btn-enviar">
        {{ esSecretaria ? 'Registrar Cita' : 'Generar mi cita' }}
      </button>

    </form>
  </div>
</template>

<style scoped>
.contenedorCitas {
  max-width: 450px;
  margin: 20px auto;
  padding: 25px;
  background: linear-gradient(60deg, rgb(2, 64, 71) 20%,  rgb(4, 196, 196));
  border-radius: 12px;
  box-shadow: 0 10px 25px rgba(0,0,0,0.1);
  font-family: sans-serif;
}

.titulo { color: #2c3e50; margin-bottom: 20px; text-align: center; }

.campoCitas { margin-bottom: 18px; display: flex; flex-direction: column; }

.campoCitas label { font-size: 0.9rem; font-weight: 600; margin-bottom: 6px; color: #ffffff; }

.input-control {
  padding: 10px;
  border: 1px solid #dcdfe6;
  border-radius: 6px;
  font-size: 1rem;
}

.btn-enviar {
  width: 100%;
  padding: 12px;
  background-color: #409eff;
  color: white;
  border: none;
  border-radius: 6px;
  font-weight: bold;
  cursor: pointer;
  transition: 0.3s;
}

.btn-enviar:hover { background-color: #66b1ff; }

.msj-exito { color: #67c23a; font-size: 0.8rem; margin-top: 5px; }
.msj-error { color: #f56c6c; font-size: 0.8rem; margin-top: 5px; }

.botonCerrar {
  float: right;
  border: none;
  background: transparent;
  font-size: 20px;
  cursor: pointer;
}
</style>
