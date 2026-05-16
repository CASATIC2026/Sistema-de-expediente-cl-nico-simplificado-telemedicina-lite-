import { createRouter, createWebHistory } from 'vue-router'
import { logoutPro } from '@/services/api'
import { useVideoStore } from '@/stores/videoStore'


import PantallaPrincipal   from '@/views/pantallaPrincipal.vue'
import Terminos            from '@/views/terminos.vue'
import Privacidad          from '@/views/privacidad.vue'
import PreguntasFrecuentes from '@/views/preguntasFrecuentes.vue'
import acercaDeNosotros from '@/views/acercaDeNosotros.vue'
import ChangePassword      from '@/viewsDoctor/ChangePassword.vue'
import CompletarPerfil     from '@/views/completarPerfil.vue'
import ResetPassword       from '@/views/ResetPassword.vue'


const DashboardLayout   = () => import('@/componentsDoctor/ui/layout/DashboardLayout.vue')
const MedDashboard      = () => import('@/viewsDoctor/dashboards/MedDashboard.vue')
const PacienteDashboard = () => import('@/views/dashboardPaciente.vue')
const AdminDashboard    = () => import('@/views/dashboardAdmin.vue')

// Rutas internas
import Agenda     from '@/viewsDoctor/Agenda.vue'
import Pacientes  from '@/viewsDoctor/Pacientes.vue'
import Consultas  from '@/viewsDoctor/Consultas.vue'
import Historial  from '@/viewsDoctor/Historial.vue'
import SalaTelmed from '@/viewsDoctor/SalaTelmed.vue'

// ===============================
// UTILIDADES
// ===============================
const getRolFromToken = () => {
  const token = localStorage.getItem('token')
  if (!token) return null
  try {
    const payload = JSON.parse(atob(token.split('.')[1]))
    const rol = (
      payload.role ||
      payload.rol ||
      payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] ||
      ''
    ).toLowerCase()
    return rol || null
  } catch (error) {
    console.error('Error extrayendo rol del token:', error)
    return null
  }
}

const tokenExpirado = () => {
  const token = localStorage.getItem('token')
  if (!token) return true
  try {
    const payload = JSON.parse(atob(token.split('.')[1]))
    const expirado = payload.exp * 1000 < Date.now()
    if (expirado) {
      console.warn('Token expirado')
    }
    return expirado
  } catch (error) {
    console.error('Error validando expiración del token:', error)
    return true
  }
}

const redireccionPorRol = (rol) => {
  const dashboards = {
    doctor:   '/app/MedDashboard',
    paciente: '/app/dashboardPaciente',
    admin:    '/app/dashboardAdmin'
  }
  const destino = dashboards[rol] || '/'
  return destino
}

const tienePermisoParaRuta = (rol, rutasPermitidas) => {
  if (!rutasPermitidas || rutasPermitidas.length === 0) return true
  if (!rol) return false
  return rutasPermitidas.includes(rol)
}

