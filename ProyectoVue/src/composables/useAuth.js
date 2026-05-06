import { useRouter } from 'vue-router'

export const useAuth = () => {
  const router = useRouter()

  const logout = () => {
    localStorage.clear()
    router.replace('/')
  }

  const getToken = () => localStorage.getItem('token')
  const getRol = () => localStorage.getItem('rol')

  const isAuthenticated = () => !!getToken()

  return {
    logout,
    getToken,
    getRol,
    isAuthenticated
  }
}
