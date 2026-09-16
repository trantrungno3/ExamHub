import {afterEach, beforeEach, expect, it, vi} from 'vitest'
import {globalConfig} from '../configs/common'
import {authService} from './authService'

beforeEach(() => {
    globalConfig.apiBaseUrl = 'https://api.test'
})

afterEach(() => {
    vi.unstubAllGlobals()
})

it('posts both tokens to the refresh-token endpoint', async () => {
    const fetchMock = vi.fn().mockResolvedValue(new Response(JSON.stringify({
        status: 1,
        message: 'ok',
        data: {
            accessToken: 'new-access',
            refreshToken: 'new-refresh',
        },
    }), {
        status: 200,
        headers: {'Content-Type': 'application/json'},
    }))
    vi.stubGlobal('fetch', fetchMock)

    const result = await authService.refresh({
        accessToken: 'old-access',
        refreshToken: 'old-refresh',
    })

    expect(fetchMock).toHaveBeenCalledOnce()
    expect(fetchMock).toHaveBeenCalledWith(
        'https://api.test/api/Auth/refresh-token',
        expect.objectContaining({
            method: 'POST',
            body: JSON.stringify({
                accessToken: 'old-access',
                refreshToken: 'old-refresh',
            }),
        }),
    )
    expect(result.data).toEqual({
        accessToken: 'new-access',
        refreshToken: 'new-refresh',
    })
})
