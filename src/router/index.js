import { createRouter, createWebHistory } from 'vue-router'
import PantallaPrincipal from '../views/pantallaPrincipal.vue'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      name: 'pantallaPrincipal',
      component: PantallaPrincipal,
    },
    
  ],
})

export default router
