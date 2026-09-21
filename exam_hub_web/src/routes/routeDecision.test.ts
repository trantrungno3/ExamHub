import {describe, expect, it} from 'vitest'
import {ROUTES} from './paths'
import {routeDecision} from './routeDecision'

const APP_ROLES = ['Admin', 'Teacher']
const STUDENT_ROLES = ['Student']

describe('routeDecision', () => {
    it('sends an unauthenticated visitor to login', () => {
        expect(routeDecision(false, undefined, APP_ROLES)).toBe(ROUTES.LOGIN)
        expect(routeDecision(false, ['Admin'], APP_ROLES)).toBe(ROUTES.LOGIN)
    })

    it('sends an authenticated user with no role to the no-role page', () => {
        expect(routeDecision(true, [], APP_ROLES)).toBe(ROUTES.NO_ROLE)
        expect(routeDecision(true, undefined, APP_ROLES)).toBe(ROUTES.NO_ROLE)
    })

    it('keeps a student out of the admin app', () => {
        expect(routeDecision(true, STUDENT_ROLES, APP_ROLES)).toBe(ROUTES.FORBIDDEN)
    })

    it('keeps teachers and admins out of the student portal', () => {
        expect(routeDecision(true, ['Teacher'], STUDENT_ROLES)).toBe(ROUTES.FORBIDDEN)
        expect(routeDecision(true, ['Admin'], STUDENT_ROLES)).toBe(ROUTES.FORBIDDEN)
    })

    it('lets each role into its own area', () => {
        expect(routeDecision(true, ['Teacher'], APP_ROLES)).toBeNull()
        expect(routeDecision(true, ['Admin'], APP_ROLES)).toBeNull()
        expect(routeDecision(true, STUDENT_ROLES, STUDENT_ROLES)).toBeNull()
    })

    it('lets a user holding several roles in through any matching one', () => {
        expect(routeDecision(true, ['Student', 'Teacher'], APP_ROLES)).toBeNull()
        expect(routeDecision(true, ['Student', 'Teacher'], STUDENT_ROLES)).toBeNull()
    })

    it('requires only authentication when no role list is given', () => {
        expect(routeDecision(true, ['Student'])).toBeNull()
        expect(routeDecision(true, ['Student'], [])).toBeNull()
    })
})
