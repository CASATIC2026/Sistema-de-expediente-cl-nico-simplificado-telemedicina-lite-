<script setup>
import { ref } from 'vue'; 
const emit = defineEmits(['abrirRegistro']);


// 1. Definimos el evento para avisar a la Vista Principal que debe abrir el modal de registro




const contraseñaVisible = ref(false);
const toggleContraseña = () => {
  contraseñaVisible.value = !contraseñaVisible.value;
};

// Lógica de inicio de sesión (puedes expandirla luego)
const loginData = ref({
  email: '',
  password: ''
});

const handleLogin = () => {
  console.log("Intentando iniciar sesión con:", loginData.value);
  // Aquí iría tu fetch a la API de login
};
</script>

<template>
  <div class="w-full max-w-md p-4 md:p-8 text-white">
    
    <div class="mb-8">
      <h3 class="text-2xl font-bold uppercase tracking-wide">Ingresa a tu Salud Digital</h3>
      <p class="text-xs text-white/50 mt-1 uppercase tracking-widest">Acceso para pacientes</p>
    </div>

    <form @submit.prevent="handleLogin" class="space-y-6">
      
      <div>
        <label class="block text-sm font-medium mb-2 opacity-80 uppercase tracking-tighter">Correo Electrónico</label>
        <input 
          v-model="loginData.email"
          type="email" 
          required
          class="w-full bg-white/5 border border-white/10 rounded-xl px-4 py-3.5 text-white focus:outline-none focus:ring-2 focus:ring-cyan-500/50 focus:bg-white/10 transition-all placeholder:text-white/20"
          placeholder="ejemplo@salud.com"
        >
      </div>

      

      <div>
        <div class="flex justify-between items-center mb-2">
          <label class="text-sm font-medium opacity-80 uppercase tracking-tighter">Contraseña</label>
          
          <button 
            type="button" 
            @click="toggleContraseña"
            class="text-[10px] uppercase tracking-widest font-bold text-cyan-400 hover:text-cyan-300 transition-colors flex items-center gap-1.5"
          >
            <span>{{ contraseñaVisible ? '🙈 Ocultar' : '👁️ Ver clave' }}</span>
          </button>
        </div>

        <input 
          v-model="loginData.password"
          :type="contraseñaVisible ? 'text' : 'password'"
          required
          class="w-full bg-white/5 border border-white/10 rounded-xl px-4 py-3.5 text-white focus:outline-none focus:ring-2 focus:ring-cyan-500/50 focus:bg-white/10 transition-all placeholder:text-white/20"
          placeholder="********"
        >
        <div class="text-right mt-2">
          <a href="#" class="text-[11px] text-white/40 hover:text-cyan-400 transition-colors">¿Olvidaste tu contraseña?</a>
        </div>
      </div>

      <button 
        type="submit" 
        class="w-full py-4 bg-cyan-600 hover:bg-cyan-500 text-white font-bold rounded-xl transition-all active:scale-[0.98] shadow-lg shadow-cyan-950/20 uppercase tracking-[0.2em] text-sm"
      >
        INGRESAR
      </button>

      <div class="flex items-center gap-4 my-6 opacity-20">
        <div class="h-[1px] bg-white flex-grow"></div>
        <span class="text-[10px] uppercase font-bold">O</span>
        <div class="h-[1px] bg-white flex-grow"></div>
      </div>

      <p class="text-center text-sm text-white/60">
        ¿No tienes cuenta? 
        <button 
          type="button" 
          @click="$emit('abrirRegistro')"
          class="text-cyan-400 font-bold hover:text-cyan-300 hover:underline uppercase transition-all ml-1.5"
        >
          Registrarme
        </button>
      </p>

    </form>

   
   
    
    
  </div>


</template>

<style scoped>
/* Mejora la apariencia del autocompletado del navegador */
input:-webkit-autofill,
input:-webkit-autofill:hover, 
input:-webkit-autofill:focus {
  -webkit-text-fill-color: white;
  -webkit-box-shadow: 0 0 0px 1000px #1f2e4e inset;
  transition: background-color 5000s ease-in-out 0s;
}
</style>