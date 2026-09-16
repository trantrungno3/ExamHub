import {describe, expect, it} from 'vitest'
import {getStudentSessionAction} from './studentSessionAction'

const session = (overrides: Partial<MySession> = {}): MySession => ({
    id: 'session-1',
    title: 'Kỳ thi',
    openAt: 1,
    closeAt: 2,
    pickMode: 'Random',
    availability: 'open',
    maxAttempts: 1,
    usedAttempts: 0,
    ...overrides,
})

describe('getStudentSessionAction', () => {
    it.each([
        ['upcoming', 'Chưa mở'],
        ['closed', 'Đã đóng'],
    ] as const)('blocks %s even when an in-progress submission exists', (availability, label) => {
        expect(getStudentSessionAction(session({
            availability,
            inProgressSubmissionId: 'submission-1',
            inProgressExamId: 'exam-1',
        }))).toEqual({kind: 'unavailable', label})
    })

    it('resumes only when session is open', () => {
        expect(getStudentSessionAction(session({
            inProgressSubmissionId: 'submission-1',
            inProgressExamId: 'exam-1',
        }))).toEqual({kind: 'resume'})
    })

    it('shows results when attempts are exhausted', () => {
        expect(getStudentSessionAction(session({usedAttempts: 1}))).toEqual({kind: 'results'})
    })

    it('routes open StudentChoice sessions to pool', () => {
        expect(getStudentSessionAction(session({pickMode: 'StudentChoice'}))).toEqual({kind: 'pick'})
    })

    it('starts an open Random session', () => {
        expect(getStudentSessionAction(session())).toEqual({kind: 'start'})
    })
})
