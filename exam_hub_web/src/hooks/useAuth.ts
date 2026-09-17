import { useAuthStore } from '../stores/authStore'

export function useAuth() {
    const token = useAuthStore(s => s.token)
    const user = useAuthStore(s => s.user)
    const isAuthenticated = useAuthStore(s => s.isAuthenticated)
    const login = useAuthStore(s => s.login)
    const logout = useAuthStore(s => s.logout)
    const refresh = useAuthStore(s => s.refresh)
    return { token, user, isAuthenticated, login, logout, refresh }
}
