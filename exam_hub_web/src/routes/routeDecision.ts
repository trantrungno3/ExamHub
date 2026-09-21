import {ROUTES} from './paths'

/**
 * Quyết định điều hướng của route guard, tách khỏi component để test được: vitest chạy
 * `environment: 'node'` nên không render React, và guard này là logic phân quyền — phần cần test
 * nhất. Trả về path cần redirect, hoặc null = cho render Outlet.
 *
 * Đây chỉ là UX: backend authorization vẫn là security boundary thật.
 */
export function routeDecision(
    isAuthenticated: boolean,
    roles: string[] | undefined,
    allowedRoles?: string[],
): string | null {
    if (!isAuthenticated) return ROUTES.LOGIN
    if (!roles?.length) return ROUTES.NO_ROLE
    if (!allowedRoles?.length) return null
    return roles.some(r => allowedRoles.includes(r)) ? null : ROUTES.FORBIDDEN
}
