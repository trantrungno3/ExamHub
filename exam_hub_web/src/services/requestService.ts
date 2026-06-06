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
        const stored = localStorage.getItem(globalConfig.storageKey.auth)
        if (!stored) return null
        const parsed = JSON.parse(stored) as { state?: { token?: { accessToken?: string } } }
        return parsed?.state?.token?.accessToken ?? null
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

/** Headers cho multipart: KHÔNG set Content-Type để trình duyệt tự thêm boundary. */
function buildFormHeaders(auth: boolean): Headers {
    const headers = new Headers()
    if (auth) {
        const token = getToken()
        if (token) headers.set('Authorization', `Bearer ${token}`)
    }
    return headers
}

/** Bỏ các giá trị undefined/null/'' khỏi object query để không gửi param rỗng. */
export function cleanParams(
    query: Record<string, string | number | boolean | undefined | null>,
): Record<string, string | number | boolean> {
    const out: Record<string, string | number | boolean> = {}
    Object.entries(query).forEach(([k, v]) => {
        if (v !== undefined && v !== null && v !== '') out[k] = v
    })
    return out
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
        postForm<T>(path: string, form: FormData): Promise<ApiResponse<T>> {
            return fetch(buildUrl(path), {
                method: 'POST', headers: buildFormHeaders(auth), body: form,
            }).then(handleResponse<T>)
        },
    }
}

export const AuthHttp = createHttp(true)
export const Http = createHttp(false)
