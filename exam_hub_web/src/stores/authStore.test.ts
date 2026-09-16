import {afterEach, beforeEach, describe, expect, it, vi} from 'vitest'
import {authService} from '../services/authService'
import type {ApiResponse} from '../services/requestService'
import {useAuthStore} from './authStore'

vi.mock('../services/authService', () => ({
    authService: {
        login: vi.fn(),
        refresh: vi.fn(),
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

describe('authStore.refresh', () => {
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

    it('sends the current pair and stores the rotated pair', async () => {
        const oldPair = {
            accessToken: token(1_900_000_000),
            refreshToken: token(2_000_000_000),
        }
        const newPair = {
            accessToken: token(2_100_000_000),
            refreshToken: token(2_200_000_000),
        }
        useAuthStore.getState().setTokens(oldPair)
        vi.mocked(authService.refresh).mockResolvedValue({
            status: 1,
            message: 'ok',
            data: newPair,
        })

        await expect(useAuthStore.getState().refresh()).resolves.toBe(true)

        expect(authService.refresh).toHaveBeenCalledWith(oldPair)
        expect(useAuthStore.getState().token).toEqual({
            ...newPair,
            expiresAt: 2_100_000_000_000,
            refreshExpiresAt: 2_200_000_000_000,
        })
    })

    it('shares one refresh request across concurrent callers', async () => {
        const oldPair = {
            accessToken: token(1_900_000_000),
            refreshToken: token(2_000_000_000),
        }
        const newPair = {
            accessToken: token(2_100_000_000),
            refreshToken: token(2_200_000_000),
        }
        useAuthStore.getState().setTokens(oldPair)
        const pending = deferred<ApiResponse<TokenPair>>()
        vi.mocked(authService.refresh).mockReturnValue(pending.promise)

        const first = useAuthStore.getState().refresh()
        const second = useAuthStore.getState().refresh()
        pending.resolve({status: 1, message: 'ok', data: newPair})

        await expect(Promise.all([first, second])).resolves.toEqual([true, true])
        expect(authService.refresh).toHaveBeenCalledOnce()
        expect(useAuthStore.getState().isRefreshing).toBe(false)
    })
})
