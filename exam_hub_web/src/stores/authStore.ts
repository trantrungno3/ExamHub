import { create } from 'zustand'
import { persist } from 'zustand/middleware'
import { extractUserFromToken, getTokenExpiresAt, isTokenExpired } from '../utils/jwt'
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
                // isRefreshing chặn nhiều request 401 cùng lúc gọi refresh() song song (race) —
                // request đến sau thấy cờ đang bật thì bỏ qua thay vì gọi refresh-token 2 lần.
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
            // Chỉ persist token/user — isAuthenticated và isRefreshing là state suy ra được (không phải
            // nguồn sự thật), lưu xuống localStorage dễ lệch với token thật khi token hết hạn giữa 2 lần mở app.
            partialize: (state) => ({ token: state.token, user: state.user }),
            // Sau khi rehydrate từ localStorage, suy lại isAuthenticated/user trực tiếp từ token đã lưu
            // (thay vì tin state cũ) để tránh hiển thị "đã đăng nhập" với token đã hết hạn/bị sửa tay.
            // Phải check hết hạn NGAY ĐÂY (đồng bộ, trước render đầu) — nếu chỉ set isAuthenticated=true
            // rồi để AppLayout tự phát hiện hết hạn sau trong useEffect thì ProtectedRoute/StudentLayout/
            // LoginPage đều đã kịp điều hướng theo cờ sai, gây nhảy qua lại login/trang cũ vài nhịp.
            onRehydrateStorage: () => (state) => {
                if (state?.token && !isTokenExpired(state.token.refreshExpiresAt)) {
                    state.isAuthenticated = true
                    state.user = extractUserFromToken(state.token.accessToken)
                } else if (state) {
                    state.token = null
                    state.user = null
                    state.isAuthenticated = false
                }
            },
        }
    )
)
