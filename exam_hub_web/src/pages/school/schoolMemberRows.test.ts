import {describe, expect, it} from 'vitest'
import {buildMemberRows, filterMemberRows} from './schoolMemberRows'

const user = (overrides: Partial<UserResponse> = {}): UserResponse => ({
    id: 'user-1',
    userName: 'teacher01',
    displayName: 'Nguyễn A',
    email: 'a@examhub.vn',
    phoneNumber: null,
    sex: true,
    avartar: null,
    address: null,
    description: null,
    roles: ['Teacher'],
    lockoutEnabled: false,
    isDeleted: false,
    ...overrides,
})

const cohort = (overrides: Partial<Cohort> = {}): Cohort => ({
    id: 10,
    schoolId: 1,
    name: 'K24',
    startYear: 2024,
    endYear: 2027,
    gradeStart: 10,
    numClasses: 3,
    isActive: true,
    ...overrides,
} as Cohort)

const staff = (overrides: Partial<SchoolMember> = {}): SchoolMember => ({
    id: 'same-id',
    schoolId: 1,
    userId: 'user-1',
    role: 'Teacher',
    isActive: true,
    joinedAt: 0,
    ...overrides,
})

const student = (overrides: Partial<CohortMember> = {}): CohortMember => ({
    id: 'same-id',
    cohortId: 10,
    studentId: 'user-2',
    section: 'A',
    joinedAt: 0,
    isActive: true,
    ...overrides,
})

describe('buildMemberRows', () => {
    it('keys stay unique when both sources share a record id', () => {
        const rows = buildMemberRows([staff()], [student()], [], [])
        expect(rows.map(r => r.key)).toEqual(['staff:same-id', 'student:same-id'])
    })

    it('enriches name and email from users, falling back to the raw id', () => {
        const rows = buildMemberRows(
            [staff(), staff({id: 'm2', userId: 'ghost'})],
            [],
            [user()],
            [],
        )
        expect(rows[0]).toMatchObject({displayName: 'Nguyễn A', email: 'a@examhub.vn', role: 'Teacher'})
        expect(rows[1]).toMatchObject({displayName: 'ghost', email: '—'})
    })

    it('falls back to userName when displayName is empty', () => {
        const rows = buildMemberRows([staff()], [], [user({displayName: ''})], [])
        expect(rows[0].displayName).toBe('teacher01')
    })

    it('labels students with cohort and section', () => {
        const rows = buildMemberRows([], [student()], [], [cohort()])
        expect(rows[0]).toMatchObject({kind: 'student', role: 'Student', cohortLabel: 'K24 / A'})
    })

    it('marks unassigned section and unknown cohort', () => {
        const rows = buildMemberRows(
            [],
            [student({section: null}), student({id: 's2', cohortId: 99})],
            [],
            [cohort()],
        )
        expect(rows[0].cohortLabel).toBe('K24 / Chưa xếp')
        expect(rows[1].cohortLabel).toBe('#99')
    })

    it('leaves staff without a cohort label', () => {
        const rows = buildMemberRows([staff()], [], [], [cohort()])
        expect(rows[0].cohortLabel).toBeUndefined()
    })
})

describe('filterMemberRows', () => {
    const rows = buildMemberRows(
        [staff()],
        [student()],
        [user(), user({id: 'user-2', displayName: 'Trần B', email: 'b@examhub.vn'})],
        [cohort()],
    )

    it('keeps everything when filter is all and keyword empty', () => {
        expect(filterMemberRows(rows, 'all', '   ')).toHaveLength(2)
    })

    it('filters by kind', () => {
        expect(filterMemberRows(rows, 'student', '').map(r => r.displayName)).toEqual(['Trần B'])
        expect(filterMemberRows(rows, 'staff', '').map(r => r.displayName)).toEqual(['Nguyễn A'])
    })

    it('matches name or email, case-insensitively', () => {
        expect(filterMemberRows(rows, 'all', 'trần').map(r => r.userId)).toEqual(['user-2'])
        expect(filterMemberRows(rows, 'all', 'A@EXAMHUB').map(r => r.userId)).toEqual(['user-1'])
    })

    it('combines kind and keyword', () => {
        expect(filterMemberRows(rows, 'staff', 'trần')).toEqual([])
    })
})
