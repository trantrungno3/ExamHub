import {afterEach, beforeEach, describe, expect, it, vi} from 'vitest'
import {authService} from '../services/authService'
import type {ApiResponse} from '../services/requestService'
import {useAuthStore} from './authStore'

vi.mock('../services/authService', () => ({
    authService: {
        login: vi.fn(),
        refresh: vi.fn(),
        logout: vi.fn(),
    },
}))

function token(exp: number, userName = 'teacher1'): string {
    const payload = btoa(JSON.stringify({exp, UserName: userName, Role: 'Teacher'}))
        .replaceAll('+', '-')
        .replaceAll('/', '_')
        .replaceAll('=', '')
    return `header.${payload}.signature`
}

function deferred<T>() {
    let resolve!: (value: T) => void
    const promise = new Promise<T>((done) => {
        resolve = done
    })
    return {promise, resolve}
}

beforeEach(() => {
    localStorage.clear()
    useAuthStore.setState({
        token: null,
        user: null,
        isAuthenticated: false,
        isRefreshing: false,
    })
    vi.clearAllMocks()
})

afterEach(() => {
    vi.restoreAllMocks()
})

describe('authStore.refresh', () => {
    it('sends the current access token and stores the rotated one', async () => {
        const oldAccess = token(1_900_000_000)
        useAuthStore.getState().setTokens({accessToken: oldAccess})
        vi.mocked(authService.refresh).mockResolvedValue({
            status: 1,
            message: 'ok',
            data: {accessToken: token(2_100_000_000)},
        })

        await expect(useAuthStore.getState().refresh()).resolves.toBe(true)

        expect(authService.refresh).toHaveBeenCalledWith(oldAccess)
        expect(useAuthStore.getState().token).toEqual({
            accessToken: token(2_100_000_000),
            expiresAt: 2_100_000_000_000,
        })
    })

    it('shares one refresh request across concurrent callers', async () => {
        useAuthStore.getState().setTokens({accessToken: token(1_900_000_000)})
        const pending = deferred<ApiResponse<AccessTokenResponse>>()
        vi.mocked(authService.refresh).mockReturnValue(pending.promise)

        const first = useAuthStore.getState().refresh()
        const second = useAuthStore.getState().refresh()
        pending.resolve({status: 1, message: 'ok', data: {accessToken: token(2_100_000_000)}})

        await expect(Promise.all([first, second])).resolves.toEqual([true, true])
        expect(authService.refresh).toHaveBeenCalledOnce()
        expect(useAuthStore.getState().isRefreshing).toBe(false)
    })

    it('clears the session locally when refresh fails, without calling logout endpoint', async () => {
        useAuthStore.getState().setTokens({accessToken: token(1_900_000_000)})
        vi.mocked(authService.refresh).mockResolvedValue({status: 0, message: 'hết hạn'})

        await expect(useAuthStore.getState().refresh()).resolves.toBe(false)

        expect(useAuthStore.getState().isAuthenticated).toBe(false)
        expect(useAuthStore.getState().token).toBeNull()
        // Gọi /Auth/logout ở đây sẽ tự kích hoạt lại refresh qua handler 401 → vòng lặp.
        expect(authService.logout).not.toHaveBeenCalled()
    })
})

describe('authStore persistence and logout', () => {
    it('never holds a refresh token in state, so nothing can persist one', () => {
        useAuthStore.getState().setTokens({accessToken: token(1_900_000_000)})

        // partialize persist nguyên `token`, nên shape của nó chính là thứ xuống localStorage.
        expect(Object.keys(useAuthStore.getState().token!).toSorted())
            .toEqual(['accessToken', 'expiresAt'])
    })

    it('revokes on the server then clears local state', async () => {
        useAuthStore.getState().setTokens({accessToken: token(1_900_000_000)})
        vi.mocked(authService.logout).mockResolvedValue({status: 1, message: 'ok', data: true})

        await useAuthStore.getState().logout()

        expect(authService.logout).toHaveBeenCalledOnce()
        expect(useAuthStore.getState().token).toBeNull()
        expect(useAuthStore.getState().isAuthenticated).toBe(false)
    })

    it('still clears local state when the server logout fails', async () => {
        useAuthStore.getState().setTokens({accessToken: token(1_900_000_000)})
        vi.mocked(authService.logout).mockRejectedValue(new Error('network down'))

        await useAuthStore.getState().logout()

        expect(useAuthStore.getState().token).toBeNull()
        expect(useAuthStore.getState().isAuthenticated).toBe(false)
    })
})
