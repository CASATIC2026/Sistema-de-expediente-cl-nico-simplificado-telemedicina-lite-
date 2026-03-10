<script>

export default {
  data() {
    return {
      // Información del usuario logueado
      usuario: {
        nombre: "Juan Pérez",
        id: "USR-98765"
      },
      // Estado para controlar qué se muestra
      consultaSeleccionada: null,
      // Datos de ejemplo (Simulando una API)
      historial: [
        { 
          id: 101, 
          fecha: "22-22-22", 
          doctor: "Dr. yxljaslkjak", 
          especialidad: "DermatólogoJ",
          diagnostico: "Cansado de no hacer nada",
          urlReceta: "#" 
        },
        { 
          id: 98, 
          fecha: "24-24-24", 
          doctor: "Dra. xxxxxxxxxxxxx", 
          especialidad: "xxxxxxxxxxx",
          diagnostico: "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
          urlReceta: "#" 
        },
        { 
          id: 98, 
          fecha: "25-25-26", 
          doctor: "Dra. xxxxxxxxxxxxx", 
          especialidad: "xxxxxxxxxxx",
          diagnostico: "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
          urlReceta: "#" 
        }
      ]
    };
  },
  methods: {
    verDetalle(consulta) {
      this.consultaSeleccionada = consulta;
    }
  }
};


</script>







<template>
   <div class="telemedicina-container">
    <h2 v-if="!consultaSeleccionada">Historial Médico</h2>
    <button v-else @click="consultaSeleccionada = null" class="botonRegresar">
      ← Regresar
    </button>

    <hr>

    <div v-if="!consultaSeleccionada" class="lista-historial">
      <div v-for="consulta in historial" :key="consulta.id" class="consulta-item">
        <div class="consulta-info">
          <p class="txt-fecha">Fecha: {{ consulta.fecha }}</p>
          <p class="txt-doctor"><strong>Dr/a:</strong> {{ consulta.doctor }}</p>
          <p class="txt-especialidad">Especialidad: {{ consulta.especialidad }}</p>
        </div>
        <div class="consulta-accion">
          <button @click="verDetalle(consulta)" class="botonVer">Ver Detalles</button>
        </div>
      </div>
    </div>

    <div v-else class="detalle-card">
      <div class="detalle-header">
        <h3>Detalle consoulta  #{{ consultaSeleccionada.id }}</h3>
        <span class="fecha-detalle">Fecha: {{ consultaSeleccionada.fecha }}</span>
      </div>

      <div class="detalle-body">
        <section class="info-seccion">
          <h4>Información del Paciente</h4>
          <p><strong>Nombre:</strong> {{ usuario.nombre }}</p>
          <p><strong>DUI usuario: </strong> {{ usuario.id }}</p>
        </section>

        <section class="info-seccion">
          <h4>Atendido por:</h4>
          <p><strong>{{ consultaSeleccionada.doctor }}</strong></p>
          <p class="subtext">{{ consultaSeleccionada.especialidad }}</p>
        </section>

        <section class="info-seccion">
          <h4>Diagnóstico / Resumen:</h4>
          <div class="cuadro-texto">
            {{ consultaSeleccionada.diagnostico }}
          </div>
        </section>

        <section class="descargas">
          <h4>Documentos disponibles:</h4>
          <div class="botones-descarga">
            <a :href="consultaSeleccionada.urlReceta" class="descargar" download>
              📄 Descargar receta (PDF)
            </a>
          </div>
        </section>
      </div>
    </div>
  </div>


</template>

<style scoped>
/* CONTENEDOR PRINCIPAL CON MAX-WIDTH */
.telemedicina-container {
  font-family: 'Segoe UI', Roboto, sans-serif;
  max-width: 600px; /* Limitamos el ancho para que se vea bien en web y móvil */
  margin: 20px auto;
  padding: 20px;
  background: linear-gradient(60deg, rgb(2, 64, 71) 20%,  rgb(4, 196, 196));
  border-radius: 12px;
  box-shadow: 0 4px 15px rgba(145, 89, 89, 0.1);
  color: rgb(238, 237, 244) !important; /* Letras oscuras legibles */
}
.botonRegresar{
  color: white;
  background-color: teal;
  margin-bottom: 2%;
  border-radius: 5px;
  padding: 7px;
}
.botonRegresar:hover{
  background-color: rgb(102, 205, 170);
}

h2, h3, h4 {
  color:white !important;
  margin-top: 15px;
  font-weight: 600;
}

/* ESTILOS DE LA LISTA (Sustituye a la tabla) */
.consulta-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 15px;
  border-bottom: 1px solid #ffffff;
  transition: 0.3s;
  border-radius: 10px;
}

.consulta-item:hover {
  background: rgb(43, 91, 131);
}

.consulta-info p {
  margin: 4px 0;
  color: #ffffff !important;
  font-weight: 600;
}

.txt-fecha {
  font-size: 0.85rem;
  color: #285eae !important;
}



/* BOTONES */
.botonVer {
  background-color: rgb(208, 255, 255);
  color: white;
  border: none;
  padding: 10px 15px;
  border-radius: 6px;
  cursor: pointer;
  font-weight: 600;
}
.botonVer {
  background-color: teal;
}
.botonVer:hover {
  background-color: rgb(58, 185, 185);
}

.descargar {
  display: block;
  text-align: center;
  background:rgb(86, 150, 150);
  color: white;
  text-decoration: none;
  padding: 12px;
  border-radius: 8px;
  margin-top: 10px;
}
.descargar:hover{
  background-color: rgb(2, 82, 82);
}

/* RESPONSIVIDAD AUTOMÁTICA */
@media (max-width: 480px) {
  .consulta-item {
    flex-direction: column;
    align-items: flex-start;
  }
  .consulta-accion {
    width: 100%;
    margin-top: 10px;
  }
  .btn-ver {
    width: 100%;
  }
}
</style>