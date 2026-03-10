<<template>
  <div class="telemedicina-container">
    <!-- BOTÓN INICIAL -->
    <div v-if="!modoCrearNuevo" class="header-flex">
      <button @click="abrirConsulta" class="btn-crear">+ Nueva Consulta</button>
    </div>

    <!-- BOTÓN REGRESAR -->
    <button v-if="modoCrearNuevo" @click="regresar" class="btn-volver">
      ← Regresar
    </button>

    <hr v-if="modoCrearNuevo" />

    <!-- FORMULARIO (LO QUE EL DOCTOR VE) -->
    <div v-if="modoCrearNuevo" class="espacio-doctor">
      <h3>{{ editandoReceta ? 'Redactar Receta Médica' : 'Nueva Entrada de Consulta' }}</h3>
      
      <!-- SECCIÓN DIAGNÓSTICO -->
      <div v-if="!editandoReceta">
        <div class="campo">
          <label>Observaciones - Sintomas</label>
          <textarea v-model="nuevaConsulta.observaciones" class="area-observaciones"></textarea>
        </div>
        <div class="campo">
          <label>Resumen / diagnóstico</label>
          <textarea v-model="nuevaConsulta.diagnostico" class="area-diagnostico"></textarea>
        </div>
      </div>

      <!-- SECCIÓN RECETA -->
      <div v-else class="campo">
        <label>Prescripción de medicamentos</label>
        <textarea v-model="nuevaConsulta.receta" class="area-receta" placeholder="Espacio para escribir prescripción medica"></textarea>
      </div>

      <!-- BOTONES DE ACCIÓN -->
      <div class="botonesFinales">
        <button v-if="!editandoReceta" @click="editandoReceta = true" class="btn-receta">
          crear receta
        </button>
        <button v-else @click="editandoReceta = false" class="btn-volver-consulta">
          ← Volver a diagnóstico
        </button>

        <!-- ESTE BOTÓN DISPARA LA PETICIÓN AL SERVIDOR -->
        <button @click="enviarAlServidorYGenerarPDF" class="btn-guardar">
          Guardar y Generar PDF
        </button>
      </div>
    </div>
  </div>
</template>

<script>
import axios from 'axios'; // Importante: instala con npm install axios

export default {
  data() {
    return {
      modoCrearNuevo: false,
      editandoReceta: false,
      nuevaConsulta: {
        diagnostico: '',
        observaciones: '',
        receta: ''
      }
    }
  },
  methods: {
    abrirConsulta() { this.modoCrearNuevo = true; },
    regresar() { this.modoCrearNuevo = false; this.editandoReceta = false; },
    
    async enviarAlServidorYGenerarPDF() {
      if (!this.nuevaConsulta.receta) return alert("Escribe la receta primero.");

      try {
        // LLAMADA AL BACKEND (Donde vive QuestPDF o Puppeteer)
        // debemos reemplazar la url del servidor local o remoto
        const response = await axios.post('http://dirección/url/servidora enviar la informacion', this.nuevaConsulta, {
          responseType: 'blob', // Necesario para recibir el archivo PDF
          headers: {
            'Authorization': 'Bearer TU_TOKEN_AQUÍ' // Aquí aplicas el requisito de Roles (RBAC)
          }
        });

        // DESCARGA AUTOMÁTICA DEL PDF RECIBIDO
        const url = window.URL.createObjectURL(new Blob([response.data]));
        const link = document.createElement('a');
        link.href = url;
        link.setAttribute('download', `Receta_${Date.now()}.pdf`);
        document.body.appendChild(link);
        link.click();

        alert("¡Éxito! Consulta guardada y PDF generado por el servidor.");
        this.regresar();
      } catch (error) {
        console.error("Error:", error);
        alert("Hubo un error al procesar la receta en el servidor.");
      }
    }
  }
}
</script>

<style scoped>

.telemedicina-container {
  max-width: 650px;
  margin: auto;
  padding: 20px;
  background: linear-gradient(60deg, rgb(2, 64, 71) 20%,  rgb(4, 196, 196));
  border-radius: 15px;
  color: white;
}
.botonesFinales { display: flex; gap: 10px; margin-top: 20px; }
.btn-guardar { background: #28a745; color: white; padding: 15px; border-radius: 8px; flex: 1; cursor: pointer; border: none; font-weight: bold; }
.area-receta, .area-diagnostico, .area-observaciones { 
  width: 100%; padding: 50px; border-radius: 8px; background: rgba(255, 255, 255, 0.1); color: white; box-sizing: border-box;
}

</style>
