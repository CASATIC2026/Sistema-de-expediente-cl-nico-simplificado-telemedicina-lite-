<script setup>
import { ref } from 'vue';
import loginGeneral from '../components/loginGeneral.vue';
import formularioRegistro from '../components/formularioRegistro.vue';


const llamarRegistro = ref(false);
const abrirRegistro = () => {
  llamarRegistro.value = !llamarRegistro.value
};


const cerrarRegistro = () => {
  llamarRegistro.value = false;
};



</script>

<template>
  <div class="min-h-screen bg-[#0a0f16] flex items-center justify-center p-4 md:p-8 font-sans">
    
    <div class="relative w-full max-w-7xl min-h-[600px] bg-[#111827] rounded-[40px] shadow-[0_0_50px_rgba(34,211,238,0.15)] border border-white/10 flex flex-col md:flex-row overflow-hidden">
      
      <div class="w-full md:w-1/2 p-8 md:p-12 flex flex-col justify-between relative z-10">
        
        <nav class="flex flex-wrap gap-4 text-[10px] md:text-xs font-bold text-white/60 uppercase tracking-widest">
          <a href="#" class="hover:text-cyan-400 transition-colors">Empresa</a>
          <a href="#" class="hover:text-cyan-400 transition-colors">Contacto</a>
          <a href="#" class="hover:text-cyan-400 transition-colors">Ayuda</a>
          <a href="#" class="hover:text-cyan-400 transition-colors">Servicios</a>
          <a href="#" class="hover:text-cyan-400 transition-colors">Blog</a>
        </nav>

        <div class="py-12 text-center md:text-left">
          <div class="mb-6 flex justify-center md:justify-start">
             <svg class="w-16 h-16 text-white opacity-80" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" d="M19.5 10.5c0-4.142-3.358-7.5-7.5-7.5S4.5 6.358 4.5 10.5m0 0a7.5 7.5 0 1015 0m-15 0v.75m15-.75v.75m-11.25 0h7.5"></path></svg>
          </div>
          <h1 class="text-3xl md:text-5xl font-extrabold text-white leading-tight mb-4">
            NUESTRA PRIORIDAD ES <br> <span class="text-cyan-400">TU SALUD.</span>
          </h1>
          <p class="text-lg md:text-xl text-white/70 font-light">
            Telemedicina Accesible y Humanizada
          </p>
          
          <div class="mt-8 flex items-center gap-2 text-xs text-white/40 uppercase tracking-tighter">
             <span>Últimas Noticias</span>
             <div class="flex gap-1">
               <span class="w-2 h-2 rounded-full bg-cyan-400"></span>
               <span class="w-2 h-2 rounded-full bg-white/20"></span>
               <span class="w-2 h-2 rounded-full bg-white/20"></span>
             </div>
          </div>
        </div>

        <div class="text-[10px] text-white/30 flex gap-4">
          <a href="#">Aviso de Privacidad</a>
          <a href="#">Términos y Condiciones</a>
        </div>
      </div>

      <div class="w-full md:w-1/2 bg-white/5 backdrop-blur-sm flex flex-col items-center justify-center p-8 border-l border-white/5 relative">
        
        <div class="absolute top-8 right-12 text-right">
          <div class="text-xl font-black text-white italic">MI <span class="text-cyan-400">LOGO</span></div>
          <div class="text-[8px] text-white/40 tracking-[0.3em] uppercase">Salud Digital</div>
        </div>

        <loginGeneral @abrirRegistro="abrirRegistro" />
       


        <div class="absolute bottom-8 right-12 text-[10px] text-white/30">
          © 2025-2026 Mi Salud Digital - Todos los derechos reservados.
        </div>
      </div>
     

    </div>


    <div class="fixed bottom-6 left-6 bg-green-500 p-3 rounded-full shadow-lg cursor-pointer hover:scale-110 transition-transform">
      <img src="/whatsapp.png" class="w-8 h-8" alt="WA">
    </div>

    <formularioRegistro v-if="llamarRegistro" @cerrar="cerrarRegistro" />

   
   



    
  

  </div>
</template>