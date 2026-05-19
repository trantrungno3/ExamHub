/**
 * @deprecated Shim for backward compatibility — logic moved to authStore.
 * useAuth() reads from useAuthStore; existing callers keep working unchanged.
 */
import type { ReactNode } from 'react'
import { useAuthStore } from './stores/authStore'

export function AuthProvider({ children }: Readonly<{ children: ReactNode }>) {
    return <>{children}</>
}

export function useAuth() {
    const { token, user, isAuthenticated, login, logout, refresh } = useAuthStore()
    return { token, user, isAuthenticated, login, logout, refresh }
}
