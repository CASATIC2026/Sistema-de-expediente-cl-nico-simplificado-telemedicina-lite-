<script setup>
import { ref, computed, watch, onMounted } from 'vue'
import { perfil } from '@/services/api'
import api from '@/services/api'
import { profileIcons } from '@/utils/profileIcons'
import {
  XMarkIcon,
  UserCircleIcon,
  IdentificationIcon,
  PhoneIcon,
  EnvelopeIcon,
  MapPinIcon,
  ShieldCheckIcon,
  PencilSquareIcon,
  EyeIcon,
  EyeSlashIcon,
  BuildingOffice2Icon,
  ClockIcon,
  CheckCircleIcon,
  ExclamationTriangleIcon,
  ClipboardDocumentIcon,
  NoSymbolIcon
} from '@heroicons/vue/24/outline'

// ── 1. PROPS Y EMITS primero siempre ──────────────────
const props = defineProps({
  usuarioData: { type: Object, default: () => ({}) },
  rolSesion:   { type: String, default: 'paciente' },
  modoAdmin:   { type: Boolean, default: false },
  pacienteId:  { type: Number, default: null }
})

onMounted(() => {
  if (props.rolSesion?.toLowerCase() === 'admin') {
    cargarPerfilClinico()
  }
})

const emit = defineEmits(['cerrar', 'actualizar'])

// ── 2. COMPUTED que dependen de props ─────────────────
const tituloModal = computed(() => {
  const rol = props.rolSesion?.toLowerCase()
  if(props.modoAdmin) {
    return formulario.value.rol?.toLowerCase() === 'doctor'
     ? 'Perfil del Profesional'
     : 'Perfil del Paciente'
  }
  if(rol === 'admin')  return 'Panel de Administración'
  if(rol === 'doctor') return 'Mi Perfil Médico'
  return 'Mi Perfil'
})

const subtituloModal = computed(() => {
  if (props.modoAdmin)  return 'Vista de solo lectura'
  const rol = props.rolSesion?.toLowerCase()
  if (rol === 'admin')  return 'Gestión y Configuración'
  if (rol === 'doctor') return 'Información del Profesional'
  return 'Identidad Digital'
})

const puedeEditar = computed(() => {
  if (props.modoAdmin) return false
  return ['paciente', 'admin', 'doctor'].includes(props.rolSesion?.toLowerCase())
})

const esDoctor = computed(() =>{
 return formulario.value.rol?.toLowerCase() === 'doctor'
})

const esUsuarioGoogle = computed(
  () => formulario.value.esGoogle === true)

  // Iconos disponibles para perfil
  const iconosDisponibles = computed(() => {
  const rol = formulario.value.rol?.toLowerCase()

  if (rol === 'admin') return profileIcons.admin
  if (rol === 'doctor') return profileIcons.doctor

  return profileIcons.paciente
})

// ── Badge de rol (sugerencia nueva) ──────────────────
const rolBadgeConfig = computed(() => {
  const rol = formulario.value.rol?.toLowerCase()
  if (rol === 'admin')  return { label: 'Administrador', cls: 'bg-amber-500/15 text-amber-400 border-amber-500/25 dark:bg-amber-500/10 dark:text-amber-300 dark:border-amber-500/20' }
  if (rol === 'doctor') return { label: 'Doctor',        cls: 'bg-teal-500/15 text-teal-600 border-teal-500/25 dark:bg-teal-500/10 dark:text-teal-300 dark:border-teal-500/20' }
  return                       { label: 'Paciente',      cls: 'bg-indigo-500/15 text-indigo-600 border-indigo-500/25 dark:bg-indigo-500/10 dark:text-indigo-300 dark:border-indigo-500/20' }
})

// ── 3. Estado local ───────────────────────────────────
const editarPerfil = ref(false)
const cargando     = ref(false)
const formulario   = ref({})
const clinicProfile = ref({
  clinicName: '',
  slogan: '',
  horario: '',
  telefono: ''
})

const guardandoClinica = ref(false)

// ── Notificación inline ───────────────────────────────
const notificacion = ref({ mensaje: '', tipo: '' })
const mostrarNotificacion = (mensaje, tipo = 'exito') => {
  notificacion.value = { mensaje, tipo }
  setTimeout(() => notificacion.value = { mensaje: '', tipo: '' }, 4000)
}

// ── Copiar email (sugerencia nueva) ──────────────────
const copiarEmail = () => {
  if (!formulario.value.email) return
  navigator.clipboard.writeText(formulario.value.email)
    .then(() => mostrarNotificacion('Email copiado al portapapeles.'))
    .catch(() => mostrarNotificacion('No se pudo copiar el email.', 'error'))
}

