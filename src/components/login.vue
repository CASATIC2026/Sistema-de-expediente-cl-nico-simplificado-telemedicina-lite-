<script setup>

import { ref } from 'vue';  
const emit =    defineEmits(['cerrar']);
const contraseñaVisible = ref(false);
const verMicontraseña = () => {
  contraseñaVisible.value = !contraseñaVisible.value;
};

</script>


<template>
  <div class="modal-overlay"> 
    <div class="login-card">
        <button @click="emit('cerrar')">X cerrar</button>

      <form @submit.prevent="handleSubmit" id="form-profesional">
        <div class="form-header">
          <h3>¡Bienvenido!</h3>
          <p>Ingresa tus credenciales para continuar</p>
        </div>
        
        <div class="input-group">
          <label for="email">Correo Electrónico</label>
          <input id="email" type="email" placeholder="ejemplo@correo.com" required>
        </div>

        <div class="input-group">
          <label for="password">Contraseña</label>
          <input type="checkbox" @click="verMicontraseña"><span> Mostrar Contraseña</span>
          <input id="password" placeholder="********" required :type="contraseñaVisible ? 'text' : 'password'">
          
        </div>

       

        <button type="submit" class="btn-primary">Iniciar Sesión</button>
        
        <p class="footer-text">
          ¿Tienes dudas? Contactanos para más informacion....
        </p>
      </form>
    </div>
  </div>
</template>


<style>
/* Contenedor que centra todo */
.modal-overlay {
  position: fixed;
  top: 0; left: 0;
  
  width: 100vw;
  height: 100vh;
  display: flex;
  justify-content: center;
  padding: 1rem;
  align-items: center;
  background: rgba(15, 23, 42, 0.7); /* Oscuro azulado para elegancia */
  backdrop-filter: blur(8px); /* Efecto de desenfoque de fondo */
  z-index: 9999;
}

/* La tarjeta del login */
.login-card {
  width: 100%;
  max-width: 420px;
  background: white;
  padding: 2rem;
  border-radius: 20px;
  box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.25);
  box-sizing: border-box;
  margin: auto;
}

.form-header {
  text-align: center;
  margin-bottom: 30px;
}

.form-header h3 {
  font-size: 24px;
  color: #1e293b;
  margin-bottom: 8px;
  font-weight: 700;
}

.form-header p {
  color: #64748b;
  font-size: 14px;
}

/* Inputs mejorados */
.input-group {
  margin-bottom: 20px;
}

label {
  display: block;
  font-size: 13px;
  font-weight: 600;
  color: #475569;
  margin-bottom: 6px;
}

input[type="email"],
input[id="password"] {
  width: 100%;
  padding: 12px 16px;
  border: 1.5px solid #e2e8f0;
  border-radius: 10px;
  font-size: 14px;
  transition: all 0.3s ease;
  box-sizing: border-box;
}
input[type="checkbox"] {
  margin-top: 10px;
  transform: scale(1.2);
  cursor: pointer;
}


input:focus {
  outline: none;
  border-color: #3b82f6;
  box-shadow: 0 0 0 4px rgba(59, 130, 246, 0.1);
}

/* Opciones y Botón */


.forgot-link {
  color: #3b82f6;
  text-decoration: none;
  font-weight: 500;
}

.btn-primary {
  width: 100%;
  padding: 14px;
  background: linear-gradient(135deg, #2C92B8, #36BBA7);
  color: white;
  border: none;
  border-radius: 10px;
  font-size: 16px;
  font-weight: 600;
  cursor: pointer;
  transition: transform 0.2s ease;
}

.btn-primary:hover {
  transform: translateY(-2px);
  filter: brightness(1.1);
}

.footer-text {
  text-align: center;
  font-size: 13px;
  color: #64748b;
  margin-top: 25px;
}
</style>