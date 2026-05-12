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
  if (!token) return ''
  try {
    const payload = JSON.parse(atob(token.split('.')[1]))
    return (
      payload.role ||
      payload.rol ||
      payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] ||
      ''
    ).toLowerCase()
  } catch {
    return ''
  }
}

const tokenExpirado = () => {
  const token = localStorage.getItem('token')
  if (!token) return true
  try {
    const payload = JSON.parse(atob(token.split('.')[1]))
    return payload.exp * 1000 < Date.now()
  } catch {
    return true
  }
}

const redireccionPorRol = (rol) => {
  const dashboards = {
    doctor:   '/app/MedDashboard',
    paciente: '/app/dashboardPaciente',
    admin:    '/app/dashboardAdmin'
  }
  return dashboards[rol] || '/'
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
      { path: 'Consultas/:id',     name: 'Consulta',          component: Consultas, props: true, meta: { roles: ['doctor'] } },
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


// GUARD GLOBAL

router.beforeEach((to) => {
  const videoStore     = useVideoStore()
  const token          = localStorage.getItem('token')
  const rol            = getRolFromToken()
  const perfilCompleto = localStorage.getItem('perfilCompleto') === 'true'
  const requiereCambio = localStorage.getItem('requiereCambioPassword') === 'true'

  // BLOQUEO POR VIDEOLLAMADA
 if (videoStore.isActive) {
  // permitir solo la misma consulta activa
  if (
    to.name === 'Consulta' &&
    String(to.params.id) === String(videoStore.citaId)
  ) {
    return true
  }

  return false
}

  // 1. Token expirado
  if (token && tokenExpirado()) {
    logoutPro()
    return '/'
  }

  // 2. Ya logueado → evitar landing
  if (to.path === '/' && token) {
    return redireccionPorRol(rol)
  }

  // 3. Pública
  if (!to.meta.requiresAuth) return true

  // 4. Sin token
  if (!token) return '/'

  // 5. Cambio contraseña
  if (requiereCambio && to.path !== '/change-password') {
    return '/change-password'
  }

  // 6. Completar perfil
  if (!perfilCompleto && to.name !== 'completarPerfil' && to.path !== '/change-password') {
    return '/completar-perfil'
  }

  // 7. Rol
  if (to.meta.roles && !to.meta.roles.includes(rol)) {
    return redireccionPorRol(rol)
  }

  return true
})

export default router
