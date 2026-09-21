import {afterEach, beforeEach, expect, it, vi} from 'vitest'
import {globalConfig} from '../configs/common'
import {authService} from './authService'

beforeEach(() => {
    globalConfig.apiBaseUrl = 'https://api.test'
})

afterEach(() => {
    vi.unstubAllGlobals()
})

function okResponse(data: unknown) {
    return new Response(JSON.stringify({status: 1, message: 'ok', data}), {
        status: 200,
        headers: {'Content-Type': 'application/json'},
    })
}

it('posts only the access token to refresh-token and sends the cookie', async () => {
    const fetchMock = vi.fn().mockResolvedValue(okResponse({accessToken: 'new-access'}))
    vi.stubGlobal('fetch', fetchMock)

    const result = await authService.refresh('old-access')

    expect(fetchMock).toHaveBeenCalledOnce()
    expect(fetchMock).toHaveBeenCalledWith(
        'https://api.test/api/Auth/refresh-token',
        expect.objectContaining({
            method: 'POST',
            // Refresh token nằm trong cookie HttpOnly nên body chỉ còn access token.
            body: JSON.stringify({accessToken: 'old-access'}),
            credentials: 'include',
        }),
    )
    expect(result.data).toEqual({accessToken: 'new-access'})
})

it('logs out through the server so the refresh cookie is revoked and cleared', async () => {
    const fetchMock = vi.fn().mockResolvedValue(okResponse(true))
    vi.stubGlobal('fetch', fetchMock)

    await authService.logout()

    expect(fetchMock).toHaveBeenCalledWith(
        'https://api.test/api/Auth/logout',
        expect.objectContaining({method: 'POST', credentials: 'include'}),
    )
})
