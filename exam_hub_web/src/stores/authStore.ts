import { create } from 'zustand'
import { persist } from 'zustand/middleware'
import { extractUserFromToken, getTokenExpiresAt } from '../utils/jwt'
import { authService } from '../services/authService'
import { statusCode } from '../services/requestService'

interface AuthState {
    token: TokenModel | null
    user: UserInfo | null
    isAuthenticated: boolean
    isRefreshing: boolean
}

interface AuthActions {
    login: (userName: string, password: string, isRemember?: boolean) => Promise<string | null>
    logout: () => void
    setTokens: (raw: { accessToken: string; refreshToken: string }) => void
    refresh: () => Promise<boolean>
}

export type AuthStore = AuthState & AuthActions

export const useAuthStore = create<AuthStore>()(
    persist(
        (set, get) => ({
            token: null,
            user: null,
            isAuthenticated: false,
            isRefreshing: false,

            setTokens(raw) {
                const expiresAt = getTokenExpiresAt(raw.accessToken)
                const refreshExpiresAt = getTokenExpiresAt(raw.refreshToken)
                const token: TokenModel = { ...raw, expiresAt, refreshExpiresAt }
                const user = extractUserFromToken(raw.accessToken)
                set({ token, user, isAuthenticated: true })
            },

            async login(userName, password, isRemember = false) {
                try {
                    const res = await authService.login({ userName, password, isRemember })
                    if (res.status === statusCode.Error || !res.data) return res.message ?? 'Đăng nhập thất bại!'
                    get().setTokens(res.data as { accessToken: string; refreshToken: string })
                    return null
                } catch {
                    return 'Không thể kết nối đến máy chủ!'
                }
            },

            logout() {
                set({ token: null, user: null, isAuthenticated: false })
            },

            async refresh() {
                const { token, isRefreshing } = get()
                if (isRefreshing || !token?.refreshToken) return false
                set({ isRefreshing: true })
                try {
                    const res = await authService.refresh(token.refreshToken)
                    if (res.status === statusCode.Error || !res.data) {
                        get().logout()
                        return false
                    }
                    get().setTokens(res.data as { accessToken: string; refreshToken: string })
                    return true
                } catch {
                    get().logout()
                    return false
                } finally {
                    set({ isRefreshing: false })
                }
            },
        }),
        {
            name: 'examhub_auth',
            partialize: (state) => ({ token: state.token, user: state.user }),
            onRehydrateStorage: () => (state) => {
                if (state?.token) state.isAuthenticated = true
            },
        }
    )
)
