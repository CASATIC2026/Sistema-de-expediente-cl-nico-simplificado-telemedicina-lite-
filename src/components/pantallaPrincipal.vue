<script setup>
import { ref, stop, onMounted, onUnmounted} from 'vue';
import login from './login.vue';
import registroUsuario from './registroUsuario.vue';


const accesoLogin = ref(false);
const accesoFormulario = ref(false);
const mostrarLogin = () => {
   accesoLogin.value = !accesoLogin.value;
}

const mostrarFormulario = () => {
   accesoFormulario.value = !accesoFormulario.value;
}


const imagenes =[
{src: new URL('@/assets/imagen1.JPG', import.meta.url).href, texto: 'texto numero 1' },
{src: new URL('@/assets/imagen2.JPG', import.meta.url).href, texto: 'texto numero 1' },
{src: new URL('@/assets/imagen4.JPG', import.meta.url).href, texto: 'texto numero 1' },
{src: new URL('@/assets/imagen6.JPG', import.meta.url).href, texto: 'texto numero 1' },
{src: new URL('@/assets/imagen7.JPG', import.meta.url).href, texto: 'texto numero 1' },

];
const indiceActivo = ref(0);
let intervalo = null;

onMounted(() => {
  intervalo = setInterval(() => {
    indiceActivo.value =
      (indiceActivo.value + 1) % imagenes.length;
  }, 5000); //TIEMPO EN MILISEGUNDOS EN QUE CAMBIARÁ LA IMAGEN
});

onUnmounted(() => {
  clearInterval(intervalo);
});

</script>


<template> 
    <nav class="header">
        <div class="logo">Mi LOGO <br> xdx <br>dxdxd</div>

        <div class="nav-links" id="dropdown">
            <a href="#" class="enlace">Servicios ▼</a>
            <ul class="dropdown-menu">
                <li>Teleconsultas</li>
                <li>Atención Psicológica</li>
                <li>Teleconsulta por enfermedades crónicas</li>
                <li>Recetas virtuales</li>
                <li>Otros servicios</li>
            </ul>
        </div>
        <div class="nav-links" id="dropdown">
            <a href="#" class="enlace">Acerca de nosotros</a>
            <ul class="dropdown-menu">
                <li>Misión</li>
                <p>Brindar soluciones de salud de manera inmmediata a través del uso de la tecnología conectando a cada paciente con un solo click</p>
                <li>Misión</li>
                <p>Transformar el sistema de salud en un ecosistema conectado donde la tecnología y la empatía médica trabajen juntas para el bienestar integral de la comunidad.</p>

            </ul>

        </div>
        

        <div class="auth-buttons">
             <button @click="mostrarLogin" class="btn-login">Iniciar sesión</button>

             <button @click="mostrarFormulario" class="btn-signup">Crear cuenta</button>
        </div>
    </nav>


 


    <div v-if="accesoLogin" class="modal-overlay" @click.self="mostrarLogin">  <login v-if="accesoLogin" @cerrar="accesoLogin =false" /></div>
    <div v-if="accesoFormulario" class="contenedor" @click.self="mostrarFormulario" >  <registroUsuario v-if="accesoFormulario" @cerrar="accesoFormulario =false" /></div>
    
    <div class="visor-corte">
  <div
    class="contenedorImagenes"
    :style="{ transform: `translateX(-${indiceActivo * 100}%)` }"
  >
    <div
      class="imagen"
      v-for="(img, index) in imagenes"
      :key="index"
    >
      <img :src="img.src" alt="imagen ilustrativa medicina" />
      <h4>{{ img.texto }}</h4>
    </div>
  </div>
</div>
  <a
    href="https://wa.me/503XXXXXXXX"
    target="_blank"
    class="whatsapp-float"
  >
    <img src="/whatsapp.png" alt="WhatsApp" />
  </a>

    
    
</template>

