function decodePayload(token: string): Record<string, unknown> {
    try {
        // JWT payload dùng base64url (- _ thay cho + /, không padding) — atob() chỉ hiểu base64
        // chuẩn, nên phải đổi ký tự lại trước khi decode.
        const base64 = token.split('.')[1].replace(/-/g, '+').replace(/_/g, '/')
        return JSON.parse(atob(base64)) as Record<string, unknown>
    } catch {
        return {}
    }
}

/** Unix ms khi token hết hạn (0 nếu không parse được) */
export function getTokenExpiresAt(accessToken: string): number {
    const exp = decodePayload(accessToken)['exp']
    return typeof exp === 'number' ? exp * 1000 : 0
}

/**
 * true nếu token đã hết hạn.
 * @param bufferMs kiểm tra sớm hơn thực tế (VD: 5 phút = 300_000)
 */
export function isTokenExpired(expiresAt: number, bufferMs = 0): boolean {
    return Date.now() >= expiresAt - bufferMs
}

function extractIds(raw: unknown): number[] {
    const arr = Array.isArray(raw) ? raw : raw !== undefined && raw !== null ? [raw] : []
    return arr.map(Number).filter((n) => !Number.isNaN(n))
}

/** Đọc UserInfo từ payload JWT (claim names của ASP.NET Core) */
export function extractUserFromToken(accessToken: string): UserInfo {
    const p = decodePayload(accessToken)
    const userName = String(
        p['UserName'] ?? p['unique_name'] ?? p['name'] ?? ''
    )
    const id = String(
        p['UserId'] ?? p['userId'] ??
        p['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'] ??
        p['nameid'] ?? p['sub'] ?? ''
    )
    const raw = p['Role'] ??
        p['http://schemas.microsoft.com/ws/2008/06/identity/claims/role']
    const roles = Array.isArray(raw) ? (raw as string[]) : raw ? [String(raw)] : []
    const schoolIds = extractIds(p['SchoolId'])
    const cohortClassIds = extractIds(p['CohortClassId'])
    const subjectIds = extractIds(p['SubjectId'])
    return {id, userName, roles, schoolIds, cohortClassIds, subjectIds}
}
