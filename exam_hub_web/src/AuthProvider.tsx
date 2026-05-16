import type {ReactNode} from 'react'
import {createContext, useCallback, useContext, useLayoutEffect, useMemo, useRef, useState} from 'react'
import {globalConfig} from './configs/common'
import {authService} from './services/authService.ts'
import {extractUserFromToken, getTokenExpiresAt} from './utils/jwt.ts'

interface AuthContextValue {
    token: TokenModel | null
    user: UserInfo | null
    isAuthenticated: boolean
    login: (userName: string, password: string, isRemember?: boolean) => Promise<string | null>
    logout: () => void
    refresh: () => Promise<boolean>
}

const TOKEN_KEY = globalConfig.storageKey.token

const AuthContext = createContext<AuthContextValue | null>(null)

function readStoredToken(): TokenModel | null {
    try {
        const stored = localStorage.getItem(TOKEN_KEY)
        if (!stored) return null
        const parsed = JSON.parse(stored) as Partial<TokenModel>
        if (!parsed.accessToken || !parsed.refreshToken) return null
        const expiresAt = parsed.expiresAt ?? getTokenExpiresAt(parsed.accessToken)
        const refreshExpiresAt = parsed.refreshExpiresAt ?? getTokenExpiresAt(parsed.refreshToken)
        return {accessToken: parsed.accessToken, refreshToken: parsed.refreshToken, expiresAt, refreshExpiresAt}
    } catch {
        return null
    }
}

export function AuthProvider({children}: Readonly<{children: ReactNode}>) {
    const [token, setToken] = useState<TokenModel | null>(readStoredToken)
    const [user, setUser] = useState<UserInfo | null>(() => {
        const t = readStoredToken()
        return t ? extractUserFromToken(t.accessToken) : null
    })
    const isRefreshingRef = useRef(false)
    const tokenRef = useRef(token)
    useLayoutEffect(() => { tokenRef.current = token }, [token])

    const saveToken = useCallback((t: TokenModel) => {
        localStorage.setItem(TOKEN_KEY, JSON.stringify(t))
        setToken(t)
        setUser(extractUserFromToken(t.accessToken))
    }, [])

    const logout = useCallback(() => {
        localStorage.removeItem(TOKEN_KEY)
        setToken(null)
        setUser(null)
    }, [])

    const login = useCallback(async (userName: string, password: string, isRemember = false): Promise<string | null> => {
        try {
            const res = await authService.login({userName, password, isRemember})
            if (!res.isSuccess || !res.data) return res.message ?? 'Đăng nhập thất bại!'
            const raw = res.data as {accessToken: string; refreshToken: string}
            const expiresAt = getTokenExpiresAt(raw.accessToken)
            const refreshExpiresAt = getTokenExpiresAt(raw.refreshToken)
            saveToken({...raw, expiresAt, refreshExpiresAt})
            return null
        } catch {
            return 'Không thể kết nối đến máy chủ!'
        }
    }, [saveToken])

    const refresh = useCallback(async (): Promise<boolean> => {
        if (isRefreshingRef.current) return false
        const current = tokenRef.current
        if (!current?.refreshToken) return false
        isRefreshingRef.current = true
        try {
            const res = await authService.refresh(current.refreshToken)
            if (!res.isSuccess || !res.data) {
                logout()
                return false
            }
            const raw = res.data as {accessToken: string; refreshToken: string}
            const expiresAt = getTokenExpiresAt(raw.accessToken)
            const refreshExpiresAt = getTokenExpiresAt(raw.refreshToken)
            saveToken({...raw, expiresAt, refreshExpiresAt})
            return true
        } catch {
            logout()
            return false
        } finally {
            isRefreshingRef.current = false
        }
    }, [saveToken, logout])

    const ctxValue = useMemo(
        () => ({token, user, isAuthenticated: !!token, login, logout, refresh}),
        [token, user, login, logout, refresh]
    )

    return (
        <AuthContext.Provider value={ctxValue}>
            {children}
        </AuthContext.Provider>
    )
}

// eslint-disable-next-line react-refresh/only-export-components
export function useAuth(): AuthContextValue {
    const ctx = useContext(AuthContext)
    if (!ctx) throw new Error('useAuth must be used within AuthProvider')
    return ctx
}
