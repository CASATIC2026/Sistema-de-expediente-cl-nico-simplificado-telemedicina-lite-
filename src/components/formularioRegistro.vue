<script setup>
import { reactive, ref } from 'vue'

const verContraseña = ref(false)

const form = reactive({
  nombre: '',
  apellido: '',
   Género: '',
  fechanacimiento: '',
 // dui: '',
  direccion: '',
  telefono: '',
  email: '',
  password: ''
})


const emit = defineEmits(['cerrar'])

const cerrar = () => {
  emit('cerrar')
}

// 2. ESTO ENVÍA LOS DATOS (La conexión con el backend)
const enviarFormulario = async () => {
  // Validación básica antes de enviar
  if (form.password.length < 8) {
    alert('La contraseña debe tener al menos 8 caracteres')
    return
  }

  try {
    const respuesta = await fetch('http://localhost:3000/api/registro', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(form) 
    });

    const resultado = await respuesta.json();
    
    if (respuesta.ok) {
      alert(resultado.mensaje);
      cerrar();
    } else {
      alert("Error del servidor: " + resultado.error);
    }
  } catch (error) {
    alert("No se pudo conectar con el servidor. ¿Olvidaste encenderlo?");
    console.error(error);
  }
}
</script>

<template>
  <div class="fixed inset-0 z-[9999] flex items-center justify-center bg-slate-950/80 backdrop-blur-md p-4 overflow-hidden animacion-suave">
    
    <form 
      class="bg-[#1f2e4e]/95 w-full max-w-[460px] p-6 md:p-10 rounded-[2.5rem] shadow-2xl flex flex-col gap-5 max-h-[90vh] overflow-y-auto border border-white/10 text-white custom-scrollbar transition-all"
      @submit.prevent="enviarFormulario"
    >
      
      <button 
        class="self-start text-xs font-bold uppercase tracking-widest bg-white/10 hover:bg-white/20 text-cyan-400 px-5 py-2.5 rounded-2xl transition-all flex items-center gap-2 mb-2 group"
        type="button" 
        @click="emit('cerrar')"
      >
        <span class="group-hover:-translate-x-1 transition-transform">←</span> Regresar
      </button>

      <div class="text-center md:text-left mb-4">
        <h3 class="text-3xl font-extrabold tracking-tight text-white">Crear <span class="text-cyan-400">Cuenta</span></h3>
        <p class="text-sm text-slate-400 mt-2 leading-relaxed">Únete a nuestra plataforma de salud digital y conecta con especialistas en un click.</p>
      </div>

      <div class="flex flex-col gap-5">
        <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div class="flex flex-col gap-2">
            <label class="text-[11px] font-bold uppercase tracking-widest text-slate-400 ml-1">Nombres</label>
            <input type="text" v-model="form.nombre" required placeholder="Ej. Juan" 
              class="bg-white/5 border border-white/10 p-3.5 rounded-2xl outline-none text-white placeholder:text-slate-600 focus:ring-2 focus:ring-cyan-500/50 focus:bg-white/10 transition-all">
          </div>

          <div class="flex flex-col gap-2">
            <label class="text-[11px] font-bold uppercase tracking-widest text-slate-400 ml-1">Apellidos</label>
            <input type="text" v-model="form.apellido" required placeholder="Ej. Pérez"
              class="bg-white/5 border border-white/10 p-3.5 rounded-2xl outline-none text-white placeholder:text-slate-600 focus:ring-2 focus:ring-cyan-500/50 focus:bg-white/10 transition-all">
          </div>
        </div>

        <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div class="flex flex-col gap-2">
            <label class="text-[11px] font-bold uppercase tracking-widest text-slate-400 ml-1">Nacimiento</label>
            <input type="date" v-model="form.fechanacimiento" required 
              class="bg-white/5 border border-white/10 p-3.5 rounded-2xl outline-none text-white focus:ring-2 focus:ring-cyan-500/50 focus:bg-white/10 transition-all">
          </div>

          <!--<div class="flex flex-col gap-2">
            <label class="text-[11px] font-bold uppercase tracking-widest text-slate-400 ml-1">N° DUI</label>
            <input type="text" v-model="form.dui" required placeholder="00000000-0"
              class="bg-white/5 border border-white/10 p-3.5 rounded-2xl outline-none text-white placeholder:text-slate-600 focus:ring-2 focus:ring-cyan-500/50 focus:bg-white/10 transition-all">
          </div>-->
        </div>



        <div class="flex flex-col gap-2">
            <label class="text-[11px] font-bold uppercase tracking-widest text-slate-400 ml-1">Dirección</label>
            <input type="text" v-model="form.direccion" required placeholder="Ej. Calle Principal #123, Ciudad"
              class="bg-white/5 border border-white/10 p-3.5 rounded-2xl outline-none text-white placeholder:text-slate-600 focus:ring-2 focus:ring-cyan-500/50 focus:bg-white/10 transition-all">
          
        </div>




        

        <div class="flex flex-col gap-2">
            <label class="text-[11px] font-bold uppercase tracking-widest text-slate-400 ml-1">Seleccionar un género</label>
            <select 
                v-model="form.Género" 
                required
                class="bg-white/5 border border-white/10 p-3.5 rounded-2xl outline-none text-white focus:ring-2 focus:ring-cyan-500/50 focus:bg-[#1f2e4e] transition-all appearance-none cursor-pointer">
                <option value="" disabled selected class="bg-slate-900">--Género--</option>
                <option value="F" class="bg-slate-900 text-white">Femenino</option>
                <option value="M" class="bg-slate-900 text-white">Masculino</option>
          </select>
        </div>

        

        <div class="flex flex-col gap-2">
          <label class="text-[11px] font-bold uppercase tracking-widest text-slate-400 ml-1">Teléfono</label>
          <input type="text" v-model="form.telefono" required placeholder="+503 0000-0000"
            class="bg-white/5 border border-white/10 p-3.5 rounded-2xl outline-none text-white placeholder:text-slate-600 focus:ring-2 focus:ring-cyan-500/50 focus:bg-white/10 transition-all">
        </div>

        <div class="flex flex-col gap-2">
          <label class="text-[11px] font-bold uppercase tracking-widest text-slate-400 ml-1">Correo Electrónico</label>
          <input type="email" v-model="form.email" required placeholder="usuario@ejemplo.com"
            class="bg-white/5 border border-white/10 p-3.5 rounded-2xl outline-none text-white placeholder:text-slate-600 focus:ring-2 focus:ring-cyan-500/50 focus:bg-white/10 transition-all">
        </div>

        <div class="flex flex-col gap-2">
          <div class="flex justify-between items-center ml-1">
            <label for="password" class="text-[11px] font-bold uppercase tracking-widest text-slate-400">Contraseña</label>
            <button type="button" @click="verContraseña = !verContraseña" class="text-[10px] uppercase font-bold text-cyan-500 hover:text-cyan-300 transition-colors">
              {{ verContraseña ? 'Ocultar' : 'Ver clave' }}
            </button>
          </div>
          <input 
            id="password" 
            v-model="form.password" 
            placeholder="Mínimo 8 caracteres" 
            required 
            :type="verContraseña ? 'text': 'password'"
            class="bg-white/5 border border-white/10 p-3.5 rounded-2xl outline-none text-white placeholder:text-slate-600 focus:ring-2 focus:ring-cyan-500/50 focus:bg-white/10 transition-all"
          >
        </div>
      </div>

      <button 
        type="submit" 
        class="mt-6 p-4 bg-cyan-600 hover:bg-cyan-500 text-white font-bold rounded-2xl shadow-lg shadow-cyan-900/20 hover:scale-[1.02] transition-all active:scale-95 text-sm uppercase tracking-[0.2em]"
      >
        Registrarme ahora
      </button>

    </form>
  </div>
</template>

<style scoped>
/* Animación de entrada manual */
.animacion-suave {
  animation: fadeIn 0.3s ease-out;
}

@keyframes fadeIn {
  from { opacity: 0; transform: scale(0.95); }
  to { opacity: 1; transform: scale(1); }
}

/* Scrollbar ultra-fino */
.custom-scrollbar::-webkit-scrollbar {
  width: 4px;
}
.custom-scrollbar::-webkit-scrollbar-track {
  background: transparent;
}
.custom-scrollbar::-webkit-scrollbar-thumb {
  background: rgba(34, 211, 238, 0.2);
  border-radius: 20px;
}
.custom-scrollbar::-webkit-scrollbar-thumb:hover {
  background: rgba(34, 211, 238, 0.5);
}

input {
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
}
</style>