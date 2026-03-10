<script setup>
import { reactive, ref } from 'vue'

let verContraseña = ref(false)
let mostrarContraseña = ()=>{
    verContraseña.value = true
};
/* formulario reactivo */
const form = reactive({
  nombre: '',
  apellido: '',
  fechaNacimiento: '',
  dui: '',
  telefono: '',
  email: '',
  password: ''
})

/* emitir evento para cerrar modal */
const emit = defineEmits(['cerrar'])

const cerrar = () => {
  emit('cerrar')
}


/* envío del formulario */
const enviarFormulario = () => {
  console.log('Datos enviados:', form)

  // aquí luego usarás fetch o axios
  // fetch('URL_BACKEND', { method:'POST', body: JSON.stringify(form) })

  cerrar()
}

</script>

<template>
  <div class="contenedor">
    <form class="formulario" @submit.prevent="enviarFormulario">

      <button class="cerrar" type="button" @click="cerrar">✕ Regresar</button>

      <h3>¡Bienvenido!</h3>
      <p>Por favor complete los siguientes campos para crear su cuenta</p>

      <div class="componentes">
        <label>Nombres según DUI</label>
        <input type="text" v-model="form.nombre" required>
      </div>

      <div class="componentes">
        <label>Apellidos</label>
        <input type="text" v-model="form.apellido" required>
      </div>

      <div class="componentes">
        <label>Fecha nacimiento</label>
        <input type="date" v-model="form.fechaNacimiento" required>
      </div>

      <div class="componentes">
        <label>N° DUI</label>
        <input type="text" v-model="form.dui" required>
      </div>

      <div class="componentes">
        <label>Teléfono</label>
        <input type="text" v-model="form.telefono" required>
      </div>

      <div class="componentes">
        <label>Email</label>
        <input type="email" v-model="form.email" required>
      </div>

      <div class="componentes">
        <label for="password">Contraseña  <input type="checkbox" @click="mostrarContraseña"><span>(Ver contraseña) </span>   </label>            
        <input  id="password" placeholder="***********"  required :type="verContraseña ? 'text': 'password'">
       
      </div>

      <button type="submit" class="enviar">
        Enviar información
      </button>

    </form>
  </div>
</template>


<style>
.contenedor {
  position:fixed;
  top: 0;
  left: 0;
  width: 100vw;
  height: 100vh;
  background-color:  rgba(15, 23, 42, 0.7);

  display: flex;
  justify-content: center;
  align-items: center;      /* 👈 CLAVE */

  padding: 1rem;
  overflow: hidden;         /* 👈 NO scroll aquí */
  z-index: 9999;
}

.formulario {
  background-color: rgb(15, 144, 151);
  width: 100%;
  max-width: 420px;

  padding: 1.5rem;
  border-radius: 12px;
  box-shadow: 0 25px 50px rgba(0,0,0,.25);

  display: flex;
  flex-direction: column;
  gap: 1rem;

  max-height: 90vh;         /* 👈 límite pantalla */
  overflow-y: auto;         /* 👈 scroll solo aquí */

  box-sizing: border-box;
}

.componentes {
  display: flex;
  flex-direction: column;

}

input {
  padding: 12px;
  border-radius: 10px;
  border: none;
  outline: none;
}

button {
  margin-top: 1rem;
  padding: 14px;
  border-radius: 10px;
  border: none;
  font-weight: bold;
  cursor: pointer;
}
button:hover{
    background-color: teal;
}

</style>