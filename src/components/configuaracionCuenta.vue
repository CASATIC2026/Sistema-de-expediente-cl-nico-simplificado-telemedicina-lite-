<script setup>
const emit = defineEmits(['cerrar']);
import { ref, computed } from 'vue'

const editarPerfil = ref(false)
const verPassword = ref(false)

const etiquetas = {
  nombre: "Nombres",
  apellido: "Apellidos",
  fechaNacimiento: "Fecha de nacimiento",
  duiUsuario: "ID / DUI",
  telefonoUsuario: "Teléfono",
  email: "Email",
  contraseña: "Contraseña",
  jvpm: "jvpm"
}

const registroUsuario = ref({
  nombre: "Juan",
  apellido: "Pérez",
  fechaNacimiento: "1990-01-01",
  duiUsuario: "00000000-0",
  telefonoUsuario: "7000-0000",
  email: "juan@mail.com",
  contraseña: "password123",
  jvpm: "jvpm"
})

const editando = () => {
  editarPerfil.value = !editarPerfil.value
}

const guardarEdicionCambios = () => {
  editarPerfil.value = false
  alert("Cambios guardados con éxito")
}

const contrasenaOculta = computed(() => {
  return "*".repeat(registroUsuario.value.contraseña.length)
})
</script>

<template>
  <div class="fixed inset-0 flex items-center justify-center bg-slate-900/60 backdrop-blur-md z-[9999] p-5">
    
    <div class="bg-white text-slate-800 w-full max-w-[520px] max-h-[90vh] overflow-y-auto rounded-2xl p-6 md:p-8 shadow-2xl animate-in fade-in zoom-in duration-300">

      <header class="flex justify-between items-center mb-6 pb-4 border-b border-slate-200">
        <button class="bg-none border-none text-blue-600 font-semibold cursor-pointer text-sm hover:underline" @click="emit('cerrar')">
          ← Regresar
        </button>

        <button 
          v-if="!editarPerfil" 
          @click="editando" 
          class="bg-teal-600 hover:bg-teal-700 text-white border-none py-2 px-4 rounded-lg cursor-pointer font-medium transition-all hover:-translate-y-0.5 shadow-sm">
          Editar perfil
        </button>
      </header>

      <div class="space-y-0">
        <div 
          v-for="(valor, llave) in etiquetas" 
          :key="llave"
          class="flex flex-col sm:flex-row sm:justify-between sm:items-center py-4 border-b border-slate-100 last:border-none"
        >
          
          <label class="font-bold text-slate-500 mb-1 sm:mb-0">{{ valor }}:</label>

          <div class="w-full sm:w-auto sm:text-right">
            
            <strong v-if="!editarPerfil" class="text-slate-900 break-all">
              {{ llave === 'contraseña' ? contrasenaOculta : registroUsuario[llave] }}
            </strong>

            <div v-else class="flex items-center gap-2 w-full sm:max-w-[250px]">
              <input 
                v-model="registroUsuario[llave]" 
                :type="(llave === 'contraseña' && !verPassword) ? 'password' : 'text'" 
                class="flex-1 px-3 py-2 border border-slate-300 rounded-lg outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all text-sm">

              <button 
                v-if="llave === 'contraseña'" 
                type="button" 
                @click="verPassword = !verPassword" 
                class="bg-none border-none text-blue-600 cursor-pointer text-xs font-semibold hover:text-blue-800">
                {{ verPassword ? 'Ocultar' : 'Ver' }}
              </button>
            </div>

          </div>
        </div>
      </div>

      <div v-if="editarPerfil" class="mt-8 flex flex-col gap-3">
        <button @click="guardarEdicionCambios" class="bg-blue-600 hover:bg-blue-700 text-white border-none py-3 rounded-xl cursor-pointer font-bold transition-all shadow-lg active:scale-95">
          Guardar cambios
        </button>
        <button @click="editarPerfil = false" class="bg-none border-none text-red-600 cursor-pointer font-semibold hover:underline text-center">
          Cancelar
        </button>
      </div>

    </div>
  </div>
</template>

<style scoped>
/* Personalización del scroll para que se vea limpio */
div::-webkit-scrollbar {
  width: 6px;
}
div::-webkit-scrollbar-track {
  background: transparent;
}
div::-webkit-scrollbar-thumb {
  background: #e2e8f0;
  border-radius: 10px;
}
div::-webkit-scrollbar-thumb:hover {
  background: #cbd5e1;
}
</style>