<style>
.visor-corte {
  width: 100%;
  max-width: 900px;
  margin: 0 auto;
  overflow: hidden;
  border-radius: 16px;
  box-shadow: 0 10px 25px rgba(0, 0, 0, 0.15);
}

.contenedorImagenes {
  display: flex;
  transition: transform 1s ease-in-out;
}

.imagen {
  min-width: 100%;
  height: 300px;
  position: relative;
}

.imagen img {
  width: 100%;
  height: 100%;
  object-fit: fill;
}

.imagen h4 {
  position: absolute;
  bottom: 15px;
  left: 50%;
  transform: translateX(-50%);
  background: rgba(0, 0, 0, 0.6);
  color: #ffffff;
  padding: 8px 16px;
  border-radius: 8px;
  font-size: 1.2rem;
}

/* Desktop */
@media (min-width: 768px) {
  .imagen {
    height: 450px;
  }

  .imagen h4 {
    font-size: 1.8rem;
  }
}
body {
  margin: 0;
  padding: 0;
  font-family: sans-serif;
  background: linear-gradient(90deg, #182f36 0%, #30b6eb 100%);
  overflow-y: auto;
  min-height: 100vh;
  position: relative;
}




/* HEADER */
.header {
  display: flex;
  flex-wrap: wrap;                 /* 👈 CLAVE */
  background: linear-gradient(90deg, #1c3139 0%, #035b8e 100%);
  justify-content: space-between;
  align-items: center;
  padding: 12px 20px;              /* 👈 adaptable */
  gap: 1rem;
  box-shadow: 0 2px 5px rgba(2, 1, 1, 0.1);

  position: sticky;
  top: 0;
  z-index: 1000;
}

#dropdown{
    position: relative;
    display: inline;
}
.dropdown-menu{
    display: none;
    position: absolute;
    background:  linear-gradient(135deg, rgb(5, 126, 177), rgb(2, 55, 44));
    max-width: 5000px;
    color:white ;
    box-shadow: 8px 8px 16px rgba(2, 26, 50, 0.987);
    padding: 50px;
    z-index: 1;
    border-radius: 8px;
    top: 100%;
    left: 50%;
    transform: translate(-50%);
    border-top: 3px solid#03101f;
    border-right: 3px solid#03101f;
    border-bottom:  3px solid#03101f;
    font-size: 22px;

    }
p{
    font-size: 10px;
}
#dropdown:hover .dropdown-menu{
    display:block;

}



/* LOGO */
.logo {
  font-weight: bold;
  color: #0056b3;
  font-size: 1.5rem;
}

/* LINKS */
.nav-links {
  display: flex;
  gap: 1rem;
  background-color: azure;
  padding: 8px 12px;
  border-radius: 10px;
  flex-wrap: wrap;                 /* 👈 */
}

/* ENLACES */
.enlace {
  text-decoration: none;
  color: #333;
  font-weight: 500;
}

.enlace:hover {
  color: #0056b3;
}

/* BOTONES */
.auth-buttons {
  display: flex;
  gap: 0.5rem;
}

/* Botones */
.btn-login,
.btn-signup {
  background-color: #0056b3;
  border: none;
  color: white;
  padding: 10px 14px;
  border-radius: 5px;
  cursor: pointer;
  font-size: 14px;
}

.btn-login:hover,
.btn-signup:hover {
  background-color: #004494;
}

/* 📱 MOBILE */
@media (max-width: 768px) {
  .header {
    flex-direction: column;        /* 👈 APILA */
    align-items: stretch;
    text-align: center;
  }

  .nav-links,
  .auth-buttons {
    justify-content: center;
  }

  .btn-login,
  .btn-signup {
    width: 100%;                   /* 👈 BOTONES FULL */
  }
}


.whatsapp-float {
  position: fixed;
  bottom: 20px;
  left: 20px;
  z-index: 9999;
}

.whatsapp-float img {
  width: 75px;
  height: 75px;
  cursor: pointer;
  border-radius: 30px;
}
.imagen1 {
  height: 300px;
  background-size: cover;
  background-position: center;
}




</style>