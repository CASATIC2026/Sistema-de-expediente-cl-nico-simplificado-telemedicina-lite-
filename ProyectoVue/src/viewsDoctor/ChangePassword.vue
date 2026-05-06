<script setup>
import { ref } from "vue";
import { useRouter } from "vue-router";
import { changePassword } from "@/services/api";

const router = useRouter();

const passwordActual = ref("");
const passwordNueva = ref("");
const mensaje = ref("");
const error = ref("");
const cargando = ref(false);

// Control de visibilidad
const verActual = ref(false);
const verNueva = ref(false);

const submit = async () => {
  mensaje.value = "";
  error.value = "";
  cargando.value = true;

  try {
    // Llamada al servicio con la respuesta
    const response = await changePassword({
      PasswordActual: passwordActual.value,
      PasswordNueva: passwordNueva.value
    });

    // Guardar nuevo token para evitar 403
    if (response.data && response.data.token) {
      localStorage.setItem("token", response.data.token);
    }

    mensaje.value = "Contraseña actualizada correctamente";

    // Actualizar flags de estado
    localStorage.setItem("requiereCambioPassword", "false");
    localStorage.setItem("perfilCompleto", "true");

    const rol = localStorage.getItem("rol")?.toLowerCase();

    const rutas = {
      paciente: "/app/dashboardPaciente",
      doctor: "/app/MedDashboard",
      admin: "/app/dashboardAdmin"
    };

    // Pausa visual antes de redirigir
    setTimeout(() => {
      router.replace(rutas[rol] || "/");
    }, 1200);

  } catch (err) {
    console.error(err);
    error.value =
      err.response?.data?.message ||
      err.response?.data ||
      "Error al cambiar contraseña";
  } finally {
    cargando.value = false;
  }
};
</script>

<template>
  <div class="flex items-center justify-center h-screen bg-slate-900 text-white">
    <div class="bg-slate-800 p-8 rounded-xl w-80 shadow-lg">

      <h2 class="text-xl mb-6 text-center font-bold">
        Cambiar Contraseña
      </h2>

      <!-- CONTRASEÑA ACTUAL -->
      <div class="mb-4">
        <label class="block text-sm mb-1 opacity-80">
          Contraseña actual
        </label>
        <div class="flex">
          <input
            v-model="passwordActual"
            :type="verActual ? 'text' : 'password'"
            class="w-full p-2 rounded-l bg-slate-700 focus:outline-none focus:ring-2 focus:ring-cyan-500"
          />
          <button
            type="button"
            @click="verActual = !verActual"
            class="bg-slate-600 px-3 rounded-r transition-colors hover:bg-slate-500"
          >
            {{ verActual ? "◡̈" : "👁️" }}
          </button>
        </div>
      </div>

      <!-- NUEVA CONTRASEÑA -->
      <div class="mb-4">
        <label class="block text-sm mb-1 opacity-80">
          Nueva contraseña
        </label>
        <div class="flex">
          <input
            v-model="passwordNueva"
            :type="verNueva ? 'text' : 'password'"
            class="w-full p-2 rounded-l bg-slate-700 focus:outline-none focus:ring-2 focus:ring-cyan-500"
          />
          <button
            type="button"
            @click="verNueva = !verNueva"
            class="bg-slate-600 px-3 rounded-r transition-colors hover:bg-slate-500"
          >
            {{ verNueva ? "◡̈" : "👁️" }}
          </button>
        </div>
      </div>

      <button
        @click="submit"
        :disabled="cargando"
        class="w-full bg-green-600 hover:bg-green-700 p-2 rounded disabled:opacity-50 transition-colors"
      >
        {{ cargando ? "Actualizando..." : "Cambiar contraseña" }}
      </button>

      <p v-if="mensaje" class="text-green-400 mt-3 text-sm text-center">
        {{ mensaje }}
      </p>

      <p v-if="error" class="text-red-400 mt-3 text-sm text-center">
        {{ error }}
      </p>

    </div>
  </div>
</template>
