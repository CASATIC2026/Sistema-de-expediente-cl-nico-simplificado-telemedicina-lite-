const express = require('express');
const { Pool } = require('pg');
const cors = require('cors');

const app = express();
app.use(cors()); 
app.use(express.json());

// CONFIGURACIÓN DE NEON
const pool = new Pool({
  connectionString: 'postgresql://neondb_owner:npg_MiyL9WF1zuIO@ep-aged-hat-a8p7joh8-pooler.eastus2.azure.neon.tech/TelMedLite?sslmode=require&channel_binding=require', 
  ssl: { rejectUnauthorized: false }
});

app.post('/api/registro', async (req, res) => {
  const { nombre, apellido, fechanacimiento, direccion, Género, telefono, email, password } = req.body;
  
  try {
    // 1. Usamos "usuario" en singular.
    // 2. "Género" entre comillas dobles.
    // 3. Agregamos "debe_cambiar_password" con valor false para evitar el error de NULL.
    const query = `
      INSERT INTO usuario 
      (nombre, apellido, "Género", fechanacimiento, direccion, email, password, telefono, debe_cambiar_password, rol) 
      VALUES ($1, $2, $3, $4, $5, $6, $7, $8, $9, $10) 
      RETURNING *`;
    
    // El orden debe coincidir con los $ de arriba
    const values = [
        nombre, 
        apellido, 
        Género, 
        fechanacimiento, 
        direccion, 
        email, 
        password, 
        telefono, 
        false,// Este es el valor para debe_cambiar_password
        'Paciente' // Asignamos el rol de paciente por defecto

    ];
    
    await pool.query(query, values);
    res.json({ mensaje: "¡Éxito! Datos guardados en Neon" });
  } catch (err) {
    console.error("Error en Neon:", err.message);
    res.status(500).json({ mensaje: "Error", error: err.message });
  }
});

// ESTA LÍNEA ES VITAL: Es la que activa el servidor
app.listen(3000, () => console.log("Servidor listo en http://localhost:3000"));