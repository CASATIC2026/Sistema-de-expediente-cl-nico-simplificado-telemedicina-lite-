import { createRouter, createWebHistory } from 'vue-router'
import pantallaPrincipal from '../views/pantallaPrincipal.vue'
import terminos from '../views/terminos.vue'
import privacidad from '../views/privacidad.vue'
import preguntasFrecuentes from '../views/preguntasFrecuentes.vue'



import dashboardPaciente from '../views/dashboardPaciente.vue'




const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      name: 'pantallaPrincipal',
      component: pantallaPrincipal,
    },
    {
      path: '/terminos',
      name: 'terminos',
      component: terminos,
    },

    {
      path: '/privacidad',
      name: 'privacidad',
      component: privacidad,
    },
    {
      path: '/preguntasFrecuentes',
      name: 'preguntasFrecuentes',
      component: preguntasFrecuentes,
    },





    { path: '/dashboardPaciente', 
      name: 'dashboardPaciente',
      component: dashboardPaciente 
    },
    
    
    
    
  ],
})

export default router
