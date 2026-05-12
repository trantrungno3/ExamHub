import {globalConfig} from '../configs/common'

interface ApiResponse<T> {
    isSuccess: boolean
    message: string
    data?: T
    total?: number
}

function getToken(): string | null {
    try {
        const stored = localStorage.getItem(globalConfig.storageKey.token)
        if (!stored) return null
        const parsed = JSON.parse(stored)
        return parsed?.accessToken ?? null
    } catch {
        return null
    }
}

function buildHeaders(extra?: HeadersInit, isAuth: boolean = true): Headers {
    const headers = new Headers({'Content-Type': 'application/json', ...extra})
    const token = getToken()
    if (token && isAuth) headers.set('Authorization', `Bearer ${token}`)
    return headers
}

function buildUrl(path: string, params?: Record<string, string | number | boolean>): string {
    const url = new URL('/api' + path, globalConfig.apiBaseUrl)
    if (params) {
        Object.entries(params).forEach(([k, v]) => url.searchParams.set(k, String(v)))
    }
    return url.toString()
}

async function handleResponse<T>(res: Response): Promise<ApiResponse<T>> {
    if (!res.ok) {
        return {isSuccess: false, message: `Lỗi ${res.status}: ${res.statusText}`}
    }
    return res.json()
}

export const AuthHttp = {
    get<T>(path: string, params?: Record<string, string | number | boolean>): Promise<ApiResponse<T>> {
        return fetch(buildUrl(path, params), {
            method: 'GET',
            headers: buildHeaders(),
        }).then(handleResponse<T>)
    },

    post<T>(path: string, body?: unknown): Promise<ApiResponse<T>> {
        return fetch(buildUrl(path), {
            method: 'POST',
            headers: buildHeaders(),
            body: body === undefined ? undefined : JSON.stringify(body),
        }).then(handleResponse<T>)
    },

    put<T>(path: string, body?: unknown): Promise<ApiResponse<T>> {
        return fetch(buildUrl(path), {
            method: 'PUT',
            headers: buildHeaders(),
            body: body === undefined ? undefined : JSON.stringify(body),
        }).then(handleResponse<T>)
    },

    delete<T>(path: string): Promise<ApiResponse<T>> {
        return fetch(buildUrl(path), {
            method: 'DELETE',
            headers: buildHeaders(),
        }).then(handleResponse<T>)
    },
}

export const Http = {
    get<T>(path: string, params?: Record<string, string | number | boolean>): Promise<ApiResponse<T>> {
        return fetch(buildUrl(path, params), {
            method: 'GET',
            headers: buildHeaders(undefined, false),
        }).then(handleResponse<T>)
    },

    post<T>(path: string, body?: unknown): Promise<ApiResponse<T>> {
        return fetch(buildUrl(path), {
            method: 'POST',
            headers: buildHeaders(undefined, false),
            body: body === undefined ? undefined : JSON.stringify(body),
        }).then(handleResponse<T>)
    },

    put<T>(path: string, body?: unknown): Promise<ApiResponse<T>> {
        return fetch(buildUrl(path), {
            method: 'PUT',
            headers: buildHeaders(undefined, false),
            body: body === undefined ? undefined : JSON.stringify(body),
        }).then(handleResponse<T>)
    },

    delete<T>(path: string): Promise<ApiResponse<T>> {
        return fetch(buildUrl(path), {
            method: 'DELETE',
            headers: buildHeaders(undefined, false),
        }).then(handleResponse<T>)
    },
}