// ===============================
// RUTAS
// ===============================
const routes = [
  { path: '/', name: 'pantallaPrincipal', component: PantallaPrincipal },
  { path: '/terminos', component: Terminos },
  { path: '/privacidad', component: Privacidad },
  { path: '/preguntas-frecuentes', component: PreguntasFrecuentes },
  {path: '/acerca-de-nosotros', component: acercaDeNosotros },
  { path: '/reset-password', name: 'resetPassword', component: ResetPassword },

  {
    path: '/change-password',
    component: ChangePassword,
    meta: { requiresAuth: true }
  },
  {
    path: '/completar-perfil',
    name: 'completarPerfil',
    component: CompletarPerfil,
    meta: { requiresAuth: true }
  },

  {
    path: '/app',
    component: DashboardLayout,
    redirect: () => redireccionPorRol(getRolFromToken()),
    meta: { requiresAuth: true },
    children: [
      { path: 'MedDashboard',      name: 'MedDashboard',      component: MedDashboard,      meta: { roles: ['doctor'] } },
      { path: 'dashboardPaciente', name: 'dashboardPaciente', component: PacienteDashboard, meta: { roles: ['paciente'] } },
      { path: 'dashboardAdmin',    name: 'dashboardAdmin',    component: AdminDashboard,    meta: { roles: ['admin'] } },
      { path: 'Agenda',            name: 'Agenda',            component: Agenda,            meta: { roles: ['doctor'] } },
      { path: 'Pacientes',         name: 'Pacientes',         component: Pacientes,         meta: { roles: ['doctor', 'admin'] } },
      { path: 'Consultas',         name: 'Consultas',         component: Consultas,         meta: { roles: ['doctor', 'admin'] } },
      { path: 'Consultas/:id',     name: 'Consulta',          component: Consultas, props: true, meta: { roles: ['doctor', 'admin'] } },
      { path: 'Historial',         name: 'Historial',         component: Historial,         meta: { roles: ['doctor', 'admin'] } },
      { path: 'SalaTelmed',        name: 'SalaTelmed',        component: SalaTelmed,        meta: { roles: ['doctor', 'admin', 'paciente'] } }
    ]
  },
  {
    path: '/verify-email',
    component: () => import('@/views/VerifyEmail.vue')
  },
  {
    path: '/verify-email-notice',
    component: () => import('@/views/VerifyEmailNotice.vue')
  }
]

// Crear router PRIMERO
const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes
})


// GUARD GLOBAL - SEGURIDAD DE RUTAS
router.beforeEach((to, from, next) => {
  const videoStore     = useVideoStore()
  const token          = localStorage.getItem('token')
  const rol            = getRolFromToken()
  const perfilCompleto = localStorage.getItem('perfilCompleto') === 'true'
  const requiereCambio = localStorage.getItem('requiereCambioPassword') === 'true'

  // 1. BLOQUEO POR VIDEOLLAMADA ACTIVA
  if (videoStore.isActive) {
    if (
      to.name === 'Consulta' &&
      String(to.params.id) === String(videoStore.citaId)
    ) {
      return next()
    }
    // Bloquear cualquier otra navegación durante videollamada
    return next(false)
  }

  // 2. VALIDAR TOKEN EXPIRADO
  if (token && tokenExpirado()) {
    logoutPro()
    return next('/')
  }

  // 3. USUARIO YA LOGUEADO INTENTA ACCEDER AL LANDING
  if (to.path === '/' && token && rol) {
    return next(redireccionPorRol(rol))
  }

  // 4. RUTA PÚBLICA - SIN AUTENTICACIÓN REQUERIDA
  if (!to.meta.requiresAuth) {
    return next()
  }

  // 5. RUTA REQUIERE AUTENTICACIÓN PERO NO HAY TOKEN
  if (!token) {
    return next('/')
  }

  // 6. SIN ROL - LOGOUT POR SEGURIDAD
  if (!rol) {
    logoutPro()
    return next('/')
  }

  // 7. REQUIERE CAMBIO DE CONTRASEÑA - PRIORIDAD
  if (requiereCambio && to.path !== '/change-password') {
    return next('/change-password')
  }

  // 8. COMPLETAR PERFIL - PRIORIDAD (excepto cambio de contraseña)
  if (!perfilCompleto && to.name !== 'completarPerfil' && to.path !== '/change-password') {
    return next('/completar-perfil')
  }

  // 9. VALIDAR ROLES - ES LA VALIDACIÓN PRINCIPAL
  if (to.meta.roles && to.meta.roles.length > 0) {
    const tienePermiso = tienePermisoParaRuta(rol, to.meta.roles)
    
    if (!tienePermiso) {
      console.warn(`Acceso denegado: rol '${rol}' no autorizado para ruta '${to.path}'`)
      // Redirigir al dashboard del rol sin permitir acceso
      return next(redireccionPorRol(rol))
    }
  }

  // 10. VALIDACIÓN EXITOSA
  return next()
})


export default router
