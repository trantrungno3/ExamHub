import {globalConfig} from '../configs/common'

export const statusCode = {
    Error: 0,
    Success: 1,
    Created: 2,
    Updated: 3,
    Deleted: 4,
    NotFound: 5,
    Conflict: 6,
}

export interface ApiResponse<T> {
    status: number
    message: string
    data?: T
    total?: number
}

type UnauthorizedHandler = () => Promise<boolean>

let unauthorizedHandler: UnauthorizedHandler | null = null

export function setUnauthorizedHandler(handler: UnauthorizedHandler | null): void {
    unauthorizedHandler = handler
}

// Đọc thẳng từ localStorage (đúng key/shape mà zustand persist đã ghi ở authStore.ts) thay vì
// import useAuthStore.getState() — requestService là module thuần, không phụ thuộc React/store để
// tránh vòng phụ thuộc (authStore cũng import requestService để lấy statusCode).
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

const REQUEST_TIMEOUT_MS = 15_000

async function fetchWithTimeout(input: string, init: RequestInit): Promise<Response> {
    const controller = new AbortController()
    const timer = setTimeout(() => controller.abort(), REQUEST_TIMEOUT_MS)
    try {
        // credentials:'include' đặt một lần ở đây cho mọi request: cookie refresh HttpOnly chỉ được
        // gửi kèm khi có flag này, và endpoint refresh/logout phụ thuộc hoàn toàn vào nó.
        return await fetch(input, {...init, credentials: 'include', signal: controller.signal})
    } catch (err) {
        if (err instanceof DOMException && err.name === 'AbortError') {
            throw new Error('Yêu cầu quá thời gian chờ. Vui lòng thử lại.')
        }
        throw new Error('Không thể kết nối đến máy chủ. Vui lòng kiểm tra kết nối mạng.')
    } finally {
        clearTimeout(timer)
    }
}

async function fetchWithUnauthorizedRetry(
    request: () => Promise<Response>,
    auth: boolean,
    retried = false,
): Promise<Response> {
    const response = await request()
    if (!auth || retried || response.status !== 401 || !unauthorizedHandler) {
        return response
    }

    const refreshed = await unauthorizedHandler()
    return refreshed
        ? fetchWithUnauthorizedRetry(request, auth, true)
        : response
}

async function wrapNetworkError<T>(promise: Promise<ApiResponse<T>>): Promise<ApiResponse<T>> {
    try {
        return await promise
    } catch (err) {
        return {
            status: statusCode.Error,
            message: err instanceof Error ? err.message : 'Đã xảy ra lỗi không xác định.',
        }
    }
}

/** Exception chưa được backend bắt trả 500 kèm stack trace .NET dạng text thuần,
 *  không phải JSON — lấy message ở dòng đầu, bỏ tên exception. */
export function parseDotnetError(text: string): string {
    const first = text.split('\n', 1)[0].trim()
    return first.replace(/^[\w.+]*Exception:\s*/, '').split(' ---> ')[0].trim()
        || 'Đã xảy ra lỗi không xác định.'
}

async function handleResponse<T>(res: Response): Promise<ApiResponse<T>> {
    if (res.status === 204) return {status: statusCode.Success, message: 'Thành công'}
    const text = await res.text()
    if (!text) return {status: res.ok ? statusCode.Success : statusCode.Error, message: ''}
    try {
        return JSON.parse(text) as ApiResponse<T>
    } catch {
        return {status: statusCode.Error, message: parseDotnetError(text)}
    }
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
    const requestJson = <T>(
        method: string,
        path: string,
        body?: unknown,
        params?: Record<string, string | number | boolean>,
    ): Promise<ApiResponse<T>> => wrapNetworkError(
        fetchWithUnauthorizedRetry(
            () => fetchWithTimeout(buildUrl(path, params), {
                method,
                headers: buildHeaders(auth),
                body: body === undefined ? undefined : JSON.stringify(body),
            }),
            auth,
        ).then(handleResponse<T>),
    )

    return {
        get<T>(path: string, params?: Record<string, string | number | boolean>): Promise<ApiResponse<T>> {
            return requestJson<T>('GET', path, undefined, params)
        },
        post<T>(path: string, body?: unknown): Promise<ApiResponse<T>> {
            return requestJson<T>('POST', path, body)
        },
        put<T>(path: string, body?: unknown): Promise<ApiResponse<T>> {
            return requestJson<T>('PUT', path, body)
        },
        delete<T>(path: string, body?: unknown): Promise<ApiResponse<T>> {
            return wrapNetworkError(fetchWithUnauthorizedRetry(
                () => fetchWithTimeout(buildUrl(path), {
                    method: 'DELETE',
                    headers: buildHeaders(auth),
                    body: body === undefined ? undefined : JSON.stringify(body),
                }),
                auth,
            ).then(async res => {
                if (res.status === 204) return {status: statusCode.Deleted, message: 'Xoá thành công'}
                // 409 = backend từ chối xoá vì bản ghi đang được tham chiếu (vd. xoá môn học còn đề thi
                // dùng nó) — map riêng sang statusCode.Conflict để UI hiển thị cảnh báo khác lỗi thường.
                if (res.status === 409) {
                    const responseBody = await handleResponse<T>(res)
                    return {...responseBody, status: statusCode.Conflict}
                }
                return handleResponse<T>(res)
            }))
        },
        patch<T>(path: string, body?: unknown): Promise<ApiResponse<T>> {
            return requestJson<T>('PATCH', path, body)
        },
        postForm<T>(path: string, form: FormData): Promise<ApiResponse<T>> {
            return wrapNetworkError(fetchWithUnauthorizedRetry(
                () => fetchWithTimeout(buildUrl(path), {
                    method: 'POST',
                    headers: buildFormHeaders(auth),
                    body: form,
                }),
                auth,
            ).then(handleResponse<T>))
        },
        async getBlob(path: string): Promise<Blob> {
            const res = await fetchWithUnauthorizedRetry(
                () => fetchWithTimeout(buildUrl(path), {
                    method: 'GET',
                    headers: buildFormHeaders(auth),
                }),
                auth,
            )
            if (!res.ok) throw new Error('Không thể tải file từ máy chủ.')
            return res.blob()
        },
    }
}

export const AuthHttp = createHttp(true)
export const Http = createHttp(false)
