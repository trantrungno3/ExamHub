import type {ReactNode} from 'react'
import {createContext, useCallback, useContext, useEffect, useState} from 'react'
import {globalConfig} from './configs/common'
import {AuthHttp} from "./services/requestService.ts";
import {authService} from "./services/authService.ts";

interface AuthContextValue {
    token: TokenModel | null
    user: UserInfo | null
    isAuthenticated: boolean
    login: (userName: string, password: string, isRemember?: boolean) => Promise<string | null>
    logout: () => void
}

const TOKEN_KEY = globalConfig.storageKey.token;

const AuthContext = createContext<AuthContextValue | null>(null)

export function AuthProvider({children}: Readonly<{ children: ReactNode }>) {
    const [token, setToken] = useState<TokenModel | null>(() => {
        try {
            const stored = localStorage.getItem(TOKEN_KEY)
            return stored ? JSON.parse(stored) : null
        } catch {
            return null
        }
    })
    const [user, setUser] = useState<UserInfo | null>(null)
    useEffect(() => {
        if (!token?.accessToken) return;
        AuthHttp.get<UserInfo>('/Auth/info')
            .then((res) => {
                if (res.isSuccess && res.data) setUser(res.data)
            })
            .catch(() => {
            })
    }, [token])

    const login = useCallback(async (userName: string, password: string, isRemember = false): Promise<string | null> => {
        try {
            const data = await authService.login({userName, password, isRemember});
            if (data.isSuccess || !data.data) return data.message ?? 'Đăng nhập thất bại!';
            localStorage.setItem(TOKEN_KEY, JSON.stringify(data.data))
            setToken(data.data as TokenModel)
            return null
        } catch {
            return 'Không thể kết nối đến máy chủ!'
        }
    }, [])

    const logout = useCallback(() => {
        localStorage.removeItem(TOKEN_KEY)
        setToken(null)
        setUser(null)
    }, [])

    return (
        <AuthContext.Provider value={{token, user, isAuthenticated: !!token, login, logout}}>
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
