import {globalConfig} from '../configs/common'

export const statusCode = {
    Error: 0,
    Success: 1,
    Created: 2,
    Updated: 3,
    Deleted: 4,
    NotFound: 5,
}

export interface ApiResponse<T> {
    status: number
    message: string
    data?: T
    total?: number
}

function getToken(): string | null {
    try {
        const stored = localStorage.getItem(globalConfig.storageKey.token)
        if (!stored) return null
        const parsed = JSON.parse(stored) as { accessToken?: string }
        return parsed?.accessToken ?? null
    } catch {
        return null
    }
}

function buildUrl(path: string, params?: Record<string, string | number | boolean>): string {
    const url = new URL('/api' + path, globalConfig.apiBaseUrl)
    if (params) {
        Object.entries(params).forEach(([k, v]) => url.searchParams.set(k, String(v)))
    }
    return url.toString()
}

async function handleResponse<T>(res: Response): Promise<ApiResponse<T>> {
    return res.json().then(r => r as ApiResponse<T>) as Promise<ApiResponse<T>>
}

function buildHeaders(auth: boolean): Headers {
    const headers = new Headers({'Content-Type': 'application/json'})
    if (auth) {
        const token = getToken()
        if (token) headers.set('Authorization', `Bearer ${token}`)
    }
    return headers
}

function createHttp(auth: boolean) {
    return {
        get<T>(path: string, params?: Record<string, string | number | boolean>): Promise<ApiResponse<T>> {
            return fetch(buildUrl(path, params), {method: 'GET', headers: buildHeaders(auth)}).then(handleResponse<T>)
        },
        post<T>(path: string, body?: unknown): Promise<ApiResponse<T>> {
            return fetch(buildUrl(path), {
                method: 'POST', headers: buildHeaders(auth),
                body: body !== undefined ? JSON.stringify(body) : undefined,
            }).then(handleResponse<T>)
        },
        put<T>(path: string, body?: unknown): Promise<ApiResponse<T>> {
            return fetch(buildUrl(path), {
                method: 'PUT', headers: buildHeaders(auth),
                body: body !== undefined ? JSON.stringify(body) : undefined,
            }).then(handleResponse<T>)
        },
        delete<T>(path: string): Promise<ApiResponse<T>> {
            return fetch(buildUrl(path), {method: 'DELETE', headers: buildHeaders(auth)}).then(res => {
                if (res.status === 204) return {status: statusCode.Deleted, message: 'Xoá thành công'} as ApiResponse<T>
                return handleResponse<T>(res)
            })
        },
        patch<T>(path: string, body?: unknown): Promise<ApiResponse<T>> {
            return fetch(buildUrl(path), {
                method: 'PATCH', headers: buildHeaders(auth),
                body: body !== undefined ? JSON.stringify(body) : undefined,
            }).then(handleResponse<T>)
        },
    }
}

export const AuthHttp = createHttp(true)
export const Http = createHttp(false)
