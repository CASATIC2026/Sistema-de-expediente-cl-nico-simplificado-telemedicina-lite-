import axios from "axios";
import router from "@/router";

const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL,
  headers: {
    "Content-Type": "application/json"
  }
});

// REQUEST → Enviar token automáticamente
api.interceptors.request.use(config => {
  const token = localStorage.getItem("token");

  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }

  return config;
});

// RESPONSE → Manejar errores globales
api.interceptors.response.use(
  response => response,
  error => {
    const status = error.response?.status;
    console.error("API ERROR:", error.response?.data || error.message);

    if (status === 401) {

      const currentPath = router.currentRoute.value.path;
      if (
        currentPath !== '/' &&
         currentPath !== '/login'
        ) {
          logoutPro();

        }

    }

    if (status === 403) {
      const msg = String(error.response?.data?.message || '');

    if (msg.toLowerCase().includes('password')) {
        localStorage.setItem('requiereCambioPassword', 'true');
        if (router.currentRoute.value.path !== "/change-password") {
          router.push("/change-password");
        }
      }
    }
    return Promise.reject(error);
  }
);


 export const buscarPacientePorDui = async (dui) => {
  const res = await api.get('/citas/buscar-dui', { params: { dui } });
  return res.data;
}

// CITAS Y GESTIÓN
export const getCitas = async (params) => {
  const res = await api.get('/citas', { params });
  return res.data;
};

export const getCalendario = async () => {
  const res = await api.get('/citas/calendario');
  return res.data;
};

export const getCitaById = async (id) => {
  const res = await api.get(`/citas/${id}`);
  return res.data;
};

export const getResumen = async () => {
  const res = await api.get('/citas/resumen');
  return res.data;
};

export const crearCita = async (data) => {
  const res = await api.post('/citas', data);
  return res.data;
};

export const updateCita = async (id, data) => {
  const res = await api.put(`/citas/${id}`, data);
  return res.data;
};

export const confirmarCita = async (id) => {
  const res = await api.put(`/citas/${id}/confirmar`);
  return res.data;
};

export const cancelarCita = async (id) => {
  const res = await api.put(`/citas/${id}/cancelar`);
  return res.data;
};

export const getDoctores = async () => {
  const res = await api.get('/users/doctors');
  return res.data;
};

export const getPacientes = async () => {
  const res = await api.get('/users/patients');
  return res.data;
};

export const getHistorialPaciente = async (pacienteId) => {
  const res = await api.get(`/citas/historial-paciente/${pacienteId}`);
  return res.data;
}

export const getHorariosOcupados = async (doctorId, fecha) => {
  const fechaStr = typeof fecha === 'string'
    ? fecha
    : new Date(fecha).toISOString().split('T')[0]

  const res = await api.get(`/citas/calendario`, {
    params: { doctorId, fecha: fechaStr, soloOcupados: true }
  })
  return res.data
};

export const getMisCitasDetalle = async () => {
  const res = await api.get('/citas/mis-citas-detalle');
  return res.data;
};


// ADMIN CREAR UN NUEVO DOCTOR =================================================
export const getDoctoresAdmin = async () => {
  const res = await api.get('/users/doctors-admin')
  return res.data
}

export const toggleEstadoDoctor = async (id, activo) => {
  const res = await api.put(`/users/${id}/estado`, { activo })
  return res.data
}

// ADMIN - GESTIÓN DE PACIENTES
export const getPacientesAdmin = async () => {
  const res = await api.get('/users/patients-admin')
  return res.data
}


// DOCTOR - GESTIÓN DE PACIENTES
export const getPacientesDoctor = async () => {
  const res = await api.get('/users/my-patients')
  return res.data
}

export const toggleEstadoPaciente = async (id, activo) => {
  const res = await api.put(`/users/${id}/estado`, { activo })
  return res.data
}


// CONSULTA (Videollamada)===============================
export const iniciarConsulta = async (id) => {
  const res = await api.put(`/citas/${id}/iniciar`);
  return res.data;
};

export const finalizarConsulta = async (id) => {
  const res = await api.put(`/citas/${id}/finalizar`);
  return res.data;
};


// RECETA MÉDICA==============================
export const descargarReceta = async (id) => {
  const res = await api.get(`/citas/${id}/receta`, {
    responseType: 'blob'
  });
  return res.data;
};

export const descargarRecetaAdmin = async (citaId) => {
  const res = await api.get(`/citas/${citaId}/receta`, {
    responseType: 'blob'
  })
  return res.data
}

// AUTH (Identificación) =========================
export const login = async (data) => {
  const res = await api.post('/auth/login', data);

  if (res.data.id) {
    localStorage.setItem('user_id', res.data.id)
  }

  return res.data
};


// olvidé mi contraseña y restablecerla ===========================
export const forgotPassword = async (email) => {
  const res = await api.post('/auth/forgot-password', { email });
  return res.data;
};

// restablecer contraseña con token
export const resetPassword = async (token, newPassword) => {
  const res = await api.post('/auth/reset-password', { token, newPassword });
  return res.data;
};


export const register = async (data) => {
  const res = await api.post('/auth/register', data);
  return res.data;
};

export const perfil = async () => {
  const res = await api.get('/users/me');
  const data = res.data;

  if (!data.id) {
    throw new Error("No viene ID");
  }

  return data;
};

export const changePassword = async (data) => {
  const res = await api.post('/auth/cambiar-password', data);
  return res.data;
};

export const createDoctor = async (data) => {
  const res = await api.post('/auth/create-doctor', data);
  return res.data;
};

// Horario actual de un doctor
export const getHorarioDoctor = (doctorId) =>
  api.get(`/citas/horarios-disponibles?doctorId=${doctorId}`).then(r => r.data)

// Guardar horario modificado por el admin
export const actualizarHorarioDoctor = (doctorId, payload) =>
  api.put(`/citas/horarios-doctor/${doctorId}`, payload).then(r => r.data)

// Slots libres para que el paciente elija al agendar
export const getSlotsDisponibles = (doctorId, fecha) => {
  const fechaStr =
    typeof fecha === 'string'
      ? fecha
      : new Date(fecha).toISOString().split('T')[0]

  return api.get(`/citas/slots-disponibles`, {
    params: {
      doctorId,
      fecha: fechaStr
    }
  }).then(r => r.data)
}

// Eliminar paciente (soft delete)
export const eliminarPacienteSoft = (id) =>
  api.delete(`/users/${id}`).then(r => r.data)


export const getEstadisticas = async () => {
  const res = await api.get('/users/estadisticas');
  return res.data;
};


// CIERRE DE SESIÓN ================================
export const logoutPro = () => {
  localStorage.removeItem('token');
localStorage.removeItem('user_id');
localStorage.removeItem('requiereCambioPassword');
  sessionStorage.clear();
  window.location.href = '/';
};

export default api;
