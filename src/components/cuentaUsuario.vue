<script>
export default {
  data() {
    return {
      editarPerfil: false,
      verPassword: false,
      etiquetas: {
        nombre: "Nombres",
        apellido: "Apellidos",
        fechaNacimiento: "Fecha de nacimiento",
        duiUsuario: "ID / DUI",
        telefonoUsuario: "Teléfono",
        email: "Email",
        contraseña: "Contraseña",
        jvpm: "jvpm"
      },
      registroUsuario: {
        nombre: "Juan",
        apellido: "Pérez",
        fechaNacimiento: "1990-01-01",
        duiUsuario: "00000000-0",
        telefonoUsuario: "7000-0000",
        email: "juan@mail.com",
        contraseña: "password123",
        jvpm: "jvpm"
        
      }
    }
  },
  methods: {
    editando() { this.editarPerfil = !this.editarPerfil; },
    guardarEdicionCambios() {
      this.editarPerfil = false;
      alert("Cambios guardados con éxito");
    }
  },
  computed: {
    contrasenaOculta() {
      // Crea una cadena de asteriscos del mismo largo que la contraseña
      return "*".repeat(this.registroUsuario.contraseña.length);
    }
  }
}
</script>

<template>
  <div class="contenedor">
    <div class="tarjeta">

      <header class="encabezado">
        <button class="btn-regresar">← Regresar</button>

        <button 
          v-if="!editarPerfil" 
          @click="editando" 
          class="btn-editar">
          Editar perfil
        </button>
      </header>

      <div class="campo" 
           v-for="(valor, llave) in etiquetas" 
           :key="llave">
        
        <label>{{ valor }}:</label>

        <div class="dato">
          
          <strong v-if="!editarPerfil">
            {{ llave === 'contraseña' ? contrasenaOculta : registroUsuario[llave] }}
          </strong>

          <div v-else class="grupo-input">
            <input 
              v-model="registroUsuario[llave]" 
              :type="(llave === 'contraseña' && !verPassword) ? 'password' : 'text'" 
              class="input-moderno">

            <button 
              v-if="llave === 'contraseña'" 
              type="button" 
              @click="verPassword = !verPassword" 
              class="btn-ojo">
              {{ verPassword ? 'cerrar' : 'ver' }}
            </button>
          </div>

        </div>
      </div>

      <div v-if="editarPerfil" class="seccion-botones">
        <button @click="guardarEdicionCambios" class="btn-guardar">
          Guardar cambios
        </button>
        <button @click="editarPerfil = false" class="btn-cancelar">
          Cancelar
        </button>
      </div>

    </div>
  </div>
</template>


<style>

/* ===== OVERLAY (FONDO OSCURO) ===== */
.contenedor { 
  position: fixed;
  inset: 0;
  display: flex;
  justify-content: center;
  align-items: center;
  background: rgba(15, 23, 42, 0.6);
  backdrop-filter: blur(6px);
  z-index: 9999;
  padding: 20px;
}

/* ===== TARJETA MODAL ===== */
.tarjeta {
  background: #ffffff;
  color: #1e293b;
  width: 100%;
  max-width: 520px;
  max-height: 90vh;
  overflow-y: auto;
  border-radius: 18px;
  padding: 30px;
  box-shadow: 0 20px 40px rgba(0, 0, 0, 0.15);
  animation: aparecer 0.25s ease;
}

/* Animación suave */
@keyframes aparecer {
  from {
    opacity: 0;
    transform: translateY(10px) scale(0.97);
  }
  to {
    opacity: 1;
    transform: translateY(0) scale(1);
  }
}

/* ===== HEADER DEL MODAL ===== */
.encabezado {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 25px;
  padding-bottom: 15px;
  border-bottom: 1px solid #e2e8f0;
}

.btn-regresar {
  background: none;
  border: none;
  color: #2563eb;
  font-weight: 600;
  cursor: pointer;
  font-size: 14px;
}

.btn-regresar:hover {
  text-decoration: underline;
}

.btn-editar {
  background: #0d9488;
  color: white;
  border: none;
  padding: 8px 16px;
  border-radius: 8px;
  cursor: pointer;
  font-weight: 500;
  transition: all 0.2s ease;
}

.btn-editar:hover {
  background: #0f766e;
  transform: translateY(-1px);
}

/* ===== CAMPOS ===== */
.campo {
  display: flex;
  flex-direction: column;
  padding: 14px 0;
  border-bottom: 1px solid #f1f5f9;
}

@media (min-width: 480px) {
  .campo {
    flex-direction: row;
    justify-content: space-between;
    align-items: center;
  }
}

label {
  font-weight: 600;
  color: #64748b;
  margin-bottom: 6px;
}

.dato {
  width: 100%;
  text-align: left;
}

strong {
  color: #0f172a;
}

/* ===== INPUTS ===== */
.grupo-input {
  display: flex;
  align-items: center;
  gap: 8px;
  width: 100%;
}

.input-moderno {
  flex: 1;
  padding: 10px 12px;
  border: 1px solid #cbd5e1;
  border-radius: 8px;
  outline: none;
  transition: border 0.2s ease;
}

.input-moderno:focus {
  border-color: #2563eb;
}

/* Botón mostrar contraseña */
.btn-ojo {
  background: none;
  border: none;
  color: #2563eb;
  cursor: pointer;
  font-size: 13px;
}

/* ===== BOTONES INFERIORES ===== */
.seccion-botones {
  margin-top: 25px;
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.btn-guardar {
  background: #2563eb;
  color: white;
  border: none;
  padding: 12px;
  border-radius: 10px;
  cursor: pointer;
  font-weight: 600;
  transition: all 0.2s ease;
}

.btn-guardar:hover {
  background: #1d4ed8;
}

.btn-cancelar {
  background: none;
  border: none;
  color: #dc2626;
  cursor: pointer;
  font-weight: 500;
}

.btn-cancelar:hover {
  text-decoration: underline;
}

/* ===== RESPONSIVE EXTRA ===== */
@media (max-width: 480px) {
  .tarjeta {
    padding: 20px;
  }

  .encabezado {
    flex-direction: row;
  }
}

</style>
