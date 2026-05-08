///ESTA ES LA SECCION  ( CONTIENE EL CALENDARIO Y LOS STATCARDS)
// Y ABAJO UNA SECCION PARA VER LOS DETALLES DE LA CONSULTA QUE SELECCION DESDE EL CALENDARIO


<script setup>


import { ref, computed, onMounted } from 'vue'
import Calendar from '@/componentsDoctor/calendar/Calendar.vue'
import StatCard from '@/componentsDoctor/ui/StatsCard.vue'
import { useRouter } from 'vue-router'
import { useCitasStore } from '@/stores/citas'
import { useVideoStore } from '@/stores/videoStore'

const router = useRouter()
const citasStore = useCitasStore()
const videoStore = useVideoStore()

const citas   = computed(() => citasStore.citas)
const resumen = computed(() => citasStore.resumen)
const selectedCita = ref(null)

// ─── CARGAR TODO AL MONTAR 
onMounted(async () => {
  await Promise.all([
    citasStore.cargarCitas(),
    citasStore.cargarResumen()
  ])
})


function prepararConsulta(cita) {
  if (!cita.linkReunion) {
    alert('Esta cita no tiene enlace de videollamada configurado.')
    return
  }



  const roomName = cita.linkReunion.split('#')[0].split('?')[0].split('/').pop()

  
  videoStore.iniciarLlamada(roomName, cita.idCita)

  router.push({ name: 'Consulta', params: { id: cita.idCita } })
}

function seleccionarCita(citaDelCalendario) {

  const citaEncontrada = citasStore.citas.find(c => c.idCita === citaDelCalendario.idCita);
  
  if (citaEncontrada) {
    selectedCita.value = citaEncontrada;
    console.log("Cita seleccionada con éxito:", selectedCita.value);
  } else {
    // Si no la encuentra por idCita, intenta por id (a veces el calendario usa 'id')
    selectedCita.value = citasStore.citas.find(c => c.id === citaDelCalendario.id);
  }
}

function moverCita(cita) {
  console.log('Cita movida:', cita)
}
</script>

<template>
  <div class="space-y-6 w-full min-h-screen bg-slate-100 p-6">

    <h1 class="text-2xl font-bold text-gray-800">AgendaMedica Médica</h1>

    <div class="grid grid-cols-1 lg:grid-cols-4 gap-6 w-full">

      <!-- ── STATCARDS -->
      <div class="space-y-4 lg:col-span-1">
        <StatCard title="Citas hoy"   :value="resumen.hoy"        color="text-sky-500"/>
        <StatCard title="Pendientes"  :value="resumen.pendientes"  color="text-amber-500"/>
        <StatCard title="En consulta" :value="resumen.enConsulta" color="text-teal-600"/>
        <StatCard title="Finalizadas" :value="resumen.finalizadas" color="text-green-500"/>
        <StatCard title="Canceladas"  :value="resumen.canceladas"  color="text-red-400"/>
      </div>

      <!-- ── CALENDARIO  -->
      <div class="lg:col-span-3 bg-white p-6 rounded-xl shadow border w-full">

        <div v-if="citasStore.cargando" class="text-gray-400 text-sm">
          Cargando citas...
        </div>

        <Calendar
          v-else-if="citas.length"
          :key="citas.length"
          :citas="citas"
          @cita-click="seleccionarCita"
          @cita-movida="moverCita"
        />

        <div v-else class="text-gray-400 text-sm">
          No hay citas registradas.
        </div>
      </div>

      <!-- DETALLE -->
        <div
          class="p-5 rounded-xl shadow mt-2 lg:col-span-4"
          style="background: linear-gradient(135deg, #1a2e4a 0%, #1e4d6b 100%);"
        >
          <!-- Sin cita seleccionada -->
          <div v-if="!selectedCita" class="text-sm text-white/40 text-center py-2">
            Seleccioná una cita en el calendario para ver los detalles.
          </div>

          <!-- Con cita seleccionada -->
          <div v-else>

            <!-- Header-->
            <div class="flex items-center justify-between mb-4 flex-wrap gap-2">
              <span class="text-sm font-semibold text-white/75 flex items-center gap-2">
                <span class="inline-block w-0.5 h-4 bg-white/50 rounded"></span>
                Detalle de la cita
              </span>
              <span
                class="text-xs font-semibold px-3 py-1 rounded-full"
                :class="{
                  'bg-white/20 text-white':      selectedCita.estado === 'Pendiente',
                  'bg-green-400/30 text-white':  selectedCita.estado === 'EnConsulta',
                  'bg-white/10 text-white/60':   selectedCita.estado === 'Finalizada',
                  'bg-red-400/30 text-white':    selectedCita.estado === 'Cancelada',
                }"
              >
                {{ selectedCita.estado === 'EnConsulta' ? 'En consulta' : selectedCita.estado }}
              </span>
            </div>

            <!-- Grid horizontal -->
            <div class="grid grid-cols-2 md:grid-cols-4 gap-4 items-end">

              <div class="flex flex-col gap-1">
                <span class="text-[11px] font-semibold uppercase tracking-wide text-white/60">Paciente</span>
                <span class="text-sm font-semibold text-white">{{ selectedCita.paciente }}</span>
              </div>

              <div class="flex flex-col gap-1">
                <span class="text-[11px] font-semibold uppercase tracking-wide text-white/60">Hora</span>
                <span class="text-sm font-semibold text-white">
                  {{ new Date(selectedCita.start).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }) }}
                </span>
              </div>

              <div class="flex flex-col gap-1">
                <span class="text-[11px] font-semibold uppercase tracking-wide text-white/60">Tipo de consulta</span>
                <span class="text-sm font-semibold text-white">{{ selectedCita.tipo }}</span>
              </div>

              <!--  -->
              <div class="flex justify-end">

                <button
                  v-if="selectedCita.estado === 'Pendiente' || selectedCita.estado === 'EnConsulta'"
                  @click="prepararConsulta(selectedCita)"
                  class="text-sm font-semibold px-5 py-2 rounded-lg transition text-white border border-white/30 hover:bg-white/20"
                >
                  Entrar a consulta
                </button>
                <span v-else class="text-xs text-white/40">
                  Sin acciones disponibles
                </span>
              </div>

            </div>
          </div>
        </div>
    </div>
  </div>
</template>