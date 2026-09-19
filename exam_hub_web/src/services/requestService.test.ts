import {afterEach, beforeEach, expect, it, vi} from 'vitest'
import {globalConfig} from '../configs/common'
import {AuthHttp, setUnauthorizedHandler} from './requestService'

function storeAccessToken(accessToken: string) {
    localStorage.setItem('examhub_auth', JSON.stringify({
        state: {token: {accessToken}},
        version: 0,
    }))
}

function jsonResponse(status: number, body: unknown) {
    return new Response(JSON.stringify(body), {
        status,
        headers: {'Content-Type': 'application/json'},
    })
}

beforeEach(() => {
    globalConfig.apiBaseUrl = 'https://api.test'
    localStorage.clear()
    setUnauthorizedHandler(null)
})

afterEach(() => {
    setUnauthorizedHandler(null)
    vi.unstubAllGlobals()
})

it('refreshes and retries an authenticated request once after 401', async () => {
    storeAccessToken('old-access')
    const fetchMock = vi.fn()
        .mockResolvedValueOnce(jsonResponse(401, {status: 0, message: 'expired'}))
        .mockResolvedValueOnce(jsonResponse(200, {status: 1, message: 'ok', data: ['menu']}))
    vi.stubGlobal('fetch', fetchMock)
    const refresh = vi.fn().mockImplementation(async () => {
        storeAccessToken('new-access')
        return true
    })
    setUnauthorizedHandler(refresh)

    const result = await AuthHttp.get<string[]>('/menu')

    expect(result.data).toEqual(['menu'])
    expect(refresh).toHaveBeenCalledOnce()
    expect(fetchMock).toHaveBeenCalledTimes(2)
    const firstHeaders = fetchMock.mock.calls[0][1]?.headers as Headers
    const secondHeaders = fetchMock.mock.calls[1][1]?.headers as Headers
    expect(firstHeaders.get('Authorization')).toBe('Bearer old-access')
    expect(secondHeaders.get('Authorization')).toBe('Bearer new-access')
})

it('sends the refresh cookie on every request', async () => {
    storeAccessToken('access')
    const fetchMock = vi.fn().mockResolvedValue(jsonResponse(200, {status: 1, message: 'ok'}))
    vi.stubGlobal('fetch', fetchMock)

    await AuthHttp.get('/menu')

    // Không có credentials:'include' thì cookie HttpOnly không bao giờ tới server và refresh chết.
    expect(fetchMock.mock.calls[0][1]?.credentials).toBe('include')
})

it('does not retry more than once when the retried request is also 401', async () => {
    storeAccessToken('old-access')
    const fetchMock = vi.fn()
        .mockResolvedValueOnce(jsonResponse(401, {status: 0, message: 'expired'}))
        .mockResolvedValueOnce(jsonResponse(401, {status: 0, message: 'still unauthorized'}))
    vi.stubGlobal('fetch', fetchMock)
    const refresh = vi.fn().mockResolvedValue(true)
    setUnauthorizedHandler(refresh)

    const result = await AuthHttp.get<string[]>('/menu')

    expect(result.status).toBe(0)
    expect(result.message).toBe('still unauthorized')
    expect(refresh).toHaveBeenCalledOnce()
    expect(fetchMock).toHaveBeenCalledTimes(2)
})
