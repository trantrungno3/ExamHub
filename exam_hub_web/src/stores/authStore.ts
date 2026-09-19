import { create } from 'zustand'
import { persist } from 'zustand/middleware'
import { extractUserFromToken, getTokenExpiresAt } from '../utils/jwt'
import { authService } from '../services/authService'
import { setUnauthorizedHandler, statusCode } from '../services/requestService'

interface AuthState {
    token: TokenModel | null
    user: UserInfo | null
    isAuthenticated: boolean
    isRefreshing: boolean
}

interface AuthActions {
    login: (userName: string, password: string, isRemember?: boolean) => Promise<string | null>
    /** Revoke phía server (best-effort) rồi xoá state local. */
    logout: () => Promise<void>
    /** Xoá state local, KHÔNG gọi server — dùng khi refresh đã thất bại. */
    clearSession: () => void
    setTokens: (raw: AccessTokenResponse) => void
    refresh: () => Promise<boolean>
}

export type AuthStore = AuthState & AuthActions

let refreshPromise: Promise<boolean> | null = null

export const useAuthStore = create<AuthStore>()(
    persist(
        (set, get) => ({
            token: null,
            user: null,
            isAuthenticated: false,
            isRefreshing: false,

            setTokens(raw) {
                const token: TokenModel = { accessToken: raw.accessToken, expiresAt: getTokenExpiresAt(raw.accessToken) }
                const user = extractUserFromToken(raw.accessToken)
                set({ token, user, isAuthenticated: true })
            },

            async login(userName, password, isRemember = false) {
                try {
                    const res = await authService.login({ userName, password, isRemember })
                    if (res.status === statusCode.Error || !res.data) return res.message ?? 'Đăng nhập thất bại!'
                    get().setTokens(res.data)
                    return null
                } catch {
                    return 'Không thể kết nối đến máy chủ!'
                }
            },

            clearSession() {
                set({ token: null, user: null, isAuthenticated: false })
            },

            async logout() {
                // Best-effort: server revoke refresh token và xoá cookie. Dù lỗi mạng vẫn phải
                // xoá state local, nếu không user thấy mình còn đăng nhập.
                try {
                    await authService.logout()
                } catch {
                    // bỏ qua — vẫn clear ở finally
                } finally {
                    get().clearSession()
                }
            },

            refresh() {
                if (refreshPromise) return refreshPromise

                const { token } = get()
                if (!token?.accessToken) return Promise.resolve(false)

                const currentAccessToken = token.accessToken
                set({ isRefreshing: true })

                // ponytail: single-flight qua module-level promise — chặn nhiều request 401 song song
                // gọi refresh-token nhiều lần; caller đến sau nhận chung promise thay vì bị bỏ qua.
                refreshPromise = (async () => {
                    try {
                        const res = await authService.refresh(currentAccessToken)
                        if (res.status === statusCode.Error || !res.data) {
                            // clearSession, KHÔNG logout: gọi /Auth/logout ở đây sẽ nhận 401 và
                            // kích hoạt lại refresh qua unauthorized handler → vòng lặp.
                            get().clearSession()
                            return false
                        }
                        get().setTokens(res.data)
                        return true
                    } catch {
                        get().clearSession()
                        return false
                    } finally {
                        set({ isRefreshing: false })
                        refreshPromise = null
                    }
                })()

                return refreshPromise
            },
        }),
        {
            name: 'examhub_auth',
            // Chỉ persist token/user — isAuthenticated và isRefreshing là state suy ra được (không phải
            // nguồn sự thật), lưu xuống localStorage dễ lệch với token thật khi token hết hạn giữa 2 lần mở app.
            partialize: (state) => ({ token: state.token, user: state.user }),
            // Sau khi rehydrate, suy lại isAuthenticated/user trực tiếp từ token đã lưu thay vì tin
            // state cũ. Không còn refresh expiry ở client để đối chiếu — cookie HttpOnly là nguồn sự
            // thật duy nhất và JS không đọc được nó — nên có access token đã lưu là coi như còn
            // phiên; AppLayout refresh ngay khi access token hết hạn và clearSession nếu thất bại.
            // Đổi lại: access token hết hạn sẽ hiện app shell một nhịp trước khi bị đẩy về login,
            // đây là hệ quả tất yếu của việc client không biết cookie còn sống hay không.
            onRehydrateStorage: () => (state) => {
                if (state?.token?.accessToken) {
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

setUnauthorizedHandler(() => useAuthStore.getState().refresh())