// ── 4. watchEffect: reacciona cuando llegan datos ─────
watch(
  () => props.usuarioData,
  (d) => {
    if (!d || Object.keys(d).length === 0) return

    formulario.value = {
      nombre: d.nombre ?? d.Nombre ?? '',
      apellido: d.apellido ?? d.Apellido ?? '',
      email: d.email ?? d.Email ?? '',
      dui: d.dui ?? d.DUI ?? 'No disponible',
      telefono: d.telefono ?? d.Telefono ?? '',
      direccion: d.direccion ?? d.Direccion ?? '',
      genero: d.genero ?? d.Genero ?? 'No especificado',
      rol: d.rol ?? d.Rol ?? '',
      especialidad: d.especialidad ?? d.Especialidad ?? '',
      jvpm: d.jvpm ?? d.JVPM ?? '',
      esGoogle: d.esGoogle ?? d.EsGoogle ?? false,
      avatarIcono: d.avatarIcono ?? d.AvatarIcono ?? '',
      fotoUrl: d.fotoUrl ?? d.FotoUrl ?? ''
    }

    if (
      formulario.value.esGoogle &&
      !formulario.value.avatarIcono
    ) {
      formulario.value.avatarIcono = '__GOOGLE__'
    }
  },
  {
    immediate: true
  }
)

const cargarPerfilClinico = async () => {
  try {
    const res = await api.get('/ClinicProfile')
    clinicProfile.value = {
      clinicName: res.data.clinicName || '',
      slogan: res.data.slogan || '',
      horario: res.data.horario || '',
      telefono: res.data.telefono || ''
    }
  } catch {}
}

// ── 5. Guardar cambios (solo modo propio) ─────────────
const guardarEdicionCambios = async () => {
  if (props.modoAdmin) return
  try {
    cargando.value = true
    await api.put('/Users/perfil', {
    nombre: formulario.value.nombre,
    apellido: formulario.value.apellido,
    direccion: formulario.value.direccion,
    telefono: formulario.value.telefono,
    avatarIcono:
      formulario.value.avatarIcono === '__GOOGLE__'
        ? ''
        : formulario.value.avatarIcono
  })
    editarPerfil.value = false
    try {
      const actualizado = await perfil()
      console.log({
  avatarIcono: actualizado.avatarIcono,
  fotoUrl: actualizado.fotoUrl,
  esGoogle: actualizado.esGoogle
})
      formulario.value = {
        nombre:    actualizado.nombre    ?? actualizado.Nombre    ?? '',
        apellido:  actualizado.apellido  ?? actualizado.Apellido  ?? '',
        email:     actualizado.email     ?? actualizado.Email     ?? '',
        dui:       actualizado.dui       ?? actualizado.DUI       ?? 'No disponible',
        telefono:  actualizado.telefono  ?? actualizado.Telefono  ?? '',
        direccion: actualizado.direccion ?? actualizado.Direccion ?? '',
        genero:    actualizado.genero    ?? actualizado.Genero    ?? 'No especificado',
        rol:       actualizado.rol       ?? actualizado.Rol       ?? '',
        avatarIcono: actualizado.avatarIcono ?? actualizado.AvatarIcono ?? '',
        fotoUrl: actualizado.fotoUrl ?? actualizado.FotoUrl ?? '',
        esGoogle: actualizado.esGoogle ?? actualizado.EsGoogle ?? false,
      }

      if (
          formulario.value.esGoogle &&
          !formulario.value.avatarIcono
        ) {
          formulario.value.avatarIcono = '__GOOGLE__'
        }
      localStorage.setItem(
        'user_name',
        `${actualizado.nombre} ${actualizado.apellido}`
      )
    } catch {}
    emit('actualizar')
    mostrarNotificacion('Perfil actualizado correctamente.')
  } catch (e) {
  mostrarNotificacion(
    e.response?.data?.message ||
    'No se pudo actualizar la información.',
    'error'
  )
} finally {
    cargando.value = false
  }
}

const guardarPerfilClinico = async () => {
  try {
    guardandoClinica.value = true
    await api.put('/ClinicProfile', clinicProfile.value)
    emit('actualizarClinic')
    mostrarNotificacion('Información clínica actualizada correctamente.')
  } catch (_) {
  mostrarNotificacion(
    'No se pudo actualizar la información clínica.',
    'error'
  )
} finally {
    guardandoClinica.value = false
  }
}

// ── Cambio de contraseña ──────────────────────────────
const mostrarCambioPassword = ref(false)
const cargandoPassword      = ref(false)
const passwordForm = ref({ actual: '', nueva: '', confirmar: '' })

const passwordValido = computed(() => {
  const nueva = passwordForm.value.nueva
  return (
    passwordForm.value.actual &&
    nueva &&
    nueva === passwordForm.value.confirmar &&
    nueva.length >= 6 &&
    /[A-Z]/.test(nueva) &&
    /[a-z]/.test(nueva) &&
    /\d/.test(nueva) &&
    /[@$!%*?&]/.test(nueva)
  )
})

const cancelarCambioPassword = () => {
  mostrarCambioPassword.value = false
  Object.assign(passwordForm.value, { actual: '', nueva: '', confirmar: '' })
}

const cambiarPassword = async () => {
  if (!passwordValido.value) return
  try {
    cargandoPassword.value = true
    await api.post('/Auth/cambiar-password', {
      passwordActual: passwordForm.value.actual,
      passwordNueva:  passwordForm.value.nueva
    })
    mostrarNotificacion('¡Contraseña actualizada correctamente!')
    cancelarCambioPassword()
  } catch (e) {
    mostrarNotificacion(e.response?.data?.message || 'No se pudo cambiar la contraseña.', 'error')
  } finally {
    cargandoPassword.value = false
  }
}

const verPassword = ref({ actual: false, nueva: false, confirmar: false })
</script>

<template>
  <div
  class="fixed inset-0 z-[9999] flex items-center justify-center overflow-y-auto bg-slate-950/80 backdrop-blur-sm p-2 md:p-4"
  @click.self="emit('cerrar')"
>
    <!-- Modal wrapper -->
    <div class="
      bg-white dark:bg-slate-900
      text-slate-800 dark:text-white
      w-full max-w-xl max-h-[92vh] md:max-h-[95vh]
      overflow-hidden rounded-[1.5rem] md:rounded-[2rem]
      shadow-2xl flex flex-col animate-in
     border border-slate-200 dark:border-slate-700
    ">

      <!-- ── HEADER ────────────────────────────────────────── -->

      <header class="
        relative overflow-hidden px-5 py-5 md:px-6 md:pt-6 md:pb-4
        text-center shrink-0
        border-b border-slate-500/20 dark:border-indigo-500/10
        bg-slate-900 dark:bg-slate-950
        shadow-[0_0_40px_rgba(6,182,212,0.10)]
      ">

        <!-- Glows decorativos -->
        <div class="absolute -top-20 left-1/2 -translate-x-1/2 w-72 h-72 rounded-full bg-white/5 dark:bg-indigo-500/10 blur-3xl pointer-events-none" />
        <div class="absolute top-0 right-0 w-40 h-40 rounded-full bg-white/5 dark:bg-violet-500/10 blur-3xl pointer-events-none" />
        <div class="absolute inset-0 opacity-[0.03] bg-[radial-gradient(circle_at_top,white_0%,transparent_60%)]" />


        <!-- Botón cerrar -->
        <button
          @click="emit('cerrar')"
          class="absolute top-4 right-4 md:top-5 md:right-6 text-red-300 bg-red-500/10 hover:bg-red-500/25 border border-red-400/20 transition-all p-2 rounded-full cursor-pointer"
          title="Cerrar"
        >
          <XMarkIcon class="h-4 w-4" />
        </button>


        <!-- Avatar del header-->
        <div class="mx-auto w-16 h-16 md:w-20 md:h-20 rounded-2xl overflow-hidden bg-white/10 border border-white/20 shadow-lg">

          <!-- FOTO GOOGLE -->
          <img
            v-if="
              formulario.fotoUrl &&
              formulario.avatarIcono === '__GOOGLE__'
            "
            :src="formulario.fotoUrl"
            class="w-full h-full object-cover"
          />

          <!-- ICONO PERSONALIZADO -->
          <img
            v-else-if="
              formulario.avatarIcono &&
              formulario.avatarIcono !== '__GOOGLE__'
            "
            :src="iconosDisponibles.find(i => i.id === formulario.avatarIcono)?.src"
            class="w-full h-full object-cover"
          />

          <!-- INICIALES -->
          <div
            v-else
            class="w-full h-full flex items-center justify-center"
          >
            <span class="text-xl md:text-2xl text-white font-black uppercase tracking-wider">
              {{ formulario.nombre?.[0] || '?' }}{{ formulario.apellido?.[0] || '' }}
            </span>
          </div>
        </div>

        <!-- Titulo del modal-->
        <h2 class="text-base md:text-lg font-black text-white tracking-tight leading-tight uppercase drop-shadow-[0_0_10px_rgba(255,255,255,0.08)]">
          {{ tituloModal }}
        </h2>
        <p class="text-slate-300 text-[10px] md:text-xs font-semibold mt-1 uppercase tracking-[0.18em]">
          {{ subtituloModal }}
        </p>

        <!-- Badge de rol  -->
        <div class="mt-2.5 flex justify-center">
          <span
            class="text-[9px] font-black uppercase tracking-widest px-3 py-1 rounded-full border"
            :class="rolBadgeConfig.cls"
          >
            {{ rolBadgeConfig.label }}
          </span>
        </div>
      </header>

      <!-- ── NOTIFICACIÓN ──────────────────────────────────── -->
      <transition name="notif">
        <div
          v-if="notificacion.mensaje"
          class="mx-4 md:mx-6 mt-4 px-4 py-3 rounded-xl text-xs md:text-sm font-bold flex items-center gap-3 shrink-0"
          :class="notificacion.tipo === 'exito'
            ? 'bg-emerald-500/10 text-emerald-600 dark:text-emerald-400 border border-emerald-500/20'
            : 'bg-red-500/10 text-red-600 dark:text-red-400 border border-red-500/20'"
        >
          <CheckCircleIcon v-if="notificacion.tipo === 'exito'" class="h-4 w-4 shrink-0" />
          <ExclamationTriangleIcon v-else class="h-4 w-4 shrink-0" />
          {{ notificacion.mensaje }}
        </div>
      </transition>

      <!-- ── CUERPO SCROLLEABLE ─────────────────────────────── -->
      <div class="
        flex-1 overflow-y-auto
        px-4 pb-4 pt-4 md:px-6 md:pb-6 md:pt-4
        custom-scrollbar space-y-5
        bg-slate-50 dark:bg-slate-950
      ">

        <!-- DATOS GENERALES ─────────────────────────────────── -->
        <div class="space-y-2.5">
          <div class="flex items-center gap-2 px-1">
            <UserCircleIcon class="h-3.5 w-3.5 text-slate-900 dark:text-indigo-400" />
            <h3 class="text-[10px] md:text-[11px] font-black uppercase tracking-[0.15em] text-slate-600 dark:text-indigo-400">
              Datos generales
            </h3>
          </div>

          <div class="grid grid-cols-1 sm:grid-cols-2 gap-2.5">
            <!-- Nombre completo -->
            <div class="sm:col-span-2 bg-indigo-50 dark:bg-indigo-500/5 p-3.5 rounded-xl border border-indigo-200 dark:border-indigo-500/15 flex flex-col gap-0.5">
              <span class="text-[9px] md:text-[10px] font-black text-slate-500 dark:text-slate-400 uppercase tracking-wider">Nombre Completo</span>
              <span class="text-sm md:text-base font-bold text-slate-800 dark:text-slate-100 tracking-wide">
                {{ formulario.nombre }} {{ formulario.apellido }}
              </span>
            </div>

            <!-- DUI -->
            <div class="bg-indigo-50 dark:bg-indigo-500/5 p-3.5 rounded-xl border border-indigo-200 dark:border-indigo-500/15 flex flex-col gap-0.5">
              <span class="text-[9px] md:text-[10px] font-black text-slate-500 dark:text-slate-400 uppercase tracking-wider flex items-center gap-1.5">
                <IdentificationIcon class="h-3 w-3" />
                DUI
              </span>
              <span class="text-xs md:text-sm font-bold text-slate-800 dark:text-slate-100 tracking-wider font-mono">
                {{ formulario.dui || 'No disponible' }}
              </span>
            </div>

            <!-- Género -->
            <div
              class="bg-indigo-50 dark:bg-indigo-500/5 p-3.5 rounded-xl border border-indigo-200 dark:border-indigo-500/15 flex flex-col gap-0.5"
              :class="{'sm:col-span-2': !esDoctor}"
            >
              <span class="text-[9px] md:text-[10px] font-black text-slate-500 dark:text-slate-400 uppercase tracking-wider">Género</span>
              <span class="text-xs md:text-sm font-bold text-slate-800 dark:text-slate-100">
                {{ formulario.genero || 'No especificado' }}
              </span>
            </div>

            <!-- Especialidad y JVPM (solo doctor) -->
            <template v-if="esDoctor">
              <div class="bg-indigo-50 dark:bg-indigo-500/5 p-3.5 rounded-xl border border-indigo-200 dark:border-indigo-500/15 flex flex-col gap-0.5">
                <span class="text-[9px] md:text-[10px] font-black text-slate-500 dark:text-slate-400 uppercase tracking-wider">Especialidad</span>
                <span class="text-xs md:text-sm font-bold text-slate-800 dark:text-slate-100">
                  {{ formulario.especialidad || 'No asignada' }}
                </span>
              </div>
              <div class="bg-indigo-50 dark:bg-indigo-500/5 p-3.5 rounded-xl border border-indigo-200 dark:border-indigo-500/15 flex flex-col gap-0.5">
                <span class="text-[9px] md:text-[10px] font-black text-slate-500 dark:text-slate-400 uppercase tracking-wider">JVPM</span>
                <span class="text-xs md:text-sm font-bold text-slate-800 dark:text-slate-100 tracking-widest">
                  {{ formulario.jvpm || 'No asignado' }}
                </span>
              </div>
            </template>
          </div>
        </div>

        <!-- CONTACTO ─────────────────────────────────────────── -->
        <div class="space-y-2.5">
          <div class="flex items-center justify-between px-1">
            <div class="flex items-center gap-2">
              <PhoneIcon class="h-3.5 w-3.5 text-slate-800 dark:text-violet-300" />
              <h3 class="text-[10px] md:text-[11px] font-black uppercase tracking-[0.15em] text-slate-600 dark:text-indigo-400">
                Información de contacto
              </h3>
            </div>
            <button
              v-if="!editarPerfil && puedeEditar && !props.modoAdmin"
              @click="editarPerfil = true"
              class="text-[9px] md:text-[10px] font-black uppercase bg-indigo-100 dark:bg-indigo-500/10 text-slate-500 dark:text-indigo-300 border border-indigo-300 dark:border-indigo-500/20 px-3 py-1.5 rounded-xl hover:bg-indigo-500 hover:text-white hover:border-transparent transition-all cursor-pointer flex items-center gap-1.5"
            >
              <PencilSquareIcon class="h-3 w-3" />
              Editar
            </button>
          </div>

          <div class="bg-indigo-50 dark:bg-indigo-500/5 rounded-xl border border-indigo-200 dark:border-indigo-500/15 overflow-hidden">

            <!-- Teléfono -->
            <div class="flex flex-col gap-1.5 p-3.5 border-b border-indigo-100 dark:border-indigo-500/10">
              <label class="text-[9px] md:text-[10px] font-black text-slate-500 dark:text-slate-400 uppercase tracking-wider flex items-center gap-1.5">
                <PhoneIcon class="h-3 w-3" />
                Teléfono
              </label>
              <div v-if="!editarPerfil" class="text-xs md:text-sm font-bold text-slate-800 dark:text-slate-100 px-0.5">
                {{ formulario.telefono || 'No asignado' }}
              </div>
              <input
                v-else
                v-model="formulario.telefono"
                type="text"
                class="w-full px-3 py-2 bg-white dark:bg-[#0a0f1e] border border-slate-300 dark:border-indigo-500/20 focus:border-indigo-500 dark:focus:border-indigo-400 rounded-xl outline-none transition-all font-bold text-slate-800 dark:text-white text-sm focus:ring-1 focus:ring-indigo-500/20"
              />
            </div>

            <!-- Email + botón copiar -->
            <div class="flex flex-col gap-1 p-3.5 border-b border-indigo-100 dark:border-indigo-500/10">
              <label class="text-[9px] md:text-[10px] font-black text-slate-500 dark:text-slate-400 uppercase tracking-wider flex items-center gap-1.5">
                <EnvelopeIcon class="h-3 w-3" />
                Correo Electrónico
              </label>
              <div class="flex items-center justify-between gap-2">
                <div class="text-xs md:text-sm font-bold text-slate-900 dark:text-indigo-300 break-all px-0.5">
                  {{ formulario.email }}
                </div>
                <button
                  @click="copiarEmail"
                  title="Copiar email"
                  class="shrink-0 text-slate-500 hover:text-indigo-500
                  dark:hover:text-indigo-400 transition-colors p-1 rounded-lg hover:bg-indigo-100 dark:hover:bg-indigo-500/10 cursor-pointer"
                >
                  <ClipboardDocumentIcon class="h-3.5 w-3.5" />
                </button>
              </div>
            </div>

            <!-- Avatar Selector -->
            <div
              v-if="editarPerfil"
              class="flex flex-col gap-2 p-3.5 border-b border-indigo-100 dark:border-indigo-500/10">

              <label class="text-[9px] md:text-[10px] font-black text-slate-500 dark:text-slate-400 uppercase tracking-wider">
                Avatar de Perfil
              </label>

              <div class="grid grid-cols-3 sm:grid-cols-4 gap-3">

                <button
                  v-if="!esUsuarioGoogle"
                  type="button"
                  @click="formulario.avatarIcono = ''"
                  class="rounded-2xl border-2 aspect-square flex flex-col items-center justify-center gap-2 transition-all cursor-pointer"
                  :class="
                    !formulario.avatarIcono
                      ? 'border-indigo-500 bg-indigo-500/10'
                      : 'border-slate-200 dark:border-slate-700'
                  "
                >
                  <NoSymbolIcon class="w-8 h-8 text-slate-400" />
                  <span class="text-[10px] font-black uppercase tracking-wider text-slate-500 dark:text-slate-400">
                    Sin icono
                  </span>
                </button>

                <button
                      v-if="esUsuarioGoogle"
                      type="button"
                      @click="formulario.avatarIcono = '__GOOGLE__'"
                      class="rounded-2xl border-2 aspect-square flex flex-col items-center justify-center gap-2 transition-all cursor-pointer"
                      :class="
                        formulario.avatarIcono === '__GOOGLE__'
                          ? 'border-indigo-500 bg-indigo-500/10'
                          : 'border-slate-200 dark:border-slate-700'
                      "
                    >
                      <img
                        :src="formulario.fotoUrl"
                        class="w-10 h-10 rounded-full object-cover"
                      />

                      <span class="text-[10px] font-black uppercase tracking-wider">
                        Google
                      </span>
                    </button>

                <button
                  v-for="icono in iconosDisponibles"
                  :key="icono.id"
                  type="button"
                  @click="formulario.avatarIcono = icono.id"
                  class="relative rounded-2xl overflow-hidden border-2 transition-all aspect-square bg-white dark:bg-[#0a0f1e] hover:scale-105 cursor-pointer"
                  :class="
                    formulario.avatarIcono === icono.id
                      ? 'border-indigo-500 shadow-[0_0_20px_rgba(99,102,241,0.45)]'
                      : 'border-slate-200 dark:border-slate-700'
                  "
                >
                  <img
                    :src="icono.src"
                    class="w-full h-full object-cover"
                  />

                  <div
                    v-if="formulario.avatarIcono === icono.id"
                    class="absolute inset-0 bg-indigo-500/20 flex items-center justify-center"
                  >
                    <CheckCircleIcon class="w-6 h-6 text-white drop-shadow-lg" />
                  </div>
                </button>
              </div>
            </div>

            <!-- Dirección -->
            <div class="flex flex-col gap-1 p-3.5">
              <label class="text-[9px] md:text-[10px] font-black text-slate-500 dark:text-slate-400 uppercase tracking-wider flex items-center gap-1.5">
                <MapPinIcon class="h-3 w-3" />
                Dirección
              </label>
              <div
                v-if="!editarPerfil"
                class="text-xs md:text-sm font-semibold text-slate-600 dark:text-slate-300 italic break-words bg-white/80 dark:bg-[#0a0f1e]/50 p-3 rounded-xl border border-indigo-100 dark:border-indigo-500/10"
              >
                "{{ formulario.direccion || 'No asignada' }}"
              </div>
              <textarea
                v-else
                v-model="formulario.direccion"
                rows="2"
                class="w-full px-3 py-2 bg-white dark:bg-[#0a0f1e] border border-slate-300 dark:border-indigo-500/20 focus:border-indigo-500 dark:focus:border-indigo-400 rounded-xl outline-none transition-all font-bold text-slate-800 dark:text-white text-sm resize-none focus:ring-1 focus:ring-indigo-500/20"
              ></textarea>
            </div>
          </div>
        </div>

        <!-- PERFIL CLÍNICO ───────────────────────────────────── -->
        <div
          v-if="props.rolSesion?.toLowerCase() === 'admin'"
          class="space-y-2.5"
        >
          <div class="flex items-center gap-2 px-1">
            <BuildingOffice2Icon class="h-3.5 w-3.5 text-teal-600 dark:text-teal-200" />
            <h3 class="text-[10px] md:text-[11px] font-black uppercase tracking-[0.15em] text-teal-600 dark:text-teal-200">
              Perfil Clínico
            </h3>
          </div>

          <div class="bg-teal-50 dark:bg-teal-200/5 rounded-xl border border-teal-200/60 dark:border-teal-200/15 p-4 space-y-4">

            <div>
              <label class="text-xs text-slate-500 dark:text-slate-400 font-bold uppercase">
                Nombre Clínica
              </label>
              <input
                v-model="clinicProfile.clinicName"
                type="text"
                class="w-full mt-1 px-4 py-2 rounded-xl bg-white dark:bg-[#0a0f1e] border border-teal-300 dark:border-teal-200/20 text-slate-800 dark:text-white outline-none focus:border-teal-500 dark:focus:border-teal-400 transition-all"
              />
            </div>

            <div>
              <label class="text-xs text-slate-500 dark:text-slate-400 font-bold uppercase">
                Slogan
              </label>
              <input
                v-model="clinicProfile.slogan"
                type="text"
                class="w-full mt-1 px-4 py-2 rounded-xl bg-white dark:bg-[#0a0f1e] border border-teal-300 dark:border-teal-200/20 text-slate-800 dark:text-white outline-none focus:border-teal-500 dark:focus:border-teal-400 transition-all"
              />
            </div>

            <div>
              <label class="text-xs text-slate-500 dark:text-slate-400 font-bold uppercase flex items-center gap-1.5">
                <ClockIcon class="h-3 w-3" />
                Horario
              </label>
              <input
                v-model="clinicProfile.horario"
                type="text"
                class="w-full mt-1 px-4 py-2 rounded-xl bg-white dark:bg-[#0a0f1e] border border-teal-300 dark:border-teal-200/20 text-slate-800 dark:text-white outline-none focus:border-teal-500 dark:focus:border-teal-400 transition-all"
              />
            </div>

            <div>
              <label class="text-xs text-slate-500 dark:text-slate-400 font-bold uppercase">
                Teléfono
              </label>
              <input
                v-model="clinicProfile.telefono"
                type="text"
                class="w-full mt-1 px-4 py-2 rounded-xl bg-white dark:bg-[#0a0f1e] border border-teal-300 dark:border-teal-200/20 text-slate-800 dark:text-white outline-none focus:border-teal-500 dark:focus:border-teal-400 transition-all"
              />
            </div>

            <button
              @click="guardarPerfilClinico"
              :disabled="guardandoClinica"
              class="w-full bg-teal-500 dark:bg-teal-200 hover:bg-teal-600 dark:hover:bg-teal-300 text-white dark:text-slate-950 font-black py-3 rounded-xl uppercase tracking-wider transition-all"
            >
              {{ guardandoClinica ? 'Guardando...' : 'Guardar Perfil Clínico' }}
            </button>
          </div>
        </div>

        <!-- SEGURIDAD ─────────────────────────────────────────── -->
        <div v-if="!props.modoAdmin && !esUsuarioGoogle" class="space-y-2.5">
          <div class="flex items-center justify-between px-1">
            <div class="flex items-center gap-2">
              <ShieldCheckIcon class="h-3.5 w-3.5 text-slate-400" />
              <h3 class="text-[10px] md:text-[11px] font-black uppercase tracking-[0.15em] text-slate-500">Seguridad</h3>
            </div>
            <button
              v-if="!mostrarCambioPassword"
              @click="mostrarCambioPassword = true"
              class="text-[9px] md:text-[10px] font-black uppercase bg-slate-100 dark:bg-slate-500/10 text-slate-600 dark:text-slate-400 border border-slate-300 dark:border-slate-500/20 px-3 py-1.5 rounded-xl hover:bg-slate-700 hover:text-white dark:hover:bg-slate-100 dark:hover:text-slate-950 hover:border-transparent transition-all cursor-pointer"
            >
              Cambiar
            </button>
          </div>

          <div class="bg-slate-50 dark:bg-slate-500/5 rounded-xl border border-slate-200 dark:border-slate-500/15 p-4">
            <div v-if="!mostrarCambioPassword" class="flex items-center gap-2 text-xs md:text-sm text-slate-500 dark:text-slate-400 font-bold px-0.5">
              <ShieldCheckIcon class="h-4 w-4 text-slate-400 dark:text-slate-500" />
              Contraseña protegida
            </div>

            <transition name="expand">
              <div v-if="mostrarCambioPassword" class="space-y-4">

                <!-- Contraseña actual -->
                <div class="flex flex-col gap-1">
                  <label class="text-[9px] md:text-[10px] font-black text-slate-500 dark:text-slate-400 uppercase tracking-wider">Contraseña Actual</label>
                  <div class="relative">
                    <input
                      v-model="passwordForm.actual"
                      :type="verPassword.actual ? 'text' : 'password'"
                      placeholder="••••••••"
                      class="w-full px-4 py-2 bg-white dark:bg-[#0a0f1e] border border-slate-300 dark:border-slate-700 focus:border-indigo-500 dark:focus:border-indigo-400 rounded-xl outline-none transition-all font-bold text-slate-800 dark:text-white text-sm focus:ring-1 focus:ring-indigo-500/20"
                    />
                    <button type="button" @click="verPassword.actual = !verPassword.actual" class="absolute right-3 top-1/2 -translate-y-1/2 text-slate-400 hover:text-indigo-500 dark:hover:text-indigo-400 p-1 transition-colors cursor-pointer">
                      <EyeIcon v-if="!verPassword.actual" class="h-4 w-4" />
                      <EyeSlashIcon v-else class="h-4 w-4" />
                    </button>
                  </div>
                </div>

                <!-- Nueva contraseña -->
                <div class="flex flex-col gap-1">
                  <label class="text-[9px] md:text-[10px] font-black text-slate-500 dark:text-slate-400 uppercase tracking-wider">Nueva Contraseña</label>
                  <div class="relative">
                    <input
                      v-model="passwordForm.nueva"
                      :type="verPassword.nueva ? 'text' : 'password'"
                      placeholder="••••••••"
                      class="w-full px-4 py-2 bg-white dark:bg-[#0a0f1e] border border-slate-300 dark:border-slate-700 focus:border-indigo-500 dark:focus:border-indigo-400 rounded-xl outline-none transition-all font-bold text-slate-800 dark:text-white text-sm focus:ring-1 focus:ring-indigo-500/20"
                    />
                    <button type="button" @click="verPassword.nueva = !verPassword.nueva" class="absolute right-3 top-1/2 -translate-y-1/2 text-slate-400 hover:text-indigo-500 dark:hover:text-indigo-400 p-1 transition-colors cursor-pointer">
                      <EyeIcon v-if="!verPassword.nueva" class="h-4 w-4" />
                      <EyeSlashIcon v-else class="h-4 w-4" />
                    </button>
                  </div>

                  <!-- Validadores -->
                  <div v-if="passwordForm.nueva.length > 0" class="grid grid-cols-2 gap-x-2 gap-y-1 mt-2 text-[10px] font-semibold bg-slate-100 dark:bg-[#0a0f1e] p-2.5 rounded-xl border border-slate-200 dark:border-slate-800">
                    <p :class="/[A-Z]/.test(passwordForm.nueva) ? 'text-emerald-500 dark:text-emerald-400' : 'text-slate-400 dark:text-slate-500'">● Una mayúscula</p>
                    <p :class="/[a-z]/.test(passwordForm.nueva) ? 'text-emerald-500 dark:text-emerald-400' : 'text-slate-400 dark:text-slate-500'">● Una minúscula</p>
                    <p :class="/\d/.test(passwordForm.nueva) ? 'text-emerald-500 dark:text-emerald-400' : 'text-slate-400 dark:text-slate-500'">● Un número</p>
                    <p :class="/[@$!%*?&]/.test(passwordForm.nueva) ? 'text-emerald-500 dark:text-emerald-400' : 'text-slate-400 dark:text-slate-500'">● Un símbolo</p>
                    <p :class="passwordForm.nueva.length >= 6 ? 'text-emerald-500 dark:text-emerald-400' : 'text-slate-400 dark:text-slate-500'" class="col-span-2">● Mínimo 6 caracteres</p>
                  </div>
                </div>

                <!-- Confirmar contraseña -->
                <div class="flex flex-col gap-1">
                  <label class="text-[9px] md:text-[10px] font-black text-slate-500 dark:text-slate-400 uppercase tracking-wider">Confirmar contraseña</label>
                  <div class="relative">
                    <input
                      v-model="passwordForm.confirmar"
                      :type="verPassword.confirmar ? 'text' : 'password'"
                      placeholder="••••••••"
                      class="w-full px-4 py-2 bg-white dark:bg-[#0a0f1e] border border-slate-300 dark:border-slate-700 focus:border-indigo-500 dark:focus:border-indigo-400 rounded-xl outline-none transition-all font-bold text-slate-800 dark:text-white text-sm focus:ring-1 focus:ring-indigo-500/20"
                    />
                    <button type="button" @click="verPassword.confirmar = !verPassword.confirmar" class="absolute right-3 top-1/2 -translate-y-1/2 text-slate-400 hover:text-indigo-500 dark:hover:text-indigo-400 p-1 transition-colors cursor-pointer">
                      <EyeIcon v-if="!verPassword.confirmar" class="h-4 w-4" />
                      <EyeSlashIcon v-else class="h-4 w-4" />
                    </button>
                  </div>
                  <p
                    v-if="passwordForm.confirmar.length > 0"
                    :class="passwordForm.nueva === passwordForm.confirmar ? 'text-emerald-500 dark:text-emerald-400' : 'text-red-500 dark:text-red-400'"
                    class="text-[10px] font-bold mt-1.5"
                  >
                    {{ passwordForm.nueva === passwordForm.confirmar ? '● Las contraseñas coinciden' : '● Las contraseñas no coinciden' }}
                  </p>
                </div>

                <div class="flex gap-2 pt-1">
                  <button
                    @click="cancelarCambioPassword"
                    class="flex-1 py-2.5 text-[10px] font-black uppercase tracking-wider text-slate-500 dark:text-slate-400 rounded-xl hover:bg-slate-100 dark:hover:bg-[#0a0f1e] border border-transparent hover:border-slate-200 dark:hover:border-slate-800 transition-all cursor-pointer"
                  >
                    Cancelar
                  </button>
                  <button
                    @click="cambiarPassword"
                    :disabled="cargandoPassword || !passwordValido"
                    class="flex-[2] bg-slate-100 dark:bg-[#0a0f1e] text-indigo-600 dark:text-indigo-400 border border-indigo-300 dark:border-indigo-500/20 hover:bg-indigo-500 hover:text-white hover:border-transparent py-2.5 rounded-xl font-black text-[10px] uppercase tracking-widest disabled:opacity-20 disabled:pointer-events-none transition-all cursor-pointer"
                  >
                    Confirmar
                  </button>
                </div>

              </div>
            </transition>
          </div>
        </div>

      </div>

      <!-- ── FOOTER ─────────────────────────────────────────── -->
      <footer class="px-5 py-4 border-t border-slate-200 dark:border-indigo-500/15 bg-slate-50 dark:bg-[#0a0f1e] shrink-0">
        <div v-if="editarPerfil" class="flex flex-col xs:flex-row items-center gap-2 md:gap-3">
          <button
            @click="editarPerfil = false"
            :disabled="cargando"
            class="w-full xs:flex-1 text-[10px] font-black uppercase tracking-widest text-slate-500 dark:text-slate-400 py-3 rounded-xl hover:bg-slate-100 dark:hover:bg-[#0f172a] border border-transparent hover:border-slate-200 dark:hover:border-slate-800 transition-all disabled:opacity-40 cursor-pointer"
          >
            Cancelar
          </button>
          <button
            @click="guardarEdicionCambios"
            :disabled="cargando"
            class="w-full xs:flex-[2] bg-indigo-500 text-white hover:bg-indigo-600 dark:hover:bg-indigo-400 py-3.5 rounded-xl font-black text-[10px] uppercase tracking-[0.15em] transition-all flex justify-center items-center disabled:opacity-40 disabled:pointer-events-none cursor-pointer"
          >
            <span v-if="!cargando">Guardar Cambios</span>
            <span v-else class="animate-spin border-2 border-white/30 border-t-white rounded-full h-4 w-4"></span>
          </button>
        </div>

        <button
          v-else
          @click="emit('cerrar')"
          class="w-full bg-indigo-500 hover:bg-indigo-600 dark:hover:bg-indigo-400 text-white font-black text-[10px] md:text-xs uppercase tracking-[0.15em] py-3.5 rounded-xl transition-all shadow-md active:scale-[0.99] cursor-pointer"
        >
          {{ esUsuarioGoogle ? 'Google Account' : 'Cerrar Perfil' }}
        </button>
      </footer>

    </div>
  </div>
</template>
